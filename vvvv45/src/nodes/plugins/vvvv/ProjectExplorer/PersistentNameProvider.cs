using System;
using System.IO;
using VVVV.Core;
using VVVV.Core.Model;
using VVVV.Utils;

namespace VVVV.HDE.ProjectExplorer
{
    /// <summary>
    /// Must be registered for PersistentIDContainer.
    /// </summary>
    public class PersistentNameProvider : Disposable, INamed
    {
        private readonly PersistentIDContainer FIDContainer;
        
        public PersistentNameProvider(PersistentIDContainer idContainer)
        {
            FIDContainer = idContainer;
            FIDContainer.Renamed += idContainer_Renamed;
            
            Name = Path.GetFileName(FIDContainer.Location.LocalPath);
        }
        
        protected override void DisposeManaged()
        {
            FIDContainer.Renamed -= idContainer_Renamed;
            base.DisposeManaged();
        }
        
        void idContainer_Renamed(INamed sender, string newName)
        {
            newName = Path.GetFileName(newName);
            OnRenamed(newName);
            Name = newName;
        }
        
        public event RenamedHandler Renamed;
        
        protected virtual void OnRenamed(string newName)
        {
            if (Renamed != null) {
                Renamed(this, newName);
            }
        }
        
        public string Name 
        {
            get;
            private set;
        }
    }
}
