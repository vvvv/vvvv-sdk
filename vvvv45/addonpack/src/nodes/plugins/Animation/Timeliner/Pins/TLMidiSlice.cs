using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;

using VVVV.Utils.MidiScore;
using VVVV.Utils.VColor;
using VVVV.PluginInterfaces.V1;

namespace VVVV.Nodes.Timeliner
{
	public class TLMidiSlice :  TLSlice
	{
		private Font FFont = new Font("Verdana", 7);
		/*		private IValueIO FKeyTime;
		private IStringIO FKeyValue;
		private IValueIO FNoteIn;
		private IValueIO FBarIn;
		private IValueIO FReset;
		private IValueIO FMaxWindowNotes;
		 */		private string FOutput;
		private TMidiScore FMidiScore;
		
		
		public string Output
		{
			get {return FOutput;}
		}
		
		private string FTrackName;
		public string TrackName
		{
			set{FTrackName = value;}
		}
		
		private int FTrackLength;
		public int TrackLength
		{
			set{FTrackLength = value;}
		}
		
		private int FBPM;
		public int BPM
		{
			set{FBPM = value;}
		}
		
		private int FDivision = 120;
		public int Division
		{
			set{FDivision = value;}
		}
		
		private int FEnumerator = 4;
		public int Enumerator
		{
			set{FEnumerator = value;}
		}
		
		private int FDenominator = 4;
		public int Denominator
		{
			set{FDenominator = value;}
		}
		
		private int FMinNote;
		public int MinNote
		{
			get { return FMinNote;}
			set
			{
				FMinNote = value;
				foreach (TLMidiKeyFrame k in FKeyFrames)
					k.MinNote = FMinNote;
			}
		}
		
		private int FMaxNote;
		public int MaxNote
		{
			get { return FMaxNote;}
			set
			{
				FMaxNote = value;
				foreach (TLMidiKeyFrame k in FKeyFrames)
					k.MaxNote = FMaxNote;
			}
		}
		
		
		public TLMidiSlice(IPluginHost Host, TLBasePin Pin, int SliceIndex, int PinsOrder): base(Host, Pin, SliceIndex, PinsOrder)
		{
			//FMidiScore = MidiScore;
		}
		
		protected override void CreatePins()
		{

		}
		
		public override void DestroyPins()
		{
			FKeyFrames.Clear();
		}
		
		protected override void SliceOrPinOrderChanged()
		{
			/*	FKeyTime.Name = FPin.Name + "-Time" + FSliceIndex.ToString();
			FKeyValue.Name = FPin.Name + "-Value" + FSliceIndex.ToString();
			FKeyInType.Name = FPin.Name + "-InType" + FSliceIndex.ToString();
			FKeyOutType.Name = FPin.Name + "-OutType" + FSliceIndex.ToString();		*/
		}
		
		public override void Evaluate(double CurrentTime)
		{
			FOutputAsString = FTrackName;
			
			/*	FMidiScore.SaveLastWindowStart();
			
			if (FBarIn.PinIsChanged)
			{
				double dval;
				FBarIn.GetValue(0, out dval);
				FMidiScore.ExternalBar = (int) dval;
				
				this.Invalidate(FMidiScore.GetWindowRect(new Rectangle(0, 0, Width, Height)));
			}
			
			if (FMaxWindowNotes.PinIsChanged)
			{
				double dval;
				FMaxWindowNotes.GetValue(0, out dval);
				FMidiScore.MaxWindowNotes = (int) dval;
			}
			
			if (FReset.PinIsChanged)
			{
				double dval;
				FReset.GetValue(0, out dval);
				if (dval == 1)
					FMidiScore.ClearPlayedNotes();
				this.Invalidate();
			}
			
			FMidiScore.FNotesFound.Clear();
			if (FNoteIn.PinIsChanged)
			{
				double dval = -1;

				
				//adjust WindowOfNotes
				FMidiScore.PrepareNotesOfWindow();
				
				for (int i=0; i<FNoteIn.SliceCount; i++)
				{
					FNoteIn.GetValue(i, out dval);
					FMidiScore.ShowNote((int) dval, 100);
				}
				
				//System.Diagnostics.Debug.WriteLine("dval: " + dval.ToString() + " notecount: " + FNoteIn.SliceCount.ToString());
				if (dval >= 0)
				{
					FMidiScore.AdjustWindowBorders(FMidiScore.FNotesFound.Count);
					
				}
				this.Invalidate(FMidiScore.GetWindowRect(new Rectangle(0, 0, Width, Height)));
			}*/
			
			
		}

