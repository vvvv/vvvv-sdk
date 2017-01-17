#region licence/info

//////project name
//MakePath

//////description
//combines strings to a path

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
using System.Collections.Generic;
using VVVV.PluginInterfaces.V1;

//the vvvv node namespace
namespace VVVV.Nodes
{
	
	//class definition
	public class MakePath: IPlugin, IDisposable
    {	          	
    	#region field declaration
    	
    	//the host (mandatory)
    	private IPluginHost FHost; 
    	// Track whether Dispose has been called.
   		private bool FDisposed = false;

   		//configs
   		private IValueConfig FInCount;
   		
    	//input pin declaration
    	private IValueIn FPrepend;
    	private IStringIn FIn1;
    	
		//output pin declaration
    	private IStringOut FOutput;
    	
    	//further fields
    	private List<IStringIn> FInList;
    	private bool evaluate;
    	
    	#endregion field declaration
       
    	#region constructor/destructor
    	
        public MakePath()
        {
			//the nodes constructor
			FInList = new List<IStringIn>();
			evaluate = false;
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
        			FInList.Clear();
        		}
        		// Release unmanaged resources. If disposing is false,
        		// only the following code is executed.
	        	
        		FHost.Log(TLogType.Debug, "MakePath (File Path) is being deleted");
        		
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
        ~MakePath()
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
					FPluginInfo.Name = "MakePath";
					//the nodes category: try to use an existing one
					FPluginInfo.Category = "String";
					//the nodes version: optional. leave blank if not
					//needed to distinguish two nodes of the same name and category
					FPluginInfo.Version = "";
					
					//the nodes author: your sign
					FPluginInfo.Author = "woei";
					//describe the nodes function
					FPluginInfo.Help = "combines strings to a path";
					//specify a comma separated list of tags that describe the node
					FPluginInfo.Tags = "file, directory, folder";
					
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
	    	
	    	//create config
	    	FHost.CreateValueConfig("Input Count", 1, null, TSliceMode.Single, TPinVisibility.True, out FInCount);
	    	FInCount.SetSubType(1, double.MaxValue, 1, 1, false, false, true);

	    	//create inputs
	    	FHost.CreateValueInput("Prepend Patch Path", 1, null, TSliceMode.Single, TPinVisibility.OnlyInspector, out FPrepend);
	    	FPrepend.SetSubType(0, 1, 1, 1, false, true, false);
	    	
	    	FHost.CreateStringInput("Input 1", TSliceMode.Dynamic, TPinVisibility.True, out FIn1);
	    	FIn1.SetSubType("", false);
	    	
	    	//create outputs
	    	FHost.CreateStringOutput("Output", TSliceMode.Dynamic, TPinVisibility.True, out FOutput);
	    	FOutput.SetSubType("", false);
	    	
	    	//
	    	FInList.Add(FIn1);
        }

        #endregion pin creation
        
        #region mainloop
        
        public void Configurate(IPluginConfig Input)
        {
        	if (Input == FInCount)
        	{
        		double inCount;
        		FInCount.GetValue(0, out inCount);
        		int idiff = FInList.Count - (int)Math.Round(inCount);
        		
        		if (idiff > 0) //delete pins
        		{
        			for(int i = 0; i < idiff; i++)
        			{
        				IStringIn delPin = FInList[FInList.Count-1];
        				FInList.Remove(delPin);
        				FHost.DeletePin(delPin);
        				
        			}
        			
        		}
        		else if (idiff < 0) //create pins
        		{
        			for(int i = 0; i > idiff; i--)
        			{
        				IStringIn newPin;
        				FHost.CreateStringInput("Input " + (FInList.Count+1), TSliceMode.Dynamic, TPinVisibility.True, out newPin);
        				newPin.SetSubType("", false);
        				FInList.Add(newPin);
        			}
        		}
        		evaluate=true;
        	}
        }

        int GetSpreadMax(int a, int b)
        {
            if (a <= 0) return 0;
            else if (b <= 0) return 0;
            else return Math.Max(a, b);
        }
        
        //here we go, thats the method called by vvvv each frame
        //all data handling should be in here
        public void Evaluate(int SpreadMax)
        {   
        	try
        	{
        		int maxSlice = FPrepend.SliceCount;
        		foreach (IStringIn p in FInList)
        		{
        			if (p.PinIsChanged)
        				evaluate=true;
        			maxSlice = GetSpreadMax(maxSlice, p.SliceCount);
        		}
        		
        		if (evaluate || FPrepend.PinIsChanged)
        		{
        			evaluate = false;
        			string root;
        			FHost.GetHostPath(out root);
        			root = Path.GetDirectoryName(root);
        			
        			double tmpPrepend;
        			FPrepend.GetValue(0, out tmpPrepend);
        			bool prepend = tmpPrepend>0.5;
        			
        			FOutput.SliceCount=maxSlice;
        			for (int s=0; s<maxSlice; s++)
        			{
        				string builder;
        				FInList[0].GetString(s, out builder);
        				if (string.IsNullOrEmpty(builder))
        						builder="";
        				bool isRooted = Path.IsPathRooted(builder);
        				if (!isRooted)
        				    builder = Path.Combine(root,builder);
        				   
        				string curPath;
        				for (int i=1; i<FInList.Count; i++)
        				{
        					FInList[i].GetString(s, out curPath);
        					if (string.IsNullOrEmpty(curPath))
        						curPath="";
        					builder=Path.Combine(builder, curPath);
        				}

                        bool endsWithSeparator = builder.EndsWith(@"\"); // HACK .net doesn't recognize d: as path, only c:
                        if (!endsWithSeparator)
                            builder += @"\";
        				builder = Path.GetFullPath(builder);  // combines c:\foo\bar + ..\fighters -> c:\foo\figthers
                        if (!endsWithSeparator)
                            builder = builder.Remove(builder.Length - 1);
        				if (!(isRooted || prepend))           // but Path.GetFullPath needs a rooted path.     
        					builder = builder.Replace(root+"\\", string.Empty); //want it relative, subtract root again
        				FOutput.SetString(s, builder);
        			}
        		}
        	}
        	catch (Exception e)
        	{
        		FHost.Log(TLogType.Debug, e.Message);
        		FHost.Log(TLogType.Debug, e.StackTrace);
        		
        	}
        }
             
        #endregion mainloop 
        
        
       
	}
}
