using System;
using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;

namespace VVVV.Hosting.Pins.Input
{
	public class GenericInputPin<T> : DiffPin<T>, IPinUpdater
	{
		protected INodeIn FNodeIn;
		protected IGenericIO FUpstreamInterface;
		protected IGenericIO FDefaultInterface;
		
		public GenericInputPin(IPluginHost host, InputAttribute attribute)
			: base(host, attribute)
		{
			FDefaultInterface = new GenericIO();
			
			host.CreateNodeInput(attribute.Name, (TSliceMode)attribute.SliceMode, (TPinVisibility)attribute.Visibility, out FNodeIn);
			FNodeIn.SetSubType(new Guid[] { typeof(T).GUID }, typeof(T).FullName);
			
			FUpstreamInterface = FDefaultInterface;
			
			base.Initialize(FNodeIn);
		}
		
		public override void Connect(IPin otherPin)
		{
			INodeIOBase usI;
			FNodeIn.GetUpstreamInterface(out usI);
			FUpstreamInterface = usI as IGenericIO;
		}
		
		public override void Disconnect(IPin otherPin)
		{
			FUpstreamInterface = FDefaultInterface;
		}
		
		public override bool IsChanged
		{
			get
			{
				return FNodeIn.PinIsChanged;
			}
		}
		
		public override void Update()
		{
			if (IsChanged)
			{
				SliceCount = FNodeIn.SliceCount;
				
				for (int i = 0; i < FSliceCount; i++)
				{
					int usS;
					if (FUpstreamInterface != null)
					{
						FNodeIn.GetUpsreamSlice(i, out usS);
						FData[i] = (T) FUpstreamInterface.GetSlice(usS);
					}
					else
					{
						FData[i] = default(T);
					}
				}
			}
			
			base.Update();
		}
	}
}
