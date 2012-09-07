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

        private List<TodoVariable> vars = new List<TodoVariable>();

        private bool invalidatevalue = true;
        private bool invalidateprops = true;

        public TodoHdeVariable(INode2 node, TodoEngine engine, string varname)
        {
            this.node = node;

            /*TodoVariable var = engine.GetVariableByName(varname);
            if (var == null)
            {
                var = new TodoVariable(varname);
                var.Category = "Global";
                engine.RegisterVariable(var, false);
            }


            this.var = var;*/

            this.valuepin = this.node.FindPin("Y Input Value");

            if (this.valuepin.SliceCount == 1)
            {
                TodoVariable var = engine.GetVariableByName(varname);
                if (var == null)
                {
                    var = new TodoVariable(varname);
                    var.Category = "Global";
                    var.ValueChanged += var_ValueChanged;
                    var.VariableUpdated += var_VariableUpdated;
                    engine.RegisterVariable(var, false);
                }

                this.vars.Add(var);
            }
            else if (this.valuepin.SliceCount > 1)
            {
                for (int i = 0; i < this.valuepin.SliceCount; i++)
                {
                    string vn = varname + "-" + i.ToString();

                    TodoVariable var = engine.GetVariableByName(vn);
                    if (var == null)
                    {
                        var = new TodoVariable(vn);
                        var.Category = "Global";
                        engine.RegisterVariable(var, false);
                    }

                    var.ValueChanged += var_ValueChanged;
                    var.VariableUpdated += var_VariableUpdated;

                    this.vars.Add(var);
                }
            }
            //this.valuepin.s


            this.minpin = this.node.FindPin("Minimum");
            this.maxpin = this.node.FindPin("Maximum");

            this.minpin.Changed += minpin_Changed;
            this.maxpin.Changed += maxpin_Changed;
        }

        void minpin_Changed(object sender, EventArgs e)
        {
            IPin2 pin = sender as IPin2;

            foreach (TodoVariable var in this.vars)
            {
                var.Mapper.MinValue = Convert.ToDouble(pin.Spread);
                var.MarkForUpdate(false);
            }
        }

        void maxpin_Changed(object sender, EventArgs e)
        {
            foreach (TodoVariable var in this.vars)
            {
                IPin2 pin = sender as IPin2;
                var.Mapper.MaxValue = Convert.ToDouble(pin.Spread);
                var.MarkForUpdate(false);
            }
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
                if (this.valuepin.SliceCount == 1)
                {
                    this.valuepin.Spread = vars[0].Value.ToString();
                }
                else if (this.valuepin.SliceCount > 1)
                {
                    string d = "";
                    foreach (TodoVariable var in this.vars)
                    {
                        d += var.Value.ToString() + ",";
                    }
                    this.valuepin.Spread = d;
                }
                //Update spread
                //this.valuepin.Spread = var.Value.ToString();
            }
            this.invalidatevalue = false;


            if (this.invalidateprops)
            {
                foreach (TodoVariable var in this.vars)
                {
                    this.minpin.Spread = var.Mapper.MinValue.ToString();
                    this.maxpin.Spread = var.Mapper.MaxValue.ToString();
                }
            }
            this.invalidateprops = false;
        }

        public void Dispose()
        {
            this.minpin.Changed -= minpin_Changed;
            this.maxpin.Changed -= maxpin_Changed;

            foreach (TodoVariable var in this.vars)
            {
                var.ValueChanged -= var_ValueChanged;
                var.VariableUpdated -= var_VariableUpdated;
            }
        }
    }
}
