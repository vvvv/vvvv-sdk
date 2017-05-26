using System;
using System.Collections.Generic;
using System.Text;
using VVVV.PluginInterfaces.V2;
using VVVV.TodoMap.Lib;
using System.ComponentModel.Composition;

namespace VVVV.TodoMap.Nodes.Variables
{
    [PluginInfo(Name="TodoSetValue",Author="vux",Category="TodoMap",AutoEvaluate=true)]
    public class TodoSetValueNode : IPluginEvaluate, IPartImportsSatisfiedNotification
    {
        [Input("Engine",IsSingle=true)]
        Pin<TodoEngine> FInEngine;

        [Input("Variable Name")]
        IDiffSpread<string> FInVarName;

        [Input("Input")]
        ISpread<double> FInput;

        [Input("Set Value",MinValue=0,MaxValue=1)]
        ISpread<bool> FInSetValue;

        [Output("Set")]
        ISpread<bool> FOutIsSet;

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
                    this.FInvalidate = true;
                }
                else
                {
                    this.FInEngine[0].VariableValueChanged -= TodoGetValueNode_VariableValueChanged;
                    this.FInEngine[0].OnReset -= TodoGetValueNode_OnReset;
                    this.FInvalidate = true;
                }
                this.FInvalidateConnect = false;
            }

            if (this.FInEngine.PluginIO.IsConnected)
            {
                this.FOutIsFound.SliceCount = Math.Max(this.FInVarName.SliceCount, this.FInSetValue.SliceCount);
                this.FOutIsSet.SliceCount = this.FOutIsFound.SliceCount;
                for (int i = 0; i < this.FOutIsFound.SliceCount; i++)
                {
                    TodoVariable var = this.FInEngine[0].GetVariableByName(this.FInVarName[i]);
                    if (var == null)
                    {
                        this.FOutIsFound[i] = false;
                        this.FOutIsSet[i] = false;
                    }
                    else
                    {
                        this.FOutIsFound[i] = true;
                        if (this.FInSetValue[i])
                        {
                            double val = Math.Min(this.FInput[i],1.0);
                            val = Math.Max(0.0,val);

                            var.SetValue(null, val);
                            this.FOutIsSet[i] = true;
                        }
                        else
                        {
                            this.FOutIsSet[i] = false;
                        }
                        
                    }
                }
            }
            else
            {
                this.FOutIsSet.SliceCount = 0;
                this.FOutIsFound.SliceCount = 0;
            }

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
