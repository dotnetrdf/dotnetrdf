/*

Copyright dotNetRDF Project 2009-12
dotnetrdf-develop@lists.sf.net

------------------------------------------------------------------------

This file is part of dotNetRDF.

dotNetRDF is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

dotNetRDF is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with dotNetRDF.  If not, see <http://www.gnu.org/licenses/>.

------------------------------------------------------------------------

dotNetRDF may alternatively be used under the LGPL or MIT License

http://www.gnu.org/licenses/lgpl.html
http://www.opensource.org/licenses/mit-license.php

If these licenses are not suitable for your intended use please contact
us at the above stated email address to discuss alternative
terms.

*/

namespace VDS.RDF.Utilities.Server.GUI
{
    partial class fclsServerManager
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.tabControls = new System.Windows.Forms.TabControl();
            this.tabCreate = new System.Windows.Forms.TabPage();
            this.btnCreate = new System.Windows.Forms.Button();
            this.chkStartAuto = new System.Windows.Forms.CheckBox();
            this.chkRunExternal = new System.Windows.Forms.CheckBox();
            this.chkQuietMode = new System.Windows.Forms.CheckBox();
            this.chkVerboseMode = new System.Windows.Forms.CheckBox();
            this.txtBaseDirectory = new System.Windows.Forms.TextBox();
            this.lblBaseDirectory = new System.Windows.Forms.Label();
            this.txtConfigFile = new System.Windows.Forms.TextBox();
            this.lblConfigFile = new System.Windows.Forms.Label();
            this.cboLogFormat = new System.Windows.Forms.ComboBox();
            this.lblLogFormat = new System.Windows.Forms.Label();
            this.txtLogFile = new System.Windows.Forms.TextBox();
            this.lblLogFile = new System.Windows.Forms.Label();
            this.chkUseLogFile = new System.Windows.Forms.CheckBox();
            this.txtPort = new System.Windows.Forms.MaskedTextBox();
            this.lblPort = new System.Windows.Forms.Label();
            this.btnUseLocalhost = new System.Windows.Forms.Button();
            this.txtHost = new System.Windows.Forms.TextBox();
            this.lblHost = new System.Windows.Forms.Label();
            this.tabCreateAdvanced = new System.Windows.Forms.TabPage();
            this.chkStartAutoAdvanced = new System.Windows.Forms.CheckBox();
            this.btnCreateAdvanced = new System.Windows.Forms.Button();
            this.txtCreateAdvanced = new System.Windows.Forms.TextBox();
            this.lblCreateAdvanced = new System.Windows.Forms.Label();
            this.tabRunningServers = new System.Windows.Forms.TabPage();
            this.btnStop = new System.Windows.Forms.Button();
            this.btnPauseResume = new System.Windows.Forms.Button();
            this.btnStart = new System.Windows.Forms.Button();
            this.cboServers = new System.Windows.Forms.ComboBox();
            this.lblServers = new System.Windows.Forms.Label();
            this.monServers = new VDS.RDF.Utilities.Server.GUI.ServerMonitor();
            this.ttpTips = new System.Windows.Forms.ToolTip(this.components);
            this.tabControls.SuspendLayout();
            this.tabCreate.SuspendLayout();
            this.tabCreateAdvanced.SuspendLayout();
            this.tabRunningServers.SuspendLayout();
            this.SuspendLayout();
            // 
            // tabControls
            // 
            this.tabControls.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.tabControls.Controls.Add(this.tabCreate);
            this.tabControls.Controls.Add(this.tabCreateAdvanced);
            this.tabControls.Controls.Add(this.tabRunningServers);
            this.tabControls.Location = new System.Drawing.Point(12, 12);
            this.tabControls.Name = "tabControls";
            this.tabControls.SelectedIndex = 0;
            this.tabControls.Size = new System.Drawing.Size(449, 330);
            this.tabControls.TabIndex = 0;
            // 
            // tabCreate
            // 
            this.tabCreate.Controls.Add(this.btnCreate);
            this.tabCreate.Controls.Add(this.chkStartAuto);
            this.tabCreate.Controls.Add(this.chkRunExternal);
            this.tabCreate.Controls.Add(this.chkQuietMode);
            this.tabCreate.Controls.Add(this.chkVerboseMode);
            this.tabCreate.Controls.Add(this.txtBaseDirectory);
            this.tabCreate.Controls.Add(this.lblBaseDirectory);
            this.tabCreate.Controls.Add(this.txtConfigFile);
            this.tabCreate.Controls.Add(this.lblConfigFile);
            this.tabCreate.Controls.Add(this.cboLogFormat);
            this.tabCreate.Controls.Add(this.lblLogFormat);
            this.tabCreate.Controls.Add(this.txtLogFile);
            this.tabCreate.Controls.Add(this.lblLogFile);
            this.tabCreate.Controls.Add(this.chkUseLogFile);
            this.tabCreate.Controls.Add(this.txtPort);
            this.tabCreate.Controls.Add(this.lblPort);
            this.tabCreate.Controls.Add(this.btnUseLocalhost);
            this.tabCreate.Controls.Add(this.txtHost);
            this.tabCreate.Controls.Add(this.lblHost);
            this.tabCreate.Location = new System.Drawing.Point(4, 22);
            this.tabCreate.Name = "tabCreate";
            this.tabCreate.Padding = new System.Windows.Forms.Padding(3);
            this.tabCreate.Size = new System.Drawing.Size(441, 304);
            this.tabCreate.TabIndex = 0;
            this.tabCreate.Text = "Create Server";
            this.tabCreate.UseVisualStyleBackColor = true;
            // 
            // btnCreate
            // 
            this.btnCreate.Location = new System.Drawing.Point(183, 275);
            this.btnCreate.Name = "btnCreate";
            this.btnCreate.Size = new System.Drawing.Size(75, 23);
            this.btnCreate.TabIndex = 18;
            this.btnCreate.Text = "Create";
            this.btnCreate.UseVisualStyleBackColor = true;
            this.btnCreate.Click += new System.EventHandler(this.btnCreate_Click);
            // 
            // chkStartAuto
            // 
            this.chkStartAuto.AutoSize = true;
            this.chkStartAuto.Location = new System.Drawing.Point(9, 255);
            this.chkStartAuto.Name = "chkStartAuto";
            this.chkStartAuto.Size = new System.Drawing.Size(153, 17);
            this.chkStartAuto.TabIndex = 17;
            this.chkStartAuto.Text = "Start Server Automatically?";
            this.chkStartAuto.UseVisualStyleBackColor = true;
            // 
            // chkRunExternal
            // 
            this.chkRunExternal.AutoSize = true;
            this.chkRunExternal.Location = new System.Drawing.Point(9, 232);
            this.chkRunExternal.Name = "chkRunExternal";
            this.chkRunExternal.Size = new System.Drawing.Size(248, 17);
            this.chkRunExternal.TabIndex = 16;
            this.chkRunExternal.Text = "Run as External Process (for persistent servers)";
            this.chkRunExternal.UseVisualStyleBackColor = true;
            // 
            // chkQuietMode
            // 
            this.chkQuietMode.AutoSize = true;
            this.chkQuietMode.Location = new System.Drawing.Point(9, 209);
            this.chkQuietMode.Name = "chkQuietMode";
            this.chkQuietMode.Size = new System.Drawing.Size(358, 17);
            this.chkQuietMode.TabIndex = 15;
            this.chkQuietMode.Text = "Enable Quiet Mode (advanced users only - supercedes verbose mode)";
            this.chkQuietMode.UseVisualStyleBackColor = true;
            // 
            // chkVerboseMode
            // 
            this.chkVerboseMode.AutoSize = true;
            this.chkVerboseMode.Checked = true;
            this.chkVerboseMode.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkVerboseMode.Location = new System.Drawing.Point(9, 186);
            this.chkVerboseMode.Name = "chkVerboseMode";
            this.chkVerboseMode.Size = new System.Drawing.Size(322, 17);
            this.chkVerboseMode.TabIndex = 14;
            this.chkVerboseMode.Text = "Enable Verbose Mode (recommended for monitoring of servers)";
            this.chkVerboseMode.UseVisualStyleBackColor = true;
            // 
            // txtBaseDirectory
            // 
            this.txtBaseDirectory.Location = new System.Drawing.Point(91, 160);
            this.txtBaseDirectory.Name = "txtBaseDirectory";
            this.txtBaseDirectory.Size = new System.Drawing.Size(344, 20);
            this.txtBaseDirectory.TabIndex = 13;
            this.ttpTips.SetToolTip(this.txtBaseDirectory, "Sets the Base Directory from which static content is served.  If left blank then " +
                    "static content will not be served.  Enter . to serve static content from servers" +
                    " working directory");
            // 
            // lblBaseDirectory
            // 
            this.lblBaseDirectory.AutoSize = true;
            this.lblBaseDirectory.Location = new System.Drawing.Point(6, 163);
            this.lblBaseDirectory.Name = "lblBaseDirectory";
            this.lblBaseDirectory.Size = new System.Drawing.Size(79, 13);
            this.lblBaseDirectory.TabIndex = 12;
            this.lblBaseDirectory.Text = "Base Directory:";
            // 
            // txtConfigFile
            // 
            this.txtConfigFile.Location = new System.Drawing.Point(103, 61);
            this.txtConfigFile.Name = "txtConfigFile";
            this.txtConfigFile.Size = new System.Drawing.Size(332, 20);
            this.txtConfigFile.TabIndex = 11;
            this.ttpTips.SetToolTip(this.txtConfigFile, "Sets the dotNetRDF Configuration File used to specify the dataset served by this " +
                    "rdfServer (leave blank to start with an empty non-persistent in-memory dataset)");
            // 
            // lblConfigFile
            // 
            this.lblConfigFile.AutoSize = true;
            this.lblConfigFile.Location = new System.Drawing.Point(6, 64);
            this.lblConfigFile.Name = "lblConfigFile";
            this.lblConfigFile.Size = new System.Drawing.Size(91, 13);
            this.lblConfigFile.TabIndex = 10;
            this.lblConfigFile.Text = "Configuration File:";
            // 
            // cboLogFormat
            // 
            this.cboLogFormat.FormattingEnabled = true;
            this.cboLogFormat.Items.AddRange(new object[] {
            "common",
            "combined",
            "all",
            "user-agent"});
            this.cboLogFormat.Location = new System.Drawing.Point(69, 133);
            this.cboLogFormat.Name = "cboLogFormat";
            this.cboLogFormat.Size = new System.Drawing.Size(366, 21);
            this.cboLogFormat.TabIndex = 9;
            this.ttpTips.SetToolTip(this.cboLogFormat, "Set the log format to use for logging.  Select either a predefined format or ente" +
                    "r your own, log format is specified in Apache mod_log style");
            // 
            // lblLogFormat
            // 
            this.lblLogFormat.AutoSize = true;
            this.lblLogFormat.Location = new System.Drawing.Point(6, 136);
            this.lblLogFormat.Name = "lblLogFormat";
            this.lblLogFormat.Size = new System.Drawing.Size(63, 13);
            this.lblLogFormat.TabIndex = 8;
            this.lblLogFormat.Text = "Log Format:";
            // 
            // txtLogFile
            // 
            this.txtLogFile.Enabled = false;
            this.txtLogFile.Location = new System.Drawing.Point(69, 105);
            this.txtLogFile.Name = "txtLogFile";
            this.txtLogFile.Size = new System.Drawing.Size(366, 20);
            this.txtLogFile.TabIndex = 7;
            this.txtLogFile.Text = "log.txt";
            this.ttpTips.SetToolTip(this.txtLogFile, "Sets the name of the Log File to which HTTP requests to the server are logged");
            // 
            // lblLogFile
            // 
            this.lblLogFile.AutoSize = true;
            this.lblLogFile.Location = new System.Drawing.Point(6, 108);
            this.lblLogFile.Name = "lblLogFile";
            this.lblLogFile.Size = new System.Drawing.Size(47, 13);
            this.lblLogFile.TabIndex = 6;
            this.lblLogFile.Text = "Log File:";
            // 
            // chkUseLogFile
            // 
            this.chkUseLogFile.AutoSize = true;
            this.chkUseLogFile.Location = new System.Drawing.Point(9, 82);
            this.chkUseLogFile.Name = "chkUseLogFile";
            this.chkUseLogFile.Size = new System.Drawing.Size(105, 17);
            this.chkUseLogFile.TabIndex = 5;
            this.chkUseLogFile.Text = "Enable Log File?";
            this.chkUseLogFile.UseVisualStyleBackColor = true;
            this.chkUseLogFile.CheckedChanged += new System.EventHandler(this.chkUseLogFile_CheckedChanged);
            // 
            // txtPort
            // 
            this.txtPort.Location = new System.Drawing.Point(69, 35);
            this.txtPort.Mask = "######";
            this.txtPort.Name = "txtPort";
            this.txtPort.Size = new System.Drawing.Size(100, 20);
            this.txtPort.TabIndex = 4;
            this.txtPort.Text = "1986";
            this.ttpTips.SetToolTip(this.txtPort, "Sets the port on which the the server listens e.g. 1986");
            // 
            // lblPort
            // 
            this.lblPort.AutoSize = true;
            this.lblPort.Location = new System.Drawing.Point(6, 38);
            this.lblPort.Name = "lblPort";
            this.lblPort.Size = new System.Drawing.Size(29, 13);
            this.lblPort.TabIndex = 3;
            this.lblPort.Text = "Port:";
            // 
            // btnUseLocalhost
            // 
            this.btnUseLocalhost.Location = new System.Drawing.Point(324, 7);
            this.btnUseLocalhost.Name = "btnUseLocalhost";
            this.btnUseLocalhost.Size = new System.Drawing.Size(111, 23);
            this.btnUseLocalhost.TabIndex = 2;
            this.btnUseLocalhost.Text = "Use localhost";
            this.btnUseLocalhost.UseVisualStyleBackColor = true;
            this.btnUseLocalhost.Click += new System.EventHandler(this.btnUseLocalhost_Click);
            // 
            // txtHost
            // 
            this.txtHost.Location = new System.Drawing.Point(69, 9);
            this.txtHost.Name = "txtHost";
            this.txtHost.Size = new System.Drawing.Size(249, 20);
            this.txtHost.TabIndex = 1;
            this.txtHost.Text = "localhost";
            this.ttpTips.SetToolTip(this.txtHost, "Set the host name on which the server listens for requests e.g. localhost");
            // 
            // lblHost
            // 
            this.lblHost.AutoSize = true;
            this.lblHost.Location = new System.Drawing.Point(6, 12);
            this.lblHost.Name = "lblHost";
            this.lblHost.Size = new System.Drawing.Size(32, 13);
            this.lblHost.TabIndex = 0;
            this.lblHost.Text = "Host:";
            // 
            // tabCreateAdvanced
            // 
            this.tabCreateAdvanced.Controls.Add(this.chkStartAutoAdvanced);
            this.tabCreateAdvanced.Controls.Add(this.btnCreateAdvanced);
            this.tabCreateAdvanced.Controls.Add(this.txtCreateAdvanced);
            this.tabCreateAdvanced.Controls.Add(this.lblCreateAdvanced);
            this.tabCreateAdvanced.Location = new System.Drawing.Point(4, 22);
            this.tabCreateAdvanced.Name = "tabCreateAdvanced";
            this.tabCreateAdvanced.Padding = new System.Windows.Forms.Padding(3);
            this.tabCreateAdvanced.Size = new System.Drawing.Size(441, 304);
            this.tabCreateAdvanced.TabIndex = 1;
            this.tabCreateAdvanced.Text = "Create Server Advanced";
            this.tabCreateAdvanced.UseVisualStyleBackColor = true;
            // 
            // chkStartAutoAdvanced
            // 
            this.chkStartAutoAdvanced.AutoSize = true;
            this.chkStartAutoAdvanced.Location = new System.Drawing.Point(9, 119);
            this.chkStartAutoAdvanced.Name = "chkStartAutoAdvanced";
            this.chkStartAutoAdvanced.Size = new System.Drawing.Size(153, 17);
            this.chkStartAutoAdvanced.TabIndex = 2;
            this.chkStartAutoAdvanced.Text = "Start Server Automatically?";
            this.chkStartAutoAdvanced.UseVisualStyleBackColor = true;
            // 
            // btnCreateAdvanced
            // 
            this.btnCreateAdvanced.Location = new System.Drawing.Point(183, 144);
            this.btnCreateAdvanced.Name = "btnCreateAdvanced";
            this.btnCreateAdvanced.Size = new System.Drawing.Size(75, 23);
            this.btnCreateAdvanced.TabIndex = 3;
            this.btnCreateAdvanced.Text = "Create";
            this.btnCreateAdvanced.UseVisualStyleBackColor = true;
            this.btnCreateAdvanced.Click += new System.EventHandler(this.btnCreateAdvanced_Click);
            // 
            // txtCreateAdvanced
            // 
            this.txtCreateAdvanced.Location = new System.Drawing.Point(9, 48);
            this.txtCreateAdvanced.Multiline = true;
            this.txtCreateAdvanced.Name = "txtCreateAdvanced";
            this.txtCreateAdvanced.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtCreateAdvanced.Size = new System.Drawing.Size(415, 65);
            this.txtCreateAdvanced.TabIndex = 1;
            this.txtCreateAdvanced.Text = "-base . -log log-gui.txt -format common -verbose";
            // 
            // lblCreateAdvanced
            // 
            this.lblCreateAdvanced.Location = new System.Drawing.Point(6, 13);
            this.lblCreateAdvanced.Name = "lblCreateAdvanced";
            this.lblCreateAdvanced.Size = new System.Drawing.Size(429, 32);
            this.lblCreateAdvanced.TabIndex = 0;
            this.lblCreateAdvanced.Text = "To create a server enter the command line arguments to pass to rdfServer below.  " +
                "Advanced servers are always started as external processes";
            // 
            // tabRunningServers
            // 
            this.tabRunningServers.Controls.Add(this.btnStop);
            this.tabRunningServers.Controls.Add(this.btnPauseResume);
            this.tabRunningServers.Controls.Add(this.btnStart);
            this.tabRunningServers.Controls.Add(this.cboServers);
            this.tabRunningServers.Controls.Add(this.lblServers);
            this.tabRunningServers.Controls.Add(this.monServers);
            this.tabRunningServers.Location = new System.Drawing.Point(4, 22);
            this.tabRunningServers.Name = "tabRunningServers";
            this.tabRunningServers.Size = new System.Drawing.Size(441, 304);
            this.tabRunningServers.TabIndex = 2;
            this.tabRunningServers.Text = "Running Servers";
            this.tabRunningServers.UseVisualStyleBackColor = true;
            // 
            // btnStop
            // 
            this.btnStop.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.btnStop.Enabled = false;
            this.btnStop.Location = new System.Drawing.Point(261, 244);
            this.btnStop.Name = "btnStop";
            this.btnStop.Size = new System.Drawing.Size(75, 23);
            this.btnStop.TabIndex = 5;
            this.btnStop.Text = "Stop";
            this.btnStop.UseVisualStyleBackColor = true;
            this.btnStop.Click += new System.EventHandler(this.btnStop_Click);
            // 
            // btnPauseResume
            // 
            this.btnPauseResume.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.btnPauseResume.Enabled = false;
            this.btnPauseResume.Location = new System.Drawing.Point(180, 244);
            this.btnPauseResume.Name = "btnPauseResume";
            this.btnPauseResume.Size = new System.Drawing.Size(75, 23);
            this.btnPauseResume.TabIndex = 4;
            this.btnPauseResume.Text = "Pause";
            this.btnPauseResume.UseVisualStyleBackColor = true;
            this.btnPauseResume.Click += new System.EventHandler(this.btnPauseResume_Click);
            // 
            // btnStart
            // 
            this.btnStart.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.btnStart.Enabled = false;
            this.btnStart.Location = new System.Drawing.Point(99, 244);
            this.btnStart.Name = "btnStart";
            this.btnStart.Size = new System.Drawing.Size(75, 23);
            this.btnStart.TabIndex = 3;
            this.btnStart.Text = "Start";
            this.btnStart.UseVisualStyleBackColor = true;
            this.btnStart.Click += new System.EventHandler(this.btnStart_Click);
            // 
            // cboServers
            // 
            this.cboServers.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboServers.FormattingEnabled = true;
            this.cboServers.Location = new System.Drawing.Point(107, 12);
            this.cboServers.Name = "cboServers";
            this.cboServers.Size = new System.Drawing.Size(319, 21);
            this.cboServers.TabIndex = 1;
            this.cboServers.SelectedIndexChanged += new System.EventHandler(this.cboServers_SelectedIndexChanged);
            // 
            // lblServers
            // 
            this.lblServers.AutoSize = true;
            this.lblServers.Location = new System.Drawing.Point(12, 15);
            this.lblServers.Name = "lblServers";
            this.lblServers.Size = new System.Drawing.Size(89, 13);
            this.lblServers.TabIndex = 0;
            this.lblServers.Text = "Running Servers:";
            // 
            // monServers
            // 
            this.monServers.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.monServers.BufferSize = 50;
            this.monServers.Location = new System.Drawing.Point(15, 39);
            this.monServers.Name = "monServers";
            this.monServers.Size = new System.Drawing.Size(411, 199);
            this.monServers.TabIndex = 2;
            // 
            // ttpTips
            // 
            this.ttpTips.IsBalloon = true;
            // 
            // fclsServerManager
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(473, 354);
            this.Controls.Add(this.tabControls);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Name = "fclsServerManager";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.Text = "rdfServer GUI";
            this.Load += new System.EventHandler(this.fclsServerManager_Load);
            this.tabControls.ResumeLayout(false);
            this.tabCreate.ResumeLayout(false);
            this.tabCreate.PerformLayout();
            this.tabCreateAdvanced.ResumeLayout(false);
            this.tabCreateAdvanced.PerformLayout();
            this.tabRunningServers.ResumeLayout(false);
            this.tabRunningServers.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabControl tabControls;
        private System.Windows.Forms.TabPage tabCreate;
        private System.Windows.Forms.TabPage tabCreateAdvanced;
        private System.Windows.Forms.Label lblCreateAdvanced;
        private System.Windows.Forms.TabPage tabRunningServers;
        private System.Windows.Forms.TextBox txtCreateAdvanced;
        private System.Windows.Forms.Button btnCreateAdvanced;
        private System.Windows.Forms.Label lblServers;
        private System.Windows.Forms.ComboBox cboServers;
        private ServerMonitor monServers;
        private System.Windows.Forms.Button btnStart;
        private System.Windows.Forms.Button btnStop;
        private System.Windows.Forms.Button btnPauseResume;
        private System.Windows.Forms.CheckBox chkStartAutoAdvanced;
        private System.Windows.Forms.TextBox txtHost;
        private System.Windows.Forms.Label lblHost;
        private System.Windows.Forms.Button btnUseLocalhost;
        private System.Windows.Forms.Label lblPort;
        private System.Windows.Forms.MaskedTextBox txtPort;
        private System.Windows.Forms.CheckBox chkUseLogFile;
        private System.Windows.Forms.TextBox txtLogFile;
        private System.Windows.Forms.Label lblLogFile;
        private System.Windows.Forms.ComboBox cboLogFormat;
        private System.Windows.Forms.Label lblLogFormat;
        private System.Windows.Forms.TextBox txtConfigFile;
        private System.Windows.Forms.Label lblConfigFile;
        private System.Windows.Forms.ToolTip ttpTips;
        private System.Windows.Forms.Label lblBaseDirectory;
        private System.Windows.Forms.TextBox txtBaseDirectory;
        private System.Windows.Forms.CheckBox chkVerboseMode;
        private System.Windows.Forms.CheckBox chkQuietMode;
        private System.Windows.Forms.CheckBox chkRunExternal;
        private System.Windows.Forms.CheckBox chkStartAuto;
        private System.Windows.Forms.Button btnCreate;
    }
}

