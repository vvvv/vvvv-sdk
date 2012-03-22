using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using Microsoft.Practices.Unity;
using VVVV.Core.Service;
using System.Linq;
using System.Diagnostics;

namespace VVVV.Core
{
    internal class TypeMappings
    {
        /// <summary>
        /// The lookup table stores under which name a possible mapping needs to
        /// be resolved in the container.
        /// </summary>
        private Dictionary<Type, string> FLookup;

        public Type SourceType { get; private set; }

        /// <summary>
        /// The IUnityContainer used to do the mapping.
        /// </summary>
        internal IUnityContainer Container { get; private set; }

        private int FRegistering;

        internal string this[Type type]
        {
            get
            {
                return FLookup[type];
            }
        }

        public TypeMappings(IUnityContainer container, Type sourcetype)
        {
            SourceType = sourcetype;
            Container = container;

            // Create our own registration lookup table (IsRegistered method from Unity is terrible slow).
            FLookup = new Dictionary<Type, string>();
        }

        internal void InitLookupTable()
        {
            // Clear the lookup table
            FLookup.Clear();

            // Create a table containing our type hierachy and assign a
            // priority to each entry (lower is better).
            var myTypes = new Dictionary<Type, int>();
            int priority = 0;

            var baseType = SourceType;
            while ((baseType != null) && baseType != typeof(object))
            {
                myTypes[baseType] = priority++;
                if (baseType.IsGenericType)
                {
                    Type genBaseType = baseType.GetGenericTypeDefinition();
                    myTypes[genBaseType] = priority++;
                }
                baseType = baseType.BaseType;
            }

            foreach (var interf in SourceType.GetInterfaces())
                myTypes[interf] = priority++;

            if (SourceType.IsGenericType)
            {
                Type genBaseType = SourceType.GetGenericTypeDefinition();
                foreach (var interf in genBaseType.GetInterfaces())
                    myTypes[interf] = priority++;
            }

            // Now create a table where we store the best type mappings
            var bestMappings = new Dictionary<Type, int>();

            foreach (var registration in Container.Registrations)
            {
                var name = registration.Name;
                var type = registration.RegisteredType;

                // Default registrations are valid for all types
                if (name == null)
                {
                    FLookup[type] = null;
                    // Assign lowest priority to default mapping
                    bestMappings[type] = int.MaxValue;
                }
                else
                {
                    var typeAndPrio = myTypes.Where((t) => t.Key.FullName == name);

                    // See if we can find this name in our type hierachy.
                    if (typeAndPrio.Count() > 0) //..ContainsKey(name))
                    {
                        var prio = typeAndPrio.First().Value;

                        // Initialize this mapping with lowest priority
                        if (!bestMappings.ContainsKey(type))
                            bestMappings[type] = int.MaxValue;

                        // See if this registration is a better match for our type
                        if (bestMappings[type] > prio)
                        {
                            FLookup[type] = name;
                            bestMappings[type] = prio;
                        }
                    }
                }
            }
        }

        internal bool IsRegistered<TDEST>()
        {
            return FLookup.ContainsKey(typeof(TDEST));
        }

        public void RegisterMapping(Type modelType, Type fromType, object toInstance)
        {
            var name = modelType.FullName;
            BeginRegister();
            Container.RegisterInstance(fromType, name, toInstance, new ExternallyControlledLifetimeManager());
            EndRegister();
        }
        
        public void RegisterMapping(Type fromType, object toInstance)
        {
            BeginRegister();
            Container.RegisterInstance(fromType, toInstance, new ExternallyControlledLifetimeManager());
            EndRegister();
        }
        
        public void RegisterMapping(Type modelType, Type fromType, Type toType)
        {
            var name = modelType.FullName;
            BeginRegister();
            Container.RegisterType(fromType, toType, name, new HierarchicalLifetimeManager());
            EndRegister();
        }

        public object Map(Type destType)
        {
            if (IsRegistered(destType))
            {
                var destName = this[destType];
                return Container.Resolve(destType, destName);
            }
            else
                if (destType.IsAssignableFrom(SourceType)) //&& FLookup.ContainsKey(SourceType.FullName))
                    return Container.Resolve(SourceType);
                else
            {
                var msg = string.Format("Can't find mapping to {0}.", destType);
                throw new Exception(msg);
            }
        }

        public TDEST Map<TDEST>(object source)
        {
            if (IsRegistered<TDEST>())
            {
                var destType = typeof(TDEST);
                var destName = this[destType];

                return Container.Resolve<TDEST>(destName);
            }
            else if (source is TDEST)
                return (TDEST)source;
            else
            {
                var msg = string.Format("Can't find mapping from {0} to {1}.", source, typeof(TDEST));
                throw new Exception(msg);
                //                return Container.Resolve<TDEST>();
            }
        }

