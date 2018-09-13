using System;
using System.IO;
using System.Linq;
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
            if (model != null && model.data != null)
            {
                foreach (var info in model.data.children)
                {
                    BuildDataDir(info, root);
                }
            }

            SetupDepthsFromParentsAndChildren(root);
            return root;
        }

        private void BuildDataDir(AssetTreeModel.AssetInfo dirInfo, TreeViewItem parent)
        {
            var info = dirInfo as AssetDependenciesTreeModel.FileBeDependInfo;
            if (info != null)
            {
                var item = new AssetTreeViewItem<AssetDependenciesTreeModel.FileBeDependInfo>(GetAutoID(), -1, info.displayName, info);
                parent.AddChild(item);

                if (info.beDependPaths != null)
                {
                    foreach (var beDependPath in info.beDependPaths)
                    {
                        var item2 = new AssetTreeViewItem<AssetTreeModel.AssetInfo>(GetAutoReverseID(), -1, String.Empty, beDependPath);
                        item.AddChild(item2);
                    }
                }
            }
            else
            {
                var dirItem = new AssetTreeViewItem<AssetTreeModel.AssetInfo>(GetAutoID(), -1, dirInfo.displayName, dirInfo);
                dirItem.icon = AssetDanshariStyle.Get().folderIcon;
                parent.AddChild(dirItem);

                if (dirInfo.hasChildren)
                {
                    foreach (var childInfo in dirInfo.children)
                    {
                        BuildDataDir(childInfo, dirItem);
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
            if (item2 != null && IsReverseItem(item2.id))
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

        #region  数据变化
        
        protected override bool OnWatcherMovedAssetsEvent(string[] movedFromAssetPaths, string[] movedAssets)
        {
            bool ret = base.OnWatcherMovedAssetsEvent(movedFromAssetPaths, movedAssets);
            if (!ret)
            {
                return false;
            }

            // 先移除
            foreach (var watcherItem in watcherItems)
            {
                if (watcherItem.parent != null)
                {
                    watcherItem.parent.children.Remove(watcherItem);
                }

                watcherItem.parent = null;

                var assetInfo = GetItemAssetInfo(watcherItem);
                if (assetInfo != null)
                {
                    if (assetInfo.parent != null)
                    {
                        assetInfo.parent.children.Remove(assetInfo);
                    }

                    assetInfo.parent = null;
                }
            }

            return true;
        }

        #endregion
    }
}