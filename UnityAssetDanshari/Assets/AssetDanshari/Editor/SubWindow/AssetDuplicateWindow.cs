using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace AssetDanshari
{
    public class AssetDuplicateWindow : EditorWindow
    {
        public static void CheckPaths(string[] refPaths, string[] paths, string[] commonPaths)
        {
            var window = GetWindow<AssetDuplicateWindow>();
            window.Focus();
            window.SetCheckPaths(refPaths, paths, commonPaths);
        }

        private SearchField m_SearchField;
        private AssetTreeModel m_TreeModel;
        private AssetTreeView m_TreeView;

        [SerializeField]
        private TreeViewState m_TreeViewState;
        [SerializeField]
        private MultiColumnHeaderState m_MultiColumnHeaderState;

        private void Awake()
        {
            titleContent = AssetDanshariStyle.Get().duplicateTitle;
        }

        private void OnEnable()
        {
            Init();
        }

        private void OnGUI()
        {
            if (m_TreeModel.data != null)
            {
                EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
                if (GUILayout.Button(AssetDanshariStyle.Get().expandAll, EditorStyles.toolbarButton, GUILayout.Width(60f)))
                {
                    m_TreeView.ExpandAll();
                }
                if (GUILayout.Button(AssetDanshariStyle.Get().collapseAll, EditorStyles.toolbarButton, GUILayout.Width(60f)))
                {
                    m_TreeView.CollapseAll();
                }
                m_TreeView.searchString = m_SearchField.OnToolbarGUI(m_TreeView.searchString);
                EditorGUILayout.EndHorizontal();
                m_TreeView.OnGUI(GUILayoutUtility.GetRect(0, 100000, 0, 100000));
            }
            else
            {
                ShowNotification(AssetDanshariStyle.Get().duplicateNothing);
            }
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

            var multiColumnHeader = new AssetMultiColumnHeader(headerState);
            if (firstInit)
            {
                multiColumnHeader.ResizeToFit();
            }

            m_TreeModel = new AssetTreeModel();
            m_TreeView = new AssetTreeView(m_TreeViewState, multiColumnHeader, m_TreeModel);

            m_SearchField = new SearchField();
        }

        private MultiColumnHeaderState CreateMultiColumnHeader()
        {
            var columns = new[]
            {
                new MultiColumnHeaderState.Column
                {
                    headerContent = AssetDanshariStyle.Get().duplicateHeaderContent,
                    headerTextAlignment = TextAlignment.Left,
                    canSort = false,
                    sortingArrowAlignment = TextAlignment.Right,
                    width = 200,
                    minWidth = 150,
                    autoResize = false,
                    allowToggleVisibility = false
                },
                new MultiColumnHeaderState.Column
                {
                    headerContent = AssetDanshariStyle.Get().duplicateHeaderContent2,
                    headerTextAlignment = TextAlignment.Left,
                    canSort = false,
                    width = 300,
                    minWidth = 100,
                    autoResize = false,
                    allowToggleVisibility = true
                },
                new MultiColumnHeaderState.Column
                {
                    headerContent = AssetDanshariStyle.Get().duplicateHeaderContent3,
                    headerTextAlignment = TextAlignment.Left,
                    canSort = false,
                    width = 60,
                    minWidth = 60,
                    autoResize = false,
                    allowToggleVisibility = true
                },
                new MultiColumnHeaderState.Column
                {
                    headerContent = AssetDanshariStyle.Get().duplicateHeaderContent4,
                    headerTextAlignment = TextAlignment.Left,
                    canSort = false,
                    width = 110,
                    minWidth = 60,
                    autoResize = true
                }
            };

            return new MultiColumnHeaderState(columns);
        }

        private void SetCheckPaths(string[] refPaths, string[] paths, string[] commonPaths)
        {
            RemoveNotification();
            m_TreeModel.SetDataPaths(refPaths, paths, commonPaths);
            m_TreeView.Reload();
            m_TreeView.ExpandAll();
        }
    }
}
