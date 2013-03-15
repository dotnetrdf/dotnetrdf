/*
dotNetRDF is free and open source software licensed under the MIT License

-----------------------------------------------------------------------------

Copyright (c) 2009-2012 dotNetRDF Project (dotnetrdf-developer@lists.sf.net)

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is furnished
to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/

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

namespace VDS.RDF.Utilities.Server.GUI
{
    /// <summary>
    /// Server Manager Form
    /// </summary>
    public partial class fclsServerManager
        : Form
    {
        private BindingList<IServerHarness> _servers = new BindingList<IServerHarness>();

        /// <summary>
        /// Creates a new Server Manager Form
        /// </summary>
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

        /// <summary>
        /// Event Handler which ensures server harnesses are disposed of when the application is closed
        /// </summary>
        /// <param name="sender">Sender</param>
        /// <param name="e">Event Arguments</param>
        void fclsServerManager_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (this._servers.Count > 0)
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
                try
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
                catch (Exception ex)
                {
                    MessageBox.Show("Unable to create an External Process Server due to the following error:\n" + ex.Message, "Error Creating Server", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
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
            }
        }

        private void UpdateServerControls(IServerHarness server)
        {
            this.btnStart.Enabled = !server.IsRunning;
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
                        server.AttachMonitor(this.monServers);
                        server.Start();
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
                try
                {
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
                catch (Exception ex)
                {
                    MessageBox.Show("Unable to create an In-Process server due to the following error:\n" + ex.Message, "Error Creating Server", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
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
