using System;
using System.IO;
using System.Net;
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
        /// Creates an instance with the default configuration and the current root directory.
        /// </summary>
        public PackageDeployerService()
            : this(Directory.GetCurrentDirectory())
        {
        }

        /// <summary>
        /// Creates an instance with the default configuration and the specified root dirctory..
        /// </summary>
        public PackageDeployerService(string rootDirectory)
            : this(Dns.GetHostName(), MrdsConstants.PackageDeployerServiceDefaultPort, rootDirectory)
        {
        }

        /// <summary>
        /// Creates an instance with the specified parameters.
        /// </summary>
        public PackageDeployerService(string hostName, int tcpPort, string rootDirectory)
        {
            HostName = hostName;
            TcpPort = tcpPort;
            RootDirectory = rootDirectory;
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
            // Dispose all resources (includign managed)
            Dispose(true);

            // Suppress finaliazer as no longer necessary
            GC.SuppressFinalize(this);
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

        #region Public Properties

        /// <summary>
        /// Package Deployer service host.
        /// </summary>
        private DssRuntimeLoader _host;

        /// <summary>
        /// Host name of the DSS host, used by remote nodes to callback the service.
        /// </summary>
        public string HostName { get; private set; }

        /// <summary>
        /// TCP port of the service.
        /// </summary>
        public int TcpPort { get; private set; }

        /// <summary>
        /// Working directory for the DSS host, used to resolve relative paths accessed by services.
        /// </summary>
        public string RootDirectory { get; private set; }

        #endregion

        #region Public Methods

        /// <summary>
        /// Starts the service in the DSS host.
        /// </summary>
        public void Start()
        {
            var configuration = new DssRuntimeConfiguration
            {
                HostName = HostName,
                PublicTcpPort = TcpPort,
                HostRootDir = RootDirectory
            };
            _host = new DssRuntimeLoader(configuration);
            _host.InitializeByContract(new Uri(MrdsConstants.PackageDeployerServiceContractUri));
        }

        /// <summary>
        /// Stops the DSS host if running.
        /// </summary>
        public void Stop()
        {
            // Stop services
            if (_host != null)
                _host.WaitForShutdown();
        }

        #endregion
    }
}
