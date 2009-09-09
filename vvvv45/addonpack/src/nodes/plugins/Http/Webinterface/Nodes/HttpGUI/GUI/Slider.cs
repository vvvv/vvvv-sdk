using System;
using System.Collections.Generic;
using System.Text;
using VVVV.PluginInterfaces.V1;
using VVVV.Webinterface.Utilities;

namespace VVVV.Nodes.HttpGUI
{
    class Slider : GuiNodeDynamic, IPlugin, IDisposable
    {

        #region field declaration

        private bool FDisposed = false;
        private IStringIn FName;
        private IEnumIn FOrientation;
        private IValueOut FResponse;


        #endregion field declaration


        #region constructor/destructor

        /// <summary>
        /// the nodes constructor
        /// nothing to declare for this node
        /// </summary>
        public Slider()
        {
        }

        /// <summary>
        /// Implementing IDisposable's Dispose method.
        /// Do not make this method virtual.
        /// A derived class should not be able to override this method.
        /// </summary>
        /// 

        #region Dispose

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
                //mWebinterfaceSingelton.DeleteNode(mObserver);
                FHost.Log(TLogType.Message, FPluginInfo.Name.ToString() + "Slider (Http Gui) Node is being deleted");

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
        ~Slider()
        {
            // Do not re-create Dispose clean-up code here.
            // Calling Dispose(false) is optimal in terms of
            // readability and maintainability.
            Dispose(false);
        }

        #endregion dispose

        #endregion constructor/destructor


        #region Pugin Information

        public static IPluginInfo FPluginInfo;

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
                    FPluginInfo.Name = "Slider";
                    //the nodes category: try to use an existing one
                    FPluginInfo.Category = "HTTP";
                    //the nodes version: optional. leave blank if not
                    //needed to distinguish two nodes of the same name and category
                    FPluginInfo.Version = "GUI";

                    //the nodes author: your sign
                    FPluginInfo.Author = "phlegma";
                    //describe the nodes function
                    FPluginInfo.Help = "Slider node for the Renderer (HTTP)";
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


        #endregion Plugin Information


        #region pin creation

        protected override void OnSetPluginHost()
        {
            // create required pins
            FHost.CreateValueOutput("Response", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out FResponse);
            FResponse.SetSubType(0, 1, 1, 0, false, false, true);

            FHost.CreateStringInput("Name", TSliceMode.Dynamic, TPinVisibility.True, out FName);
            FName.SetSubType("", false);

            FHost.UpdateEnum("Orientation", "X", new string[] { "X", "Y"});
            FHost.CreateEnumInput("Orientation", TSliceMode.Single, TPinVisibility.True, out FOrientation);
            FOrientation.SetSubType("Orientation");
            


        }

        #endregion pin creation


        #region Main Loop



        protected override void OnEvaluate(int SpreadMax)
        {
            //if (mChangedSpreadSize)
            //{
            for (int i = 0; i < SpreadMax; i++)
            {

                string currentOrientation = String.Empty;
                string currentName = String.Empty;

                FName.GetString(i, out currentName);
                FOrientation.GetString(i, out currentOrientation);

                string SliderId = "SliderID" + i.ToString();

                HtmlDiv tMainContainer = new HtmlDiv();
                HtmlDiv tSlider = new HtmlDiv(SliderId);
                HTMLText tText = new HTMLText(currentName, false);

                //string AttributeSliderContent = "position:absolute; right:0; width:80%";
                //HTMLAttribute tSliderAttribute = new HTMLAttribute("style", AttributeSliderContent);
                //tSlider.AddAttribute(tSliderAttribute);



                tMainContainer.Insert(tText);
                tMainContainer.Insert(tSlider);
                

                SetTag(i, tMainContainer);


                FResponse.SliceCount = SpreadMax;


                string tResponse;
                GetNewDataFromServer(mGuiDataList[i].SliceId, out tResponse);


                if (tResponse != "")
                {
                    FResponse.SetValue(i, Convert.ToInt32(tResponse));
                }
                else
                {
                    FResponse.SetValue(i, 0);
                }

//                string tContent = @"var id = $(this).attr('id');
//                $(this).fadeOut(20,function()
//                {
//                    $(this).fadeIn(20);
//                });
//
//                $.post('ToVVVV.xml', id + '= 1', null);
//                ";

                SetJavaScript(i, new JqueryFunction(true,"#" + SliderId, "slider()").Text);
            }




            

            //}
        }


        #endregion Main Loop
    }
}
