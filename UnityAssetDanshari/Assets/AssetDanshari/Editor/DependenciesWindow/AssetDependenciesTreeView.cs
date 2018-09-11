using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.IMGUI.Controls;

namespace AssetDanshari
{
    public class AssetDependenciesTreeView : AssetTreeView
    {
        private AssetDependenciesTreeModel model { get; set; }

        public AssetDependenciesTreeView(TreeViewState state, MultiColumnHeader multiColumnHeader, AssetTreeModel model) : base(state, multiColumnHeader, model)
        {
            this.model = model as AssetDependenciesTreeModel;
        }

        protected override TreeViewItem BuildRoot()
        {
            var root = new TreeViewItem { id = 0, depth = -1, displayName = "Root" };

            int id = 1;
            if (model != null && model.data != null)
            {
                foreach (var info in model.data)
                {
                    var infoItem = new AssetTreeViewItem<AssetDependenciesTreeModel.FileBeDependInfo>(id++, -1, info.displayName, info);
                    root.AddChild(infoItem);
                }
            }

            SetupDepthsFromParentsAndChildren(root);
            return root;
        }
    }
}