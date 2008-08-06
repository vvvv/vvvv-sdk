using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using System.Collections.Generic;

namespace VVVV.Nodes.Timeliner
{
	public partial class TLSliceArea : UserControl
	{
		protected Point FLastMousePoint = new Point();
		protected Rectangle FSelectionArea = new Rectangle();
		private Region FTimeBar;
		private float FTimeAsX;
		private List<TLBasePin> FPinsWithSelectedKeyframes = new List<TLBasePin>();
		
		private List<TLBasePin> FOutputPins;
		public List<TLBasePin> OutputPins
		{
			set {FOutputPins = value;}
		}
		private enum TLMouseState {msIdle, msSelecting, msDragging, msDraggingXOnly, msDraggingYOnly, msPanning, msEditing, msDraggingTimeBar};
		private static TLMouseState FMouseState = TLMouseState.msIdle;
		
		private TLEditor FEditor;
		private TLTransformer FTransformer;
		public TLTransformer Transformer
		{
			set {FTransformer = value;}
		}
		
		private TLTime FTimer;
		public TLTime Timer
		{
			set {FTimer = value;}
		}
		
		private int FSplitterPosition;
		public int SplitterPosition
		{
			set {FSplitterPosition = value;}
		}
		
		private bool FValuesChanged = false;
		Region FCurrentValueRegions = new Region(new Rectangle(0, 0, 1, 1));
		
		public TLSliceArea()
		{
			//
			// The InitializeComponent() call is required for Windows Forms designer support.
			//
			InitializeComponent();
			
			FTimeBar = new Region(new Rectangle(0, 0, 10, Height));
			
			//GDI+ Speed Settings ????
			SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
			SetStyle(ControlStyles.UserPaint, true);
			SetStyle(ControlStyles.AllPaintingInWmPaint, true);
			this.UpdateStyles();
		}
		
		
		private double FTime;
		
		public void Evaluate()
		{
			//update Timebar
			FTime = FTimer.CurrentTime;
			float timeasx = FTransformer.TransformPoint(new PointF((float) FTime, 0)).X;
			bool timechanged = timeasx != FTimeAsX;
			
			//update current-value regions if any value changed
			if (FValuesChanged || timechanged)
			{
				//update last frames value regions
				this.Invalidate(FCurrentValueRegions);
				//clear it
				FCurrentValueRegions = new Region(new Rectangle(0, 0, 1, 1));
				
				//invalidate areas on each slice to show their outputs
				Graphics g = this.CreateGraphics();
				foreach(TLBasePin bp in FOutputPins)
				{
					if (bp.OutputSlices.Count > 0)
					{
						float sHeight = bp.Height / bp.OutputSlices.Count;
						
						foreach(TLSlice s in bp.OutputSlices)
						{
							float sTop = bp.Top + sHeight * s.SliceIndex;
							SizeF sz = g.MeasureString(s.OutputAsString, new Font("Lucida Sans Unicode", 7));
							FCurrentValueRegions.Union(new Region(new RectangleF(Width-sz.Width-6, sTop + sHeight-sz.Height-1, sz.Width+6, sz.Height+1)));
						}
					}
				}
				g.Dispose();
				
				this.Invalidate(FCurrentValueRegions);
				FValuesChanged = false;
			}
			
			if (timeasx != FTimeAsX)
			{
				//update current-value regions if time changed
				this.Invalidate(FCurrentValueRegions);
				
				//invalidate last time
				this.Invalidate(FTimeBar);
				
				FTimeAsX = timeasx;
				FTimeBar = new Region(new RectangleF(FTimeAsX-6, 0, 10, Height));
				
				//invalidate new time
				this.Invalidate(FTimeBar);
			}
		}
		
		protected override void OnPaintBackground(PaintEventArgs e)
		{
			base.OnPaintBackground(e);
			//don't draw background for maximum performance ?!
			//Graphics g = e.Graphics;
			//g.FillRectangle(new HatchBrush(HatchStyle.LargeCheckerBoard, Color.White, Color.Gray), g.ClipBounds);
		}
		
