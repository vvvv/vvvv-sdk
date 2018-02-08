using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Windows.Forms;
using VVVV.Hosting;
using VVVV.Hosting.IO;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.IO;
using VVVV.Utils.Win32;
using WindowsInput;
using WindowsInput.Native;

namespace VVVV.Nodes.Input
{
    [PluginInfo(Name = "Keyboard", Category = "System", Version = "Global Legacy2", AutoEvaluate = true)]
    public class LegacyGlobalKeyboardNode : GlobalInputNode
    {
        private static readonly IInputSimulator InputSimulator = new InputSimulator();
#pragma warning disable 0649
        [Input("Keyboard", IsSingle = true)]
        ISpread<Keyboard> FKeyboardIn;

        // For backward compatibility only
        [Input("Keyboard Input", Visibility = PinVisibility.OnlyInspector)]
        ISpread<string> FLegacyKeyStringIn;

        [Output("Keyboard", IsSingle = true)]
        ISpread<Keyboard> FKeyboardOut;

        // For backward compatibility only
        [Output("Keyboard Output", Visibility = PinVisibility.OnlyInspector)]
        ISpread<string> FLegacyKeyStringOut;
        [Output("Buffered Keyboard Output", Visibility = PinVisibility.OnlyInspector)]
        ISpread<string> FLegacyKeyBufferOut; // Outputs all keys from WM_KEYDOWN
#pragma warning restore 0649

        Subject<KeyNotification> FLegacyInputKeyboardSubject = new Subject<KeyNotification>();
        Keyboard FLegacyInputKeyboard;
        List<Keys> FLastLegacyInputKeys = new List<Keys>();
        Subscription<Keyboard, KeyNotification> FInputKeyboardSubscription;
        KeyboardState FLastKeyboardState = KeyboardState.Empty;
        PluginContainer FKeyboardSplitNode;
        Stopwatch FStopwatch = new Stopwatch();
        List<Keys> FKeyDowns = new List<Keys>();
        Subject<KeyNotification> FKeyNotificationSubject = new Subject<KeyNotification>();
        long FLastTime;

        public override void OnImportsSatisfied()
        {
            FLegacyInputKeyboard = new Keyboard(FLegacyInputKeyboardSubject, true);
            FInputKeyboardSubscription = new Subscription<Keyboard, KeyNotification>(
                keyboard => keyboard.KeyNotifications,
                (keyboard, notification) =>
                {
                    unchecked
                    {
                        switch (notification.Kind)
                        {
                            case KeyNotificationKind.KeyDown:
                                InputSimulator.Keyboard.KeyDown((VirtualKeyCode)((KeyDownNotification)notification).KeyCode);
                                break;
                            case KeyNotificationKind.KeyPress:
                                InputSimulator.Keyboard.TextEntry(((KeyPressNotification)notification).KeyChar);
                                break;
                            case KeyNotificationKind.KeyUp:
                                InputSimulator.Keyboard.KeyUp((VirtualKeyCode)((KeyUpNotification)notification).KeyCode);
                                break;
                            default:
                                break;
                        }
                    }
                }
            );
            FKeyboardOut[0] = new Keyboard(FKeyNotificationSubject, true);
            // Create a keyboard split node for us and connect our keyboard out to its keyboard in
            var nodeInfo = FIOFactory.NodeInfos.First(n => n.Name == "KeyboardState" && n.Category == "System" && n.Version == "Split Legacy");
            FKeyboardSplitNode = FIOFactory.CreatePlugin(nodeInfo, c => c.IOAttribute.Name == "Keyboard", c => FKeyboardOut);
            base.OnImportsSatisfied();
        }

        public override void Dispose()
        {
            FLegacyInputKeyboardSubject.Dispose();
            FInputKeyboardSubscription.Dispose();
            FKeyNotificationSubject.Dispose();
            FKeyboardSplitNode.Dispose();
            base.Dispose();
        }

