using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace VVVV.Core
{
    /// <summary>
    /// Uses a ModelMapper to do the actual mapping. Asking for mappings
    /// before real ModelMapper is set will yield to an InvalidOperationException.
    /// Registrations can be done before real ModelMapper is set.
    /// </summary>
    public class LazyModelMapper : IModelMapper
    {
        protected IModelMapper FMapper;
        protected LazyModelMapper FParent;
        protected Dictionary<Type, object> FInstanceMappings;
        protected Dictionary<Type, object> FDefaultInstanceMappings;
        protected List<Type[]> FTypeMappings;
        
        public LazyModelMapper(object model)
        {
            Model = model;
            FInstanceMappings = new Dictionary<Type, object>();
            FDefaultInstanceMappings = new Dictionary<Type, object>();
            FTypeMappings = new List<Type[]>();
        }
        
        public bool IsInitialized
        {
            get
            {
                return (FMapper != null) && !(FMapper is LazyModelMapper);
            }
        }
        
        public event EventHandler Initialized;
        
        protected virtual void OnInitialized()
        {
            if (Initialized != null) {
                Initialized(this, EventArgs.Empty);
            }
        }
        
        public void Initialize(IModelMapper parentMapper)
        {
            // Is the parent mapper also lazy?
            if (parentMapper is LazyModelMapper)
            {
                var lazyParent = parentMapper as LazyModelMapper;
                if (lazyParent.IsInitialized)
                {
                    // Parent is initialized -> we can intialize.
                    InitializeForReal(lazyParent);
                }
                else
                {
                    // Parent is not initialized -> wait for him to be.
                    lazyParent.Initialized += lazyParent_Initialized;
                }
            }
            else
            {
                InitializeForReal(parentMapper);
            }
        }
        
        void lazyParent_Initialized(object sender, EventArgs e)
        {
            var lazyParent = sender as LazyModelMapper;
            lazyParent.Initialized -= lazyParent_Initialized;
            InitializeForReal(lazyParent);
        }
        
        protected void InitializeForReal(IModelMapper parentMapper)
        {
            FMapper = parentMapper.CreateChildMapper(Model);
            foreach (var mapping in FInstanceMappings)
                FMapper.RegisterMapping(mapping.Key, mapping.Value);
            foreach (var mapping in FDefaultInstanceMappings)
                FMapper.RegisterDefault(mapping.Key, mapping.Value);
            foreach (var mapping in FTypeMappings)
            	FMapper.RegisterMapping(mapping[0], mapping[1], mapping[2]);
            OnInitialized();
        }
        
        public object Model {
            get;
            private set;
        }
        
        public TTo Map<TTo>()
        {
            if (FMapper == null)
                throw new InvalidOperationException("ModelMapper must be set before Map can be invoked on LazyModelMapper.");
            
            return FMapper.Map<TTo>();
        }
        
        public bool CanMap<TTo>()
        {
            if (FMapper == null)
                throw new InvalidOperationException("ModelMapper must be set before CanMap can be invoked on LazyModelMapper.");
            
            return FMapper.CanMap<TTo>();
        }
        
        public IModelMapper CreateChildMapper(object model)
        {
            if (FMapper == null)
                return new LazyModelMapper(model);
            return FMapper.CreateChildMapper(model);
        }

        public void RegisterMapping(Type fromType, object toInstance)
        {
            if (FMapper == null)
                FInstanceMappings[fromType] = toInstance;
            else
                FMapper.RegisterMapping(fromType, toInstance);
        }
        
        public void RegisterDefault(Type fromType, object toInstance)
        {
            if (FMapper == null)
                FDefaultInstanceMappings[fromType] = toInstance;
            else
                FMapper.RegisterDefault(fromType, toInstance);
        }
        
        public void RegisterMapping(Type forType, Type fromType, Type toType)
		{
        	FTypeMappings.Add(new Type[] {forType, fromType, toType});
		}
        
        public void Dispose()
        {
            if (FMapper != null)
                FMapper.Dispose();
            
            FInstanceMappings.Clear();
        }
    }
}
