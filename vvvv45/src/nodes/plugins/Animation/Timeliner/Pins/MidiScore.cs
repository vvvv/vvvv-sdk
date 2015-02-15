using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

using Sanford.Multimedia.Midi;

using VVVV.Nodes.Timeliner;



namespace VVVV.Utils.VMidiScore
{
	public delegate void NotesParsedHandler();
	

	public struct TTimeSignature
	{
		public byte Numerator;
		public byte Denominator;
		public byte MetronomePulse;
		public byte NumberOf32nds;
	}

	public class TMidiScore
	{
		public static readonly Color[] KeyColor = new Color[] {Color.White, Color.Black, Color.White, Color.Black, Color.White, Color.White, Color.Black, Color.White, Color.Black, Color.White, Color.Black, Color.White};
		
		private Sanford.Multimedia.Midi.Sequence FSequence = new Sequence();
		public Sanford.Multimedia.Midi.Sequence Sequence
		{
			get { return FSequence; }
		}
		
		public event NotesParsedHandler OnNotesParsed;
		private bool FLoading = false;
		
		private string FFilename;
		
		public List<TMidiNote> FMidiNotes = new List<TMidiNote>();
		
		private List<TMidiNote> FCurrentMidiNotes = new List<TMidiNote>();
		public List<TMidiNote> FOpenMidiNotes = new List<TMidiNote>();
		public List<TMidiNote> FNotesOfBar = new List<TMidiNote>();
		public List<TMidiNote> FNotesOfWindow = new List<TMidiNote>();
		public List<TMidiNote> FNotesFound = new List<TMidiNote>();
 		
		public int TrackCount
		{
			get{return FSequence.Count;}
		}
		
		private int FDivision;
		public int Division
		{
			get{return FDivision;}
		}
		
		private int FBPM = 120;
		public int BPM
		{
			get{return FBPM;}
			set
			{
				FBPM = value;
				UpdateTimeScaling();
			}
		}
		
		public int Length
		{
			get{return FSequence.GetLength();}
		}
		
		private float FSecondsPerMidiUnit;
		public float SecondsPerMidiUnit
		{
			get{return FSecondsPerMidiUnit;}
		}
		
		private float FSecondsPerBeat;
		public float SecondsPerBeat
		{
			get{return FSecondsPerBeat;}
		}
		
		private float FSecondsPerBar;
		public float SecondsPerBar
		{
			get{return FSecondsPerBar;}
		}
		
		private int FBarCount;
		public int BarCount
		{
			get{return FBarCount;}
		}
		
		private int FBeatCount;
		public int BeatCount
		{
			get{return FBeatCount;}
		}
		
		private TTimeSignature FTimeSignature;
		public TTimeSignature TimeSignature
		{
			get{return FTimeSignature;}
			set
			{
				FTimeSignature = value;
				UpdateTimeScaling();
			}
		}
		
		private string[] FTrackNames;

		/*		private int FRhythmChangedAtBar = 0;
		private int FRhythmChangedOffset = 0;
		private float FRhythmChangedOffsetInPixel = 0;
		private int FWindowFromNote;
		private int FWindowFromNoteOld;
		private int FWindowMaxNotes;
		private int FWindowToNote;
		
		private int FBarFrom;
		private int FBarTo;
		private int FPosition = 0;
		private int FExternalBar = 0;
		
		private int FBarLength = 0;
		private float FMUpT;
		private float Fppmu;
		private float FOffset;*/
		
		public TMidiScore()
		{
			FSequence.LoadCompleted += HandleLoadCompleted;
			
			FTimeSignature = new TTimeSignature();
			FTimeSignature.Denominator = 4;
			FTimeSignature.Numerator = 4;
			FTimeSignature.MetronomePulse = 24;
			FTimeSignature.NumberOf32nds = 8;
		}
		
