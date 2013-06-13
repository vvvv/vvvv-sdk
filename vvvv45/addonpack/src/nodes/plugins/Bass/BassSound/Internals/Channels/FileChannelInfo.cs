using System;
using System.Collections.Generic;
using System.Text;
using Un4seen.Bass;
using Un4seen.Bass.AddOn.Fx;

namespace BassSound.Internals
{
    public class FileChannelInfo : TempoChannelInfo
    {
        private string filename;
        private SYNCPROC _syncProc = null;
        private int loopSyncHandle;
        private double loopstart, loopend;

        public event EventHandler OnStreamEnd;

        public string FileName
        {
            get { return filename; }
            set { filename = value; }
        }



        public double LoopStart
        {
            get { return loopstart; }
            set { loopstart = value; }
        }

        public double LoopEnd
        {
            get { return loopend; }
            set
            {
                this.loopend = value;
                this.OnLoopEndUpdated();
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

        private SYNCPROC _mySyncProc;
        private void LoopEndReached(int handle, int channel, int data, IntPtr user)
        {
            if (this.Loop)
            {
                Bass.BASS_ChannelSetPosition(this.BassHandle.Value, this.LoopStart);
            }
            else
            {
                Bass.BASS_ChannelPause(this.BassHandle.Value);
            }
        }

        protected override void OnLoopEndUpdated()
        {
            if (this.BassHandle.HasValue)
            {
                if (loopSyncHandle > 0)
                {
                    Bass.BASS_ChannelRemoveSync(this.BassHandle.Value, loopSyncHandle);
                    _mySyncProc = null;
                }

                _mySyncProc = new SYNCPROC(LoopEndReached);

                long end;
                if (this.LoopEnd == 0)
                {
                    end = Bass.BASS_ChannelSeconds2Bytes(this.BassHandle.Value, this.Length);
                }
                else
                {
                    end = Bass.BASS_ChannelSeconds2Bytes(this.BassHandle.Value, this.LoopEnd);
                }
                loopSyncHandle = Bass.BASS_ChannelSetSync(this.BassHandle.Value, BASSSync.BASS_SYNC_POS | BASSSync.BASS_SYNC_MIXTIME, end, _mySyncProc, IntPtr.Zero);

            }
        }
    }

}
