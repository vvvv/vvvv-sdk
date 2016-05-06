using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace VVVV.Nodes.Timeliner
{
	public partial class TLEditorState : TLEditor
	{
		private string FStartName = "";
		private string FStartEvents = "";
		
		public TLEditorState()
		{
			//InitializeComponent();
		}
		
		public TLEditorState(List<TLBaseKeyFrame> Keys): base(Keys)
		{
			//
			// The InitializeComponent() call is required for Windows Forms designer support.
			//
			InitializeComponent();

			//check if all keyframes have the same time/value
			double time = FKeyFrames[0].Time;
			string name = (FKeyFrames[0] as TLStateKeyFrame).Name;
			string onEndAction = (FKeyFrames[0] as TLStateKeyFrame).Events[0].Action.ToString();
			
			TLBaseKeyFrame kf = FKeyFrames.Find(delegate(TLBaseKeyFrame k) {return k.Time != time;});
			if (kf == null)
				TimeBox.Value = time;
			
			kf = FKeyFrames.Find(delegate(TLBaseKeyFrame k) {return (k as TLStateKeyFrame).Name != name;});
			if (kf == null)
				NameBox.Text = name;
			FStartName = NameBox.Text;
			
			
			string events = "";
			for (int i=1; i<(FKeyFrames[0] as TLStateKeyFrame).Events.Count; i++)
				events += (FKeyFrames[0] as TLStateKeyFrame).Events[i].ToString();
			
			events = events.Replace(";", "\r\n");
			events = events.Trim();
			EventsBox.Text = events;
			
			
			kf = FKeyFrames.Find(delegate(TLBaseKeyFrame k) {return (k as TLStateKeyFrame).Events[0].Action.ToString() != onEndAction;});
			if (kf == null)
				EndActionBox.Text = onEndAction;
			
			FStartEvents = (FKeyFrames[0] as TLStateKeyFrame).EventsAsString;

			Height = TimeBox.Height + NameBox.Height + label3.Height + EventsBox.Height + EndActionBox.Height + 4;

			//make the valuebox have the focus
			EndActionBox.SelectAll();

			this.ActiveControl = EndActionBox;
		}
		
		void TextKeyPress(object sender, KeyPressEventArgs e)
		{
			if (e.KeyChar == (char)13)	//enter
			{
				e.Handled = true;
				Exit(true);			
			}
			else if (e.KeyChar == (char)27)	//escape
			{
				e.Handled = true;
				Exit(false);
			}
		}
		
		public override void Exit(bool Accept)
		{
			if (Accept)
			{
				//set value and time to keyframes
				foreach (TLStateKeyFrame k in FKeyFrames)
				{
					if (FTimeChanged)
						k.Time = (double) TimeBox.Value;
					
					if (FStartName != NameBox.Text)
						(k as TLStateKeyFrame).Name = NameBox.Text;
				
					string events = "OnEnd " + EndActionBox.Text + ";";
					EventsBox.Text = EventsBox.Text.Trim();
					for (int i=0; i<EventsBox.Lines.Length; i++)
					{
						events += EventsBox.Lines[i].TrimStart().TrimEnd() + ";";
					}
					
					if (FStartEvents != events)
						(k as TLStateKeyFrame).EventsAsString  = events;
				}
			}
			
			base.Exit(Accept);
		}
		
		protected override bool ProcessKeyPreview(ref Message m)
		{
			const int WM_KEYDOWN = 0x100;
    
    		bool handled = false;
    		
			if (m.Msg == WM_KEYDOWN)
			{
				KeyEventArgs ke = new KeyEventArgs((Keys)m.WParam.ToInt32() | ModifierKeys);
				
				if (ke.KeyCode == Keys.Space)
					handled = true;
			}
				
			return handled;
		}
	}
}