        public TDEST Map<TDEST>()
        {
            return (TDEST) Map(typeof(TDEST));
        }

        public bool CanMap(Type destType)
        {
            return (IsRegistered(destType) || destType.IsAssignableFrom(SourceType));
        }

        public bool CanMap<TDEST>(object source)
        {
            return (IsRegistered<TDEST>() || (source is TDEST));
        }

        public bool CanMap<TDEST>()
        {
            return (IsRegistered<TDEST>());
        }

        internal void BeginRegister()
        {
            FRegistering++;
        }

        internal void EndRegister()
        {
            FRegistering--;
            if (FRegistering == 0)
                InitLookupTable();
        }

        public void Dispose()
        {
            FLookup.Clear();

            // Delegate the call to the unity container.
            Container.Dispose();
        }

        private bool IsRegistered(Type destType)
        {
            return FLookup.ContainsKey(destType);
        }
        
        public IEnumerable<Type> GetRegisteredModelTypes()
        {
            var cache = new Dictionary<string, Type>();
            
            foreach (var registration in Container.Registrations)
            {
                var name = registration.Name;
                if (name != null && !cache.ContainsKey(name))
                {
                    Type t = null;
                    foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
                    {
                        t = assembly.GetType(name);
                        if (t != null)
                            break;
                    }
                    
                    if (t != null)
                        cache[name] = t;
                }
            }
            
            return cache.Values;
        }
    }

    /// <summary>
    /// A ModelMapper provides mappings for a model element.
    /// </summary>
    public class ModelMapper : IDisposable
    {
        /// <summary>
        /// The MappingRegistry containing all the mapping info.
        /// </summary>
        protected MappingRegistry Registry { get; private set; }
        
        /// <summary>
        /// The model element we provide mappings for.
        /// </summary>
        public object Model { get; private set; }

        private TypeMappings TypeMappings;
        private bool FDisposed;
        
        protected ModelMapper(object model, IUnityContainer parentContainer, MappingRegistry registry)
        {
            Model = model;
            Registry = registry;
            
            // Create a child container out of parent container so newly mapped objects live
            // in its own scope for this model element.
            var container = parentContainer.CreateChildContainer();

            TypeMappings = new TypeMappings(container, model.GetType());
            
            // Register this ModelMapper once, so that child mappers can access their parent mapper at their construction
            container.RegisterInstance<ModelMapper>(this, new ExternallyControlledLifetimeManager());
            // container.RegisterInstance<IModelMapper>(this, new ExternallyControlledLifetimeManager());
            
            // Register model, so that implementors can fetch it on construction
            container.RegisterInstance(model, new ExternallyControlledLifetimeManager());
            container.RegisterInstance(model.GetType(), model, new ExternallyControlledLifetimeManager());
            if (model is IIDItem)
                container.RegisterInstance((IIDItem) model, new ExternallyControlledLifetimeManager());
            
            // Register the model in the container under the name registrations were made for it.
            // For example if a mapping from A to B was provided for T,
            // we wanna be able to fetch T in the constructor of B as T.
            foreach (var t in TypeMappings.GetRegisteredModelTypes())
            {
                // model is t
                if (t.IsAssignableFrom(model.GetType()))
                    container.RegisterInstance(t, model, new ExternallyControlledLifetimeManager());
            }
            
            // See if the registry has special registrations for us.
            foreach (var registration in registry.GetRegistrations(model.GetType()))
            {
                container.RegisterType(registration.TypeFrom, registration.TypeTo, registration.LifetimeManagerCreator());
                // Resolve it once to make sure the class is created in the context of this model element.
                //container.Resolve(registration.TypeFrom);
            }

            TypeMappings.InitLookupTable();
        }
        
        public ModelMapper(object model, ModelMapper parentMapper)
            
            : this(model, parentMapper.TypeMappings.Container, parentMapper.Registry)
        {}

        public ModelMapper(object model, MappingRegistry registry)
            
            : this(model, registry.Container, registry)
        {}

        /// <summary>
        /// Registers mapping from Type fromType to instance toInstance.
        /// The mapping is only valid for this model class and subclasses.
        /// </summary>
        public void RegisterMapping(Type fromType, object toInstance)
        {
            TypeMappings.RegisterMapping(Model.GetType(), fromType, toInstance);
        }
        
        /// <summary>
        /// Registers mapping from Type fromType to instance toInstance.
        /// The mapping is valid for this model object and child objects.
        /// </summary>
        public void RegisterDefault(Type fromType, object toInstance)
        {
            TypeMappings.RegisterMapping(fromType, toInstance);
        }
        
        /// <summary>
        /// Registers mapping for Type forType from Type fromType to Type toType.
        /// The mapping is only valid for model objects of type forType.
        /// </summary>
        public void RegisterMapping(Type forType, Type fromType, Type toType)
        {
            TypeMappings.RegisterMapping(forType, fromType, toType);
        }
        
