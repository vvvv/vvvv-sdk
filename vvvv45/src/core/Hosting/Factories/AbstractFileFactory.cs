using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
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
	public abstract class AbstractFileFactory<TNodeHost> : IAddonFactory where TNodeHost: IAddonHost
	{
		#region fields and constructor
		
		[Import]
		protected ILogger FLogger;
		
		//directory to watch
		private string FDirectory;
		private string FFileExtension;
		protected Dictionary<string, List<INodeInfo>> FNodeInfoCache = new Dictionary<string, List<INodeInfo>>();
		private Dictionary<string, bool> FLoadedFiles = new Dictionary<string, bool>();
		private FileSystemWatcher FDirectoryWatcher;
		private GenericSynchronizingObject FSyncContext;
		
		public AbstractFileFactory(string directoryToWatch, string fileExtension)
		{
			FDirectory = Path.GetFullPath(directoryToWatch);
			FFileExtension = fileExtension;
			FSyncContext = new GenericSynchronizingObject();
		}
		
		#endregion fields and constructor
		
		public string DirectoryToWatch
		{
			get
			{
				return FDirectory;
			}
		}
		
		public string FileExtension
		{
			get
			{
				return FFileExtension;
			}
		}
		
		protected bool IsLoaded(string filename)
		{
			return FLoadedFiles.ContainsKey(filename) && FLoadedFiles[filename];
		}
		
		#region IAddonFactory
		public event NodeInfoEventHandler NodeInfoAdded;
		protected virtual void OnNodeInfoAdded(INodeInfo nodeInfo)
		{
			if (NodeInfoAdded != null)
				NodeInfoAdded(this, nodeInfo);
		}
		
		public event NodeInfoEventHandler NodeInfoUpdated;
		protected virtual void OnNodeInfoUpdated(INodeInfo nodeInfo)
		{
			if (NodeInfoUpdated != null)
				NodeInfoUpdated(this, nodeInfo);
		}
		
		public event NodeInfoEventHandler NodeInfoRemoved;
		protected virtual void OnNodeInfoRemoved(INodeInfo nodeInfo)
		{
			if (NodeInfoRemoved != null)
				NodeInfoRemoved(this, nodeInfo);
		}
		
		//return nodeinfos from systemname
		public IEnumerable<INodeInfo> ExtractNodeInfos(string systemname)
		{
			// systemname is of form FILENAME[|ARGUMENTS], for example:
			// - C:\Path\To\Assembly.dll
			// or
			// - C:\Path\To\Assembly.dll|Namespace.Class
			
			string filename = systemname;
			string arguments = null;
			
			int pipeIndex = systemname.IndexOf('|');
			if (pipeIndex >= 0)
			{
				filename = systemname.Substring(0, pipeIndex);
				arguments = systemname.Substring(pipeIndex + 1);
			}
			
			if (Path.GetExtension(filename) != FileExtension) return new INodeInfo[0];
			
			IEnumerable<INodeInfo> nodeInfos;
			
			// Regardless of the arguments, we need to load the node infos first.
			// Do we have the required node infos cached?
			if (HasCachedNodeInfos(filename))
				nodeInfos = GetCachedNodeInfos(filename);
			else
			{
				nodeInfos = LoadAndCacheNodeInfos(filename);
			}
			
			// If additional arguments are present vvvv is only interested in one specific
			// NodeInfo -> look for it.
			if (arguments != null)
			{
				foreach (var nodeInfo in nodeInfos)
				{
					if (nodeInfo.Arguments != null && nodeInfo.Arguments == arguments)
						return new INodeInfo[] { nodeInfo };
				}
				
				return new INodeInfo[0];
			}
			
			return nodeInfos;
		}
		
		protected abstract IEnumerable<INodeInfo> GetNodeInfos(string filename);
		
		public virtual void StartWatching()
		{
			if (Directory.Exists(FDirectory))
			{
				//give subclasses a chance to cleanup before we start to scan.
				DeleteArtefacts(FDirectory);
				ScanForFiles(FDirectory);
				
				//watch this directory
				if (FDirectoryWatcher == null)
				{
					FDirectoryWatcher = new FileSystemWatcher(FDirectory, @"*" + FFileExtension);
					FDirectoryWatcher.SynchronizingObject = FSyncContext;
					FDirectoryWatcher.IncludeSubdirectories = true;
					FDirectoryWatcher.EnableRaisingEvents = true;
					FDirectoryWatcher.Changed += new FileSystemEventHandler(FDirectoryWatcher_Changed);
					FDirectoryWatcher.Created += new FileSystemEventHandler(FDirectoryWatcher_Created);
					FDirectoryWatcher.Deleted += new FileSystemEventHandler(FDirectoryWatcher_Deleted);
					FDirectoryWatcher.Renamed += new RenamedEventHandler(FDirectoryWatcher_Renamed);
				}
				else
				{
					FDirectoryWatcher.Path = FDirectory;
				}
			}
		}
		
		public bool Create(INodeInfo nodeInfo, IAddonHost host)
		{
			if (host is TNodeHost && Path.GetExtension(nodeInfo.Filename) == FileExtension)
			{
				if (!IsLoaded(nodeInfo.Filename))
					LoadAndCacheNodeInfos(nodeInfo.Filename);
				
				return CreateNode(nodeInfo, (TNodeHost) host);
			}
			
			return false;
		}
		
		protected abstract bool CreateNode(INodeInfo nodeInfo, TNodeHost nodeHost);
		
		public bool Delete(IAddonHost host)
		{
			if (host is TNodeHost)
				return DeleteNode((TNodeHost) host);
			
			return false;
		}
		
		protected abstract bool DeleteNode(TNodeHost nodeHost);
		
		public bool Clone(INodeInfo nodeInfo, string path, string name, string category, string version)
		{
			if (Path.GetExtension(nodeInfo.Filename) == FileExtension)
			{
				if (!IsLoaded(nodeInfo.Filename))
					LoadAndCacheNodeInfos(nodeInfo.Filename);
				
				return CloneNode(nodeInfo, path, name, category, version);
			}
			
			return false;
		}
		
		protected virtual bool CloneNode(INodeInfo nodeInfo, string path, string name, string category, string version)
		{
			return false;
		}
		
		#endregion IAddonFactory
		
		#region directory and watcher
		
		void FDirectoryWatcher_Changed(object sender, FileSystemEventArgs e)
		{
			if (System.IO.Path.HasExtension(e.FullPath))
				FileChanged(e.FullPath);
			else
				DirectoryChanged(e.FullPath);
		}
		
		void FDirectoryWatcher_Created(object sender, FileSystemEventArgs e)
		{
			AddFile(e.FullPath);
		}
		
		void FDirectoryWatcher_Deleted(object sender, FileSystemEventArgs e)
		{
			RemoveFile(e.FullPath);
		}
		
		void FDirectoryWatcher_Renamed(object sender, RenamedEventArgs e)
		{
			RemoveFile(e.OldFullPath);
			AddFile(e.FullPath);
		}
		
		#endregion directory and watcher
		
		#region caching
		
		protected bool HasCachedNodeInfos(string filename)
		{
			if (FNodeInfoCache.ContainsKey(filename))
				return true;
			else
			{
				// See if node info is cached on disk.
				var cacheFile = GetCacheFile(filename);
				return File.Exists(cacheFile) && IsCacheFileUpToDate(filename, cacheFile);
			}
		}
		
		protected List<INodeInfo> GetCachedNodeInfos(string filename)
		{
			if (!FNodeInfoCache.ContainsKey(filename))
			{
				// Load node infos from cache file into memory.
				try 
				{
					var cacheFile = GetCacheFile(filename);
					var formatter = new BinaryFormatter();
					using (var stream = new FileStream(cacheFile, FileMode.Open, FileAccess.Read, FileShare.Read))
					{
						FNodeInfoCache[filename] = (List<INodeInfo>) formatter.Deserialize(stream);
					}
				} 
				catch (Exception) 
				{
					FLogger.Log(LogType.Warning, "Cache file for {0} missing or not valid.", filename);
					LoadAndCacheNodeInfos(filename);
				}
			}
			return FNodeInfoCache[filename];
		}
		
		protected IEnumerable<INodeInfo> LoadAndCacheNodeInfos(string filename)
		{
			var nodeInfos = GetNodeInfos(filename);
			FLogger.Log(LogType.Debug, "Loaded node infos from {0}.", filename);
			
			FLoadedFiles[filename] = true;
			CacheNodeInfos(filename, nodeInfos.ToList());
			
			return nodeInfos;
		}
		
		protected void CacheNodeInfos(string filename, List<INodeInfo> nodeInfos)
		{
			FNodeInfoCache[filename] = nodeInfos;
			
			var cacheFile = GetCacheFile(filename);
			var cacheDir = Path.GetDirectoryName(cacheFile);
			if (!Directory.Exists(cacheDir))
			{
				// Create hidden cache dir.
				var directoryInfo = Directory.CreateDirectory(cacheDir);
				directoryInfo.Attributes = FileAttributes.Directory | FileAttributes.Hidden;
			}
			
			// Write nodeInfoList to cache file.
			var formatter = new BinaryFormatter();
			using (var stream = new FileStream(cacheFile, FileMode.Create, FileAccess.Write, FileShare.None))
			{
				formatter.Serialize(stream, nodeInfos);
			}
			
			// Set last write time of cache file to time of filename.
			File.SetLastWriteTime(cacheFile, File.GetLastWriteTime(filename));
			
			FLogger.Log(LogType.Debug, "Cached node infos of {0}.", filename);
		}
		
		protected void ClearCachedNodeInfos(string filename)
		{
			var cacheFile = GetCacheFile(filename);
			if (File.Exists(cacheFile))
				File.Delete(cacheFile);
			
			FNodeInfoCache.Remove(filename);
			
			FLogger.Log(LogType.Debug, "Cleared node info cache of {0}.", filename);
		}
		
		private string GetCacheFile(string filename)
		{
			var cacheDir = Path.GetDirectoryName(filename).ConcatPath(".cache");
			var cacheFilename = Path.GetFileName(filename) + ".cache";
			return cacheDir.ConcatPath(cacheFilename);
		}
		
		private bool IsCacheFileUpToDate(string file, string cacheFile)
		{
			return File.GetLastWriteTime(cacheFile) == File.GetLastWriteTime(file);
		}
		
		#endregion
		
		#region file handling
		//remove all addons included with this filename
		protected virtual void RemoveFile(string filename)
		{
			var nodeInfoCache = GetCachedNodeInfos(filename);
			
			foreach (var nodeInfo in nodeInfoCache)
				OnNodeInfoRemoved(nodeInfo);
		}
		
		//add all addons included with this filename
		protected virtual void AddFile(string filename)
		{
			foreach(var nodeInfo in ExtractNodeInfos(filename))
				OnNodeInfoAdded(nodeInfo);
		}
		
		//allow subclasses to react to a filechange
		protected virtual void FileChanged(string filename)
		{
			//compare those new nodeinfos
			//with nodeinfos so far associated with this filename
			//add nodeinfos that are new in this filename
			//remove nodeinfos that are no longer with this filename
			
			//get all old nodeinfos associated with this filename
			var oldInfos = new Dictionary<INodeInfo, bool>();
			
			if (HasCachedNodeInfos(filename))
			{
				var nodeInfoCache = GetCachedNodeInfos(filename);
				foreach(var nodeInfo in nodeInfoCache)
					oldInfos.Add(nodeInfo, false);
				
				// Clear the cache.
				ClearCachedNodeInfos(filename);
			}
			
			//compare those oldInfos with the newly extracted ones
			//if a newly extracted one is present in the old ones do nothing
			//if a newly extracted one is not present in the old ones add it
			
			foreach(var newNodeInfo in ExtractNodeInfos(filename))
			{
				bool present = false;
				foreach(var entry in oldInfos)
				{
					var oldNodeInfo = entry.Key;
					if (oldNodeInfo.Equals(newNodeInfo))
					{
						oldInfos[oldNodeInfo] = true; //mark as used
						//adding a nodeinfo with the same username will trigger a nodeinfo.update
						OnNodeInfoAdded(newNodeInfo);
						present = true;
						break;
					}
				}
				if (!present)
					OnNodeInfoAdded(newNodeInfo);
			}
			
			//remove old nodeinfos that have no new compatible
			foreach(var entry in oldInfos)
			{
				var oldNodeInfo = entry.Key;
				var present = entry.Value;
				if (!present)
					OnNodeInfoRemoved(oldNodeInfo);
			}
		}
		
		//allow subclasses to react to a directorychange
		protected virtual void DirectoryChanged(string path)
		{
			//nothing to do here
		}
		
		//allow subclasses to cleanup before directory scan.
		protected virtual void DeleteArtefacts(string dir)
		{
			foreach (string subDir in Directory.GetDirectories(dir))
				DeleteArtefacts(subDir);
		}
		
		//register all files in a directory
		protected virtual void ScanForFiles(string dir)
		{
			foreach (string filename in Directory.GetFiles(dir, @"*" + FFileExtension))
				AddFile(filename);
			
			foreach (string subDir in Directory.GetDirectories(dir))
				ScanForFiles(subDir);
		}
		#endregion file handling
	}
}
