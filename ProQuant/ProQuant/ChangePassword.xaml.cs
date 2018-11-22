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
	public partial class ChangePassword : ContentPage
	{
        TokenInfo tokenInfo;
        bool connected = false;

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

        public ChangePassword (TokenInfo _tokenInfo)
		{
			InitializeComponent ();
            warning.IsVisible = false;
            tokenInfo = _tokenInfo;
            EmailLabel.Text = tokenInfo.Email;
            if (!string.IsNullOrWhiteSpace(tokenInfo.Temp))
            {
                BackButton.IsEnabled = false;
                BackButton.IsVisible = false;
                OldPassword.Text = tokenInfo.Temp;
                OldPassword.IsEnabled = false;
                OldPassword.IsPassword = false;
            }           
		}

        async void Button_Clicked(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(NewPassword.Text) || string.IsNullOrEmpty(ConfirmPassword.Text))
            {
                await DisplayAlert("Password Error", "You must input a password.", "Ok");
                return;
            }

            if (NewPassword.Text.Length < 5)
            {
                if (warning.IsVisible == false)
                {
                    warning.IsVisible = true;
                }
                else
                {
                    await DisplayAlert("Password Error","Your password must contain more that 5 characters.", "Ok");
                }
                return;
            }

            if (ConfirmPassword.Text != NewPassword.Text)
            {
                await DisplayAlert("Password Error", "Your new password doesn't match the password confirmation.", "Ok");
            }
            else
            {

                //correct request layout:
                // https://pqapi.co.uk:58330/api/api/5?id=id$~9999~cmd$~setpassword
                // header -->
                //username: oliver.filmer@proquantestimating.co.uk 
                //password: {y[qheCv:tester
                //ie: old:new

                string key = "/api/api/5?id=id$~9999~cmd$~setpassword";
                string user = tokenInfo.Email;

                //do all formating password checks before this.
                string passwordkey = string.Format("{0}:{1}", OldPassword.Text, NewPassword.Text);

                ConnectionCheck();
                var response = await Client.GETChangePassword(passwordkey, user, key);
                if (response == "errorerrorerror")
                {
                    await DisplayAlert("Http Request Error", "Please try again.\n\nError Code: CP01\n\nIf this keeps happening, please contact us.", "Ok");
                    LoginPage.SendError("CP01", "Http GET request error on Client.GetChangePassword() call.");
                    return;
                }
                //do a check to see if it came back ok, or try catch exceptions here.

                TokenInfo newTokenInfo = TokenInfo.FromJson(response);
                if (string.IsNullOrEmpty(newTokenInfo.Error))
                {
                    Connection cnx = new Connection();
                    cnx.ID = newTokenInfo.Id;
                    cnx.Name = newTokenInfo.Name;
                    cnx.Token = newTokenInfo.Token;
                    cnx.TokenInfoJsonProps = newTokenInfo;

                    LoginPage.cnx = cnx;
                    if (string.IsNullOrWhiteSpace(tokenInfo.Temp))
                    {
                        MessagingCenter.Send(this, "sendcnx", cnx);
                    }
                    await DisplayAlert("Password Changed", "Your password has been changed.", "Ok");
                    await Navigation.PopModalAsync(true);
                }
                else
                {
                    if(string.IsNullOrWhiteSpace(tokenInfo.Temp))
                    {
                        LoginPage.SendError("CP02", "There is a newTokenInfo.Error & No tokenInfo.Temp - Possibly entered old password incorrectly.");
                        await DisplayAlert("Password Change Error", "Please check you have entered the old password correctly.\n\nIf this keeps happening please contact the office\n\n Error Code:CP02", "Ok");

                    }
                    else
                    {
                        LoginPage.SendError("CP03", "There is a newTokenInfo. Error & the temp password is correct. Further investigation required.");
                        await DisplayAlert("Password Change Error", "Please try again.\n\nIf this keeps happening please contact the office\n\n Error Code:CP03", "Ok");

                    }
                }
                    
            }
        }

        private async void Back_Clicked(object sender, EventArgs e)
        {
            await Navigation.PopModalAsync(true);
        }
    }
}