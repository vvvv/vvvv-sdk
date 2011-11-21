using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Practices.Unity;

namespace VVVV.Core.Service
{
    internal struct ServiceRegistration
    {
        public Type ServiceType { get; private set; }
        public Type ProviderType { get; private set; }
        public bool ShowUpInChildMappers { get; private set; }
        public object Provider { get; private set; }
        public bool ControlLifetime { get; private set; }

        public static ServiceRegistration newProvider(Type serviceType, object provider, bool showUpInChilds, bool controlLifetime)
        {
            var r = new ServiceRegistration();

            r.ServiceType = serviceType;
            r.ProviderType = provider.GetType();
            r.ShowUpInChildMappers = showUpInChilds;
            r.Provider = provider;
            r.ControlLifetime = controlLifetime;

            return r;
        }

        public static ServiceRegistration newProviderType(Type serviceType, Type providerType, bool showUpInChilds)
        {
            var r = new ServiceRegistration();

            r.ServiceType = serviceType;
            r.ProviderType = providerType;
            r.ShowUpInChildMappers = showUpInChilds;
            r.Provider = null;
            r.ControlLifetime = true;

            return r;
        }
    }

    class Mapper : IMapper
    {
        internal Dictionary<Type, ServiceRegistration> Services;
        internal Dictionary<Type, ServiceRegistration> NewServices;

        public Mapper Parent { get; internal set; }

        public Mapper()
        {
            Services = new Dictionary<Type, ServiceRegistration>();
            NewServices = new Dictionary<Type, ServiceRegistration>();
        }

        internal bool Dirty { get { return NewServices.Count > 0; } }

        internal IUnityContainer Container { get; private set; }

        #region IMapper Members

        public void Register(Type serviceType, ServiceRegistration service)
        {
            // if an instance is registered already we won't throw it away
            if (Services.ContainsKey(serviceType))
            {
                if (Services[serviceType].Provider != null)
                    return;
                else
                    LetGo(Services[serviceType]);
            }

            // latest registration counts
            Services[serviceType] = service;

            // just to know which were added lately (will be cleaned just bevor mapping)
            NewServices[serviceType] = service;
        }

        public void Register(Type serviceType, Type providerType, bool showUpInChilds)
        {
            Register(serviceType, ServiceRegistration.newProviderType(serviceType, providerType, showUpInChilds));
        }

        public void RegisterInstance(Type serviceType, object provider, bool showUpInChilds, bool controlLifetime)
        {
            Register(serviceType, ServiceRegistration.newProvider(serviceType, provider, showUpInChilds, controlLifetime));
        }

        public virtual bool CanMap(Type serviceType)
        {
            Cleanup();
            return Container.IsRegistered(serviceType);
        }

        public virtual object Map(Type serviceType)
        {
            Cleanup();
            return Container.Resolve(serviceType);
        }

        protected Func<LifetimeManager> GetManagerFor(ServiceRegistration service)
        {
            if (service.ControlLifetime)
                if (service.ShowUpInChildMappers)
                    return () => new ContainerControlledLifetimeManager();
                else
                    return () => new HierarchicalLifetimeManager();
            else
                return () => new ExternallyControlledLifetimeManager();
        }

        protected void AddServiceToContainer(Type serviceType, ServiceRegistration service)
        {
            if (service.Provider == null)
                Container.RegisterType(serviceType, service.ProviderType, GetManagerFor(service)()); 
            else
                Container.RegisterInstance(serviceType, service.Provider, GetManagerFor(service)()); 
        }

        protected virtual void Cleanup()
        {
            if (Parent != null)
                Parent.Cleanup();

            if (Container == null)
                if (Parent == null)
                    Container = new UnityContainer();
                else
                    Container = Parent.Container.CreateChildContainer();

            if (Dirty)
            {
                // adding new services to the container 
                foreach (var s in NewServices.Values)
                    AddServiceToContainer(s.ServiceType, s);

                // make sure that provider instances for childs exist
                foreach (var s in NewServices.Values)
                    if (s.ShowUpInChildMappers)
                        Container.Resolve(s.ServiceType);

                NewServices.Clear();
            }
        }

        #endregion

        #region IDisposable Members

        protected void LetGo(ServiceRegistration service)
        {
            if (service.ControlLifetime && (service.Provider != null) && (service.Provider is IDisposable))
                ((IDisposable)service.Provider).Dispose();
        }

        public void Dispose()
        {
            foreach (var s in NewServices.Values)
                LetGo(s);
            NewServices.Clear();

            foreach (var s in Services.Values)
                LetGo(s);
            Services.Clear();
        }

        #endregion
    }
}
