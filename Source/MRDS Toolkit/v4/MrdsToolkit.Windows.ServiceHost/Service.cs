using System;
using System.ComponentModel;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Reflection;
using System.ServiceProcess;
using System.Threading;
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
            // Initialize designer classes
            InitializeComponent();

            // Initialize members
            _log = new TraceSource(MrdsToolkitConstants.ServiceHostTraceSourceName);
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
                    // Close services
                    if (_packageDeployer != null)
                        _packageDeployer.Dispose();

                    // Close log (may hold locks)
                    if (_log != null)
                        _log.Close();

                    // Dispose any other components
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

        /// <summary>
        /// Event log.
        /// </summary>
        private readonly TraceSource _log;

        #endregion

        #region Event Handlers

        /// <summary>
        /// Starts the services when the Windows service is started.
        /// </summary>
        protected override void OnStart(string[] args)
        {
            // Un-comment the following section between the "---"'s to debug service start-up:
            // -------------------------------------------------------------------------------
#if DEBUG
            // Wait for debugger attach (in DEBUG builds when "/debug" parameter is passed)
            var waitSeconds = 30;
            RequestAdditionalTime(15 * 60 * 1000);      // Long start timeout for debugging
            while (waitSeconds-- > 0)
            {
                RequestAdditionalTime(1000);
                Thread.Sleep(1000); // Set breakpoint here
            }
#endif
            // -------------------------------------------------------------------------------

            // Start service...
            // No service start/success message logging is required because the AutoLog feature is enabled
            // and required to catch messages during .NET AppDomain creation, e.g. assembly load failure.
            try
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
                    rootDirectory = Path.GetDirectoryName(Path.GetFullPath(serviceAssembly.Location));
                }

                // Start Package Deployer
                _packageDeployer = new PackageDeployerService(
                    hostName, Settings.Default.PackageDeployerPort, rootDirectory);
                _packageDeployer.Start();
            }
            catch (ConfigurationException error)
            {
                // Log configuration errors directly to event log (TraceSource will not work as config failed)
                EventLog.WriteEntry(MrdsToolkitConstants.ServiceHostTraceSourceName, error.GetFullMessage(true),
                                    EventLogEntryType.Error);
            }
            catch (Exception error)
            {
                // Log failure
                _log.TraceEvent(TraceEventType.Error, 0, Resources.ServiceFailedToStart,
                                error.GetFullMessage(true));

                // Re-throw exception to cause start to fail
                throw;
            }
        }

        /// <summary>
        /// Stops services when the Windows service is stopped.
        /// </summary>
        protected override void OnStop()
        {
            // No service stop/success message logging required because the AutoLog feature is enabled
            // and required to catch messages during .NET AppDomain creation, e.g. assembly load failure.
            try
            {
                // Stop services
                if (_packageDeployer != null)
                    _packageDeployer.Stop();
            }
            catch (Exception error)
            {
                // Log failure
                _log.TraceEvent(TraceEventType.Error, 0, Resources.ServiceFailedToStop,
                                error.GetFullMessage(true));

                // Re-throw exception to cause stop to fail
                throw;
            }
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
