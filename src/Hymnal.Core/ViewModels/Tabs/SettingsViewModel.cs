using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using Hymnal.Core.Extensions;
using Hymnal.Core.Helpers;
using Hymnal.Core.Models;
using Hymnal.Core.Services;
using Microsoft.AppCenter.Analytics;
using MvvmCross.Commands;
using MvvmCross.Navigation;
using MvvmCross.ViewModels;
using Xamarin.Essentials;

namespace Hymnal.Core.ViewModels
{
    public class SettingsViewModel : MvxViewModel
    {
        private readonly IMvxNavigationService navigationService;
        private readonly IPreferencesService preferencesService;
        private readonly IDialogService dialogService;

        private readonly BrowserLaunchOptions browserLaunchOptions = new BrowserLaunchOptions
        {
            LaunchMode = BrowserLaunchMode.SystemPreferred,
            TitleMode = BrowserTitleMode.Show,
            PreferredToolbarColor = Color.Black,
            PreferredControlColor = Color.White
        };

        public int HymnFontSize
        {
            get => preferencesService.HymnalsFontSize;
            set => preferencesService.HymnalsFontSize = value;
        }
        public int MinimumHymnFontSize => Constants.MINIMUM_HYMNALS_FONT_SIZE;
        public int MaximumHymnFontSize => Constants.MAXIMUM_HYMNALS_FONT_SIZE;

        private string appVersionString;
        public string AppVersionString
        {
            get => appVersionString;
            set => SetProperty(ref appVersionString, value);
        }

        private string appBuildString;
        public string AppBuildString
        {
            get => appBuildString;
            set => SetProperty(ref appBuildString, value);
        }

        private HymnalLanguage hymnalLanguage;
        public HymnalLanguage HymnalLanguage
        {
            get => hymnalLanguage;
            set => SetProperty(ref hymnalLanguage, value);
        }

        public bool KeepScreenOn
        {
            get
            {
                if (DeviceInfo.Platform == DevicePlatform.iOS ||
                    DeviceInfo.Platform == DevicePlatform.Android)
                {
                return DeviceDisplay.KeepScreenOn;

                }
                return false;
            }
            set
            {
                DeviceDisplay.KeepScreenOn = value;
                preferencesService.KeepScreenOn = value;
                RaisePropertyChanged(nameof(KeepScreenOn));
            }
        }

        public SettingsViewModel(
            IMvxNavigationService navigationService,
            IPreferencesService preferencesService,
            IDialogService dialogService
            )
        {
            this.navigationService = navigationService;
            this.preferencesService = preferencesService;
            this.dialogService = dialogService;
        }

        public override async Task Initialize()
        {
            await base.Initialize();
            HymnalLanguage = preferencesService.ConfiguratedHymnalLanguage;

            AppVersionString = AppInfo.VersionString;
            AppBuildString = AppInfo.BuildString;
        }

        public override void ViewAppeared()
        {
            base.ViewAppeared();

            Analytics.TrackEvent(Constants.TrackEv.Navigation, new Dictionary<string, string>
            {
                { Constants.TrackEv.NavigationReferenceScheme.PageName, nameof(SettingsViewModel) },
                { Constants.TrackEv.NavigationReferenceScheme.CultureInfo, Constants.CurrentCultureInfo.Name },
                { Constants.TrackEv.NavigationReferenceScheme.HymnalVersion, preferencesService.ConfiguratedHymnalLanguage.Id }
            });
        }

        public MvxAsyncCommand ChooseLanguageCommand => new MvxAsyncCommand(ChooseLanguageExecuteAsync);
        private async Task ChooseLanguageExecuteAsync()
        {
            var languages = new Dictionary<string, HymnalLanguage>();

            foreach (HymnalLanguage hl in Constants.HymnsLanguages)
            {
                languages.Add($"({hl.Detail}) {hl.Name}", hl);
            }

            var title = Languages.ChooseYourHymnal;
            var cancelButton = Languages.Cancel;

            var languageKey = await dialogService.DisplayActionSheet(
                title,
                cancelButton,
                null,
                languages.Select(hld => hld.Key).ToArray()
                );

            if (string.IsNullOrEmpty(languageKey) || languageKey.Equals(cancelButton))
                return;

            HymnalLanguage = languages[languageKey];
            preferencesService.ConfiguratedHymnalLanguage = HymnalLanguage;
        }

        public MvxAsyncCommand HelpCommand => new MvxAsyncCommand(HelpExecuteAsync);
        private async Task HelpExecuteAsync()
        {
            await navigationService.Navigate<HelpViewModel>();
        }

        public MvxAsyncCommand OpenGitHubCommand => new MvxAsyncCommand(OpenGitHubExecuteAsync);
        private async Task OpenGitHubExecuteAsync()
        {
            await Browser.OpenAsync(Constants.WebLinks.GitHubDevelopingLink, browserLaunchOptions);
        }

        public MvxAsyncCommand DeveloperCommand => new MvxAsyncCommand(DeveloperExecuteAsync);
        private async Task DeveloperExecuteAsync()
        {
            await Browser.OpenAsync(Constants.WebLinks.DeveloperWebSite, browserLaunchOptions);
        }
    }
}
