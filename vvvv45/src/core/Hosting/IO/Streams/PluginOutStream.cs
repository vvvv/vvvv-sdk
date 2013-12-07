using System;
using System.Linq;
using SlimDX.Direct3D9;
using VVVV.Hosting.Interfaces;
using VVVV.Hosting.Interfaces.EX9;
using VVVV.Hosting.Pins;
using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.PluginInterfaces.V2.EX9;
using VVVV.Utils.Streams;
using VVVV.Utils.VMath;
using SlimDX;
using System.Runtime.InteropServices;
using System.IO;
using VVVV.Utils.IO;
using System.Reactive.Subjects;
using System.Drawing;
using VVVV.Utils.Win32;
using System.Windows.Forms;

namespace VVVV.Hosting.IO.Streams
{
    class StringOutStream : MemoryIOStream<string>
    {
        private readonly IStringOut FStringOut;
        
        public StringOutStream(IStringOut stringOut)
        {
            FStringOut = stringOut;
        }

        public override void Flush(bool force = false)
        {
            if (force || IsChanged)
            {
                FStringOut.SliceCount = Length;
                for (int i = 0; i < Length; i++)
                {
                    FStringOut.SetString(i, this[i]);
                }
            }
            base.Flush(force);
        }
    }
    
    class EnumOutStream<T> : MemoryIOStream<T>
    {
        protected readonly IEnumOut FEnumOut;
        
        public EnumOutStream(IEnumOut enumOut)
        {
            FEnumOut = enumOut;
        }

        public override void Flush(bool force = false)
        {
            if (force || IsChanged)
            {
                FEnumOut.SliceCount = Length;
                for (int i = 0; i < Length; i++)
                {
                    SetSlice(i, this[i]);
                }
            }
            base.Flush(force);
        }
        
        protected virtual void SetSlice(int index, T value)
        {
            FEnumOut.SetString(index, value.ToString());
        }
    }
    
    class DynamicEnumOutStream : EnumOutStream<EnumEntry>
    {
        public DynamicEnumOutStream(IEnumOut enumOut)
            : base(enumOut)
        {
        }
        
        protected override void SetSlice(int index, EnumEntry value)
        {
            FEnumOut.SetOrd(index, value.Index);
        }
    }
    
    class NodeOutStream<T> : MemoryIOStream<T>, IGenericIO
    {
        private readonly INodeOut FNodeOut;
        
        public NodeOutStream(INodeOut nodeOut)
            : this(nodeOut, new DefaultConnectionHandler())
        {
            
        }
        
        public NodeOutStream(INodeOut nodeOut, IConnectionHandler handler)
        {
            FNodeOut = nodeOut;
            FNodeOut.SetInterface(this);
            FNodeOut.SetConnectionHandler(handler, this);
        }
        
        object IGenericIO.GetSlice(int index)
        {
            return this[VMath.Zmod(index, Length)];
        }

        public override void Flush(bool force = false)
        {
            if (force || IsChanged)
            {
                FNodeOut.SliceCount = Length;
                FNodeOut.MarkPinAsChanged();
            }
            base.Flush(force);
        }
    }

    class KeyboardStateToKeyboardOutStream : MemoryIOStream<KeyboardState>, IGenericIO, IDisposable
    {
        private readonly INodeOut FNodeOut;
        private readonly Spread<Subject<KeyNotification>> FSubjects = new Spread<Subject<KeyNotification>>();
        private readonly Spread<Keyboard> FKeyboards = new Spread<Keyboard>();
        private readonly Spread<KeyboardState> FKeyboardStates = new Spread<KeyboardState>();

        public KeyboardStateToKeyboardOutStream(INodeOut nodeOut)
        {
            FNodeOut = nodeOut;
            FNodeOut.SetInterface(this);
        }

        public void Dispose()
        {
            FSubjects.ResizeAndDispose(0);
        }

        object IGenericIO.GetSlice(int index)
        {
            return FKeyboards[index];
        }

        public override void Flush(bool force = false)
        {
            if (force || IsChanged)
            {
                FSubjects.ResizeAndDispose(Length);
                FKeyboardStates.ResizeAndDismiss(Length);
                FKeyboards.ResizeAndDismiss(
                    Length,
                    slice =>
                    {
                        var subject = FSubjects[slice];
                        return new Keyboard(subject, true);
                    }
                );
                for (int i = 0; i < Length; i++)
                {
                    var keyboardState = this.Buffer[i];
                    var previousKeyboardState = FKeyboardStates[i];
                    if (keyboardState != previousKeyboardState)
                    {
                        var subject = FSubjects[i];
                        var keyDowns = keyboardState.KeyCodes.Except(previousKeyboardState.KeyCodes);
                        foreach (var keyDown in keyDowns)
                            subject.OnNext(new KeyDownNotification(keyDown));
                        var keyUps = previousKeyboardState.KeyCodes.Except(keyboardState.KeyCodes);
                        foreach (var keyUp in keyUps)
                            subject.OnNext(new KeyUpNotification(keyUp));
                    }
                    FKeyboardStates[i] = keyboardState;
                }

                FNodeOut.SliceCount = Length;
                FNodeOut.MarkPinAsChanged();
            }
            base.Flush(force);
        }
    }

