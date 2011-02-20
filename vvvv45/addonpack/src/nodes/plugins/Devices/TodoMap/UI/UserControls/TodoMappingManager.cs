using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace VVVV.TodoMap.UI.UserControls
{
    public partial class TodoMappingManager : UserControl
    {
        public TodoMappingManager()
        {
            InitializeComponent();

            this.SetupInputColumns();
            this.SetupOutputColumns();
            this.SetupVariablesColumns();
        }

        #region Setup all columns
        private void SetupVariablesColumns()
        {
            this.lvVariables.Columns.Add("Category");
            this.lvVariables.Columns.Add("Name");
            this.lvVariables.Columns.Add("Default Value");
            this.lvVariables.Columns.Add("Min Value");
            this.lvVariables.Columns.Add("Max Value");
            this.lvVariables.Columns.Add("Tweener");
            this.lvVariables.Columns.Add("Ease Mode");
        }

        private void SetupInputColumns()
        {
            this.lvInputs.Columns.Add("Control Type");
            this.lvInputs.Columns.Add("Control Data");
            this.lvInputs.Columns.Add("TakeOver Mode");
            this.lvInputs.Columns.Add("Allow Feedback");
        }

        private void SetupOutputColumns()
        {
            this.lvOutputs.Columns.Add("Control Type");
            this.lvOutputs.Columns.Add("Control Data");
        }
        #endregion

        #region When variable registered
        /*void FEngine_VariableRegistered(AbstractTodoInput input)
        {
            BeginInvoke((MethodInvoker)delegate()
            {
                ListViewItem lv = this.FRows[input.Variable.Name];

                lv.SubItems[1].Text = input.InputType;
                lv.SubItems[2].Text = input.InputMap;

            });
        }*/
        #endregion
    }
}
