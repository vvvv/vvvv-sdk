#region usings
using System;
using System.ComponentModel.Composition;

using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.VColor;
using VVVV.Utils.VMath;

using VVVV.Core.Logging;
#endregion usings

namespace VVVV.Nodes
{
	public class Wrap<T>
	{
		
	}
	
	[PluginInfo(Name = "Source", Category = "Test", Tags = "c#")]
	public class Source : IPluginEvaluate
	{
		[Output("Output")]
		public ISpread<Wrap<object>> FOutput;

		[Import()]
		public ILogger FLogger;

		//called when data for any output pin is requested
		public void Evaluate(int SpreadMax)
		{
			FOutput.SliceCount = 2;

			//FLogger.Log(LogType.Debug, "hi tty!");
		}
	}
	
	[PluginInfo(Name = "Sink", Category = "Test", Tags = "c#")]
	public class Sink : IPluginEvaluate, IPartImportsSatisfiedNotification, IConnectionHandler
	{
		[Input("Input")]
		public INodeIn FInput;
		
		[Output("Connected")]
		public ISpread<bool> FConnected;

		[Import()]
		public ILogger FLogger;

        public void OnImportsSatisfied()
        {
            FInput.SetConnectionHandler(this, this);
        }

        public bool Accepts(object source, object sink)
        {
            Type[] sourcetype = source.GetType().GetGenericArguments();

            if (sourcetype.Length > 0)
            {
                if (!sourcetype[0].IsGenericType) { return false; }

                if (sourcetype[0].GetGenericTypeDefinition() == typeof(Wrap<>))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        public string GetFriendlyNameForSink(object sink)
        {
            var sinkDataType = sink.GetType().GetGenericArguments()[1];
            return string.Format(" [ Needs: {0} ]", sinkDataType.FullName);          
        }

        public string GetFriendlyNameForSource(object source)
        {
            var sourceDataType = source.GetType().GetGenericArguments()[1];
            return string.Format(" [ Supports: {0} ]", sourceDataType.FullName);
        }
		
		//called when data for any output pin is requested
		public void Evaluate(int SpreadMax)
		{
			FConnected[0] = FInput.IsConnected;

		}
	}
		
}
