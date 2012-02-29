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
                (factory, attribute, type) =>
                {
                    var host = factory.PluginHost;
                    var pluginIO = host.CreateValueFastInput(attribute, type);
                    return new PluginIOContainer(host, pluginIO);
                });
            
            RegisterInput<IValueIn>(
                (factory, attribute, type) =>
                {
                    var host = factory.PluginHost;
                    var pluginIO = host.CreateValueInput(attribute, type);
                    return new PluginIOContainer(host, pluginIO);
                });
            
            RegisterInput<ITransformIn>(
                (factory, attribute, type) =>
                {
                    var host = factory.PluginHost;
                    var pluginIO = host.CreateTransformInput(attribute, type);
                    return new PluginIOContainer(host, pluginIO);
                });
            
            RegisterInput<IColorIn>(
                (factory, attribute, type) =>
                {
                    var host = factory.PluginHost;
                    var pluginIO = host.CreateColorInput(attribute, type);
                    return new PluginIOContainer(host, pluginIO);
                });
            
            RegisterInput<IStringIn>(
                (factory, attribute, type) =>
                {
                    var host = factory.PluginHost;
                    var pluginIO = host.CreateStringInput(attribute, type);
                    return new PluginIOContainer(host, pluginIO);
                });
            
            RegisterInput<IEnumIn>(
                (factory, attribute, type) =>
                {
                    var host = factory.PluginHost;
                    var pluginIO = host.CreateEnumInput(attribute, type);
                    return new PluginIOContainer(host, pluginIO);
                });
            
            RegisterInput<INodeIn>(
                (factory, attribute, type) =>
                {
                    var host = factory.PluginHost;
                    var pluginIO = host.CreateNodeInput(attribute, type);
                    return new PluginIOContainer(host, pluginIO);
                });
            
            RegisterOutput<IValueOut>(
                (factory, attribute, type) =>
                {
                    var host = factory.PluginHost;
                    var pluginIO = host.CreateValueOutput(attribute, type);
                    return new PluginIOContainer(host, pluginIO);
                });
            
            RegisterOutput<ITransformOut>(
                (factory, attribute, type) =>
                {
                    var host = factory.PluginHost;
                    var pluginIO = host.CreateTransformOutput(attribute, type);
                    return new PluginIOContainer(host, pluginIO);
                });
            
            RegisterOutput<IColorOut>(
                (factory, attribute, type) =>
                {
                    var host = factory.PluginHost;
                    var pluginIO = host.CreateColorOutput(attribute, type);
                    return new PluginIOContainer(host, pluginIO);
                });
            
            RegisterOutput<IStringOut>(
                (factory, attribute, type) =>
                {
                    var host = factory.PluginHost;
                    var pluginIO = host.CreateStringOutput(attribute, type);
                    return new PluginIOContainer(host, pluginIO);
                });
            
            RegisterOutput<IEnumOut>(
                (factory, attribute, type) =>
                {
                    var host = factory.PluginHost;
                    var pluginIO = host.CreateEnumOutput(attribute, type);
                    return new PluginIOContainer(host, pluginIO);
                });
            
            RegisterOutput<INodeOut>(
                (factory, attribute, type) =>
                {
                    var host = factory.PluginHost;
                    var pluginIO = host.CreateNodeOutput(attribute, type);
                    return new PluginIOContainer(host, pluginIO);
                });
            
            RegisterConfig<IValueConfig>(
                (factory, attribute, type) =>
                {
                    var host = factory.PluginHost;
                    var pluginIO = host.CreateValueConfig(attribute, type);
                    return new PluginIOContainer(host, pluginIO);
                });
            
            RegisterConfig<IColorConfig>(
                (factory, attribute, type) =>
                {
                    var host = factory.PluginHost;
                    var pluginIO = host.CreateColorConfig(attribute, type);
                    return new PluginIOContainer(host, pluginIO);
                });
            
            RegisterConfig<IStringConfig>(
                (factory, attribute, type) =>
                {
                    var host = factory.PluginHost;
                    var pluginIO = host.CreateStringConfig(attribute, type);
                    return new PluginIOContainer(host, pluginIO);
                });
            
            RegisterConfig<IEnumConfig>(
                (factory, attribute, type) =>
                {
                    var host = factory.PluginHost;
                    var pluginIO = host.CreateEnumConfig(attribute, type);
                    return new PluginIOContainer(host, pluginIO);
                });
        }
    }
}
