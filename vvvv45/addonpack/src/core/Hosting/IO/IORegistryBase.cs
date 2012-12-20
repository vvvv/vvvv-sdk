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
    public class IORegistryBase : IIORegistry
    {
        protected readonly List<IIORegistry> FRegistries = new List<IIORegistry>();
        protected readonly Dictionary<Type, Func<IIOFactory, IOBuildContext<InputAttribute>, IIOContainer>> FInputDelegates = new Dictionary<Type, Func<IIOFactory, IOBuildContext<InputAttribute>, IIOContainer>>();
        protected readonly Dictionary<Type, Func<IIOFactory, IOBuildContext<OutputAttribute>, IIOContainer>> FOutputDelegates = new Dictionary<Type, Func<IIOFactory, IOBuildContext<OutputAttribute>, IIOContainer>>();
        protected readonly Dictionary<Type, Func<IIOFactory, IOBuildContext<ConfigAttribute>, IIOContainer>> FConfigDelegates = new Dictionary<Type, Func<IIOFactory, IOBuildContext<ConfigAttribute>, IIOContainer>>();
        
        public void RegisterInput(Type ioType, Func<IIOFactory, IOBuildContext<InputAttribute>, IIOContainer> createInputFunc, bool registerInterfaces = true)
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
        
        public void RegisterInput<TIO>(Func<IIOFactory, IOBuildContext<InputAttribute>, IIOContainer> createInputFunc, bool registerInterfaces = true)
        {
            RegisterInput(typeof(TIO), createInputFunc, registerInterfaces);
        }
        
        public void RegisterOutput(Type ioType, Func<IIOFactory, IOBuildContext<OutputAttribute>, IIOContainer> createOutputFunc, bool registerInterfaces = true)
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
        
        public void RegisterOutput<TIO>(Func<IIOFactory, IOBuildContext<OutputAttribute>, IIOContainer> createOutputFunc, bool registerInterfaces = true)
        {
            RegisterOutput(typeof(TIO), createOutputFunc, registerInterfaces);
        }
        
        public void RegisterConfig(Type ioType, Func<IIOFactory, IOBuildContext<ConfigAttribute>, IIOContainer> createConfigFunc, bool registerInterfaces = true)
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
        
        public void RegisterConfig<TIO>(Func<IIOFactory, IOBuildContext<ConfigAttribute>, IIOContainer> createConfigFunc, bool registerInterfaces = true)
        {
            RegisterConfig(typeof(TIO), createConfigFunc, registerInterfaces);
        }
        
        public void Register(IIORegistry registry, bool first = false)
        {
            if (first)
            {
                FRegistries.Insert(0, registry);
            }
            else
            {
                FRegistries.Add(registry);
            }
        }

        
        public virtual bool CanCreate(IOBuildContext context)
        {
            foreach (var registry in FRegistries)
            {
                if (registry.CanCreate(context))
                {
                    return true;
                }
            }
            var ioType = context.IOType;
            var openIOType = ioType.IsGenericType ? ioType.GetGenericTypeDefinition() : ioType;
            switch (context.Direction) {
                case PinDirection.Input:
                    return FInputDelegates.ContainsKey(ioType) || FInputDelegates.ContainsKey(openIOType);
                case PinDirection.Output:
                    return FOutputDelegates.ContainsKey(ioType) || FOutputDelegates.ContainsKey(openIOType);
                case PinDirection.Configuration:
                    return FConfigDelegates.ContainsKey(ioType) || FConfigDelegates.ContainsKey(openIOType);
                default:
                    return false;
            }
        }
        
        public virtual IIOContainer CreateIOContainer(IIOFactory factory, IOBuildContext context)
        {
            foreach (var registry in FRegistries)
            {
                if (registry.CanCreate(context))
                {
                    return registry.CreateIOContainer(factory, context);
                }
            }
            
            var ioType = context.IOType;
            var dataType = context.DataType;
            var openIOType = ioType.IsGenericType ? ioType.GetGenericTypeDefinition() : ioType;
            
            switch (context.Direction) 
            {
                case PinDirection.Input:
                    if (FInputDelegates.ContainsKey(ioType))
                        return FInputDelegates[ioType](factory, context as IOBuildContext<InputAttribute>);
                    else if (FInputDelegates.ContainsKey(openIOType))
                        return FInputDelegates[openIOType](factory, context as IOBuildContext<InputAttribute>);
                    break;
                case PinDirection.Output:
                    if (FOutputDelegates.ContainsKey(ioType))
                        return FOutputDelegates[ioType](factory, context as IOBuildContext<OutputAttribute>);
                    else if (FOutputDelegates.ContainsKey(openIOType))
                        return FOutputDelegates[openIOType](factory, context as IOBuildContext<OutputAttribute>);
                    break;
                case PinDirection.Configuration:
                    if (FConfigDelegates.ContainsKey(ioType))
                        return FConfigDelegates[ioType](factory, context as IOBuildContext<ConfigAttribute>);
                    else if (FConfigDelegates.ContainsKey(openIOType))
                        return FConfigDelegates[openIOType](factory, context as IOBuildContext<ConfigAttribute>);
                    break;
            }
            
            throw new NotSupportedException(string.Format("Can't create {0} of type '{1}'.", context, ioType));
        }
    }
}
