using System;
using System.Collections.Generic;
using System.IO;

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

        public abstract void SaveTo(string path);
    }
}
