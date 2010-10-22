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
	public abstract class AbstractFileFactory<TNodeHost> : IDisposable, IAddonFactory where TNodeHost: IAddonHost
	{
		#region fields and constructor
		
		[Import]
		protected ILogger FLogger;
		
		[Import]
		protected IHDEHost FHDEHost;
		
		//directory to watch
		private string FDirectory;
		private string FFileExtension;
		protected Dictionary<string, List<INodeInfo>> FNodeInfoCache = new Dictionary<string, List<INodeInfo>>();
		private Dictionary<string, bool> FLoadedFiles = new Dictionary<string, bool>();
		private List<FileSystemWatcher> FDirectoryWatcher = new List<FileSystemWatcher>();
		private GenericSynchronizingObject FSyncContext;
		
		public AbstractFileFactory(string fileExtension)
		{
			FFileExtension = fileExtension;
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
		
		protected void UpdateNodeInfos(string filename)
		{
			if (!IsLoaded(filename))
			{
				foreach (var nodeInfo in LoadAndCacheNodeInfos(filename))
				{
					OnNodeInfoUpdated(nodeInfo);
				}
			}
		}
		
		public bool Create(INodeInfo nodeInfo, IAddonHost host)
		{
			if (host is TNodeHost && Path.GetExtension(nodeInfo.Filename) == FileExtension)
			{
				// We don't know if nodeInfo was cached. Some properties like Executable
				// might be not set -> Update the nodeInfo.
				UpdateNodeInfos(nodeInfo.Filename);
				
				return CreateNode(nodeInfo, (TNodeHost) host);
			}
			
			return false;
		}
		
		protected abstract bool CreateNode(INodeInfo nodeInfo, TNodeHost nodeHost);
		
		public bool Delete(INodeInfo nodeInfo, IAddonHost host)
		{
			if (host is TNodeHost && Path.GetExtension(nodeInfo.Filename) == FileExtension)
				return DeleteNode(nodeInfo, (TNodeHost) host);
			
			return false;
		}
		
		protected abstract bool DeleteNode(INodeInfo nodeInfo, TNodeHost nodeHost);
		
		public bool Clone(INodeInfo nodeInfo, string path, string name, string category, string version)
		{
			if (Path.GetExtension(nodeInfo.Filename) == FileExtension)
			{
				// We don't know if nodeInfo was cached. Some properties like Executable
				// might be not set -> Update the nodeInfo.
				UpdateNodeInfos(nodeInfo.Filename);
				
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
		
		private void AddSubDir(string dir)
		{
			foreach (string filename in Directory.GetFiles(dir, @"*" + FFileExtension))
			{
				try {
					AddFile(filename);
				} catch (Exception e) {
					FLogger.Log(e);
				}
			}
			
			foreach (string subDir in Directory.GetDirectories(dir))
			{
				if (!(subDir.EndsWith(".svn") || subDir.EndsWith(".cache")))
					try {
						AddSubDir(subDir);
					} catch (Exception e) {
						FLogger.Log(e);
					}
			}						
		}
		
		
		//register all files in a directory		
		public void AddDir(string dir)
		{		
			//give subclasses a chance to cleanup before we start to scan.
			DeleteArtefacts(dir);

			AddSubDir(dir);
			
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
		
		private void RemoveSubDir(string dir)
		{
			foreach (string filename in Directory.GetFiles(dir, @"*" + FFileExtension))
			{
				try {
					RemoveFile(filename);
				} catch (Exception e) {
					FLogger.Log(e);
				}
			}			

			foreach (string subDir in Directory.GetDirectories(dir))
			{
				if (!(subDir.EndsWith(".svn") || subDir.EndsWith(".cache")))
					try {
						RemoveSubDir(subDir);
					} catch (Exception e) {
						FLogger.Log(e);
					}
			}			
		}
		
		public void RemoveDir(string dir)
		{
			RemoveSubDir(dir);
			
			for (int i=FDirectoryWatcher.Count-1; i>=0; i--)
			{
				var dirWatcher = FDirectoryWatcher[i];
				if (dirWatcher.Path == dir)
				{
					dirWatcher.Changed -= new FileSystemEventHandler(FDirectoryWatcher_Changed);
					dirWatcher.Created -= new FileSystemEventHandler(FDirectoryWatcher_Created);
					dirWatcher.Deleted -= new FileSystemEventHandler(FDirectoryWatcher_Deleted);
					dirWatcher.Renamed -= new RenamedEventHandler(FDirectoryWatcher_Renamed);
					FDirectoryWatcher.RemoveAt(i);
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
						var nodeInfos = (List<INodeInfo>) formatter.Deserialize(stream);
						if (ValidateNodeInfos(filename, nodeInfos))
							FNodeInfoCache[filename] = nodeInfos;
						else
						{
							stream.Close();
							LoadAndCacheNodeInfos(filename);
						}
					}
				}
				catch (Exception)
				{
					FLogger.Log(LogType.Warning, "Cache file for {0} missing or not valid.", filename);
					LoadAndCacheNodeInfos(filename);
				}
			}
			
//			if (FNodeInfoCache[filename].Count == 0)
//				FLogger.Log(LogType.Warning, "Empty cache for {0}.", filename);
			
			return FNodeInfoCache[filename];
		}
		
		protected bool ValidateNodeInfos(string fileName, List<INodeInfo> nodeInfos)
		{
			var path = Path.GetDirectoryName(fileName);
			foreach(var n in nodeInfos)
				if (Path.GetDirectoryName(n.Filename) != path) return false;
			
			return true;
		}
		
		protected IEnumerable<INodeInfo> LoadAndCacheNodeInfos(string filename)
		{
			try
			{
				var nodeInfos = GetNodeInfos(filename);
				FLogger.Log(LogType.Debug, "Loaded node infos from {0}.", filename);
				
				FLoadedFiles[filename] = true;
				CacheNodeInfos(filename, nodeInfos.ToList());
				
				return nodeInfos;
			}
			catch (Exception e)
			{
				FLogger.Log(e);
				return new INodeInfo[0];
			}
		}
		
		protected void CacheNodeInfos(string filename, List<INodeInfo> nodeInfos)
		{
			try {
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
			} catch (Exception e) {
				FLogger.Log(LogType.Error, "Caching node infos of {0} failed:", filename);
				FLogger.Log(e);
			}
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
				if (nodeInfo.Filename == filename)
					OnNodeInfoRemoved(nodeInfo);
		}
		
		//add all addons included with this filename
		protected virtual void AddFile(string filename)
		{
			foreach(var nodeInfo in ExtractNodeInfos(filename))
				if (nodeInfo.Filename == filename)
					OnNodeInfoAdded(nodeInfo);
				else
					throw new Exception(filename + " wants to add a nodeinfo with incorrect filename property: " + nodeInfo.Filename);
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
						OnNodeInfoUpdated(newNodeInfo);
						present = true;
						break;
					}
				}
				if (!present)
					OnNodeInfoUpdated(newNodeInfo);
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
			{
				try {
					DeleteArtefacts(subDir);
				} catch (Exception e) {
					FLogger.Log(e);
				}
			}
		}
		
		#endregion file handling
		
		public void Dispose()
		{
			throw new NotImplementedException();
		}
	}
}
