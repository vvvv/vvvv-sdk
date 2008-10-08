using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;

using Un4seen.Bass;
using Un4seen.Bass.Misc;
using System.Runtime.InteropServices;

using VVVV.PluginInterfaces.V1;

namespace VVVV.Nodes.Timeliner
{
	public class TLWavSlice :  TLSlice
	{
		private IValueOut FValueOut;
		
		private WaveForm FWaveForm = null;
		private Bitmap FWaveBMP = null;
		private int FWaveWidth, FWaveFrom, FWaveTo;
		private int FHandle;
		private GCHandle _hGCFile;
		private double FLength;
		
		private Color FBackGround = Color.FromArgb(255, 230, 230, 230);
		private OpenFileDialog FOpenDialog = new OpenFileDialog();
		
		private string FFilename;
		public string Filename
		{
			set
			{		
				LoadFile(value);
			}
		}
		
		private int FChannelCount;
		public int ChannelCount
		{
			get{return FChannelCount;}
		}
		
		private double[] FChannelValues;
		public double[] ChannelValues
		{
			get{return FChannelValues;}
		}
		
		public TLWavSlice(IPluginHost Host, TLBasePin Pin, int SliceIndex, int PinsOrder): base(Host, Pin, SliceIndex, PinsOrder)
		{}
		
		private void LoadFile(string Filename)
		{
			FFilename = Filename;
			
			System.IO.FileStream fs = System.IO.File.OpenRead(Filename);
			// get the legth of the file
			long length = fs.Length;
			// create the buffer which will keep the file in memory
			byte[] buffer = new byte[length];
			// read the file into the buffer
			fs.Read(buffer, 0, (int) length);
			// buffer is filled, file can be closed
			fs.Close();

			// now create a pinned handle, so that the Garbage Collector will not move this object
			_hGCFile = GCHandle.Alloc( buffer, GCHandleType.Pinned );

			FHandle = Bass.BASS_StreamCreateFile(_hGCFile.AddrOfPinnedObject(), 0L, length, BASSFlag.BASS_STREAM_DECODE);

        	FWaveForm = new WaveForm();
        	FWaveForm.ColorBackground = FBackGround;
        	FWaveForm.RenderStart(FHandle, true);
        	
        	//load again because .RenderStart freed handle
        	FHandle = Bass.BASS_StreamCreateFile(_hGCFile.AddrOfPinnedObject(), 0L, length, BASSFlag.BASS_STREAM_DECODE);
        	
        	long bytes = Bass.BASS_ChannelGetLength(FHandle);
			FLength = Bass.BASS_ChannelBytes2Seconds(FHandle, bytes);
			
			BASS_CHANNELINFO chI = new BASS_CHANNELINFO();
			Bass.BASS_ChannelGetInfo(FHandle, chI);
			FChannelCount = chI.chans;
			
			FChannelValues = new double[FChannelCount];
		}
		
		protected override void CreatePins()
		{}
		
		public override void DestroyPins()
		{
			//FHost.DeletePin(FValueOut);
			//FValueOut = null;

			FKeyFrames.Clear();
		}
		
		protected override void SliceOrPinOrderChanged()
		{
			//FValueOut.Name = FPin.Name + "-Sample" + FSliceIndex.ToString();
		}
		
		public override TLBaseKeyFrame AddKeyFrame(Point P)
		{
			//called only from GUI-doubleclick
			FOpenDialog = new OpenFileDialog();
			if (FOpenDialog.ShowDialog() == DialogResult.OK)
				LoadFile(FOpenDialog.FileName);
			
			return null;
		}
		
		public override void SaveKeyFrames()
		{
			
		}
		
		public override void Evaluate(double CurrentTime)
		{
			double left, right;
			
			if ((CurrentTime > FLength) || (CurrentTime < 0) || (FWaveForm == null))
				left = right = 0;
			else
			{
				long bytePos = Bass.BASS_ChannelSeconds2Bytes(FHandle, CurrentTime);
				BASSError e = Bass.BASS_ErrorGetCode();

				long dataPos = bytePos / FWaveForm.Wave.bpf;
				left = (FWaveForm.Wave.data[dataPos].left + 32768) / 32768.0 - 1;
				right = (FWaveForm.Wave.data[dataPos].right + 32768) / 32768.0 - 1;
			}
			
			string sLeft = "", sRight = "";
			if (FChannelValues != null)
			{
				FChannelValues[0] = left;
				sLeft = left.ToString("f4", TimelinerPlugin.GNumberFormat);
				if (FChannelValues.Length > 1)
				{
					FChannelValues[1] = right;
					sRight = right.ToString("f4", TimelinerPlugin.GNumberFormat);
				}
			}
			
			FOutputAsString = "L: " + sLeft + " R:  " + sRight +  " "  + System.IO.Path.GetFileName(FFilename);
		}
		
		public override void Configurate(IPluginConfig Input, bool FirstFrame)
		{
			//if Input = last ConfigInput created!

			base.Configurate(Input, FirstFrame);
		}
		
		public override void DrawSlice(Graphics g, double From, double To, bool AllInOne)
		{
			base.DrawSlice(g, From, To, AllInOne);
			
			if (FWaveForm == null)
				return;
			
			float xStart = FPin.Transformer.TransformPoint(new PointF(0, 0)).X;
			//float xLength = FPin.Transformer.TransformPoint(new PointF((float) FLength, 0)).X - xStart;
			

			double visFrom = Math.Max(0, From);
			double visTo = Math.Min(FLength, To);
			float pixFrom = FPin.Transformer.TransformPoint(new PointF((float) visFrom, 0)).X;
			float pixTo = FPin.Transformer.TransformPoint(new PointF((float) visTo, 0)).X;

			int bmpWidth = (int) (pixTo - pixFrom); //Math.Min(g.ClipBounds.Width, pixLength);
			
			int fromFrame = FWaveForm.Position2Frames(Math.Max(0, From));
			int toFrame = FWaveForm.Position2Frames(Math.Min(To, FLength));
			
			int sliceHeight = FPin.Height / FPin.SliceCount;
			FWaveBMP = FWaveForm.CreateBitmap(bmpWidth, (int) sliceHeight, fromFrame, toFrame, false);
			if (FWaveBMP != null)
				g.DrawImage(FWaveBMP, Math.Max(0, xStart), 0);
			
			SizeF sz = g.MeasureString(FOutputAsString, FFont);
			g.FillRectangle(new SolidBrush(FBackGround), new RectangleF(g.ClipBounds.Width-sz.Width, sliceHeight-sz.Height, sz.Width, sz.Height));
			g.DrawString(FOutputAsString, FFont, new SolidBrush(Color.Gray), g.ClipBounds.Width-sz.Width+2, sliceHeight-16);
		}
	}			
}

