using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace AssetDanshari
{
	//[CreateAssetMenu (fileName = "AssetDanshariSetting", menuName = "AssetDanshari Setting", order = 1)]
    public class AssetDanshariSetting : ScriptableObject
    {
        public string ripgrepPath;

	    [Serializable]
        public class AssetReferenceInfo
        {
            public string title = String.Empty;
            public string referenceFolder = String.Empty;
            public string assetFolder = String.Empty;
            public string assetCommonFolder = String.Empty;
        }

        [SerializeField]
        private List<AssetReferenceInfo> m_AssetReferenceInfos = new List<AssetReferenceInfo>();

        public List<AssetReferenceInfo> assetReferenceInfos
        {
            get { return m_AssetReferenceInfos; }
        }

        private static readonly string kUserSettingsPath = "UserSettings/AssetDanshariSetting.asset";

        private static AssetDanshariSetting sSetting;

        public static AssetDanshariSetting Get()
        {
            if (sSetting == null)
            {
                LoadSetting();
            }

            return sSetting;
        }

        private static void LoadSetting()
        {
            if (sSetting != null)
            {
                return;
            }

            UnityEngine.Object[] objects = InternalEditorUtility.LoadSerializedFileAndForget(kUserSettingsPath);
            if (objects != null && objects.Length > 0)
            {
                sSetting = objects[0] as AssetDanshariSetting;
            }
            if (sSetting == null)
            {
                string[] guids = AssetDatabase.FindAssets("t:" + typeof(AssetDanshariSetting).Name);
                if (guids.Length > 0)
                {
                    string path = AssetDatabase.GUIDToAssetPath(guids[0]);
                    sSetting = AssetDatabase.LoadAssetAtPath<AssetDanshariSetting>(path);
                }
            }
            if (sSetting == null)
            {
                if (AssetDanshariHandler.onCreateSetting != null)
                {
                    sSetting = AssetDanshariHandler.onCreateSetting();
                }
                else
                {
                    sSetting = CreateInstance<AssetDanshariSetting>();
                }
                SaveSetting();
            }
        }

        public static void SaveSetting()
        {
            if (sSetting == null)
            {
                return;
            }

            var settingPath = AssetDatabase.GetAssetPath(sSetting);
            if (!string.IsNullOrEmpty(settingPath))
            {
                return;
            }

            string folderPath = Path.GetDirectoryName(kUserSettingsPath);
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            InternalEditorUtility.SaveToSerializedFileAndForget(new[] { sSetting }, kUserSettingsPath, true);
        }
    }
}