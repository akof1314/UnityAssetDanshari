using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace AssetDanshari
{
    public class AssetDuplicateWindow : EditorWindow
    {
        public static void CheckPaths(string refPaths, string paths, string commonPaths)
        {
            var window = GetWindow<AssetDuplicateWindow>();
            window.Focus();
            window.SetCheckPaths(refPaths, paths, commonPaths);
        }

        private SearchField m_SearchField;
        private SearchField m_SearchField2;
        private AssetTreeModel m_TreeModel;
        private AssetTreeView m_TreeView;

        [SerializeField]
        private TreeViewState m_TreeViewState;
        [SerializeField]
        private MultiColumnHeaderState m_MultiColumnHeaderState;

        private void Awake()
        {
            titleContent = AssetDanshariStyle.Get().duplicateTitle;
            minSize = new Vector2(727f, 331f);
        }

        private void OnEnable()
        {
            Init();
        }

        private void OnGUI()
        {
            var style = AssetDanshariStyle.Get();

            if (m_TreeModel.data != null)
            {
                EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
                if (GUILayout.Button(style.expandAll, EditorStyles.toolbarButton, GUILayout.Width(50f)))
                {
                    m_TreeView.ExpandAll();
                }
                if (GUILayout.Button(style.collapseAll, EditorStyles.toolbarButton, GUILayout.Width(50f)))
                {
                    m_TreeView.CollapseAll();
                }
                m_TreeView.searchString = m_SearchField.OnToolbarGUI(m_TreeView.searchString);
                EditorGUILayout.Space();
                EditorGUILayout.LabelField(style.assetReferenceAsset, GUILayout.Width(52f));
                m_SearchField2.OnToolbarGUI(m_TreeModel.assetPaths);
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
            m_SearchField2 = new SearchField();
            m_SearchField2.searchFieldControlID++;
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

        private void SetCheckPaths(string refPaths, string paths, string commonPaths)
        {
            RemoveNotification();
            m_TreeModel.SetDataPaths(refPaths, paths, commonPaths);
            m_TreeView.Reload();
            m_TreeView.ExpandAll();
        }
    }
}
