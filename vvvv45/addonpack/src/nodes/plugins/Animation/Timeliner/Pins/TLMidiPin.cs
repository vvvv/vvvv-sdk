using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Xml;

using VVVV.Utils.MidiScore;


using VVVV.PluginInterfaces.V1;

namespace VVVV.Nodes.Timeliner
{
	public partial class TLMidiPin: TLPin
	{
		// FIELDS
		///////////////////////

		private bool FUpdateOutputs = false;
		private string FFilename = "E:\\bwv1087.mid";

		private OpenFileDialog FOpenDialog;
		
		// PINS
		///////////////////////
		/// 
		private IValueOut FValueOut;
		//private IStringIO FFilename;
	/*	private IValueIO FTrackedNoteID;
		private IValueIO FVOut;
		private IValueIO FRhythm;
		private IValueIO FBpm;
		private IValueIO FChannelOut;
		private IValueIO FNoteOut;
		private IValueIO FVelocityOut;
		private IValueIO FStartOut;
		private IValueIO FEndOut;
		private IValueIO FNoteRange;
		private IValueIO FBar;
		*/
		// CONTROLS
		///////////////////////

		private TMidiScore FMidiScore;
		
		public TLMidiPin(IPluginHost Host, TLTransformer Transformer, int Order, XmlNode PinSettings):base(Host, Transformer, Order, PinSettings, true, true)
		{
			InitializeComponent();
			
			//only load slices after all super constructors have been executed:
			XmlAttribute attr = PinSettings.Attributes.GetNamedItem("SliceCount") as XmlAttribute;
        	LoadSlices(int.Parse(attr.Value));
        	
        	try
        	{
        		attr = PinSettings.Attributes.GetNamedItem("Filename") as XmlAttribute;
				FFilename = attr.Value;
        	}
        	catch
        	{
        		FFilename = "E:\\988-aria.mid";
        	}
        	
        	try
        	{
        		attr = PinSettings.Attributes.GetNamedItem("MinNote") as XmlAttribute;
				MinNote.Value = double.Parse(attr.Value, TimelinerPlugin.GNumberFormat);
        	}
        	catch
        	{
        		MinNote.Value = 30;
        	}
        	
        	try
        	{
        		attr = PinSettings.Attributes.GetNamedItem("MaxNote") as XmlAttribute;
				MaxNote.Value = double.Parse(attr.Value, TimelinerPlugin.GNumberFormat);
        	}
        	catch
        	{
        		MaxNote.Value = 97;
        	}
        	
        	//bpm
        	
		 	UpdateSliceSpecificSettings();

		}

		public override void UpdateSliceSpecificSettings()
		{
			base.UpdateSliceSpecificSettings();
			//UpdateMinMax();	
        	//UpdateInterpolationInState();
		}
		
