#region licence/info

//////project name
//ReaderFileAdv

//////description
//Reader (File) with advanced features

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
using System.Text;
using VVVV.PluginInterfaces.V1;

//the vvvv node namespace
namespace VVVV.Nodes
{
    
    //class definition
    public class ReaderFileAdv: IPlugin, IDisposable
    {
        #region field declaration
        
        //the host (mandatory)
        private IPluginHost FHost;
        // Track whether Dispose has been called.
        private bool FDisposed = false;

        //input pin declaration
        private IValueConfig FAnsiUtf;
        private IValueConfig FLineWise;
        private IStringIn FFilename;
        private IValueIn FIndex;
        private IValueIn FCount;
        private IValueIn FUpdate;
        
        //output pin declaration
        private IStringOut FContent;
        
        //vars
        bool FLineWiseChanged = false;
        int FLineWiseTog = 0;
        bool FAnsiUtfChanged = false;
        int FAnsiUtfTog = 0;
        
        #endregion field declaration
        
        #region constructor/destructor
        
        public ReaderFileAdv()
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
                
                FHost.Log(TLogType.Debug, "Reader (File Advanced) is being deleted");
                
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
        ~ReaderFileAdv()
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
                    FPluginInfo.Name = "Reader";
                    //the nodes category: try to use an existing one
                    FPluginInfo.Category = "File";
                    //the nodes version: optional. leave blank if not
                    //needed to distinguish two nodes of the same name and category
                    FPluginInfo.Version = "Advanced";
                    
                    //the nodes author: your sign
                    FPluginInfo.Author = "woei";
                    //describe the nodes function
                    FPluginInfo.Help = "Returns specified parts of a file";
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
            FHost.CreateValueConfig("Ansi/UTF8", 1, null, TSliceMode.Single, TPinVisibility.OnlyInspector, out FAnsiUtf);
            FAnsiUtf.SetSubType(0, 1, 1, 0, false, true, false);
            
            FHost.CreateValueConfig("Toggle line-wise", 1, null, TSliceMode.Single, TPinVisibility.OnlyInspector, out FLineWise);
            FLineWise.SetSubType(0, 1, 1, 0, false, true, false);
            
            FHost.CreateStringInput("Filename", TSliceMode.Dynamic, TPinVisibility.True, out FFilename);
	    	FFilename.SetSubType("", true);	
            
            FHost.CreateValueInput("Startindex", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out FIndex);
            FIndex.SetSubType(0, double.MaxValue, 1, 0, false, false, true);
            
            FHost.CreateValueInput("Count", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out FCount);
            FCount.SetSubType(0, double.MaxValue, 1, 1, false, false, true);
            
            FHost.CreateValueInput("Update", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out FUpdate);
            FUpdate.SetSubType(0, 1, 1, 0, true, false, false);
            
            //create outputs
            FHost.CreateStringOutput("Content", TSliceMode.Dynamic, TPinVisibility.True, out FContent);
            FContent.SetSubType("", false);
        }

        #endregion pin creation
        
        #region mainloop
        
        public void Configurate(IPluginConfig Input)
        {
            if (Input == FLineWise)
            {
                FLineWiseChanged=true;
                double tmpLineWise;
                FLineWise.GetValue(0, out tmpLineWise);
                FLineWiseTog = (int)Math.Round(tmpLineWise);
            }
            
            if (Input == FAnsiUtf)
            {
                FAnsiUtfChanged = true;
                double tmpAnsiUtf;
                FAnsiUtf.GetValue(0, out tmpAnsiUtf);
                FAnsiUtfTog = (int)Math.Round(tmpAnsiUtf);
            }
        }
        
        //here we go, thats the method called by vvvv each frame
        //all data handling should be in here
        public void Evaluate(int SpreadMax)
        {
            bool doUpdate = false;
            if (FUpdate.PinIsChanged)
            {
                double curUpdate;
                FUpdate.GetValue(0, out curUpdate);
                if (curUpdate==1)
                {
                    doUpdate=true;
                }
            }
            
            //if any of the inputs has changed
            //recompute the outputs
            if (FFilename.PinIsChanged ||
                FIndex.PinIsChanged ||
                FCount.PinIsChanged ||
                doUpdate ||
                FLineWiseChanged ||
                FAnsiUtfChanged)
            {
                doUpdate = false;
                FLineWiseChanged = false;
                FAnsiUtfChanged = false;
                
                //outSliceCount
                FContent.SliceCount = SpreadMax;
                
                //the variables to fill with the input data
                string curFilename;
                int curIndex, curCount;
                
                Encoding enc = Encoding.GetEncoding(1252);
                if (FAnsiUtfTog != 0)
                {
                    enc = Encoding.UTF8;
                }
                
                //loop for all slices
                for (int i=0; i<SpreadMax; i++)
                {
                    //read data from inputs
                    FFilename.GetString(i, out curFilename);
                    
                    double tmpIndex, tmpCount;
                    FIndex.GetValue(i, out tmpIndex);
                    FCount.GetValue(i, out tmpCount);
                    curIndex = (int)Math.Floor(tmpIndex);
                    curCount= (int)Math.Floor(tmpCount);
                    
                    string outString = string.Empty;
                    
                    if (File.Exists(curFilename))
                    {
                        if (FLineWiseTog == 1)
                        {
                            string[] allLines = File.ReadAllLines(curFilename, enc);
                            int maxlen = Math.Min(allLines.Length,curIndex+curCount);
                            for (int j=curIndex; j<maxlen; j++)
                            {
                                outString+=allLines[j];
                                if (j != maxlen-1)
                                    outString+=Environment.NewLine;
                            }
                        }
                        else
                        {
                            TextReader curFile = new StreamReader(curFilename, enc);
                            for (int j=0; j<curIndex+curCount; j++)
                            {
                            	int v = curFile.Read();
                            	if (v == -1)
                            		break;
                            	else if (j>=curIndex)
                            		outString+=(char)v;
                            	
                            }
                            curFile.Close();
                        }
                    }
                    
                    //write data to outputs
                    FContent.SetString(i, outString);
                }
            }
        }
        
        #endregion mainloop
    }
}
