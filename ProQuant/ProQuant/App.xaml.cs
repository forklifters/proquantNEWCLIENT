using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using System.Globalization;
using System.Threading;

using System.Threading.Tasks;
using Refit;
using Plugin.Connectivity;

using Microsoft.AppCenter;
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;

[assembly: XamlCompilation (XamlCompilationOptions.Compile)]
namespace ProQuant
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
            CultureInfo.DefaultThreadCurrentCulture = new CultureInfo("en-GB");
            MainPage = new NavigationPage(new Splash())
            {
                BarBackgroundColor = Color.FromHex("#B80000"),
                BarTextColor = Color.White,
            };
        }

        protected override void OnStart()
        {
            AppCenter.Start("android=d3d541a3-9fd1-4a9b-b729-02b80d478a25;" 
                + "ios=3b9898ff-a95d-4553-9d41-9cb05d3e9746;"
                //"uwp={Your UWP App secret here};" +
                , typeof(Analytics), typeof(Crashes));

        }

        public static bool CheckConnection()
        {
            if (CrossConnectivity.Current.IsConnected != true)
            {
                return false;
            }
            else return true;
        }

        public static void ExitApp()
        {
            System.Diagnostics.Process.GetCurrentProcess().Kill();
        }

        protected override void OnSleep()
        {
            // Handle when your app sleeps
        }

        protected override void OnResume()
        {
            // Handle when your app resumes
        }

    }
}
