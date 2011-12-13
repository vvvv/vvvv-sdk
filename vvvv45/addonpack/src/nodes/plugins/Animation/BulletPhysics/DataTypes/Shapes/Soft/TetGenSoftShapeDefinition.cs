using System;
using System.Collections.Generic;
using System.Text;
using BulletSharp.SoftBody;
using BulletSharp;

namespace VVVV.DataTypes.Bullet
{
	public class TetGenDataSoftShapeDefinition : AbstractSoftShapeDefinition
	{
		private string elements;
		private string faces;
		private string nodes;
		private bool facelinks;
		private bool tetralinks;
		private bool facesfromtetra;

		public TetGenDataSoftShapeDefinition(string elements, string faces, string nodes,
			bool facelinks, bool tetralinks, bool facesfromtetra)
		{
			this.elements = elements;
			this.faces = faces;
			this.nodes = nodes;
			this.tetralinks = tetralinks;
			this.facelinks = facelinks;
			this.facesfromtetra = facesfromtetra;
		}
		
		protected override SoftBody CreateSoftBody(SoftBodyWorldInfo si)
		{
			si.SparseSdf.Reset();

			SoftBody sb = SoftBodyHelpers.CreateFromTetGenData(si, this.elements, this.faces, this.nodes,
				this.facelinks, this.tetralinks, this.facesfromtetra);

			this.SetConfig(sb);
	
			return sb;
		}


	}
}



