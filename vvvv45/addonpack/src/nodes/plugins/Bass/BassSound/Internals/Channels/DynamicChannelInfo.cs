using System;
using System.Collections.Generic;
using System.Text;
using Un4seen.Bass;
using System.Runtime.InteropServices;
using System.IO;

namespace BassSound.Internals
{
    public class DynamicChannelInfo : ChannelInfo
    {
        private STREAMPROC _myStreamCreate;
        
        private float[] fbuffer = null;
        private int bufferposition = 0;

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

        #region Initialize
        public override void Initialize(int deviceid)
        {
            Bass.BASS_SetDevice(deviceid);

            _myStreamCreate = new STREAMPROC(MyFileProc);

            BASSFlag flag = BASSFlag.BASS_SAMPLE_FLOAT;
            if (this.IsDecoding)
            {
                flag = flag | BASSFlag.BASS_STREAM_DECODE;
            }

            int handle = Bass.BASS_StreamCreate(44100, 1,flag, _myStreamCreate, IntPtr.Zero);


            this.BassHandle = handle;

            this.OnInitialize();
        }
        #endregion

        #region Stream Proc
        private int MyFileProc(int handle, IntPtr buffer, int length, IntPtr user)
        {
            int flength = length / 4;
            float[] data;

            data = new float[flength];

            for (int i = 0; i < flength; i++)
            {
                data[i] = this.Buffer[this.bufferposition];
                this.bufferposition++;
                if (this.bufferposition == this.Buffer.Length)
                {
                    this.bufferposition = 0;
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
    }
}
