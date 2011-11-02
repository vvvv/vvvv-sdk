using System;
using System.Collections.Generic;
using System.Text;
using VVVV.PluginInterfaces.V1;
using SpeechLib;

namespace vvvv.Nodes
{
    /// <summary>
    /// Provides the default behaviour for speech nodes
    /// </summary>
    public abstract class AbstractSpeechNode
    {
        //Speech Context
        private SpSharedRecoContext FContext;
        private ISpeechRecoGrammar FGrammar = null;
        private ISpeechGrammarRule FMenuRule = null;

        //The Host
        protected IPluginHost FHost;

        //Abstract methods to implement
        protected abstract void OnSetPluginHost();
        protected abstract void OnEvaluate(int SpreadMax);

        //Outputs
        private IStringOut FPinOutWord;
        private IValueOut FPinOutBang;
        private IStringOut FPinOutStatus;

        //Data and Refresh
        private bool FInvalidate = false;
        private string FData = "";


        #region Set Plugin Host
        public void SetPluginHost(IPluginHost Host)
        {
            this.FHost = Host;

            //Create outputs
            this.FHost.CreateStringOutput("Word", TSliceMode.Dynamic, TPinVisibility.True, out this.FPinOutWord);

            this.FHost.CreateValueOutput("Bang", 1, null, TSliceMode.Single, TPinVisibility.True, out this.FPinOutBang);
            this.FPinOutBang.SetSubType(0, 1, 0, 0, true, false, true);

            this.FHost.CreateStringOutput("Status", TSliceMode.Single, TPinVisibility.OnlyInspector, out this.FPinOutStatus);

            try
            {
                //Load the Speech context
                this.FContext = new SpeechLib.SpSharedRecoContext();
                this.FGrammar = this.FContext.CreateGrammar(0);

                //Just to avoid double event
                this.FContext.Recognition -= OnRecognition;
                this.FContext.Recognition += OnRecognition;
                this.FPinOutStatus.SetString(0, "OK");
            }
            catch (Exception ex)
            {
                this.FPinOutStatus.SetString(0,"Error: " + ex.Message);
            }

            this.OnSetPluginHost();
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
            if (this.FInvalidate)
            {
                this.FPinOutWord.SetString(0, this.FData);
                this.FPinOutBang.SetValue(0, 1);
                this.FInvalidate = false;
            }
            else
            {
                this.FPinOutBang.SetValue(0, 0);
                this.FPinOutWord.SetString(0, "");
            }

            this.OnEvaluate(SpreadMax);
        }
        #endregion

        #region Auto Evaluate
        public bool AutoEvaluate
        {
            get { return false; }
        }
        #endregion

        #region On Recognition
        private void OnRecognition(int StreamNumber, object StreamPosition, SpeechRecognitionType RecognitionType, ISpeechRecoResult Result)
        {
            string word = Result.PhraseInfo.GetText(0, -1, true);

            //Notify plugin to output word on next time it runs evaluate
            this.FInvalidate = true;
            this.FData = word;
        }
        #endregion

        #region Build Dictionnary
        protected void BuildDictionnary(string[] Words)
        {
            //If words list has changed, we rebuild the dictionnary
            this.FGrammar.Reset(0);
            this.FMenuRule = this.FGrammar.Rules.Add("Commands", SpeechRuleAttributes.SRATopLevel | SpeechRuleAttributes.SRADynamic, 1);

            object PropValue = "";

            foreach (string word in Words)
            {
                //Add all words in the recognition list
                if (word.Trim().Length > 0)
                {
                    this.FMenuRule.InitialState.AddWordTransition(null, word, " ", SpeechGrammarWordType.SGLexical, word, 1, ref PropValue, 1.0F);
                }

            }

            this.FGrammar.Rules.Commit();
            this.FGrammar.CmdSetRuleState("Commands", SpeechRuleState.SGDSActive);
        }
        #endregion
    }
}
