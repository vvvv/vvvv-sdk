using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Practices.Unity;

namespace VVVV.Core.Service
{
    public static class UnityContainerExtensions
    {
        public static LifetimeManager Clone(this LifetimeManager reg)
        {
            if (reg is TransientLifetimeManager)
                return new TransientLifetimeManager();
            if (reg is ContainerControlledLifetimeManager)
                return new ContainerControlledLifetimeManager();
            if (reg is HierarchicalLifetimeManager)
                return new HierarchicalLifetimeManager();
            if (reg is PerResolveLifetimeManager)
                return new PerResolveLifetimeManager();
            if (reg is PerThreadLifetimeManager)
                return new PerThreadLifetimeManager();
            if (reg is ExternallyControlledLifetimeManager)
                return new ExternallyControlledLifetimeManager();
            else
                return null;
        }

        public static void RegisterClosedTypes(this IUnityContainer container, Type cc, Type concrete, bool defaultmapping)
        {
            foreach (var r in container.Registrations)
            {
                if ((r.RegisteredType.FullName == null) || (r.MappedToType.FullName == null))
                {
                    Type newRegType = r.RegisteredType.CloseBySubstitution(cc, concrete);

                    if (newRegType != r.RegisteredType)
                    {
                        Tuple<Type, Type>[] typeParams;
                        if (concrete.CanBeMadeOf(cc, out typeParams))
                        {
                            Type newMapToType = r.MappedToType.CloseBySubstitution(typeParams);
                            //r.MappedToType.CloseByParameterization(typeParams);

                            container.RegisterType(
                                newRegType,
                                newMapToType,
                                defaultmapping ? null : r.Name,
                                r.LifetimeManager.Clone()
                               );
                        }
                    }
                    else
                    {
                        Type newMapToType = r.MappedToType.CloseBySubstitution(cc, concrete);

                        if (newMapToType != r.MappedToType)

                            container.RegisterType(
                                newRegType,
                                newMapToType,
                                defaultmapping ? null : r.Name,
                                r.LifetimeManager.Clone()
                               );
                    }
                }
            }
        }


        public static void RegisterDefaultMappings(this IUnityContainer container, Type forType)
        {
            foreach (var r in container.Registrations)
            {
                if ((forType.FullName == r.Name) && (r.Name != null))
                    container.RegisterType(
                        r.RegisteredType,
                        r.MappedToType,
                        null,
                        r.LifetimeManager.Clone()
                       );
            }
        }
    }
}
