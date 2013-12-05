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
    [PluginInfo(Name = "Keyboard", Category = "Devices", Version = "Window")]
    public class KeyboardNode : WindowMessageNode, IPluginEvaluate
    {
        [Output("Device", IsSingle = true)]
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
            var nodeInfo = FIOFactory.NodeInfos.First(n => n.Name == "KeyStates" && n.Category == "Keyboard" && n.Version == "Split");
            FKeyboardStatesSplitNode = FIOFactory.CreatePlugin(nodeInfo, c => c.IOAttribute.Name == "Keyboard", c => KeyboardOut);
        }

        public override void Dispose()
        {
            FKeyboardStatesSplitNode.Dispose();
            base.Dispose();
        }

        public void Evaluate(int spreadMax)
        {
            KeyboardOut[0].CapsLock = Control.IsKeyLocked(Keys.CapsLock);
            // Evaluate our split plugin
            FKeyboardStatesSplitNode.Evaluate(spreadMax);
        }
    }

    [PluginInfo(Name = "Keyboard", Category = "Devices", Version = "Desktop", Bugs = "Not spreadable")]
    public class GlobalKeyboardNode : GlobalDeviceInputNode<Keyboard>
    {
        public GlobalKeyboardNode()
            : base(DeviceType.Keyboard, "KeyStates", "Keyboard")
        {
        }

        protected override Keyboard CreateDevice(DeviceInfo deviceInfo, int slice)
        {
            var notifications = Observable.FromEventPattern<KeyboardInputEventArgs>(typeof(Device), "KeyboardInput")
                .Where(_ => EnabledIn.SliceCount > 0 && EnabledIn[slice])
                .Where(ep => ep.EventArgs.Device == deviceInfo.Handle)
                .SelectMany<EventPattern<KeyboardInputEventArgs>, KeyNotification>(ep => GenerateKeyNotifications(ep.EventArgs, slice));
            return new Keyboard(notifications, true);
        }

        private IEnumerable<KeyNotification> GenerateKeyNotifications(KeyboardInputEventArgs args, int slice)
        {
            DeviceOut[slice].CapsLock = Control.IsKeyLocked(Keys.CapsLock);
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
    }

    [PluginInfo(Name = "KeyEvents", Category = "Keyboard", Version = "Join", Bugs = "Not spreadable")]
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

    [PluginInfo(Name = "KeyStates", Category = "Keyboard", Version = "Split", AutoEvaluate = true)]
    public class KeyboardStatesSplitNode : IPluginEvaluate, IDisposable
    {
        class KeyNotificationComparer : IEqualityComparer<KeyNotification>
        {
            public bool Equals(KeyNotification x, KeyNotification y)
            {
                if (x.Kind == y.Kind)
                {
                    if (x.Kind == KeyNotificationKind.KeyPress)
                    {
                        var xPress = x as KeyPressNotification;
                        var yPress = y as KeyPressNotification;
                        return xPress.KeyChar == yPress.KeyChar;
                    }
                    else
                    {
                        var xCode = x as KeyCodeNotification;
                        var yCode = y as KeyCodeNotification;
                        return xCode.KeyCode == yCode.KeyCode;
                    }
                }
                return false;
            }

            public int GetHashCode(KeyNotification obj)
            {
                return obj.GetHashCode();
            }
        }

        [Input("Keyboard")]
        public ISpread<Keyboard> KeyboardIn;

        [Input("Schedule Mode")]
        public ISpread<ScheduleMode> ScheduleModeIn;

        [Output("Key Name")]
        public ISpread<ISpread<string>> KeyNameOut;

        [Output("Key Code")]
        public ISpread<ISpread<int>> KeyCodeOut;

        [Output("Key Char")]
        public ISpread<string> KeyCharOut;

        Spread<FrameBasedScheduler> FSchedulers = new Spread<FrameBasedScheduler>();
        Spread<Subscription<Keyboard, KeyNotification>> FSubscriptions = new Spread<Subscription<Keyboard, KeyNotification>>();

        public void Dispose()
        {
            foreach (var subscription in FSubscriptions)
                subscription.Dispose();
            FSubscriptions.SliceCount = 0;
        }

        public void Evaluate(int spreadMax)
        {
            KeyNameOut.SliceCount = spreadMax;
            KeyCodeOut.SliceCount = spreadMax;
            KeyCharOut.SliceCount = spreadMax;

            FSchedulers.ResizeAndDismiss(spreadMax, () => new FrameBasedScheduler());
            FSubscriptions.ResizeAndDispose(
                spreadMax,
                slice =>
                {
                    return new Subscription<Keyboard, KeyNotification>(
                        keyboard =>
                        {
                            return keyboard.KeyNotifications
                                .DistinctUntilChanged(new KeyNotificationComparer());
                        },
                        (keyboard, n) =>
                        {
                            var keyCodeOut = KeyCodeOut[slice];
                            var keyNameOut = KeyNameOut[slice];
                            switch (n.Kind)
                            {
                                case KeyNotificationKind.KeyDown:
                                    var keyDown = n as KeyDownNotification;
                                    if (!keyCodeOut.Contains((int)keyDown.KeyCode))
                                    {
                                        keyCodeOut.Add((int)keyDown.KeyCode);
                                        keyNameOut.Add(keyDown.KeyCode.ToString());
                                    }
                                    break;
                                case KeyNotificationKind.KeyPress:
                                    var keyPress = n as KeyPressNotification;
                                    KeyCharOut[slice] = new string(keyPress.KeyChar, 1);
                                    break;
                                case KeyNotificationKind.KeyUp:
                                    var keyUp = n as KeyUpNotification;
                                    keyCodeOut.RemoveAll(k => k == (int)keyUp.KeyCode);
                                    keyNameOut.RemoveAll(k => k == keyUp.KeyCode.ToString());
                                    KeyCharOut[slice] = string.Empty;
                                    break;
                                default:
                                    throw new NotImplementedException();
                            }
                        },
                        FSchedulers[slice]
                    );
                }
            );

            for (int i = 0; i < spreadMax; i++)
            {
                //resubsribe if necessary
                FSubscriptions[i].Update(KeyboardIn[i]);
                //process events
                FSchedulers[i].Run(ScheduleModeIn[i]);
            }
        }
    }

    [PluginInfo(Name = "KeyEvents", Category = "Keyboard", Version = "Split", AutoEvaluate = true, Bugs = "Not spreadable")]
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

    [PluginInfo(Name = "AsKeyName", Category = "Value", Tags = "keyboard, convert")]
    public class KeyCodeAsKey : IPluginEvaluate
    {
        [Input("Key Code")]
        public IDiffSpread<int> KeyCodeIn;

        [Output("Key Name")]
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
        [Input("Key Name")]
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
                            if (Enum.TryParse<Keys>(s, true, out result))
                                return (int)result;
                            else
                                return (int)Keys.None;
                        }
                    )
                );
        }
    }
}
