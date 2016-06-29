using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.ApplicationModel.AppService;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace ManagedInvokerApp
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

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            await LaunchConnection();
        }

        private async Task LaunchConnection()
        {
            using (var connection = new AppServiceConnection())
            {
                connection.AppServiceName = "JavaScriptService";
                connection.PackageFamilyName = "ddf1732b-1e76-4cf8-92ef-cf02408ff6cd_m8xzdp2rtcct6";
                var state = await connection.OpenAsync();

                if (state == AppServiceConnectionStatus.Success)
                {
                    var message = new ValueSet();
                    message.Add("X1", "Test Test Test");
                    await connection.SendMessageAsync(message);
                }

                await Task.Delay(TimeSpan.FromMinutes(10));
            }
        }
    }
}
