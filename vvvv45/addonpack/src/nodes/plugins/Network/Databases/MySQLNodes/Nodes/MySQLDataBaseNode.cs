using System;
using System.Collections.Generic;
using System.Text;
using VVVV.PluginInterfaces.V1;
using VVVV.DataTypes;
using Devart.Data.MySql;

namespace VVVV.Nodes
{
    public class MySQLDataBaseNode : AbstractDatabaseNode<MySqlConnectionDbDataType, MySqlConnection>, IPlugin
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
                    FPluginInfo.Category = "MySQL";
                    FPluginInfo.Version = "Network";
                    FPluginInfo.Author = "vux";
                    FPluginInfo.Help = "MySQL connection";
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

        protected override MySqlConnectionDbDataType CreateConnectionObject()
        {
            var result = new MySqlConnectionDbDataType();
            result.Connection = new MySqlConnection();
            return result;
        }
    }
}
