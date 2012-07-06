using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using System.Data.OleDb;
using System.Data;

namespace VVVV.DataTypes
{

    [Guid("0CD1131C-D195-4daa-BB30-6CB1A61BF5F8"),
        InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IOleDbConnectionDataType : IDbConnectionDataType<OleDbConnection>
    {
    }

    public class OleDbConnectionDbDataType : AbstractDbConnection<OleDbConnection>
    {
        public override Guid GUID
        {
            get { return new Guid("0CD1131C-D195-4daa-BB30-6CB1A61BF5F8"); }
        }

        public override string FriendlyName
        {
            get { return "OleDb Connection"; }
        }

        public override IDbDataAdapter GetDataAdapter(string sql)
        {
            return new OleDbDataAdapter(sql, this.Connection);
        }

        public override IDbCommand GetCommand()
        {
            OleDbCommand cmd = new OleDbCommand();
            cmd.Connection = this.Connection;
            return cmd;
        }
    }
}
