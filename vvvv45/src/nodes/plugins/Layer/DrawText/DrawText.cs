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
using VVVV.Core.Logging;

using SlimDX;
using SlimDX.Direct3D9;
#endregion usings

namespace VVVV.Nodes
{
    public struct FontIdentifier
    {
        public Device Device;
        public string Name;
        public int Size;
        public bool Italic;
        public FontWeight Weight;
        //public Precision Precision;
        public FontQuality Quality;
    }

    public struct DeviceHelpers
    {
        public Sprite Sprite;
        public Texture Texture;
    }

    [PluginInfo(Name = "Text",
                 Category = "EX9",
                 Author = "vvvv group",
                 Help = "Draws flat Text")]
    public class DrawText : IPluginEvaluate, IPluginDXLayer
    {
        #region fields & pins
        [Input("Text", DefaultString = "vvvv")]
        protected ISpread<string> FTextInput;

        [Input("Character Encoding", EnumName = "CharEncoding")]
        protected ISpread<EnumEntry> FCharEncoding;

        [Input("Font", EnumName = "SystemFonts")]
        protected IDiffSpread<EnumEntry> FFontInput;

        [Input("Italic")] //, IsSingle = true)]
        protected IDiffSpread<bool> FItalicInput;

        [Input("Bold")] //, IsSingle = true)]
        protected IDiffSpread<bool> FBoldInput;

        [Input("Size", DefaultValue = 150, MinValue = 0)] //, IsSingle = true)]
        protected IDiffSpread<int> FSizeInput;

        //[Input("Precision", DefaultEnumEntry = "Default")]
        //protected ISpread<Precision> FPrecision;

        [Input("Quality", DefaultEnumEntry = "Default")]
        protected ISpread<FontQuality> FQuality;

        [Input("Color", DefaultColor = new double[4] { 1, 1, 1, 1 })]
        protected ISpread<RGBAColor> FColorInput;

        [Input("Brush Color", DefaultColor = new double[4] { 0, 0, 0, 1 })]
        protected ISpread<RGBAColor> FBrushColor;

        [Input("Show Brush")]
        protected ISpread<bool> FShowBrush;

        [Input("Horizontal Align", EnumName = "HorizontalAlign")]
        protected ISpread<EnumEntry> FHorizontalAlignInput;

        [Input("Vertical Align", EnumName = "VerticalAlign")]
        protected ISpread<EnumEntry> FVerticalAlignInput;

        [Input("Text Rendering Mode", EnumName = "TextRenderingMode")]
        protected ISpread<EnumEntry> FTextRenderingModeInput;

        [Input("Normalize", EnumName = "Normalize", IsSingle = true)]
        protected ISpread<EnumEntry> FNormalizeInput;

        [Input("Width [px] (Multiline Mode)", DefaultValue = 300)]
        protected ISpread<int> FWidth;

        [Input("Enabled", DefaultValue = 1, IsSingle = true)]
        protected ISpread<bool> FEnabledInput;

        [Output("Text Size")]
        protected ISpread<Vector2D> FSizeOutput;

        [Import]
        protected ILogger Logger { get; set; }

        private ITransformIn FTransformIn;
        private IDXRenderStateIn FRenderStatePin;
        private IDXLayerIO FLayerOutput;

        private int FSpreadMax;
        private Dictionary<FontIdentifier, SlimDX.Direct3D9.Font> FFonts = new Dictionary<FontIdentifier, SlimDX.Direct3D9.Font>();
        private Dictionary<Device, DeviceHelpers> FDeviceHelpers = new Dictionary<Device, DeviceHelpers>();
        #endregion field declarationPL

