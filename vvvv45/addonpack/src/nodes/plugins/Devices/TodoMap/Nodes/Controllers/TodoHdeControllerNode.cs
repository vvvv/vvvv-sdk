using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VVVV.PluginInterfaces.V2;
using VVVV.TodoMap.Lib;
using System.ComponentModel.Composition;
using VVVV.TodoMap.Lib.Engine.Hde;
using VVVV.PluginInterfaces.V1;

namespace VVVV.TodoMap.Nodes.Controllers
{
    [PluginInfo(Name = "TodoHdeController", Category = "TodoMap", Author = "vux", AutoEvaluate = true)]
    public class TodoHdeControllerNode : IPluginEvaluate, IDisposable, IPluginConnections
    {
        [Input("Engine", IsSingle = true)]
        Pin<TodoEngine> FInEngine;

        [Input("Prefix", IsSingle = true,DefaultString="todo_var_")]
        IDiffSpread<string> FInPrefix;

        [Import()]
        IHDEHost FHdeHost;

        private TodoHdeManager FHdeManager;

        private bool FInvalidateConnect;

        public void Evaluate(int SpreadMax)
        {
            if (this.FInvalidateConnect || this.FInPrefix.IsChanged)
            {
                //Kill Manager
                if (this.FInEngine.PluginIO.IsConnected)
                {
                    this.FHdeManager = new Lib.Engine.Hde.TodoHdeManager(this.FInEngine[0], this.FHdeHost,this.FInPrefix[0]);
                }
                else
                {
                    this.FHdeManager = null;
                }

                this.FInvalidateConnect = false;
            }

            if (this.FHdeManager != null) { this.FHdeManager.Update(); }
        }

        public void Dispose()
        {
            this.FHdeManager = null;
        }

        public void ConnectPin(IPluginIO pin)
        {
            if (pin == this.FInEngine.PluginIO) { this.FInvalidateConnect = true; }
        }

        public void DisconnectPin(IPluginIO pin)
        {
            if (pin == this.FInEngine.PluginIO) { this.FInvalidateConnect = true; }
        }
    }
}
