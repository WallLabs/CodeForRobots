using Microsoft.VisualStudio.TestTools.UnitTesting;
using MrdsToolkit.Windows.Services;

namespace MrdsToolkit.Tests
{
    [TestClass]
    public class ServiceTests
    {
        /// <summary>
        /// Tests the <see cref="PackageDeployerService"/>.
        /// </summary>
        [TestMethod]
        public void ServiceTestPackageDeployer()
        {
            // Get security settings
            var security = SecurityManager.CreateDefault();
            var securitySettings = SecurityManager.Serialize(security);

            // Start Package Deployer
            using (var service = new PackageDeployerService("Services\\Package Deployer", true, securitySettings))
            {
                service.Start();
                //Thread.Sleep(5000);
                service.Stop();
            }
        }
    }
}
