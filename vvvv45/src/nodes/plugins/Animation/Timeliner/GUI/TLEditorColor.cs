using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

using VVVV.Utils.VColor;

namespace VVVV.Nodes.Timeliner
{
	public partial class TLEditorColor : TLEditor
	{
		private double FOldH, FOldS, FOldV, FOldA;
		
		private bool FHueChanged, FSatChanged, FLumChanged, FAlphaChanged = false;
		
		public TLEditorColor(List<TLBaseKeyFrame> Keys): base(Keys)
		{
			//
			// The InitializeComponent() call is required for Windows Forms designer support.
			//
			InitializeComponent();

			//check if all keyframes have the same time/value
			//if they have the same value, show that value, if not leave it blank
			double time = FKeyFrames[0].Time;
			TLBaseKeyFrame kf = FKeyFrames.Find(delegate(TLBaseKeyFrame k) {return k.Time != time;});
			if (kf == null)
				TimeBox.Value = time;
			
			RGBAColor rgba = (FKeyFrames[0] as TLColorKeyFrame).RGBAColor;
			VColor.RGBtoHSV(rgba.R, rgba.G, rgba.B, out FOldH, out FOldS, out FOldV);
			FOldA = rgba.A;
			
			/*
			FOldH = (FKeyFrames[0] as TLColorKeyFrame).Color.GetHue();
			FOldS = (FKeyFrames[0] as TLColorKeyFrame).Color.GetSaturation();
			FOldL = (FKeyFrames[0] as TLColorKeyFrame).Color.GetBrightness();
			FOldA = (FKeyFrames[0] as TLColorKeyFrame).Color.A;
			*/
			
			kf = FKeyFrames.Find(delegate(TLBaseKeyFrame k) {return (k as TLColorKeyFrame).Hue != FOldH;});
			if (kf == null)
				HueBox.Value = FOldH;
			kf = FKeyFrames.Find(delegate(TLBaseKeyFrame k) {return (k as TLColorKeyFrame).Saturation != FOldS;});
			if (kf == null)
				SatBox.Value = FOldS;
			kf = FKeyFrames.Find(delegate(TLBaseKeyFrame k) {return (k as TLColorKeyFrame).Value != FOldV;});
			if (kf == null)
				LumBox.Value = FOldV;
			kf = FKeyFrames.Find(delegate(TLBaseKeyFrame k) {return (k as TLColorKeyFrame).Alpha != FOldA;});
			if (kf == null)
				AlphaBox.Value = FOldA;	
			
			Height = TimeBox.Height + HueBox.Height + SatBox.Height + LumBox.Height + AlphaBox.Height + 4;

			this.ActiveControl = LumBox;
		}
		
		public override void Exit(bool Accept)
		{
			if (Accept)
			{
				double h, s, l, a;
				double boxH, boxS, boxL, boxA;
				
				boxH = HueBox.Value;
				boxS = SatBox.Value;
				boxL = LumBox.Value;
				boxA = AlphaBox.Value;
				
				//set value and time to keyframes
				foreach (TLColorKeyFrame k in FKeyFrames)
				{
					if (FTimeChanged)
						k.Time = (double) TimeBox.Value;
					
					if (FHueChanged)
						h = boxH;
					else
						h = k.Hue;
					
					if (FSatChanged)
						s = boxS;
					else
						s = k.Saturation;
					
					if (FLumChanged)
						l = boxL;
					else
						l = k.Value;
					
					if (FAlphaChanged)
						a = boxA;
					else
						a = k.Alpha;
					
					k.SetColorComponents(h, s, l, a);
				}
			}
			
			base.Exit(Accept);
		}
		
		void EditorColorKeyPress(object sender, KeyPressEventArgs e)
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
		
		void HueBoxOnValueChange(double Value)
		{
			FHueChanged = true;
		}
		
		void SatBoxOnValueChange(double Value)
		{
			FSatChanged = true;
		}
		
		void LumBoxOnValueChange(double Value)
		{
			FLumChanged = true;
		}
		
		void AlphaBoxOnValueChange(double Value)
		{
			FAlphaChanged = true;
		}
		
		void BoxKeyPress(object sender, KeyPressEventArgs e)
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
