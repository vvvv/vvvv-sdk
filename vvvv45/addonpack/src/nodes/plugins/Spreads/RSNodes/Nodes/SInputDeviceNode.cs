using System;
using System.Collections.Generic;
using System.Text;
using VVVV.PluginInterfaces.V1;
using VVVV.Lib;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.IO;

namespace VVVV.Nodes
{
    [PluginInfo(Name = "S", Category = "Mouse", Author = "vux", AutoEvaluate = true)]
    public class SMouseNode : IPluginEvaluate, IDisposable
    {
        #region Fields
        private IPluginHost FHost;
        private MouseDataHolder FData;

        [Input("Input")]
        IDiffSpread<Mouse> FInput;

        [Input("Send String", IsSingle = true, DefaultString = "send")]
        IDiffSpread<string> FSend;

        string FKey = "";
        #endregion

        public SMouseNode()
        {
            this.FData = MouseDataHolder.Instance;
        }


        #region Evaluate
        public void Evaluate(int SpreadMax)
        {
            bool update = false;
            if (this.FSend.IsChanged)
            {
                string key = this.FSend[0];

                //First frame
                if (this.FKey == null)
                {
                    if (key.Length > 0)
                    {
                        this.FData.AddInstance(key);
                    }
                    this.FKey = key;
                }
                else
                {
                    if (this.FKey.Length > 0)
                    {
                        this.FData.RemoveInstance(this.FKey);
                    }

                    if (key.Length > 0)
                    {
                        this.FData.AddInstance(key);
                    }
                    this.FKey = key;
                }

                update = true;
            }

            if (this.FInput.IsChanged || update)
            {
                List<Mouse> msl = new List<Mouse>();
                msl.Add(this.FInput[0]);
                this.FData.UpdateData(this.FKey, msl);
            }
        }
        #endregion

        #region Dispose
        public void Dispose()
        {
            this.FData.RemoveInstance(this.FKey);
        }
        #endregion
    }

    [PluginInfo(Name = "S", Category = "Keyboard", Author = "vux", AutoEvaluate = true)]
    public class SKeyboardNode : IPluginEvaluate, IDisposable
    {
        #region Fields
        private KeyboardDataHolder FData;

        [Input("Input")]
        IDiffSpread<Keyboard> FInput;

        [Input("Send String", IsSingle = true, DefaultString = "send")]
        IDiffSpread<string> FSend;

        string FKey = "";
        #endregion

        public SKeyboardNode()
        {
            this.FData = KeyboardDataHolder.Instance;
        }


        #region Evaluate
        public void Evaluate(int SpreadMax)
        {
            bool update = false;
            if (this.FSend.IsChanged)
            {
                string key = this.FSend[0];

                //First frame
                if (this.FKey == null)
                {
                    if (key.Length > 0)
                    {
                        this.FData.AddInstance(key);
                    }
                    this.FKey = key;
                }
                else
                {
                    if (this.FKey.Length > 0)
                    {
                        this.FData.RemoveInstance(this.FKey);
                    }

                    if (key.Length > 0)
                    {
                        this.FData.AddInstance(key);
                    }
                    this.FKey = key;
                }

                update = true;
            }

            if (this.FInput.IsChanged || update)
            {
                List<Keyboard> msl = new List<Keyboard>();
                msl.Add(this.FInput[0]);
                this.FData.UpdateData(this.FKey, msl);
            }
        }
        #endregion

        #region Dispose
        public void Dispose()
        {
            this.FData.RemoveInstance(this.FKey);
        }
        #endregion
    }

    [PluginInfo(Name = "S", Category = "Touch", Author = "vux", AutoEvaluate = true)]
    public class STouchNode : IPluginEvaluate, IDisposable
    {
        #region Fields
        private IPluginHost FHost;
        private TouchDeviceDataHolder FData;

        [Input("Input")]
        IDiffSpread<TouchDevice> FInput;

        [Input("Send String", IsSingle = true, DefaultString = "send")]
        IDiffSpread<string> FSend;

        string FKey = "";
        #endregion

        public STouchNode()
        {
            this.FData = TouchDeviceDataHolder.Instance;
        }


        #region Evaluate
        public void Evaluate(int SpreadMax)
        {
            bool update = false;
            if (this.FSend.IsChanged)
            {
                string key = this.FSend[0];

                //First frame
                if (this.FKey == null)
                {
                    if (key.Length > 0)
                    {
                        this.FData.AddInstance(key);
                    }
                    this.FKey = key;
                }
                else
                {
                    if (this.FKey.Length > 0)
                    {
                        this.FData.RemoveInstance(this.FKey);
                    }

                    if (key.Length > 0)
                    {
                        this.FData.AddInstance(key);
                    }
                    this.FKey = key;
                }

                update = true;
            }

            if (this.FInput.IsChanged || update)
            {
                List<TouchDevice> msl = new List<TouchDevice>();
                msl.Add(this.FInput[0]);
                this.FData.UpdateData(this.FKey, msl);
            }
        }
        #endregion

        #region Dispose
        public void Dispose()
        {
            this.FData.RemoveInstance(this.FKey);
        }
        #endregion
    }
}
