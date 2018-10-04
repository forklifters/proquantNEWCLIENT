using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace ProQuant
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class Splash : ContentPage
    {



        public Splash()
        {
            InitializeComponent();
            wait(1000);
        }

        async private void wait(int x)
        {
            await Task.Delay(x);
            go();

        }

        async private void go()
        {

            await Navigation.PushAsync(new LoginPage());
        }

        protected override void OnDisappearing()
        {
            Navigation.RemovePage(this);
            base.OnDisappearing();
        }
    }
}