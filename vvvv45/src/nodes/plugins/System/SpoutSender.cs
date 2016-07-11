using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Threading;
using System.Runtime.InteropServices;

//adapted from: https://github.com/joreg/SpoutCSharp

namespace Spout
{
    public class SpoutSender: IDisposable
    {
        protected const string SenderNamesMMF = "SpoutSenderNames";
        protected const string ActiveSenderMMF = "";
        protected const int SpoutWaitTimeout = 100;
        public const int MaxSenders = 10;
        public const int SenderNameLength = 256;

        protected TextureDesc FTextureDesc;
        protected string FSenderName;
    	protected MemoryMappedFile FSharedMemory;
        protected MemoryMappedViewStream FSharedMemoryVS;

        public SpoutSender(string senderName, uint sharedHandle, uint width, uint height, uint format, uint usage)
        {
            FSenderName = senderName;
            FTextureDesc = new TextureDesc(sharedHandle, width, height, format, usage, new byte[256]);
        }

        public void Dispose()
        {
            RemoveNameFromSendersList(FSenderName);
        	
        	if (FSharedMemoryVS != null)
                FSharedMemoryVS.Dispose();
            if (FSharedMemory != null)
                FSharedMemory.Dispose();
        }

        public bool Initialize()
        {
            if (AddNameToSendersList(FSenderName))
            {
    			FSharedMemory = MemoryMappedFile.CreateNew(FSenderName, 280);
    			FSharedMemoryVS = FSharedMemory.CreateViewStream();
                var nameBytes = Encoding.Unicode.GetBytes(FSenderName);
                Array.Copy(nameBytes, 0, FTextureDesc.Description, 0, nameBytes.Length);
                var desc = FTextureDesc.ToByteArray();
                FSharedMemoryVS.Write(desc, 0, desc.Length);
            	return true;
            }
        	return false;
        }

        public static bool AddNameToSendersList(string name)
        {
            bool createdNew;
            var mutex = new Mutex(true, SenderNamesMMF + "_mutex", out createdNew);
            if (mutex == null)
                return false;
            var success = false;
            try
            {
                if (mutex.WaitOne(SpoutWaitTimeout))
                {
                    success = true;
                }
                else
                {
                    success = false;
                }
            }
            catch (AbandonedMutexException e)
            {
                success = true;    
            }
            finally
            {
                if (success)
                {
                    List<string> senders = GetSenderNames();
                    if (senders.Contains(name))
                    {
                        success = false;
                    }
                    else
                    {
                        senders.Add(name);
                        WriteSenderNamesToMMF(senders);
                    }
                }
                mutex.ReleaseMutex();
                mutex.Dispose();
            }
            return success;
        }

        public static void RemoveNameFromSendersList(string name)
        {
            bool createdNew;
            var mutex = new Mutex(true, SenderNamesMMF + "_mutex", out createdNew);
            if (mutex == null)
                return;
            try
            {
                mutex.WaitOne(SpoutWaitTimeout);
            }
            catch (AbandonedMutexException e)
            {
                //Log.Add(e);     
            }
            finally
            {
                var senders = GetSenderNames();
                if (senders.Contains(name))
                {
                    senders.Remove(name);
                    WriteSenderNamesToMMF(senders);
                }
                mutex.ReleaseMutex();
                mutex.Dispose();
            }
        }

        static void WriteSenderNamesToMMF(List<string> senders)
        {
            var len = SenderNameLength * MaxSenders;
            using (var mmf = MemoryMappedFile.CreateOrOpen(SenderNamesMMF, len))
        	{
        		using (var mmfVS = mmf.CreateViewStream())
        		{
		            var count = 0;
		            for (int i = 0; i < senders.Count; i++)
		            {
		                var nameBytes = GetNameBytes(senders[i]);
		                mmfVS.Write(nameBytes, 0, nameBytes.Length);
		                count += nameBytes.Length;
		            }
		            var b = new byte[len - count];
		            mmfVS.Write(b, 0, b.Length);
        		}
        	}
        }

        public static List<string> GetSenderNames()
        {
        	var namesList = new List<string>();
		    var name = new StringBuilder();
            var len = MaxSenders * SenderNameLength;
            
        	//Read the memory mapped file in to a byte array and close this shit.
        	using (var mmf = MemoryMappedFile.CreateOrOpen(SenderNamesMMF, len))
        	{
        		using (var mmfVS = mmf.CreateViewStream())
        		{
		            var b = new byte[len];
		            mmfVS.Read(b, 0, len);

		            //split into strings searching for the nulls 
		            for (int i = 0; i < len; i++)
		            {
		                if (b[i] == 0)
		                {
		                    if (name.Length == 0)
		                    {
		                        i += SenderNameLength - (i % SenderNameLength) - 1;
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

        protected static byte[] GetNameBytes(string name)
        {
            var b = new byte[SenderNameLength];
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

        public TextureDesc(uint sharedHandle, uint width, uint height, uint format, uint usage, byte[] description)
        {
            SharedHandle = sharedHandle;
            Width = width;
            Height = height;
            Format = format;
            Usage = usage;
            Description = description;
        }

        public TextureDesc(System.IO.MemoryMappedFiles.MemoryMappedViewStream mmvs)
        {
            var br = new BinaryReader(mmvs);
            SharedHandle = br.ReadUInt32();
            Width = br.ReadUInt32();
            Height = br.ReadUInt32();
            Format = br.ReadUInt32();
            Usage = br.ReadUInt32();
            Description = br.ReadBytes(256);
        }

        public byte[] ToByteArray()
        {
            var b = new byte[280];
            var ms = new MemoryStream(b);
            var bw = new BinaryWriter(ms);
            bw.Write(SharedHandle);
            bw.Write(Width);
            bw.Write(Height);
            bw.Write(Format);
            bw.Write(Usage);
            bw.Write(Description,0, Description.Length);
            bw.Dispose();
            ms.Dispose();
            return b;
        }
    }
}
