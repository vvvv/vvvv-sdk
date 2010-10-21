using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel.Composition;

using VVVV.Core;
using VVVV.Core.Logging;
using VVVV.Hosting;
using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;

namespace VVVV.Hosting.Factories
{	
	public class SearchPath
	{
		public string Path {get; private set;}
		public int RefCount {get; private set;}
		public bool IsGarbage{ get{ return RefCount <= 0; }}
		public IAddonFactory Factory;
		public SearchPath(string path, IAddonFactory factory)
		{			
			Path = path;
			Factory = factory;
			RefCount = 1;
		}	
		
		public void Inc()
		{
			RefCount++;
		}
		public void Dec()
		{
			RefCount--;
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
		
		[Import()]
		ILogger Flogger;

		[Import]
		public HDEHost HDEHost{get; protected set;}
		
		public NodeCollection()
		{
		}

		public void Collect()
		{
			for (int i = FPaths.Count-1; i >= 0; i--) 
				if (FPaths[i].IsGarbage)
				{
					var p = FPaths[i];
					FPaths.RemoveAt(i);					
					Flogger.Log(LogType.Debug, "removing " + p.Path + " to available " + p.Factory.JobStdSubPath);				
					p.Factory.RemoveDir(p.Path);
				}
		}
			
		
		private void Add(SearchPath path)
		{
			bool found = false;
			foreach (var p in FPaths) 
				if ((p.Factory == path.Factory) && (p.Path == path.Path))
				{
					p.Inc();
					found = true;
				}
			
			if (!found)
			{
				FPaths.Add(path);
				Flogger.Log(LogType.Debug, "adding " + path.Path + " to available " + path.Factory.JobStdSubPath);				
				path.Factory.AddDir(path.Path);						
			}
		}
		
		private void Add(string path, IAddonFactory factory)
		{
			Add(new SearchPath(path, factory));
		}
		
		private void Remove(SearchPath path)
		{
			foreach (var p in FPaths) 
				if ((p.Factory == path.Factory) && (p.Path == path.Path))
					p.Dec();
		}
		
		private void Remove(string path, IAddonFactory factory)
		{
			Remove(new SearchPath(path, factory));
		}
				
		public void AddJob(string dir)
		{
			if (Directory.Exists(dir))
				foreach (var factory in HDEHost.AddonFactories)
				{
					var subDir = dir.ConcatPath(factory.JobStdSubPath);
					if (Directory.Exists(subDir))
						Add(subDir, factory);
				}
		}
		
		public void AddUnsorted(string dir)
		{
			if (Directory.Exists(dir))
				foreach (var factory in HDEHost.AddonFactories)
					Add(dir, factory);
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
		
		
	}
}
