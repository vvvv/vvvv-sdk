using System;
using System.Collections.Generic;
using System.Text;
using Un4seen.Bass;
using Un4seen.Bass.AddOn.Fx;

namespace BassSound.Internals
{
    class FileChannelInfo : ChannelInfo
    {
        private string filename;
        private double pitch;
        private double tempo;
        private SYNCPROC _syncProc = null;

        public event EventHandler OnStreamEnd;

        public string FileName
        {
            get { return filename; }
            set { filename = value; }
        }

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

        #region Initialize
        public override void Initialize(int deviceid)
        {
            Bass.BASS_SetDevice(deviceid);

            int handle = Bass.BASS_StreamCreateFile(this.filename, 0, 0, BASSFlag.BASS_STREAM_DECODE | BASSFlag.BASS_SAMPLE_FLOAT);

            BASSFlag flag = BASSFlag.BASS_SAMPLE_FLOAT | BASSFlag.BASS_FX_FREESOURCE;

            if (this.IsDecoding)
            {
                flag = flag | BASSFlag.BASS_STREAM_DECODE;
            }

            if (this.Mono)
            {
                flag = flag | BASSFlag.BASS_SAMPLE_MONO;
            }

            handle = BassFx.BASS_FX_TempoCreate(handle, flag);

            long len = Bass.BASS_ChannelGetLength(handle);
            this.Length = Bass.BASS_ChannelBytes2Seconds(handle, len);

            this.BassHandle = handle;

            _syncProc = new SYNCPROC(TrackSync);

            // setup a new sync
            Bass.BASS_ChannelSetSync(handle,BASSSync.BASS_SYNC_END,0,_syncProc,IntPtr.Zero);
            
            this.OnInitialize();
        }
        #endregion

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

        protected override void OnCreate()
        {
            base.OnCreate();
            this.OnPitchUpdated();
            this.OnTempoUpdated();
        }
        #endregion

        private void TrackSync(int syncHandle, int channel, int data, IntPtr user)
        {
            if (OnStreamEnd != null)
            {
                OnStreamEnd(this, new EventArgs());
            }
        }
    }

}
