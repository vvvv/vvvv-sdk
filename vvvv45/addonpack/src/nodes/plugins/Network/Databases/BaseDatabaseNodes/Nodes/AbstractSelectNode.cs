using System;
using System.Collections.Generic;
using System.Text;
using VVVV.PluginInterfaces.V1;
using VVVV.DataTypes;
using System.Data;
using System.Linq;

namespace VVVV.Nodes
{
    public abstract class AbstractSelectNode<T,D> : IPluginConnections where T : AbstractDbConnection<D> where D : IDbConnection
    {
        protected IPluginHost FHost;
        protected AbstractDbConnection<D> FConnectionObject;
        protected string FQuery;

        #region Pins
        //Config
        private IStringConfig FPinCfgFields;

        //Inputs
        private INodeIn FPinInConnection;
        
        private IStringIn FPinInFields;
        private IStringIn FPinInTables;
        //private IStringIn FPinInJoin;
        private IStringIn FPinInWhere;
        private IStringIn FPinInGroupBy;
        private IStringIn FPinInHaving;
        private IStringIn FPinInOrderBy;
        
        private IValueFastIn FPinInSendQuery;

       
        //Outputs
        private IStringOut FPinOutStatement;
        private IStringOut FPinOutStatus;
        private IStringOut FPinOutFieldNames;
        private IValueOut FPinOutOnData;
        private Dictionary<string, IStringOut> FPinOutFields = new Dictionary<string, IStringOut>();
        
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

            //Configuration
            this.FHost.CreateStringConfig("Field Names", TSliceMode.Single, TPinVisibility.True, out this.FPinCfgFields);
            this.FPinCfgFields.SetSubType("", false);

            //Inputs
            this.FHost.CreateNodeInput("Connection", TSliceMode.Single, TPinVisibility.True, out this.FPinInConnection);
            this.FPinInConnection.SetSubType(new Guid[1] { dtype.GUID } , dtype.FriendlyName);

            
            this.FHost.CreateStringInput("Fields", TSliceMode.Single, TPinVisibility.True, out this.FPinInFields);
            this.FPinInFields.SetSubType("*", false);
           
            this.FHost.CreateStringInput("Tables", TSliceMode.Single, TPinVisibility.True, out this.FPinInTables);
            this.FPinInTables.SetSubType("", false);
 
            this.FHost.CreateStringInput("Where", TSliceMode.Single, TPinVisibility.True, out this.FPinInWhere);
            this.FPinInWhere.SetSubType("", false);

            this.FHost.CreateStringInput("Group By", TSliceMode.Single, TPinVisibility.True, out this.FPinInGroupBy);
            this.FPinInGroupBy.SetSubType("", false);
       
            this.FHost.CreateStringInput("Having", TSliceMode.Single, TPinVisibility.True, out this.FPinInHaving);
            this.FPinInHaving.SetSubType("", false);

            this.FHost.CreateStringInput("Order By", TSliceMode.Single, TPinVisibility.True, out this.FPinInOrderBy);
            this.FPinInOrderBy.SetSubType("", false);
        
        
            this.FHost.CreateValueFastInput("SendQuery", 1, null, TSliceMode.Single, TPinVisibility.True, out this.FPinInSendQuery);
            this.FPinInSendQuery.SetSubType(0, 1, 1, 0, true, false, false);

            //Outputs
            
            this.FHost.CreateStringOutput("Statement", TSliceMode.Single, TPinVisibility.True, out this.FPinOutStatement);
            this.FPinOutStatement.SetSubType("", false);
        

            this.FHost.CreateStringOutput("Status", TSliceMode.Single, TPinVisibility.True, out this.FPinOutStatus);
            this.FPinOutStatus.SetSubType("", false);

            this.FHost.CreateValueOutput("OnData", 1, null, TSliceMode.Single, TPinVisibility.True, out this.FPinOutOnData);
            this.FPinOutOnData.SetSubType(0, 1, 1, 0, true, false, false);

            this.FHost.CreateStringOutput("Dataset Column Names", TSliceMode.Dynamic, TPinVisibility.True, out this.FPinOutFieldNames);
            this.FPinOutFieldNames.SetSubType("", false);
        
        }
        #endregion

        #region Configurate
        public void Configurate(IPluginConfig Input)
        {
            if (Input == this.FPinCfgFields)
            {
                //Clear all field pins
                foreach (IStringOut sout in this.FPinOutFields.Values)
                {
                    this.FHost.DeletePin(sout);
                }
                this.FPinOutFields.Clear();

                string fnames;
                this.FPinCfgFields.GetString(0, out fnames);
                string[] fields = fnames.Split(",".ToCharArray());

                string[] reservedNames = { "Statement", "Status", "OnData", "Dataset Column Names" };

                //Add field pin only if same name is not added already
                foreach (string f in fields)
                {
                	var field = f.Trim();
                    if (field.Length > 0)
                    {
                        if (!this.FPinOutFields.ContainsKey(field))
                        {
                            if (reservedNames.Contains(field))
                                field = field + "_";

                            IStringOut so;
                            this.FHost.CreateStringOutput(field, TSliceMode.Dynamic, TPinVisibility.True, out so);
                            this.FPinOutFields.Add(field, so);
                        }
                    }
                }
            }
        }
        #endregion

