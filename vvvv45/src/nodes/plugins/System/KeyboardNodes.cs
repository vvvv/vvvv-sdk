using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.IO;
using System.ComponentModel.Composition;
using VVVV.Utils.Win32;
using VVVV.Hosting.Graph;
using System.Windows.Forms;
using WindowsInput;
using VVVV.Hosting;
using VVVV.Hosting.IO;
using System.Diagnostics;
using VVVV.PluginInterfaces.V1;
using System.Reactive.Subjects;
using System.Reactive;
using System.Reactive.Linq;
using SharpDX.RawInput;
using SharpDX.Multimedia;

namespace VVVV.Nodes.Input
{
    [PluginInfo(Name = "Keyboard", Category = "System")]
    public class KeyboardNode : WindowMessageNode, IPluginEvaluate
    {
        [Output("Keyboard", IsSingle = true)]
        public ISpread<Keyboard> KeyboardOut;

        private PluginContainer FKeyboardStatesSplitNode;

        protected override void Initialize(IObservable<WMEventArgs> windowMessages)
        {
            var keyNotifications = windowMessages
                .Select<WMEventArgs, KeyNotification>(e =>
                {
                    switch (e.Message)
                    {
                        case WM.KEYDOWN:
                        case WM.SYSKEYDOWN:
                            return new KeyDownNotification((Keys)e.WParam);
                        case WM.CHAR:
                        case WM.SYSCHAR:
                            return new KeyPressNotification((char)e.WParam);
                        case WM.KEYUP:
                        case WM.SYSKEYUP:
                            return new KeyUpNotification((Keys)e.WParam);
                    }
                    return null;
                }
                )
                .OfType<KeyNotification>();
            KeyboardOut[0] = new Keyboard(keyNotifications);

            // Create a keyboard states node for us and connect our keyboard out to its keyboard in
            var nodeInfo = FIOFactory.NodeInfos.First(n => n.Name == "KeyboardStates" && n.Category == "System" && n.Version == "Split");
            FKeyboardStatesSplitNode = FIOFactory.CreatePlugin(nodeInfo, c => c.IOAttribute.Name == "Keyboard", c => KeyboardOut);
        }

        public override void Dispose()
        {
            FKeyboardStatesSplitNode.Dispose();
            base.Dispose();
        }

        public void Evaluate(int spreadMax)
        {
            // Evaluate our split plugin
            FKeyboardStatesSplitNode.Evaluate(spreadMax);
        }
    }

    [PluginInfo(Name = "Keyboard", Category = "System", Version = "Global")]
    public class GlobalKeyboardNode : IPluginEvaluate, IPartImportsSatisfiedNotification, IDisposable
    {
        [Input("Enabled", DefaultBoolean = true, IsSingle = true)]
        public ISpread<bool> EnabledIn;

        [Input("Index", IsSingle = true)]
        public IDiffSpread<int> IndexIn;

        [Output("Keyboard", IsSingle = true)]
        public ISpread<Keyboard> KeyboardOut;

        [Import]
        protected IOFactory FIOFactory;
        private PluginContainer FKeyboardStatesSplitNode;

        public void OnImportsSatisfied()
        {
            RawInputService.DevicesChanged += RawKeyboardService_DevicesChanged;
            IndexIn.Changed += IndexIn_Changed;
            SubscribeToDevices();

            // Create a keyboard states node for us and connect our keyboard out to its keyboard in
            var nodeInfo = FIOFactory.NodeInfos.First(n => n.Name == "KeyboardStates" && n.Category == "System" && n.Version == "Split");
            FKeyboardStatesSplitNode = FIOFactory.CreatePlugin(nodeInfo, c => c.IOAttribute.Name == "Keyboard", c => KeyboardOut);
        }

        void IndexIn_Changed(IDiffSpread<int> spread)
        {
            SubscribeToDevices();
        }

        void RawKeyboardService_DevicesChanged(object sender, EventArgs e)
        {
            SubscribeToDevices();
        }

        public void Dispose()
        {
            IndexIn.Changed -= IndexIn_Changed;
            RawInputService.DevicesChanged -= RawKeyboardService_DevicesChanged;
            FKeyboardStatesSplitNode.Dispose();
        }

        private void SubscribeToDevices()
        {
            var keyboardDevices = Device.GetDevices()
                .Where(d => d.DeviceType == DeviceType.Keyboard)
                .OrderBy(d => d, new DeviceComparer())
                .ToList();
            var index = IndexIn.SliceCount > 0 ? IndexIn[0] : 0;
            if (keyboardDevices.Count > 0)
            {
                var keyboardDevice = keyboardDevices[index % keyboardDevices.Count];
                KeyboardOut.SliceCount = 1;
                KeyboardOut[0] = CreateKeyboard(keyboardDevice, 0);
            }
            else
            {
                KeyboardOut.SliceCount = 0;
            }
            //KeyboardOut.SliceCount = keyboardDevices.Count;
            //for (int i = 0; i < keyboardDevices.Count; i++)
            //{
            //    KeyboardOut[i] = CreateKeyboard(keyboardDevices[i], i);
            //}
        }

