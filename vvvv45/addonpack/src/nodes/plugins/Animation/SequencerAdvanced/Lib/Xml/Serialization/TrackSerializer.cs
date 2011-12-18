using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.IO;

namespace VVVV.Lib
{
    public static class TrackSerializer
    {
        public static void SaveTrack(string path, SeqTrack track)
        {
            if (track.TimeLine.Count > 0)
            {
                StreamWriter sw = new StreamWriter(path + "\\Track_" + track.Id + ".xml");
                sw.WriteLine("<Track Id=\"" + track.Id + "\" LastRecordTime=\"" + track.LastRecordTime + "\" >");
                sw.WriteLine("<TimeLine>");

                foreach (TimeValuePair tvp in track.TimeLine)
                {
                    sw.WriteLine("<KeyFrame Time=\"" + tvp.Time + "\" Value=\"" + tvp.Value + "\" />");
                }

                sw.WriteLine("</TimeLine>");
                sw.WriteLine("</Track>");

                sw.Close();

            }
        }

        public static SeqTrack LoadTrack(string path,double bufferlen)
        {
            //XmlTextReader reader;
            try
            {
                XmlDocument doc = new XmlDocument();
                //Load the the document with the last book node.
                XmlTextReader reader = new XmlTextReader(path);
                reader.Read();
                // load reader
                doc.Load(reader);

                reader.Close();


                SeqTrack trck = new SeqTrack();

                XmlElement elem = doc.DocumentElement;

                trck.Id = elem.Attributes["Id"].Value;
                trck.BufferLength = bufferlen;

                trck.Play = true;
                trck.StartRecording(0);

                foreach (XmlNode node in elem.ChildNodes[0].ChildNodes)
                {
                    trck.RecordValue(Convert.ToDouble(node.Attributes["Time"].Value),
                        Convert.ToDouble(node.Attributes["Value"].Value));
                }

                trck.StopRecording(Convert.ToDouble(elem.Attributes["LastRecordTime"].Value));
                trck.Play = false;

                //reader.Close();

                return trck;
            }
            catch
            {


                return null;
            }
        }
    }
}
