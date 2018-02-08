#region usings
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.ComponentModel.Composition;
using System.Linq;

using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.VColor;
using VVVV.Utils.VMath;
using VVVV.Utils.Win32;

using VVVV.Core.Logging;
using System.Runtime.InteropServices.ComTypes;
using VVVV.Utils.Streams;
#endregion usings

namespace VVVV.Nodes
{
    public class SomeTypeA
    {

    }
	
    [PluginInfo(Name = "OutputSomeTypeA", Category = "Node")]
    public class OutputSomeTypeFloatNode : IPluginEvaluate
    {
        [Output("Output")]
        public ISpread<SomeTypeA> FOutput;

        //called when data for any output pin is requested
        public void Evaluate(int spreadMax)
        {
            FOutput.SliceCount = 1;
            FOutput[0] = new SomeTypeA();
        }
    }
	
    public class DynamiclyTypedSpread
    {
        INodeIn NodeIn;

        public DynamiclyTypedSpread(INodeIn nodeIn)
        {
            NodeIn = nodeIn;
        }

        public ISpread<T> TryTypedSpread<T>()
        {
            var type = typeof(T);
            object upData;
            NodeIn.GetUpstreamInterface(out upData);
            var stream = upData as IInStream<T>;

            if (stream != null)
                return GetGenericSpread<T>(upData);

            if (type == typeof(float) || type == typeof(double))
                return GetValueSpread<T>(upData);

            if (type == typeof(RGBAColor))
                return GetColorSpread<T>(upData);

            if (type == typeof(string))
                return GetStringSpread<T>(upData);

            if (type == typeof(System.Runtime.InteropServices.ComTypes.IStream))
                return GetRawSpread<T>(upData);

            return null;
        } 

        ISpread<T> GetGenericSpread<T>(object upData)
        {
            var upStream = upData as IInStream<T>;
            if (upStream != null)
            {
                var currentSpread = new Spread<T>(NodeIn.SliceCount);
                var upperSpread = upStream.ToSpread();
                for (int i = 0; i < NodeIn.SliceCount; i++)
                {
                    int upSlice;
                    NodeIn.GetUpsreamSlice(i, out upSlice);
                    currentSpread[i] = (T)((object)upperSpread[upSlice]);
                }
                return currentSpread;
            }
            return null;
        }

        ISpread<T> GetValueSpread<T>(object upData)
        {
            var upValueData = upData as IValueData;
            if (upValueData != null)
            {
                var currentSpread = new Spread<T>(NodeIn.SliceCount);
                for (int i = 0; i < NodeIn.SliceCount; i++)
                {
                    int upSlice;
                    NodeIn.GetUpsreamSlice(i, out upSlice);
                    double value;
                    upValueData.GetValue(upSlice, out value);
                    currentSpread[i] = (T)((object)value);
                }
                return currentSpread;
            }
            return null;
        }

        ISpread<T> GetColorSpread<T>(object upData)
        {
            var upColorData = upData as IColorData;
            if (upColorData != null)
            {
                var currentSpread = new Spread<T>(NodeIn.SliceCount);
                for (int i = 0; i < NodeIn.SliceCount; i++)
                {
                    int upSlice;
                    NodeIn.GetUpsreamSlice(i, out upSlice);
                    RGBAColor value;
                    upColorData.GetColor(upSlice, out value);
                    currentSpread[i] = (T)((object)value);
                }
                return currentSpread;
            }
            return null;
        }

        ISpread<T> GetStringSpread<T>(object upData)
        {
            var upStringData = upData as IStringData;
            if (upStringData != null)
            {
                var currentSpread = new Spread<T>(NodeIn.SliceCount);
                for (int i = 0; i < NodeIn.SliceCount; i++)
                {
                    int upSlice;
                    NodeIn.GetUpsreamSlice(i, out upSlice);
                    string value;
                    upStringData.GetString(upSlice, out value);
                    currentSpread[i] = (T)((object)value);
                }
                return currentSpread;
            }
            return null;
        }

        ISpread<T> GetRawSpread<T>(object upData)
        {
            var upRawData = upData as IRawData;
            if (upRawData != null)
            {
                var currentSpread = new Spread<T>(NodeIn.SliceCount);
                for (int i = 0; i < NodeIn.SliceCount; i++)
                {
                    int upSlice;
                    NodeIn.GetUpsreamSlice(i, out upSlice);
                    System.Runtime.InteropServices.ComTypes.IStream value;
                    upRawData.GetData(upSlice, out value);
                    currentSpread[i] = (T)(value);
                }
                return currentSpread;
            }
            return null;
        }
    }

    [PluginInfo(Name = "TestA", Category = "Node")]
    public class VariantTestANode : IPluginEvaluate, IPartImportsSatisfiedNotification
    {
        [Input("X")]
        public INodeIn FX; // -> node pin without any subtype info set
        DynamiclyTypedSpread FXSpread;

        [Output("Output")]
        public ISpread<string> FOutput;

        [Output("Color")]
        public ISpread<RGBAColor> FColorOut;

        [Output("Value")]
        public ISpread<double> FValueOut;

  		[Output("Connected")]
		public ISpread<bool> FConnected;  	
    	
        [Import()]
        public ILogger FLogger;

        public void OnImportsSatisfied()
        {
            FXSpread = new DynamiclyTypedSpread(FX);

            // the following internally sets some legacy "or-connection handler"

                        FX.SetSubType(
                            new Guid[]{
                                typeof(IValueData).GUID,
                                typeof(IColorData).GUID,
                                typeof(IStringData).GUID,
                                typeof(IRawData).GUID,
                                typeof(SomeTypeA).GUID,
                             },
                            "Value | Color | String | Raw | SomeTypeA");
        }

        //called when data for any output pin is requested
        public void Evaluate(int spreadMax)
        {
        	FOutput.SliceCount = spreadMax;
        	FColorOut.SliceCount = spreadMax;        	
        	FValueOut.SliceCount = spreadMax;  
        	
        	FConnected.SliceCount = 1;
        	FConnected[0] = FX.IsConnected;
        	
        	if (FX.SliceCount>0)
        	{
	            var values = FXSpread.TryTypedSpread<double>();
	        	if (values != null)
	            {
		            FOutput[0] = "first value: " + values[0].ToString();
	                for (int i = 0; i < values.SliceCount; i++)
	            	{
		                FValueOut[i] = values[i];            		
	            	}
	            	return;
	            }
	
	            var strings = FXSpread.TryTypedSpread<string>();
	            if (strings != null)
	            {
	                FOutput[0] = "first string: " + strings[0].ToString();	            	
	                return;
	            }
	
	            var colors = FXSpread.TryTypedSpread<RGBAColor>();
	            if (colors != null)
	            {
	                FOutput[0] = "first color: " + colors[0].ToString();
	                for (int i = 0; i < colors.SliceCount; i++)
	            	{
		                FColorOut[i] = colors[i];            		
	            	}
	                return;
	            }
	
	            var streams = FXSpread.TryTypedSpread<System.Runtime.InteropServices.ComTypes.IStream>();
	            if (streams != null)
	            {
	                FOutput[0] = "first raw: " + streams[0].ToString();
	                return;
	            }
	
	            var someTypes = FXSpread.TryTypedSpread<SomeTypeA>();
	            if (someTypes != null)
	            {
	                FOutput[0] = "first someType: " + someTypes[0].ToString();
	                return;
	            }
        		
        		FOutput[0] = "unconnected";        	
			}
        }
    }
}
