using Hymnal.Core.ViewModels;
using MvvmCross.Forms.Presenters.Attributes;
using MvvmCross.Forms.Views;
using MvvmCross.Presenters;
using MvvmCross.Presenters.Attributes;
using MvvmCross.ViewModels;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Hymnal.XF.UI.Pages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class RecordsPage : MvxContentPage<RecordsViewModel>, IMvxOverridePresentationAttribute
    {
        public RecordsPage()
        {
            InitializeComponent();

            if ((Device.RuntimePlatform == Device.iOS && DeviceInfo.Version.Major < 13)
                || Device.RuntimePlatform != Device.iOS)
            {
                SlideBar.IsVisible = false;
            }
        }

        public MvxBasePresentationAttribute PresentationAttribute(MvxViewModelRequest request)
        {
            if (Device.RuntimePlatform == Device.iOS && DeviceInfo.Version.Major >= 13)
            {
                return new MvxModalPresentationAttribute
                {
                    WrapInNavigationPage = false
                };

            } else
            {
                return new MvxContentPagePresentationAttribute
                {
                    WrapInNavigationPage = true,
                    NoHistory = false
                };
            }
        }
    }
}
