using System;
using System.Collections.Generic;
using System.Text;
using Un4seen.Bass;
using Un4seen.Bass.AddOn.Mix;

namespace BassSound.Internals
{
    public abstract class ChannelInfo
    {
        public event EventHandler OnInit;

        #region Internal fields
        private int internalhandle;
        private int? basshandle = null;
        #endregion

        #region Fields
        private bool mono;
        private bool loop;
        private bool play;
        private bool isdecoding;
        private double length;

        
        #endregion

        #region Internal Properties
        public int InternalHandle
        {
            get { return internalhandle; }
            set { internalhandle = value; }
        }

        public int? BassHandle
        {
            get { return basshandle; }
            set 
            { 
                basshandle = value;
                OnCreate();
            }
        }
        #endregion

        #region Play Properties
        public bool Mono
        {
            get { return mono; }
            set { mono = value; }
        }

        public bool Loop
        {
            get { return loop; }
            set 
            { 
                loop = value;
                this.OnLoopUpdated();
            }
        }

        public bool Play
        {
            get { return play; }
            set 
            {   
                play = value;
                this.OnPlayUpdated();
            }
        }
        


        public bool IsDecoding
        {
            get { return isdecoding; }
            set { isdecoding = value; }
        }

        public double Length
        {
            get { return length; }
            set { length = value; }
        }
        #endregion

        #region Abstract Methods
        public abstract void Initialize(int deviceid);
        protected abstract void OnLoopEndUpdated();
        #endregion
    
        #region Protected methods
        protected void OnInitialize()
        {
            if (OnInit != null)
            {
                OnInit(this, new EventArgs());
            }
        }
        
        protected void OnPlayUpdated()
        {
            if (this.BassHandle.HasValue)
            {
                //Check if channel is within a mixer or not
                int mixerhandle = BassMix.BASS_Mixer_ChannelGetMixer(this.BassHandle.Value);

                if (mixerhandle != 0)
                {
                    //In a mixer, updated the proper status
                    if (this.play)
                    {
                        BassMix.BASS_Mixer_ChannelPlay(this.basshandle.Value);
                    }
                    else
                    {
                        BassMix.BASS_Mixer_ChannelPause(this.basshandle.Value);
                    }
                }
                else
                {
                    //Not in a mixer, just updated standard status
                    if (this.play)
                    {
                        Bass.BASS_ChannelPlay(this.basshandle.Value, false);
                    }
                    else
                    {
                        Bass.BASS_ChannelPause(this.basshandle.Value);
                    }
                }
            }
        }

        protected void OnLoopUpdated()
        {
            if (this.BassHandle.HasValue)
            {
                if (this.loop)
                {
                    Bass.BASS_ChannelFlags(this.basshandle.Value, BASSFlag.BASS_SAMPLE_LOOP, BASSFlag.BASS_SAMPLE_LOOP);
                }
                else
                {
                    Bass.BASS_ChannelFlags(this.basshandle.Value, BASSFlag.BASS_DEFAULT, BASSFlag.BASS_SAMPLE_LOOP);
                }
            }    
        }

        protected virtual void OnCreate()
        {
            this.OnPlayUpdated();
            this.OnLoopUpdated();
            this.OnLoopEndUpdated();
        }


        #endregion

    }

    #region Channel List
    public class ChannelList : List<ChannelInfo>
    {
        public ChannelInfo GetByID(int id)
        {
            foreach (ChannelInfo chan in this)
            {
                if (id == chan.InternalHandle)
                {
                    return chan;
                }
            }

            return null;
        }

        public void RefreshPlay()
        {
            foreach (ChannelInfo info in this)
            {
                info.Play = info.Play;
            }
        }

        public void PauseAll()
        {
            foreach (ChannelInfo info in this)
            {
                if (info.BassHandle.HasValue)
                {
                    Bass.BASS_ChannelPause(info.BassHandle.Value);
                }
            }
        }
    }
    #endregion
}
