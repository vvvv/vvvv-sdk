using System;
using System.Collections;
using System.Runtime.InteropServices;

using VVVV.PluginInterfaces.V1;
using VVVV.Utils.VMath;

namespace Hoster
{
	public class TNodePin: TBasePin, INodeIn, INodeOut, IPluginConfig
	{
		private Guid[] FGuids;
		private string FFriendlyName;
		
		public TNodePin(IPluginHost Parent, string PinName, TPinDirection Direction, TSliceMode SliceMode, TPinVisibility Visibility)
		: base(Parent, PinName, 1, Direction, null, SliceMode, Visibility)
		{}
		
		public void GetUpsreamSlice(int Slice, out int UpstreamSlice)
		{
			//should do magic to deal with GetSlice index permutations
			UpstreamSlice = Slice;
		}
		
		unsafe public void GetUpStreamSlices(out int sliceCount, out int* slices)
		{
		  //TODO: not implemented 
		  sliceCount = 0;
		  slices = (int*)0;
		}
		
		public void GetUpstreamInterface(out INodeIOBase UpstreamInterface)
		{
			//should return interface of upstream connected pin
			UpstreamInterface = null;
		}
		
		public void GetUpstreamInterface([MarshalAs(UnmanagedType.IUnknown)] out object UpstreamInterface)
		{
			//should return interface of upstream connected pin
			UpstreamInterface = null;
		}
		
		public void SetSubType(Guid[] Guids, string FriendlyName)
		{
			FGuids = Guids;
			FFriendlyName = FriendlyName;
		}
		
		public void SetSubType2(Type type, Guid[] guids, string friendlyName)
		{
		    //TODO: not implemented
		}
		
		public void MarkPinAsChanged()
		{
			//todo
		}
		
		public void SetInterface(INodeIOBase TheInterface)
		{}
		
		public void SetInterface([MarshalAs(UnmanagedType.IUnknown)] object TheInterface)
		{}
		
		public void SetConnectionHandler(IConnectionHandler handler, [MarshalAs(UnmanagedType.IUnknown)] object source)
		{}
		
		override protected void ChangeSliceCount()
		{}


		override protected string AsString(int index)
		{
			return "";
		}
		
		override public void SetSpreadAsString(string Spread)
		{}
		
		public bool IsConvoluted 
		{
		    get {return false;}
		}
	}	
}
