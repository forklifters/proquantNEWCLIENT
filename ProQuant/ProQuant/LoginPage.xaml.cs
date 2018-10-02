using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Refit;
using System.Net;
using Xamarin.Essentials;
using Plugin.Connectivity;
using System.Net.Http;

namespace ProQuant
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class LoginPage : ContentPage
    {
        bool busy = false;

        public static bool connected = false;


        public static Connection cnx = new Connection();


        public LoginPage()
        {
            InitializeComponent();
            ConnectionCheck();
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

        private async void SignUpClicked(object sender, EventArgs e)
        {
            ConnectionCheck();
            if (connected == true)
            {
                if (busy == false)
                {
                    busy = true;
                    await Navigation.PushAsync(new Register()
                    {
                        Title = "Sign Up"
                    });
                   

                   



                    busy = false;
                }
            }

            //potentially a pop up to ring us
        }

        protected override bool OnBackButtonPressed()
        {
            return true;
        }

        private void ForgotPasswordClicked(object sender, EventArgs e)
        {
            ConnectionCheck();

            //pop up?
        }

        private async void LogInClicked(object sender, EventArgs e)
        {
            ConnectionCheck();
            if (connected == true)
            {
                if (busy == false)
                {
                    busy = true;
                    cnx.User = EmailEntry.Text;
                    cnx.Pass = PassEntry.Text;

                    string tokenKey = "/api/api/5?id=cmd$~gettoken";



                    //################################################################
                    //WHEN NOT TESTING COMMENT THESE LINES OUT AND REPLACE WITH BELOW
                    //string user = "egbbuilders@aol.com"; //change to EmailEntry.Text                           //when not testing comment these lines out.
                    //string pass = "proQuant97"; //change to PassEntry.Text
                    //string response = await Client.GET_Token(tokenKey, "Basic", user, pass);
                    //TokenInfoJsonParse tokenInfo = TokenInfoJsonParse.FromJson(response);
                    
                    
                    

                    //################################################################

                    //VV this works when not testing uncomment this and comment the user and other stuff

                    string response = await Client.GET_Token(tokenKey, "Basic", cnx.User, cnx.Pass);
                    TokenInfo tokenInfo = TokenInfo.FromJson(response);

                    cnx.Token = tokenInfo.Token;
                    cnx.ID = tokenInfo.Id;
                    cnx.Name = tokenInfo.Name;
                    cnx.TokenInfoJsonProps = tokenInfo;

                    if (!string.IsNullOrEmpty(tokenInfo.Temp))
                    {
                        //FORCE CHANGE PASSWORD
                        await Navigation.PushModalAsync(new ChangePassword(tokenInfo), true);
                        go();
                    }
                    else
                    {
                        if (string.IsNullOrEmpty(tokenInfo.Error))
                        {
                            go();
                        }
                        else
                        {
                            if (tokenInfo.Error.Contains("error unautherised user"))
                            {
                                await DisplayAlert("User Not Recognised",
                                    "Your Email and/or Password has not been recognised.\n\n" +
                                    "If you were using a temporary password it may have expired.\n\n" +
                                    "If problems persist, try signing up again, or call us", "Ok");
                            }
                            else
                            {
                                await DisplayAlert("ERROR", tokenInfo.Error, "Ok");
                            }
                        }
                    }


                    

                    busy = false;
                }
            }
        }


    async private void go()
        {
            ConnectionCheck();
            if (cnx != null)
            {
                await Navigation.PushAsync(new main(cnx)
                {
                    BarBackgroundColor = Color.FromHex("#fe0000"),
                    BarTextColor = Color.White
                });
            }
        }

        public static string Base64Encode(string plainText)
        {
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
            return System.Convert.ToBase64String(plainTextBytes);
        }


    }
}