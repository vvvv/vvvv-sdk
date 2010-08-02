/*
 * Erstellt mit SharpDevelop.
 * Benutzer: TF
 * Datum: 03.08.2010
 * Zeit: 00:03
 * 
 * Sie können diese Vorlage unter Extras > Optionen > Codeerstellung > Standardheader ändern.
 */
using System;
using VVVV.PluginInterfaces.V1;

namespace VVVV.PluginInterfaces.V2.Output
{
	/// <summary>
	/// Description of EnumOutputPin.
	/// </summary>
	public class EnumOutputPin<T> : Pin<T>
	{
		protected IEnumOut FEnumConfigPin;
		protected Type FEnumType;
		
		public EnumOutputPin(IPluginHost host, OutputAttribute attribute)
		{
			FEnumType = typeof(T);
			
			var entrys = Enum.GetNames(FEnumType);
			var defEntry = (attribute.DefaultEnumEntry != "") ? attribute.DefaultEnumEntry : entrys[0];	
			host.UpdateEnum(FEnumType.Name, defEntry, entrys);
			
			host.CreateEnumOutput(attribute.Name, attribute.SliceMode, attribute.Visibility, out FEnumConfigPin);
			FEnumConfigPin.SetSubType(FEnumType.Name);
		}

		public override int SliceCount 
		{
			get
			{
				return FEnumConfigPin.SliceCount;
			}
			set
			{
				FEnumConfigPin.SliceCount = value;
			}
		}

		public override T this[int index]
		{
			get
			{
				throw new NotSupportedException();
			}
			set
			{
				FEnumConfigPin.SetString(index, value.ToString());
			}
		}
	}
}
