using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VVVV.PluginInterfaces.V2;
using VVVV.PluginInterfaces.V1;
using VVVV.Hosting.Factories;
using System.ComponentModel.Composition;

namespace VVVV.Nodes.vvvv.StartableInfo
{
    [PluginInfo(Name="Info",Category="VVVV",Version="Startables",Author="vux",Help="Returns info about any autostart modules in VVVV")]
    public class StartableInfoNode : IPluginEvaluate
    {
        [Input("Update", IsSingle = true, IsBang = true)]
        ISpread<bool> FInUpdate;

        [Output("Name")]
        ISpread<string> FOutName;

        [Output("Is Started")]
        ISpread<bool> FOutStarted;

        [Output("Message")]
        ISpread<string> FOutMessage;

        [Output("Type Name",Visibility=PinVisibility.Hidden)]
        ISpread<string> FOutTypeName;

        IStartableRegistry FStartableRegistry;

        bool FFirstFrame = true;

        [ImportingConstructor()]
        public StartableInfoNode(IStartableRegistry startableRegistry)
        {
            FStartableRegistry = startableRegistry;
        }

        public void Evaluate(int SpreadMax)
        {
            if (FInUpdate[0] || FFirstFrame)
            {
                List<IStartableStatus> status = FStartableRegistry.Status;
                int cnt = status.Count;
                FOutMessage.SliceCount = cnt;
                FOutName.SliceCount = cnt;
                FOutStarted.SliceCount = cnt;
                FOutTypeName.SliceCount = cnt;

                for (int i = 0; i < cnt; i++)
                {
                    IStartableStatus s = status[i];
                    FOutTypeName[i] = s.TypeName;
                    FOutStarted[i] = s.Success;
                    FOutMessage[i] = s.Message;
                    FOutName[i] = s.Name;
                }

                FFirstFrame = false;
            }
        }
    }
}
