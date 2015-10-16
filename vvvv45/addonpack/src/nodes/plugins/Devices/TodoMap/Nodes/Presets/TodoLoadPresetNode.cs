using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VVVV.PluginInterfaces.V2;
using VVVV.TodoMap.Lib;
using System.IO;
using VVVV.TodoMap.Lib.Persist;

namespace VVVV.TodoMap.Nodes.Presets
{
    [PluginInfo(Name = "TodoLoadPreset", Author = "vux", Category = "TodoMap", AutoEvaluate = true)]
    public class TodoLoadPresetNode : IPluginEvaluate
    {
        [Input("Engine", IsSingle = true)]
        Pin<TodoEngine> FInEngine;

        [Input("Path", StringType = StringType.Filename, IsSingle = true)]
        ISpread<string> FInPath;

        [Input("Do Load", IsBang = true)]
        ISpread<bool> FInDoLoad;

        //[Input("Apply")]
        //ISpread<bool> FInApply;

        //[Output("Variable Name")]
        //ISpread<string> FOutVarName;

        //[Output("Preset Value")]
        //ISpread<double> FOutValue;

        #region IPluginEvaluate Members
        public void Evaluate(int SpreadMax)
        {
            if (this.FInEngine.PluginIO.IsConnected)
            {
                if (this.FInDoLoad[0])
                {
                    string path = this.FInPath[0];
                    try
                    {
                        if (File.Exists(path))
                        {
                            StreamReader sr = null;
                            try
                            {
                                sr = new StreamReader(path);
                                string xml = sr.ReadToEnd();
                                sr.Close();

                                TodoPresetWrapper.LoadXml(this.FInEngine[0], xml);
                            }
                            catch
                            {
                                if (sr != null)
                                {
                                    sr.Close();
                                }
                            }
                        }
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
