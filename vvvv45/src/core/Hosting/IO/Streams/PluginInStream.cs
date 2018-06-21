using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using VVVV.Hosting.Pins;
using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils;
using VVVV.Utils.IO;
using VVVV.Utils.Reflection;
using VVVV.Utils.Streams;
using VVVV.Utils.VMath;
using VVVV.Utils.Win32;
using com = System.Runtime.InteropServices.ComTypes;

namespace VVVV.Hosting.IO.Streams
{
    class StringInStream : MemoryIOStream<string>
    {
        private readonly IStringIn FStringIn;
        private readonly bool FAutoValidate;
        
        public StringInStream(IStringIn stringIn)
        {
            FStringIn = stringIn;
            FAutoValidate = stringIn.AutoValidate;
        }
        
        public override bool Sync()
        {
            IsChanged = FAutoValidate ? FStringIn.PinIsChanged : FStringIn.Validate();
            if (IsChanged)
            {
                Length = FStringIn.SliceCount;
                using (var writer = GetWriter())
                {
                    for (int i = 0; i < Length; i++)
                    {
                        string result;
                        FStringIn.GetString(i, out result);
                        writer.Write(result ?? string.Empty);
                    }
                }
            }
            return base.Sync();
        }
    }

    class CharInStream : MemoryIOStream<char>
    {
        private readonly IStringIn FStringIn;
        private readonly bool FAutoValidate;

        public CharInStream(IStringIn stringIn)
        {
            FStringIn = stringIn;
            FAutoValidate = stringIn.AutoValidate;
        }

        public override bool Sync()
        {
            IsChanged = FAutoValidate ? FStringIn.PinIsChanged : FStringIn.Validate();
            if (IsChanged)
            {
                Length = FStringIn.SliceCount;
                using (var writer = GetWriter())
                {
                    for (int i = 0; i < Length; i++)
                    {
                        string result;
                        FStringIn.GetString(i, out result);
                        writer.Write(result != null && result.Length > 0 ? result[0] : char.MinValue);
                    }
                }
            }
            return base.Sync();
        }
    }

    class EnumInStream<T> : MemoryIOStream<T>
    {
        protected readonly IEnumIn FEnumIn;
        protected readonly bool FAutoValidate;
        
        public EnumInStream(IEnumIn enumIn)
        {
            FEnumIn = enumIn;
            FAutoValidate = enumIn.AutoValidate;
        }
        
        public override bool Sync()
        {
            IsChanged = FAutoValidate ? FEnumIn.PinIsChanged : FEnumIn.Validate();
            if (IsChanged)
            {
                Length = FEnumIn.SliceCount;
                using (var writer = GetWriter())
                {
                    for (int i = 0; i < Length; i++)
                    {
                        string entry;
                        FEnumIn.GetString(i, out entry);
                        try
                        {
                            writer.Write((T)Enum.Parse(typeof(T), entry));
                        }
                        catch (Exception)
                        {
                            writer.Write(default(T));
                        }
                    }
                }
            }
            return base.Sync();
        }
    }
    
    class DynamicEnumInStream : EnumInStream<EnumEntry>, IEnumChangedListener
    {
        private readonly string FEnumName;
        
        public DynamicEnumInStream(IEnumIn enumIn, string enumName)
            : base(enumIn)
        {
            FEnumName = enumName;
            enumIn.SetEnumChangedListener(this);
        }
        
        public override bool Sync()
        {
            IsChanged = FAutoValidate ? FEnumIn.PinIsChanged : FEnumIn.Validate();
            if (IsChanged)
                DoSync();
            return IsChanged;
        }

        private void DoSync()
        {
            Length = FEnumIn.SliceCount;
            using (var writer = GetWriter())
            {
                for (int i = 0; i < Length; i++)
                {
                    int ord;
                    string name;
                    FEnumIn.GetOrd(i, out ord);
                    FEnumIn.GetString(i, out name);
                    writer.Write(new EnumEntry(FEnumName, ord, name));
                }
            }
        }

        void IEnumChangedListener.EnumChangedCB(string name, string defaultEntry, string[] entries)
        {
            DoSync();
        }
    }

