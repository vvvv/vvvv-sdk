using SharpDX.RawInput;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using VVVV.Hosting.IO;
using VVVV.Nodes.Input;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.IO;

namespace VVVV.Nodes.Input
{
    public abstract class GlobalDeviceInputNode<TDevice> : IPluginEvaluate, IPartImportsSatisfiedNotification, IDisposable
    {
        [Input("Enabled", DefaultBoolean = true, IsSingle = true, Order = int.MinValue)]
        public ISpread<bool> EnabledIn;

        [Input("Index", IsSingle = true, Order = int.MinValue + 1)]
        public IDiffSpread<int> IndexIn;

        [Output("Device", IsSingle = true)]
        public ISpread<TDevice> DeviceOut;

        [Output("Device Name", IsSingle = true, Visibility = PinVisibility.Hidden)]
        public ISpread<string> DeviceNameOut;

        [Import]
        protected IOFactory FIOFactory;
        private PluginContainer FDeviceStatesSplitNode;
        private readonly DeviceType FDeviceType;
        private readonly string FSplitNodeName;
        private readonly string FSplitNodeDeviceOutputName;

        public GlobalDeviceInputNode(DeviceType deviceType, string splitNodeName, string splitNodeDeviceOutputName)
        {
            FDeviceType = deviceType;
            FSplitNodeName = splitNodeName;
            FSplitNodeDeviceOutputName = splitNodeDeviceOutputName;
        }

        public virtual void OnImportsSatisfied()
        {
            RawInputService.DevicesChanged += RawKeyboardService_DevicesChanged;
            IndexIn.Changed += IndexIn_Changed;
            SubscribeToDevices();

            var nodeInfo = FIOFactory.NodeInfos.First(n => n.Name == FSplitNodeName && n.Category == FSplitNodeDeviceOutputName && n.Version == "Split");
            FDeviceStatesSplitNode = FIOFactory.CreatePlugin(nodeInfo, c => c.IOAttribute.Name == FSplitNodeDeviceOutputName, c => DeviceOut);
        }

        void IndexIn_Changed(IDiffSpread<int> spread)
        {
            SubscribeToDevices();
        }

        void RawKeyboardService_DevicesChanged(object sender, EventArgs e)
        {
            SubscribeToDevices();
        }

        public virtual void Dispose()
        {
            IndexIn.Changed -= IndexIn_Changed;
            RawInputService.DevicesChanged -= RawKeyboardService_DevicesChanged;
            FDeviceStatesSplitNode.Dispose();
        }

        protected virtual void SubscribeToDevices()
        {
            var devices = Device.GetDevices()
                .Where(d => d.DeviceType == FDeviceType)
                .OrderBy(d => d, new DeviceComparer())
                .ToList();
            var index = IndexIn.SliceCount > 0 ? IndexIn[0] : 0;
            if (devices.Count > 0)
            {
                var device = devices[index % devices.Count];
                DeviceOut.SliceCount = 1;
                DeviceOut[0] = CreateDevice(device, 0);
                DeviceNameOut.SliceCount = 1;
                DeviceNameOut[0] = device.GetDeviceDescription();
            }
            else
            {
                DeviceOut.SliceCount = 0;
                DeviceNameOut.SliceCount = 0;
            }
        }

        protected abstract TDevice CreateDevice(DeviceInfo deviceInfo, int slice);

        public void Evaluate(int spreadMax)
        {
            // Evaluate our split plugin
            FDeviceStatesSplitNode.Evaluate(spreadMax);
        }
    }
}
