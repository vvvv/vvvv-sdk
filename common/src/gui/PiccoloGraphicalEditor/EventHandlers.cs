﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using System.Collections;
using UMD.HCIL.Piccolo;
using UMD.HCIL.Piccolo.Nodes;
using UMD.HCIL.PiccoloX.Events;
using UMD.HCIL.Piccolo.Event;
using UMD.HCIL.Piccolo.Util;
using System.Reflection;
using VVVV.Core.Viewer.GraphicalEditor;
using VVVV.Core.View.GraphicalEditor;
using VVVV.Core;

namespace VVVV.HDE.GraphicalEditing
{
    /// <summary>
    /// handles drawing links between Connectables
    /// </summary>
    internal class PathEventHandler : PBasicInputEventHandler
    {
        GraphEditor FGraphEditor;
        PLayer FLinkLayer;

        public PathEventHandler(GraphEditor graphEditor)
        {
            FGraphEditor = graphEditor;
            FLinkLayer = FGraphEditor.LinkLayer;
        }

        // The mouse press location for the current pressed, drag, release sequence.
        protected PointF FMouseDownPoint;

        private bool FTempPathStarted = false;
        private TempPath FTempPath;
        private IConnectable FStartingConnectable;

        private void DrawingEnded()
        {
            FGraphEditor.LinkRoot.Remove(FTempPath);
            FTempPathStarted = false;
            FTempPath = null;
            FStartingConnectable = null;
            FGraphEditor.NoAwaitingConnections();
        }

        public override void OnClick(object sender, PInputEventArgs e)
        {
            base.OnClick(sender, e);

            Solid target = FGraphEditor.GetConnectionCandidate(e.Position, FStartingConnectable);

            if ((FTempPathStarted) && (e.Button == MouseButtons.Right))
            {
                // stop drawing path
                DrawingEnded();
            }
            else if ((target == null) && (FTempPath != null))
            {
                //add linkpoint
                FTempPath.AddPoint(e.Position);
            }
            else if (target != null)
            {
                var t = target.Connectable;

                if (FTempPathStarted)
                {
                    // end link
                    var s = FTempPath.StartSolid.Connectable;

                    if ((t.ConnectableType != s.ConnectableType)
                        || (s.ConnectableType == ConnectableType.CanBeBoth)
                        || (t.ConnectableType == ConnectableType.CanBeBoth))
                    {
                        if (t.ConnectableType == ConnectableType.StartingPoint)
                        {
                            // end point wants to be starting point
                            // let's invert the temp path, so that the host can rely on
                            // Path.Start is a Connectable with ConnectableType.StartingPoint
                            // Path.End is a Connectable with ConnectableType.EndPoint

                            FTempPath.Revert(target, out target);
                        }

                        FGraphEditor.Host.FinishPath(FTempPath, target.Connectable);
                        DrawingEnded();
                    }
                }
                else
                {
                    // starting link even when mousedown and mouseup positions doesn't match exactly
                    var x = Math.Pow(e.Position.X - FMouseDownPoint.X, 2);
                    var y = Math.Pow(e.Position.Y - FMouseDownPoint.Y, 2);
                    if (Math.Sqrt(x + y) < 5)
                    {
                        // start link
                        FTempPath = new TempPath(null, target);
                        FTempPathStarted = true;
                        FStartingConnectable = target.Connectable;
                        t.DecorateStartingPath(FTempPath);
                        FGraphEditor.ShowAwaitingConnections(target.Connectable);

                        FGraphEditor.LinkRoot.Add(FTempPath);
                    }
                }
            }
        }

        public override void OnMouseMove(object sender, PInputEventArgs e)
        {
            if (FTempPathStarted)
            {
                GraphElement target = FGraphEditor.GetConnectionCandidate(e.Position, FStartingConnectable);
                FTempPath.SetEndPoint(e.Position);
            }
        }

        public override void OnMouseDown(object sender, PInputEventArgs e)
        {
            base.OnMouseDown(sender, e);
            FMouseDownPoint = e.Position;
        }

        // Make the event handler only work with BUTTON1 events, so that it does
        // not conflict with the zoom event handler that is installed by default.
        public override bool DoesAcceptEvent(PInputEventArgs e)
        {
            return (base.DoesAcceptEvent(e) && e.IsMouseEvent);
        }
    }

    /// <summary>
    /// handles drag/drop of objects on the canvas
    /// </summary>
    internal class DragDropEventHandler : PBasicInputEventHandler
    {
        public ICanvasHost Host
        {
            get;
            internal set;
        }

        public override bool DoesAcceptEvent(PInputEventArgs e)
        {
            if (e.IsDragDropEvent)
            {
                var dict = CreateDropItems(e.DragDropData);

                foreach (var o in dict.Values)
                {
                    if (o is IIDItem)
                        return Host.AcceptsSolid(o as IIDItem);

                    if (o is string)
                        return Host.AcceptsSolid(o as string);
                }

                return false;
            }
            else
            {
                return false;
            }
        }

