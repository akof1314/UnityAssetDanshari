using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AssetDanshari
{
    public class AssetDanshariStyle
    {
        public class Style
        {
            public GUIContent assetReferenceTitle = new GUIContent("检查列表");
            public GUIContent assetReferenceAsset = new GUIContent("资源文件夹");
            public GUIContent assetReferenceReference = new GUIContent("引用文件夹");
            public GUIContent assetReferenceCheckDup = new GUIContent("检查重复");
            public GUIContent assetReferenceCheckRef = new GUIContent("引用查找");

            public GUIContent duplicateTitle = new GUIContent("重复资源检查");

            public string errorTitle = "错误信息";
            public string continueStr = "继续执行";
            public string cancelStr = "取消";
        }

        private static Style sStyle;

        public static Style Get()
        {
            if (sStyle == null)
            {
                sStyle = new Style();
            }

            return sStyle;
        }
    }
}