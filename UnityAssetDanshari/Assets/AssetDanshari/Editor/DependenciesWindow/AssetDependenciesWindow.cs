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

        protected override void DrawGUI(GUIContent waiting, GUIContent nothing, bool expandCollapseComplex)
        {
            base.DrawGUI(AssetDanshariStyle.Get().dependenciesWaiting, AssetDanshariStyle.Get().dependenciesNothing, true);
        }

        protected override MultiColumnHeaderState CreateMultiColumnHeader()
        {
            var columns = new[]
            {
                new MultiColumnHeaderState.Column
                {
                    headerContent = AssetDanshariStyle.Get().nameHeaderContent,
                    headerTextAlignment = TextAlignment.Left,
                    canSort = false,
                    sortingArrowAlignment = TextAlignment.Right,
                    width = 280,
                    minWidth = 150,
                    autoResize = false,
                    allowToggleVisibility = false
                },
                new MultiColumnHeaderState.Column
                {
                    headerContent = AssetDanshariStyle.Get().dependenciesHeaderContent2,
                    headerTextAlignment = TextAlignment.Left,
                    canSort = false,
                    width = 350,
                    minWidth = 100,
                    autoResize = false,
                    allowToggleVisibility = true
                }
            };

            return new MultiColumnHeaderState(columns);
        }
    }
}