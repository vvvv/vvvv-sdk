using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Drawing;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Windows.Forms;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.IO;
using VVVV.Utils.VMath;
using VVVV.Utils.Win32;

namespace VVVV.Nodes.Input
{
    [PluginInfo(Name = "MouseState", Category = "System", Version = "Join Legacy")]
    public class LegacyMouseStateJoinNode : IPluginEvaluate, IPartImportsSatisfiedNotification
    {
        [Input("X")]
        public IDiffSpread<double> XIn;
        [Input("Y")]
        public IDiffSpread<double> YIn;
        [Input("Mouse Wheel")]
        public IDiffSpread<int> MouseWheelIn;
        [Input("Left Button")]
        public IDiffSpread<bool> LeftButtonIn;
        [Input("Middle Button")]
        public IDiffSpread<bool> MiddleButtonIn;
        [Input("Right Button")]
        public IDiffSpread<bool> RightButtonIn;
        [Input("X Button 1")]
        public IDiffSpread<bool> X1ButtonIn;
        [Input("X Button 2")]
        public IDiffSpread<bool> X2ButtonIn;
        [Output("Mouse")]
        public ISpread<Mouse> MouseOut;

        public void OnImportsSatisfied()
        {
            MouseOut.SliceCount = 0;
        }

        public void Evaluate(int spreadMax)
        {
            MouseOut.ResizeAndDismiss(
                spreadMax,
                slice =>
                {
                    var mouseMoves = XIn.ToObservable(slice)
                        .CombineLatest(YIn.ToObservable(slice), (x, y) => new Vector2D(x, y))
                        .Select(v => new MouseMoveNotification(ToMousePoint(v), FClientArea, this));
                    var mouseButtons = Observable.Merge(
                        LeftButtonIn.ToObservable(slice).Select(x => Tuple.Create(x, MouseButtons.Left)),
                        MiddleButtonIn.ToObservable(slice).Select(x => Tuple.Create(x, MouseButtons.Middle)),
                        RightButtonIn.ToObservable(slice).Select(x => Tuple.Create(x, MouseButtons.Right)),
                        X1ButtonIn.ToObservable(slice).Select(x => Tuple.Create(x, MouseButtons.XButton1)),
                        X2ButtonIn.ToObservable(slice).Select(x => Tuple.Create(x, MouseButtons.XButton2))
                    );
                    var mouseDowns = mouseButtons.Where(x => x.Item1)
                        .Select(x => x.Item2)
                        .Select(x => new MouseDownNotification(ToMousePoint(new Vector2D(XIn[slice], YIn[slice])), FClientArea, x, this));
                    var mouseUps = mouseButtons.Where(x => !x.Item1)
                        .Select(x => x.Item2)
                        .Select(x => new MouseUpNotification(ToMousePoint(new Vector2D(XIn[slice], YIn[slice])), FClientArea, x, this));
                    var mouseWheelDeltas = MouseWheelIn.ToObservable(0)
                        .StartWith(0)
                        .Buffer(2, 1)
                        .Select(b => b[1] - b[0])
                        .Select(d => new MouseWheelNotification(ToMousePoint(new Vector2D(XIn[slice], YIn[slice])), FClientArea, d * Const.WHEEL_DELTA, this))
                        .Cast<MouseNotification>();
                    var notifications = Observable.Merge<MouseNotification>(
                        mouseMoves,
                        mouseDowns,
                        mouseUps,
                        mouseWheelDeltas);
                    return new Mouse(notifications);
                }
            );
        }

        static Point ToMousePoint(Vector2D normV)
        {
            var clientArea = new Vector2D(FClientArea.Width - 1, FClientArea.Height - 1);
            var v = VMath.Map(normV, new Vector2D(-1, 1), new Vector2D(1, -1), Vector2D.Zero, clientArea, TMapMode.Clamp);
            return new Point((int)v.x, (int)v.y);
        }

        static Size FClientArea = new Size(short.MaxValue, short.MaxValue);
    }
}
