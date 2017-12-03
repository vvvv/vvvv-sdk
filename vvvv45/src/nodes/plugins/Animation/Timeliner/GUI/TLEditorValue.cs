using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace VVVV.Nodes.Timeliner
{
	public partial class TLEditorValue : TLEditor
	{
		private double FStartValue = 0.0D;
		
		public TLEditorValue()
		{
			//InitializeComponent();
		}
		
		public TLEditorValue(List<TLBaseKeyFrame> Keys): base(Keys)
		{
			//
			// The InitializeComponent() call is required for Windows Forms designer support.
			//
			InitializeComponent();
			
			ValueBox.Minimum = double.MinValue;
			ValueBox.Maximum = double.MaxValue;

			//check if all keyframes have the same time/value
			double time = FKeyFrames[0].Time;
			double val = (FKeyFrames[0] as TLValueKeyFrame).Value;
			TLBaseKeyFrame kf = FKeyFrames.Find(delegate(TLBaseKeyFrame k) {return k.Time != time;});
			if (kf == null)
				TimeBox.Value = time;
			
			kf = FKeyFrames.Find(delegate(TLBaseKeyFrame k) {return (k as TLValueKeyFrame).Value != val;});
			if (kf == null)
				ValueBox.Value = val;
			
			FStartValue = val;

			Height = TimeBox.Height + ValueBox.Height + 1;
			this.ActiveControl = ValueBox;
			ValueBox.ShowValueBox();
		}
		
		public override void Exit(bool Accept)
		{
			if (Accept)
			{
				//set value and time to keyframes
				foreach (TLValueKeyFrame k in FKeyFrames)
				{
					if (FTimeChanged)
						k.Time = (double) TimeBox.Value;
					
					if (ValueBox.Value != FStartValue)
						(k as TLValueKeyFrame).SetValue(ValueBox.Value);
				}
			}
			
			base.Exit(Accept);
		}
		
		void ValueBoxKeyPress(object sender, KeyPressEventArgs e)
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