        #region Evaluate
        public void Evaluate(int SpreadMax)
        {
            double dblsend;
            this.FPinInSendQuery.GetValue(0, out dblsend);
            bool ondata = false;

            if (this.FPinInFields.PinIsChanged ||
                this.FPinInTables.PinIsChanged ||
                this.FPinInWhere.PinIsChanged ||
                this.FPinInGroupBy.PinIsChanged ||
                this.FPinInHaving.PinIsChanged ||
                this.FPinInOrderBy.PinIsChanged
                )
            {
                string fields, tables,where,groupby,orderby,having;

                this.FPinInFields.GetString(0,out fields);
                this.FPinInTables.GetString(0,out tables);
                this.FPinInWhere.GetString(0,out where);
                this.FPinInGroupBy.GetString(0, out groupby);
                this.FPinInHaving.GetString(0,out having);
                this.FPinInOrderBy.GetString(0, out orderby);

                tables = tables == null ? "" : tables;
                fields = fields == null ? "" : fields;
                where = where == null ? "" : where;
                groupby = groupby == null ? "" : groupby;
                having = having == null ? "" : having;
                orderby = orderby == null ? "" : orderby;


                this.FQuery = "SELECT ";
                this.FQuery += fields;
                
                if (tables.Trim().Length > 0)
                {
                    this.FQuery += " FROM ";
                    this.FQuery += tables;
                }

                if (where.Trim().Length > 0)
                {
                    this.FQuery += " WHERE ";
                    this.FQuery += where;
                }

                if (groupby.Trim().Length > 0)
                {
                    this.FQuery += " GROUP BY ";
                    this.FQuery += groupby;
                }

                if (having.Trim().Length > 0)
                {
                    this.FQuery += " HAVING ";
                    this.FQuery += having;
                }

                if (orderby.Trim().Length > 0)
                {
                    this.FQuery += " ORDER BY ";
                    this.FQuery += orderby;
                }

                this.FPinOutStatement.SetString(0, this.FQuery);
            }


            if (this.FPinInConnection.IsConnected && dblsend >= 0.5)
            {
                if (this.FConnectionObject.Connection.State == ConnectionState.Open)
                {

                    try
                    {
                        IDbDataAdapter da = this.FConnectionObject.GetDataAdapter(this.FQuery);
                        DataSet dst = new DataSet();
                        da.Fill(dst);


                        List<string> matchingfields = new List<string>();
                        this.FPinOutFieldNames.SliceCount = dst.Tables[0].Columns.Count;

                        //Browse column list
                        int colcount = dst.Tables[0].Columns.Count;
                        int rowcount = dst.Tables[0].Rows.Count;


                        for (int i = 0; i < colcount; i++)
                        {
                            string colname = dst.Tables[0].Columns[i].ColumnName;
                            this.FPinOutFieldNames.SetString(i, colname);
                            if (this.FPinOutFields.ContainsKey(colname))
                            {
                                matchingfields.Add(colname);
                                this.FPinOutFields[colname].SliceCount = rowcount;
                            }
                        }

                        foreach (string fname in this.FPinOutFields.Keys)
                        {
                            if (!matchingfields.Contains(fname))
                            {
                                this.FPinOutFields[fname].SliceCount = 0;
                            }
                        }

                        for (int i = 0; i < rowcount; i++)
                        {
                            DataRow row = dst.Tables[0].Rows[i];
                            foreach (string matchedfield in matchingfields)
                            {
                                this.FPinOutFields[matchedfield].SetString(i, Convert.ToString(row[matchedfield]));
                            }
                        }

                        ondata = true;
                        this.FPinOutStatus.SetString(0, "OK");
                    }
                    catch (Exception ex)
                    {
                        this.FPinOutFieldNames.SliceCount = 0;
                        this.FPinOutStatus.SetString(0, ex.Message);

                        foreach (IStringOut so in this.FPinOutFields.Values)
                        {
                            so.SliceCount = 0;
                        }
                    }
                }
                else
                {
                    this.FPinOutFieldNames.SliceCount = 0;
                    this.FPinOutStatus.SetString(0, "Not Connected");

                    foreach (IStringOut so in this.FPinOutFields.Values)
                    {
                        so.SliceCount = 0;
                    }
                }
            }

            if (ondata)
            {
                this.FPinOutOnData.SetValue(0, 1);
            }
            else
            {
                this.FPinOutOnData.SetValue(0, 0);
            }
        }
        #endregion

        #region AutoEvaluate
        public bool AutoEvaluate
        {
            get { return false; }
        }
        #endregion
    }
}
