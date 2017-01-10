using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using VVVV.TodoMap.Lib;
using VVVV.TodoMap.Lib.Modules.Osc;

namespace VVVV.TodoMap.UI.UserControls.Osc
{
    public partial class TodoOscManager : UserControl
    {
        private TodoEngine engine;

        public TodoOscManager()
        {
            InitializeComponent();
        }

        public TodoEngine Engine
        {
            set
            {
                this.engine = value;
                this.engine.Osc.OscInputStatusChanged += new OscStatusChangedDelegate(Osc_OscInputStatusChanged);
                this.engine.Osc.OscOutputStatusChanged += new OscOutputStatusChangedDelegate(Osc_OscOutputStatusChanged);
                this.Reset();
            }
        }

        public void Reset()
        {
            this.tbInputPort.Text = this.engine.Osc.LocalPort.ToString();

            this.chkAutoStartIn.Checked = this.engine.Osc.AutoStartInput;
            this.chkEnableIn.Checked = this.engine.Osc.LocalStatus == eTodoOscStatus.Started;
            this.lblstatus.Text = "Status: " + this.engine.Osc.LocalStatus.ToString();
            
            
            this.tbOutput.Text = this.engine.Osc.RemotePort.ToString();

            this.chkOutput.Checked = this.engine.Osc.RemoteEnabled;        
            this.chkAutoStartOut.Checked = this.engine.Osc.AutoStartOutput;
        }

        void Osc_OscOutputStatusChanged(bool enabled)
        {
            this.Reset();
        }

        void Osc_OscInputStatusChanged(eTodoOscStatus status)
        {
            this.Reset();
        }

        private void chkEnableIn_CheckedChanged(object sender, EventArgs e)
        {
            if (chkEnableIn.Checked)
            {
                int port;
                if (int.TryParse(this.tbInputPort.Text, out port))
                {
                    this.engine.Osc.LocalPort = port;
                    this.engine.Osc.SetEnabled(true);
                }
                else
                {
                    this.lblstatus.Text = "Status: Invalid Port Number";
                    this.chkEnableIn.Checked = false;
                }
            }
            else
            {
                this.engine.Osc.SetEnabled(false);
            }

            this.tbInputPort.ReadOnly = chkEnableIn.Checked;

        }

        private void chkOutput_CheckedChanged(object sender, EventArgs e)
        {
            if (chkOutput.Checked)
            {
                int port;
                if (int.TryParse(this.tbOutput.Text, out port))
                {
                    this.engine.Osc.RemotePort = port;
                    this.engine.Osc.SetOutputEnabled(true);
                }
                else
                {
                    this.chkOutput.Checked = false;
                }
            }
            else
            {
                this.engine.Osc.SetOutputEnabled(false);
            }

            this.tbOutput.ReadOnly = chkOutput.Checked;
        }

        private void chkAutoStartIn_CheckedChanged(object sender, EventArgs e)
        {
            this.engine.Osc.AutoStartInput = chkAutoStartIn.Checked;
        }

        private void chkAutoStartOut_CheckedChanged(object sender, EventArgs e)
        {
            this.engine.Osc.AutoStartOutput = chkAutoStartOut.Checked;
        }

    }
}
