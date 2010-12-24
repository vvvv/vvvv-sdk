using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;

using Nito.Async;
using VVVV.Core;
using VVVV.Core.Logging;
using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;

namespace VVVV.Hosting.Factories
{
	/// <summary>
	/// Superclass for factories which watch files in a directory
	/// </summary>
	public abstract class AbstractFileFactory<TNodeHost> : IDisposable, IAddonFactory where TNodeHost: INode
	{
		#region fields and constructor
		
		[Import]
		protected ILogger FLogger;
		
		[Import]
		protected IHDEHost FHDEHost;
		
		[Import]
		protected INodeInfoFactory FNodeInfoFactory;
		
		//directory to watch
		private string FFileExtension;
		private List<string> FFiles = new List<string>();
		
		private List<FileSystemWatcher> FDirectoryWatcher = new List<FileSystemWatcher>();
		
		private GenericSynchronizingObject FSyncContext;
		
		public AbstractFileFactory(string fileExtension)
		{
			FFileExtension = fileExtension;
			Debug.Assert(SynchronizationContext.Current != null, "SynchronizationContext not set.");
			FSyncContext = new GenericSynchronizingObject();
		}
		
		#endregion fields and constructor
		public abstract string JobStdSubPath
		{
			get;
		}
		
		public List<string> DirectoriesToWatch
		{
			get; private set;
		}
		
		public string FileExtension
		{
			get
			{
				return FFileExtension;
			}
		}
		
		#region IAddonFactory
		//return nodeinfos from systemname
		public IEnumerable<INodeInfo> ExtractNodeInfos(string filename, string arguments)
		{
			if (Path.GetExtension(filename) != FileExtension)
				return new INodeInfo[0];
			
			// Regardless of the arguments, we need to load the node infos first.
			var nodeInfos = LoadNodeInfos(filename).ToList();
			
			if (nodeInfos.Count > 0)
				FLogger.Log(LogType.Debug, "Loaded node infos from {0}.", filename);
			else
				((HDEHost) FHDEHost).MarkFileAsEmpty(filename);
			
			// If additional arguments are present vvvv is only interested in one specific
			// NodeInfo -> look for it.
			if ((arguments != null) && (arguments != ""))
			{
				foreach (var nodeInfo in nodeInfos)
				{
					if (nodeInfo.Arguments != null && nodeInfo.Arguments == arguments)
						return new INodeInfo[] { nodeInfo };
				}
				
				// give back nothing if not found
				return new INodeInfo[0];
			}
			
			return nodeInfos;
		}
		
		protected abstract IEnumerable<INodeInfo> LoadNodeInfos(string filename);
		
		public bool Create(INodeInfo nodeInfo, INode host)
		{
			if (host is TNodeHost && Path.GetExtension(nodeInfo.Filename) == FileExtension)
				return CreateNode(nodeInfo, (TNodeHost) host);
			
			return false;
		}
		
		protected abstract bool CreateNode(INodeInfo nodeInfo, TNodeHost nodeHost);
		
		public bool Delete(INodeInfo nodeInfo, INode host)
		{
			if (host is TNodeHost && Path.GetExtension(nodeInfo.Filename) == FileExtension)
				return DeleteNode(nodeInfo, (TNodeHost) host);
			
			return false;
		}
		
		protected abstract bool DeleteNode(INodeInfo nodeInfo, TNodeHost nodeHost);
		
		public bool Clone(INodeInfo nodeInfo, string path, string name, string category, string version, out INodeInfo newNodeInfo)
		{
			if (Path.GetExtension(nodeInfo.Filename) == FileExtension)
			{
				string filename;
				if (CloneNode(nodeInfo, path, name, category, version, out filename))
				{
					AddFile(filename);
					
					foreach (var possibleNodeInfo in ((HDEHost) FHDEHost).GetCachedNodeInfos(filename))
					{
						if (possibleNodeInfo.Name == name && possibleNodeInfo.Category == category && possibleNodeInfo.Version == version)
						{
							newNodeInfo = possibleNodeInfo;
							return true;
						}
					}
				}
			}
			
			newNodeInfo = null;
			return false;
		}
		
		protected virtual bool CloneNode(INodeInfo nodeInfo, string path, string name, string category, string version, out string filename)
		{
			filename = null;
			return false;
		}
		
		#endregion IAddonFactory
		
		#region directory and watcher
		
		protected virtual void AddSubDir(string dir, bool recursive)
		{
			foreach (string filename in Directory.GetFiles(dir, @"*" + FFileExtension))
			{
				try {
					AddFile(filename);
				} catch (Exception e) {
					FLogger.Log(e);
				}
			}
			
			if (recursive)
				foreach (string subDir in Directory.GetDirectories(dir))
			{
				if (!(subDir.EndsWith(".svn") || subDir.EndsWith(".cache")))
					try {
					AddSubDir(subDir, recursive);
				} catch (Exception e) {
					FLogger.Log(e);
				}
			}
		}
		
		
		//register all files in a directory
		public void AddDir(string dir, bool recursive)
		{
			//give subclasses a chance to cleanup before we start to scan.
			DeleteArtefacts(dir, recursive);

			AddSubDir(dir, recursive);
			
			var dirWatcher = new FileSystemWatcher(dir, @"*" + FFileExtension);
			dirWatcher.SynchronizingObject = FSyncContext;
			dirWatcher.IncludeSubdirectories = true;
			dirWatcher.EnableRaisingEvents = true;
			dirWatcher.Changed += new FileSystemEventHandler(FDirectoryWatcher_Changed);
			dirWatcher.Created += new FileSystemEventHandler(FDirectoryWatcher_Created);
			dirWatcher.Deleted += new FileSystemEventHandler(FDirectoryWatcher_Deleted);
			dirWatcher.Renamed += new RenamedEventHandler(FDirectoryWatcher_Renamed);
			
			FDirectoryWatcher.Add(dirWatcher);
		}
		
