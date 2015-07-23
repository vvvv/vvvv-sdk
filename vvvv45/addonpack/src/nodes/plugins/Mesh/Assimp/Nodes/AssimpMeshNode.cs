using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VVVV.PluginInterfaces.V2;
using VVVV.PluginInterfaces.V1;
using SlimDX.Direct3D9;
using VVVV.Assimp.Lib;
using System.ComponentModel.Composition;
using SlimDX;

namespace VVVV.Assimp.Nodes
{
    [PluginInfo(Name = "Mesh", Category = "EX9.Geometry", Version = "Assimp DX9", Author = "vux, flateric")]
    public class AssimpMeshNode : IPluginEvaluate,IPluginDXMesh,IDisposable,IPluginConnections
    {
        [Input("Scene",IsSingle=true)]
        private IDiffSpread<AssimpScene> FInScene;

        private IDXMeshOut FOutMesh;

        [Output("Material Index",Order = 10)]
        private ISpread<int> FOutMaterialIndex;

        private Dictionary<Device, Mesh> FMeshes = new Dictionary<Device, Mesh>();

        private bool FInvalidate = false;

        [ImportingConstructor()]
        public AssimpMeshNode(IPluginHost host)
        {
            host.CreateMeshOutput("Mesh", TSliceMode.Dynamic, TPinVisibility.True, out this.FOutMesh);
            this.FOutMesh.Order = 0;
        }

        public void Evaluate(int SpreadMax)
        {
            this.FInvalidate = false;
            if (this.FInScene.IsChanged)
            {
                if (this.FInScene[0] != null)
                {
                    this.FOutMesh.SliceCount = this.FInScene[0].MeshCount;
                    this.FOutMaterialIndex.SliceCount = this.FInScene[0].MeshCount;
                    for (int i = 0; i < this.FInScene[0].MeshCount; i++)
                    {
                        this.FOutMaterialIndex[i] = this.FInScene[0].Meshes[i].MaterialIndex;
                    }
                }
                else
                {
                    this.FOutMesh.SliceCount = 0;
                    this.FOutMaterialIndex.SliceCount = 0;
                }
                this.FInvalidate = true;
            }
        }

        public Mesh GetMesh(IDXMeshOut ForPin, Device OnDevice)
        {
            if (this.FMeshes.ContainsKey(OnDevice))
            {
                return this.FMeshes[OnDevice];
            }
            else
            {
                return null;
            }
        }

        public void UpdateResource(IPluginOut ForPin, Device OnDevice)
        {
            if (this.FInvalidate || !this.FMeshes.ContainsKey(OnDevice))
            {
                //Destroy old mesh
                DestroyResource(ForPin, OnDevice, false);

                if (this.FInScene[0] == null) { return; }

                List<Mesh> meshes = new List<Mesh>();

                for (int i = 0; i < this.FInScene[0].MeshCount; i++)
                {
                    AssimpMesh assimpmesh = this.FInScene[0].Meshes[i];
                    Mesh mesh = new Mesh(OnDevice, assimpmesh.Indices.Count / 3, assimpmesh.VerticesCount, MeshFlags.Dynamic | MeshFlags.Use32Bit, assimpmesh.GetVertexBinding().ToArray());
                    DataStream vS = mesh.LockVertexBuffer(LockFlags.Discard);
                    DataStream iS = mesh.LockIndexBuffer(LockFlags.Discard);
                    assimpmesh.Write(vS);
                    iS.WriteRange(assimpmesh.Indices.ToArray());

                    mesh.UnlockVertexBuffer();
                    mesh.UnlockIndexBuffer();

                    meshes.Add(mesh);
                }


                try
                {
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
                }
                catch (Exception ex)
                {

                }
                foreach (Mesh m in meshes)
                {
                    m.Dispose();
                }
            }          
        }

        public void DestroyResource(IPluginOut ForPin, Device OnDevice, bool OnlyUnManaged)
        {
            if (this.FMeshes.ContainsKey(OnDevice))
            {
                this.FMeshes[OnDevice].Dispose();
                this.FMeshes.Remove(OnDevice);
            }          
        }

        #region IDisposable Members

        public void Dispose()
        {
            foreach (Device dev in this.FMeshes.Keys)
            {
                this.FMeshes[dev].Dispose();
            }
            this.FMeshes.Clear();
        }

        #endregion

        #region IPluginConnections Members

        public void ConnectPin(IPluginIO pin)
        {
            
        }

        public void DisconnectPin(IPluginIO pin)
        {
            
        }

        #endregion
    }
}
