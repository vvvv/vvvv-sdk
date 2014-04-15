using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using Sanford.Multimedia.Midi;
using VVVV.TodoMap.Lib;
using VVVV.TodoMap.Lib.Modules.Midi;

namespace VVVV.TodoMap.UI.UserControls
{
    public partial class TodoDeviceManagerCtrl : UserControl
    {
        private ListViewEx.ListViewEx lvMidiInput;
        private ListViewEx.ListViewEx lvMidiOutput;

        private List<Control> midiInputEditors = new List<Control>();
        private List<Control> midiOutputEditors = new List<Control>();

        private CheckBox chkenabled;

        private TodoEngine engine;


        public TodoDeviceManagerCtrl()
        {
            InitializeComponent();
            this.lvMidiInput = new ListViewEx.ListViewEx();
            this.lvMidiOutput = new ListViewEx.ListViewEx();
            
            this.lvMidiInput.Dock = DockStyle.Fill;
            this.lvMidiOutput.Dock = DockStyle.Fill;

            this.lvMidiInput.DoubleClickActivation = true;
            this.lvMidiOutput.DoubleClickActivation = true;

            this.lvMidiInput.View = System.Windows.Forms.View.Details;
            this.lvMidiOutput.View = System.Windows.Forms.View.Details;

            this.grpMidiInput.Controls.Add(this.lvMidiInput);
            this.grpMidiOutput.Controls.Add(this.lvMidiOutput);

            this.lvMidiInput.SubItemClicked += this.MidiInput_SubItemClicked;
            this.lvMidiInput.SubItemEndEditing += this.MidiInput_SubItemEndEditing;

            this.lvMidiOutput.SubItemClicked += this.MidiOutput_SubItemClicked;
            this.lvMidiOutput.SubItemEndEditing += this.MidiOutput_SubItemEndEditing;

            this.SetupListViewColumns();

            

            this.RefreshInputDevice();
            this.RefreshOutputDevice();
        }

        public TodoEngine Engine
        {
            set 
            { 
                this.engine = value;
                this.engine.Midi.DeviceInputStatusChanged += Midi_DeviceInputStatusChanged;
                this.engine.Midi.DeviceOutputStatusChanged += Midi_DeviceOutputStatusChanged;
                this.engine.Midi.DeviceInputAutoChanged += Midi_DeviceInputAutoChanged;
                this.engine.Midi.DeviceOutputAutoChanged += Midi_DeviceOutputAutoChanged;
                this.engine.Midi.ClockValueChanged += Midi_ClockValueChangedDelegate;
            }
        }



        void Midi_ClockValueChangedDelegate(int ticks)
        {
            BeginInvoke((MethodInvoker)delegate()
            {
                double dbl = Convert.ToDouble(ticks) / 24.0;
                dbl = Math.Round(dbl, 3);
                this.lbltime.Text = dbl.ToString();
            });
        }

        private void Midi_DeviceInputStatusChanged(int index, eTodoMidiStatus status)
        {
            BeginInvoke((MethodInvoker)delegate()
            {
                this.lvMidiInput.Items[index].SubItems[4].Text = status.ToString();
                if (status == eTodoMidiStatus.Disconnected || status == eTodoMidiStatus.Error)
                {
                    this.lvMidiInput.Items[index].BackColor = Color.LightSalmon;
                    this.lvMidiInput.Items[index].SubItems[2].Text =  "False";
                }
                else
                {
                    if (status == eTodoMidiStatus.Started)
                    {
                        this.lvMidiInput.Items[index].BackColor = Color.LightGreen;
                        this.lvMidiInput.Items[index].SubItems[2].Text =  "True";
                    }
                    else
                    {
                        this.lvMidiInput.Items[index].BackColor = Color.White;
                        this.lvMidiInput.Items[index].SubItems[2].Text =  "False";
                    }
                }
            });
        }

        private void Midi_DeviceInputAutoChanged(int index, bool auto)
        {
            BeginInvoke((MethodInvoker)delegate()
            {
                this.lvMidiInput.Items[index].SubItems[3].Text = auto.ToString();
            });
        }

