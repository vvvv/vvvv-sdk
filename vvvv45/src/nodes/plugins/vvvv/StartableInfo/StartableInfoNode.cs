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

        DotNetPluginFactory FDNFactory;

        bool FFirstFrame = true;

        [ImportingConstructor()]
        public StartableInfoNode(DotNetPluginFactory dnFactory)
        {
            FDNFactory = dnFactory;
        }

        public void Evaluate(int SpreadMax)
        {
            if (FInUpdate[0] || FFirstFrame)
            {
                List<StartableStatus> status = FDNFactory.StartableRegistry.Status;
                int cnt = status.Count;
                FOutMessage.SliceCount = cnt;
                FOutName.SliceCount = cnt;
                FOutStarted.SliceCount = cnt;
                FOutTypeName.SliceCount = cnt;

                for (int i = 0; i < cnt; i++)
                {
                    StartableStatus s = status[i];
                    FOutTypeName[i] = s.Info.Type.FullName;
                    FOutStarted[i] = s.Success;
                    FOutMessage[i] = s.Message;
                    FOutName[i] = s.Info.Name;
                }

                FFirstFrame = false;
            }
        }
    }
}
