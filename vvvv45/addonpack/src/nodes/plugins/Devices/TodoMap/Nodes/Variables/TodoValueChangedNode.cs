using System;
using System.Collections.Generic;
using System.Text;
using VVVV.PluginInterfaces.V2;
using VVVV.TodoMap.Lib;
using System.ComponentModel.Composition;

namespace VVVV.TodoMap.Nodes.Variables
{
    [PluginInfo(Name = "TodoValueChanged", Author = "vux", Category = "TodoMap")]
    public class TodoValueChangedNode : IPluginEvaluate, IPartImportsSatisfiedNotification
    {
        [Input("Engine", IsSingle = true)]
        Pin<TodoEngine> FInEngine;

        [Input("Variable Name")]
        IDiffSpread<string> FInVarName;

        [Output("Output",IsBang = true)]
        ISpread<bool> FOutput;

        [Output("Null Source", IsBang = true)]
        ISpread<bool> FOutNullSource;


        bool FInvalidateConnect;

        private List<string> varnames = new List<string>();
        private List<bool> changed = new List<bool>();
        private List<bool> nullsource = new List<bool>();

        public void Evaluate(int SpreadMax)
        {
            if (this.FInvalidateConnect)
            {
                if (this.FInEngine.PluginIO.IsConnected)
                {
                    this.FInEngine[0].VariableValueChangedExtended += TodoValueChangedNode_VariableValueChangedExtended;
                    this.FInEngine[0].OnReset += TodoGetValueNode_OnReset;
                }
                else
                {
                    this.FInEngine[0].VariableValueChangedExtended -= TodoValueChangedNode_VariableValueChangedExtended;
                    this.FInEngine[0].OnReset -= TodoGetValueNode_OnReset;
                }
                this.FInvalidateConnect = false;
            }

            if (this.FInVarName.SliceCount != this.changed.Count || this.FInVarName.IsChanged)
            {
                this.changed.Clear();
                this.nullsource.Clear();
                this.varnames.Clear();
                for (int i = 0; i < this.FInVarName.SliceCount; i++) { this.varnames.Add(this.FInVarName[i]); this.changed.Add(false); this.nullsource.Add(false); }
            }

            if (this.FInEngine.PluginIO.IsConnected)
            {
                this.FOutput.SliceCount = this.varnames.Count;
                this.FOutNullSource.SliceCount = this.varnames.Count;

                for (int i = 0; i < this.varnames.Count; i++)
                {
                    this.FOutput[i] = this.changed[i];
                    this.FOutNullSource[i] = this.nullsource[i];
                    this.changed[i] = false;
                }

            }
            else
            {
                this.FOutput.SliceCount = 0;
            }

        }

        void TodoValueChangedNode_VariableValueChangedExtended(TodoVariable var, AbstractTodoInput source)
        {
            if (this.varnames.Contains(var.Name))
            {
                this.changed[this.varnames.IndexOf(var.Name)] = true;
                this.nullsource[this.varnames.IndexOf(var.Name)] = source == null;
            }
        }


        void TodoGetValueNode_OnReset(object sender, EventArgs e)
        {
        }

        public void OnImportsSatisfied()
        {
            this.FInEngine.Connected += new PinConnectionEventHandler(FInEngine_CnnEvent);
            this.FInEngine.Disconnected += new PinConnectionEventHandler(FInEngine_CnnEvent);
        }

        void FInEngine_CnnEvent(object sender, PinConnectionEventArgs args)
        {
            this.FInvalidateConnect = true;
        }


    }
}
