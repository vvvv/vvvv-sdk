using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using VVVV.PluginInterfaces.V1;
using VVVV.DataTypes;

namespace VVVV.Nodes
{
    public abstract class AbstractDatabaseNode<T,D> : IDisposable where T : AbstractDbConnection<D> where D : IDbConnection
    {
        protected IPluginHost FHost;
        protected AbstractDbConnection<D> FConnectionObject;
        private bool cnxvalid;

        #region Pins
        private IStringIn FPinInConnectionString;
        private IValueIn FPinInConnect;

        private INodeOut FPinOutConnection;
        private IStringOut FPinOutStatus;
        private IValueOut FPinOutConnected;
        #endregion

        protected abstract T CreateConnectionObject();

        public AbstractDatabaseNode()
        {
            this.FConnectionObject = this.CreateConnectionObject();
        }

        #region Set plugin Host
        public void SetPluginHost(IPluginHost Host)
        {
            this.FHost = Host;

            this.FHost.CreateStringInput("Connection String", TSliceMode.Single, TPinVisibility.True, out this.FPinInConnectionString);
            this.FPinInConnectionString.SetSubType("", false);

            this.FHost.CreateValueInput("Connect", 1, null, TSliceMode.Single, TPinVisibility.True, out this.FPinInConnect);
            this.FPinInConnect.SetSubType(0, 1, 1, 0, false, true, false);

            //Outputs    	
            this.FHost.CreateNodeOutput("Connection", TSliceMode.Single, TPinVisibility.True, out this.FPinOutConnection);
            this.FPinOutConnection.SetSubType2(typeof(D), new Guid[1] { this.FConnectionObject.GUID }, this.FConnectionObject.FriendlyName);
            this.FPinOutConnection.SetInterface(this.FConnectionObject);
            
            this.FHost.CreateStringOutput("Status", TSliceMode.Single, TPinVisibility.True, out this.FPinOutStatus);
            this.FPinOutStatus.SetSubType("", false);

            this.FHost.CreateValueOutput("Is Connected", 1, null, TSliceMode.Single, TPinVisibility.True, out this.FPinOutConnected);
            this.FPinOutConnected.SetSubType(0, 1, 1, 0, false, true, false);
          
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
            bool connectionreset = false;
            this.FConnectionObject.HasChanged = false;
            if (this.FPinInConnectionString.PinIsChanged)
            {
                this.Reset();
                connectionreset = true;
                string cnx;
                this.FPinInConnectionString.GetString(0, out cnx);

                try
                {
                    this.FConnectionObject.Connection.ConnectionString = cnx;

                    connectionreset = true;
                    this.FConnectionObject.HasChanged = true;
                    this.cnxvalid = true;
                }
                catch
                {
                    this.cnxvalid = false; //Some db engine throw exceptions
                }
            }

            if (this.FPinInConnect.PinIsChanged || connectionreset)
            {
                double connect;
                this.FPinInConnect.GetValue(0, out connect);
                if (connect >= 0.5 && this.FConnectionObject.Connection.State == ConnectionState.Closed)
                {
                    if (this.cnxvalid)
                    {
                        try
                        {
                            this.FConnectionObject.Connection.Open();
                            this.FPinOutConnected.SetValue(0, 1);
                            this.FPinOutStatus.SetString(0, "Connected");
                            this.FConnectionObject.HasChanged = true;
                        }
                        catch (Exception ex)
                        {
                            this.FPinOutConnected.SetValue(0, 0);
                            this.FPinOutStatus.SetString(0, ex.Message);
                            this.FConnectionObject.HasChanged = true;
                        }
                    }
                }

                if (connect < 0.5 && this.FConnectionObject.Connection.State != ConnectionState.Closed)
                {
                    this.Reset();
                    this.FPinOutConnected.SetValue(0, 0);
                    this.FPinOutStatus.SetString(0, "Closed");
                }
            }
        }
        #endregion

        #region AutoEvaluate
        public bool AutoEvaluate
        {
            get { return false; }
        }
        #endregion

        #region Dispose
        public void Dispose()
        {
            this.Reset();
        }
        #endregion

        #region Reset Connection
        private void Reset()
        {
            if (this.FConnectionObject.Connection != null)
            {
                if (this.FConnectionObject.Connection.State != ConnectionState.Closed)
                {
                    try
                    {
                        this.FConnectionObject.Connection.Close();
                    }
                    catch
                    {

                    }
                }
                
            }
        }
        #endregion
    }
}
