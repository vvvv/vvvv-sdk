#include "stdafx.h"
#include "TemplateRenderer.h"
#include "../../../SyntopiaCore/Math/Vector3.h"
#include "../../../SyntopiaCore/Logging/Logging.h"
#include "../../../SyntopiaCore/Exceptions/Exception.h"

#include <QDomDocument>
#include <QIODevice>
#include <QFile>
#include <QMap>

using namespace SyntopiaCore::Math;
using namespace SyntopiaCore::Logging;

namespace StructureSynth {
	namespace Model {	
		namespace Rendering {

			class Template {
			public:
				Template() {};
				Template(QString def) : def(def) {};
				Template(const Template& t) { this->def = t.def; };

				QString getText() { return def; }

				void substitute(QString before, QString after) {
					def.replace(before, after);
				};

				bool contains(QString input) {
					return def.contains(input);
				};

			private:
				QString def;
			};

			TemplateRenderer::TemplateRenderer(QString xmlDefinitionFile) {
				counter = 0;
				QDomDocument doc;
				QFile file(xmlDefinitionFile);
				if (!file.open(QIODevice::ReadOnly)) {
					WARNING("Unable to open file: " + xmlDefinitionFile);
					return;
				}
				if (!doc.setContent(&file)) {
					WARNING("Unable to parsefile: " + xmlDefinitionFile);
					file.close();
					return;
				}
				file.close();

				QDomElement docElem = doc.documentElement();

				QDomNode n = docElem.firstChild();
				while(!n.isNull()) {
					QDomElement e = n.toElement(); // try to convert the node to an element.
					if(!e.isNull()) {
						if (e.tagName() != "substitution") {
							WARNING("Expected 'substitution' element, found: " + e.tagName());
							continue;
						}
						if (!e.hasAttribute("name")) {
							WARNING("Substitution without name attribute found!");
							continue;
						}


						QString type = "";
						if (e.hasAttribute("type")) {
							type = "::" + e.attribute("type");
						}


						QString name = e.attribute("name") + type;
						INFO(QString("%1 = %2").arg(name).arg(e.text()));
						templates[name] = Template(e.text());
					}
					n = n.nextSibling();
				}

				
			}


			TemplateRenderer::TemplateRenderer()  
			{
				counter = 0;
			}

			TemplateRenderer::~TemplateRenderer() {
			}

			void TemplateRenderer::assertTemplateExists(QString templateName) {
				if (!templates.contains(templateName)) {
						throw SyntopiaCore::Exceptions::Exception(
							QString("Template error: the primitive '%1' is not defined.").arg(templateName));
					
				}
					
			} 

			void TemplateRenderer::drawBox(SyntopiaCore::Math::Vector3f base, 
				SyntopiaCore::Math::Vector3f dir1 , 
				SyntopiaCore::Math::Vector3f dir2, 
				SyntopiaCore::Math::Vector3f dir3,
				const QString& classID) 
			{
				QString alternateID = (classID.isEmpty() ? "" : "::" + classID);
				assertTemplateExists("box"+alternateID);
				Template t(templates["box"+alternateID]); 
				if (t.contains("{matrix}")) {
					QString mat = QString("%1 %2 %3 0 %4 %5 %6 0 %7 %8 %9 0 %10 %11 %12 1")
					.arg(dir1.x()).arg(dir1.y()).arg(dir1.z())
					.arg(dir2.x()).arg(dir2.y()).arg(dir2.z())
					.arg(dir3.x()).arg(dir3.y()).arg(dir3.z())
					.arg(base.x()).arg(base.y()).arg(base.z());
				
					t.substitute("{matrix}", mat);
				}
				

				if (t.contains("{uid}")) {
					t.substitute("{uid}", QString("Box%1").arg(counter++));
				}

				t.substitute("{r}", QString::number(rgb.x()));
				t.substitute("{g}", QString::number(rgb.y()));
				t.substitute("{b}", QString::number(rgb.z()));

				t.substitute("{alpha}", QString::number(alpha));
				t.substitute("{oneminusalpha}", QString::number(1-alpha));
				

				output.append(t.getText());

			};

