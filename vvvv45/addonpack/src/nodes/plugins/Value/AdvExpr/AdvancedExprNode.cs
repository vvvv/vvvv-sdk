using System;
using System.Collections.Generic;
using System.Text;
using VVVV.PluginInterfaces.V1;
using Microsoft.CSharp;
using System.CodeDom.Compiler;
using System.Reflection;
using VVVV.Lib;
using System.IO;

namespace VVVV.Nodes
{
    
    public unsafe class AdvancedExprNode : IPlugin, IDisposable
    {
        #region Plugin Info
        public static IPluginInfo PluginInfo
        {
            get
            {
                IPluginInfo Info = new PluginInfo();
                Info.Name = "Expr";							//use CamelCaps and no spaces
                Info.Category = "Value";						//try to use an existing one
                Info.Version = "Advanced";						//versions are optional. leave blank if not needed
                Info.Help = "Auto compiled version of Expr (Value)";
                Info.Bugs = "";
                Info.Credits = "";								//give credits to thirdparty code used
                Info.Warnings = "";
                Info.Author = "vux";

                //leave below as is
                System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace(true);
                System.Diagnostics.StackFrame sf = st.GetFrame(0);
                System.Reflection.MethodBase method = sf.GetMethod();
                Info.Namespace = method.DeclaringType.Namespace;
                Info.Class = method.DeclaringType.Name;
                return Info;
                //leave above as is
            }
        }
        #endregion

        #region Fields
        private IPluginHost FHost;

        private List<IValueFastIn> FPinInVariables = new List<IValueFastIn>();
        //private Sorted

        private IStringIn FPinInTerm;
        private IStringConfig FPinInVarName;

        private IValueOut FPinOutput;
        private IStringOut FPinOutCode;
        private IStringOut FPinOutError;

        private bool FValid = false;
        private List<string> FVarNames = new List<string>();

        private Assembly FAssembly;
        private IFunctionEval FFormula;
        private bool FInvalidate = false;
        #endregion

        #region Auto Evaluate
        public bool AutoEvaluate
        {
            get { return false; }
        }
        #endregion

        #region Set Plugin Host
        public void SetPluginHost(IPluginHost Host)
        {
            //assign host
            this.FHost = Host;

            AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler(CurrentDomain_AssemblyResolve);

            this.FHost.CreateStringInput("Term", TSliceMode.Single, TPinVisibility.True, out this.FPinInTerm);
            this.FPinInTerm.SetSubType("0+0", false);

            this.FHost.CreateStringConfig("Variables Name", TSliceMode.Single, TPinVisibility.True, out this.FPinInVarName);
            this.FPinInVarName.SetSubType("A", false);
          
            this.FHost.CreateValueOutput("Output", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out this.FPinOutput);
            this.FPinOutput.SetSubType(double.MinValue, double.MaxValue, 0.01, 0, false, false, false);

            
            this.FHost.CreateStringOutput("Output Code", TSliceMode.Single, TPinVisibility.Hidden, out this.FPinOutCode);
            this.FPinOutCode.SetSubType("", false);
              
            this.FHost.CreateStringOutput("Error", TSliceMode.Single, TPinVisibility.True, out this.FPinOutError);
            this.FPinOutError.SetSubType("", false);

            Configurate(this.FPinInVarName);
           
        }

        Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            if (args.Name == Assembly.GetExecutingAssembly().FullName)
            {
                return Assembly.GetExecutingAssembly();
            }
            else
            {
                return null;
            }
        }
        #endregion

        #region Configurate
        public void Configurate(IPluginConfig Input)
        {
            string vars;
            this.FPinInVarName.GetString(0, out vars);
            vars = vars == null ? String.Empty : vars;
            string[] arrvars = vars.Split(",".ToCharArray());
            List<string> varllst = new List<string>(arrvars);

            //Remove old pins
            List<IValueFastIn> todelete = new List<IValueFastIn>();
            foreach (IValueFastIn pin in this.FPinInVariables)
            {
                if (!varllst.Contains(pin.Name))
                {
                    todelete.Add(pin);
                }  
            }

            foreach (IValueFastIn pin in todelete)
            {
                this.FHost.DeletePin(pin);
                this.FPinInVariables.Remove(pin);
                this.FVarNames.Remove(pin.Name);
            }


            List<string> sorted = new List<string>();
            foreach (string v in arrvars)
            {
                if (!this.FVarNames.Contains(v))
                {
                    IValueFastIn pin;
                    this.FHost.CreateValueFastInput(v, 1, null, TSliceMode.Dynamic, TPinVisibility.True, out pin);
                    this.FPinInVariables.Add(pin);
                    this.FVarNames.Add(v);
                }

                if (!sorted.Contains(v))
                {
                    sorted.Add(v);
                }
            }

            foreach (IValueFastIn pin in this.FPinInVariables)
            {
                pin.Order = sorted.IndexOf(pin.Name);
            }

            //int order = 0;

            this.FPinInTerm.Order = arrvars.Length + 1;

            
            this.FInvalidate = true;
        }
        #endregion

