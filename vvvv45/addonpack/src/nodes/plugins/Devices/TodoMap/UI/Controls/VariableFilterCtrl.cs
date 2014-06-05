using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using VVVV.TodoMap.Lib.Engine.Filters;
using VVVV.TodoMap.Lib;
using VVVV.TodoMap.Lib.Persist;
using System.IO;
using System.Threading;

namespace VVVV.TodoMap.UI.Controls
{
    public partial class VariableFilterCtrl : UserControl
    {
        private TodoCategoryFilter filter;
        private TodoEngine engine;

        public event EventHandler OnChange;

        public TodoEngine Engine { set { this.engine = value; } }

        public VariableFilterCtrl()
        {
            InitializeComponent();

            this.engine = engine;
            this.cmbCatFilter.SelectedIndexChanged += new EventHandler(cmbCatFilter_SelectedIndexChanged);
        }

        void cmbCatFilter_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (this.OnChange != null) { this.OnChange(this, e); }
        }

        public TodoCategoryFilter Filter
        {
            set { this.filter = value; }
        }

        public List<TodoVariable> Variables
        {
            get
            {
                if (this.cmbCatFilter.SelectedIndex == 0)
                {
                    return this.filter.AllVariables();
                }
                else
                {
                    return this.filter.Filter(this.cmbCatFilter.SelectedItem.ToString());
                }
            }
        }


        public void Reset()
        {
            this.cmbCatFilter.Items.Clear();
            this.cmbCatFilter.Items.Add("-- All --");
            this.cmbCatFilter.Items.AddRange(this.filter.Categories.ToArray());
            this.cmbCatFilter.SelectedIndex = 0;
        }

        private void lblsave_Click(object sender, EventArgs e)
        {
            this.lblsave.BackColor = Color.LightGreen;
            try
            {
                StreamWriter sw = new StreamWriter(this.engine.SavePath);
                sw.Write(TodoXmlWrapper.Persist(this.engine));
                sw.Close();
            }
            catch
            {

            }
            this.lblsave.BackColor = Color.White;
        }

        private void lblnewVar_Click(object sender, EventArgs e)
        {
            if (this.tbvarname.Text.Trim().Length > 0)
            {
                string cat = this.cmbCatFilter.Text.Trim().Length == 0 ? "Global" : this.cmbCatFilter.Text.Trim();
                TodoVariable var = new TodoVariable(this.tbvarname.Text);
                var.Category = cat;

                this.engine.RegisterVariable(var,true);  
            }           
        }

        private void label1_Click(object sender, EventArgs e)
        {
            
        }
    }
}
