using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VVVV.PluginInterfaces.V2;
using VVVV.PluginInterfaces.V2.Graph;
using VVVV.Core;

namespace VVVV.TodoMap.Lib.Engine.Hde
{
    public class TodoHdeManager
    {
        private TodoEngine engine;
        private IHDEHost hde;
        private string prefix = "todomap_var_";

        private Dictionary<INode2, TodoHdeVariable> hdevars = new Dictionary<INode2, TodoHdeVariable>();
        private List<INode2> ioboxes = new List<INode2>();

        public TodoHdeManager(TodoEngine engine, IHDEHost hde,string prefix)
        {
            this.engine = engine;
            this.hde = hde;
            this.prefix = prefix;

            //Add events
            this.hde.RootNode.Added += NodeAdded;
            this.hde.RootNode.Removed += NodeRemoved;

            this.RegisterNode(this.hde.RootNode);
        }

        private void NodeAdded(IViewableCollection<INode2> collection, INode2 item)
        {
            this.RegisterNode(item);
        }

        private void NodeRemoved(IViewableCollection<INode2> collection, INode2 item)
        {
            this.UnRegisterNode(item);
            this.UnmapIoBox(item);
        }

        private void RegisterNode(INode2 node)
        {
            if (node.HasPatch)
            {
                node.Added += this.NodeAdded;
                node.Removed += this.NodeRemoved;

                foreach (INode2 child in node)
                {
                   this.RegisterNode(child);
                   this.ProcessIOBox(child);
                }
            }
        }

        private void UnRegisterNode(INode2 node)
        {
            if (node.HasPatch)
            {
                node.Added -= this.NodeAdded;
                node.Removed -= this.NodeRemoved;

                foreach (INode2 child in node)
                {
                    if (child.HasPatch) { this.UnRegisterNode(child); }
                }
            }

            if (this.ioboxes.Contains(node))
            {
                this.ioboxes.Remove(node);
            }
        }

        private void ProcessIOBox(INode2 node)
        {
            if (node.NodeInfo.Systemname == "IOBox (Value Advanced)")
            {
                node.LabelPin.Changed += LabelPin_Changed;

                this.MapIoBox(node);
            }
        }

        private void LabelPin_Changed(object sender, EventArgs e)
        {
            IPin2 pin = sender as IPin2;

            this.UnmapIoBox(pin.ParentNode);

            //Remap iobox
            this.MapIoBox(pin.ParentNode);

        }

        private void MapIoBox(INode2 node)
        {
            if (node.LabelPin.Spread.StartsWith(this.prefix))
            {
                //Find a variable
                string varname = node.LabelPin.Spread.Replace(this.prefix, "");

                TodoVariable var = this.engine.GetVariableByName(varname);
                if (var == null)
                {
                    var = new TodoVariable(varname);
                    var.Category = "Global";
                    this.engine.RegisterVariable(var, false);
                }

                this.hdevars[node] = new TodoHdeVariable(node, var);
            }
        }

        private void UnmapIoBox(INode2 node)
        {
            if (this.hdevars.ContainsKey(node))
            {
                this.hdevars.Remove(node);
            }
        }

        public void Update()
        {
            foreach (TodoHdeVariable hv in this.hdevars.Values)
            {
                hv.Update();
            }
        }

        
    }
}
