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
		protected INodeIn FNodeIn;

        public GenericInputPin(IPluginHost host, InputAttribute attribute) : this(host, attribute, new DefaultConnectionHandler()) { }
		
		public GenericInputPin(IPluginHost host, InputAttribute attribute, IConnectionHandler handler)
			: base(host, attribute)
		{
			host.CreateNodeInput(attribute.Name, (TSliceMode)attribute.SliceMode, (TPinVisibility)attribute.Visibility, out FNodeIn);
			
			var type = typeof(T);
			if (type.IsGenericType)
			{
				// Set the GUID of the generic type definition and let the ConnectionHandler figure out
				// if generic types are assignable or not.
				FNodeIn.SetSubType(new Guid[] { type.GetGenericTypeDefinition().GUID }, type.GetGenericTypeDefinition().GetCSharpName());
			}
			else
			{
				FNodeIn.SetSubType(new Guid[] { typeof(T).GUID }, typeof(T).GetCSharpName());
			}
			
			FNodeIn.SetConnectionHandler(handler, this);
			
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
