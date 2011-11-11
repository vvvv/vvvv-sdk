#region licence/info

//////project name
//Info

//////description
//reads some file attributes; renames and removes files

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
	public class InfoFile: IPlugin, IDisposable
    {	          	
    	#region field declaration
    	
    	//the host (mandatory)
    	private IPluginHost FHost;
    	// Track whether Dispose has been called.
   		private bool FDisposed = false;
    	
    	//input pin declaration
    	private IStringIn FFile;
    	private IStringIn FCustomRoot;
    	private IValueIn FUpdate;
    	
    	//output pin declaration
    	private IValueOut FExists;
    	private IValueOut FFileSize;
    	private IValueOut FReadOnly;
    	private IValueOut FHidden;
		
    	#endregion field declaration
       
    	#region constructor/destructor
    	
       public InfoFile()
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
	        	
        		FHost.Log(TLogType.Debug, "Info (File) is being deleted");
        		
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
        ~InfoFile()
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
	        	Info.Category = "File";
	        	Info.Version = "";
	        	Info.Help = "Retrieves information of files";
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
	    	FHost.CreateStringInput("Filename", TSliceMode.Dynamic, TPinVisibility.True, out FFile);
	    	FFile.SetSubType("", true);
	    	
	    	FHost.CreateStringInput("Custom Root", TSliceMode.Dynamic, TPinVisibility.Hidden, out FCustomRoot);
	    	FCustomRoot.SetSubType("", false);
	    	
	    	FHost.CreateValueInput("Update", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out FUpdate);
	    	FUpdate.SetSubType(0,1,1,0,true,false, false);
	    	
	    	
	    	//create outputs	    	
	    	FHost.CreateValueOutput("Exists", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out FExists);
	    	FExists.SetSubType(0, 1, 1, 0, false, true, false);
	    	
	    	FHost.CreateValueOutput("Filesize", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out FFileSize);
	    	FFileSize.SetSubType(0, double.MaxValue, 0.01, 0, false, false, true);
	    	
	    	FHost.CreateValueOutput("Read Only", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out FReadOnly);
	    	FReadOnly.SetSubType(0, 1, 1, 0, false, true, false);
	    	
	    	FHost.CreateValueOutput("Hidden", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out FHidden);
	    	FHidden.SetSubType(0, 1, 1, 0, false, true, false);
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
        	if (FFile.PinIsChanged ||
        	    FCustomRoot.PinIsChanged ||
        	    FUpdate.PinIsChanged)
        	{	    
        		string currentFile;
        		string curCustomRoot;
        		double currentUpdate;
        		
        		FExists.SliceCount = SpreadMax;
        		FFileSize.SliceCount = SpreadMax;
        		FReadOnly.SliceCount = SpreadMax;
        		FHidden.SliceCount = SpreadMax;
        		
        		string hostPath;
        		FHost.GetHostPath(out hostPath);
        		
        		//loop for all slices
        		for (int i=0; i<=SpreadMax; i++)
        		{		
        			FFile.GetString(i, out currentFile);
        			FCustomRoot.GetString(i, out curCustomRoot);
        			FUpdate.GetValue(i, out currentUpdate);
        			
        			int exist = 0;
        			double fileSize = 0;
        			int readOnly = 0;
        			int hidden = 0;
        			
    
        			
        			if (!Path.IsPathRooted(currentFile))
        			{
        				if (curCustomRoot == string.Empty)
        				{
        					curCustomRoot=hostPath;
        				}
        				else
        				{
        					if (!Path.IsPathRooted(curCustomRoot))
        					{
        						string message = "\'";
        						message+=curCustomRoot;
        						message+= "\' is not a valid root-path";
        						FHost.Log(TLogType.Warning, message);
        						curCustomRoot=hostPath;
        					}
        				}
        				currentFile = Path.Combine(curCustomRoot, currentFile);
        			}
        			
        			if (File.Exists(currentFile))
        			{
        				exist = 1;
        				
        				FileInfo curFileInfo = new FileInfo(currentFile);
        				fileSize = (double)curFileInfo.Length;
        				
        				if ((File.GetAttributes(currentFile) & FileAttributes.ReadOnly) == FileAttributes.ReadOnly)
        				{
        					readOnly = 1;
        				}
        				if ((File.GetAttributes(currentFile) & FileAttributes.Hidden) == FileAttributes.Hidden)
        				{
        					hidden = 1;
        				}
        				
        			}
        			FExists.SetValue(i, (double)exist);
        			FFileSize.SetValue(i, fileSize);
        			FReadOnly.SetValue(i, (double)readOnly);
        			FHidden.SetValue(i, (double)hidden);
           		}
        	}
        	      	
        }
             
        #endregion mainloop  
	}
}
