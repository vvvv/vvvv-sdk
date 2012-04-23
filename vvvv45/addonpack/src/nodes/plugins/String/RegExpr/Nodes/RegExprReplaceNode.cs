using System;
using System.Collections.Generic;
using System.Text;

using VVVV.PluginInterfaces.V1;
using System.Text.RegularExpressions;

namespace VVVV.Nodes
{
    
    public class RegExprReplaceNode : IPlugin, IDisposable
    {
        #region Plugin Info
        public static IPluginInfo PluginInfo
        {
            get
            {
                IPluginInfo Info = new PluginInfo();
                Info.Name = "RegExpr";							//use CamelCaps and no spaces
                Info.Category = "String";						//try to use an existing one
                Info.Version = "Replace";						//versions are optional. leave blank if not needed
                Info.Help = "Regular Expression replacer";
                Info.Bugs = "";
                Info.Author = "vux & dep";
                Info.Credits = "";								//give credits to thirdparty code used
                Info.Warnings = "";

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

        #region Fields
        private IPluginHost FHost;

        private IStringIn FPinInput;
        private IStringIn FPinInRegExpr;
        private IStringIn FPinInReplacement;
        private IValueIn FPinInCS;

        private IStringOut FPinOutput;
        private IStringOut FPinOutMatches;
        private IValueOut FPinOutErrors;
        private IValueOut FPinOutBins;
        #endregion

        #region Auto Evaluate
        public bool AutoEvaluate
        {
            get { return false; }
        }
        #endregion

        #region Set Plugin Host
        public void SetPluginHost(IPluginHost Host)
        {
            //assign host
            this.FHost = Host;

            
            this.FHost.CreateStringInput("Input", TSliceMode.Dynamic, TPinVisibility.True, out this.FPinInput);
            this.FPinInput.SetSubType("", false);
         
            this.FHost.CreateStringInput("Regular Expression", TSliceMode.Dynamic, TPinVisibility.True, out this.FPinInRegExpr);
            this.FPinInRegExpr.SetSubType("", false);

            this.FHost.CreateStringInput("To", TSliceMode.Dynamic, TPinVisibility.True, out this.FPinInReplacement);
            this.FPinInReplacement.SetSubType("", false);

            this.FHost.CreateValueInput("Case Sensitive Match", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out this.FPinInCS);
            this.FPinInCS.SetSubType(0, 1, 1, 0, false, true, true);
        
            this.FHost.CreateStringOutput("Output", TSliceMode.Dynamic, TPinVisibility.True, out this.FPinOutput);
            this.FPinOutput.SetSubType("", false);

            this.FHost.CreateStringOutput("Matches", TSliceMode.Dynamic, TPinVisibility.True, out this.FPinOutMatches);
            this.FPinOutMatches.SetSubType("", false);

            this.FHost.CreateValueOutput("Errors", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out this.FPinOutErrors);
            this.FPinOutErrors.SetSubType(0, 1, 1, 0, false, true, false);

            this.FHost.CreateValueOutput("Output Bins", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out this.FPinOutBins);
            this.FPinOutBins.SetSubType(0, double.MaxValue, 1, 0, false, false, true);      
           
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
            if (this.FPinInRegExpr.PinIsChanged
              ||this.FPinInCS.PinIsChanged
              || this.FPinInput.PinIsChanged
              || this.FPinInReplacement.PinIsChanged)
            {
                this.FPinOutput.SliceCount = SpreadMax;
                this.FPinOutErrors.SliceCount = SpreadMax;
                int total_matches = 0, bins = 0;

                for (int i = 0; i < SpreadMax; i++)
                {
                    string expr, input,rep;
                    double cs;
                    this.FPinInput.GetString(i, out input);
                    this.FPinInRegExpr.GetString(i, out expr);
                    this.FPinInCS.GetValue(i, out cs);
                    this.FPinInReplacement.GetString(i, out rep);

                    if (rep == null)
                    {
                        rep = String.Empty;
                    }

                    RegexOptions reg = RegexOptions.None;
                    if (cs < 0.5)
                    {
                        reg = RegexOptions.IgnoreCase;
                    }

                    try
                    {
                        string output = Regex.Replace(input, expr, rep, reg);   //replace
                        this.FPinOutput.SetString(i, output);
                        MatchCollection coll = Regex.Matches(input, expr, reg);
                        int j = 0, c = total_matches;
                        total_matches += coll.Count;
                        this.FPinOutMatches.SliceCount = total_matches;
                        foreach (Match match in coll)     //matches
                        {
                            this.FPinOutMatches.SetString(c + j, match.Value);
                            j++;
                        }
                        bins++;
                        this.FPinOutBins.SliceCount = bins;
                        this.FPinOutBins.SetValue(i, coll.Count);
                        this.FPinOutErrors.SetValue(i, 0);
                    }
                    catch
                    {
                        this.FPinOutput.SetString(i, "");
                        this.FPinOutErrors.SetValue(i, 1);
                    }


                }
            }

        }
        #endregion

        #region Dispose
        public void Dispose()
        {
        }
        #endregion
    }
        
        

    public class Node
    {
    }
}
