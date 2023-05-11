namespace Osmy.Core.Configuration
{
    public static class DefaultServerConfig
    {
        /// <summary>
        /// Unixソケットのパス
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
        /// データディレクトリのパス
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
        /// 設定ディレクトリ
        /// </summary>
        public static string SettingDirectory
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
    }
}
