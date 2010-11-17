using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace rdfServer.GUI
{
    public partial class fclsServerManager : Form
    {
        private BindingList<IServerHarness> _servers = new BindingList<IServerHarness>();

        public fclsServerManager()
        {
            InitializeComponent();

            this.cboServers.DataSource = this._servers;

            this.FormClosing += new FormClosingEventHandler(fclsServerManager_FormClosing);
            this.tabControls.SelectedIndexChanged += new EventHandler(tabControls_SelectedIndexChanged);
        }

        void tabControls_SelectedIndexChanged(object sender, EventArgs e)
        {
            this.cboServers_SelectedIndexChanged(sender, e);
        }

        void fclsServerManager_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (MessageBox.Show("Are you sure you wish to exit?  Any In-Process Servers will be terminated while any running External Process servers will continue to run until you terminate them manually?", "Confirm Exit", MessageBoxButtons.OKCancel) == DialogResult.OK)
            {
                foreach (IServerHarness server in this._servers)
                {
                    server.Dispose();
                }
            }
            else
            {
                e.Cancel = true;
            }
        }

        private void btnCreateAdvanced_Click(object sender, EventArgs e)
        {
            String path = Path.Combine(Environment.CurrentDirectory, "rdfServer.exe");
            if (!File.Exists(path))
            {
                MessageBox.Show("Unable to start an rdfServer instance as could not find rdfServer.exe");
            }
            else
            {
                ProcessStartInfo info = new ProcessStartInfo();
                info.FileName = path;
                info.Arguments = this.txtCreateAdvanced.Text;
                info.CreateNoWindow = true;
                info.RedirectStandardOutput = true;
                info.RedirectStandardError = true;
                info.UseShellExecute = false;

                IServerHarness harness = new ExternalProcessServerHarness(info);
                if (this.chkStartAutoAdvanced.Checked)
                {
                    try
                    {
                        harness.Start();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message, "Error Starting Server", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                this._servers.Add(harness);
            }

        }

        private void cboServers_SelectedIndexChanged(object sender, EventArgs e)
        {
            //Detach Monitor from any and all servers
            foreach (IServerHarness harness in this._servers)
            {
                harness.DetachMonitor();
            }
            this.monServers.Clear();

            if (this.cboServers.SelectedIndex > -1)
            {
                if (this.tabControls.SelectedTab.Name.Equals("tabRunningServers"))
                {
                    IServerHarness server = this._servers[this.cboServers.SelectedIndex];
                    if (server.IsRunning) server.AttachMonitor(this.monServers);
                    this.UpdateServerControls(server);
                }
            }
            else
            {
                this.btnStart.Enabled = false;
                this.btnStop.Enabled = false;
                this.btnPauseResume.Enabled = false;
                this.btnPauseResume.Text = "Pause";
            }
        }

        private void UpdateServerControls(IServerHarness server)
        {
            this.btnStart.Enabled = !server.IsRunning;
            this.btnPauseResume.Enabled = server.CanPauseAndResume;
            if (server.IsRunning)
            {
                this.btnPauseResume.Text = "Pause";
            }
            else
            {
                this.btnPauseResume.Text = "Resume";
            }
            this.btnStop.Enabled = server.IsRunning;
        }

        private IServerHarness GetSelectedServer()
        {
            if (this.cboServers.SelectedIndex > -1)
            {
                return this._servers[this.cboServers.SelectedIndex];
            }
            else
            {
                return null;
            }
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            IServerHarness server = this.GetSelectedServer();
            if (server != null)
            {
                if (!server.IsRunning)
                {
                    try
                    {
                        server.Start();
                        if (server.IsRunning) server.AttachMonitor(this.monServers);
                        this._servers.ResetBindings();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message, "Error Starting Server", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    this.UpdateServerControls(server);

                }
            }
        }

        private void btnPauseResume_Click(object sender, EventArgs e)
        {
            IServerHarness server = this.GetSelectedServer();
            if (server != null)
            {
                if (server.CanPauseAndResume)
                {
                    try
                    {
                        if (server.IsRunning)
                        {
                            server.Pause();
                        }
                        else
                        {
                            server.Resume();
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message, "Error Pausing/Resuming Server", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    this._servers.ResetBindings();
                    this.UpdateServerControls(server);
                }
            }
        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            IServerHarness server = this.GetSelectedServer();
            if (server != null)
            {
                if (server.IsRunning)
                {
                    try
                    {
                        server.Stop();
                        server.DetachMonitor();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message, "Error Stopping Server", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    this._servers.ResetBindings();
                    this.UpdateServerControls(server);
                }
            }
        }

        private void btnUseLocalhost_Click(object sender, EventArgs e)
        {
            this.txtHost.Text = "localhost";
        }

        private void chkUseLogFile_CheckedChanged(object sender, EventArgs e)
        {
            this.txtLogFile.Enabled = this.chkUseLogFile.Checked;
        }

        private void btnCreate_Click(object sender, EventArgs e)
        {
            //Build up the command line arguments as an array
            List<String> args = new List<string>();

            bool external = this.chkRunExternal.Checked;

            if (!this.txtHost.Equals(String.Empty))
            {
                args.Add("-host");
                args.Add(this.txtHost.Text);
            }
            if (!this.txtPort.Equals(String.Empty))
            {
                args.Add("-port");
                args.Add(this.txtPort.Text);
            }
            if (!this.txtConfigFile.Text.Equals(String.Empty))
            {
                args.Add("-config");
                args.Add(this.Escape(this.txtConfigFile.Text, external));
            }
            if (this.chkUseLogFile.Checked)
            {
                if (!this.txtLogFile.Text.Equals(String.Empty))
                {
                    args.Add("-log");
                    args.Add(this.Escape(this.txtLogFile.Text, external));
                }
            }
            if (!this.cboLogFormat.Text.Equals(String.Empty))
            {
                args.Add("-format");
                args.Add(this.Escape(this.cboLogFormat.Text, external));
            }
            if (!this.txtBaseDirectory.Text.Equals(String.Empty))
            {
                args.Add("-base");
                args.Add(this.Escape(this.txtBaseDirectory.Text, external));
            }
            if (this.chkVerboseMode.Checked)
            {
                args.Add("-verbose");
            }
            if (this.chkQuietMode.Checked)
            {
                args.Add("-quiet");
            }

            if (external)
            {
                //Essentially hijack the Create Advanced stuff
                String temp = this.txtCreateAdvanced.Text;
                bool temp2 = this.chkStartAutoAdvanced.Checked;
                this.chkStartAutoAdvanced.Checked = this.chkStartAuto.Checked;

                this.txtCreateAdvanced.Text = String.Join(" ", args.ToArray());
                this.btnCreateAdvanced_Click(sender, e);

                this.txtCreateAdvanced.Text = temp;
                this.chkStartAutoAdvanced.Checked = temp2;
            }
            else
            {
                RdfServerOptions options = new RdfServerOptions(args.ToArray());
                IServerHarness server = new InProcessServerHarness(options.GetServerInstance(), this.cboLogFormat.Text);
                if (this.chkStartAuto.Checked)
                {
                    try
                    {
                        server.Start();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message, "Error Starting Server", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                this._servers.Add(server);
            }
        }

        private String Escape(String input, bool external)
        {
            if (external)
            {
                if (input.Contains(' '))
                {
                    if (input.Contains('"'))
                    {
                        return "\"" + input.Replace("\"", "\\\"") + "\"";
                    }
                    else if (input.Contains("\\"))
                    {
                        return "\"" + input.Replace("\\", "\\\\") + "\"";
                    }
                    else
                    {
                        return "\"" + input + "\"";
                    }
                } 
                else if (input.Contains('"'))
                {
                    return "\"" + input.Replace("\"", "\\\"") + "\"";
                }
                else if (input.Contains("\\"))
                {
                    return "\"" + input.Replace("\\", "\\\\") + "\"";
                }
                else
                {
                    return input;
                }
            }
            else
            {
                return input;
            }
        }

        private void fclsServerManager_Load(object sender, EventArgs e)
        {
            try
            {
                foreach (Process p in Process.GetProcessesByName("rdfServer"))
                {
                    ProcessStartInfo info = p.StartInfo;
                    
                    ExternalProcessServerHarness server = new ExternalProcessServerHarness(info, p);
                    this._servers.Add(server);
                }
            }
            catch
            {
                //Ignore exceptions here - just means we failed to enumerate relevant processes successfully
            }
        }
    }
}
