using System;
using System.Collections.Generic;
using System.Text;
using SlimDX.Direct3D9;

namespace VVVV.Internals.Bullet.EX9
{
	public class BulletMesh : IDisposable
	{
		private List<Mesh> meshes = new List<Mesh>();

		public BulletMesh(Mesh mesh)
		{
			meshes.Add(mesh);
		}

		public BulletMesh(IEnumerable<Mesh> meshes)
		{
			this.meshes.AddRange(meshes);
		}

		public IEnumerable<Mesh> Meshes
		{
			get { return this.meshes; }
		}


		public void Dispose()
		{
			foreach (Mesh m in this.meshes)
			{
				m.Dispose();
			}
		}
	}
}
