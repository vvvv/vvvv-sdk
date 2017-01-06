using System;
using System.Collections.Generic;
using System.Text;
using VVVV.DataTypes;
using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using Devart.Data.MySql;

namespace VVVV.Nodes
{
    public class MySQLUpdateNode : AbstractUpdateNode<MySqlConnectionDbDataType, MySqlConnection>, IPlugin
    {
        #region Plugin Info
        private static IPluginInfo FPluginInfo;
        public static IPluginInfo PluginInfo
        {
            get
            {
                if (FPluginInfo == null)
                {
                    FPluginInfo = new PluginInfo();
                    FPluginInfo.Name = "Update";
                    FPluginInfo.Category = "MySQL";
                    FPluginInfo.Version = "Network";
                    FPluginInfo.Author = "vux";
                    FPluginInfo.Help = "Update Data via a MySQL connection";
                    FPluginInfo.Tags = "database";
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
        #endregion

    }
}
