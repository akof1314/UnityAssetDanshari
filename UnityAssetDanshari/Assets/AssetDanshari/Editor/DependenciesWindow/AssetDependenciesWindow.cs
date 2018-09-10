using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace AssetDanshari
{
    public class AssetDependenciesWindow : EditorWindow
    {
        public static void CheckPaths(string refPaths, string paths, string commonPaths)
        {
            var window = GetWindow<AssetDependenciesWindow>();
            window.Focus();
        }
    }
}