using System;
using System.Collections;
using System.Collections.Generic;
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
    }
}