		private void UpdateTimeScaling()
		{
			int MUpWholeNote = FDivision * 4;
			int MUpBeat = MUpWholeNote / FTimeSignature.Denominator;
			int MUpBar = FTimeSignature.Numerator * MUpBeat;
			FBarCount = Length / MUpBar;
			FBeatCount = FBarCount * FTimeSignature.Numerator;
			float duration = FBeatCount / (float)(FBPM) * 60;
			FSecondsPerBar = duration / FBarCount;
			FSecondsPerBeat = FSecondsPerBar / (float) FTimeSignature.Numerator;
			FSecondsPerMidiUnit = FSecondsPerBeat / (float) FDivision;
		}
		
		public void SetFilename(string Filename)
		{
			try
			{
				if ((!FLoading) && (System.IO.File.Exists(Filename)) && (System.IO.Path.GetExtension(Filename) == ".mid"))
				{
					FSequence.Load(Filename);
					FFilename = Filename;
					
					ParseSequence();
					OnNotesParsed();
					
					FLoading = true;
				}
			}
			catch(Exception e)
			{
				MessageBox.Show(e.ToString());
			};
		}
		
		public void SaveFile()
		{
			FSequence.Save(FFilename);
		}
		
		public List<TMidiNote> GetNotesOfTrack(int track)
		{
			return FMidiNotes.FindAll(delegate (TMidiNote mn) {return mn.Track == track;});
		}
		
		public string GetTrackName(int track)
		{
			return FTrackNames[track];
		}
		
		private void HandleLoadCompleted(object sender, AsyncCompletedEventArgs e)
		{
			//ClearPlayedNotes();
			ParseSequence();
			//ComputeBarLength();
			OnNotesParsed();
			
			FLoading = false;
		}
		/*
		public int Position
		{
			get{return FPosition;}
			set{FPosition = value;}
		}
		
		public int MaxWindowNotes
		{
			get{return FWindowMaxNotes;}
			set{FWindowMaxNotes = value;}
		}
		
		public float MUpT
		{
			get{return FMUpT;}
			set{FMUpT = value;}
		}

		public int ExternalBar
		{
			set
			{
						if (value == 0)
				{
					FRhythmChangedAtBar = 0;
					FRhythmChangedOffset = 0;
					FRhythmChangedOffsetInPixel = 0;
				}
				if (value < FExternalBar)
				{
					FExternalBar = value;
					
					int min = FRhythmChangedOffset + FBarLength * (FExternalBar-1-FRhythmChangedAtBar);
					MidiNote m = FMidiNotes.Find(delegate (MidiNote mn) {return mn.Start >= (min);});
					
					FWindowFromNote = FMidiNotes.IndexOf(m);
					FWindowToNote = FWindowFromNote + FWindowMaxNotes;
				}
				else
				{
					FExternalBar = value;
					
					//first note of this bar
					int min = FRhythmChangedOffset + FBarLength * (FExternalBar-1-FRhythmChangedAtBar);
					MidiNote m = FMidiNotes.Find(delegate (MidiNote mn) {return mn.Start >= (min);});
				    
					//open the window 4 notes to the left (in case the bar was fwdd a bit too early)
					FWindowFromNote = Math.Max(FWindowFromNote, FMidiNotes.IndexOf(m) - 4);
					
					//open the window to the right
					FWindowToNote = Math.Min(FWindowToNote, FWindowFromNote + FWindowMaxNotes);
					
					//make sure the window is at least open 5 notes into the new bar
					FWindowToNote = Math.Max(FWindowFromNote + 4 + 5, FWindowToNote);
					
					//clamp at max index
					FWindowToNote = Math.Min(FWindowToNote, FMidiNotes.Count);
				}
				
				//System.Diagnostics.Debug.WriteLine("external bar: from: " + FWindowFromNote.ToString() + " to: " + FWindowToNote.ToString());
			}
		}
		
		public int BarFrom
		{
			get{return FBarFrom;}
			set
			{
				FBarFrom = value;
				
				UpdateNotesOfBar();
			}
		}
		
		public int BarTo
		{
			get{return FBarTo;}
			set
			{
				FBarTo = value;
				
				UpdateNotesOfBar();
			}
		}
		
		public void SaveLastWindowStart()
		{
			FWindowFromNoteOld = FWindowFromNote;
		}
		
		private void UpdateNotesOfBar()
		{
			FNotesOfBar.Clear();
			if ((FBarFrom == 0) && (FBarTo == 0))
				FNotesOfBar = new List<TMidiNote>(FMidiNotes.ToArray());
			else
			{
				int min = (FBarFrom-1) * FBarLength;
				int max = (FBarTo) * FBarLength;
				foreach (TMidiNote mn in FMidiNotes)
				{
					if ((mn.Start >= min) && (mn.Start < max))
						FNotesOfBar.Add(mn);
				}
			}
		}*/
		
