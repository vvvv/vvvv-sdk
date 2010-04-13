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
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using VVVV.PluginInterfaces.V1;
using VVVV.Utils.VColor;
using VVVV.Utils.VMath;
using System.Text.RegularExpressions;
using System.Globalization;
using VVVV.Utils.SharedMemory;

//the vvvv node namespace
namespace VVVV.Nodes
{
	
	//class definition
	public class Skindeformer: IPlugin, IDisposable
    {	          	
    	#region field declaration
    	
    	//the host (mandatory)
    	private IPluginHost FHost; 
    	// Track whether Dispose has been called.
   		private bool FDisposed = false;

    	private IValueIn FVerticesInput;
    	private IValueIn FBindIndicesInput;
    	private IValueIn FSkinWeightsInput;
    	private IValueIn FIndicesInput;
    	private ITransformIn FJointTransformsInput;
    	
    	//output pin declaration

    	private IValueOut FVerticesOutput;	
    	
    	private ArrayList vertices;
    	private ArrayList vertTransformed;
    	private Dictionary<int, Dictionary<int,double>> skinWeights;
    	
    	#endregion field declaration
       
    	#region constructor/destructor
    	
        public Skindeformer()
        {
			vertices = new ArrayList();
			
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
        ~Skindeformer()
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
					FPluginInfo.Name = "Skindeformer";
					//the nodes category: try to use an existing one
					FPluginInfo.Category = "Skeleton";
					//the nodes version: optional. leave blank if not
					//needed to distinguish two nodes of the same name and category
					FPluginInfo.Version = "";
					
					//the nodes author: your sign
					FPluginInfo.Author = "Matthias Zauner";
					//describe the nodes function
					FPluginInfo.Help = "Skin Deformer";
					//specify a comma separated list of tags that describe the node
					FPluginInfo.Tags = "";
					
					//give credits to thirdparty code used
					FPluginInfo.Credits = "";
					//any known problems?
					FPluginInfo.Bugs = "certainly";
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
	    	
	    	System.Guid[] guids = new System.Guid[1];
	    	guids[0] = new Guid("AB312E34-8025-40F2-8241-1958793F3D39");

	    	//create inputs
	    	FHost.CreateValueInput("Vertices", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out FVerticesInput);
	    	
	    	FHost.CreateValueInput("BindIndices", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out FBindIndicesInput);
	    	
	    	FHost.CreateValueInput("SkinWeights", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out FSkinWeightsInput);
	    	
	    	FHost.CreateValueInput("Indices", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out FIndicesInput);
	    	
	    	FHost.CreateTransformInput("Joint Transformations", TSliceMode.Dynamic, TPinVisibility.True, out FJointTransformsInput);
			
	    	//outputs
	    	FHost.CreateValueOutput("Vertices XYZ", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out FVerticesOutput);
	    	
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
        	
        	bool recalculate = false;
        	
        	if (FSkinWeightsInput.PinIsChanged || FBindIndicesInput.PinIsChanged || FIndicesInput.PinIsChanged)
        	{
        		skinWeights = new Dictionary<int, Dictionary<int,double>>();
        		for (int i=0; i<FIndicesInput.SliceCount; i++)
        		{
        			double vertexIndex, jointIndex, skinWeight;
        			FIndicesInput.GetValue(i, out vertexIndex);
        			FBindIndicesInput.GetValue(i, out jointIndex);
        			FSkinWeightsInput.GetValue(i, out skinWeight);
        			if (skinWeights.ContainsKey((int)vertexIndex))
        			{
        				skinWeights[(int)vertexIndex].Add((int)jointIndex, skinWeight);
        			}
        			else
        			{
        				Dictionary<int,double> jointTable = new Dictionary<int,double>();
        				jointTable.Add((int)jointIndex, skinWeight);
        				skinWeights.Add((int)vertexIndex, jointTable);
        			}
        		}
        		
        		recalculate = true;
        	}
        	
        	if (FVerticesInput.PinIsChanged)
        	{
        		vertices = new ArrayList();
        		vertTransformed = new ArrayList();
        		double x, y, z;
	        	for (int i=0; i<FVerticesInput.SliceCount-2; i+=3)
	        	{
	        		FVerticesInput.GetValue(i, out x);
	        		FVerticesInput.GetValue(i+1, out y);
	        		FVerticesInput.GetValue(i+2, out z);
	        		vertices.Add(new Vector3D(x,y,z));
	        		vertTransformed.Add(new Vector3D(0));
	        	}
	        	
	        	recalculate = true;
        	}
        	
        	if (FJointTransformsInput.PinIsChanged)
        	{
        		recalculate = true;
        	}
        	
        	
        	
        	if (recalculate)
        	{
        		for (int i=0; i<vertTransformed.Count; i++)
	        	{
	        		vertTransformed[i] = new Vector3D(0);
	        		calculateTransformedVertex(i);
	        	}
        	}
        	
        	
        	
        	FVerticesOutput.SliceCount = vertices.Count*3;
			Vector3D currVertex;
        	for (int i=0; i<vertTransformed.Count; i++)
			{
        		currVertex = (Vector3D)vertTransformed[i];
        		if (currVertex.x==0 && currVertex.y==0 && currVertex.z==0)
        			currVertex = (Vector3D)vertices[i];
        		
        		
				FVerticesOutput.SetValue(i*3, currVertex.x);
				FVerticesOutput.SetValue(i*3+1, currVertex.y);
				FVerticesOutput.SetValue(i*3+2, currVertex.z);
        	}
        	
        	
        	
        	

        }
             
        #endregion mainloop  
        
        #region helper
        
        
        public void calculateTransformedVertex(int vertexIndex)
        {
        	if (!skinWeights.ContainsKey(vertexIndex))
        	    return;
        	IDictionaryEnumerator influenceEnum = skinWeights[vertexIndex].GetEnumerator();
        	
        	Matrix4x4 jointTransform;
        	while (influenceEnum.MoveNext())
        	{
        		FJointTransformsInput.GetMatrix((int)influenceEnum.Key, out jointTransform);
        		vertTransformed[vertexIndex] = (Vector3D)vertTransformed[vertexIndex] + (double)influenceEnum.Value * (jointTransform * (Vector3D)vertices[vertexIndex]);
        	}
        	
        }
        
       
        	
        
        #endregion helper
        
        
        
	}
	
}
