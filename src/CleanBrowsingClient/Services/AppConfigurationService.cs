using CleanBrowsingClient.Config;
using CleanBrowsingClient.Models;
using Nett;
using Prism.Logging;
using Serilog.Events;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace CleanBrowsingClient.Services
{
    public interface IAppConfigurationService
    {
        AppConfiguration Configuration { get; set; }
        void Reset();
    }

    public sealed class AppConfigurationService : IAppConfigurationService
    {
        private readonly ILoggerFacade _logger;
        private readonly ReaderWriterLockSlim _readerWriterLockSlim = new ReaderWriterLockSlim();

        public AppConfigurationService(ILoggerFacade logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            if (!File.Exists(Path.Combine(Directory.GetCurrentDirectory(), Global.AppConfigurationFile)))
            {
                //initialize a new configuration with default values
                Initialize();
            }
        }

        public AppConfiguration Configuration
        {
            get
            {
                _readerWriterLockSlim.EnterReadLock();
                try
                {
                    return ReadFromFile();
                }
                finally
                {
                    _readerWriterLockSlim.ExitReadLock();
                }
            }
            set
            {
                _readerWriterLockSlim.EnterWriteLock();
                try
                {
                    if (value != null)
                    {
                        WriteToFile(value);
                    }
                }
                finally
                {
                    _readerWriterLockSlim.ExitWriteLock();
                }
            }
        }

        public void Reset()
        {
            Initialize();
        }

        private void Initialize()
        {
            //initialize a new configuration with default values
            Configuration = new AppConfiguration
            {
                LogLevel = 2,
                Proxies = new List<Proxy>
                {
                    new Proxy
                    {
                        Name = Global.DefaultFamilyFilterKey,
                        Stamp = Global.DefaultFamilyFilterStamp
                    },
                    new Proxy
                    {
                        Name = Global.DefaultAdultFilterKey,
                        Stamp = Global.DefaultAdultFilterStamp
                    }
                }
            };
        }

        private bool WriteToFile(AppConfiguration appConfiguration)
        {
            try
            {
                var configFile = Path.Combine(Directory.GetCurrentDirectory(), Global.AppConfigurationFile);
                var settings = TomlSettings.Create(s => s.ConfigurePropertyMapping(m => m.UseKeyGenerator(standardGenerators => standardGenerators.LowerCase)));
                Toml.WriteFile(appConfiguration, configFile, settings);
                return true;
            }
            catch (Exception exception)
            {
                _logger.Log(exception.Message, Category.Exception, Priority.Medium);
                return false;
            }
        }

        private AppConfiguration ReadFromFile()
        {
            try
            {
                var settings = TomlSettings.Create(s => s.ConfigurePropertyMapping(m => m.UseTargetPropertySelector(standardSelectors => standardSelectors.IgnoreCase)));
                var configuration = Toml.ReadFile<AppConfiguration>(Path.Combine(Directory.GetCurrentDirectory(), Global.AppConfigurationFile), settings);
                return configuration;
            }
            catch (Exception exception)
            {
                _logger.Log(exception.Message, Category.Exception, Priority.Medium);
                return new AppConfiguration();
            }
        }
    }
}