    class MouseStateToMouseOutStream : MemoryIOStream<MouseState>, IGenericIO, IDisposable
    {
        private readonly INodeOut FNodeOut;
        private readonly Spread<Subject<MouseNotification>> FSubjects = new Spread<Subject<MouseNotification>>();
        private readonly Spread<Mouse> FMouses = new Spread<Mouse>();
        private readonly Spread<MouseState> FMouseStates = new Spread<MouseState>();

        public MouseStateToMouseOutStream(INodeOut nodeOut)
        {
            FNodeOut = nodeOut;
            FNodeOut.SetInterface(this);
        }

        public void Dispose()
        {
            FSubjects.ResizeAndDispose(0);
        }

        object IGenericIO.GetSlice(int index)
        {
            return FMouses[index];
        }

        public override void Flush(bool force = false)
        {
            if (force || IsChanged)
            {
                FSubjects.ResizeAndDispose(Length);
                FMouseStates.ResizeAndDismiss(Length);
                FMouses.ResizeAndDismiss(
                    Length,
                    slice =>
                    {
                        var subject = FSubjects[slice];
                        return new Mouse(subject);
                    }
                );
                for (int i = 0; i < Length; i++)
                {
                    var mouseState = this.Buffer[i];
                    var previousMouseState = FMouseStates[i];
                    if (mouseState != previousMouseState)
                    {
                        var subject = FSubjects[i];
                        var v = new Vector2D(mouseState.X, mouseState.Y);
                        var position = ToMousePoint(v);
                        if (mouseState.X != previousMouseState.X || mouseState.Y != previousMouseState.Y)
                            subject.OnNext(new MouseMoveNotification(position, FClientArea));
                        if (mouseState.Buttons != previousMouseState.Buttons)
                        {
                            if (mouseState.IsLeft && !previousMouseState.IsLeft)
                                subject.OnNext(new MouseDownNotification(position, FClientArea, MouseButtons.Left));
                            else if (!mouseState.IsLeft && previousMouseState.IsLeft)
                                subject.OnNext(new MouseUpNotification(position, FClientArea, MouseButtons.Left));
                            if (mouseState.IsMiddle && !previousMouseState.IsMiddle)
                                subject.OnNext(new MouseDownNotification(position, FClientArea, MouseButtons.Middle));
                            else if (!mouseState.IsMiddle && previousMouseState.IsMiddle)
                                subject.OnNext(new MouseUpNotification(position, FClientArea, MouseButtons.Middle));
                            if (mouseState.IsRight && !previousMouseState.IsRight)
                                subject.OnNext(new MouseDownNotification(position, FClientArea, MouseButtons.Right));
                            else if (!mouseState.IsRight && previousMouseState.IsRight)
                                subject.OnNext(new MouseUpNotification(position, FClientArea, MouseButtons.Right));
                            if (mouseState.IsXButton1 && !previousMouseState.IsXButton1)
                                subject.OnNext(new MouseDownNotification(position, FClientArea, MouseButtons.XButton1));
                            else if (!mouseState.IsXButton1 && previousMouseState.IsXButton1)
                                subject.OnNext(new MouseUpNotification(position, FClientArea, MouseButtons.XButton1));
                            if (mouseState.IsXButton2 && !previousMouseState.IsXButton2)
                                subject.OnNext(new MouseDownNotification(position, FClientArea, MouseButtons.XButton2));
                            else if (!mouseState.IsXButton2 && previousMouseState.IsXButton2)
                                subject.OnNext(new MouseUpNotification(position, FClientArea, MouseButtons.XButton2));
                        }
                        if (mouseState.MouseWheel != previousMouseState.MouseWheel)
                        {
                            var wheelDelta = previousMouseState.MouseWheel - mouseState.MouseWheel;
                            subject.OnNext(new MouseWheelNotification(position, FClientArea, wheelDelta * Const.WHEEL_DELTA));
                        }
                    }
                    FMouseStates[i] = mouseState;
                }

                FNodeOut.SliceCount = Length;
                FNodeOut.MarkPinAsChanged();
            }
            base.Flush(force);
        }

        static Point ToMousePoint(Vector2D normV)
        {
            var clientArea = new Vector2D(FClientArea.Width - 1, FClientArea.Height - 1);
            var v = VMath.Map(normV, new Vector2D(-1, 1), new Vector2D(1, -1), Vector2D.Zero, clientArea, TMapMode.Clamp);
            return new Point((int)v.x, (int)v.y);
        }

