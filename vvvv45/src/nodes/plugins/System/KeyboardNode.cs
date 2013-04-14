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
using VVVV.Hosting.IO;
using System.Diagnostics;

namespace VVVV.Nodes.Input
{
    [PluginInfo(Name = "Keyboard", Category = "System", Version = "Global", AutoEvaluate = true)]
    public class GlobalKeyboardNode : GlobalInputNode
    {
#pragma warning disable 0649
        [Input("Keyboard", IsSingle = true)]
        ISpread<KeyboardState> FKeyboardIn;

        // For backward compatibility only
        [Input("Keyboard Input", Visibility = PinVisibility.OnlyInspector)]
        ISpread<string> FLegacyKeyStringIn;

        [Output("Keyboard", IsSingle = true)]
        ISpread<KeyboardState> FKeyboardOut;

        // For backward compatibility only
        [Output("Keyboard Output", Visibility = PinVisibility.OnlyInspector)]
        ISpread<string> FLegacyKeyStringOut;
        [Output("Buffered Keyboard Output", Visibility = PinVisibility.OnlyInspector)]
        ISpread<string> FLegacyKeyBufferOut; // Outputs all keys from WM_KEYDOWN
#pragma warning restore 0649

        KeyboardState FLastKeyboardIn = KeyboardState.Empty;
        KeyboardState FLastKeyboard = KeyboardState.Empty;
        PluginContainer FKeyboardSplitNode;
        Stopwatch FStopwatch = new Stopwatch();
        List<string> FKeyDowns = new List<string>();

        public override void OnImportsSatisfied()
        {
            // Create a keyboard split node for us and connect our keyboard out to its keyboard in
            var nodeInfo = FIOFactory.NodeInfos.First(n => n.Name == "KeyboardState" && n.Category == "System" && n.Version == "Split");
            FKeyboardSplitNode = FIOFactory.CreatePlugin(nodeInfo, c => c.IOAttribute.Name == "Keyboard", c => FKeyboardOut);
            base.OnImportsSatisfied();
        }

        public override void Dispose()
        {
            FKeyboardSplitNode.Dispose();
            base.Dispose();
        }

        protected override void Evaluate(int spreadMax, bool enabled)
        {
            if (enabled)
            {
                // To support old keyboard node
                if (FLegacyKeyStringIn.SliceCount > 0)
                {
                    var keys = FLegacyKeyStringIn.Select(keyAsString => LegacyKeyboardHelper.StringToVirtualKeycode(keyAsString));
                    FKeyboardIn.SliceCount = 1;
                    FKeyboardIn[0] = new KeyboardState(keys);
                }
                if (FKeyboardIn.SliceCount > 0)
                {
                    var currentKeyboardIn = FKeyboardIn[0] ?? KeyboardState.Empty;
                    if (currentKeyboardIn != FLastKeyboardIn)
                    {
                        var modifierKeys = currentKeyboardIn.ModifierKeys.Select(k => (VirtualKeyCode)k);
                        var keys = currentKeyboardIn.KeyCodes.Select(k => (VirtualKeyCode)k).Except(modifierKeys);
                        InputSimulator.SimulateModifiedKeyStroke(modifierKeys, keys);
                    }
                    FLastKeyboardIn = currentKeyboardIn;
                }
                // Works when we call GetKeyState before calling GetKeyboardState ?!
                //if (FHost.IsRunningInBackground)
                //    FKeysOut[0] = KeyboardState.CurrentAsync;
                //else
                var keyboard = KeyboardState.Current;
                if (keyboard != KeyboardState.Empty && keyboard == FLastKeyboard)
                {
                    // Keyboard stayed the same
                    if (FStopwatch.ElapsedMilliseconds > 500)
                    {
                        // Simulate key repeat by generating a new keyboard with different time code
                        var time = FStopwatch.ElapsedMilliseconds / 50;
                        var isNewKeyDown = time != FKeyboardOut[0].Time;
                        FKeyboardOut[0] = new KeyboardState(keyboard.KeyCodes, keyboard.CapsLock, (int)time);
                        // Simulate legacy output
                        if (isNewKeyDown)
                        {
                            FLegacyKeyBufferOut.AssignFrom(FKeyDowns);
                        }
                        else
                        {
                            FLegacyKeyBufferOut.SliceCount = 1;
                            FLegacyKeyBufferOut[0] = string.Empty;
                        }
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
                    FKeyboardOut[0] = keyboard;
                    // Simulate legacy output
                    FLegacyKeyStringOut.AssignFrom(keyboard.KeyCodes.Select(k => LegacyKeyboardHelper.VirtualKeycodeToString(k)));
                    FKeyDowns = keyboard.KeyCodes
                        .Except(FLastKeyboard.KeyCodes)
                        .Select(k => LegacyKeyboardHelper.VirtualKeycodeToString(k))
                        .ToList();
                    if (FKeyDowns.Count > 0)
                        FLegacyKeyBufferOut.AssignFrom(FKeyDowns);
                    else
                    {
                        FLegacyKeyBufferOut.SliceCount = 1;
                        FLegacyKeyBufferOut[0] = string.Empty;
                    }
                    // Evaluate our split plugin
                    FKeyboardSplitNode.Evaluate(spreadMax);
                }
                FLastKeyboard = keyboard;
            }
        }
    }

