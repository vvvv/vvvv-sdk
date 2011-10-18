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
using System.Collections.Generic;

using VVVV.PluginInterfaces.V1;
using VVVV.Utils.VColor;
using VVVV.Utils.VMath;

using VVVV.Webinterface.Utilities;
using VVVV.Nodes.Http.BaseNodes;





//the vvvv node namespace
namespace VVVV.Nodes.HttpGUI.CSS
{

    
    /// <summary>
    /// css node class definition
    /// generates css code for the padding values
    /// </summary>
    public class Padding: BaseCssNode, IPlugin, IDisposable
    {





        #region field declaration

        //the host (mandatory)
        // Track whether Dispose has been called.
        private bool FDisposed = false;

        //input pin declaration
        private IValueIn FTop;
        private IValueIn FLeft;
        private IValueIn FRight;
        private IValueIn FBottom;
        private IEnumIn FUnit;

        #endregion field declaration






        #region constructor/destructor

        /// <summary>
        /// the nodes constructor
        /// nothing to declare for this node
        /// </summary>
        public Padding()
        {

        }

        /// <summary>
        /// Implementing IDisposable's Dispose method.
        /// Do not make this method virtual.
        /// A derived class should not be able to override this method.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            // Take yourself off the Finalization queue
            // to prevent finalization code for this object
            // from executing a second time.
            GC.SuppressFinalize(this);
        }

        

