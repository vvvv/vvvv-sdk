using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Xml;

using VVVV.Utils.VMidiScore;


using VVVV.PluginInterfaces.V1;

namespace VVVV.Nodes.Timeliner
{
	public partial class TLMidiPin: TLPin
	{
		// FIELDS
		///////////////////////

		private TMidiScore FMidiScore;
		public TMidiScore MidiScore
		{
			get{return FMidiScore;}
		}
		
		// PINS
		///////////////////////
		/// 
		private IValueOut FChannelOut;
		private IValueOut FVelocityOut;
		
		private IStringConfig FFilenameIn;
		// CONTROLS
		///////////////////////

		
		
		public TLMidiPin(IPluginHost Host, TLTransformer Transformer, int Order, XmlNode PinSettings):base(Host, Transformer, Order, PinSettings, true, true)
		{
			InitializeComponent();
			
			//only load slices after all super constructors have been executed:
			XmlAttribute attr = PinSettings.Attributes.GetNamedItem("SliceCount") as XmlAttribute;
			LoadSlices(int.Parse(attr.Value));
			
			try
			{
				attr = PinSettings.Attributes.GetNamedItem("MinNote") as XmlAttribute;
				MinNote.Value = double.Parse(attr.Value, TimelinerPlugin.GNumberFormat);
			}
			catch
			{}
			
			try
			{
				attr = PinSettings.Attributes.GetNamedItem("MaxNote") as XmlAttribute;
				MaxNote.Value = double.Parse(attr.Value, TimelinerPlugin.GNumberFormat);
			}
			catch
			{}
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
			
			FHost.CreateStringConfig(Name + "-Filename", TSliceMode.Single, TPinVisibility.True, out FFilenameIn);
			FFilenameIn.Order=Order;
			FFilenameIn.SetSubType("*.mid", true);
			
			FHost.CreateValueOutput(Name + "-Channel", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out FChannelOut);
			FChannelOut.SliceCount = 128;
			FChannelOut.Order = Order;
			FChannelOut.SetSubType(0, 15, 1, 0, false, false, true);
			
			FHost.CreateValueOutput(Name + "-Velocity", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out FVelocityOut);
			FVelocityOut.SliceCount = 128;
			FVelocityOut.Order = Order;
			FVelocityOut.SetSubType(0, 1, 1/128, 0, false, false, false);
		}
		
		protected override void PinNameChanged()
		{
			base.PinNameChanged();
			
			FFilenameIn.Name = Name + "-Filename";
			FChannelOut.Name = Name + "-Channel";
			FVelocityOut.Name = Name + "-Velocity";
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
			FMinimalHeight = DIP(110);
			
			if (FUncollapsedHeight == 0)
				FUncollapsedHeight = DIP(150);
			
			base.InitializeHeight();
		}
		
		protected override XmlNode GetSettings()
		{
			XmlNode pin = base.GetSettings();

			XmlAttribute attr = FSettings.CreateAttribute("MinNote");
			string tmp = String.Format("{0:F0}", MinNote.Value);
			attr.Value = tmp.Replace(',', '.');
			pin.Attributes.Append(attr);
			
			attr = FSettings.CreateAttribute("MaxNote");
			tmp = String.Format("{0:F0}", MaxNote.Value);
			attr.Value = tmp.Replace(',', '.');
			pin.Attributes.Append(attr);

			return pin;
		}
		
		public override void DestroyPins()
		{
			
			// DELETE ALL PINS
			///////////////////////
			/// 
			
			FHost.DeletePin(FFilenameIn);
			FFilenameIn = null;
			
			FHost.DeletePin(FChannelOut);
			FChannelOut = null;
			
			FHost.DeletePin(FVelocityOut);
			FVelocityOut = null;
			
			//delete midiscore
			FMidiScore = null;
			
			base.DestroyPins();
		}
		
		protected override void LoadSlices(int SavedSliceCount)
		{
			for(int i=0; i<SavedSliceCount; i++)
				AddSlice(FOutputSlices.Count);
		}
		
		public override void RemoveSlice(TLSlice Slice)
		{
			base.RemoveSlice(Slice);
		}
		
