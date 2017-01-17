using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Windows.Forms;
using VVVV.Utils.VMath;
using System.Runtime.InteropServices;

namespace VVVV.Nodes.Input
{
    public class DeltaMouse
    {
        private readonly int FStepSize;
        private Point FStartPosition;
        private Point FPosition;
        private Rectangle FMonitor;
        private int FCycleX;
        private int FCycleY;
        private bool FXCycle;
        private bool FYCycle;
        private bool FLastcycle;
        private Point FDelta;

        public DeltaMouse(int stepSize = 1)
        {
            FStepSize = stepSize;
            FMonitor = Screen.PrimaryScreen.Bounds;
            EnableCycles = true;
        }

        public void Initialize(Point mousePos)
        {
            FStartPosition = mousePos;
            FPosition = mousePos;
            // Ridiculously slow
            //FMonitor = Screen.GetBounds(mousePos);
            FMonitor = GetBounds(mousePos);

            FCycleX = 0;
            FCycleY = 0;

            Update();
        }

        public void Update()
        {
            var newPos = Cursor.Position;

            try
            {
                if (EnableCycles)
                {
                    if (!FLastcycle)
                    {
                        if (newPos.Y >= FMonitor.Top + FMonitor.Height - 1)
                        {
                            FCycleY++;
                            FYCycle = true;
                            newPos.Y = FMonitor.Top + 1;
                        }
                        else if (newPos.Y <= FMonitor.Top)
                        {
                            FCycleY--;
                            FYCycle = true;
                            newPos.Y = FMonitor.Top + FMonitor.Height - 2;
                        }

                        if (newPos.X >= FMonitor.Left + FMonitor.Width - 1)
                        {
                            FCycleX++;
                            FXCycle = true;
                            newPos.X = FMonitor.Left + 1;
                        }
                        else if (newPos.X <= FMonitor.Left)
                        {
                            FCycleX--;
                            FXCycle = true;
                            newPos.X = FMonitor.Left + FMonitor.Width - 2;
                        }
                    }
                }
                else
                    // Ridiculously slow
                    //FMonitor = Screen.GetBounds(newPos);
                    FMonitor = GetBounds(newPos);

                EndlessFloatX = 2f * newPos.X / (FMonitor.Width - 1f) - 1f + 2f * FCycleX;
                EndlessFloatY = 1f - 2f * newPos.Y / (FMonitor.Height - 1f) - 2f * FCycleY;
            }
            finally
            {
                if (FXCycle)
                    FPosition.X = newPos.X;
                if (FYCycle)
                    FPosition.Y = newPos.Y;

                FDelta = new Point(0, 0);
                if (Math.Abs(FPosition.X - newPos.X) > FStepSize)
                {
                    FDelta = new Point((FPosition.X - newPos.X) / (FStepSize + 1), 0);
                    FPosition.X = newPos.X;
                }

                if (Math.Abs(FPosition.Y - newPos.Y) > FStepSize)
                {
                    FDelta = new Point(FDelta.X, (FPosition.Y - newPos.Y) / (FStepSize + 1));
                    FPosition.Y = newPos.Y;
                }

                FLastcycle = FXCycle || FYCycle;
                if (FLastcycle)
                    Cursor.Position = newPos;

                FXCycle = false;
                FYCycle = false;
            }
        }

        public bool EnableCycles { get; set; }
        public double EndlessFloatX;
        public double EndlessFloatY;

        static Rectangle GetBounds(Point pt)
        {
            var screens = Screen.AllScreens;
            for (int i = 0; i < screens.Length; i++)
            {
                var bounds = screens[i].Bounds;
                if (bounds.Contains(pt))
                    return bounds;
            }
            return Rectangle.Empty;
        }
    }
}
