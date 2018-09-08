using System;
using UnityEngine;
using System.Collections;

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
    }
}