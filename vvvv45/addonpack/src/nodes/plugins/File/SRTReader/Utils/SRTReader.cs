using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace vvvv.Nodes.Subtitles
{
    internal enum TEnumSeekStatus { Index, Time, Text }

    public class TSRTReader
    {
        #region Load From String
        public static TSubtitleList LoadFromFile(string path)
        {
            string content = File.ReadAllText(path, Encoding.Default);
            return LoadFromString(content);
        }
        #endregion

        #region Load From string
        public static TSubtitleList LoadFromString(string content)
        {
            TSubtitleList result = new TSubtitleList();
            TSubtitle currentsub = new TSubtitle();

            content = content.Replace("\r", "");
            string[] lines = content.Split("\n".ToCharArray());
            int index = 1;
            TEnumSeekStatus seek = TEnumSeekStatus.Index;

            int i = 0;
            while (i < lines.Length)
            {
                if (seek == TEnumSeekStatus.Index)
                {
                    //Ignore empty line
                    if (lines[i].Trim().Length > 0)
                    {
                        if (lines[i].Trim() == index.ToString())
                        {
                            currentsub = new TSubtitle();
                            seek = TEnumSeekStatus.Time;
                        }
                        else
                        {
                            throw new Exception("Invalid format");
                        }
                    }
                }
                else
                {

                    if (seek == TEnumSeekStatus.Time)
                    {
                        string[] times = lines[i].Replace(" ","").Split("-->".ToCharArray());
                        currentsub.TimeFrom = GetTime(times[0]);
                        currentsub.TimeTo = GetTime(times[3]);
                        seek = TEnumSeekStatus.Text;
                    }
                    else
                    {
                        if (seek == TEnumSeekStatus.Text)
                        {
                            if (lines[i].Length > 0)
                            {
                                currentsub.Text += lines[i] + "\n";
                            }
                            else
                            {
                                seek = TEnumSeekStatus.Index;
                                index++;
                                result.Add(currentsub);
                            }
                        }
                    }
                } 


                i++;
            }

            return result;
        }
        #endregion

        #region Get Time
        private static double GetTime(string time)
        {
            time = time.Trim();
            string[] splitsm = time.Split(",".ToCharArray());
            string[] splithms = splitsm[0].Split(":".ToCharArray());

            double result = 0;
            result = double.Parse(splithms[0]) * 3600.0;
            result += double.Parse(splithms[1]) * 60.0;
            result += double.Parse(splithms[2]);
            result += double.Parse(splitsm[1]) / 1000.0;
            return result;

        }
        #endregion

    }


}