using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace AssetDanshari
{
    public class AssetDuplicateTreeModel
    {
        public class FileMd5Info
        {
            public string filePath;
            public string fileRelativePath;
            public string displayName;
            public string fileLength;
            public string fileTime;
            public long fileSize;
            public string md5;
            public bool deleted;
        }

        public class CommonDirInfo
        {
            public string fileRelativePath;
            public string displayName;
        }

        private IEnumerable<IGrouping<string, FileMd5Info>> m_Groups;

        private List<CommonDirInfo> m_CommonDirInfos;

        private string[] m_RefPaths;

        public IEnumerable<IGrouping<string, FileMd5Info>> data
        {
            get { return m_Groups; }
        }

        public List<CommonDirInfo> dirs
        {
            get { return m_CommonDirInfos; }
        }

        public string assetPaths { get; private set; }

        public void SetDataPaths(string refPathStr, string pathStr, string commonPathStr)
        {
            assetPaths = pathStr;
            var fileList = new List<FileMd5Info>();
            var refPaths = AssetDanshariUtility.PathStrToArray(refPathStr);
            var paths = AssetDanshariUtility.PathStrToArray(pathStr);
            var commonPaths = AssetDanshariUtility.PathStrToArray(commonPathStr);

            foreach (var path in paths)
            {
                EditorUtility.DisplayProgressBar(AssetDanshariStyle.Get().progressTitle, String.Empty, 0f);
                var allFiles = Directory.GetFiles(path, "*", SearchOption.AllDirectories);
                for (var i = 0; i < allFiles.Length;)
                {
                    FileInfo fileInfo = new FileInfo(allFiles[i]);
                    if (fileInfo.Extension == ".meta")
                    {
                        i++;
                        continue;
                    }

                    EditorUtility.DisplayProgressBar(AssetDanshariStyle.Get().progressTitle, fileInfo.Name, i * 1f / allFiles.Length);
                    try
                    {
                        using (var md5 = MD5.Create())
                        {
                            using (var stream = File.OpenRead(fileInfo.FullName))
                            {
                                FileMd5Info info = new FileMd5Info();
                                info.filePath = fileInfo.FullName;
                                info.displayName = fileInfo.Name;
                                info.fileSize = fileInfo.Length;
                                info.fileTime = fileInfo.CreationTime.ToString("yyyy-MM-dd HH:mm:ss");
                                info.md5 = BitConverter.ToString(md5.ComputeHash(stream)).ToLower();
                                fileList.Add(info);
                            }
                        }

                        i++;
                    }
                    catch (Exception e)
                    {
                        if (!EditorUtility.DisplayDialog(AssetDanshariStyle.Get().errorTitle, path + "\n" + e.Message,
                            AssetDanshariStyle.Get().continueStr, AssetDanshariStyle.Get().cancelStr))
                        {
                            EditorUtility.ClearProgressBar();
                            return;
                        }
                    }
                }
            }

            m_Groups = fileList.GroupBy(info => info.md5).Where(g => g.Count() > 1);

            var dataPathLen = Application.dataPath.Length - 6;
            foreach (var group in m_Groups)
            {
                foreach (var info in group)
                {
                    info.fileRelativePath = info.filePath.Substring(dataPathLen).Replace('\\', '/');
                    if (info.fileSize >= (1 << 20))
                    {
                        info.fileLength = string.Format("{0:F} Mb", info.fileSize / 1024f / 1024f);
                    }
                    else if (info.fileSize >= (1 << 10))
                    {
                        info.fileLength = string.Format("{0:F} Kb", info.fileSize / 1024f);
                    }
                }
            }

            m_CommonDirInfos = new List<CommonDirInfo>();
            foreach (var commonPath in commonPaths)
            {
                var commonName = Path.GetFileNameWithoutExtension(commonPath);
                var commonLen = commonPath.Length - commonName.Length;
                m_CommonDirInfos.Add(new CommonDirInfo()
                {
                    fileRelativePath = commonPath, displayName = commonName
                });

                var allDirs = Directory.GetDirectories(commonPath, "*", SearchOption.AllDirectories);
                foreach (var allDir in allDirs)
                {
                    var dirInfo = new CommonDirInfo();
                    dirInfo.fileRelativePath = allDir.Replace('\\', '/');
                    dirInfo.displayName = dirInfo.fileRelativePath.Substring(commonLen);
                    m_CommonDirInfos.Add(dirInfo);
                }
            }

            m_RefPaths = refPaths;
            EditorUtility.ClearProgressBar();
        }

        /// <summary>
        /// 去引用到的目录查找所有用到的guid，批量更改
        /// </summary>
        /// <param name="group"></param>
        /// <param name="useInfo"></param>
        public void SetUseThis(IGrouping<string, FileMd5Info> group, FileMd5Info useInfo)
        {
            var style = AssetDanshariStyle.Get();

            string patternStr = String.Empty;
            foreach (var info in group)
            {
                if (info != useInfo)
                {
                    patternStr += String.Format("({0})|", AssetDatabase.AssetPathToGUID(info.fileRelativePath));
                }
            }

            patternStr = patternStr.TrimEnd('|');
            Regex pattern = new Regex(patternStr);
            string replaceStr = AssetDatabase.AssetPathToGUID(useInfo.fileRelativePath);

            foreach (var refPath in m_RefPaths)
            {
                EditorUtility.DisplayProgressBar(style.progressTitle, String.Empty, 0f);
                var allFiles = Directory.GetFiles(refPath, "*", SearchOption.AllDirectories);

                for (var i = 0; i < allFiles.Length;)
                {
                    var file = allFiles[i];
                    if (!AssetDanshariUtility.IsPlainTextExt(file))
                    {
                        i++;
                        continue;
                    }

                    EditorUtility.DisplayProgressBar(style.progressTitle, file, i * 1f / allFiles.Length);
                    try
                    {
                        string text = File.ReadAllText(file);
                        string text2 = pattern.Replace(text, replaceStr);
                        if (!string.Equals(text, text2))
                        {
                            File.WriteAllText(file, text2);
                        }
                        i++;
                    }
                    catch (Exception e)
                    {
                        if (!EditorUtility.DisplayDialog(style.errorTitle, file + "\n" + e.Message,
                            style.continueStr, style.cancelStr))
                        {
                            EditorUtility.ClearProgressBar();
                            return;
                        }
                    }
                }
            }
            EditorUtility.ClearProgressBar();
            EditorUtility.DisplayDialog(String.Empty, style.progressFinish, style.sureStr);
        }

        public void SetMoveToCommon(FileMd5Info moveInfo, string destDir)
        {
            var style = AssetDanshariStyle.Get();
            string destPath = String.Format("{0}/{1}", destDir, moveInfo.displayName);
            var errorStr = AssetDatabase.MoveAsset(moveInfo.fileRelativePath, destPath);
            if (!string.IsNullOrEmpty(errorStr))
            {
                EditorUtility.DisplayDialog(style.errorTitle, errorStr, style.sureStr);
            }
            else
            {
                moveInfo.fileRelativePath = destPath;
                EditorUtility.DisplayDialog(String.Empty, style.progressFinish, style.sureStr);
            }
        }

        public void SetRemoveAllOther(IGrouping<string, FileMd5Info> group, FileMd5Info selectInfo)
        {
            var style = AssetDanshariStyle.Get();
            if (!EditorUtility.DisplayDialog(String.Empty, style.sureStr + style.duplicateContextDelOther.text,
                style.sureStr, style.cancelStr))
            {
                return;
            }

            foreach (var info in group)
            {
                if (info != selectInfo && !info.deleted)
                {
                    if (AssetDatabase.DeleteAsset(info.fileRelativePath))
                    {
                        info.deleted = true;
                    }
                }
            }
            EditorUtility.DisplayDialog(String.Empty, style.progressFinish, style.sureStr);
        }
    }
}