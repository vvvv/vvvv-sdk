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


        private List<TodoVariable> vars = new List<TodoVariable>();

        private bool invalidatevalue = true;
        int varindex = 0;
        string enumname;

        public TodoHdeEnumVariable(INode2 node, TodoEngine engine, string varname)
        {
            this.node = node;
            //this.var = var;

            this.valuepin = this.node.FindPin("Input Enum");

            enumname = this.valuepin.SubType.Split(",".ToCharArray())[1].Trim();

            int ecnt = EnumManager.GetEnumEntryCount(enumname);

            for (int i = 0; i < ecnt; i++)
            {
                string eval = EnumManager.GetEnumEntryString(enumname, i);
                string vn = varname + "-" + eval;

                TodoVariable var = engine.GetVariableByName(vn);
                if (var == null)
                {
                    var = new TodoVariable(vn);
                    var.Category = "Global";
                    engine.RegisterVariable(var, false);
                }

                var.ValueChanged += var_ValueChanged;
                vars.Add(var);
            }
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
                if (var.Value > 0.5)
                {
                    this.varindex = vars.IndexOf(var);
                    this.invalidatevalue = true;
                }
            }
        }

        public void Update()
        {
            if (this.invalidatevalue)
            {
                string v = EnumManager.GetEnumEntryString(enumname, varindex);
                this.valuepin.Spread = v;
            }
            this.invalidatevalue = false;
        }

        public void Dispose()
        {
            foreach (TodoVariable var in this.vars)
            {
                var.ValueChanged -= var_ValueChanged;
            }
            //var.ValueChanged -= var_ValueChanged;
            //var.VariableUpdated -= var_VariableUpdated;
        }
    }
}
