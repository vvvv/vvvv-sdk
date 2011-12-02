using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;

using VVVV.PluginInterfaces.V1;
using VVVV.Utils.VMath;

namespace VVVV.Nodes.Timeliner
{
	public enum TLAutomataCommand {NoChange, Play, Pause, Jump};
	
	public class TLAutomataSlice :  TLSlice
	{
		private IValueConfig FStateTime;
		private IStringConfig FStateName, FStateEvents;
		
		private TLStateKeyFrame FCurrentState;
		public TLStateKeyFrame CurrentState
		{
			get{return FCurrentState;}
		}
		
		private TLAutomataCommand FCommand = TLAutomataCommand.NoChange;
		public TLAutomataCommand Command
		{
			get{return FCommand;}
		}
		
		public double TimeToJumpTo
		{
			get
			{	
				FCommand = TLAutomataCommand.Play;
				return FKeyFrames[Math.Max(0, FKeyFrames.IndexOf(FCurrentState) - 1)].Time + 0.000001;
			}
		}
		
		private double FPauseTime;
		public double PauseTime
		{
			get{return FPauseTime;}
		}
		
		private List<IValueFastIn> FEventPins = new List<IValueFastIn>();

		public TLAutomataSlice(IPluginHost Host, TLBasePin Pin, int SliceIndex, int PinsOrder): base(Host, Pin, SliceIndex, PinsOrder)
		{}
		
		public void InitializeWithLoop()
		{
			AddKeyFrame("-INF", 0, "OnEnd next;");
			AddKeyFrame("loop", 10, "OnEnd loop;");
			AddKeyFrame("+INF", double.MaxValue, "OnEnd pause;");
			
			SaveKeyFrames();
			
			FCurrentState = FKeyFrames[1] as TLStateKeyFrame;
		}
		
		public void ForceStateFromCurrentTime(double CurrentTime)
		{
			FCurrentState = (FKeyFrames.Find(delegate(TLBaseKeyFrame s) {return s.Time > CurrentTime;}) as TLStateKeyFrame);
		}
		
		protected override void CreatePins()
		{
			FHost.CreateValueConfig(FPin.Name + "-Time" + FSliceIndex.ToString(), 1, null, TSliceMode.Dynamic, TPinVisibility.Hidden, out FStateTime);        	
			FStateTime.SliceCount=0;
			FStateTime.SetSubType(Double.MinValue, Double.MaxValue,0.001D,0,false, false,false);
	    	
	    	FHost.CreateStringConfig(FPin.Name + "-Name" + FSliceIndex.ToString(), TSliceMode.Dynamic, TPinVisibility.Hidden, out FStateName);
        	FStateName.SliceCount=0;
			FStateName.SetSubType("default",false);
			
			FHost.CreateStringConfig(FPin.Name + "-Events" + FSliceIndex.ToString(), TSliceMode.Dynamic, TPinVisibility.Hidden, out FStateEvents);
        	FStateEvents.SliceCount=0;
			FStateEvents.SetSubType("default",false);
		}
		
		public override void DestroyPins()
		{
			FHost.DeletePin(FStateTime);
			FHost.DeletePin(FStateName);
			FHost.DeletePin(FStateEvents);
			FStateTime = null;
			FStateName = null;
			FStateEvents = null;
			
			FKeyFrames.Clear();
		}
		
		protected override void SliceOrPinOrderChanged()
		{
			FStateTime.Name = FPin.Name + "-Time" + FSliceIndex.ToString();
			FStateName.Name = FPin.Name + "-Name" + FSliceIndex.ToString();
			FStateEvents.Name = FPin.Name + "-Events" + FSliceIndex.ToString();
			
			FStateTime.Order = FPin.Order;
			FStateName.Order = FPin.Order;
			FStateEvents.Order = FPin.Order;
		}
	
		private TLBaseKeyFrame AddKeyFrame(string Name, double EndTime, string Events)
		{
			//called via configpinchanged and gui-doubleclick
			float sliceheight = FPin.Height; // / FPin.SliceCount;
			float slicetop = FPin.Top + FSliceIndex * sliceheight;
			FKeyFrames.Add(new TLStateKeyFrame(FPin.Transformer, EndTime, Name, Events, slicetop, sliceheight));
			
			return FKeyFrames[FKeyFrames.Count-1];
		}
		
		public override TLBaseKeyFrame AddKeyFrame(Point P)
		{
			//called only from GUI-doubleclick

			return AddKeyFrame("x", FPin.Transformer.XPosToTime(P.X), "OnEnd loop;");
		}
		
