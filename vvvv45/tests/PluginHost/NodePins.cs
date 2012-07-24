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
		private object FTheInterface;
		
		public TNodePin(IPluginHost Parent, string PinName, TPinDirection Direction, TSliceMode SliceMode, TPinVisibility Visibility)
		: base(Parent, PinName, 1, Direction, null, SliceMode, Visibility)
		{
			base.Initialize();
		}
		
		public void GetUpsreamSlice(int Slice, out int UpstreamSlice)
		{
			//should do magic to deal with GetSlice index permutations
			UpstreamSlice = Slice;
		}
		public void GetUpstreamInterface(out INodeIOBase UpstreamInterface)
		{
			//should return interface of upstream connected pin
			UpstreamInterface = null;
		}
		
		public void SetSubType(Guid[] Guids, string FriendlyName)
		{
			FGuids = Guids;
			FFriendlyName = FriendlyName;
		}
		
		public void MarkPinAsChanged()
		{
			//todo
		}
		
		public void SetInterface(INodeIOBase TheInterface)
		{
			FTheInterface = TheInterface;
		}
		
		override protected void ChangeSliceCount()
		{}


		override protected string AsString(int index)
		{
			return "";
		}
		
		override public void SetSpreadAsString(string Spread)
		{}
		
		public void GetUpstreamInterface(out object UpstreamInterface)
		{
			//should return interface of upstream connected pin
			UpstreamInterface = null;
		}
		
		public void SetInterface(object TheInterface)
		{
			FTheInterface = TheInterface;
		}
	    
        public void SetConnectionHandler(IConnectionHandler handler, object source)
        {
            throw new NotImplementedException();
        }
	}	
}
