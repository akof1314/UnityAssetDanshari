using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace AssetDanshari
{
    public class AssetTreeView : TreeView
    {
        private AssetTreeModel m_Model;
        private List<TreeViewItem> m_Rows;

        public AssetTreeView(TreeViewState state, MultiColumnHeader multiColumnHeader, AssetTreeModel model) : base(state, multiColumnHeader)
        {
            m_Model = model;
			rowHeight = 20f;
            showAlternatingRowBackgrounds = true;
            showBorder = true;
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
                    var groupItem = new AssetTreeViewItem<IGrouping<string, AssetTreeModel.FileMd5Info>>(id++, -1, 
                        String.Format(AssetDanshariStyle.Get().duplicateGroup, group.Count()), group);
                    root.AddChild(groupItem);

                    foreach (var info in group)
                    {
                        var infoItem = new AssetTreeViewItem<AssetTreeModel.FileMd5Info>(id++, -1, info.displayName, info);
                        groupItem.AddChild(infoItem);
                    }
                }
            }

            SetupDepthsFromParentsAndChildren(root);
            return root;
        }

        protected override void RowGUI(RowGUIArgs args)
        {
            var item = args.item as AssetTreeViewItem<AssetTreeModel.FileMd5Info>;
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

        private void CellGUI(Rect cellRect, AssetTreeViewItem<AssetTreeModel.FileMd5Info> item, int column, ref RowGUIArgs args)
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
                    if (iconForItem == null)
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
                        cellRect.xMin += 18f;
                        item.icon = iconForItem as Texture2D;
                    }

                    DefaultGUI.Label(cellRect, info.displayName, args.selected, args.focused);
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
            var item = FindItem(id, rootItem) as AssetTreeViewItem<AssetTreeModel.FileMd5Info>;
            if (item == null)
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
            var item = FindItem(id, rootItem) as AssetTreeViewItem<AssetTreeModel.FileMd5Info>;
            if (item == null)
            {
                return;
            }

            GenericMenu menu = new GenericMenu();
            menu.AddItem(AssetDanshariStyle.Get().duplicateContextLocation, false, OnContextSetActiveItem, id);
            menu.AddSeparator(String.Empty);
            menu.AddItem(AssetDanshariStyle.Get().duplicateContextUseThis, false, OnContextUseThisItem, item);
            if (m_Model.dirs != null)
            {
                foreach (var dir in m_Model.dirs)
                {
                    menu.AddItem(new GUIContent(AssetDanshariStyle.Get().duplicateContextMoveComm + dir.displayName), false, OnContextMoveItem, item);
                }
            }
            menu.AddItem(AssetDanshariStyle.Get().duplicateContextDelOther, false, OnContextRemoveAllOther, item);
            menu.ShowAsContext();
        }

        private void OnContextSetActiveItem(object userdata)
        {
            DoubleClickedItem((int)userdata);
        }

        private void OnContextUseThisItem(object userdata)
        {
            var item = userdata as AssetTreeViewItem<AssetTreeModel.FileMd5Info>;
            if (item != null)
            {
                var itemParent = item.parent as AssetTreeViewItem<IGrouping<string, AssetTreeModel.FileMd5Info>>;
                m_Model.SetUseThis(itemParent.data, item.data);
                Repaint();
            }
        }

        private void OnContextMoveItem(object userdata)
        {
        }

        private void OnContextRemoveAllOther(object userdata)
        {
        }
    }
}