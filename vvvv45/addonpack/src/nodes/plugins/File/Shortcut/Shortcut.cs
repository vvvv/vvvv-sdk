#region licence/info

//////project name
//Shortcut

//////description
//create a shortcut to a file/folder
//resolve shortcut destination


//////licence
//GNU Lesser General Public License (LGPL)
//english: http://www.gnu.org/licenses/lgpl.html
//german: http://www.gnu.de/lgpl-ger.html

//////language/ide
//C# sharpdevelop 

//////dependencies
//VVVV.PluginInterfaces.V1;
//IWshRuntimeLibrary;

//////initial author
//woei

#endregion licence/info

//use what you need
using System;
using VVVV.PluginInterfaces.V1;
using IWshRuntimeLibrary;

//the vvvv node namespace
namespace VVVV.Nodes
{
	
	//class definition
	public class ShortcutCreate: IPlugin, IDisposable
    {	          	
    	#region field declaration
    	
    	//the host (mandatory)
    	private IPluginHost FHost; 
    	// Track whether Dispose has been called.
   		private bool FDisposed = false;

    	//input pin declaration
    	private IStringIn FSource;
    	private IStringIn FDestination;
    	private IValueIn FDo;
    	
    	//output pin declaration
    	private IValueOut FDone;
    	
    	#endregion field declaration
       
    	#region constructor/destructor
    	
        public ShortcutCreate()
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
	        	
        		FHost.Log(TLogType.Debug, "Shortcut (File Create) is being deleted");
        		
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
        ~ShortcutCreate()
        {
        	// Do not re-create Dispose clean-up code here.
        	// Calling Dispose(false) is optimal in terms of
        	// readability and maintainability.
        	Dispose(false);
        }
        #endregion constructor/destructor
        
        #region node name and infos
       
        //provide node infos 
        private static IPluginInfo FPluginInfo;
        public static IPluginInfo PluginInfo
	    {
	        get 
	        {
	        	if (FPluginInfo == null)
				{
					//fill out nodes info
					//see: http://www.vvvv.org/tiki-index.php?page=Conventions.NodeAndPinNaming
					FPluginInfo = new PluginInfo();
					
					//the nodes main name: use CamelCaps and no spaces
					FPluginInfo.Name = "Shortcut";
					//the nodes category: try to use an existing one
					FPluginInfo.Category = "File";
					//the nodes version: optional. leave blank if not
					//needed to distinguish two nodes of the same name and category
					FPluginInfo.Version = "Create";
					
					//the nodes author: your sign
					FPluginInfo.Author = "woei";
					//describe the nodes function
					FPluginInfo.Help = "creates shortcuts to files/folders";
					//specify a comma separated list of tags that describe the node
					FPluginInfo.Tags = "";
					
					//give credits to thirdparty code used
					FPluginInfo.Credits = "";
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
				}
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
	    	FHost.CreateStringInput("Source", TSliceMode.Dynamic, TPinVisibility.True, out FSource);
	    	FSource.SetSubType("c:", true);	
	    	
	    	FHost.CreateStringInput("Destination", TSliceMode.Dynamic, TPinVisibility.True, out FDestination);
	    	FDestination.SetSubType(@"c:\self", false);	

	    	FHost.CreateValueInput("Save", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out FDo);
	    	FDo.SetSubType(0, 1, 1, 0, true, false, false);
	    	
	    	//create outputs	    	
	    	FHost.CreateValueOutput("Done", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out FDone);
	    	FDone.SetSubType(0, 1, 1, 0, true, false, false);
	    	
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
        	//if any of the inputs has changed
        	//recompute the outputs
        	if (FSource.PinIsChanged || 
        	    FDestination.PinIsChanged ||
        	    FDo.PinIsChanged)
        	{	
	        	//first set slicecounts for all outputs
	        	//the incoming int SpreadMax is the maximum slicecount of all input pins, which is a good default
	        	FDone.SliceCount = SpreadMax; 	
	        	
	        	//the variables to fill with the input data
	        	double curDo;
	        	string curSource, curDest;

	        	
        		//loop for all slices
        		for (int i=0; i<SpreadMax; i++)
        		{		
        			//read data from inputs
        			FSource.GetString(i, out curSource);
        			FDestination.GetString(i, out curDest);
        			FDo.GetValue(i, out curDo);

        			double curDone = 0;
        			if (curDo == 1)
        			{
        			IWshShell wsh = new WshShellClass();
        			IWshShortcut curShortcut = (IWshShortcut)wsh.CreateShortcut(curDest+".lnk");
					curShortcut.WindowStyle = 1; //for default window, 3 for maximize, 7 for minimize
					curShortcut.TargetPath = curSource; //for me, or any valid Path string
					curShortcut.Save();
        			curDone=1;
        			}

        			//write data to outputs
        			FDone.SetValue(i, curDone);

        		}
        	}      	
        }
             
        #endregion mainloop  
	}
	
