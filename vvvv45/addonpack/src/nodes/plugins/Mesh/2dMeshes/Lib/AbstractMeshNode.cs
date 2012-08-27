using System;
using System.Collections.Generic;
using System.Text;
using VVVV.PluginInterfaces.V1;
using SlimDX.Direct3D9;
using SlimDX;

namespace VVVV.Lib
{
    public enum e2dMeshTextureMapping { Stretch, Crop }

    public unsafe abstract class AbstractMeshNode
    {
        #region Fields and abstract stuff
        protected IPluginHost FHost;

        protected List<Vertex[]> FVertex = new List<Vertex[]>();
        protected List<short[]> FIndices = new List<short[]>();
        protected bool FInvalidate;

        private IDXMeshOut FPinOutMesh;
        private Dictionary<Device, Mesh> FMeshes = new Dictionary<Device, Mesh>();

        protected abstract void SetInputs();
        #endregion

        #region Set Plugin Host
        public void SetPluginHost(IPluginHost Host)
        {
            //assign host
            this.FHost = Host;

            this.SetInputs();

            this.FHost.CreateMeshOutput("Output", TSliceMode.Dynamic, TPinVisibility.True, out this.FPinOutMesh);
        }
        #endregion

        #region Auto Evaluate
        public bool AutoEvaluate
        {
            get { return true; }
        }
        #endregion

        #region Configurate
        public void Configurate(IPluginConfig Input)
        {

        }
        #endregion

        #region Dispose
        public void Dispose()
        {
        }
        #endregion

        #region IPluginDXResource Members
        public void DestroyResource(IPluginOut ForPin, Device OnDevice, bool OnlyUnManaged)
        {
            try
            {
                if (this.FMeshes.ContainsKey(OnDevice))
                {
                    Mesh m = this.FMeshes[OnDevice];
                    this.FMeshes.Remove(OnDevice); 
                    m.Dispose();
                }
            }
            catch
            {

            }
        }

        private void RemoveResource(Device OnDevice)
        {
            Mesh m = FMeshes[OnDevice];
            FMeshes.Remove(OnDevice);
            m.Dispose();
        }

        public void UpdateResource(IPluginOut ForPin, Device OnDevice)
        {
            if (ForPin == this.FPinOutMesh)
            {
                bool update = this.FMeshes.ContainsKey(OnDevice);
                if (update && this.FInvalidate)
                {
                    RemoveResource(OnDevice);
                }

                if (!update)
                {
                    this.FInvalidate = true;
                }

                if (this.FInvalidate)
                {
                    List<Mesh> meshes = new List<Mesh>();

                    for (int i = 0; i < this.FVertex.Count; i++)
                    {

                        Mesh mesh = new Mesh(OnDevice, this.FIndices[i].Length / 3, this.FVertex[i].Length, MeshFlags.Dynamic | MeshFlags.WriteOnly, Vertex.Format);
                        DataStream vS = mesh.LockVertexBuffer(LockFlags.Discard);
                        DataStream iS = mesh.LockIndexBuffer(LockFlags.Discard);

                        vS.WriteRange(this.FVertex[i]);
                        iS.WriteRange(this.FIndices[i]);

                        mesh.UnlockVertexBuffer();
                        mesh.UnlockIndexBuffer();

                        meshes.Add(mesh);
                    }

                    Mesh merge = null;
                    if (OnDevice is DeviceEx)
                    {
                        merge = Mesh.Concatenate(OnDevice, meshes.ToArray(), MeshFlags.Use32Bit);
                    }
                    else
                    {
                        merge = Mesh.Concatenate(OnDevice, meshes.ToArray(), MeshFlags.Use32Bit | MeshFlags.Managed);
                    }

                    this.FMeshes.Add(OnDevice, merge);

                    foreach (Mesh m in meshes)
                    {
                        m.Dispose();
                    }
                }
            }
        }
        #endregion

        #region IPluginDXMesh Members
        public Mesh GetMesh(IDXMeshOut ForPin, Device OnDevice)
        {
            //in case the plugin has several mesh outputpins a test for the pin can be made here to get the right mesh.
            if (ForPin == this.FPinOutMesh && this.FMeshes.ContainsKey(OnDevice))
            	return this.FMeshes[OnDevice];
            else
            	return null;
        }

        #endregion

        protected void InvalidateMesh(int slicecount)
        {
            this.FInvalidate = true;

            this.FPinOutMesh.SliceCount = slicecount;
            this.FPinOutMesh.MarkPinAsChanged();
        }

    }
}
