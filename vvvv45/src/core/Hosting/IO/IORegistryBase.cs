using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;

using SlimDX;
using SlimDX.Direct3D9;
using VVVV.Hosting.Interfaces;
using VVVV.Hosting.IO.Streams;
using VVVV.Hosting.Pins.Config;
using VVVV.Hosting.Pins.Input;
using VVVV.Hosting.Pins.Output;
using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.PluginInterfaces.V2.EX9;
using VVVV.Utils.Streams;
using VVVV.Utils.VColor;
using VVVV.Utils.VMath;

namespace VVVV.Hosting.IO
{
    [ComVisible(false)]
    class IORegistryBase : IIORegistry
    {
        protected readonly List<IIORegistry> FRegistries = new List<IIORegistry>();
        protected readonly Dictionary<Type, Func<IIOFactory, InputAttribute, Type, IIOHandler>> FInputDelegates = new Dictionary<Type, Func<IIOFactory, InputAttribute, Type, IIOHandler>>();
        protected readonly Dictionary<Type, Func<IIOFactory, OutputAttribute, Type, IIOHandler>> FOutputDelegates = new Dictionary<Type, Func<IIOFactory, OutputAttribute, Type, IIOHandler>>();
        protected readonly Dictionary<Type, Func<IIOFactory, ConfigAttribute, Type, IIOHandler>> FConfigDelegates = new Dictionary<Type, Func<IIOFactory, ConfigAttribute, Type, IIOHandler>>();
        
        public void RegisterInput(Type ioType, Func<IIOFactory, InputAttribute, Type, IIOHandler> createInputFunc, bool registerInterfaces = true)
        {
            FInputDelegates.Add(ioType, createInputFunc);
            if (registerInterfaces)
            {
                foreach (var interf in ioType.GetInterfaces().Where(i => i.IsGenericType))
                {
                    var parameter = interf.GetGenericArguments().First();
                    if (parameter.IsGenericParameter)
                        FInputDelegates.Add(interf.GetGenericTypeDefinition(), createInputFunc);
                    else
                        FInputDelegates.Add(interf, createInputFunc);
                }
            }
        }
        
        public void RegisterInput<TIO>(Func<IIOFactory, InputAttribute, Type, IIOHandler> createInputFunc, bool registerInterfaces = true)
        {
            RegisterInput(typeof(TIO), createInputFunc, registerInterfaces);
        }
        
        public void RegisterOutput(Type ioType, Func<IIOFactory, OutputAttribute, Type, IIOHandler> createOutputFunc, bool registerInterfaces = true)
        {
            FOutputDelegates.Add(ioType, createOutputFunc);
            if (registerInterfaces)
            {
                foreach (var interf in ioType.GetInterfaces().Where(i => i.IsGenericType))
                {
                    var parameter = interf.GetGenericArguments().First();
                    if (parameter.IsGenericParameter)
                        FOutputDelegates.Add(interf.GetGenericTypeDefinition(), createOutputFunc);
                    else
                        FOutputDelegates.Add(interf, createOutputFunc);
                }
            }
        }
        
        public void RegisterOutput<TIO>(Func<IIOFactory, OutputAttribute, Type, IIOHandler> createOutputFunc, bool registerInterfaces = true)
        {
            RegisterOutput(typeof(TIO), createOutputFunc, registerInterfaces);
        }
        
        public void RegisterConfig(Type ioType, Func<IIOFactory, ConfigAttribute, Type, IIOHandler> createConfigFunc, bool registerInterfaces = true)
        {
            FConfigDelegates.Add(ioType, createConfigFunc);
            if (registerInterfaces)
            {
                foreach (var interf in ioType.GetInterfaces().Where(i => i.IsGenericType))
                {
                    var parameter = interf.GetGenericArguments().First();
                    if (parameter.IsGenericParameter)
                        FConfigDelegates.Add(interf.GetGenericTypeDefinition(), createConfigFunc);
                    else
                        FConfigDelegates.Add(interf, createConfigFunc);
                }
            }
        }
        
        public void RegisterConfig<TIO>(Func<IIOFactory, ConfigAttribute, Type, IIOHandler> createConfigFunc, bool registerInterfaces = true)
        {
            RegisterConfig(typeof(TIO), createConfigFunc, registerInterfaces);
        }
        
        public void Register(IIORegistry registry)
        {
            FRegistries.Add(registry);
        }
        
        public virtual bool CanCreate(Type ioType, IOAttribute attribute)
        {
            foreach (var registry in FRegistries)
            {
                if (registry.CanCreate(ioType, attribute))
                {
                    return true;
                }
            }
            
            var openIOType = ioType.GetGenericTypeDefinition();
            var inputAttribute = attribute as InputAttribute;
            if (inputAttribute != null)
            {
                return FInputDelegates.ContainsKey(ioType) || FInputDelegates.ContainsKey(openIOType);
            }
            
            var outputAttribute = attribute as OutputAttribute;
            if (outputAttribute != null)
            {
                return FOutputDelegates.ContainsKey(ioType) || FOutputDelegates.ContainsKey(openIOType);
            }
            
            var configAttribute = attribute as ConfigAttribute;
            if (configAttribute != null)
            {
                return FConfigDelegates.ContainsKey(ioType) || FConfigDelegates.ContainsKey(openIOType);
            }
            
            return false;
        }
        
        public virtual IIOHandler CreateIOHandler(Type ioType, IIOFactory factory, IOAttribute attribute)
        {
            foreach (var registry in FRegistries)
            {
                if (registry.CanCreate(ioType, attribute))
                {
                    return registry.CreateIOHandler(ioType, factory, attribute);
                }
            }
            
            var ioDataType = ioType.GetGenericArguments().FirstOrDefault();
            var openIOType = ioType.IsGenericType ? ioType.GetGenericTypeDefinition() : ioType;
            
            var inputAttribute = attribute as InputAttribute;
            if (inputAttribute != null)
            {
                if (FInputDelegates.ContainsKey(ioType))
                    return FInputDelegates[ioType](factory, inputAttribute, ioDataType);
                else if (FInputDelegates.ContainsKey(openIOType))
                    return FInputDelegates[openIOType](factory, inputAttribute, ioDataType);
            }
            
            var outputAttribute = attribute as OutputAttribute;
            if (outputAttribute != null)
            {
                if (FOutputDelegates.ContainsKey(ioType))
                    return FOutputDelegates[ioType](factory, outputAttribute, ioDataType);
                else if (FOutputDelegates.ContainsKey(openIOType))
                    return FOutputDelegates[openIOType](factory, outputAttribute, ioDataType);
            }
            
            var configAttribute = attribute as ConfigAttribute;
            if (configAttribute != null)
            {
                if (FConfigDelegates.ContainsKey(ioType))
                    return FConfigDelegates[ioType](factory, configAttribute, ioDataType);
                else if (FConfigDelegates.ContainsKey(openIOType))
                    return FConfigDelegates[openIOType](factory, configAttribute, ioDataType);
            }
            
            throw new NotSupportedException(string.Format("Can't create {0} of type '{1}'.", attribute, ioType));
        }
    }
}
