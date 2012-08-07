using System;
using System.Collections.Generic;
using System.Text;
using VVVV.PluginInterfaces.V1;
using VVVV.DataTypes;
using System.Data.SQLite;

namespace VVVV.Nodes
{
    public class SQLiteDatabaseNode : AbstractDatabaseNode<SQLiteConnectionDataType, SQLiteConnection>, IPlugin, IDisposable
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
                    FPluginInfo.Category = "SQLite";
                    FPluginInfo.Version = "Network";
                    FPluginInfo.Author = "vux";
                    FPluginInfo.Help = "SQLite connection";
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

        protected override SQLiteConnectionDataType CreateConnectionObject()
        {
            SQLiteConnectionDataType result = new SQLiteConnectionDataType();
            try
            {
                result.Connection = new SQLiteConnection();
            }
            catch (Exception ex)
            {
                this.FHost.Log(TLogType.Error, ex.Message);
                throw;
            }
            return result;
        }
    }
}
