using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Xml;

using Un4seen.Bass;


using VVVV.PluginInterfaces.V1;

namespace VVVV.Nodes.Timeliner
{
	public class TLWavPin: TLPin
	{
		private IStringConfig FFilename;
		private IValueOut FValueOut;
		
		private OpenFileDialog FOpenDialog = new OpenFileDialog();
		
		public TLWavPin(IPluginHost Host, TLTransformer Transformer, int Order, XmlNode PinSettings):base(Host, Transformer, Order, PinSettings, true, true)
		{
			Bass.BASS_Init(-1, 44100, BASSInit.BASS_DEVICE_LATENCY, Handle, null);
			
			int ret = Bass.BASS_StreamCreateFile("e:\\wave.wav", 0, 0, 0);
			
			long bytes = Bass.BASS_ChannelGetLength(ret);
			bool r = Bass.BASS_ChannelPlay(ret, true);
			
			byte[] data = new byte[1000];
			int hr = Bass.BASS_ChannelGetData(ret, data, 1000);
			
			//only load slices after all super constructors have been executed:
			XmlAttribute attr = PinSettings.Attributes.GetNamedItem("SliceCount") as XmlAttribute;
        	LoadSlices(int.Parse(attr.Value));
		}

		protected override void CreatePins()
		{
			base.CreatePins();
			
			FHost.CreateStringConfig(Name + "-Filename", TSliceMode.Dynamic, TPinVisibility.True, out FFilename);
        	//FFilename.SliceCount=0;
			FFilename.SetSubType("", true);
			
			FHost.CreateValueOutput(Name, 1, null, TSliceMode.Dynamic, TPinVisibility.True, out FValueOut);
        	FValueOut.Order=Order;
        	FValueOut.SetSubType(double.MinValue, double.MaxValue, 0.0001D, 0, false, false, false);
		}
		
		public override void DestroyPins()
		{
			// DELETE ALL PINS
			///////////////////////
			//FHost.DeletePin(FArm);

			FHost.DeletePin(FFilename);
			FFilename = null;
			
			FHost.DeletePin(FValueOut);
			FValueOut = null;
			
			base.DestroyPins();
		}
		
		private void UpdateOutputSliceCount()
		{
			int count = 0;
			for (int i=0; i<FOutputSlices.Count; i++)
				count += (FOutputSlices[i] as TLWavSlice).ChannelCount;
			
			FValueOut.SliceCount = count;
		}
		
		protected override void LoadSlices(int SavedSliceCount)
		{
			for(int i=0; i<SavedSliceCount; i++)
				AddSlice(FOutputSlices.Count);
			
			FFilename.SliceCount = SavedSliceCount;
			UpdateOutputSliceCount();
		}
		
		protected override void PinNameChanged()
		{
			base.PinNameChanged();

			FFilename.Name = Name + "-Filename";
			FValueOut.Name = Name;
		}
		
		public override void PinOrderChanged()
		{
			base.PinOrderChanged();
			FValueOut.Order = FOrder;
		}
		
		public override void RemoveSlice(TLSlice Slice)
		{
			base.RemoveSlice(Slice);
			
			FFilename.SliceCount = FOutputSlices.Count;
			UpdateOutputSliceCount();
		}
		
		public override void AddSlice(int At)
		{
			TLWavSlice ws = new TLWavSlice(FHost, this, FOutputSlices.Count, FOrder);

			AddSlice(At, ws);
			
			FFilename.SliceCount = FOutputSlices.Count;
			UpdateOutputSliceCount();
		}

		public override void Configurate(IPluginConfig Input, bool FirstFrame)
		{
			//if Input = last ConfigInput created!
			if (Input == FFilename)
			{
				string filename;
				for (int i=0; i<FFilename.SliceCount; i++)
				{
					FFilename.GetString(i, out filename);
					(FOutputSlices[i] as TLWavSlice).Filename = filename;
				}
			}
		}
		
		public override void Evaluate(double CurrentTime)
		{
			base.Evaluate(CurrentTime);
			
			int idx = 0;
			UpdateOutputSliceCount();
			for (int i=0; i<FOutputSlices.Count; i++)
			{
				for (int j=0; j<(FOutputSlices[i] as TLWavSlice).ChannelCount; j++)
				{
					FValueOut.SetValue(idx, (FOutputSlices[i] as TLWavSlice).ChannelValues[j]);
					idx++;
				}
			}				
		}
	}
}
