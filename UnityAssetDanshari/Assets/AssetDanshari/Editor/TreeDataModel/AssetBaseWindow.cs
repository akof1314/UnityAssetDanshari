using UnityEngine;
using UnityEditor;
using UnityEditor.IMGUI.Controls;

namespace AssetDanshari
{
    public class AssetBaseWindow : EditorWindow
    {
        public static void CheckPaths<T>(string refPaths, string paths, string commonPaths) where T : AssetBaseWindow
        {
            var window = GetWindow<T>();
            window.Focus();
            window.SetCheckPaths(refPaths, paths, commonPaths);
        }

        private SearchField m_SearchField;
        protected AssetTreeModel m_AssetTreeModel;
        protected AssetTreeView m_AssetTreeView;

        [SerializeField]
        protected TreeViewState m_TreeViewState;
        [SerializeField]
        private MultiColumnHeaderState m_MultiColumnHeaderState;

        private void OnEnable()
        {
            Init();
        }

        private void OnGUI()
        {
            DrawGUI(GUIContent.none, GUIContent.none);
        }

        private void Init()
        {
            if (m_SearchField != null)
            {
                return;
            }

            if (m_TreeViewState == null)
            {
                m_TreeViewState = new TreeViewState();
            }

            bool firstInit = m_MultiColumnHeaderState == null;
            var headerState = CreateMultiColumnHeader();
            if (MultiColumnHeaderState.CanOverwriteSerializedFields(m_MultiColumnHeaderState, headerState))
                MultiColumnHeaderState.OverwriteSerializedFields(m_MultiColumnHeaderState, headerState);
            m_MultiColumnHeaderState = headerState;

            var multiColumnHeader = new MultiColumnHeader(headerState);
            if (firstInit)
            {
                multiColumnHeader.ResizeToFit();
            }

            m_SearchField = new SearchField();
            InitTree(multiColumnHeader);
        }

        protected virtual void InitTree(MultiColumnHeader multiColumnHeader)
        {
            m_AssetTreeModel = new AssetTreeModel();
            m_AssetTreeView = new AssetTreeView(m_TreeViewState, multiColumnHeader, m_AssetTreeModel);
        }

        protected virtual void DrawGUI(GUIContent waiting, GUIContent nothing)
        {
            var style = AssetDanshariStyle.Get();
            style.InitGUI();

            if (m_AssetTreeModel.assetPaths != null)
            {
                if (!m_AssetTreeModel.HasData())
                {
                    ShowNotification(nothing);
                    GUILayout.FlexibleSpace();
                }
                else
                {
                    EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
                    if (GUILayout.Button(style.expandAll, EditorStyles.toolbarButton, GUILayout.Width(50f)))
                    {
                        m_AssetTreeView.ExpandAll();
                    }
                    if (GUILayout.Button(style.collapseAll, EditorStyles.toolbarButton, GUILayout.Width(50f)))
                    {
                        m_AssetTreeView.CollapseAll();
                    }
                    m_AssetTreeView.searchString = m_SearchField.OnToolbarGUI(m_AssetTreeView.searchString);
                    if (GUILayout.Button(style.exportCsv, EditorStyles.toolbarButton, GUILayout.Width(70f)))
                    {
                        m_AssetTreeModel.ExportCsv();
                    }
                    EditorGUILayout.EndHorizontal();
                    m_AssetTreeView.OnGUI(GUILayoutUtility.GetRect(0, 100000, 0, 100000));
                }
                EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
                GUILayout.FlexibleSpace();
                EditorGUILayout.LabelField(m_AssetTreeModel.assetPaths, style.labelStyle);
                EditorGUILayout.EndHorizontal();
            }
            else
            {
                ShowNotification(waiting);
            }
        }

        protected virtual MultiColumnHeaderState CreateMultiColumnHeader()
        {
            return null;
        }

        private void SetCheckPaths(string refPaths, string paths, string commonPaths)
        {
            RemoveNotification();
            m_AssetTreeModel.SetDataPaths(refPaths, paths, commonPaths);
            if (m_AssetTreeModel.HasData())
            {
                m_AssetTreeView.Reload();
                m_AssetTreeView.ExpandAll();
            }
        }
    }
}
