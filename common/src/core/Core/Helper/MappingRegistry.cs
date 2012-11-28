using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.Practices.Unity;

namespace VVVV.Core
{

    public enum MapInstantiation 
    { 
        PerClass, 
        PerInstanceAndItsChilds, 
        PerInstance
    };
                                        

    /// <summary>
    /// The MappingRegistry holds all the possible type mappings which are later
    /// used in the ModelMappers to map model objects to some arbritary types.
    /// </summary>
    public class MappingRegistry
    {
        public class Registration
        {
            public Type TypeFrom;
            public Type TypeTo;
            public Func<LifetimeManager> LifetimeManagerCreator;
            
            public Registration(Type typeFrom, Type typeTo, Func<LifetimeManager> managerCreator)
            {
                TypeFrom = typeFrom;
                TypeTo = typeTo;
                LifetimeManagerCreator = managerCreator;
            }
        }
        
        protected Dictionary<Type, Registration> FRegistrations;
        
        public IUnityContainer Container
        {
            get;
            private set;
        }

        internal MappingRegistry(IUnityContainer container)
        {
            Container = container;           
            FRegistrations = new Dictionary<Type, Registration>();
        }

        public MappingRegistry()
            : this( new UnityContainer() )
        {
        }

        public MappingRegistry CreateChildRegistry()
        {
            var childRegistry = new MappingRegistry(Container.CreateChildContainer());
            childRegistry.FRegistrations = FRegistrations;
            return childRegistry;
        }
        
        /// <summary>
        /// Registers default mappings of
        /// T -> T
        /// and
        /// all implemented interfaces of T -> T
        /// </summary>
        public void RegisterDefaultMapping<T>()
        {
            var destinationType = typeof(T);
    
            Container.RegisterType<T>(new HierarchicalLifetimeManager());
    
            foreach (var destinationInterface in destinationType.GetInterfaces())
                Container.RegisterType(destinationInterface, destinationType, new HierarchicalLifetimeManager());
        }
        
        /// <summary>
        /// Registers default mapping of
        /// TFrom -> TTo.
        /// </summary>
        public void RegisterDefaultMapping<TFrom, TTo>() where TTo: TFrom
        {
            RegisterDefaultMapping<TFrom, TTo>(MapInstantiation.PerInstance);
        }
        
        /// <summary>
        /// Registers default mapping of
        /// TFrom -> TTo.
        /// </summary>
        public void RegisterDefaultMapping<TFrom, TTo>(MapInstantiation instantiation) where TTo: TFrom
        {
            RegisterDefaultMapping(typeof(TFrom), typeof(TTo), instantiation);
        }

        public void RegisterDefaultMapping(Type fromType, Type toType, MapInstantiation instantiation)
        {
            var manager = GetManagerFor(instantiation);

            switch (instantiation)
            {
                case MapInstantiation.PerClass:
                case MapInstantiation.PerInstance:
                    Container.RegisterType(fromType, toType, manager());
                    return;

                default:
                    throw new Exception("mapping type not implemented");
            }
        }

        /// <summary>
        /// Registers default mapping of
        /// TInterface -> instance.
        /// </summary>
        /// <param name="instance">The instance implementing TInterface.</param>
        public void RegisterDefaultInstance<TInterface>(TInterface instance, bool holdWeakReference)
        {
            if (holdWeakReference)
                Container.RegisterInstance<TInterface>(instance, new ExternallyControlledLifetimeManager());
            else
                Container.RegisterInstance<TInterface>(instance, new ContainerControlledLifetimeManager());
        }
        
        /// <summary>
        /// Registers default mapping of
        /// TInterface -> instance.
        /// </summary>
        /// <param name="instance">The instance implementing TInterface.</param>
        public void RegisterDefaultInstance<TInterface>(TInterface instance)
        {
            RegisterDefaultInstance<TInterface>(instance, true);
        }
        
        /// <summary>
        /// Registers named mapping of
        /// TInterface -> instance
        /// for N.
        /// </summary>
        public void RegisterInstance<N, TInterface>(TInterface instance, bool holdWeakReference)
        {
            var name = typeof(N).FullName;
            if (holdWeakReference)
                Container.RegisterInstance(name, instance, new ExternallyControlledLifetimeManager());
            else
                Container.RegisterInstance(name, instance, new ContainerControlledLifetimeManager());
        }
        
