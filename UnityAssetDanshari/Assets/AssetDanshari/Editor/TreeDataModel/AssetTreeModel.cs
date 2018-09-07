using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace AssetDanshari
{
    public class AssetTreeModel
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

        public void SetDataPaths(string[] refPaths, string[] paths, string[] commonPaths)
        {
            var fileList = new List<FileMd5Info>();

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
        /// <param name="info"></param>
        public void SetUseThis(IGrouping<string, FileMd5Info> group, FileMd5Info info)
        {
            foreach (var refPath in m_RefPaths)
            {
                EditorUtility.DisplayProgressBar(AssetDanshariStyle.Get().progressTitle, String.Empty, 0f);
                var allFiles = Directory.GetFiles(refPath, "*", SearchOption.AllDirectories);

                for (var i = 0; i < allFiles.Length;)
                {
                    var file = allFiles[i];
                    if (!IsSupportExt(file))
                    {
                        i++;
                        continue;
                    }

                    EditorUtility.DisplayProgressBar(AssetDanshariStyle.Get().progressTitle, file, i * 1f / allFiles.Length);
                    try
                    {
                        string text = File.ReadAllText(file);

                        i++;
                    }
                    catch (Exception e)
                    {
                        if (!EditorUtility.DisplayDialog(AssetDanshariStyle.Get().errorTitle, file + "\n" + e.Message,
                            AssetDanshariStyle.Get().continueStr, AssetDanshariStyle.Get().cancelStr))
                        {
                            EditorUtility.ClearProgressBar();
                            return;
                        }
                    }
                }
            }
        }

        public bool IsSupportExt(string ext)
        {
            ext = ext.ToLower();
            return ext.EndsWith(".prefab") || ext.EndsWith(".unity") || 
                   ext.EndsWith(".mat") || ext.EndsWith(".asset") ||
                   ext.EndsWith(".controller") || ext.EndsWith(".anim");
        }
    }
}