		public override void Configurate(IPluginConfig Input, bool FirstFrame)
		{
			//if Input = last ConfigInput created!
			if (Input == FStateEvents && FirstFrame)
			{
				FKeyFrames.Clear();
				
				string name, events;
				double time;
				
				for (int i = 0; i<FStateEvents.SliceCount;i++)	
				{
					FStateTime.GetValue(i, out time);
					FStateName.GetString(i, out name);
					FStateEvents.GetString(i, out events);
					AddKeyFrame(name, time, events);
				}
				
				FKeyFrames.Sort(delegate(TLBaseKeyFrame k0, TLBaseKeyFrame k1) { return k0.Time.CompareTo(k1.Time); });	
			}
			
			//make sure every state's events have according input-pins
			//remove all inputs that don't have an according event
			//go through all events and find a corresponding pin
			List<IValueFastIn> tmpList = new List<IValueFastIn>();
			IValueFastIn tmpEventPin;
			foreach (TLStateKeyFrame skf in FKeyFrames)
			{
				foreach(TLEvent e in skf.Events)
				{
					if (e.Name != "OnEnd")
					{
						IValueFastIn ep = FEventPins.Find(delegate(IValueFastIn p) {return p.Name == e.Name;});
						
						if (ep == null)
						{
							FHost.CreateValueFastInput(e.Name, 1, null, TSliceMode.Single, TPinVisibility.True, out tmpEventPin);
							tmpEventPin.SetSubType(0, 1, 1, 0, false, false, false);
							e.EventPin = tmpEventPin;
							tmpList.Add(tmpEventPin);
							FEventPins.Add(tmpEventPin);
						}
						else
						{
							e.EventPin = ep;
							if (!tmpList.Contains(ep))
								tmpList.Add(ep);
						}
					}
				}
			}
			
			for (int i=0; i<FEventPins.Count; i++)
			{
				if (!tmpList.Contains(FEventPins[i]))
					FHost.DeletePin(FEventPins[i]);
			}
			
			FEventPins.Clear();
			FEventPins.AddRange(tmpList);
		}

		public override void SaveKeyFrames()
		{
			FStateTime.SliceCount = FKeyFrames.Count;
			FStateName.SliceCount = FKeyFrames.Count;
			FStateEvents.SliceCount = FKeyFrames.Count;
			
			FKeyFrames.Sort(delegate(TLBaseKeyFrame k0, TLBaseKeyFrame k1) { return  k0.Time.CompareTo(k1.Time); });
			
			for (int i = 0; i<FKeyFrames.Count; i++)
			{
				FStateTime.SetValue(i, FKeyFrames[i].Time);
				FStateName.SetString(i, (FKeyFrames[i] as TLStateKeyFrame).Name);
				FStateEvents.SetString(i, (FKeyFrames[i] as TLStateKeyFrame).EventsAsString);
			}
		}
		
		private void SetCurrentStateAndCommand(TLEvent Event, TLStateKeyFrame LastState, double CurrentTime)
		{
			switch (Event.Action.Command)
					{
						case TLActionCommand.next:
							{
								FCurrentState = (FKeyFrames.Find(delegate(TLBaseKeyFrame s) {return s.Time > LastState.Time;}) as TLStateKeyFrame);
								FCommand = TLAutomataCommand.Jump;
								break;
							}
						case TLActionCommand.previous:
							{
								FCurrentState = (FKeyFrames.FindLast(delegate(TLBaseKeyFrame s) {return s.Time < LastState.Time;}) as TLStateKeyFrame);
								FCommand = TLAutomataCommand.Jump;
								break;
							}
						case TLActionCommand.loop:
							{
								FCurrentState = LastState;
								FCommand = TLAutomataCommand.Jump;
								break;
							}
						case TLActionCommand.play:
							{
								FCurrentState = (FKeyFrames.Find(delegate(TLBaseKeyFrame s) {return s.Time > CurrentTime;}) as TLStateKeyFrame);
								FCommand = TLAutomataCommand.Play;
								break;
							}
						case TLActionCommand.pause:
							{
								FCurrentState = LastState;
								FPauseTime = CurrentTime;
								FCommand = TLAutomataCommand.Pause;
								break;
							}
						case TLActionCommand.jump:
							{
								FCurrentState = (TLStateKeyFrame) FKeyFrames.Find(delegate(TLBaseKeyFrame k) {return (k as TLStateKeyFrame).Name == Event.GotoState;});
								if (FCurrentState == null)
								{
									FCurrentState = LastState;
									FCommand = TLAutomataCommand.Pause;
								}
								else
									FCommand = TLAutomataCommand.Jump;
								
								break;
							}
					}
		}
		