        static Size FClientArea = new Size(short.MaxValue, short.MaxValue);
    }

    class RawOutStream : IOutStream<System.IO.Stream>
    {
        private readonly IRawOut FRawOut;
        private int FLength;
        private bool FMarkPinAsChanged;

        public RawOutStream(IRawOut rawOut)
        {
            FRawOut = rawOut;
        }

        public void Flush(bool force = false)
        {
            if (force || FMarkPinAsChanged)
            {
                this.FRawOut.MarkPinAsChanged();
            }
        }

        public object Clone()
        {
            throw new NotImplementedException();
        }

        public int Length
        {
            get { return this.FLength; }
            set
            {
                if (value != this.FLength)
                {
                    this.FRawOut.SliceCount = value;
                    this.FLength = value;
                }
            }
        }

        public IStreamWriter<System.IO.Stream> GetWriter()
        {
            FMarkPinAsChanged = true;
            return new Writer(this);
        }

        class Writer : IStreamWriter<System.IO.Stream>
        {
            private RawOutStream FRawOutStream;

            public Writer(RawOutStream rawOutStream)
            {
                this.FRawOutStream = rawOutStream;
            }

            public void Write(System.IO.Stream value, int stride = 1)
            {
                this.FRawOutStream.FRawOut.SetData(this.Position, new ComIStream(value));
                this.Position += stride;
            }

            public int Write(System.IO.Stream[] buffer, int index, int length, int stride = 1)
            {
                var numSlicesToWrite = StreamUtils.GetNumSlicesAhead(this, index, length, stride);
                for (int i = index; i < numSlicesToWrite; i++)
                {
                    Write(buffer[i], stride);
                }
                return numSlicesToWrite;
            }

            public void Reset()
            {
                Position = 0;
            }

            public bool Eos
            {
                get { return Position >= Length; }
            }

            public int Position
            {
                get;
                set;
            }

            public int Length
            {
                get { return this.FRawOutStream.Length; }
            }

            public void Dispose()
            {
                
            }
        }
    }

    abstract class ResourceOutStream<T, TResource, TMetadata> : MemoryIOStream<T>, IDXResourcePin
        where T : DXResource<TResource, TMetadata>
        where TResource : ComObject
    {
        void IDXResourcePin.UpdateResources(Device device)
        {
            foreach (var resource in this)
            {
                resource.UpdateResource(device);
            }
        }

        void IDXResourcePin.DestroyResources(Device device, bool onlyUnmanaged)
        {
            var isDx9ExDevice = device is DeviceEx;
            foreach (var resource in this)
            {
                // If we should destroy only unmanaged resources (those in the default pool)
                // do so only if we're on DirectX9 and the resource is in the default pool.
                // In case of DirectX9Ex where all resources are in the default pool we don't
                // need to do anything.
                if (!onlyUnmanaged || (resource.IsDefaultPool && !isDx9ExDevice))
                {
                    resource.DestroyResource(device);
                }
            }
        }
    }

    class TextureOutStream<T, TMetadata> : ResourceOutStream<T, Texture, TMetadata>, IDXTexturePin
        where T : DXResource<Texture, TMetadata>
    {
        private readonly IDXTextureOut FInternalTextureOut;
        
        public TextureOutStream(IInternalPluginHost host, OutputAttribute attribute)
        {
            FInternalTextureOut = host.CreateTextureOutput2(
                this,
                attribute.Name,
                (TSliceMode) attribute.SliceMode,
                (TPinVisibility) attribute.Visibility
               );
        }

        public override void Flush(bool force = false)
        {
            if (force || IsChanged)
            {
                FInternalTextureOut.SliceCount = Length;
                FInternalTextureOut.MarkPinAsChanged();
            }
            base.Flush(force);
        }
        
        Texture IDXTexturePin.this[Device device, int slice]
        {
            get
            {
                return this[slice][device];
            }
        }
    }

    class MeshOutStream<T, TMetadata> : ResourceOutStream<T, Mesh, TMetadata>, IDXMeshPin
        where T : DXResource<Mesh, TMetadata>
    {
        private readonly IDXMeshOut FInternalMeshOut;

        public MeshOutStream(IInternalPluginHost host, OutputAttribute attribute)
        {
            FInternalMeshOut = host.CreateMeshOutput2(
                this,
                attribute.Name,
                (TSliceMode)attribute.SliceMode,
                (TPinVisibility)attribute.Visibility
               );
        }

        public override void Flush(bool force = false)
        {
            if (force || IsChanged)
            {
                FInternalMeshOut.SliceCount = Length;
                FInternalMeshOut.MarkPinAsChanged();
            }
            base.Flush(force);
        }

        Mesh IDXMeshPin.this[Device device, int slice]
        {
            get
            {
                return this[slice][device];
            }
        }
    }
}