		protected override void OnPaint(PaintEventArgs e)
		{
			base.OnPaint(e);
			
			Graphics g = e.Graphics;
			
			g.SmoothingMode = SmoothingMode.HighSpeed;
			g.CompositingQuality = CompositingQuality.HighSpeed;
			
			//pixeloffset setting for having no flickers between color keyframes
			g.PixelOffsetMode = PixelOffsetMode.Half;
			//g.TextRenderingHint =
			
			//debug cliprect
			//Random r = new Random();
			//Color c = Color.FromArgb(255, r.Next(255), r.Next(255), r.Next(255));
			//g.FillRectangle(new SolidBrush(c), g.ClipBounds);
			
			if (FTransformer == null)
				return;
			
			double from = FTransformer.XPosToTime(g.ClipBounds.X);
			double to = FTransformer.XPosToTime(g.ClipBounds.X + g.ClipBounds.Width);
			
			//draw all pins and their slices
			foreach(TLBasePin bp in FOutputPins)
			{
				g.Transform = new Matrix();
				g.TranslateTransform(0, bp.Top);

				int y = 0;
				int height = bp.Height;
				if ((string) bp.Parent.Tag == "0")
				{
					y = 0;
					height = Math.Max(0, Math.Min(bp.Height, FSplitterPosition-bp.Top));
				}
				else //pin is in lower pinheader and may be cut off due to scrolling
				{
					y = Math.Max(0, FSplitterPosition - bp.Top);
					height = bp.Height;
				}
				
				g.Clip = new Region(new RectangleF(0, y, Width, height));
				
				//draw pins slices
				bp.DrawSlices(g, from, to);
			}
			
			//reset clipping and transformation
			g.ResetClip();
			g.Transform = new Matrix();
			
			//draw splitter
			g.DrawLine(new Pen(Color.DarkGray, 5), 0, FSplitterPosition+3, Width, FSplitterPosition+3);
			
			//draw timebar
			float time = FTransformer.TransformPoint(new PointF((float) FTime, 0)).X;
			g.DrawLine(new Pen(Color.Black), time, 0, time, Height);
			PointF[] tri = new PointF[3];
			tri[0] = new PointF(time, 7);
			tri[1] = new PointF(time-4, 0);
			tri[2] = new PointF(time+4, 0);
			g.FillPolygon(new SolidBrush(Color.Black), tri);
			
			//draw selection rectangle
			Pen pn = new Pen(Color.Silver, 1);
			//pn.DashStyle = DashStyle.Dash;
			g.DrawRectangle(pn, StraightenRectangle(FSelectionArea));/**/
		}
		
		private void ExitEditor(bool Accept)
		{
			if (Accept)
				this.Invalidate();

			FEditor.Dispose();
			
			SaveAllKeyFrames();
			
			FMouseState = TLMouseState.msIdle;
		}
		
		public void HideSliceMenu()
		{
			SliceMenuPanel.Hide();
		}
		
		private void SaveAllKeyFrames()
		{
			foreach (TLBasePin p in FPinsWithSelectedKeyframes)
				p.SaveKeyFrames();
		}
		
		private TLBasePin PosToPin(Point p)
		{
			//which pin does the mouse hover?
			//beware: FindLast is needed because of splitpane!
			//pins from lower panel are "lying on top of" pins in upper panel
			TLBasePin pin = null;
			if (p.Y < FSplitterPosition)
				pin	= FOutputPins.Find(delegate(TLBasePin bp)
				                       {
				                       	Region r = new Region(new Rectangle(0, bp.Top, Width, bp.Height));
				                       	return r.IsVisible(p);
				                       });
			else
				pin	= FOutputPins.FindLast(delegate(TLBasePin bp)
				                           {
				                           	Region r = new Region(new Rectangle(0, bp.Top, Width, bp.Height));
				                           	return r.IsVisible(p);
				                           });
			return pin;
		}
		
