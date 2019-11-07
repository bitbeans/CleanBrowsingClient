using CleanBrowsingClient.Config;
using CleanBrowsingClient.Events;
using CleanBrowsingClient.Services;
using CleanBrowsingClient.Views;
using MaterialDesignThemes.Wpf;
using Prism.Events;
using Prism.Ioc;
using Prism.Regions;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using System.Windows;

namespace CleanBrowsingClient
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        public LoggingLevelSwitch LoggingLevelSwitch;
        public App()
        {
            SplashScreen splashScreen = new SplashScreen("Images/splash_small.png");
            splashScreen.Show(true);
            LoggingLevelSwitch = new LoggingLevelSwitch
            {
                MinimumLevel = Global.DefaultLogEventLevel
            };
        }

        protected override void OnInitialized()
        {
            var eventAggregator = Container.Resolve<IEventAggregator>();
            eventAggregator.GetEvent<LogLevelChangedEvent>().Subscribe(LogLevelChanged);

            var regionManager = Container.Resolve<IRegionManager>();
            regionManager.RequestNavigate("ContentRegion", "MainView");
            base.OnInitialized();
        }

        private void LogLevelChanged(LogEventLevel logEventLevel)
        {
            LoggingLevelSwitch.MinimumLevel = logEventLevel;
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            Log.Logger = new LoggerConfiguration()
            .MinimumLevel.ControlledBy(LoggingLevelSwitch)
            .WriteTo.File("logs\\client_.log",
            rollingInterval: RollingInterval.Day,
            retainedFileCountLimit: 14).CreateLogger();
            base.OnStartup(e);
        }

        protected override void OnExit(ExitEventArgs e)
        {
            Log.CloseAndFlush();
            base.OnExit(e);
        }

        protected override Window CreateShell()
        {
            return Container.Resolve<MainWindow>();
        }

        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterSerilog();
            containerRegistry.RegisterSingleton<ISnackbarMessageQueue, SnackbarMessageQueue>();
            containerRegistry.RegisterSingleton<IAppConfigurationService, AppConfigurationService>();
            containerRegistry.RegisterForNavigation<MainView>();
            containerRegistry.RegisterForNavigation<StampView>();
            containerRegistry.RegisterForNavigation<SettingsView>();
            containerRegistry.RegisterForNavigation<AboutView>();
        }
    }
}
