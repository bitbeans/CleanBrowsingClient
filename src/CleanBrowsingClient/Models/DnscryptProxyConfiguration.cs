using DnsCrypt.Models;
using Nett;
using Prism.Mvvm;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace CleanBrowsingClient.Models
{
    public class DnscryptProxyConfiguration : BindableBase
    {
        private ObservableCollection<string> _server_names;
        private ObservableCollection<string> _listen_addresses;
        private int _max_clients;
        private bool _require_nofilter;
        private bool _ipv4_servers;
        private bool _ipv6_servers;
        private bool _dnscrypt_servers;
        private bool _doh_servers;
        private bool _require_dnssec;
        private bool _require_nolog;
        private bool _force_tcp;
        private string _http_proxy;
        private int _timeout;
        private int _keepalive;
        private string _lb_strategy;
        private bool _lb_estimator;
        private int _cert_refresh_delay;
        private bool _dnscrypt_ephemeral_keys;
        private bool _tls_disable_session_tickets;
        private bool _log;
        private int _log_level;
        private string _log_file;
        private Dictionary<string, Source> _sources;
        private bool _use_syslog;
        private int _netprobe_timeout;
        private string _netprobe_address;
        private bool _offline_mode;
        private int _log_files_max_size;
        private int _log_files_max_age;
        private int _log_files_max_backups;
        private bool _block_ipv6;
        private bool _block_unqualified;
        private bool _block_undelegated;
        private int _reject_ttl;
        private string _forwarding_rules;
        private string _cloaking_rules;
        private int _cache_neg_min_ttl;
        private int _cache_neg_max_ttl;
        private int _cache_max_ttl;
        private int _cache_min_ttl;
        private int _cache_size;
        private bool _cache;
        private ObservableCollection<string> _fallback_resolvers;
        private bool _ignore_system_dns;
        private Dictionary<string, Static> _static;
        private string _proxy;
        private ObservableCollection<string> _disabled_server_names;
        private string _blocked_query_response;

        /// <summary>
        ///     List of servers to use.
        /// </summary>
        public ObservableCollection<string> server_names
        {
            get { return _server_names; }
            set { SetProperty(ref _server_names, value); }
        }

        /// <summary>
        ///     List of local addresses and ports to listen to. Can be IPv4 and/or IPv6.
        /// </summary>
        public ObservableCollection<string> listen_addresses
        {
            get { return _listen_addresses; }
            set { SetProperty(ref _listen_addresses, value); }
        }

        /// <summary>
        ///     Maximum number of simultaneous client connections to accept.
        /// </summary>
        public int max_clients
        {
            get { return _max_clients; }
            set { SetProperty(ref _max_clients, value); }
        }

        /// <summary>
        ///     Use servers reachable over IPv4
        /// </summary>
        public bool ipv4_servers
        {
            get { return _ipv4_servers; }
            set { SetProperty(ref _ipv4_servers, value); }
        }

        /// <summary>
        ///     Use servers reachable over IPv6 -- Do not enable if you don't have IPv6 connectivity
        /// </summary>
        public bool ipv6_servers
        {
            get { return _ipv6_servers; }
            set { SetProperty(ref _ipv6_servers, value); }
        }

        /// <summary>
        ///     Server names to avoid even if they match all criteria
        /// </summary>
        public ObservableCollection<string> disabled_server_names
        {
            get { return _disabled_server_names; }
            set { SetProperty(ref _disabled_server_names, value); }
        }

        /// <summary>
        ///		Response for blocked queries.  Options are `refused`, `hinfo` (default) or
        ///		an IP response.  To give an IP response, use the format `a:<IPv4>,aaaa:<IPv6>`.
        ///		Using the `hinfo` option means that some responses will be lies.
        ///		Unfortunately, the `hinfo` option appears to be required for Android 8+
        /// </summary>
        public string blocked_query_response
        {
            get { return _blocked_query_response; }
            set { SetProperty(ref _blocked_query_response, value); }
        }

        /// <summary>
        ///		Use servers implementing the DNSCrypt protocol
        /// </summary>
        public bool dnscrypt_servers
        {
            get { return _dnscrypt_servers; }
            set { SetProperty(ref _dnscrypt_servers, value); }
        }

        /// <summary>
        ///		Use servers implementing the DNS-over-HTTPS protocol
        /// </summary>
        public bool doh_servers
        {
            get { return _doh_servers; }
            set { SetProperty(ref _doh_servers, value); }
        }

        /// <summary>
        ///     Server must support DNS security extensions
        /// </summary>
        public bool require_dnssec
        {
            get { return _require_dnssec; }
            set { SetProperty(ref _require_dnssec, value); }
        }

        /// <summary>
        ///     Server must not log user queries
        /// </summary>
        public bool require_nolog
        {
            get { return _require_nolog; }
            set { SetProperty(ref _require_nolog, value); }
        }

        /// <summary>
        ///     Server must not enforce its own blacklist (for parental control, ads blocking...)
        /// </summary>
        public bool require_nofilter
        {
            get { return _require_nofilter; }
            set { SetProperty(ref _require_nofilter, value); }
        }

        /// <summary>
        ///     linux only.
        /// </summary>
        public bool daemonize { get; set; } = false;

        /// <summary>
        ///     Always use TCP to connect to upstream servers.
        /// </summary>
        public bool force_tcp
        {
            get { return _force_tcp; }
            set { SetProperty(ref _force_tcp, value); }
        }

        /// <summary>
        /// DNSCrypt: Create a new, unique key for every single DNS query
        /// This may improve privacy but can also have a significant impact on CPU usage
        /// Only enable if you don't have a lot of network load
        /// </summary>
        public bool dnscrypt_ephemeral_keys
        {
            get { return _dnscrypt_ephemeral_keys; }
            set { SetProperty(ref _dnscrypt_ephemeral_keys, value); }
        }

        /// <summary>
        /// DoH: Disable TLS session tickets - increases privacy but also latency.
        /// </summary>
        public bool tls_disable_session_tickets
        {
            get { return _tls_disable_session_tickets; }
            set { SetProperty(ref _tls_disable_session_tickets, value); }
        }

        /// <summary>
        /// Offline mode - Do not use any remote encrypted servers.
        /// The proxy will remain fully functional to respond to queries that
        /// plugins can handle directly (forwarding, cloaking, ...)
        /// </summary>
        public bool offline_mode
        {
            get { return _offline_mode; }
            set { SetProperty(ref _offline_mode, value); }
        }

        /// <summary>
        /// SOCKS proxy
        /// Uncomment the following line to route all TCP connections to a local Tor node
        /// Tor doesn't support UDP, so set `force_tcp` to `true` as well.
        /// </summary>
        public string proxy
        {
            get { return _proxy; }
            set { SetProperty(ref _proxy, value); }
        }

        /// <summary>
        /// HTTP/HTTPS proxy
        /// Only for DoH servers
        /// </summary>
        public string http_proxy
        {
            get { return _http_proxy; }
            set { SetProperty(ref _http_proxy, value); }
        }

        /// <summary>
        ///     How long a DNS query will wait for a response, in milliseconds.
        /// </summary>
        public int timeout
        {
            get { return _timeout; }
            set { SetProperty(ref _timeout, value); }
        }

        /// <summary>
        ///     Keepalive for HTTP (HTTPS, HTTP/2) queries, in seconds.
        /// </summary>
        public int keepalive
        {
            get { return _keepalive; }
            set { SetProperty(ref _keepalive, value); }
        }

        /// <summary>
        /// Load-balancing strategy: 'p2' (default), 'ph', 'first' or 'random'
        /// </summary>
        public string lb_strategy
        {
            get { return _lb_strategy; }
            set { SetProperty(ref _lb_strategy, value); }
        }

        /// <summary>
        /// Set to `true` to constantly try to estimate the latency of all the resolvers
        /// and adjust the load-balancing parameters accordingly, or to `false` to disable.
        /// </summary>
        public bool lb_estimator
        {
            get { return _lb_estimator; }
            set { SetProperty(ref _lb_estimator, value); }
        }

        /// <summary>
        /// Maximum time (in seconds) to wait for network connectivity before initializing the proxy.
        /// Useful if the proxy is automatically started at boot, and network
        /// connectivity is not guaranteed to be immediately available.
        /// Use 0 to disable.
        /// </summary>
        public int netprobe_timeout
        {
            get { return _netprobe_timeout; }
            set { SetProperty(ref _netprobe_timeout, value); }
        }

        /// <summary>
        /// Address and port to try initializing a connection to, just to check
        /// if the network is up. It can be any address and any port, even if
        /// there is nothing answering these on the other side. Just don't use
        /// a local address, as the goal is to check for Internet connectivity.
        /// On Windows, a datagram with a single, nul byte will be sent, only
        /// when the system starts.
        /// On other operating systems, the connection will be initialized
        /// but nothing will be sent at all.
        /// </summary>
        public string netprobe_address
        {
            get { return _netprobe_address; }
            set { SetProperty(ref _netprobe_address, value); }
        }

        /// <summary>
        ///     Log level (0-6, default: 2 - 0 is very verbose, 6 only contains fatal errors).
        /// </summary>
        public int log_level
        {
            get { return _log_level; }
            set { SetProperty(ref _log_level, value); }
        }

        /// <summary>
        ///     log file for the application.
        /// </summary>
        public string log_file
        {
            get { return _log_file; }
            set { SetProperty(ref _log_file, value); }
        }

        /// <summary>
        ///     Use the system logger (Windows Event Log)
        /// </summary>
        public bool use_syslog
        {
            get => _use_syslog;
            set => SetProperty(ref _use_syslog, value);
        }

        /// <summary>
        ///     Delay, in minutes, after which certificates are reloaded.
        /// </summary>
        public int cert_refresh_delay
        {
            get => _cert_refresh_delay;
            set => SetProperty(ref _cert_refresh_delay, value);
        }

        /// <summary>
        ///     Fallback resolvers
        /// 	These are normal, non-encrypted DNS resolvers, that will be only used
        /// 	for one-shot queries when retrieving the initial resolvers list, and
        /// 	only if the system DNS configuration doesn't work.
        /// 	No user application queries will ever be leaked through these resolvers,
        /// 	and they will not be used after IP addresses of resolvers URLs have been found.
        /// 	They will never be used if lists have already been cached, and if stamps
        /// 	don't include host names without IP addresses.
        /// 	They will not be used if the configured system DNS works.
        /// 	Resolvers supporting DNSSEC are recommended.
        /// 	
        /// 	People in China may need to use 114.114.114.114:53 here.
        /// 	Other popular options include 8.8.8.8 and 1.1.1.1.
        /// 	
        /// 	If more than one resolver is specified, they will be tried in sequence.
        /// </summary>
        public ObservableCollection<string> fallback_resolvers
        {
            get => _fallback_resolvers;
            set => SetProperty(ref _fallback_resolvers, value);
        }

        /// <summary>
        ///     Never try to use the system DNS settings;
        ///     unconditionally use the fallback resolver.
        /// </summary>
        public bool ignore_system_dns
        {
            get => _ignore_system_dns;
            set => SetProperty(ref _ignore_system_dns, value);
        }

        /// <summary>
        ///  Maximum log files size in MB.
        /// </summary>
        public int log_files_max_size
        {
            get => _log_files_max_size;
            set => SetProperty(ref _log_files_max_size, value);
        }

        /// <summary>
        /// Maximum log files age in days.
        /// </summary>
        public int log_files_max_age
        {
            get => _log_files_max_age;
            set => SetProperty(ref _log_files_max_age, value);
        }

        /// <summary>
        /// Maximum log files backups to keep.
        /// </summary>
        public int log_files_max_backups
        {
            get { return _log_files_max_backups; }
            set { SetProperty(ref _log_files_max_backups, value); }
        }

        /// <summary>
        ///     Immediately respond to IPv6-related queries with an empty response
        ///     This makes things faster when there is no IPv6 connectivity, but can
        ///     also cause reliability issues with some stub resolvers.
        /// </summary>
        public bool block_ipv6
        {
            get { return _block_ipv6; }
            set { SetProperty(ref _block_ipv6, value); }
        }

        /// <summary>
        ///		TTL for synthetic responses sent when a request has been blocked (due to IPv6 or blacklists).
        /// </summary>
        public int reject_ttl
        {
            get { return _reject_ttl; }
            set { SetProperty(ref _reject_ttl, value); }
        }

        /// <summary>
        ///     Forwarding rule file.
        /// </summary>
        public string forwarding_rules
        {
            get { return _forwarding_rules; }
            set { SetProperty(ref _forwarding_rules, value); }
        }

        /// <summary>
        ///     Cloaking rule file.
        /// </summary>
        public string cloaking_rules
        {
            get { return _cloaking_rules; }
            set { SetProperty(ref _cloaking_rules, value); }
        }

        /// <summary>
        ///     Enable a DNS cache to reduce latency and outgoing traffic.
        /// </summary>
        public bool cache
        {
            get { return _cache; }
            set { SetProperty(ref _cache, value); }
        }

        /// <summary>
        ///     Cache size.
        /// </summary>
        public int cache_size
        {
            get { return _cache_size; }
            set { SetProperty(ref _cache_size, value); }
        }

        /// <summary>
        ///     Minimum TTL for cached entries.
        /// </summary>
        public int cache_min_ttl
        {
            get { return _cache_min_ttl; }
            set { SetProperty(ref _cache_min_ttl, value); }
        }

        /// <summary>
        ///     Maxmimum TTL for cached entries.
        /// </summary>
        public int cache_max_ttl
        {
            get { return _cache_max_ttl; }
            set { SetProperty(ref _cache_max_ttl, value); }
        }

        /// <summary>
        ///     Minimum TTL for negatively cached entries.
        /// </summary>
        public int cache_neg_min_ttl
        {
            get { return _cache_neg_min_ttl; }
            set { SetProperty(ref _cache_neg_min_ttl, value); }
        }

        /// <summary>
        ///     Maximum TTL for negatively cached entries
        /// </summary>
        public int cache_neg_max_ttl
        {
            get { return _cache_neg_max_ttl; }
            set { SetProperty(ref _cache_neg_max_ttl, value); }
        }

        /// <summary>
        ///     Log client queries to a file.
        /// </summary>
        public QueryLog query_log { get; set; }

        /// <summary>
        ///     Log queries for nonexistent zones.
        /// </summary>
        public NxLog nx_log { get; set; }

        /// <summary>
        ///     Pattern-based blocking (blacklists).
        /// </summary>
        public Blacklist blacklist { get; set; }

        /// <summary>
        ///     Pattern-based IP blocking (IP blacklists).
        /// </summary>
        public Blacklist ip_blacklist { get; set; }


        /// <summary>
        /// </summary>
        public Dictionary<string, Source> sources
        {
            get { return _sources; }
            set { SetProperty(ref _sources, value); }
        }

        /// <summary>
        ///     Remote lists of available servers.
        /// </summary>
        public Dictionary<string, Static> Static
        {
            get { return _static; }
            set { SetProperty(ref _static, value); }
        }
    }

    /// <summary>
    /// </summary>
    public class QueryLog : BindableBase
    {
        private string _format;
        private string _file;
        private ObservableCollection<string> _ignored_qtypes;

        /// <summary>
        ///     Query log format (SimpleDnsCrypt: ltsv).
        /// </summary>
        public string format
        {
            get { return _format; }
            set { SetProperty(ref _format, value); }
        }

        /// <summary>
        ///     Path to the query log file (absolute, or relative to the same directory as the executable file).
        /// </summary>
        public string file
        {
            get { return _file; }
            set { SetProperty(ref _file, value); }
        }

        /// <summary>
        ///     Do not log these query types, to reduce verbosity. Keep empty to log everything.
        /// </summary>
        public ObservableCollection<string> ignored_qtypes
        {
            get { return _ignored_qtypes; }
            set { SetProperty(ref _ignored_qtypes, value); }
        }
    }

    /// <summary>
    ///     Log queries for nonexistent zones.
    /// </summary>
    public class NxLog : BindableBase
    {
        private string _format;
        private string _file;

        /// <summary>
        ///     Query log format (SimpleDnsCrypt: ltsv).
        /// </summary>
        public string format
        {
            get { return _format; }
            set { SetProperty(ref _format, value); }
        }

        /// <summary>
        ///     Path to the query log file (absolute, or relative to the same directory as the executable file).
        /// </summary>
        public string file
        {
            get { return _file; }
            set { SetProperty(ref _file, value); }
        }
    }

    /// <summary>
    ///     Blacklists.
    /// </summary>
    public class Blacklist : BindableBase
    {
        private string _log_format;
        private string _blacklist_file;
        private string _log_file;

        /// <summary>
        ///     Path to the file of blocking rules.
        /// </summary>
        public string blacklist_file
        {
            get { return _blacklist_file; }
            set { SetProperty(ref _blacklist_file, value); }
        }

        /// <summary>
        ///     Path to a file logging blocked queries.
        /// </summary>
        public string log_file
        {
            get { return _log_file; }
            set { SetProperty(ref _log_file, value); }
        }

        /// <summary>
        ///     Log format (SimpleDnsCrypt: ltsv).
        /// </summary>
        public string log_format
        {
            get { return _log_format; }
            set { SetProperty(ref _log_format, value); }
        }
    }

    public class Source : BindableBase
    {
        private ObservableCollection<Stamp> _stamps;
        public string[] urls { get; set; }
        public string minisign_key { get; set; }
        public string cache_file { get; set; }
        public string format { get; set; }
        public int refresh_delay { get; set; }
        public string prefix { get; set; }

        [TomlIgnore]
        public ObservableCollection<Stamp> Stamps
        {
            get { return _stamps; }
            set { SetProperty(ref _stamps, value); }
        }
    }

    public class Static
    {
        public string stamp { get; set; }
    }
}
