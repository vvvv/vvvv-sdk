using System;
using System.Collections.Generic;
using System.Text;
using VVVV.PluginInterfaces.V1;
using VVVV.Webinterface.Utilities;
using VVVV.Webinterface.jQuery;
using VVVV.Nodes.Http.BaseNodes;

namespace VVVV.Nodes.Http.Jquery
{
    class Attr : JQueryNode, IPlugin, IDisposable
    {
    	
    	#region field declaration 

        private bool FDisposed = false;
		private IStringIn FAttributeStringInput;
		private IStringIn FValueStringInput;

		private MethodCall FAttrMethodCall;

        #endregion field declaration


        #region constructor/destructor

        /// <summary>
        /// the nodes constructor
        /// nothing to declare for this node
        /// </summary>
        public Attr()
        {
			FAttrMethodCall = new MethodCall("attr", "Attribute", "Value");
			FExpression.AddChainedOperation(FAttrMethodCall);
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
                    FHost.Log(TLogType.Message, FPluginInfo.Name.ToString() + "(Http Gui) Node is being deleted");

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
        ~Attr()
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
                    FPluginInfo.Name = "Attr";
                    //the nodes category: try to use an existing one
                    FPluginInfo.Category = "JQuery";
                    //the nodes version: optional. leave blank if not
                    //needed to distinguish two nodes of the same name and category
                    FPluginInfo.Version = "";

                    //the nodes author: your sign
                    FPluginInfo.Author = "iceberg";
                    //describe the nodes function
                    FPluginInfo.Help = "Node for calling the JQuery Attr function";
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
            FHost.CreateStringInput("Attribute", TSliceMode.Single, TPinVisibility.True, out FAttributeStringInput);
            FAttributeStringInput.SetSubType("Attribute", false);

			FHost.CreateStringInput("Value", TSliceMode.Single, TPinVisibility.True, out FValueStringInput);
			FValueStringInput.SetSubType("Value", false);
        }

        #endregion pin creation


        #region Main Loop



		protected override void OnEvaluate(int SpreadMax, bool changedSpreadSize, string NodeId, List<string> SlideId, bool ReceivedNewString, List<string> ReceivedString)
		{
			if (changedSpreadSize || DynamicPinsAreChanged())
			{
				string attributeSlice;
				string valueSlice;

				FAttributeStringInput.GetString(0, out attributeSlice);
				FValueStringInput.GetString(0, out valueSlice);

				if (attributeSlice == null)
				{
					attributeSlice = "";
				}

				if (valueSlice == null)
				{
					valueSlice = "";
				}

				FAttrMethodCall.SetParameters(attributeSlice, valueSlice);
			}
		}
        
        #endregion Main Loop

		protected override bool DynamicPinsAreChanged()
		{
			return (FAttributeStringInput.PinIsChanged || FValueStringInput.PinIsChanged);
		}
	}
}
