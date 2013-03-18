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
            RegisterInput(typeof(MemoryIOStream<>), CreateInput);
            RegisterOutput(typeof(IOutStream<>), CreateOutput);
            RegisterConfig(typeof(MemoryIOStream<>), CreateConfig);
        }
        
        private static IIOContainer CreateInput(IIOFactory factory, IOBuildContext<InputAttribute> context)
        {
            var container = factory.CreateIOContainer(context.ReplaceIOType(typeof(IEnumIn)));
            var streamType = typeof(EnumInStream<>).MakeGenericType(context.DataType);
            var stream = Activator.CreateInstance(streamType, container.RawIOObject) as IInStream;
            // Using ManagedIOStream -> needs to be synced on managed side.
            if (context.IOAttribute.AutoValidate)
                return IOContainer.Create(context, stream, container, s => s.Sync());
            else
                return IOContainer.Create(context, stream, container);
        }
        
        private static IIOContainer CreateOutput(IIOFactory factory, IOBuildContext<OutputAttribute> context)
        {
            var container = factory.CreateIOContainer(context.ReplaceIOType(typeof(IEnumOut)));
            var streamType = typeof(EnumOutStream<>).MakeGenericType(context.DataType);
            var stream = Activator.CreateInstance(streamType, container.RawIOObject) as IOutStream;
            if (context.IOAttribute.AutoFlush)
                return IOContainer.Create(context, stream, container, null, s => s.Flush());
            else
                return IOContainer.Create(context, stream, container);
        }
        
        private static IIOContainer CreateConfig(IIOFactory factory, IOBuildContext<ConfigAttribute> context)
        {
            var container = factory.CreateIOContainer(context.ReplaceIOType(typeof(IEnumConfig)));
            var streamType = typeof(EnumConfigStream<>).MakeGenericType(context.DataType);
            var stream = Activator.CreateInstance(streamType, container.RawIOObject) as IIOStream;
            return IOContainer.Create(context, stream, container, null, s => s.Flush(), s => s.Sync());
        }
        
        public override bool CanCreate(IOBuildContext context)
        {
            var ioType = context.IOType;
            var dataType = context.DataType;
            var attribute = context.IOAttribute;
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
