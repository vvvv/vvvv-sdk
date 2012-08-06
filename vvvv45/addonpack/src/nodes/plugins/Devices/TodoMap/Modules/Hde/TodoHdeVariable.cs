using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VVVV.PluginInterfaces.V2.Graph;
using VVVV.PluginInterfaces.V2;

namespace VVVV.TodoMap.Lib.Engine.Hde
{
    public class TodoHdeVariable
    {
        private INode2 node;

        private IPin2 valuepin;

        private IPin2 minpin;
        private IPin2 maxpin;

        private TodoVariable var;

        private bool invalidatevalue = true;
        private bool invalidateprops = true;

        public TodoHdeVariable(INode2 node, TodoVariable var)
        {
            this.node = node;
            this.var = var;

            this.valuepin = this.node.FindPin("Y Input Value");
            this.minpin = this.node.FindPin("Minimum");
            this.maxpin = this.node.FindPin("Maximum");

            this.minpin.Changed += minpin_Changed;
            this.maxpin.Changed += maxpin_Changed;

            var.ValueChanged += var_ValueChanged;
            var.VariableUpdated += var_VariableUpdated;
        }

        void minpin_Changed(object sender, EventArgs e)
        {
            IPin2 pin = sender as IPin2;
            var.Mapper.MinValue = Convert.ToDouble(pin.Spread);
            var.MarkForUpdate(false);
        }

        void maxpin_Changed(object sender, EventArgs e)
        {
            IPin2 pin = sender as IPin2;
            var.Mapper.MaxValue = Convert.ToDouble(pin.Spread);
            var.MarkForUpdate(false);
        }


        private void var_VariableUpdated(TodoVariable var, bool gui)
        {
            this.invalidateprops = true;
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
                this.valuepin.Spread = var.Value.ToString();
            }
            this.invalidatevalue = false;

            if (this.invalidateprops)
            {
                this.minpin.Spread = var.Mapper.MinValue.ToString();
                this.maxpin.Spread = var.Mapper.MaxValue.ToString();
            }
            this.invalidateprops = false;
        }

        public void Dispose()
        {
            this.minpin.Changed -= minpin_Changed;
            this.maxpin.Changed -= maxpin_Changed;

            var.ValueChanged -= var_ValueChanged;
            var.VariableUpdated -= var_VariableUpdated;
        }
    }
}
