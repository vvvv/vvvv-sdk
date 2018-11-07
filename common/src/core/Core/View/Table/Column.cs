using System;

namespace VVVV.Core.View.Table
{
    public class Column : INamed
    {
        protected string FName;
        
        public event RenamedHandler Renamed;
        
        public AutoSizeColumnMode AutoSizeMode
        {
        	get;
        	set;
        }
        
        public string Name 
        {
            get 
            {
                return FName;
            }
            private set
            {
                if (FName != value)
                    OnRenamed(value);
                FName = value;
            }
        }
        
        public Column(string name)
            :this(name, AutoSizeColumnMode.ColumnHeader)
        {
        }
        
        public Column(string name, AutoSizeColumnMode autoSizeMode)
        {
            Name = name;
            AutoSizeMode = autoSizeMode;
        }
        
        protected virtual void OnRenamed(string newName)
        {
            if (Renamed != null) {
                Renamed(this, newName);
            }
        }
    }
}
