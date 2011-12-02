using System;
using System.Linq;
using System.Runtime.InteropServices;
using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.Reflection;

namespace VVVV.Hosting.Pins.Input
{
    [ComVisible(false)]
	public class GenericInputPin<T> : DiffPin<T>, IPinUpdater
	{
		class ConnectionHandler : IConnectionHandler
		{
			public bool Accepts(object source, object sink)
			{
				var sourceDataType = source.GetType().GetGenericArguments().First();
				var sinkDataType = sink.GetType().GetGenericArguments().First();
				
				return sinkDataType.IsAssignableFrom(sourceDataType);
			}
		}
		
		protected INodeIn FNodeIn;
		
		public GenericInputPin(IPluginHost host, InputAttribute attribute)
			: base(host, attribute)
		{
			host.CreateNodeInput(attribute.Name, (TSliceMode)attribute.SliceMode, (TPinVisibility)attribute.Visibility, out FNodeIn);
			
			var type = typeof(T);
			if (type.IsGenericType)
			{
				// Set the GUID of the generic type definition and let the ConnectionHandler figure out
				// if generic types are assignable or not.
				FNodeIn.SetSubType(new Guid[] { type.GetGenericTypeDefinition().GUID }, type.GetCSharpName());
				FNodeIn.SetConnectionHandler(new ConnectionHandler(), this);
			}
			else
			{
				FNodeIn.SetSubType(new Guid[] { typeof(T).GUID }, typeof(T).GetCSharpName());
			}
			
			base.InitializeInternalPin(FNodeIn);
		}
		
		protected override bool IsInternalPinChanged
		{
			get
			{
				return FNodeIn.PinIsChanged;
			}
		}
		
		unsafe protected override void DoUpdate()
		{
			SliceCount = FNodeIn.SliceCount;
			
			object usI;
			FNodeIn.GetUpstreamInterface(out usI);
			var upstreamInterface = usI as IGenericIO;
			
			for (int i = 0; i < FSliceCount; i++)
			{
				int usS;
				if (upstreamInterface != null)
				{
					FNodeIn.GetUpsreamSlice(i, out usS);
					FBuffer[i] = (T) upstreamInterface.GetSlice(usS);
				}
				else
				{
					FBuffer[i] = default(T);
				}
			}
		}
	}
}
