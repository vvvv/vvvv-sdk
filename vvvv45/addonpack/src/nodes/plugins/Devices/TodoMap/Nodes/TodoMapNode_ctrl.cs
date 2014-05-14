using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using VVVV.TodoMap.UI.UserControls;
using VVVV.TodoMap.Lib;
using System.IO;
using VVVV.TodoMap.Lib.Persist;
using VVVV.TodoMap.UI.UserControls.Osc;

namespace VVVV.TodoMap.Nodes
{
    public partial class TodoMapNode : UserControl
    {
        private TodoDeviceManagerCtrl ucDeviceManager;
        private TodoMappingManager ucMappingManager;
        private TodoOscManager ucOscManager;
        private TodoLoggerCtrl ucLogger;
        //private TodoTreeView ctrlTree;

        public TodoMapNode()
        {
            InitializeComponent();

            this.ucMappingManager = new TodoMappingManager();
            this.ucMappingManager.Dock = DockStyle.Fill;
            this.tabMapper.Controls.Add(this.ucMappingManager);
            

            this.ucDeviceManager = new TodoDeviceManagerCtrl();
            this.ucDeviceManager.Dock = DockStyle.Fill;
            this.tabMidi.Controls.Add(this.ucDeviceManager);

            this.ucOscManager = new TodoOscManager();
            this.ucOscManager.Dock = DockStyle.Fill;
            this.tabOsc.Controls.Add(this.ucOscManager);

            this.ucLogger = new TodoLoggerCtrl();
            this.ucLogger.Dock = DockStyle.Fill;
            this.tabLog.Controls.Add(this.ucLogger);

           /* this.ctrlTree = new TodoTreeView();
            this.ctrlTree.Dock = DockStyle.Fill;
            this.grpTree.Controls.Add(ctrlTree);*/
            //this.tabTreeVar.Controls.Add(ctrlTree);

            this.FEngine = new TodoEngine();
            //this.FEngine.Osc.SetEnabled(true);


            this.ucDeviceManager.Engine = this.FEngine;
            this.ucMappingManager.Engine = this.FEngine;
            this.ucOscManager.Engine = this.FEngine;
            //this.ctrlTree.Initialize(this.FEngine,this.treelist);

            //this.ctrlTree.OscNodeSelected += new EventHandler(ctrlTree_OscNodeSelected);
           // this.ctrlTree.MidiNodeSelected += new EventHandler(ctrlTree_MidiNodeSelected);

        }

        void ctrlTree_MidiNodeSelected(object sender, EventArgs e)
        {
            if (this.grpProps.Controls.Count > 0)
            {
                this.grpProps.Controls.RemoveAt(0);
            }
            this.grpProps.Controls.Add(this.ucDeviceManager);
        }

        void ctrlTree_OscNodeSelected(object sender, EventArgs e)
        {
            if (this.grpProps.Controls.Count > 0)
            {
                this.grpProps.Controls.RemoveAt(0);
            }
            this.grpProps.Controls.Add(this.ucOscManager);
        }










    }
}
