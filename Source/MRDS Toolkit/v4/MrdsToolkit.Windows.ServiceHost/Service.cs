using System;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Reflection;
using System.ServiceProcess;
using MrdsToolkit.Windows.ServiceHost.Properties;
using MrdsToolkit.Windows.Services;

namespace MrdsToolkit.Windows.ServiceHost
{
    /// <summary>
    /// Microsoft Robotics Developer Studio Windows service host.
    /// </summary>
    public partial class Service : ServiceBase
    {
        #region Lifetime

        /// <summary>
        /// Creates the service.
        /// </summary>
        public Service()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            try
            {
                // Dispose managed resources during dispose
                if (disposing)
                {
                    if (_packageDeployer != null)
                        _packageDeployer.Dispose();
                    if (_components != null)
                        _components.Dispose();
                }
            }
            finally
            {
                // Dispose base class
                base.Dispose(disposing);
            }
        }

        #endregion

        #region Private Fields

        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private readonly IContainer _components = null;

        #endregion

        #region Event Handlers

        /// <summary>
        /// Starts the services when the Windows service is started.
        /// </summary>
        protected override void OnStart(string[] args)
        {
            // Get host name from configuration or local DNS name
            var hostName = Settings.Default.HostName;
            if (hostName != null)
                hostName = hostName.Trim();
            if (String.IsNullOrWhiteSpace(hostName) ||
                hostName.Equals("localhost", StringComparison.OrdinalIgnoreCase) ||
                hostName.Equals("127.0.0.1", StringComparison.OrdinalIgnoreCase) ||
                hostName.Equals("::1", StringComparison.OrdinalIgnoreCase))
                hostName = Dns.GetHostName();

            // Get root directory from configuration or service path
            var rootDirectory = Settings.Default.PackageDeployerRootDirectory;
            if (rootDirectory != null)
                rootDirectory = rootDirectory.Trim();
            if (String.IsNullOrWhiteSpace(rootDirectory))
            {
                var serviceAssembly = Assembly.GetExecutingAssembly();
                rootDirectory = Path.GetFullPath(serviceAssembly.CodeBase);
            }

            // Start Package Deployer
            _packageDeployer = new PackageDeployerService(hostName, 
                Settings.Default.PackageDeployerPort, rootDirectory);
            _packageDeployer.Start();
        }

        /// <summary>
        /// Stops services when the Windows service is stopped.
        /// </summary>
        protected override void OnStop()
        {
            // Stop services
            if (_packageDeployer != null)
                _packageDeployer.Stop();
        }

        #endregion

        #region Package Deployer Service

        /// <summary>
        /// Package Deployer service host.
        /// </summary>
        private PackageDeployerService _packageDeployer;

        #endregion
    }
}
