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
using VVVV.SkeletonInterfaces;

//the vvvv node namespace
namespace VVVV.Nodes
{
	
	//class definition
	public class LoadSkeleton: IPlugin, IDisposable
    {	          	
    	#region field declaration
    	
    	//the host (mandatory)
    	private IPluginHost FHost; 
    	// Track whether Dispose has been called.
   		private bool FDisposed = false;

    	private IStringIn FMyStringInput;
    	
    	//output pin declaration
    	private INodeOut FSkeletonOutput;
    	private ITransformOut FInverseBindPoseOutput;
    	private IValueOut FBindIndicesOutput;
    	private IValueOut FSkinWeightsOutput;
    	private IValueOut FIndicesOutput;
    	
    	private JointInfo outputJoint;
    	private Skeleton outputSkeleton;

    	private Dictionary<int, Dictionary<int,double>> skinWeights;
    	private Dictionary<int, Matrix4x4> inverseBindPoseMatrices;
    	private StreamReader tr;
    	private string fileName;
    	private int maxVertexIndex = 0;
    	private Matrix4x4 bindShapeMatrix;
    	
    	#endregion field declaration
       
    	#region constructor/destructor
    	
        public LoadSkeleton()
        {

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
        ~LoadSkeleton()
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
					FPluginInfo.Name = "LoadSkeleton";
					//the nodes category: try to use an existing one
					FPluginInfo.Category = "Skeleton";
					//the nodes version: optional. leave blank if not
					//needed to distinguish two nodes of the same name and category
					FPluginInfo.Version = "";
					
					//the nodes author: your sign
					FPluginInfo.Author = "Matthias Zauner";
					//describe the nodes function
					FPluginInfo.Help = "LoadSkeleton";
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
	    	
	    	FHost.CreateStringInput("Filename", TSliceMode.Single, TPinVisibility.True, out FMyStringInput);
	    	FMyStringInput.SetSubType("rig.x", true);

	    	//create outputs
	    	
	    	FHost.CreateValueOutput("Bind Indices", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out FBindIndicesOutput);
	    	FBindIndicesOutput.Order = 1;
	    	
	    	FHost.CreateValueOutput("Skin Weights", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out FSkinWeightsOutput);
	    	FSkinWeightsOutput.Order = 2;
	    	
	    	FHost.CreateValueOutput("Indices", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out FIndicesOutput);
	    	FIndicesOutput.Order = 3;
	    	
	    	FHost.CreateTransformOutput("Inverse Bind Pose", TSliceMode.Dynamic, TPinVisibility.True, out FInverseBindPoseOutput);
	    	FInverseBindPoseOutput.Order = 4;
	    	
	    	FHost.CreateNodeOutput("Skeleton", TSliceMode.Single, TPinVisibility.True, out FSkeletonOutput);
	    	System.Guid[] guids = new System.Guid[1];
	    	guids[0] = new Guid("AB312E34-8025-40F2-8241-1958793F3D39");
	    	FSkeletonOutput.SetSubType(guids, "Skeleton");
	    	FSkeletonOutput.Order = 5;
	    	
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
        	
        	string currLine;
        	string fileContents = "";
        	NumberFormatInfo nf = new CultureInfo("en-US", false).NumberFormat;
        	
        	if (FMyStringInput.PinIsChanged)
        	{	
	        	//first set slicecounts for all outputs
	        	//the incoming int SpreadMax is the maximum slicecount of all input pins, which is a good default
	        	
    			//read data from inputs
    			
    			FMyStringInput.GetString(0, out fileName);
    			
    			tr = new StreamReader(fileName);
    			if (tr!=null)
    			{
	    			while ((currLine = tr.ReadLine())!=null) {
	    				fileContents += currLine;
	    			}
    			}
    			
    			skinWeights = new Dictionary<int, Dictionary<int,double>>();
    			inverseBindPoseMatrices = new Dictionary<int, Matrix4x4>();
    			
    			bindShapeMatrix = this.getBindShapeMatrix(fileContents);
    			this.readJoints(fileContents);
    			
    			int sliceNum = 0;
    			for (int i=0; i<maxVertexIndex+1; i++)
  				{
    				if (!skinWeights.ContainsKey(i))
    					continue;
    				IDictionaryEnumerator boneEnum = skinWeights[i].GetEnumerator();
    				while (boneEnum.MoveNext())
  					{
    					FIndicesOutput.SliceCount = sliceNum+1;
		    			FBindIndicesOutput.SliceCount = sliceNum+1;
		    			FSkinWeightsOutput.SliceCount = sliceNum+1;
    					FIndicesOutput.SetValue(sliceNum, i);
    					FBindIndicesOutput.SetValue(sliceNum, (int)boneEnum.Key);
    					FSkinWeightsOutput.SetValue(sliceNum, (double)boneEnum.Value);
    					sliceNum++;
  					}
  				}
    			
    			FInverseBindPoseOutput.SliceCount = inverseBindPoseMatrices.Count;
    			for (int i=0; i<inverseBindPoseMatrices.Count; i++)
    			{
    				FInverseBindPoseOutput.SetMatrix(i, inverseBindPoseMatrices[i]);
    			}
    			
    			
    			
    			
    			
        	}
    			
		
        	FSkeletonOutput.SetInterface(outputSkeleton);
        }
             
