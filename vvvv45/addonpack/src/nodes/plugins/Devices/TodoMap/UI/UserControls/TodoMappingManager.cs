using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using VVVV.TodoMap.Lib;
using VVVV.TodoMap.UI.Controls;
using VVVV.TodoMap.Lib.Engine.Filters;

namespace VVVV.TodoMap.UI.UserControls
{
    public partial class TodoMappingManager : UserControl
    {
        private ListViewEx.ListViewEx lvInputs;
        private ListViewEx.ListViewEx lvOutputs;
        private ListViewEx.ListViewEx lvVariables;
        private VariableFilterCtrl ucFilter;

        private List<Control> varEditors = new List<Control>();
        private List<Control> inputEditors = new List<Control>();

        private TodoEngine engine;

        public TodoMappingManager()
        {
            InitializeComponent();


            this.lvInputs = new ListViewEx.ListViewEx();
            this.lvOutputs = new ListViewEx.ListViewEx();
            this.lvVariables = new ListViewEx.ListViewEx();

            this.ucFilter = new VariableFilterCtrl();

            this.lvInputs.Dock = DockStyle.Fill;
            this.lvOutputs.Dock = DockStyle.Fill;
            this.lvVariables.Dock = DockStyle.Fill;
            this.ucFilter.Dock = DockStyle.Fill;

            this.lvInputs.View = System.Windows.Forms.View.Details;
            this.lvOutputs.View = System.Windows.Forms.View.Details;
            this.lvVariables.View = System.Windows.Forms.View.Details;

            this.lvInputs.DoubleClickActivation = true;
            this.lvOutputs.DoubleClickActivation = true;
            this.lvVariables.DoubleClickActivation = true;

            this.lvInputs.MultiSelect = false;

            this.layoutInputs.Controls.Add(this.lvInputs,0,1);
            this.layoutOutputs.Controls.Add(this.lvOutputs,0,1);
            //this..Controls.Add(this.lvVariables);
            this.tblVarLayout.Controls.Add(this.ucFilter, 0, 0);
            this.tblVarLayout.Controls.Add(this.lvVariables, 0, 1);

            this.SetupInputColumns();
            this.SetupOutputColumns();
            this.SetupVariablesColumns();

            this.lvVariables.ItemSelectionChanged += lvVariables_ItemSelectionChanged;
            this.lvVariables.SubItemClicked += this.lvVariables_SubItemClicked;
            this.lvVariables.SubItemEndEditing += lvVariables_SubItemEndEditing;
            this.lvVariables.SubItemEditComplete += lvVariables_SubItemEditComplete;
            this.lvVariables.KeyDown += lvVariables_KeyDown;

            this.lvInputs.ItemSelectionChanged += lvInputs_ItemSelectionChanged;
            this.lvInputs.SubItemClicked += this.lvInputs_SubItemClicked;
            this.lvInputs.SubItemEndEditing += lvInputs_SubItemEndEditing;
            this.lvInputs.KeyDown += lvInputs_KeyDown;

            this.ucFilter.OnChange += new EventHandler(ucFilter_OnChange);
        }

        void ucFilter_OnChange(object sender, EventArgs e)
        {
            lvVariables.Items.Clear();
            foreach (TodoVariable var in this.ucFilter.Variables)
            {
                this.AddVariable(var);
            }
        }

        public TodoEngine Engine
        {
            set
            {
                this.engine = value;
                this.engine.VariableRegistered += this.Engine_VariableRegistered;
                this.engine.VariableMappingChanged += engine_VariableMappingUpdated;
                this.engine.VariableValueChanged += engine_VariableValueChanged;
                this.engine.VariableDeleted += engine_VariableDeleted;
                this.engine.VariableChanged += engine_VariableChanged;

                this.ucFilter.Filter = new TodoCategoryFilter(this.engine);
                this.ucFilter.Engine = this.engine;
            }
        }

        private void engine_VariableChanged(TodoVariable var, bool gui)
        {
            if (!gui)
            {
                BeginInvoke((MethodInvoker)delegate()
                {
                    int idx = this.engine.Variables.IndexOf(var);
                    ListViewItem lv = this.lvVariables.Items[idx];
                    lv.SubItems[0].Text = var.Category;
                    lv.SubItems[2].Text = var.Default.ToString();
                    lv.SubItems[3].Text = var.Mapper.MinValue.ToString();
                    lv.SubItems[4].Text = var.Mapper.MaxValue.ToString();
                    lv.SubItems[6].Text = var.Mapper.TweenMode.ToString();
                    lv.SubItems[7].Text = var.Mapper.EaseMode.ToString();
                    lv.SubItems[8].Text = var.TakeOverMode.ToString();
                    lv.SubItems[9].Text = var.AllowFeedBack.ToString();
                });
            }
        }

