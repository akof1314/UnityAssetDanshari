using System;
using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;

namespace AssetDanshari
{
    public class AssetDependenciesTreeModel : AssetTreeModel
    {
        public class FileBeDependInfo : AssetInfo
        {
            public string filePath;
            public List<AssetInfo> beDependPaths;
            public Regex regex;

            public int GetBeDependCount()
            {
                if (beDependPaths != null)
                {
                    return beDependPaths.Count;
                }
                return 0;
            }
        }

        public AssetInfo data { get; private set; }

        public override bool HasData()
        {
            return data != null;
        }

        public override void SetDataPaths(string refPathStr, string pathStr, string commonPathStr)
        {
            data = null;
            base.SetDataPaths(refPathStr, pathStr, commonPathStr);
            var dirInfo = new AssetInfo();
            var style = AssetDanshariStyle.Get();

            for (var i = 0; i < resPaths.Length; i++)
            {
                var path = resPaths[i];
                if (!Directory.Exists(path))
                {
                    continue;
                }

                EditorUtility.DisplayProgressBar(style.progressTitle, path, i * 1f / resPaths.Length);
                LoadDirData(path, dirInfo);
            }

            foreach (var refPath in refPaths)
            {
                if (!Directory.Exists(refPath))
                {
                    continue;
                }

                EditorUtility.DisplayProgressBar(style.progressTitle, String.Empty, 0f);
                var allFiles = Directory.GetFiles(refPath, "*", SearchOption.AllDirectories);

                for (var i = 0; i < allFiles.Length;)
                {
                    var file = PathToStandardized(allFiles[i]);
                    if (!AssetDanshariUtility.IsPlainTextExt(file))
                    {
                        i++;
                        continue;
                    }

                    EditorUtility.DisplayProgressBar(style.progressTitle, file, i * 1f / allFiles.Length);
                    try
                    {
                        string text = File.ReadAllText(file);
                        AssetInfo fileInfo = new AssetInfo()
                        {
                            displayName = Path.GetFileName(file),
                            fileRelativePath = file
                        };
                        CheckFileMatch(dirInfo, fileInfo, text);
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

            data = dirInfo;
            EditorUtility.ClearProgressBar();
        }

        private void LoadDirData(string path, AssetInfo dirInfo)
        {
            var allDirs = Directory.GetDirectories(path, "*", SearchOption.TopDirectoryOnly);
            foreach (var allDir in allDirs)
            {
                AssetInfo info = new AssetInfo();
                info.fileRelativePath = PathToStandardized(allDir);
                info.displayName = Path.GetFileName(allDir);
                dirInfo.AddChild(info);

                LoadDirData(info.fileRelativePath, info);
            }

            var allFiles = Directory.GetFiles(path, "*", SearchOption.TopDirectoryOnly);
            for (var i = 0; i < allFiles.Length; i++)
            {
                FileInfo fileInfo = new FileInfo(allFiles[i]);
                if (fileInfo.Extension == ".meta")
                {
                    continue;
                }

                FileBeDependInfo info = new FileBeDependInfo();
                info.filePath = fileInfo.FullName;
                info.fileRelativePath = FullPathToRelative(info.filePath);
                info.displayName = fileInfo.Name;
                info.regex = new Regex(AssetDatabase.AssetPathToGUID(info.fileRelativePath));
                dirInfo.AddChild(info);
            }
        }

        private void CheckFileMatch(AssetInfo dirInfo, AssetInfo filePath, string fileText)
        {
            if (dirInfo.hasChildren)
            {
                foreach (var info in dirInfo.children)
                {
                    CheckFileMatch(info, filePath, fileText);
                }
            }
            else
            {
                var info = dirInfo as FileBeDependInfo;
                if (info != null)
                {
                    if (info.regex.IsMatch(fileText))
                    {
                        if (info.beDependPaths == null)
                        {
                            info.beDependPaths = new List<AssetInfo>();
                        }
                        info.beDependPaths.Add(filePath);
                    }
                }
            }
        }
    }
}