		private TLSlice PosPinToSlice(Point p, TLBasePin pin)
		{
			//which slice does the mouse hover?
			float top = pin.Top;
			float sliceheight = pin.Height / pin.SliceCount;
			int sliceidx = (int) ((p.Y - top) / sliceheight);
			return pin.OutputSlices[sliceidx];
		}
		
		private void UpdateSliceMenu(Point p)
		{
			TLBasePin pin = PosToPin(p);
			if (pin == null || pin is TLRulerPin || pin is TLAutomataPin || pin.Collapsed)
				SliceMenuPanel.Hide();
			else
			{
				RemoveSliceButton.Enabled = (pin.SliceCount > 1);
				
				//	TLSlice slice = PosPinToSlice(p, pin);
				float top = pin.Top;
				float sliceheight = pin.Height / pin.SliceCount;
				int sliceidx = (int) ((p.Y - top) / sliceheight);
				
				MoveUpButton.Enabled = (sliceidx > 0);
				MoveDownButton.Enabled = (sliceidx < pin.SliceCount-1);
				
				SliceMenuPanel.Top = (int) Math.Round(top + sliceidx*sliceheight);
				SliceMenuPanel.Height = (int) Math.Round(sliceheight);
				SliceMenuPanel.Show();
			}
		}
		
		public void TimeAlignSelectedKeyFrames()
		{
			bool first = true;
			double newtime = 0;
			foreach (TLBasePin p in FOutputPins)
				foreach (TLSlice s in p.OutputSlices)
				foreach (TLBaseKeyFrame k in s.KeyFrames)
				if (k.Selected)
			{
				if (first)
				{
					first = false;
					newtime = k.Time;
				}
				this.Invalidate(k.RedrawArea);
				k.Time = newtime;
				this.Invalidate(k.RedrawArea);
			}
			
			SaveAllKeyFrames();
		}
		
		public void DeleteSelectedKeyFrames(TLBasePin Pin)
		{
			foreach(TLSlice s in Pin.OutputSlices)
				for (int i=s.KeyFrames.Count-1; i>-1; i--)
				if (s.KeyFrames[i].Selected)
			{
				DeleteKeyFrame(Pin, s, s.KeyFrames[i]);
			}
			
			Pin.SaveKeyFrames();
		}
		
		private void DeleteKeyFrame(TLBasePin Pin, TLSlice Slice, TLBaseKeyFrame KeyFrame)
		{
			this.Invalidate(GetUpdateRegion(Pin, Slice, KeyFrame));
			
			Slice.KeyFrames.Remove(KeyFrame);
		}
		
		private Rectangle StraightenRectangle(Rectangle r)
		{
			if (r.Width < 0)
			{
				r.Width = Math.Abs(r.Width);
				r.X -= r.Width;
			}
			
			if (r.Height < 0)
			{
				r.Height = Math.Abs(r.Height);
				r.Y -= r.Height;
			}
			
			return r;
		}
		
		void MenuPanelResize(object sender, EventArgs e)
		{
			MoveUpButton.Height = SliceMenuPanel.Height / 2;
			AddAboveButton.Height = SliceMenuPanel.Height / 2;
		}
		
		void RemoveSliceButtonClick(object sender, EventArgs e)
		{
			Point pos = new Point(SliceMenuPanel.Left, SliceMenuPanel.Top + 2);
			TLBasePin pin = PosToPin(pos);
			if (pin.SliceCount > 1)
			{
				TLSlice slice = PosPinToSlice(pos, pin);
				pin.RemoveSlice(slice);
				
				UpdateSliceMenu(pos);
				this.Invalidate(new Rectangle(0, pin.Top, Width, pin.Height));
			}
		}
		