        private void engine_VariableValueChanged(string name, double newvalue)
        {
            if (this.engine.SelectedVariable != null)
            {
                if (this.engine.SelectedVariable.Name == name)
                {

                    foreach (ListViewItem lv in this.lvInputs.Items)
                    {
                        try
                        {
                            if (lv.Tag == (object)this.engine.SelectedVariable.LastActiveControl)
                            {
                                lv.BackColor = Color.LightGreen;
                            }
                            else
                            {
                                lv.BackColor = Color.White;
                            }
                        }
                        catch
                        {

                        }
                    }
                }
            }
        }

        #region Variables
        private void Engine_VariableRegistered(TodoVariable var, bool gui)
        {
            BeginInvoke((MethodInvoker)delegate()
            {
                //this.AddVariable(var);
                this.ucFilter.Reset();
            });
        }

        

        private void AddVariable(TodoVariable var)
        {
            ListViewItem lv = this.lvVariables.Items.Add(var.Category);
            lv.Tag = var.Name;
            lv.SubItems.Add(var.Name);
            lv.SubItems.Add(var.Default.ToString());
            lv.SubItems.Add(var.Mapper.MinValue.ToString());
            lv.SubItems.Add(var.Mapper.MaxValue.ToString());
            lv.SubItems.Add(var.Mapper.Reverse.ToStringEnglish());
            lv.SubItems.Add(var.Mapper.TweenMode.ToString());
            lv.SubItems.Add(var.Mapper.EaseMode.ToString());
            lv.SubItems.Add(var.TakeOverMode.ToString());
            lv.SubItems.Add(var.AllowFeedBack.ToStringEnglish());

            //Preserve selected var
            //if (this.lvVariables.a
        }

        void engine_VariableDeleted(TodoVariable var,bool gui)
        {
            BeginInvoke((MethodInvoker)delegate()
            {
                this.ucFilter.Reset();
            });
        }



        void lvVariables_KeyDown(object sender, KeyEventArgs e)
        {
            if (this.lvVariables.SelectedItems.Count > 0 && e.KeyCode == Keys.Delete)
            {
                string varname = this.lvVariables.SelectedItems[0].Tag.ToString();
                this.engine.DeleteVariable(this.engine.GetVariableByName(varname),true);
                
                //Clear anyway as it's the selected var
                this.lvInputs.Items.Clear();
            }
        }

        void lvInputs_KeyDown(object sender, KeyEventArgs e)
        {
            if (this.lvInputs.SelectedItems.Count > 0 && e.KeyCode == Keys.Delete)
            {
                ListViewItem lvi = this.lvInputs.SelectedItems[0];
                this.engine.RemoveInput(lvi.Tag as AbstractTodoInput);
                this.lvInputs.Items.Remove(lvi);
            }

            this.ucFilter.Reset();
        }

        void engine_VariableMappingUpdated(AbstractTodoInput input,bool isnew)
        {
            BeginInvoke((MethodInvoker)delegate()
            {
                this.lvInputs.Select();
                if (isnew)
                {
                    ListViewItem lv = this.lvInputs.Items.Add(input.InputType);
                    lv.SubItems.Add(input.Device);
                    lv.SubItems.Add(input.InputMap);
                    lv.SubItems.Add(input.TakeOverMode.ToString());
                    lv.SubItems.Add(input.FeedBackMode.ToString());
                    lv.Selected = true;
                    lv.Tag = input;
                }
                else
                {
                    int idx = this.engine.SelectedVariable.Inputs.IndexOf(input);
                    ListViewItem lv = this.lvInputs.Items[idx];
                    lv.SubItems[1].Text = input.Device;
                    lv.SubItems[2].Text = input.InputMap;
                    lv.SubItems[3].Text = input.TakeOverMode.ToString();
                    lv.SubItems[4].Text = input.FeedBackMode.ToString();
                    lv.Selected = true;
                    lv.Tag = input;
                }
                
            });
        }

        private void ResetMapping(TodoVariable var)
        {
            BeginInvoke((MethodInvoker)delegate()
            {
                this.lvInputs.Items.Clear();
                foreach (AbstractTodoInput input in var.Inputs)
                {
                    ListViewItem lv = this.lvInputs.Items.Add(input.InputType);
                    lv.SubItems.Add(input.Device);
                    lv.SubItems.Add(input.InputMap);
                    lv.SubItems.Add(input.TakeOverMode.ToString());
                    lv.SubItems.Add(input.FeedBackMode.ToString());
                    lv.Tag = input;
                    if (input == var.LastActiveControl)
                    {
                        lv.BackColor = Color.LightGreen;
                    }
                    if (input == this.engine.SelectedInput)
                    {
                        lv.Selected = true;
                    }
                }
            });
        }

