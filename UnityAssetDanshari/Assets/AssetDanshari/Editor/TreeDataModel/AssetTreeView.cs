using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.IMGUI.Controls;

namespace AssetDanshari
{
    public class AssetTreeView : TreeView
    {
        protected AssetTreeModel m_Model;
        protected List<TreeViewItem> m_Rows;

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
    }
}