    unsafe class NodeInStream<T> : IInStream<T>
    {
        class ConvolutedReader : IStreamReader<T>
        {
            private readonly MemoryIOStream<T> FStream;
            private readonly int* FUpstreamSlices;
            private readonly int FLength;

            public ConvolutedReader(MemoryIOStream<T> stream, int length, int* upstreamSlices)
            {
                FStream = stream;
                FUpstreamSlices = upstreamSlices;
                FLength = length;
            }

            public T Read(int stride = 1)
            {
                var upstreamSlice = VMath.Zmod(FUpstreamSlices[Position], FStream.Length);
                Position += stride;
                return FStream.Buffer[upstreamSlice];
            }

            public int Read(T[] buffer, int offset, int length, int stride = 1)
            {
                int slicesAhead = Length - Position;

                if (stride > 1)
                {
                    int r = 0;
                    slicesAhead = Math.DivRem(slicesAhead, stride, out r);
                    if (r > 0)
                        slicesAhead++;
                }

                int numSlicesToRead = Math.Max(Math.Min(length, slicesAhead), 0);

                switch (numSlicesToRead)
                {
                    case 0:
                        return 0;
                    case 1:
                        buffer[offset] = Read(stride);
                        return 1;
                    default:
                        switch (stride)
                        {
                            case 0:
                                if (offset == 0 && numSlicesToRead == buffer.Length)
                                    buffer.Init(Read(stride)); // Slightly faster
                                else
                                    buffer.Fill(offset, numSlicesToRead, Read(stride));
                                break;
                            default:
                                Debug.Assert(Position + numSlicesToRead <= Length);
                                int* position = FUpstreamSlices + Position;
                                for (int i = offset; i < numSlicesToRead; i++)
                                {
                                    var upstreamSlice = VMath.Zmod(*position, FStream.Length);
                                    buffer[i] = FStream.Buffer[upstreamSlice];
                                    position += stride;
                                }
                                Position += numSlicesToRead * stride;
                                break;
                        }
                        break;
                }

                return numSlicesToRead;
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
                get { return FLength; }
            }

            public void Dispose()
            {
                
            }

            public T Current
            {
                get;
                private set;
            }

            object System.Collections.IEnumerator.Current
            {
                get { return Current; }
            }

            public bool MoveNext()
            {
                var result = !Eos;
                if (result)
                {
                    Current = Read();
                }
                return result;
            }

            public void Reset()
            {
                Position = 0;
            }
        }

        private MemoryIOStream<T> FNullStream = new MemoryIOStream<T>();
        private readonly INodeIn FNodeIn;
        private readonly bool FAutoValidate;
        private readonly T FDefaultValue;
        private MemoryIOStream<T> FUpstreamStream;
        private int FLength;
        private int* FUpStreamSlices;

        public NodeInStream(INodeIn nodeIn, IConnectionHandler handler, T defaultValue = default(T))
        {
            handler = handler ?? new DefaultConnectionHandler(null, typeof(T));
            FNodeIn = nodeIn;
            object inputInterface;
            if (typeof(T).UsesDynamicAssembly())
                inputInterface = new DynamicTypeWrapper(this);
            else
                inputInterface = this;
            FNodeIn.SetConnectionHandler(handler, inputInterface);
            FAutoValidate = nodeIn.AutoValidate;
            FDefaultValue = defaultValue;
            FUpstreamStream = FNullStream;
        }

        public NodeInStream(INodeIn nodeIn)
            : this(nodeIn, null)
        {
        }

        public IStreamReader<T> GetReader()
        {
            if (FNodeIn.IsConvoluted || FUpstreamStream.Length != FLength)
                return new ConvolutedReader(FUpstreamStream, FLength, FUpStreamSlices);
            return FUpstreamStream.GetReader();
        } 

        public int Length
        {
            get { return FLength; }
        }

        public object Clone()
        {
            throw new NotImplementedException();
        }