		public void Clear()
		{
			FSequence.Clear();
		}
		
		public void AddMetaData()
		{
			//create metadata track
			Track t = new Track();
			
			byte[] tempo = new byte[3];
			int ms_per_min = 60000000;
			int ms_per_quarternote = ms_per_min / FBPM;
			tempo[0] = (byte) ((ms_per_quarternote >> 16) & byte.MaxValue);
			tempo[1] = (byte) ((ms_per_quarternote >> 8) & byte.MaxValue);
			tempo[2] = (byte) (ms_per_quarternote & byte.MaxValue);
			TempoChangeBuilder tcBuilder = new TempoChangeBuilder(new MetaMessage(MetaType.Tempo, tempo));
			tcBuilder.Build();
			t.Insert(1, tcBuilder.Result);
			
			byte[] timesignature = new byte[4];
			timesignature[0] = FTimeSignature.Numerator;
			timesignature[1] = (byte) Math.Log(FTimeSignature.Denominator, 2);
			timesignature[2] = FTimeSignature.MetronomePulse;
			timesignature[3] = FTimeSignature.NumberOf32nds;
			TimeSignatureBuilder tsBuilder = new TimeSignatureBuilder(new MetaMessage(MetaType.TimeSignature, timesignature));
			tsBuilder.Build();
			t.Insert(2, tsBuilder.Result);
			
			FSequence.Add(t);
		}
		
		public void AddTrack(string TrackName, List<TLBaseKeyFrame> KeyFrames)
		{
			Track t = new Track();
			
			MetaTextBuilder mtBuilder = new MetaTextBuilder(MetaType.TrackName, TrackName);
			mtBuilder.Build();
			t.Insert(0, mtBuilder.Result);

			foreach (TLMidiKeyFrame mk in KeyFrames)
			{
				ChannelMessageBuilder cmBuilder = new ChannelMessageBuilder();
				cmBuilder.Command = ChannelCommand.NoteOn;
				cmBuilder.MidiChannel = mk.Channel;
				cmBuilder.Data1 = mk.Note;
				cmBuilder.Data2 = mk.Velocity;
				cmBuilder.Build();
				t.Insert((int) (mk.Time / SecondsPerMidiUnit), cmBuilder.Result);
				
				cmBuilder.Command = ChannelCommand.NoteOff;
				cmBuilder.Data1 = mk.Note;
				cmBuilder.Data2 = 0;
				cmBuilder.Build();
				t.Insert((int) (mk.End / SecondsPerMidiUnit), cmBuilder.Result);
			}
			
			FSequence.Add(t);
		}
		