        /// <summary>
        /// Dispose(bool disposing) executes in two distinct scenarios.
        /// If disposing equals true, the method has been called directly
        /// or indirectly by a user's code. Managed and unmanaged resources
        /// can be disposed.
        /// If disposing equals false, the method has been called by the
        /// other objects. Only unmanaged resources can be disposed.
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing)
        {
            // Check to see if Dispose has already been called.
            if (FDisposed == false)
            {
                if (disposing)
                {
                    // Dispose managed resources.
                }
                // Release unmanaged resources. If disposing is false,
                // only the following code is executed.

                FHost.Log(TLogType.Message, "Padding (HTTP CSS) Node is being deleted");

                // Note that this is not thread safe.
                // Another thread could start disposing the object
                // after the managed resources are disposed,
                // but before the disposed flag is set to true.
                // If thread safety is necessary, it must be
                // implemented by the client.
            }

            FDisposed = true;
        }

        
        /// <summary>
        /// Use C# destructor syntax for finalization code.
        /// This destructor will run only if the Dispose method
        /// does not get called.
        /// It gives your base class the opportunity to finalize.
        /// Do not provide destructors in WebTypes derived from this class.
        /// </summary>
        ~Padding()
        {
            // Do not re-create Dispose clean-up code here.
            // Calling Dispose(false) is optimal in terms of
            // readability and maintainability.
            Dispose(false);
        }

        #endregion constructor/destructor






        #region node name and infos


        private static IPluginInfo FPluginInfo;

        /// <summary>
        /// provide node infos 
        /// </summary>
        public static IPluginInfo PluginInfo
        {
            get
            {
                if (FPluginInfo == null)
                {
                    // fill out nodes info
                    // see: http://www.vvvv.org/tiki-index.php?page=vvvv+naming+conventions
                    FPluginInfo = new PluginInfo();

                    // the nodes main name: use CamelCaps and no spaces
                    FPluginInfo.Name = "Padding";
                    // the nodes category: try to use an existing one
                    FPluginInfo.Category = "HTTP";
                    // the nodes version: optional. leave blank if not
                    // needed to distinguish two nodes of the same name and category
                    FPluginInfo.Version = "CSS";

                    // the nodes author: your sign
                    FPluginInfo.Author = "phlegma";
                    // describe the nodes function
                    FPluginInfo.Help = "node for html page creation";
                    // specify a comma separated list of tags that describe the node
                    FPluginInfo.Tags = "";

                    // give credits to thirdparty code used
                    FPluginInfo.Credits = "";
                    // any known problems?
                    FPluginInfo.Bugs = "";
                    // any known usage of the node that may cause troubles?
                    FPluginInfo.Warnings = "";

                    // leave below as is
                    System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace(true);
                    System.Diagnostics.StackFrame sf = st.GetFrame(0);
                    System.Reflection.MethodBase method = sf.GetMethod();
                    FPluginInfo.Namespace = method.DeclaringType.Namespace;
                    FPluginInfo.Class = method.DeclaringType.Name;
                    // leave above as is
                }

                return FPluginInfo;
            }
        }




        #endregion node name and infos






        #region pin creation

        /// <summary>
        /// this method is called by vvvv when the node is created
        /// </summary>
        /// <param name="Host"></param>
        protected override void OnPluginHostSet()
        {
            // create inputs
            FHost.CreateValueInput("Top", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out FLeft);
            FLeft.SetSubType(-1,1, 0.01, 1, false, false, false);

			FHost.CreateValueInput("Left", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out FTop);
			FTop.SetSubType(-1,1, 0.01, 1, false, false, false);

            FHost.CreateValueInput("Right", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out FRight);
            FTop.SetSubType(-1, 1, 0.01, 1, false, false, false);

            FHost.CreateValueInput("Bottom", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out FBottom);
            FTop.SetSubType(-1, 1, 0.01, 1, false, false, false);

            FHost.UpdateEnum("Unit", "Percent", new string[] { "Percent", "Pixel" });
            FHost.CreateEnumInput("Unit", TSliceMode.Dynamic, TPinVisibility.OnlyInspector, out FUnit);
            FUnit.SetSubType("Unit");
           
        }

        #endregion pin creation






        #region mainloop

        /// <summary>
        /// nothing to configure in this plugin
        /// only used in conjunction with inputs of WebType cmpdConfigurate
        /// </summary>
        /// <param name="Input"></param>
        protected override void OnConfigurate(IPluginConfig Input)
        {
            
        }

        
        /// <summary>
        /// here we go, thats the method called by vvvv each frame
        /// all data handling should be in here
        /// </summary>
        /// <param name="SpreadMax">max number of slices</param>
        protected override void OnEvaluate(int SpreadMax)
        {

			if (FLeft.PinIsChanged || FTop.PinIsChanged || FRight.PinIsChanged || FBottom.PinIsChanged)
            {

                IPluginIn[] tInputs = { FLeft, FTop};
                int tSliceCount = GetSliceCount(tInputs);
                
                mCssPropertiesOwn.Clear();

                for (int i = 0; i < tSliceCount; i++)
                {

                    double currentLeftSlice;
                    double currentTopSlice;
                    double currentRightSlice;
                    double currentBottomSlice;
                    string currentUnitSlice;

                    SortedList<string, string> tCssProperty = new SortedList<string, string>();
                    // get current values
                    FLeft.GetValue(i, out currentTopSlice);
                    FTop.GetValue(i, out currentLeftSlice);
                    FRight.GetValue(i, out currentRightSlice);
                    FBottom.GetValue(i, out currentBottomSlice);
                    FUnit.GetString(i, out currentUnitSlice);

					// add css webattributes
                    if (currentUnitSlice == "Percent")
                    {
                        tCssProperty.Add("padding-top", (((double)Math.Round(HTMLToolkit.MapScale(currentTopSlice, 0, 2, 0, 100), 1)).ToString() + "%").Replace(",", "."));
                        tCssProperty.Add("padding-left", (((double)Math.Round(HTMLToolkit.MapScale(currentLeftSlice, 0, 2, 0, 100), 1)).ToString() + "%").Replace(",", "."));
                        tCssProperty.Add("padding-right", (((double)Math.Round(HTMLToolkit.MapScale(currentRightSlice, 0, 2, 0, 100), 1)).ToString() + "%").Replace(",", "."));
                        tCssProperty.Add("padding-bottom", (((double)Math.Round(HTMLToolkit.MapScale(currentBottomSlice, 0, 2, 0, 100), 1)).ToString() + "%").Replace(",", "."));

                        mCssPropertiesOwn.Add(i, tCssProperty);
                    }
                    else
                    {
                        tCssProperty.Add("padding-top", Convert.ToString((int)currentTopSlice) + "px");
                        tCssProperty.Add("padding-left", Convert.ToString((int)currentLeftSlice) + "px");
                        tCssProperty.Add("padding-right", Convert.ToString((int)currentRightSlice) + "px");
                        tCssProperty.Add("padding-bottom", Convert.ToString((int)currentBottomSlice) + "px");
                        mCssPropertiesOwn.Add(i, tCssProperty);
                    }
                }
            }
        }	

        #endregion mainloop

        protected override bool DynamicPinIsChanged()
        {
            return (FLeft.PinIsChanged || FTop.PinIsChanged || FRight.PinIsChanged || FBottom.PinIsChanged || FUnit.PinIsChanged);
        }
    }
}