        #region Evaluate
        public void Evaluate(int SpreadMax)
        {
            if (this.FPinInTerm.PinIsChanged || this.FInvalidate)
            {
                this.FAssembly = null;


                string term;
                this.FPinInTerm.GetString(0, out term);
                term = term == null ? String.Empty : term;

                CSharpCodeProvider csp = new CSharpCodeProvider();
                ICodeCompiler cc = csp.CreateCompiler();

                #region Build Code
                CompilerParameters cp = new CompilerParameters();
                cp.ReferencedAssemblies.Add("System.dll");
                cp.ReferencedAssemblies.Add("System.Data.dll");
                cp.ReferencedAssemblies.Add("System.Xml.dll");
                cp.ReferencedAssemblies.Add("mscorlib.dll");
                cp.ReferencedAssemblies.Add(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\AdvExpr.dll");
                cp.GenerateInMemory = true;
                cp.GenerateExecutable = false;
                cp.CompilerOptions = "/optimize";

                //Header
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("using System;");
                sb.AppendLine("using System.Collections.Generic;");
                sb.AppendLine("namespace VVVV.Lib");
                sb.AppendLine("{");
                sb.AppendLine("public class Evaluator : IFunctionEval");
                sb.AppendLine("{");
                sb.AppendLine("public double Eval(double[] values)");
                sb.AppendLine("{");

                for (int i = 0; i < this.FVarNames.Count; i++)
                {
                    sb.AppendLine("double " + this.FVarNames[i] + " =values[" + i.ToString() + "];");
                }

                sb.AppendLine(term);

                //Footer
                sb.AppendLine("}");
                sb.AppendLine("}");
                sb.AppendLine("}");
                #endregion

                string code = sb.ToString();
                CompilerResults cr = cc.CompileAssemblyFromSource(cp, code);

                if (cr.Errors.Count > 0)
                {
                    this.FValid = false;
                    string err = "";

                    foreach (CompilerError ce in cr.Errors)
                    {
                        err += ce.ErrorNumber + ":" + ce.ErrorText + "\n";
                    }

                    this.FPinOutError.SetString(0, err);               
                }
                else
                {
                    this.FValid = true;
                    this.FAssembly = cr.CompiledAssembly;
                    this.FPinOutError.SetString(0, "");

                    foreach (Type t in this.FAssembly.GetTypes())
                    {
                        if (typeof(IFunctionEval).IsAssignableFrom(t))
                        {
                            object o = Activator.CreateInstance(t);
                            this.FFormula = o as IFunctionEval;
                        }
                    }
                }
                this.FPinOutCode.SetString(0, code);
                this.FInvalidate = false;

            }

            this.FPinOutput.SliceCount = SpreadMax;
            if (this.FValid)
            {
                
                double* dbloutput;
                this.FPinOutput.GetValuePointer(out dbloutput);
                double[] vals = new double[this.FPinInVariables.Count];

                List<IntPtr> pointers = new List<IntPtr>();
                List<int> ptrcount = new List<int>();
                for (int j = 0; j < this.FPinInVariables.Count; j++)
                {
                    double* ptr;
                    int cnt;
                    this.FPinInVariables[j].GetValuePointer(out cnt, out ptr);
                    pointers.Add(new IntPtr(ptr));
                    ptrcount.Add(cnt);
                }


                for (int i = 0; i < SpreadMax; i++)
                {
                    for (int j = 0; j < this.FPinInVariables.Count; j++)
                    {
                        vals[j] = ((double*)pointers[j])[i % ptrcount[j]];
                    }

                    dbloutput[i] = this.FFormula.Eval(vals);
                }
            }
            else
            {
                for (int i = 0; i < SpreadMax; i++)
                {
                    this.FPinOutput.SetValue(i, 0);
                }
            }
        }
        #endregion

        #region Dispose
        public void Dispose()
        {
        }
        #endregion
    }
        
        
}
