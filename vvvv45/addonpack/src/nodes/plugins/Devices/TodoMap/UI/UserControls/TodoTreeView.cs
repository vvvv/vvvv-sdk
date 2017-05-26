using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using VVVV.TodoMap.Lib;
using Sanford.Multimedia.Midi;

namespace VVVV.TodoMap.UI.UserControls
{
   

    public partial class TodoTreeView : TreeView
    {
        private TodoEngine engine;

        private TreeNode rootcatnode;

        private TreeNode rootdevicenode;

        private TreeNode rootmidinode;
        private TreeNode rootmidiinputnode;
        private TreeNode rootmidioutputnode;
        private TreeNode rootmidiclocknode;

        private TreeNode rootoscnode;

        private Dictionary<TodoVariable, TreeNode> vardic = new Dictionary<TodoVariable, TreeNode>();

        public event EventHandler OscNodeSelected;
        public event EventHandler MidiNodeSelected;

        public TodoTreeView()
        {
            InitializeComponent();
        }

        public void Initialize(TodoEngine engine, ImageList iml)
        {
            this.ImageList = iml;
            this.CheckBoxes = true;
            this.engine = engine;

            this.ItemDrag += new ItemDragEventHandler(TodoTreeView_ItemDrag);
            this.DragOver += new DragEventHandler(TodoTreeView_DragOver);
            this.DragEnter += new DragEventHandler(TodoTreeView_DragEnter);
            this.NodeMouseClick += new TreeNodeMouseClickEventHandler(TodoTreeView_NodeMouseClick);


            this.engine.VariableRegistered += new TodoVariableEventDelegate(engine_VariableRegistered);
            this.engine.VariableMappingChanged += new TodoInputChangedEventDelegate(engine_VariableMappingChanged);
            this.CreateBaseTree();

            foreach (TodoVariable var in this.engine.Variables)
            {
                this.AddVariable(var);
            }

            
        }

        void TodoTreeView_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            if (e.Node == this.rootoscnode)
            {
                if (this.OscNodeSelected != null)
                {
                    this.OscNodeSelected(this, new EventArgs());
                }

            }
            else if (e.Node == this.rootmidinode)
            {
                if (this.MidiNodeSelected != null)
                {
                    this.MidiNodeSelected(this, new EventArgs());
                }
            }

            if (e.Node.Tag != null)
            {
                TodoVariable var = (TodoVariable)e.Node.Tag;

                //this.props.PropertyTabs[0].
            }
        }

        void TodoTreeView_DragOver(object sender, DragEventArgs e)
        {

            Console.WriteLine("Test");
        }

        void TodoTreeView_DragEnter(object sender, DragEventArgs e)
        {
            Console.WriteLine("Test");
            /*TreeNode node = (TreeNode)e.Data;

            if (node.Tag is TodoVariable)
            {
                this.DoDragDrop(node, DragDropEffects.All);
            }*/
            //e.
        }

        void TodoTreeView_ItemDrag(object sender, ItemDragEventArgs e)
        {
            TreeNode node = (TreeNode)e.Item;
            

            if (node.Tag is TodoVariable)
            {
                this.DoDragDrop(e.Item, DragDropEffects.Move);
            }
        }


        void engine_VariableMappingChanged(AbstractTodoInput input, bool isnew)
        {
            this.BuildMappings(input.Variable);     
        }

        void engine_VariableRegistered(TodoVariable var, bool gui)
        {
            this.AddVariable(var);
        }

        protected override void OnPaint(PaintEventArgs pe)
        {
            base.OnPaint(pe);
        }

        private void CreateBaseTree()
        {

            this.rootdevicenode = this.Nodes.Add("Devices");
            this.rootdevicenode.ImageIndex = 4;
            this.rootdevicenode.SelectedImageIndex = 4;

            this.rootcatnode = this.Nodes.Add("Categories");
            this.rootcatnode.ImageIndex = 0;
            this.rootcatnode.SelectedImageIndex = 0;

            this.rootoscnode = rootdevicenode.Nodes.Add("Osc");
            this.rootoscnode.ImageIndex = 3;
            this.rootoscnode.SelectedImageIndex = 3;
            

            this.rootmidinode = rootdevicenode.Nodes.Add("Midi");
            this.rootmidinode.ImageIndex = 3;
            this.rootmidinode.SelectedImageIndex = 3;

            this.rootmidiinputnode = rootmidinode.Nodes.Add("Inputs");
            this.rootmidiinputnode.ImageIndex = 3;
            this.rootmidiinputnode.SelectedImageIndex = 3;


            for (int j = 0; j < InputDevice.DeviceCount; j++)
            {
                TreeNode node = this.rootmidiinputnode.Nodes.Add(InputDevice.GetDeviceCapabilities(j).name);
                node.ImageIndex = 2;
                node.SelectedImageIndex = 2;
            }

            this.rootmidioutputnode = rootmidinode.Nodes.Add("Outputs");
            this.rootmidioutputnode.ImageIndex = 3;
            this.rootmidioutputnode.SelectedImageIndex = 3;


            for (int j = 0; j < OutputDevice.DeviceCount; j++)
            {
                TreeNode node = this.rootmidioutputnode.Nodes.Add(OutputDevice.GetDeviceCapabilities(j).name);
                node.ImageIndex = 2;
                node.SelectedImageIndex = 2;   
            }

            /*this.rootmidiclocknode.Nodes.Add("Clock");
            this.rootmidiclocknode.ImageIndex = 3;
            this.rootmidiclocknode.SelectedImageIndex = 3;*/

        }

        private void AddVariable(TodoVariable var)
        {
            TreeNode catnode = this.GetCategoryNode(var.Category);

            TreeNode varnode = catnode.Nodes.Add(var.Name);
            varnode.Tag = var;
            varnode.ImageIndex = 1;
            varnode.SelectedImageIndex = 1;

            this.vardic.Add(var, varnode);

            this.BuildMappings(var);

            var.VariableCategoryChanged += new TodoVariableCategoryChangedDelegate(var_VariableCategoryChanged);
        }

        void var_VariableCategoryChanged(TodoVariable var, string oldcat)
        {
            TreeNode node = this.vardic[var];
            node.Parent.Nodes.Remove(node);

            this.GetCategoryNode(var.Category).Nodes.Add(node);
        }

        private void BuildMappings(TodoVariable var)
        {
            TreeNode node = this.vardic[var];
            node.Nodes.Clear();
            //node.TreeView.e
            foreach (AbstractTodoInput input in var.Inputs)
            {
                TreeNode inode = node.Nodes.Add(input.InputType + " : " + input.InputMap);
                inode.ImageIndex = 2;
                inode.SelectedImageIndex = 2;
            }
        }

        private TreeNode GetCategoryNode(string name)
        {
            TreeNode catnode = null;
            foreach (TreeNode node in this.rootcatnode.Nodes)
            {
                if (node.Text == name)
                {
                    catnode = node;
                }
            }

            if (catnode == null)
            {
                catnode = this.rootcatnode.Nodes.Add(name);
            }

            catnode.ImageIndex = 0;
            catnode.SelectedImageIndex = 0;



            return catnode;
        }
    }
}
