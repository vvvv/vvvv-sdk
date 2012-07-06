using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using Npgsql;
using System.Data;

namespace VVVV.DataTypes
{
    [Guid("A91683C7-5FD0-4e2a-9E3B-7892CA9BFE37"),
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IPostgreSQLConnectionDataType : IDbConnectionDataType<NpgsqlConnection>
    {
    }

    public class PostgreSQLConnectionDataType : AbstractDbConnection<NpgsqlConnection>
    {

        public override Guid GUID
        {
            get { return new Guid("A91683C7-5FD0-4e2a-9E3B-7892CA9BFE37"); }
        }

        public override string FriendlyName
        {
            get { return "PostgreSQL Connection"; }
        }

        public override IDbDataAdapter GetDataAdapter(string sql)
        {
            return new NpgsqlDataAdapter(sql, this.Connection);
        }

        public override IDbCommand GetCommand()
        {
            NpgsqlCommand cmd = new NpgsqlCommand();
            cmd.Connection = this.Connection;
            return cmd;
        }
    }
}
