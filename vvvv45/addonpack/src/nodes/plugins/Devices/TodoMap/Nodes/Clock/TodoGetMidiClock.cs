using System;
using System.Collections.Generic;
using System.Text;
using VVVV.PluginInterfaces.V2;
using VVVV.TodoMap.Lib;
using System.ComponentModel.Composition;

namespace VVVV.TodoMap.Nodes.Variables
{
    [PluginInfo(Name = "TodoGetClock", Author = "vux", Category = "TodoMap")]
    public class TodoGetMidiClock : IPluginEvaluate, IPartImportsSatisfiedNotification
    {
        [Input("Engine", IsSingle = true)]
        Pin<TodoEngine> FInEngine;

        [Output("Output")]
        ISpread<double> FOutput;

        [Output("Is Enabled")]
        ISpread<bool> FOutIsFound;

        bool FInvalidate;
        bool FInvalidateConnect;

        public void Evaluate(int SpreadMax)
        {
            if (this.FInvalidateConnect)
            {
                if (this.FInEngine.PluginIO.IsConnected)
                {
                    this.FInEngine[0].Midi.ClockValueChanged += Midi_ClockValueChangedDelegate;
                    this.FInEngine[0].Midi.ClockDeviceChanged += Midi_ClockDeviceChanged;
                    this.FInEngine[0].OnReset += TodoGetValueNode_OnReset;
                    this.FInvalidate = true;
                }
                else
                {
                    this.FInEngine[0].Midi.ClockValueChanged -= Midi_ClockValueChangedDelegate;
                    this.FInEngine[0].Midi.ClockDeviceChanged -= Midi_ClockDeviceChanged;
                    this.FInEngine[0].OnReset -= TodoGetValueNode_OnReset;
                    this.FInvalidate = true;
                }
                this.FInvalidateConnect = false;
            }

            if (this.FInEngine.PluginIO.IsConnected)
            {
                if (this.FInvalidate)
                {
                    this.FOutIsFound[0] = this.FInEngine[0].Midi.ClockEnabled;
                    this.FOutput[0] = this.FInEngine[0].Midi.ClockTime;
                    this.FInvalidate = false;
                }
            }
            else
            {
                this.FOutput[0] = 0;
                this.FOutIsFound[0] = false;
            }

        }

        void Midi_ClockDeviceChanged()
        {
            this.FInvalidate = true;
        }

        void Midi_ClockValueChangedDelegate(int ticks)
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
