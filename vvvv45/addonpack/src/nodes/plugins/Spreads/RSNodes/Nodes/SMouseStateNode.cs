using System;
using System.Collections.Generic;
using System.Text;
using VVVV.PluginInterfaces.V1;
using VVVV.Lib;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.IO;

namespace VVVV.Nodes
{
    [PluginInfo(Name="S",Category="MouseState",Version="Advanced",Author="vux",AutoEvaluate=true)]
    public class SMouseStateNode : IPluginEvaluate, IDisposable
    {
        #region Fields
        private IPluginHost FHost;
        private MouseStateDataHolder FData;

        [Input("Input")]
        IDiffSpread<MouseState> FInput;

        [Input("Send String", IsSingle = true, DefaultString="send")]
        IDiffSpread<string> FSend;

        string FKey = "";
        #endregion

        public SMouseStateNode()
        {
            this.FData = MouseStateDataHolder.Instance;
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
                List<MouseState> msl = new List<MouseState>();
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

    [PluginInfo(Name = "S", Category = "KeyboardState", Version = "Advanced", Author = "vux", AutoEvaluate = true)]
    public class SKeyStateNode : IPluginEvaluate, IDisposable
    {
        #region Fields
        private KeyStateDataHolder FData;

        [Input("Input")]
        IDiffSpread<KeyboardState> FInput;

        [Input("Send String", IsSingle = true, DefaultString = "send")]
        IDiffSpread<string> FSend;

        string FKey = "";
        #endregion

        public SKeyStateNode()
        {
            this.FData = KeyStateDataHolder.Instance;
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
                List<KeyboardState> msl = new List<KeyboardState>();
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
