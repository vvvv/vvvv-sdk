using System;
using System.Collections.Generic;
using System.Text;
using VVVV.PluginInterfaces.V1;
using VVVV.Lib;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.IO;

namespace VVVV.Nodes
{
    [PluginInfo(Name = "R", Category = "Mouse", Author = "vux")]
    public class RMouseNode : IPluginEvaluate, IDisposable
    {

        #region Fields
        private MouseDataHolder FData;

        [Input("Receive String", IsSingle = true, DefaultString = "send")]
        IDiffSpread<string> FReceive;

        [Output("Output", IsSingle = true)]
        ISpread<Mouse> FOut;

        [Output("Is Found", IsSingle = true)]
        ISpread<bool> FOutMC;

        private string FKey = "";

        private bool FInvalidate = false;

        #endregion

        public RMouseNode()
        {
            this.FData = MouseDataHolder.Instance;
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
                List<Mouse> dbl = this.FData.GetData(this.FKey, out found);

                if (found)
                {
                    this.FOut[0] = dbl[0];
                }
                else
                {
                    this.FOut[0] = Mouse.Empty;
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

    [PluginInfo(Name = "R", Category = "Keyboard", Author = "vux")]
    public class RKeyboardNode : IPluginEvaluate, IDisposable
    {

        #region Fields
        private KeyboardDataHolder FData;

        [Input("Receive String", IsSingle = true, DefaultString = "send")]
        IDiffSpread<string> FReceive;

        [Output("Output", IsSingle = true)]
        ISpread<Keyboard> FOut;

        [Output("Is Found", IsSingle = true)]
        ISpread<bool> FOutMC;

        private string FKey = "";

        private bool FInvalidate = false;

        #endregion

        public RKeyboardNode()
        {
            this.FData = KeyboardDataHolder.Instance;
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
                List<Keyboard> dbl = this.FData.GetData(this.FKey, out found);

                if (found)
                {
                    this.FOut[0] = dbl[0];
                }
                else
                {
                    this.FOut[0] = Keyboard.Empty;
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

    [PluginInfo(Name = "R", Category = "Touch", Author = "vux")]
    public class RTouchDeviceNode : IPluginEvaluate, IDisposable
    {

        #region Fields
        private TouchDeviceDataHolder FData;

        [Input("Receive String", IsSingle = true, DefaultString = "send")]
        IDiffSpread<string> FReceive;

        [Output("Output", IsSingle = true)]
        ISpread<TouchDevice> FOut;

        [Output("Is Found", IsSingle = true)]
        ISpread<bool> FOutMC;

        private string FKey = "";

        private bool FInvalidate = false;

        #endregion

        public RTouchDeviceNode()
        {
            this.FData = TouchDeviceDataHolder.Instance;
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
                List<TouchDevice> dbl = this.FData.GetData(this.FKey, out found);

                if (found)
                {
                    this.FOut[0] = dbl[0];
                }
                else
                {
                    this.FOut[0] = TouchDevice.Empty;
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
