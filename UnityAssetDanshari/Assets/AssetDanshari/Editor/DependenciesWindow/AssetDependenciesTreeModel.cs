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
        }

        public List<FileBeDependInfo> data { get; private set; }

        public override bool HasData()
        {
            return data != null;
        }

        public override void SetDataPaths(string refPathStr, string pathStr, string commonPathStr)
        {
            data = null;
            assetPaths = pathStr;
            var fileList = new List<FileBeDependInfo>();
            var refPaths = AssetDanshariUtility.PathStrToArray(refPathStr);
            var paths = AssetDanshariUtility.PathStrToArray(pathStr);
            var commonPaths = AssetDanshariUtility.PathStrToArray(commonPathStr);
            var dataPathLen = Application.dataPath.Length - 6;
            var style = AssetDanshariStyle.Get();

            foreach (var path in paths)
            {
                if (!Directory.Exists(path))
                {
                    continue;
                }

                EditorUtility.DisplayProgressBar(style.progressTitle, String.Empty, 0f);
                var allFiles = Directory.GetFiles(path, "*", SearchOption.AllDirectories);
                for (var i = 0; i < allFiles.Length; i++)
                {
                    FileInfo fileInfo = new FileInfo(allFiles[i]);
                    if (fileInfo.Extension == ".meta")
                    {
                        continue;
                    }

                    EditorUtility.DisplayProgressBar(style.progressTitle, fileInfo.Name, i * 1f / allFiles.Length);
                    FileBeDependInfo info = new FileBeDependInfo();
                    info.filePath = fileInfo.FullName;
                    info.fileRelativePath = info.filePath.Substring(dataPathLen).Replace('\\', '/');
                    info.displayName = fileInfo.Name;
                    info.regex = new Regex(AssetDatabase.AssetPathToGUID(info.fileRelativePath));
                    fileList.Add(info);
                }
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
                        foreach (var info in fileList)
                        {
                            if (info.regex.IsMatch(text))
                            {
                                if (info.beDependPaths == null)
                                {
                                    info.beDependPaths = new List<string>();
                                }
                                info.beDependPaths.Add(file);
                            }
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

            data = fileList;
            EditorUtility.ClearProgressBar();
        }
    }
}