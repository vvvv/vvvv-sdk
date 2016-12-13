using System;
using System.Collections.Generic;
using System.Text;
using System.Management;

namespace VVVV.TodoMap.Lib.Utils
{
    public class UsbDetector
    {
        private ManagementEventWatcher aw;
        private ManagementEventWatcher rw;

        public event EventHandler UsbAdded;
        public event EventHandler UsbRemoved;

        public UsbDetector()
        {

        }

        public void Start()
        {
            this.AddRemoveUSBHandler();
            this.AddInsetUSBHandler();
        }

        public void Stop()
        {
            if (aw != null)
                aw.Stop();

            if (rw != null) rw.Stop();
        }

        private void AddRemoveUSBHandler()
        {

            WqlEventQuery q;
            ManagementScope scope = new ManagementScope("root\\CIMV2");
            scope.Options.EnablePrivileges = true;

            try
            {

                q = new WqlEventQuery();
                q.EventClassName = "__InstanceDeletionEvent";
                q.WithinInterval = new TimeSpan(0, 0, 3);
                q.Condition = @"TargetInstance ISA 'Win32_USBControllerdevice'";
                aw = new ManagementEventWatcher(scope, q);
                aw.EventArrived += new EventArrivedEventHandler(USBRemoved);
                aw.Start();

            }

            catch (Exception e)
            {

                Console.WriteLine(e.Message);
                if (aw != null)
                    aw.Stop();

            }

        }

        private void AddInsetUSBHandler()
        {

            WqlEventQuery q;
            ManagementScope scope = new ManagementScope("root\\CIMV2");
            scope.Options.EnablePrivileges = true;

            try
            {

                q = new WqlEventQuery();
                q.EventClassName = "__InstanceCreationEvent";
                q.WithinInterval = new TimeSpan(0, 0, 3);
                q.Condition = @"TargetInstance ISA 'Win32_USBControllerdevice'";
                rw = new ManagementEventWatcher(scope, q);
                rw.EventArrived += new EventArrivedEventHandler(USBAdded);
                rw.Start();

            }
            catch (Exception e)
            {

                Console.WriteLine(e.Message);
                if (rw != null)
                    rw.Stop();

            }

        }

        public void USBAdded(object sender, EventArgs e)
        {
            if (this.UsbAdded != null) { this.UsbAdded(sender, e); }
        }

        public void USBRemoved(object sender, EventArgs e)
        {
            if (this.UsbRemoved != null) { this.UsbRemoved(sender, e); }
        }
    }
}
