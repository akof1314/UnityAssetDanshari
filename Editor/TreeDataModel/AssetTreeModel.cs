﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
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
            public bool isFolder;
            public bool isExtra;
            public object bindObj;

            public bool deleted;
            public bool added;

            public AssetInfo parent;
            public List<AssetInfo> children;

            public AssetInfo(int id, string fileRelativePath, string displayName)
            {
                this.id = id;
                this.fileRelativePath = fileRelativePath;
                this.displayName = displayName;
            }

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
        /// 数据根
        /// </summary>
        public AssetInfo data { get; protected set; }

        /// <summary>
        /// 右下角的路径
        /// </summary>
        public string assetPaths { get; protected set; }
        protected string grepPath { get; private set; }

        protected string[] refPaths { get; private set; }
        protected string[] resPaths { get; private set; }
        protected string[] commonPaths { get; private set; }

        /// <summary>
        /// 公共目录
        /// </summary>
        public List<AssetInfo> commonDirs { get; private set; }

        private int m_DataPathLen = 0;
        private int m_Id = 0;

        protected void ResetAutoId()
        {
            m_Id = 0;
        }

        protected int GetAutoId()
        {
            return m_Id++;
        }

        /// <summary>
        /// 是否存在数据
        /// </summary>
        /// <returns></returns>
        public bool HasData()
        {
            return data != null;
        }

        public virtual void SetDataPaths(string refPathStr, string pathStr, string commonPathStr, string grepFilePath)
        {
            data = null;
            ResetAutoId();
            assetPaths = pathStr;
            refPaths = AssetDanshariUtility.PathStrToArray(refPathStr);
            resPaths = AssetDanshariUtility.PathStrToArray(pathStr);
            commonPaths = AssetDanshariUtility.PathStrToArray(commonPathStr);
            m_DataPathLen = Application.dataPath.Length - 6;
            grepPath = grepFilePath;

            commonDirs = new List<AssetInfo>();
            foreach (var commonPath in commonPaths)
            {
                if (!Directory.Exists(commonPath))
                {
                    continue;
                }

                var commonName = Path.GetFileNameWithoutExtension(commonPath);
                var commonLen = commonPath.Length - commonName.Length;
                commonDirs.Add(new AssetInfo(GetAutoId(), commonPath, commonName));

                var allDirs = Directory.GetDirectories(commonPath, "*", SearchOption.AllDirectories);
                foreach (var allDir in allDirs)
                {
                    var dirInfo = GenAssetInfo(PathToStandardized(allDir));
                    dirInfo.displayName = dirInfo.fileRelativePath.Substring(commonLen);
                    commonDirs.Add(dirInfo);
                }
            }
        }

        public AssetInfo GenAssetInfo(string assetPath)
        {
            AssetInfo info = new AssetInfo(GetAutoId(), PathToStandardized(assetPath), Path.GetFileName(assetPath));
            if (AssetDatabase.IsValidFolder(assetPath))
            {
                info.isFolder = true;
            }
            return info;
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
            if (string.IsNullOrEmpty(fileRelativePath))
            {
                return;
            }
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

        public static string PathToStandardized(string path)
        {
            return path.Replace('\\', '/');
        }

        #region 继承的方法

        /// <summary>
        /// 引用目录的文件列表
        /// </summary>
        /// <returns></returns>
        protected List<string> GetRefFileList()
        {
            List<string> fileList = new List<string>();

            foreach (var refPath in refPaths)
            {
                if (!Directory.Exists(refPath))
                {
                    // 不是目录，那么可能是资源文件
                    if (File.Exists(refPath))
                    {
                        fileList.Add(PathToStandardized(refPath));
                    }

                    continue;
                }

                EditorUtility.DisplayProgressBar(AssetDanshariStyle.Get().progressTitle, String.Empty, 0f);
                var allFiles = Directory.GetFiles(refPath, "*", SearchOption.AllDirectories);

                for (var i = 0; i < allFiles.Length; i++)
                {
                    var file = allFiles[i];
                    // 不止文本资源，二进制的meta文件也可能引用资源，比如fbx.
                    // 这里为了界面显示，只过滤掉meta文件，但实际搜索还是需要搜索
                    if (AssetDanshariUtility.IsMetaExt(file))
                    {
                        continue;
                    }

                    fileList.Add(PathToStandardized(file));
                }
            }

            return fileList;
        }

        /// <summary>
        /// 得到资源文件列表
        /// </summary>
        /// <returns></returns>
        protected List<string> GetResFileList()
        {
            List<string> fileList = new List<string>();

            foreach (var resPath in resPaths)
            {
                if (!Directory.Exists(resPath))
                {
                    // 不是目录，那么可能是资源文件
                    if (File.Exists(resPath))
                    {
                        fileList.Add(PathToStandardized(resPath));
                    }

                    continue;
                }

                EditorUtility.DisplayProgressBar(AssetDanshariStyle.Get().progressTitle, AssetDanshariStyle.Get().progressTime, 0f);
                var allFiles = Directory.GetFiles(resPath, "*", SearchOption.AllDirectories);

                for (var i = 0; i < allFiles.Length; i++)
                {
                    var file = allFiles[i];
                    if (AssetDanshariUtility.IsMetaExt(file))
                    {
                        continue;
                    }

                    fileList.Add(PathToStandardized(file));
                }
            }

            return fileList;
        }

        /// <summary>
        /// 获取文件列表的GUID
        /// </summary>
        /// <param name="fileList"></param>
        /// <returns></returns>
        protected List<string> GetGuidFromFileList(List<string> fileList)
        {
            List<string> guidList = new List<string>();
            foreach (var file in fileList)
            {
                guidList.Add(AssetDatabase.AssetPathToGUID(file));
            }

            return guidList;
        }

        /// <summary>
        /// 获取文件列表的GUID
        /// </summary>
        /// <param name="fileList"></param>
        protected Dictionary<string, string> GetGuidMapFromFileList(List<string> fileList)
        {
            Dictionary<string, string> guidList = new Dictionary<string, string>();
            foreach (var file in fileList)
            {
                if (!guidList.ContainsKey(file))
                {
                    guidList.Add(file, AssetDatabase.AssetPathToGUID(file));
                }
            }

            return guidList;
        }

        protected bool[][] GetSearchResultList(int fileCount, int searchCount)
        {
            var ret = new bool[fileCount][];
            for (int i = 0; i < fileCount; i++)
            {
                ret[i] = new bool[searchCount];
            }

            return ret;
        }

        /// <summary>
        /// 首先构建目录树
        /// </summary>
        protected AssetInfo DirToAssetInfoTree(string[] paths)
        {
            var rooInfo = new AssetInfo(GetAutoId(), String.Empty, String.Empty);
            rooInfo.isFolder = true;

            foreach (var path in paths)
            {
                if (!Directory.Exists(path))
                {
                    // 不是目录，那么可能是文件，文件的话就构造一个文件夹
                    if (File.Exists(path))
                    {
                        AssetInfo info2 = GenAssetInfo(Path.GetDirectoryName(path));
                        rooInfo.AddChild(info2);
                    }

                    continue;
                }

                AssetInfo info = GenAssetInfo(path);
                rooInfo.AddChild(info);
                DirToAssetInfoTreeSub(info, path);
            }

            return rooInfo;
        }

        private void DirToAssetInfoTreeSub(AssetInfo rootInfo, string path)
        {
            var allDirs = Directory.GetDirectories(path, "*", SearchOption.TopDirectoryOnly);
            foreach (var drDir in allDirs)
            {
                AssetInfo info = GenAssetInfo(drDir);
                rootInfo.AddChild(info);

                DirToAssetInfoTreeSub(info, drDir);
            }
        }

        /// <summary>
        /// 再把文件转成信息列表
        /// </summary>
        protected List<AssetInfo> FileListToAssetInfos(List<string> fileList)
        {
            var assetInfos = new List<AssetInfo>();
            foreach (var file in fileList)
            {
                AssetInfo info = GenAssetInfo(file);
                assetInfos.Add(info);
            }

            return assetInfos;
        }

        /// <summary>
        /// 最后合并文件夹和列表
        /// </summary>
        /// <param name="rootInfo"></param>
        /// <param name="infoList"></param>
        protected void MergeAssetInfo(AssetInfo rootInfo, List<AssetInfo> infoList)
        {
            foreach (var info in infoList)
            {
                var parent = FindAssetInfo(rootInfo, PathToStandardized(Path.GetDirectoryName(info.fileRelativePath)));
                if (parent != null)
                {
                    parent.AddChild(info);
                }
            }
        }

        /// <summary>
        /// 查找
        /// </summary>
        protected AssetInfo FindAssetInfo(AssetInfo searchFromThisItem, string assetPath)
        {
            if (searchFromThisItem == null)
            {
                return null;
            }

            if (searchFromThisItem.fileRelativePath == assetPath)
            {
                return searchFromThisItem;
            }

            if (searchFromThisItem.hasChildren)
            {
                foreach (var child in searchFromThisItem.children)
                {
                    var item = FindAssetInfo(child, assetPath);
                    if (item != null)
                    {
                        return item;
                    }
                }
            }
            return null;
        }

        #endregion

        #region  多线程执行

        private class JobBaseTextSearchReplace
        {
            public ManualResetEvent doneEvent;
            public string exception;
        }

        private class JobGrepTextSearchReplace : JobBaseTextSearchReplace
        {
            private string m_GrepPath;
            private string[] m_SearchList;
            private List<string> m_Patterns;
            private string m_ReplaceStr;
            private List<string> m_FileList;
            private Dictionary<string, string> m_FileGuidMap;
            private bool[][] m_SearchRetList;

            public JobGrepTextSearchReplace(string grepFilePath, string[] searchList, List<string> patterns, string replaceStr,
                List<string> fileList, Dictionary<string, string> fileGuidMap, bool[][] searchRetList)
            {
                m_GrepPath = grepFilePath;
                m_SearchList = searchList;
                m_Patterns = patterns;
                m_ReplaceStr = replaceStr;
                m_FileList = fileList;
                m_FileGuidMap = fileGuidMap;
                m_SearchRetList = searchRetList;
                doneEvent = new ManualResetEvent(false);
                exception = String.Empty;
            }

            public void ThreadPoolCallback(System.Object threadContext)
            {
                try
                {
                    string patternsFile = "Library/AssetDansharipPatterns.txt";
#if UNITY_2020_1_OR_NEWER
                    File.WriteAllLines(patternsFile, m_Patterns);
#else
                    File.WriteAllLines(patternsFile, m_Patterns.ToArray());
#endif

                    ProcessStartInfo startInfo = new ProcessStartInfo(new FileInfo(m_GrepPath).FullName)
                    {
                        UseShellExecute = false,
                        CreateNoWindow = true,
                        RedirectStandardOutput = true,
                        Arguments = string.Format("-f {0} -o -N -F --no-config --no-ignore", patternsFile)
                    };

                    foreach (var path in m_SearchList)
                    {
                        startInfo.Arguments += string.Format(" \"{0}\"", path);
                    }

                    Dictionary<string, HashSet<string>> matchDict = new Dictionary<string, HashSet<string>>();
                    using (Process process = new Process())
                    {
                        process.StartInfo = startInfo;

                        try
                        {
                            process.Start();

                            using (StreamReader reader = process.StandardOutput)
                            {
                                string line;
                                while ((line = reader.ReadLine()) != null)
                                {
                                    if (!string.IsNullOrEmpty(line))
                                    {
                                        var pos = line.LastIndexOf(':');
                                        if (pos > -1)
                                        {
                                            var matchFile = PathToStandardized(line.Substring(0, pos));
                                            var matchGuid = line.Substring(pos + 1);

                                            // 如果是meta引用了，那么就认为是原始引用，但需排除同个文件
                                            if (AssetDanshariUtility.IsMetaExt(matchFile))
                                            {
                                                matchFile = AssetDanshariUtility.GetPathFromTextMetaFilePath(matchFile);

                                                // 如果是meta就不要计算引用自身的情况
                                                string refGuid;
                                                if (m_FileGuidMap.TryGetValue(matchFile, out refGuid))
                                                {
                                                    if (refGuid == matchGuid)
                                                    {
                                                        continue;
                                                    }
                                                }
                                            }

                                            HashSet<string> matchSet;
                                            if (!matchDict.TryGetValue(matchFile, out matchSet))
                                            {
                                                matchSet = new HashSet<string>();
                                                matchDict.Add(matchFile, matchSet);
                                            }

                                            matchSet.Add(matchGuid);
                                        }
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            exception += ex.Message + "\n";
                        }
                    }

                    // 映射到原数组
                    for (int i = 0; i < m_FileList.Count; i++)
                    {
                        HashSet<string> matchSet;
                        if (matchDict.TryGetValue(m_FileList[i], out matchSet))
                        {
                            // 搜索
                            if (string.IsNullOrEmpty(m_ReplaceStr))
                            {
                                for (int j = 0; j < m_Patterns.Count; j++)
                                {
                                    if (matchSet.Contains(m_Patterns[j]))
                                    {
                                        m_SearchRetList[i][j] = true;
                                    }
                                }
                            }
                            else
                            {
                                string text = File.ReadAllText(m_FileList[i]);

                                StringBuilder sb = new StringBuilder(text, text.Length + 2);

                                foreach (var pattern in m_Patterns)
                                {
                                    if (matchSet.Contains(pattern))
                                    {
                                        sb.Replace(pattern, m_ReplaceStr);
                                    }
                                }

                                string text2 = sb.ToString();
                                File.WriteAllText(m_FileList[i], text2);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    exception = ex.Message;
                }

                doneEvent.Set();
            }
        }

        private class JobFileTextSearchReplace : JobBaseTextSearchReplace
        {
            private string m_Path;
            private List<string> m_Patterns;
            private string m_ReplaceStr;
            private bool[] m_SearchRet;
            private Dictionary<string, string> m_FileGuidMap;

            public JobFileTextSearchReplace(string path, List<string> patterns, string replaceStr, Dictionary<string, string> fileGuidMap, bool[] searchRet)
            {
                m_Path = path;
                m_Patterns = patterns;
                m_ReplaceStr = replaceStr;
                m_SearchRet = searchRet;
                m_FileGuidMap = fileGuidMap;
                doneEvent = new ManualResetEvent(false);
            }

            public void ThreadPoolCallback(System.Object threadContext)
            {
                try
                {
                    string text = File.ReadAllText(m_Path);

                    // 搜索
                    if (string.IsNullOrEmpty(m_ReplaceStr))
                    {
                        for (int i = 0; i < m_Patterns.Count; i++)
                        {
                            if (text.Contains(m_Patterns[i]))
                            {
                                m_SearchRet[i] = true;
                            }
                        }
                    }
                    else
                    {
                        StringBuilder sb = new StringBuilder(text, text.Length * 2);
                        foreach (var pattern in m_Patterns)
                        {
                            sb.Replace(pattern, m_ReplaceStr);
                        }

                        string text2 = sb.ToString();
                        if (!string.Equals(text, text2))
                        {
                            File.WriteAllText(m_Path, text2);
                        }
                    }

                    // 处理其对应的meta文件
                    string refGuid;
                    if (m_FileGuidMap.TryGetValue(m_Path, out refGuid))
                    {
                        string metaPath = AssetDanshariUtility.GetTextMetaFilePathFromPath(m_Path);
                        text = File.ReadAllText(metaPath);

                        // 搜索
                        if (string.IsNullOrEmpty(m_ReplaceStr))
                        {
                            for (int i = 0; i < m_Patterns.Count; i++)
                            {
                                if (refGuid != m_Patterns[i] && text.Contains(m_Patterns[i]))
                                {
                                    m_SearchRet[i] = true;
                                }
                            }
                        }
                        else
                        {
                            StringBuilder sb = new StringBuilder(text, text.Length * 2);
                            foreach (var pattern in m_Patterns)
                            {
                                if (refGuid != pattern)
                                {
                                    sb.Replace(pattern, m_ReplaceStr);
                                }
                            }

                            string text2 = sb.ToString();
                            if (!string.Equals(text, text2))
                            {
                                File.WriteAllText(metaPath, text2);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    exception = m_Path + "\n" + ex.Message;
                }

                doneEvent.Set();
            }
        }

        /// <summary>
        /// 线程文本搜索
        /// </summary>
        /// <param name="grepFilePath">grep执行文件的路径</param>
        /// <param name="searchList">引用目录的数组</param>
        /// <param name="fileList">引用目录的文件列表</param>
        /// <param name="fileGuidMap">引用目录的文件列表的GUID字典，方便排除自身</param>
        /// <param name="patterns">文本搜索数组，这里都是GUID数组</param>
        /// <param name="replaceStr">文本替换字符串</param>
        /// <param name="searchRetList">二维数组，记录搜索的结果</param>
        protected void ThreadDoFilesTextSearchReplace(string grepFilePath, string[] searchList, List<string> fileList,
            Dictionary<string, string> fileGuidMap, List<string> patterns, string replaceStr, bool[][] searchRetList)
        {
            if (fileList.Count == 0)
            {
                return;
            }

            List<JobBaseTextSearchReplace> jobList = new List<JobBaseTextSearchReplace>();
            List<ManualResetEvent> eventList = new List<ManualResetEvent>();

            int numFiles = fileList.Count;
            int numFinished = 0;
            AssetDanshariUtility.DisplayThreadProgressBar(numFiles, numFinished);

            int timeout = 600000;  // 10 分钟超时

            if (string.IsNullOrEmpty(grepFilePath))
            {
                for (var i = 0; i < fileList.Count; i++)
                {
                    JobFileTextSearchReplace job = new JobFileTextSearchReplace(fileList[i], patterns, replaceStr, fileGuidMap, searchRetList[i]);
                    jobList.Add(job);
                    eventList.Add(job.doneEvent);
                    ThreadPool.QueueUserWorkItem(job.ThreadPoolCallback);

                    if (eventList.Count >= Environment.ProcessorCount)
                    {
                        WaitForDoFile(eventList, timeout);
                        AssetDanshariUtility.DisplayThreadProgressBar(numFiles, numFinished);
                        numFinished++;
                    }
                }
            }
            else
            {
                JobGrepTextSearchReplace job = new JobGrepTextSearchReplace(grepFilePath, searchList, patterns, replaceStr, fileList, fileGuidMap, searchRetList);
                jobList.Add(job);
                eventList.Add(job.doneEvent);
                ThreadPool.QueueUserWorkItem(job.ThreadPoolCallback);

                // 超时时间按一个文件一分钟来算
                timeout = fileList.Count * 60000;
                AssetDanshariUtility.DisplayTimeThreadProgressBar();
                numFinished = fileList.Count - 1;
            }

            while (eventList.Count > 0)
            {
                WaitForDoFile(eventList, timeout);
                AssetDanshariUtility.DisplayThreadProgressBar(numFiles, numFinished);
                numFinished++;
            }

            foreach (var job in jobList)
            {
                if (!string.IsNullOrEmpty(job.exception))
                {
                    UnityEngine.Debug.LogError(job.exception);
                }
            }
        }

        private void WaitForDoFile(List<ManualResetEvent> events, int timeout)
        {
            int finished = WaitHandle.WaitAny(events.ToArray(), timeout);
            if (finished == WaitHandle.WaitTimeout)
            {
                // 超时
            }

            if (events.Count > finished)
            {
                events.RemoveAt(finished);
            }
        }

        #endregion
    }
}