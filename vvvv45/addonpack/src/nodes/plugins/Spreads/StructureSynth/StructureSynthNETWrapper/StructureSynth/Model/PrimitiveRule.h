#pragma once

#include "Rule.h"

namespace StructureSynth {
	namespace Model {	

		/// These are the built-in primitives,
		/// for drawing boxes, spheres and other simple geometric shapes.
		class PrimitiveRule : public Rule {
			public:
				enum PrimitiveType { Box, Sphere, Dot, Grid, Cylinder, Line, Mesh, Other } ;
				
				PrimitiveRule(PrimitiveType type);
				virtual void apply(Builder* builder) const;
	
				/// Returns a list over rules that this rule references.
				/// (Empty for all PrimitiveRules!)
				virtual QList<RuleRef*> getRuleRefs() const { return QList<RuleRef*>(); }

				/// 'class' is an identifier used for distinguishing between
				/// different forms of the the same PrimiteType.
				/// This is used together with Template Renderers.
				///
				/// For instance 'box::metal' will be parsed in to a 'box' primitive with a 'metal' class identifier.
				void setClass(QString classID) { this->classID = classID; }
				QString getClass() { return classID; }
		    protected:
			    QString classID;
			private:
				PrimitiveType type;
				
		};

		/// Triangle rules are special, since they have explicit coordinate representation.
		class TriangleRule : public PrimitiveRule {
			public:
				
				TriangleRule(SyntopiaCore::Math::Vector3f p1,
					          SyntopiaCore::Math::Vector3f p2,
							   SyntopiaCore::Math::Vector3f p3);

				virtual void apply(Builder* builder) const;
	
			private:
				SyntopiaCore::Math::Vector3f p1;
				SyntopiaCore::Math::Vector3f p2;
				SyntopiaCore::Math::Vector3f p3;
				
		};

	}
}

