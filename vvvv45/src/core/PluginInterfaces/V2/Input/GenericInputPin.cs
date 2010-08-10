using System;
using VVVV.PluginInterfaces.V1;

namespace VVVV.PluginInterfaces.V2.Input
{
	public class GenericInputPin<T> : ObservablePin<T>, IPinUpdater
	{
		protected INodeIn FNodeIn;
		protected IGenericIO<T> FUpstreamInterface;
		protected IGenericIO<T> FDefaultInterface;
		
		public GenericInputPin(IPluginHost host, InputAttribute attribute)
		{
			FDefaultInterface = new GenericIO<T>();
			
			host.CreateNodeInput(attribute.Name, attribute.SliceMode, attribute.Visibility, out FNodeIn);
			FNodeIn.SetSubType(new Guid[] { typeof(T).GUID }, typeof(T).Name);
			
			FNodeIn.SetPinUpdater(this);

			FUpstreamInterface = FDefaultInterface;
		}
		
		public override IPluginIO PluginIO 
		{
			get
			{
				return FNodeIn;
			}
		}
		
		public override void Connect()
		{
			INodeIOBase usI;
			FNodeIn.GetUpstreamInterface(out usI);
			FUpstreamInterface = usI as IGenericIO<T>;
		}
		
		public override void Disconnect()
		{
			FUpstreamInterface = FDefaultInterface;
		}
		
		public override int SliceCount 
		{
			get
			{
				return FNodeIn.SliceCount;
			}
			set
			{
				throw new NotImplementedException();
			}
		}
		
		public override bool IsChanged 
		{
			get 
			{
				return FNodeIn.PinIsChanged;
			}
		}
		
		public override T this[int index] 
		{
			get 
			{
				int usS;
				FNodeIn.GetUpsreamSlice(index, out usS);
				return FUpstreamInterface.GetSlice(usS);
			}
			set 
			{
				throw new NotImplementedException();
			}
		}
	}
}
