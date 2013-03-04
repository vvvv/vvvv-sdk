#pragma once

#include <QString>
#include <QVector>
#include <QStringList>
#include <cmath>


namespace SyntopiaCore {
	namespace Math {	

		/// A simple class for representing three-dimensional vectors.
		template <class scalar> class Vector3 {
		public:
			Vector3() { s[0] = 0; s[1] = 0; s[2] = 0;}
			Vector3(scalar x, scalar y, scalar z) { s[0] = x; s[1] = y; s[2] = z; }

			// Constructor. Parses input such as "[1 2 3]";
			Vector3(QString input, bool& succes2) {
				input.remove('[');
				input.remove(']');

				QStringList sl = input.split(" ");
				if (sl.size() != 3) { succes2 = false; return; }

				bool succes = false;
				float f;
				f = sl[0].toFloat(&succes); if (!succes)  { succes2 = false; return; }; s[0] = f;
				f = sl[1].toFloat(&succes); if (!succes)  { succes2 = false; return; }; s[1] = f;
				f = sl[2].toFloat(&succes); if (!succes)  { succes2 = false; return; }; s[2] = f;

				succes2 = true;
			}

		
			// data access
			scalar x() const { return s[0]; }
			scalar y() const { return s[1]; };
			scalar z() const { return s[2]; };
			scalar& x() { return s[0]; }
			scalar& y() { return s[1]; };
			scalar& z() { return s[2]; };
			scalar operator[] (int index) const { return s[index]; };
			scalar& operator[] (int index) { return s[index]; };

			scalar sqrLength() const { return s[0]*s[0]+s[1]*s[1]+s[2]*s[2]; }
			scalar length() const { return sqrt(s[0]*s[0]+s[1]*s[1]+s[2]*s[2]); }

			Vector3<scalar> normalize() const { scalar l = length(); return Vector3<scalar>(s[0]/l,s[1]/l,s[2]/l); }
			Vector3<scalar> operator- (const Vector3<scalar>& rhs) const { return Vector3<scalar>(s[0]-rhs.s[0], s[1]-rhs.s[1], s[2]-rhs.s[2]); }
			Vector3<scalar> operator+ (const Vector3<scalar>& rhs) const { return Vector3<scalar>(s[0]+rhs.s[0], s[1]+rhs.s[1], s[2]+rhs.s[2]); }
			Vector3<scalar> operator- () const { return Vector3<scalar>(-s[0], -s[1], -s[2]); }

			Vector3<scalar> operator* (scalar rhs) const { return Vector3<scalar>(s[0]*rhs, s[1]*rhs, s[2]*rhs); }
			Vector3<scalar> operator/ (scalar rhs) const { return Vector3<scalar>(s[0]/rhs, s[1]/rhs, s[2]/rhs); }

			QString toString() const {
				return QString("[%1 %2 %3]").arg(s[0]).arg(s[1]).arg(s[2]);
			}

			Vector3<scalar> cross(const Vector3<scalar> b) const { 
				return Vector3<scalar>(
					s[1]*b.s[2] - s[2]*b.s[1] ,
					s[2]*b.s[0] - s[0]*b.s[2] ,
					s[0]*b.s[1] - s[1]*b.s[0]);
			}

		private:
			scalar s[3];
		};

		typedef Vector3<float> Vector3f ;
		typedef Vector3<double> Vector3d ;

	}
}

