using System;
using VVVV.Core;
using VVVV.Core.Runtime;
using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;

namespace VVVV.Hosting
{
	public class DynamicPluginWrapperV1 : IPlugin
	{
		IPlugin FWrappedPlugin;
		IPluginHost FPluginHost;
		IExecutable FExecutable;
		
		public IPlugin WrappedPlugin
		{
			get
			{
				return FWrappedPlugin;
			}
		}
		
		public DynamicPluginWrapperV1(IPlugin wrappedPlugin, IExecutable executable)
		{
			FWrappedPlugin = wrappedPlugin;
			FExecutable = executable;
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
				FExecutable.RuntimeErrors.Clear();
			}
			catch (Exception e)
			{
				FPluginHost.Log(TLogType.Error, string.Format("{0}\n{1}", e.Message, e.StackTrace));
				if (FExecutable.RuntimeErrors.Count == 0)
					FExecutable.RuntimeErrors.Add(new RuntimeError(e));
				throw e;
			}
		}
		
		public void Configurate(IPluginConfig input)
		{
			try
			{
				FWrappedPlugin.Configurate(input);
				FExecutable.RuntimeErrors.Clear();
			}
			catch (Exception e)
			{
				FPluginHost.Log(TLogType.Error, string.Format("{0}\n{1}", e.Message, e.StackTrace));
				if (FExecutable.RuntimeErrors.Count == 0)
					FExecutable.RuntimeErrors.Add(new RuntimeError(e));
				throw e;
			}
		}
		
		public void Evaluate(int SpreadMax)
		{
			try
			{
				FWrappedPlugin.Evaluate(SpreadMax);
				FExecutable.RuntimeErrors.Clear();
			}
			catch (Exception e)
			{
				FPluginHost.Log(TLogType.Error, string.Format("{0}\n{1}", e.Message, e.StackTrace));
				if (FExecutable.RuntimeErrors.Count == 0)
					FExecutable.RuntimeErrors.Add(new RuntimeError(e));
				throw e;
			}
		}
	}
	
	public class DynamicPluginWrapperV2 : IPluginEvaluate
	{
		IPluginEvaluate FWrappedPlugin;
		IPluginHost FPluginHost;
		IExecutable FExecutable;
		
		public IPluginEvaluate WrappedPlugin
		{
			get
			{
				return FWrappedPlugin;
			}
		}
		
		public DynamicPluginWrapperV2(IPluginEvaluate wrappedPlugin, IPluginHost pluginHost, IExecutable executable)
		{
			FWrappedPlugin = wrappedPlugin;
			FPluginHost = pluginHost;
			FExecutable = executable;
		}
		
		public void Evaluate(int SpreadMax)
		{
			try
			{
				FWrappedPlugin.Evaluate(SpreadMax);
				FExecutable.RuntimeErrors.Clear();
			}
			catch (Exception e)
			{
				FPluginHost.Log(TLogType.Error, string.Format("{0}\n{1}", e.Message, e.StackTrace));
				if (FExecutable.RuntimeErrors.Count == 0)
					FExecutable.RuntimeErrors.Add(new RuntimeError(e));
				throw e;
			}
		}
	}
}
