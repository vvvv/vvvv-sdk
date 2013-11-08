using System;
using System.Collections.Generic;
using System.Text;
using VVVV.DataTypes;
using Npgsql;
using VVVV.PluginInterfaces.V1;

namespace VVVV.Nodes
{
    public class PostgreSQLInsertNode : AbstractInsertNode<PostgreSQLConnectionDataType, NpgsqlConnection>, IPlugin
    {
        private static IPluginInfo FPluginInfo;
        public static IPluginInfo PluginInfo
        {
            get
            {
                if (FPluginInfo == null)
                {
                    FPluginInfo = new PluginInfo();
                    FPluginInfo.Name = "Insert";
                    FPluginInfo.Category = "PostgreSQL";
                    FPluginInfo.Version = "Network";
                    FPluginInfo.Author = "vux";
                    FPluginInfo.Help = "Insert records on a postgreSQL database";
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

    }
}
