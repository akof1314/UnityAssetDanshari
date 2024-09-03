using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace AssetDanshari
{
    public class AssetDanshariWindow : EditorWindow
    {
        [MenuItem("美术工具/资源断舍离")]
        static void ShowWindow()
        {
            GetWindow<AssetDanshariWindow>();
        }

        private Vector2 m_ScrollViewVector2;
        private ReorderableList m_ReorderableList;
        private bool m_IsForceText;
        private bool m_ShowGrepSetting;


        private void Awake()
        {
            titleContent.text = "资源断舍离";
            minSize = new Vector2(600, 340);
        }

        private void OnGUI()
        {
            Init();
            var style = AssetDanshariStyle.Get();

            if (!m_IsForceText)
            {
                EditorGUILayout.LabelField(style.forceText);
                return;
            }

            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            GUILayout.FlexibleSpace();
            EditorGUI.BeginChangeCheck();
            m_ShowGrepSetting = GUILayout.Toggle(m_ShowGrepSetting, string.IsNullOrEmpty(AssetDanshariSetting.Get().ripgrepPath) ?
                    style.grepNotSet : style.grepEnabled, EditorStyles.toolbarButton);
            if (EditorGUI.EndChangeCheck())
            {
                AssetDanshariSetting.SaveSetting();
            }
            EditorGUILayout.EndHorizontal();
            if (m_ShowGrepSetting)
            {
                EditorGUILayout.BeginHorizontal();
                AssetDanshariSetting.Get().ripgrepPath = EditorGUILayout.TextField(style.grepPath, AssetDanshariSetting.Get().ripgrepPath);
                if (GUILayout.Button("O", GUILayout.ExpandWidth(false)))
                {
                    var path = EditorUtility.OpenFilePanel(style.grepPath.text, "", "*");
                    if (!string.IsNullOrEmpty(path))
                    {
                        GUI.FocusControl(null);
                        AssetDanshariSetting.Get().ripgrepPath = path;
                    }
                }
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.Space();
            }

            if (m_ReorderableList != null)
            {
                m_ScrollViewVector2 = GUILayout.BeginScrollView(m_ScrollViewVector2);
                m_ReorderableList.DoLayoutList();
                GUILayout.EndScrollView();
            }
        }

        private void Init()
        {
            if (m_ReorderableList == null)
            {
                m_IsForceText = EditorSettings.serializationMode == SerializationMode.ForceText;
                if (!m_IsForceText)
                {
                    return;
                }
            }

            if (m_ReorderableList == null)
            {
                m_ReorderableList = new ReorderableList(AssetDanshariSetting.Get().assetReferenceInfos, null, true, true, true, true);
                m_ReorderableList.drawHeaderCallback = OnDrawHeaderCallback;
                m_ReorderableList.drawElementCallback = OnDrawElementCallback;
                m_ReorderableList.elementHeight += 60;
            }
        }

        private void OnDrawHeaderCallback(Rect rect)
        {
            EditorGUI.LabelField(rect, AssetDanshariStyle.Get().assetReferenceTitle);
        }

        private void OnDrawElementCallback(Rect rect, int index, bool isactive, bool isfocused)
        {
            if (AssetDanshariSetting.Get() == null || AssetDanshariSetting.Get().assetReferenceInfos.Count < index)
            {
                return;
            }

            var style = AssetDanshariStyle.Get();
            var info = AssetDanshariSetting.Get().assetReferenceInfos[index];
            rect.height = EditorGUIUtility.singleLineHeight;
            rect.y += 2;

            EditorGUI.BeginChangeCheck();
            info.title = EditorGUI.TextField(rect, info.title);
            rect.y += EditorGUIUtility.singleLineHeight + 2;

            Rect rect2 = new Rect(rect) { width = 85f };
            Rect rect3 = new Rect(rect) { x = rect2.x + rect2.width, width = rect.width - rect2.width - 150f };
            Rect rect4 = new Rect(rect) { x = rect3.x + rect3.width + 5f, width = 70f };
            Rect rect5 = new Rect(rect) { x = rect4.x + rect4.width + 5f, width = 70f };
            EditorGUI.LabelField(rect2, style.assetReferenceReference);
            info.referenceFolder = EditorGUI.TextField(rect3, info.referenceFolder);
            info.referenceFolder = OnDrawElementAcceptDrop(rect3, info.referenceFolder);
            bool valueChanged = EditorGUI.EndChangeCheck();
            if (GUI.Button(rect4, style.assetReferenceCheckRef))
            {
                AssetDanshariSetting.SaveSetting();
                DisplayReferenceWindow(info.referenceFolder, info.assetFolder, info.assetCommonFolder);
            }

            rect2.y += EditorGUIUtility.singleLineHeight + 2;
            rect3.y += EditorGUIUtility.singleLineHeight + 2;
            rect4.y += EditorGUIUtility.singleLineHeight + 2;
            rect5.y += EditorGUIUtility.singleLineHeight + 2;
            EditorGUI.LabelField(rect2, style.assetReferenceAsset);
            EditorGUI.BeginChangeCheck();
            info.assetFolder = EditorGUI.TextField(rect3, info.assetFolder);
            info.assetFolder = OnDrawElementAcceptDrop(rect3, info.assetFolder);
            valueChanged |= EditorGUI.EndChangeCheck();
            if (GUI.Button(rect4, style.assetReferenceCheckDup))
            {
                AssetDanshariSetting.SaveSetting();
                DisplayDuplicateWindow(info.referenceFolder, info.assetFolder, info.assetCommonFolder);
            }
            if (GUI.Button(rect5, style.assetReferenceDepend))
            {
                AssetDanshariSetting.SaveSetting();
                DisplayDependenciesWindow(info.referenceFolder, info.assetFolder, info.assetCommonFolder);
            }

            rect2.y += EditorGUIUtility.singleLineHeight + 2;
            rect3.y += EditorGUIUtility.singleLineHeight + 2;
            rect3.width = rect.width - rect2.width;
            EditorGUI.LabelField(rect2, style.assetReferenceAssetCommon);
            EditorGUI.BeginChangeCheck();
            info.assetCommonFolder = EditorGUI.TextField(rect3, info.assetCommonFolder);
            info.assetCommonFolder = OnDrawElementAcceptDrop(rect3, info.assetCommonFolder);
            valueChanged |= EditorGUI.EndChangeCheck();
        }

        private string OnDrawElementAcceptDrop(Rect rect, string label)
        {
            if (rect.Contains(Event.current.mousePosition))
            {
                if (DragAndDrop.paths != null && DragAndDrop.paths.Length > 0 && !string.IsNullOrEmpty(DragAndDrop.paths[0]))
                {
                    if (Event.current.type == EventType.DragUpdated || Event.current.type == EventType.DragPerform)
                    {
                        DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
                    }

                    if (Event.current.type == EventType.DragPerform)
                    {
                        DragAndDrop.AcceptDrag();
                        GUI.changed = true;

                        //  安装ctrl表示添加
                        if (Event.current.modifiers == EventModifiers.Control)
                        {
                            var labels = AssetDanshariUtility.PathStrToArray(label);
                            ArrayUtility.AddRange(ref labels, DragAndDrop.paths);
                            return AssetDanshariUtility.PathArrayToStr(labels);
                        }
                        else
                        {
                            return AssetDanshariUtility.PathArrayToStr(DragAndDrop.paths);
                        }
                    }
                }
            }

            return label;
        }

        /// <summary>
        /// 显示引用查找窗口
        /// </summary>
        /// <param name="refPaths">引用的文件、目录集合</param>
        /// <param name="resPaths">资源的文件、目录集合</param>
        /// <param name="commonPaths">公共资源目录集合</param>
        public static void DisplayReferenceWindow(string refPaths, string resPaths, string commonPaths = "")
        {
            AssetBaseWindow.CheckPaths<AssetReferenceWindow>(refPaths, resPaths, commonPaths, AssetDanshariSetting.Get().ripgrepPath);
        }

        /// <summary>
        /// 显示被引用查找窗口
        /// </summary>
        public static void DisplayDependenciesWindow(string refPaths, string resPaths, string commonPaths = "")
        {
            AssetBaseWindow.CheckPaths<AssetDependenciesWindow>(refPaths, resPaths, commonPaths, AssetDanshariSetting.Get().ripgrepPath);
        }

        /// <summary>
        /// 显示重复资源检查窗口
        /// </summary>
        public static void DisplayDuplicateWindow(string refPaths, string resPaths, string commonPaths = "")
        {
            AssetBaseWindow.CheckPaths<AssetDuplicateWindow>(refPaths, resPaths, commonPaths, AssetDanshariSetting.Get().ripgrepPath);
        }
    }
}