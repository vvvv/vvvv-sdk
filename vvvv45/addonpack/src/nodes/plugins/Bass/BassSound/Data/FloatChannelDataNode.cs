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
                Info.Name = "ChannelData";
                Info.Category = "Bass";
                Info.Version = "";
                Info.Help = "Get Float data samples from the channel";
                Info.Bugs = "";
                Info.Credits = "";
                Info.Warnings = "";
                Info.Author = "vux";
                Info.Tags = "Audio,Sound";

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

                long len = Bass.BASS_ChannelSeconds2Bytes(this.FMyBassHandle, ms / 1000.0);
                return Convert.ToInt32(len) ;
            }
        }

        protected override int DataLength
        {
            get { return this.DataType /4; }
        }

        protected override string ErrorMsg
        {
            get { return "Length must be greater than 0"; }
        }

        protected override void SetData(float[] samples)
        {
            BASS_CHANNELINFO info = Bass.BASS_ChannelGetInfo(this.FChannel.BassHandle.Value);
            int len = samples.Length;
            this.FPinOutLeft.SliceCount = len /2;
            this.FPinOutRight.SliceCount = len/2;
            if (info.chans > 1)
            {
                //Note: Change that to make sure it Goes with any channel soundtrack.
                for (int i = 0; i < len; i++)
                {
                    if (i % 2 == 0)
                    {
                        this.FPinOutLeft.SetValue(i / 2, (double)samples[i]);
                    }
                    else
                    {
                        this.FPinOutRight.SetValue(i / 2, (double)samples[i]);
                    }
                }
            }
            else
            {
                this.FPinOutLeft.SliceCount = len;
                this.FPinOutRight.SliceCount = len;
                for (int i = 0; i < len; i++)
                {
                    this.FPinOutLeft.SetValue(i, (double)samples[i]);
                    this.FPinOutRight.SetValue(i, (double)samples[i]);
                }
            }
        }
    }
}
