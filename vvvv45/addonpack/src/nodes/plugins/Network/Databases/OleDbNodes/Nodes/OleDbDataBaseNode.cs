using System;
using System.Collections.Generic;
using System.Text;
using System.Data.OleDb;
using VVVV.PluginInterfaces.V1;
using VVVV.DataTypes;

namespace VVVV.Nodes
{
    public class OleDbDataBaseNode : AbstractDatabaseNode<OleDbConnectionDbDataType, OleDbConnection>, IPlugin, IDisposable
    {
        private static IPluginInfo FPluginInfo;
        public static IPluginInfo PluginInfo
        {
            get
            {
                if (FPluginInfo == null)
                {
                    FPluginInfo = new PluginInfo();
                    FPluginInfo.Name = "Database";
                    FPluginInfo.Category = "OleDb";
                    FPluginInfo.Version = "Network";
                    FPluginInfo.Author = "vux";
                    FPluginInfo.Help = "OleDb connection";
                    FPluginInfo.Tags = "";
                    FPluginInfo.Credits = "";
                    FPluginInfo.Bugs = "";
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

        protected override OleDbConnectionDbDataType CreateConnectionObject()
        {
            OleDbConnectionDbDataType result = new OleDbConnectionDbDataType();
            result.Connection = new OleDbConnection();
            return result;
        }
    }
}
