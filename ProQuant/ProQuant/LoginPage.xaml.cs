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
            SavedPassCheck();
            
        }

        

        private async void SavedPassCheck()
        {
            string posUsername = null;
            string posPassword = null;

            try
            {
                 posUsername = await SecureStorage.GetAsync("username");
                 posPassword = await SecureStorage.GetAsync("password");
            }catch(Exception ex)
            {
                //send details to the proquant log.
            }

            if (!string.IsNullOrEmpty(posUsername) && !string.IsNullOrEmpty(posPassword))
            {
                busy = true;
                busyNow();

                string tokenKey = "/api/api/5?id=cmd$~gettoken";

                string response = await Client.GET_Token(tokenKey, "Basic", posUsername, posPassword);
                if (response == "errorerrorerror")
                    return;

                TokenInfo tokenInfo = TokenInfo.FromJson(response);

                cnx.Token = tokenInfo.Token;
                cnx.ID = tokenInfo.Id;
                cnx.MD = tokenInfo.Md;
                cnx.Name = tokenInfo.Name;
                cnx.TokenInfoJsonProps = tokenInfo;

                if (!string.IsNullOrEmpty(tokenInfo.Temp))
                {
                    try
                    {
                        SecureStorage.RemoveAll();
                    }
                    catch (Exception ex)
                    {
                        // sent details to proquant log
                    }

                    busy = false;
                    Notbusy();
                    return;   
                }
                else
                {
                    if (string.IsNullOrEmpty(tokenInfo.Error))
                    {
                        go();
                        busy = false;
                        Notbusy();
                    }
                    else
                    {
                        if (tokenInfo.Error.Contains("error unautherised user"))
                        {
                            try
                            {
                                SecureStorage.RemoveAll();
                            }
                            catch (Exception ex)
                            {
                                // sent details to proquant log
                            }
                            busy = false;
                            Notbusy();
                            return;
                        }
                        else
                        {
                            busy = false;
                            Notbusy();
                            return;
                        }
                    }
                }
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

        private async void SignUpClicked(object sender, EventArgs e)
        {
            ConnectionCheck();
            if (connected == true)
            {
                if (busy == false)
                {
                    busy = true;
                    busyNow();
                    await Navigation.PushAsync(new Register()
                    {
                        Title = "Sign Up"
                    });
                   

                   



                    busy = false;
                    Notbusy();
                }
            }

            //potentially a pop up to ring us
        }

        protected override bool OnBackButtonPressed()
        {
            return true;
        }


        private async void LogInClicked(object sender, EventArgs e)
        {
            ConnectionCheck();
            if (connected == true)
            {
                if (busy == false)
                {
                    busy = true;
                    busyNow();

                    cnx.User = EmailEntry.Text;
                    cnx.Pass = PassEntry.Text;
                    PassEntry.Text = "";

                    string tokenKey = "/api/api/5?id=cmd$~gettoken";

                    //cnx.User = "oliver.filmer@proquantestimating.co.uk";
                    //cnx.Pass = "password2";

                    //cnx.User = "dominic.bright@jewson.co.uk";
                    //cnx.Pass = "proQuant97";

                    //VV this works when not testing uncomment this and comment the user and other stuff

                    string response = await Client.GET_Token(tokenKey, "Basic", cnx.User, cnx.Pass);
                    if(response == "errorerrorerror")
                    {
                        await DisplayAlert("Http Request Error", "Please try again.\n\nError Code: LP01\n\nIf this keeps happening, please contact us.", "Ok");
                        busy = false;
                        Notbusy();
                        return;
                    }
                    TokenInfo tokenInfo = TokenInfo.FromJson(response);

                    cnx.Token = tokenInfo.Token;
                    cnx.ID = tokenInfo.Id;
                    cnx.MD = tokenInfo.Md;
                    cnx.Name = tokenInfo.Name;
                    cnx.TokenInfoJsonProps = tokenInfo;

                    if (!string.IsNullOrEmpty(tokenInfo.Temp))
                    {
                        //FORCE CHANGE PASSWORD
                        await Navigation.PushModalAsync(new ChangePassword(tokenInfo), true);
                        PassEntry.Text = "";
                        go();
                    }
                    else
                    {
                        if (string.IsNullOrEmpty(tokenInfo.Error))
                        {
                            try
                            {
                                SecureStorage.RemoveAll();
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine(ex.Message);
                            }

                            try
                            {
                                await SecureStorage.SetAsync("username", cnx.User);
                                await SecureStorage.SetAsync("password", cnx.Pass);

                            }
                            catch (Exception ex)
                            {
                                //TODO: send exception log to proquant.
                            }

                            go();

                        }
                        else
                        {
                            if (tokenInfo.Error.Contains("error unautherised user"))
                            {
                                await DisplayAlert("User Not Recognised",
                                    "Your Email and/or Password has not been recognised.\n\n" +
                                    "If you were using a temporary password it may have expired.\n\n" +
                                    "If problems persist, try signing up again, or call us\n\nError Code: LP02", "Ok");
                            }
                            else
                            {
                                await DisplayAlert("ERROR", tokenInfo.Error, "Ok");
                            }
                        }
                    }


                    

                    busy = false;
                    Notbusy();
                }
            }
        }


    async private void go()
        {
            ConnectionCheck();
            if (cnx != null)
            {
                main main = new main(cnx)
                {
                    BarBackgroundColor = Color.FromHex("#fe0000"),
                    BarTextColor = Color.White
                };

                await Navigation.PushAsync(main);
            }
        }

        public static string Base64Encode(string plainText)
        {
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
            return System.Convert.ToBase64String(plainTextBytes);
        }

        public void busyNow()
        {
            PassEntry.IsEnabled = false;
            LogInButton.IsEnabled = false;
            EmailEntry.IsEnabled = false;
            SignUpButton.IsEnabled = false;
        }

        public void Notbusy()
        {
            PassEntry.IsEnabled = true;
            LogInButton.IsEnabled = true;
            EmailEntry.IsEnabled = true;
            SignUpButton.IsEnabled = true;
        }


    }
}