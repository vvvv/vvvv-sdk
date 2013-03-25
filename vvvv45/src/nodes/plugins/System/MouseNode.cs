using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.IO;
using System.Windows.Forms;
using VVVV.Utils.VMath;
using System.Runtime.InteropServices;

namespace VVVV.Nodes.Input
{
    public enum CycleMode
    {
        NoCycle,
        Cycle,
        IncrementCycle
    }

    [PluginInfo(Name = "Mouse", Category = "System", Version = "Global New")]
    public class GlobalMouseNode : IPluginEvaluate
    {
#pragma warning disable 0649
        [Input("Enabled", IsSingle = true, DefaultBoolean = true)]
        ISpread<bool> FEnabledIn;
        [Input("Cycle Mode", IsSingle = true)]
        ISpread<CycleMode> FCycleModeIn;
        [Output("Mouse State", IsSingle = true)]
        ISpread<MouseState> FMouseOut; 
#pragma warning restore 0649

        private readonly DeltaMouse FDeltaMouse = new DeltaMouse();
        private bool FLastFrameCycle;

        public void Evaluate(int SpreadMax)
        {
            var enabled = FEnabledIn.SliceCount > 0 && FEnabledIn[0];
            var cycleMode = FCycleModeIn.SliceCount > 0
                ? FCycleModeIn[0]
                : CycleMode.NoCycle;
            var doCycle = cycleMode != CycleMode.NoCycle;
            if (enabled)
            {
                FDeltaMouse.EnableCycles = doCycle;

                if (doCycle != FLastFrameCycle)
                    FDeltaMouse.Initialize(Cursor.Position);
                else
                    FDeltaMouse.Update();

                double x, y;
                switch (cycleMode)
                {
                    case CycleMode.Cycle:
                        x = FDeltaMouse.EndlessFloatX - Math.Floor((FDeltaMouse.EndlessFloatY + 1) / 2) * 2;
                        y = FDeltaMouse.EndlessFloatY - Math.Floor((FDeltaMouse.EndlessFloatY + 1) / 2) * 2;
                        break;
                    default:
                        x = FDeltaMouse.EndlessFloatX;
                        y = FDeltaMouse.EndlessFloatY;
                        break;
                }

                var buttons = GetPressedMouseButtons();
                FMouseOut[0] = new MouseState(x, y, buttons, 0);
            }
            FLastFrameCycle = doCycle;
        }

        static MouseButton GetPressedMouseButtons()
        {
            MouseButton button = MouseButton.None;
            if ((GetKeyState(Keys.LButton) & KEY_PRESSED) > 0)
                button |= MouseButton.Left;
            if ((GetKeyState(Keys.RButton) & KEY_PRESSED) > 0)
                button |= MouseButton.Right;
            if ((GetKeyState(Keys.MButton) & KEY_PRESSED) > 0)
                button |= MouseButton.Middle;
            return button;
        }

        [DllImport("user32.dll")]
        static extern short GetKeyState(System.Windows.Forms.Keys vKey);
        const byte KEY_PRESSED = 0x80;
    }
}
