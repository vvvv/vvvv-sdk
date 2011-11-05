using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Microsoft.Cci;
using VVVV.Core;

namespace CCIAssembliesNodesCollector
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();

            var console = new ConsoleWidget.ConsoleForm();

            using (var dialog = new OpenFileDialog())
            {
                var res = dialog.ShowDialog();
                
                using (var host = new PeReader.DefaultHost()) 
                {
                    var x = new VVVV.Core.AssemblyNodeDefinitionCollector(host);

                    foreach (string assemblyName in dialog.FileNames) 
                    {
                        var assembly = host.LoadUnitFrom(assemblyName) as IAssembly;
                        if (assembly == null || assembly == Dummy.Assembly)
                        {
                            Console.WriteLine("The file '" + assemblyName + "' is not a PE file" +
                            " containing a CLR assembly, or an error occurred when loading it.");
                            continue;
                        }

                        foreach (var n in x.Collect(assembly))
                        {
                            Console.WriteLine(n.Systemname);
                            if (n is IDataflowNodeDefinition)
                            {
                                foreach (var p in (n as IDataflowNodeDefinition).Inputs)
                                    Console.WriteLine("  " + TypeHelper.GetTypeName(p.Type, NameFormattingOptions.SmartTypeName) + " " + p.Name + 
                                        (p.HasDefaultValue ? " = " + p.DefaultValue.Value.ToString() : ""));
                                foreach (var p in (n as IDataflowNodeDefinition).Outputs)
                                    Console.WriteLine("  out " + TypeHelper.GetTypeName(p.Type, NameFormattingOptions.SmartTypeName) + " " + p.Name);
                            }
                        }
                    }                
                }
            }

            console.Show();
        }
    }
}
