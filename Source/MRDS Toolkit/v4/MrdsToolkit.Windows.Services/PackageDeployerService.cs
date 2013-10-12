using System;
using System.Configuration;
using System.IO;
using CodeForDotNet.Full.Net;
using Microsoft.Ccr.Core;
using Microsoft.Dss.Core;

namespace MrdsToolkit.Windows.Services
{
    /// <summary>
    /// MSRDS Package Deployer DSS service.
    /// </summary>
    public class PackageDeployerService : IDisposable
    {
        #region Lifetime

        /// <summary>
        /// Creates an instance with the default configuration as specified in the
        /// DSS runtime application settings section of this programs configuration file.
        /// </summary>
        public PackageDeployerService()
        {
            var security = SecurityManager.CreateDefault();
            _configuration = new DssRuntimeConfiguration {SecuritySettings = SecurityManager.Serialize(security)};
        }

        /// <summary>
        /// Creates an instance with the default configuration, current directory as root and the
        /// specified remote option.
        /// </summary>
        public PackageDeployerService(bool allowRemote, string securitySettings)
            : this(Directory.GetCurrentDirectory(), allowRemote, securitySettings)
        {
        }

        /// <summary>
        /// Creates an instance with the default configuration with the specified root dirctory and remote option.
        /// </summary>
        public PackageDeployerService(string rootDirectory, bool allowRemote, string securitySettings)
            : this(NetworkExtensions.GetFullHostName(), MrdsConstants.PackageDeployerServiceDefaultPort, 
            rootDirectory, allowRemote, securitySettings)
        {
        }

        /// <summary>
        /// Creates an instance with the specified parameters.
        /// </summary>
        public PackageDeployerService(string hostName, int tcpPort, string rootDirectory, 
            bool allowRemote, string securitySettings)
        {
            // Validate
            if (String.IsNullOrWhiteSpace(hostName)) 
                throw new ArgumentNullException("hostName");
            if (tcpPort <= 0) 
                throw new ArgumentOutOfRangeException("tcpPort");
            if (String.IsNullOrWhiteSpace(rootDirectory))
            {
                // Use current directory when root not specified
                rootDirectory = Directory.GetCurrentDirectory();
            }
            else
            {
                // Expand, check and create path when specified
                rootDirectory = Path.GetFullPath(rootDirectory);
                if (!Directory.Exists(rootDirectory))
                    Directory.CreateDirectory(rootDirectory);
            }
            if (String.IsNullOrWhiteSpace(securitySettings))
                throw new ArgumentNullException("securitySettings");

            // Override configuration options with our specific settings
            ConfigurationManager.AppSettings[MrdsConstants.AllowRemoteAccessAppSettingName] = allowRemote.ToString();
            _configuration = new DssRuntimeConfiguration
            {
                HostName = hostName,
                PublicTcpPort = tcpPort,
                HostRootDir = rootDirectory,
                SecuritySettings = securitySettings
            };
        }

        /// <summary>
        /// Disposes unmanaged resources during finalization when <see cref="Dispose()"/> has not been called.
        /// </summary>
        ~PackageDeployerService()
        {
            // Partial (unmanaged) dispose only (no managed access guarranteed during fianliaztion)
            Dispose(false);
        }

        /// <summary>
        /// Proactively frees resources owned by this object.
        /// </summary>
        public void Dispose()
        {
            // Dispose only once
            if (_disposed)
                return;
            _disposed = true;

            // Dispose all resources (including managed resources)
            try
            {
                Dispose(true);
            }
            finally
            {
                // Suppress finaliazer as no longer necessary
                GC.SuppressFinalize(this);
            }
        }

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        private void Dispose(bool disposing)
        {
            // Dispose managed resources during dispose
            if (disposing)
            {
                if (_host != null)
                    _host.Dispose();
            }
        }

        #endregion

        #region Private Fields

        /// <summary>
        /// Indicates this instance has already been disposed.
        /// </summary>
        private bool _disposed;

        /// <summary>
        /// Package Deployer service host.
        /// </summary>
        private DssRuntimeLoader _host;

        /// <summary>
        /// Configuration used to start the host.
        /// </summary>
        private readonly DssRuntimeConfiguration _configuration;

        #endregion

        #region Public Events

        /// <summary>
        /// Thrown when the <see cref="DssRuntimeLoader.UserTaskQueue"/>'s <see cref="DispatcherQueue.UnhandledException"/> event is fired.
        /// </summary>
        public event UnhandledExceptionEventHandler Error;

        #endregion

        #region Public Methods

        /// <summary>
        /// Starts the service in the DSS host.
        /// </summary>
        public void Start()
        {
            // Initialize host
            PrepareServiceDirectory(_configuration.HostRootDir);
             _host = new DssRuntimeLoader(_configuration);
            _host.UserTaskQueue.UnhandledException += (sender, args) =>
                {
                    // Bubble error event
                    if (Error != null)
                        Error(sender, args);
                };

            // Initialize service
            _host.InitializeByContract(new Uri(MrdsConstants.PackageDeployerServiceContractUri));
        }

        /// <summary>
        /// Stops the DSS host if running.
        /// </summary>
        public void Stop()
        {
            // Do nothing when not running
            if (_host == null) return;

            // Stop services
            _host.Dispose();
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Checks a service directory is ready to start.
        /// Ensure subdirectories are created for which the DSS host throws errors on the first pass when they do not exist.
        /// </summary>
        private void PrepareServiceDirectory(string path)
        {
            // Create the store subdirectory if it doesn't exist
            var storeDirectory = Path.Combine(path, LayoutPaths.StoreDir);
            if (!Directory.Exists(storeDirectory))
                Directory.CreateDirectory(storeDirectory);

            // Create the security settings file if it doesn't exist
            var securityFilePath = Path.Combine(storeDirectory, MrdsConstants.SecuritySettingsFileName);
            if (!File.Exists(securityFilePath))
                File.WriteAllText(securityFilePath, _configuration.SecuritySettings);
        }

        #endregion
    }
}
