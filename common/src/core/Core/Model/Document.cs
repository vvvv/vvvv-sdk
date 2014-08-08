using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Caching;
using VVVV.Core.Logging;

namespace VVVV.Core.Model
{
    public abstract class Document : IDContainer, IDocument
    {
        public Document(string name, string path)
            : base(name)
        {
            LocalPath = path;
        }

        protected override void DisposeManaged()
        {
            OnDisposed();
            base.DisposeManaged();
        }
        
        public IProject Project
        {
            get;
            set;
        }
        
        public virtual bool CanBeCompiled
        {
            get
            {
                return false;
            }
        }

        public event EventHandler<ContentChangedEventArgs> ContentChanged;

        private Stream FContent;
        public Stream Content
        {
            get
            {
                if (FContent == null)
                {
                    FContent = new MemoryStream();
                    ContentOnDisk.CopyTo(FContent);
                }
                return FContent;
            }
            set
            {
                if (value != FContent)
                {
                    var oldContent = FContent;
                    FContent = value;
                    try
                    {
                        if (!oldContent.StreamEquals(value))
                            OnContentChanged(value);
                    }
                    finally
                    {
                        oldContent.Dispose();
                    }
                }
            }
        }

        public Stream ContentOnDisk
        {
            get
            {
                var cache = MemoryCache.Default;
                var path = LocalPath;
                var content = cache.Get(path) as Stream;
                if (content == null)
                {
                    var policy = new CacheItemPolicy();
                    policy.AbsoluteExpiration = DateTimeOffset.Now.AddSeconds(10.0);

                    var filePaths = new List<string>();
                    filePaths.Add(path);

                    policy.ChangeMonitors.Add(new
                        HostFileChangeMonitor(filePaths));

                    content = new MemoryStream();
                    if (File.Exists(path))
                    {
                        using (var fileStream = new FileStream(path, FileMode.Open, FileAccess.Read))
                            fileStream.CopyTo(content);
                        content.Position = 0;
                    }

                    cache.Add(path, content, policy);
                }
                return content;
            }
        }
        
        public override string ToString()
        {
            return string.Format("Document {0}", Name);
        }

        public string LocalPath { get; private set; }

        public event EventHandler Disposed;

        protected virtual void OnDisposed()
        {
            if (Disposed != null)
                Disposed(this, EventArgs.Empty);
        }

        public void Save()
        {
            SaveTo(LocalPath);
        }

        public virtual void SaveTo(string path)
        {
            // Make sure the path exists.
            var documentDir = Path.GetDirectoryName(path);
            if (!Directory.Exists(documentDir))
                Directory.CreateDirectory(documentDir);
            try
            {
                Content.Position = 0;
                using (var fileStream = new FileStream(path, FileMode.Create))
                    Content.CopyTo(fileStream);
            }
            catch (Exception e)
            {
                Shell.Instance.Logger.Log(e);
            }
        }

        protected virtual void OnContentChanged(Stream newContent)
        {
            if (ContentChanged != null)
                ContentChanged(this, new ContentChangedEventArgs(newContent));
        }

        public bool IsDirty { get { return !Content.StreamEquals(ContentOnDisk); } }

        public bool IsReadOnly
        {
            get
            {
                if (File.Exists(LocalPath))
                    return (File.GetAttributes(LocalPath) & FileAttributes.ReadOnly) == FileAttributes.ReadOnly;
                else
                    return false;
            }
            set
            {
                new FileInfo(LocalPath).IsReadOnly = value;
            }
        }
    }
}
