using UnityEngine;
using UnityEditor;

namespace AssetDanshari
{
    public class AssetTreeModel
    {
        /// <summary>
        /// 右下角的路径
        /// </summary>
        public string assetPaths { get; protected set; }

        protected string[] refPaths { get; private set; }
        protected string[] resPaths { get; private set; }
        protected string[] commonPaths { get; private set; }

        private int m_DataPathLen = 0;

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
            assetPaths = pathStr;
            refPaths = AssetDanshariUtility.PathStrToArray(refPathStr);
            resPaths = AssetDanshariUtility.PathStrToArray(pathStr);
            commonPaths = AssetDanshariUtility.PathStrToArray(commonPathStr);
            m_DataPathLen = Application.dataPath.Length - 6;
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

        public string FullPathToRelative(string path)
        {
            return path.Substring(m_DataPathLen).Replace('\\', '/');
        }
    }
}