using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel.Composition;

using VVVV.Core;
using VVVV.Core.Logging;
using VVVV.PluginInterfaces.V1;

namespace VVVV.PluginInterfaces.V2
{
    public enum SearchPathState { AddPending, DisablePending, Added, Disabled };

	public class SearchPath
	{
        public SearchPathState State {get; private set;}
		public string Path {get; private set;}
		public int RefCount {get; private set;}
        public IAddonFactory Factory { get; private set; }
        public bool IsUserDefined { get; private set; }
		protected ILogger Flogger { get; private set; }

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
        
        internal SearchPath(string path, IAddonFactory factory, bool recursive, bool isuserdefined, ILogger logger)
		{	
			Path = path;
			Factory = factory;
			RefCount = 1;
			Recursive = recursive;
            IsUserDefined = isuserdefined;

            Flogger = logger;
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
                Flogger.Log(LogType.Debug, "adding " + Path + " to available " + Factory.JobStdSubPath);
                Factory.AddDir(Path, Recursive);
                State = SearchPathState.Added;
                return true;
            }
            return false;
        }

        public bool RemoveFromFactory()
        {
            if ((State == SearchPathState.DisablePending) || (IsGarbage))
            {
                Flogger.Log(LogType.Debug, "removing " + Path + " from available " + Factory.JobStdSubPath);
                Factory.RemoveDir(Path);
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
				return dir.StartsWith(this.Path);
			else
				return this.Path == dir;
		}
    }
	
	/// <summary>
	/// Description of NodeCollection.
	/// </summary>
	[PartCreationPolicy(CreationPolicy.Shared)]
	[Export(typeof(NodeCollection))]
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
		protected ILogger Flogger;

		[Import]
		public IHDEHost HDEHost{get; protected set;}
		
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
				if ((p.Factory == path.Factory) && (String.Compare(p.Path, path.Path, true)==0))
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
                    if ((p.Recursive) && (p.Factory == path.Factory) && path.Path.StartsWith(p.Path, true, null))
                    {
                        path.DisableLater();
                        Flogger.Log(LogType.Debug, "adding disabled path " + path.Path + " to available " + p.Factory.JobStdSubPath + ". already watched by: " + p.Path);
                        FPaths.Add(path);
                        return;
                    }
                
                if (path.Recursive)
                    // new folder is set recursive, thus covering all its subfolders
                    // we need to check if any subfolders were already added and either disable them or add the new parent folder as not recursive
                    foreach (var p in FPaths)
                    {
                        if ((p.Factory == path.Factory) && p.Path.StartsWith(path.Path, true, null))
                        {
                            Flogger.Log(LogType.Debug, "adding recursive path " + path.Path + " to available " + p.Factory.JobStdSubPath + ". conflicting with already added path: " + p.Path);
                            Flogger.Log(LogType.Debug, "adding path as nonrecursive.");
                            p.DisableLater();						                                                      
                            //path.Recursive = false;
                        }
					}
					    				
				FPaths.Add(path);
			}
		}
		
		public void Add(string path, IAddonFactory factory, bool recursive, bool isuserdefined)
		{
            Add(new SearchPath(path, factory, recursive, isuserdefined, Flogger));
		}
		
		private void Remove(SearchPath path)
		{
			foreach (var p in FPaths) 
				if ((p.Factory == path.Factory) && (p.Path == path.Path))
					p.Dec();
		}
		
		private void Remove(string path, IAddonFactory factory)
		{
			Remove(new SearchPath(path, factory, false, false, Flogger));
		}
				
		public void AddJob(string dir, bool isuserdefined)
		{
			if (Directory.Exists(dir))
				foreach (var factory in HDEHost.AddonFactories)
				{
					var subDir = dir.ConcatPath(factory.JobStdSubPath);
					if (Directory.Exists(subDir))
                        Add(subDir, factory, true, isuserdefined);
				}
		}

        public void AddUnsorted(string dir, bool recursive, bool isuserdefined)
		{
			if (Directory.Exists(dir))
				foreach (var factory in HDEHost.AddonFactories)
                    Add(dir, factory, recursive, isuserdefined);
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