        #region constructur
        [ImportingConstructor]
        public DrawText(IPluginHost host)
        {
            host.CreateRenderStateInput(TSliceMode.Single, TPinVisibility.True, out FRenderStatePin);
            FRenderStatePin.Order = -2;
            host.CreateTransformInput("Transform", TSliceMode.Dynamic, TPinVisibility.True, out FTransformIn);
            FTransformIn.Order = -1;
            host.CreateLayerOutput("Layer", TPinVisibility.True, out FLayerOutput);
            FLayerOutput.Order = -1;
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
        private void RemoveFont(FontIdentifier id)
        {
            var df = FFonts[id];
            FFonts.Remove(id);
            df.Dispose();
        }

        public FontIdentifier CreateFontIdentifier(Device dev, int slice)
        {
            FontIdentifier id = new FontIdentifier();
            id.Device = dev;
            id.Name = FFontInput[slice].Name;
            id.Size = FSizeInput[slice];
            id.Italic = FItalicInput[slice];

            if (FBoldInput[slice])
                id.Weight = FontWeight.Bold;
            else
                id.Weight = FontWeight.Light;

            //id.Precision = FPrecision[slice];
            id.Quality = FQuality[slice];

            return id;
        }

        public SlimDX.Direct3D9.Font CreateFont(FontIdentifier id)
        {
            try
            {
                return FFonts[id];
            }
            catch
            {
                var f = new SlimDX.Direct3D9.Font(
                    id.Device, id.Size, 0, id.Weight, 0, id.Italic, CharacterSet.Default, Precision.Default, //id.Precision, 
                    id.Quality, PitchAndFamily.Default, id.Name);
                f.PreloadCharacters(0, 255);
                f.PreloadGlyphs(0, 255);
                FFonts.Add(id, f);
                return f;
            }
        }

        public void UpdateResource(IPluginOut ForPin, int OnDevice)
        {
            Device dev = Device.FromPointer(new IntPtr(OnDevice));

            //create device specific helpers on given device if not already present
            try
            {
                var dh = FDeviceHelpers[dev];
            }
            catch
            {
                var dh = new DeviceHelpers();

                dh.Sprite = new Sprite(dev);
                dh.Texture = new Texture(dev, 1, 1, 1, Usage.None, Format.L8, Pool.Managed);// Format.A8R8G8B8, Pool.Default);

                //need to fill texture white to be able to set color on sprite later
                DataRectangle tex = dh.Texture.LockRectangle(0, LockFlags.None);
                tex.Data.WriteByte(255);
                dh.Texture.UnlockRectangle(0);

                FDeviceHelpers.Add(dev, dh);
            }
        }

        public void DestroyResource(IPluginOut ForPin, int OnDevice, bool OnlyUnManaged)
        {
            Device dev = Device.FromPointer(new IntPtr(OnDevice));

            //dispose resources that were created on given device
            try
            {
                var dh = FDeviceHelpers[dev];
                dh.Sprite.Dispose();
                dh.Texture.Dispose();

                var ids = FFonts.FindAllKeys(df => df.Device == dev);
                foreach (var id in ids)
                    RemoveFont(id);
            }
            catch
            {
                //resource is not available for this device. good. nothing to do then.
            }
        }

        public void SetStates()
        {
            FRenderStatePin.SetRenderState(RenderState.AlphaTestEnable, 1);
            FRenderStatePin.SetRenderState(RenderState.SourceBlend, (int)Blend.SourceAlpha);
            FRenderStatePin.SetRenderState(RenderState.DestinationBlend, (int)Blend.InverseSourceAlpha);
        }

        public void Render(IDXLayerIO ForPin, IPluginDXDevice DXDevice)
        {
            //concerning the cut characters in some fonts, especially when rendered italic see:
            //http://www.gamedev.net/community/forums/topic.asp?topic_id=441338
            //seems to be an official bug and we'd need to write our very own font rendering to fix that

            if (!FEnabledInput[0])
                return;

            Device dev = Device.FromPointer(new IntPtr(DXDevice.DevicePointer()));

            //from the docs: D3DXSPRITE_OBJECTSPACE -> The world, view, and projection transforms are not modified.
            //for view and projection transforms this is exactly what we want: it allows placing the text within the
            //same world as all the other objects. however we don't want to work in object space but in world space
            //that's why we need to to set the world transform to a neutral value: identity
            dev.SetTransform(TransformState.World, Matrix.Identity);
            FTransformIn.SetRenderSpace();

            //set states that are defined via upstream nodes
            FRenderStatePin.SetSliceStates(0);

            var currentids = new List<FontIdentifier>();
            DeviceHelpers dh = FDeviceHelpers[dev];
            FontIdentifier id = CreateFontIdentifier(dev, 0);
            currentids.Add(id);
            SlimDX.Direct3D9.Font f = CreateFont(id);

            dh.Sprite.Begin(SpriteFlags.ObjectSpace | SpriteFlags.DoNotAddRefTexture);
            try
            {
                int size = id.Size;
                int normalize = FNormalizeInput[0].Index;

                Matrix4x4 preScale = VMath.Scale(1, -1, 1);
                Matrix4x4 world;
                string text;
                Rectangle tmpRect = new Rectangle(0, 0, 0, 0);
                int hAlign, vAlign;
                float x, y;
                int width, height;

                for (int i = 0; i < FSpreadMax; i++)
                {
                    FontIdentifier newid = CreateFontIdentifier(dev, i);
                    if (!newid.Equals(id))
                    {
                        if (!currentids.Contains(newid))
                            currentids.Add(newid);
                        id = newid;
                        f = CreateFont(id);
                        size = id.Size;
                    }

                    text = FTextInput[i];

                    if (string.IsNullOrEmpty(text))
                        continue;

                    if (FCharEncoding[i] == "UTF8")
                    {
                        byte[] utf8bytes = Encoding.Default.GetBytes(text);
                        text = Encoding.UTF8.GetString(utf8bytes);
                    }

                    DrawTextFormat format = DrawTextFormat.NoClip | DrawTextFormat.ExpandTabs;

                    hAlign = FHorizontalAlignInput[i].Index;
                    switch (hAlign)
                    {
                        case 0: format |= DrawTextFormat.Left; break;
                        case 1: format |= DrawTextFormat.Center; break;
                        case 2: format |= DrawTextFormat.Right; break;
                    }

                    vAlign = FVerticalAlignInput[i].Index;
                    switch (vAlign)
                    {
                        case 0: format |= DrawTextFormat.Top; break;
                        case 1: format |= DrawTextFormat.VerticalCenter; break;
                        case 2: format |= DrawTextFormat.Bottom; break;
                    }

                    switch (FTextRenderingModeInput[i].Index)
                    {
                        case 0: format |= DrawTextFormat.SingleLine; break;
                        case 2: format |= DrawTextFormat.WordBreak; break;
                    }

                    tmpRect = new Rectangle(0, 0, FWidth[i], 0);
                    height = f.MeasureString(dh.Sprite, text, format, ref tmpRect);
                    width = tmpRect.Width; //FWidth[i]
                    height = tmpRect.Height;

                    switch (normalize)
                    {
                        case 1: preScale = VMath.Scale(1f / width, -1f / width, 1); break;
                        //"width" means that the texture width will have no influence on the width of the sprite. Width will be always 1.

                        case 2: preScale = VMath.Scale(1f / height, -1f / height, 1); break;
                        //"height" means that the texture height will have no influence on the height of the sprite. Height will be always 1.

                        case 3: preScale = VMath.Scale(1f / width, -1f / height, 1); break;
                        //"on" means that the particle will always be a unit quad. independant of texture size
                    }

                    FTransformIn.GetRenderWorldMatrix(i, out world);
                    dh.Sprite.Transform = (preScale * world).ToSlimDXMatrix();

                    switch (hAlign)
                    {
                        case 1: x = width / 2; break;
                        case 2: x = width; break;
                        default: x = 0; break;
                    }

                    switch (vAlign)
                    {
                        case 1: y = height / 2; break;
                        case 2: y = height; break;
                        default: y = 0; break;
                    }

                    if (FShowBrush[i])
                        dh.Sprite.Draw(dh.Texture, new Rectangle(0, 0, width, height),
                            new Vector3(x, y, -0.001f), null, new Color4(FBrushColor[i].Color.ToArgb()));

                    width = FWidth[i];
                    switch (hAlign)
                    {
                        case 1: x = width / 2; break;
                        case 2: x = width; break;
                        default: x = 0; break;
                    }
                    f.DrawString(dh.Sprite, text, new Rectangle((int)-x, (int)-y, width, height), format, (Color)FColorInput[i]);

                    FSizeOutput[i] = new Vector2D(width, height);
                }
            }
            catch (Exception e)
            {
                Logger.Log(e);
            }
            finally
            {
                dh.Sprite.End();
            }

            var keys = FFonts.FindAllKeys(k => k.Device == dev);

            foreach (var k in keys)
                if (!currentids.Contains(k))
                    RemoveFont(k);
        }
        #endregion
    }

    public static class DictionaryExtensions
    {
        public static IEnumerable<T> FindAll<K, T>(this Dictionary<K, T> dict, Predicate<K> predicate)
        {
            foreach (var k in dict.Keys)
                if (predicate(k)) yield return dict[k];
        }

        public static IEnumerable<K> FindAllKeys<K, T>(this Dictionary<K, T> dict, Predicate<K> predicate)
        {
            foreach (var k in dict.Keys)
                if (predicate(k)) yield return k;
        }
    }
}