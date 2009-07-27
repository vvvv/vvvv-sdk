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
using System.Diagnostics;

using VVVV.PluginInterfaces.V1;
using VVVV.Utils.VColor;
using VVVV.Utils.VMath;

using VVVV.Webinterface.Utilities;




//the vvvv node namespace
namespace VVVV.Nodes.HttpGUI.CSS
{




    
    /// <summary>
    /// CSS Node to generates css code
    /// </summary>
    public class Border: BaseCssNode,IDisposable,IPlugin
    {






        #region field declaration


        // Track whether Dispose has been called.
        private bool FDisposed = false;

        //input pin declaration
		private IValueIn FBorderWidthIn;
		private IColorIn FColorInput;
        private IEnumIn FBorderStyleIn;



        #endregion field declaration






        #region constructor/destructor

        /// <summary>
        /// the nodes constructor
        /// nothing to declare for this node
        /// </summary>
        public Border()
        {
           //FPluginInfo.Name = "Border";
        }


            #region Dispose
            /// <summary>
            /// Implementing IDisposable's Dispose method.
            ///  Do not make this method virtual.
            ///  A derived class should not be able to override this method.
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
            /// runtime from inside the finalizer and you should not reference
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

                    FHost.Log(TLogType.Message, "Border(Http CSS) Node is being deleted");

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
            ~Border()
            {
                // Do not re-create Dispose clean-up code here.
                // Calling Dispose(false) is optimal in terms of
                // readability and maintainability.
                Dispose(false);
            }
            #endregion Dispose


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


                    FPluginInfo.Name = "Border";
                    // the nodes main name: use CamelCaps and no spaces

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
                    FPluginInfo.Tags = "Css";

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
        /// <param name="Host">instance to vvvv</param>
        protected override void OnPluginHostSet()
        {
            // assign host

            // create inputs
             
                FHost.CreateValueInput("Border Width", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out FBorderWidthIn);
                FBorderWidthIn.SetSubType(double.MinValue, double.MaxValue, 0.01, 0.01, false, false, false);

                FHost.CreateColorInput("Color", TSliceMode.Dynamic, TPinVisibility.True, out FColorInput);
                FColorInput.SetSubType(VColor.Black, false);

                FHost.UpdateEnum("BorderOutlineStyle", "solid", new string[] { "none", "hidden", "dotted", "dashed", "solid", "double", "groove", "ridge", "inset", "outset" });
                FHost.CreateEnumInput("Style", TSliceMode.Single, TPinVisibility.True, out FBorderStyleIn);
                FBorderStyleIn.SetSubType("BorderOutlineStyle");

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
        /// <param name="SpreadMax">number of slices</param>
        protected override void OnEvaluate(int SpreadMax)
        {

            //////Debug.WriteLine("Enter OnEvaluate Border");
            try
            {
                if (FBorderWidthIn.PinIsChanged || FColorInput.PinIsChanged || FBorderStyleIn.PinIsChanged)
                {

                    IPluginIn[] tInputs = { FBorderWidthIn, FColorInput, FBorderStyleIn };
                    int tSliceCount = GetSliceCount(tInputs);

                    mCssPropertiesOwn.Clear();

                    for (int i = 0; i < tSliceCount; i++)
                    {

                        double currentBorderWidthSlice;
                        RGBAColor currentColorSlice;
                        string currentBorderStyle;
                        SortedList<string, string> tCssProperty = new SortedList<string, string>();

                        // get current values
                        FBorderWidthIn.GetValue(i, out currentBorderWidthSlice);
                        FColorInput.GetColor(i, out currentColorSlice);
                        FBorderStyleIn.GetString(i, out currentBorderStyle);

                        tCssProperty.Add("border-width", ((Math.Round(currentBorderWidthSlice * 100, 1).ToString() + "px").Replace(",", ".")));
                        tCssProperty.Add("border-color", "rgb(" + Math.Round(currentColorSlice.R * 100) + "%," + Math.Round(currentColorSlice.G * 100) + "%," + Math.Round(currentColorSlice.B * 100) + "%)");
                        tCssProperty.Add("border-style", currentBorderStyle);

                        mCssPropertiesOwn.Add(i, tCssProperty);
                    }
                }
            }
            catch (Exception ex)
            {
                FHost.Log(TLogType.Error, ex.Message);
            }

        }
				
       #endregion mainloop
    }
}
