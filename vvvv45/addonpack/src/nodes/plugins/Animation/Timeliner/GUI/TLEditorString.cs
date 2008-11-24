using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace VVVV.Nodes.Timeliner
{
	public partial class TLEditorString : TLEditor
	{
		private string FStartValue = "";
		
		public TLEditorString()
		{
			//InitializeComponent();
		}
		
		public TLEditorString(List<TLBaseKeyFrame> Keys): base(Keys)
		{
			//
			// The InitializeComponent() call is required for Windows Forms designer support.
			//
			InitializeComponent();

			//check if all keyframes have the same time/value
			double time = FKeyFrames[0].Time;
			string text = (FKeyFrames[0] as TLStringKeyFrame).Value;
			TLBaseKeyFrame kf = FKeyFrames.Find(delegate(TLBaseKeyFrame k) {return k.Time != time;});
			if (kf == null)
				TimeBox.Value = time;
			
			kf = FKeyFrames.Find(delegate(TLBaseKeyFrame k) {return (k as TLStringKeyFrame).Value != text;});
			if (kf == null)
				ValueBox.Text = text;	
			
			FStartValue = ValueBox.Text;

			Height = TimeBox.Height + ValueBox.Height + 1;

			//make the valuebox have the focus
			ValueBox.SelectAll();

			this.ActiveControl = ValueBox;
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
				foreach (TLStringKeyFrame k in FKeyFrames)
				{
					if (FTimeChanged)
						k.Time = (double) TimeBox.Value;
					
					if (FStartValue != ValueBox.Text)
						(k as TLStringKeyFrame).Value = ValueBox.Text;
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
