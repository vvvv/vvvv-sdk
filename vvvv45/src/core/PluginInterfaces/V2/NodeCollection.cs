using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel.Composition;

using VVVV.Core;
using VVVV.Core.Logging;
using VVVV.PluginInterfaces.V1;

namespace VVVV.PluginInterfaces.V2
{	
	public class SearchPath
	{
		public bool IsInitialized {get; private set;}
		public string Path {get; private set;}
		public int RefCount {get; private set;}
		public bool IsGarbage{ get{ return RefCount <= 0; }}
		public IAddonFactory Factory;
		public bool Disabled;
		public bool NeedsToBeDisabled;
		public bool Recursive {get; set;}
		public SearchPath(string path, IAddonFactory factory, bool recursive)
		{	
			Path = path;
			Factory = factory;
			RefCount = 1;
			Recursive = recursive;
		}	
		
		public void Inc()
		{
			RefCount++;
		}
		public void Dec()
		{
			RefCount--;
		}
		public void Init()
		{
			IsInitialized = true;
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
		}

		public void Collect()
		{
			IsCollecting = true;
			
			try
			{
				for (int i = FPaths.Count-1; i >= 0; i--)
				{
					if (FPaths[i].IsGarbage) 
					{
						var p = FPaths[i];
						FPaths.RemoveAt(i);					
						Flogger.Log(LogType.Debug, "removing " + p.Path + " from available " + p.Factory.JobStdSubPath);				
						p.Factory.RemoveDir(p.Path);
					}
				}
				
				for (int i = FPaths.Count-1; i >= 0; i--) 
				{
					if (FPaths[i].NeedsToBeDisabled)
					{
						var p = FPaths[i];
						p.Disabled = true;   
						p.Factory.RemoveDir(p.Path);
					}
				}
				
				foreach (var path in FPaths)
				{
					if (!path.IsInitialized)
					{
						path.Init();	
						Flogger.Log(LogType.Debug, "adding " + path.Path + " to available " + path.Factory.JobStdSubPath);				
						path.Factory.AddDir(path.Path, path.Recursive);						
					}
				}
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
					p.Inc();
					found = true;
					break;
				}
			
			if (!found)
			{
				foreach (var p in FPaths) 
					if ((p.Factory == path.Factory) && p.Path.StartsWith(path.Path, true, null))
					{
						if (path.Recursive)
						{
						 	Flogger.Log(LogType.Debug, "error adding recursive path " + path.Path + " to available " + p.Factory.JobStdSubPath + ". conflicting with already added path: " + p.Path);
						 	Flogger.Log(LogType.Debug, "adding path as nonrecursive.");	
						 	path.Recursive = false;
						}
  						FPaths.Add(path);
						//p.NeedsToBeDisabled = true;						
						return;
					}
					    
				foreach (var p in FPaths) 
					if ((p.Factory == path.Factory) && path.Path.StartsWith(p.Path, true, null))
					{
						FPaths.Add(path);
						if (p.Recursive)
						{
							path.Init();
							path.Disabled = true;
						 	Flogger.Log(LogType.Debug, "adding disabled path " + path.Path + " to available " + p.Factory.JobStdSubPath + ". already wathced by: " + p.Path);
						}
						return;
					}				
				
				FPaths.Add(path);
			}
		}
		
		public void Add(string path, IAddonFactory factory, bool recursive)
		{
			Add(new SearchPath(path, factory, recursive));
		}
		
		private void Remove(SearchPath path)
		{
			foreach (var p in FPaths) 
				if ((p.Factory == path.Factory) && (p.Path == path.Path))
					p.Dec();
		}
		
		private void Remove(string path, IAddonFactory factory)
		{
			Remove(new SearchPath(path, factory, false));
		}
				
		public void AddJob(string dir)
		{
			if (Directory.Exists(dir))
				foreach (var factory in HDEHost.AddonFactories)
				{
					var subDir = dir.ConcatPath(factory.JobStdSubPath);
					if (Directory.Exists(subDir))
						Add(subDir, factory, true);
				}
		}
		
		public void AddUnsorted(string dir, bool recursive)
		{
			if (Directory.Exists(dir))
				foreach (var factory in HDEHost.AddonFactories)
					Add(dir, factory, recursive);
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
		
		public void AddCombined(string dir)
		{
			AddJob(dir);
			AddUnsorted(dir, false);
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
	}
}
