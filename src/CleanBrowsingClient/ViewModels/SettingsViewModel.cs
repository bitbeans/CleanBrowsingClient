using CleanBrowsingClient.Config;
using CleanBrowsingClient.Events;
using CleanBrowsingClient.Models;
using CleanBrowsingClient.Services;
using MaterialDesignThemes.Wpf;
using Prism.Commands;
using Prism.Events;
using Prism.Logging;
using Prism.Mvvm;
using Prism.Regions;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace CleanBrowsingClient.ViewModels
{
    public class SettingsViewModel : BindableBase
    {
        private readonly ILoggerFacade _logger;
        private ISnackbarMessageQueue _messageQueue;
        private readonly IRegionManager _regionManager;
        private readonly IEventAggregator _eventAggregator;
        private readonly IAppConfigurationService _appConfigurationService;
        private readonly AppConfiguration _appConfiguration;

        public ISnackbarMessageQueue MessageQueue
        {
            get { return _messageQueue; }
            set { SetProperty(ref _messageQueue, value); }
        }

        private ObservableCollection<LogLevel> _logLevels;
        private LogLevel _currentLogLevel;

        public ObservableCollection<LogLevel> LogLevels
        {
            get { return _logLevels; }
            set { SetProperty(ref _logLevels, value); }
        }

        public LogLevel CurrentLogLevel
        {
            get { return _currentLogLevel; }
            set
            {
                if (value != null)
                {
                    if (value != _currentLogLevel)
                    {
                        _eventAggregator.GetEvent<LogLevelChangedEvent>().Publish((Serilog.Events.LogEventLevel)value.Level);
                        _appConfiguration.LogLevel = value.Level;
                        _appConfigurationService.Configuration = _appConfiguration;
                        SetProperty(ref _currentLogLevel, value);
                    }
                }
            }
        }

        public DelegateCommand NavigateToMainView { get; private set; }
        public DelegateCommand ResetToDefaultSettingsCommand { get; private set; }
        public DelegateCommand OpenLogDirectoryCommand { get; private set; }

        public SettingsViewModel(
            ILoggerFacade logger,
            IRegionManager regionManager,
            IEventAggregator eventAggregator,
            ISnackbarMessageQueue snackbarMessageQueue,
            IAppConfigurationService appConfigurationService)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _regionManager = regionManager ?? throw new ArgumentNullException(nameof(regionManager));
            _eventAggregator = eventAggregator ?? throw new ArgumentNullException(nameof(eventAggregator));
            _messageQueue = snackbarMessageQueue ?? throw new ArgumentNullException(nameof(snackbarMessageQueue));
            _appConfigurationService = appConfigurationService ?? throw new ArgumentNullException(nameof(appConfigurationService));
            NavigateToMainView = new DelegateCommand(NavigateToMain);
            ResetToDefaultSettingsCommand = new DelegateCommand(ResetToDefaultSettings);
            OpenLogDirectoryCommand = new DelegateCommand(OpenLogDirectory);
            LogLevels = new ObservableCollection<LogLevel>
            {
                new LogLevel
                {
                    Level = 0,
                    Name = "Verbose"
                },
                new LogLevel
                {
                    Level = 1,
                    Name = "Debug"
                },
                new LogLevel
                {
                    Level = 2,
                    Name = "Information"
                },
                new LogLevel
                {
                    Level = 3,
                    Name = "Warning"
                },
                new LogLevel
                {
                    Level = 4,
                    Name = "Error"
                },
                new LogLevel
                {
                    Level = 5,
                    Name = "Fatal"
                },
            };
            _appConfiguration = _appConfigurationService.Configuration;
            if (_appConfiguration != null)
            {
                if (_appConfiguration.LogLevel > _logLevels.Count - 1)
                {
                    _currentLogLevel = _logLevels[(int)Global.DefaultLogEventLevel];
                }
                else
                {
                    _currentLogLevel = _logLevels[_appConfiguration.LogLevel];
                }
            }
        }

        private void NavigateToMain()
        {
            _regionManager.RequestNavigate("ContentRegion", "MainView");
        }

        private void ResetToDefaultSettings()
        {
            if (RestoreConfiguration())
            {
                _logger.Log("default configuration restored successfully", Category.Info, Priority.Low);
                MessageQueue.Enqueue("default configuration restored successfully");
            }
            else
            {
                _logger.Log("failed to restore default configuration", Category.Warn, Priority.Medium);
                MessageQueue.Enqueue("failed to restore default configuration");
            }
        }

        private void OpenLogDirectory()
        {
            try
            {
                var logDirectory = Path.Combine(Directory.GetCurrentDirectory(), "logs");
                if (!Directory.Exists(logDirectory))
                {
                    Directory.CreateDirectory(logDirectory);
                }
                Process.Start("explorer.exe", logDirectory);
            }
            catch (Exception exception)
            {
                _logger.Log(exception.Message, Category.Exception, Priority.High);
            }
        }

        private bool RestoreConfiguration()
        {
            try
            {
                var file = Assembly.GetExecutingAssembly().GetManifestResourceStream("CleanBrowsingClient.dnscrypt_proxy.dnscrypt-proxy.toml");
                if (file != null)
                {
                    var configFile = Path.Combine(Directory.GetCurrentDirectory(), Global.DnsCryptProxyFolder, Global.DnsCryptConfigurationFile);
                    using (FileStream fileStream = File.Create(configFile))
                    {
                        file.CopyTo(fileStream);
                    }
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception exception)
            {
                _logger.Log(exception.Message, Category.Exception, Priority.High);
                return false;
            }
        }
    }
}
