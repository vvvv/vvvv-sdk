using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VVVV.Core.Service
{
    public interface IMapper : IDisposable //, IEnumerable<IMapper>
    {
        /// <summary>
        /// registers a service provider
        /// </summary>
        /// <param name="serviceType">usually a service interface</param>
        /// <param name="providerType">a type implementing the service interface</param>
        /// <param name="showUpInChilds">if true hands over the service provider instance to child mappers, if they fail to retrieve one by their own</param>
        void Register(Type serviceType, Type providerType, bool showUpInChilds);

        /// <summary>
        /// registers a service provider instance
        /// </summary>
        /// <param name="serviceType">usually a service interface</param>
        /// <param name="provider">the instance implementing the service interface</param>
        /// <param name="showUpInChilds">if true hands over the service provider instance to child mappers, if they fail to retrieve one by their own</param>
        /// <param name="controlLifetime">if true dispose will be called on the instance when the container gets disposed</param>
        void RegisterInstance(Type serviceType, object provider, bool showUpInChilds, bool controlLifetime);

        bool CanMap(Type serviceType);  

        object Map(Type serviceType);   
    }

    public static class MapperExtensions
    {
        public static void Register(this IMapper mapper, Type serviceType, Type providerType)
        {
            mapper.Register(serviceType, providerType, false);
        }

        public static void Register<TService, TProvider>(this IMapper mapper, bool showUpInChilds) where TProvider : TService
        {
            mapper.Register(typeof(TService), typeof(TProvider), showUpInChilds);
        }

        public static void Register<TService, TProvider>(this IMapper mapper) where TProvider : TService
        {
            mapper.Register(typeof(TService), typeof(TProvider), false);
        }


        
        public static void RegisterInstance(this IMapper mapper, object provider, Type serviceType)
        {
            mapper.RegisterInstance(serviceType, provider, false, false);
        }

        public static void RegisterInstance<TService>(this IMapper mapper, TService provider, bool showUpInChilds, bool controlLifetime)
        {
            mapper.RegisterInstance(typeof(TService), provider, showUpInChilds, controlLifetime);
        }

        public static void RegisterInstance<TService>(this IMapper mapper, TService provider)
        {
            mapper.RegisterInstance(typeof(TService), provider, false, false);
        }



        public static bool CanMap<TService>(this IMapper mapper)
        {
            return mapper.CanMap(typeof(TService));
        }

        public static TService Map<TService>(this IMapper mapper)
        {
            return (TService)mapper.Map(typeof(TService));
        }

    }
}