        #endregion mainloop  
        
        #region helper
        
        public Matrix4x4 getBindShapeMatrix(string fileContents)
        {
        	NumberFormatInfo nf = new CultureInfo("en-US", false).NumberFormat;
        	char[] separators = {',', ';'};
        	string[] el;
        	Match m = Regex.Match(fileContents, @"FrameTransformMatrix \{([^\}]+)\}\s*Mesh");
        	if (m.Length==0)
        		return VMath.IdentityMatrix;
        	el = m.Groups[1].ToString().Split(separators);
        	
        	Matrix4x4 t = new Matrix4x4(Double.Parse(el[0], nf), Double.Parse(el[1], nf), Double.Parse(el[2], nf),Double.Parse(el[3], nf),
				                            Double.Parse(el[4], nf), Double.Parse(el[5], nf), Double.Parse(el[6], nf), Double.Parse(el[7], nf),
				                            Double.Parse(el[8], nf), Double.Parse(el[9], nf), Double.Parse(el[10], nf), Double.Parse(el[11], nf),
				                            Double.Parse(el[12], nf), Double.Parse(el[13], nf), Double.Parse(el[14], nf), Double.Parse(el[15], nf));
        	return VMath.Inverse(t);
        }
        
        public void readJoints(string fileContents)
		{
        	getChildJoints(fileContents);
			
        }
        
        public void getChildJoints(string code)
        {
        	NumberFormatInfo nf = new CultureInfo("en-US", false).NumberFormat;
        	Stack s = new Stack();
        	Regex r = new Regex(@"\s*Frame (joint[0-9]+)+");
        	
        	char[] separators = new char[2];
        	separators[0] = ',';
        	separators[1] = ';';
        	string[] el;
  
        	int currJointIndex = 0;
        
			MatchCollection m = r.Matches(code);
			
			Match m2;
			Match m3;
			for (int i=0; i<m.Count; i++)
			{
				m2 = Regex.Match(m[i].Groups[0].ToString(), @"^\s*");
				int depth = m2.ToString().Length / 4;
				m3 = Regex.Match(code, m[i].Groups[1].ToString()+@" \{\s*FrameTransformMatrix \{([^\}]+)\}");
				el = m3.Groups[1].ToString().Split(separators);
				JointInfo jointInfo = new JointInfo(m[i].Groups[1].ToString());
				jointInfo.index = currJointIndex;
				currJointIndex++;
				Matrix4x4 t = new Matrix4x4(Double.Parse(el[0], nf), Double.Parse(el[1], nf), Double.Parse(el[2], nf),Double.Parse(el[3], nf),
				                            Double.Parse(el[4], nf), Double.Parse(el[5], nf), Double.Parse(el[6], nf), Double.Parse(el[7], nf),
				                            Double.Parse(el[8], nf), Double.Parse(el[9], nf), Double.Parse(el[10], nf), Double.Parse(el[11], nf),
				                            Double.Parse(el[12], nf), Double.Parse(el[13], nf), Double.Parse(el[14], nf), Double.Parse(el[15], nf));
				jointInfo.setTransform(t);
				int maxJointVertexIndex = readSkinWeights(jointInfo, code);
				if (maxJointVertexIndex>maxVertexIndex)
					maxVertexIndex = maxJointVertexIndex;
				
				while (depth<s.Count)
				{
					s.Pop();
				}
				
				if (depth>0)
				{
					jointInfo.Parent = (JointInfo)s.Peek();
					//((JointInfo)s.Peek()).AddChild(jointInfo);
				}
				s.Push(jointInfo);
				
			}
			
			while (s.Count>1)
			{
				s.Pop();
			}
			outputJoint = (JointInfo)s.Peek();
			outputSkeleton = new Skeleton(outputJoint);
			
			outputSkeleton.BuildJointTable();
			
			
        }
        
