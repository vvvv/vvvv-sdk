using System;
using System.Runtime.InteropServices;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Xml;
using VVVV.Core;
using VVVV.Core.Logging;
using VVVV.PluginInterfaces.V1;

namespace VVVV.PluginInterfaces.V2
{
    [ComVisible(false)]
    public enum SearchPathState { AddPending, DisablePending, Added, Disabled };

    [ComVisible(false)]
    public class SearchPath
    {
        public SearchPathState State {get; private set;}
        public string Nodelist { get; private set; }
        public string Dir {get; private set;}
        public int RefCount {get; private set;}
        public IAddonFactory Factory { get; private set; }
        public bool IsUserDefined { get; private set; }
        protected ILogger FLogger { get; private set; }

        protected bool FRecursive;
        public bool Recursive
        {
            get
            {
                return FRecursive;
            }
            set
            {
                if (State == SearchPathState.AddPending)
                {
                    FRecursive = value;
                }
            }
        }
        
        private readonly INodeInfoFactory FNodeInfoFactory;
        
        internal SearchPath(string nodelist, string dir, IAddonFactory factory, bool recursive, bool isuserdefined, ILogger logger, INodeInfoFactory nodeInfoFactory)
        {
            Nodelist = nodelist;
            Dir = Path.GetFullPath(dir);
            Factory = factory;
            RefCount = 1;
            Recursive = recursive;
            IsUserDefined = isuserdefined;

            FLogger = logger;
            FNodeInfoFactory = nodeInfoFactory;
        }

        public bool IsGarbage { get { return RefCount <= 0; } }
        
        internal void Inc(bool isuserdefined)
        {
            RefCount++;
            
            IsUserDefined = IsUserDefined || isuserdefined;
        }

        internal void Dec()
        {
            RefCount--;
        }

        public bool AddToFactory()
        {
            if (State == SearchPathState.AddPending)
            {
                if (File.Exists(Nodelist) && Factory.AllowCaching)
                {
                    FLogger.Log(LogType.Debug, "adding " + Dir + " to " + Factory.Name + " (cached by " + Nodelist + ")");
                    CollectFromNodeList();
                }
                else
                {
                    FLogger.Log(LogType.Debug, "adding " + Dir + " to " + Factory.Name);
                    Factory.AddDir(Dir, Recursive);
                }
                
                State = SearchPathState.Added;
                return true;
            }
            return false;
        }

        public bool RemoveFromFactory()
        {
            if ((State == SearchPathState.DisablePending) || (IsGarbage))
            {
                FLogger.Log(LogType.Debug, "removing " + Dir + " from " + Factory.Name);
                Factory.RemoveDir(Dir);
                State = SearchPathState.Disabled;
                return true;
            }
            return false;
        }

        public void AddLater()
        {
            if (State == SearchPathState.Disabled)
                State = SearchPathState.AddPending;
            if (State == SearchPathState.DisablePending)
                State = SearchPathState.Added;
        }

        public void DisableLater()
        {
            if (State == SearchPathState.Added)
                State = SearchPathState.DisablePending;
            if (State == SearchPathState.AddPending)
                State = SearchPathState.Disabled;
        }
        
        public bool Contains(string dir)
        {
            if (Recursive)
                return dir.StartsWith(this.Dir);
            else
                return this.Dir.ToLower() == dir.ToLower();
        }
        
