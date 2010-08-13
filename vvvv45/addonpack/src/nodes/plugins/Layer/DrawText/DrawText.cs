//////licence
//GNU Lesser General Public License (LGPL)
//english: http://www.gnu.org/licenses/lgpl.html
//german: http://www.gnu.de/lgpl-ger.html

#region usings
//use what you need
using System;
using System.Runtime.InteropServices;
using System.Drawing;
using System.Collections.Generic;
using System.Collections;
using System.Text;
using System.ComponentModel.Composition;

using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils;
using VVVV.Utils.VColor;
using VVVV.Utils.VMath;
using VVVV.Shared.VSlimDX;
using VVVV.Core.Logging;

using SlimDX;
using SlimDX.Direct3D9;
#endregion usings

namespace VVVV.Nodes
{
    public struct DeviceFont
    {
        public SlimDX.Direct3D9.Font Font;
        public SlimDX.Direct3D9.Sprite Sprite;
        public SlimDX.Direct3D9.Texture Texture;
    }
    
    [PluginInfo (Name = "Text",
                 Category = "EX9",
                 Author = "vvvv group",
                 Help = "Draws flat Text")]
    public class DrawText: IPluginEvaluate, IPluginDXLayer
    {
        #region fields & pins
        [Input ("Rectangle")]
        ISpread<Vector2D> FRectInput;
        
        [Input ("Text", DefaultString = "vvvv")]
        ISpread<string> FTextInput;
        
        [Input ("Character Encoding", EnumName = "CharEncoding")]
        ISpread<EnumEntry> FCharEncoding;
        
        [Input ("Font", EnumName = "SystemFonts")]
        IObservableSpread<EnumEntry> FFontInput;
        
        [Input ("Italic", SliceMode = TSliceMode.Single)]
        IObservableSpread<bool> FItalicInput;
        
        [Input ("Bold", SliceMode = TSliceMode.Single)]
        IObservableSpread<bool> FBoldInput;
        
        [Input ("Size", DefaultValue = 150, MinValue = 0, SliceMode = TSliceMode.Single)]
        IObservableSpread<int> FSizeInput;
        
        [Input ("Color", DefaultValues = new double[4]{1, 1, 1, 1})]
        ISpread<RGBAColor> FColorInput;
        
        [Input ("Brush Color", DefaultValues = new double[4]{0, 0, 0, 1})]
        ISpread<RGBAColor> FBrushColor;
        
        [Input ("Show Brush")]
        ISpread<bool> FShowBrush;
        
        [Input ("Horizontal Align", EnumName = "HorizontalAlign")]
        ISpread<EnumEntry> FHorizontalAlignInput;
        
        [Input ("Vertical Align", EnumName = "VerticalAlign")]
        ISpread<EnumEntry> FVerticalAlignInput;
        
        [Input ("Text Rendering Mode", EnumName = "TextRenderingMode")]
        ISpread<EnumEntry> FTextRenderingModeInput;
        
        [Input ("Normalize", EnumName = "Normalize", SliceMode = TSliceMode.Single)]
        ISpread<EnumEntry> FNormalizeInput;
        
        [Input ("Enabled", DefaultValue = 1, SliceMode = TSliceMode.Single)]
        ISpread<bool> FEnabledInput;
        
        [Output ("Text Size")]
        ISpread<Vector2D> FSizeOutput;
        
        [Import]
    	protected ILogger Logger { get; set; }
        
        private ITransformIn FTransformIn;
        private IDXRenderStateIn FRenderStatePin;
        private IDXLayerIO FLayerOutput;
        
        private int FSpreadMax;
        private Dictionary<int, DeviceFont> FDeviceFonts = new Dictionary<int, DeviceFont>();
        #endregion field declaration
        
        #region constructur
        [ImportingConstructor]
        public DrawText(IPluginHost host)
        {
            host.CreateRenderStateInput(TSliceMode.Single, TPinVisibility.True, out FRenderStatePin);
            host.CreateTransformInput("Transform", TSliceMode.Dynamic, TPinVisibility.True, out FTransformIn);
            host.CreateLayerOutput("Layer", TPinVisibility.True, out FLayerOutput);
        }
        #endregion constructur
        
