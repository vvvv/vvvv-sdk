using System;
using System.Collections.Generic;
using System.Text;
using VVVV.PluginInterfaces.V2;
using VVVV.TodoMap.Lib;
using System.ComponentModel.Composition;
using VVVV.Utils.OSC;

namespace VVVV.TodoMap.Nodes.Variables
{
    [PluginInfo(Name = "TodoMidiFeedBack", Author = "vux", Category = "TodoMap",AutoEvaluate=true)]
    public class TodoSendCustomFeedBack : IPluginEvaluate
    {
        [Input("Engine", IsSingle = true)]
        Pin<TodoEngine> FInEngine;

        [Input("Variable Name")]
        ISpread<string> FInVariableName;

        [Input("Midi Device")]
        ISpread<string> FInDevice;

        [Input("Input")]
        ISpread<double> FInValue;

        [Input("Do Send",IsBang =true)]
        ISpread<bool> FInDoSend;


        [Output("Can Send")]
        ISpread<bool> FOutCanSend;



        public void Evaluate(int SpreadMax)
        {
            if (this.FInEngine.PluginIO.IsConnected)
            {
                this.FOutCanSend.SliceCount = SpreadMax;
                for (int i = 0; i < SpreadMax; i++)
                {
                    this.FOutCanSend[i] = false;
                    if (this.FInDoSend[i])
                    {
                        TodoVariable var = this.FInEngine[0].GetVariableByName(this.FInVariableName[i]);
                        if (var != null && this.FInDevice[i] != "")
                        {
                            this.FInEngine[0].Midi.CustomFeedBack(this.FInDevice[i], var, this.FInValue[i]);
                            this.FOutCanSend[i] = true;
                        }
                        
                    }
                    //this.FInEngine[0].Midi.FeedBack(
                }
            }
            else
            {
                this.FOutCanSend.SliceCount = 1;
                this.FOutCanSend[0] = false;
            }
        }


    }
}
