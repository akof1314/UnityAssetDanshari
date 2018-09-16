using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace AssetDanshari
{
    public class AssetReferenceTreeModel : AssetTreeModel
    {
        public override void SetDataPaths(string refPathStr, string pathStr, string commonPathStr)
        {
            data = null;
            ResetAutoId();
            base.SetDataPaths(refPathStr, pathStr, commonPathStr);
            assetPaths = refPathStr;
            var rooInfo = new AssetInfo(GetAutoId(), String.Empty, String.Empty);
            rooInfo.isFolder = true;
            var style = AssetDanshariStyle.Get();

            for (var i = 0; i < refPaths.Length; i++)
            {
                var path = refPaths[i];
                if (!Directory.Exists(path))
                {
                    continue;
                }

                EditorUtility.DisplayProgressBar(style.progressTitle, path, i * 1f / refPaths.Length);
                AssetInfo info = GenAssetInfo(path);
                rooInfo.AddChild(info);
                LoadDirData(path, info);
            }

            var rooResInfo = new AssetInfo(GetAutoId(), String.Empty, String.Empty);
            rooResInfo.isFolder = true;
            for (var i = 0; i < resPaths.Length; i++)
            {
                var path = resPaths[i];
                if (!Directory.Exists(path))
                {
                    continue;
                }

                EditorUtility.DisplayProgressBar(style.progressTitle, path, i * 1f / resPaths.Length);
                var allFiles = Directory.GetFiles(path, "*", SearchOption.AllDirectories);
                foreach (var file in allFiles)
                {
                    FileInfo fileInfo = new FileInfo(file);
                    if (fileInfo.Extension == ".meta")
                    {
                        continue;
                    }

                    AssetInfo info = GenAssetInfo(FullPathToRelative(fileInfo.FullName));
                    info.bindObj = new Regex(AssetDatabase.AssetPathToGUID(info.fileRelativePath));

                    info.displayName = String.Empty;
                    var assetImporter = AssetImporter.GetAtPath(info.fileRelativePath);
                    TextureImporter textureImporter = assetImporter as TextureImporter;
                    if (textureImporter)
                    {
                        info.displayName = textureImporter.spritePackingTag;
                    }

                    rooResInfo.AddChild(info);
                }
            }

            CheckDirMatch(rooInfo, rooResInfo);

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
                dirInfo.AddChild(info);
            }
        }

        private void CheckFileMatch(AssetInfo dirInfo, AssetInfo beCheckInfo, string fileText)
        {
            if (dirInfo.isFolder)
            {
                if (dirInfo.hasChildren)
                {
                    foreach (var info in dirInfo.children)
                    {
                        CheckFileMatch(info, beCheckInfo, fileText);
                    }
                }
            }
            else
            {
                var regex = dirInfo.bindObj as Regex;
                if (regex != null && regex.IsMatch(fileText))
                {
                    AssetInfo info = GenAssetInfo(dirInfo.fileRelativePath);
                    info.bindObj = dirInfo.displayName;
                    info.isExtra = true;
                    beCheckInfo.AddChild(info);
                }
            }
        }

        private void CheckDirMatch(AssetInfo dirInfo, AssetInfo dirCheckInfo)
        {
            if (dirInfo.isFolder)
            {
                if (dirInfo.hasChildren)
                {
                    foreach (var info in dirInfo.children)
                    {
                        CheckDirMatch(info, dirCheckInfo);
                    }
                }
            }
            else
            {
                if (!AssetDanshariUtility.IsPlainTextExt(dirInfo.fileRelativePath))
                {
                    return;
                }

                do
                {
                    try
                    {
                        string text = File.ReadAllText(dirInfo.fileRelativePath);
                        CheckFileMatch(dirCheckInfo, dirInfo, text);
                    }
                    catch (Exception e)
                    {
                        var style = AssetDanshariStyle.Get();
                        if (EditorUtility.DisplayDialog(style.errorTitle, dirInfo.fileRelativePath + "\n" + e.Message,
                            style.continueStr, style.cancelStr))
                        {
                            continue;
                        }
                    }
                    break;
                } while (false);
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
            sb.AppendFormat("\"{0}\",", style.nameHeaderContent.text);
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