		public override void Configurate(IPluginConfig Input, bool FirstFrame)
		{
			//if Input = last ConfigInput created!
			/*	if (Input == FKeyOutType && FirstFrame)
			{
				FKeyFrames.Clear();
				//System.Windows.Forms.MessageBox.Show("FKeyOutType: " + FKeyOutType.SliceCount.ToString());
				double time, val, intype, outtype;
				for (int i = 0; i<FKeyOutType.SliceCount;i++)
				{
					FKeyTime.GetValue(i, out time);
					FKeyValue.GetValue(i, out val);
					FKeyInType.GetValue(i, out intype);
					FKeyOutType.GetValue(i, out outtype);
					AddKeyFrame(time, val, (TLInterpolationType) intype, (TLInterpolationType) outtype);
				}
				
				FKeyFrames.Sort(delegate(TLBaseKeyFrame k0, TLBaseKeyFrame k1) { return k0.Time.CompareTo(k1.Time); });
			}
			 */
			base.Configurate(Input, FirstFrame);
		}
		
		public void InitializeNotes(List<TMidiNote> Notes)
		{
			foreach (TMidiNote mn in Notes)
				AddKeyFrame(mn.Start / 1000.0, mn.End / 1000.0, mn.Track, mn.Channel, mn.Note, mn.Velocity);
		}
		
		private TLBaseKeyFrame AddKeyFrame(double Start, double End, int Track, int Channel, int Note, int Velocity)
		{
			//called via configpinchanged and gui-doubleclick
			float sliceheight = FPin.Height / FPin.SliceCount;
			float slicetop = FPin.Top + FSliceIndex * sliceheight;
			TLMidiKeyFrame k = new TLMidiKeyFrame(FPin.Transformer, Track, Channel, Note, Velocity, Start, End, 0, 127, slicetop, sliceheight);
			FKeyFrames.Add(k);
			return k;
		}
		
		public override TLBaseKeyFrame AddKeyFrame(Point P)
		{
			//called only from GUI-doubleclick
			float sliceheight = FPin.Height / FPin.SliceCount;
			float slicetop = FPin.Top + FSliceIndex * sliceheight;
			return null; //AddKeyFrame(FPin.Transformer.XPosToTime(P.X), (float) VMath.Map(P.Y, slicetop, slicetop + sliceheight, FMaxValue, FMinValue, TMapMode.Float), FCurrentInType, FCurrentOutType);
		}
		
		public override void SaveKeyFrames()
		{
			/*		FKeyTime.SliceCount = FKeyFrames.Count;
			FKeyValue.SliceCount = FKeyFrames.Count;
			FKeyInType.SliceCount = FKeyFrames.Count;
			FKeyOutType.SliceCount = FKeyFrames.Count;
			
			FKeyFrames.Sort(delegate(TLBaseKeyFrame k0, TLBaseKeyFrame k1) { return  k0.Time.CompareTo(k1.Time); });
			
			for (int i = 0; i<FKeyFrames.Count; i++)
			{
				FKeyTime.SetValue(i, FKeyFrames[i].Time);
				FKeyValue.SetValue(i, (FKeyFrames[i] as TLValueKeyFrame).Value);
				FKeyInType.SetValue(i, (int) (FKeyFrames[i] as TLValueKeyFrame).InType);
				FKeyOutType.SetValue(i, (int) (FKeyFrames[i] as TLValueKeyFrame).OutType);
			}*/
		}
		
		public void SetKeyFrameTypes(TLInterpolationType InType, TLInterpolationType OutType)
		{
			/*	FCurrentInType = InType;
			FCurrentOutType = OutType;
			
			foreach (TLValueKeyFrame k in KeyFrames)
			{
				if (k.Selected)
				{
					k.InType = InType;
					k.OutType = OutType;
				}
			}*/
		}
		
