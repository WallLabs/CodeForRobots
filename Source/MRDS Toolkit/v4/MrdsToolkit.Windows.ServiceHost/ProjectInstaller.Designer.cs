namespace CodeForRobots.Mrds.Windows.ServiceHost
{
    partial class ProjectInstaller
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
            this._processInstaller = new System.ServiceProcess.ServiceProcessInstaller();
            this._installer = new System.ServiceProcess.ServiceInstaller();
            // 
            // _processInstaller
            // 
            this._processInstaller.Account = System.ServiceProcess.ServiceAccount.NetworkService;
            this._processInstaller.Password = null;
            this._processInstaller.Username = null;
            // 
            // _installer
            // 
            this._installer.Description = "Hosts DSS services including the Package Deployer enabling remote deployment.";
            this._installer.DisplayName = "Microsoft Robotics Studio Service Host";
            this._installer.ServiceName = "Microsoft Robotics Package Deployer";
            this._installer.StartType = System.ServiceProcess.ServiceStartMode.Automatic;
            // 
            // ProjectInstaller
            // 
            this.Installers.AddRange(new System.Configuration.Install.Installer[] {
            this._processInstaller,
            this._installer});

        }

        #endregion

        private System.ServiceProcess.ServiceProcessInstaller _processInstaller;
        private System.ServiceProcess.ServiceInstaller _installer;
    }
}