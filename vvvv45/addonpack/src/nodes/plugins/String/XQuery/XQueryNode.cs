using System;
using System.Collections.Generic;
using System.Text;
using VVVV.PluginInterfaces.V1;
using Microsoft.Xml.XQuery;
using System.Xml.XPath;
using System.Xml;
using System.IO;

namespace VVVV.Nodes
{
    
    public class XQueryNode : IPlugin, IDisposable
    {
        #region Plugin Info
        public static IPluginInfo PluginInfo
        {
            get
            {
                IPluginInfo Info = new PluginInfo();
                Info.Name = "XQuery";							//use CamelCaps and no spaces
                Info.Category = "XML";						//try to use an existing one
                Info.Version = "";						//versions are optional. leave blank if not needed
                Info.Help = "Performs an XQuery on a node";
                Info.Bugs = "";
                Info.Credits = "";								//give credits to thirdparty code used
                Info.Warnings = "";
				Info.Author = "vux";

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
        #endregion

        #region Fields
        private IPluginHost FHost;

        private IStringIn FPinInput;
        private IStringIn FPinInAlias;
        private IStringIn FPinInQuery;
        

        private IStringOut FPinOutput;
        private IValueOut FPinOutIsValid;

        #endregion

        #region Auto Evaluate
        public bool AutoEvaluate
        {
            get { return false; }
        }
        #endregion

        #region Set Plugin Host
        public void SetPluginHost(IPluginHost Host)
        {
            //assign host
            this.FHost = Host;
       
            this.FHost.CreateStringInput("Input", TSliceMode.Dynamic, TPinVisibility.True, out this.FPinInput);
            this.FPinInput.SetSubType("", false);

            this.FHost.CreateStringInput("Alias", TSliceMode.Dynamic, TPinVisibility.True, out this.FPinInAlias);
            this.FPinInAlias.SetSubType("doc", false);
          
            this.FHost.CreateStringInput("Query", TSliceMode.Dynamic, TPinVisibility.True, out this.FPinInQuery);
            this.FPinInQuery.SetSubType("", false);
    
            this.FHost.CreateStringOutput("Output", TSliceMode.Dynamic, TPinVisibility.True, out this.FPinOutput);
            this.FPinOutput.SetSubType("", false);

            this.FHost.CreateValueOutput("Is Valid", 1, null, TSliceMode.Single, TPinVisibility.True, out this.FPinOutIsValid);
            this.FPinOutIsValid.SetSubType(0, 1, 1, 0, false, true, false);
          
        }
        #endregion

        #region Configurate
        public void Configurate(IPluginConfig Input)
        {
        }
        #endregion

        #region Evaluate
        public void Evaluate(int SpreadMax)
        {
            if (this.FPinInput.PinIsChanged || this.FPinInQuery.PinIsChanged || this.FPinInAlias.PinIsChanged)
            {
                this.FPinOutIsValid.SliceCount = SpreadMax;
                this.FPinOutput.SliceCount = SpreadMax;
                for (int i = 0; i < SpreadMax; i++)
                {
                    string input, query,alias;
                    this.FPinInput.GetString(i, out input);
                    this.FPinInAlias.GetString  (i, out alias);
                    this.FPinInQuery.GetString(i, out query);

                    if (input == null) { input = String.Empty;}
                    if (alias == null) { alias = String.Empty;}
                    if (query == null) { query = String.Empty;}

                    try
                    {

                        XQueryNavigatorCollection col = new XQueryNavigatorCollection();

                        XmlDocument doc = new XmlDocument();
                        doc.LoadXml(input);
                        col.AddNavigator(doc.CreateNavigator(), alias);

                        XQueryExpression expr = new XQueryExpression(query);
                        string result = expr.Execute(col).ToXml();

                        this.FPinOutput.SetString(i, result);
                        this.FPinOutIsValid.SetValue(i, 1);
                    }
                    catch
                    {
                        this.FPinOutput.SetString(i, "");
                        this.FPinOutIsValid.SetValue(i, 0);
                    }
                }
            }
        }
        #endregion

        #region Dispose
        public void Dispose()
        {
        }
        #endregion
    }
        
        
}
