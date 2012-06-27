using System;
using System.Collections.Generic;
using System.Text;
using VVVV.PluginInterfaces.V2;
using VVVV.TodoMap.Lib;
using System.ComponentModel.Composition;

namespace VVVV.TodoMap.Nodes.Variables
{
    [PluginInfo(Name = "TodoListVariables", Author = "vux", Category = "TodoMap")]
    public class TodoListVariablesNode : IPluginEvaluate, IPartImportsSatisfiedNotification
    {
        [Input("Engine", IsSingle = true)]
        Pin<TodoEngine> FInEngine;

        [Input("Category")]
        IDiffSpread<string> FInCat;

        [Output("Category",BinName="Output Bin Size")]
        ISpread<ISpread<string>> FOutputCat;

        [Output("Variable Name")]
        ISpread<string> FOutputVar;

        bool FInvalidate;
        bool FInvalidateConnect;

        public void Evaluate(int SpreadMax)
        {
            if (this.FInvalidateConnect)
            {
                if (this.FInEngine.PluginIO.IsConnected)
                {
                    this.FInEngine[0].OnReset += TodoGetValueNode_OnReset;
                    this.FInEngine[0].VariableRegistered += TodoGetValueNode_VariableInvalidate;
                    this.FInEngine[0].VariableChanged += TodoGetValueNode_VariableInvalidate;
                    this.FInEngine[0].VariableDeleted += TodoGetValueNode_VariableInvalidate;
                    this.FInvalidate = true;
                }
                else
                {
                    this.FInEngine[0].OnReset -= TodoGetValueNode_OnReset;
                    this.FInEngine[0].VariableRegistered -= TodoGetValueNode_VariableInvalidate;
                    this.FInEngine[0].VariableChanged -= TodoGetValueNode_VariableInvalidate;
                    this.FInEngine[0].VariableDeleted -= TodoGetValueNode_VariableInvalidate;
                    this.FInvalidate = true;
                }
                this.FInvalidateConnect = false;
            }

            if (this.FInEngine.PluginIO.IsConnected)
            {
                if (this.FInvalidate || this.FInCat.IsChanged)
                {
                    
                    List<string> vars = new List<string>();

                    this.FOutputCat.SliceCount = this.FInCat.SliceCount;

                    for (int i = 0; i < this.FInCat.SliceCount; i++)
                    {
                        List<string> cats = new List<string>();

                        string cat = this.FInCat[i];

                        if (cat == "")
                        {
                            foreach (TodoVariable var in this.FInEngine[0].Variables)
                            {
                                cats.Add(var.Category);
                                vars.Add(var.Name);
                            }
                        }
                        else
                        {
                            foreach (TodoVariable var in this.FInEngine[0].Variables)
                            {
                                if (var.Category == cat)
                                {
                                    cats.Add(var.Category);
                                    vars.Add(var.Name);
                                }
                            }
                        }

                        this.FOutputCat[i].AssignFrom(cats);
                    }

                        this.FOutputVar.AssignFrom(vars);



                    this.FInvalidate = false;
                }
            }
            else
            {
                this.FOutputCat.SliceCount = 0;
                this.FOutputVar.SliceCount = 0;
            }

        }

        void TodoGetValueNode_VariableInvalidate(TodoVariable var, bool gui)
        {
            this.FInvalidate = true;
        }

        void TodoGetValueNode_OnReset(object sender, EventArgs e)
        {
            this.FInvalidate = true;
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
