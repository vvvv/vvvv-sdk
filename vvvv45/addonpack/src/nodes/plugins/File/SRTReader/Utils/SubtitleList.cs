using System;
using System.Collections.Generic;
using System.Text;

namespace vvvv.Nodes.Subtitles
{
    public class TSubtitleList : List<TSubtitle>
    {
        public string GetText(double time)
        {
            foreach (TSubtitle sub in this)
            {
                if (time >= sub.TimeFrom && time <= sub.TimeTo)
                {
                    return sub.Text;
                }
            }
            return "";
        }

        public TSubtitle GetSubtitle(double time)
        {
            //Will update that with a quick search algorithm
            foreach (TSubtitle sub in this)
            {
                if (time >= sub.TimeFrom && time <= sub.TimeTo)
                {
                    return sub;
                }
            }
            return null;
        }
    }
}