		void MoveSlice(bool Up)
		{
			Point pos = new Point(SliceMenuPanel.Left, SliceMenuPanel.Top + 2);
			TLBasePin pin = PosToPin(pos);
			
			TLSlice slice = PosPinToSlice(pos, pin);
			int oldidx = pin.OutputSlices.IndexOf(slice);
			pin.OutputSlices.Remove(slice);
			
			if (Up)
				pin.OutputSlices.Insert(oldidx-1, slice);
			else
				pin.OutputSlices.Insert(oldidx+1, slice);
			
			UpdateSliceMenu(pos);
			
			//call those here manually since no PinChangedCB will be issued if only slices are moved
			pin.UpdateSliceIndices();
			pin.UpdateKeyFrameAreas();
			
			this.Invalidate(new Rectangle(0, pin.Top, Width, pin.Height));
		}
		
		void MoveUpButtonClick(object sender, EventArgs e)
		{
			MoveSlice(true);
		}

		void MoveDownButtonClick(object sender, EventArgs e)
		{
			MoveSlice(false);
		}
		
		void AddSlice(bool Above)
		{
			Point pos = new Point(SliceMenuPanel.Left, SliceMenuPanel.Top + 2);
			TLBasePin pin = PosToPin(pos);
			
			TLSlice slice = PosPinToSlice(pos, pin);
			int idx = pin.OutputSlices.IndexOf(slice);
			if (Above)
				pin.AddSlice(idx);
			else
				pin.AddSlice(idx+1);
			
			UpdateSliceMenu(pos);
			//this.Invalidate();
			//pin.UpdateKeyFrameAreas();
		}
		void AddAboveButtonClick(object sender, EventArgs e)
		{
			AddSlice(true);
		}
		
		void AddBelowButtonClick(object sender, EventArgs e)
		{
			AddSlice(false);
		}
		
		public void SelectAll(bool Select)
		{
			foreach (TLBasePin p in FOutputPins)
				foreach (TLSlice s in p.OutputSlices)
				foreach (TLBaseKeyFrame k in s.KeyFrames)
				if ((k.Selected && !Select) || (!k.Selected && Select))
			{
				k.Selected = Select;
				this.Invalidate(this.GetUpdateRegion(p, s, k));
			}
			if (!Select)
				FPinsWithSelectedKeyframes.Clear();
		}
		
		public void SelectAllOfPin()
		{
			Point pt = this.PointToClient(Control.MousePosition);
			TLBasePin p = PosToPin(pt);
			
			foreach (TLSlice s in p.OutputSlices)
				foreach (TLBaseKeyFrame k in s.KeyFrames)
				if (!k.Selected)
			{
				k.Selected = true;
				this.Invalidate(k.RedrawArea);
			}
		}
		
		public void SelectAllOfSlice()
		{
			Point pt = this.PointToClient(Control.MousePosition);
			TLBasePin p = PosToPin(pt);
			TLSlice s = PosPinToSlice(pt, p);
			
			foreach (TLBaseKeyFrame k in s.KeyFrames)
				if (!k.Selected)
			{
				k.Selected = true;
				this.Invalidate(k.RedrawArea);
			}
		}
		
		private Region GetUpdateRegion(TLBasePin p, TLSlice s, TLBaseKeyFrame k)
		{
			if (k is TLColorKeyFrame || k is TLValueKeyFrame || k is TLStateKeyFrame)
			{
				int idx = s.KeyFrames.IndexOf(k);
				float left = s.KeyFrames[Math.Max(0, idx-1)].GetTimeAsX();
				float right = Math.Min(s.KeyFrames[Math.Min(idx+1, s.KeyFrames.Count-1)].GetTimeAsX(), Width);
				if (idx == 0)
					left = 0;
				if (idx == s.KeyFrames.Count-1)
					right = Width;
				
				return new Region(new RectangleF(left-10, k.SliceTop, right-left+20, k.SliceHeight));
			}
			else
				return k.RedrawArea;
		}
		
