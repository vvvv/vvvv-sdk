using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using System.Data.Odbc;
using System.Data;
using Devart.Data.MySql;

namespace VVVV.DataTypes
{
    [Guid("D8B1B8BA-6163-4695-A847-8B949FBE0EE0"),
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IMySqlConnectionDataType : IDbConnectionDataType<MySqlConnection>
    {
    }

    public class MySqlConnectionDbDataType : AbstractDbConnection<MySqlConnection>
    {
        public override Guid GUID
        {
            get { return new Guid("A28CB9A6-E865-4D9F-B08E-1356E6595B2B"); }
        }

        public override string FriendlyName
        {
            get { return "MySQL Connection"; }
        }

        public override IDbDataAdapter GetDataAdapter(string sql)
        {
            return new MySqlDataAdapter(sql, this.Connection);
        }

        public override IDbCommand GetCommand()
        {
            var cmd = new MySqlCommand();
            cmd.Connection = this.Connection;
            return cmd;
        }
    }
}