        private void Midi_DeviceOutputAutoChanged(int index, bool auto)
        {
            BeginInvoke((MethodInvoker)delegate()
            {
                this.lvMidiOutput.Items[index].SubItems[3].Text = auto.ToString();
            });
        }

        private void Midi_DeviceOutputStatusChanged(int index, eTodoMidiStatus status)
        {
            BeginInvoke((MethodInvoker)delegate()
            {
                this.lvMidiOutput.Items[index].SubItems[4].Text = status.ToString();
                if (status == eTodoMidiStatus.Disconnected || status == eTodoMidiStatus.Error)
                {
                    this.lvMidiOutput.Items[index].BackColor = Color.LightSalmon;
                    this.lvMidiOutput.Items[index].SubItems[2].Text = false.ToString();// "False";
                }
                else
                {
                    if (status == eTodoMidiStatus.Started)
                    {
                        this.lvMidiOutput.Items[index].BackColor = Color.LightGreen;
                        this.lvMidiOutput.Items[index].SubItems[2].Text = true.ToString();// "True";
                    }
                    else
                    {
                        this.lvMidiOutput.Items[index].BackColor = Color.White;
                        this.lvMidiOutput.Items[index].SubItems[2].Text = false.ToString(); //"False";
                    }
                }
            });
        }

        #region Setup List Views
        private void SetupListViewColumns()
        {
            //Set Columns
            this.lvMidiInput.Columns.Add("Index");
            this.lvMidiInput.Columns.Add("Device Name");
            this.lvMidiInput.Columns[1].Width = 300;
            this.lvMidiInput.Columns.Add("Enabled");
            this.lvMidiInput.Columns[2].Width = 150;
            this.lvMidiInput.Columns.Add("Auto Start");
            this.lvMidiInput.Columns[3].Width = 150;
            this.lvMidiInput.Columns.Add("Status");
            this.lvMidiInput.Columns[4].Width = 400;

            this.lvMidiOutput.Columns.Add("Index");
            this.lvMidiOutput.Columns.Add("Device Name");
            this.lvMidiOutput.Columns[1].Width = 300;
            this.lvMidiOutput.Columns.Add("Enabled");
            this.lvMidiOutput.Columns[2].Width = 150;
            this.lvMidiOutput.Columns.Add("Auto Start");
            this.lvMidiOutput.Columns[3].Width = 150;
            this.lvMidiOutput.Columns.Add("Status");
            this.lvMidiOutput.Columns[4].Width = 400;

            
            //Set editors
            this.chkenabled = new CheckBox();
            this.chkenabled.Text = "";
            this.chkenabled.Checked = false;
            this.Controls.Add(this.chkenabled);

            TextBox tb = new TextBox();
			tb.Visible = false;
			this.Controls.Add(tb);
            
            CheckBox cbMidiOutEnable = new CheckBox();
            cbMidiOutEnable.Checked = false;
            this.Controls.Add(cbMidiOutEnable);

            this.midiInputEditors.Add(tb);
            this.midiInputEditors.Add(tb);
            this.midiInputEditors.Add(chkenabled);
            this.midiInputEditors.Add(chkenabled);

            this.midiOutputEditors.Add(tb);
            this.midiOutputEditors.Add(tb);
            this.midiOutputEditors.Add(chkenabled);
            this.midiOutputEditors.Add(chkenabled);
        }
        #endregion

        #region Refresh Devices
        public void RefreshInputDevice()
        {
            this.lvMidiInput.Items.Clear();
            this.cmbClock.Items.Clear();
            

            for (int i = 0; i < InputDevice.DeviceCount; i++)
            {
                ListViewItem lv = new ListViewItem(i.ToString());
                this.lvMidiInput.Items.Add(lv);
                lv.SubItems.Add(InputDevice.GetDeviceCapabilities(i).name);
                lv.SubItems.Add(new ListViewItem.ListViewSubItem(lv, "False"));
                lv.SubItems.Add(new ListViewItem.ListViewSubItem(lv, "False"));
                lv.SubItems.Add(new ListViewItem.ListViewSubItem(lv, "Connected"));
                this.cmbClock.Items.Add(InputDevice.GetDeviceCapabilities(i).name);
            }
        }

