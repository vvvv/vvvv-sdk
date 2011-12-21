using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

using SlimDX.Direct3D9;
using VVVV.Hosting.Interfaces.EX9;
using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.PluginInterfaces.V2.EX9;

namespace VVVV.Hosting.Pins.Output
{
    [ComVisible(false)]
    public class TextureOutputPin<T, TMetadata> : Pin<T>, IDXTexturePin where T : DXResource<Texture, TMetadata>
    {
        private readonly IDXTextureOut FInternalTexturePin;
        private bool FChanged;
        
        public TextureOutputPin(IInternalPluginHost pluginHost, OutputAttribute outputAttribute)
            : base(pluginHost, outputAttribute)
        {
            FInternalTexturePin = pluginHost.CreateTextureOutput2(this, FName, FSliceMode, FVisibility);
            InitializeInternalPin(FInternalTexturePin);
            
            // Set SliceCount to zero. Avoids stupid null checks in plugin.
            SliceCount = 0;
        }
        
        public override T this[int index] 
        {
            get 
            { 
                return base[index];
            }
            set                                                                                                                                                           
            {
                FChanged = true;
                
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
                if (FSliceCount != value)
                    FChanged = true;
                
                base.SliceCount = value; 
            }
        }
        
        public override void Update()
		{
			base.Update();
			
			if (FChanged) 
			{
				if (FAttribute.SliceMode != SliceMode.Single)
					FInternalTexturePin.SliceCount = FSliceCount;
				
				FInternalTexturePin.MarkPinAsChanged();
			}
			
			FChanged = false;
		}
        
        Texture IDXTexturePin.this[Device device, int slice]
        {
            get 
            {
                return this[slice][device];
            }
        }
        
        void IDXResourcePin.UpdateResources(Device device)
        {
            for (int i = 0; i < SliceCount; i++)
            {
                this[i].UpdateResource(device);
            }
        }
        
        void IDXResourcePin.DestroyResources(Device device, bool onlyUnmanaged)
        {
            for (int i = 0; i < SliceCount; i++)
            {
                this[i].DestroyResource(device);
            }
        }
    }
}
