using System;
using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;

namespace VVVV.Hosting
{
	public class DynamicPluginWrapperV1 : IPlugin
	{
		IPlugin FWrappedPlugin;
		IPluginHost FPluginHost;
		
		public IPlugin WrappedPlugin
		{
			get
			{
				return FWrappedPlugin;
			}
		}
		
		public DynamicPluginWrapperV1(IPlugin wrappedPlugin)
		{
			FWrappedPlugin = wrappedPlugin;
		}
		
		public bool AutoEvaluate 
		{
			get 
			{
				return FWrappedPlugin.AutoEvaluate;
			}
		}
		
		public void SetPluginHost(IPluginHost Host)
		{
			FPluginHost = Host;
			try
			{
				FWrappedPlugin.SetPluginHost(Host);
			}
			catch (Exception e)
			{
				FPluginHost.Log(TLogType.Error, string.Format("{0}\n{1}", e.Message, e.StackTrace));
				throw e;
			}
		}
		
		public void Configurate(IPluginConfig input)
		{
			try
			{
				FWrappedPlugin.Configurate(input);
			}
			catch (Exception e)
			{
				FPluginHost.Log(TLogType.Error, string.Format("{0}\n{1}", e.Message, e.StackTrace));
				throw e;
			}
		}
		
		public void Evaluate(int SpreadMax)
		{
			try
			{
				FWrappedPlugin.Evaluate(SpreadMax);
			}
			catch (Exception e)
			{
				FPluginHost.Log(TLogType.Error, string.Format("{0}\n{1}", e.Message, e.StackTrace));
				throw e;
			}
		}
	}
	
	public class DynamicPluginWrapperV2 : IPluginEvaluate
	{
		IPluginEvaluate FWrappedPlugin;
		IPluginHost FPluginHost;
		
		public IPluginEvaluate WrappedPlugin
		{
			get
			{
				return FWrappedPlugin;
			}
		}
		
		public DynamicPluginWrapperV2(IPluginEvaluate wrappedPlugin, IPluginHost pluginHost)
		{
			FWrappedPlugin = wrappedPlugin;
			FPluginHost = pluginHost;
		}
		
		public void Evaluate(int SpreadMax)
		{
			try
			{
				FWrappedPlugin.Evaluate(SpreadMax);
			}
			catch (Exception e)
			{
				FPluginHost.Log(TLogType.Error, string.Format("{0}\n{1}", e.Message, e.StackTrace));
				throw e;
			}
		}
	}
}