        public bool Sync()
        {
            IsChanged = FAutoValidate ? FNodeIn.PinIsChanged : FNodeIn.Validate();
            if (IsChanged)
            {
                object usI;
                FNodeIn.GetUpstreamInterface(out usI);

                FNodeIn.GetUpStreamSlices(out FLength, out FUpStreamSlices);
                // Check fastest way first: TUpstream == T 
                var wrapper = usI as DynamicTypeWrapper;
                if (wrapper != null)
                    usI = wrapper.Value;

                FUpstreamStream = usI as MemoryIOStream<T>;
                if (FUpstreamStream == null)
                {
                    // TUpstream is a subtype of T
                    // Copy the upstream stream through the covariant IEnumerable interface
                    var enumerable = usI as IEnumerable<T>;
                    if (enumerable != null)
                        FUpstreamStream = enumerable.ToStream();
                    if (FUpstreamStream == null)
                    {
                        // TUpstream to T needs explicit cast
                        // For example TUpstream is a value type and T is a reference type
                        var objects = usI as IEnumerable;
                        if (objects != null)
                            FUpstreamStream = objects.Cast<T>().ToStream();
                        else
                        {
                            // Not connected
                            FUpstreamStream = FNullStream;
                            FUpstreamStream.Length = FLength;
                            using (var writer = FUpstreamStream.GetWriter())
                                while (!writer.Eos)
                                    writer.Write(FDefaultValue);
                        }
                    }
                }
            }
            return IsChanged;
        }

        public bool IsChanged
        {
            get;
            private set;
        }

        public IEnumerator<T> GetEnumerator()
        {
            return GetReader();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetReader();
        }
    }

    class KeyboardToKeyboardStateInStream : MemoryIOStream<KeyboardState>, IDisposable
    {
        private readonly IIOFactory FFactory;
        private readonly INodeIn FNodeIn;
        private readonly bool FAutoValidate;
        private readonly Spread<Subscription<Keyboard, KeyNotification>> FSubscriptions = new Spread<Subscription<Keyboard, KeyNotification>>();

        public KeyboardToKeyboardStateInStream(IIOFactory factory, INodeIn nodeIn)
        {
            FFactory = factory;
            FNodeIn = nodeIn;
            FAutoValidate = nodeIn.AutoValidate;
            FFactory.Flushing += FFactory_Flushing;
        }

        public void Dispose()
        {
            FFactory.Flushing -= FFactory_Flushing;
            foreach (var subscription in FSubscriptions)
                subscription.Dispose();
        }

        void FFactory_Flushing(object sender, EventArgs e)
        {
            // Reset IsChanged flag
            Flush();
        }

        public override bool Sync()
        {
            var nodeConnectionChanged = FAutoValidate ? FNodeIn.PinIsChanged : FNodeIn.Validate();
            if (nodeConnectionChanged)
            {
                Length = FNodeIn.SliceCount;

                FSubscriptions.ResizeAndDispose(
                    Length,
                    slice =>
                    {
                        return new Subscription<Keyboard, KeyNotification>(
                            keyboard => keyboard.KeyNotifications,
                            (keyboard, n) =>
                            {
                                var keyboardState = this.Buffer[slice];
                                IEnumerable<Keys> keys;
                                switch (n.Kind)
                                {
                                    case KeyNotificationKind.KeyDown:
                                        var downNotification = n as KeyDownNotification;
                                        keys = keyboardState.KeyCodes.Concat(new [] { downNotification.KeyCode });
                                        keyboardState = new KeyboardState(keys, keyboard.CapsLock, keyboardState.Time + 1);
                                        break;
                                    case KeyNotificationKind.KeyUp:
                                        var upNotification = n as KeyUpNotification;
                                        keys = keyboardState.KeyCodes.Except(new[] { upNotification.KeyCode });
                                        keyboardState = new KeyboardState(keys, keyboardState.CapsLock, keyboardState.Time + 1);
                                        break;
                                }
                                SetKeyboardState(slice, keyboardState);
                            }
                        );
                    }
                );

                object usI;
                FNodeIn.GetUpstreamInterface(out usI);
                var stream = usI as MemoryIOStream<Keyboard>;

                for (int i = 0; i < Length; i++)
                {
                    int usS;
                    var keyboard = Keyboard.Empty;
                    if (stream != null)
                    {
                        FNodeIn.GetUpsreamSlice(i, out usS);
                        keyboard = stream[usS];
                    }
                    if (FSubscriptions[i].Update(keyboard))
                        SetKeyboardState(i, KeyboardState.Empty);
                }
            }
            return base.Sync();
        }