        protected override void Evaluate(int spreadMax, bool enabled)
        {
            if (enabled)
            {
                Keyboard inputKeyboard;
                if (FKeyboardIn.SliceCount > 0 && FKeyboardIn[0] != null)
                    inputKeyboard = FKeyboardIn[0];
                else
                {
                    // To support old keyboard node
                    var keys = FLegacyKeyStringIn.Select(keyAsString => LegacyKeyboardHelper.StringToVirtualKeycode(keyAsString))
                        .ToList();
                    var keyDowns = keys.Except(FLastLegacyInputKeys);
                    foreach (var keyDown in keyDowns)
                        FLegacyInputKeyboardSubject.OnNext(new KeyDownNotification(keyDown, this));
                    var keyUps = FLastLegacyInputKeys.Except(keys);
                    foreach (var keyUp in keyUps)
                        FLegacyInputKeyboardSubject.OnNext(new KeyUpNotification(keyUp, this));
                    FLastLegacyInputKeys = keys;
                    inputKeyboard = FLegacyInputKeyboard;
                }
                FInputKeyboardSubscription.Update(inputKeyboard);
                // Works when we call GetKeyState before calling GetKeyboardState ?!
                //if (FHost.IsRunningInBackground)
                //    FKeysOut[0] = KeyboardState.CurrentAsync;
                //else
                var keyboardState = KeyboardState.Current;
                if (keyboardState != KeyboardState.Empty && keyboardState == FLastKeyboardState)
                {
                    // Keyboard stayed the same
                    if (FStopwatch.ElapsedMilliseconds > 500)
                    {
                        // Simulate key repeat by generating a new keyboard with different time code
                        var time = FStopwatch.ElapsedMilliseconds / 50;
                        var isNewKeyDown = time != FLastTime;
                        // Simulate legacy output
                        if (isNewKeyDown)
                        {
                            foreach (var keyDown in FKeyDowns)
                                FKeyNotificationSubject.OnNext(new KeyDownNotification(keyDown, this));
                            FLegacyKeyBufferOut.AssignFrom(FKeyDowns.Select(k => LegacyKeyboardHelper.VirtualKeycodeToString(k)));
                        }
                        else
                        {
                            FLegacyKeyBufferOut.SliceCount = 1;
                            FLegacyKeyBufferOut[0] = string.Empty;
                        }
                        FLastTime = time;
                        // Evaluate our split plugin
                        FKeyboardSplitNode.Evaluate(spreadMax);
                    }
                    else
                    {
                        // Simulate legacy output
                        FLegacyKeyBufferOut.SliceCount = 1;
                        FLegacyKeyBufferOut[0] = string.Empty;
                    }
                }
                else
                {
                    FStopwatch.Restart();

                    var keyDowns = keyboardState.KeyCodes.Except(FLastKeyboardState.KeyCodes);
                    foreach (var keyDown in keyDowns)
                        FKeyNotificationSubject.OnNext(new KeyDownNotification(keyDown, this));
                    var keyUps = FLastKeyboardState.KeyCodes.Except(keyboardState.KeyCodes);
                    foreach (var keyUp in keyUps)
                        FKeyNotificationSubject.OnNext(new KeyUpNotification(keyUp, this));
                    FKeyboardOut[0].CapsLock = Control.IsKeyLocked(Keys.CapsLock);

                    // Simulate legacy output
                    FLegacyKeyStringOut.AssignFrom(keyboardState.KeyCodes.Select(k => LegacyKeyboardHelper.VirtualKeycodeToString(k)));
                    FKeyDowns = keyboardState.KeyCodes
                        .Except(FLastKeyboardState.KeyCodes)
                        .ToList();
                    if (FKeyDowns.Count > 0)
                        FLegacyKeyBufferOut.AssignFrom(FKeyDowns.Select(k => LegacyKeyboardHelper.VirtualKeycodeToString(k)));
                    else
                    {
                        FLegacyKeyBufferOut.SliceCount = 1;
                        FLegacyKeyBufferOut[0] = string.Empty;
                    }
                    // Evaluate our split plugin
                    FKeyboardSplitNode.Evaluate(spreadMax);
                }
                FLastKeyboardState = keyboardState;
            }
        }
    }

    [PluginInfo(Name = "Keyboard", Category = "System", Version = "Window Legacy2")]
    public class LegacyWindowKeyboardNode : WindowMessageNode, IPluginEvaluate
    {
#pragma warning disable 0649
        [Output("Keyboard", IsSingle = true)]
        ISpread<Keyboard> FKeyboardOut;

        // For backward compatibility only
        [Output("Keyboard Output", Visibility = PinVisibility.OnlyInspector)]
        ISpread<string> FLegacyKeyStringOut;
#pragma warning restore 0649

        IDisposable FKeyboardSubscription;
        PluginContainer FKeyboardSplitNode;

        public override void Dispose()
        {
            FKeyboardSubscription.Dispose();
            FKeyboardSplitNode.Dispose();
            base.Dispose();
        }

        protected override void Initialize(IObservable<EventPattern<WMEventArgs>> windowMessages, IObservable<bool> disabled)
        {
            var keyNotifications = windowMessages
                .Select<EventPattern<WMEventArgs>, KeyNotification>(e =>
                {
                    var a = e.EventArgs;
                    switch (a.Message)
                    {
                        case WM.KEYDOWN:
                        case WM.SYSKEYDOWN:
                            return new KeyDownNotification((Keys)a.WParam, this);
                        case WM.CHAR:
                        case WM.SYSCHAR:
                            return new KeyPressNotification((char)a.WParam, this);
                        case WM.KEYUP:
                        case WM.SYSKEYUP:
                            return new KeyUpNotification((Keys)a.WParam, this);
                    }
                    return null;
                }
                )
                .OfType<KeyNotification>();
            var keyboard = new Keyboard(keyNotifications);
            FKeyboardOut[0] = keyboard;

            // Subscribe to the keyboard so we can write the legacy keyboard string output
            FKeyboardSubscription = keyboard.KeyNotifications
                .OfType<KeyCodeNotification>()
                .Subscribe(keyNotification =>
                {
                    var keyCode = keyNotification.KeyCode;
                    var keyName = LegacyKeyboardHelper.VirtualKeycodeToString(keyCode);
                    switch (keyNotification.Kind)
                    {
                        case KeyNotificationKind.KeyDown:
                            if (!FLegacyKeyStringOut.Contains(keyName))
                                FLegacyKeyStringOut.Add(keyName);
                            break;
                        case KeyNotificationKind.KeyUp:
                            FLegacyKeyStringOut.Remove(keyName);
                            break;
                        default:
                            break;
                    }
                }
            );

            // Create a keyboard split node for us and connect our keyboard out to its keyboard in
            var nodeInfo = FIOFactory.NodeInfos.First(n => n.Name == "KeyboardState" && n.Category == "System" && n.Version == "Split Legacy");
            FKeyboardSplitNode = FIOFactory.CreatePlugin(nodeInfo, c => c.IOAttribute.Name == "Keyboard", c => FKeyboardOut);
        }

        public void Evaluate(int spreadMax)
        {
            FKeyboardOut[0].CapsLock = Control.IsKeyLocked(Keys.CapsLock);
            FKeyboardSplitNode.Evaluate(spreadMax);
        }
    }
}
