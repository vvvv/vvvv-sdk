
using System;

namespace VVVV.Core.Model
{
    public abstract class ProjectItem : IDItem, IProjectItem
    {
        public ProjectItem(string name)
            : base(name)
        {
        }

        protected override void DisposeManaged()
        {
            OnDisposed();
            base.DisposeManaged();
        }
        
        public virtual IProject Project 
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

        public event EventHandler Disposed;

        protected virtual void OnDisposed()
        {
            if (Disposed != null)
                Disposed(this, EventArgs.Empty);
        }
    }
}
