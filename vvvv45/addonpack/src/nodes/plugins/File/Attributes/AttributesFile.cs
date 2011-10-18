#region licence/info

//////project name
//AttributesFile

//////description
//gets and sets file attributes: hidden, readonly, system


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
	public class AttributesFile: IPlugin, IDisposable
    {	          	
    	#region field declaration
    	
    	//the host (mandatory)
    	private IPluginHost FHost; 
    	// Track whether Dispose has been called.
   		private bool FDisposed = false;

    	//input pin declaration
    	private IStringIn FInput;
    	private IValueIn FHidden;
    	private IValueIn FReadOnly;
    	private IValueIn FSystem;
    	private IValueIn FSet;
    	
    	//output pin declaration
    	private IValueOut FIsHidden;
    	private IValueOut FIsReadOnly;
    	private IValueOut FIsSystem;
    	
    	#endregion field declaration
       
    	#region constructor/destructor
    	
        public AttributesFile()
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
	        	
        		FHost.Log(TLogType.Debug, "AttributesFile is being deleted");
        		
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
        ~AttributesFile()
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
					FPluginInfo.Name = "Attributes";
					//the nodes category: try to use an existing one
					FPluginInfo.Category = "File";
					//the nodes version: optional. leave blank if not
					//needed to distinguish two nodes of the same name and category
					FPluginInfo.Version = "";
					
					//the nodes author: your sign
					FPluginInfo.Author = "woei";
					//describe the nodes function
					FPluginInfo.Help = "gets and sets file attributes: hidden, readonly, system";
					//specify a comma separated list of tags that describe the node
					FPluginInfo.Tags = "file, directory, attribute";
					
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
	    	FHost.CreateStringInput("Input", TSliceMode.Dynamic, TPinVisibility.True, out FInput);
	    	FInput.SetSubType("", true);
	    	
	    	FHost.CreateValueInput("Hidden", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out FHidden);
	    	FHidden.SetSubType(0, 1, 1, 0, false, true, false);
	    	
	    	FHost.CreateValueInput("ReadOnly", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out FReadOnly);
	    	FReadOnly.SetSubType(0, 1, 1, 0, false, true, false);
	    	
	    	FHost.CreateValueInput("System", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out FSystem);
	    	FSystem.SetSubType(0, 1, 1, 0, false, true, false);
	    	
	    	FHost.CreateValueInput("Set", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out FSet);
	    	FSet.SetSubType(0, 1, 1, 0, true, false, false);
	    	
	    	//create outputs	    	
	    	FHost.CreateValueOutput("Hidden", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out FIsHidden);
	    	FIsHidden.SetSubType(0, 1, 1, 0, false, true, false);
	    	
	    	FHost.CreateValueOutput("ReadOnly", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out FIsReadOnly);
	    	FIsReadOnly.SetSubType(0, 1, 1, 0, false, true, false);
	    	
	    	FHost.CreateValueOutput("System", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out FIsSystem);
	    	FIsSystem.SetSubType(0, 1, 1, 0, false, true, false);
	    	
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
        	if (FInput.PinIsChanged || FSet.PinIsChanged)
        	{	
        		FIsHidden.SliceCount=SpreadMax;
        		FIsReadOnly.SliceCount=SpreadMax;
        		FIsSystem.SliceCount=SpreadMax;
        		
        		string curFile;
        		double doSet, h, r, s;
        		for (int i=0; i<SpreadMax; i++)
        		{
        			FInput.GetString(i, out curFile);
        			h=0;
        			r=0;
        			s=0;
        			try
        			{
        				FileInfo f = new FileInfo(curFile);
        				if ((f.Attributes & FileAttributes.Hidden) == FileAttributes.Hidden)
        					h=1;
        				if ((f.Attributes & FileAttributes.ReadOnly) == FileAttributes.ReadOnly)
        					r=1;
        				if ((f.Attributes & FileAttributes.System) == FileAttributes.System)
        					s=1;
        				
        				double _h, _r, _s;
        				FSet.GetValue(i, out doSet);
        				if (doSet>0.5)
        				{
        					FHidden.GetValue(i, out _h);
        					FReadOnly.GetValue(i, out _r);
        					FSystem.GetValue(i, out _s);
        					if (h!=_h)
        					{
        						f.Attributes ^= FileAttributes.Hidden;
        						h=_h;
        					}
        					if (r!=_r)
        					{
        						f.Attributes ^= FileAttributes.ReadOnly;
        						r=_r;
        					}
        					if (s!=_s)
        					{
        						f.Attributes ^= f.Attributes | FileAttributes.System;
        						s=_s;
        					}
        				}
        				
        				FIsHidden.SetValue(i, h);
        				FIsReadOnly.SetValue(i, r);
        				FIsSystem.SetValue(i, s);
        			}
        			catch
        			{
        				FHost.Log(TLogType.Debug, "AttributeFile: cannot handle "+curFile);
        			}
        			
        		}
        	}      	
        }
             
        #endregion mainloop  
	}
}
