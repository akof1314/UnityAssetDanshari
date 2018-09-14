using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace AssetDanshari
{
    public class AssetDuplicateTreeModel : AssetTreeModel
    {
        public class FileMd5Info
        {
            public string filePath;
            public string fileLength;
            public string fileTime;
            public long fileSize;
            public string md5;
        }

        public override void SetDataPaths(string refPathStr, string pathStr, string commonPathStr)
        {
            data = null;
            ResetAutoId();
            base.SetDataPaths(refPathStr, pathStr, commonPathStr);
            var rooInfo = new AssetInfo(GetAutoId(), String.Empty, String.Empty);
            var style = AssetDanshariStyle.Get();
            var fileList = new List<FileMd5Info>();

            foreach (var path in resPaths)
            {
                if (!Directory.Exists(path))
                {
                    continue;
                }

                EditorUtility.DisplayProgressBar(style.progressTitle, String.Empty, 0f);
                var allFiles = Directory.GetFiles(path, "*", SearchOption.AllDirectories);
                for (var i = 0; i < allFiles.Length;)
                {
                    FileInfo fileInfo = new FileInfo(allFiles[i]);
                    if (fileInfo.Extension == ".meta")
                    {
                        i++;
                        continue;
                    }

                    EditorUtility.DisplayProgressBar(style.progressTitle, fileInfo.Name, i * 1f / allFiles.Length);
                    try
                    {
                        using (var md5 = MD5.Create())
                        {
                            using (var stream = File.OpenRead(fileInfo.FullName))
                            {
                                FileMd5Info info = new FileMd5Info();
                                info.filePath = fileInfo.FullName;
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
                        if (!EditorUtility.DisplayDialog(style.errorTitle, path + "\n" + e.Message,
                            style.continueStr, style.cancelStr))
                        {
                            EditorUtility.ClearProgressBar();
                            return;
                        }
                    }
                }
            }

            var groups = fileList.GroupBy(info => info.md5).Where(g => g.Count() > 1);
            foreach (var group in groups)
            {
                AssetInfo dirInfo = new AssetInfo(GetAutoId(), String.Empty, String.Format(style.duplicateGroup, group.Count()));
                dirInfo.isExtra = true;
                rooInfo.AddChild(dirInfo);

                foreach (var member in group)
                {
                    AssetInfo info = new AssetInfo(GetAutoId(), FullPathToRelative(member.filePath), Path.GetFileName(member.filePath));
                    info.bindObj = member;
                    dirInfo.AddChild(info);

                    if (member.fileSize >= (1 << 20))
                    {
                        member.fileLength = string.Format("{0:F} Mb", member.fileSize / 1024f / 1024f);
                    }
                    else if (member.fileSize >= (1 << 10))
                    {
                        member.fileLength = string.Format("{0:F} Kb", member.fileSize / 1024f);
                    }
                }
            }

            data = rooInfo;
            EditorUtility.ClearProgressBar();
        }

        /// <summary>
        /// 去引用到的目录查找所有用到的guid，批量更改
        /// </summary>
        /// <param name="group"></param>
        /// <param name="useInfo"></param>
        public void SetUseThis(AssetInfo group, AssetInfo useInfo)
        {
            var style = AssetDanshariStyle.Get();

            string patternStr = String.Empty;
            foreach (var info in group.children)
            {
                if (info != useInfo)
                {
                    patternStr += String.Format("({0})|", AssetDatabase.AssetPathToGUID(info.fileRelativePath));
                }
            }

            patternStr = patternStr.TrimEnd('|');
            Regex pattern = new Regex(patternStr);
            string replaceStr = AssetDatabase.AssetPathToGUID(useInfo.fileRelativePath);

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

        public void SetRemoveAllOther(AssetInfo group, AssetInfo selectInfo)
        {
            var style = AssetDanshariStyle.Get();
            if (!EditorUtility.DisplayDialog(String.Empty, style.sureStr + style.duplicateContextDelOther.text,
                style.sureStr, style.cancelStr))
            {
                return;
            }

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
            EditorUtility.DisplayDialog(String.Empty, style.progressFinish, style.sureStr);
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

                    FileMd5Info md5Info = info.bindObj as FileMd5Info;
                    sb.AppendFormat("\"{0}\",", md5Info.fileLength);
                    sb.AppendFormat("\"{0}\"\n", md5Info.fileTime);
                }
            }

            AssetDanshariUtility.SaveFileText(path, sb.ToString());
            GUIUtility.ExitGUI();
        }
    }
}