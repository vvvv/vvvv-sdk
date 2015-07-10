using System;
using System.Collections.Generic;
using System.Text;
using VVVV.TodoMap.Lib.Modules.Midi;
using System.Xml;
using VVVV.TodoMap.Lib.Modules.Osc;
using System.Globalization;

namespace VVVV.TodoMap.Lib.Persist
{
    public class TodoXmlUnwrapper
    {
        public static void LoadXml(TodoEngine engine, string xml)
        {
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
                        LoadVariable(engine, child);
                    }
                }

                if (node.Name == "TodoModules")
                {
                    foreach (XmlNode child in node.ChildNodes)
                    {
                        LoadModules(engine, child);
                    }
                }
            }
        }

        private static void LoadModules(TodoEngine engine, XmlNode node)
        {
            if (node.Name == "Osc")
            {
                LoadOscModule(engine, node);
            }

            if (node.Name == "Midi")
            {
                LoadMidiModule(engine, node);
            }
        }


        private static void LoadOscModule(TodoEngine engine, XmlNode node)
        {
            //Stop module for reload
            engine.Osc.SetEnabled(false);
            engine.Osc.SetOutputEnabled(false);
            foreach (XmlNode child in node.ChildNodes)
            {
                if (child.Name == "Input")
                {
                    engine.Osc.AutoStartInput = BoolExtension.ParseEnglish(child.Attributes["AutoStart"].Value);
                    engine.Osc.LocalPort = int.Parse(child.Attributes["Port"].Value);
                }

                if (child.Name == "Output")
                {
                    engine.Osc.AutoStartOutput = BoolExtension.ParseEnglish(child.Attributes["AutoStart"].Value);
                    engine.Osc.RemotePort = int.Parse(child.Attributes["Port"].Value);
                }
            }

            if (engine.Osc.AutoStartInput) { engine.Osc.SetEnabled(true); }
            if (engine.Osc.AutoStartOutput) { engine.Osc.SetOutputEnabled(true); }

        }

        private static void LoadMidiModule(TodoEngine engine, XmlNode node)
        {
            foreach (XmlNode child in node.ChildNodes)
            {
                if (child.Name == "Inputs")
                {
                    foreach (XmlNode n in child.ChildNodes)
                    {
                        if (n.Name == "Input")
                        {
                            engine.Midi.SetInputAutoStart(n.Attributes["AutoStart"].Value, true);
                            //engine.Osc.AutoStartInput = bool.Parse(child.Attributes["AutoStart"].Value);
                            //engine.Osc.LocalPort = int.Parse(child.Attributes["Port"].Value);
                        }
                    }
                }

                if (child.Name == "Outputs")
                {
                    foreach (XmlNode n in child.ChildNodes)
                    {
                        if (n.Name == "Output")
                        {
                            engine.Midi.SetOutputAutoStart(n.Attributes["AutoStart"].Value, true);
                            //engine.Osc.AutoStartOutput = bool.Parse(child.Attributes["AutoStart"].Value);
                            //engine.Osc.RemotePort = int.Parse(child.Attributes["Port"].Value);
                        }
                    }
                }
            }

            if (engine.Osc.AutoStartInput) { engine.Osc.SetEnabled(true); }
            if (engine.Osc.AutoStartOutput) { engine.Osc.SetOutputEnabled(true); }
        }

        private static void LoadVariable(TodoEngine engine, XmlNode node)
        {
            TodoVariable var = new TodoVariable();
            var.Inputs = new List<AbstractTodoInput>();

            foreach (XmlNode child in node.ChildNodes)
            {
                if (child.Name == "Name")
                {
                    var.Name = child.InnerText;
                }
                if (child.Name == "Category")
                {
                    var.Category = child.InnerText;
                }
                if (child.Name == "AllowFeedBack")
                {
                    var.AllowFeedBack = BoolExtension.ParseEnglish(child.InnerText);
                }
                if (child.Name == "Default")
                {
                    var.Default = Convert.ToDouble(child.InnerText,CultureInfo.InvariantCulture);
                }
                if (child.Name == "MinValue")
                {
                    var.Mapper.MinValue = Convert.ToDouble(child.InnerText, CultureInfo.InvariantCulture);
                }
                if (child.Name == "MaxValue")
                {
                    var.Mapper.MaxValue = Convert.ToDouble(child.InnerText, CultureInfo.InvariantCulture);
                }
                if (child.Name == "TweenMode")
                {
                    var.Mapper.TweenMode = (eTweenMode)Enum.Parse(typeof(eTweenMode), child.InnerText);
                }
                if (child.Name == "EaseMode")
                {
                    var.Mapper.EaseMode = (eTweenEaseMode)Enum.Parse(typeof(eTweenEaseMode), child.InnerText);
                }
                if (child.Name == "TakeOverMode")
                {
                    var.TakeOverMode = (eTodoGlobalTakeOverMode)Enum.Parse(typeof(eTodoGlobalTakeOverMode), child.InnerText);
                }
                if (child.Name == "TodoInputs")
                {
                    foreach (XmlNode input in child.ChildNodes)
                    {
                        LoadInputs(engine, input, var);
                    }
                }
            }

            if (var.Category == null) { var.Category = "Global"; }
            else { if (var.Category.Length == 0) { var.Category = "Global"; } }
            engine.RegisterVariable(var,false);
            var.SetDefault();
            
            //foreach (AbstractTodoInput ti in var.Inputs)
            //{
            //    engine.RegisterInput(ti);
            //}
        }

        private static void LoadInputs(TodoEngine engine, XmlNode node, TodoVariable var)
        {
            string type = node.Attributes["Type"].Value;

            
            if (type == "OSC")
            {
                TodoOscInput osc = new TodoOscInput(var);
                
                foreach (XmlNode child in node.ChildNodes)
                {
                    if (child.Name == "Message")
                    {
                        osc.Message = child.InnerText;
                    }
                    if (child.Name == "TakeOverMode")
                    {
                        osc.TakeOverMode = (eTodoLocalTakeOverMode)Enum.Parse(typeof(eTodoLocalTakeOverMode), child.InnerText);
                    }
                    if (child.Name == "FeedbackMode")
                    {
                        osc.FeedBackMode = (eTodoLocalFeedBackMode)Enum.Parse(typeof(eTodoLocalFeedBackMode), child.InnerText);
                    }
                }
                engine.Osc.RegisterInput(osc);
            }

            if (type == "Midi")
            {
                TodoMidiInput midi = new TodoMidiInput(var);
                midi.Device = "Any";
                foreach (XmlNode child in node.ChildNodes)
                {
                    if (child.Name == "ControlType")
                    {
                        midi.ControlType = (eTodoMidiType)Enum.Parse(typeof(eTodoMidiType), child.InnerText);
                    }
                    if (child.Name == "ControlValue")
                    {
                        midi.ControlValue = Convert.ToInt32(child.InnerText);
                    }
                    if (child.Name == "Channel")
                    {
                        midi.MidiChannel = Convert.ToInt32(child.InnerText);
                    }
                    if (child.Name == "Device")
                    {
                        midi.Device = child.InnerText;
                    }
                    if (child.Name == "TakeOverMode")
                    {
                        midi.TakeOverMode = (eTodoLocalTakeOverMode)Enum.Parse(typeof(eTodoLocalTakeOverMode), child.InnerText);
                    }
                    if (child.Name == "FeedbackMode")
                    {
                        midi.FeedBackMode = (eTodoLocalFeedBackMode)Enum.Parse(typeof(eTodoLocalFeedBackMode), child.InnerText);
                    }

                }
                engine.Midi.RegisterInput(midi);
            }
        }
    }
}
