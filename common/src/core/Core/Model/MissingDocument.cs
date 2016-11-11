using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace VVVV.Core.Model
{
    public class MissingDocument : IDContainer, IDocument
    {
        public MissingDocument(string name, string path, bool canBeCompiled)
            : base(name)
        {
            LocalPath = path;
            CanBeCompiled = canBeCompiled;
        }

        protected override void DisposeManaged()
        {
            if (Disposed != null)
                Disposed(this, EventArgs.Empty);
            base.DisposeManaged();
        }

        public string LocalPath
        {
            get;
            private set;
        }

        public void SaveTo(string path)
        {
            // Do nothing
        }

        public IProject Project
        {
            get;
            set;
        }

        public bool CanBeCompiled
        {
            get;
            private set;
        }

        public event EventHandler Disposed;

        public event EventHandler<ContentChangedEventArgs> ContentChanged;

        public event EventHandler<EventArgs> FileChanged;

        public bool IsDirty
        {
            get { return false; }
        }

        public bool IsReadOnly
        {
            get { return false; }
            set { throw new NotSupportedException(); }
        }

        public Stream Content
        {
            get { return null; }
            set { throw new NotSupportedException(); }
        }

        public Stream ContentOnDisk
        {
            get { return null; }
        }
    }
}
