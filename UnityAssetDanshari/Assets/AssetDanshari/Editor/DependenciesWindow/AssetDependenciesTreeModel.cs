using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

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
            rooInfo.isFolder = true;
            var style = AssetDanshariStyle.Get();

            for (var i = 0; i < resPaths.Length; i++)
            {
                var path = resPaths[i];
                if (!Directory.Exists(path))
                {
                    continue;
                }

                EditorUtility.DisplayProgressBar(style.progressTitle, path, i * 1f / resPaths.Length);
                AssetInfo info = GenAssetInfo(path);
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

            if (rooInfo.hasChildren)
            {
                data = rooInfo;
            }
            EditorUtility.ClearProgressBar();
        }

        private void LoadDirData(string path, AssetInfo dirInfo)
        {
            var allDirs = Directory.GetDirectories(path, "*", SearchOption.TopDirectoryOnly);
            foreach (var allDir in allDirs)
            {
                AssetInfo info = GenAssetInfo(allDir);
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

                AssetInfo info = GenAssetInfo(FullPathToRelative(fileInfo.FullName));
                info.bindObj = new Regex(AssetDatabase.AssetPathToGUID(info.fileRelativePath));
                dirInfo.AddChild(info);
            }
        }

        private void CheckFileMatch(AssetInfo dirInfo, string filePath, string fileText)
        {
            if (dirInfo.isFolder && !dirInfo.isExtra && dirInfo.hasChildren)
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
                    AssetInfo info = GenAssetInfo(filePath);
                    info.isExtra = true;
                    dirInfo.AddChild(info);
                }
            }
        }

        public override void ExportCsv()
        {
            string path = AssetDanshariUtility.GetSaveFilePath(typeof(AssetDependenciesWindow).Name);
            if (string.IsNullOrEmpty(path))
            {
                return;
            }

            var style = AssetDanshariStyle.Get();
            var sb = new StringBuilder();
            sb.AppendFormat("\"{0}\",", style.dependenciesHeaderContent.text);
            sb.AppendFormat("\"{0}\"\n", style.dependenciesHeaderContent2.text);

            foreach (var info in data.children)
            {
                ExportCsvDataDir(info, sb, "├");
            }

            AssetDanshariUtility.SaveFileText(path, sb.ToString());
            GUIUtility.ExitGUI();
        }

        private void ExportCsvDataDir(AssetInfo assetInfo, StringBuilder sb, string pre)
        {
            if (assetInfo.isExtra)
            {
                sb.AppendFormat(",\"{0}\"\n", assetInfo.displayName);
            }
            else if (assetInfo.isFolder)
            {
                sb.AppendLine(pre + assetInfo.displayName);
            }
            else
            {
                sb.AppendFormat("\"{0}\"", pre + assetInfo.displayName);
                if (assetInfo.hasChildren && assetInfo.children.Count > 0)
                {
                    sb.AppendFormat(",\"{0}\"", assetInfo.children.Count.ToString());
                }
                sb.AppendLine();
            }

            if (assetInfo.hasChildren)
            {
                foreach (var childInfo in assetInfo.children)
                {
                    ExportCsvDataDir(childInfo, sb, pre + pre);
                }
            }
        }
    }
}