		protected override void CreatePins()
		{
			base.CreatePins();
		
			FHost.CreateValueOutput(Name, 1, null, TSliceMode.Dynamic, TPinVisibility.True, out FValueOut);
	    	FValueOut.SliceCount = 128;
	    	FValueOut.Order = Order;
			FValueOut.SetSubType(0, 1, 1/127, 0, false, false, false);
			
			
						
		/*	FHost.CreateStringPin("Filename", TMPinDirection.cmpdInput, TMSliceMode.cmsmSingle, TMPinVisibility.pivTrue, out FFilename);
        	FFilename.Order=Order;
			FFilename._SetSubType("default", true);
			
			FHost.CreateValuePin("Rhythm", true, 2, null, TMPinDirection.cmpdInput, TMSliceMode.cmsmManual, TMPinVisibility.pivTrue, out FRhythm);
	    	FRhythm.SliceCount=1;
	    	FRhythm.Order=Order;
			FRhythm._SetSubType(1, 64, 1, 4, false, false, true);
			
			FHost.CreateValuePin("BPM", true, 1, null, TMPinDirection.cmpdInput, TMSliceMode.cmsmManual, TMPinVisibility.pivTrue, out FBpm);
	    	FBpm.SliceCount=1;
	    	FBpm.Order=Order;
			FBpm._SetSubType(20, 999, 1, 120, false, false, true);
			
			FHost.CreateValuePin("Note Range", true, 2, null, TMPinDirection.cmpdInput, TMSliceMode.cmsmManual, TMPinVisibility.pivTrue, out FNoteRange);
	    	FNoteRange.SliceCount=1;
	    	FNoteRange.Order=Order;
			FNoteRange._SetSubType(0, 127, 1, 0, false, false, true);
			
			FHost.CreateValuePin("Bar", true, 2, null, TMPinDirection.cmpdInput, TMSliceMode.cmsmManual, TMPinVisibility.pivTrue, out FBar);
	    	FBar.SliceCount=1;
	    	FBar.Order=Order;
			FBar._SetSubType(0, Int32.MaxValue, 1, 0, false, false, true);
			
			
			
			FHost.CreateValuePin(Name + " Channel", false, 1, null, TMPinDirection.cmpdOutput, TMSliceMode.cmsmManual, TMPinVisibility.pivTrue, out FChannelOut);
	    	FChannelOut.SliceCount=1;
	    	FChannelOut.Order=Order;
			FChannelOut._SetSubType(0, 127, 1, 0, false, false, true);
			
			FHost.CreateValuePin(Name + " Note", false, 1, null, TMPinDirection.cmpdOutput, TMSliceMode.cmsmManual, TMPinVisibility.pivTrue, out FNoteOut);
	    	FNoteOut.SliceCount=1;
	    	FNoteOut.Order=Order;
			FNoteOut._SetSubType(0, 1, 1, 0, false, false, false);
			
			FHost.CreateValuePin(Name + " Velocity", false, 1, null, TMPinDirection.cmpdOutput, TMSliceMode.cmsmManual, TMPinVisibility.pivTrue, out FVelocityOut);
	    	FVelocityOut.SliceCount=1;
	    	FVelocityOut.Order=Order;
			FVelocityOut._SetSubType(0, 1, 1, 0, false, false, false);
			
			FHost.CreateValuePin(Name + " Start", false, 1, null, TMPinDirection.cmpdOutput, TMSliceMode.cmsmManual, TMPinVisibility.pivTrue, out FStartOut);
	    	FStartOut.SliceCount=1;
	    	FStartOut.Order=Order;
			FStartOut._SetSubType(0, Int32.MaxValue, 1, 0, false, false, true);
			
			FHost.CreateValuePin(Name + " End", false, 1, null, TMPinDirection.cmpdOutput, TMSliceMode.cmsmManual, TMPinVisibility.pivTrue, out FEndOut);
	    	FEndOut.SliceCount=1;
	    	FEndOut.Order=Order;
			FEndOut._SetSubType(0, Int32.MaxValue, 1, 0, false, false, true);
			
			FHost.CreateValuePin(Name + " TrackedNoteID", false, 1, null, TMPinDirection.cmpdOutput, TMSliceMode.cmsmManual, TMPinVisibility.pivTrue, out FTrackedNoteID);
	    	FTrackedNoteID.SliceCount=128;
	    	FTrackedNoteID.Order=Order;
			FTrackedNoteID._SetSubType(0, Int32.MaxValue, 1, 0, false, false, true);
			*/
		}
		
		void FilenameLabelClick(object sender, EventArgs e)
		{
			OpenFileDialog ofd = new OpenFileDialog();
			ofd.ShowDialog();
			
			
			FMidiScore = new TMidiScore();
			FMidiScore.OnNotesParsed += NotesParsed;
			FMidiScore.SetFilename(ofd.FileName);
			FilenameLabel.Text = System.IO.Path.GetFileName(ofd.FileName);
			
		}
		
		private void MinMaxIOChangedCB(double NewValue)
		{
			UpdateMinMax();
			PinChanged();
		}
		
		private void UpdateMinMax()
		{
			foreach (TLMidiSlice vs in FOutputSlices)
			{
				vs.MinNote = (int) MinNote.Value;
				vs.MaxNote = (int) MaxNote.Value;
			}			
		}
		
		protected override void InitializeHeight()
		{
			FMinimalHeight = 110;
			
			if (FUncollapsedHeight == 0)
				FUncollapsedHeight = 150;
			
			base.InitializeHeight();
		}
		
