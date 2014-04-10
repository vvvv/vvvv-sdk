using System;
using System.Collections.Generic;
using System.Text;
using VVVV.PluginInterfaces.V2;
using VVVV.TodoMap.Lib;
using System.ComponentModel.Composition;
using VVVV.TodoMap.Lib.Modules.Osc;
using VVVV.TodoMap.Lib.Modules.Midi;

namespace VVVV.TodoMap.Nodes.Variables
{
    [PluginInfo(Name = "TodoCreateMidiMapping", Author = "vux", Category = "TodoMap", AutoEvaluate = true)]
    public class TodoCreateMidiMappingNode : IPluginEvaluate
    {
        [Input("Engine", IsSingle = true)]
        Pin<TodoEngine> FInEngine;

        [Input("Name")]
        ISpread<string> FInName;

        [Input("Device",DefaultString="Any")]
        ISpread<int> FInDevice;

        [Input("Channel")]
        ISpread<int> FInChannel;

        [Input("Message")]
        ISpread<int> FInMsg;

        [Input("FeedBack Mode",DefaultEnumEntry="Parent")]
        ISpread<eTodoLocalFeedBackMode> FInFeedBack;

        [Input("TakeOver Mode", DefaultEnumEntry = "Parent")]
        ISpread<eTodoLocalTakeOverMode> FinTakeOver;

        [Input("Create", IsBang = true)]
        ISpread<bool> FInDoCreate;

        [Output("Is Found")]
        ISpread<bool> FOutIsFound;

        bool FInvalidateConnect;

        public void Evaluate(int SpreadMax)
        {
            if (this.FInEngine.PluginIO.IsConnected)
            {
                this.FOutIsFound.SliceCount = SpreadMax;
                for (int i = 0; i < SpreadMax; i++)
                {

                    TodoVariable var = this.FInEngine[i].GetVariableByName(this.FInName[i]);
                    if (var != null)
                    {
                        if (this.FInDoCreate[i])
                        {
                            bool found = true;
                            TodoMidiInput item = null;
                            foreach (AbstractTodoInput input in var.Inputs)
                            {
                                if (input is TodoMidiInput)
                                {
                                    TodoMidiInput midi = (TodoMidiInput)input;
                                    if (midi.Device == this.FInDevice[i] || midi.ControlValue == 
                                    //if (osc.Message == this.FInMessage[i]) { item = osc; found = true; }
                                }
                            }
                            if (item == null)
                            {
                                TodoMidiInput input = new TodoMidiInput(var);
                                input.Device = this.FInDevice[i];
                                input.MidiChannel = this.FInChannel[i];
                                input.ControlValue = this.FInMsg[i];
                               // input.Message = this.FInMessage[i];
                            }
                            item.FeedBackMode = this.FInFeedBack[i];
                            item.TakeOverMode = this.FinTakeOver[i];

                            this.FOutIsFound[i] = !found;
                        }
                        else
                        {
                            this.FOutIsFound[i] = false;
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
                this.FOutIsFound[0] = false;
            }

        }


    }
}

