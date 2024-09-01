using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace AssetDanshari
{
    public static class AssetDanshariHandlerDemo
    {
        [InitializeOnLoadMethod]
        private static void InitOnLoad()
        {
            AssetDanshariHandler.onCreateSetting += OnCreateSetting;
            AssetDanshariHandler.onDependenciesLoadDataMore += OnDependenciesLoadDataMore;
            AssetDanshariHandler.onDependenciesContextDraw += OnDependenciesContextDraw;
        }

        private static AssetDanshariSetting OnCreateSetting()
        {
            var setting = ScriptableObject.CreateInstance<AssetDanshariSetting>();
            setting.ripgrepPath = "Assets/Samples/Asset Danshari/1.0.0/Simple Demo/rg/rg.exe";
            setting.assetReferenceInfos.Add(new AssetDanshariSetting.AssetReferenceInfo()
            {
                referenceFolder = "\"Assets/Samples/Asset Danshari/1.0.0/Simple Demo/Prefab\" || \"Assets/Samples/Asset Danshari/1.0.0/Simple Demo/Samples\"",
                assetFolder = "\"Assets/Samples/Asset Danshari/1.0.0/Simple Demo/PNG\"",
                assetCommonFolder = "\"Assets/Samples/Asset Danshari/1.0.0/Simple Demo/PNG/Common\""
            });
            return setting;
        }

        private static void OnDependenciesLoadDataMore(string resPath, List<AssetTreeModel.AssetInfo> resInfos, AssetTreeModel treeModel)
        {
            // 去代码定义文件去查找
            if (resPath != "\"Assets/Samples/Asset Danshari/1.0.0/Simple Demo/PNG\"")
            {
                return;
            }

            int preLen = resPath.Length - "PNG".Length - 2;
            string codePath = "Assets/Samples/Asset Danshari/1.0.0/Simple Demo/Scripts/Editor/UISpriteDefine.cs";
            try
            {
                string text = File.ReadAllText(codePath);

                foreach (var assetInfo in resInfos)
                {
                    string searchText = assetInfo.fileRelativePath.Substring(preLen);
                    searchText = searchText.Remove(searchText.Length - 4);

                    if (text.Contains(searchText))
                    {
                        AssetTreeModel.AssetInfo info = treeModel.GenAssetInfo(codePath);
                        info.isExtra = true;
                        assetInfo.AddChild(info);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError(ex.Message);
            }

            string csvPath = "Assets/Samples/Asset Danshari/1.0.0/Simple Demo/Csv/image_config.csv";
            try
            {
                string text = File.ReadAllText(csvPath);

                foreach (var assetInfo in resInfos)
                {
                    string searchText = assetInfo.fileRelativePath.Substring(preLen);
                    searchText = searchText.Remove(searchText.Length - 4);

                    if (text.Contains(searchText))
                    {
                        AssetTreeModel.AssetInfo info = treeModel.GenAssetInfo(csvPath);
                        info.isExtra = true;
                        assetInfo.AddChild(info);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError(ex.Message);
            }
        }

        private static void OnDependenciesContextDraw(GenericMenu menu)
        {
            menu.AddSeparator(String.Empty);
            menu.AddItem(new GUIContent("生成代码定义/C# 定义"), false, OnDependenciesContextGenCodeCSharp);
            menu.AddItem(new GUIContent("生成代码定义/Lua 定义"), false, OnDependenciesContextGenCodeLua);
        }

        private static void OnDependenciesContextGenCodeCSharp()
        {

        }

        private static void OnDependenciesContextGenCodeLua()
        {

        }
    }
}