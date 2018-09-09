using System;
using UnityEngine;
using System.IO;
using System.Text;
using UnityEditor;

namespace AssetDanshari
{
    public static class AssetDanshariUtility
    {
        public static string[] PathStrToArray(string paths)
        {
            paths = paths.Trim('\"');
            return paths.Split(new[] { "\" || \"" }, StringSplitOptions.RemoveEmptyEntries);
        }

        public static string PathArrayToStr(string[] paths)
        {
            var pathStr = '\"' + string.Join("\" || \"", paths) + '\"';
            return pathStr;
        }

        public static bool IsPlainTextExt(string ext)
        {
            ext = ext.ToLower();
            return ext.EndsWith(".prefab") || ext.EndsWith(".unity") ||
                   ext.EndsWith(".mat") || ext.EndsWith(".asset") ||
                   ext.EndsWith(".controller") || ext.EndsWith(".anim");
        }

        public static string GetSaveFilePath(string key)
        {
            string path = EditorPrefs.GetString("RecentSaveFilePath" + key, Application.dataPath + key + ".csv");
            path = EditorUtility.SaveFilePanel("Save File ..", Path.GetDirectoryName(path), Path.GetFileName(path), "csv");
            path = path.Replace('\\', '/');
            if (!string.IsNullOrEmpty(path))
            {
                EditorPrefs.SetString("RecentSaveFilePath" + key, path);
            }
            return path;
        }

        public static void SaveFileText(string path, string text)
        {
            try
            {
                File.WriteAllText(path, text, Encoding.UTF8);
                System.Diagnostics.Process.Start(path);
            }
            catch (Exception e)
            {
                EditorUtility.DisplayDialog(AssetDanshariStyle.Get().errorTitle, e.Message, AssetDanshariStyle.Get().sureStr);
                throw;
            }
        }
    }
}