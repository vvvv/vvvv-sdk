using System;
using System.Collections.Generic;
using System.Text;
using Un4seen.Bass;

namespace BassSound.Internals
{
    public abstract class TempoChannelInfo : ChannelInfo
    {
        private double pitch;
        private double tempo;

        public double Pitch
        {
            get { return pitch; }
            set
            {
                pitch = value;
                this.OnPitchUpdated();
            }
        }

        public double Tempo
        {
            get { return tempo; }
            set
            {
                tempo = value;
                this.OnTempoUpdated();
            }
        }

        #region Events 
        protected void OnPitchUpdated()
        {
            if (this.BassHandle.HasValue)
            {
                Bass.BASS_ChannelSetAttribute(this.BassHandle.Value, BASSAttribute.BASS_ATTRIB_TEMPO_PITCH, (float)this.pitch);
            }
        }

        protected void OnTempoUpdated()
        {
            if (this.BassHandle.HasValue)
            {
                Bass.BASS_ChannelSetAttribute(this.BassHandle.Value, BASSAttribute.BASS_ATTRIB_TEMPO, (float)this.tempo);
            }
        }
        #endregion
    }
}
