using UnityEngine;
using System.Collections;
using UnityEditor;

namespace AssetDanshari
{
    public class AssetTreeModel
    {
        /// <summary>
        /// 右下角的路径
        /// </summary>
        public string assetPaths { get; protected set; }

        /// <summary>
        /// 是否存在数据
        /// </summary>
        /// <returns></returns>
        public virtual bool HasData()
        {
            return false;
        }

        public virtual void SetDataPaths(string refPathStr, string pathStr, string commonPathStr)
        {

        }

        public virtual void ExportCsv()
        {

        }

        public void PingObject(string fileRelativePath)
        {
            var obj = AssetDatabase.LoadAssetAtPath<Object>(fileRelativePath);
            if (obj)
            {
                Selection.activeObject = obj;
                EditorGUIUtility.PingObject(obj);
            }
        }
    }
}