        private void lvVariables_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
        {
            if (this.lvVariables.SelectedIndices.Count > 0)
            {
                this.engine.SelectInput(-1);
                this.engine.SelectVariable(this.lvVariables.SelectedItems[0].SubItems[1].Text);
                this.ResetMapping(this.engine.SelectedVariable);
                this.lvInputs.Tag = this.engine.SelectedVariable;
                
            }
            else
            {
                this.engine.DeselectVariable();
            }
        }

        void lvVariables_SubItemClicked(object sender, ListViewEx.SubItemEventArgs e)
        {
            if (e.SubItem >= 2 || e.SubItem == 0)
            {
                this.lvVariables.StartEditing(this.varEditors[e.SubItem], e.Item, e.SubItem);
            }
        }

        void lvVariables_SubItemEndEditing(object sender, ListViewEx.SubItemEndEditingEventArgs e)
        {
            TodoVariable tv = this.engine.GetVariableByName(e.Item.SubItems[1].Text);
            TodoTweenMapper mapper = tv.Mapper;
            if (e.SubItem == 0)
            {
                tv.Category = e.DisplayText;
            }
            if (e.SubItem == 2)
            {
                try { tv.Default = Convert.ToDouble(e.DisplayText);} catch { e.Cancel = true; }
            }
            if (e.SubItem == 3)
            {
              try { mapper.MinValue = Convert.ToDouble(e.DisplayText); } catch { e.Cancel = true; }
            }
            if (e.SubItem == 4)
            {
                try { mapper.MaxValue = Convert.ToDouble(e.DisplayText); } catch { e.Cancel = true; }
            }
            if (e.SubItem == 5)
            {
                tv.Mapper.Reverse = BoolExtension.ParseEnglish(e.DisplayText);
            }
            if (e.SubItem == 6)
            {
                mapper.TweenMode = (eTweenMode)Enum.Parse(typeof(eTweenMode), e.DisplayText);
            }
            if (e.SubItem == 7)
            {
                mapper.EaseMode = (eTweenEaseMode)Enum.Parse(typeof(eTweenEaseMode), e.DisplayText);
            }
            if (e.SubItem == 8)
            {
                tv.TakeOverMode = (eTodoGlobalTakeOverMode)Enum.Parse(typeof(eTodoGlobalTakeOverMode), e.DisplayText);
            }
            if (e.SubItem == 9)
            {
                tv.AllowFeedBack = BoolExtension.ParseEnglish(e.DisplayText);
            }
            tv.MarkForUpdate(true);
            
        }

        void lvVariables_SubItemEditComplete(object sender, int subitemindex)
        {
            if (subitemindex == 0) { this.ucFilter.Reset(); }
        }
        #endregion


        #region Inputs
        void lvInputs_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
        {
            if (this.lvInputs.SelectedIndices.Count > 0)
            {
                this.engine.SelectVariable(((TodoVariable)this.lvInputs.Tag).Name);
                this.engine.SelectInput(this.lvInputs.SelectedIndices[0]);
            }
            else
            {
                this.engine.SelectInput(-1);
            }
            this.SetVariablesColor();
        }

        void lvInputs_SubItemClicked(object sender, ListViewEx.SubItemEventArgs e)
        {
            if (e.SubItem >= 3)
            {
                try
                {
                    this.lvInputs.StartEditing(this.inputEditors[e.SubItem], e.Item, e.SubItem);
                }
                catch { }
            }
        }

        void lvInputs_SubItemEndEditing(object sender, ListViewEx.SubItemEndEditingEventArgs e)
        {
            AbstractTodoInput input = this.engine.SelectedInput;

            if (e.SubItem == 3)
            {
                input.TakeOverMode = (eTodoLocalTakeOverMode)Enum.Parse(typeof(eTodoLocalTakeOverMode), e.DisplayText);
            }
            if (e.SubItem == 4)
            {
                input.FeedBackMode = (eTodoLocalFeedBackMode)Enum.Parse(typeof(eTodoLocalFeedBackMode), e.DisplayText);
            }
        }
        #endregion

        public void Reset()
        {
            this.lvInputs.Items.Clear();
            this.lvOutputs.Items.Clear();
            this.lvVariables.Items.Clear();
        }

        public void ResetMappings()
        {
            this.lvInputs.Items.Clear();
            this.lvOutputs.Items.Clear();
        }

        private void SetVariablesColor()
        {
            foreach (ListViewItem item in this.lvVariables.Items)
            {
                if (item.Tag.ToString() == this.engine.SelectedVariable.Name)
                {
                    item.BackColor = Color.LightGreen;
                }
                else
                {
                    item.BackColor = Color.White;
                }
            }
        }


