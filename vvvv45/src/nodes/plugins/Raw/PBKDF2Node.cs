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
	[PluginInfo(Name = "PBKDF2", Category = "Raw", Help = "Generates a key using the PBKDF2 function.", Tags = "cryptography, key, password, RFC2898")]
    #endregion PluginInfo
   	public class RawPBKDF2Node : IPluginEvaluate, IPartImportsSatisfiedNotification
	{
		#region fields & pins
		[Input("Password")]
		public IDiffSpread<string> FPass;
		
		[Input("Salt", DefaultString="EveryhingYouKnowIsWrong")]
		public IDiffSpread<string> FKeySalt;
		
		[Input("Iteration Count", DefaultValue = 1000, MinValue=1)]
		public IDiffSpread<int> FKeyIterations;
		
		[Input("Key Length", DefaultValue = 24, MinValue=1)]
		public IDiffSpread<int> FKeyLength;
		
		[Output("Key")]
		public ISpread<Stream> FStreamOut;
		
		[Output("Error")]
		public ISpread<string> FErrorOut;
		
		[Import]
		protected ILogger FLogger;

		private readonly Spread<KeyGenerator> FKeyGens = new Spread<KeyGenerator>();
		
		#endregion fields & pins

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
			FStreamOut.ResizeAndDispose(spreadMax, () => new MemoryComStream());
			FKeyGens.SliceCount = spreadMax;
			
			if (FPass.IsChanged || FKeyLength.IsChanged || FKeyIterations.IsChanged || FKeySalt.IsChanged)
			{
				for (int i = 0; i < spreadMax; i++) 
				{
					var outputStream=FStreamOut[i];
					
					try
					{	
						FKeyGens[i]=new KeyGenerator();
						var key=FKeyGens[i].GenerateKey (FPass[i], FKeyLength[i], FKeyIterations[i], FKeySalt[i]);
						outputStream.SetLength(0);
						outputStream.Write(key, 0, key.Length);
					}
					catch (Exception e)
					{
						FLogger.Log(e);
						outputStream.SetLength(0);
						FErrorOut[i] = e.Message;
					}
				}
				
				//this will force the changed flag of the output pin to be set
				FStreamOut.Flush(true);
			}
		}
	}
    
    #region helper class
    public class KeyGenerator
	{	
		public byte[] GenerateKey(string pass, int keyLength, int keyIterations, string keySalt)
		{
			if (keyLength > 0)
			{
				// Salt for the Key
				byte[] salt = new System.Text.UTF8Encoding(true).GetBytes(keySalt);
	
				//Generate Key
				Rfc2898DeriveBytes key = new Rfc2898DeriveBytes(pass, salt, keyIterations);
	
				//Take only 24 bytes
				return key.GetBytes(keyLength);
			}
			return new byte[1];
		}
	}
    #endregion helper class
    	
}
