using System;
using System.IO;
using System.Runtime.Caching;
using System.Collections.Generic;
using VVVV.Core.Logging;

namespace VVVV.Core.Model
{
    public class TextDocument : Document, ITextDocument
    {
        private string FTextContent;
        
        public event TextDocumentHandler ContentChanged;
        
        public string TextContent
        {
            get
            {
                if (FTextContent == null)
                    FTextContent = TextContentOnDisk;
                return FTextContent;
            }
            set
            {
                var oldValue = FTextContent;
                FTextContent = value;
                if (oldValue != value)
                    OnContentChanged(oldValue, value);
            }
        }

        public string TextContentOnDisk
        {
            get
            {
                var cache = MemoryCache.Default;
                var path = LocalPath;
                var content = cache.Get(path) as string;
                if (content == null)
                {
                    var policy = new CacheItemPolicy();
                    policy.AbsoluteExpiration = DateTimeOffset.Now.AddSeconds(10.0);

                    var filePaths = new List<string>();
                    filePaths.Add(path);

                    policy.ChangeMonitors.Add(new
                        HostFileChangeMonitor(filePaths));

                    content = File.Exists(path)
                        ? File.ReadAllText(path)
                        : string.Empty;

                    cache.Add(path, content, policy);
                }
                return content;
            }
        }
        
        public TextDocument(string name, string path)
            : base(name, path)
        {
        }
        
        public override void SaveTo(string path)
        {
            // Make sure the path exists.
            var documentDir = Path.GetDirectoryName(path);
            if (!Directory.Exists(documentDir))
                Directory.CreateDirectory(documentDir);
            try
            {
                File.WriteAllText(path, TextContent);
            }
            catch (Exception e)
            {
                Shell.Instance.Logger.Log(e);
            }
        }
        
        protected virtual void OnContentChanged(string oldConent, string content)
        {
            if (ContentChanged != null)
                ContentChanged(this, content);
        }

        public bool IsDirty { get { return TextContent != TextContentOnDisk; } }

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