        #region mainloop
        public void Evaluate(int SpreadMax)
        {
            FSpreadMax = SpreadMax;
            FSizeOutput.SliceCount = SpreadMax;
        }
        #endregion mainloop
        
        #region DXLayer
        private void RemoveResource(int OnDevice)
        {
            DeviceFont df = FDeviceFonts[OnDevice];
            FDeviceFonts.Remove(OnDevice);
            
            df.Font.Dispose();
            df.Sprite.Dispose();
            df.Texture.Dispose();
        }
        
        public void UpdateResource(IPluginOut ForPin, int OnDevice)
        {
            bool needsupdate = false;
            
            try
            {
                DeviceFont df = FDeviceFonts[OnDevice];
                if (FFontInput.IsChanged || FSizeInput.IsChanged || FBoldInput.IsChanged || FItalicInput.IsChanged)
                {
                    RemoveResource(OnDevice);
                    needsupdate = true;
                }
            }
            catch
            {
                //if resource is not yet created on given Device, create it now
                needsupdate = true;
            }
            
            if (needsupdate)
            {
                //FHost.Log(TLogType.Debug, "Creating Resource...");
                Device dev = Device.FromPointer(new IntPtr(OnDevice));

                DeviceFont df = new DeviceFont();
                
                FontWeight weight;
                if (FBoldInput[0])
                    weight = FontWeight.Bold;
                else
                    weight = FontWeight.Light;
                
                df.Font = new SlimDX.Direct3D9.Font(dev, FSizeInput[0], 0, weight, 0, FItalicInput[0], CharacterSet.Default, Precision.Default, FontQuality.Default, PitchAndFamily.Default, FFontInput[0].Name);
                df.Sprite = new Sprite(dev);
                
                df.Texture = new Texture(dev, 1, 1, 1, Usage.None, Format.L8, Pool.Managed);// Format.A8R8G8B8, Pool.Default);
                //need to fill texture white to be able to set color on sprite later
                DataRectangle tex = df.Texture.LockRectangle(0, LockFlags.None);
                tex.Data.WriteByte(255);
                df.Texture.UnlockRectangle(0);
                
                FDeviceFonts.Add(OnDevice, df);
                
                //dispose device
                dev.Dispose();
            }
        }
        
        public void DestroyResource(IPluginOut ForPin, int OnDevice, bool OnlyUnManaged)
        {
            //dispose resources that were created on given OnDevice
            try
            {
                RemoveResource(OnDevice);
            }
            catch
            {
                //resource is not available for this device. good. nothing to do then.
            }
        }
        
        public void SetStates()
        {
            FRenderStatePin.SetRenderState(RenderState.AlphaTestEnable, 1);
            FRenderStatePin.SetRenderState(RenderState.SourceBlend, Blend.SourceAlpha);
            FRenderStatePin.SetRenderState(RenderState.DestinationBlend, Blend.InverseSourceAlpha);
        }
        
