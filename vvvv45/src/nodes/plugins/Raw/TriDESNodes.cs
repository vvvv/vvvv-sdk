using System;
using System.IO;
using System.ComponentModel.Composition;
using System.Runtime.InteropServices;
using System.Linq;
using System.Security.Cryptography;
using VVVV.PluginInterfaces.V2;
using VVVV.Core.Logging;

namespace VVVV.Nodes.Raw
{
	
	#region PluginInfo
	[PluginInfo(Name = "Encrypt", Category = "Raw", Version = "3DES", Help = "Encrypts a raw stream using the Triple DES algorithm.", Tags = "cryptography, triDES, DESede, triple")]
	#endregion PluginInfo
	public class RawEncryptNode : RawEncryptDecryptNode
	{
		public RawEncryptNode() : base(true)
		{
		}
	}
	
	
	#region PluginInfo
	[PluginInfo(Name = "Decrypt", Category = "Raw", Version = "3DES", Help = "Decrypts a raw stream using the Triple DES algorithm.", Tags = "cryptography, triDES, DESede, triple")]
	#endregion PluginInfo
	public class RawDecryptNode : RawEncryptDecryptNode
	{
		public RawDecryptNode() : base(false)
		{
		}
	}
	
    public class RawEncryptDecryptNode : IPluginEvaluate, IPartImportsSatisfiedNotification
	{
		#region fields & pins
		[Input("Input")]
		public ISpread<Stream> FStreamIn;
		
		[Input("Key")]
		public ISpread<Stream> FKey;
		
		[Input("Initialization Vector")]
		public ISpread<Stream> FIV;
		
		[Input("Cipher Mode", DefaultEnumEntry = "CBC")]
        public IDiffSpread<CipherMode> FCipherMode;
		
		[Input("Padding Mode", DefaultEnumEntry = "PKCS7")]
        public IDiffSpread<PaddingMode> FPaddingMode;
		
		[Output("Output")]
		public ISpread<Stream> FStreamOut;
		
		[Output("Error")]
		public ISpread<string> FErrorOut;
		
		[Import]
		protected ILogger FLogger;

		private readonly Spread<EncryptDecrypt> FEncryptors = new Spread<EncryptDecrypt>();
		private readonly bool FIsEncrypt;
		
		//when dealing with byte streams (what we call Raw in the GUI) it's always
		//good to have a byte buffer around. we'll use it when copying the data.
		private readonly byte[] FBuffer = new byte[4096];
		#endregion fields & pins
		
		public RawEncryptDecryptNode(bool isEncrypt)
		{
			FIsEncrypt = isEncrypt;
		}

		//called when all inputs and outputs defined above are assigned from the host
		public void OnImportsSatisfied()
		{
			//start with an empty stream output
			FStreamOut.SliceCount = 0;
		}

		//called when data for any output pin is requested
		public void Evaluate(int spreadMax)
		{
			//ResizeAndDispose will adjust the spread length and thereby call
			//the given constructor function for new slices and Dispose on old
			//slices.
			FStreamOut.ResizeAndDispose(spreadMax, () => new LeaveOpenStream());
			FEncryptors.SliceCount = spreadMax;
			FErrorOut.SliceCount = spreadMax;
			
			for (int i = 0; i < spreadMax; i++)
			{
				var encryptor = FEncryptors[i];
				var key = FKey[i].ToKeyBytes();
				var iv = FIV[i].ToIvBytes();
				if (encryptor == null || !key.SequenceEqual(encryptor.Key) || !iv.SequenceEqual(encryptor.IV))
				{
					try
					{
						encryptor = new EncryptDecrypt(key, iv, FCipherMode[i], FPaddingMode[i]);
						FEncryptors[i] = encryptor;
					}
					catch (Exception e)
					{
						encryptor = null;
						FErrorOut[i] = e.Message;
					}
				}

				var outputStream = FStreamOut[i];
				var inputStream = FStreamIn[i];
				
				if (encryptor != null && inputStream.Length>0)
				{	
					try
						{	
							inputStream.Position = 0;
							outputStream.Position = 0;
							encryptor.EncryptOrDecrypt(inputStream, outputStream, FIsEncrypt, FBuffer);
							FErrorOut[i] = string.Empty;
						}
						catch (Exception e)
						{
							FLogger.Log(e);
							outputStream.SetLength(0);
							FErrorOut[i] = e.Message;
						}
				}
				else
				{
					outputStream.SetLength(0);
				}
				FStreamOut[i] = outputStream;
			}
		}
	}

    
    #region helper classes
    
	class LeaveOpenStream : MemoryStream
	{
		protected override void Dispose(bool disposing)
		{
			// Do nothing!
		}
		
		public override void Close()
		{
			// Do nothing!
		}
	}
	
	public static class StreamHelpers
	{
		private static readonly byte[] DefaultKey = new byte[24];
		private static readonly byte[] DefaultIv = new byte[8];
		
		static StreamHelpers()
		{
			for	(int i = 0; i < DefaultKey.Length; i++)
				DefaultKey[i] = (byte)i;
		}
		
		public static byte[] ToBytes(this Stream stream, byte[] defaultValue)
		{
			if (stream == null || stream.Length == 0)
				return defaultValue;
			stream.Position = 0;
			var result = new byte[stream.Length];
			stream.Read(result, 0, result.Length);
			return result;
		}
		
		public static byte[] ToKeyBytes(this Stream stream)
		{
			return stream.ToBytes(DefaultKey);
		}
		
		public static byte[] ToIvBytes(this Stream stream)
		{
			return stream.ToBytes(DefaultIv);
		}
	}
	
	public class EncryptDecrypt
	{
		public readonly byte[] Key;
		public readonly byte[] IV;
		// TripleDES service provider
		private TripleDESCryptoServiceProvider tdes;

		// Encryptor/Decryptor Objects
		private ICryptoTransform encryptor;
		private ICryptoTransform decryptor;

		public EncryptDecrypt(byte[] key, byte[] iv, CipherMode cm, PaddingMode pm)
		{
			Key = key;
			IV = iv;
			
			// Set up tripple DES service provider
			tdes = new TripleDESCryptoServiceProvider();
			tdes.Mode = cm;
			tdes.Padding = pm;
			tdes.IV = iv;

			// Create encryptor/decryptor Objects
			encryptor = tdes.CreateEncryptor(key, iv);
			decryptor = tdes.CreateDecryptor(key, iv);
		}

		private void Encrypt(Stream dataStream, Stream destinationStream, byte[] buffer)
		{
			destinationStream.SetLength(0);
			using (var cryptoStream = new CryptoStream(destinationStream, encryptor, CryptoStreamMode.Write))
				dataStream.CopyTo(cryptoStream, buffer);
		}

		private void Decrypt(Stream dataStream, Stream destinationStream, byte[] buffer)
		{
			destinationStream.SetLength(0);
			using (var cryptoStream = new CryptoStream(dataStream, decryptor, CryptoStreamMode.Read))
				cryptoStream.CopyTo(destinationStream, buffer);
		}

		public void EncryptOrDecrypt(Stream dataStream, Stream destinationStream, bool encrypt, byte[] buffer)
		{
			if (encrypt)
				Encrypt(dataStream, destinationStream, buffer);
			else
				Decrypt(dataStream, destinationStream, buffer);
		}
	}
	
	#endregion helper classes

}