        private void CollectFromNodeList()
        {
            var baseDir = Path.GetDirectoryName(Nodelist);
            
            using (var streamReader = new StreamReader(Nodelist))
            {
                var settings = new XmlReaderSettings();
                settings.DtdProcessing = DtdProcessing.Ignore;
                
                using (var xmlReader = XmlReader.Create(streamReader, settings))
                {
                    while (xmlReader.ReadToFollowing("NODE"))
                    {
                        var factory = xmlReader.GetAttribute("factory");
                        if (factory != Factory.Name) continue;
                        
                        var name = xmlReader.GetAttribute("name");
                        var category = xmlReader.GetAttribute("category");
                        var version = xmlReader.GetAttribute("version");
                        var filename = Path.Combine(baseDir, xmlReader.GetAttribute("filename"));
                        
                        var nodeInfo = FNodeInfoFactory.CreateNodeInfo(name, category, version, filename, true);
                        nodeInfo.Factory = Factory;
                        nodeInfo.Ignore = int.Parse(xmlReader.GetAttribute("ignore")) == 0 ? false : true;
                        nodeInfo.AutoEvaluate = int.Parse(xmlReader.GetAttribute("autoevaluate")) == 0 ? false : true;
                        nodeInfo.Type = (NodeType) NodeType.Parse(typeof(NodeType), xmlReader.GetAttribute("type"));
                        nodeInfo.Arguments = xmlReader.GetAttribute("arguments");
                        
                        var ibs = xmlReader.GetAttribute("ibs");
                        nodeInfo.InitialBoxSize = new System.Drawing.Size(int.Parse(ibs.Split(',')[0]), int.Parse(ibs.Split(',')[1]));
                        var iws = xmlReader.GetAttribute("iws");
                        nodeInfo.InitialWindowSize = new System.Drawing.Size(int.Parse(iws.Split(',')[0]), int.Parse(iws.Split(',')[1]));
                        nodeInfo.InitialComponentMode = (TComponentMode) NodeType.Parse(typeof(TComponentMode), xmlReader.GetAttribute("icm"));

                        try
                        {
                            Factory.ParseNodeEntry(xmlReader, nodeInfo);
                        }
                        catch (Exception e)
                        {
                            FLogger.Log(e);
                        }
                        
                        using (var nodeReader = xmlReader.ReadSubtree())
                        {
                            while (nodeReader.Read())
                            {
                                switch (nodeReader.Name)
                                {
                                    case "TAGS":
                                        nodeInfo.Tags = nodeReader.ReadString().TrimEnd();
                                        break;
                                    case "SHORTCUT":
                                        nodeInfo.Shortcut = nodeReader.ReadString().TrimEnd();
                                        break;
                                    case "HELP":
                                        nodeInfo.Help = nodeReader.ReadString().TrimEnd();
                                        break;
                                    case "WARNINGS":
                                        nodeInfo.Warnings = nodeReader.ReadString().TrimEnd();
                                        break;
                                    case "BUGS":
                                        nodeInfo.Bugs = nodeReader.ReadString().TrimEnd();
                                        break;
                                    case "AUTHOR":
                                        nodeInfo.Author = nodeReader.ReadString().TrimEnd();
                                        break;
                                    case "CREDITS":
                                        nodeInfo.Credits = nodeReader.ReadString().TrimEnd();
                                        break;
                                }
                            }
                        }
                        
                        nodeInfo.CommitUpdate();
                    }
                }
            }
        }
    }

    /// <summary>
    /// Description of NodeCollection.
    /// </summary>
    [PartCreationPolicy(CreationPolicy.Shared)]
    [Export(typeof(NodeCollection))]
    [ComVisible(false)]
    public class NodeCollection
    {
        List<SearchPath> FPaths = new List<SearchPath>();
        public List<SearchPath> Paths
        {
            get
            {
                return FPaths;
            }
        }
        [Import]
        protected ILogger FLogger;

        [Import]
        public IHDEHost HDEHost{get; protected set;}
        
        [Import]
        public INodeInfoFactory NodeInfoFactory { get; protected set; }
        
        public NodeCollection()
        {
            // To simulate vvvv startup.
            IsCollecting = true;
        }

        public void Collect()
        {
            IsCollecting = true;
            
            try
            {
                // 
                for (int i = FPaths.Count-1; i >= 0; i--)
                {
                    if (FPaths[i].IsGarbage)
                    {
                        var p = FPaths[i];
                        FPaths.RemoveAt(i);
                        p.RemoveFromFactory();
                    }
                }
                
                // remove paths that need to be removed
                for (int i = FPaths.Count-1; i >= 0; i--)
                    FPaths[i].RemoveFromFactory();
                
                // add paths that need to be added
                foreach (var path in FPaths)
                    path.AddToFactory();
            }
            finally
            {
                IsCollecting = false;
                if (Collected != null)
                    Collected(this, EventArgs.Empty);
            }
        }
        
