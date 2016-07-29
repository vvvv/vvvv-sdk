using System;
using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;

namespace VVVV.Hosting.IO
{
    class PluginIORegistry : IORegistryBase
    {
        public PluginIORegistry()
        {
            RegisterInput<IValueFastIn>(
                (factory, context) =>
                {
                    var host = factory.PluginHost;
                    var pluginIO = host.CreateValueFastInput(context.IOAttribute, context.DataType);
                    return new PluginIOContainer(context, factory, pluginIO);
                });
            
            RegisterInput<IValueIn>(
                (factory, context) =>
                {
                    var host = factory.PluginHost;
                    var pluginIO = host.CreateValueInput(context.IOAttribute, context.DataType);
                    return new PluginIOContainer(context, factory, pluginIO);
                });
            
            RegisterInput<ITransformIn>(
                (factory, context) =>
                {
                    var host = factory.PluginHost;
                    var pluginIO = host.CreateTransformInput(context.IOAttribute, context.DataType);
                    return new PluginIOContainer(context, factory, pluginIO);
                });

            RegisterInput<IRawIn>(
                (factory, context) =>
                {
                    var host = factory.PluginHost;
                    var pluginIO = host.CreateRawInput(context.IOAttribute);
                    return new PluginIOContainer(context, factory, pluginIO);
                });
            
            RegisterInput<IColorIn>(
                (factory, context) =>
                {
                    var host = factory.PluginHost;
                    var pluginIO = host.CreateColorInput(context.IOAttribute, context.DataType);
                    return new PluginIOContainer(context, factory, pluginIO);
                });
            
            RegisterInput<IStringIn>(
                (factory, context) =>
                {
                    var host = factory.PluginHost;
                    var pluginIO = host.CreateStringInput(context.IOAttribute, context.DataType);
                    return new PluginIOContainer(context, factory, pluginIO);
                });
            
            RegisterInput<IEnumIn>(
                (factory, context) =>
                {
                    var host = factory.PluginHost;
                    var pluginIO = host.CreateEnumInput(context.IOAttribute, context.DataType);
                    return new PluginIOContainer(context, factory, pluginIO);
                });
            
            RegisterInput<INodeIn>(
                (factory, context) =>
                {
                    var host = factory.PluginHost;
                    var pluginIO = host.CreateNodeInput(context.IOAttribute, context.DataType);
                    return new PluginIOContainer(context, factory, pluginIO);
                });
            
            RegisterInput<IDXRenderStateIn>(
                (factory, context) =>
                {
                    var host = factory.PluginHost;
                    var attribute = context.IOAttribute;
                    IDXRenderStateIn pin;
                    host.CreateRenderStateInput((TSliceMode)attribute.SliceMode, (TPinVisibility)attribute.Visibility, out pin);
                    return new PluginIOContainer(context, factory, pin);
                });
            
            RegisterInput<IDXSamplerStateIn>(
                (factory, context) =>
                {
                    var host = factory.PluginHost;
                    var attribute = context.IOAttribute;
                    IDXSamplerStateIn pin;
                    host.CreateSamplerStateInput((TSliceMode)attribute.SliceMode, (TPinVisibility)attribute.Visibility, out pin);
                    return new PluginIOContainer(context, factory, pin);
                });
            
            RegisterOutput<IValueOut>(
                (factory, context) =>
                {
                    var host = factory.PluginHost;
                    var pluginIO = host.CreateValueOutput(context.IOAttribute, context.DataType);
                    return new PluginIOContainer(context, factory, pluginIO);
                });
            
            RegisterOutput<ITransformOut>(
                (factory, context) =>
                {
                    var host = factory.PluginHost;
                    var pluginIO = host.CreateTransformOutput(context.IOAttribute, context.DataType);
                    return new PluginIOContainer(context, factory, pluginIO);
                });

            RegisterOutput<IRawOut>(
                (factory, context) =>
                {
                    var host = factory.PluginHost;
                    var pluginIO = host.CreateRawOutput(context.IOAttribute);
                    return new PluginIOContainer(context, factory, pluginIO);
                });
            
            RegisterOutput<IColorOut>(
                (factory, context) =>
                {
                    var host = factory.PluginHost;
                    var pluginIO = host.CreateColorOutput(context.IOAttribute, context.DataType);
                    return new PluginIOContainer(context, factory, pluginIO);
                });
            
            RegisterOutput<IStringOut>(
                (factory, context) =>
                {
                    var host = factory.PluginHost;
                    var pluginIO = host.CreateStringOutput(context.IOAttribute, context.DataType);
                    return new PluginIOContainer(context, factory, pluginIO);
                });
            
            RegisterOutput<IEnumOut>(
                (factory, context) =>
                {
                    var host = factory.PluginHost;
                    var pluginIO = host.CreateEnumOutput(context.IOAttribute, context.DataType);
                    return new PluginIOContainer(context, factory, pluginIO);
                });
            
            RegisterOutput<INodeOut>(
                (factory, context) =>
                {
                    var host = factory.PluginHost;
                    var pluginIO = host.CreateNodeOutput(context.IOAttribute, context.DataType);
                    return new PluginIOContainer(context, factory, pluginIO);
                });
            
            RegisterOutput<IDXLayerIO>(
                (factory, context) =>
                {
                    var host = factory.PluginHost;
                    var attribute = context.IOAttribute;
                    IDXLayerIO pin;
                    host.CreateLayerOutput(attribute.Name, (TPinVisibility)attribute.Visibility, out pin);
                    return new PluginIOContainer(context, factory, pin);
                });
            
            RegisterOutput<IDXMeshOut>(
                (factory, context) =>
                {
                    var host = factory.PluginHost;
                    var attribute = context.IOAttribute;
                    IDXMeshOut pin;
                    host.CreateMeshOutput(attribute.Name, (TSliceMode)attribute.SliceMode, (TPinVisibility)attribute.Visibility, out pin);
                    return new PluginIOContainer(context, factory, pin);
                });
            
            RegisterOutput<IDXTextureOut>(
                (factory, context) =>
                {
                    var host = factory.PluginHost;
                    var attribute = context.IOAttribute;
                    IDXTextureOut pin;
                    host.CreateTextureOutput(attribute.Name, (TSliceMode)attribute.SliceMode, (TPinVisibility)attribute.Visibility, out pin);
                    return new PluginIOContainer(context, factory, pin);
                });
            
            RegisterConfig<IValueConfig>(
                (factory, context) =>
                {
                    var host = factory.PluginHost;
                    var pluginIO = host.CreateValueConfig(context.IOAttribute, context.DataType);
                    return new PluginIOContainer(context, factory, pluginIO);
                });
            
            RegisterConfig<IColorConfig>(
                (factory, context) =>
                {
                    var host = factory.PluginHost;
                    var pluginIO = host.CreateColorConfig(context.IOAttribute, context.DataType);
                    return new PluginIOContainer(context, factory, pluginIO);
                });
            
            RegisterConfig<IStringConfig>(
                (factory, context) =>
                {
                    var host = factory.PluginHost;
                    var pluginIO = host.CreateStringConfig(context.IOAttribute, context.DataType);
                    return new PluginIOContainer(context, factory, pluginIO);
                });
            
            RegisterConfig<IEnumConfig>(
                (factory, context) =>
                {
                    var host = factory.PluginHost;
                    var pluginIO = host.CreateEnumConfig(context.IOAttribute, context.DataType);
                    return new PluginIOContainer(context, factory, pluginIO);
                });
        }
    }
}
