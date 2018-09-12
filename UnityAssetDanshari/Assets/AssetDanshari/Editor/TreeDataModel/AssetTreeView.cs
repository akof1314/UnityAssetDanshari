using System;
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
        private int m_Id = 0;
        private int m_ReverseId = 0;
        protected List<TreeViewItem> m_WatcherItems = new List<TreeViewItem>();

        public AssetTreeView(TreeViewState state, MultiColumnHeader multiColumnHeader, AssetTreeModel model) : base(state, multiColumnHeader)
        {
            m_Model = model;
            rowHeight = 20f;
            showAlternatingRowBackgrounds = true;
            showBorder = true;
            multiColumnHeader.height = 23f;

            AssetDanshariWatcher.onImportedAssets += OnWatcherImportedAssets;
            AssetDanshariWatcher.onDeletedAssets += OnWatcherDeletedAssets;
            AssetDanshariWatcher.onMovedAssets += OnWatcherMovedAssets;
        }

        public void Destroy()
        {
            AssetDanshariWatcher.onImportedAssets -= OnWatcherImportedAssets;
            AssetDanshariWatcher.onDeletedAssets -= OnWatcherDeletedAssets;
            AssetDanshariWatcher.onMovedAssets -= OnWatcherMovedAssets;
        }

        protected override TreeViewItem BuildRoot()
        {
            return null;
        }


        protected void ResetAutoID()
        {
            m_Id = 0;
            m_ReverseId = Int32.MaxValue - 1;
        }

        protected int GetAutoID()
        {
            return m_Id++;
        }

        protected int GetAutoReverseID()
        {
            return m_ReverseId--;
        }

        protected virtual AssetTreeModel.AssetInfo GetItemAssetInfo(TreeViewItem item)
        {
            var item2 = item as AssetTreeViewItem<AssetTreeModel.AssetInfo>;
            if (item2 != null)
            {
                return item2.data;
            }

            return null;
        }

        protected void DrawItemWithIcon(Rect cellRect, TreeViewItem item, ref RowGUIArgs args,
            string displayName, string fileRelativePath, bool deleted, bool contentIndent = true, bool foldoutIndent = false)
        {
            if (contentIndent)
            {
                float num = GetContentIndent(item);
                cellRect.xMin += num;
            }

            if (foldoutIndent)
            {
                float num = GetFoldoutIndent(item);
                cellRect.xMin += num;
            }

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

        protected override void DoubleClickedItem(int id)
        {
            var assetInfo = GetItemAssetInfo(FindItem(id, rootItem));
            if (assetInfo == null || assetInfo.deleted)
            {
                return;
            }

            m_Model.PingObject(assetInfo.fileRelativePath);
        }


        protected void OnContextSetActiveItem(object userdata)
        {
            DoubleClickedItem((int)userdata);
        }

        protected void OnContextExplorerActiveItem(object userdata)
        {
            var assetInfo = GetItemAssetInfo((TreeViewItem)userdata);
            if (assetInfo == null || assetInfo.deleted)
            {
                return;
            }

            EditorUtility.RevealInFinder(assetInfo.fileRelativePath);
        }

        protected void AddContextMoveComm(GenericMenu menu)
        {
            if (m_Model.commonDirs != null)
            {
                foreach (var dir in m_Model.commonDirs)
                {
                    menu.AddItem(new GUIContent(AssetDanshariStyle.Get().duplicateContextMoveComm + dir.displayName), false, OnContextMoveItem, dir.fileRelativePath);
                }
            }
        }

        private void OnContextMoveItem(object userdata)
        {
            if (!HasSelection())
            {
                return;
            }

            var selects = GetSelection();
            foreach (var select in selects)
            {
                var assetInfo = GetItemAssetInfo(FindItem(select, rootItem));
                if (assetInfo == null || assetInfo.deleted)
                {
                    continue;
                }

                var dirPath = userdata as string;
                m_Model.SetMoveToCommon(assetInfo, dirPath);
            }
        }

        /// <summary>
        /// 展开全部，除了最后一层
        /// </summary>
        public void ExpandAllExceptLast()
        {
            ExpandAll();
            SetExpandedAtLast(rootItem, false);
        }

        public void CollapseOnlyLast()
        {
            SetExpandedAtLast(rootItem, false);
        }

        public bool SetExpandedAtLast(TreeViewItem item, bool expanded)
        {
            if (item.hasChildren)
            {
                foreach (var child in item.children)
                {
                    if (SetExpandedAtLast(child, expanded))
                    {
                        break;
                    }
                }
            }
            else if (IsReverseItem(item.id))
            {
                SetExpanded(item.parent.id, expanded);
                return true;
            }

            return false;
        }

        public bool IsReverseItem(int id)
        {
            return id >= m_ReverseId;
        }

        /// <summary>
        /// 选中包括了额外显示项
        /// </summary>
        /// <returns></returns>
        public bool IsSelectionContainsReverseItem()
        {
            var selects = GetSelection();
            foreach (var select in selects)
            {
                if (IsReverseItem(select))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// 是否选中了多个
        /// </summary>
        /// <returns></returns>
        public bool IsSelectionMulti()
        {
            var selects = GetSelection();
            return selects.Count > 1;
        }

        #region  数据变化

        private void OnWatcherImportedAssets(string[] importedAssets)
        {
            Debug.Log("importedAsset");
            foreach (var importedAsset in importedAssets)
            {
                Debug.Log(importedAsset);
            }
            if (OnWatcherImportedAssetsEvent(importedAssets))
            {
                Repaint();
            }
        }

        private void OnWatcherDeletedAssets(string[] deletedAssets)
        {
            Debug.Log("deletedAssets");
            foreach (var importedAsset in deletedAssets)
            {
                Debug.Log(importedAsset);
            }
            if (OnWatcherDeletedAssetsEvent(deletedAssets))
            {
                Repaint();
            }
        }

        private void OnWatcherMovedAssets(string[] movedFromAssetPaths, string[] movedAssets)
        {
            Debug.Log("movedAssets");
            foreach (var importedAsset in movedFromAssetPaths)
            {
                Debug.Log(importedAsset);
            }
            if (OnWatcherMovedAssetsEvent(movedFromAssetPaths, movedAssets))
            {
                Repaint();
            }
        }

        protected virtual bool OnWatcherImportedAssetsEvent(string[] importedAssets)
        {
            return false;
        }

        protected virtual bool OnWatcherDeletedAssetsEvent(string[] deletedAssets)
        {
            m_WatcherItems.Clear();
            FindItemsByAssetPaths(rootItem, deletedAssets, m_WatcherItems);
            return false;
        }

        protected virtual bool OnWatcherMovedAssetsEvent(string[] movedFromAssetPaths, string[] movedAssets)
        {
            m_WatcherItems.Clear();
            FindItemsByAssetPaths(rootItem, movedFromAssetPaths, m_WatcherItems);
            return false;
        }

        private void FindItemsByAssetPaths(TreeViewItem searchFromThisItem, string[] assetPaths, List<TreeViewItem> result)
        {
            if (searchFromThisItem == null)
            {
                return;
            }

            var assetInfo = GetItemAssetInfo(searchFromThisItem);
            if (assetInfo != null)
            {
                foreach (var assetPath in assetPaths)
                {
                    if (assetPath == assetInfo.fileRelativePath)
                    {
                        result.Add(searchFromThisItem);
                        break;
                    }
                }
            }

            if (searchFromThisItem.hasChildren)
            {
                foreach (var child in searchFromThisItem.children)
                {
                    FindItemsByAssetPaths(child, assetPaths, result);
                }
            }
        }

        #endregion

    }
}