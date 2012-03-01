using System;
using System.Collections.Generic;
using System.Linq;
using VVVV.Hosting.IO.Streams;
using VVVV.PluginInterfaces.V1;
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
        
        private static IIOContainer CreateInput(IOBuildContext<InputAttribute> context)
        {
            var factory = context.Factory;
            var container = factory.CreateIOContainer(context.ReplaceIOType(typeof(IEnumIn)));
            var streamType = typeof(EnumInStream<>).MakeGenericType(context.DataType);
            var stream = Activator.CreateInstance(streamType, container.RawIOObject) as IInStream;
            // Using ManagedIOStream -> needs to be synced on managed side.
            if (context.IOAttribute.AutoValidate)
                return IOContainer.Create(context, stream, container, s => s.Sync());
            else
                return IOContainer.Create(context, stream, container);
        }
        
        private static IIOContainer CreateOutput(IOBuildContext<OutputAttribute> context)
        {
            var factory = context.Factory;
            var container = factory.CreateIOContainer(context.ReplaceIOType(typeof(IEnumOut)));
            var stream = Activator.CreateInstance(typeof(EnumOutStream<>).MakeGenericType(t), new object[] { enumOut }) as IOutStream;
            return IOContainer.Create(factory, stream, enumOut, null, s => s.Flush());
        }
        
        private static IIOContainer CreateConfig(IOBuildContext<ConfigAttribute> context)
        {
            var host = factory.PluginHost;
            var enumConfig = host.CreateEnumConfig(attribute, t);
            var streamType = typeof(EnumConfigStream<>).MakeGenericType(t);
            var stream = Activator.CreateInstance(streamType, new object[] { enumConfig }) as IIOStream;
            return IOContainer.Create(factory, stream, enumConfig, null, s => s.Flush(), s => s.Sync());
        }
        
        public override bool CanCreate(IOBuildContext context)
        {
            var ioType = context.IOType;
            var dataType = context.DataType;
            if (dataType != null)
            {
                var baseType = dataType.BaseType;
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