        /// <summary>
        /// Maps the containing model element to TDEST.
        /// </summary>
        public TDEST Map<TDEST>()
        {
            return TypeMappings.Map<TDEST>(Model);
        }

        /// <summary>
        /// Whether or not the containing model element can be mapped to TDEST.
        /// </summary>
        public bool CanMap<TDEST>()
        {
            return TypeMappings.CanMap<TDEST>(Model);
        }
        
        /// <summary>
        /// Creates a new mapper for the specified model element which inherits
        /// all mappings from this mapper.
        /// </summary>
        /// <param name="model">The model element to create a new mapper for.</param>
        /// <returns>A new model mapper which wraps itself around the specified model element.</returns>
        public ModelMapper CreateChildMapper(object model)
        {
            Debug.Assert(!FDisposed, "Tried to create model mapper on already disposed object graph!", string.Format("Model: {0}", Model.ToString()));
            return new ModelMapper(model, this);
        }
        
        protected bool IsRegistered<TDEST>()
        {
            return TypeMappings.IsRegistered<TDEST>();
        }
        
        public void Dispose()
        {
            TypeMappings.Dispose();
            FDisposed = true;
        }
    }

    public static class ModelMapperExtensions
    {
        /// <summary>
        /// Registers mapping from TInterface to instance.
        /// The mapping is only valid for this model class and subclasses.
        /// </summary>
        public static void RegisterMapping<TInterface>(this ModelMapper mapper, TInterface instance)
        {
            mapper.RegisterMapping(typeof(TInterface), instance);
        }
        
        /// <summary>
        /// Registers mapping from TInterface to instance.
        /// The mapping is valid for this model object and child objects.
        /// </summary>
        public static void RegisterDefault<TInterface>(this ModelMapper mapper, TInterface instance)
        {
            mapper.RegisterDefault(typeof(TInterface), instance);
        }
        
        /// <summary>
        /// Registers mapping for TFor from TFrom to TTo.
        /// The mapping is only valid for TFor.
        /// </summary>
        public static void RegisterMapping<TFor, TFrom, TTo>(this ModelMapper mapper)
        {
            mapper.RegisterMapping(typeof(TFor), typeof(TFrom), typeof(TTo));
        }
    }

    public class Mapper
    {
        /// <summary>
        /// The MappingRegistry containing all the mapping info.
        /// </summary>
        protected MappingRegistry Registry { get; private set; }
        
        private Dictionary<Type, TypeMappings> FTypeMappings;

        internal TypeMappings this[Type type]
        {
            get
            {
                if (!FTypeMappings.ContainsKey(type))
                {
                    var tm = new TypeMappings(Registry.Container.CreateChildContainer(), type);
                    FTypeMappings.Add(type, tm);
                    tm.Container.RegisterDefaultMappings(type);

                    foreach (var t in tm.GetRegisteredModelTypes())
                    {
                        Tuple<Type, Type>[] typeParams;
                        if /*((t != type) &&*/ (type.CanBeMadeOf(t, out typeParams))//)
                            tm.Container.RegisterClosedTypes(t, type, true);
                    }

                    tm.InitLookupTable();
                }

                return FTypeMappings[type];
            }
        }
        
        protected Mapper(IUnityContainer parentContainer, MappingRegistry registry)
        {
            Registry = registry;
            
            // Create a child container out of parent container so newly mapped objects live
            // in its own scope for this model element.
            //var container = parentContainer.CreateChildContainer();

            FTypeMappings = new Dictionary<Type, TypeMappings>();
        }
        
        public Mapper(MappingRegistry registry)
            
            : this(registry.Container, registry)
        {}

        public TDEST Map<TDEST>(object source)
        {
            return this[source.GetType()].Map<TDEST>(source);
        }

        public bool CanMap<TDEST>(object source)
        {
            return this[source.GetType()].CanMap<TDEST>(source);
        }
        
        public object Map(Type sourceType, Type destType)
        {
            return this[sourceType].Map(destType);
        }

        public bool CanMap(Type sourceType, Type destType)
        {
            return this[sourceType].CanMap(destType);
        }

        public TDEST Map<TDEST>(Type sourceType)
        {
            return this[sourceType].Map<TDEST>();
        }

        public bool CanMap<TDEST>(Type sourceType)
        {
            return this[sourceType].CanMap<TDEST>();
        }

        public TDEST Map<TSRC, TDEST>()
        {
            return this[typeof(TSRC)].Map<TDEST>();
        }

        public bool CanMap<TSRC, TDEST>()
        {
            return this[typeof(TSRC)].CanMap<TDEST>();
        }

        public void Dispose()
        {
            foreach (var tm in FTypeMappings.Values)
                tm.Dispose();

            FTypeMappings.Clear();
        }
        
    }
}