        private void Add(SearchPath path)
        {
            bool found = false;
            foreach (var p in FPaths)
                if ((p.Factory == path.Factory) && (String.Compare(p.Dir, path.Dir, true)==0))
            {
                p.Inc(path.IsUserDefined);
                found = true;
                break;
            }
            
            if (!found)
            {
                foreach (var p in FPaths)
                    // check for any parent path that is already covering the new subpath
                    // if found then disable the new path
                    if ((p.Recursive) && (p.Factory == path.Factory) && path.Dir.StartsWith(p.Dir, true, null))
                {
                    path.DisableLater();
                    FLogger.Log(LogType.Debug, "adding disabled path " + path.Dir + " to available " + p.Factory.JobStdSubPath + ". already watched by: " + p.Dir);
                    FPaths.Add(path);
                    return;
                }
                
                if (path.Recursive)
                    // new folder is set recursive, thus covering all its subfolders
                    // we need to check if any subfolders were already added and either disable them or add the new parent folder as not recursive
                    foreach (var p in FPaths)
                {
                    if ((p.Factory == path.Factory) && p.Dir.StartsWith(path.Dir, true, null))
                    {
                        FLogger.Log(LogType.Debug, "adding recursive path " + path.Dir + " to available " + p.Factory.JobStdSubPath + ". conflicting with already added path: " + p.Dir);
                        FLogger.Log(LogType.Debug, "adding path as nonrecursive.");
                        p.DisableLater();
                        //path.Recursive = false;
                    }
                }
                
                FPaths.Add(path);
            }
        }
        
        public void Add(string nodelist, string path, IAddonFactory factory, bool recursive, bool isuserdefined)
        {
            Add(new SearchPath(nodelist, path, factory, recursive, isuserdefined, FLogger, NodeInfoFactory));
        }
        
        private void Remove(SearchPath path)
        {
            foreach (var p in FPaths)
                if ((p.Factory == path.Factory) && (p.Dir == path.Dir))
                    p.Dec();
        }
        
        private void Remove(string path, IAddonFactory factory)
        {
            Remove(new SearchPath(string.Empty, path, factory, false, false, FLogger, NodeInfoFactory));
        }
        
        public void AddJob(string dir, bool isuserdefined)
        {
            if (Directory.Exists(dir))
            {
                var nodelist = Path.Combine(dir, "nodelist.xml");
                foreach (var factory in HDEHost.AddonFactories)
                {
                    var subDir = dir.ConcatPath(factory.JobStdSubPath);
                    if (Directory.Exists(subDir))
                        Add(nodelist, subDir, factory, true, isuserdefined);
                }
            }
        }

        public void AddUnsorted(string dir, bool recursive, bool isuserdefined)
        {
            if (Directory.Exists(dir))
                foreach (var factory in HDEHost.AddonFactories)
                    Add(string.Empty, dir, factory, recursive, isuserdefined);
        }
        
        public void RemoveJob(string dir)
        {
            if (Directory.Exists(dir))
                foreach (var factory in HDEHost.AddonFactories)
            {
                var subDir = dir.ConcatPath(factory.JobStdSubPath);
                if (Directory.Exists(subDir))
                    Remove(subDir, factory);
            }
        }
        
        public void RemoveUnsorted(string dir)
        {
            if (Directory.Exists(dir))
                foreach (var factory in HDEHost.AddonFactories)
                    Remove(dir, factory);
        }

        public void AddCombined(string dir, bool isuserdefined)
        {
            AddJob(dir, isuserdefined);
            AddUnsorted(dir, false, isuserdefined);
        }
        
        public void RemoveCombined(string dir)
        {
            RemoveJob(dir);
            RemoveUnsorted(dir);
        }
        
        public bool IsCollecting
        {
            get;
            private set;
        }
        
        public event EventHandler Collected;
        
        public bool IsInUserDefinedSearchPath(IAddonFactory factory, string dir)
        {
            foreach (var sp in Paths)
            {
                if (sp.State == SearchPathState.Added && sp.Factory == factory && sp.IsUserDefined && sp.Contains(dir))
                    return true;
            }
            return false;
        }
    }
}
