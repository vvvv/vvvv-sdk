﻿using System;
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
using VVVV.Utils.Streams;

namespace VVVV.Nodes.Input
{
    [PluginInfo(Name = "Keyboard", 
	            Category = "Devices", 
	            Version = "Window",
	            Help = "Returns the keyboard of the current render window.")]
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

    [PluginInfo(Name = "Keyboard", 
                Category = "Devices", 
                Version = "Desktop",
                Help = "Returns the systemwide keyboard.")]
    public class DesktopKeyboardNode : DesktopDeviceInputNode<Keyboard>
    {
        public DesktopKeyboardNode()
            : base(DeviceType.Keyboard, "KeyStates", "Keyboard")
        {
        }

        protected override Keyboard CreateDevice(DeviceInfo deviceInfo, int slice)
        {
            var notifications = Observable.FromEventPattern<KeyboardInputEventArgs>(typeof(Device), "KeyboardInput")
                .Where(_ => EnabledIn.SliceCount > 0 && EnabledIn[slice])
                .Where(ep => ep.EventArgs.Device == deviceInfo.Handle)
                .Select(ep => ep.EventArgs.GetCorrectedKeyboardInputEventArgs())
                .Where(args => args != null)
                .SelectMany(args => GenerateKeyNotifications(args, slice));
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

    [PluginInfo(Name = "KeyEvents", Category = "Keyboard", Version = "Join")]
    public class KeyboardEventsJoinNode : IPluginEvaluate, IDisposable
    {
        public ISpread<ISpread<KeyNotificationKind>> EventTypeIn;
        public ISpread<ISpread<int>> KeyCodeIn;
        public ISpread<ISpread<string>> KeyCharIn;
        public ISpread<Keyboard> KeyboardOut;

        private readonly Spread<Subject<KeyNotification>> FSubjects = new Spread<Subject<KeyNotification>>();
        private IIOContainer<IInStream<int>> BinSizePin;

        [ImportingConstructor]
        public KeyboardEventsJoinNode(IIOFactory factory)
        {
            BinSizePin = factory.CreateBinSizeInput(new InputAttribute("Bin Size") { DefaultValue = InputAttribute.DefaultBinSize, Order = int.MaxValue });
            EventTypeIn = BinSizePin.CreateBinSizeSpread<KeyNotificationKind>(new InputAttribute("Event Type"));
            KeyCodeIn = BinSizePin.CreateBinSizeSpread<int>(new InputAttribute("Key Code"));
            KeyCharIn = BinSizePin.CreateBinSizeSpread<string>(new InputAttribute("Key Char"));
            KeyboardOut = factory.CreateSpread<Keyboard>(new OutputAttribute("Keyboard"));
            KeyboardOut.SliceCount = 0;
        }

        public void Dispose()
        {
            foreach (var subject in FSubjects)
                subject.Dispose();
            BinSizePin.Dispose();
        }

        public void Evaluate(int spreadMax)
        {
            var binCount = BinSizePin.IOObject.Length;
            FSubjects.ResizeAndDispose(binCount);
            KeyboardOut.ResizeAndDismiss(binCount, slice => new Keyboard(FSubjects[slice]));
            for (int bin = 0; bin < binCount; bin++)
            {
                var subject = FSubjects[bin];
                var notificationCount = EventTypeIn[bin].SliceCount;
                for (int i = 0; i < notificationCount; i++)
                {
                    KeyNotification notification;
                    switch (EventTypeIn[bin][i])
                    {
                        case KeyNotificationKind.KeyDown:
                            notification = new KeyDownNotification((Keys)KeyCodeIn[bin][i]);
                            break;
                        case KeyNotificationKind.KeyPress:
                            var s = KeyCharIn[bin][i];
                            notification = s.Length > 0
                                ? new KeyPressNotification(s[0])
                                : null;
                            break;
                        case KeyNotificationKind.KeyUp:
                            notification = new KeyUpNotification((Keys)KeyCodeIn[bin][i]);
                            break;
                        default:
                            throw new NotImplementedException();
                    }
                    if (notification != null)
                        subject.OnNext(notification);
                }
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

        [Input("Queue Mode")]
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

    [PluginInfo(Name = "KeyEvents", Category = "Keyboard", Version = "Split", AutoEvaluate = true)]
    public class KeyboardEventsSplitNode : IPluginEvaluate, IDisposable
    {
        public ISpread<Keyboard> KeyboardIn;
        public ISpread<ISpread<KeyNotificationKind>> EventTypeOut;
        public ISpread<ISpread<int>> KeyCodeOut;
        public ISpread<ISpread<string>> KeyCharOut;

        private static readonly IList<KeyNotification> FEmptyList = new List<KeyNotification>(0);
        private Spread<Tuple<Keyboard, IEnumerator<IList<KeyNotification>>>> FEnumerators = new Spread<Tuple<Keyboard, IEnumerator<IList<KeyNotification>>>>();
        private IIOContainer<IOutStream<int>> BinSizePin;

        [ImportingConstructor]
        public KeyboardEventsSplitNode(IIOFactory factory)
        {
            KeyboardIn = factory.CreateSpread<Keyboard>(new InputAttribute("Keyboard"));
            BinSizePin = factory.CreateBinSizeOutput(new OutputAttribute("Bin Size") { Order = int.MaxValue });
            EventTypeOut = BinSizePin.CreateBinSizeSpread<KeyNotificationKind>(new OutputAttribute("Event Type"));
            KeyCodeOut = BinSizePin.CreateBinSizeSpread<int>(new OutputAttribute("Key Code"));
            KeyCharOut = BinSizePin.CreateBinSizeSpread<string>(new OutputAttribute("Key Char"));
        }

        public void Dispose()
        {
            foreach (var tuple in FEnumerators)
                Unsubscribe(tuple);
            BinSizePin.Dispose();
        }

        static Tuple<Keyboard, IEnumerator<IList<KeyNotification>>> Subscribe(Keyboard keyboard)
        {
            return Tuple.Create(
                keyboard,
                keyboard.KeyNotifications
                    .Chunkify()
                    .GetEnumerator()
            );
        }

        static void Unsubscribe(Tuple<Keyboard, IEnumerator<IList<KeyNotification>>> tuple)
        {
            tuple.Item2.Dispose();
        }

        public void Evaluate(int spreadMax)
        {
            FEnumerators.Resize(
                spreadMax,
                slice =>
                {
                    var keyboard = KeyboardIn[slice] ?? Keyboard.Empty;
                    return Subscribe(keyboard);
                },
                Unsubscribe
            );

            EventTypeOut.SliceCount = spreadMax;
            KeyCodeOut.SliceCount = spreadMax;
            KeyCharOut.SliceCount = spreadMax;

            for (int bin = 0; bin < spreadMax; bin++)
            {
                var keyboard = KeyboardIn[bin] ?? Keyboard.Empty;
                var tuple = FEnumerators[bin];
                if (keyboard != tuple.Item1)
                {
                    Unsubscribe(tuple);
                    tuple = Subscribe(keyboard);
                }

                var enumerator = tuple.Item2;
                var notifications = enumerator.MoveNext()
                    ? enumerator.Current
                    : FEmptyList;

                EventTypeOut[bin].SliceCount = notifications.Count;
                KeyCodeOut[bin].SliceCount = notifications.Count;
                KeyCharOut[bin].SliceCount = notifications.Count;

                for (int i = 0; i < notifications.Count; i++)
                {
                    var notification = notifications[i];
                    EventTypeOut[bin][i] = notification.Kind;
                    switch (notification.Kind)
                    {
                        case KeyNotificationKind.KeyDown:
                            var keyDown = notification as KeyDownNotification;
                            KeyCodeOut[bin][i] = (int)keyDown.KeyCode;
                            KeyCharOut[bin][i] = null;
                            break;
                        case KeyNotificationKind.KeyPress:
                            var keyPress = notification as KeyPressNotification;
                            KeyCodeOut[bin][i] = 0;
                            KeyCharOut[bin][i] = keyPress.KeyChar.ToString();
                            break;
                        case KeyNotificationKind.KeyUp:
                            var keyUp = notification as KeyUpNotification;
                            KeyCodeOut[bin][i] = (int)keyUp.KeyCode;
                            KeyCharOut[bin][i] = null;
                            break;
                        default:
                            throw new NotImplementedException();
                    }
                }

                FEnumerators[bin] = tuple;
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
