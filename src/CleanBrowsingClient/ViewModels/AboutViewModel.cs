using CleanBrowsingClient.Helper;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Regions;
using System;

namespace CleanBrowsingClient.ViewModels
{
    public class AboutViewModel : BindableBase
    {
        private readonly IRegionManager _regionManager;

        public DelegateCommand NavigateToMainView { get; private set; }
        public DelegateCommand<string> OpenWebCommand { get; private set; }

        public AboutViewModel(IRegionManager regionManager)
        {
            _regionManager = regionManager ?? throw new ArgumentNullException(nameof(regionManager));
            NavigateToMainView = new DelegateCommand(NavigateToMain);
            OpenWebCommand = new DelegateCommand<string>(OpenUrl);
        }

        private void NavigateToMain()
        {
            _regionManager.RequestNavigate("ContentRegion", "MainView");
        }

        private void OpenUrl(string url)
        {
            CoreHelper.OpenBrowser(url);
        }
    }
}