		public override void AddSlice(int At)
		{
			TLMidiSlice sm = new TLMidiSlice(FHost, this, FOutputSlices.Count, FOrder);
			
			AddSlice(At, sm);
		}
		
		void FilenameLabelClick(object sender, EventArgs e)
		{
			OpenFileDialog ofd = new OpenFileDialog();
			ofd.ShowDialog();
			FFilenameIn.SetString(0, ofd.FileName);
		}
		
		void SetMidiFile(string Filename)
		{
			if (System.IO.File.Exists(Filename))
			{
				FMidiScore = new TMidiScore();
				FMidiScore.OnNotesParsed += NotesParsed;
				FMidiScore.SetFilename(Filename);
				FilenameLabel.Text = System.IO.Path.GetFileName(Filename);
			}
			else
				FHost.Log(TLogType.Warning, " \"" + Filename + "\" does not exist!");
		}
		
		public override void Configurate(IPluginConfig Input, bool FirstFrame)
		{
			if (Input == FFilenameIn)
			{
				string fn;
				FFilenameIn.GetString(0, out fn);
				SetMidiFile(fn);
			}
		}
		
		public override void Evaluate(double CurrentTime)
		{
			base.Evaluate(CurrentTime);
			
			if (FMidiScore == null)
				return;
			
			List <TLBaseKeyFrame> activeNotes = new List<TLBaseKeyFrame>();
			foreach (TLMidiSlice ms in FOutputSlices)
				activeNotes.AddRange(ms.KeyFrames.FindAll(delegate (TLBaseKeyFrame mk) {return mk.Time <= CurrentTime && CurrentTime < (mk as TLMidiKeyFrame).End;}));
			
			for (int i = 0; i<128; i++)
			{
				TLMidiKeyFrame m = activeNotes.Find(delegate (TLBaseKeyFrame mk) {return (mk as TLMidiKeyFrame).Note == i;}) as TLMidiKeyFrame;
				if (m == null)
				{
					FChannelOut.SetValue(i, 0);
					FVelocityOut.SetValue(i, 0);
				}
				else
				{
					FChannelOut.SetValue(i, m.Channel);
					FVelocityOut.SetValue(i, m.Velocity);
				}
			}
		}
		
		void NotesParsed()
		{
			List<TMidiNote> notes;
			FOutputSlices.Clear();
			for (int i=0; i<FMidiScore.TrackCount; i++)
			{
				notes = FMidiScore.GetNotesOfTrack(i);
				if (notes.Count > 0)
				{
					AddSlice(FOutputSlices.Count);
					(FOutputSlices[FOutputSlices.Count-1] as TLMidiSlice).InitializeNotes(FMidiScore.GetNotesOfTrack(i));
					(FOutputSlices[FOutputSlices.Count-1] as TLMidiSlice).TrackName = FMidiScore.GetTrackName(i);
					(FOutputSlices[FOutputSlices.Count-1] as TLMidiSlice).MinNote = (int) MinNote.Value;
					(FOutputSlices[FOutputSlices.Count-1] as TLMidiSlice).MaxNote = (int) MaxNote.Value;
				}
			}
			
			Numerator.Value = FMidiScore.TimeSignature.Numerator;
			Denominator.Value = FMidiScore.TimeSignature.Denominator;
			BPM.Value = FMidiScore.BPM;
			
			this.Refresh();
		}
		
		void TimeSignatureChange(double Value)
		{
			if (FMidiScore != null)
			{
				TTimeSignature ts = new TTimeSignature();
				ts.Numerator = (byte) Numerator.Value;
				ts.Denominator = (byte) Denominator.Value;
				FMidiScore.TimeSignature = ts;
				
				FMidiScore.BPM = (int) BPM.Value;
				
				this.Refresh();
			}
		}
		
		void SaveButtonClick(object sender, EventArgs e)
		{
			//clear current score
			FMidiScore.Clear();
			
			//add metadata
			FMidiScore.AddMetaData();
			
			//move all data from keyframes back to miditracks;
			foreach (TLMidiSlice ms in FOutputSlices)
				FMidiScore.AddTrack(ms.TrackName, ms.KeyFrames);

			FMidiScore.SaveFile();
		}
	}
}
