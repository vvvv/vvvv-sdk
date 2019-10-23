#region usings
using System;
using System.ComponentModel.Composition;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.VColor;
using VVVV.Utils.VMath;
using VVVV.Core.Logging;

using V2 = System.Numerics.Vector2;
using V3 = System.Numerics.Vector3;
using V4 = System.Numerics.Vector4;

using RCP;
using RCP.Parameters;
using RCP.Transporter;

using Kaitai;
#endregion usings

namespace VVVV.Nodes
{
	#region PluginInfo
	[PluginInfo(Name = "Stick", 
				Category = "RCP", 
				Help = "An RCP Client",
				Tags = "remote, rabbit, client",
				AutoEvaluate = true)]
	#endregion PluginInfo
	public class RCPStickNode : IPluginEvaluate, IPartImportsSatisfiedNotification, IDisposable
	{
		#region fields & pins
		[Input("Remote Host", IsSingle=true, DefaultString = "127.0.0.1")]
		public IDiffSpread<string> FRemoteHost; 
		
		[Input("Remote Port", IsSingle=true, DefaultValue = 10000)]
		public IDiffSpread<int> FRemotePort; 
		
		[Input("Initialize", IsSingle=true, IsBang=true)]
		public ISpread<bool> FInit;
		
		[Output("Client")]
		public ISpread<RCPClient> FClientOut;
		
		[Output("Connected")]
		public ISpread<bool> FIsConnected;

        [Output("Connected Server Info")]
        public ISpread<string> FServerInfo;

        [Output("Parameters")]
		public ISpread<RcpParameter> FParameters;
		
		[Import()]
		public ILogger FLogger;

        [Import()]
        public IHDEHost FHDEHost;

        RCPClient FRCPClient;
		WebsocketClientTransporter FTransporter;
		HashSet<short> FParamIds = new HashSet<short>();
		#endregion fields & pins
		
		public RCPStickNode()
		{
			
		}
		
		public void OnImportsSatisfied()
		{
            FRCPClient = new RCPClient("vvvv beta " + FHDEHost.Version.ToString());

            FRCPClient.ParameterAdded += ParameterAdded;
            FRCPClient.ParameterRemoved += ParameterRemoved;

            //FWebsocketTransporter = new WebsocketClientTransporter("127.0.0.1", 10000);
            //FRCPClient.SetTransporter(FWebsocketTransporter);

            //FRCPClient.Log = (s) => FLogger.Log(LogType.Debug, "client: " + s);
            FClientOut[0] = FRCPClient;
		}
		
		public void Dispose()
		{
			FLogger.Log(LogType.Debug, "Disposing the RCP Client");
			FRCPClient.Dispose();
		}
		
		private void ParameterAdded(object sender, IParameter parameter)
		{
			parameter.Updated += ParameterUpdated;
            var valueParameter = parameter as IValueParameter;
            if (valueParameter != null)
                valueParameter.ValueUpdated += ParameterUpdated;
			FParamIds.Add(parameter.Id);
			UpdateOutputs();			
		}
		
		private void ParameterRemoved(object sender, IParameter parameter)
		{
			parameter.Updated -= ParameterUpdated;
            var valueParameter = parameter as IValueParameter;
            if (valueParameter != null)
                valueParameter.ValueUpdated -= ParameterUpdated;
            FParamIds.Remove(parameter.Id);
			UpdateOutputs();
		}

		//called when data for any output pin is requested
		public void Evaluate(int SpreadMax)
		{
			if (FTransporter == null)
			{
				FTransporter = new WebsocketClientTransporter();
				FTransporter.Connected = () => Initialize();
				FRCPClient.SetTransporter(FTransporter);
				FTransporter.Connect(FRemoteHost[0], FRemotePort[0]);
			}			
			else if (FRemoteHost.IsChanged || FRemotePort.IsChanged)
				FTransporter.Connect(FRemoteHost[0], FRemotePort[0]);
			
			//request all values from the server
			if (FInit[0])
			{
				if (!FTransporter.IsConnected)
					FTransporter.Connect(FRemoteHost[0], FRemotePort[0]);
				else
					Initialize();
			}
			
			FIsConnected[0] = FTransporter.IsConnected;
            if (FIsConnected[0])
            {
                if (!string.IsNullOrWhiteSpace(FRCPClient.ConnectedServerVersion))
                    FServerInfo[0] = "RCP Version: " + FRCPClient.ConnectedServerVersion + ", Application Id: " + FRCPClient.ConnectedServerApplicationId;
            }
            else
            {
                FParamIds.Clear();
                UpdateOutputs();
            }
		}
		
		private void Initialize()
		{
			FParamIds.Clear();
			FRCPClient.Initialize();
            FRCPClient.RequestInfo();
		}
		
		//updates all parameters
		private void UpdateOutputs()
		{
			var parameters = FParamIds.Select(id => FRCPClient.GetParameter(id)).OrderBy(p => p.Order);
			var ps = new List<RcpParameter>();
			
			foreach (var p in parameters)
				ps.Add(GetParameter(p));
			
			FParameters.AssignFrom(ps);
		}
		
		//update one parameter
		private void ParameterUpdated(object sender, EventArgs e)
		{
			var p = sender as Parameter;
			var orderedParams = FParamIds.Select(pid => FRCPClient.GetParameter(pid)).OrderBy(param => param.Order).ToList();
			FParameters[orderedParams.IndexOf(p)] = GetParameter(p);
		}
		
		private RcpParameter GetParameter(Parameter p)
		{
			var v = RCP.Helpers.PipeUnEscape(RCP.Helpers.ValueToString(p));
			var datatype = RCP.Helpers.DatatypeToString(p.TypeDefinition);
			var typedef = RCP.Helpers.TypeDefinitionToString(p.TypeDefinition);
			var userdata = p.Userdata != null ? Encoding.UTF8.GetString(p.Userdata) : "";
			
			var grp = "";
			if (p.ParentId != 0)
				grp = FRCPClient.GetParameter(p.ParentId)?.Label ?? "invalid group ID: " + p.ParentId;
			
			return new RcpParameter(p.Id, datatype, typedef, v, p.Label, grp, p.Widget, userdata);
		}
	}
	
	#region PluginInfo
	[PluginInfo(Name = "Carrot", 
				Category = "RCP", 
				Help = "An RCP Client",
				Tags = "remote, client",
				AutoEvaluate = true)]
	#endregion PluginInfo
	public class RCPCarrotNode : IPluginEvaluate
	{
		#region fields & pins
		[Input("Client")]
		public ISpread<RCPClient> FClientIn;
		
		[Input("ID")]
		public ISpread<string> FID;
		
		[Input("Value")]
		public ISpread<string> FValue;
		
		[Input("Userdata")]
		public ISpread<string> FUserdata;
		
		[Input("Send")]
		public ISpread<bool> FSend;
		
		[Import()]
		public ILogger FLogger;
		#endregion fields & pins
		
		//called when data for any output pin is requested
		public void Evaluate(int SpreadMax)
		{
			for (int i=0; i<SpreadMax; i++)
				if (FSend[i] && FClientIn[0] != null)
				{
					short id;
					if (short.TryParse(FID[i], out id))
					{
						var param = FClientIn[0].GetParameter(id);
						if (param != null)
						{
							var p = RCP.Helpers.StringToValue(param, FValue[i]);
							p.Userdata = Encoding.UTF8.GetBytes(FUserdata[i]);
							FClientIn[0].Update();
						}
					}
				}
		}
	}
}
