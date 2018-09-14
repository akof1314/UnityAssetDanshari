using System;
using System.IO;
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

        protected override void CellGUI(Rect cellRect, AssetTreeViewItem<AssetTreeModel.AssetInfo> item, int column, ref RowGUIArgs args)
        {
            var info = item.data;

            switch (column)
            {
                case 0:
                    if (!info.isExtra)
                    {
                        DrawItemWithIcon(cellRect, item, ref args, info.displayName, info.fileRelativePath, info.deleted);
                    }
                    break;
                case 1:
                    if (info.isExtra)
                    {
                        DrawItemWithIcon(cellRect, item, ref args, info.displayName, info.fileRelativePath, false, false);
                    }
                    else
                    {
                        if (info.hasChildren && info.children.Count > 0)
                        {
                            DefaultGUI.Label(cellRect, info.children.Count.ToString(), args.selected, args.focused);
                        }
                    }
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

            if (watcherItems.Count == 0)
            {
                return true;
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

            // 排序，以防止先处理了文件
            watcherItems.Sort((a, b) =>
            {
                var aa = GetItemAssetInfo(a);
                var bb = GetItemAssetInfo(b);
                if (aa != null && bb != null)
                {
                    return EditorUtility.NaturalCompare(aa.fileRelativePath, bb.fileRelativePath);
                }

                return EditorUtility.NaturalCompare(a.displayName, b.displayName);
            });

            foreach (var watcherItem in watcherItems)
            {
                var assetInfo = GetItemAssetInfo(watcherItem);
                if (assetInfo == null)
                {
                    continue;
                }
                var item = FindItemByAssetPath(rootItem, Path.GetDirectoryName(assetInfo.fileRelativePath));
                if (item == null)
                {
                    continue;
                }
                var assetInfo2 = GetItemAssetInfo(item);
                if (assetInfo2 == null)
                {
                    continue;
                }

                item.AddChild(watcherItem);
                assetInfo2.AddChild(assetInfo);
            }
            SetupDepthsFromParentsAndChildren(rootItem);
            SortTreeViewNaturalCompare(rootItem);

            return true;
        }

        #endregion
    }
}