using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VVVV.Core.Service
{
    public interface IServiceRegistry : IMapper
    {
        ISourceTypeMapper this[Type sourceType] {get;}    

        ISourceMapper CreateSourceMapper(object source);
    }

    public static class ServiceRegsitryExtensions
    {
        public static void Register<TSource, TService, TProvider>(this IServiceRegistry registry, bool showUpInChilds) where TProvider : TService
        {
            registry[typeof(TSource)].Register<TService, TProvider>(showUpInChilds);
        }

        public static void Register(this IServiceRegistry registry, Type sourceType, Type serviceType, Type providerType, bool showUpInChilds) 
        {
            registry[sourceType].Register(serviceType, providerType, showUpInChilds);
        }
    }
}
