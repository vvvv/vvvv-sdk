using SharpDX.RawInput;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using VVVV.Hosting.IO;
using VVVV.Nodes.Input;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.IO;
using VVVV.Utils.VMath;

namespace VVVV.Nodes.Input
{
    public abstract class DesktopDeviceInputNode<TDevice> : IPluginEvaluate, IPartImportsSatisfiedNotification, IDisposable
    {
        [Input("Enabled", DefaultBoolean = true, Order = int.MinValue)]
        public IDiffSpread<bool> EnabledIn;

        [Input("Index", Order = int.MinValue + 1, Visibility = PinVisibility.OnlyInspector, MinValue = -1, DefaultValue = -1)]
        public IDiffSpread<int> IndexIn;

        [Output("Device")]
        public ISpread<TDevice> DeviceOut;

        [Output("Device Name", Visibility = PinVisibility.OnlyInspector)]
        public ISpread<string> DeviceNameOut;

        [Output("Device Description", Visibility = PinVisibility.OnlyInspector)]
        public ISpread<string> DeviceDescriptionOut;

        [Output("Device Count", Order = int.MaxValue, Visibility = PinVisibility.OnlyInspector, IsSingle = true)]
        public ISpread<int> DeviceCountOut;

        [Import]
        protected IOFactory FIOFactory;
        private PluginContainer FDeviceStatesSplitNode;
        private readonly DeviceType FDeviceType;
        private readonly string FSplitNodeName;
        private readonly string FSplitNodeDeviceOutputName;

        public DesktopDeviceInputNode(DeviceType deviceType, string splitNodeName, string splitNodeDeviceOutputName)
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
            // The following function doesn't work properly in Windows XP, returning installed mouses only.
            var devices = Device.GetDevices()
                .Where(d => d.DeviceType == FDeviceType)
                .OrderBy(d => d, new DeviceComparer())
                .ToList();
            // So let's not rely on it if we're running on XP.
            if (devices.Count > 0 || !Environment.OSVersion.IsWinVistaOrHigher())
            {   
                var spreadMax = GetMaxSpreadCount();
                DeviceOut.SliceCount = spreadMax;
                DeviceNameOut.SliceCount = spreadMax;
                DeviceDescriptionOut.SliceCount = spreadMax;
                // An index of -1 means to merge all devices into one
                for (int i = 0; i < spreadMax; i++)
                {
                    var index = IndexIn[i];
                    if (index < 0 || devices.Count == 0)
                    {
                        DeviceOut[i] = CreateMergedDevice(i);
                        DeviceNameOut[i] = string.Empty;
                        DeviceDescriptionOut[i] = "Merged";
                    }
                    else
                    {
                        var device = devices[index % devices.Count];
                        DeviceOut[i] = CreateDevice(device, i);
                        DeviceNameOut[i] = device.DeviceName;
                        DeviceDescriptionOut[i] = device.GetDeviceDescription();
                    }
                }
            }
            else
            {
                DeviceOut.SliceCount = 1;
                DeviceOut[0] = CreateDummy();
                DeviceNameOut.SliceCount = 1;
                DeviceNameOut[0] = "Dummy";
                DeviceDescriptionOut.SliceCount = 1;
                DeviceDescriptionOut[0] = "Dummy";
            }
            DeviceCountOut[0] = devices.Count;
        }

        protected virtual int GetMaxSpreadCount()
        {
            return SpreadUtils.SpreadMax(EnabledIn, IndexIn);
        }

        protected abstract TDevice CreateDevice(DeviceInfo deviceInfo, int slice);
        protected abstract TDevice CreateMergedDevice(int slice);
        protected abstract TDevice CreateDummy();

        protected IObservable<bool> GetDisabledNotifications(int slice)
        {
            return EnabledIn.ToObservable(slice)
                .DistinctUntilChanged()
                .Where(enabled => !enabled);
        }

        public void Evaluate(int spreadMax)
        {
            // Evaluate our split plugin
            if (DeviceOut.SliceCount > 0)
                FDeviceStatesSplitNode.Evaluate(spreadMax);
            else
                FDeviceStatesSplitNode.Evaluate(0);
        }
    }
}
