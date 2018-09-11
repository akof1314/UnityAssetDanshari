using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.IMGUI.Controls;

namespace AssetDanshari
{
    public class AssetTreeView : TreeView
    {
        protected AssetTreeModel m_Model;
        protected List<TreeViewItem> m_Rows;
        private int m_Id = 0;

        public AssetTreeView(TreeViewState state, MultiColumnHeader multiColumnHeader, AssetTreeModel model) : base(state, multiColumnHeader)
        {
            m_Model = model;
            rowHeight = 20f;
            showAlternatingRowBackgrounds = true;
            showBorder = true;
            multiColumnHeader.height = 23f;
        }

        protected override TreeViewItem BuildRoot()
        {
            return null;
        }


        protected void ResetAutoID()
        {
            m_Id = 0;
        }

        protected int GetAutoID()
        {
            return m_Id++;
        }

        protected void DrawItemWithIcon(Rect cellRect, TreeViewItem item, ref RowGUIArgs args,
            string displayName, string fileRelativePath, bool deleted)
        {
            float num = GetFoldoutIndent(item);
            cellRect.xMin += num;

            Rect position = cellRect;
            position.width = 16f;
            position.height = 16f;
            position.y += 2f;
            Texture iconForItem = item.icon;
            if (iconForItem == null && !deleted)
            {
                iconForItem = AssetDatabase.GetCachedIcon(fileRelativePath);
                if (iconForItem)
                {
                    item.icon = iconForItem as Texture2D;
                }
            }
            if (iconForItem)
            {
                GUI.DrawTexture(position, iconForItem, ScaleMode.ScaleToFit);
                item.icon = iconForItem as Texture2D;
            }

            cellRect.xMin += 18f;
            DefaultGUI.Label(cellRect, displayName, args.selected, args.focused);
            if (deleted)
            {
                position.x = cellRect.xMax - 40f;
                position.y += 3f;
                position.height = 9f;
                position.width = 40f;
                GUI.DrawTexture(position, AssetDanshariStyle.Get().duplicateDelete.image, ScaleMode.ScaleToFit);
            }
        }
    }
}