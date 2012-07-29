using System;
using System.Collections.Generic;
using System.Text;
using VVVV.PluginInterfaces.V2;
using VVVV.TodoMap.Lib;
using System.ComponentModel.Composition;

namespace VVVV.TodoMap.Nodes.Variables
{
    [PluginInfo(Name = "TodoDeleteMapping", Author = "vux", Category = "TodoMap", Version="Selection", AutoEvaluate=true)]
    public class TodoDeleteMappingNode : IPluginEvaluate
    {
        [Input("Engine", IsSingle = true)]
        Pin<TodoEngine> FInEngine;

        [Input("Mapping Index", IsSingle=true)]
        ISpread<int> FInMappingIdx;

        [Input("Delete",IsSingle=true,IsBang=true)]
        ISpread<bool> FInDoDelete;

        [Output("Is Found",IsSingle=true)]
        ISpread<bool> FOutIsFound;

        bool FInvalidateConnect;

        public void Evaluate(int SpreadMax)
        {
            if (this.FInEngine.PluginIO.IsConnected)
            {
                TodoVariable var = this.FInEngine[0].SelectedVariable;
                if (var != null)
                {
                    if (this.FInDoDelete[0])
                    {
                        AbstractTodoInput input = var.Inputs[this.FInMappingIdx[0] % var.Inputs.Count];
                        this.FInEngine[0].RemoveInput(input);
                        this.FOutIsFound[0] = true;
                    }
                    else
                    {
                        this.FOutIsFound[0] = false;
                    }
                }
                else
                {
                    this.FOutIsFound[0] = false;
                }
            }
            else
            {
                this.FOutIsFound[0] = false;
            }

        }


    }
}

