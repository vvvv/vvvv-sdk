using System;
using System.Runtime.InteropServices;
using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;

namespace VVVV.Hosting.Pins.Output
{
    [ComVisible(false)]
	public class StringOutputPin : Pin<string>
	{
		private bool FIsChanged = true;
		protected IStringOut FStringOut;
		
		public StringOutputPin(IPluginHost host, OutputAttribute attribute)
			: base(host, attribute)
		{
			host.CreateStringOutput(attribute.Name, (TSliceMode)attribute.SliceMode, (TPinVisibility)attribute.Visibility, out FStringOut);
			FStringOut.SetSubType2(attribute.DefaultString, attribute.MaxChars, attribute.FileMask, (TStringType)attribute.StringType);
			
			base.InitializeInternalPin(FStringOut);
		}
		
		public override string this[int index]
		{
			get
			{
				return base[index];
			}
			set
			{
				FIsChanged = true;
				base[index] = value;
			}
		}
		
		public override int SliceCount
		{
			get
			{
				return base.SliceCount;
			}
			set
			{
				FIsChanged = true;
				base.SliceCount = value;
			}
		}
		
		public override void Update()
		{
			base.Update();
			
			// String marshaling is very expensive. So avoid it.
			if (FIsChanged)
			{
				if (FAttribute.SliceMode != SliceMode.Single)
					FStringOut.SliceCount = FSliceCount;
				
				for (int i = 0; i < FSliceCount; i++)
				{
					FStringOut.SetString(i, FBuffer[i]);
				}
			}
			
			FIsChanged = false;
		}
	}
}