		public override void DrawSlice(Graphics g, double From, double To, bool AllInOne)
		{
			base.DrawSlice(g, From, To, AllInOne);
			
			float sliceHeight;
			if (AllInOne)
				sliceHeight = FPin.Height;
			else
				sliceHeight = FPin.Height / FPin.SliceCount;
			
			float noteHeight = Math.Max(2, sliceHeight / (FMaxNote - FMinNote));

			float visibleBeats = FBPM / 60 * (float) (To-From);
			float pixelPerBeat = g.ClipBounds.Width / visibleBeats;
			float ppmu = pixelPerBeat / FDivision;

			//pixel per time:
			float ppt = g.ClipBounds.Width / (float) (To-From);

			//midi units per time: MUpT
			float visibleMidiUnits = visibleBeats * FDivision;

			//draw notelines
			Pen p = new Pen(Color.Silver);			               
			for (int i=0; i<FMaxNote - FMinNote; i++)
			{
				g.DrawLine(p, 0, i*noteHeight, g.ClipBounds.Width, i*noteHeight);
			}
			
			float x, y;
			//draw notes
			foreach (TLMidiKeyFrame k in FInvalidKeyFrames)
			{
				//transform the keyframes time by the current transformation
				float size = 3;
				x = k.GetTimeAsX();
				y = k.GetValueAsY();// - noteHeight/2;
				float length = k.GetDurationAsX();
				
				if (k.Selected)
				{
					g.FillRectangle(new SolidBrush(Color.Silver), x, y, length, noteHeight);
					g.DrawString(k.Time.ToString("f2", TimelinerPlugin.GNumberFormat)+"s", FFont, new SolidBrush(Color.Black), x, y-14);
					g.DrawString(k.Note.ToString("f0", TimelinerPlugin.GNumberFormat), FFont, new SolidBrush(Color.Black), x, y+7);
				}
				else
				{
					Color vel = Color.White;
					if (AllInOne)
					{
						double hue = (k.Channel * 0.66) % 1;
						vel = VColor.FromHSVA(hue, 1, .8, 1).Color;
					}
					else
						vel = Color.FromArgb(k.Note*2, 255, 255, 255);
					
					g.FillRectangle(new SolidBrush(vel), x, y, length, noteHeight);
					g.DrawRectangle(new Pen(Color.Black), x, y, length, noteHeight);
				}
			}
			
			int ganze = FDivision * 4;
			int part = ganze / FDenominator;
			int barLength = FEnumerator * part;
			float barcount = FTrackLength / barLength;

        	//draw bars
        	for (int i = 0; i < barcount; i++)
        	{
        		x = barLength * i * ppmu;
        		if (i % 16 == 0)
	        		p.Width = 2;
        		else
        			p.Width = 1;
        		
        		g.DrawLine(p, x, 0, x, sliceHeight);
        		g.DrawString((i+1).ToString(), FFont, new SolidBrush(Color.Gray), new PointF(x, 0));
        	}
			
			float sWidth = g.MeasureString(FOutputAsString, FFont).Width + 2;
			g.DrawString(FOutputAsString, FFont, new SolidBrush(Color.Gray), g.ClipBounds.Width-sWidth, sliceHeight-16);
		}
		/*
		protected override void OnMouseDoubleClick (MouseEventArgs e)
		{
			switch(FMouseState)
			{
				case TLMouseState.msSelecting://not msIdle, because on mousedown state has already become msSelecting
				{
					FLoading = false;
					AddKeyFrame(TLMain.XPosToTime(e.X), "(empty)");
					
					SaveKeyFrames();
					break;
				}
					
				case TLMouseState.msDragging:
				{
					List<TLKeyFrame> keys = FKeyFrames.FindAll(delegate(TLKeyFrame k) {return k.Selected;});
					
					if (keys != null)
					{
						FEditor = new TLEditorString(keys, FFont);
						FEditor.OnExit += new ExitHandler(ExitEditor);
						FEditor.Left = e.X;
						FEditor.Top = e.Y;
						FEditor.Show();
						this.Controls.Add(FEditor);
						
						FMouseState = TLMouseState.msEditing;
					}
					
					break;
				}
					
				case TLMouseState.msEditing:
				{
					FEditor.Exit(true);
					break;
				}
			}
		}

		protected override void OnMouseDown(MouseEventArgs e)
		{
			switch(FMouseState)
			{
				case TLMouseState.msIdle:
				{
					//mouse is not hovering a keyframe and rightclicking -> start panning
					if (e.Button == MouseButtons.Right)
					{
						this.Cursor = new Cursor(GetType(),"images.GrabHand.cur");
						FMouseState = TLMouseState.msPanning;
						break;
					}
					
					break;
				}
			}
		}
		
		protected override void OnMouseMove(MouseEventArgs e)
		{
			switch(FMouseState)
			{
				case TLMouseState.msPanning:
				{
					double delta = TLMain.XDeltaToTime(e.X - FLastMousePoint.X);
					TranslateTime(delta);
					break;
				}
			}
			
			FLastMousePoint = e.Location;
		}
		
		protected override void OnMouseUp(MouseEventArgs e)
		{
		 	switch(FMouseState)
			{
				case TLMouseState.msPanning:
				{
					this.Cursor = null;
					FMouseState = TLMouseState.msIdle;
					break;
				}
			}

			this.Invalidate();
		}
		 */
		
		
	}
}

