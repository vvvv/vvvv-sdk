using System;
using System.Collections.Generic;
using System.Text;
using VVVV.PluginInterfaces.V1;
using vvvv.Utils;

namespace vvvv.Nodes
{
    public class EyesWebMoCapDecoderNode : IPlugin 
    {
        #region Plugin Information
        public static IPluginInfo PluginInfo
        {
            get
            {
                IPluginInfo Info = new PluginInfo();
                Info.Name = "MoCapDecoder";
                Info.Category = "Network";
                Info.Version = "EyesWeb";
                Info.Help = "Read MoCap values from Eyesweb (TCP)";
                Info.Bugs = "";
                Info.Credits = "";
                Info.Warnings = "";
                Info.Author = "vux";
                Info.Tags = "motion,analysis";

                //leave below as is
                System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace(true);
                System.Diagnostics.StackFrame sf = st.GetFrame(0);
                System.Reflection.MethodBase method = sf.GetMethod();
                Info.Namespace = method.DeclaringType.Namespace;
                Info.Class = method.DeclaringType.Name;
                return Info;
                //leave above as is
            }
        }
        #endregion

        private IPluginHost FHost;

        private IStringIn FPinInValue;
        private IStringIn FPinInFilter;
        private IValueIn FPinInBang;

        private IStringOut FPinOutName;
        private IStringOut FPinOutDescription;
        private IStringOut FPinOutType;
        private IValueOut FPinOutValue1;
        private IValueOut FPinOutValue2;
        private IValueOut FPinOutValue3;
        private IValueOut FPinOutValue4;
        private IValueOut FPinOutValue5;
        private IValueOut FPinOutValue6;
        private IValueOut FPinOutFactor;
        private IValueOut FPinOutReliability;

        private List<string> FExclude = new List<string>();


        #region Set Plugin Host
        public void SetPluginHost(IPluginHost Host)
        {
            this.FHost = Host;

            this.FHost.CreateStringInput("Input", TSliceMode.Single, TPinVisibility.True, out this.FPinInValue);
            this.FHost.CreateStringInput("Exclude", TSliceMode.Dynamic, TPinVisibility.True, out this.FPinInFilter);
            this.FHost.CreateValueInput("Bang", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out this.FPinInBang);
            this.FPinInBang.SetSubType(0, 1, 0, 0, true, false, true);

            this.FHost.CreateStringOutput("Name", TSliceMode.Dynamic, TPinVisibility.True, out this.FPinOutName);
            this.FHost.CreateStringOutput("Description", TSliceMode.Dynamic, TPinVisibility.True, out this.FPinOutDescription);
            this.FHost.CreateStringOutput("Type", TSliceMode.Dynamic, TPinVisibility.True, out this.FPinOutType);
            this.FHost.CreateValueOutput("Value1", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out this.FPinOutValue1);
            this.FHost.CreateValueOutput("Value2", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out this.FPinOutValue2);
            this.FHost.CreateValueOutput("Value3", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out this.FPinOutValue3);
            this.FHost.CreateValueOutput("Value4", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out this.FPinOutValue4);
            this.FHost.CreateValueOutput("Value5", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out this.FPinOutValue5);
            this.FHost.CreateValueOutput("Value6", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out this.FPinOutValue6);
            this.FHost.CreateValueOutput("Factor", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out this.FPinOutFactor);
            this.FHost.CreateValueOutput("Reliability", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out this.FPinOutReliability);

        }
        #endregion


        #region Configurate
        public void Configurate(IPluginConfig Input)
        {

        }
        #endregion

        #region Evaluate
        public void Evaluate(int SpreadMax)
        {
            if (this.FPinInFilter.PinIsChanged)
            {
                for (int i = 0; i < this.FPinInFilter.SliceCount; i++)
                {
                    string exclude_item;
                    this.FPinInFilter.GetString(i, out exclude_item);
                    this.FExclude.Add(exclude_item);
                }
            }

            if (this.FPinInBang.PinIsChanged)
            {
                string val;
                this.FPinInValue.GetString(0, out val);
                val = val == null ? String.Empty : val;

                if (val.Length > 0)
                {
                    try
                    {
                        //First 4 characters are packet length
                        string head = val.Substring(0, 4);

                        //Remainder is data
                        string tail = val.Substring(4);
                        byte[] msg = TTypeConverter.GetArray(tail);
                        MoCapDataList data = MoCapReader.ReadMessage(msg);


                        int count = 0;
                        foreach (MoCapDataItem item in data.Values)
                        {
                            if (!this.FExclude.Contains(item.Name))
                            {
                                this.FPinOutName.SliceCount = count + 1;
                                this.FPinOutDescription.SliceCount = count + 1;
                                this.FPinOutType.SliceCount = count + 1;
                                this.FPinOutValue1.SliceCount = count + 1;
                                this.FPinOutValue2.SliceCount = count + 1;
                                this.FPinOutValue3.SliceCount = count + 1;
                                this.FPinOutValue4.SliceCount = count + 1;
                                this.FPinOutValue5.SliceCount = count + 1;
                                this.FPinOutValue6.SliceCount = count + 1;
                                this.FPinOutFactor.SliceCount = count + 1;
                                this.FPinOutReliability.SliceCount = count + 1;

                                this.FPinOutName.SetString(count, item.Name);
                                this.FPinOutDescription.SetString(count, item.Description);
                                this.FPinOutType.SetString(count, item.Item.dwType.ToString());
                                this.FPinOutValue1.SetValue(count, item.Item.dValue1);
                                this.FPinOutValue2.SetValue(count, item.Item.dValue2);
                                this.FPinOutValue3.SetValue(count, item.Item.dValue3);
                                this.FPinOutValue4.SetValue(count, item.Item.dValue4);
                                this.FPinOutValue5.SetValue(count, item.Item.dValue5);
                                this.FPinOutValue6.SetValue(count, item.Item.dValue6);
                                this.FPinOutFactor.SetValue(count, item.Item.dFactor);
                                this.FPinOutReliability.SetValue(count, item.Item.dReliability);

                                count++;
                            }
                        }
                    }
                    catch
                    {
                        SetNAN();
                    }
                }
                else
                {
                    SetNAN();
                }
            }
        }
        #endregion

        private void SetNAN()
        {
            this.FPinOutName.SliceCount = 0;
            this.FPinOutDescription.SliceCount = 0;
            this.FPinOutValue1.SliceCount = 0;
            this.FPinOutValue2.SliceCount = 0;
            this.FPinOutValue3.SliceCount = 0;
            this.FPinOutValue4.SliceCount = 0;
            this.FPinOutValue5.SliceCount = 0;
            this.FPinOutValue6.SliceCount = 0;
            this.FPinOutType.SliceCount = 0;
            this.FPinOutFactor.SliceCount = 0;
            this.FPinOutReliability.SliceCount = 0;
        }

        #region Auto Evaluate
        public bool AutoEvaluate
        {
            get { return false; }
        }
        #endregion
    }
}