        private void SetKeyboardState(int i, KeyboardState keyboardState)
        {
            if (this.Buffer[i] != keyboardState)
                this.IsChanged = true;
            this.Buffer[i] = keyboardState;
        }
    }

    class MouseToMouseStateInStream : MemoryIOStream<MouseState>, IDisposable
    {
        private readonly IIOFactory FFactory;
        private readonly INodeIn FNodeIn;
        private readonly bool FAutoValidate;
        private readonly Spread<Subscription<Mouse, MouseNotification>> FSubscriptions = new Spread<Subscription<Mouse, MouseNotification>>();
        private readonly Spread<int> FRawMouseWheels = new Spread<int>();
        private readonly Spread<int> FRawMouseHWheels = new Spread<int>();

        public MouseToMouseStateInStream(IIOFactory factory, INodeIn nodeIn)
        {
            FFactory = factory;
            FNodeIn = nodeIn;
            FAutoValidate = nodeIn.AutoValidate;
            FFactory.Flushing += FFactory_Flushing;
        }

        public void Dispose()
        {
            FFactory.Flushing -= FFactory_Flushing;
            foreach (var subscription in FSubscriptions)
                subscription.Dispose();
        }

        void FFactory_Flushing(object sender, EventArgs e)
        {
            // Reset IsChanged flag
            Flush();
        }
        
        public override bool Sync()
        {
            var nodeConnectionChanged = FAutoValidate ? FNodeIn.PinIsChanged : FNodeIn.Validate();
            if (nodeConnectionChanged)
            {
                Length = FNodeIn.SliceCount;

                FRawMouseWheels.SliceCount = Length;
                FRawMouseHWheels.SliceCount = Length;
                FSubscriptions.ResizeAndDispose(
                    Length,
                    slice =>
                    {
                        return new Subscription<Mouse, MouseNotification>(
                            mouse => mouse.MouseNotifications,
                            (mouse, n) =>
                            {
                                var position = new Vector2D(n.Position.X, n.Position.Y);
                                var clientArea = new Vector2D(n.ClientArea.Width - 1, n.ClientArea.Height - 1);
                                var normalizedPosition = VMath.Map(position, Vector2D.Zero, clientArea, new Vector2D(-1, 1), new Vector2D(1, -1), TMapMode.Float);

                                var mouseState = this.Buffer[slice];
                                switch (n.Kind)
                                {
                                    case MouseNotificationKind.MouseDown:
                                        var downNotification = n as MouseButtonNotification;
                                        mouseState = new MouseState(normalizedPosition.x, normalizedPosition.y, mouseState.Buttons | downNotification.Buttons, mouseState.MouseWheel, mouseState.MouseHorizontalWheel);
                                        break;
                                    case MouseNotificationKind.MouseUp:
                                        var upNotification = n as MouseButtonNotification;
                                        mouseState = new MouseState(normalizedPosition.x, normalizedPosition.y, mouseState.Buttons & ~upNotification.Buttons, mouseState.MouseWheel, mouseState.MouseHorizontalWheel);
                                        break;
                                    case MouseNotificationKind.MouseMove:
                                        mouseState = new MouseState(normalizedPosition.x, normalizedPosition.y, mouseState.Buttons, mouseState.MouseWheel, mouseState.MouseHorizontalWheel);
                                        break;
                                    case MouseNotificationKind.MouseWheel:
                                        var wheelNotification = n as MouseWheelNotification;
                                        FRawMouseWheels[slice] += wheelNotification.WheelDelta;
                                        var wheel = (int)Math.Round((float)FRawMouseWheels[slice] / Const.WHEEL_DELTA);
                                        mouseState = new MouseState(normalizedPosition.x, normalizedPosition.y, mouseState.Buttons, wheel, mouseState.MouseHorizontalWheel);
                                        break;
                                    case MouseNotificationKind.MouseHorizontalWheel:
                                        var hwheelNotification = n as MouseHorizontalWheelNotification;
                                        FRawMouseHWheels[slice] += hwheelNotification.WheelDelta;
                                        var hwheel = (int)Math.Round((float)FRawMouseHWheels[slice] / Const.WHEEL_DELTA);
                                        mouseState = new MouseState(normalizedPosition.x, normalizedPosition.y, mouseState.Buttons, mouseState.MouseWheel, hwheel);
                                        break;
                                    case MouseNotificationKind.DeviceLost:
                                        mouseState = new MouseState();
                                        break;
                                }
                                SetMouseState(slice, ref mouseState);
                            }
                        );
                    }
                );

                object usI;
                FNodeIn.GetUpstreamInterface(out usI);
                var stream = usI as MemoryIOStream<Mouse>;

                for (int i = 0; i < Length; i++)
                {
                    int usS;
                    var mouse = Mouse.Empty;
                    if (stream != null)
                    {
                        FNodeIn.GetUpsreamSlice(i, out usS);
                        mouse = stream[usS];
                    }
                    if (FSubscriptions[i].Update(mouse))
                    {
                        var emptyMouseState = new MouseState();
                        SetMouseState(i, ref emptyMouseState);
                    }
                }
            }
            return base.Sync();
        }

