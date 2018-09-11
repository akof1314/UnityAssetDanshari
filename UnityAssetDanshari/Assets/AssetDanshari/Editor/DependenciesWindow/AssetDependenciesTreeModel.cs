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
        public class FileBeDependInfo
        {
            public string filePath;
            public string fileRelativePath;
            public string displayName;
            public List<string> beDependPaths;
            public bool deleted;
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

        public class DirInfo
        {
            public string fileRelativePath;
            public string displayName;

            public List<DirInfo> dirs;
            public List<FileBeDependInfo> files;
        }

        public DirInfo data { get; private set; }

        public override bool HasData()
        {
            return data != null;
        }

        public override void SetDataPaths(string refPathStr, string pathStr, string commonPathStr)
        {
            data = null;
            base.SetDataPaths(refPathStr, pathStr, commonPathStr);
            var dirInfo = new DirInfo();
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
                        CheckFileMatch(dirInfo, file, text);
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

        private void LoadDirData(string path, DirInfo dirInfo)
        {
            var allDirs = Directory.GetDirectories(path, "*", SearchOption.TopDirectoryOnly);
            foreach (var allDir in allDirs)
            {
                DirInfo info = new DirInfo();
                info.fileRelativePath = allDir.Replace('\\', '/');
                info.displayName = Path.GetFileName(allDir);
                if (dirInfo.dirs == null)
                {
                    dirInfo.dirs = new List<DirInfo>();
                }
                dirInfo.dirs.Add(info);

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
                if (dirInfo.files == null)
                {
                    dirInfo.files = new List<FileBeDependInfo>();
                }
                dirInfo.files.Add(info);
            }
        }

        private void CheckFileMatch(DirInfo dirInfo, string filePath, string fileText)
        {
            if (dirInfo.dirs != null)
            {
                foreach (var info in dirInfo.dirs)
                {
                    CheckFileMatch(info, filePath, fileText);
                }
            }

            if (dirInfo.files != null)
            {
                foreach (var info in dirInfo.files)
                {
                    if (info.regex.IsMatch(fileText))
                    {
                        if (info.beDependPaths == null)
                        {
                            info.beDependPaths = new List<string>();
                        }
                        info.beDependPaths.Add(filePath);
                    }
                }
            }
        }
    }
}