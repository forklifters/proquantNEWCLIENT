﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Xamarin.Essentials;

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
                LoginPage.SendError("R01", "Http GET request error on Client.GetnoAuth() call.");
                await DisplayAlert("Http Request Error", "Please try again.\n\nError Code: R01\n\nIf this keeps happening, please contact us.", "Ok");
                return;
            }

            if (!string.IsNullOrWhiteSpace(response))
            {
                RegisterResponse RegResponse = RegisterResponse.FromJson(response);
                if (string.IsNullOrWhiteSpace(RegResponse.error))
                {
                    MessagingCenter.Send(this, "message", email);
                    await DisplayAlert("Temporary Password", $"A temporary password has been sent to:\n {email}\n\nIf you have not received this, please ring us.", "Ok");
                    await DisplayAlert("Temporary Password", $"You will be asked to change the temporary password after your first log in.", "Ok");              
                    await Navigation.PopAsync();
                }
                else
                {
                    LoginPage.SendError("R02", "There has been an error returned with Register Response", RegResponse.error);
                    await DisplayAlert("Error", "There has been an error.\n\nPlease contact the office\n\nError Code: R02", "Ok");
                }
            }
            else
            {

                LoginPage.SendError("R03", "Nothing in response string.");
                await DisplayAlert("Error", "No response from server. \nPlease call us for assistance.\n\nError Code: R03", "Ok");
            }



        }

        private async void CallOffice(object sender, EventArgs e)
        {
            string phoneNumber = await LoginPage.GetPhoneNumber();

            try
            {
                //CHANGE TO REQUEST NUMBER
                PhoneDialer.Open(phoneNumber);
            }
            catch (FeatureNotSupportedException ex)
            {
                await DisplayAlert("ERROR", "Dialer feature not supported.\n\nError Code: R04", "OK");
                LoginPage.SendError("R04", "Dialer is unsupported on this device.");
                return;

            }
            catch (Exception ex)
            {
                LoginPage.SendError("R05", "Unknown exception", ex.Message);
                return;
            }
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