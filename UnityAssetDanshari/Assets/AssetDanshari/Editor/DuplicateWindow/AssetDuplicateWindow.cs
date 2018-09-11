using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace AssetDanshari
{
    public class AssetDuplicateWindow : AssetBaseWindow
    {
        private void Awake()
        {
            titleContent = AssetDanshariStyle.Get().duplicateTitle;
            minSize = new Vector2(727f, 331f);
        }

        protected override void InitTree(MultiColumnHeader multiColumnHeader)
        {
            m_AssetTreeModel = new AssetDuplicateTreeModel();
            m_AssetTreeView = new AssetDuplicateTreeView(m_TreeViewState, multiColumnHeader, m_AssetTreeModel);
        }

        protected override void DrawGUI(GUIContent waiting, GUIContent nothing)
        {
            base.DrawGUI(AssetDanshariStyle.Get().duplicateWaiting, AssetDanshariStyle.Get().duplicateNothing);
        }

        protected override MultiColumnHeaderState CreateMultiColumnHeader()
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
    }
}