		protected override XmlNode GetSettings()
		{
			XmlNode pin = base.GetSettings();
			
			XmlAttribute attr = FSettings.CreateAttribute("Filename");
			attr.Value = FFilename;
    		pin.Attributes.Append(attr);
    		
    		//bpm
    		/*
    		attr = FSettings.CreateAttribute("Minimum");
    		string tmp = String.Format("{0:F4}", FMinIO.Value);
			attr.Value = tmp.Replace(',', '.');
    		pin.Attributes.Append(attr);
    		
    		attr = FSettings.CreateAttribute("Maximum");
			tmp = String.Format("{0:F4}", FMaxIO.Value);
			attr.Value = tmp.Replace(',', '.');
    		pin.Attributes.Append(attr);
    		*/
    		return pin;				
		}
		
		public override void DestroyPins()
		{
	
			// DELETE ALL PINS
			///////////////////////
			/// 
			
			FHost.DeletePin(FValueOut);
			FValueOut = null;
			
		/*	FHost.DeletePin(FFilename);
			FFilename = null;
			
			FHost.DeletePin(FRhythm);
			FRhythm = null;
			
			FHost.DeletePin(FBpm);
			FBpm = null;
			
			FHost.DeletePin(FChannelOut);
			FChannelOut = null;
			
			FHost.DeletePin(FNoteOut);
			FNoteOut = null;
			
			FHost.DeletePin(FVelocityOut);
			FVelocityOut = null;
		
			FHost.DeletePin(FStartOut);
			FStartOut = null;
			
			FHost.DeletePin(FEndOut);
			FEndOut = null;
			
			FHost.DeletePin(FNoteRange);
			FNoteRange = null;
			
			FHost.DeletePin(FBar);
			FBar = null;
			
			FHost.DeletePin(FVOut);
			FVOut = null;
			
			FHost.DeletePin(FTrackedNoteID);
			FTrackedNoteID = null;						
			
			//delete midiscore
			FMidiScore = null;*/
			
			base.DestroyPins();
		}
		
		protected override void LoadSlices(int SavedSliceCount)
		{
			for(int i=0; i<SavedSliceCount; i++)
				AddSlice(FOutputSlices.Count);
			
			FValueOut.SliceCount = SavedSliceCount;
		}
		
		public override void RemoveSlice(TLSlice Slice)
		{
			base.RemoveSlice(Slice);
			FValueOut.SliceCount = FOutputSlices.Count;
		}
		
