using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace ProQuant
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class Register : ContentPage
	{
        public bool connected;

		public Register ()
		{
			InitializeComponent ();
		}

        private async void RegisterButton(object sender, EventArgs e)
        {
            ConnectionCheck();
            string email = EmailBar.Text;
            //do a regex check on the email here
            string key = string.Format("/api/api/5?id=id$~9999~cmd$~register~{0}", email);
            var response = await Client.GETnoAuth(key);
            if (response == "errorerrorerror")
            {
                await DisplayAlert("Http Request Error", "Please try again.\n\nIf this keeps happening, please contact us.", "Ok");
                return;
            }

            if (!string.IsNullOrWhiteSpace(response))
            {
                RegisterResponse RegResponse = RegisterResponse.FromJson(response);
                if (string.IsNullOrWhiteSpace(RegResponse.error))
                {
                    await DisplayAlert("Temporary Password", $"A temporary password has been sent to:\n {email}\n\nIf you have not received this, please ring us.", "Ok");
                    await DisplayAlert("Temporary Password", $"You will be asked to change the temporary password after your first log in.", "Ok");
                    await Navigation.PopAsync();
                }
                else
                {
                    await DisplayAlert("Error", "Message: " + RegResponse.error, "Ok");
                }
            }
            else
            {
                await DisplayAlert("Error", "No response from server. \nPlease call us for assistance.", "Ok");
            }
            
            


        }

        private void CallOffice(object sender, EventArgs e)
        {
            //CALL OFFICE
        }

        public async void ConnectionCheck()
        {
            bool Connected = App.CheckConnection();
            if (Connected == false)
            {
                connected = false;
                await DisplayAlert("No Connection", "Internet Connection is needed for this app to function", "Ok, Exit App");
                App.ExitApp();

            }
            connected = true;
        }
    }
}