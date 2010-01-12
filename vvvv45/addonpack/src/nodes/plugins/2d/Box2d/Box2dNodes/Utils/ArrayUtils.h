#pragma once

namespace VVVV 
{
	namespace Utils 
	{
		public ref class ArrayUtils
		{
			public:
				static array<String ^> ^ Array1D() 
				{
					array<String ^> ^ arr1d = gcnew array<String ^>(1);
					arr1d->SetValue("X",0);

					return arr1d;
				}

				static array<String ^> ^ Array2D() 
				{
					array<String ^> ^ arr = gcnew array<String ^>(2);
					arr->SetValue("X",0);
					arr->SetValue("Y",1);
					return arr;
				}

				static array<String ^> ^ Array3D() 
				{
					array<String ^> ^ arr = gcnew array<String ^>(3);
					arr->SetValue("X",0);
					arr->SetValue("Y",1);
					arr->SetValue("Z",2);
					return arr;
				}

				static array<String ^> ^ Array4D() 
				{
					array<String ^> ^ arr = gcnew array<String ^>(4);
					arr->SetValue("X",0);
					arr->SetValue("Y",1);
					arr->SetValue("Z",2);
					arr->SetValue("W",3);
					return arr;
				}

				static array<Guid> ^ SingleGuidArray(Guid^ uid) 
				{
					array<Guid> ^ arr1d = gcnew array<Guid>(1);
					arr1d->SetValue(uid,0);

					return arr1d;
				}

				static array<Guid> ^ DoubleGuidArray(Guid^ uid1,Guid^ uid2) 
				{
					array<Guid> ^ arr1d = gcnew array<Guid>(2);
					arr1d->SetValue(uid1,0);
					arr1d->SetValue(uid2,1);

					return arr1d;
				}

		};
	}
}
