using CleanBrowsingClient.Config;
using CleanBrowsingClient.Events;
using CleanBrowsingClient.Models;
using DnsCrypt.Stamps;
using MaterialDesignThemes.Wpf;
using Prism.Commands;
using Prism.Events;
using Prism.Logging;
using Prism.Mvvm;
using Prism.Regions;
using System;

namespace CleanBrowsingClient.ViewModels
{
    public class StampViewModel : BindableBase
    {
        private string _stamp;
        private readonly ILoggerFacade _logger;
        private readonly IEventAggregator _eventAggregator;
        private readonly IRegionManager _regionManager;
        private ISnackbarMessageQueue _messageQueue;

        public ISnackbarMessageQueue MessageQueue
        {
            get { return _messageQueue; }
            set { SetProperty(ref _messageQueue, value); }
        }

        public string Stamp
        {
            get { return _stamp; }
            set { SetProperty(ref _stamp, value); }
        }

        public DelegateCommand NavigateToMainView { get; private set; }
        public DelegateCommand SaveStampCommand { get; private set; }
        public StampViewModel(
            ILoggerFacade logger,
            IRegionManager regionManager,
            IEventAggregator eventAggregator,
            ISnackbarMessageQueue snackbarMessageQueue)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _regionManager = regionManager ?? throw new ArgumentNullException(nameof(regionManager));
            _eventAggregator = eventAggregator ?? throw new ArgumentNullException(nameof(eventAggregator));
            _messageQueue = snackbarMessageQueue ?? throw new ArgumentNullException(nameof(snackbarMessageQueue));
            NavigateToMainView = new DelegateCommand(NavigateToMain);
            SaveStampCommand = new DelegateCommand(SaveStamp);
        }

        private void NavigateToMain()
        {
            _regionManager.RequestNavigate("ContentRegion", "MainView");
        }

        private void SaveStamp()
        {
            if (!string.IsNullOrEmpty(Stamp))
            {
                var decodedStamp = StampTools.Decode(Stamp.Trim());
                if (decodedStamp != null)
                {
                    var addStamp = false;
                    if (decodedStamp.Protocol == DnsCrypt.Models.StampProtocol.DnsCrypt)
                    {
                        //simple check if the stamp is a valid cleanbrowsing stamp
                        if (decodedStamp.ProviderName.Equals(Global.ValidCleanBrowsingDnsCryptStamp))
                        {
                            _logger.Log("valid DnsCrypt stamp", Category.Info, Priority.Low);
                            addStamp = true;
                        }
                    }
                    else if (decodedStamp.Protocol == DnsCrypt.Models.StampProtocol.DoH)
                    {
                        //simple check if the stamp is a valid cleanbrowsing stamp
                        if (decodedStamp.Hostname.Equals(Global.ValidCleanBrowsingDohStamp))
                        {
                            _logger.Log("valid DoH stamp", Category.Info, Priority.Low);
                            addStamp = true;
                        }
                    }
                    else
                    {
                        //unsupported stamp
                        _logger.Log("unsupported stamp type", Category.Warn, Priority.Medium);
                        addStamp = false;
                    }

                    if (addStamp)
                    {
                        _eventAggregator.GetEvent<StampAddedEvent>().Publish(new Proxy
                        {
                            Name = Global.DefaultCustomFilterKey,
                            Stamp = Stamp.Trim()
                        });
                        NavigateToMain();
                    }
                    else
                    {
                        MessageQueue.Enqueue("not a valid stamp://");
                    }
                }
                else
                {
                    MessageQueue.Enqueue("not a valid stamp://");
                }
            }
        }
    }
}