        /// <summary>
        /// Registers named mapping of
        /// TInterface -> instance
        /// for N.
        /// </summary>
        public void RegisterInstance<N, TInterface>(TInterface instance)
        {
            RegisterInstance<N, TInterface>(instance, true);
        }


        protected Func<LifetimeManager> GetManagerFor(MapInstantiation instantiation)
        {
            switch (instantiation)
            {
                case MapInstantiation.PerClass:
                case MapInstantiation.PerInstanceAndItsChilds:
                    return () => new ContainerControlledLifetimeManager();

                case MapInstantiation.PerInstance:
                    return () => new HierarchicalLifetimeManager();
                    
                default:
                    return null;
            }
        }

        /// <summary>
        /// Registers named mappings of
        /// T -> T
        /// and
        /// all implemented interfaces of T -> T
        /// for N.
        /// </summary>
        public void RegisterMapping<N, T>(MapInstantiation instantiation)
        {
            var manager = GetManagerFor(instantiation);
            var tfor = typeof(N);
            var destinationType = typeof(T);

            switch (instantiation)
            {
                case MapInstantiation.PerClass:
                case MapInstantiation.PerInstance:
                    Container.RegisterType<T>(tfor.FullName, manager());
                    foreach (var destinationInterface in destinationType.GetInterfaces())
                        Container.RegisterType(destinationInterface, destinationType, tfor.FullName, manager());
                    return;

                case MapInstantiation.PerInstanceAndItsChilds:
                    FRegistrations[tfor] = new Registration(destinationType, destinationType, manager);
                    foreach (var destinationInterface in destinationType.GetInterfaces())
                        FRegistrations[tfor] = new Registration(destinationInterface, destinationType, manager);
                    return;

                default:
                    throw new Exception("mapping type not implemented");
            }
        }

        /// <summary>
        /// Registers named mappings of
        /// T -> T
        /// and
        /// all implemented interfaces of T -> T
        /// for N.
        /// </summary>
        public void RegisterMapping<N, T>()
        {
            RegisterMapping<N, T>(MapInstantiation.PerInstance);
        }

        /// <summary>
        /// Registers named mapping of
        /// TFrom -> TTo
        /// for N.
        /// </summary>
        public void RegisterMapping<N, TFrom, TTo>(MapInstantiation instantiation) where TTo: TFrom
        {
            RegisterMapping(typeof(N), typeof(TFrom), typeof(TTo), instantiation);
        }

        /// <summary>
        /// Registers named mapping of
        /// TFrom -> TTo
        /// for N.
        /// </summary>
        public void RegisterMapping<N, TFrom, TTo>() where TTo : TFrom
        {
            RegisterMapping<N, TFrom, TTo>(MapInstantiation.PerInstance);
        }


        /// <summary>
        /// Registers named mapping of
        /// TFrom -> TTo
        /// for N.
        /// </summary>
        public void RegisterMapping(Type forType, Type fromType, Type toType, MapInstantiation instantiation)
        {
            var manager = GetManagerFor(instantiation);

            switch (instantiation)
            {
                case MapInstantiation.PerClass:
                case MapInstantiation.PerInstance:
                    Container.RegisterType(fromType, toType, forType.FullName, manager());
                    return;

                case MapInstantiation.PerInstanceAndItsChilds:
                    FRegistrations[forType] = new Registration(fromType, toType, manager);
                    return;

                default:
                    throw new Exception("mapping type not implemented");
            }
        }
        
        // TODO: See vvvv50 todo. Think this through! ModelMapper is super complicated for such an easy task.
        public T LocateService<T>()
        {
            return Container.Resolve<T>();
        }
        
        public bool HasService<T>()
        {
            return Container.Registrations.Any(registration => registration.RegisteredType == typeof(T));
        }
        
        public void RegisterService<TService, TServiceImplementation>() where TServiceImplementation : TService
        {
            Container.RegisterType<TService, TServiceImplementation>(new ContainerControlledLifetimeManager());
        }


        /// <summary>
        /// Returns all the registrations for type including its basetypes and implemented 
        /// interfaces.
        /// </summary>
        internal IEnumerable<Registration> GetRegistrations(Type type)
        {
            var baseType = type;
            while (baseType != typeof(object))
            {
                if (FRegistrations.ContainsKey(baseType))
                    yield return FRegistrations[baseType];
                baseType = baseType.BaseType;
            }
            
            foreach (var interf in type.GetInterfaces())
            {
                if (FRegistrations.ContainsKey(interf))
                    yield return FRegistrations[interf];
            }
        }
    }
}
