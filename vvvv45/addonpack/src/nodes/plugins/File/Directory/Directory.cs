#region licence/info

//////project name
//vvvv plugin - Directory (File)

//////description
//Checks if a directory exists, can create, delete and rename

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
using System.IO;
using VVVV.PluginInterfaces.V1;

//the vvvv node namespace
namespace VVVV.Nodes
{
	
	//class definition
	public class Directory: IPlugin, IDisposable
    {	          	
    	#region field declaration
    	
    	//the host (mandatory)
    	private IPluginHost FHost; 
    	// Track whether Dispose has been called.
   		private bool FDisposed = false;
    	
    	//input pin declaration
    	private IStringIn FDir;
    	private IValueIn FCreate;
    	private IValueIn FDelete;
    	private IStringIn FNewDir;
    	private IValueIn FRename;
    	
    	//output pin declaration
    	private IValueOut FExists;
		
    	#endregion field declaration
       
    	#region constructor/destructor
    	
        public Directory()
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
	        	
        		FHost.Log(TLogType.Debug, "Directory is being deleted");
        		
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
        ~Directory()
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
					//see: http://www.vvvv.org/tiki-index.php?page=vvvv+naming+conventions
					FPluginInfo = new PluginInfo();
					
					//the nodes main name: use CamelCaps and no spaces
					FPluginInfo.Name = "Directory";
					//the nodes category: try to use an existing one
					FPluginInfo.Category = "File";
					//the nodes version: optional. leave blank if not
					//needed to distinguish two nodes of the same name and category
					FPluginInfo.Version = "";
					
					//the nodes author: your sign
					FPluginInfo.Author = "woei";
					//describe the nodes function
					FPluginInfo.Help = "Checks if a directory exists, can create, delete and rename";
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
	    	FHost.CreateStringInput("Directory", TSliceMode.Dynamic, TPinVisibility.True, out FDir);
	    	FDir.SetSubType2(@"C:\", -1, string.Empty, TStringType.Directory);
	    	
	    	FHost.CreateValueInput("Create", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out FCreate);
	    	FCreate.SetSubType(0,1,1,0,true,false, false);
	    	
	    	FHost.CreateValueInput("Remove", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out FDelete);
	    	FDelete.SetSubType(0,1,1,0,true,false, false);
	    	
	    	FHost.CreateStringInput("New Name", TSliceMode.Dynamic, TPinVisibility.True, out FNewDir);
	    	FNewDir.SetSubType2(@"C:\", -1, string.Empty, TStringType.Directory);
	    	
	    	FHost.CreateValueInput("Rename", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out FRename);
	    	FRename.SetSubType(0,1,1,0,true,false, false);
	    	
	    	//create outputs	    	
	    	FHost.CreateValueOutput("Exists", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out FExists);
	    	FExists.SetSubType(0, 1, 1, 0, false, true, false);
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
        	if (FDir.PinIsChanged ||
        	    FCreate.PinIsChanged ||
        	    FDelete.PinIsChanged ||
        	    FNewDir.PinIsChanged ||
        	    FRename.PinIsChanged)
        	{	    
        		string currentDir;
        		double currentCreate;
        		double currentDelete;
        		string curNewDir;
        		double currentRename;
        		
        		FExists.SliceCount = SpreadMax;
        		
        		//loop for all slices
        		for (int i=0; i<=SpreadMax; i++)
        		{		
        			FDir.GetString(i, out currentDir);
        			FCreate.GetValue(i, out currentCreate);
        			FDelete.GetValue(i, out currentDelete);
        			FNewDir.GetString(i, out curNewDir);
        			FRename.GetValue(i, out currentRename);
        			
        			string hostPath;
        			FHost.GetHostPath(out hostPath);
        			hostPath = Path.GetDirectoryName(hostPath);

        			
        			if (!Path.IsPathRooted(currentDir))
        			{
        				currentDir = Path.Combine(hostPath, currentDir);
        			}
        			
        			System.IO.DirectoryInfo curDirectory = new System.IO.DirectoryInfo(currentDir);
        			
        			if (!curDirectory.Exists) 
        			{
        				if (currentCreate==1) 
        				{
        					curDirectory.Create();
        					FExists.SetValue(i, 1.0);
        				}
        				else 
        				{
        					FExists.SetValue(i, 0.0);	
        				}
        			}
        			else 
        			{
        				if (currentDelete==1)
        				{
        					curDirectory.Delete();
        					FExists.SetValue(i, 0.0);
        				}
        				else
        				{
        					if (currentRename==1)
        					{
        						if (!Path.IsPathRooted(curNewDir))
        						{
        							curNewDir = Path.Combine(hostPath, curNewDir);
        						}
        						curDirectory.MoveTo(curNewDir);
        						FExists.SetValue(i, 0.0);
        					}
        					else
        					{
        						FExists.SetValue(i, 1.0);
        					}
        				}
	       			}    			      			
           		}
        	}
        	      	
        }
             
        #endregion mainloop  
	}
}
