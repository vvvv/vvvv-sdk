using System;
using System.Collections.Generic;
using System.Text;
using VVVV.PluginInterfaces.V1;
using Tamir.SharpSsh;

namespace SSHClient
{
    public class SshExecNode : IPlugin, IDisposable
    {
        #region Plugin Info
        public static IPluginInfo PluginInfo
        {
            get
            {
                IPluginInfo Info = new PluginInfo();
                Info.Name = "Ssh";							//use CamelCaps and no spaces
                Info.Category = "Network";						//try to use an existing one
                Info.Version = "";						//versions are optional. leave blank if not needed
                Info.Help = "Executes comands on an ssh server";
                Info.Bugs = "";
                Info.Credits = "SharpSSH: http://www.tamirgal.com/home/dev.aspx?Item=SharpSsh";
                Info.Warnings = "";
                Info.Author = "vux";
                Info.Tags = "Shell,Execution,Remote";

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

        private IPluginHost FHost;

        private IStringIn FPinInCommand;
        private IStringIn FPinInHost;
        private IStringIn FPinInUsername;
        private IStringIn FPinInPassword;
        private IValueIn FPinInDoExecute;
        private IValueIn FPinInKeepConnected;

        private IValueOut FPinOuResult;
        private IStringOut FPinOutput;

        private SshExec exec;
        private bool FKeepConnect = false;

        #region Set Plugin Host
        public void SetPluginHost(IPluginHost Host)
        {
            this.FHost = Host;

            this.FHost.CreateStringInput("Host", TSliceMode.Single, TPinVisibility.True, out this.FPinInHost);

            this.FHost.CreateStringInput("User Name", TSliceMode.Single, TPinVisibility.True, out this.FPinInUsername);
            this.FHost.CreateStringInput("Password", TSliceMode.Single, TPinVisibility.True, out this.FPinInPassword);
            this.FHost.CreateStringInput("Command", TSliceMode.Single, TPinVisibility.True, out this.FPinInCommand);

            this.FHost.CreateValueInput("Keep Connection", 1, null, TSliceMode.Single, TPinVisibility.True, out this.FPinInKeepConnected);
            this.FPinInKeepConnected.SetSubType(0, 1, 1, 0, false, true, true);

            this.FHost.CreateValueInput("Do Execute", 1, null, TSliceMode.Single, TPinVisibility.True, out this.FPinInDoExecute);
            this.FPinInDoExecute.SetSubType(0, 1, 1, 0, true, false, true);

            this.FHost.CreateValueOutput("Result", 1, null, TSliceMode.Single, TPinVisibility.True, out this.FPinOuResult);
            this.FPinOuResult.SetSubType(double.MinValue, double.MaxValue, 1, 0, false, false, true);

            this.FHost.CreateStringOutput("Output", TSliceMode.Single, TPinVisibility.True, out this.FPinOutput);

        }
        #endregion

        #region Configurate
        public void Configurate(IPluginConfig Input)
        {
        }
        #endregion

        #region Evaluate
        public void Evaluate(int SpreadMax)
        {
            //If we change connection settings, disconnect
            if (this.FPinInHost.PinIsChanged || this.FPinInPassword.PinIsChanged || this.FPinInUsername.PinIsChanged)
            {
                this.Disconnect();
            }

            if (this.FPinInKeepConnected.PinIsChanged)
            {
                double keep;
                this.FPinInKeepConnected.GetValue(0, out keep);
                this.FKeepConnect = keep == 1;

                if (!this.FKeepConnect)
                {
                    this.Disconnect();
                }
            }

            

            if (this.FPinInDoExecute.PinIsChanged)
            {
                double doexec;

                this.FPinInDoExecute.GetValue(0, out doexec);

                if (doexec == 1)
                {

                    string username;
                    string pwd;
                    string host;
                    string command;

                    string stdoutput = "";
                    string stderr = "";

                    this.FPinInHost.GetString(0, out host);
                    this.FPinInPassword.GetString(0, out pwd);
                    this.FPinInUsername.GetString(0, out username);
                    this.FPinInCommand.GetString(0, out command);

                    this.FPinOuResult.SliceCount = 1;
                    this.FPinOutput.SliceCount = 1;

                    try
                    {
                        if (exec == null)
                        {
                            exec = new SshExec(host, username, pwd);
                        }

                        if (!exec.Connected)
                        {
                            exec.Connect();
                        }

                        int i = exec.RunCommand(command, ref stdoutput, ref stderr);

                        this.FPinOuResult.SetValue(0, i);
                        this.FPinOutput.SetString(0, stdoutput);

                        if (!this.FKeepConnect)
                        {
                            this.Disconnect();
                        }
                    }
                    catch (Exception ex)
                    {

                    }
                }
            }
        }
        #endregion

        public bool AutoEvaluate
        {
            get { return false; }
        }

        #region IDisposable Members

        public void Dispose()
        {
            this.Disconnect();
        }

        #endregion

        private void Disconnect()
        {
            if (exec != null)
            {
                if (exec.Connected)
                {
                    exec.Close();
                }
            }
        }
    }
}
