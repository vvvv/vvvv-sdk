using System;
using System.Collections.Generic;
using System.Text;
using VVVV.PluginInterfaces.V2;
using VVVV.TodoMap.Lib;
using System.ComponentModel.Composition;
using VVVV.Utils.OSC;

namespace VVVV.TodoMap.Nodes.Variables
{
    [PluginInfo(Name = "OSCDecoder", Author = "vux", Category = "TodoMap")]
    public class TodoGetOsc : IPluginEvaluate, IPartImportsSatisfiedNotification
    {
        [Input("Engine", IsSingle = true)]
        Pin<TodoEngine> FInEngine;

        [Output("Message")]
        IDiffSpread<string> FMessage;

        [Output("Output")]
        ISpread<ISpread<string>> FOutput;

        bool FInvalidate;
        bool FInvalidateConnect;

        private List<string> FMessages = new List<string>();

        public void Evaluate(int SpreadMax)
        {
            if (this.FInvalidateConnect)
            {
                if (this.FInEngine.PluginIO.IsConnected)
                {
                    //this.FInEngine[0].Osc.
                    this.FInEngine[0].OnReset += TodoGetValueNode_OnReset;
                    this.FInEngine[0].Osc.OscDataReceived += Osc_OscDataReceived;
                    this.FInvalidate = true;
                }
                else
                {
                    this.FInEngine[0].OnReset -= TodoGetValueNode_OnReset;
                    this.FInEngine[0].Osc.OscDataReceived -= Osc_OscDataReceived;
                    this.FInvalidate = true;
                }
                this.FInvalidateConnect = false;
            }

            if (this.FMessage.IsChanged)
            {
                this.FMessages = new List<string>(this.FMessage);
            }

            if (this.FInEngine.PluginIO.IsConnected)
            {
                if (this.FInvalidate)
                {

                    this.FInvalidate = false;
                }
            }
            else
            {
                this.FOutput.SliceCount = 0;
            }

        }

        void Osc_OscDataReceived(OSCMessage msg)
        {
            if (this.FMessages.Contains(msg.Address))
            {

                this.FInvalidate = true;
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

    }
}
