using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;
using VVVV.Utils.VMath;

namespace VVVV.Lib
{
    [XmlRoot("Track")]
    public class SeqTrack
    {
        #region Fields
        private string id;
        private TimeLine timeline = new TimeLine();
        private double bufferlength;
        private int trackindex;
        private bool play;
        private bool record;
        private double starttick;
        private double realbufferlength;
        private double currentposition = 0;
        private double lastrecordtime;
        private double positioninbuffer = 0;
        #endregion

        #region Is Empty
        public bool IsEmpty
        {
            get { return this.timeline.Count == 0; }
        }
        #endregion

        public void Clear()
        {
            this.timeline.Clear();
        }

        [XmlIgnore()]
        public double Starttick
        {
            get { return starttick; }
            set { starttick = value; }
        }

        public void StartPlay(double time)
        {
            if (!play && !record)
            {
                this.play = true;
                this.starttick = time;
                this.currentposition = time;
            }
        }

        public void StartPlay(double time,bool force)
        {
            if (!play && !record || force)
            {
                this.play = true;
                this.starttick = time - this.positioninbuffer;
                this.currentposition = time;
            }
        }

        public void DoSeek(double time)
        {
            if (!record)
            {
                this.starttick = time;
            }
        }

        [XmlAttribute()]
        public double LastRecordTime
        {
            get { return this.lastrecordtime; }
        }

        [XmlIgnore()]
        public double RealBufferLength
        {
            get { return this.realbufferlength; }
        }

        [XmlIgnore()]
        public bool Play
        {
            get { return play; }
            set 
            { 
                play = value;
                if (!play)
                {
                    this.record = false;
                }
            }
        }

        [XmlIgnore()]
        public bool Record
        {
            get { return record; }
        }

        [XmlAttribute()]
        public string Id
        {
            get { return id; }
            set { id = value; }
        }

        [XmlElement()]
        [XmlArrayItem("KeyFrame")]
        public TimeLine TimeLine
        {
            get { return timeline; }
            set { timeline = value; }
        }

        public double BufferPosition
        {
            get { return this.positioninbuffer; }
        }

        [XmlIgnore()]
        public double BufferLength
        {
            get { return bufferlength; }
            set 
            { 
                bufferlength = value;
                realbufferlength = value;

                if (this.timeline.Count > 0)
                {
                    double lastrecord = this.lastrecordtime;

                    if (lastrecord >= realbufferlength)
                    {
                        while (lastrecord >= realbufferlength)
                        {
                            realbufferlength += bufferlength; //2.0;
                        }
                    }
                }  
            }
        }

        [XmlIgnore()]
        public int TrackIndex
        {
            get { return trackindex; }
            set { trackindex = value; }
        }

        public void StartRecording(double initialtime)
        {
            //Only if not already recording
            if (!this.Record && this.play)
            {
                this.record = true;
                this.starttick = initialtime;
                this.lastrecordtime = 0;
                this.timeline.Clear();
                this.positioninbuffer = 0;
            }
        }

        public bool StopRecording(double time)
        {
            if (this.record)
            {
                this.record = false;
                this.lastrecordtime = time - this.starttick;

                //Increase real buffer length
                if (lastrecordtime >= realbufferlength)
                {
                    while (lastrecordtime >= realbufferlength)
                    {
                        realbufferlength += bufferlength;// 2.0;
                    }
                }
                return true;
            }
            else
            {
                return false;
            }
        }

        public double Getvalue(double time,bool interpolate,bool loop, out double pos)
        {
            if (this.timeline.Count == 0)
            {
                pos = 0;
                return 0;
            }

            double elasped = time - this.starttick;

            elasped = elasped % this.realbufferlength;

            double result = this.timeline[0].Value;
            bool found = false;


            for (int i = this.timeline.Count - 1; i >= 0  && !found; i--)
            {
                if (elasped >= this.timeline[i].Time)
                {
                    result = this.timeline[i].Value;
                    if (interpolate)
                    {
                        double lastval;
                        if (i == this.timeline.Count - 1)
                        {
                            if (loop)
                            {
                                lastval = this.timeline[0].Value;   
                                double range = this.realbufferlength - this.timeline[i].Time;
                                double ratio = (elasped - this.timeline[i].Time) / range;
                                result = result + (lastval - result) * ratio;
                            }
                        }
                        else
                        {
                            lastval = this.timeline[i + 1].Value;
                            double range = this.timeline[i + 1].Time - this.timeline[i].Time;
                            double ratio = (elasped - this.timeline[i].Time) / range;
                            result = result + (lastval - result) * ratio;

                        }
                    }
                    found = true;
                }
            }


            pos = elasped;
            this.positioninbuffer = elasped;

            return result;
        }

        public double RecordValue(double time, double val)
        {
            if (this.record)
            {
                bool dorecord = false;
                double realtime = time - this.starttick;
                if (this.TimeLine.Count == 0)
                {
                    dorecord = true;
                }
                else
                {

                    TimeValuePair tvp = this.TimeLine[this.TimeLine.Count - 1];
                    if (realtime != tvp.Time && val != tvp.Value)
                    {
                        //need to have changed time and value
                        dorecord = true;
                    }
                }

                if (dorecord)
                {
                    TimeValuePair newtime = new TimeValuePair();
                    newtime.Time = realtime;
                    newtime.Value = val;
                    this.TimeLine.Add(newtime);

                    //Increase real buffer length
                    if (realtime >= realbufferlength)
                    {
                        while (realtime >= realbufferlength)
                        {
                            realbufferlength += bufferlength;// 2.0;
                        }
                    }

                    this.lastrecordtime = realtime;
                }

                return realtime;
            }
            else
            {
                return 0;
            }
        }

    }

    public class TrackDictionnary : Dictionary<string, SeqTrack> { }
}
