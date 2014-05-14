#include "POVRenderer.h"
#include "../../../SyntopiaCore/Math/Vector3.h"

using namespace SyntopiaCore::Math;

#include "../../../SyntopiaCore/Logging/Logging.h"

using namespace SyntopiaCore::Logging;

namespace StructureSynth {
	namespace Model {	
		namespace Rendering {

			void POVRenderer::writeline(QString text) const {
				output += text + "\r\n";
			};

			void POVRenderer::write(QString text) const {
				output += text;
			};

			void POVRenderer::drawBox(SyntopiaCore::Math::Vector3f base, 
				SyntopiaCore::Math::Vector3f dir1 , 
				SyntopiaCore::Math::Vector3f dir2, 
				SyntopiaCore::Math::Vector3f dir3,  const QString &) 
			{
				writeline("object {   ");                               
				writeline("  box { <0,  0.0, 0>, <1,  1,  1> }");
				writeline(QString("matrix < %1, %2, %3, %4, %5, %6, %7, %8, %9, %10, %11, %12 > ")
					.arg(dir1.x()).arg(dir1.y()).arg(dir1.z())
					.arg(dir2.x()).arg(dir2.y()).arg(dir2.z())
					.arg(dir3.x()).arg(dir3.y()).arg(dir3.z())
					.arg(base.x()).arg(base.y()).arg(base.z()));
				writeline(
					QString("  texture { pigment { color rgbt <%1,%2,%3,%4> } finish { DEFFIN } normal { DEFNOR } }")
					.arg(rgb.x()).arg(rgb.y()).arg(rgb.z()).arg(1-alpha));
				writeline("}");

			};

			void POVRenderer::drawGrid(SyntopiaCore::Math::Vector3f /*base*/, 
				SyntopiaCore::Math::Vector3f /*dir1*/ , 
				SyntopiaCore::Math::Vector3f /*dir2*/, 
				SyntopiaCore::Math::Vector3f /*dir3*/,  const QString &) {
					// TODO
			};

			void POVRenderer::drawLine(SyntopiaCore::Math::Vector3f /*from*/, SyntopiaCore::Math::Vector3f /* to*/,  const QString &) {
				// TODO
			};

			void POVRenderer::drawDot(SyntopiaCore::Math::Vector3f /*v*/,  const QString &) {
				// TODO	
			};

			void POVRenderer::drawSphere(SyntopiaCore::Math::Vector3f center, float radius,  const QString &) {
				writeline("object {   ");                               
				writeline(QString("  sphere { <%1, %2, %3>, %4 }")
					.arg(center.x()).arg(center.y()).arg(center.z()).arg(radius));
				writeline(
					QString("  texture { pigment { color rgbt <%1,%2,%3,%4> } finish { DEFFIN } normal { DEFNOR } }")
					.arg(rgb.x()).arg(rgb.y()).arg(rgb.z()).arg(1-alpha));
				writeline("}");
			};

			void POVRenderer::begin() {
				writeline("// Global settings");
				writeline("global_settings {");
				writeline("  max_trace_level 5");
				writeline("  ambient_light rgb <1,1,1>");
				writeline("}");
				writeline("");
				writeline("// Finish and normal");
				writeline("#declare DEFFIN = finish { ambient 0.1 diffuse 0.5 specular 0.5 };");
				writeline("#declare DEFNOR = normal { dents 0 scale 0.01 };");
					writeline("");
				writeline("// Background");
				writeline("plane {");
				writeline("  z, 100.0");
				writeline("  texture {");
				writeline("    pigment { color rgb <0.0,0.0,0.0> }");
				writeline("    finish { ambient 1 }");
				writeline("  }");
				writeline("  hollow");
				writeline("}");
				writeline("");
				writeline("// Camera");
				writeline("camera {");
				writeline("  location <-0.0,0.0,-15.0>");
				writeline("  look_at <-0.0,-0.0,-0.0>");
				writeline("  right -x");
				writeline("  up y");
				writeline("  angle 60");
				writeline("}");
				writeline("");
				writeline("// Lights");
				writeline("light_source { <500,500,-1000> rgb <1,1,1> shadowless } ");
				writeline("light_source { <-500,-500,-1000> rgb <1,1,1> shadowless } ");
				writeline("light_source { <-500,500,1000> rgb <1,1,1> shadowless } ");
				writeline("");
			};

			void POVRenderer::end() {
				// TODO
			};

			void POVRenderer::setBackgroundColor(SyntopiaCore::Math::Vector3f /*rgb*/) {
				// TODO
			}

		}
	}
}

