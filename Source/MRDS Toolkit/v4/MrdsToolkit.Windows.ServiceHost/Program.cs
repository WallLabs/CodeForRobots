using System.ServiceProcess;

namespace MrdsToolkit.Windows.ServiceHost
{
    /// <summary>
    /// Main program.
    /// </summary>
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main()
        {
            var servicesToRun = new ServiceBase[] { new Service() };
            ServiceBase.Run(servicesToRun);
        }
    }
}