		protected override void OnMouseDoubleClick (MouseEventArgs e)
		{
			switch(FMouseState)
			{
				case TLMouseState.msSelecting://not msIdle, because on mousedown state has already become msSelecting
					{
						TLBasePin bp = PosToPin(e.Location);
						TLBaseKeyFrame k = (bp as TLPin).AddKeyFrame(e.Location);
						TLSlice s = PosPinToSlice(e.Location, bp);
						
						if (k != null)
							this.Invalidate(GetUpdateRegion(bp, s, k));
						
						break;
					}
					
				case TLMouseState.msDragging://not msIdle, because on mousedown state has already become msDragging
					{
						Cursor.Show();	//was hidden on mousedown
						
						List<TLBaseKeyFrame> allkeys = new List<TLBaseKeyFrame>();
						TLBasePin tPin = PosToPin(e.Location);
						
						if (tPin is TLStringPin)
						{
							foreach (TLBasePin p in FOutputPins)
								if (p is TLStringPin)
								foreach (TLSlice s in p.OutputSlices)
							{
								List<TLBaseKeyFrame> keys = s.KeyFrames.FindAll(delegate(TLBaseKeyFrame k) {return k.Selected;});
								allkeys.AddRange(keys);
							}
						}
						else if (tPin is TLValuePin)
						{
							foreach (TLBasePin p in FOutputPins)
								if (p is TLValuePin)
								foreach (TLSlice s in p.OutputSlices)
							{
								List<TLBaseKeyFrame> keys = s.KeyFrames.FindAll(delegate(TLBaseKeyFrame k) {return k.Selected;});
								allkeys.AddRange(keys);
							}
						}
						else if (tPin is TLColorPin)
						{
							foreach (TLBasePin p in FOutputPins)
								if (p is TLColorPin)
								foreach (TLSlice s in p.OutputSlices)
							{
								List<TLBaseKeyFrame> keys = s.KeyFrames.FindAll(delegate(TLBaseKeyFrame k) {return k.Selected;});
								allkeys.AddRange(keys);
							}
						}
						else if (tPin is TLAutomataPin)
						{
							foreach (TLBasePin p in FOutputPins)
								if (p is TLAutomataPin)
								foreach (TLSlice s in p.OutputSlices)
							{
								List<TLBaseKeyFrame> keys = s.KeyFrames.FindAll(delegate(TLBaseKeyFrame k) {return k.Selected;});
								allkeys.AddRange(keys);
							}
						}
						
						
						if (allkeys.Count > 0)
						{
							if (tPin is TLStringPin)
								FEditor = new TLEditorString(allkeys);
							else if (tPin is TLValuePin)
								FEditor = new TLEditorValue(allkeys);
							else if (tPin is TLColorPin)
								FEditor = new TLEditorColor(allkeys);
							else if (tPin is TLAutomataPin)
								FEditor = new TLEditorState(allkeys);
							
							FEditor.OnExit += new ExitHandler(ExitEditor);
							FEditor.Location = new Point(e.X - FEditor.Width/2, e.Y - 5);
							FEditor.Show();
							this.Controls.Add(FEditor);
							this.ActiveControl = FEditor;
							this.Focus();
							
							FMouseState = TLMouseState.msEditing;
						}
						
						break;
					}
					
				case TLMouseState.msDraggingYOnly:
					{
						if (e.Button == MouseButtons.Right)
						{
							TLBasePin pin = PosToPin(e.Location);
							if (pin != null)
							{
								foreach(TLSlice s in pin.OutputSlices)
									for (int i=s.KeyFrames.Count-1; i>-1; i--)
									if (s.KeyFrames[i].HitByPoint(e.Location) && s.KeyFrames[i].Selected)
								{
									DeleteKeyFrame(pin, s, s.KeyFrames[i]);
								}
								
								pin.SaveKeyFrames();
							}
						}
						break;
					}
			}
		}

