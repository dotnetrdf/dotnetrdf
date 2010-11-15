namespace rdfServer
{
    partial class RdfServerServiceInstaller
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

        #region Component Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.svcInstaller = new System.ServiceProcess.ServiceProcessInstaller();
            this.svcRdfServerService = new System.ServiceProcess.ServiceInstaller();
            // 
            // svcInstaller
            // 
            this.svcInstaller.Account = System.ServiceProcess.ServiceAccount.NetworkService;
            this.svcInstaller.Password = null;
            this.svcInstaller.Username = null;
            // 
            // svcRdfServerService
            // 
            this.svcRdfServerService.DisplayName = "rdfServerService";
            this.svcRdfServerService.ServiceName = "RdfServerService";
            // 
            // ProjectInstaller
            // 
            this.Installers.AddRange(new System.Configuration.Install.Installer[] {
            this.svcInstaller,
            this.svcRdfServerService});

        }

        #endregion

        private System.ServiceProcess.ServiceProcessInstaller svcInstaller;
        private System.ServiceProcess.ServiceInstaller svcRdfServerService;
    }
}