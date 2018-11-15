using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using System.Globalization;
using System.Threading;

using System.Threading.Tasks;
using Refit;
using Plugin.Connectivity;

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
            // Handle when your app starts
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