        private void SetMouseState(int i, ref MouseState mouseState)
        {
            if (this.Buffer[i] != mouseState)
                this.IsChanged = true;
            this.Buffer[i] = mouseState;
        }
    }

    class RawInStream : IInStream<Stream>
    {
        class RawInStreamReader : IStreamReader<Stream>
        {
            private readonly RawInStream FRawInStream;
            private readonly IRawIn FRawIn;

            public RawInStreamReader(RawInStream stream)
            {
                FRawInStream = stream;
                FRawIn = stream.FRawIn;
            }

            public Stream Read(int stride = 1)
            {
                com.IStream comStream;
                FRawIn.GetData(Position, out comStream);
                Position += stride;
                if (comStream != null)
                {
                    var stream = comStream as Stream;
                    if (stream != null)
                        return stream;
                    return new ComAdapterStream(comStream);
                }
                return EmptyComStream.Instance;
            }

            public int Read(Stream[] buffer, int offset, int length, int stride = 1)
            {
                var numSlicesToRead = StreamUtils.GetNumSlicesAhead(this, offset, length, stride);
                for (int i = offset; i < offset + numSlicesToRead; i++)
                    buffer[i] = Read(stride);
                return numSlicesToRead;
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
                get { return FRawInStream.Length; }
            }

            public void Dispose()
            {
                
            }

            public Stream Current
            {
                get;
                private set;
            }

            object System.Collections.IEnumerator.Current
            {
                get { return Current; }
            }

            public bool MoveNext()
            {
                var result = !Eos;
                if (result)
                {
                    Current = Read();
                }
                return result;
            }

            public void Reset()
            {
                Position = 0;
            }
        }

        private readonly IRawIn FRawIn;
        private readonly bool FAutoValidate;
        
        public RawInStream(IRawIn rawIn)
        {
            this.FRawIn = rawIn;
            this.FAutoValidate = rawIn.AutoValidate;
            this.Length = rawIn.SliceCount;
        }

        public bool IsChanged { get; private set; }
        public int Length { get; private set; }
        
        public bool Sync()
        {
            IsChanged = FAutoValidate ? FRawIn.PinIsChanged : FRawIn.Validate();
            if (IsChanged)
            {
                Length = FRawIn.SliceCount;
            }
            return IsChanged;
        }

        public IStreamReader<Stream> GetReader()
        {
            return new RawInStreamReader(this);
        }

        public object Clone()
        {
            throw new NotImplementedException();
        }

        public System.Collections.Generic.IEnumerator<Stream> GetEnumerator()
        {
            return GetReader();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
