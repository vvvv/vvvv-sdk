using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VVVV.PluginInterfaces.V2;
using System.ComponentModel.Composition;
using VVVV.TodoMap.DataTypes;

namespace VVVV.TodoMap.Nodes.Variables
{
    [PluginInfo(Name = "TodoUpdateVariable",Category="TodoMap" , Author = "vux")]
    public class TodoUpdateVariableNode : IPluginEvaluate, IPartImportsSatisfiedNotification
    {
        [Input("Engine", IsSingle = true)]
        Pin<TodoVariableDataType> FInEngine;

        [Input("Variable Name")]
        IDiffSpread<string> FInVarName;

        [Input("Input")]
        ISpread<double> FInput;

        [Input("Set Value", MinValue = 0, MaxValue = 1)]
        ISpread<bool> FInSetValue;

        [Output("Set")]
        ISpread<bool> FOutIsSet;

        [Output("Is Found")]
        ISpread<bool> FOutIsFound;


        public void OnImportsSatisfied()
        {
            
        }

        public void Evaluate(int SpreadMax)
        {
            
        }
    }
}
