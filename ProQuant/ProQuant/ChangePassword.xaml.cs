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
            tokenInfo = _tokenInfo;
            EmailLabel.Text = tokenInfo.Email;
            if (!string.IsNullOrWhiteSpace(tokenInfo.Temp))
            {
                OldPassword.Text = tokenInfo.Temp;
                OldPassword.IsEnabled = false;
            }           
		}

        private async void Button_Clicked(object sender, EventArgs e)
        {
            if(ConfirmPassword.Text != NewPassword.Text)
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
                    await DisplayAlert("Http Request Error", "Please try again.\n\nIf this keeps happening, please contact us.", "Ok");
                    return;
                }
                //do a check to see if it came back ok, or try catch exceptions here.

                TokenInfo newTokenInfo = TokenInfo.FromJson(response);

                Connection cnx = new Connection();
                cnx.ID = newTokenInfo.Id;
                cnx.Name = newTokenInfo.Name;
                cnx.Token = newTokenInfo.Token;
                cnx.TokenInfoJsonProps = newTokenInfo;

                LoginPage.cnx = cnx;
                await DisplayAlert("Password Changed", "Your password has been changed.", "Ok");
                await Navigation.PopModalAsync(true);
            }
        }
    }
}