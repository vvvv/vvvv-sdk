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

				static array<Guid> ^ SingleGuidArray(Guid^ uid) 
				{
					array<Guid> ^ arr1d = gcnew array<Guid>(1);
					arr1d->SetValue(uid,0);

					return arr1d;
				}

		};
	}
}
