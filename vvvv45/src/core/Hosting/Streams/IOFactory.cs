using System;
using System.Collections.Generic;
using VVVV.Hosting.Interfaces;
using VVVV.Hosting.Streams.Registry;
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
		}
		
		private readonly IInternalPluginHost FPluginHost;
		private readonly IORegistry FIORegistry;
		private readonly List<IOHandler> FInputs = new List<IOHandler>();
		private readonly List<IOHandler> FOutputs = new List<IOHandler>();
		private readonly IPluginNodeListener FPluginNodeListener;
		
		public IOFactory(
			IInternalPluginHost pluginHost,
			IORegistry streamRegistry
		)
		{
			FPluginHost = pluginHost;
			FPluginNodeListener = new PluginNodeListener(this);
			FIORegistry = streamRegistry;
			
			// HACK: FPluginHost is null in case of WindowSwitcher/NodeBrowser/etc. Fix this.
			if (FPluginHost != null)
			{
				FPluginHost.AddListener(FPluginNodeListener);
			}
		}
		
		public void Dispose()
		{
			// HACK: FPluginHost is null in case of WindowSwitcher/NodeBrowser/etc. Fix this.
			if (FPluginHost != null)
			{
				FPluginHost.RemoveListener(FPluginNodeListener);
			}
		}
		
		public IOHandler CreateIOHandler(Type type, IOAttribute attribute)
		{
			IOHandler io = null;
			if (!FIORegistry.CanCreate(type, attribute))
			{
				if (type.IsGenericType)
				{
					var openGenericType = type.GetGenericTypeDefinition();
					if (FIORegistry.CanCreate(openGenericType, attribute))
					{
						io = FIORegistry.CreateIO(openGenericType, type, FPluginHost, attribute);
					}
				}
			}
			
			if (io == null)
			{
				throw new NotSupportedException(string.Format("Can't create IO of type '{0}'.", type));
			}
			
			if (io.IsBeforeEvalActionEnabled)
			{
				FInputs.Add(io);
			}
			if (io.IsAfterEvalActionEnabled)
			{
				FOutputs.Add(io);
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
	}
}
