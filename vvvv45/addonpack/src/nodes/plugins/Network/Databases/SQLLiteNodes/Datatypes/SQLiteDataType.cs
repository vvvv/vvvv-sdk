using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using System.Data;
using System.Data.SQLite;

namespace VVVV.DataTypes
{
    [Guid("A91683C7-5FD0-4e2a-9E3B-7932CA9BFE37"),
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface ISQLiteConnectionDataType : IDbConnectionDataType<SQLiteConnection>
    {
    }

    public class SQLiteConnectionDataType : AbstractDbConnection<SQLiteConnection>
    {

        public override Guid GUID
        {
            get { return new Guid("A91683C7-5FD0-4e2a-9E3B-7932CA9BFE37"); }
        }

        public override string FriendlyName
        {
            get { return "SQLite Connection"; }
        }

        public override IDbDataAdapter GetDataAdapter(string sql)
        {
            return new SQLiteDataAdapter(sql, this.Connection);
        }

        public override IDbCommand GetCommand()
        {
            SQLiteCommand cmd = new SQLiteCommand();
            cmd.Connection = this.Connection;
            return cmd;
        }
    }
}
