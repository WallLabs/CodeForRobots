using System;
using System.ComponentModel;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.ServiceProcess;
using System.Threading;
using CodeForDotNet.Full.Net;
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

            // Redirect console output (used explicitly by MRDS) to the central log (a standard trace source)
            _consoleOutputWriter = new TraceSourceTextWriter(_log, TraceEventType.Information);
            Console.SetOut(_consoleOutputWriter);
            _consoleErrorWriter = new TraceSourceTextWriter(_log, TraceEventType.Error);
            Console.SetError(_consoleErrorWriter);
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
                    if (_consoleOutputWriter != null)
                        _consoleOutputWriter.Close();
                    if (_consoleErrorWriter != null)
                        _consoleErrorWriter.Close();
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

        /// <summary>
        /// Console output writer which redirects to the log as information events.
        /// </summary>
        private readonly TraceSourceTextWriter _consoleOutputWriter;

        /// <summary>
        /// Console error writer which redirects to the log as error events.
        /// </summary>
        private readonly TraceSourceTextWriter _consoleErrorWriter;

        #endregion

        #region Event Handlers

        /// <summary>
        /// Starts the services when the Windows service is started.
        /// </summary>
        protected override void OnStart(string[] args)
        {
            // Start service...
            // No service start/success message logging is required because the AutoLog feature is enabled
            // and required to catch messages during .NET AppDomain creation, e.g. assembly load failure.
            try
            {
                // Delay start when option is configured
                var delay = Settings.Default.StartDelay;
                while (delay-- > 0)
                {
                    RequestAdditionalTime(1000);
                    Thread.Sleep(1000);             // Set breakpoint here to debug service start-up
                }

                // Set current directory to service executable path (default is system directory)
                var programDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                Debug.Assert(programDirectory != null);
                Directory.SetCurrentDirectory(programDirectory);

                // Get host name from configuration or local DNS name
                var hostName = Settings.Default.HostName;
                if (hostName != null)
                    hostName = hostName.Trim();
                if (String.IsNullOrWhiteSpace(hostName) ||
                    hostName.Equals("localhost", StringComparison.OrdinalIgnoreCase) ||
                    hostName.Equals("127.0.0.1", StringComparison.OrdinalIgnoreCase) ||
                    hostName.Equals("::1", StringComparison.OrdinalIgnoreCase))
                    hostName = NetworkExtensions.GetFullHostName();

                // Get root directory from configuration or service path
                var rootDirectory = Settings.Default.PackageDeployerRootDirectory;
                if (rootDirectory != null)
                    rootDirectory = rootDirectory.Trim();
                rootDirectory = !String.IsNullOrWhiteSpace(rootDirectory)
                                    ? Path.Combine(programDirectory, rootDirectory)
                                    : programDirectory;

                // Get security settings
                var security = SecurityManager.CreateDefault();
                var securitySettings = SecurityManager.Serialize(security);

                // Start Package Deployer
                RequestAdditionalTime(Settings.Default.ServiceStartTimeout * 1000);
                _packageDeployer = new PackageDeployerService(hostName, Settings.Default.PackageDeployerPort,
                                                              rootDirectory, true, securitySettings);
                _packageDeployer.Error += OnHostedServiceError;
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
                {
                    RequestAdditionalTime(Settings.Default.ServiceStopTimeout * 1000);
                    _packageDeployer.Stop();
                }
            }
            catch (Exception error)
            {
                // Log failure
                _log.TraceEvent(TraceEventType.Error, 0, Resources.ServiceFailedToStop,
                                error.GetFullMessage(true));
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Handles exceptions in hosted services, logging them as errors.
        /// </summary>
        private void OnHostedServiceError(object sender, UnhandledExceptionEventArgs args)
        {
            // Parse error message
            var error = args.ExceptionObject as Exception;
            if (error == null) throw new ArgumentOutOfRangeException("args");
            var message = error.GetFullMessage(true);

            // Log error
            _log.TraceEvent(TraceEventType.Error, 0, message);
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
