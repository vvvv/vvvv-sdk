
using System;
using System.Linq;
using VVVV.Hosting.Pins;
using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.Streams;
using System.IO;
using VVVV.Utils.IO;
using VVVV.Utils.VMath;
using VVVV.Utils.Win32;
using VVVV.Utils.Reflection;
using System.Collections.Generic;
using System.Windows.Forms;

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
    
    class DynamicEnumInStream : EnumInStream<EnumEntry>
    {
        private readonly string FEnumName;
        
        public DynamicEnumInStream(IEnumIn enumIn, string enumName)
            : base(enumIn)
        {
            FEnumName = enumName;
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
                        int ord;
                        FEnumIn.GetOrd(i, out ord);
                        writer.Write(new EnumEntry(FEnumName, ord));
                    }
                }
            }
            return IsChanged;
        }
    }
    
    class NodeInStream<T> : MemoryIOStream<T>
    {
        private readonly INodeIn FNodeIn;
        private readonly bool FAutoValidate;
        
        public NodeInStream(INodeIn nodeIn, IConnectionHandler handler)
        {
            FNodeIn = nodeIn;
            FNodeIn.SetConnectionHandler(handler, this);
            FAutoValidate = nodeIn.AutoValidate;
        }
        
        public NodeInStream(INodeIn nodeIn)
            : this(nodeIn, new DefaultConnectionHandler())
        {
        }
        
        public override bool Sync()
        {
            IsChanged = FAutoValidate ? FNodeIn.PinIsChanged : FNodeIn.Validate();
            if (IsChanged)
            {
                Length = FNodeIn.SliceCount;
                using (var writer = GetWriter())
                {
                    object usI;
                    FNodeIn.GetUpstreamInterface(out usI);
                    var upstreamInterface = usI as IGenericIO;
                    
                    for (int i = 0; i < Length; i++)
                    {
                        int usS;
                        var result = default(T);
                        if (upstreamInterface != null)
                        {
                            FNodeIn.GetUpsreamSlice(i, out usS);
                            result = (T) upstreamInterface.GetSlice(usS);
                        }
                        writer.Write(result);
                    }
                }
            }
            return base.Sync();
        }
    }

    class KeyboardToKeyboardStateInStream : MemoryIOStream<KeyboardState>, IDisposable
    {
        private readonly INodeIn FNodeIn;
        private readonly bool FAutoValidate;
        private readonly Spread<Subscription<Keyboard, KeyNotification>> FSubscriptions = new Spread<Subscription<Keyboard, KeyNotification>>();

        public KeyboardToKeyboardStateInStream(INodeIn nodeIn)
        {
            FNodeIn = nodeIn;
            FAutoValidate = nodeIn.AutoValidate;
        }

        public void Dispose()
        {
            foreach (var subscription in FSubscriptions)
                subscription.Dispose();
        }

        public override bool Sync()
        {
            IsChanged = FAutoValidate ? FNodeIn.PinIsChanged : FNodeIn.Validate();
            if (IsChanged)
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
                                this.Buffer[slice] = keyboardState;
                            }
                        );
                    }
                );

                using (var writer = GetWriter())
                {
                    object usI;
                    FNodeIn.GetUpstreamInterface(out usI);
                    var upstreamInterface = usI as IGenericIO;

                    for (int i = 0; i < Length; i++)
                    {
                        int usS;
                        var keyboard = Keyboard.Empty;
                        if (upstreamInterface != null)
                        {
                            FNodeIn.GetUpsreamSlice(i, out usS);
                            keyboard = (Keyboard)upstreamInterface.GetSlice(usS);
                        }
                        writer.Write(KeyboardState.Empty);
                        FSubscriptions[i].Update(keyboard);
                    }
                }
            }
            return base.Sync();
        }
    }

    class MouseToMouseStateInStream : MemoryIOStream<MouseState>, IDisposable
    {
        private readonly INodeIn FNodeIn;
        private readonly bool FAutoValidate;
        private readonly Spread<Subscription<Mouse, MouseNotification>> FSubscriptions = new Spread<Subscription<Mouse, MouseNotification>>();
        private readonly Spread<int> FRawMouseWheels = new Spread<int>();
        
        public MouseToMouseStateInStream(INodeIn nodeIn)
        {
            FNodeIn = nodeIn;
            FAutoValidate = nodeIn.AutoValidate;
        }

        public void Dispose()
        {
            foreach (var subscription in FSubscriptions)
                subscription.Dispose();
        }
        
        public override bool Sync()
        {
            IsChanged = FAutoValidate ? FNodeIn.PinIsChanged : FNodeIn.Validate();
            if (IsChanged)
            {
                Length = FNodeIn.SliceCount;

                FRawMouseWheels.SliceCount = Length;
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
                                        mouseState = new MouseState(normalizedPosition.x, normalizedPosition.y, mouseState.Buttons | downNotification.Buttons, mouseState.MouseWheel);
                                        break;
                                    case MouseNotificationKind.MouseUp:
                                        var upNotification = n as MouseButtonNotification;
                                        mouseState = new MouseState(normalizedPosition.x, normalizedPosition.y, mouseState.Buttons & ~upNotification.Buttons, mouseState.MouseWheel);
                                        break;
                                    case MouseNotificationKind.MouseMove:
                                        mouseState = new MouseState(normalizedPosition.x, normalizedPosition.y, mouseState.Buttons, mouseState.MouseWheel);
                                        break;
                                    case MouseNotificationKind.MouseWheel:
                                        var wheelNotification = n as MouseWheelNotification;
                                        FRawMouseWheels[slice] += wheelNotification.WheelDelta;
                                        var wheel = (int)Math.Round((float)FRawMouseWheels[slice] / Const.WHEEL_DELTA);
                                        mouseState = new MouseState(normalizedPosition.x, normalizedPosition.y, mouseState.Buttons, wheel);
                                        break;
                                }
                                this.Buffer[slice] = mouseState;
                            }
                        );
                    }
                );

                using (var writer = GetWriter())
                {
                    object usI;
                    FNodeIn.GetUpstreamInterface(out usI);
                    var upstreamInterface = usI as IGenericIO;
                    
                    for (int i = 0; i < Length; i++)
                    {
                        int usS;
                        var mouse = Mouse.Empty;
                        if (upstreamInterface != null)
                        {
                            FNodeIn.GetUpsreamSlice(i, out usS);
                            mouse = (Mouse) upstreamInterface.GetSlice(usS);
                        }
                        writer.Write(new MouseState());
                        FSubscriptions[i].Update(mouse);
                    }
                }
            }
            return base.Sync();
        }
    }

    class RawInStream : IInStream<System.IO.Stream>
    {
        class RawInStreamReader : IStreamReader<System.IO.Stream>
        {
            private readonly RawInStream FRawInStream;

            public RawInStreamReader(RawInStream stream)
            {
                FRawInStream = stream;
            }

            public Stream Read(int stride = 1)
            {
                VVVV.Utils.Win32.IStream stream;
                FRawInStream.FRawIn.GetData(Position, out stream);
                Position += stride;
                if (stream != null)
                {
                    var result = new ComStream(stream);
                    result.Position = 0;
                    return result;
                }
                else
                    return new MemoryStream(0);
            }

            public int Read(Stream[] buffer, int offset, int length, int stride = 1)
            {
                var numSlicesToRead = StreamUtils.GetNumSlicesAhead(this, offset, length, stride);
                for (int i = offset; i < numSlicesToRead; i++)
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
        private int FLength;
        
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
