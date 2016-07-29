using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Security.Authentication.Web.Core;
using Windows.Security.Credentials;
using Windows.UI.ApplicationSettings;

namespace Authentication
{
    class AuthenticationCommand
    {
        public string Scope { get; set; }
        public string ClientId { get; set; }

        public async void CommandInvoked(WebAccountProviderCommand command)
        {
            await AuthenticateWithRequestToken(command.WebAccountProvider, Scope, ClientId);
        }

        private static async Task AuthenticateWithRequestToken(WebAccountProvider provider, string scope, string clientId)
        {
            var tokenRequest = new WebTokenRequest(provider, scope, clientId, WebTokenRequestPromptType.ForceAuthentication);
            var result = await WebAuthenticationCoreManager.RequestTokenAsync(tokenRequest);
            if (result.ResponseStatus == WebTokenRequestStatus.Success)
            {
                DumpResponse(result.ResponseData[0]);
            }
        }

        private static void DumpResponse(WebTokenResponse response)
        {
            Debug.WriteLine($"Token = {response.Token}");
            foreach (var s in response.Properties)
            {
                Debug.WriteLine($"{s.Key} : {s.Value}");
            }
            var webAccount = response.WebAccount;
            if (webAccount != null)
            {
                Debug.WriteLine($"AccountId : {webAccount.Id}");
                Debug.WriteLine($"State : {webAccount.State}");
                Debug.WriteLine($"UserName : {webAccount.UserName}");
                foreach (var s in webAccount.Properties)
                {
                    Debug.WriteLine($"{s.Key} : {s.Value}");
                }
            }

            //var username = webAccount?.UserName;
            //UserNameBlock.Text = string.IsNullOrEmpty(username) ? "No User Name" : username;

            //try
            //{
            //    if (webAccount != null)
            //    {
            //        BitmapImage image = new BitmapImage();
            //        await image.SetSourceAsync(await webAccount.GetPictureAsync(WebAccountPictureSize.Size64x64));
            //        LoginImage.Source = image;
            //    }
            //}
            //catch (Exception ex)
            //{
            //    Debug.WriteLine($"Failed to load image: {ex.Message}");
            //}
        }
    }
}
