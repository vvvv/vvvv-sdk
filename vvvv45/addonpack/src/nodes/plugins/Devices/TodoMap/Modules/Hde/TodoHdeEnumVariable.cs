using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VVVV.PluginInterfaces.V2.Graph;
using VVVV.PluginInterfaces.V2;

namespace VVVV.TodoMap.Lib.Engine.Hde
{
    public class TodoHdeEnumVariable
    {
        private INode2 node;

        private IPin2 valuepin;

        private IHDEHost hde;

        private List<TodoVariable> var = new List<TodoVariable>();

        private bool invalidatevalue = true;

        public TodoHdeEnumVariable(INode2 node,TodoEngine engine, IHDEHost hde)
        {
            this.node = node;
            this.hde = hde;
            //this.var = var;

            this.valuepin = this.node.FindPin("Input Enum");

            string enumname = this.valuepin.SubType.Split("|".ToCharArray())[1];
            //this.valuepin.
            //this.valuepin.Type

            //var.ValueChanged += var_ValueChanged;
            //var.VariableUpdated += var_VariableUpdated;
        }

        private void var_VariableUpdated(TodoVariable var, bool gui)
        {
            this.invalidatevalue = true;
        }

        private void var_ValueChanged(TodoVariable var, AbstractTodoInput source)
        {
            if (source != null)
            {
                this.invalidatevalue = true;
            }
        }

        public void Update()
        {
            if (this.invalidatevalue)
            {
                //this.valuepin.Spread = var.Value.ToString();
            }
            this.invalidatevalue = false;
        }

        public void Dispose()
        {
            //var.ValueChanged -= var_ValueChanged;
            //var.VariableUpdated -= var_VariableUpdated;
        }
    }
}
