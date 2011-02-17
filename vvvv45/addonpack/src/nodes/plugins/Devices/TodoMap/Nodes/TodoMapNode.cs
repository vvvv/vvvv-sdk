using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VVVV.PluginInterfaces.V2;
using VVVV.PluginInterfaces.V1;

namespace VVVV.TodoMap.Nodes
{
    [PluginInfo(Name="TodoMap",Category="TodoMap",Author="vux",InitialComponentMode=TComponentMode.InAWindow,InitialWindowWidth=700,InitialWindowHeight=500)]
    public partial class TodoMapNode : IPluginEvaluate
    {
        [Input("Variable Name")]
        ISpread<string> FInVariableName;

        [Input("Register Variable")]
        ISpread<bool> FInRegisterVariable;

        [Input("Learn Mode",IsSingle=true)]
        ISpread<bool> FInLearnMode;

        [Input("Path", IsSingle = true,StringType=StringType.Filename)]
        ISpread<string> FInFileName;

        [Input("Load", IsSingle = true, IsBang = true)]
        ISpread<bool> FInLoad;

        [Input("Save", IsSingle = true, IsBang = true)]
        ISpread<bool> FInSave;

        [Input("Enabled", IsSingle = true)]
        ISpread<bool> FInEnabled;

        [Input("Reset",IsSingle = true,IsBang=true)]
        ISpread<bool> FInReset;

        #region IPluginEvaluate Members
        public void Evaluate(int SpreadMax)
        {
        }
        #endregion
    }
}
