using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;

namespace AssetDanshari
{
    public class AssetTreeModel
    {
        public class AssetInfo
        {
            public int id;
            public string fileRelativePath;
            public string displayName;
            public bool deleted;
            public bool added;
            public AssetInfo parent;
            public List<AssetInfo> children;

            public bool hasChildren
            {
                get
                {
                    return children != null && children.Count > 0;
                }
            }

            public void AddChild(AssetInfo info)
            {
                if (children == null)
                {
                    children = new List<AssetInfo>();
                }
                children.Add(info);
                info.parent = this;
            }
        }

        /// <summary>
        /// 右下角的路径
        /// </summary>
        public string assetPaths { get; protected set; }

        protected string[] refPaths { get; private set; }
        protected string[] resPaths { get; private set; }
        protected string[] commonPaths { get; private set; }

        /// <summary>
        /// 公共目录
        /// </summary>
        public List<AssetInfo> commonDirs { get; private set; }

        private int m_DataPathLen = 0;

        /// <summary>
        /// 是否存在数据
        /// </summary>
        /// <returns></returns>
        public virtual bool HasData()
        {
            return false;
        }

        public virtual AssetInfo FindData(int id)
        {
            return null;
        }

        public virtual void SetDataPaths(string refPathStr, string pathStr, string commonPathStr)
        {
            assetPaths = pathStr;
            refPaths = AssetDanshariUtility.PathStrToArray(refPathStr);
            resPaths = AssetDanshariUtility.PathStrToArray(pathStr);
            commonPaths = AssetDanshariUtility.PathStrToArray(commonPathStr);
            m_DataPathLen = Application.dataPath.Length - 6;

            commonDirs = new List<AssetInfo>();
            foreach (var commonPath in commonPaths)
            {
                if (!Directory.Exists(commonPath))
                {
                    continue;
                }

                var commonName = Path.GetFileNameWithoutExtension(commonPath);
                var commonLen = commonPath.Length - commonName.Length;
                commonDirs.Add(new AssetInfo()
                {
                    fileRelativePath = commonPath,
                    displayName = commonName
                });

                var allDirs = Directory.GetDirectories(commonPath, "*", SearchOption.AllDirectories);
                foreach (var allDir in allDirs)
                {
                    var dirInfo = new AssetInfo();
                    dirInfo.fileRelativePath = PathToStandardized(allDir);
                    dirInfo.displayName = dirInfo.fileRelativePath.Substring(commonLen);
                    commonDirs.Add(dirInfo);
                }
            }
        }

        public bool SetMoveToCommon(AssetInfo moveInfo, string destDir)
        {
            var style = AssetDanshariStyle.Get();
            string destPath = String.Format("{0}/{1}", destDir, moveInfo.displayName);
            if (moveInfo.fileRelativePath == destPath)
            {
                return true;
            }

            var errorStr = AssetDatabase.MoveAsset(moveInfo.fileRelativePath, destPath);
            if (!string.IsNullOrEmpty(errorStr))
            {
                EditorUtility.DisplayDialog(style.errorTitle, errorStr, style.sureStr);
                return false;
            }

            return true;
        }

        public virtual void ExportCsv()
        {

        }

        public void PingObject(string fileRelativePath)
        {
            var obj = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(fileRelativePath);
            if (obj)
            {
                Selection.activeObject = obj;
                EditorGUIUtility.PingObject(obj);
            }
        }

        public string FullPathToRelative(string path)
        {
            return PathToStandardized(path.Substring(m_DataPathLen));
        }

        public string PathToStandardized(string path)
        {
            return path.Replace('\\', '/');
        }
    }
}