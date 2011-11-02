using System;
using System.Collections.Generic;
using System.Text;
using VVVV.PluginInterfaces.V1;

namespace vvvv.Nodes
{
    public class SpreadSpeechNode : AbstractSpeechNode, IPlugin
    {
        #region Plugin Information
        public static IPluginInfo PluginInfo
        {
            get
            {
                IPluginInfo Info = new PluginInfo();
                Info.Name = "Speech";
                Info.Category = "String";
                Info.Version = "";
                Info.Help = "Output words as it is recognized by speech recognition, Dictionary provided by a string Spread";
                Info.Bugs = "";
                Info.Credits = "";
                Info.Warnings = "";
                Info.Author = "vux";
                Info.Tags = "recognition,sapi";

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

        private IStringIn FPinInWords;

        #region On Set Plugin Host
        protected override void OnSetPluginHost()
        {
            //Create the input pins
            this.FHost.CreateStringInput("Words", TSliceMode.Dynamic, TPinVisibility.True, out this.FPinInWords);
        }
        #endregion

        #region On Evaluate
        protected override void OnEvaluate(int SpreadMax)
        {
            if (this.FPinInWords.PinIsChanged)
            {
                List<string> dictionary = new List<string>();
                for (int i = 0; i < SpreadMax; i++)
                {
                    string word;
                    this.FPinInWords.GetString(i, out word);
                    
                    //Ignore empty strings
                    if (word.Trim().Length > 0)
                    {
                        //No need to add the same word twice
                        if (!dictionary.Contains(word))
                        {
                            dictionary.Add(word);
                        }
                    }
                }

                this.BuildDictionnary(dictionary.ToArray());
            }
        }
        #endregion
    }
}
