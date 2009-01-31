#region licence/info

//////project name
//vvvv plugin template

//////description
//basic vvvv node plugin template.
//Copy this an rename it, to write your own plugin node.

//////licence
//GNU Lesser General Public License (LGPL)
//english: http://www.gnu.org/licenses/lgpl.html
//german: http://www.gnu.de/lgpl-ger.html

//////language/ide
//C# sharpdevelop 

//////dependencies
//VVVV.PluginInterfaces.V1;
//VVVV.Utils.VColor;
//VVVV.Utils.VMath;

//////initial author
//vvvv group

#endregion licence/info

//use what you need
using System;
using System.Drawing;
using VVVV.PluginInterfaces.V1;
using VVVV.Utils.VColor;
using VVVV.Utils.VMath;

//the vvvv node namespace
namespace VVVV.Nodes
{
	
	//class definition
	public class PluginEnumTemplate: IPlugin, IDisposable
    {	          	
    	#region field declaration
    	
    	//the host (mandatory)
    	private IPluginHost FHost; 
    	// Track whether Dispose has been called.
   		private bool FDisposed = false;

    	//input pin declaration
    	private IEnumIn FV4EnumInput;
    	private IEnumIn FCustomEnumInput;
    	private IEnumIn FDynamicEnumInput;
    	private IStringIn FEnumEntryInput;
    	
    	//output pin declaration
    	private IStringOut FEnumEntriesOutput;
    	private IStringOut FCustomSelectionOutput;
    	private IValueOut FDynamicCountOutput;
    	private IEnumOut FDynamicEnumOutput;
    	
    	#endregion field declaration
       
    	#region constructor/destructor
    	
        public PluginEnumTemplate()
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
	        	
        		FHost.Log(TLogType.Debug, "PluginTemplate is being deleted");
        		
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
        ~PluginEnumTemplate()
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
					FPluginInfo.Name = "Template";
					//the nodes category: try to use an existing one
					FPluginInfo.Category = "Template";
					//the nodes version: optional. leave blank if not
					//needed to distinguish two nodes of the same name and category
					FPluginInfo.Version = "Enum";
					
					//the nodes author: your sign
					FPluginInfo.Author = "vvvv group";
					//describe the nodes function
					FPluginInfo.Help = "Offers a basic code layout to start from when writing a vvvv plugin";
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
	    	FHost.CreateEnumInput("VVVV Enum", TSliceMode.Single, TPinVisibility.True, out FV4EnumInput);
	    	FV4EnumInput.SetSubType("AllEnums");
	    	
	    	FHost.UpdateEnum("CustomPluginEnum", "mene", new string[]{"ene", "mene", "muh", "un", "draus", "bis", "du"});
	    	FHost.CreateEnumInput("Custom Enum", TSliceMode.Single, TPinVisibility.True, out FCustomEnumInput);
	    	FCustomEnumInput.SetSubType("CustomPluginEnum");
	    	
	    	FHost.CreateEnumInput("Dynamic Enum", TSliceMode.Single, TPinVisibility.True, out FDynamicEnumInput);
	    	FDynamicEnumInput.SetSubType("DynamicPluginEnum");
	    	
	    	FHost.CreateStringInput("Custom Enum Entries", TSliceMode.Dynamic, TPinVisibility.True, out FEnumEntryInput);
	    	FEnumEntryInput.SetSubType("", false);	
	    	
	    	//create outputs	    	
	    	FHost.CreateStringOutput("VVVV Enum Entries", TSliceMode.Dynamic, TPinVisibility.True, out FEnumEntriesOutput);
	    	FEnumEntriesOutput.SetSubType("", false);
	    	
	    	FHost.CreateStringOutput("Custom Enum Selection", TSliceMode.Single, TPinVisibility.True, out FCustomSelectionOutput);
	    	FCustomSelectionOutput.SetSubType("", false);
	    	
	    	FHost.CreateValueOutput("Dynamic Enum Entry Count", 1, null, TSliceMode.Single, TPinVisibility.True, out FDynamicCountOutput);
	    	FDynamicCountOutput.SetSubType(0, int.MaxValue, 1, 0, false, false, true);
	    	
	    	FHost.CreateEnumOutput("Dynamic Enum Output", TSliceMode.Single, TPinVisibility.True, out FDynamicEnumOutput);
	    	FDynamicEnumOutput.SetSubType("DynamicPluginEnum");
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
        	if (FV4EnumInput.PinIsChanged)
        	{	
	        	string enumName;
	        	FV4EnumInput.GetString(0, out enumName);
	        	int count;
	        	FHost.GetEnumEntryCount(enumName, out count);
	        	FEnumEntriesOutput.SliceCount = count;
	        	
	        	string enumEntry;
	        	for (int i=0; i<count; i++)
	        	{
	        		FHost.GetEnumEntry(enumName, i, out enumEntry);
	        		FEnumEntriesOutput.SetString(i, enumEntry);
	        	}
        	}      
        	
        	if (FCustomEnumInput.PinIsChanged)
        	{
        		string selection;
        		FCustomEnumInput.GetString(0, out selection);
        		FCustomSelectionOutput.SetString(0, selection);
        	}
        	
        	if (FEnumEntryInput.PinIsChanged)
        	{
        		string[] entries = new string[FEnumEntryInput.SliceCount];
        		
        		for (int i=0; i<FEnumEntryInput.SliceCount; i++)
        			FEnumEntryInput.GetString(i, out entries[i]);
        		
        		FHost.UpdateEnum("DynamicPluginEnum", entries[0], entries);
        		
        		FDynamicCountOutput.SetValue(0, FEnumEntryInput.SliceCount);
        	}
        	
        	if (FDynamicEnumInput.PinIsChanged)
        	{
        		int entry;
        		FDynamicEnumInput.GetOrd(0, out entry);
        		FDynamicEnumOutput.SetOrd(0, entry);
        	}
        		
        }
             
        #endregion mainloop  
	}
}
