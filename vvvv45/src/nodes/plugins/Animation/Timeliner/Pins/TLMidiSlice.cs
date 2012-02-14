using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;

using VVVV.Utils.VMidiScore;
using VVVV.Utils.VColor;
using VVVV.Utils.VMath;
using VVVV.PluginInterfaces.V1;

namespace VVVV.Nodes.Timeliner
{
	public class TLMidiSlice :  TLSlice
	{
		/*
		private IValueIO FNoteIn;
		private IValueIO FBarIn;
		private IValueIO FReset;
		private IValueIO FMaxWindowNotes;
		 */
		/*
		private string FOutput;
		public string Output
		{
			get {return FOutput;}
		}
		*/
		
		public TMidiScore MidiScore
		{
			get{return (FPin as TLMidiPin).MidiScore;}
		}
		
		private string FTrackName;
		public string TrackName
		{
			set{FTrackName = value;}
			get{return FTrackName;}
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
			FFont = new Font("Verdana", 7);
		}
		
		protected override void CreatePins()
		{}
		
		public override void DestroyPins()
		{
			FKeyFrames.Clear();
		}
		
		protected override void SliceOrPinOrderChanged()
		{
			/*
			FKeyOutType.Name = FPin.Name + "-OutType" + FSliceIndex.ToString();		*/
		}
		
		public override void Evaluate(double CurrentTime)
		{
			base.Evaluate(CurrentTime);
			
			OutputAsString = FTrackName;
			
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
			base.Configurate(Input, FirstFrame);
		}
		
		public void InitializeNotes(List<TMidiNote> Notes)
		{
			foreach (TMidiNote mn in Notes)
				AddKeyFrame(mn.Start * MidiScore.SecondsPerMidiUnit, mn.End * MidiScore.SecondsPerMidiUnit, mn.Track, mn.Channel, mn.Note, mn.Velocity);
		}
		
		private TLBaseKeyFrame AddKeyFrame(double Start, double End, int Track, int Channel, int Note, int Velocity)
		{
			//called via configpinchanged and gui-doubleclick
			float sliceheight = FPin.Height / FPin.SliceCount;
			float slicetop = FPin.Top + FSliceIndex * sliceheight;
			TLMidiKeyFrame k = new TLMidiKeyFrame(FPin.Transformer, Track, Channel, Note, Velocity, Start, End, FMinNote, FMaxNote, slicetop, sliceheight);
			FKeyFrames.Add(k);
			return k;
		}
		
		public override TLBaseKeyFrame AddKeyFrame(Point P)
		{
			//called only from GUI-doubleclick
			float sliceheight = FPin.Height / FPin.SliceCount;
			float slicetop = FPin.Top + FSliceIndex * sliceheight;
			double start = FPin.Transformer.XPosToTime(P.X);
			double end = start + 1;
			int note = (int) VMath.Map(P.Y, slicetop, slicetop + sliceheight, FMaxNote, FMinNote, TMapMode.Float);
			return AddKeyFrame(start, end, FSliceIndex, FSliceIndex, note, 100);
		}
		
		public override void SaveKeyFrames()
		{
		}
		
		public override void DrawSlice(Graphics g, double From, double To, bool AllInOne, bool Collapsed)
		{
			base.DrawSlice(g, From, To, AllInOne, Collapsed);
			if ((MidiScore == null) || (MidiScore.TimeSignature.Denominator == 0))
				return;
			
			float sliceHeight;
			if (AllInOne)
				sliceHeight = FPin.Height;
			else
				sliceHeight = FPin.Height / FPin.SliceCount;
			
			float noteHeight = Math.Max(2, sliceHeight / (FMaxNote - FMinNote));

			//draw piano
			SolidBrush black = new SolidBrush(Color.Black);
			SolidBrush white = new SolidBrush(Color.White);
			for (int i=0; i<FMaxNote - FMinNote; i++)
			{
				if (TMidiScore.KeyColor[(FMinNote + i) % 12] == Color.Black)
					g.FillRectangle(black, 0, i*noteHeight, 10, noteHeight);
				else
					g.FillRectangle(white, 0, i*noteHeight, 10, noteHeight);
			}
			white.Dispose();
			
			//draw notelines
			using (Pen p = new Pen(Color.Silver))
			{
				for (int i=0; i<FMaxNote - FMinNote; i++)
				{
					g.DrawLine(p, 0, i*noteHeight, g.ClipBounds.Width, i*noteHeight);
				}
			}

			//draw bars
			float x;
			SolidBrush gray = new SolidBrush(Color.Gray);
			using (Pen p = new Pen(Color.Silver))
			{
				for (int i = 0; i < MidiScore.BeatCount; i++)
				{
					x = FPin.Transformer.TransformPoint(new PointF((float) MidiScore.SecondsPerBeat * i, 0)).X;
					if (i % MidiScore.TimeSignature.Numerator == 0)
						p.Width = 2;
					else
						p.Width = 1;
					
					g.DrawLine(p, x, 0, x, sliceHeight);
					if (SliceIndex == 0)
						g.DrawString((i+1).ToString(), FFont, gray, new PointF(x, 0));
				}
			}
			
			//MU..midi units
			int MUpWholeNote = MidiScore.Division * 4;
			int MUpBeat = MUpWholeNote / MidiScore.TimeSignature.Denominator;
			int MUpBar = MidiScore.TimeSignature.Numerator * MUpBeat;
			int barCount = MidiScore.Length / MUpBar;
			int beatCount = barCount * MidiScore.TimeSignature.Numerator;
			
			float y;
			//draw notes
			Region clip = g.Clip;
			SolidBrush silver = new SolidBrush(Color.Silver);
			Pen blackPen = new Pen(Color.Black);
			foreach (TLMidiKeyFrame k in FInvalidKeyFrames)
			{
				//transform the keyframes time by the current transformation
				x = k.GetTimeAsX();
				y = k.GetValueAsY();
				float length = k.GetDurationAsX();
				
				if (k.Selected)
				{
					g.FillRectangle(silver, x, y, length, noteHeight);
					g.Clip = new Region();
					g.DrawString(k.Time.ToString("f2", TimelinerPlugin.GNumberFormat)+"s", FFont, black, x, y-14);
					g.DrawString(k.Note.ToString("f0", TimelinerPlugin.GNumberFormat), FFont, black, x, y+7);
					g.Clip = clip;
				}
				else
				{
					Color vel = Color.White;
					if (AllInOne)
					{
						double hue = (k.Channel * 0.8) % 1;
						vel = VColor.FromHSVA(hue, 1, .8, 1).Color;
					}
					else
						vel = Color.FromArgb(k.Note*2, 255, 255, 255);
					
					using (SolidBrush b = new SolidBrush(vel))
						g.FillRectangle(b, x, y, length, noteHeight);
					g.DrawRectangle(blackPen, x, y, length, noteHeight);
				}
			}
			black.Dispose();
			silver.Dispose();
			blackPen.Dispose();
			
			
			
			float sWidth = g.MeasureString(OutputAsString, FFont).Width + 2;
			g.DrawString(OutputAsString, FFont, gray, g.ClipBounds.Width-sWidth, sliceHeight-16);
			gray.Dispose();
		}
	}
}

