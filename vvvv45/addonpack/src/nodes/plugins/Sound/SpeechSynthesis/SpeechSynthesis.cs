using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using SpeechLib;
using VVVV.PluginInterfaces.V1;
using VVVV.Utils.VColor;
using VVVV.Utils.VMath;

//the vvvv node namespace
namespace VVVV.Nodes
{
	
	//class definition
	public class SpeechSynthesis: IPlugin, IDisposable
    {	          	
    	#region field declaration
    	
    	//the host (mandatory)
    	private IPluginHost FHost; 
    	// Track whether Dispose has been called.
   		private bool FDisposed = false;

    	//input pin declaration
    	private IStringIn FStringInput;
        private IValueIn FSpeedInput;
        private IValueIn FSpeakInput;
    	private IEnumIn FNarratorInput;

    	//output pin declaration
    	private IValueOut FDoneOutput;

        private string input_string = "";
        private bool done = false;
        private SpVoice vox = new SpVoice();
    	    	
    	#endregion field declaration
       
    	#region constructor/destructor
    	
        public SpeechSynthesis()
        {
			//the nodes constructor

            //attach the event handler:
            vox.EndStream += 
                new SpeechLib._ISpeechVoiceEvents_EndStreamEventHandler(this.tts_EndStream);
		}
        
        // Implementing IDisposable's Dispose method.
        // Do not make this method virtual.
        // A derived class should not be able to override this method.
        public void Dispose()
        {
        	Dispose(true);
        	// Take yourself off the Finalization queue
        	// to prevent finalization code for this object
        	// from executing a second time.
        	GC.SuppressFinalize(this);
        }
        
        // Dispose(bool disposing) executes in two distinct scenarios.
        // If disposing equals true, the method has been called directly
        // or indirectly by a user's code. Managed and unmanaged resources
        // can be disposed.
        // If disposing equals false, the method has been called by the
        // runtime from inside the finalizer and you should not reference
        // other objects. Only unmanaged resources can be disposed.
        protected virtual void Dispose(bool disposing)
        {
        	// Check to see if Dispose has already been called.
        	if(!FDisposed)
        	{
        		if(disposing)
        		{
        			// Dispose managed resources.
        		}
        		// Release unmanaged resources. If disposing is false,
        		// only the following code is executed.
	        	
        		FHost.Log(TLogType.Debug, "SpeechSynthesis is being deleted");
        		
        		// Note that this is not thread safe.
        		// Another thread could start disposing the object
        		// after the managed resources are disposed,
        		// but before the disposed flag is set to true.
        		// If thread safety is necessary, it must be
        		// implemented by the client.
        	}
        	FDisposed = true;
        }

        // Use C# destructor syntax for finalization code.
        // This destructor will run only if the Dispose method
        // does not get called.
        // It gives your base class the opportunity to finalize.
        // Do not provide destructors in types derived from this class.
        ~SpeechSynthesis()
        {
        	// Do not re-create Dispose clean-up code here.
        	// Calling Dispose(false) is optimal in terms of
        	// readability and maintainability.
        	Dispose(false);
        }
        #endregion constructor/destructor
        
        #region node name and infos
       
        public static IPluginInfo PluginInfo
	    {
	        get 
	        {
				IPluginInfo FPluginInfo = new PluginInfo();
				//the nodes main name: use CamelCaps and no spaces
				FPluginInfo.Name = "SpeechSynthesis";
				//the nodes category: try to use an existing one
				FPluginInfo.Category = "String";
				//the nodes version: optional. leave blank if not
				//needed to distinguish two nodes of the same name and category
				FPluginInfo.Version = "";
				
				//the nodes author: your sign
				FPluginInfo.Author = "dep";
				//describe the nodes function
				FPluginInfo.Help = "Interface to Microsoft Speech API. SAPI must be installed.";
				//specify a comma separated list of tags that describe the node
				FPluginInfo.Tags = "speech synthesis, sapi";
				
				//give credits to thirdparty code used
				FPluginInfo.Credits = "Scott Lysle";
				//any known problems?
				FPluginInfo.Bugs = "";
				//any known usage of the node that may cause troubles?
				FPluginInfo.Warnings = "";
				
				//leave below as is
				System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace(true);
				System.Diagnostics.StackFrame sf = st.GetFrame(0);
				System.Reflection.MethodBase method = sf.GetMethod();
				FPluginInfo.Namespace = method.DeclaringType.Namespace;
				FPluginInfo.Class = method.DeclaringType.Name;
				//leave above as is
				return FPluginInfo;
	        }
		}

