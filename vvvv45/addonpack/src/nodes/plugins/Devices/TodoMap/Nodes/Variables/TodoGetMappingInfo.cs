using System;
using System.Collections.Generic;
using System.Text;
using VVVV.PluginInterfaces.V2;
using VVVV.TodoMap.Lib;
using System.ComponentModel.Composition;

namespace VVVV.TodoMap.Nodes.Variables
{
    [PluginInfo(Name = "TodoGetMappingInfo", Author = "vux", Category = "TodoMap", Version = "Selection")]
    public class TodoGetMappingInfoNode : IPluginEvaluate
    {
        [Input("Engine", IsSingle = true)]
        Pin<TodoEngine> FInEngine;

        [Output("Type")]
        ISpread<string> FOutType;

        [Output("Device")]
        ISpread<string> FOutDevice;

        [Output("Mapping")]
        ISpread<string> FOutMapping;

        [Output("TakeOver Mode")]
        ISpread<eTodoLocalTakeOverMode> FOutTakeOver;

        [Output("Feedback Mode")]
        ISpread<eTodoLocalFeedBackMode> FOutFeedBack;

        private TodoVariable oldvar; 
        private bool first = true;

        public void Evaluate(int SpreadMax)
        {
            if (this.FInEngine.PluginIO.IsConnected)
            {
                TodoVariable var = this.FInEngine[0].SelectedVariable;
                
                if (var != oldvar || first)
                {
                    if (var != null)
                    {
                        this.FOutType.SliceCount = var.Inputs.Count;
                        this.FOutMapping.SliceCount = var.Inputs.Count;
                        this.FOutDevice.SliceCount = var.Inputs.Count;
                        this.FOutFeedBack.SliceCount = var.Inputs.Count;
                        this.FOutTakeOver.SliceCount = var.Inputs.Count;

                        for (int i = 0; i < var.Inputs.Count; i++)
                        {
                            AbstractTodoInput input = var.Inputs[i];
                            this.FOutType[i] = input.InputType;
                            this.FOutMapping[i] = input.InputMap;
                            this.FOutDevice[i] = input.Device;
                            this.FOutFeedBack[i] = input.FeedBackMode;
                            this.FOutTakeOver[i] = input.TakeOverMode;
                        }

                    }
                    else
                    {
                        this.FOutType.SliceCount = 0;
                        this.FOutMapping.SliceCount = 0;
                        this.FOutDevice.SliceCount = 0;
                        this.FOutFeedBack.SliceCount = 0;
                        this.FOutTakeOver.SliceCount = 0;
                    }
                    first = false;
                }
            }
            else
            {
                this.FOutType.SliceCount = 0;
                this.FOutMapping.SliceCount = 0;
                this.FOutDevice.SliceCount = 0;
                this.FOutFeedBack.SliceCount = 0;
                this.FOutTakeOver.SliceCount = 0;
            }

        }


    }
}

