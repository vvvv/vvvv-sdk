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
using System.Runtime.InteropServices;
using System.Drawing;
using System.Collections.Generic;
using System.Collections;

using VVVV.PluginInterfaces.V1;
using VVVV.Utils.VColor;
using VVVV.Utils.VMath;

using VVVV.Collada;
using VVVV.Collada.ColladaDocument;
using VVVV.Collada.ColladaPipeline;
using VVVV.Collada.ColladaModel;

using SlimDX.Direct3D9;

//the vvvv node namespace
namespace VVVV.Nodes
{
	//class definition
	public class PluginColladaLoader: IPlugin, IDisposable, IColladaModelNodeIO
    {	          	
    	#region field declaration
    	
    	//the host (mandatory)
    	private IPluginHost FHost; 
    	// Track whether Dispose has been called.
   		private bool FDisposed = false;

    	//input pin declaration
    	private IStringIn FFileNameInput;
    	
    	//output pin declaration
    	private INodeOut FColladaModelOutput;
    	
    	private Model FColladaModel;
    	
    	#endregion field declaration
       
    	#region constructor/destructor
    	
        public PluginColladaLoader()
        {
			//the nodes constructor
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
	        	
        		FHost.Log(TLogType.Debug, "PluginColladaLoader is being deleted");
        		
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
        ~PluginColladaLoader()
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
					FPluginInfo.Name = "ColladaFile";
					//the nodes category: try to use an existing one
					FPluginInfo.Category = "EX9.Geometry";
					//the nodes version: optional. leave blank if not
					//needed to distinguish two nodes of the same name and category
					FPluginInfo.Version = "";
					
					//the nodes author: your sign
					FPluginInfo.Author = "vvvv group";
					//describe the nodes function
					FPluginInfo.Help = "Loads a COLLADA *.dae file.";
					//specify a comma separated list of tags that describe the node
					FPluginInfo.Tags = "Collada,dae,load,read";
					
					//give credits to thirdparty code used
					FPluginInfo.Credits = "https://collada.org/public_forum/viewtopic.php?t=676";
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
	    	FHost.CreateStringInput("Filename", TSliceMode.Single, TPinVisibility.True, out FFileNameInput);
			FFileNameInput.SetSubType("", true);
	    	
	    	//create outputs	    	
	    	FHost.CreateNodeOutput("COLLADA Model", TSliceMode.Dynamic, TPinVisibility.True, out FColladaModelOutput);
	    	FColladaModelOutput.SetSubType(new Guid[1]{ColladaModelNodeIO.GUID}, ColladaModelNodeIO.FriendlyName);
	    	FColladaModelOutput.SetInterface(this);
	    	
	    	COLLADAUtil.Logger = new LoggerWrapper(FHost);
        } 

        #endregion pin creation
        
        #region IColladaModelNodeIO
		public void GetSlice(int Index, out Model ColladaModel)
		{
			ColladaModel = FColladaModel;
		}
		
        #endregion
        
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
        	if (FFileNameInput.PinIsChanged)
			{
        		FColladaModel = null;
        		FColladaModelOutput.SliceCount = 0;
        		
        		string filename;
    			FFileNameInput.GetString(0, out filename);
    			if (filename.Length == 0) 
    			{
    				FColladaModelOutput.SliceCount = 0;
    				return;
    			}
    			
    			Log(TLogType.Message, "Loading " + filename);
    			try
				{
					Document colladaDocument = new Document(filename);
					Conditioner.ConvexTriangulator(colladaDocument);
					//Conditioner.Reindexor(colladaDocument);
					FColladaModel = new Model(colladaDocument);
					
					Log(TLogType.Message, filename + " loaded.");
				}
				catch (Exception e)
				{
					Log(TLogType.Error, e.Message);
					Log(TLogType.Debug, e.StackTrace);
				}
				
				FColladaModelOutput.SliceCount = 1;
			}     	
        }
             
        #endregion mainloop  
        
        #region helper functions
		private void Log(TLogType logType, string message)
		{
			FHost.Log(logType, "ColladaLoader: " + message);
		}
        #endregion
	}
}