        public int readSkinWeights(JointInfo currJoint, string fileContents)
        {
        	
        	NumberFormatInfo nf = new CultureInfo("en-US", false).NumberFormat;
        	char[] separators = new char[1];
    		separators[0] = ',';
    		string[] vertexNums;
    		string[] weights;
    		string[] transformCoords;
    		int influenceCount = 0;
    		int maxJointVertexIndex = 0;
        	
    		Match m = Regex.Match(fileContents, @"SkinWeights \{\s*."+currJoint.shortname+@".;([0-9\s]+);([^;]+);([^;]+);([^;]+);");
    		bool res = int.TryParse(m.Groups[1].ToString().Trim(), out influenceCount);
    		
    		if (m.ToString()=="")
    			return 0;
    		vertexNums = m.Groups[2].ToString().Split(separators);
    		weights = m.Groups[3].ToString().Split(separators);
    		int vertexNum;
    		double weight;
    		for (int i=0; i<vertexNums.Length; i++)
    		{
    			if (vertexNums[i].Trim()!="")
    			{
    				vertexNum = int.Parse(vertexNums[i].Trim());
    				weight = Double.Parse(weights[i].Trim(), nf);
    				
    				if (vertexNum>maxJointVertexIndex)
    					maxJointVertexIndex = vertexNum;
    				
    				if (skinWeights.ContainsKey(vertexNum))
    				    skinWeights[vertexNum].Add(currJoint.index, weight);
    				else
    				{
    					Dictionary<int, double> boneWeights = new Dictionary<int, double>();
    					boneWeights.Add(currJoint.index, weight);
    					skinWeights.Add(vertexNum, boneWeights);
    				}
    			}
    		}
    		transformCoords = m.Groups[4].ToString().Split(separators);
    		
    		Matrix4x4 inverseBindPoseMatrix = new Matrix4x4();
    		
    		inverseBindPoseMatrix.m11 = Double.Parse(transformCoords[0].Trim(), nf);
    		inverseBindPoseMatrix.m12 = Double.Parse(transformCoords[1].Trim(), nf);
    		inverseBindPoseMatrix.m13 = Double.Parse(transformCoords[2].Trim(), nf);
    		inverseBindPoseMatrix.m14 = Double.Parse(transformCoords[3].Trim(), nf);
    		
    		inverseBindPoseMatrix.m21 = Double.Parse(transformCoords[4].Trim(), nf);
    		inverseBindPoseMatrix.m22 = Double.Parse(transformCoords[5].Trim(), nf);
    		inverseBindPoseMatrix.m23 = Double.Parse(transformCoords[6].Trim(), nf);
    		inverseBindPoseMatrix.m24 = Double.Parse(transformCoords[7].Trim(), nf);
    		
    		inverseBindPoseMatrix.m31 = Double.Parse(transformCoords[8].Trim(), nf);
    		inverseBindPoseMatrix.m32 = Double.Parse(transformCoords[9].Trim(), nf);
    		inverseBindPoseMatrix.m33 = Double.Parse(transformCoords[10].Trim(), nf);
    		inverseBindPoseMatrix.m34 = Double.Parse(transformCoords[11].Trim(), nf);
    		
    		inverseBindPoseMatrix.m41 = Double.Parse(transformCoords[12].Trim(), nf);
    		inverseBindPoseMatrix.m42 = Double.Parse(transformCoords[13].Trim(), nf);
    		inverseBindPoseMatrix.m43 = Double.Parse(transformCoords[14].Trim(), nf);
    		inverseBindPoseMatrix.m44 = Double.Parse(transformCoords[15].Trim(), nf);
    		
    		inverseBindPoseMatrices.Add(currJoint.index, bindShapeMatrix *inverseBindPoseMatrix);
        	
        	return maxJointVertexIndex;
        	
        }
        	
        
        #endregion helper
        
        
        
	}
	
}