		private void ParseSequence()
		{
			FOpenMidiNotes.Clear();
			FMidiNotes.Clear();

			FTrackNames = new string[FSequence.Count];
			
			FDivision = FSequence.Division;
			
			bool tempoSet = false;
			
			int trackID = 0;
			foreach (Track tTrack in FSequence)
			{
				foreach(MidiEvent meEvent in tTrack.Iterator())
				{
					if (meEvent.MidiMessage is MetaMessage)
					{
						MetaMessage mmMessage = meEvent.MidiMessage as MetaMessage;
						byte[] data;// = new byte[mmMessage.Length];
						
						data = mmMessage.GetBytes();
						
						switch (mmMessage.MetaType)
						{
							case MetaType.InstrumentName:
								{
									
									break;
								}
							case MetaType.TrackName:
								{
									FTrackNames[trackID] = System.Text.Encoding.UTF8.GetString(data);
									break;
								}
							case MetaType.TimeSignature:
								{
									FTimeSignature.Numerator = data[0];
									FTimeSignature.Denominator = (byte) Math.Pow(2, data[1]);
									FTimeSignature.MetronomePulse = data[2];
									FTimeSignature.NumberOf32nds = data[3];
									break;
								}
							case MetaType.Tempo:
								{
									if (!tempoSet)
									{
										int ms_per_min = 60000000;
										int ms_per_quarternote = (int) ((data[0]<<16) + (data[1]<<8) + data[2]);
										FBPM = ms_per_min / ms_per_quarternote;
										//MPQN = MICROSECONDS_PER_MINUTE / BPM
										tempoSet = true;
									}
									break;
								}
						}
					}
					else if (meEvent.MidiMessage is ChannelMessage)
					{
						ChannelMessage cmMessage = meEvent.MidiMessage as ChannelMessage;
						
						if (cmMessage != null)
						{
							if (cmMessage.Command == ChannelCommand.NoteOn)
							{
								if (cmMessage.Data2 != 0)
								{
									TMidiNote mn = new TMidiNote();
									mn.Track = trackID;
									mn.Channel = cmMessage.MidiChannel;
									mn.Note = cmMessage.Data1;
									mn.Velocity = cmMessage.Data2;
									mn.Start = meEvent.AbsoluteTicks;
									FOpenMidiNotes.Add(mn);
								}
								else
								{
									foreach (TMidiNote mn in FOpenMidiNotes)
										if (mn.Note == cmMessage.Data1)
									{
										mn.End = meEvent.AbsoluteTicks;
										FMidiNotes.Add(mn);
										FOpenMidiNotes.Remove(mn);
										break;
									}
								}
							}
							if (cmMessage.Command == ChannelCommand.NoteOff)
								foreach (TMidiNote mn in FOpenMidiNotes)
								if (mn.Note == cmMessage.Data1)
							{
								mn.End = meEvent.AbsoluteTicks;
								FMidiNotes.Add(mn);
								FOpenMidiNotes.Remove(mn);
								break;
							}
						}
					}
				}
				
				trackID++;
			}
			
			FMidiNotes.Sort(delegate(TMidiNote mn1, TMidiNote mn2) {return mn1.Start.CompareTo(mn2.Start);});
			UpdateTimeScaling();
			//UpdateNotesOfBar();
		}
		
		public void ClearCurrentNotes()
		{
			FCurrentMidiNotes.Clear();
		}
		
		/*	public void ComputeBarLength()
		{
			if (FExternalBar != 0)
			{
				FRhythmChangedAtBar = FExternalBar;
				FRhythmChangedOffset = FExternalBar * FBarLength;
				System.Diagnostics.Debug.WriteLine("rhythm changed at: " + FRhythmChangedAtBar.ToString() + " old barlength: " + FBarLength.ToString());
				FRhythmChangedOffsetInPixel = FRhythmChangedOffset * Fppmu;
				
			}
			
			int ganze = FDivision*4;
			int part = ganze / FNenner;
			FBarLength = FZähler * part;
			System.Diagnostics.Debug.WriteLine("new barlength: " + FBarLength.ToString());
		}
		
		public void ClearPlayedNotes()
		{
			foreach (TMidiNote mn in FMidiNotes)
				mn.Played = false;
			
			foreach (TMidiNote mn in FNotesOfBar)
				mn.Played = false;
			
			FWindowFromNote = 0;
			FWindowToNote = 5;
			FNotesFound.Clear();
		}
		
		public void PrepareNotesOfWindow()
		{
			//System.Diagnostics.Debug.WriteLine("PrepareNotesOfWindow: from: " + FWindowFromNote.ToString() + " to: " + FWindowToNote.ToString());

			if (FMidiNotes.Count < 5)
				return;
			
			FNotesOfWindow = FMidiNotes.GetRange(FWindowFromNote, FWindowToNote - FWindowFromNote);
			//output notes of window!!!!
			for (int i=0; i<FNotesOfWindow.Count; i++)
			{
				System.Diagnostics.Debug.WriteLine("note in window: " + FNotesOfWindow[i].Note.ToString());
			}
		}*/
		
