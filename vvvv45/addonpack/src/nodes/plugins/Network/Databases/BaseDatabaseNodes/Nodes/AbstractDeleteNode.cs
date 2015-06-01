using System;
using System.Collections.Generic;
using System.Text;
using VVVV.PluginInterfaces.V1;
using VVVV.DataTypes;
using System.Data;

namespace VVVV.Nodes
{
    public abstract class AbstractDeleteNode<T, D> : IPluginConnections
        where T : AbstractDbConnection<D>
        where D : IDbConnection
    {
        protected IPluginHost FHost;
        protected AbstractDbConnection<D> FConnectionObject;
        protected string FQuery;


        #region Pins
        //Inputs
        private INodeIn FPinInConnection;
        private IStringIn FPinInTable;
        private IStringIn FPinInWhere;
        private IValueFastIn FPinInSendQuery;

        //Outputs
        private IStringOut FPinOutStatement;
        private IStringOut FPinOutStatus;
        private IValueOut FPinOutRowsAffected;
        #endregion

        #region Connections
        public void ConnectPin(IPluginIO Pin)
        {
            //cache a reference to the upstream interface when the NodeInput pin is being connected
            if (Pin == FPinInConnection)
            {
                object usI;
                FPinInConnection.GetUpstreamInterface(out usI);
                FConnectionObject = usI as AbstractDbConnection<D>;
            }
        }

        public void DisconnectPin(IPluginIO Pin)
        {
            //reset the cached reference to the upstream interface when the NodeInput is being disconnected
            if (Pin == FPinInConnection)
            {
                FConnectionObject = null;
            }
        }
        #endregion

        #region Set plugin Host
        public void SetPluginHost(IPluginHost Host)
        {
            T dtype = (T)Activator.CreateInstance(typeof(T));
            this.FHost = Host;


            //Inputs
            this.FHost.CreateNodeInput("Connection", TSliceMode.Single, TPinVisibility.True, out this.FPinInConnection);
            this.FPinInConnection.SetSubType(new Guid[1] { dtype.GUID } , dtype.FriendlyName);

            this.FHost.CreateStringInput("Table", TSliceMode.Single, TPinVisibility.True, out this.FPinInTable);
            this.FPinInTable.SetSubType("", false);

            this.FHost.CreateStringInput("Where", TSliceMode.Single, TPinVisibility.True, out this.FPinInWhere);
            this.FPinInWhere.SetSubType("1=0", false);

            this.FHost.CreateValueFastInput("SendQuery", 1, null, TSliceMode.Single, TPinVisibility.True, out this.FPinInSendQuery);
            this.FPinInSendQuery.SetSubType(0, 1, 1, 0, true, false, false);

            //Outputs
            this.FHost.CreateStringOutput("Statement", TSliceMode.Single, TPinVisibility.True, out this.FPinOutStatement);
            this.FPinOutStatement.SetSubType("", false);

            this.FHost.CreateStringOutput("Status", TSliceMode.Single, TPinVisibility.True, out this.FPinOutStatus);
            this.FPinOutStatus.SetSubType("", false);

            this.FHost.CreateValueOutput("Rows Affected", 1, null, TSliceMode.Single, TPinVisibility.True, out this.FPinOutRowsAffected);
            this.FPinOutRowsAffected.SetSubType(0, double.MaxValue, 1, 0, false, false, true);

        }
        #endregion

        #region Configurate
        public void Configurate(IPluginConfig Input)
        {

        }
        #endregion

        #region Evaluate
        public void Evaluate(int SpreadMax)
        {
            double dblsend;
            this.FPinInSendQuery.GetValue(0, out dblsend);

            if (this.FPinInTable.PinIsChanged ||
                this.FPinInWhere.PinIsChanged)
            {
                string table, where;
                this.FPinInTable.GetString(0, out table);
                this.FPinInWhere.GetString(0, out where);

                table = table == null ? "" : table;
                where = where == null ? "" : where;

                this.FQuery = "DELETE ";
                this.FQuery += table;

                if (where.Trim().Length > 0)
                {
                    this.FQuery += " WHERE " + where;
                }

                this.FPinOutStatement.SetString(0, this.FQuery);
            }

            if (this.FPinInConnection.IsConnected && dblsend >= 0.5)
            {
                if (this.FConnectionObject.Connection.State == ConnectionState.Open)
                {

                    try
                    {
                        IDbCommand cmd = this.FConnectionObject.GetCommand();
                        cmd.CommandType = CommandType.Text;
                        cmd.CommandText = this.FQuery;
                        int result = cmd.ExecuteNonQuery();

                        this.FPinOutStatus.SetString(0, "OK");
                        this.FPinOutRowsAffected.SetValue(0, result);
                    }
                    catch (Exception ex)
                    {
                        this.FPinOutStatus.SetString(0, ex.Message);
                        this.FPinOutRowsAffected.SetValue(0, 0);
                    }
                }
                else
                {
                    this.FPinOutStatus.SetString(0, "Not Connected");
                    this.FPinOutRowsAffected.SetValue(0, 0);
                }
            }
        }
        #endregion

        #region AutoEvaluate
        public bool AutoEvaluate
        {
            get { return true; }
        }
        #endregion
    }
}