        private Keyboard CreateKeyboard(DeviceInfo deviceInfo, int slice)
        {
            var notifications = Observable.FromEventPattern<KeyboardInputEventArgs>(typeof(Device), "KeyboardInput")
                .Where(_ => EnabledIn.SliceCount > 0 && EnabledIn[slice])
                .Where(ep => ep.EventArgs.Device == deviceInfo.Handle)
                .SelectMany<EventPattern<KeyboardInputEventArgs>, KeyNotification>(ep => GenerateKeyNotifications(ep.EventArgs));
            return new Keyboard(notifications, true);
        }

        private IEnumerable<KeyNotification> GenerateKeyNotifications(KeyboardInputEventArgs args)
        {
            var key = args.Key;
            switch (args.State)
            {
                case KeyState.KeyDown:
                case KeyState.SystemKeyDown:
                    if (key == Keys.Menu)
                    {
                        // We need to add the CONTROL key in case of right ALT key
                        if ((args.ScanCodeFlags & ScanCodeFlags.E0) > 0)
                            yield return new KeyDownNotification(Keys.ControlKey);
                    }
                    yield return new KeyDownNotification(key);
                    break;
                case KeyState.KeyUp:
                case KeyState.SystemKeyUp:
                    yield return new KeyUpNotification(key);
                    if (key == Keys.Menu)
                    {
                        // We need to add the CONTROL key in case of right ALT key
                        if ((args.ScanCodeFlags & ScanCodeFlags.E0) > 0)
                            yield return new KeyUpNotification(Keys.ControlKey);
                    }
                    break;
            }
            yield break;
        }

        public void Evaluate(int spreadMax)
        {
            // Evaluate our split plugin
            FKeyboardStatesSplitNode.Evaluate(spreadMax);
        }
    }

    [PluginInfo(Name = "KeyboardEvents", Category = "System", Version = "Join")]
    public class KeyboardEventsJoinNode : IPluginEvaluate, IPartImportsSatisfiedNotification
    {
        [Input("Event Type")]
        public ISpread<KeyNotificationKind> FEventTypeIn;

        [Input("Key Code")]
        public ISpread<int> FKeyCodeIn;

        [Input("Key Char")]
        public ISpread<string> FKeyCharIn;

        [Output("Keyboard", IsSingle = true)]
        public ISpread<Keyboard> FOutput;

        private readonly Subject<KeyNotification> FSubject = new Subject<KeyNotification>();

        public void OnImportsSatisfied()
        {
            FOutput[0] = new Keyboard(FSubject);
        }

        public void Evaluate(int spreadMax)
        {
            for (int i = 0; i < spreadMax; i++)
            {
                KeyNotification notification;
                switch (FEventTypeIn[i])
                {
                    case KeyNotificationKind.KeyDown:
                        notification = new KeyDownNotification((Keys)FKeyCodeIn[i]);
                        break;
                    case KeyNotificationKind.KeyPress:
                        var s = FKeyCharIn[i];
                        notification = s.Length > 0
                            ? new KeyPressNotification(s[0])
                            : null;
                        break;
                    case KeyNotificationKind.KeyUp:
                        notification = new KeyUpNotification((Keys)FKeyCodeIn[i]);
                        break;
                    default:
                        throw new NotImplementedException();
                }
                if (notification != null)
                    FSubject.OnNext(notification);
            }
        }
    }

    [PluginInfo(Name = "KeyboardStates", Category = "System", Version = "Split", AutoEvaluate = true)]
    public class KeyboardStatesSplitNode : IPluginEvaluate, IPartImportsSatisfiedNotification, IDisposable
    {
        class KeyCodeNotificationComparer : IEqualityComparer<KeyCodeNotification>
        {
            public bool Equals(KeyCodeNotification x, KeyCodeNotification y)
            {
                return x.Kind == y.Kind && x.KeyCode == y.KeyCode;
            }

            public int GetHashCode(KeyCodeNotification obj)
            {
                return obj.GetHashCode();
            }
        }

        [Input("Keyboard", IsSingle = true)]
        public ISpread<Keyboard> KeyboardIn;

        [Input("Schedule Mode", IsSingle = true)]
        public ISpread<ScheduleMode> ScheduleModeIn;

        [Output("Pressed Key Code")]
        public ISpread<int> KeyCodeOut;

        private readonly FrameBasedScheduler FScheduler = new FrameBasedScheduler();
        private Subscription<Keyboard, KeyCodeNotification> FSubscription;

