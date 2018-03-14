using System;
using System.Runtime.InteropServices;
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
using System.Collections.Concurrent;

namespace VVVV.Hosting.Factories
{
    /// <summary>
    /// Superclass for factories which watch files in a directory
    /// </summary>
    [ComVisible(false)]
    public abstract class AbstractFileFactory<TNodeHost> : IDisposable, IAddonFactory where TNodeHost: INode
    {
        public class TimestampedFile
        {
            public DateTime Time;
            public string Filename;
        }

        #region fields and constructor
        
        [Import]
        protected ILogger FLogger;
        
        [Import]
        protected IHDEHost FHDEHost;
        
        [Import]
        protected INodeInfoFactory FNodeInfoFactory;
        
        //directory to watch
        private List<string> FFileExtension;
        private List<string> FFiles = new List<string>();
        
        private List<FileSystemWatcher> FDirectoryWatcher = new List<FileSystemWatcher>();
        
        private GenericSynchronizingObject FSyncContext;

        private ConcurrentQueue<TimestampedFile> FNewFilesQueue = new ConcurrentQueue<TimestampedFile>();
        private System.Timers.Timer FNewFilesTimer = new System.Timers.Timer(1000);
        
        public AbstractFileFactory(string fileExtension)
        {
            FFileExtension = new List<string>();
            FFileExtension.AddRange(fileExtension.Split(new char[]{';'}, StringSplitOptions.RemoveEmptyEntries));
            Debug.Assert(SynchronizationContext.Current != null, "SynchronizationContext not set.");
            FSyncContext = new GenericSynchronizingObject();

            FNewFilesTimer.SynchronizingObject = FSyncContext;
            FNewFilesTimer.Elapsed += FNewFilesTimer_Elapsed;
        }

        private void FNewFilesTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            TimestampedFile file;
            while (FNewFilesQueue.TryPeek(out file))
            {
                //dequeue a file only if is has been in the queue for at least 1s 
                if (DateTime.Now - file.Time > TimeSpan.FromSeconds(1))
                {
                    if (FNewFilesQueue.TryDequeue(out file))
                        AddFile(file.Filename);
                }
                else //don't check any more files in the queue for now if the latest one is not yet old enough
                    break;
            }

            //keep the timer active as long as there are items in the queue
            FNewFilesTimer.Enabled = FNewFilesQueue.Any();
        }

        #endregion fields and constructor
        public abstract string JobStdSubPath
        {
            get;
        }
        
        public string Name
        {
            get
            {
                return ToString();
            }
        }
        
        public bool AllowCaching
        {
            get
            {
                return true;
            }
        }
        
        public List<string> DirectoriesToWatch
        {
            get; private set;
        }
        
        public List<string> FileExtension
        {
            get
            {
                return FFileExtension;
            }
        }
        
        #region IAddonFactory
        //return nodeinfos from systemname
        public INodeInfo[] ExtractNodeInfos(string filename, string arguments)
        {
            if (!FileExtension.Contains(Path.GetExtension(filename)))
                return new INodeInfo[0];
            if (filename.EndsWith("nodelist.xml"))
                return new INodeInfo[0];
            
            // Regardless of the arguments, we need to load the node infos first.
            var nodeInfos = LoadNodeInfos(filename).ToArray();
            
            // If additional arguments are present vvvv is only interested in one specific
            // NodeInfo -> look for it.
            if (!string.IsNullOrEmpty(arguments))
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
            if (host is TNodeHost && FileExtension.Contains(Path.GetExtension(nodeInfo.Filename)))
                return CreateNode(nodeInfo, (TNodeHost) host);
            
            return false;
        }
        
        protected abstract bool CreateNode(INodeInfo nodeInfo, TNodeHost nodeHost);
        
        public bool Delete(INodeInfo nodeInfo, INode host)
        {
            if (host is TNodeHost && FileExtension.Contains(Path.GetExtension(nodeInfo.Filename)))
                return DeleteNode(nodeInfo, (TNodeHost) host);
            
            return false;
        }
        
        protected abstract bool DeleteNode(INodeInfo nodeInfo, TNodeHost nodeHost);
        
        public bool Clone(INodeInfo nodeInfo, string path, string name, string category, string version, out INodeInfo newNodeInfo)
        {
            if (FileExtension.Contains(Path.GetExtension(nodeInfo.Filename)))
            {
                string filename;
                if (CloneNode(nodeInfo, path, name, category, version, out filename))
                {
                    AddFile(filename);
                    
                    foreach (var possibleNodeInfo in FNodeInfoFactory.NodeInfos.Where(ni => string.Compare(ni.Filename, filename, true) == 0))
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

        public virtual bool GetNodeListAttribute(INodeInfo nodeInfo, out string name, out string value)
        {
            name = string.Empty;
            value = string.Empty;
            return false;
        }

        public virtual void ParseNodeEntry(System.Xml.XmlReader xmlReader, INodeInfo nodeInfo)
        {
            
        }
        
        #endregion IAddonFactory
        
        #region directory and watcher
        
        protected virtual void AddSubDir(string dir, bool recursive)
        {
            foreach (var ext in FFileExtension)
            {
                foreach (string filename in Directory.GetFiles(dir, @"*" + ext))
                {
                    try
                    {
                        AddFile(filename);
                    }
                    catch (Exception e)
                    {
                        FLogger.Log(e);
                    }
                }
            }
            
            if (recursive)
            {
                foreach (string subDir in Directory.GetDirectories(dir))
                {
                    if (!(subDir.EndsWith(".svn") || subDir.EndsWith(".cache")))
                    {
                        try
                        {
                            AddSubDir(subDir, recursive);
                        }
                        catch (Exception e)
                        {
                            FLogger.Log(e);
                        }
                    }
                }
            }
        }
        
        
        //register all files in a directory
        public void AddDir(string dir, bool recursive)
        {
            if (!Directory.Exists(dir))
            {
                FLogger.Log(LogType.Warning, "{0} can't scan non-existent directory: {1}", this, dir);
                return;
            }
            
            AddSubDir(dir, recursive);

            var dirWatcher = new FileSystemWatcher(dir, @"*" + FFileExtension[0]);
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
            {
                FNewFilesQueue.Enqueue(new TimestampedFile() { Time = DateTime.Now, Filename = e.FullPath });
                FNewFilesTimer.Enabled = true;
            }
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
            if (File.Exists(e.FullPath) && FileExtension.Contains(Path.GetExtension(e.FullPath)))
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
            var fnUri = new Uri(filename);
            foreach (var nodeInfo in FNodeInfoFactory.NodeInfos)
            {
                if (!string.IsNullOrEmpty(nodeInfo.Filename))
                {
                    Uri niUri = null;
                    if (Uri.TryCreate(nodeInfo.Filename, UriKind.RelativeOrAbsolute, out niUri))
                        if (niUri == fnUri)
                            FNodeInfoFactory.DestroyNodeInfo(nodeInfo);
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
            ExtractNodeInfos(filename, null);
        }
        
        //allow subclasses to react to a filechange
        protected void FileChanged(string filename)
        {
            //compare those new nodeinfos
            //with nodeinfos so far associated with this filename
            //add nodeinfos that are new in this filename
            //remove nodeinfos that are no longer with this filename
            
            //get all old nodeinfos associated with this filename
            var oldInfos = FNodeInfoFactory.NodeInfos.Where((ni) => ni.Filename == filename && ni.Factory == this).ToList();
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
        
        #endregion file handling
        
        public virtual void Dispose()
        {
            FNewFilesTimer.Enabled = false;
            FNewFilesTimer.Dispose();
        }
    }
}
