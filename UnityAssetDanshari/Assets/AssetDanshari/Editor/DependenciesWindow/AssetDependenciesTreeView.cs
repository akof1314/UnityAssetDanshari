using System;
using UnityEngine;
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
            var root = new TreeViewItem { id = -1, depth = -1, displayName = "Root" };

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

                    if (info.beDependPaths != null)
                    {
                        foreach (var beDependPath in info.beDependPaths)
                        {
                            var item2 = new AssetTreeViewItem<AssetTreeModel.AssetInfo>(GetAutoReverseID(), -1, String.Empty, beDependPath);
                            item.AddChild(item2);
                        }
                    }
                }
            }
        }

        protected override AssetTreeModel.AssetInfo GetItemAssetInfo(TreeViewItem item)
        {
            var item2 = item as AssetTreeViewItem<AssetDependenciesTreeModel.FileBeDependInfo>;
            if (item2 != null)
            {
                return item2.data;
            }
            var item3 = item as AssetTreeViewItem<AssetDependenciesTreeModel.DirInfo>;
            if (item3 != null)
            {
                return item3.data;
            }
            return base.GetItemAssetInfo(item);
        }

        protected override void RowGUI(RowGUIArgs args)
        {
            var item = args.item as AssetTreeViewItem<AssetDependenciesTreeModel.FileBeDependInfo>;
            if (item != null)
            {
                for (int i = 0; i < args.GetNumVisibleColumns(); ++i)
                {
                    CellGUI(args.GetCellRect(i), item, args.GetColumn(i), ref args);
                }
                return;
            }

            var item2 = args.item as AssetTreeViewItem<AssetTreeModel.AssetInfo>;
            if (item2 != null)
            {
                for (int i = 0; i < args.GetNumVisibleColumns(); ++i)
                {
                    CellGUI2(args.GetCellRect(i), item2, args.GetColumn(i), ref args);
                }
                return;
            }

            base.RowGUI(args);
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
                    int count = info.GetBeDependCount();
                    if (count > 0)
                    {
                        DefaultGUI.Label(cellRect, count.ToString(), args.selected, args.focused);
                    }
                    break;
            }
        }

        private void CellGUI2(Rect cellRect, AssetTreeViewItem<AssetTreeModel.AssetInfo> item, int column, ref RowGUIArgs args)
        {
            var info = item.data;

            switch (column)
            {
                case 0:
                    break;
                case 1:
                    DrawItemWithIcon(cellRect, item, ref args, info.displayName, info.fileRelativePath, false, false);
                    break;
            }
        }

        protected override bool CanMultiSelect(TreeViewItem item)
        {
            return true;
        }

        protected override void ContextClickedItem(int id)
        {
            var item = FindItem(id, rootItem);
            var assetInfo = GetItemAssetInfo(item);
            if (item == null || assetInfo == null || assetInfo.deleted)
            {
                return;
            }

            GenericMenu menu = new GenericMenu();
            if (!IsSelectionMulti())
            {
                menu.AddItem(AssetDanshariStyle.Get().duplicateContextLocation, false, OnContextSetActiveItem, id);
                menu.AddItem(AssetDanshariStyle.Get().duplicateContextExplorer, false, OnContextExplorerActiveItem, item);
                menu.AddSeparator(String.Empty);
            }

            if (!IsSelectionContainsReverseItem())
            {
                AddContextMoveComm(menu);
            }

            if (menu.GetItemCount() > 0)
            {
                menu.ShowAsContext();
            }
        }
    }
}