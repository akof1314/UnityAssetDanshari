using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
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

            ResetAutoID();
            if (model != null && model.data != null && model.data.dirs != null)
            {
                foreach (var info in model.data.dirs)
                {
                    BuildDataDir(info, root);
                }
            }

            SetupDepthsFromParentsAndChildren(root);
            return root;
        }

        private void BuildDataDir(AssetDependenciesTreeModel.DirInfo dirInfo, TreeViewItem parent)
        {
            var dirItem = new AssetTreeViewItem<AssetDependenciesTreeModel.DirInfo>(GetAutoID(), -1, dirInfo.displayName, dirInfo);
            dirItem.icon = AssetDanshariStyle.Get().folderIcon;
            parent.AddChild(dirItem);

            if (dirInfo.dirs != null)
            {
                foreach (var info in dirInfo.dirs)
                {
                    BuildDataDir(info, dirItem);
                }
            }

            if (dirInfo.files != null)
            {
                foreach (var info in dirInfo.files)
                {
                    var item = new AssetTreeViewItem<AssetDependenciesTreeModel.FileBeDependInfo>(GetAutoID(), -1, info.displayName, info);
                    dirItem.AddChild(item);
                }
            }
        }

        protected override void RowGUI(RowGUIArgs args)
        {
            var item = args.item as AssetTreeViewItem<AssetDependenciesTreeModel.FileBeDependInfo>;
            if (item == null)
            {
                base.RowGUI(args);
                return;
            }

            for (int i = 0; i < args.GetNumVisibleColumns(); ++i)
            {
                CellGUI(args.GetCellRect(i), item, args.GetColumn(i), ref args);
            }
        }

        private void CellGUI(Rect cellRect, AssetTreeViewItem<AssetDependenciesTreeModel.FileBeDependInfo> item, int column, ref RowGUIArgs args)
        {
            var info = item.data;

            switch (column)
            {
                case 0:
                    DrawItemWithIcon(cellRect, item, ref args, info.displayName, info.fileRelativePath, info.deleted);
                    break;
                case 1:
                    DefaultGUI.Label(cellRect, info.fileRelativePath, args.selected, args.focused);
                    break;
                case 2:
                    int count = info.GetBeDependCount();
                    if (count > 0)
                    {
                        DefaultGUI.Label(cellRect, count.ToString(), args.selected, args.focused);
                    }
                    break;
            }
        }
    }
}