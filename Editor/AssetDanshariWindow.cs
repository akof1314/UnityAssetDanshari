using System.IO;
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

        private AssetDanshariSetting m_AssetDanshariSetting;
        private Vector2 m_ScrollViewVector2;
        private ReorderableList m_ReorderableList;
        private bool m_IsForceText;
        private bool m_ShowGrepSetting;

        private static readonly string kUserSettingsPath = "UserSettings/AssetDanshariSetting.asset";

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
            m_ShowGrepSetting = GUILayout.Toggle(m_ShowGrepSetting, string.IsNullOrEmpty(m_AssetDanshariSetting.ripgrepPath) ?
                    style.grepNotSet : style.grepEnabled, EditorStyles.toolbarButton);
            if (EditorGUI.EndChangeCheck())
            {
                SaveSetting();
            }
            EditorGUILayout.EndHorizontal();
            if (m_ShowGrepSetting)
            {
                EditorGUILayout.BeginHorizontal();
                m_AssetDanshariSetting.ripgrepPath = EditorGUILayout.TextField(style.grepPath, m_AssetDanshariSetting.ripgrepPath);
                if (GUILayout.Button("O", GUILayout.ExpandWidth(false)))
                {
                    var path = EditorUtility.OpenFilePanel(style.grepPath.text, "", "*");
                    if (!string.IsNullOrEmpty(path))
                    {
                        GUI.FocusControl(null);
                        m_AssetDanshariSetting.ripgrepPath = path;
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
            if (m_AssetDanshariSetting == null)
            {
                m_IsForceText = EditorSettings.serializationMode == SerializationMode.ForceText;
                if (!m_IsForceText)
                {
                    return;
                }

                Object[] objects = InternalEditorUtility.LoadSerializedFileAndForget(kUserSettingsPath);
                if (objects != null && objects.Length > 0)
                {
                    m_AssetDanshariSetting = objects[0] as AssetDanshariSetting;
                }
                if (m_AssetDanshariSetting == null)
                {
                    string[] guids = AssetDatabase.FindAssets("t:" + typeof(AssetDanshariSetting).Name);
                    if (guids.Length > 0)
                    {
                        string path = AssetDatabase.GUIDToAssetPath(guids[0]);
                        m_AssetDanshariSetting = AssetDatabase.LoadAssetAtPath<AssetDanshariSetting>(path);
                    }
                }
                if (m_AssetDanshariSetting == null)
                {
                    if (AssetDanshariHandler.onCreateSetting != null)
                    {
                        m_AssetDanshariSetting = AssetDanshariHandler.onCreateSetting();
                    }
                    else
                    {
                        m_AssetDanshariSetting = CreateInstance<AssetDanshariSetting>();
                    }
                    SaveSetting();
                }
            }

            if (m_ReorderableList == null)
            {
                m_ReorderableList = new ReorderableList(m_AssetDanshariSetting.assetReferenceInfos, null, true, true, true, true);
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
            if (m_AssetDanshariSetting == null || m_AssetDanshariSetting.assetReferenceInfos.Count < index)
            {
                return;
            }

            var style = AssetDanshariStyle.Get();
            var info = m_AssetDanshariSetting.assetReferenceInfos[index];
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
                SaveSetting();
                AssetBaseWindow.CheckPaths<AssetReferenceWindow>(info.referenceFolder,
                    info.assetFolder, info.assetCommonFolder, m_AssetDanshariSetting.ripgrepPath);
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
                SaveSetting();
                AssetBaseWindow.CheckPaths<AssetDuplicateWindow>(info.referenceFolder,
                    info.assetFolder, info.assetCommonFolder, m_AssetDanshariSetting.ripgrepPath);
            }
            if (GUI.Button(rect5, style.assetReferenceDepend))
            {
                SaveSetting();
                AssetBaseWindow.CheckPaths<AssetDependenciesWindow>(info.referenceFolder,
                    info.assetFolder, info.assetCommonFolder, m_AssetDanshariSetting.ripgrepPath);
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

        private void SaveSetting()
        {
            if (m_AssetDanshariSetting == null)
            {
                return;
            }

            var settingPath = AssetDatabase.GetAssetPath(m_AssetDanshariSetting);
            if (!string.IsNullOrEmpty(settingPath))
            {
                return;
            }

            string folderPath = Path.GetDirectoryName(kUserSettingsPath);
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            InternalEditorUtility.SaveToSerializedFileAndForget(new[] { m_AssetDanshariSetting }, kUserSettingsPath, true);
        }
    }
}