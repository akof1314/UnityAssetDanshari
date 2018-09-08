using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace AssetDanshari
{
    public class AssetDanshariStyle
    {
        public class Style
        {
            public GUIContent assetReferenceTitle = new GUIContent("检查列表");
            public GUIContent assetReferenceAsset = new GUIContent("资源目录", "存放资源的文件夹路径");
            public GUIContent assetReferenceAssetCommon = new GUIContent("公共资源目录", "整理资源时所放置的公共文件夹路径");
            public GUIContent assetReferenceReference = new GUIContent("引用目录", "使用到资源的预制文件夹路径");
            public GUIContent assetReferenceCheckDup = new GUIContent("检查重复");
            public GUIContent assetReferenceCheckRef = new GUIContent("引用查找");

            public GUIContent duplicateTitle = new GUIContent("重复资源检查");
            public GUIContent duplicateNothing = new GUIContent("所检查的文件夹没有重复资源");
            public GUIContent duplicateHeaderContent = new GUIContent("名称");
            public GUIContent duplicateHeaderContent2 = new GUIContent("路径");
            public GUIContent duplicateHeaderContent3 = new GUIContent("大小");
            public GUIContent duplicateHeaderContent4 = new GUIContent("创建时间");
            public GUIContent duplicateDelete = EditorGUIUtility.IconContent("AS Badge Delete");
            public GUIContent duplicateContextLocation = new GUIContent("定位");
            public GUIContent duplicateContextExplorer = new GUIContent("打开所在文件夹");
            public GUIContent duplicateContextUseThis = new GUIContent("仅使用此资源");
            public GUIContent duplicateContextDelOther = new GUIContent("删除其余资源");
            public string duplicateContextMoveComm = "移入公共目录/";
            public string duplicateGroup = "文件数：{0}";

            public string progressTitle = "正在处理";
            public string errorTitle = "错误信息";
            public string continueStr = "继续执行";
            public string cancelStr = "取消";
            public string sureStr = "确定";
            public string progressFinish = "处理结束";
            public GUIContent expandAll = new GUIContent("展开");
            public GUIContent collapseAll = new GUIContent("折叠");

            public string toolbarSeachTextField = "ToolbarSeachTextField";
            public GUIStyle toolbarSeachTextFieldStyle;
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