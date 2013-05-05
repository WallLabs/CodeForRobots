namespace MrdsToolkit.Windows.Services
{
    /// <summary>
    /// Defines constants used throughout Microsoft Robotics Developer Studio.
    /// </summary>
    public static class MrdsConstants
    {
        /// <summary>
        /// Package Deployer service control URI.
        /// </summary>
        public const string PackageDeployerServiceContractUri = "http://schemas.microsoft.com/dss/2008/01/packagedeployer.html";

        /// <summary>
        /// Default port for the Package Deployer service.
        /// </summary>
        public const int PackageDeployerServiceDefaultPort = 55555;

        /// <summary>
        /// DSS runtime configuration application setting name which enables remote access.
        /// </summary>
        public const string AllowRemoteAccessAppSettingName = "Microsoft.Dss.Services.Transports.AllowUnsecuredRemoteAccess";

        /// <summary>
        /// Name of the security settings XML file.
        /// </summary>
        public const string SecuritySettingsFileName = "SecuritySettings.xml";
    }
}
