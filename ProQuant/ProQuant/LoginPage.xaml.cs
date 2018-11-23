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
using Microsoft.AppCenter.Analytics;

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
                SendError("LP06", "Exception occured when getting username/password from securedstorage.", ex.Message);
            }

            if (!string.IsNullOrEmpty(posUsername) && !string.IsNullOrEmpty(posPassword))
            {
                busy = true;
                busyNow();

                string tokenKey = "/api/api/5?id=cmd$~gettoken";

                string response = await Client.GET_Token(tokenKey, "Basic", posUsername, posPassword);
                if (response == "errorerrorerror")
                {
                    SendError("LP05", "Http GET request error on Client.GET_Token() call. - Returning user information");
                    return;
                }
                   
                TokenInfo tokenInfo = TokenInfo.FromJson(response);

                cnx.Token = tokenInfo.Token;
                cnx.ID = tokenInfo.Id;
                cnx.MD = tokenInfo.Md;
                cnx.Name = tokenInfo.Name;
                cnx.TokenInfoJsonProps = tokenInfo;
                cnx.User = tokenInfo.Email;
                

                if (!string.IsNullOrEmpty(tokenInfo.Temp))
                {
                    try
                    {
                        SecureStorage.RemoveAll();
                    }
                    catch (Exception ex)
                    {
                        SendError("LP03", "Unable to remove secure storage Entries", ex.Message);
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
                                //Error Code LP03 - Unable to remove Secure Storage Entries
                                SendError("LP04","Unable to remove secure storage Entries", ex.Message);
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
                        SendError("LP01", "Http GET request error on Client.GET_Token() call during login.");
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
                                //Error Code: LP09
                                SendError("LP09", "Error when saving username/password to Secure Storage");
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
                                SendError("LP02", "User Not Recognised");
                            }
                            else
                            {
                                await DisplayAlert("ERROR", tokenInfo.Error, "Ok");
                                SendError("LP07", "Error that isn't unauthorised user", tokenInfo.Error);
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

                //test write to log
              
                Log log = new Log()
                {
                    LogLog = $"[MOBILE]: {cnx.User} list entry log 1",
                    Datetime = $"{DateTime.Now:r}"
                };

                Log log2 = new Log()
                {
                    LogLog = $"[MOBILE]: {cnx.User} list entry log 2",
                    Datetime = $"{DateTime.Now:r}"
                };


                List<Log> logs = new List<Log>();
                logs.Add(log);
                logs.Add(log2);

                //SendLogs(logs, cnx);
                //SendError("TestError", "This is a description");

                Analytics.TrackEvent("Successful Login", new Dictionary<string, string>
                {
                    {"MD",cnx.MD},
                    {"ID",cnx.ID},
                    {"Email",cnx.User},
                    {"Name",cnx.Name}
                });
            }
        }

        public static async void SendLogs(List<Log> logs, Connection cnx)
        {
            Cmd cmd = new Cmd()
            {
                Command = "writetologs"
            };

            User user = new User()
            {
                Token = cnx.Token,
                Id = cnx.ID,
                Md = cnx.MD,
                Name = cnx.Name,
                Email = cnx.User,
                Error = "",
                Temp = "",
            };

            WriteToLogs outgoing = new WriteToLogs()
            {
                Cmd = cmd,
                User = user,
                Logs = logs
            };

            string outgoingJson = outgoing.ToJson();

            string key = $"/api/api/5?id=";
            var response = await Client.Post(cnx.Token, key, outgoingJson);


            var x = response;
        
        }

        public static string Base64Encode(string plainText)
        {
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
            return System.Convert.ToBase64String(plainTextBytes);
        }

        public static async Task<string> GetPhoneNumber()
        {
            string phoneNumber = "";
            string key = "/api/api";
            string response = await Client.GETnoAuth(key);

            try
            {
                string[] resPts = response.Split(' ');
                foreach (string x in resPts)
                {
                    if (x.Contains("Tel:"))
                    {
                        string[] prts = x.Split(':');
                        phoneNumber = prts[1];
                    }
                }
            }
            catch (Exception ex)
            {
                SendError("LP08", "Error occured whilst trying to parse phone number", ex.Message);
            }

            return phoneNumber;
        }

        public static List<Log> MakeSingleLog(string v)
        {
            List<Log> logs = new List<Log>();
            Log log = new Log()
            {
                LogLog = v,
                Datetime = $"{DateTime.Now:r}"
            };
            logs.Add(log);

            return logs;
        }

        public static void SendError(string ErrorCode, string ErrorDescription, string info = "")
        {
            List<Log> logs = MakeSingleLog($"[MOBILE]: Error Code: {ErrorCode}");
            SendLogs(logs, cnx);
            try
            {
                Analytics.TrackEvent("Error Occured", new Dictionary<string, string>
                                    {
                                        {"MD",cnx.MD},
                                        {"ID",cnx.ID},
                                        {"Email",cnx.User},
                                        {"Name",cnx.Name},
                                        {"ErrorCode", ErrorCode},
                                        {"ErrorDesc", ErrorDescription},
                                        {"Device", DeviceInfo.Model},
                                        {"Manufacturer", DeviceInfo.Manufacturer},
                                        {"OS_v.", DeviceInfo.VersionString},
                                        {"Platform",DeviceInfo.Platform},
                                        {"Info", info}
                                    }
                );
            }
            catch (Exception)
            {
                return;
            }
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