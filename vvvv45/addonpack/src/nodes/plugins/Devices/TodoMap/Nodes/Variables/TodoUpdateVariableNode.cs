using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VVVV.PluginInterfaces.V2;
using System.ComponentModel.Composition;
using VVVV.TodoMap.DataTypes;
using VVVV.TodoMap.Lib;

namespace VVVV.TodoMap.Nodes.Variables
{
    [PluginInfo(Name = "TodoUpdateVariable",Category="TodoMap" , Author = "vux",AutoEvaluate=true)]
    public class TodoUpdateVariableNode : IPluginEvaluate, IPartImportsSatisfiedNotification
    {
        [Input("Engine", IsSingle = true)]
        Pin<TodoEngine> FInEngine;

        [Input("Variable Name")]
        IDiffSpread<string> FInVarName;

        [Input("Category")]
        ISpread<string> FInCategory;

        [Input("Set Category", IsBang=true)]
        ISpread<bool> FInSetCategory;

        [Input("Default")]
        ISpread<double> FInDefault;

        [Input("Set Default", IsBang = true)]
        ISpread<bool> FInSetDefault;

        [Input("Minimum")]
        ISpread<double> FInMinimum;

        [Input("Set Minimum", IsBang = true)]
        ISpread<bool> FInSetMinimum;

        [Input("Maximum")]
        ISpread<double> FInMaximum;

        [Input("Set Maximum", IsBang = true)]
        ISpread<bool> FInSetMaximum;

        [Input("Tween Mode")]
        ISpread<eTweenMode> FInTweenMode;

        [Input("Set Tween Mode", IsBang = true)]
        ISpread<bool> FInSetTweenMode;

        [Input("Ease Mode")]
        ISpread<eTweenEaseMode> FInEaseMode;

        [Input("Set Ease Mode", IsBang = true)]
        ISpread<bool> FInSetEaseMode;

        [Input("TakeOver Mode")]
        ISpread<eTodoGlobalTakeOverMode> FInTakeOverMode;

        [Input("Set TakeOver Mode", IsBang = true)]
        ISpread<bool> FInSetTOMode;

        [Input("Allow Feedback")]
        ISpread<bool> FInAllowFeedBack;

        [Input("Set Allow Feedback", IsBang = true)]
        ISpread<bool> FInSetAllowFeedback;

        [Input("Set All", IsBang = true)]
        ISpread<bool> FInSetAll;

        [Output("Set")]
        ISpread<bool> FOutIsSet;



        public void OnImportsSatisfied()
        {
            
        }

        public void Evaluate(int SpreadMax)
        {
            if (this.FInEngine.PluginIO.IsConnected)
            {
                this.FOutIsSet.SliceCount = this.FInVarName.SliceCount;

                for (int i = 0; i < this.FInVarName.SliceCount; i++)
                {
                    if (this.FInSetCategory[i] || this.FInSetAll[i] || this.FInSetDefault[i]
                        || this.FInSetMinimum[i] || this.FInSetMaximum[i]
                        || this.FInSetTweenMode[i] || this.FInSetEaseMode[i] 
                        || this.FInSetTOMode[i] || this.FInSetAllowFeedback[i])
                    {
                        TodoVariable var = this.FInEngine[0].GetVariableByName(this.FInVarName[i]);

                        if (this.FInSetCategory[i] || this.FInSetAll[i]) { var.Category = this.FInCategory[i]; }
                        if (this.FInSetDefault[i] || this.FInSetAll[i]) { var.Default = this.FInDefault[i]; }
                        if (this.FInSetMinimum[i] || this.FInSetAll[i]) { var.Mapper.MinValue = this.FInMinimum[i]; }
                        if (this.FInSetMaximum[i] || this.FInSetAll[i]) { var.Mapper.MaxValue = this.FInMaximum[i]; }
                        if (this.FInSetTweenMode[i] || this.FInSetAll[i]) { var.Mapper.TweenMode = this.FInTweenMode[i]; }
                        if (this.FInSetEaseMode[i] || this.FInSetAll[i]) { var.Mapper.EaseMode = this.FInEaseMode[i]; }
                        if (this.FInSetTOMode[i] || this.FInSetAll[i]) { var.TakeOverMode = this.FInTakeOverMode[i]; }
                        if (this.FInSetAllowFeedback[i] || this.FInSetAll[i]) { var.AllowFeedBack = this.FInAllowFeedBack[i]; }


                        var.MarkForUpdate(false);
                    }

                    
                }
            }
            else
            {
                this.FOutIsSet.SliceCount = 0;
            }
        }
    }
}
