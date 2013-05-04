using System.ComponentModel;

namespace CodeForRobots.Mrds.Windows.ServiceHost
{
    /// <summary>
    /// Installs resources required by this assembly.
    /// </summary>
    [RunInstaller(true)]
    public partial class ProjectInstaller : System.Configuration.Install.Installer
    {
        /// <summary>
        /// Creates the installer.
        /// </summary>
        public ProjectInstaller()
        {
            InitializeComponent();
        }
    }
}
