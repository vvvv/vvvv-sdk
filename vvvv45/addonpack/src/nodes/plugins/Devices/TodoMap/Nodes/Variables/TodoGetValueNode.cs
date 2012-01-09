using System;
using System.Collections.Generic;
using System.Text;
using VVVV.PluginInterfaces.V2;
using VVVV.TodoMap.Lib;
using System.ComponentModel.Composition;

namespace VVVV.TodoMap.Nodes.Variables
{
    [PluginInfo(Name="TodoGetValue",Author="vux",Category="TodoMap")]
    public class TodoGetValueNode : IPluginEvaluate, IPartImportsSatisfiedNotification
    {
        [Input("Engine",IsSingle=true)]
        Pin<TodoEngine> FInEngine;

        [Input("Variable Name")]
        IDiffSpread<string> FInVarName;

        [Input("Normalized Value",IsSingle= true)]
        IDiffSpread<bool> FInRawValue;

        [Input("Auto Register",DefaultValue=0,IsSingle=true)]
        IDiffSpread<bool> FAutoRegister;

        [Output("Output")]
        ISpread<double> FOutput;

        [Output("Is Found")]
        ISpread<bool> FOutIsFound;

        bool FInvalidate;
        bool FInvalidateConnect;

        public void Evaluate(int SpreadMax)
        {
            if (this.FInvalidateConnect)
            {
                if (this.FInEngine.PluginIO.IsConnected)
                {
                    this.FInEngine[0].VariableValueChanged += TodoGetValueNode_VariableValueChanged;
                    this.FInEngine[0].OnReset += TodoGetValueNode_OnReset;
                    this.FInEngine[0].VariableRegistered += TodoGetValueNode_VariableInvalidate;
                    this.FInEngine[0].VariableChanged += TodoGetValueNode_VariableInvalidate;
                    this.FInEngine[0].VariableDeleted += TodoGetValueNode_VariableInvalidate;
                    this.FInvalidate = true;
                }
                else
                {
                    this.FInEngine[0].VariableValueChanged -= TodoGetValueNode_VariableValueChanged;
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
                if (this.FInvalidate || this.FInVarName.IsChanged || this.FInRawValue.IsChanged || this.FAutoRegister.IsChanged)
                {
                    this.FOutIsFound.SliceCount = this.FInVarName.SliceCount;
                    this.FOutput.SliceCount = this.FInVarName.SliceCount;
                    for (int i = 0; i < this.FInVarName.SliceCount; i++)
                    {
                        TodoVariable var = this.FInEngine[0].GetVariableByName(this.FInVarName[i]);
                        if (var == null)
                        {
                            if (this.FAutoRegister[0])
                            {
                                TodoVariable nvar = new TodoVariable(this.FInVarName[i]);
                                nvar.Category = "Global";
                                this.FInEngine[0].RegisterVariable(nvar,false);
                                this.FOutput[i] = 0;
                                this.FOutIsFound[i] = true;
                            }
                            else
                            {
                                this.FOutput[i] = 0;
                                this.FOutIsFound[i] = false;
                            }
                        }
                        else
                        {
                            if (this.FInRawValue[0])
                            {
                                this.FOutput[i] = var.ValueRaw;
                            }
                            else
                            {
                                this.FOutput[i] = var.Value;
                            }
                            this.FOutIsFound[i] = true;
                        }
                    }
                    this.FInvalidate = false;
                }
            }
            else
            {
                this.FOutput.SliceCount = 0;
                this.FOutIsFound.SliceCount = 0;
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

        void TodoGetValueNode_VariableValueChanged(string name, double newvalue)
        {
            this.FInvalidate = true;
        }

    }
}
