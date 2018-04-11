using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Un4seen.Bass.AddOn.Fx;
using Un4seen.Bass;

namespace BassSound.Data.BeatScanner
{
    public class ThreadedBeatScanner
    {
        private int handle;
        private Thread thread;
        private BeatScannerParameters parameters;

        public delegate void BeatFoundDelegate(int index, double position,double progress);
        public delegate void BeatScannerDelegate(int index);

        public event BeatFoundDelegate OnBeatFound;
        public event BeatScannerDelegate OnComplete;
        public event BeatScannerDelegate OnAbort;

        private BPMBEATPROC _beatProc;
        private double len;
  

        public ThreadedBeatScanner(int handle,BeatScannerParameters parameters)
        {
            this.handle = handle;
            this.parameters = parameters;
        }

        public ThreadedBeatScanner(string filename,BeatScannerParameters parameters)
        {
            this.parameters = parameters;
            this.handle = Bass.BASS_StreamCreateFile(filename, 0, 0, BASSFlag.BASS_STREAM_DECODE | BASSFlag.BASS_SAMPLE_FLOAT);
        }

        #region Start
        public void Start()
        {
            this.thread = new Thread(this.Scan);
            thread.Start();
        }
        #endregion

        #region Stop
        public void Stop()
        {
            try
            {
                thread.Abort();
            }
            catch
            {

            }
            Bass.BASS_StreamFree(this.handle);
            if (OnAbort != null)
            {
                OnAbort(this.parameters.Index);
            }
        }
        #endregion

        #region Scan
        private void Scan()
        {
            long pos = Bass.BASS_ChannelGetLength(this.handle);
            this.len = Bass.BASS_ChannelBytes2Seconds(this.handle, pos);
            _beatProc = new BPMBEATPROC(MyBeatProc);

            BassFx.BASS_FX_BPM_BeatCallbackSet(this.handle, _beatProc, IntPtr.Zero);
            BassFx.BASS_FX_BPM_BeatSetParameters(this.handle, this.parameters.Width, this.parameters.Center,this.parameters.Release);
            BassFx.BASS_FX_BPM_BeatDecodeGet(this.handle, 0.0, this.len, BASSFXBpm.BASS_FX_BPM_BKGRND, _beatProc, IntPtr.Zero);


            Bass.BASS_StreamFree(this.handle);
            if (OnComplete != null)
            {
                OnComplete(this.parameters.Index);
            } 
        }
        #endregion

        #region Beat Proc
        private void MyBeatProc(int channel, double beatpos, IntPtr user)
        {
            if (OnBeatFound != null)
            {
                double percent = (beatpos / this.len) * 100.0;

                OnBeatFound(this.parameters.Index, beatpos,percent);
            }
        }
        #endregion


    }
}
