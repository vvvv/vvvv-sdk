using System;
using System.Collections.Generic;
using System.Text;
using VVVV.PluginInterfaces.V2;
using VVVV.TodoMap.Lib;
using System.ComponentModel.Composition;

namespace VVVV.TodoMap.Nodes.Variables
{
    [PluginInfo(Name="TodoDeleteVariable",Author="vux",Category="TodoMap")]
    public class TodoDeleteVariableNode : IPluginEvaluate
    {
        [Input("Engine",IsSingle=true)]
        Pin<TodoEngine> FInEngine;

        [Input("Variable Name")]
        IDiffSpread<string> FInVarName;

        [Input("Delete")]
        ISpread<bool> FInDoDelete;

        [Output("Is Found")]
        ISpread<bool> FOutIsFound;

        bool FInvalidateConnect;

        public void Evaluate(int SpreadMax)
        {
            if (this.FInEngine.PluginIO.IsConnected)
            {
                this.FOutIsFound.SliceCount = this.FInVarName.SliceCount;

                for (int i = 0; i < this.FInVarName.SliceCount; i++)
                {
                    if (this.FInDoDelete[i])
                    {
                        TodoVariable var = this.FInEngine[0].GetVariableByName(this.FInVarName[i]);
                        if (var == null)
                        {
                            this.FOutIsFound[i] = false;
                        }
                        else
                        {
                            this.FInEngine[0].DeleteVariable(var,false);
                            this.FOutIsFound[i] = true;
                        }
                    }
                    else
                    {
                        this.FOutIsFound[i] = false;
                    }
                }
            }
            else
            {
                this.FOutIsFound.SliceCount = 0;
            }

        }


    }
}

