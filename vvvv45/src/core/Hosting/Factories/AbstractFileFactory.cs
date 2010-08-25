using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Threading;
using System.Windows.Forms;

using Nito.Async;
using VVVV.Core;
using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;

namespace VVVV.Hosting.Factories
{
	/// <summary>
	/// Superclass for factories which watch files in a directory
	/// </summary>
	
	public abstract class AbstractFileFactory : IAddonFactory
	{
		#region fields and constructor
		
		//directory to watch
		protected string FDirectory;
		protected string FFileExtension;
		private FileSystemWatcher FDirectoryWatcher;
		private Dictionary<INodeInfo, string> FNodeInfos = new Dictionary<INodeInfo, string>();
		private GenericSynchronizingObject FSyncContext;
		
		public AbstractFileFactory()
		{
			FDirectory = Shell.CallerPath;
			FSyncContext = new GenericSynchronizingObject();
		}
		
		#endregion fields and constructor
		
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
		
		//return nodeinfos from filename
		public abstract IEnumerable<INodeInfo> ExtractNodeInfos(string filename);
		
		public virtual void StartWatching()
		{
			if (Directory.Exists(FDirectory))
			{
				//watch this directory
				if (FDirectoryWatcher == null)
				{
					FDirectoryWatcher = new FileSystemWatcher(FDirectory, @"*" + FFileExtension);
					FDirectoryWatcher.SynchronizingObject = FSyncContext;
					FDirectoryWatcher.IncludeSubdirectories = true;
					FDirectoryWatcher.NotifyFilter = NotifyFilters.LastWrite;
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
				
				ScanForFiles(FDirectory);
			}
		}
		
		public virtual bool Create(INodeInfo nodeInfo, IAddonHost host)
		{
		    return false;
		}
		
		public virtual bool Delete(IAddonHost host)
		{
			return false;
		}
		
		// TODO: Make this abstract
		public virtual bool Clone(INodeInfo nodeInfo, string path, out INodeInfo newNodeInfo)
		{
			newNodeInfo = null;
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
		
		#region file handling
		//remove all addons included with this filename
		private void RemoveFile(string filename)
		{
			foreach(var ni in ExtractNodeInfos(filename))
			{
				FNodeInfos.Remove(ni);
				OnNodeInfoRemoved(ni);
			}
		}
		
		//add all addons included with this filename
		protected void AddFile(string filename)
		{
			foreach(var ni in ExtractNodeInfos(filename))
			{
				FNodeInfos.Add(ni, filename);
				OnNodeInfoAdded(ni);
			}
		}
		
		//allow subclasses to react to a filechange
		private void FileChanged(string filename)
		{
			//compare those new nodeinfos
			//with nodeinfos so far associated with this filename
			//add nodeinfos that are new in this filename
			//remove nodeinfos that are no longer with this filename
			
			//get all old nodeinfos associated with this filename
			Dictionary<INodeInfo, bool> oldInfos = new Dictionary<INodeInfo, bool>();
			foreach(var ni in FNodeInfos)
				if (ni.Key.Filename == filename)
					oldInfos.Add(ni.Key, false);
			
			//compare those oldInfos with the newly extracted ones
			//if a newly extracted one is present in the old ones do nothing
			//if a newly extracted one is not present in the old ones add it
			
			foreach(var newNI in ExtractNodeInfos(filename))
			{
				bool present = false;
				foreach(var oldNI in oldInfos)
				{
					if (oldNI.Key.Equals(newNI))
					{
						oldInfos[oldNI.Key] = true; //mark as used
						//adding a nodeinfo with the same username will trigger a nodeinfo.update
						OnNodeInfoAdded(newNI);
						present = true;
						break;
					}
				}
				if (!present)
				{
					FNodeInfos.Add(newNI, filename);
					OnNodeInfoAdded(newNI);
				}
			}
			
			//remove old nodeinfos that have no new compatible
			foreach(var old in oldInfos)
			{
				if (!old.Value)
				{
					FNodeInfos.Remove(old.Key);
					OnNodeInfoRemoved(old.Key);
				}
			}
		}
		
		//allow subclasses to react to a directorychange
		protected virtual void DirectoryChanged(string path)
		{
			//nothing to do here
		}
		
		//register all files in a directory
		protected virtual void ScanForFiles(string dir)
		{
			foreach (string filename in Directory.GetFiles(dir, @"*" + FFileExtension))
			{
				AddFile(filename);
			}
			
			foreach (string subDir in Directory.GetDirectories(dir))
			{
				ScanForFiles(subDir);
			}
		}
		#endregion file handling
	}
}