        #region Setup all columns
        private void SetupVariablesColumns()
        {
            this.lvVariables.Columns.Add("Category");
            this.lvVariables.Columns.Add("Name");
            this.lvVariables.Columns.Add("Default");
            this.lvVariables.Columns.Add("Minimum");
            this.lvVariables.Columns.Add("Maximum");
            this.lvVariables.Columns.Add("Reverse");
            this.lvVariables.Columns.Add("Tweener");
            this.lvVariables.Columns.Add("Ease");
            this.lvVariables.Columns.Add("TakeOver");
            this.lvVariables.Columns.Add("FeedBack");


            //Set variable editors
            TextBox tb = new TextBox();
            tb.Visible = false;
            this.Controls.Add(tb);

            ComboBox cbtruefalse = new ComboBox();
            cbtruefalse.Items.Add("False");
            cbtruefalse.Items.Add("True");
            cbtruefalse.DropDownStyle = ComboBoxStyle.DropDownList;
            this.Controls.Add(cbtruefalse);

            ComboBox cbtw = new ComboBox();
            cbtw.DropDownStyle = ComboBoxStyle.DropDownList;
            cbtw.Items.AddRange(Enum.GetNames(typeof(eTweenMode)));
            this.Controls.Add(cbtw);

            ComboBox cbease = new ComboBox();
            cbease.DropDownStyle = ComboBoxStyle.DropDownList;
            cbease.Items.AddRange(Enum.GetNames(typeof(eTweenEaseMode)));
            this.Controls.Add(cbease);

            ComboBox cbto = new ComboBox();
            cbto.DropDownStyle = ComboBoxStyle.DropDownList;
            cbto.Items.AddRange(Enum.GetNames(typeof(eTodoGlobalTakeOverMode)));
            this.Controls.Add(cbto);

            this.varEditors.Add(tb); //Category
            this.varEditors.Add(tb); // Name
            this.varEditors.Add(tb); // Default Value
            this.varEditors.Add(tb); //Min Value
            this.varEditors.Add(tb); //Max value
            this.varEditors.Add(cbtruefalse); //Reverse
            this.varEditors.Add(cbtw); //Tweener
            this.varEditors.Add(cbease); //Ease Mode
            this.varEditors.Add(cbto); //Take Over
            this.varEditors.Add(cbtruefalse); //FeedBack

        }

        private void SetupInputColumns()
        {          
            this.lvInputs.Columns.Add("Control Type");
            this.lvInputs.Columns.Add("Device");
            this.lvInputs.Columns.Add("Control Data");
            this.lvInputs.Columns.Add("TakeOver");
            this.lvInputs.Columns.Add("Feedback");

            //Set variable editors
            TextBox tb = new TextBox();
            tb.Visible = false;
            this.Controls.Add(tb);

            ComboBox cbto = new ComboBox();
            cbto.DropDownStyle = ComboBoxStyle.DropDownList;
            cbto.Items.AddRange(Enum.GetNames(typeof(eTodoLocalTakeOverMode)));
            this.Controls.Add(cbto);

            ComboBox cbfeedback = new ComboBox();
            cbfeedback.Items.AddRange(Enum.GetNames(typeof(eTodoLocalFeedBackMode)));
            cbfeedback.DropDownStyle = ComboBoxStyle.DropDownList;
            this.Controls.Add(cbfeedback);

            this.inputEditors.Add(tb);
            this.inputEditors.Add(tb);
            this.inputEditors.Add(tb);
            this.inputEditors.Add(cbto);
            this.inputEditors.Add(cbfeedback);


        }

        private void SetupOutputColumns()
        {
            this.lvOutputs.Columns.Add("Device");
            this.lvOutputs.Columns.Add("Control Type");
            this.lvOutputs.Columns.Add("Control Data");
            this.lvOutputs.Columns.Add("Normalized");
            this.lvOutputs.Columns.Add("Map Mode");
        }
        #endregion

        private void lblLearnMode_Click(object sender, EventArgs e)
        {
            this.engine.LearnMode = !this.engine.LearnMode;
            this.LearnModeUpdated();
        }

        public void LearnModeUpdated()
        {
            if (this.engine.LearnMode) { this.lblLearnMode.BackColor = Color.LightGreen; }
            else { this.lblLearnMode.BackColor = Color.White; }
        }

        private void label1_Click(object sender, EventArgs e)
        {
            //this.engine.LearnMode = !this.engine.LearnMode;
            //if (this.engine.LearnMode) { this.lblLearnMode.BackColor = Color.LightGreen; }
            //else { this.lblLearnMode.BackColor = Color.White; }
        }

        private void lbldevall_Click(object sender, EventArgs e)
        {
            this.engine.AnyDevice = !this.engine.AnyDevice;
            if (this.engine.AnyDevice) 
            {
                this.lbldevall.BackColor = Color.White;
                this.lbldevall.Text = "Any Device";
 
            }
            else 
            {
                this.lbldevall.BackColor = Color.LightGreen;
                this.lbldevall.Text = "Source Device";
            }
        }

    }
}
