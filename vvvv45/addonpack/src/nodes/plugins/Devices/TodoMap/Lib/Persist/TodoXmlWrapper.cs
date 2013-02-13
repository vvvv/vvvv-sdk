using System;
using System.Collections.Generic;
using System.Text;
using VVVV.TodoMap.Lib.Modules.Midi;
using VVVV.TodoMap.Lib.Modules.Osc;
using System.Globalization;

namespace VVVV.TodoMap.Lib.Persist
{
    public class TodoXmlWrapper
    {
        public static string Persist(TodoEngine engine)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("<TodoMap>");

            //Modules
            sb.AppendLine("<TodoModules>");
            PersistOsc(sb, engine.Osc);
            PersistMidi(sb, engine.Midi);
            sb.AppendLine("</TodoModules>");


            //Variables
            sb.AppendLine("<TodoVariables>");

            foreach (TodoVariable var in engine.Variables)
            {
                PersistVariable(sb, var);
            }

            sb.AppendLine("</TodoVariables>");
            sb.AppendLine("</TodoMap>");

            return sb.ToString();
        }

        private static void PersistOsc(StringBuilder sb, TodoOscDevice osc)
        {
            sb.AppendLine("<Osc>");

            string inputline = "<Input AutoStart=\"[inputauto]\" Port=\"[inputport]\" />";
            string outputline = "<Output AutoStart=\"[outputauto]\" Port=\"[outputport]\" />";

            inputline = inputline.Replace("[inputauto]", osc.AutoStartInput.ToStringEnglish());
            inputline = inputline.Replace("[inputport]", osc.LocalPort.ToString());

            outputline = outputline.Replace("[outputauto]", osc.AutoStartOutput.ToStringEnglish());
            outputline = outputline.Replace("[outputport]", osc.RemotePort.ToString());

            sb.AppendLine(inputline);
            sb.AppendLine(outputline);

            sb.AppendLine("</Osc>");
        }

        private static void PersistMidi(StringBuilder sb, TodoMidiDevice midi)
        {
            sb.AppendLine("<Midi>");
            
            sb.AppendLine("<Inputs>");
            foreach (string s in midi.InputAuto)
            {
                string inputline = "<Input AutoStart=\"[inputname]\" />";
                inputline = inputline.Replace("[inputname]", s);
                sb.AppendLine(inputline);
            }
            sb.AppendLine("</Inputs>");

            sb.AppendLine("<Outputs>");
            foreach (string s in midi.OutputAuto)
            {
                string outputline = "<Output AutoStart=\"[outputname]\" />";
                outputline = outputline.Replace("[outputname]", s);
                sb.AppendLine(outputline);
            }
            sb.AppendLine("</Outputs>");
            sb.AppendLine("</Midi>");
        }

        private static void PersistVariable(StringBuilder sb, TodoVariable var)
        {
          
            sb.AppendLine("<TodoVariable>");
            sb.AppendLine("<Name>" + var.Name + "</Name>");
            sb.AppendLine("<Category>" + var.Category + "</Category>");
            sb.AppendLine("<AllowFeedBack>" + var.AllowFeedBack.ToStringEnglish() + "</AllowFeedBack>");
            sb.AppendLine("<TakeOverMode>" + var.TakeOverMode.ToString() + "</TakeOverMode>");
            sb.AppendLine("<Default>" + var.Default.ToString(CultureInfo.InvariantCulture) + "</Default>");
            sb.AppendLine("<MinValue>" + var.Mapper.MinValue.ToString(CultureInfo.InvariantCulture) + "</MinValue>");
            sb.AppendLine("<MaxValue>" + var.Mapper.MaxValue.ToString(CultureInfo.InvariantCulture) + "</MaxValue>");
            sb.AppendLine("<TweenMode>" + var.Mapper.TweenMode + "</TweenMode>");
            sb.AppendLine("<EaseMode>" + var.Mapper.EaseMode + "</EaseMode>");
            sb.AppendLine("<TodoInputs>");
            foreach (AbstractTodoInput input in var.Inputs)
            {
                PersistInput(sb, input);
            }
            sb.AppendLine("</TodoInputs>");
            sb.AppendLine("</TodoVariable>");
        }

        private static void PersistInput(StringBuilder sb, AbstractTodoInput input)
        {
            sb.Append("<TodoInput Type=\"");

            
            if (input is TodoOscInput)
            {
                TodoOscInput osc = input as TodoOscInput;
                sb.AppendLine("OSC\">");
                sb.AppendLine("<Message>" + osc.Message + "</Message>");
            }

            if (input is TodoMidiInput)
            {
                TodoMidiInput midi = input as TodoMidiInput;
                sb.AppendLine("Midi\">");
                sb.AppendLine("<Channel>" + midi.MidiChannel + "</Channel>");
                sb.AppendLine("<ControlType>" + midi.ControlType + "</ControlType>");
                sb.AppendLine("<ControlValue>" + midi.ControlValue + "</ControlValue>");
                sb.AppendLine("<Device>" + midi.Device + "</Device>");
            }
            sb.AppendLine("<TakeOverMode>" + input.TakeOverMode.ToString() + "</TakeOverMode>");
            sb.AppendLine("<FeedbackMode>" + input.FeedBackMode.ToString() + "</FeedbackMode>");
            

            sb.AppendLine("</TodoInput>");
        }
    }
}