        public void Render(IDXLayerIO ForPin, IPluginDXDevice DXDevice)
        {
            //concerning the cut characters in some fonts, especially when rendered italic see:
            //http://www.gamedev.net/community/forums/topic.asp?topic_id=441338
            //seems to be an official bug and we'd need to write our very own font rendering to fix that
            
            if (!FEnabledInput[0])
                return;
            
            //from the docs: D3DXSPRITE_OBJECTSPACE -> The world, view, and projection transforms are not modified.
            //for view and projection transforms this is exactly what we want: it allows placing the text within the
            //same world as all the other objects. however we don't want to work in object space but in world space
            //that's why we need to to set the world transform to a neutral value: identity
            Device dev = Device.FromPointer(new IntPtr(DXDevice.DevicePointer()));
            dev.SetTransform(TransformState.World, Matrix.Identity);

            DeviceFont df = FDeviceFonts[DXDevice.DevicePointer()];
            FTransformIn.SetRenderSpace();

            //set states that are defined via upstream nodes
            FRenderStatePin.SetSliceStates(0);

            df.Sprite.Begin(SpriteFlags.ObjectSpace | SpriteFlags.DoNotAddRefTexture);
                
            int size = FSizeInput[0];
            int normalize = FNormalizeInput[0].Index;
            
            Matrix4x4 preScale = VMath.Scale(1, -1, 1);
            switch (normalize)
            {
                    case 0: preScale = VMath.Scale(1, -1, 1); break;
                    //"off" means that text will be in pixels
            }
            
            Matrix4x4 world;
            string text;
            RGBAColor textColor, brushColor;
            Rectangle tmpRect = new Rectangle(0, 0, 0, 0);
            
            int hAlign, wi, hi;
            double showBrush, w, h;
            float x, y;
            
            for (int i=0; i<FSpreadMax; i++)
            {
                text = FTextInput[i];
                
                if (string.IsNullOrEmpty(text))
                    continue;
                
                if (FCharEncoding[i] == "UTF8")
                {
                    byte[] utf8bytes = Encoding.Default.GetBytes(text);
                    text = Encoding.UTF8.GetString(utf8bytes);
                }

                DrawTextFormat dtf = DrawTextFormat.NoClip | DrawTextFormat.ExpandTabs;
                
                hAlign = FHorizontalAlignInput[i].Index;
                switch (hAlign)
                {
                        case 0: dtf |= DrawTextFormat.Left; break;
                        case 1: dtf |= DrawTextFormat.Center; break;
                        case 2: dtf |= DrawTextFormat.Right; break;
                }
                
                switch (FVerticalAlignInput[i].Index)
                {
                        case 0: dtf |= DrawTextFormat.Top; break;
                        case 1: dtf |= DrawTextFormat.VerticalCenter; break;
                        case 2: dtf |= DrawTextFormat.Bottom; break;
                }
                
                switch (FTextRenderingModeInput[i].Index)
                {
                        case 0: dtf |= DrawTextFormat.SingleLine; break;
                        case 2: dtf |= DrawTextFormat.WordBreak; break;
                }

                Vector2D rect = FRectInput[i] * size * 10;
                tmpRect.Width = (int) rect.x;
                tmpRect.Height = (int) rect.y;
                
                df.Font.MeasureString(df.Sprite, text, dtf, ref tmpRect);
                FSizeOutput[i] = new Vector2D(tmpRect.Width, tmpRect.Height);
                
                FTransformIn.GetRenderWorldMatrix(i, out world);
                
                switch (normalize)
                {
                        case 1: preScale = VMath.Scale(1f/tmpRect.Width, -1f/tmpRect.Width, 1); break;
                        //"width" means that the texture width will have no influence on the width of the sprite. Width will be always 1.
                        
                        case 2: preScale = VMath.Scale(1f/tmpRect.Height, -1f/tmpRect.Height, 1); break;
                        //"height" means that the texture height will have no influence on the height of the sprite. Height will be always 1.
                        
                        case 3: preScale = VMath.Scale(1f/tmpRect.Width, -1f/tmpRect.Height, 1); break;
                        //"on" means that the particle will always be a unit quad. independant of texture size
                }
                
                df.Sprite.Transform = VSlimDXUtils.Matrix4x4ToSlimDXMatrix(preScale * world);

                if (FShowBrush[i])
                {
                    x = tmpRect.Width/2;
                    y = tmpRect.Height/2;
                    if (hAlign == 0)
                        x -= x;
                    else if (hAlign == 2)
                        x += x;
                    
                    /*workaround for slimdx(august09)
					Matrix4x4 spriteBugWorkaround = VMath.Translate(-x, -y, 0.001);
					df.Sprite.Transform = VSlimDXUtils.Matrix4x4ToSlimDXMatrix(spriteBugWorkaround * preScale * world);
					 df.Sprite.Draw(df.Texture, new Rectangle(0, 0, tmpRect.Width, tmpRect.Height), new Color4(brushColor.Color));
					df.Sprite.Transform = VSlimDXUtils.Matrix4x4ToSlimDXMatrix(preScale * world);
					workaround end*/
                    
                    df.Sprite.Draw(df.Texture, new Rectangle(0, 0, tmpRect.Width, tmpRect.Height), new Vector3(x, y, -0.001f), null, new Color4(FBrushColor[i].Color.ToArgb()));
                }
                
                df.Font.DrawString(df.Sprite, text, new Rectangle(-tmpRect.Width/2, -tmpRect.Height/2, tmpRect.Width, tmpRect.Height), dtf, (Color) FColorInput[i]);
            }
            
            df.Sprite.End();
        }
        #endregion
    }
}
