using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Collections.Generic;

using VVVV.Utils.VMath;

namespace VVVV.Nodes.Timeliner
{
	public class TLStateKeyFrame: TLBaseKeyFrame
	{
		private string FName;
		public string Name
		{
			get{return FName;}
			set{FName = value;}
		}
		
		public TLEvent OnStateEndReached
		{
			//event 0 always is: OnStateEndReached
			get{return FEvents[0];}
		}
		
		private List<TLEvent> FEvents = new List<TLEvent>();		
		public List<TLEvent> Events
		{
			get{return FEvents;}
		}
			
		public string EventsAsString
		{
			get
			{
				string events = "";
				
				foreach (TLEvent e in FEvents)
					events += e.ToString();
				
				return events;
			}
			set
			{
				FEvents.Clear();
				char[] s = {';'};
				
				string[] events = value.Split(s);
				for (int i=0; i<events.Length-1; i++)
					FEvents.Add(new TLEvent(events[i]));
			}
		}

		public TLStateKeyFrame(TLTransformer Transformer, double Time, string Name, string Events, float SliceTop, float SliceHeight): base(Transformer, Time, SliceTop, SliceHeight)
		{
			FName = Name;
			this.EventsAsString = Events;
		}
		
		protected override Region GetRedrawArea()
		{
			//needs to be bigger than hitarea to include the white splitters covering all of slicearea
			Region flag = new Region(new RectangleF((float) (GetTimeAsX())-20, 0, 25, 10000));
			return flag;
		}
		
		protected override Region GetHitArea()
		{
			Region flag = new Region(new RectangleF((float) (GetTimeAsX())-20, FSliceTop, 20, FSliceHeight));
			return flag;
		}		
	}
}
