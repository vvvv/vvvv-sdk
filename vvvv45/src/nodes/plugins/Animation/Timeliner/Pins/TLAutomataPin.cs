using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Xml;

using VVVV.PluginInterfaces.V1;

namespace VVVV.Nodes.Timeliner
{
	public class TLAutomataPin: TLPin
	{
		private IStringOut FCurrentStateOut;
		private IStringOut FStates;
		private IValueOut FStateTimes;
		
		public TLStateKeyFrame CurrentState
		{
			get{return (FOutputSlices[0] as TLAutomataSlice).CurrentState;}
		}
		
		public TLStateKeyFrame NextState
		{
			get
			{
				int current = FOutputSlices[0].KeyFrames.IndexOf(CurrentState);
				
				return (TLStateKeyFrame) FOutputSlices[0].KeyFrames[Math.Min(current+1, FOutputSlices[0].KeyFrames.Count-2)];
			}
		}
		
		public TLStateKeyFrame PreviousState
		{
			get
			{
				int current = FOutputSlices[0].KeyFrames.IndexOf(CurrentState);
				
				return (TLStateKeyFrame) FOutputSlices[0].KeyFrames[Math.Max(current-1, 0)];
			}
		}
		
		public TLAutomataCommand Command
		{
			get{return (FOutputSlices[0] as TLAutomataSlice).Command;}
		}
		
		public double TimeToJumpTo
		{
			get{return (FOutputSlices[0] as TLAutomataSlice).TimeToJumpTo;}
		}
		
		public double PauseTime
		{
			get{return (FOutputSlices[0] as TLAutomataSlice).PauseTime;}
		}
		
		public TLAutomataPin(IPluginHost Host, TLTransformer Transformer, int Order, XmlNode PinSettings):base(Host, Transformer, Order, PinSettings, false, false)
		{
			AddSlice(0);
		}
		
		protected override void CreatePins()
		{
			base.CreatePins();
			
	    	FHost.CreateStringOutput(Name, TSliceMode.Dynamic, TPinVisibility.True, out FCurrentStateOut);
        	FCurrentStateOut.SetSubType("", false);
	    	FCurrentStateOut.Order = Order;
	    	
			FHost.CreateStringOutput(Name + "-State", TSliceMode.Dynamic, TPinVisibility.OnlyInspector, out FStates);
	    	FStates.SetSubType("", false);
	    	FStates.Order = Order;
			
	    	FHost.CreateValueOutput(Name + "-StateTime", 1, null, TSliceMode.Dynamic, TPinVisibility.OnlyInspector, out FStateTimes);
	    	FStateTimes.SetSubType(double.MinValue, double.MaxValue, 0.0001, 0, false, false, false);
	    	FStateTimes.Order = Order;
		}
		
		public override void DestroyPins()
		{
			// DELETE ALL PINS
			///////////////////////
			FHost.DeletePin(FCurrentStateOut);
			FCurrentStateOut = null;
			
			FHost.DeletePin(FStates);
			FStates = null;
			
			FHost.DeletePin(FStateTimes);
			FStateTimes = null;

			base.DestroyPins();
		}
		
		public override void AddSlice(int At)
		{
			TLAutomataSlice ss = new TLAutomataSlice(FHost, this, FOutputSlices.Count, FOrder);

			AddSlice(At, ss);
		}
		
		protected override void InitializeHeight()
		{
			if (FUncollapsedHeight == 0)
				FUncollapsedHeight = DIP(40);
			
			base.InitializeHeight();
		}
		
		public void InitializeWithLoop()
		{
			(FOutputSlices[0] as TLAutomataSlice).InitializeWithLoop();
		}
		
		public void ForceStateFromCurrentTime(double CurrentTime)
		{
			(FOutputSlices[0] as TLAutomataSlice).ForceStateFromCurrentTime(CurrentTime);
		}

		public override void Evaluate(double CurrentTime)
		{
			base.Evaluate(CurrentTime);
			
			if (CurrentState == null)
				return;
			
			FCurrentStateOut.SetString(0, CurrentState.Name);
			
			var stateCount = (FOutputSlices[0] as TLAutomataSlice).KeyFrames.Count-1;
			
			FStateTimes.SliceCount = stateCount;
			for (int i=0; i<(FOutputSlices[0] as TLAutomataSlice).KeyFrames.Count-1; i++)
				FStateTimes.SetValue(i, (FOutputSlices[0] as TLAutomataSlice).KeyFrames[i].Time);
			
			FStates.SliceCount = stateCount;
			for (int i=0; i<(FOutputSlices[0] as TLAutomataSlice).KeyFrames.Count-1; i++)
				FStates.SetString(i, ((FOutputSlices[0] as TLAutomataSlice).KeyFrames[i] as TLStateKeyFrame).Name);
		}
	}
}
