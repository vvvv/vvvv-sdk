using System;
using System.Collections.Generic;
using VVVV.Hosting.Interfaces;
using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;

namespace VVVV.Hosting.IO
{
	class IOFactory : IIOFactory, IDisposable
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
				foreach (var input in FIOFactory.FPreHandlers)
				{
					input.PreEvaluate();
				}
			}
			
			public void AfterEvaluate()
			{
				foreach (var output in FIOFactory.FPostHandler)
				{
					output.PostEvaluate();
				}
			}
			
			public void Configurate(IPluginConfig configPin)
			{
				foreach (var config in FIOFactory.FConfigHandlers)
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
		private readonly List<IOHandler> FPreHandlers = new List<IOHandler>();
		private readonly List<IOHandler> FPostHandler = new List<IOHandler>();
		private readonly List<IOHandler> FConfigHandlers = new List<IOHandler>();
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
		
		public IPluginHost2 PluginHost
		{
			get
			{
				return FPluginHost;
			}
		}
		
		public IIOHandler CreateIOHandler(Type type, IOAttribute attribute)
		{
			var io = FIORegistry.CreateIOHandler(
				type.IsGenericType ? type.GetGenericTypeDefinition() : type,
				type,
				this,
				attribute
			);
			
			if (io == null)
			{
				throw new NotSupportedException(string.Format("Can't create {0} of type '{1}'.", attribute, type));
			}
			
			if (io.NeedsPreEvaluation)
			{
				HookPluginNode();
				FPreHandlers.Add(io);
			}
			if (io.NeedsPostEvaluation)
			{
				HookPluginNode();
				FPostHandler.Add(io);
			}
			if (io.NeedsConfiguration)
			{
				HookPluginNode();
				FConfigHandlers.Add(io);
			}
			
			return io;
		}
		
		public bool CanCreateIOHandler(Type type, IOAttribute attribute)
		{
			if (!FIORegistry.CanCreate(type, attribute))
			{
				if (type.IsGenericType)
				{
					var openGenericType = type.GetGenericTypeDefinition();
					return CanCreateIOHandler(openGenericType, attribute);
				}
				
				return false;
			}
			
			return true;
		}
		
		public void DestroyIOHandler(IIOHandler io_interface)
		{
			// HACK: Remove this cast
			var io = (IOHandler) io_interface;
			
			if (io.NeedsPreEvaluation)
			{
				FPreHandlers.Remove(io);
			}
			if (io.NeedsPostEvaluation)
			{
				FPostHandler.Remove(io);
			}
			if (io.NeedsConfiguration)
			{
				FConfigHandlers.Remove(io);
			}
			
			var pluginIO = io.Metadata as IPluginIO;
			if (pluginIO != null)
			{
				FPluginHost.DeletePin(pluginIO);
			}
		}
	}
}
