/*
 * Erstellt mit SharpDevelop.
 * Benutzer: TF
 * Datum: 04.08.2010
 * Zeit: 00:40
 * 
 * Sie können diese Vorlage unter Extras > Optionen > Codeerstellung > Standardheader ändern.
 */
using System;
using VVVV.PluginInterfaces.V1;

namespace VVVV.PluginInterfaces.V2.Output
{
	/// <summary>
	/// Description of GenericOutputPin.
	/// </summary>
	public class GenericOutputPin<T> : Pin<T>, IPinUpdater, IGenericIO<T>
	{
		protected INodeOut FNodeOut;
		protected IGenericIO<T> FUpstreamInterface;
		protected T[] FData;
		protected bool FChanged;
		
		public GenericOutputPin(IPluginHost host, OutputAttribute attribute)
		{
			host.CreateNodeOutput(attribute.Name, attribute.SliceMode, attribute.Visibility, out FNodeOut);
			FNodeOut.SetSubType(GenericNodeIO<T>.GUID, GenericNodeIO<T>.FriendlyName);
			FNodeOut.SetInterface(this);
			
			FNodeOut.SetPinUpdater(this);
			FData = new T[1];

		}
		
		public override int SliceCount 
		{
			get
			{
				return FData.Length;
			}
			set
			{
				if (FData.Length != value)
					FData = new T[value];
				
				FNodeOut.SliceCount = value;
			}
		}
		
		public override T this[int index] 
		{
			get 
			{
				throw new NotImplementedException();
			}
			set 
			{
				FData[index % FData.Length] = value;
				FChanged = true;
			}
		}
		
		public T GetSlice(int slice)
		{
			return FData[slice % FData.Length];
		}
		
		public override void Update()
		{
			if(FChanged) FNodeOut.MarkPinAsChanged();
			FChanged = false;
		}
	}
}
