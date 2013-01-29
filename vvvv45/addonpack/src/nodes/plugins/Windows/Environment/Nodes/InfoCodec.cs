#region licence/info

//////project name
//InfoCodec

//////description
//Retreives information about installed audio and video codecs

//////licence
//GNU Lesser General Public License (LGPL)
//english: http://www.gnu.org/licenses/lgpl.html
//german: http://www.gnu.de/lgpl-ger.html

//////language/ide
//C# sharpdevelop 

//////dependencies
//VVVV.PluginInterfaces.V1;

//////initial author
//woei

#endregion licence/info

//use what you need
using System;
using System.ComponentModel;
using System.Management;
using System.Collections.Generic;
using VVVV.PluginInterfaces.V1;

//the vvvv node namespace
namespace VVVV.Nodes
{
	
	//class definition
	public class InfoCodec: IPlugin, IDisposable
    {	          	
    	#region field declaration
    	
    	//the host (mandatory)
    	private IPluginHost FHost;
    	// Track whether Dispose has been called.
   		private bool FDisposed = false;
    	
    	//input pin declaration
    	private IValueIn FRefresh;
    	
    	//output pin declaration
    	private IStringOut FAudio;
    	private IStringOut FVideo;
		
    	#endregion field declaration
       
    	#region constructor/destructor
    	
