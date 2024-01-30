namespace Osmy.Server.Data.Settings
{
    public class GeneralSetting
    {
        public TimeSpan VulnerabilityScanInterval { get; set; } = TimeSpan.FromDays(1);

        public TimeSpan ChecksumVerificationInterval { get; set; } = TimeSpan.FromDays(1);
    }
}