        public bool AutoEvaluate
        {
        	//return true if this node needs to calculate every frame even if nobody asks for its output
        	get {return true;}
        }
        
        #endregion node name and infos
        
      	#region pin creation
        
        //this method is called by vvvv when the node is created
        public void SetPluginHost(IPluginHost Host)
	    {
        	//assign host
	    	FHost = Host;

	    	//create inputs
	    	FHost.CreateStringInput("Input", TSliceMode.Single, TPinVisibility.True, out FStringInput);
            FStringInput.SetSubType("", false);

	    	FHost.CreateValueInput("Speed", 1, null, TSliceMode.Single, TPinVisibility.True, out FSpeedInput);
            //Supported values range from -10 to 10
	    	FSpeedInput.SetSubType(-10.0, 10.0, 1.0, 5.0, false, false, true);

	    	FHost.CreateValueInput("Speak", 1, null, TSliceMode.Single, TPinVisibility.True, out FSpeakInput);
	    	FSpeakInput.SetSubType(0.0, 1.0, 1.0, 0.0, true, false, true);

            string[] narrators = new string[vox.GetVoices(string.Empty, string.Empty).Count];
            int i = 0;
            foreach (ISpeechObjectToken Token in vox.GetVoices(string.Empty, string.Empty))
            {
                narrators[i++] = Token.GetAttribute("Name");
            }

            FHost.CreateEnumInput("Narrator", TSliceMode.Single, TPinVisibility.True, out FNarratorInput);
            FNarratorInput.SetSubType("SpeechSynthesisNarrator");
            FHost.UpdateEnum("SpeechSynthesisNarrator", narrators[0], narrators);

            //create outputs
            FHost.CreateValueOutput("Done", 1, null, TSliceMode.Single, TPinVisibility.True, out FDoneOutput);
            FDoneOutput.SetSubType(0.0, 1.0, 1.0, 0.0, true, false, true);
        }

        #endregion pin creation
        
        #region mainloop
        
        public void Configurate(IPluginConfig Input)
        {
        	//nothing to configure in this plugin
        	//only used in conjunction with inputs of type cmpdConfigurate
        }
        
        //here we go, thats the method called by vvvv each frame
        //all data handling should be in here
        public void Evaluate(int SpreadMax)
        {     	
        	if (FSpeedInput.PinIsChanged || FNarratorInput.PinIsChanged || FStringInput.PinIsChanged)
        	{
        		//loop for all slices
        		for (int i=0; i<SpreadMax; i++)
        		{		
        			//read data from inputs
                    double rate = 5.0;
                    FSpeedInput.GetValue(i, out rate);
                    vox.Rate = (int)rate;
                    int voiceindex = 0;
                    FNarratorInput.GetOrd(i, out voiceindex);
                    vox.Voice = vox.GetVoices(string.Empty, string.Empty).Item(voiceindex);
                    FStringInput.GetString(i, out input_string);
        		}
        	}      	
        	if (FSpeakInput.PinIsChanged)
        	{
                double speak = 0.0;
                FSpeakInput.GetValue(0, out speak);
                if (speak > 0.5)
                {
                    vox.Speak(input_string, SpeechVoiceSpeakFlags.SVSFlagsAsync|SpeechVoiceSpeakFlags.SVSFPurgeBeforeSpeak);
                }
            }
            //remember that we are running with autoevaluate == true
            if (done == true)   //endstream was fired
            {
                done = false;
                FDoneOutput.SetValue(0, 1.0);
            }
            else
            {
                FDoneOutput.SetValue(0, 0.0);
            }
        }

        #endregion mainloop

        //event handler for speech endstream event:
        private void tts_EndStream(int StreamNumber, object StreamPosition)
        {
            done = true;
        }   
    }
}
