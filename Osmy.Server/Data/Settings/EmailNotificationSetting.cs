namespace Osmy.Server.Data.Settings
{
    public class EmailNotificationSetting
    {
        public bool IsEnabled { get; set; }

        public string Host { get; set; } = string.Empty;

        public int Port { get; set; }

        public string Username { get; set; } = string.Empty;

        public string Password { get; set; } = string.Empty;

        public string[] To { get; set; } = [];

        public string[] Cc { get; set; } = [];

        public string[] Bcc { get; set; } = [];
    }
}
