#region licence/info

//////project name
//vvvv TCP Client Advanced

//////description
//Re-implementation and extension of vvvv standard TCP Client node.

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
//iceberg (Joshua Samberg)

#endregion licence/info

//use what you need
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using VVVV.PluginInterfaces.V1;
using System.Diagnostics;
using System.Globalization;

//the vvvv node namespace
namespace VVVV.Nodes
{

    //class definition
    public class FormatedDate : IPlugin, IDisposable
    {



        #region field declaration

        //the host (mandatory)
        private IPluginHost FHost;
        // Track whether Dispose has been called.
        private bool FDisposed = false;

        //input pin declaration
        private IStringIn FFormate;
        private IValueIn FUpdate;
        private IStringIn FCulturInfo;

        //output pins
        private IStringOut FCurrentDate;
        private IStringOut FUTC;


        private double FUpdateSlice;
        #endregion field declaration



        #region constructor/destructor

        public FormatedDate()
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
            if (!FDisposed)
            {
                // Release unmanaged resources. If disposing is false,
                // only the following code is executed.


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
        ~FormatedDate()
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
                    FPluginInfo.Name = "Date";
                    //the nodes category: try to use an existing one
                    FPluginInfo.Category = "Astronomy";
                    //the nodes version: optional. leave blank if not
                    //needed to distinguish two nodes of the same name and category
                    FPluginInfo.Version = "";

                    //the nodes author: your sign
                    FPluginInfo.Author = "phlegma";
                    //describe the nodes function
                    FPluginInfo.Help = "Spit outs the current Date. Can be Formated";
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
            get { return true; }
        }

        #endregion node name and infos



        #region pin creation

        //this method is called by vvvv when the node is created
        public void SetPluginHost(IPluginHost Host)
        {
            //assign host
            FHost = Host;

            //create inputs
            FHost.CreateStringInput("Formate", TSliceMode.Dynamic, TPinVisibility.True, out FFormate);
            FFormate.SetSubType("", false);

            FHost.CreateStringInput("CultureInfo", TSliceMode.Dynamic, TPinVisibility.True,out FCulturInfo);
            FCulturInfo.SetSubType("en-US", false);

            FHost.CreateValueInput("Update", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out FUpdate);
            FUpdate.SetSubType(0.0, 1.0, 1.0, 1.0, false, true, false); 



            //create outputs	    	
            FHost.CreateStringOutput("Current Date", TSliceMode.Dynamic, TPinVisibility.True, out FCurrentDate);
            FCurrentDate.SetSubType("", false);

            FHost.CreateStringOutput("UTC", TSliceMode.Dynamic, TPinVisibility.True, out FUTC);
            FUTC.SetSubType("", false);
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
            FCurrentDate.SliceCount = SpreadMax;
            FUTC.SliceCount = SpreadMax;

            for (int i = 0; i < SpreadMax; i++)
			{
                
                if(FUpdate.PinIsChanged)
                {
                    FUpdate.GetValue(i, out FUpdateSlice);
                }

                DateTime CurrentDate = System.DateTime.Now;
                DateTime CurrentUTC = System.DateTime.UtcNow;

                try
                {
                    if (FUpdateSlice > 0.5 || FFormate.PinIsChanged || FCulturInfo.PinIsChanged)
                    {
                        string currentFormatSlice;
                        string currentCultureInfo;
                        string currentDate = "";
                        string currentUTC = "";

                        FFormate.GetString(i, out currentFormatSlice);
                        FCulturInfo.GetString(i, out currentCultureInfo);

                        if (currentFormatSlice != null && currentCultureInfo != null)
                        {
                            CultureInfo culture = new CultureInfo(currentCultureInfo);
                            currentDate = CurrentDate.ToString(currentFormatSlice, culture);
                            currentUTC = CurrentUTC.ToString(currentFormatSlice, culture);
                        }
                        else if (currentFormatSlice != null)
                        {
                            currentDate = CurrentDate.ToString(currentFormatSlice);
                            currentUTC = CurrentUTC.ToString(currentFormatSlice);
                        }
                        else if (currentCultureInfo != null)
                        {
                            CultureInfo culture = new CultureInfo(currentCultureInfo);
                            currentDate = CurrentDate.ToString(culture);
                            currentUTC = CurrentUTC.ToString(culture);
                        }
                        else
                        {
                            currentDate = CurrentDate.ToString();
                            currentUTC = CurrentUTC.ToString();

                        }

                        FCurrentDate.SetString(i, currentDate); 
                        FUTC.SetString(i, currentUTC); 
                    }
                }
                catch (Exception ex)
                {
                    FHost.Log(TLogType.Error, ex.Message);
                }


                


			}
        }

        #endregion mainloop
    }
}
