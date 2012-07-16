using System;
using System.Collections.Generic;
using System.Text;
using VVVV.PluginInterfaces.V1;
using vvvv.Nodes.Subtitles;
using System.IO;

namespace vvvv.Nodes
{
    public class SRTReaderNode : IPlugin
    {
        #region Plugin Information
        public static IPluginInfo PluginInfo
        {
            get
            {
                IPluginInfo Info = new PluginInfo();
                Info.Name = "SRTReader";
                Info.Category = "File";
                Info.Version = "";
                Info.Help = "Read subtitles from an SRT file";
                Info.Bugs = "";
                Info.Credits = "";
                Info.Warnings = "";
                Info.Author = "vux";
                Info.Tags = "Subtitles,SRT";

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


        //The host
        private IPluginHost FHost;

        //Input pins
        private IStringIn FPinInFilename;
        private IValueIn FPinInPosition;
        private IValueIn FPinInDelay; //Delay to apply

        //Output pins
        private IStringOut FPinOutput;
        private IValueOut FPinOutElapsed;
        private IValueOut FPinOutRemaining;
        private IStringOut FPinOutStatus;
        private IValueOut FPinOutNbLines;

        //Private members
        private TSubtitleList FSubtitles;
        private bool FValid = false;
        private double FDelay = 0;

        #region Set plugin host
        public void SetPluginHost(IPluginHost Host)
        {
            this.FHost = Host;

            this.FHost.CreateStringInput("Filename", TSliceMode.Single, TPinVisibility.True, out this.FPinInFilename);
            this.FPinInFilename.SetSubType("", true);

            this.FHost.CreateValueInput("Position", 1, null, TSliceMode.Single, TPinVisibility.True, out this.FPinInPosition);
            this.FPinInPosition.SetSubType(0, double.MaxValue, 0.01, 0, false, false, false);

            this.FHost.CreateValueInput("Delay", 1, null, TSliceMode.Single, TPinVisibility.True, out this.FPinInDelay);
            this.FPinInDelay.SetSubType(double.MinValue, double.MaxValue,0.01, 0, false, false, false);

            this.FHost.CreateStringOutput("Status", TSliceMode.Single, TPinVisibility.OnlyInspector, out this.FPinOutStatus);

            this.FHost.CreateStringOutput("Output", TSliceMode.Dynamic, TPinVisibility.True, out this.FPinOutput);

            this.FHost.CreateValueOutput("Line Count", 1, null, TSliceMode.Single, TPinVisibility.True, out this.FPinOutNbLines);
            this.FPinOutNbLines.SetSubType(0, double.MaxValue, 1, 0, false, false, true);
            
            this.FHost.CreateValueOutput("Elapsed", 1, null, TSliceMode.Single, TPinVisibility.True, out this.FPinOutElapsed);
            this.FPinOutElapsed.SetSubType(0, double.MaxValue, 0.01, 0, false, false, false);

            this.FHost.CreateValueOutput("Remaining",1, null, TSliceMode.Single, TPinVisibility.True, out this.FPinOutRemaining);
            this.FPinOutRemaining.SetSubType(0, double.MaxValue, 0.01, 0, false, false, false);

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
            #region Pin Filename changed
            if (this.FPinInFilename.PinIsChanged)
            {
                string path;
                this.FPinInFilename.GetString(0, out path);

                //Check if file exists
                if (File.Exists(path))
                {
                    try
                    {
                        //Try to load the subtitles
                        this.FSubtitles = TSRTReader.LoadFromFile(path);
                        this.FValid = true;
                        this.FPinOutStatus.SetString(0, "OK");
                    }
                    catch (Exception ex) 
                    {
                        //Show the error on the status pin
                        this.FSubtitles = new TSubtitleList();
                        this.FPinOutStatus.SetString(0, ex.Message);
                        this.FValid = false;
                    }
                }
                else
                {
                    //Error message (file doesnt exist)
                    this.FSubtitles = new TSubtitleList();
                    this.FValid = false;
                    this.FPinOutStatus.SetString(0, "File does not exist");
                }
            }
            #endregion

            if (this.FPinInDelay.PinIsChanged)
            {
                this.FPinInDelay.GetValue(0, out this.FDelay);
            }

            #region Pin Position changed
            if (this.FPinInPosition.PinIsChanged)
            {
                //Check if we have valid subtitle
                if (this.FValid)
                {
                    //Get position
                    double position;
                    this.FPinInPosition.GetValue(0, out position);
                    
                    //Apply delay
                    position = position + this.FDelay;

                    //Check if a subtitle is supposed to appear
                    TSubtitle sub = this.FSubtitles.GetSubtitle(position);

                    if (sub != null)
                    {
                        //Show the subtitle
                        List<string> lines = sub.Lines;
                        this.FPinOutNbLines.SetValue(0, lines.Count);

                        this.FPinOutput.SliceCount = lines.Count;
                        for (int i = 0; i < lines.Count; i++)
                        {
                            this.FPinOutput.SetString(i, lines[i]);
                        }
  
                        this.FPinOutElapsed.SetValue(0, position - sub.TimeFrom);
                        this.FPinOutRemaining.SetValue(0, sub.TimeTo - position);
                        
                    }
                    else
                    {
                        //No subtitle to show
                        this.FPinOutElapsed.SetValue(0, 0);
                        this.FPinOutput.SetString(0, "");
                        this.FPinOutRemaining.SetValue(0, 0);
                        this.FPinOutNbLines.SliceCount = 1;
                        this.FPinOutNbLines.SetValue(0, 0);
                    }
                }
                else
                {
                    this.FPinOutElapsed.SetValue(0, 0);
                    this.FPinOutput.SetString(0, "");
                    this.FPinOutRemaining.SetValue(0, 0);
                    this.FPinOutNbLines.SetValue(0, 0);
                }

            }
            #endregion
        }
        #endregion

        #region Auto evaluate
        public bool AutoEvaluate
        {
            get { return false; }
        }
        #endregion
    }
}
