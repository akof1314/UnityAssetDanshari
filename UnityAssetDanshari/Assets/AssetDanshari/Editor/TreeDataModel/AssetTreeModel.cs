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
            public string displayName;
            public string fileLength;
            public string fileTime;
            public long fileSize;
            public string md5;
        }

        private readonly List<FileMd5Info> m_Data = new List<FileMd5Info>();

        public void SetDataPaths(string[] paths)
        {
            m_Data.Clear();

            foreach (var path in paths)
            {
                var allFiles = Directory.GetFiles(path, "*", SearchOption.AllDirectories);
                for (var i = 0; i < allFiles.Length;)
                {
                    var file = allFiles[i];
                    FileInfo fileInfo = new FileInfo(file);
                    if (fileInfo.Extension == ".meta")
                    {
                        i++;
                        continue;
                    }

                    FileMd5Info info = new FileMd5Info();
                    info.filePath = fileInfo.FullName;
                    info.fileSize = fileInfo.Length;
                    info.fileTime = fileInfo.LastWriteTime.ToString();

                    try
                    {
                        using (var md5 = MD5.Create())
                        {
                            using (var stream = File.OpenRead(fileInfo.FullName))
                            {
                                info.md5 = BitConverter.ToString(md5.ComputeHash(stream)).ToLower();
                                m_Data.Add(info);
                            }
                        }

                        i++;
                    }
                    catch (Exception e)
                    {
                        if (!EditorUtility.DisplayDialog(AssetDanshariStyle.Get().errorTitle, path + "\n" + e.Message,
                            AssetDanshariStyle.Get().continueStr, AssetDanshariStyle.Get().cancelStr))
                        {
                            return;
                        }
                    }
                }
            }

            var groups = m_Data.GroupBy(info => info.md5);
        }
    }
}