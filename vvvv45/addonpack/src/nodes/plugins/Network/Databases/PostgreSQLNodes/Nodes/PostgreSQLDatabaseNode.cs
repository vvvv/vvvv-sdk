using System;
using System.Collections.Generic;
using System.Text;
using VVVV.PluginInterfaces.V1;
using VVVV.DataTypes;
using Npgsql;

namespace VVVV.Nodes
{
    public class PostgreSQLDatabaseNode : AbstractDatabaseNode<PostgreSQLConnectionDataType, NpgsqlConnection>, IPlugin, IDisposable
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
                    FPluginInfo.Category = "PostgreSQL";
                    FPluginInfo.Version = "Network";
                    FPluginInfo.Author = "vux";
                    FPluginInfo.Help = "PostgreSQL connection";
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

        protected override PostgreSQLConnectionDataType CreateConnectionObject()
        {
            PostgreSQLConnectionDataType result = new PostgreSQLConnectionDataType();
            try
            {
                result.Connection = new NpgsqlConnection();
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
