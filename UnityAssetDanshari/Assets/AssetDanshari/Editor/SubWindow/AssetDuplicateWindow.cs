using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace AssetDanshari
{
    public class AssetDuplicateWindow : EditorWindow
    {
        public static void CheckPaths(string[] paths)
        {
            var window = GetWindow<AssetDuplicateWindow>();
            window.SetCheckPaths(paths);
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
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            m_TreeView.searchString = m_SearchField.OnToolbarGUI(m_TreeView.searchString);
            EditorGUILayout.EndHorizontal();
            m_TreeView.OnGUI(GUILayoutUtility.GetRect(0, 100000, 0, 100000));
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
            var columns = new MultiColumnHeaderState.Column[3];
            for (int i = 0; i < columns.Length; i++)
            {
                columns[i] = new MultiColumnHeaderState.Column
                {
                    headerContent = new GUIContent((i).ToString()),
                    minWidth = 70f,
                    canSort = false,
                };
            }

            return new MultiColumnHeaderState(columns);
        }

        private void SetCheckPaths(string[] paths)
        {
            m_TreeModel.SetDataPaths(paths);
            m_TreeView.Reload();
        }
    }
}
