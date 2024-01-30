namespace Osmy.Core.Configuration
{
    public static class DefaultServerConfig
    {
        /// <summary>
        /// Default path for Unix sockets used for communication between server and client
        /// </summary>
        public static string UnixSocketPath
        {
            get
            {
                return Environment.OSVersion.Platform switch
                {
                    PlatformID.Win32NT => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "Osmy", "osmy.server.sock"),
                    PlatformID.Unix => "/var/run/Osmy/osmy.server.sock",
                    _ => throw new NotSupportedException(),
                };
            }
        }

        /// <summary>
        /// Default directory path for data files
        /// </summary>
        public static string DataDirectory
        {
            get
            {
                return Environment.OSVersion.Platform switch
                {
                    PlatformID.Win32NT => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "Osmy"),
                    PlatformID.Unix => "/var/lib/Osmy",
                    _ => throw new NotSupportedException(),
                };
            }
        }

        /// <summary>
        /// Default directory path for configuration files
        /// </summary>
        public static string ConfigDirectory
        {
            get
            {
                return Environment.OSVersion.Platform switch
                {
                    PlatformID.Win32NT => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "Osmy", "Setting"),
                    PlatformID.Unix => "/etc/Osmy",
                    _ => throw new NotSupportedException(),
                };
            }
        }

        /// <summary>
        /// Default file name for general setting
        /// </summary>
        public static string GeneralSettingFileName => "general.json";

        /// <summary>
        /// Default file name for notification setting
        /// </summary>
        public static string NotificationSettingFileName => "notification.json";
    }
}
