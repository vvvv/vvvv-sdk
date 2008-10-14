using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using VVVV.PluginInterfaces.V1;
using Un4seen.Bass.Misc;
using System.IO;
using System.Drawing;
using Un4seen.Bass;

namespace BassSound.UI
{
    public class WaveFormNode : UserControl, IPlugin
    {
        #region node name and infos

        //provide node infos 
        public static IPluginInfo PluginInfo
        {
            get
            {
                //fill out nodes info
                //see: http://www.vvvv.org/tiki-index.php?page=vvvv+naming+conventions
                IPluginInfo Info = new PluginInfo();
                Info.Name = "WaveForm";							//use CamelCaps and no spaces
                Info.Category = "Bass";						//try to use an existing one
                Info.Version = "";							//versions are optional. leave blank if not needed
                Info.Help = "Render a waveform trough bass";
                Info.Bugs = "";
                Info.Credits = "";								//give credits to thirdparty code used
                Info.Warnings = "";
                Info.InitialBoxSize = new Size(200, 100);		//defines initial size of node in box-mode
                Info.InitialWindowSize = new Size(400, 300);	//defines initial size of node in window-mode
                Info.InitialComponentMode = TComponentMode.InAWindow;	//defines initial component mode

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
        #endregion node name and infos
   
        private IPluginHost FHost;

        private IStringIn FPinInFile;
        private IValueIn FPinInCurrentPosition;
        private IValueIn FPinInMarkers;
        private IValueIn FPinInStart;
        private IValueIn FPinInEnd;

        private WaveForm FWaveForm;
        private int FFrameStart = -1, FFrameEnd = -1;

        public WaveFormNode()
        {
            this.InitializeComponent();
        }

        #region Initialize Component
        private void InitializeComponent()
        {
            this.SuspendLayout();
            // 
            // GUITemplatePlugin
            // 
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.DoubleBuffered = true;
            this.Name = "BassWaveForm";
            this.Size = new System.Drawing.Size(310, 169);
            this.ResumeLayout(false);
        }
        #endregion

        #region Auto Evaluate
        public bool AutoEvaluate
        {
            //return true if this node needs to calculate every frame even if nobody asks for its output
            get { return true; }
        }
        #endregion

        #region Set Plugin Host
        //this method is called by vvvv when the node is created
        public void SetPluginHost(IPluginHost Host)
        {
            //assign host
            FHost = Host;

            Bass.BASS_Init(0, 48000, 0, IntPtr.Zero, null);

            this.FHost.CreateStringInput("File Name", TSliceMode.Single, TPinVisibility.True, out this.FPinInFile);
            this.FPinInFile.SetSubType("", true);

            this.FHost.CreateValueInput("Current Position", 1, null, TSliceMode.Single, TPinVisibility.True, out this.FPinInCurrentPosition);
            this.FPinInCurrentPosition.SetSubType(-1, double.MaxValue, 0.01, 0, false, false, false);

            this.FHost.CreateValueInput("Start Position", 1, null, TSliceMode.Single, TPinVisibility.True, out this.FPinInStart);
            this.FPinInStart.SetSubType(0, double.MaxValue, 0.01, 0, false, false, false);

            this.FHost.CreateValueInput("End Position", 1, null, TSliceMode.Single, TPinVisibility.True, out this.FPinInEnd);
            this.FPinInEnd.SetSubType(0, double.MaxValue, 0.01, 0, false, false, false);

            this.FHost.CreateValueInput("Markers", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out this.FPinInMarkers);
            this.FPinInMarkers.SetSubType(0, double.MaxValue, 0.01, 0, false, false, false);
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
            if (this.FPinInFile.PinIsChanged)
            {
                string path;
                this.FPinInFile.GetString(0, out path);
                if (File.Exists(path))
                {
                    this.FWaveForm = new WaveForm(path, new WAVEFORMPROC(MyWaveFormCallback), this);
                    this.FWaveForm.FrameResolution = 0.01f; // 10ms are nice
                    this.FWaveForm.CallbackFrequency = 2000; // every 30 seconds rendered (3000*10ms=30sec)
                    this.FWaveForm.ColorBackground = Color.WhiteSmoke;
                    this.FWaveForm.ColorLeft = Color.Gainsboro;
                    this.FWaveForm.ColorLeftEnvelope = Color.Gray;
                    this.FWaveForm.ColorRight = Color.LightGray;
                    this.FWaveForm.ColorRightEnvelope = Color.DimGray;
                    this.FWaveForm.ColorMarker = Color.DarkBlue;
                    this.FWaveForm.DrawWaveForm = WaveForm.WAVEFORMDRAWTYPE.Stereo;
                    this.FWaveForm.DrawMarker = WaveForm.MARKERDRAWTYPE.Line;
                    this.FWaveForm.MarkerLength = 1f;

                    this.FWaveForm.RenderStart(true, BASSFlag.BASS_DEFAULT);
                }
            }

            #region Start / End
            if (this.FPinInStart.PinIsChanged || this.FPinInEnd.PinIsChanged)
            {
                double start, end;
                this.FPinInStart.GetValue(0, out start);
                this.FPinInEnd.GetValue(0, out end);

                if (this.FWaveForm != null)
                {
                    if (start <= 0)
                    {
                        this.FFrameStart = -1;
                    }
                    else
                    {
                        this.FFrameStart = this.FWaveForm.Position2Frames(start);
                    }

                    
                    if (end <= 0 || end <= start)
                    {
                        this.FFrameEnd = -1;
                    }
                    else
                    {
                        this.FFrameEnd = this.FWaveForm.Position2Frames(end);
                    }
                    this.RedrawWF();
                }
            }
            #endregion

            #region Position Marker
            if (this.FPinInCurrentPosition.PinIsChanged || this.FPinInMarkers.PinIsChanged)
            {
                if (this.FWaveForm != null)
                {
                    double markerpos;
                    this.FWaveForm.ClearAllMarker();
                    for (int i = 0; i < this.FPinInMarkers.SliceCount; i++)
                    {
                        this.FPinInMarkers.GetValue(i, out markerpos);

                        if (markerpos > 0)
                        {
                            this.FWaveForm.AddMarker("Marker" + i.ToString(), markerpos);
                        }
                    }
                    
                    double position;
                    this.FPinInCurrentPosition.GetValue(0, out position);

                    if (position < 0)
                    {
                        this.FWaveForm.RemoveMarker("Position");
                    }
                    else
                    {
                        this.FWaveForm.AddMarker("Position", position);
                    }
                    this.RedrawWF();
                }
            }
            #endregion
        }
        #endregion

        #region All stuff to redraw
        private void MyWaveFormCallback(int framesDone, int framesTotal, TimeSpan elapsedTime, bool finished)
        {
            this.RedrawWF();
        }

        private void RedrawWF()
        {
            if (this.FWaveForm != null)
                this.BackgroundImage = this.FWaveForm.CreateBitmap(this.Width, this.Height,this.FFrameStart,this.FFrameEnd, true);
            else
                this.BackgroundImage = null;
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            this.RedrawWF();
        }
        #endregion

    }
}
