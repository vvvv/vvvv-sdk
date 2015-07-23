using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VVVV.PluginInterfaces.V2;
using VVVV.Assimp.Lib;
using SlimDX;

namespace VVVV.Assimp.Nodes
{
    [PluginInfo(Name = "Node", Category = "Assimp", Version = "DX9", Author = "vux, flateric")]
    public class AssimpWorldNode : IPluginEvaluate
    {
        [Input("Scene", IsSingle = true)]
        private IDiffSpread<AssimpScene> FInScene;

        [Input("Root Node")]
        private IDiffSpread<string> FInRootNode;

        [Input("Recurse", DefaultValue = 1)]
        private IDiffSpread<bool> FInRecurse;

        [Output("Name")]
        ISpread<string> FOutName;

        [Output("Mesh Id")]
        ISpread<ISpread<int>> FOutMeshId;

        [Output("Local Transform")]
        ISpread<Matrix> FOutLocalTransform;

        [Output("World Transform")]
        ISpread<Matrix> FOutWorldTransform;



        public void Evaluate(int SpreadMax)
        {
            if (this.FInScene.IsChanged || this.FInRootNode.IsChanged || this.FInRecurse.IsChanged)
            {
                if (this.FInScene[0] != null)
                {
                    List<AssimpNode> allnodes = new List<AssimpNode>();
                    //string[] str = this.FInRootNode[0].Split("/".ToCharArray());

                    this.RecurseNodes(allnodes, this.FInScene[0].RootNode);

                    List<AssimpNode> filterednodes = new List<AssimpNode>();

                    for (int i = 0; i < Math.Max(this.FInRootNode.SliceCount, this.FInRecurse.SliceCount); i++)
                    {
                        if (this.FInRootNode[i] != "")
                        {
                            AssimpNode found = null;
                            foreach (AssimpNode node in allnodes) { if (node.Name == this.FInRootNode[i]) { found = node; } }

                            if (found != null)
                            {
                                if (this.FInRecurse[i])
                                {
                                    this.RecurseNodes(filterednodes, found);
                                }
                                else
                                {
                                    filterednodes.Add(found);
                                }
                            }
                        }
                        else
                        {
                            if (this.FInRecurse[i]) { filterednodes.AddRange(allnodes); }
                            else { filterednodes.Add(this.FInScene[0].RootNode); }
                        }
                    }


                    this.FOutMeshId.SliceCount = filterednodes.Count;
                    this.FOutWorldTransform.SliceCount = filterednodes.Count;
                    this.FOutLocalTransform.SliceCount = filterednodes.Count;
                    this.FOutName.SliceCount = filterednodes.Count;

                    for (int i = 0; i < filterednodes.Count; i++)
                    {
                        this.FOutWorldTransform[i] = filterednodes[i].RelativeTransform;
                        this.FOutLocalTransform[i] = filterednodes[i].LocalTransform;
                        this.FOutName[i] = filterednodes[i].Name;
                        this.FOutMeshId[i].SliceCount = filterednodes[i].MeshCount;
                        this.FOutMeshId[i].AssignFrom(filterednodes[i].MeshIndices);
                    }
                }
                else
                {
                    this.FOutMeshId.SliceCount = 0;
                    this.FOutLocalTransform.SliceCount = 0;
                    this.FOutWorldTransform.SliceCount = 0;
                    this.FOutName.SliceCount = 0;
                }

            }
        }

        private void RecurseNodes(List<AssimpNode> nodes, AssimpNode current)
        {
            nodes.Add(current);
            foreach (AssimpNode child in current.Children)
            {
                RecurseNodes(nodes, child);
            }
        }
    }
}
