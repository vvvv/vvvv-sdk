using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using VVVV.TodoMap.UI.UserControls;

namespace VVVV.TodoMap.Nodes
{
    public partial class TodoMapNode : UserControl
    {
        private TodoDeviceManagerCtrl ucDeviceManager;
        private TodoMappingManager ucMappingManager;

        public TodoMapNode()
        {
            InitializeComponent();

            this.ucMappingManager = new TodoMappingManager();
            this.ucMappingManager.Dock = DockStyle.Fill;
            this.tabMapper.Controls.Add(this.ucMappingManager);
            

            this.ucDeviceManager = new TodoDeviceManagerCtrl();
            this.ucDeviceManager.Dock = DockStyle.Fill;
            this.tabDevices.Controls.Add(this.ucDeviceManager);


        }
    }
}