        private Dictionary<string, object> CreateDropItems(IDataObject data)
        {
            var dropItems = new Dictionary<string, object>();
            foreach (string df in data.GetFormats(true))
                dropItems[df] = data.GetData(df, true);
            return dropItems;
        }

        public override void OnDragOver(object sender, PInputEventArgs e)
        {
            base.OnDragOver(sender, e);
            e.DragDropEffect = DragDropEffects.Copy;
        }

        public override void OnDragDrop(object sender, PInputEventArgs args)
        {
            try
            {
                base.OnDragDrop(sender, args);

                var dict = CreateDropItems(args.DragDropData);

                foreach (var o in dict.Values)
                {
                    var idItem = o as IIDItem;
                    if (idItem != null)
                    {
                        Host.CreateSolid(idItem, args.Position);
                        break;
                    }

                    var str = o as string;
                    if (!string.IsNullOrEmpty(str))
                    {
                        // NodeBrowser serializes nodeinfo with 'Systemname||Filename'
                        if (str.Contains("||"))
                            Host.CreateSolid(str.Split(new string[] { "||" }, StringSplitOptions.None)[0], args.Position);
                        else
                            Host.CreateSolid(str, args.Position);
                        break;
                    }
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
                throw;
            }
        }
    }

    /// <summary>
    /// handles the canvas panning
    /// </summary>
    internal class PanEventHandler : PPanEventHandler
    {
        public override bool DoesAcceptEvent(PInputEventArgs e)
        {
            return e.IsMouseEvent && (e.Button == MouseButtons.Right) && (e.PickedNode is PCamera);
        }

    }

    /// <summary>
    /// Does selections
    /// </summary>
    internal class SelectionEventHandler : PSelectionEventHandler
    {
        GraphEditor FGraphEditor;

        public override bool DoesAcceptEvent(PInputEventArgs e)
        {
            if (e.IsMouseEvent)
                return base.DoesAcceptEvent(e) && (e.Button == MouseButtons.Left);
            else
                return base.DoesAcceptEvent(e);
        }

        public SelectionEventHandler(GraphEditor graphEditor, PNode marquee, PNode parent)
            : base(marquee, parent)
        {
            FGraphEditor = graphEditor;
        }

        public override void DecorateSelectedNode(PNode node)
        {
            GraphElement el = node.Tag as GraphElement;
            if (el.IsSelectable)
                el.Selectable.Selected = true;
        }

        public override void UndecorateSelectedNode(PNode node)
        {
            GraphElement el = node.Tag as GraphElement;
            if (el.IsSelectable)
                el.Selectable.Selected = false;
        }

        //node is selectable if its tag is an ISelectable
        protected override bool IsSelectable(PNode node)
        {
            var host = node.Tag as GraphElement;
            if (host != null) return host.IsSelectable;
            else return false;
        }

        protected override void EndStandardSelection(PInputEventArgs e)
        {
            base.EndStandardSelection(e);

            FGraphEditor.EndSelectionDrag();
        }
    }

    /// <summary>
    /// handles the zoom of the canvas
    /// </summary>
    internal class ZoomEventHandler : PBasicInputEventHandler
    {

        public override bool DoesAcceptEvent(PInputEventArgs e)
        {
            return e.IsMouseEvent;
        }


        public override void OnMouseWheel(object sender, PInputEventArgs e)
        {
            //base.OnMouseWheel(sender, e);

            //if (FLeftPressed && FRightPressed)
            {
                float s = 1 + Math.Sign(e.WheelDelta) * 0.1f;
                e.Camera.ScaleViewBy(s, e.Position.X, e.Position.Y);
            }
        }
    }

    /// <summary>
    /// passes mouse events to the uderlaying Control
    /// </summary>
    internal class EventPassThrougHandler : PBasicInputEventHandler
    {
        protected GraphEditor FGraphEditor;

        public EventPassThrougHandler(GraphEditor eventReceiver)
        {
            FGraphEditor = eventReceiver;
        }

        //only if click on the canvas, not a solid
        public override bool DoesAcceptEvent(PInputEventArgs e)
        {
            return e.IsMouseEvent && (e.PickedNode == FGraphEditor.SolidLayer.CamerasReference[0]);
        }

        #region mouse
        public override void OnClick(object sender, PInputEventArgs e)
        {
            base.OnClick(sender, e);
            FGraphEditor.FCanvas_MouseClick(sender, e.SourceEventArgs as MouseEventArgs);
        }

        public override void OnDoubleClick(object sender, PInputEventArgs e)
        {
            base.OnDoubleClick(sender, e);
            FGraphEditor.FCanvas_MouseDoubleClick(sender, e.SourceEventArgs as MouseEventArgs);
        }

        public override void OnMouseDown(object sender, PInputEventArgs e)
        {
            base.OnMouseDown(sender, e);
            FGraphEditor.FCanvas_MouseDown(sender, e.SourceEventArgs as MouseEventArgs);
        }

        public override void OnMouseUp(object sender, PInputEventArgs e)
        {
            base.OnMouseUp(sender, e);
            FGraphEditor.FCanvas_MouseUp(sender, e.SourceEventArgs as MouseEventArgs);
        }
        #endregion mouse
    }

}
