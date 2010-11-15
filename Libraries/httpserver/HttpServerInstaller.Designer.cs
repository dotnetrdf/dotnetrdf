namespace VDS.Web
{
    partial class HttpServerInstaller
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(HttpServerInstaller));
            this.svcInstaller = new System.ServiceProcess.ServiceProcessInstaller();
            this.svcHttpServer = new System.ServiceProcess.ServiceInstaller();
            // 
            // svcInstaller
            // 
            this.svcInstaller.Account = System.ServiceProcess.ServiceAccount.NetworkService;
            this.svcInstaller.Password = null;
            this.svcInstaller.Username = null;
            // 
            // svcHttpServer
            // 
            this.svcHttpServer.Description = resources.GetString("svcHttpServer.Description");
            this.svcHttpServer.DisplayName = "VDS.Web.HttpServer";
            this.svcHttpServer.ServiceName = "HttpServerService";
            // 
            // HttpServerInstaller
            // 
            this.Installers.AddRange(new System.Configuration.Install.Installer[] {
            this.svcInstaller,
            this.svcHttpServer});

        }

        #endregion

        private System.ServiceProcess.ServiceProcessInstaller svcInstaller;
        private System.ServiceProcess.ServiceInstaller svcHttpServer;
    }
}