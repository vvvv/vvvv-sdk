using System;
using System.Collections.Generic;
using VVVV.Hosting.Interfaces;
using VVVV.Hosting.Streams.Registry;
using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.Streams;

namespace VVVV.Hosting.Streams
{
	public class IOFactory : IDisposable
	{
		class PluginNodeListener : IPluginNodeListener
		{
			private readonly IOFactory FIOFactory;
			
			public PluginNodeListener(IOFactory streamFactory)
			{
				FIOFactory = streamFactory;
			}
			
			public void BeforeEvaluate()
			{
				foreach (var input in FIOFactory.FInputs)
				{
					input.BeforeEvaluate();
				}
			}
			
			public void AfterEvaluate()
			{
				foreach (var output in FIOFactory.FOutputs)
				{
					output.AfterEvaluate();
				}
			}
			
			public void Configurate(IPluginConfig configPin)
			{
				foreach (var config in FIOFactory.FConfigs)
				{
					if (config.Metadata == configPin)
					{
						config.Configurate();
						break;
					}
				}
			}
		}
		
		private readonly IInternalPluginHost FPluginHost;
		private readonly IORegistry FIORegistry;
		private readonly List<IOHandler> FInputs = new List<IOHandler>();
		private readonly List<IOHandler> FOutputs = new List<IOHandler>();
		private readonly List<IOHandler> FConfigs = new List<IOHandler>();
		private IPluginNodeListener FPluginNodeListener;
		
		public IOFactory(
			IInternalPluginHost pluginHost,
			IORegistry streamRegistry
		)
		{
			FPluginHost = pluginHost;
			FIORegistry = streamRegistry;
		}
		
		public void Dispose()
		{
			// HACK: FPluginHost is null in case of WindowSwitcher/NodeBrowser/etc. Fix this.
			if (FPluginHost != null && FPluginNodeListener != null)
			{
				FPluginHost.RemoveListener(FPluginNodeListener);
			}
		}
		
		private void HookPluginNode()
		{
			// HACK: FPluginHost is null in case of WindowSwitcher/NodeBrowser/etc. Fix this.
			if (FPluginHost != null && FPluginNodeListener == null)
			{
				FPluginNodeListener = new PluginNodeListener(this);
				FPluginHost.AddListener(FPluginNodeListener);
			}
		}
		
		public IInternalPluginHost PluginHost
		{
			get
			{
				return FPluginHost;
			}
		}
		
		public IOHandler CreateIOHandler<T>(IOAttribute attribute)
		{
			return CreateIOHandler(typeof(T), attribute);
		}
		
		public IOHandler CreateIOHandler(Type type, IOAttribute attribute)
		{
			var io = FIORegistry.CreateIO(
				type.IsGenericType ? type.GetGenericTypeDefinition() : type,
				type,
				this,
				attribute
			);
			
			if (io == null)
			{
				throw new NotSupportedException(string.Format("Can't create {0} of type '{1}'.", attribute, type));
			}
			
			if (io.IsBeforeEvalActionEnabled)
			{
				HookPluginNode();
				FInputs.Add(io);
			}
			if (io.IsAfterEvalActionEnabled)
			{
				HookPluginNode();
				FOutputs.Add(io);
			}
			if (io.IsConfigActionEnabled)
			{
				HookPluginNode();
				FConfigs.Add(io);
			}
			
			return io;
		}
		
		public object CreateRawIOObject(Type type, IOAttribute attribute)
		{
			return CreateIOHandler(type, attribute).RawIOObject;
		}
		
		public T CreateIO<T>(IOAttribute attribute)
			where T : class
		{
			return CreateRawIOObject(typeof(T), attribute) as T;
		}
		
		public bool CanCreateIO(Type type, IOAttribute attribute)
		{
			if (!FIORegistry.CanCreate(type, attribute))
			{
				if (type.IsGenericType)
				{
					var openGenericType = type.GetGenericTypeDefinition();
					return CanCreateIO(openGenericType, attribute);
				}
				
				return false;
			}
			
			return true;
		}
		
		public void DestroyIOHandler(IOHandler io)
		{
			if (io.IsBeforeEvalActionEnabled)
			{
				FInputs.Remove(io);
			}
			if (io.IsAfterEvalActionEnabled)
			{
				FOutputs.Remove(io);
			}
			if (io.IsConfigActionEnabled)
			{
				FConfigs.Remove(io);
			}
			
			var pluginIO = io.Metadata as IPluginIO;
			if (pluginIO != null)
			{
				FPluginHost.DeletePin(pluginIO);
			}
		}
	}
}
