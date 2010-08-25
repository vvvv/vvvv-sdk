using System;
using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;

namespace VVVV.Hosting.Pins.Output
{
	/// <summary>
	/// Description of GenericOutputPin.
	/// </summary>
	public class GenericOutputPin<T> : Pin<T>, IPinUpdater, IGenericIO<T>
	{
		protected INodeOut FNodeOut;
		protected IGenericIO<T> FUpstreamInterface;
		protected T[] FData;
		protected bool FChanged;
		protected int FSliceCount;
		
		public GenericOutputPin(IPluginHost host, OutputAttribute attribute)
			: base(host, attribute)
		{
			host.CreateNodeOutput(attribute.Name, (TSliceMode)attribute.SliceMode, (TPinVisibility)attribute.Visibility, out FNodeOut);
			FNodeOut.SetSubType(new Guid[] { typeof(T).GUID }, typeof(T).FullName);
			FNodeOut.SetInterface(this);
			
			FNodeOut.SetPinUpdater(this);
			
			SliceCount = 1;
		}
		
		public override IPluginIO PluginIO 
		{
			get
			{
				return FNodeOut;
			}
		}
		
		public override int SliceCount 
		{
			get
			{
				return FSliceCount;
			}
			set
			{
				if (FSliceCount != value)
					FData = new T[value];
				
				FSliceCount = value;
				
				if (FAttribute.SliceMode != SliceMode.Single)
					FNodeOut.SliceCount = value;
			}
		}
		
		public override T this[int index] 
		{
			get 
			{
				throw new NotImplementedException();
			}
			set 
			{
				FData[index % FSliceCount] = value;
				FChanged = true;
			}
		}
		
		public T GetSlice(int slice)
		{
			return FData[slice % FSliceCount];
		}
		
		public override void Update()
		{
			base.Update();
			
			if(FChanged) FNodeOut.MarkPinAsChanged();
			FChanged = false;
		}
	}
}