		public void ShowNote(int Note, int Velocity)
		{
			//System.Diagnostics.Debug.WriteLine("show note: " + Note.ToString());
			
			if (FMidiNotes.Count < 5)
				return;
			//everytime a note is played:
			//if found in FNotesOfWindow then mark the note as played
			//if not found or already played: note is a mistake or note is out of range -> do nothing
			
			
			foreach (TMidiNote mdn in FNotesOfWindow)
			{
				if ((mdn.Note == Note) && (!mdn.Played) )
				{
					System.Diagnostics.Debug.WriteLine("found: " + Note.ToString());
					mdn.Played = true;
					FNotesFound.Add(mdn);
					break;
				}
			}
		}
		/*
		public void AdjustWindowBorders(int shiftright)
		{
			FWindowToNote += shiftright;
			//clamp at max index
			FWindowToNote = Math.Min(FWindowToNote, FMidiNotes.Count);
			
			TMidiNote first = FNotesOfWindow.Find(delegate(TMidiNote m) {return !m.Played;} );
			FWindowFromNote = FMidiNotes.IndexOf(first);

			FWindowFromNote = Math.Max(FWindowFromNote, FWindowToNote - FWindowMaxNotes);
			FWindowFromNote = Math.Max(FWindowFromNote, 0);
			
			//System.Diagnostics.Debug.WriteLine("AdjustWindowBorders: from: " + FWindowFromNote.ToString() + " to: " + FWindowToNote.ToString());
		}
		
		public Rectangle GetWindowRect(Rectangle r)
		{
			Rectangle rect = new Rectangle(0, 0, 0, 0);
			if (FMidiNotes.Count > 0)
			{
				float from = FMidiNotes[Math.Min(FWindowFromNoteOld, FMidiNotes.Count-1)].Start * Fppmu;
				float to = FMidiNotes[Math.Min(FWindowToNote, FMidiNotes.Count-1)].End * Fppmu;
				rect = new Rectangle((int) (from - FOffset), 0, (int) (to - from), r.Height);
			}
			
			return rect;
		}
		
		public Rectangle GetWindowRectNew(Rectangle r)
		{
			Rectangle rect = new Rectangle(0, 0, 0, 0);
			if (FMidiNotes.Count > 0)
			{
				float from = FMidiNotes[Math.Min(FWindowFromNote, FMidiNotes.Count-1)].Start * Fppmu;
				float to = FMidiNotes[Math.Min(FWindowToNote, FMidiNotes.Count-1)].End * Fppmu;
				rect = new Rectangle((int) (from - FOffset), 0, (int) (to - from), r.Height);
			}
			
			return rect;
		}*/
		
