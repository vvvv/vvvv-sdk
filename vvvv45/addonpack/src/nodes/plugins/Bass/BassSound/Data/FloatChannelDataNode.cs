using System;
using System.Collections.Generic;
using System.Text;
using VVVV.PluginInterfaces.V1;
using Un4seen.Bass;

namespace vvvv.Nodes
{
    public class FloatChannelDataNode : AbstractChannelData, IPlugin
    {
        #region Plugin Information
        public static new IPluginInfo PluginInfo
        {
            get
            {
                IPluginInfo Info = new PluginInfo();
                Info.Name = "Channel";
                Info.Category = "Bass";
                Info.Version = "";
                Info.Help = "Get Float data samples from the channel";
                Info.Bugs = "";
                Info.Credits = "";
                Info.Warnings = "";

                //leave below as is
                System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace(true);
                System.Diagnostics.StackFrame sf = st.GetFrame(0);
                System.Reflection.MethodBase method = sf.GetMethod();
                Info.Namespace = method.DeclaringType.Namespace;
                Info.Class = method.DeclaringType.Name;
                return Info;
            }
        }
        #endregion

        protected override void OnPluginHostSet()
        {
            this.FPinInAttribute.SetSubType(double.MinValue, double.MaxValue, 0, 0, false, false, true);
        }

        protected override string FAttributeIn
        {   
            get { return "Delay"; }
        }

        protected override int DataType
        {
            get 
            {
                double ms;
                this.FPinInAttribute.GetValue(0, out ms);

                long len = Bass.BASS_ChannelSeconds2Bytes(this.FHandle, ms / 1000.0);
                return Convert.ToInt32(len / 4);
            }
        }

        protected override int DataLength
        {
            get { return this.DataType ; }
        }

        protected override string ErrorMsg
        {
            get { return "Length must be greater than 0"; }
        }

    }
}