			void TemplateRenderer::drawGrid(SyntopiaCore::Math::Vector3f base, 
				SyntopiaCore::Math::Vector3f dir1, 
				SyntopiaCore::Math::Vector3f dir2, 
				SyntopiaCore::Math::Vector3f dir3,
								const QString& classID) {

				QString alternateID = (classID.isEmpty() ? "" : "::" + classID);
				assertTemplateExists("grid"+alternateID);
				Template t(templates["grid"+alternateID]); 
				if (t.contains("{matrix}")) {
					QString mat = QString("%1 %2 %3 0 %4 %5 %6 0 %7 %8 %9 0 %10 %11 %12 1")
					.arg(dir1.x()).arg(dir1.y()).arg(dir1.z())
					.arg(dir2.x()).arg(dir2.y()).arg(dir2.z())
					.arg(dir3.x()).arg(dir3.y()).arg(dir3.z())
					.arg(base.x()).arg(base.y()).arg(base.z());
				
					t.substitute("{matrix}", mat);
				}
				

				if (t.contains("{uid}")) {
					t.substitute("{uid}", QString("Grid%1").arg(counter++));
				}

				t.substitute("{r}", QString::number(rgb.x()));
				t.substitute("{g}", QString::number(rgb.y()));
				t.substitute("{b}", QString::number(rgb.z()));

				t.substitute("{alpha}", QString::number(alpha));
				t.substitute("{oneminusalpha}", QString::number(1-alpha));
				

				output.append(t.getText());
			};

			void TemplateRenderer::drawLine(SyntopiaCore::Math::Vector3f from, SyntopiaCore::Math::Vector3f to,const QString& classID) {
				QString alternateID = (classID.isEmpty() ? "" : "::" + classID);
				assertTemplateExists("line"+alternateID);
				Template t(templates["line"+alternateID]); 
				t.substitute("{x1}", QString::number(from.x()));
				t.substitute("{y1}", QString::number(from.y()));
				t.substitute("{z1}", QString::number(from.z()));
				
				t.substitute("{x2}", QString::number(to.x()));
				t.substitute("{y2}", QString::number(to.y()));
				t.substitute("{z2}", QString::number(to.z()));

				t.substitute("{alpha}", QString::number(alpha));
				t.substitute("{oneminusalpha}", QString::number(1-alpha));
				
				if (t.contains("{uid}")) {
					t.substitute("{uid}", QString("Line%1").arg(counter++));
				}

				output.append(t.getText());
			};

			void TemplateRenderer::drawDot(SyntopiaCore::Math::Vector3f v,const QString& classID) {
				QString alternateID = (classID.isEmpty() ? "" : "::" + classID);
				assertTemplateExists("dot"+alternateID);
				Template t(templates["dot"+alternateID]); 
				t.substitute("{x}", QString::number(v.x()));
				t.substitute("{y}", QString::number(v.y()));
				t.substitute("{z}", QString::number(v.z()));
				
				t.substitute("{r}", QString::number(rgb.x()));
				t.substitute("{g}", QString::number(rgb.y()));
				t.substitute("{b}", QString::number(rgb.z()));

				t.substitute("{alpha}", QString::number(alpha));
				t.substitute("{oneminusalpha}", QString::number(1-alpha));
				
				if (t.contains("{uid}")) {
					t.substitute("{uid}", QString("Dot%1").arg(counter++));
				}

				output.append(t.getText());
			};

			void TemplateRenderer::drawSphere(SyntopiaCore::Math::Vector3f center, float radius,const QString& classID) {
				QString alternateID = (classID.isEmpty() ? "" : "::" + classID);
				assertTemplateExists("sphere"+alternateID);
				Template t(templates["sphere"+alternateID]); 
				t.substitute("{cx}", QString::number(center.x()));
				t.substitute("{cy}", QString::number(center.y()));
				t.substitute("{cz}", QString::number(center.z()));
				
				t.substitute("{rad}", QString::number(radius));

				t.substitute("{r}", QString::number(rgb.x()));
				t.substitute("{g}", QString::number(rgb.y()));
				t.substitute("{b}", QString::number(rgb.z()));

				t.substitute("{alpha}", QString::number(alpha));
				t.substitute("{oneminusalpha}", QString::number(1-alpha));
				
				if (t.contains("{uid}")) {
					t.substitute("{uid}", QString("Sphere%1").arg(counter++));
				}

				output.append(t.getText());
			};

			void TemplateRenderer::begin() {
				assertTemplateExists("begin");
				Template t(templates["begin"]); 
				output.append(t.getText());
			};

			void TemplateRenderer::end() {
				assertTemplateExists("end");
				Template t(templates["end"]); 
				output.append(t.getText());
			};

			void TemplateRenderer::setBackgroundColor(SyntopiaCore::Math::Vector3f /*rgb*/) {
				// TODO
			}

			void TemplateRenderer::drawMesh(  SyntopiaCore::Math::Vector3f /*startBase*/, 
										SyntopiaCore::Math::Vector3f /*startDir1*/, 
										SyntopiaCore::Math::Vector3f /*startDir2*/, 
										SyntopiaCore::Math::Vector3f /*endBase*/, 
										SyntopiaCore::Math::Vector3f /*endDir1*/, 
										SyntopiaCore::Math::Vector3f /*endDir2*/, 
										const QString& /*classID*/) {
			};

			void TemplateRenderer::callCommand(const QString& renderClass, const QString& /*command*/) {
				if (renderClass != this->renderClass()) return;
				
			}


		}
	}
}

