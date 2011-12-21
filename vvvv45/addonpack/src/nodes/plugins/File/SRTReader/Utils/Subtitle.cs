using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Text;

namespace vvvv.Nodes.Subtitles
{
    public class TSubtitle
    {
        private double timefrom;
        private double timeto;
        private string text;

        public double TimeFrom
        {
            get { return timefrom; }
            set { timefrom = value; }
        }

        public double TimeTo
        {
            get { return timeto; }
            set { timeto = value; }
        }

        public string Text
        {
            get { return text; }
            set { text = value; }
        }

        public List<string> Lines
        {
            get
            {
                List<string> result = new List<string>();
                string[] lines = this.text.Split("\n".ToCharArray());
                //Remove HTML Tags
                foreach (string line in lines)
                {
                    if (line.Length > 0)
                    {
                        result.Add(Regex.Replace(line, @"<(.|\n)*?>", string.Empty));
                    }
                }

                return result;
            }
        }
    }
}
