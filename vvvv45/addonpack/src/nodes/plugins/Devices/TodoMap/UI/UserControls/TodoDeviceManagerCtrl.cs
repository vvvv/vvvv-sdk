using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Sanford.Multimedia.Midi;

namespace VVVV.TodoMap.UI.UserControls
{
    public partial class TodoDeviceManagerCtrl : UserControl
    {
        public TodoDeviceManagerCtrl()
        {
            InitializeComponent();

            this.SetupListViewColumns();

            this.RefreshInputDevice();

            this.RefreshOutputDevice();
        }

        #region Setup List Views
        private void SetupListViewColumns()
        {
            this.lvMidiInput.Columns.Add("Device Name");
            this.lvMidiInput.Columns[0].Width = 300;
            this.lvMidiInput.Columns.Add("Enabled");

            CheckBox cbMidiInEnable = new CheckBox();
            cbMidiInEnable.Checked = false;
            this.Controls.Add(cbMidiInEnable);

            this.lvMidiOutput.Columns.Add("Device Name");
            this.lvMidiOutput.Columns[0].Width = 300;
            this.lvMidiOutput.Columns.Add("Enabled");

            CheckBox cbMidiOutEnable = new CheckBox();
            cbMidiOutEnable.Checked = false;
            this.Controls.Add(cbMidiOutEnable);
        }
        #endregion

        public void RefreshInputDevice()
        {
            this.lvMidiInput.Items.Clear();

            for (int i = 0; i < InputDevice.DeviceCount; i++)
            {
                ListViewItem lv = new ListViewItem();
                this.lvMidiInput.Items.Add(lv);
                lv.Text = InputDevice.GetDeviceCapabilities(i).name;
                lv.SubItems.Add(new ListViewItem.ListViewSubItem(lv, "False"));
            }
        }

        public void RefreshOutputDevice()
        {
            this.lvMidiOutput.Items.Clear();

            for (int i = 0; i < OutputDevice.DeviceCount; i++)
            {
                ListViewItem lv = new ListViewItem();
                this.lvMidiOutput.Items.Add(lv);
                lv.Text = OutputDevice.GetDeviceCapabilities(i).name;
                lv.SubItems.Add(new ListViewItem.ListViewSubItem(lv, "False"));
            }
        }
    }
}
