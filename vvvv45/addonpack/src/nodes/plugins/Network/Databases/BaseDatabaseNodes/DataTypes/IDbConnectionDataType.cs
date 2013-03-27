using System;
using System.Collections.Generic;
using System.Text;
using VVVV.PluginInterfaces.V1;
using System.Data;

namespace VVVV.DataTypes
{
    public interface IDbConnectionDataType<T> where T : IDbConnection
    {
        bool HasChanged { get; }
        T Connection { get; }
    }

    public abstract class AbstractDbConnection<T> : IDbConnectionDataType<T> where T : IDbConnection
    {
        protected T cnx;
        protected bool changed;

        public abstract Guid GUID { get; }
        public abstract string FriendlyName { get; }
        public abstract IDbDataAdapter GetDataAdapter(string sql);
        public abstract IDbCommand GetCommand();

        public T Connection
        {
            get { return cnx; }
            set { cnx = value; }
        }

        public bool HasChanged
        {
            get { return this.changed; }
            set { this.changed = value; }
        }
    }
}