		public override void AddSlice(int At)
		{
			TLMidiSlice sm = new TLMidiSlice(FHost, this, FOutputSlices.Count, FOrder);
		
			AddSlice(At, sm);
			
			FValueOut.SliceCount = FOutputSlices.Count;
		}

		
		public override void Evaluate(double CurrentTime)
		{
			base.Evaluate(CurrentTime);
		
		/*
			for (int i=0; i<FOutputSlices.Count; i++)
				FValueOut.SetValue(i, (FOutputSlices[i] as TLValueSlice).Output);	
		
			
			bool needsrepaint = false;
			
			if (FRhythm.PinIsChanged)
			{
				double z, n;
				FRhythm.GetValue2D(0, out z, out n);
				FMidiScore.Nenner = (int) n;
				FMidiScore.Zähler = (int) z;
				
				FMidiScore.ComputeBarLength();
				
				needsrepaint = true;
			}
			
			if (FBpm.PinIsChanged)
			{
				double b;
				FBpm.GetValue(0, out b);
				FMidiScore.Bpm = (int) b;
				
				needsrepaint = true;
			}
			
			if (FNoteRange.PinIsChanged)
			{
				double min, max;
				FNoteRange.GetValue2D(0, out min, out max);
				if (min == max)
					max +=1;
				
				FMidiScore.MinNote = (int) min;
				FMidiScore.MaxNote = (int) max;
				
				needsrepaint = true;
			}
			
			if (FBar.PinIsChanged)
			{
				double b1, b2;
				FBar.GetValue2D(0, out b1, out b2);
				FMidiScore.BarFrom = (int) b1;
				FMidiScore.BarTo = (int) b2;
				
				needsrepaint = true;
								
				FUpdateOutputs = true;
			}
			
			if (FFilename.PinIsChanged)
			{
				string fn;
				FFilename.GetString(0, out fn);
				FMidiScore.SetFilename(fn);
			}

			if (needsrepaint)
				this.Refresh();
			
			//set outputs
			if (FUpdateOutputs)
			{
				UpdateOutputs();
				FUpdateOutputs = false;				
			}
			
			if (TLMain.GTime.IsRunning)
			{
				//current midi unit: cmu
				float cmu = (float) (TLMain.GTime.CurrentTime * FMidiScore.MUpT);
				List <MidiNote> activeNotes = FMidiScore.FMidiNotes.FindAll(delegate (MidiNote mn) {return mn.Start <= cmu && cmu < mn.Start + mn.Duration;});
				
				for (int i = 0; i<128; i++)
				{
					MidiNote m = activeNotes.Find(delegate (MidiNote mn) {return mn.Note == i;});
					if (m == null)
						FVOut.SetValue(i, 0);
					else
						FVOut.SetValue(i, m.Velocity);
				}
			}
			else //output current notes of scorefollower
			{
				for (int i = 0; i<128; i++)
				{
					MidiNote m = FMidiScore.FNotesFound.Find(delegate (MidiNote mn) {return mn.Note == i;});
					if (m == null)
					{
						FVOut.SetValue(i, -1);
						FTrackedNoteID.SetValue(i, -1);
					}
					else
					{
						FVOut.SetValue(i, m.Channel);
						int id = FMidiScore.FMidiNotes.IndexOf(m);
						FTrackedNoteID.SetValue(i, id);
					}
				}
			}*/
		}
		
		private void NotesParsed()
		{
			List<TMidiNote> notes;
			FOutputSlices.Clear();
			for (int i=0; i<16; i++)
			{
				notes = FMidiScore.GetNotesOfChannel(i);
				if (notes.Count > 0)
				{
					AddSlice(FOutputSlices.Count);
					(FOutputSlices[FOutputSlices.Count-1] as TLMidiSlice).InitializeNotes(FMidiScore.GetNotesOfChannel(i));
					(FOutputSlices[FOutputSlices.Count-1] as TLMidiSlice).TrackName = FMidiScore.GetTrackName(i);
					(FOutputSlices[FOutputSlices.Count-1] as TLMidiSlice).BPM = 120;
					(FOutputSlices[FOutputSlices.Count-1] as TLMidiSlice).Division = FMidiScore.Division;
					(FOutputSlices[FOutputSlices.Count-1] as TLMidiSlice).MinNote = 60;
					(FOutputSlices[FOutputSlices.Count-1] as TLMidiSlice).MaxNote = 100;
					(FOutputSlices[FOutputSlices.Count-1] as TLMidiSlice).TrackLength = FMidiScore.Length;
				}				
			}
			
			FUpdateOutputs = true;
			
			this.Refresh();
		}
		
		private void UpdateOutputs()
		{
		/*	int sc = FMidiScore.FNotesOfBar.Count;
			FChannelOut.SliceCount = sc;
			FNoteOut.SliceCount = sc;
			FVelocityOut.SliceCount=sc;
			FStartOut.SliceCount=sc;
			FEndOut.SliceCount=sc; 
			
			int i = 0;
			foreach (MidiNote mn in FMidiScore.FNotesOfBar)
			{
				FChannelOut.SetValue(i, mn.Channel);
				FNoteOut.SetValue(i, mn.Note / 127.0);
				FVelocityOut.SetValue(i, mn.Velocity / 127.0);
				FStartOut.SetValue(i, mn.Start);
				FEndOut.SetValue(i, mn.End);
				i++;
			}*/
		}

		
		void RhythmOnValueChange(double Value)
		{
			foreach (TLMidiSlice ms in FOutputSlices)
			{
				ms.Enumerator = (int) Enumerator.Value;
				ms.Denominator = (int) Denominator.Value;
			}	
		}
	}
}
