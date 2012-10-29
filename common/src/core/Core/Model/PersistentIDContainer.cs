using System;
using System.IO;
using VVVV.Core.Logging;

namespace VVVV.Core.Model
{
    public abstract class PersistentIDContainer : IDContainer, IPersistent
    {
        public PersistentIDContainer(string name, Uri location, bool isRooted = false)
            : base(name, isRooted)
        {
            Location = location;

            // If the location doesn't exist, set IsLoaded property to true
            if (!File.Exists(Location.LocalPath))
            {
                IsLoaded = true;
            }
        }

        public event EventHandler Loaded;

        protected virtual void OnLoaded(EventArgs e)
        {
            if (Loaded != null)
            {
                Loaded(this, e);
            }
        }

        public event EventHandler Unloaded;

        protected virtual void OnUnloaded(EventArgs e)
        {
            if (Unloaded != null)
            {
                Unloaded(this, e);
            }
        }

        public event EventHandler Saved;

        protected virtual void OnSaved(EventArgs e)
        {
            if (Saved != null)
            {
                Saved(this, e);
            }
        }

        public Uri Location
        {
            get;
            private set;
        }

        public bool IsDirty
        {
            get;
            protected set;
        }

        public bool IsLoaded
        {
            get;
            private set;
        }

        public virtual bool IsReadOnly
        {
            get
            {
                var fileName = Location.LocalPath;
                return (File.GetAttributes(fileName) & FileAttributes.ReadOnly) == FileAttributes.ReadOnly;
            }
            set
            {
                var fileName = Location.LocalPath;
                new FileInfo(fileName).IsReadOnly = value;
            }
        }

        public void Load()
        {
            DebugHelpers.CatchAndLog(
                () =>
                {
                    DoLoad();

                    IsLoaded = true;
                    IsDirty = false;

                    OnLoaded(EventArgs.Empty);
                },
                "Loading document",
                (e) =>
                {
                    IsLoaded = false;
                    IsDirty = true;
                });
        }

        protected abstract void DoLoad();

        public void Unload()
        {
            IsLoaded = false;
            IsDirty = false;

            DoUnload();

            OnUnloaded(EventArgs.Empty);
        }

        protected abstract void DoUnload();

        public void Save()
        {
            if (!IsLoaded)
            {
                throw new InvalidOperationException(string.Format("{0} is not loaded. Save() not allowed.", this));
            }

            SaveTo(Location);
            IsLoaded = true;
            IsDirty = false;
            OnSaved(EventArgs.Empty);
        }

        protected override void OnRenamed(string newName)
        {
            var oldLocation = Location;
            var oldFilename = oldLocation.LocalPath;
            var oldDir = Path.GetDirectoryName(oldFilename);
            var newFilename = Path.Combine(oldDir, newName);

            Save();
            File.Move(oldFilename, newFilename);
            Location = new Uri(newFilename);

            base.OnRenamed(newName);
        }

        public abstract void SaveTo(Uri newLocation);

        protected abstract string CreateName(Uri location);

        public event EventHandler Disposed;

        protected virtual void OnDisposed(EventArgs e)
        {
            if (Disposed != null)
            {
                Disposed(this, e);
            }
        }

        protected override void DisposeManaged()
        {
            Loaded = null;
            Saved = null;

            base.DisposeManaged();

            OnDisposed(EventArgs.Empty);
        }
    }
}
