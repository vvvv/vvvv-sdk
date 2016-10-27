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
    [PluginInfo(Name = "TodoSavePreset", Author = "vux", Category = "TodoMap",AutoEvaluate=true)]
    public class TodoSavePresetNode : IPluginEvaluate
    {
        [Input("Engine", IsSingle = true)]
        Pin<TodoEngine> FInEngine;

        [Input("Variable Name")]
        ISpread<string> FInVarName;

        [Input("Path",StringType=StringType.Filename,IsSingle=true)]
        ISpread<string> FInPath;

        [Input("Do Save",IsBang=true)]
        ISpread<bool> FInDoSave;

        #region IPluginEvaluate Members
        public void Evaluate(int SpreadMax)
        {
            if (this.FInEngine.PluginIO.IsConnected)
            {
                if (this.FInDoSave[0])
                {
                    string path = this.FInPath[0];
                    try
                    {
                        StreamWriter sw = new StreamWriter(path);
                        sw.Write(TodoPresetWrapper.Persist(this.FInEngine[0],this.FInVarName.ToArray()));
                        sw.Close();
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