		protected override void OnMouseDown(MouseEventArgs e)
		{
			//call bases mousedown to select the control and make OnMousWheel event fire
			base.OnMouseDown(e);
			
			switch(FMouseState)
			{
				case TLMouseState.msIdle:
					{
						TLBasePin pin = PosToPin(e.Location);
						if (pin == null)
							break;
						
						if (e.Button == MouseButtons.Left && pin is TLRulerPin)
						{
							FTimer.CurrentTime = FTransformer.XPosToTime(e.X);
							FMouseState = TLMouseState.msDraggingTimeBar;
							break;
						}
						
						TLBaseKeyFrame kf = null;
						foreach (TLSlice s in pin.OutputSlices)
						{
							kf = s.KeyFrames.Find(delegate(TLBaseKeyFrame k) {return k.HitByPoint(e.Location);});
							if (kf != null)
								break; //out of foreach
						}
						
						if (kf == null) //mouse is not hovering a keyframe
						{
							if (e.Button == MouseButtons.Left && FTimeBar.IsVisible(e.Location))
							{
								FMouseState = TLMouseState.msDraggingTimeBar;
							}
							else if (e.Button == MouseButtons.Right) //and rightclicking -> start panning
							{
								FMouseState = TLMouseState.msPanning;
								this.Cursor = Cursors.NoMove2D;;
							}
							else if (e.Button == MouseButtons.Left)	//and leftclicking -> start marching ants
							{
								FMouseState = TLMouseState.msSelecting;
								FSelectionArea.Location = e.Location;
								
								if ((Control.ModifierKeys & Keys.Shift) != Keys.Shift && (Control.ModifierKeys & Keys.Alt) != Keys.Alt)
								{
									SelectAll(false);
								}
							}
						}
						else
						{
							Cursor.Hide();
							
							if (e.Button == MouseButtons.Left)	//mouse is hovering a keyframe -> go drag
							{
								FMouseState = TLMouseState.msDragging;
							}
							else if (e.Button == MouseButtons.Right)	//mouse is hovering a keyframe -> go drag Y only
							{
								FMouseState = TLMouseState.msDraggingYOnly;
							}
							else if (e.Button == MouseButtons.Middle)	//mouse is hovering a keyframe -> go drag Y only
							{
								FMouseState = TLMouseState.msDraggingXOnly;
							}
							
							if (!kf.Selected) //deselect all other keyframes and start dragging the one the mouse is over
							{
								SelectAll(false);
								kf.Selected = true;
								FPinsWithSelectedKeyframes.Add(pin);
								this.Invalidate(kf.RedrawArea);
							}
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
		
		private double GetNextTime(List<TLBaseKeyFrame> KeyFrames, TLBaseKeyFrame Current)
		{
			if ((Current is TLStringKeyFrame) || (Current is TLMidiKeyFrame))
				return double.MaxValue;
			
			TLBaseKeyFrame next = KeyFrames.Find(delegate(TLBaseKeyFrame kf) {return kf.Time > Current.Time;});
			if (next == null)
				return double.MaxValue;
			else
				return next.Time-0.0001;
		}
		
		private double GetPrevTime(List<TLBaseKeyFrame> KeyFrames, TLBaseKeyFrame Current)
		{
			if ((Current is TLStringKeyFrame) || (Current is TLMidiKeyFrame))
				return double.MinValue;
			
			TLBaseKeyFrame prev = KeyFrames.FindLast(delegate(TLBaseKeyFrame kf) {return kf.Time < Current.Time;});
			if (prev == null)
				return double.MinValue;
			else
				return prev.Time+0.0001;
		}
		
		protected override void OnMouseMove(MouseEventArgs e)
		{
			switch(FMouseState)
			{
				case TLMouseState.msIdle:
					{
						UpdateSliceMenu(e.Location);
						
						TLBaseKeyFrame kf = null;
						TLBasePin pin = PosToPin(e.Location);
						if (pin != null)
						{
							foreach (TLSlice s in pin.OutputSlices)
							{
								kf = s.KeyFrames.Find(delegate(TLBaseKeyFrame k) {return k.HitByPoint(e.Location);});
								if (kf != null)
									break; //out of foreach
							}
						}
						
						if (kf == null && FTimeBar.IsVisible(e.Location))
							this.Cursor = Cursors.VSplit;
						else
							this.Cursor = null;
						
						break;
					}
				case TLMouseState.msSelecting:
					{
						//invalidate last SelectionArea;
						Rectangle sr = StraightenRectangle(FSelectionArea);
						sr.Inflate(2,2);
						this.Invalidate(sr);
						
						//now set new selection area
						FSelectionArea.Size = new Size(e.X - FSelectionArea.X, e.Y - FSelectionArea.Y);
						sr = StraightenRectangle(FSelectionArea);
						
						//check if any of the keyframes is within the SelectionArea
						foreach (TLBasePin p in FOutputPins)
							foreach (TLSlice s in p.OutputSlices)
							foreach (TLBaseKeyFrame k in s.KeyFrames)
						{
							bool wasSelected = k.Selected;
							if ((Control.ModifierKeys & Keys.Shift) == Keys.Shift)
								k.Selected |= k.HitByRect(sr);
							else if ((Control.ModifierKeys & Keys.Alt) == Keys.Alt)
							{
								if (k.HitByRect(sr))
									k.Selected = false;
							}
							else
								k.SelectByRect(sr);
							
							if (wasSelected != k.Selected)
								this.Invalidate(k.RedrawArea);
						}
						
						//invalidate SelectionArea
						//this.Invalidate(sr);
						
						break;
					}
					
				case TLMouseState.msDraggingYOnly:
					{
						Region update = new Region(new Rectangle(0, 0, 1, 1));
						
						foreach (TLBasePin p in FOutputPins)
							foreach (TLSlice s in p.OutputSlices)
							foreach (TLBaseKeyFrame k in s.KeyFrames)
							if (k.Selected)
						{
							double delta = e.Y - FLastMousePoint.Y;
							
							if ((Control.ModifierKeys & Keys.Control) == Keys.Control)
								delta /= 10;
							
							if (k is TLColorKeyFrame)
							{
								TLColorKeyFrame kfc = k as TLColorKeyFrame;
								if ((Control.ModifierKeys & Keys.Shift) == Keys.Shift)
									kfc.MoveAlpha(delta);
								else if ((Control.ModifierKeys & Keys.Control) == Keys.Control)
									kfc.MoveSaturation(delta);
								else
								{
									kfc.MoveHue(e.X - FLastMousePoint.X);
									kfc.MoveValue(delta);
								}
							}
							else
							{
								if ((Control.ModifierKeys & Keys.Shift) == Keys.Shift)
									delta /= 10;
								
								update.Union(k.RedrawArea);
								k.MoveY(delta);
								update.Union(k.RedrawArea);
							}
							
							//invalidate an area comprising of the previous and the next keyframe
							update.Union(GetUpdateRegion(p, s, k));
						}
						
						this.Invalidate(update);
						FValuesChanged = true;
						break;
					}
					
				case TLMouseState.msDraggingXOnly:
					{
						Region update = new Region(new Rectangle(0, 0, 1, 1));
						
						foreach (TLBasePin p in FOutputPins)
							foreach (TLSlice s in p.OutputSlices)
							foreach (TLBaseKeyFrame k in s.KeyFrames)
							if (k.Selected)
						{
							update.Union(k.RedrawArea);
							
							double delta = e.Y - FLastMousePoint.Y;
							delta *= -1;
							if ((Control.ModifierKeys & Keys.Shift) == Keys.Shift)
								delta /= 10;
							if ((Control.ModifierKeys & Keys.Control) == Keys.Control)
								delta /= 10;
							
							k.MoveTime(delta, GetPrevTime(s.KeyFrames, k), GetNextTime(s.KeyFrames, k));
							
							update.Union(GetUpdateRegion(p, s, k));
						}
						
						this.Invalidate(update);
						FValuesChanged = true;
						break;
					}
					
				case TLMouseState.msDragging:
					{
						Region update = new Region(new Rectangle(0, 0, 1, 1));
						
						foreach (TLBasePin p in FOutputPins)
							foreach (TLSlice s in p.OutputSlices)
							foreach (TLBaseKeyFrame k in s.KeyFrames)
							if (k.Selected)
						{
							update.Union(k.RedrawArea);
							
							double deltaX = e.X - FLastMousePoint.X;
							double deltaY = e.Y - FLastMousePoint.Y;
							
							if ((Control.ModifierKeys & Keys.Shift) == Keys.Shift)
							{
								deltaX /= 10;
								deltaY /= 10;
							}
							if ((Control.ModifierKeys & Keys.Control) == Keys.Control)
							{
								deltaX /= 10;
								deltaY /= 10;
							}
							
							if (k is TLColorKeyFrame)
								deltaX /= 10;
							
							//may use a linked list of keyframes for faster access to neighbours?
							//TLBaseKeyFrame next = s.KeyFrames.Find(delegate(TLBaseKeyFrame kf) {return kf.Time > k.Time;});
							//TLBaseKeyFrame last = s.KeyFrames.FindLast(delegate(TLBaseKeyFrame kf) {return kf.Time < k.Time;});
							
							k.MoveTime(deltaX, GetPrevTime(s.KeyFrames, k), GetNextTime(s.KeyFrames, k));
							k.MoveY(deltaY);
							
							update.Union(GetUpdateRegion(p, s, k));
						}

						this.Invalidate(update);
						FValuesChanged = true;
						break;
					}
				case TLMouseState.msDraggingTimeBar:
					{
						FTimer.CurrentTime = FTransformer.XPosToTime(e.X);
						break;
					}
					
				case TLMouseState.msPanning:
					{
						FTransformer.TranslateTime(e.X - FLastMousePoint.X);
						
						double scale = 1;
						if(e.Y - FLastMousePoint.Y > 1)
							scale = 1.1f;
						else if (e.Y - FLastMousePoint.Y < -1)
							scale = 0.9f;
						
						FTransformer.ScaleTime(scale, e.X + TimelinerPlugin.FHeaderWidth);
						
						Invalidate();
						break;
					}
			}
			
			FLastMousePoint = e.Location;
		}

		protected override void OnMouseUp(MouseEventArgs e)
		{
			switch(FMouseState)
			{
				case TLMouseState.msSelecting:
					{
						//invalidate last SelectionArea;
						Rectangle r = StraightenRectangle(FSelectionArea);
						r.Inflate(2,2);
						this.Invalidate(r);
						
						FSelectionArea.Size = new Size(0, 0);
						FMouseState = TLMouseState.msIdle;
						
						bool pinAdded;
						foreach (TLBasePin bp in FOutputPins)
						{
							pinAdded = false;
							foreach (TLSlice s in bp.OutputSlices)
							{
								foreach (TLBaseKeyFrame kf in s.KeyFrames)
									if (kf.Selected)
								{
									FPinsWithSelectedKeyframes.Add(bp);
									pinAdded = true;
									break;
								}
								if (pinAdded)
									break;
							}
						}
						
						break;
					}
				case TLMouseState.msDragging:
					{
						FMouseState = TLMouseState.msIdle;
						
						SaveAllKeyFrames();
						
						Cursor.Show();
						break;
					}
				case TLMouseState.msDraggingYOnly:
					{
						FMouseState = TLMouseState.msIdle;
						
						SaveAllKeyFrames();
						
						Cursor.Show();
						break;
					}
				case TLMouseState.msDraggingXOnly:
					{
						FMouseState = TLMouseState.msIdle;
						
						SaveAllKeyFrames();
						
						Cursor.Show();
						break;
					}
				case TLMouseState.msPanning:
					{
						this.Cursor = null;
						FMouseState = TLMouseState.msIdle;
						break;
					}
					
				case TLMouseState.msDraggingTimeBar:
					{
						FMouseState = TLMouseState.msIdle;
						break;
					}
			}
		}
		
		void SliceMenuLeave(object sender, EventArgs e)
		{
			if (this.PointToClient(Cursor.Position).X < SliceMenuPanel.Width)
				SliceMenuPanel.Hide();
		}
	}
}
