using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace VVVV.TodoMap.Lib.Persist
{
    public class TodoPreset
    {
        public string Name { get; set; }
        public string Category { get; set; }
        public double Value { get; set; }
    }

    public class TodoPresetWrapper
    {
        public static string Persist(TodoEngine engine,string[] variables)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("<TodoPreset>");

            //Modules



            //Variables
            sb.AppendLine("<TodoVariables>");

            foreach (TodoVariable var in engine.Variables)
            {
                if (variables.Contains(var.Name))
                {
                    PersistVariable(sb, var);
                }
            }

            sb.AppendLine("</TodoVariables>");
            sb.AppendLine("</TodoPreset>");

            return sb.ToString();
        }

        private static void PersistVariable(StringBuilder sb, TodoVariable var)
        {
            sb.AppendLine("<TodoVariable>");
            sb.AppendLine("<Name>" + var.Name + "</Name>");
            sb.AppendLine("<Category>" + var.Category + "</Category>");
            sb.AppendLine("<Value>" + var.ValueRaw.ToString() + "</Value>");
            sb.AppendLine("</TodoVariable>");
        }

        public static List<TodoPreset> LoadXml(TodoEngine engine, string xml)
        {
            List<TodoPreset> varnames = new List<TodoPreset>();
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(xml);

            XmlNode root = doc.DocumentElement;

            //node = node.ChildNodes[0];
            foreach (XmlNode node in root.ChildNodes)
            {
                if (node.Name == "TodoVariables")
                {
                    foreach (XmlNode child in node.ChildNodes)
                    {
                        LoadVariable(engine, child,varnames);
                    }
                }
            }
            return varnames;
        }

        private static void LoadVariable(TodoEngine engine, XmlNode node, List<TodoPreset> varnames)
        {
            TodoPreset preset = new TodoPreset();

            foreach (XmlNode child in node.ChildNodes)
            {
                if (child.Name == "Name")
                {
                    preset.Name = child.InnerText;
                }
                if (child.Name == "Category")
                {
                    preset.Category = child.InnerText;
                }
                if (child.Name == "Value")
                {
                    preset.Value = double.Parse(child.InnerText);
                }
            }

            TodoVariable var = engine.GetVariableByName(preset.Name);
            if (var != null)
            {
                var.SetValue(null, preset.Value);
                varnames.Add(preset);
            }
        }
    }
}
