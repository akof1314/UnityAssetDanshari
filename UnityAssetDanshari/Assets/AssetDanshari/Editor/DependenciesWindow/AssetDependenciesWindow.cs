using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace AssetDanshari
{
    public class AssetDependenciesWindow : AssetBaseWindow
    {
        private void Awake()
        {
            titleContent = AssetDanshariStyle.Get().dependenciesTitle;
            minSize = new Vector2(727f, 331f);
        }

        protected override void InitTree(MultiColumnHeader multiColumnHeader)
        {
            m_AssetTreeModel = new AssetDependenciesTreeModel();
            m_AssetTreeView = new AssetDependenciesTreeView(m_TreeViewState, multiColumnHeader, m_AssetTreeModel);
        }

        protected override void DrawGUI(GUIContent waiting, GUIContent nothing)
        {
            base.DrawGUI(AssetDanshariStyle.Get().dependenciesWaiting, AssetDanshariStyle.Get().dependenciesNothing);
        }

        protected override MultiColumnHeaderState CreateMultiColumnHeader()
        {
            var columns = new[]
            {
                new MultiColumnHeaderState.Column
                {
                    headerContent = AssetDanshariStyle.Get().dependenciesHeaderContent,
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
                    headerContent = AssetDanshariStyle.Get().dependenciesHeaderContent2,
                    headerTextAlignment = TextAlignment.Left,
                    canSort = false,
                    width = 300,
                    minWidth = 100,
                    autoResize = false,
                    allowToggleVisibility = true
                },
                new MultiColumnHeaderState.Column
                {
                    headerContent = AssetDanshariStyle.Get().dependenciesHeaderContent3,
                    headerTextAlignment = TextAlignment.Left,
                    canSort = false,
                    width = 60,
                    minWidth = 60,
                    autoResize = false,
                    allowToggleVisibility = true
                }
            };

            return new MultiColumnHeaderState(columns);
        }
    }
}