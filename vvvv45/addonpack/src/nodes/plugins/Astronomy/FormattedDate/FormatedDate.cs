#region licence/info

//////project name
//vvvv TCP Client Advanced

//////description
//Re-implementation and extension of vvvv standard TCP Client node.

//////licence
//GNU Lesser General Public License (LGPL)
//english: http://www.gnu.org/licenses/lgpl.html
//german: http://www.gnu.de/lgpl-ger.html

//////language/ide
//C# sharpdevelop 

//////dependencies
//VVVV.PluginInterfaces.V1;
//VVVV.Utils.VColor;
//VVVV.Utils.VMath;

//////initial author
//iceberg (Joshua Samberg)

#endregion licence/info

//use what you need
using System;
using System.Globalization;
using System.ComponentModel.Composition;
using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.Core.Logging;

//the vvvv node namespace
namespace VVVV.Nodes
{
    [PluginInfo(Name = "Date", 
                Category = "Astronomy", 
                Version = "", 
                Help = "Returns the current date and time in the requested format", 
                Tags = "time", 
                Author = "phlegma")]
    public class FormatedDate : IPluginEvaluate
    {
        #region field declaration
        //input pin declaration
        [Input("Format")]
        public IDiffSpread<string> FFormat;

        [Input("Culture Info", DefaultString = "en-Us")]
        public IDiffSpread<string> FCultureInfo;

        [Input("Update", DefaultBoolean = true)]
        public IDiffSpread<bool> FUpdate;

        //output pins
        [Output("Current Date")]
        public ISpread<string> FCurrentDate;

        [Output("UTC")]
        public ISpread<string> FUTC;

        [Import()]
        public ILogger FLogger;

        private string FCurrentCultureInfoString;
        private CultureInfo FCurrentCultureInfo;
        #endregion field declaration

        #region mainloop
        private void UpdateCulture(string cultureInfo)
        {
            if (cultureInfo != FCurrentCultureInfoString)
            {
                FCurrentCultureInfo = new CultureInfo(cultureInfo);
                FCurrentCultureInfoString = cultureInfo;
            }
        }

        public void Evaluate(int SpreadMax)
        {
            FCurrentDate.SliceCount = SpreadMax;
            FUTC.SliceCount = SpreadMax;

            for (int i = 0; i < SpreadMax; i++)
			{
                try
                {
                    if (FUpdate[i] || FFormat.IsChanged || FCultureInfo.IsChanged)
                    {
                        if (!string.IsNullOrWhiteSpace(FFormat[i]) && !string.IsNullOrWhiteSpace(FCultureInfo[i]))
                        {
                            UpdateCulture(FCultureInfo[i]);
                            FCurrentDate[i] = DateTime.Now.ToString(FFormat[i], FCurrentCultureInfo);
                            FUTC[i] = DateTime.UtcNow.ToString(FFormat[i], FCurrentCultureInfo);
                        }
                        else if (!string.IsNullOrWhiteSpace(FFormat[i]))
                        {
                            FCurrentDate[i] = DateTime.Now.ToString(FFormat[i]);
                            FUTC[i] = DateTime.UtcNow.ToString(FFormat[i]);
                        }
                        else if (!string.IsNullOrWhiteSpace(FCultureInfo[i]))
                        {
                            UpdateCulture(FCultureInfo[i]);
                            FCurrentDate[i] = DateTime.Now.ToString(FCurrentCultureInfo);
                            FUTC[i] = DateTime.UtcNow.ToString(FCurrentCultureInfo);
                        }
                        else
                        {
                            FCurrentDate[i] = DateTime.Now.ToString();
                            FUTC[i] = DateTime.UtcNow.ToString();
                        }
                    }
                }
                catch (Exception ex)
                {
                    FLogger.Log(LogType.Error, ex.Message);
                }
			}
        }

        #endregion mainloop
    }
}
