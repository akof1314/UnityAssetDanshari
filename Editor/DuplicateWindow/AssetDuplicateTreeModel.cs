using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace AssetDanshari
{
    public class AssetDuplicateTreeModel : AssetTreeModel
    {
        public class FileSimpleInfo
        {
            public FileInfo fileInfo;
            public long fileSize;
            public string filePath;

            /// <summary>
            /// 界面用文件大小
            /// </summary>
            public string fileLength;

            /// <summary>
            /// 创建时间
            /// </summary>
            public string fileTime;
        }

        private class DuplicateFileGroup
        {
            public long size { get; }
            public List<FileSimpleInfo> files { get; }

            public DuplicateFileGroup(long fileSize, List<FileSimpleInfo> fileList)
            {
                size = fileSize;
                files = fileList;
            }
        }

        public override void SetDataPaths(string refPathStr, string pathStr, string commonPathStr, string grepPath)
        {
            base.SetDataPaths(refPathStr, pathStr, commonPathStr, grepPath);
            var style = AssetDanshariStyle.Get();

            var resFileList = GetResFileList();
            var duplicateFileGroups = GetDuplicateFileGroupInfo(resFileList);
            if (duplicateFileGroups == null || duplicateFileGroups.Count == 0)
            {
                EditorUtility.ClearProgressBar();
                return;
            }

            var rootInfo = new AssetInfo(GetAutoId(), String.Empty, String.Empty);
            foreach (var group in duplicateFileGroups)
            {
                AssetInfo dirInfo = new AssetInfo(GetAutoId(), String.Empty, String.Format(style.duplicateGroup, group.files.Count));
                dirInfo.isExtra = true;
                rootInfo.AddChild(dirInfo);

                foreach (var member in group.files)
                {
                    dirInfo.AddChild(GetAssetInfoByFileInfo(member));
                }
            }

            if (rootInfo.hasChildren)
            {
                data = rootInfo;
            }
            EditorUtility.ClearProgressBar();
        }

        private const int kBufferSize = 1 * 1024 * 1024;

        private List<DuplicateFileGroup> GetDuplicateFileGroupInfo(List<string> fileArray)
        {
            List<DuplicateFileGroup> duplicateFileGroups = new List<DuplicateFileGroup>();

            var fileList = new List<FileSimpleInfo>();

            foreach (var file in fileArray)
            {
                // 大小为0的不考虑重复
                FileInfo fileInfo = new FileInfo(file);
                if (fileInfo.Length > 0)
                {
                    FileSimpleInfo info = new FileSimpleInfo
                    {
                        fileSize = fileInfo.Length,
                        fileInfo = fileInfo
                    };
                    fileList.Add(info);
                }
            }

            if (fileList.Count == 0)
            {
                return duplicateFileGroups;
            }

            // https://github.com/cemahseri/Duplica
            var groups = fileList.GroupBy(info => info.fileSize).Where(g => g.Count() > 1);
            foreach (var group in groups)
            {
                DuplicateFileGroup duplicateFileGroup = new DuplicateFileGroup(group.Key, new List<FileSimpleInfo>());
                var duplicateFiles = duplicateFileGroup.files;
                duplicateFileGroups.Add(duplicateFileGroup);

                foreach (var member in group)
                {
                    duplicateFiles.Add(member);
                }
            }

            var buffer = new byte[kBufferSize];
            for (var dupIndex = 0; dupIndex < duplicateFileGroups.Count; dupIndex++)
            {
                var duplicateFileGroup = duplicateFileGroups[dupIndex];
                AssetDanshariUtility.DisplayThreadProgressBar(duplicateFileGroups.Count, dupIndex);

                var numberOfChunks = duplicateFileGroup.size / kBufferSize;

                for (long chunk = 0; chunk <= numberOfChunks; chunk++)
                {
                    var hashes = new Dictionary<Hash128, List<FileSimpleInfo>>();

                    foreach (var fileSimpleInfo in duplicateFileGroup.files)
                    {
                        fileSimpleInfo.filePath = fileSimpleInfo.fileInfo.FullName;

                        using (FileStream stream = new FileStream(fileSimpleInfo.filePath, FileMode.Open,
                                   FileAccess.Read, FileShare.Read, kBufferSize, FileOptions.SequentialScan))
                        {
                            if (!stream.CanRead)
                            {
                                continue;
                            }

                            if (!stream.CanSeek)
                            {
                                continue;
                            }

                            stream.Seek(chunk * kBufferSize, SeekOrigin.Begin);

                            Array.Clear(buffer, 0, kBufferSize);

                            int len = stream.Read(buffer, 0, kBufferSize);

#if UNITY_2020_1_OR_NEWER
                            var hash = Hash128.Compute(buffer, 0, len);
#else
                            var hash = Hash128.Compute(Convert.ToBase64String(buffer, 0, len));
#endif

                            if (!hashes.ContainsKey(hash))
                            {
                                hashes.Add(hash, new List<FileSimpleInfo>());
                            }

                            hashes[hash].Add(fileSimpleInfo);
                        }
                    }

                    foreach (var fileSimpleInfo in hashes.Values.Where(f => f.Count == 1).SelectMany(a => a))
                    {
                        duplicateFileGroup.files.Remove(fileSimpleInfo);
                    }
                }
            }

            for (int i = duplicateFileGroups.Count - 1; i >= 0; i--)
            {
                if (duplicateFileGroups[i].files.Count <= 1)
                {
                    duplicateFileGroups.RemoveAt(i);
                }
            }

            return duplicateFileGroups;
        }

        private AssetInfo GetAssetInfoByFileInfo(FileSimpleInfo fileInfo)
        {
            AssetInfo info = GenAssetInfo(FullPathToRelative(fileInfo.filePath));
            info.bindObj = fileInfo;

            if (fileInfo.fileSize >= (1 << 20))
            {
                fileInfo.fileLength = string.Format("{0:F} MB", fileInfo.fileSize / 1024f / 1024f);
            }
            else if (fileInfo.fileSize >= (1 << 10))
            {
                fileInfo.fileLength = string.Format("{0:F} KB", fileInfo.fileSize / 1024f);
            }
            else
            {
                fileInfo.fileLength = string.Format("{0:F} B", fileInfo.fileSize);
            }

            fileInfo.fileTime = fileInfo.fileInfo.CreationTime.ToString("yyyy-MM-dd HH:mm:ss");
            fileInfo.fileInfo = null;

            return info;
        }

        /// <summary>
        /// 去引用到的目录查找所有用到的guid，批量更改
        /// </summary>
        /// <param name="group"></param>
        /// <param name="useInfo"></param>
        public void SetUseThis(AssetInfo group, AssetInfo useInfo)
        {
            var style = AssetDanshariStyle.Get();
            if (!EditorUtility.DisplayDialog(String.Empty, style.sureStr + style.duplicateContextOnlyUseThis.text,
                style.sureStr, style.cancelStr))
            {
                return;
            }

            List<string> patterns = new List<string>();
            foreach (var info in group.children)
            {
                if (info != useInfo)
                {
                    patterns.Add(AssetDatabase.AssetPathToGUID(info.fileRelativePath));
                }
            }

            string replaceStr = AssetDatabase.AssetPathToGUID(useInfo.fileRelativePath);
            List<string> fileList = GetRefFileList();
            var refGuidMap = GetGuidMapFromFileList(fileList);

            ThreadDoFilesTextSearchReplace(grepPath, refPaths, fileList, refGuidMap, patterns, replaceStr, GetSearchResultList(fileList.Count, 0));
            EditorUtility.DisplayProgressBar(style.progressTitle, style.deleteFile, 0.98f);
            SetRemoveAllOther(group, useInfo);
            EditorUtility.ClearProgressBar();
            EditorUtility.DisplayDialog(String.Empty, style.progressFinish, style.sureStr);
        }

        private void SetRemoveAllOther(AssetInfo group, AssetInfo selectInfo)
        {
            foreach (var info in group.children)
            {
                if (info != selectInfo && !info.deleted)
                {
                    if (AssetDatabase.DeleteAsset(info.fileRelativePath))
                    {
                        info.deleted = true;
                    }
                }
            }
        }

        /// <summary>
        /// 手动添加数据
        /// </summary>
        /// <param name="filePaths"></param>
        public int AddManualData(string[] filePaths)
        {
            var fileList = new List<FileSimpleInfo>();

            foreach (var file in filePaths)
            {
                if (!string.IsNullOrEmpty(file))
                {
                    FileInfo fileInfo = new FileInfo(file);
                    FileSimpleInfo info = new FileSimpleInfo
                    {
                        fileSize = fileInfo.Length,
                        filePath = fileInfo.FullName,
                        fileInfo = fileInfo
                    };
                    fileList.Add(info);
                }
            }

            if (fileList.Count < 2 || !HasData())
            {
                EditorUtility.ClearProgressBar();
                return 0;
            }

            var style = AssetDanshariStyle.Get();
            AssetInfo dirInfo = new AssetInfo(GetAutoId(), String.Empty, String.Format(style.duplicateGroup, fileList.Count));
            dirInfo.isExtra = true;
            data.AddChild(dirInfo);

            foreach (var member in fileList)
            {
                dirInfo.AddChild(GetAssetInfoByFileInfo(member));
            }
            EditorUtility.ClearProgressBar();
            return dirInfo.children[dirInfo.children.Count - 1].id;
        }

        public override void ExportCsv()
        {
            string path = AssetDanshariUtility.GetSaveFilePath(typeof(AssetDuplicateWindow).Name);
            if (string.IsNullOrEmpty(path))
            {
                return;
            }

            var style = AssetDanshariStyle.Get();
            var sb = new StringBuilder();
            sb.AppendFormat("\"{0}\",", style.duplicateHeaderContent.text);
            sb.AppendFormat("\"{0}\",", style.duplicateHeaderContent2.text);
            sb.AppendFormat("\"{0}\",", style.duplicateHeaderContent3.text);
            sb.AppendFormat("\"{0}\"\n", style.duplicateHeaderContent4.text);

            foreach (var group in data.children)
            {
                sb.AppendLine(String.Format(style.duplicateGroup, group.displayName));

                foreach (var info in group.children)
                {
                    sb.AppendFormat("\"{0}\",", info.displayName);
                    sb.AppendFormat("\"{0}\",", info.fileRelativePath);

                    FileSimpleInfo simpleInfo = info.bindObj as FileSimpleInfo;
                    sb.AppendFormat("\"{0}\",", simpleInfo.fileLength);
                    sb.AppendFormat("\"{0}\"\n", simpleInfo.fileTime);
                }
            }

            AssetDanshariUtility.SaveFileText(path, sb.ToString());
            GUIUtility.ExitGUI();
        }
    }
}