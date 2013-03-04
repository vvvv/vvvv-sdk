using System;
using System.Collections.Generic;
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
    }
}
