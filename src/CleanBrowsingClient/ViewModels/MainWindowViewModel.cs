using CleanBrowsingClient.Config;
using CleanBrowsingClient.Events;
using CleanBrowsingClient.Helper;
using CleanBrowsingClient.Models;
using CleanBrowsingClient.Services;
using DnsCrypt.Stamps;
using MaterialDesignThemes.Wpf;
using Prism.Commands;
using Prism.Events;
using Prism.Logging;
using Prism.Mvvm;
using Prism.Regions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace CleanBrowsingClient.ViewModels
{
    public class MainWindowViewModel : BindableBase
    {
        private readonly ILoggerFacade _logger;
        private ISnackbarMessageQueue _messageQueue;
        private readonly IEventAggregator _eventAggregator;
        private readonly IRegionManager _regionManager;
        private DnscryptProxyConfiguration _dnscryptProxyConfiguration;
        private readonly AppConfiguration _appConfiguration;
        private readonly IAppConfigurationService _appConfigurationService;

        private string _title = "";
        private string _footer = "";
        private bool _isWorking = false;
        private bool _isUpdating = false;
        private int _updateProgressValue = 0;
        private string _updateProgressStatus = "";
        private bool _isCustomFilterEnabled = false;
        private bool _isFamilyFilterEnabled = false;
        private bool _isAdultFilterEnabled = false;

        public ISnackbarMessageQueue MessageQueue
        {
            get { return _messageQueue; }
            set { SetProperty(ref _messageQueue, value); }
        }

        public string Title
        {
            get { return _title; }
            set { SetProperty(ref _title, value); }
        }

        public string Footer
        {
            get { return _footer; }
            set { SetProperty(ref _footer, value); }
        }

        public bool IsWorking
        {
            get { return _isWorking; }
            set { SetProperty(ref _isWorking, value); }
        }

        public bool IsUpdating
        {
            get { return _isUpdating; }
            set { SetProperty(ref _isUpdating, value); }
        }

        public int UpdateProgressValue
        {
            get { return _updateProgressValue; }
            set { SetProperty(ref _updateProgressValue, value); }
        }

        public string UpdateProgressStatus
        {
            get { return _updateProgressStatus; }
            set { SetProperty(ref _updateProgressStatus, value); }
        }

        public bool IsProtected => _isCustomFilterEnabled || _isFamilyFilterEnabled || _isAdultFilterEnabled;

        public string IsProtectedText
        {
            get
            {
                if (_isCustomFilterEnabled)
                {
                    return "Status: ACTIVE. You are protected by Custom Filter.";
                }
                if (_isFamilyFilterEnabled)
                {
                    return "Status: ACTIVE. You are protected by Free Family Filter.";
                }
                if (_isAdultFilterEnabled)
                {
                    return "Status: ACTIVE. You are protected by Free Adult Filter.";
                }
                return "Status: ACTIVE (unknown filter)";
            }
        }

        public bool IsCustomFilterEnabled
        {
            get { return _isCustomFilterEnabled; }
            set
            {
                SetProperty(ref _isCustomFilterEnabled, value);
                RaisePropertyChanged("IsProtected");
            }
        }

        public bool IsFamilyFilterEnabled
        {
            get { return _isFamilyFilterEnabled; }
            set
            {
                SetProperty(ref _isFamilyFilterEnabled, value);
                RaisePropertyChanged("IsProtected");
            }
        }

        public bool IsAdultFilterEnabled
        {
            get { return _isAdultFilterEnabled; }
            set
            {
                SetProperty(ref _isAdultFilterEnabled, value);
                RaisePropertyChanged("IsProtected");
            }
        }

        public DelegateCommand NavigateToAboutViewCommand { get; private set; }
        public DelegateCommand NavigateToSettingsViewCommand { get; private set; }
        public DelegateCommand NavigateToStampViewCommand { get; private set; }
        public DelegateCommand CheckForUpdatesCommand { get; private set; }
        public DelegateCommand<string> OpenWebCommand { get; private set; }
        public DelegateCommand HandleCustomFilter { get; private set; }
        public DelegateCommand HandleFamilyFilter { get; private set; }
        public DelegateCommand HandleAdultFilter { get; private set; }

        public MainWindowViewModel(
            ILoggerFacade logger,
            IRegionManager regionManager,
            IEventAggregator eventAggregator,
            ISnackbarMessageQueue snackbarMessageQueue,
            IAppConfigurationService appConfigurationService)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _eventAggregator = eventAggregator ?? throw new ArgumentNullException(nameof(eventAggregator));
            _regionManager = regionManager ?? throw new ArgumentNullException(nameof(regionManager));
            _messageQueue = snackbarMessageQueue ?? throw new ArgumentNullException(nameof(snackbarMessageQueue));
            _appConfigurationService = appConfigurationService ?? throw new ArgumentNullException(nameof(appConfigurationService));
            try
            {
                _appConfiguration = _appConfigurationService.Configuration;
                _eventAggregator.GetEvent<StampAddedEvent>().Subscribe(CustomStampAdded);
                NavigateToAboutViewCommand = new DelegateCommand(NavigateToAboutView);
                NavigateToStampViewCommand = new DelegateCommand(NavigateToStampView);
                NavigateToSettingsViewCommand = new DelegateCommand(NavigateToSettingsView);
                OpenWebCommand = new DelegateCommand<string>(OpenUrl);
                CheckForUpdatesCommand = new DelegateCommand(async () =>
                {
                    await CheckForUpdatesAsync();
                });

                HandleCustomFilter = new DelegateCommand(HandleCustomStamp);
                HandleFamilyFilter = new DelegateCommand(async () => await HandleDnsCrypt(Global.DefaultFamilyFilterKey));
                HandleAdultFilter = new DelegateCommand(async () => await HandleDnsCrypt(Global.DefaultAdultFilterKey));

                IsWorking = true;
                IsCustomFilterEnabled = false;
                IsFamilyFilterEnabled = false;
                IsAdultFilterEnabled = false;

                var year = Global.CopyrightYear.ToString();
                if (DateTime.UtcNow.Year != Global.CopyrightYear)
                {
                    year = $"{Global.CopyrightYear} - {DateTime.UtcNow.Year}";
                }
                Footer = $"Copyright © {year} {Global.CompanyName}. {VersionHelper.PublishVersion} {VersionHelper.PublishBuild}";
                ((App)Application.Current).LoggingLevelSwitch.MinimumLevel = (Serilog.Events.LogEventLevel)_appConfiguration.LogLevel;
                _logger.Log($"LogLevel: {_appConfiguration.LogLevel}", Category.Debug, Priority.Low);
                _logger.Log($"{Global.ApplicationName} {VersionHelper.PublishVersion} {VersionHelper.PublishBuild} started", Category.Info, Priority.Medium);
                // check application configuration
                _logger.Log($"checking {Global.AppConfigurationFile}", Category.Info, Priority.Medium);

                var resetApplicationConfig = false;
                if (_appConfiguration != null)
                {
                    if (_appConfiguration.Proxies != null)
                    {
                        foreach (var proxy in _appConfiguration.Proxies)
                        {
                            var decodedStamp = StampTools.Decode(proxy.Stamp);
                            if (decodedStamp != null)
                            {
                                if (decodedStamp.Protocol == DnsCrypt.Models.StampProtocol.DnsCrypt)
                                {
                                    //simple check if the stamp is a valid cleanbrowsing stamp
                                    if (!decodedStamp.ProviderName.Equals(Global.ValidCleanBrowsingDnsCryptStamp))
                                    {
                                        resetApplicationConfig = true;
                                    }
                                }
                                else if (decodedStamp.Protocol == DnsCrypt.Models.StampProtocol.DoH)
                                {
                                    //simple check if the stamp is a valid cleanbrowsing stamp
                                    if (!decodedStamp.Hostname.Equals(Global.ValidCleanBrowsingDohStamp))
                                    {
                                        resetApplicationConfig = true;
                                    }
                                }
                                else
                                {
                                    //unsupported stamp
                                    resetApplicationConfig = true;
                                }
                            }
                            else
                            {
                                resetApplicationConfig = true;
                            }
                            _logger.Log($"{proxy.Name} loaded", Category.Info, Priority.Medium);
                        }
                        _logger.Log($"{Global.AppConfigurationFile} loaded", Category.Info, Priority.Medium);
                    }
                    else
                    {
                        resetApplicationConfig = true;
                    }
                }
                else
                {
                    resetApplicationConfig = true;
                    _logger.Log($"failed to load {Global.AppConfigurationFile}", Category.Warn, Priority.Medium);
                }

                if (resetApplicationConfig)
                {
                    _logger.Log($"reset {Global.AppConfigurationFile} to default", Category.Warn, Priority.Medium);
                    _appConfigurationService.Reset();
                    _appConfiguration = _appConfigurationService.Configuration;
                    if (_appConfiguration == null)
                    {
                        _logger.Log($"failed to reset {Global.AppConfigurationFile}", Category.Exception, Priority.High);
                        Environment.Exit(-1);
                    }
                    else
                    {
                        //no validation this time, just go on
                    }
                }

                _logger.Log($"checking {Global.DnsCryptProxyFolder} folder", Category.Info, Priority.Medium);
                foreach (var proxyFile in Global.DnsCryptProxyFiles)
                {
                    var proxyFilePath = Path.Combine(Directory.GetCurrentDirectory(), Global.DnsCryptProxyFolder, proxyFile);
                    if (!File.Exists(proxyFilePath))
                    {
                        _logger.Log($"missing {proxyFile}", Category.Warn, Priority.Medium);
                    }
                    else
                    {
                        _logger.Log($"found {proxyFile}", Category.Info, Priority.Low);
                    }
                }

                var isValidConfiguration = false;
                _logger.Log($"checking {Global.DnsCryptConfigurationFile}", Category.Info, Priority.Medium);

                var configurationCheck = DnsCryptProxyManager.IsConfigurationFileValid();
                if (configurationCheck.Success)
                {
                    isValidConfiguration = true;
                    _logger.Log($"{Global.DnsCryptConfigurationFile} is valid", Category.Info, Priority.Medium);
                }
                else
                {
                    if (configurationCheck.StandardError.Contains("[FATAL] No servers configured"))
                    {
                        isValidConfiguration = true;
                        _logger.Log($"{Global.DnsCryptConfigurationFile} is valid (but no servers)", Category.Info, Priority.Medium);
                    }
                }

                if (isValidConfiguration)
                {
                    var version = DnsCryptProxyManager.GetVersion();
                    if (!string.IsNullOrEmpty(version))
                    {
                        Title = $"{Global.ApplicationName} (dnscrypt-proxy {version})";
                        _logger.Log($"dnscrypt-proxy version: {version}", Category.Info, Priority.Medium);
                    }
                    else
                    {
                        Title = $"{Global.ApplicationName} (dnscrypt-proxy unknown)";
                        _logger.Log("dnscrypt-proxy version: unknown", Category.Warn, Priority.Medium);
                    }
                    _logger.Log($"loading {Global.DnsCryptConfigurationFile}", Category.Info, Priority.Medium);
                    if (DnscryptProxyConfigurationManager.LoadConfiguration())
                    {
                        _logger.Log($"{Global.DnsCryptConfigurationFile} loaded", Category.Info, Priority.Medium);
                        _dnscryptProxyConfiguration = DnscryptProxyConfigurationManager.DnscryptProxyConfiguration;
                        if (_dnscryptProxyConfiguration.Static != null && _dnscryptProxyConfiguration.Static.Count > 0)
                        {
                            if (_dnscryptProxyConfiguration.Static.ContainsKey(Global.DefaultCustomFilterKey))
                            {
                                _logger.Log($"found {Global.DefaultCustomFilterKey} filter", Category.Info, Priority.Medium);
                                IsCustomFilterEnabled = true;
                            }
                            else if (_dnscryptProxyConfiguration.Static.ContainsKey(Global.DefaultAdultFilterKey))
                            {
                                _logger.Log($"found {Global.DefaultAdultFilterKey} filter", Category.Info, Priority.Medium);
                                IsAdultFilterEnabled = true;
                            }
                            else if (_dnscryptProxyConfiguration.Static.ContainsKey(Global.DefaultFamilyFilterKey))
                            {
                                _logger.Log($"found {Global.DefaultFamilyFilterKey} filter", Category.Info, Priority.Medium);
                                IsFamilyFilterEnabled = true;
                            }
                        }
                        else
                        {
                            _logger.Log("no static filter configured", Category.Info, Priority.Medium);
                        }

                        if (IsCustomFilterEnabled || IsFamilyFilterEnabled || IsAdultFilterEnabled)
                        {
                            if (!DnsCryptProxyManager.IsDnsCryptProxyInstalled())
                            {
                                _logger.Log($"dnscrypt-proxy service is not installed, try install", Category.Info, Priority.High);
                                //install
                                Task.Run(() => { DnsCryptProxyManager.Install(); }).ConfigureAwait(false);
                                Task.Delay(Global.ServiceInstallTime).ConfigureAwait(false);
                            }
                            else
                            {
                                _logger.Log($"dnscrypt-proxy service is already installed", Category.Info, Priority.Medium);
                            }

                            if (!DnsCryptProxyManager.IsDnsCryptProxyRunning())
                            {
                                _logger.Log($"dnscrypt-proxy service is not running, try start", Category.Info, Priority.High);
                                Task.Run(() => { DnsCryptProxyManager.Start(); }).ConfigureAwait(false);
                                Task.Delay(Global.ServiceStartTime).ConfigureAwait(false);
                            }
                            else
                            {
                                _logger.Log($"dnscrypt-proxy service is already running", Category.Info, Priority.Medium);
                            }

                            if (DnsCryptProxyManager.IsDnsCryptProxyRunning())
                            {
                                _logger.Log($"checking dns servers on network interfaces", Category.Info, Priority.High);
                                if (!HandleNetworkInterfaces(true))
                                {
                                    _logger.Log($"could not update dns servers on network interfaces", Category.Warn, Priority.High);
                                }
                            }
                            else
                            {
                                _logger.Log($"could not start dnscrypt-proxy", Category.Warn, Priority.High);
                            }
                        }
                    }
                    else
                    {
                        _logger.Log($"could not load configuration: {Global.DnsCryptConfigurationFile}", Category.Warn, Priority.High);
                        Environment.Exit(-1);
                    }
                }
                else
                {
                    _logger.Log($"invalid {Global.DnsCryptConfigurationFile}", Category.Warn, Priority.High);
                    Environment.Exit(-1);
                }
            }
            catch (Exception ex)
            {
                _logger.Log(ex.Message, Category.Exception, Priority.High);
                Environment.Exit(-1);
            }
            //MessageQueue.Enqueue("🐞 You are using a preview version! May contain bugs! 🐞");
            IsWorking = false;
        }

        private bool HandleNetworkInterfaces(bool enable = true)
        {
            var succedded = true;
            try
            {
                var listenAddresses = new List<string>(_dnscryptProxyConfiguration.listen_addresses);
                if (listenAddresses.Count > 0)
                {
                    var localNetworkInterfaces = LocalNetworkInterfaceManager.GetLocalNetworkInterfaces(listenAddresses);
                    if (localNetworkInterfaces.Count > 0)
                    {
                        foreach (var localNetworkInterface in localNetworkInterfaces)
                        {
                            if (enable)
                            {
                                if (!localNetworkInterface.UseDnsCrypt)
                                {
                                    if (LocalNetworkInterfaceManager.SetNameservers(localNetworkInterface, LocalNetworkInterfaceManager.ConvertToDnsList(listenAddresses)))
                                    {
                                        _logger.Log($"set DNS for {localNetworkInterface.Name} succedded", Category.Info, Priority.Medium);
                                    }
                                    else
                                    {
                                        _logger.Log($"failed to unset DNS for {localNetworkInterface.Name}", Category.Warn, Priority.High);
                                        succedded = false;
                                    }
                                }
                            }
                            else
                            {
                                if (localNetworkInterface.UseDnsCrypt)
                                {
                                    if (LocalNetworkInterfaceManager.UnsetNameservers(localNetworkInterface))
                                    {
                                        _logger.Log($"unset DNS for {localNetworkInterface.Name} succedded", Category.Info, Priority.Medium);
                                    }
                                    else
                                    {
                                        _logger.Log($"failed to unset DNS for {localNetworkInterface.Name}", Category.Warn, Priority.High);
                                        succedded = false;
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        _logger.Log($"there are no network interfaces for configuration", Category.Warn, Priority.High);
                        succedded = false;
                    }
                }
                else
                {
                    _logger.Log($"missing listen_addresses in dnscrypt-proxy configuration", Category.Warn, Priority.High);
                    succedded = false;
                }
            }
            catch (Exception exception)
            {
                _logger.Log($"HandleNetworkInterfaces {exception.Message}", Category.Exception, Priority.High);
                succedded = false;
            }
            return succedded;
        }

        public void NavigateToAboutView()
        {
            _regionManager.RequestNavigate("ContentRegion", "AboutView");
        }

        public void NavigateToStampView()
        {
            _regionManager.RequestNavigate("ContentRegion", "StampView");
        }

        public void NavigateToSettingsView()
        {
            _regionManager.RequestNavigate("ContentRegion", "SettingsView");
        }

        private void OpenUrl(string url)
        {
            CoreHelper.OpenBrowser(url);
        }

        public async Task CheckForUpdatesAsync()
        {
            var remoteUpdate = new RemoteUpdate();
            try
            {
                _logger.Log("update check: start", Category.Info, Priority.Low);
                var currentVersion = Assembly.GetExecutingAssembly().GetName().Version;
                remoteUpdate.CanUpdate = false;
                using var client = new HttpClient()
                {
                    DefaultRequestVersion = new Version(2, 0)
                };
                var response = await client.GetByteArrayAsync(Global.RemoteUpdateCheckUrl);

                if (response != null)
                {
                    using (var remoteUpdateDataStream = new MemoryStream(response))
                    {
                        using var remoteUpdateDataStreamReader = new StreamReader(remoteUpdateDataStream);
                        var deserializer = new DeserializerBuilder().WithNamingConvention(CamelCaseNamingConvention.Instance).Build();
                        remoteUpdate = deserializer.Deserialize<RemoteUpdate>(remoteUpdateDataStreamReader);
                    }
                    if (remoteUpdate != null)
                    {
                        var status = remoteUpdate.Update.Version.CompareTo(currentVersion);
                        // The local version is newer as the remote version
                        if (status < 0)
                        {
                            remoteUpdate.CanUpdate = false;
                            _logger.Log($"update check: You are already using the latest version: {currentVersion}", Category.Info, Priority.Low);
                            MessageQueue.Enqueue($"You are already using the latest version: {currentVersion}");
                        }
                        //The local version is the same as the remote version
                        else if (status == 0)
                        {
                            remoteUpdate.CanUpdate = false;
                            _logger.Log($"update check: The local version is the same as the remote version: {currentVersion}", Category.Info, Priority.Low);
                            MessageQueue.Enqueue($"You are already using the latest version: {currentVersion}");
                        }
                        else
                        {
                            // the remote version is newer as the local version
                            remoteUpdate.CanUpdate = true;
                            _logger.Log($"update check: the remote version is newer as the local version: {currentVersion} < {remoteUpdate.Update.Version}", Category.Info, Priority.Low);
                            MessageQueue.Enqueue($"A newer version exists: {remoteUpdate.Update.Version}", "Update Now", async () => await RunUpdateAsync(remoteUpdate));
                        }
                    }
                    else
                    {
                        _logger.Log($"failed to check for updates: invalid update file", Category.Warn, Priority.High);
                        MessageQueue.Enqueue("Failed to check for updates");
                    }
                }
                else
                {
                    _logger.Log($"failed to check for updates: no response from server", Category.Warn, Priority.High);
                    MessageQueue.Enqueue("Failed to check for updates");
                }
                _logger.Log("update check: end", Category.Info, Priority.Low);
            }
            catch (Exception exception)
            {
                MessageQueue.Enqueue("An error occurred when checking updates. Please check the logs.");
                _logger.Log($"CheckForUpdatesAsync {exception.Message}", Category.Exception, Priority.High);
            }
        }

        public async Task RunUpdateAsync(RemoteUpdate remoteUpdate)
        {
            try
            {
                _logger.Log("run update: start", Category.Info, Priority.Low);
                IsUpdating = true;
                UpdateProgressValue = 0;
                UpdateProgressStatus = "";
                var destinationFilePath = Path.Combine(Path.GetTempPath(), remoteUpdate.Update.Installer.Name); ;

                using var client = new HttpClientDownloadWithProgress(remoteUpdate.Update.Installer.Uri, destinationFilePath);
                client.ProgressChanged += (totalFileSize, totalBytesDownloaded, progressPercentage) =>
                {
                    UpdateProgressValue = (int)progressPercentage;
                    UpdateProgressStatus = $"Downloading update {progressPercentage}% ({totalBytesDownloaded}/{totalFileSize})";
                };
                _logger.Log($"start download: {remoteUpdate.Update.Installer.Uri}", Category.Info, Priority.Medium);
                await client.StartDownload();
                _logger.Log($"download completed: {remoteUpdate.Update.Installer.Uri}", Category.Info, Priority.Medium);

                if (!string.IsNullOrEmpty(destinationFilePath))
                {
                    if (File.Exists(destinationFilePath))
                    {
                        _logger.Log($"start installation of: {destinationFilePath}", Category.Info, Priority.Low);
                        UpdateProgressStatus = "Starting installation ...";
                        //automatic installation
                        const string arguments = "/qb /passive /norestart";
                        var startInfo = new ProcessStartInfo(destinationFilePath)
                        {
                            Arguments = arguments,
                            UseShellExecute = true
                        };
                        Process.Start(startInfo);
                        _logger.Log($"killing application", Category.Info, Priority.Low);
                        Process.GetCurrentProcess().Kill();
                    }
                    else
                    {
                        _logger.Log($"missing installation: {destinationFilePath}", Category.Warn, Priority.Medium);
                    }
                }
                IsUpdating = false;
                _logger.Log("run update: start", Category.Info, Priority.Low);
            }
            catch (Exception exception)
            {
                _logger.Log($"RunUpdateAsync {exception.Message}", Category.Exception, Priority.High);
            }
        }

        private async void CustomStampAdded(Proxy proxy)
        {
            try
            {
                if (proxy != null)
                {
                    if (!string.IsNullOrEmpty(proxy.Stamp))
                    {
                        var decodedStamp = StampTools.Decode(proxy.Stamp);
                        if (decodedStamp != null)
                        {
                            var addStamp = false;
                            if (decodedStamp.Protocol == DnsCrypt.Models.StampProtocol.DnsCrypt)
                            {
                                //simple check if the stamp is a valid cleanbrowsing stamp
                                if (decodedStamp.ProviderName.Equals(Global.ValidCleanBrowsingDnsCryptStamp))
                                {
                                    addStamp = true;
                                }
                            }
                            else if (decodedStamp.Protocol == DnsCrypt.Models.StampProtocol.DoH)
                            {
                                //simple check if the stamp is a valid cleanbrowsing stamp
                                if (decodedStamp.Hostname.Equals(Global.ValidCleanBrowsingDohStamp))
                                {
                                    addStamp = true;
                                }
                            }
                            else
                            {
                                //unsupported stamp
                                addStamp = false;
                            }
                            if (addStamp)
                            {
                                _appConfiguration.Proxies.Add(proxy);
                                _appConfigurationService.Configuration = _appConfiguration;
                                _logger.Log($"{proxy.Name} - {proxy.Stamp} added", Category.Info, Priority.Medium);
                                MessageQueue.Enqueue("New custom filter added");
                                await HandleDnsCrypt(Global.DefaultCustomFilterKey);
                            }
                            else
                            {
                                _logger.Log($"invalid custom stamp", Category.Warn, Priority.Medium);
                            }
                        }
                        else
                        {
                            _logger.Log($"invalid custom stamp", Category.Warn, Priority.Medium);
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                _logger.Log($"CustomStampAdded {exception.Message}", Category.Exception, Priority.High);
            }
        }

        public async void HandleCustomStamp()
        {
            try
            {
                var stamp = _appConfiguration.Proxies.FirstOrDefault(p => p.Name.Equals(Global.DefaultCustomFilterKey));
                if (stamp == null)
                {
                    _regionManager.RequestNavigate("ContentRegion", "StampView");
                }
                else
                {
                    await HandleDnsCrypt("cleanbrowsing-custom");
                }
            }
            catch (Exception exception)
            {
                _logger.Log($"HandleCustomStamp {exception.Message}", Category.Exception, Priority.High);
            }
        }

        public async Task HandleDnsCrypt(string filter)
        {
            try
            {
                IsWorking = true;
                if (filter.Equals(Global.DefaultCustomFilterKey))
                {
                    if (_isCustomFilterEnabled)
                    {
                        //disable
                        if (RemoveStaticStamps())
                        {
                            if (DnsCryptProxyManager.IsDnsCryptProxyRunning())
                            {
                                await Task.Run(() => { DnsCryptProxyManager.Stop(); }).ConfigureAwait(false);
                                await Task.Delay(Global.ServiceStopTime).ConfigureAwait(false);
                            }
                            if (DnsCryptProxyManager.IsDnsCryptProxyInstalled())
                            {
                                await Task.Run(() => { DnsCryptProxyManager.Uninstall(); }).ConfigureAwait(false);
                                await Task.Delay(Global.ServiceUninstallTime).ConfigureAwait(false);
                            }
                            HandleNetworkInterfaces(false);

                            //remove stamp from config
                            var stampIndex = _appConfiguration.Proxies.FindIndex(p => p.Name.Equals(filter));
                            if (stampIndex != -1)
                            {
                                _appConfiguration.Proxies.RemoveAt(stampIndex);
                                _appConfigurationService.Configuration = _appConfiguration;
                            }
                        }
                        IsCustomFilterEnabled = false;
                        IsAdultFilterEnabled = false;
                        IsFamilyFilterEnabled = false;
                    }
                    else
                    {
                        //enable
                        if (SetStaticStamp(filter))
                        {
                            if (!DnsCryptProxyManager.IsDnsCryptProxyInstalled())
                            {
                                await Task.Run(() => { DnsCryptProxyManager.Install(); }).ConfigureAwait(false);
                                await Task.Delay(Global.ServiceInstallTime).ConfigureAwait(false);
                            }
                            if (DnsCryptProxyManager.IsDnsCryptProxyRunning())
                            {
                                await Task.Run(() => { DnsCryptProxyManager.Restart(); }).ConfigureAwait(false);
                                await Task.Delay(Global.ServiceRestartTime).ConfigureAwait(false);
                            }
                            else
                            {
                                await Task.Run(() => { DnsCryptProxyManager.Start(); }).ConfigureAwait(false);
                                await Task.Delay(Global.ServiceStartTime).ConfigureAwait(false);
                            }
                            HandleNetworkInterfaces(true);
                            IsCustomFilterEnabled = true;
                            IsAdultFilterEnabled = false;
                            IsFamilyFilterEnabled = false;
                        }
                    }
                }
                else if (filter.Equals(Global.DefaultAdultFilterKey))
                {
                    if (_isAdultFilterEnabled)
                    {
                        //disable
                        if (RemoveStaticStamps())
                        {
                            if (DnsCryptProxyManager.IsDnsCryptProxyRunning())
                            {
                                await Task.Run(() => { DnsCryptProxyManager.Stop(); }).ConfigureAwait(false);
                                await Task.Delay(Global.ServiceStopTime).ConfigureAwait(false);
                            }
                            if (DnsCryptProxyManager.IsDnsCryptProxyInstalled())
                            {
                                await Task.Run(() => { DnsCryptProxyManager.Uninstall(); }).ConfigureAwait(false);
                                await Task.Delay(Global.ServiceUninstallTime).ConfigureAwait(false);
                            }
                            HandleNetworkInterfaces(false);
                        }
                        IsCustomFilterEnabled = false;
                        IsAdultFilterEnabled = false;
                        IsFamilyFilterEnabled = false;
                    }
                    else
                    {
                        //enable
                        if (SetStaticStamp(filter))
                        {
                            if (!DnsCryptProxyManager.IsDnsCryptProxyInstalled())
                            {
                                await Task.Run(() => { DnsCryptProxyManager.Install(); }).ConfigureAwait(false);
                                await Task.Delay(Global.ServiceInstallTime).ConfigureAwait(false);
                            }
                            if (DnsCryptProxyManager.IsDnsCryptProxyRunning())
                            {
                                await Task.Run(() => { DnsCryptProxyManager.Restart(); }).ConfigureAwait(false);
                                await Task.Delay(Global.ServiceRestartTime).ConfigureAwait(false);
                            }
                            else
                            {
                                await Task.Run(() => { DnsCryptProxyManager.Start(); }).ConfigureAwait(false);
                                await Task.Delay(Global.ServiceStartTime).ConfigureAwait(false);
                            }
                            HandleNetworkInterfaces(true);
                            IsAdultFilterEnabled = true;
                            IsCustomFilterEnabled = false;
                            IsFamilyFilterEnabled = false;
                        }
                    }
                }
                else if (filter.Equals(Global.DefaultFamilyFilterKey))
                {
                    if (_isFamilyFilterEnabled)
                    {
                        //disable
                        if (RemoveStaticStamps())
                        {
                            if (DnsCryptProxyManager.IsDnsCryptProxyRunning())
                            {
                                await Task.Run(() => { DnsCryptProxyManager.Stop(); }).ConfigureAwait(false);
                                await Task.Delay(Global.ServiceStopTime).ConfigureAwait(false);
                            }
                            if (DnsCryptProxyManager.IsDnsCryptProxyInstalled())
                            {
                                await Task.Run(() => { DnsCryptProxyManager.Uninstall(); }).ConfigureAwait(false);
                                await Task.Delay(Global.ServiceUninstallTime).ConfigureAwait(false);
                            }
                            HandleNetworkInterfaces(false);
                        }
                        IsCustomFilterEnabled = false;
                        IsFamilyFilterEnabled = false;
                        IsAdultFilterEnabled = false;
                    }
                    else
                    {
                        //enable
                        if (SetStaticStamp(filter))
                        {
                            if (!DnsCryptProxyManager.IsDnsCryptProxyInstalled())
                            {
                                await Task.Run(() => { DnsCryptProxyManager.Install(); }).ConfigureAwait(false);
                                await Task.Delay(Global.ServiceInstallTime).ConfigureAwait(false);
                            }
                            if (DnsCryptProxyManager.IsDnsCryptProxyRunning())
                            {
                                await Task.Run(() => { DnsCryptProxyManager.Restart(); }).ConfigureAwait(false);
                                await Task.Delay(Global.ServiceRestartTime).ConfigureAwait(false);
                            }
                            else
                            {
                                await Task.Run(() => { DnsCryptProxyManager.Start(); }).ConfigureAwait(false);
                                await Task.Delay(Global.ServiceStartTime).ConfigureAwait(false);
                            }
                            HandleNetworkInterfaces(true);
                            IsFamilyFilterEnabled = true;
                            IsCustomFilterEnabled = false;
                            IsAdultFilterEnabled = false;
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                _logger.Log($"HandleDnsCrypt {exception.Message}", Category.Exception, Priority.High);
            }
            finally
            {
                IsWorking = false;
            }
        }

        private bool SetStaticStamp(string filter)
        {
            try
            {
                if (_dnscryptProxyConfiguration.Static != null)
                {
                    if (_dnscryptProxyConfiguration.Static.ContainsKey(filter))
                    {
                        return true;
                    }
                    else
                    {
                        RemoveStaticStamps();
                        DnscryptProxyConfigurationManager.LoadConfiguration();
                        _dnscryptProxyConfiguration = DnscryptProxyConfigurationManager.DnscryptProxyConfiguration;
                        var stamp = _appConfiguration.Proxies.FirstOrDefault(p => p.Name.Equals(filter));
                        if (stamp != null)
                        {
                            if (!string.IsNullOrEmpty(stamp.Stamp))
                            {
                                var decodedStamp = StampTools.Decode(stamp.Stamp);
                                if (decodedStamp != null)
                                {
                                    _dnscryptProxyConfiguration.Static.Add(filter, new Static
                                    {
                                        stamp = stamp.Stamp
                                    });
                                }
                            }
                        }
                        DnscryptProxyConfigurationManager.DnscryptProxyConfiguration = _dnscryptProxyConfiguration;
                        DnscryptProxyConfigurationManager.SaveConfiguration();
                        return true;
                    }
                }
            }
            catch (Exception exception)
            {
                _logger.Log($"SetStaticStamp {exception.Message}", Category.Exception, Priority.High);
                return false;
            }
            return false;
        }

        private bool RemoveStaticStamps()
        {
            try
            {
                if (_dnscryptProxyConfiguration.Static != null)
                {
                    foreach (var key in _dnscryptProxyConfiguration.Static.Keys)
                    {
                        _dnscryptProxyConfiguration.Static.Remove(key);
                    }
                    DnscryptProxyConfigurationManager.DnscryptProxyConfiguration = _dnscryptProxyConfiguration;
                    DnscryptProxyConfigurationManager.SaveConfiguration();
                    return true;
                }
            }
            catch (Exception exception)
            {
                _logger.Log($"RemoveStaticStamps {exception.Message}", Category.Exception, Priority.High);
                return false;
            }
            return false;
        }
    }
}