    [PluginInfo(Name = "Keyboard", Category = "System", Version = "Window")]
    public class WindowKeyboardNode : WindowInputNode
    {
#pragma warning disable 0649
        [Output("Keyboard", IsSingle = true)]
        ISpread<KeyboardState> FKeyboardOut;

        // For backward compatibility only
        [Output("Keyboard Output", Visibility = PinVisibility.OnlyInspector)]
        ISpread<string> FLegacyKeyStringOut;
#pragma warning restore 0649

        KeyboardState FKeyboardState = KeyboardState.Empty;
        PluginContainer FKeyboardSplitNode;

        public override void OnImportsSatisfied()
        {
            // Create a keyboard split node for us and connect our keyboard out to its keyboard in
            var nodeInfo = FIOFactory.NodeInfos.First(n => n.Name == "KeyboardState" && n.Category == "System" && n.Version == "Split");
            FKeyboardSplitNode = FIOFactory.CreatePlugin(nodeInfo, c => c.IOAttribute.Name == "Keyboard", c => FKeyboardOut);
            base.OnImportsSatisfied();
        }

        public override void Dispose()
        {
            FKeyboardSplitNode.Dispose();
            base.Dispose();
        }

        protected override unsafe void HandleSubclassWindowMessage(object sender, WMEventArgs e)
        {
            if (e.Message >= WM.KEYFIRST && e.Message <= WM.KEYLAST)
            {
                var key = (Keys)e.WParam;
                var capsLock = Control.IsKeyLocked(Keys.CapsLock);
                var time = User32.GetMessageTime();
                var chars = Enumerable.Empty<char>();
                var keys = Enumerable.Empty<Keys>();
                switch (e.Message)
                {
                    case WM.KEYDOWN:
                    case WM.SYSKEYDOWN:
                        keys = FKeyboardState.KeyCodes.Concat(new[] { key });
                        break;
                    case WM.CHAR:
                    case WM.SYSCHAR:
                        chars = new[] { (char)e.WParam };
                        keys = FKeyboardState.KeyCodes;
                        break;
                    case WM.KEYUP:
                    case WM.SYSKEYUP:
                        keys = FKeyboardState.KeyCodes.Except(new[] { key });
                        break;
                }
                FKeyboardState = new KeyboardState(keys, chars, capsLock, time);
            }
        }

        protected override void Evaluate(int spreadMax, bool enabled)
        {
            if (enabled)
            {
                if (FKeyboardOut[0] != FKeyboardState)
                {
                    FKeyboardOut[0] = FKeyboardState;
                    FLegacyKeyStringOut.AssignFrom(FKeyboardState.KeyCodes.Select(k => LegacyKeyboardHelper.VirtualKeycodeToString(k)));
                    FKeyboardSplitNode.Evaluate(spreadMax);
                }
            }
        }
    }
}
