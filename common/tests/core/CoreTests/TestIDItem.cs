using System;
using VVVV.Core;

namespace CoreTests
{
    [Serializable]
    class TestIDItem : IIDItem
    {
        public TestIDItem(string name)
        {
            Name = name;
            Mapper = new ModelMapper(this, new MappingRegistry());
        }
        
        [NonSerialized]
        private IIDContainer FOwner;
        public IIDContainer Owner {
            get
            {
                return FOwner;
            }
            set
            {
                FOwner = value;
                IsRooted = FOwner.IsRooted;
            }
        }
        
        [NonSerialized]
        private IModelMapper FModelMapper;
        public IModelMapper Mapper 
        {
            get
            {
                return FModelMapper;
            }
            set
            {
                FModelMapper = value;
            }
        }
    
        public string Name {
            get;
            private set;
        }
        
        public event RenamedHandler Renamed;
        
        protected virtual void OnRenamed(string newName)
        {
            if (Renamed != null) {
                Renamed(this, newName);
            }
        }
        
        public bool IsRooted
        {
            get;
            private set;
        }
        
        public event RootingChangedEventHandler RootingChanged;
    }
}
