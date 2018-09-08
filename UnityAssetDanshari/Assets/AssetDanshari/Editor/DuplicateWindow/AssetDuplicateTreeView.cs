using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace AssetDanshari
{
    public class AssetDuplicateTreeView : TreeView
    {
        private AssetDuplicateTreeModel m_Model;
        private List<TreeViewItem> m_Rows;

        public AssetDuplicateTreeView(TreeViewState state, MultiColumnHeader multiColumnHeader, AssetDuplicateTreeModel model) : base(state, multiColumnHeader)
        {
            m_Model = model;
            rowHeight = 20f;
            showAlternatingRowBackgrounds = true;
            showBorder = true;
            multiColumnHeader.height = 23f;
        }

        protected override TreeViewItem BuildRoot()
        {
            var root = new TreeViewItem { id = 0, depth = -1, displayName = "Root" };

            int id = 1;
            if (m_Model != null && m_Model.data != null)
            {
                var groups = m_Model.data;
                foreach (var group in groups)
                {
                    var groupItem = new AssetTreeViewItem<IGrouping<string, AssetDuplicateTreeModel.FileMd5Info>>(id++, -1,
                        String.Format(AssetDanshariStyle.Get().duplicateGroup, group.Count()), group);
                    root.AddChild(groupItem);

                    foreach (var info in group)
                    {
                        var infoItem = new AssetTreeViewItem<AssetDuplicateTreeModel.FileMd5Info>(id++, -1, info.displayName, info);
                        groupItem.AddChild(infoItem);
                    }
                }
            }

            SetupDepthsFromParentsAndChildren(root);
            return root;
        }

        protected override void RowGUI(RowGUIArgs args)
        {
            var item = args.item as AssetTreeViewItem<AssetDuplicateTreeModel.FileMd5Info>;
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

        private void CellGUI(Rect cellRect, AssetTreeViewItem<AssetDuplicateTreeModel.FileMd5Info> item, int column, ref RowGUIArgs args)
        {
            var info = item.data;

            switch (column)
            {
                case 0:
                    float num = GetFoldoutIndent(item);
                    cellRect.xMin += num;

                    Rect position = cellRect;
                    position.width = 16f;
                    position.height = 16f;
                    position.y += 2f;
                    Texture iconForItem = item.icon;
                    if (iconForItem == null && !info.deleted)
                    {
                        iconForItem = AssetDatabase.GetCachedIcon(info.fileRelativePath);
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
                    DefaultGUI.Label(cellRect, info.displayName, args.selected, args.focused);
                    if (info.deleted)
                    {
                        position.x = cellRect.xMax - 40f;
                        position.y += 3f;
                        position.height = 9f;
                        position.width = 40f;
                        GUI.DrawTexture(position, AssetDanshariStyle.Get().duplicateDelete.image, ScaleMode.ScaleToFit);
                    }
                    break;
                case 1:
                    DefaultGUI.Label(cellRect, info.fileRelativePath, args.selected, args.focused);
                    break;
                case 2:
                    DefaultGUI.Label(cellRect, info.fileLength, args.selected, args.focused);
                    break;
                case 3:
                    DefaultGUI.Label(cellRect, info.fileTime, args.selected, args.focused);
                    break;
            }
        }

        protected override bool CanMultiSelect(TreeViewItem item)
        {
            return false;
        }

        protected override void DoubleClickedItem(int id)
        {
            var item = FindItem(id, rootItem) as AssetTreeViewItem<AssetDuplicateTreeModel.FileMd5Info>;
            if (item == null || item.data.deleted)
            {
                return;
            }

            var obj = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(item.data.fileRelativePath);
            if (obj)
            {
                Selection.activeObject = obj;
                EditorGUIUtility.PingObject(obj);
            }
        }

        protected override void ContextClickedItem(int id)
        {
            var item = FindItem(id, rootItem) as AssetTreeViewItem<AssetDuplicateTreeModel.FileMd5Info>;
            if (item == null || item.data.deleted)
            {
                return;
            }

            GenericMenu menu = new GenericMenu();
            menu.AddItem(AssetDanshariStyle.Get().duplicateContextLocation, false, OnContextSetActiveItem, id);
            menu.AddItem(AssetDanshariStyle.Get().duplicateContextExplorer, false, OnContextExplorerActiveItem, item);
            menu.AddSeparator(String.Empty);
            menu.AddItem(AssetDanshariStyle.Get().duplicateContextUseThis, false, OnContextUseThisItem, item);
            if (m_Model.dirs != null)
            {
                foreach (var dir in m_Model.dirs)
                {
                    menu.AddItem(new GUIContent(AssetDanshariStyle.Get().duplicateContextMoveComm + dir.displayName), false, OnContextMoveItem, dir.fileRelativePath);
                }
            }
            menu.AddItem(AssetDanshariStyle.Get().duplicateContextDelOther, false, OnContextRemoveAllOther, item);
            menu.ShowAsContext();
        }

        private void OnContextSetActiveItem(object userdata)
        {
            DoubleClickedItem((int)userdata);
        }

        private void OnContextExplorerActiveItem(object userdata)
        {
            var item = userdata as AssetTreeViewItem<AssetDuplicateTreeModel.FileMd5Info>;
            if (item != null)
            {
                EditorUtility.RevealInFinder(item.data.fileRelativePath);
            }
        }

        private void OnContextUseThisItem(object userdata)
        {
            var item = userdata as AssetTreeViewItem<AssetDuplicateTreeModel.FileMd5Info>;
            if (item != null)
            {
                var itemParent = item.parent as AssetTreeViewItem<IGrouping<string, AssetDuplicateTreeModel.FileMd5Info>>;
                m_Model.SetUseThis(itemParent.data, item.data);
                Repaint();
            }
        }

        private void OnContextMoveItem(object userdata)
        {
            var selects = GetSelection();
            if (selects.Count > 0)
            {
                var item = FindItem(selects[0], rootItem) as AssetTreeViewItem<AssetDuplicateTreeModel.FileMd5Info>;
                if (item == null)
                {
                    return;
                }

                var dirPath = userdata as string;
                m_Model.SetMoveToCommon(item.data, dirPath);
            }
        }

        private void OnContextRemoveAllOther(object userdata)
        {
            var item = userdata as AssetTreeViewItem<AssetDuplicateTreeModel.FileMd5Info>;
            if (item != null)
            {
                var itemParent = item.parent as AssetTreeViewItem<IGrouping<string, AssetDuplicateTreeModel.FileMd5Info>>;
                m_Model.SetRemoveAllOther(itemParent.data, item.data);
                Repaint();
            }
        }
    }
}