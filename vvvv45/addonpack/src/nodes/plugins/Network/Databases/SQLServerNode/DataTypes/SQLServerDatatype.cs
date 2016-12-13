using System;
using System.Collections.Generic;
using System.Text;
using System.Data.SqlClient;
using System.Runtime.InteropServices;
using System.Data;

namespace VVVV.DataTypes
{
    [Guid("9227011B-425F-4317-8225-8BC3B7817FB4"),
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface ISqlConnectionDataType : IDbConnectionDataType<SqlConnection>
    {
    }

    public class SqlConnectionDataType : AbstractDbConnection<SqlConnection>
    {

        public override Guid GUID
        {
            get { return new Guid("9227011B-425F-4317-8225-8BC3B7817FB4"); }
        }

        public override string FriendlyName
        {
            get { return "SQLServer Connection"; }
        }

        public override IDbDataAdapter GetDataAdapter(string sql)
        {
            return new SqlDataAdapter(sql,this.Connection);
        }

        public override IDbCommand GetCommand()
        {
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = this.Connection;
            return cmd;
        }
    }
}
