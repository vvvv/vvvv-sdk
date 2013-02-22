using System;
using System.Collections.Generic;
using System.Text;
using VVVV.PluginInterfaces.V1;
using VVVV.DataTypes;
using System.Data;

namespace VVVV.Nodes
{
    public abstract class AbstractInsertNode<T,D> : IPluginConnections where T : AbstractDbConnection<D> where D : IDbConnection
    {
        protected IPluginHost FHost;
        protected AbstractDbConnection<D> FConnectionObject;
        protected string FQuery;


        #region Pins
        //Inputs
        private INodeIn FPinInConnection;
        private IStringIn FPinInTable;
        private IStringIn FPinInFields;
        private IStringIn FPinInValues;
        private IValueFastIn FPinInSendQuery;

        //Outputs
        private IStringOut FPinOutStatement;
        private IStringOut FPinOutStatus;
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

            this.FHost.CreateStringInput("Fields", TSliceMode.Single, TPinVisibility.True, out this.FPinInFields);
            this.FPinInFields.SetSubType("", false);

            this.FHost.CreateStringInput("Values", TSliceMode.Single, TPinVisibility.True, out this.FPinInValues);
            this.FPinInValues.SetSubType("", false);

            this.FHost.CreateValueFastInput("SendQuery", 1, null, TSliceMode.Single, TPinVisibility.True, out this.FPinInSendQuery);
            this.FPinInSendQuery.SetSubType(0, 1, 1, 0, true, false, false);

   
            //Outputs
            this.FHost.CreateStringOutput("Statement", TSliceMode.Single, TPinVisibility.True, out this.FPinOutStatement);
            this.FPinOutStatement.SetSubType("", false);

            this.FHost.CreateStringOutput("Status", TSliceMode.Single, TPinVisibility.True, out this.FPinOutStatus);
            this.FPinOutStatus.SetSubType("", false);        
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
            if (this.FPinInFields.PinIsChanged ||
                this.FPinInTable.PinIsChanged ||
                this.FPinInValues.PinIsChanged)
            {
                string table, fields, values;
                this.FPinInTable.GetString(0, out table);
                this.FPinInFields.GetString(0, out fields);
                this.FPinInValues.GetString(0, out values);

                table = table == null ? "" : table;
                fields = fields == null ? "" : fields;
                values = values == null ? "" : values;

                this.FQuery = "INSERT INTO ";
                this.FQuery += table;

                if (fields.Trim().Length > 0)
                {
                    this.FQuery += "(" + fields + ")";
                }

                if (values.Trim().Length > 0)
                {
                    this.FQuery += " VALUES(" + values + ")";
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
                        cmd.ExecuteNonQuery();

                        this.FPinOutStatus.SetString(0, "OK");
                    }
                    catch (Exception ex)
                    {
                        this.FPinOutStatus.SetString(0, ex.Message);
                    }
                }
                else
                {
                    this.FPinOutStatus.SetString(0, "Not Connected");
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
