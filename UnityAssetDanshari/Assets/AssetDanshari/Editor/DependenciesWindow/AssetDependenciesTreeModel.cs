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
        public override void SetDataPaths(string refPathStr, string pathStr, string commonPathStr)
        {
            data = null;
            ResetAutoId();
            base.SetDataPaths(refPathStr, pathStr, commonPathStr);
            var rooInfo = new AssetInfo(GetAutoId(), String.Empty, String.Empty);
            var style = AssetDanshariStyle.Get();

            for (var i = 0; i < resPaths.Length; i++)
            {
                var path = resPaths[i];
                if (!Directory.Exists(path))
                {
                    continue;
                }

                EditorUtility.DisplayProgressBar(style.progressTitle, path, i * 1f / resPaths.Length);
                AssetInfo info = new AssetInfo(GetAutoId(), PathToStandardized(path), Path.GetFileName(path));
                rooInfo.AddChild(info);
                LoadDirData(path, info);
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
                        CheckFileMatch(rooInfo, file, text);
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

            data = rooInfo;
            EditorUtility.ClearProgressBar();
        }

        private void LoadDirData(string path, AssetInfo dirInfo)
        {
            var allDirs = Directory.GetDirectories(path, "*", SearchOption.TopDirectoryOnly);
            foreach (var allDir in allDirs)
            {
                AssetInfo info = new AssetInfo(GetAutoId(), PathToStandardized(allDir), Path.GetFileName(allDir));
                info.isFolder = true;
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

                AssetInfo info = new AssetInfo(GetAutoId(), FullPathToRelative(fileInfo.FullName), fileInfo.Name);
                info.bindObj = new Regex(AssetDatabase.AssetPathToGUID(info.fileRelativePath));
                dirInfo.AddChild(info);
            }
        }

        private void CheckFileMatch(AssetInfo dirInfo, string filePath, string fileText)
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
                var regex = dirInfo.bindObj as Regex;
                if (regex != null && regex.IsMatch(fileText))
                {
                    AssetInfo info = new AssetInfo(GetAutoId(), filePath, Path.GetFileName(filePath));
                    info.isExtra = true;
                    dirInfo.AddChild(info);
                }
            }
        }
    }
}