		public void RemoveDir(string dir)
		{
			// Simply dispose the file watcher. Keep the node infos alive,
			// the files still exist and therefor patches still work.
			// The node infos will be gone on next start of vvvv (not in nodebrowser
			// anymore), but patches will still work.
			for (int i=FDirectoryWatcher.Count-1; i>=0; i--)
			{
				var dirWatcher = FDirectoryWatcher[i];
				if (dirWatcher.Path == dir)
				{
					FDirectoryWatcher.RemoveAt(i);
					dirWatcher.Changed -= new FileSystemEventHandler(FDirectoryWatcher_Changed);
					dirWatcher.Created -= new FileSystemEventHandler(FDirectoryWatcher_Created);
					dirWatcher.Deleted -= new FileSystemEventHandler(FDirectoryWatcher_Deleted);
					dirWatcher.Renamed -= new RenamedEventHandler(FDirectoryWatcher_Renamed);
					dirWatcher.Dispose();
					return;
				}
			}
		}
		
		void FDirectoryWatcher_Changed(object sender, FileSystemEventArgs e)
		{
			/*	if (System.IO.Path.HasExtension(e.FullPath))
				FileChanged(e.FullPath);
			else
				DirectoryChanged(e.FullPath);*/
		}
		
		protected void FDirectoryWatcher_Created(object sender, FileSystemEventArgs e)
		{
			if (File.Exists(e.FullPath))
				AddFile(e.FullPath);
		}
		
		protected void FDirectoryWatcher_Deleted(object sender, FileSystemEventArgs e)
		{
			if (!File.Exists(e.FullPath))
				RemoveFile(e.FullPath);
		}
		
		protected void FDirectoryWatcher_Renamed(object sender, RenamedEventArgs e)
		{
			if (!File.Exists(e.OldFullPath))
				RemoveFile(e.OldFullPath);
			if (e.FullPath.EndsWith(FFileExtension, true, null) && File.Exists(e.FullPath))
				AddFile(e.FullPath);
		}
		
		#endregion directory and watcher
		
		#region file handling
		
		//remove all addons included with this filename
		protected void RemoveFile(string filename)
		{
			if (FFiles.Contains(filename))
			{
				FFiles.Remove(filename);
				DoRemoveFile(filename);
			}
		}
		
		protected virtual void DoRemoveFile(string filename)
		{
			foreach (var nodeInfo in FNodeInfoFactory.NodeInfos)
			{
				if (!string.IsNullOrEmpty(nodeInfo.Filename))
				{
					try {
						if (new Uri(nodeInfo.Filename) == new Uri(filename))
							FNodeInfoFactory.DestroyNodeInfo(nodeInfo);
					} catch (UriFormatException) {
						// Ignore wrong uris like 0.v4p ////
					}
				}
			}
		}
		
		//add all addons included with this filename
		public void AddFile(string filename)
		{
			if (!FFiles.Contains(filename))
			{
				FFiles.Add(filename);
				DoAddFile(filename);
			}
		}
		
		protected virtual void DoAddFile(string filename)
		{
			var host = (HDEHost) FHDEHost;
			if (host.HasCachedNodeInfos(filename))
			{
				var cachedNodeInfos = host.GetCachedNodeInfos(filename);
				foreach (var nodeInfo in cachedNodeInfos)
					nodeInfo.Factory = this;
			}
			else
			{
				ExtractNodeInfos(filename, null);
			}
		}
		
		//allow subclasses to react to a filechange
		protected void FileChanged(string filename)
		{
			if (!FFiles.Contains(filename))
			{
				AddFile(filename);
				return;
			}
			
			//compare those new nodeinfos
			//with nodeinfos so far associated with this filename
			//add nodeinfos that are new in this filename
			//remove nodeinfos that are no longer with this filename
			
			//get all old nodeinfos associated with this filename
			var oldInfos = ((HDEHost) FHDEHost).GetCachedNodeInfos(filename).ToList();
			var nodeInfoUsedMap = new Dictionary<INodeInfo, bool>();
			
			
			//compare those oldInfos with the newly extracted ones
			//if a newly extracted one is present in the old ones do nothing
			//if a newly extracted one is not present in the old ones add it
			
			foreach(var newNodeInfo in ExtractNodeInfos(filename, null))
			{
				foreach(var oldNodeInfo in oldInfos)
				{
					if (oldNodeInfo == newNodeInfo)
					{
						nodeInfoUsedMap[oldNodeInfo] = true; //mark as used
						break;
					}
				}
			}
			
			//remove old nodeinfos that have no new compatible
			foreach(var oldNodeInfo in oldInfos)
			{
				if (!(nodeInfoUsedMap.ContainsKey(oldNodeInfo) && nodeInfoUsedMap[oldNodeInfo]))
				{
					FNodeInfoFactory.DestroyNodeInfo(oldNodeInfo);
				}
			}
		}
		
		//allow subclasses to react to a directorychange
		protected virtual void DirectoryChanged(string path)
		{
			//nothing to do here
		}
		
		//allow subclasses to cleanup before directory scan.
		protected virtual void DeleteArtefacts(string dir, bool recursive)
		{
			if (recursive)
				foreach (string subDir in Directory.GetDirectories(dir))
			{
				try {
					DeleteArtefacts(subDir, recursive);
				} catch (Exception e) {
					FLogger.Log(e);
				}
			}
		}
		
		#endregion file handling
		
		public void Dispose()
		{

		}
	}
}