        public void RefreshOutputDevice()
        {
            this.lvMidiOutput.Items.Clear();

            for (int i = 0; i < OutputDevice.DeviceCount; i++)
            {
                ListViewItem lv = new ListViewItem(i.ToString());
                this.lvMidiOutput.Items.Add(lv);

                lv.SubItems.Add(OutputDevice.GetDeviceCapabilities(i).name);
                lv.SubItems.Add(new ListViewItem.ListViewSubItem(lv, "False"));
                lv.SubItems.Add(new ListViewItem.ListViewSubItem(lv, "False"));
                lv.SubItems.Add(new ListViewItem.ListViewSubItem(lv, "Connected"));
            }
        }
        #endregion

        #region Manage Midi Input
        private void MidiInput_SubItemClicked(object sender, ListViewEx.SubItemEventArgs e)
        {
            if (e.SubItem == 2 || e.SubItem == 3)
            {
                this.chkenabled.Checked = BoolExtension.ParseEnglish(e.Item.SubItems[e.SubItem].Text);
                this.lvMidiInput.StartEditing(this.midiInputEditors[e.SubItem], e.Item, e.SubItem);
            }
        }

        private void MidiInput_SubItemEndEditing(object sender, ListViewEx.SubItemEndEditingEventArgs e)
        {
            if (e.SubItem == 2)
            {
                e.DisplayText = this.chkenabled.Checked.ToString();
                e.Item.SubItems[e.SubItem].Text = this.chkenabled.Checked.ToString();
                this.engine.Midi.SetInputEnabled(int.Parse(e.Item.Text), BoolExtension.ParseEnglish(e.DisplayText));
            }
            if (e.SubItem ==3)
            {
                e.DisplayText = this.chkenabled.Checked.ToString();
                e.Item.SubItems[e.SubItem].Text = this.chkenabled.Checked.ToString();
                this.engine.Midi.SetInputAutoStart(int.Parse(e.Item.Text), BoolExtension.ParseEnglish(e.DisplayText));
            }
        }
        #endregion

        #region Manage Midi Output
        private void MidiOutput_SubItemClicked(object sender, ListViewEx.SubItemEventArgs e)
        {
            if (e.SubItem == 2 || e.SubItem == 3)
            {
                this.chkenabled.Checked = BoolExtension.ParseEnglish(e.Item.SubItems[e.SubItem].Text);
                this.lvMidiOutput.StartEditing(this.midiOutputEditors[e.SubItem], e.Item, e.SubItem);
            }
        }

        private void MidiOutput_SubItemEndEditing(object sender, ListViewEx.SubItemEndEditingEventArgs e)
        {
            if (e.SubItem == 2)
            {
                e.DisplayText = this.chkenabled.Checked.ToString();
                e.Item.SubItems[e.SubItem].Text = this.chkenabled.Checked.ToString();
                this.engine.Midi.SetOutputEnabled(int.Parse(e.Item.Text), BoolExtension.ParseEnglish(e.DisplayText));
            }
            if (e.SubItem == 3)
            {
                e.DisplayText = this.chkenabled.Checked.ToString();
                e.Item.SubItems[e.SubItem].Text = this.chkenabled.Checked.ToString();
                this.engine.Midi.SetOutputAutoStart(int.Parse(e.Item.Text), BoolExtension.ParseEnglish(e.DisplayText));
            }
        }
        #endregion

        private void chkenableclock_CheckedChanged(object sender, EventArgs e)
        {
            this.cmbClock.Enabled = !chkenableclock.Checked;

            if (this.chkenableclock.Checked)
            {
                this.engine.Midi.SetClockDevice(this.cmbClock.SelectedIndex);
            }
            else
            {
                this.engine.Midi.SetClockDevice(-1);
                this.lbltime.Text = "N/A";
            }
        }
    }
}
