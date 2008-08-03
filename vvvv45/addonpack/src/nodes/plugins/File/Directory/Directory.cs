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
using System.Drawing;
using System.Reflection;
using VVVV.PluginInterfaces.V1;

//the vvvv node namespace
namespace VVVV.Nodes
{
	
	//class definition
	public class Directory: IPlugin
    {	          	
    	#region field declaration
    	
    	//the host (mandatory)
    	private IPluginHost FHost; 
    	
    	//input pin declaration
    	private IStringIn FDir;
    	private IStringIn FCustomRoot;
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
        
        ~Directory()
	    {
	    	//the nodes destructor
        	//nothing to destruct
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
	        	Info.Name = "Directory";
	        	Info.Category = "File";
	        	Info.Version = "";
	        	Info.Help = "Checks if a directory exists, can create, delete and rename";
	        	Info.Bugs = "";
	        	Info.Credits = "";
	        	Info.Warnings = "";
	        	
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
	    	FHost.CreateStringInput("Directory", TSliceMode.Dynamic, TPinVisibility.True, out FDir);
	    	FDir.SetSubType("C:", false);
	    	
	    	FHost.CreateStringInput("Custom Root", TSliceMode.Dynamic, TPinVisibility.Hidden, out FCustomRoot);
	    	FCustomRoot.SetSubType("", false);
	    	
	    	FHost.CreateValueInput("Create", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out FCreate);
	    	FCreate.SetSubType(0,1,1,0,true,false, false);
	    	
	    	FHost.CreateValueInput("Remove", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out FDelete);
	    	FDelete.SetSubType(0,1,1,0,true,false, false);
	    	
	    	FHost.CreateStringInput("New Name", TSliceMode.Dynamic, TPinVisibility.True, out FNewDir);
	    	FNewDir.SetSubType("C:", false);
	    	
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
        	    FCustomRoot.PinIsChanged ||
        	    FCreate.PinIsChanged ||
        	    FDelete.PinIsChanged ||
        	    FNewDir.PinIsChanged ||
        	    FRename.PinIsChanged)
        	{	    
        		string currentDir;
        		string curCustomRoot;
        		double currentCreate;
        		double currentDelete;
        		string curNewDir;
        		double currentRename;
        		
        		FExists.SliceCount = SpreadMax;
        		
        		//loop for all slices
        		for (int i=0; i<=SpreadMax; i++)
        		{		
        			FDir.GetString(i, out currentDir);
        			FCustomRoot.GetString(i, out curCustomRoot);
        			FCreate.GetValue(i, out currentCreate);
        			FDelete.GetValue(i, out currentDelete);
        			FNewDir.GetString(i, out curNewDir);
        			FRename.GetValue(i, out currentRename);
        			
        			
        			Assembly self = Assembly.GetExecutingAssembly();
        			if (curCustomRoot == "")
        			{
        				curCustomRoot=self.Location.ToString();
        			}
        			else
        			{
        				if (!Path.IsPathRooted(curCustomRoot))
        				    {
        				    	string message = "\'";
        				    	message+=curCustomRoot;
        				    	message+= "\' is not a valid root-path";
        				    	FHost.Log(TLogType.Warning, message);
        				    	curCustomRoot=self.Location.ToString();
        				    }
        			}
        			
        			if (!Path.IsPathRooted(currentDir))
        			{
        				currentDir = Path.Combine(curCustomRoot, currentDir);
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
        							curNewDir = Path.Combine(curCustomRoot, curNewDir);
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
