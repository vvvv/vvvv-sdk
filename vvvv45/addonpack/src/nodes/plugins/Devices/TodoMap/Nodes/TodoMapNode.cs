using System;
using System.Collections.Generic;
using System.Text;
using VVVV.PluginInterfaces.V2;
using VVVV.PluginInterfaces.V1;
using VVVV.TodoMap.Lib;
using System.ComponentModel.Composition;
using System.IO;
using VVVV.TodoMap.Lib.Persist;

namespace VVVV.TodoMap.Nodes
{
    [PluginInfo(Name = "TodoMap",
	            Category = "TodoMap",
	            Author = "vux",
	            Tags = "midi, osc",
	            InitialComponentMode = TComponentMode.InAWindow,
	            InitialWindowWidth = 700,
	            InitialWindowHeight = 500,
	            AutoEvaluate = true)]
    public partial class TodoMapNode : IPluginEvaluate, IDisposable, IPartImportsSatisfiedNotification
    {
        [Input("Selected Variable", IsSingle=true)]
        ISpread<string> FInSelVar;

        [Input("Select Variable", IsSingle = true,IsBang=true)]
        ISpread<bool> FInSelect;

        [Input("Learn Mode",IsSingle=true)]
        IDiffSpread<bool> FInLearnMode;

        [Input("Path", IsSingle = true,StringType=StringType.Filename,DefaultString="TodoMap.xml")]
        IDiffSpread<string> FInPath;

        [Input("Load", IsSingle = true, IsBang = true)]
        ISpread<bool> FInLoad;

        [Input("Save", IsSingle = true, IsBang = true)]
        ISpread<bool> FInSave;

        [Input("OSC Ignore List")]
        IDiffSpread<string> FInIgnoreListOsc;

        [Input("Enabled", IsSingle = true)]
        IDiffSpread<bool> FInEnabled;

        [Input("Clear Mappings", IsSingle = true, IsBang = true)]
        ISpread<bool> FInClearMappings;

        [Input("Clear Variables",IsSingle = true,IsBang=true)]
        ISpread<bool> FInReset;

        [Input("Auto Load", IsSingle = true)]
        ISpread<bool> FInAutoLoad;

        [Input("Save On Exit", IsSingle = true)]
        ISpread<bool> FInSaveExit;

        //Outputs
        [Output("Engine",IsSingle=true)]
        ISpread<TodoEngine> FOutEngine;

        [Output("Selected Variable")]
        ISpread<string> FOutSelVarName;

        private TodoEngine FEngine;

        private bool FClearMapNextFrame;
        private bool FCLearAllNextFrame;
        private bool FSaveNextFrame;
        private bool FSaveExit;
        private string FPath;

        #region IPluginEvaluate Members
        public void Evaluate(int SpreadMax)
        {
            this.FSaveExit = this.FInSaveExit[0];
            this.FPath = this.FInPath[0];

            if (this.FInClearMappings[0] || this.FClearMapNextFrame)
            {
                this.ucMappingManager.ResetMappings();
                this.FEngine.ClearMappings();
                this.FClearMapNextFrame = false;
            }

            if (this.FInReset[0] || this.FCLearAllNextFrame)
            {
                this.ucMappingManager.Reset();
                this.FEngine.ClearVariables();
                this.FCLearAllNextFrame = false;
            }

            if (this.FInIgnoreListOsc.IsChanged)
            {
                this.FEngine.Osc.IgnoreList = new List<string>(this.FInIgnoreListOsc);
            }

            if (this.FInLearnMode.IsChanged)
            {
                this.FEngine.LearnMode = this.FInLearnMode[0];
                this.ucMappingManager.LearnModeUpdated();
            }

            if (this.FInSelect[0])
            {
                this.FEngine.SelectVariable(this.FInSelVar[0]);
            }

            if (this.FInSave[0] || this.FSaveNextFrame)
            {
                string path = this.FInPath[0];
                try
                {
                    StreamWriter sw = new StreamWriter(path);
                    sw.Write(TodoXmlWrapper.Persist(this.FEngine));
                    sw.Close();
                }
                catch
                {

                }
                this.FSaveNextFrame = false;
            }

            if (this.FInPath.IsChanged) { this.FEngine.SavePath = this.FInPath[0]; }

            if (this.FInLoad[0] || (this.FInPath.IsChanged && this.FInAutoLoad[0]))
            {
                string path = this.FInPath[0];
                
                if (File.Exists(path))
                {
                    this.FEngine.ClearVariables();
                    this.ucMappingManager.Reset();

                    StreamReader sr = null;
                    try
                    {
                        sr = new StreamReader(path);
                        string xml = sr.ReadToEnd();
                        sr.Close();

                        this.FEngine.ClearVariables();
                        TodoXmlUnwrapper.LoadXml(this.FEngine, xml);
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

            if (this.FEngine.SelectedVariable != null)
            {
                this.FOutSelVarName.SliceCount = 1;
                this.FOutSelVarName[0] = this.FEngine.SelectedVariable.Name;
            }
            else
            {
                this.FOutSelVarName.SliceCount = 0;
            }
        }
        #endregion

        #region IDisposable Members
        void IDisposable.Dispose()
        {
            if (this.FSaveExit)
            {
                try
                {
                    StreamWriter sw = new StreamWriter(this.FPath);
                    sw.Write(TodoXmlWrapper.Persist(this.FEngine));
                    sw.Close();
                }
                catch
                {

                }
            }

            this.FEngine.Dispose();
        }
        #endregion

        public void OnImportsSatisfied()
        {
            this.FOutEngine[0] = this.FEngine;
        }

    }
}
