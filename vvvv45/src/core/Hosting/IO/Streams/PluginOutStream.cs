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
using com = System.Runtime.InteropServices.ComTypes;

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

    class CharOutStream : MemoryIOStream<char>
    {
        private readonly IStringOut FStringOut;

        public CharOutStream(IStringOut stringOut)
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
                    var value = this[i];
                    FStringOut.SetString(i, value.ToString());
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
            FEnumOut.SetString(index, value.Name);
        }
    }

    internal static class DynamicAssemblyTypeHelpers
    {
        public static bool UsesDynamicAssembly(this Type type)
        {
            return type.Assembly.IsDynamic || (type.IsGenericType && type.GetGenericArguments().Any(t => UsesDynamicAssembly(t)));
        }
    }
    
    class NodeOutStream<T> : MemoryIOStream<T>, IGenericIO
    {
        private readonly INodeOut FNodeOut;
        
        public NodeOutStream(INodeOut nodeOut)
            : this(nodeOut, null)
        {}
        
        public NodeOutStream(INodeOut nodeOut, IConnectionHandler handler)
        {
            FNodeOut = nodeOut;
            if (typeof(T).UsesDynamicAssembly())
                FNodeOut.SetInterface(new DynamicTypeWrapper(this));
            else
                FNodeOut.SetInterface(this);
            if (handler != null)
                FNodeOut.SetConnectionHandler(handler, null);
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

    class KeyboardStateToKeyboardOutStream : MemoryIOStream<KeyboardState>, IDisposable
    {
        private readonly INodeOut FNodeOut;
        private readonly Spread<Subject<KeyNotification>> FSubjects = new Spread<Subject<KeyNotification>>();
        private readonly Spread<Keyboard> FKeyboards = new Spread<Keyboard>();
        private readonly Spread<KeyboardState> FKeyboardStates = new Spread<KeyboardState>();

        public KeyboardStateToKeyboardOutStream(INodeOut nodeOut)
        {
            FNodeOut = nodeOut;
            FNodeOut.SetInterface(FKeyboards.Stream);
            SetLength(nodeOut.SliceCount);
        }

        public void Dispose()
        {
            FSubjects.ResizeAndDispose(0);
        }

        public override void Flush(bool force = false)
        {
            if (force || IsChanged)
            {
                SetLength(Length);
                for (int i = 0; i < Length; i++)
                {
                    var keyboardState = this.Buffer[i];
                    var previousKeyboardState = FKeyboardStates[i];
                    if (keyboardState != previousKeyboardState)
                    {
                        var subject = FSubjects[i];
                        var keyDowns = keyboardState.KeyCodes.Except(previousKeyboardState.KeyCodes);
                        foreach (var keyDown in keyDowns)
                            subject.OnNext(new KeyDownNotification(keyDown, this));
                        var keyUps = previousKeyboardState.KeyCodes.Except(keyboardState.KeyCodes);
                        foreach (var keyUp in keyUps)
                            subject.OnNext(new KeyUpNotification(keyUp, this));
                    }
                    FKeyboardStates[i] = keyboardState;
                }

                FNodeOut.SliceCount = Length;
                FNodeOut.MarkPinAsChanged();
            }
            base.Flush(force);
        }

        private void SetLength(int length)
        {
            if (length != Length)
            {
                FSubjects.ResizeAndDispose(length);
                FKeyboardStates.ResizeAndDismiss(length);
                FKeyboards.ResizeAndDismiss(
                    length,
                    slice =>
                    {
                        var subject = FSubjects[slice];
                        return new Keyboard(subject, true);
                    }
                );
                this.ResizeAndDismiss(length, () => KeyboardState.Empty);
            }
        }
    }

    class MouseStateToMouseOutStream : MemoryIOStream<MouseState>, IDisposable
    {
        private readonly INodeOut FNodeOut;
        private readonly Spread<Subject<MouseNotification>> FSubjects = new Spread<Subject<MouseNotification>>();
        private readonly Spread<Mouse> FMouses = new Spread<Mouse>();
        private readonly Spread<MouseState> FMouseStates = new Spread<MouseState>();

        public MouseStateToMouseOutStream(INodeOut nodeOut)
        {
            FNodeOut = nodeOut;
            FNodeOut.SetInterface(FMouses.Stream);
            SetLength(nodeOut.SliceCount);
        }

        public void Dispose()
        {
            FSubjects.ResizeAndDispose(0);
        }

        public override void Flush(bool force = false)
        {
            if (force || IsChanged)
            {
                SetLength(Length);
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
                            subject.OnNext(new MouseMoveNotification(position, FClientArea, this));
                        if (mouseState.Buttons != previousMouseState.Buttons)
                        {
                            if (mouseState.IsLeft && !previousMouseState.IsLeft)
                                subject.OnNext(new MouseDownNotification(position, FClientArea, MouseButtons.Left, this));
                            else if (!mouseState.IsLeft && previousMouseState.IsLeft)
                                subject.OnNext(new MouseUpNotification(position, FClientArea, MouseButtons.Left, this));
                            if (mouseState.IsMiddle && !previousMouseState.IsMiddle)
                                subject.OnNext(new MouseDownNotification(position, FClientArea, MouseButtons.Middle, this));
                            else if (!mouseState.IsMiddle && previousMouseState.IsMiddle)
                                subject.OnNext(new MouseUpNotification(position, FClientArea, MouseButtons.Middle, this));
                            if (mouseState.IsRight && !previousMouseState.IsRight)
                                subject.OnNext(new MouseDownNotification(position, FClientArea, MouseButtons.Right, this));
                            else if (!mouseState.IsRight && previousMouseState.IsRight)
                                subject.OnNext(new MouseUpNotification(position, FClientArea, MouseButtons.Right, this));
                            if (mouseState.IsXButton1 && !previousMouseState.IsXButton1)
                                subject.OnNext(new MouseDownNotification(position, FClientArea, MouseButtons.XButton1, this));
                            else if (!mouseState.IsXButton1 && previousMouseState.IsXButton1)
                                subject.OnNext(new MouseUpNotification(position, FClientArea, MouseButtons.XButton1, this));
                            if (mouseState.IsXButton2 && !previousMouseState.IsXButton2)
                                subject.OnNext(new MouseDownNotification(position, FClientArea, MouseButtons.XButton2, this));
                            else if (!mouseState.IsXButton2 && previousMouseState.IsXButton2)
                                subject.OnNext(new MouseUpNotification(position, FClientArea, MouseButtons.XButton2, this));
                        }
                        if (mouseState.MouseWheel != previousMouseState.MouseWheel)
                        {
                            var wheelDelta = previousMouseState.MouseWheel - mouseState.MouseWheel;
                            subject.OnNext(new MouseWheelNotification(position, FClientArea, wheelDelta * Const.WHEEL_DELTA, this));
                        }
                    }
                    FMouseStates[i] = mouseState;
                }

                FNodeOut.SliceCount = Length;
                FNodeOut.MarkPinAsChanged();
            }
            base.Flush(force);
        }

        private void SetLength(int length)
        {
            if (length != Length)
            {
                FSubjects.ResizeAndDispose(length);
                FMouseStates.ResizeAndDismiss(length);
                FMouses.ResizeAndDismiss(
                    length,
                    slice =>
                    {
                        var subject = FSubjects[slice];
                        return new Mouse(subject);
                    }
                );
                this.ResizeAndDismiss(length, () => MouseState.Create(0, 0, false, false, false, false, false, 0));
            }
        }

        static Point ToMousePoint(Vector2D normV)
        {
            var clientArea = new Vector2D(FClientArea.Width - 1, FClientArea.Height - 1);
            var v = VMath.Map(normV, new Vector2D(-1, 1), new Vector2D(1, -1), Vector2D.Zero, clientArea, TMapMode.Clamp);
            return new Point((int)v.x, (int)v.y);
        }

        static Size FClientArea = new Size(short.MaxValue, short.MaxValue);
    }

    class RawOutStream : IOutStream<Stream>
    {
        private readonly IRawOut FRawOut;
        private int FLength;
        private bool FMarkPinAsChanged;

        public RawOutStream(IRawOut rawOut)
        {
            FRawOut = rawOut;
            FRawOut.SliceCount = FLength;
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

        public IStreamWriter<Stream> GetWriter()
        {
            FMarkPinAsChanged = true;
            return new Writer(this);
        }

        class Writer : IStreamWriter<Stream>
        {
            private readonly RawOutStream FRawOutStream;
            private readonly IRawOut FRawOut;

            public Writer(RawOutStream rawOutStream)
            {
                FRawOutStream = rawOutStream;
                FRawOut = rawOutStream.FRawOut;
            }

            public void Write(Stream value, int stride = 1)
            {
                if (value != null)
                {
                    var comStream = value as com.IStream ?? new AdapterComStream(value);
                    FRawOut.SetData(Position, comStream);
                }
                else
                    FRawOut.SetData(Position, null);
                this.Position += stride;
            }

            public int Write(Stream[] buffer, int index, int length, int stride = 1)
            {
                var numSlicesToWrite = StreamUtils.GetNumSlicesAhead(this, index, length, stride);
                for (int i = index; i < index + numSlicesToWrite; i++)
                    Write(buffer[i], stride);
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
                resource?.UpdateResource(device);
            }
        }

        void IDXResourcePin.DestroyResources(Device device, bool onlyUnmanaged)
        {
            var isDx9ExDevice = device is DeviceEx;
            foreach (var resource in this)
            {
                if (resource == null)
                    continue;
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
                return this[slice]?[device];
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
