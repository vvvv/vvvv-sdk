using System;
using System.Collections.Generic;
using System.Text;
using Un4seen.Bass;
using System.Runtime.InteropServices;
using System.IO;
using Un4seen.Bass.AddOn.Fx;

namespace BassSound.Internals
{
    public class DynamicChannelInfo : TempoChannelInfo
    {
        private STREAMPROC _myStreamCreate;
        
        private float[] fbuffer = null;
        private int bufferposition = 0;

        private int bufferstart;
        private int bufferend;

        public DynamicChannelInfo()
        {
            Console.WriteLine();
        }

        #region Additional Properties
        public float[] Buffer
        {
            get { return fbuffer; }
            set 
            { 
                fbuffer = value;
                if (this.bufferposition >= value.Length)
                {
                    this.bufferposition = 0;
                }
            }
        }

        public int BufferStart
        {
            get { return this.bufferstart; }
            set { this.bufferstart = value; }
        }

        public int BufferEnd
        {
            get { return this.bufferend; }
            set { this.bufferend = value; }
        }

        public int BufferPosition
        {
            get { return this.bufferposition; }
        }
        #endregion

        #region Initialize
        public override void Initialize(int deviceid)
        {
            Bass.BASS_SetDevice(deviceid);

            _myStreamCreate = new STREAMPROC(MyFileProc);

            int handle = Bass.BASS_StreamCreate(44100, 1,BASSFlag.BASS_STREAM_DECODE | BASSFlag.BASS_SAMPLE_FLOAT, _myStreamCreate, IntPtr.Zero);

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

            this.BassHandle = handle;

            this.OnInitialize();
        }
        #endregion

        #region Stream Proc
        private int MyFileProc(int handle, IntPtr buffer, int length, IntPtr user)
        {
            int flength = length / 4;
            int remaining = flength;
            int index = 0;

            float[] data = new float[flength];

            int end = this.Buffer.Length;

            //We decide where is the end to copy
            if (this.BufferEnd < this.Buffer.Length && this.BufferEnd > 0)
            {
                end = this.bufferend;
            }

            if (this.bufferposition > end)
            {
                this.bufferposition = this.bufferstart;
            }
            

            //If buffer changed, need to update
            if (this.bufferposition < this.BufferStart)
            {
                this.bufferposition = this.BufferStart;
            }

            //Improvement for the file copy
            while (remaining > 0)
            {

                int tocopy = end - this.bufferposition;
                if (tocopy < remaining)
                {
                    Array.Copy(this.Buffer,this.bufferposition,data,index,tocopy);   
                    remaining = remaining - tocopy;
                    index += tocopy;
                    this.bufferposition = this.BufferStart;
                }
                else
                {
                    Array.Copy(this.Buffer, this.bufferposition, data, index, remaining);
                    this.bufferposition = this.bufferposition + remaining;
                    remaining = 0;
                }
            }

            try
            {
                Marshal.Copy(data, 0, buffer, flength);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return 0;
            }

            return length;
        }
        #endregion

        protected override void OnLoopEndUpdated()
        {
            
        }
    }
}
