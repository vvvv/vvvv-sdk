using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Threading;
using System.Runtime.InteropServices;

//adapted from: https://github.com/ItayGal2/SpoutCSharp
//with help from: Lynn Jarvis - http://spout.zeal.co

namespace Spout
{
    public class SpoutSender : IDisposable
    {
        const string CSenderNamesHandle = "SpoutSenderNames";
        const int CMaxSenders = 40;
        const int CSenderNameLength = 256;
        const string CActiveSenderHandle = "ActiveSenderName";
        const int CSpoutWaitTimeout = 100;

        MemoryMappedFile FSenderDescriptionMap;
        MemoryMappedFile FActiveSenderMap;
        MemoryMappedFile FSenderNamesMap;

        string FSenderName;
        TextureDesc FTextureDesc;

        public SpoutSender(string senderName, uint sharedHandle, uint width, uint height, uint format, uint usage)
        {
            FSenderName = senderName;
            FTextureDesc = new TextureDesc(sharedHandle, width, height, format, usage, new byte[256], 0);
        }

        public void Dispose()
        {
            RemoveNameFromSendersList(FSenderName);

            if (FSenderDescriptionMap != null)
                FSenderDescriptionMap.Dispose();

            if (FActiveSenderMap != null)
                FActiveSenderMap.Dispose();

            if (FSenderNamesMap != null)
                FSenderNamesMap.Dispose();
        }

        public bool Initialize()
        {
            var len = CSenderNameLength * CMaxSenders;
            FSenderNamesMap = MemoryMappedFile.CreateOrOpen(CSenderNamesHandle, len);

            //add sendername to list of senders
            if (AddNameToSendersList(FSenderName))
            {
                //write sender description
                var desc = FTextureDesc.ToByteArray();
                FSenderDescriptionMap = MemoryMappedFile.CreateNew(FSenderName, desc.Length);
                using (var vs = FSenderDescriptionMap.CreateViewStream())
                    vs.Write(desc, 0, desc.Length);

                //If we are the first/only sender, create a new ActiveSenderName map.
                //This is a separate shared memory containing just a sender name
                //that receivers can use to retrieve the current active Sender.
                FActiveSenderMap = MemoryMappedFile.CreateOrOpen(CActiveSenderHandle, CSenderNameLength);
                using (var vs = FActiveSenderMap.CreateViewStream())
                {
                    var firstByte = vs.ReadByte();
                    if (firstByte == 0) //no active sender yet
                    {
                        vs.Position = 0;
                        vs.Write(GetNameBytes(FSenderName), 0, CSenderNameLength);
                    }
                }

                return true;
            }
            return false;
        }

        bool AddNameToSendersList(string name)
        {
            var success = false;

            var senders = GetSenderNames();
            if (!senders.Contains(name))
            {
                senders.Add(name);
                WriteSenderNamesToMemoryMap(senders);
                success = true;
            }

            return success;
        }

        void RemoveNameFromSendersList(string name)
        {
            var senders = GetSenderNames();
            if (senders.Contains(name))
            {
                senders.Remove(name);
                WriteSenderNamesToMemoryMap(senders);
            }
        }

        void WriteSenderNamesToMemoryMap(List<string> senders)
        {
            bool createdNew;
            var mutex = new Mutex(true, CSenderNamesHandle + "_mutex", out createdNew);
            if (mutex == null)
                return;

            try
            {
                mutex.WaitOne(CSpoutWaitTimeout);
                using (var vs = FSenderNamesMap.CreateViewStream())
                {
                    for (int i = 0; i < CMaxSenders; i++)
                    {
                        byte[] bytes;
                        if (i < senders.Count)
                            bytes = GetNameBytes(senders[i]);
                        else //fill with 0s
                            bytes = new byte[CSenderNameLength];

                        vs.Write(bytes, 0, bytes.Length);
                    }
                }
            }
            catch (AbandonedMutexException e)
            {
                //Log.Add(e);     
            }
            finally
            {
                mutex.ReleaseMutex();
                mutex.Dispose();
            }
        }

        public static List<string> GetSenderNames()
        {
            var namesList = new List<string>();
            var name = new StringBuilder();
            var len = CMaxSenders * CSenderNameLength;

            //Read the memory mapped file in to a byte array
            using (var mmf = MemoryMappedFile.CreateOrOpen(CSenderNamesHandle, len))
            {
                using (var vs = mmf.CreateViewStream())
                {
                    var b = new byte[len];
                    vs.Read(b, 0, len);

                    //split into strings searching for the nulls 
                    for (int i = 0; i < len; i++)
                    {
                        if (b[i] == 0)
                        {
                            if (name.Length == 0)
                            {
                                i += CSenderNameLength - (i % CSenderNameLength) - 1;
                                continue;
                            }
                            namesList.Add(name.ToString());
                            name.Clear();
                        }
                        else
                            name.Append((char)b[i]);
                    }
                }
            }

            return namesList;
        }

        byte[] GetNameBytes(string name)
        {
            var b = new byte[CSenderNameLength];
            var nameBytes = Encoding.ASCII.GetBytes(name);
            Array.Copy(nameBytes, b, nameBytes.Length);
            return b;
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct TextureDesc
    {
        public uint SharedHandle;
        public uint Width;
        public uint Height;
        public uint Format;
        public uint Usage;
        public byte[] Description;
        public uint PartnerId;

        public TextureDesc(uint sharedHandle, uint width, uint height, uint format, uint usage, byte[] description, uint partnerId)
        {
            SharedHandle = sharedHandle;
            Width = width;
            Height = height;
            Format = format;
            Usage = usage;
            Description = description;
            PartnerId = partnerId;
        }

        public TextureDesc(MemoryMappedViewStream mmvs)
        {
            var br = new BinaryReader(mmvs);
            SharedHandle = br.ReadUInt32();
            Width = br.ReadUInt32();
            Height = br.ReadUInt32();
            Format = br.ReadUInt32();
            Usage = br.ReadUInt32();
            Description = br.ReadBytes(256);
            PartnerId = br.ReadUInt32();
        }

        public byte[] ToByteArray()
        {
            var b = new byte[280];
            using (var ms = new MemoryStream(b))
            using (var bw = new BinaryWriter(ms))
            {
                bw.Write(SharedHandle);
                bw.Write(Width);
                bw.Write(Height);
                bw.Write(Format);
                bw.Write(Usage);
                bw.Write(Description, 0, Description.Length);
                bw.Write(PartnerId);
            }
            return b;
        }
    }
}
