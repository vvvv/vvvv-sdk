using System;
using System.Collections.Generic;
using System.Text;
using VVVV.PluginInterfaces.V1;
using VVVV.Lib;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.IO;

namespace VVVV.Nodes
{
    [PluginInfo(Name = "R", Category = "MouseState", Version = "Advanced", Author = "vux")]
    public class RMouseStateNode : IPluginEvaluate, IDisposable
    {

        #region Fields
        private MouseStateDataHolder FData;

        [Input("Receive String", IsSingle = true, DefaultString = "send")]
        IDiffSpread<string> FReceive;

        [Output("Output",IsSingle=true)]
        ISpread<MouseState> FOut;

        [Output("Is Found", IsSingle = true)]
        ISpread<bool> FOutMC;

        private string FKey = "";

        private bool FInvalidate = false;

        #endregion

        public RMouseStateNode()
        {
            this.FData = MouseStateDataHolder.Instance;
            this.FData.OnAdd += FData_OnAdd;
            this.FData.OnRemove += this.FData_OnAdd;
            this.FData.OnUpdate += this.FData_OnAdd;
        }

        #region Set Plugin Host
        void FData_OnAdd(string key)
        {
            if (this.FKey == key)
            {
                this.FInvalidate = true;
            }
        }
        #endregion

        #region Evaluate
        public void Evaluate(int SpreadMax)
        {
            if (this.FReceive.IsChanged)
            {
                this.FKey = this.FReceive[0];
                this.FInvalidate = true;
            }

            if (this.FInvalidate)
            {
                bool found;
                List<MouseState> dbl = this.FData.GetData(this.FKey,out found);

                if (found)
                {
                    this.FOut[0] = dbl[0];
                }
                else
                {
                    this.FOut[0] = new MouseState();
                }

                this.FOutMC[0] = found;
                this.FInvalidate = false;
            }

        }
        #endregion

        #region Dispose
        public void Dispose()
        {
        }
        #endregion
    }

    [PluginInfo(Name = "R", Category = "KeyBoardState", Version = "Advanced", Author = "vux")]
    public class RKeyStateNode : IPluginEvaluate, IDisposable
    {

        #region Fields
        private KeyStateDataHolder FData;

        [Input("Receive String", IsSingle = true, DefaultString = "send")]
        IDiffSpread<string> FReceive;

        [Output("Output", IsSingle = true)]
        ISpread<KeyboardState> FOut;

        [Output("Is Found", IsSingle = true)]
        ISpread<bool> FOutMC;

        private string FKey = "";

        private bool FInvalidate = false;

        #endregion

        public RKeyStateNode()
        {
            this.FData = KeyStateDataHolder.Instance;
            this.FData.OnAdd += FData_OnAdd;
            this.FData.OnRemove += this.FData_OnAdd;
            this.FData.OnUpdate += this.FData_OnAdd;
        }

        #region Set Plugin Host
        void FData_OnAdd(string key)
        {
            if (this.FKey == key)
            {
                this.FInvalidate = true;
            }
        }
        #endregion

        #region Evaluate
        public void Evaluate(int SpreadMax)
        {
            if (this.FReceive.IsChanged)
            {
                this.FKey = this.FReceive[0];
                this.FInvalidate = true;
            }

            if (this.FInvalidate)
            {
                bool found;
                List<KeyboardState> dbl = this.FData.GetData(this.FKey, out found);

                if (found)
                {
                    this.FOut[0] = dbl[0];
                }
                else
                {
                    this.FOut[0] = new KeyboardState(new List<System.Windows.Forms.Keys>());
                }

                this.FOutMC[0] = found;
                this.FInvalidate = false;
            }

        }
        #endregion

        #region Dispose
        public void Dispose()
        {
        }
        #endregion
    }      
}
