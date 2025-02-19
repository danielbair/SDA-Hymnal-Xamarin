using System.Collections.Generic;
using Hymnal.Core.Services;
using Microsoft.AppCenter.Analytics;
using MvvmCross.Logging;
using MvvmCross.Navigation;
using MvvmCross.ViewModels;
using Xamarin.Essentials;

namespace Hymnal.Core.ViewModels
{
    public class RootViewModel : MvxViewModel
    {
        private readonly IMvxNavigationService navigationService;
        private readonly IMvxLog log;
        private readonly IPreferencesService preferencesService;

        public RootViewModel(
            IMvxNavigationService navigationService,
            IMvxLog log,
            IPreferencesService preferencesService
            )
        {
            this.navigationService = navigationService;
            this.log = log;
            this.preferencesService = preferencesService;
        }

        private bool loaded = false;
        public override async void ViewAppearing()
        {
            base.ViewAppearing();

            if (loaded)
                return;

            loaded = true;

            // KeepScreenOn
            if (DeviceInfo.Platform == DevicePlatform.iOS ||
                DeviceInfo.Platform == DevicePlatform.Android ||
                DeviceInfo.Platform == DevicePlatform.UWP)
            {
                DeviceDisplay.KeepScreenOn = preferencesService.KeepScreenOn;
            }


            if (DeviceInfo.Platform == DevicePlatform.iOS ||
                DeviceInfo.Platform == DevicePlatform.Android)
            {
                await navigationService.Navigate<NumberViewModel>();
                await navigationService.Navigate<IndexViewModel>();
                await navigationService.Navigate<FavoritesViewModel>();
                await navigationService.Navigate<SettingsViewModel>();
            }
            else if (DeviceInfo.Platform == DevicePlatform.tvOS)
            {
                await navigationService.Navigate<NumberViewModel>();
                await navigationService.Navigate<SearchViewModel>();
                await navigationService.Navigate<NumericalIndexViewModel>();
                await navigationService.Navigate<SettingsViewModel>();
            }
            else if (DeviceInfo.Platform == DevicePlatform.Tizen)
            {
                await navigationService.Navigate<NumberViewModel>();
                await navigationService.Navigate<SearchViewModel>();
                await navigationService.Navigate<SettingsViewModel>();
            }
            else if (DeviceInfo.Platform == DevicePlatform.UWP)
            {
                await navigationService.Navigate<NumberViewModel>();
            }
            else
            {
                await navigationService.Navigate<SimpleViewModel>();
            }
        }

        // LifeCycle implemented in RootViewModel
        #region LifeCycle
        public override void Start()
        {
            log.Debug("App Started");

            if (DeviceInfo.Platform == DevicePlatform.iOS ||
                DeviceInfo.Platform == DevicePlatform.Android)
            {
                Analytics.TrackEvent(Constants.TrackEv.AppOpened, new Dictionary<string, string>
                {
                    { Constants.TrackEv.AppOpenedScheme.CultureInfo, Constants.CurrentCultureInfo.Name },
                    { Constants.TrackEv.AppOpenedScheme.HymnalVersion, preferencesService.ConfiguratedHymnalLanguage.Id },
                    { Constants.TrackEv.AppOpenedScheme.ThemeConfigurated, AppInfo.RequestedTheme.ToString() }
                });
            }

            base.Start();
        }
        #endregion
    }
}
