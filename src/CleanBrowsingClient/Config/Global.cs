using Serilog.Events;

namespace CleanBrowsingClient.Config
{
    public static class Global
    {
        /// <summary>
        ///     The name of this application.
        /// </summary>
        public const string ApplicationName = "Clean Browsing Client";

        /// <summary>
	    ///     Name of the company.
	    /// </summary>
        public const string CompanyName = "cleanbrowsing.org";

        /// <summary>
        ///     First year of copyright.
        /// </summary>
        public const int CopyrightYear = 2019;

        /// <summary>
	    ///     Remote URI where the application will find the update informations.
	    /// </summary>
        public const string RemoteUpdateCheckUrl = "https://raw.githubusercontent.com/bitbeans/CleanBrowsingClient/master/update.yml";

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

        /// <summary>
        ///     Name of the family filter key.
        /// </summary>
        public const string DefaultFamilyFilterKey = "cleanbrowsing-family";

        /// <summary>
        ///     Default family filter stamp.
        /// </summary>
        public const string DefaultFamilyFilterStamp = "sdns://AQMAAAAAAAAAFDE4NS4yMjguMTY4LjE2ODo4NDQzILysMvrVQ2kXHwgy1gdQJ8MgjO7w6OmflBjcd2Bl1I8pEWNsZWFuYnJvd3Npbmcub3Jn";

        /// <summary>
        ///     Name of the adult filter key.
        /// </summary>
        public const string DefaultAdultFilterKey = "cleanbrowsing-adult";

        /// <summary>
        ///     Default adult filter stamp.
        /// </summary>
        public const string DefaultAdultFilterStamp = "sdns://AQMAAAAAAAAAEzE4NS4yMjguMTY4LjEwOjg0NDMgvKwy-tVDaRcfCDLWB1AnwyCM7vDo6Z-UGNx3YGXUjykRY2xlYW5icm93c2luZy5vcmc";

        /// <summary>
        ///     Name of the custom filter key.
        /// </summary>
        public const string DefaultCustomFilterKey = "cleanbrowsing-custom";

        /// <summary>
        ///     String to validate DoH stamps.
        /// </summary>
        public const string ValidCleanBrowsingDohStamp = "doh.cleanbrowsing.org";

        /// <summary>
        ///     String to validate DnsCrypt stamps.
        /// </summary>
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

        /// <summary>
        ///     Default log level.
        /// </summary>
        public const LogEventLevel DefaultLogEventLevel = LogEventLevel.Information;

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
