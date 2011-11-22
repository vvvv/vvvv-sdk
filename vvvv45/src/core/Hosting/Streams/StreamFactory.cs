using System;
using System.Collections.Generic;
using VVVV.Hosting.Interfaces;
using VVVV.Hosting.Streams.Registry;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.Streams;

namespace VVVV.Hosting.Streams
{
	public class StreamFactory
	{
		class PluginNodeListener : IPluginNodeListener
		{
			private readonly StreamFactory FStreamFactory;
			
			public PluginNodeListener(StreamFactory streamFactory)
			{
				FStreamFactory = streamFactory;
			}
			
			public void BeforeEvaluate()
			{
				foreach (var inStream in FStreamFactory.FAutoValidatedInStreams)
				{
					inStream.Sync();
				}
			}
			
			public void AfterEvaluate()
			{
				foreach (var outStream in FStreamFactory.FOutStreams)
				{
					outStream.Flush();
				}
			}
		}
		
		private readonly IInternalPluginHost FPluginHost;
		private readonly DiffInputStreamRegistry FDiffInputRegistry;
		private readonly InputStreamRegistry FInputRegistry;
		private readonly OutputStreamRegistry FOutputRegistry;
		private readonly ConfigStreamRegistry FConfigRegistry;
		private readonly List<IInStream> FAutoValidatedInStreams = new List<IInStream>();
		private readonly List<IOutStream> FOutStreams = new List<IOutStream>();
		private IPluginNodeListener FPluginNodeListener;
		
		public StreamFactory(
			IInternalPluginHost pluginHost,
			IPluginNodeListener pluginNodeListener,
			DiffInputStreamRegistry diffInputStreamRegistry,
			InputStreamRegistry inputRegistry,
			OutputStreamRegistry outputRegistry,
			ConfigStreamRegistry configRegistry
		)
		{
			FPluginHost = pluginHost;
			FPluginNodeListener = pluginNodeListener;
			FDiffInputRegistry = diffInputStreamRegistry;
			FInputRegistry = inputRegistry;
			FOutputRegistry = outputRegistry;
			FConfigRegistry = configRegistry;
		}
		
		public StreamFactory(
			IInternalPluginHost pluginHost,
			DiffInputStreamRegistry diffInputStreamRegistry,
			InputStreamRegistry inputRegistry,
			OutputStreamRegistry outputRegistry,
			ConfigStreamRegistry configRegistry
		) : this(pluginHost, null, diffInputStreamRegistry, inputRegistry, outputRegistry, configRegistry)
		{
			FPluginNodeListener = new PluginNodeListener(this);
		}
		
		public IInStream CreateInputStream(Type type, InputAttribute attribute)
		{
			IInStream stream = null;
			if (attribute.CheckIfChanged)
			{
				stream = FDiffInputRegistry.CreateStream(type, FPluginHost, type, attribute) as IInStream;
			}
			else
			{
				stream = FInputRegistry.CreateStream(type, FPluginHost, type, attribute) as IInStream;
			}
			
			if (attribute.AutoValidate)
			{
				FAutoValidatedInStreams.Add(stream);
			}
			
			return stream;
		}
		
		public IOutStream CreateOutputStream(Type type, OutputAttribute attribute)
		{
			var stream = FOutputRegistry.CreateStream(type, FPluginHost, type, attribute) as IOutStream;
			FOutStreams.Add(stream);
			return stream;
		}
		
		public IIOStream CreateConfigStream(Type type, ConfigAttribute attribute)
		{
			return FConfigRegistry.CreateStream(type, FPluginHost, type, attribute) as IIOStream;
		}
	}
}
