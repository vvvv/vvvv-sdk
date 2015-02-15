using System;
using System.Collections.Generic;
using System.Text;
using VVVV.PluginInterfaces.V1;
using VVVV.DataTypes;
using System.Data;

namespace VVVV.Nodes
{

    public abstract class AbstractQueryBatchNode<T, D> : IPluginConnections
        where T : AbstractDbConnection<D>
        where D : IDbConnection
    {
        protected IPluginHost FHost;
        protected AbstractDbConnection<D> FConnectionObject;
        


        #region Pins
        //Inputs
        private INodeIn FPinInConnection;
        private IStringIn FPinInQuery;
        private IValueFastIn FPinInSendQuery;

        //Outputs
        private IValueOut FPinOutSuccess;
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

            this.FHost.CreateStringInput("Query Text", TSliceMode.Dynamic, TPinVisibility.True, out this.FPinInQuery);
            this.FPinInQuery.SetSubType("", false);
       
            this.FHost.CreateValueFastInput("SendQuery", 1, null, TSliceMode.Single, TPinVisibility.True, out this.FPinInSendQuery);
            this.FPinInSendQuery.SetSubType(0, 1, 1, 0, true, false, false);


            //Output
            
            this.FHost.CreateValueOutput("Success", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out this.FPinOutSuccess);
            this.FPinOutSuccess.SetSubType(0, 1, 1, 0, false, true, false);
        
            this.FHost.CreateStringOutput("Status", TSliceMode.Dynamic, TPinVisibility.True, out this.FPinOutStatus);
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

            if (dblsend >= 0.5 && this.FPinInConnection.IsConnected)
            {
                this.FPinOutSuccess.SliceCount = this.FPinInQuery.SliceCount;
                this.FPinOutStatus.SliceCount = this.FPinInQuery.SliceCount;
                for (int i = 0; i < this.FPinInQuery.SliceCount; i++)
                {
                    if (this.FConnectionObject.Connection.State == ConnectionState.Open)
                    {
                        try
                        {
                            string str;
                            this.FPinInQuery.GetString(i, out str);
                            str = str == null ? "" : str;
                            IDbCommand cmd = this.FConnectionObject.GetCommand();
                            cmd.CommandType = CommandType.Text;
                            cmd.CommandText = str;
                            cmd.ExecuteNonQuery();

                            this.FPinOutSuccess.SetValue(i,1);
                            this.FPinOutStatus.SetString(i, "OK");
                        }
                        catch (Exception ex)
                        {
                            this.FPinOutSuccess.SetValue(i,0);
                            this.FPinOutStatus.SetString(i, ex.Message);
                        }
                    }
                    else
                    {
                        this.FPinOutSuccess.SetValue(i,0);
                        this.FPinOutStatus.SetString(i, "Not Connected");
                    }
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
