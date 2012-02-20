using System;
using System.Collections.Generic;
using System.Linq;

using VVVV.Hosting.IO.Streams;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.Streams;

namespace VVVV.Hosting.IO
{
    class EnumStreamRegistry : IORegistryBase
    {
        public EnumStreamRegistry()
        {
            RegisterInput(typeof(BufferedIOStream<>), CreateInput);
            RegisterOutput(typeof(IOutStream<>), CreateOutput);
            RegisterConfig(typeof(BufferedIOStream<>), CreateConfig);
        }
        
        private static IIOHandler CreateInput(IIOFactory factory, InputAttribute attribute, Type t)
        {
            var host = factory.PluginHost;
            var enumIn = host.CreateEnumInput(attribute, t);
            var stream = Activator.CreateInstance(typeof(EnumInStream<>).MakeGenericType(t), new object[] { enumIn }) as IInStream;
            // Using ManagedIOStream -> needs to be synced on managed side.
            if (attribute.AutoValidate)
                return IOHandler.Create(stream, enumIn, s => s.Sync());
            else
                return IOHandler.Create(stream, enumIn);
        }
        
        private static IIOHandler CreateOutput(IIOFactory factory, OutputAttribute attribute, Type t)
        {
            var host = factory.PluginHost;
            var enumOut = host.CreateEnumOutput(attribute, t);
            var stream = Activator.CreateInstance(typeof(EnumOutStream<>).MakeGenericType(t), new object[] { enumOut }) as IOutStream;
            return IOHandler.Create(stream, enumOut, null, s => s.Flush());
        }
        
        private static IIOHandler CreateConfig(IIOFactory factory, ConfigAttribute attribute, Type t)
        {
            var host = factory.PluginHost;
            var enumConfig = host.CreateEnumConfig(attribute, t);
            var streamType = typeof(EnumConfigStream<>).MakeGenericType(t);
            var stream = Activator.CreateInstance(streamType, new object[] { enumConfig }) as IIOStream;
            return IOHandler.Create(stream, enumConfig, null, s => s.Flush(), s => s.Sync());
        }
        
        public override bool CanCreate(Type ioType, IOAttribute attribute)
        {
            var ioDataType = ioType.GetGenericArguments().FirstOrDefault();
            if (ioDataType != null)
            {
                var baseType = ioDataType.BaseType;
                if (baseType != null && baseType == typeof(Enum))
                {
                    var openIOType = ioType.GetGenericTypeDefinition();
                    if (attribute is InputAttribute)
                        return FInputDelegates.ContainsKey(openIOType);
                    if (attribute is OutputAttribute)
                        return FOutputDelegates.ContainsKey(openIOType);
                    if (attribute is ConfigAttribute)
                        return FConfigDelegates.ContainsKey(openIOType);
                }
            }
            return false;
        }
    }
}
