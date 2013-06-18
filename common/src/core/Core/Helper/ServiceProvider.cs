using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VVVV.Core
{
    public class ServiceProvider : IServiceProvider
    {
        private readonly Dictionary<Type, object> FServices = new Dictionary<Type, object>();
        private IServiceProvider FParentServiceProvider;

        public ServiceProvider()
        {

        }

        public ServiceProvider(IServiceProvider parentServiceProvider)
        {
            FParentServiceProvider = parentServiceProvider;
        }

        public object GetService(Type serviceType)
        {
            object service;
            if (FServices.TryGetValue(serviceType, out service))
                return service;
            if (FParentServiceProvider != null)
                return FParentServiceProvider.GetService(serviceType);
            return null;
        }

        public void RegisterService(Type serviceType, object service)
        {
            FServices[serviceType] = service;
        }

        public void RegisterService<TService>(object service)
        {
            RegisterService(typeof(TService), service);
        }

        public void RegisterService<TService>(TService service)
        {
            RegisterService(typeof(TService), service);
        }
    }
}