		public override void Evaluate(double CurrentTime)
		{
			base.Evaluate(CurrentTime);
			FCommand = TLAutomataCommand.NoChange;
			
			if (FCurrentState != null)
			{
				TLStateKeyFrame lastState = FCurrentState;
				TLStateKeyFrame tmpState = (FKeyFrames.Find(delegate(TLBaseKeyFrame s) {return s.Time > CurrentTime;}) as TLStateKeyFrame);
				
				if (tmpState.Time > lastState.Time) //lastState is over, so execute action of lastState
				{
					SetCurrentStateAndCommand(lastState.Events[0], lastState, lastState.Time);
				}
				//else
				{
					//go through all events of FCurrentState and see if their inputs are ON
					double on = 0;
					foreach (TLEvent e in FCurrentState.Events)
					{
						if (e.EventPin != null)
						{
							e.EventPin.GetValue(0, out on);
							if (on > 0.5)
							{
								SetCurrentStateAndCommand(e, lastState, CurrentTime);
								break;
							}
						}
					}
				}
				
				OutputAsString = FCurrentState.Name;
			}
		}
		
		public override void DrawSlice(Graphics g, double From, double To, bool AllInOne, bool Collapsed)
		{
			base.DrawSlice(g, From, To, AllInOne, Collapsed);
			
			float x, width;
			double lastTime = 0;
			TLStateKeyFrame k;
			SolidBrush bWhite = new SolidBrush(Color.White);
			SolidBrush bSilver = new SolidBrush(Color.Silver);
			StringFormat sf = new StringFormat();
			sf.Alignment = StringAlignment.Far;
			Pen pGray = new Pen(Color.Gray);
			
			for (int i=0; i<FInvalidKeyFrames.Count; i++)
			{
				if (i == 0)
					x = 0;
				else
				{
					x = FInvalidKeyFrames[i-1].GetTimeAsX();
					lastTime = FInvalidKeyFrames[i-1].Time;
				}
				
				if (i == FInvalidKeyFrames.Count-1)
				{
					x = Math.Max(0, x);
					width = g.ClipBounds.Width - x;
				}
				else				
					width = FInvalidKeyFrames[i].GetTimeAsX() - x;
				
				k = (TLStateKeyFrame) FInvalidKeyFrames[i];
				
				//draw state icon
				if (i != FInvalidKeyFrames.Count-1)
				{
					g.DrawRectangle(pGray, x+width-15, 0, 15, FPin.Height);
					g.DrawString(k.OnStateEndReached.Action.Icon, FFont, new SolidBrush(Color.Black), x + width - 16, 1);
				}	
				
				RectangleF clip;
				RectangleF tmpClip = g.ClipBounds;
				clip = new RectangleF(x, 0, width - 15, g.ClipBounds.Height);
				g.SetClip(clip);

				
				if (k.Selected)
				{
					g.FillRectangle(bSilver, x, 0, width - 15, FPin.Height);
					g.DrawString(k.Time.ToString("f2", TimelinerPlugin.GNumberFormat)+"s", FFont, new SolidBrush(Color.Black), x+width-15, 3, sf);
	
					//show states length
					if (i != 0)
						g.DrawString((k.Time - lastTime).ToString("f2", TimelinerPlugin.GNumberFormat)+"s", FFont, new SolidBrush(Color.Black), x+width-15, 13, sf);
				}
				
				string stateInfo = k.Name;
				float nWidth = g.MeasureString(stateInfo, FFont).Width;
				x += width/2 - nWidth/2 - 7.5f;
				g.DrawString(stateInfo, FFont, new SolidBrush(Color.Black), x, 3);
				
				char[] t = {';'};
				if (k.Events.Count > 1)
				{
					for (int j=1; j<k.Events.Count; j++)
						g.DrawString(k.Events[j].ToString().TrimEnd(t), FFont, new SolidBrush(Color.Black), x, 15+j*12);
				}
				
				g.SetClip(tmpClip);
			}
			
			float sliceheight = FPin.Height / FPin.SliceCount;
			
			float sWidth = g.MeasureString(OutputAsString, FFont).Width + 2;
			g.DrawString(OutputAsString, FFont, new SolidBrush(Color.Gray), g.ClipBounds.Width-sWidth, sliceheight-16);
		}
	}		
}

