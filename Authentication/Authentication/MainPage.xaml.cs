using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Security.Authentication.Web;
using Windows.Security.Authentication.Web.Core;
using Windows.Security.Authentication.Web.Provider;
using Windows.Security.Credentials;
using Windows.Security.Credentials.UI;
using Windows.Storage;
using Windows.System;
using Windows.UI.ApplicationSettings;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace Authentication
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            AccountsSettingsPane.GetForCurrentView().AccountCommandsRequested += MainPage_AccountCommandsRequested;
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
        }

        private async void MainPage_AccountCommandsRequested(AccountsSettingsPane sender, AccountsSettingsPaneCommandsRequestedEventArgs args)
        {
            var deferral = args.GetDeferral();
            try
            {
                // MSA
                {
                    var provider = await GetProvider(MicrosoftProviderId, MsaAuthority);
                    var command = new AuthenticationCommand { ClientId = MsaClientId, Scope = MsaClientId };
                    WebAccountProviderCommand boundCommand = new WebAccountProviderCommand(provider, command.CommandInvoked);
                    args.WebAccountProviderCommands.Add(boundCommand);
                }

                // AAD
                // https://blogs.technet.microsoft.com/enterprisemobility/2015/08/03/develop-windows-universal-apps-with-azure-ad-and-the-windows-10-identity-api/
                {
                    var provider = await GetProvider(MicrosoftProviderId, MicrosoftProviderId + "/" /*AadAuthority*/);
                    var command = new AuthenticationCommand { ClientId = AadClientId, Scope = string.Empty };
                    var boundCommand = new WebAccountProviderCommand(provider, command.CommandInvoked);
                    args.WebAccountProviderCommands.Add(boundCommand);
                }
            }
            finally
            {
                deferral.Complete();
            }
        }

        const string DefaultProvider = "https://login.windows.local";
        const string MicrosoftProviderId = "https://login.microsoft.com";
        const string MsaAuthority = "consumers";

        const string AadTenant = "llamallama.onmicrosoft.com";
        const string AadAuthority = MicrosoftProviderId + "/" + AadTenant;
        const string AadClientId = "notsure";
        // const string AadAuthority = "organizations";

        const string MsaClientId = "none";
        const string MsaScope = "wl.basic wl.signin wl.skydrive wl.photos";
        

        private async Task<WebAccountProvider> GetProvider(string providerId, string authority)
        {
            var provider = await WebAuthenticationCoreManager.FindAccountProviderAsync(providerId, authority);
            return provider;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            AccountsSettingsPane.Show();
        }

        private async void GetCurrentUserButton_Click(object sender, RoutedEventArgs e)
        {
            var users = await User.FindAllAsync();
            var desiredProperties = new string[]
            {
                KnownUserProperties.FirstName,
                KnownUserProperties.LastName,
                KnownUserProperties.ProviderName,
                KnownUserProperties.AccountName,
                KnownUserProperties.GuestHost,
                KnownUserProperties.PrincipalName,
                KnownUserProperties.DomainName,
                KnownUserProperties.SessionInitiationProtocolUri,
            };
            var properties = await users[0].GetPropertiesAsync(desiredProperties);
            foreach(var p in properties)
            {
                Debug.WriteLine($"{p.Key} : {p.Value}");
            }
        }

        const string SettingName = "MyCoolRoamingSetting";

        private void WriteSettingButton_Click(object sender, RoutedEventArgs e)
        {
            ApplicationData.Current.RoamingSettings.Values[SettingName] = SettingValueBox.Text;
        }

        private void ReadSettingButton_Click(object sender, RoutedEventArgs e)
        {
            var value = (ApplicationData.Current.RoamingSettings.Values[SettingName] as string) ?? "No Value Yet.";
            SettingValue.Text = value;
        }
    }
}