        public void OnImportsSatisfied()
        {
            KeyCodeOut.SliceCount = 0;
            FSubscription = new Subscription<Keyboard, KeyCodeNotification>(
                keyboard => 
                {
                    return keyboard.KeyNotifications
                        .OfType<KeyCodeNotification>()
                        .DistinctUntilChanged(new KeyCodeNotificationComparer());
                },
                n =>
                {
                    switch (n.Kind)
                    {
                        case KeyNotificationKind.KeyDown:
                            if (!KeyCodeOut.Contains((int)n.KeyCode))
                                KeyCodeOut.Add((int)n.KeyCode);
                            break;
                        case KeyNotificationKind.KeyPress:
                            //ignore
                            break;
                        case KeyNotificationKind.KeyUp:
                            KeyCodeOut.RemoveAll(k => k == (int)n.KeyCode);
                            break;
                        default:
                            throw new NotImplementedException();
                    }
                },
                FScheduler
            );
        }

        public void Dispose()
        {
            FSubscription.Dispose();
        }

        public void Evaluate(int spreadMax)
        {
            //resubscribe if necessary
            FSubscription.Update(KeyboardIn[0]);
            //process events
            FScheduler.Run(ScheduleModeIn[0]);
        }
    }

    [PluginInfo(Name = "KeyboardEvents", Category = "System", Version = "Split", AutoEvaluate = true)]
    public class KeyboardEventsSplitNode : IPluginEvaluate, IDisposable
    {
        [Input("Keyboard", IsSingle = true)]
        public ISpread<Keyboard> FInput;

        [Output("Event Type")]
        public ISpread<KeyNotificationKind> FEventTypeOut;

        [Output("Key Code")]
        public ISpread<int> FKeyCodeOut;

        [Output("Key Char")]
        public ISpread<string> FKeyCharOut;

        private static readonly IList<KeyNotification> FEmptyList = new List<KeyNotification>(0);
        private Keyboard FKeyboard;
        private IEnumerator<IList<KeyNotification>> FEnumerator;

        public void Dispose()
        {
            Unsubscribe();
        }

        public void Evaluate(int spreadMax)
        {
            var keyboard = FInput[0] ?? Keyboard.Empty;
            if (keyboard != FKeyboard)
            {
                Unsubscribe();
                FKeyboard = keyboard;
                Subscribe();
            }

            var notifications = FEnumerator.MoveNext()
                ? FEnumerator.Current
                : FEmptyList;
            FEventTypeOut.SliceCount = notifications.Count;
            FKeyCodeOut.SliceCount = notifications.Count;
            FKeyCharOut.SliceCount = notifications.Count;

            for (int i = 0; i < notifications.Count; i++)
            {
                var notification = notifications[i];
                FEventTypeOut[i] = notification.Kind;
                switch (notification.Kind)
                {
                    case KeyNotificationKind.KeyDown:
                        var keyDown = notification as KeyDownNotification;
                        FKeyCodeOut[i] = (int)keyDown.KeyCode;
                        FKeyCharOut[i] = null;
                        break;
                    case KeyNotificationKind.KeyPress:
                        var keyPress = notification as KeyPressNotification;
                        FKeyCodeOut[i] = 0;
                        FKeyCharOut[i] = keyPress.KeyChar.ToString();
                        break;
                    case KeyNotificationKind.KeyUp:
                        var keyUp = notification as KeyUpNotification;
                        FKeyCodeOut[i] = (int)keyUp.KeyCode;
                        FKeyCharOut[i] = null;
                        break;
                    default:
                        throw new NotImplementedException();
                }
            }
        }

        private void Subscribe()
        {
            if (FKeyboard != null)
            {
                FEnumerator = FKeyboard.KeyNotifications
                    .Chunkify()
                    .GetEnumerator();
            }
        }

        private void Unsubscribe()
        {
            if (FEnumerator != null)
            {
                FEnumerator.Dispose();
                FEnumerator = null;
            }
        }
    }

    [PluginInfo(Name = "AsKey", Category = "Value", Tags = "keyboard, convert")]
    public class KeyCodeAsKey : IPluginEvaluate
    {
        [Input("Key Code")]
        public IDiffSpread<int> KeyCodeIn;

        [Output("Key")]
        public ISpread<string> KeyOut;

        public void Evaluate(int spreadMax)
        {
            if (KeyCodeIn.IsChanged)
                KeyOut.AssignFrom(
                    KeyCodeIn.Select(k =>
                        {
                            if (k < 0 || k > 255)
                                return Keys.None.ToString();
                            return ((Keys)k).ToString();
                        }
                    )
                );
        }
    }

    [PluginInfo(Name = "AsKeyCode", Category = "String", Tags = "keyboard, convert")]
    public class KeyAsKeyCodeNode : IPluginEvaluate
    {
        [Input("Key")]
        public IDiffSpread<string> KeyIn;

        [Output("Key Code")]
        public ISpread<int> KeyCodeOut;

        public void Evaluate(int spreadMax)
        {
            if (KeyIn.IsChanged)
                KeyCodeOut.AssignFrom(
                    KeyIn.Select(s =>
                        {
                            Keys result;
                            if (Enum.TryParse<Keys>(s, out result))
                                return (int)result;
                            else
                                return (int)Keys.None;
                        }
                    )
                );
        }
    }
}
