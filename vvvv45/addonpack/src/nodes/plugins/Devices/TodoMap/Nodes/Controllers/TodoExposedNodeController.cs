using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VVVV.PluginInterfaces.V2;
using VVVV.TodoMap.Lib;
using System.ComponentModel.Composition;
using VVVV.TodoMap.Lib.Engine.Hde;
using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2.Graph;

namespace VVVV.TodoMap.Nodes.Controllers
{
    public class ExposedNode
    {
        public INode2 Node;
        public IPin2 NamePin;
        public IPin2 ValuePin;
    }

    [PluginInfo(Name = "TodoExposedController", Category = "TodoMap", Author = "vux", AutoEvaluate = true)]
    public class TodoExposedControllerNode : IPluginEvaluate, IDisposable, IPluginConnections
    {
        [Input("Engine", IsSingle = true)]
        Pin<TodoEngine> FInEngine;

        [Input("Auto Select", IsSingle = true)]
        Pin<bool> FInAutoSelect;


        IHDEHost FHDEHost;

        private Dictionary<INode, TodoHdeVariable> nodes = new Dictionary<INode, TodoHdeVariable>();
        //private Dictionary<INode, TodoHdeEnumVariable> enums = new Dictionary<INode, TodoHdeEnumVariable>();
        private List<INode2> exposednodes = new List<INode2>();

        private TodoEngine engine;

        private bool FInvalidateConnect = false;
             
        
        [ImportingConstructor]
        public TodoExposedControllerNode(IHDEHost host)
		{
			FHDEHost = host;
			FHDEHost.ExposedNodeService.NodeAdded += NodeAddedCB;
			FHDEHost.ExposedNodeService.NodeRemoved += NodeRemovedCB;
            FHDEHost.NodeSelectionChanged += new NodeSelectionEventHandler(FHDEHost_NodeSelectionChanged);
        }

        void FHDEHost_NodeSelectionChanged(object sender, NodeSelectionEventArgs args)
        {
            if (args.Nodes.Length == 1 && this.engine != null && this.FInAutoSelect[0])
            {
                INode2 n = args.Nodes[0];

                if (this.nodes.ContainsKey(n.InternalCOMInterf))
                {
                    string vn = n.LabelPin.Spread.Replace("|", "");

                    if (this.engine.GetVariableByName(vn) != null)
                    {
                        this.engine.SelectVariable(vn);
                    }
                }
            }
            
        }

        public void Evaluate(int SpreadMax)
        {
            if (this.FInvalidateConnect)
            {
                if (this.FInEngine.PluginIO.IsConnected)
                {
                    this.engine = this.FInEngine[0];

                    foreach (INode2 n in this.FHDEHost.ExposedNodeService.Nodes)
                    {
                        this.NodeAddedCB(n);
                    }
                }
                else
                {
                    this.Cleanup();

                    this.engine = null;
                }

                this.FInvalidateConnect = false;
            }

            foreach (TodoHdeVariable hdevar in this.nodes.Values)
            {
                hdevar.Update();
            }
        }

        void LabelPin_Changed(object sender, EventArgs e)
        {
            IPin2 pin = (IPin2)sender;
            INode parent = pin.ParentNode.InternalCOMInterf;

            if (nodes.ContainsKey(parent))
            {
                TodoHdeVariable var = nodes[parent];
                var.Dispose();

                nodes.Remove(parent);

                this.NodeAddedCB(pin.ParentNode);
            }
        }

        private void Cleanup()
        {
            //Clean cache
            foreach (TodoHdeVariable hdevar in this.nodes.Values)
            {
                hdevar.Dispose();
            }
            this.nodes.Clear();

            foreach (INode2 exp in this.exposednodes)
            {
                exp.LabelPin.Changed -= LabelPin_Changed;
            }
            this.exposednodes.Clear();
        }

        public void Dispose()
        {
            this.Cleanup();
        }

        public void ConnectPin(IPluginIO pin)
        {
            if (pin == this.FInEngine.PluginIO) { this.FInvalidateConnect = true; }
        }

        public void DisconnectPin(IPluginIO pin)
        {
            if (pin == this.FInEngine.PluginIO) { this.FInvalidateConnect = true; }
        }

        private void NodeAddedCB(INode2 node)
        {
            if (node.NodeInfo.Systemname == "IOBox (Value Advanced)") 
            { 
                this.ProcessNode(node);

                this.exposednodes.Add(node);
                node.LabelPin.Changed += this.LabelPin_Changed;
                node.FindPin("Tag").Changed += this.LabelPin_Changed;
            }
            /*if (node.NodeInfo.Systemname == "IOBox (Enumerations)")
            {
                this.ProcessEnumNode(node);

                this.exposednodes.Add(node);
                node.LabelPin.Changed += this.LabelPin_Changed;
                node.FindPin("Tag").Changed += this.LabelPin_Changed;
            }*/
        }

        private void ProcessNode(INode2 node)
        {
            string varname = node.LabelPin.Spread.Replace("|", "");

            //Only register variable if name not blank
            if (varname != "")
            {
                if (this.engine != null)
                {
                    TodoHdeVariable hdevar = new TodoHdeVariable(node, this.engine,varname);
                    this.nodes[node.InternalCOMInterf] = hdevar;
                }
            }
        }

        /*private void ProcessEnumNode(INode2 node)
        {
            string varname = node.LabelPin.Spread.Replace("|", "");

            //Only register variable if name not blank
            if (varname != "")
            {
                if (this.engine != null)
                {
                    /*TodoVariable var = this.engine.GetVariableByName(varname);
                    if (var == null)
                    {
                        var = new TodoVariable(varname);
                        var.Category = "Global";
                        this.engine.RegisterVariable(var, false);
                    }

                    TodoHdeEnumVariable hdevar = new TodoHdeEnumVariable(node, this.engine,this.FHDEHost);

                    this.enums[node.InternalCOMInterf] = hdevar;
                }
            }
        }*/

        private void NodeRemovedCB(INode2 node)
        {
            if (this.nodes.ContainsKey(node.InternalCOMInterf))
            {
                TodoHdeVariable hdevar = this.nodes[node.InternalCOMInterf];
                hdevar.Dispose();

                this.nodes.Remove(node.InternalCOMInterf);
            }
            /*if (this.enums.ContainsKey(node.InternalCOMInterf))
            {
                TodoHdeEnumVariable hdevar = this.enums[node.InternalCOMInterf];
                hdevar.Dispose();
            }*/

            if (this.exposednodes.Contains(node))
            {
                node.LabelPin.Changed -= LabelPin_Changed;
                this.exposednodes.Remove(node);
            }
        }

        private void PinChanged(object sender, EventArgs e)
        {

        }
    }
}
