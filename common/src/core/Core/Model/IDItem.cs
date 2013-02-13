using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VVVV.Utils;

namespace VVVV.Core.Model
{
    public class IDItem : IIDItem, IDisposable
    {
        public IDItem(string name, bool isRooted = false)
        {
            FName = name;
            IsRooted = isRooted;
            Changed = true;
        }

        #region IIDItem Members

        public bool Changed { get; private set; }

        public virtual void MarkChanged()
        {
            if (!Changed)
            {
                Changed = true;
                if (Owner != null)
                    Owner.MarkChanged();
            }
        }

        public virtual void AcknowledgeChanges()
        {
            Changed = false;
        }
        
        public ModelMapper Mapper
        {
            get;
            protected set;
        }

        private IIDContainer FOwner;
        public virtual IIDContainer Owner
        {
            get
            {
                return FOwner;
            }
            set
            {
                if (FOwner != null)
                {
                    if (value != null)
                    {
                        throw new Exception(string.Format("ID item {0} ('{1}') has parent already.", this, Name));
                    }
                    
                    // Unsubscribe from old owner
                    FOwner.RootingChanged -= FOwner_RootingChanged;
                    
                    CheckIfRootingChanged(new RootingChangedEventArgs(RootingAction.ToBeUnrooted));
                }

                FOwner = value;
                
                if (FOwner != null)
                {
                    // Subscribe to new owner
                    FOwner.RootingChanged += FOwner_RootingChanged;
                    
                    CheckIfRootingChanged(new RootingChangedEventArgs(RootingAction.Rooted));
                }
            }
        }
        
        public bool IsRooted
        {
            get;
            private set;
        }

        public event RenamedHandler Renamed;

        public event RootingChangedEventHandler RootingChanged;
        
        public virtual void Dispatch(IVisitor visitor)
        {
            visitor.Visit(this);
        }

        private void CheckIfRootingChanged(RootingChangedEventArgs args)
        {
            switch (args.Rooting)
            {
                case RootingAction.Rooted:
                    if (FOwner.IsRooted && !IsRooted)
                    {
                        IsRooted = true;
                        OnRootingChanged(args);
                    }
                    break;
                case RootingAction.ToBeUnrooted:
                    if (FOwner.IsRooted && IsRooted)
                    {
                        OnRootingChanged(args);
                        IsRooted = false;
                    }
                    break;
            }
        }
        
        private void OnRootingChanged(RootingChangedEventArgs args)
        {
            if (args.Rooting == RootingAction.Rooted)
            {
                Mapper = FOwner.Mapper.CreateChildMapper(this);
                OnRootingChanged(RootingAction.Rooted);
            }
            
            if (RootingChanged != null)
            {
                RootingChanged(this, args);
            }
            
            if (args.Rooting == RootingAction.ToBeUnrooted)
            {
                OnRootingChanged(RootingAction.ToBeUnrooted);
                Mapper.Dispose();
                Mapper = null;
            }
        }
        
        protected virtual void OnRootingChanged(RootingAction rooting)
        {
            
        }
        
        void FOwner_RootingChanged(object sender, RootingChangedEventArgs args)
        {
            // Propagate the event down further in the object graph
            CheckIfRootingChanged(args);
        }

        #endregion

        #region INamed Members

        protected virtual void OnRenamed(string newName)
        {
            if (Renamed != null)
                Renamed(this, newName);
        }

        protected string FName;
        public virtual string Name
        {
            get
            {
                return FName;
            }
            set
            {
                if (value != FName) //(CanRenameTo(value) && (value != FName))
                {
                    OnRenamed(value);
                    FName = value;
                }
            }
        }
        
        protected virtual bool IsRenameable()
        {
            return this is IRenameable;
        }

        #endregion

        #region IRenameable Members

        public virtual bool CanRenameTo(string value)
        {
            // let's return false if not IsRenamable (even if new name == old name)
            // the action itself is forbidden for not IsRenamble
            return (IsRenameable() && ((value == Name) || (Owner == null) || (Owner[value] == null)));
        }

        #endregion
        
        #region IDisposable
        // Use C# destructor syntax for finalization code.
        ~IDItem()
        {
            // Simply call Dispose(false).
            Dispose(false);
        }
        
        public bool IsDisposed
        {
            get;
            private set;
        }
        
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        
        protected void Dispose(bool disposing)
        {
            if (!IsDisposed)
            {
                if (disposing)
                {
                    // Free other state (managed objects).
                    DisposeManaged();
                }
                // Free your own state (unmanaged objects).
                // Set large fields to null.
                DisposeUnmanaged();
            }
            IsDisposed = true;
        }
        
        protected virtual void DisposeManaged()
        {
            if (Mapper != null)
                Mapper.Dispose();

            if (FOwner != null)
            {
                FOwner.RootingChanged -= FOwner_RootingChanged;
            }
        }
        
        protected virtual void DisposeUnmanaged()
        {
            
        }
        #endregion

        public override string ToString()
        {
            return string.Format("{0}: {1}", Name, this.GetType().Name);
        }
    }
}
