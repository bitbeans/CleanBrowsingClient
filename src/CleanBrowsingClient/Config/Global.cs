using Serilog.Events;

namespace CleanBrowsingClient.Config
{
    public static class Global
    {
        /// <summary>
        ///     The name of this application.
        /// </summary>
        public const string ApplicationName = "Clean Browsing Client";
        public const string ApplicationRepository = "https://github.com/bitbeans";

        public const string CompanyName = "cleanbrowsing.org";

        public const int CopyrightYear = 2019;

        public const string CompanyHomepage = "https://cleanbrowsing.org/";

        /// <summary>
        ///     The name of the application configuration file.
        /// </summary>
        public const string AppConfigurationFile = "cleanbrowsing.toml";

        /// <summary>
        ///     The folder where the dnscrypt-proxy lives in.
        /// </summary>
        public const string DnsCryptProxyFolder = "dnscrypt-proxy";

        /// <summary>
        ///     The name of the dnscrypt-proxy executable (located in DnsCryptProxyFolder).
        /// </summary>
        public const string DnsCryptProxyExecutableName = "dnscrypt-proxy.exe";

        /// <summary>
        ///     The name of the dnscrypt-proxy configuration file (located in DnsCryptProxyFolder).
        /// </summary>
        public const string DnsCryptConfigurationFile = "dnscrypt-proxy.toml";

        public const string DefaultFamilyFilterKey = "cleanbrowsing-family";
        public const string DefaultFamilyFilterStamp = "sdns://AQMAAAAAAAAAFDE4NS4yMjguMTY4LjE2ODo4NDQzILysMvrVQ2kXHwgy1gdQJ8MgjO7w6OmflBjcd2Bl1I8pEWNsZWFuYnJvd3Npbmcub3Jn";
        public const string DefaultAdultFilterKey = "cleanbrowsing-adult";
        public const string DefaultAdultFilterStamp = "sdns://AQMAAAAAAAAAEzE4NS4yMjguMTY4LjEwOjg0NDMgvKwy-tVDaRcfCDLWB1AnwyCM7vDo6Z-UGNx3YGXUjykRY2xlYW5icm93c2luZy5vcmc";
        public const string DefaultCustomFilterKey = "cleanbrowsing-custom";

        public const string ValidCleanBrowsingDohStamp = "doh.cleanbrowsing.org";
        public const string ValidCleanBrowsingDnsCryptStamp = "cleanbrowsing.org";

        /// <summary>
        ///     Time we wait on a service restart (ms).
        /// </summary>
        public const int ServiceRestartTime = 5000;

        /// <summary>
        ///     Time we wait on a service start (ms).
        /// </summary>
        public const int ServiceStartTime = 2500;

        /// <summary>
        ///     Time we wait on a service stop (ms).
        /// </summary>
        public const int ServiceStopTime = 2500;

        /// <summary>
        ///     Time we wait on a service uninstall (ms).
        /// </summary>
        public const int ServiceUninstallTime = 2500;

        /// <summary>
        ///     Time we wait on a service install (ms).
        /// </summary>
        public const int ServiceInstallTime = 3000;

        public const LogEventLevel DefaultLogEventLevel = LogEventLevel.Information;

        public const string DefaultResolverIpv4 = "127.0.0.1:53";
        public const string DefaultResolverIpv6 = "[::1]:53";

        /// <summary>
        ///     List of files must exist.
        /// </summary>
        public static readonly string[] DnsCryptProxyFiles =
        {
            "dnscrypt-proxy.exe",
            "dnscrypt-proxy.toml",
            "LICENSE"
        };

        /// <summary>
        ///     List of interfaces, marked as hidden.
        /// </summary>
        public static readonly string[] NetworkInterfaceBlacklist =
        {
            "Microsoft Virtual",
            "Hamachi Network",
            "VMware Virtual",
            "VirtualBox",
            "Software Loopback",
            "Microsoft ISATAP",
            "Microsoft-ISATAP",
            "Teredo Tunneling Pseudo-Interface",
            "Microsoft Wi-Fi Direct Virtual",
            "Microsoft Teredo Tunneling Adapter",
            "Von Microsoft gehosteter",
            "Microsoft hosted",
            "Virtueller Microsoft-Adapter",
            "TAP"
        };
    }
}
