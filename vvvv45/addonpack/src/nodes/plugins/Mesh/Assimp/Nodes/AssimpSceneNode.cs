using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VVVV.PluginInterfaces.V2;
using VVVV.Assimp.Lib;
using System.IO;

namespace VVVV.Assimp.Nodes
{
    [PluginInfo(Name = "Scene", Category = "Assimp", Version = "DX9", Author = "vux, flateric")]
    public class AssimpSceneNode : IPluginEvaluate, IDisposable
    {
        [Input("Path",StringType=StringType.Filename,IsSingle=true)]
        IDiffSpread<string> FInPath;

        [Input("Reload", IsBang = true, IsSingle = true)]
        ISpread<bool> FInReload;

        [Output("Scene",IsSingle = true)]
        ISpread<AssimpScene> FOutScene;
 
        [Output("Mesh Count", IsSingle=true)]
        ISpread<int> FOutMeshCount;

        [Output("Materials")]
        ISpread<AssimpMaterial> FOutMaterials;

        [Output("Cameras")]
        ISpread<AssimpCamera> FOutCameras;

        [Output("Is Valid")]
        ISpread<bool> FOutValid;

        private AssimpScene scene;

        public void Evaluate(int SpreadMax)
        {

            if (this.FInPath.IsChanged || this.FInReload[0])
            {
                if (this.scene != null) { this.scene.Dispose(); }

                string p = this.FInPath[0];
                if (File.Exists(p))
                {
                    try
                    {
                        this.scene = new AssimpScene(p);
                        this.FOutValid[0] = true;
                        this.FOutMeshCount[0] = this.scene.MeshCount;

                        this.FOutCameras.AssignFrom(this.scene.Cameras);
                        this.FOutMaterials.AssignFrom(this.scene.Materials);

                    }
                    catch
                    {
                        this.FOutValid[0] = false;
                        this.FOutMeshCount[0] = 0;
                        this.FOutMaterials.SliceCount = 0;
                        this.FOutCameras.SliceCount = 0;
                    }
                }
                else
                {
                    this.FOutValid[0] = false;
                    this.FOutMeshCount[0] = 0;
                    this.FOutMaterials.SliceCount = 0;
                    this.FOutCameras.SliceCount = 0;
                }
                this.FOutScene[0] = this.scene;
            }
        }

        public void Dispose()
        {
            if (this.scene != null) { this.scene.Dispose(); }
        }
    }
}