	//class definition
	public class ShortcutResolve: IPlugin, IDisposable
    {	          	
    	#region field declaration
    	
    	//the host (mandatory)
    	private IPluginHost FHost; 
    	// Track whether Dispose has been called.
   		private bool FDisposed = false;

    	//input pin declaration
    	private IStringIn FShortcut;
    	
    	//output pin declaration
    	private IStringOut FDestination;
    	private IStringOut FWorkingDir;
    	private IStringOut FIcon;
    	
    	#endregion field declaration
       
    	#region constructor/destructor
    	
        public ShortcutResolve()
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
	        	
        		FHost.Log(TLogType.Debug, "Shortcut (File Resolve) is being deleted");
        		
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
        ~ShortcutResolve()
        {
        	// Do not re-create Dispose clean-up code here.
        	// Calling Dispose(false) is optimal in terms of
        	// readability and maintainability.
        	Dispose(false);
        }
        #endregion constructor/destructor
        
        #region node name and infos
       
        //provide node infos 
        private static IPluginInfo FPluginInfo;
        public static IPluginInfo PluginInfo
	    {
	        get 
	        {
	        	if (FPluginInfo == null)
				{
					//fill out nodes info
					//see: http://www.vvvv.org/tiki-index.php?page=Conventions.NodeAndPinNaming
					FPluginInfo = new PluginInfo();
					
					//the nodes main name: use CamelCaps and no spaces
					FPluginInfo.Name = "Shortcut";
					//the nodes category: try to use an existing one
					FPluginInfo.Category = "File";
					//the nodes version: optional. leave blank if not
					//needed to distinguish two nodes of the same name and category
					FPluginInfo.Version = "Resolve";
					
					//the nodes author: your sign
					FPluginInfo.Author = "woei";
					//describe the nodes function
					FPluginInfo.Help = "resolves the destination of a shortcut";
					//specify a comma separated list of tags that describe the node
					FPluginInfo.Tags = "";
					
					//give credits to thirdparty code used
					FPluginInfo.Credits = "";
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
				}
				return FPluginInfo;
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
	    	FHost.CreateStringInput("Shortcut", TSliceMode.Dynamic, TPinVisibility.True, out FShortcut);
	    	FShortcut.SetSubType(@"c:\dummy.lnk", false);	
	    	
	    	//create outputs	    	
	    	FHost.CreateStringOutput("Destination", TSliceMode.Dynamic, TPinVisibility.True, out FDestination);
	    	FDestination.SetSubType("", true);
	    	
	    	FHost.CreateStringOutput("Working Directory", TSliceMode.Dynamic, TPinVisibility.OnlyInspector, out FWorkingDir);
	    	FWorkingDir.SetSubType("", true);
	    	
	    	FHost.CreateStringOutput("Icon Location", TSliceMode.Dynamic, TPinVisibility.OnlyInspector, out FIcon);
	    	FIcon.SetSubType("", true);
	    	
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
        	//if any of the inputs has changed
        	//recompute the outputs
        	if (FShortcut.PinIsChanged)
        	{	
	        	FDestination.SliceCount = SpreadMax; 	
	        	
	        	string curSource;        	
        		for (int i=0; i<SpreadMax; i++)
        		{		
        			FShortcut.GetString(i, out curSource);
					try
					{
        			IWshShell wsh = new WshShellClass();
        			IWshShortcut curShortcut = (IWshShortcut)wsh.CreateShortcut(curSource);
        				
        			FDestination.SetString(i, curShortcut.TargetPath);
        			FWorkingDir.SetString(i, curShortcut.WorkingDirectory);
        			FIcon.SetString(i, curShortcut.IconLocation);
					}
					catch
					{
						FHost.Log(TLogType.Error, "couldn't resolve shortcut "+curSource);
						FDestination.SetString(i, string.Empty);
						FWorkingDir.SetString(i, string.Empty);
						FIcon.SetString(i, string.Empty);
					}

        		}
        	}      	
        }
             
        #endregion mainloop  
	}
}
