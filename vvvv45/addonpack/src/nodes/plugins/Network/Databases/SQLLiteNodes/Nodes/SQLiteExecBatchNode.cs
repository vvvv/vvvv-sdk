using System;
using System.Collections.Generic;
using System.Text;
using VVVV.Nodes;
using VVVV.DataTypes;
using System.Data.SQLite;
using VVVV.PluginInterfaces.V1;

namespace SQLLiteNodes.Nodes
{

    public class SQLiteExecBatchNode : AbstractQueryBatchNode<SQLiteConnectionDataType, SQLiteConnection>, IPlugin
    {
        private static IPluginInfo FPluginInfo;
        public static IPluginInfo PluginInfo
        {
            get
            {
                if (FPluginInfo == null)
                {
                    FPluginInfo = new PluginInfo();
                    FPluginInfo.Name = "QueryBatch";
                    FPluginInfo.Category = "SQLite";
                    FPluginInfo.Version = "Network";
                    FPluginInfo.Author = "vux";
                    FPluginInfo.Help = "Executes a batch of queries on a sqlite database";
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