       public InfoCodec()
        {
			//the nodes constructor
			//nothing to declare for this node
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
	        	
        		FHost.Log(TLogType.Debug, "Info (System Codec) is being deleted");
        		
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
        ~InfoCodec()
        {
        	// Do not re-create Dispose clean-up code here.
        	// Calling Dispose(false) is optimal in terms of
        	// readability and maintainability.
        	Dispose(false);
        }

        #endregion constructor/destructor
        
        #region node name and infos
       
        //provide node infos 
        public static IPluginInfo PluginInfo
	    {
	        get 
	        {
	        	//fill out nodes info
	        	IPluginInfo Info = new PluginInfo();
	        	Info.Name = "Info";
	        	Info.Category = "System";
	        	Info.Version = "Codec";
	        	Info.Help = "Retrieves information about audio and video codecs";
	        	Info.Bugs = "";
	        	Info.Credits = "";
	        	Info.Warnings = "";
				Info.Author = "woei";
	        	
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

        public bool AutoEvaluate
        {
        	//return true if this node needs to calculate every frame even if nobody asks for its output
        	get {return false;}
        }
        
        #endregion node name and infos
        
      	#region pin creation
        
        //this method is called by vvvv when the node is created
        public void SetPluginHost(IPluginHost Host)
	    {
        	//assign host
	    	FHost = Host;

	    	//create inputs
	    	FHost.CreateValueInput("Update", 1, null, TSliceMode.Single, TPinVisibility.True, out FRefresh);
	    	FRefresh.SetSubType(0,1,1,0,true,false, false);
	    	
	    	//create outputs
	    	FHost.CreateStringOutput("Audio", TSliceMode.Dynamic, TPinVisibility.True, out FAudio);
	    	FAudio.SetSubType("", false);
	    	
	    	FHost.CreateStringOutput("Video", TSliceMode.Dynamic, TPinVisibility.True, out FVideo);
	    	FVideo.SetSubType("", false);
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
        	//compute only on refresh	
        	if (FRefresh.PinIsChanged)
        	{	
        		//Audio
        		List<string> laudioOut = new List<string>();
        		string[] audioNames = new string[6] {
        			"Description","Name",
        			"Version","Manufacturer",
        			"System","Status"};
        		laudioOut.AddRange(GetManagementClassProperties("Win32_CodecFile",audioNames,new string[2]{"Group","Audio"}));
        		FAudio.SliceCount=laudioOut.Count;
        		for (int i=0; i<laudioOut.Count; i++)
        			FAudio.SetString(i, laudioOut[i]);	
        		
        		//video
        		List<string> lvideoOut = new List<string>();
        		lvideoOut.AddRange(GetManagementClassProperties("Win32_CodecFile",audioNames,new string[2]{"Group","Video"}));
        		FVideo.SliceCount=lvideoOut.Count;
        		for (int i=0; i<lvideoOut.Count; i++)
        			FVideo.SetString(i, lvideoOut[i]);
        	}
        }
             
        #endregion mainloop
        
        private List<string> GetManagementClassProperties(string key, string[] properties, string[] filter)
        {
        	List<string> lOut = new List<string>();
        	ManagementClass mc = new ManagementClass(key);
        	mc.Options.UseAmendedQualifiers = true;
        	foreach (ManagementObject mo in mc.GetInstances())
        	{
        		bool take = false;
        		try
        		{
        			if (filter[1]==(string)mo.Properties[filter[0]].Value)
        				take=true;
        		}
        		catch
        		{}
        		string pageOut = string.Empty;
        		if (take)
        		{
        		foreach (string pdn in properties)
        		{
        			string line = string.Empty;
        			try
        			{
        				string curProp = mo.Properties[pdn].Value.GetType().ToString();
        				if (pageOut!=string.Empty)
        					pageOut+=Environment.NewLine;
        				switch (curProp)
        				{
        					case "System.String[]":
        						string[] str = (string[])mo.Properties[pdn].Value;
        						foreach (string st in str)
        							line += st + " ";
        						break;
        					case "System.UInt16[]":
        						ushort[] shortData = (ushort[])mo.Properties[pdn].Value;
        						foreach (ushort st in shortData)
        							line += st.ToString() + " ";
        						break;
        					default:
        						line = mo.Properties[pdn].Value.ToString();
        						break;
        				}
        				pageOut+=pdn+": "+line;
        			}
        			catch
        			{
        			}
        		}
        		lOut.Add(pageOut);
        	}
        	}
        	return lOut;
        }
        
        private List<string> GetManagementClassProperties(string key, string[] properties)
        {
        	List<string> lOut = new List<string>();
        	ManagementClass mc = new ManagementClass(key);
        	mc.Options.UseAmendedQualifiers = true;
        	foreach (ManagementObject mo in mc.GetInstances())
        	{
        		string pageOut = string.Empty;
        		foreach (string pdn in properties)
        		{
        			string line = string.Empty;
        			try
        			{
        				string curProp = mo.Properties[pdn].Value.GetType().ToString();
        				if (pageOut!=string.Empty)
        					pageOut+=Environment.NewLine;
        				switch (curProp)
        				{
        					case "System.String[]":
        						string[] str = (string[])mo.Properties[pdn].Value;
        						foreach (string st in str)
        							line += st + " ";
        						break;
        					case "System.UInt16[]":
        						ushort[] shortData = (ushort[])mo.Properties[pdn].Value;
        						foreach (ushort st in shortData)
        							line += st.ToString() + " ";
        						break;
        					default:
        						line = mo.Properties[pdn].Value.ToString();
        						break;
        				}
        				pageOut+=pdn+": "+line;
        			}
        			catch
        			{
        			}
        		}
        		lOut.Add(pageOut);
        		
        	}
        	return lOut;
        }
        
        private List<string> GetManagementClassProperties(string key)
        {
        	List<string> lOut = new List<string>();
        	ManagementClass mc = new ManagementClass(key);
        	mc.Options.UseAmendedQualifiers = true;
        	foreach (ManagementObject mo in mc.GetInstances())
        	{
        		List<string> properties = new List<string>();
        		foreach (PropertyData pd in mc.Properties)
        			properties.Add(pd.Name);
        			
        		string pageOut = string.Empty;
        		foreach (string pdn in properties)
        		{
        			string line = string.Empty;
        			try
        			{
        				string curProp = mo.Properties[pdn].Value.GetType().ToString();
        				if (pageOut!=string.Empty)
        					pageOut+=Environment.NewLine;
        				switch (curProp)
        				{
        					case "System.String[]":
        						string[] str = (string[])mo.Properties[pdn].Value;
        						foreach (string st in str)
        							line += st + " ";
        						break;
        					case "System.UInt16[]":
        						ushort[] shortData = (ushort[])mo.Properties[pdn].Value;
        						foreach (ushort st in shortData)
        							line += st.ToString() + " ";
        						break;
        					default:
        						line = mo.Properties[pdn].Value.ToString();
        						break;
        				}
        				pageOut+=pdn+": "+line;
        			}
        			catch
        			{
        			}
        		}
        		lOut.Add(pageOut);
        		
        	}
        	return lOut;
        }
	}
}
