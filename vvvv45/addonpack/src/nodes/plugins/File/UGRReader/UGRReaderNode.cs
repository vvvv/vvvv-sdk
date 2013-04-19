#region usings
using System;
using System.ComponentModel.Composition;
using System.Collections.Generic;

using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.VColor;
using VVVV.Utils.VMath;

using System.IO;
#endregion usings

namespace VVVV.Nodes
{
    public class UGRColor
    {
        public int index;
        public RGBAColor color;
    }

    #region PluginInfo
    [PluginInfo(Name = "UGRReader", Category = "File", Help = "Reads Ultra Fractal Gradient file",Author="vux", Tags = "")]
    #endregion PluginInfo
    public class UGRReaderNode : IPluginEvaluate
    {
        #region fields & pins
        [Input("Path", StringType = StringType.Filename)]
        IDiffSpread<string> FPath;

        [Input("Reload", IsBang = true)]
        ISpread<bool> FReset;

        [Input("Gradient Index")]
        IDiffSpread<int> FEntry;

        [Output("Output")]
        ISpread<RGBAColor> FOutput;

        [Output("Names")]
        ISpread<string> FOutNames;
        #endregion fields & pins

        //private string ename;
        private List<List<UGRColor>> g = new List<List<UGRColor>>();

        //called when data for any output pin is requested
        public void Evaluate(int SpreadMax)
        {
            bool reset = false;
            if (FPath.IsChanged || FReset[0])
            {
                reset = true;
                g.Clear();

                if (File.Exists(FPath[0]))
                {
                    string[] lines = File.ReadAllLines(FPath[0]);
                    bool name = false;



                    List<string> res = new List<string>();
                    List<UGRColor> colors = new List<UGRColor>();
                    UGRColor color = new UGRColor();

                    foreach (string s in lines)
                    {
                        string strim = s.Trim();
                        if (!name)
                        {
                            if (strim.Contains("{"))
                            {
                                string sub = s.Replace("{", "");
                                sub = sub.Replace(" ", "");
                                res.Add(sub);
                                name = true;
                                colors = new List<UGRColor>();
                            }
                        }
                        else
                        {
                            bool index = true;
                            bool done = false;

                            string[] elems = strim.Split(" ".ToCharArray());


                            //First line is gradient, ignore
                            if (!strim.StartsWith("gradient"))
                            {
                                foreach (string elem in elems)
                                {
                                    if (elem.StartsWith("index="))
                                    {
                                        color = new UGRColor();
                                        //FLogger.Log(LogType.Debug,elem);
                                        string[] spl = elem.Split("=".ToCharArray());

                                        color.index = Convert.ToInt32(spl[1]);
                                    }
                                    else
                                    {
                                        if (elem.StartsWith("color="))
                                        {
                                            //FLogger.Log(LogType.Debug,elem);
                                            string[] spl = elem.Split("=".ToCharArray());
                                            int colint = Convert.ToInt32(spl[1]);
                                            color.color = System.Drawing.Color.FromArgb(colint);
                                            color.color.A = 1;
                                            colors.Add(color);
                                        }
                                    }

                                }
                            }

                            if (strim.Contains("}"))
                            {
                                name = false;
                                g.Add(colors);
                            }
                        }
                    }
                    this.FOutNames.AssignFrom(res);
                }
            }

            //Now output
            if (g.Count > 0 && (this.FEntry.IsChanged || reset))
            {
                int idx = this.FEntry[0] % g.Count;

                List<RGBAColor> res = new List<RGBAColor>();
                List<UGRColor> ucols = g[idx];

                int colcount = ucols[ucols.Count - 1].index;

                int ucolidx = 1;
                UGRColor previous = ucols[0];
                bool first = true;

                res.Add(previous.color);

                for (int i = 1; i < colcount; i++)
                {
                    if (i == ucols[ucolidx].index)
                    {
                        //Increment and move to next color
                        ucolidx++;
                        previous = ucols[ucolidx];
                    }
                    res.Add(previous.color);
                }

                this.FOutput.SliceCount = res.Count;
                for (int i = 0; i < res.Count; i++)
                {
                    this.FOutput[i] = res[i];
                }
            }
        }
    }
}
