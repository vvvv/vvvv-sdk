using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using System.Data.Odbc;
using System.Data;

namespace VVVV.DataTypes
{

    [Guid("0D553BBE-BA38-4040-8B2B-2DA073DCC433"),
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IOdbcConnectionDataType : IDbConnectionDataType<OdbcConnection>
    {
    }

    public class OdbcConnectionDbDataType : AbstractDbConnection<OdbcConnection>
    {
        public override Guid GUID
        {
            get { return new Guid("0D553BBE-BA38-4040-8B2B-2DA073DCC433"); }
        }

        public override string FriendlyName
        {
            get { return "ODBC Connection"; }
        }

        public override IDbDataAdapter GetDataAdapter(string sql)
        {
            return new OdbcDataAdapter(sql, this.Connection);
        }

        public override IDbCommand GetCommand()
        {
            OdbcCommand cmd = new OdbcCommand();
            cmd.Connection = this.Connection;
            return cmd;
        }
    }
}