		public void MidiScorePaint(Graphics g, Rectangle r)
		{
			/*		if (FLength == 0)
				return;
			
			//g.DrawRectangle(new Pen(Color.Orange), g.VisibleClipBounds.X, g.VisibleClipBounds.Y, g.VisibleClipBounds.Width, g.VisibleClipBounds.Height);
			
        	SolidBrush b = new SolidBrush(Color.Gray);
        	Pen p = new Pen(Color.Black);

        	int noteRange = FMaxNote - FMinNote;
        	int noteHeight = r.Height/noteRange;
    		
        	float visibleBeats = FBpm / 60 * (float) TLMain.GTopRuler.VisibleTimeRange;
        	//System.Diagnostics.Debug.WriteLine(TLMain.GTopRuler.VisibleTimeRange.ToString());

        	float pixelPerBeat = r.Width / visibleBeats;

        	//System.Diagnostics.Debug.WriteLine(FDivision.ToString());
        	//pixel per midi unit: ppmu
        	Fppmu = pixelPerBeat / FDivision;
        	
        	//pixel per time:
        	float ppt = r.Width / (float) TLMain.GTopRuler.VisibleTimeRange;

        	//midi units per time: mupt
        	float visibleMidiUnits = visibleBeats * FDivision;
        	FMUpT = visibleMidiUnits / (float) TLMain.GTopRuler.VisibleTimeRange;
        	
        	FOffset = ppt * (float) TLMain.GTopRuler.Start;
        	float x = 0;
        	int barHeight = 10;
        	
        	//draw current bar
        	x = r.Left + (FBarFrom-1) * FBarLength * Fppmu - FOffset;
        	b.Color = Color.LightGray;
        	if ((FBarFrom == 0) && (FBarTo == 0))
				g.FillRectangle(b, r.Left, 0, r.Width, barHeight);
        	else
        		g.FillRectangle(b, x, 0, (FBarTo - FBarFrom + 1) * FBarLength * Fppmu, r.Height);
			        	
        	//draw note grid
    		p.Color = Color.Silver;
        	for (int i = 0; i < noteRange + 2; i++)
        	{
	       		g.DrawLine(p, r.Left, barHeight + noteHeight*i, r.Left + r.Width, barHeight + noteHeight*i);
        	}
        
        	int start = (int) (TLMain.GTopRuler.Start * FMUpT);
        	int end = (int) (TLMain.GTopRuler.End * FMUpT);
        	
        	List <MidiNote> visibleNotes = FMidiNotes.FindAll(delegate (MidiNote m) {return m.Start > start && m.End < end;});
        	//draw notes
        	foreach (MidiNote mn in visibleNotes)
        	{
        		if ((mn.Note >= FMinNote) && (mn.Note <= FMaxNote))
        		{
	        		Color tmp = Color.Black;
	       			switch (mn.Channel)
					{
						case 0:	tmp = Color.FromArgb(255, 255, 0, 0); break;
						case 1:	tmp = Color.FromArgb(255, 0, 255, 0); break;
						case 2:	tmp = Color.FromArgb(255, 0, 0, 255); break;
						case 3:	tmp = Color.FromArgb(255, 0, 255, 255); break;
					}
	       			
	       			b.Color = tmp;
	       			p.Color = tmp;
	       			
	       			if (!mn.Played)
	       				g.FillRectangle(b, r.Left+mn.Start*Fppmu - FOffset, barHeight + noteHeight*noteRange - (mn.Note-FMinNote)*noteHeight, mn.Duration*Fppmu, noteHeight);
	       			else
	       				g.DrawRectangle(p, r.Left+mn.Start*Fppmu - FOffset, barHeight + noteHeight*noteRange - (mn.Note-FMinNote)*noteHeight, mn.Duration*Fppmu, noteHeight);
        		}
        	}
        	
        	Font fnt = new Font("SmallFonts",7);
        	b.Color = Color.Black;
        	p.Color = Color.Black;
        	
        	
        	float barcount = FLength/FBarLength;
        	float rcOldToNewBarCount = FRhythmChangedOffset / FBarLength;
        	//draw bars
        	for (int i = FRhythmChangedAtBar; i < barcount-FRhythmChangedAtBar; i++)
        	{
        		System.Diagnostics.Debug.WriteLine(rcOldToNewBarCount.ToString());
        		x = r.Left - FOffset + (rcOldToNewBarCount + i-FRhythmChangedAtBar)*FBarLength*Fppmu;
        		if (i % 16 == 0)
	        		p.Width = 2;
        		else
        			p.Width = 1;
        		g.DrawLine(p, x, 0, x, r.Height);
        		g.DrawString((i+1).ToString(), fnt, b, new PointF(x, 0));
        	}
        	
        	//draw open search window
   			b.Color = Color.FromArgb(120, 50, 100, 200);
   			g.FillRectangle(b, GetWindowRectNew(r));
		}*/
		}
		
	}
	
	public class TMidiNote
	{
		private int FTrack;
		private int FChannel;
		private int FNote;
		private int FVelocity = 0;
		private int FStart;
		private int FEnd;
		private bool FPlayed;
		
		public int Track
		{
			get {return FTrack;}
			set {FTrack = value;}
		}
		public int Channel
		{
			get {return FChannel;}
			set {FChannel = value;}
		}
		public int Note
		{
			get {return FNote;}
			set {FNote = value;}
		}
		public int Velocity
		{
			get {return FVelocity;}
			set {FVelocity = value;}
		}
		public int Start
		{
			get {return FStart;}
			set {FStart = value;}
		}
		public int End
		{
			get {return FEnd;}
			set {FEnd = value;}
		}
		public int Duration
		{
			get {return FEnd - FStart;}
		}
		
		public bool Played
		{
			get {return FPlayed;}
			set {FPlayed = value;}
		}
	}
}
