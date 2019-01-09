using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Xamarin.Essentials;

using Refit;
using Newtonsoft.Json;

#if __ANDROID__
using Firebase.Iid;
#endif

#if __IOS__

#endif

namespace ProQuant
{
    //WORKING VERSION 3.0.0.561731

    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class main : TabbedPage
    {
        bool connected = false;
        bool searchBarRefreshed = false;
        public Connection Maincnx;
        public bool firstLoad = true;
        ListView listView;
        SearchBar searchbar;
        List<JobCell> _Cells = new List<JobCell>();
        string searchBarText;

        public bool APNSset = false;
        public int APNSattemptLimit = 5;
        public int APNSattemptDurationSeconds = 20;
        public int APNSattempts = 0;
        
      

        public main(Connection cnx)
        {
            InitializeComponent();
            if (firstLoad == true)
            {
                searchBarText = "";
            }

            //updateList(cnx, null);
            Maincnx = cnx;
            SendFirebaseToken();
            firstLoad = false;
            AppleCheck();
            ConnectionCheck();

            passwordButton.Clicked += PasswordButton_Clicked;
        }


        private void AppleCheck()
        {
            if (Device.RuntimePlatform == Device.iOS)
            {
                materialsButton.CornerRadius = 25;
                settingsButton.CornerRadius = 25;                
            }
        }

        public async void RetryFirebase()
        {
            await Task.Delay(TimeSpan.FromSeconds(APNSattemptDurationSeconds));
            SendFirebaseToken();
        }

        private async void SendFirebaseToken()
        {
            
#if __ANDROID__

            if (Device.RuntimePlatform == Device.Android)
            {
                try
                {
                    Maincnx.FirebaseToken = FirebaseInstanceId.Instance.Token;
                    Cmd cmd = new Cmd()
                    {
                        Command = "firebase"
                    };

                    User user = new User()
                    {
                        Token = Maincnx.Token,
                        Id = Maincnx.ID,
                        Md = Maincnx.TokenInfoJsonProps.Md,
                        Name = Maincnx.Name,
                        Email = Maincnx.TokenInfoJsonProps.Email,
                        Error = "",
                        Temp = ""
                    };
                    FirebaseProp firebase = new FirebaseProp()
                    {
                        Token = Maincnx.FirebaseToken,
                        Device = "android"
                    };

                    FirebaseJson outgoingobject = new FirebaseJson()
                    {
                        Cmd = cmd,
                        User = user,
                        Firebase = firebase
                    };

                    var x = outgoingobject;
                    string outgoingJson = outgoingobject.ToJson();

                    Maincnx.FirebaseObject = outgoingJson;
                    string key = $"/api/api/5?id=";
                    var response = await Client.Post(Maincnx.Token, key, outgoingJson);
                    var y = response;
                }
                catch (Exception e)
                {
                    LoginPage.SendError("M01", "Exception caught when trying to send server Firebase Notification Token", e.Message);
                    await DisplayAlert("Error", "Error sending Notification Token\n\nError Code: M01", "Ok");
                }
            }
#endif


            if (Device.RuntimePlatform == Device.iOS)
            {
                try
                {
                    var appleToken = await SecureStorage.GetAsync("APNStemp");               
                    if (!string.IsNullOrWhiteSpace(appleToken))
                    {
                        appleToken = appleToken.Replace(" ", "");
                        await SecureStorage.SetAsync("APNS", appleToken);
                        Maincnx.APNSToken = appleToken;
                        APNSset = true;
                    }
                    else
                    {
                        appleToken = await SecureStorage.GetAsync("APNS");
                        if (!string.IsNullOrWhiteSpace(appleToken))
                        {
                            Maincnx.APNSToken = appleToken;
                        }
                      
                    }

                    if (!string.IsNullOrWhiteSpace(Maincnx.APNSToken))
                    {
                        if (APNSset || APNSattempts == APNSattemptLimit)
                        {
                            Cmd cmd = new Cmd()
                            {
                                Command = "firebase"
                            };

                            User user = new User()
                            {
                                Token = Maincnx.Token,
                                Id = Maincnx.ID,
                                Md = Maincnx.TokenInfoJsonProps.Md,
                                Name = Maincnx.Name,
                                Email = Maincnx.TokenInfoJsonProps.Email,
                                Error = "",
                                Temp = ""
                            };
                            FirebaseProp firebase = new FirebaseProp()
                            {
                                Token = Maincnx.APNSToken,
                                Device = "iOS"
                            };

                            FirebaseJson outgoingobject = new FirebaseJson()
                            {
                                Cmd = cmd,
                                User = user,
                                Firebase = firebase
                            };

                            var x = outgoingobject;
                            string outgoingJson = outgoingobject.ToJson();

                            Maincnx.FirebaseObject = outgoingJson;
                            string key = $"/api/api/5?id=";
                            var response = await Client.Post(Maincnx.Token, key, outgoingJson);
                            var y = response;

                        }
                    }
                }
                catch (Exception e)
                {
                    LoginPage.SendError("M28", "Exception occured when sending Server APNS token.", e.Message);
                    await DisplayAlert("Error", "Error sending Notification Token\n\nError Code: M28", "Ok");
                    APNSset = true; //stops the error from looping
                }

                if (APNSset == false)
                {
                    if (APNSattempts < APNSattemptLimit)
                    {
                        RetryFirebase();
                    }
                    else
                    {
                        LoginPage.SendError("M29", "APNS attempt limit reached (timeout).", $"Limit at: {APNSattemptLimit}"); 
                    }
                }

            }
        }



        private async void PasswordButton_Clicked(object sender, EventArgs e)
        {
            ChangePassword changePassword = new ChangePassword(Maincnx.TokenInfoJsonProps);
            await Navigation.PushModalAsync(changePassword);
            MessagingCenter.Subscribe<ChangePassword, Connection>(changePassword, "sendcnx", (_sender, arg) =>
            {
                Maincnx = arg;
            });
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

        public async Task<string> GetJob(string key, string token)
        {
            ConnectionCheck();
            if (connected == true)
            {

                var response = await Client.GET(token, key);
                if (response == "errorerrorerror")
                {
                    LoginPage.SendError("M02", "Http GET request error on Client.GET() call - TASK<string> GetJob()");
                    await DisplayAlert("Http Request Error", "Please try again.\n\nError Code: M02\n\nIf this keeps happening, please contact us.", "Ok");
                    return null;
                }
                //string auth = "Bearer " + token;
                //var nsAPI = RestService.For<IMakeUpApi>("https://proq.remotewebaccess.com:58330");
                //var response = await nsAPI.GetKey(key, auth);
                return response;
            }
            return null;
        }


        async Task<List<Job>> GetContent(int listStart, int listEnd, Connection cnx)
        {

            string jobKey = String.Format("/api/api/5?id={0}$~{1}~cmd$~getjob~{2}~{3}", cnx.MD, cnx.ID, listStart, listEnd);

            string jsonRaw = "";


            jsonRaw = await Client.GET(cnx.Token, jobKey);
            if (jsonRaw == "errorerrorerror")
            {
                LoginPage.SendError("M03", "Http GET request error on Client.GET() call - TASK<List<Job>> GetContent()");
                await DisplayAlert("Http Request Error", "Please try again.\n\nError Code: M03\n\nIf this keeps happening, please contact us.", "Ok");

                //make sure that if you do return null here it doesnt break the app.
                return null;
            }




            var x = JobsFromJson.FromJson(jsonRaw);
            List<Job> Jobs = JsonDictionaryToJob(x);

            return Jobs;
        }

        public static List<Job> JsonDictionaryToJob(List<Dictionary<string, string>> x)
        {
            List<Job> Jobs = new List<Job>();


            foreach (Dictionary<string, string> i in x)
            {
                Job job = new Job();

                string jobnumber = i["job"];
                string subjobnumber = i["subjob"];
                string status = i["status"];
                string created = i["created"];
                string add1 = i["add1"];
                string add2 = i["add2"];
                string add3 = i["add3"];
                string add4 = i["add4"];
                string addpc = i["addpc"];
                string awarded = i["awarded"];
                string buildername = i["buildername"];
                string netvalue = i["netValue"];
                string grossvalue = i["grossValue"];
                string vatvalue = i["vatValue"];
                string PO = i["ponumber"];
                

                if (netvalue == null || netvalue == "")
                {
                    netvalue = "0.00";
                }

                if (grossvalue == null || grossvalue == "")
                {
                    grossvalue = "0.00";
                }

                if (vatvalue == null || vatvalue == "")
                {
                    vatvalue = "0.00";
                }

                job.job = Convert.ToInt32(jobnumber);
                job.subjob = Convert.ToInt32(subjobnumber);
                job.status = status;
                job.created = Convert.ToDateTime(created, System.Globalization.CultureInfo.CreateSpecificCulture("en-GB"));
                job.add1 = add1;
                job.add2 = add2;
                job.add3 = add3;
                job.add4 = add4;
                job.addpc = addpc;
                job.awarded = awarded;
                job.buildername = buildername;
                job.netValue = Convert.ToDouble(netvalue);
                job.grossValue = Convert.ToDouble(grossvalue);
                job.vatValue = Convert.ToDouble(vatvalue);
                job.PO = PO;

                Jobs.Add(job);

            }

            return Jobs;
        }

        async Task updateList(Connection cnx, List<JobCell> JOBCELLS)
        {
            List<Log> logs = new List<Log>();
            Log log = new Log() { LogLog = $"[MOBILE]: {cnx.User} called UpdateList()", Datetime = $"{DateTime.Now:r}" };
            logs.Add(log);
            

            this.BarBackgroundColor = Color.FromHex("#B80000");
            settingsButton.BackgroundColor = Color.FromHex("#B80000");
            materialsButton.BackgroundColor = Color.FromHex("#B80000");
            contactButton.BackgroundColor = Color.FromHex("#B80000");
            passwordButton.BackgroundColor = Color.FromHex("#B80000");

            bool isMerchant = false;
            if (Maincnx.MD == "md")
            {
                isMerchant = true;
                settingsButton.IsVisible = false;
                settingsButton.IsEnabled = false;
                AbsoluteLayout.SetLayoutBounds(materialsButton, new Rectangle(.5,.5,.75,.09));
            }

            List<Job> Jobs = new List<Job>(); ;
            List<JobCell> Cells = new List<JobCell>();


            if (JOBCELLS == null)
            {
                JobAmounts jobamounts = await GetAmounts(cnx);
                if (!string.IsNullOrEmpty(jobamounts.jobs))
                {
                    int endNumber = Int32.Parse(jobamounts.jobs);
                    Jobs = await GetContent(0, endNumber, cnx);

                    //if (Jobs == null)
                    //{
                    //    LoginPage.SendError("M04", "Jobs list still is null after GetContent() call.", ex.Message);
                    //    await DisplayAlert("Error", "There has been an issue retreiving your jobs. Please try again.\n\nError Code: M04\n\nIf this keeps happening please restart the app.", "Ok");
                    //    return;
                    //}
                }
                else
                {
                    LoginPage.SendError("M05", "Get job amounts request is coming back as null or empty.");
                    await DisplayAlert("Error", "There has been an issue retreiving your job count. Please try again.\n\nError Code: M05\n\nIf this keeps happening please restart the app.", "Ok");
                }

                foreach (Job job in Jobs)
                {
                    JobCell cell = new JobCell
                    {
                        JobNumber = job.job.ToString(),
                        Add1 = job.add1,
                        
                        JobColor = StatusSorter.JobNumberColor(job),
                        Status = StatusSorter.StatusText(job),
                        StatusColor = StatusSorter.StatusColor(job),
                        CellColor = StatusSorter.CellColor(job),
                        Builder = job.buildername,
                        PO = job.PO,
                        job = job
                    };
                    Cells.Add(cell);

                    _Cells = Cells;

                }

            }
            else
            {
                Cells = JOBCELLS;
            }

            




            Label jobHeader = new Label
            {
                Text = Maincnx.Name,
                TextColor = Color.Black,
                FontSize = Device.GetNamedSize(NamedSize.Large, typeof(Label)),
                FontAttributes = FontAttributes.Bold,
                HorizontalOptions = LayoutOptions.Center,
                HorizontalTextAlignment = TextAlignment.Center
            };

            if (Device.RuntimePlatform == Device.iOS)
            {
                searchbar = new SearchBar()
                {
                    Placeholder = "Search:",
                    Text = searchBarText,
                    CancelButtonColor = Color.Red,
                    HorizontalOptions = LayoutOptions.CenterAndExpand,
                    Margin = new Thickness(0,0,0,0)
                };
            }
            else
            {
                searchbar = new SearchBar()
                {
                    Placeholder = "Search:",
                    Text = searchBarText,
                    CancelButtonColor = Color.Red
                };
            }
            
            searchbar.SearchButtonPressed += SearchBarButtonPressedChanged;
            searchbar.TextChanged += Searchbar_TextChanged;

            listView = new ListView()
            {
                HasUnevenRows = true,
                BackgroundColor = Color.White,
                ItemsSource = Cells,

                ItemTemplate = new DataTemplate(() =>
                {
                    //create views with bindings for displaying each property.
                    Label JobNumber = new Label()
                    {
                        FontSize = Device.GetNamedSize(NamedSize.Medium, typeof(Label)),
                        FontAttributes = FontAttributes.Bold
                    };
                    JobNumber.SetBinding(Label.TextProperty, "JobNumber");
                    JobNumber.SetBinding(Label.TextColorProperty, "JobColor");

                    Label PoNumber = new Label()
                    {
                        FontSize = Device.GetNamedSize(NamedSize.Small, typeof(Label)),
                        FontAttributes = FontAttributes.Bold
                    };
                    PoNumber.SetBinding(Label.TextProperty, "PO");
                    PoNumber.HorizontalOptions = LayoutOptions.EndAndExpand;

                    Label Builder = new Label()
                    {
                        FontSize = Device.GetNamedSize(NamedSize.Small, typeof(Label)),
                        FontAttributes = FontAttributes.None
                    };
                    Builder.SetBinding(Label.TextProperty, "Builder");
                    Builder.HorizontalOptions = LayoutOptions.EndAndExpand;



                    Label JobAddress = new Label()
                    {
                        FontSize = Device.GetNamedSize(NamedSize.Small, typeof(Label)),
                        FontAttributes = FontAttributes.Italic
                    };
                    JobAddress.SetBinding(Label.TextProperty, "Add1");
                    JobAddress.TextColor = Color.Black;

                    Label Status = new Label();
                    Status.SetBinding(Label.TextProperty, "Status");
                    Status.SetBinding(Label.TextColorProperty, "StatusColor");


                    if (isMerchant)
                    {
                        if (Device.RuntimePlatform == Device.iOS)
                        {
                            return new ViewCell
                            {
                                View = new StackLayout
                                {
                                    Padding = new Thickness(10, 15),
                                    //Margin = new Thickness(15,0),
                                    Orientation = StackOrientation.Horizontal,
                                    Children =
                                    {
                                        JobNumber,
                                        new StackLayout
                                        {
                                            VerticalOptions = LayoutOptions.CenterAndExpand,
                                            Spacing = 0,
                                            Children =
                                            {
                                                JobAddress,
                                                Status
                                            }
                                        },
                                        new StackLayout
                                        {
                                            Orientation = StackOrientation.Vertical,
                                            VerticalOptions = LayoutOptions.CenterAndExpand,
                                            HorizontalOptions = LayoutOptions.EndAndExpand,
                                            Spacing = 0,
                                            Margin = 0,
                                            Children =
                                            {
                                                PoNumber,
                                                Builder
                                            }
                                        }
                                    }
                                }
                            };
                        }
                        else
                        {
                            return new ViewCell
                            {
                                View = new StackLayout
                                {
                                    Padding = new Thickness(0, 15),
                                    Orientation = StackOrientation.Horizontal,
                                    Children =
                                    {
                                        JobNumber,
                                        new StackLayout
                                        {
                                            VerticalOptions = LayoutOptions.CenterAndExpand,
                                            Spacing = 0,
                                            Children =
                                            {
                                                JobAddress,
                                                Status
                                            }
                                        },
                                        new StackLayout
                                        {
                                            Orientation = StackOrientation.Vertical,
                                            VerticalOptions = LayoutOptions.CenterAndExpand,
                                            HorizontalOptions = LayoutOptions.EndAndExpand,
                                            Spacing = 0,
                                            Margin = 0,
                                            Children =
                                            {
                                                PoNumber,
                                                Builder
                                            }
                                        }
                                    }
                                }
                            };
                        }                       
                    }
                    else
                    {
                        if (Device.RuntimePlatform == Device.iOS)
                        {
                            return new ViewCell
                            {
                                View = new StackLayout
                                {
                                    Padding = new Thickness(10, 5),
                                    //Margin = new Thickness(15, 0),
                                    Orientation = StackOrientation.Horizontal,
                                    Children =
                                    {
                                        JobNumber,
                                        new StackLayout
                                        {
                                            VerticalOptions = LayoutOptions.CenterAndExpand,
                                            Spacing = 0,
                                            Children =
                                            {
                                                JobAddress,
                                                Status
                                            }
                                        }
                                    }
                                }
                            };
                        }
                        else
                        {
                            return new ViewCell
                            {
                                View = new StackLayout
                                {
                                    Padding = new Thickness(0, 5),
                                    Orientation = StackOrientation.Horizontal,
                                    Children =
                                    {
                                        JobNumber,
                                        new StackLayout
                                        {
                                            VerticalOptions = LayoutOptions.CenterAndExpand,
                                            Spacing = 0,
                                            Children =
                                            {
                                                JobAddress,
                                                Status
                                            }
                                        }
                                    }
                                }
                            };
                        }
                    }
                })
            };

            listView.ItemSelected += ListView_ItemSelected;

            //this.Padding = new Thickness(10, 20, 10, 5);

            //if (Device.RuntimePlatform == Device.iOS)
            //{
            //    jobHeader.Margin = new Thickness(0, 10, 0, 3);
            //    Tab1.Content = new StackLayout
            //    {
            //        HorizontalOptions = LayoutOptions.CenterAndExpand,
            //        Children =
            //        {
            //            jobHeader,
            //            searchbar,
            //            listView,
            //        }
            //    };
            //}

            

            if (Device.RuntimePlatform == Device.iOS)
            {
                Tab1.Content = new AbsoluteLayout()
                {
                    HorizontalOptions = LayoutOptions.CenterAndExpand,
                    Children =
                    {
                        jobHeader,
                        searchbar,
                        listView,
                    }
                };

                //jobHeader.BackgroundColor = Color.BurlyWood;
                //searchbar.BackgroundColor = Color.CadetBlue;
                //listView.BackgroundColor = Color.DarkGray;

                jobHeader.VerticalTextAlignment = TextAlignment.Center;

                AbsoluteLayout.SetLayoutFlags(jobHeader, AbsoluteLayoutFlags.All);
                AbsoluteLayout.SetLayoutFlags(searchbar, AbsoluteLayoutFlags.All);
                AbsoluteLayout.SetLayoutFlags(listView, AbsoluteLayoutFlags.All);

                AbsoluteLayout.SetLayoutBounds(jobHeader, new Rectangle(.5, .02, .98, .1));
                AbsoluteLayout.SetLayoutBounds(searchbar, new Rectangle(.5, .13, .98, .075));
                AbsoluteLayout.SetLayoutBounds(listView, new Rectangle(.5, .9, .98, .78));

            }
            else
            {
                Tab1.Content = new StackLayout
                {
                    Children =
                    {
                        jobHeader,
                        searchbar,
                        listView,
                    }
                };
            }

            if (Device.RuntimePlatform == Device.iOS)
            {
                listView.IsPullToRefreshEnabled = true;
                listView.RefreshCommand = RefreshCommand;
            }


            // on first load bool set false, capture content then set true. COMPARE;
            var x = Tab1.Content;
        }

        public ICommand RefreshCommand
        {
            get
            {
                return new Command(async () =>
                {
                    IsBusy = true;
                    searchbar.Text = "";
                    await updateList(Maincnx, null);

                    IsBusy = false;
                });
            }
        }


        public List<JobCell> FormListFromText(string Text)
        {
         
            //Could also index origional list of jobs and use that to decide which to display, instead of creating another temp list.

            if (!string.IsNullOrEmpty(Text) && !string.IsNullOrWhiteSpace(Text))
            {

                string _text = "";

                try
                {
                    searchBarText = Text;
                    _text = Text.ToLower();

                }
                catch (NullReferenceException ex)
                {
                    Console.WriteLine("CAUGHT THE EXCEPTION 1");
                    LoginPage.SendError("M12", "Search List Parse Error part 1", ex.Message);
                }

                int indexer = 0;
                List<int> indexerList = new List<int>();

                List<JobCell> List = new List<JobCell>();

                foreach (JobCell cell in _Cells)
                {
                    bool addToList = false;
                    JobCell x = cell;
                    if (!string.IsNullOrEmpty(x.Add1) && !string.IsNullOrWhiteSpace(x.Add1))
                    {
                        indexer++;
                        string y = "";
                        try
                        {
                            y = x.Add1.ToLower();
                        }
                        catch (NullReferenceException ex)
                        {
                            LoginPage.SendError("M13", "Search List Parse Error part 2", ex.Message);
                            indexerList.Add(indexer);
                            Console.WriteLine("CAUGHT AT " + indexer);
                        }
                        Console.WriteLine(indexer + "     " + "Add1" + "     " + y + "\n");


                        if (y.Contains(_text))
                        {
                            addToList = true;
                        }
                    }

                    if (!string.IsNullOrEmpty(x.Add2) && !string.IsNullOrWhiteSpace(x.Add2))
                    {
                        indexer++;
                        string y = "";
                        try
                        {
                            y = x.Add2.ToLower();
                        }
                        catch (NullReferenceException ex)
                        {
                            LoginPage.SendError("M14", "Search List Parse Error part 3", ex.Message);
                            indexerList.Add(indexer);
                            Console.WriteLine("CAUGHT AT " + indexer);
                        }
                        Console.WriteLine(indexer + "     " + "Add1" + "     " + y + "\n");


                        if (y.Contains(_text))
                        {
                            addToList = true;
                        }
                    }

                    if (!string.IsNullOrEmpty(x.Add3) && !string.IsNullOrWhiteSpace(x.Add3))
                    {
                        indexer++;
                        string y = "";
                        try
                        {
                            y = x.Add3.ToLower();
                        }
                        catch (NullReferenceException ex)
                        {
                            LoginPage.SendError("M15", "Search List Parse Error part 4", ex.Message);
                            indexerList.Add(indexer);
                            Console.WriteLine("CAUGHT AT " + indexer);
                        }
                        Console.WriteLine(indexer + "     " + "Add1" + "     " + y + "\n");


                        if (y.Contains(_text))
                        {
                            addToList = true;
                        }
                    }

                    if (!string.IsNullOrEmpty(x.Add4) && !string.IsNullOrWhiteSpace(x.Add4))
                    {
                        indexer++;
                        string y = "";
                        try
                        {
                            y = x.Add4.ToLower();
                        }
                        catch (NullReferenceException ex)
                        {
                            LoginPage.SendError("M16", "Search List Parse Error part 5", ex.Message);
                            indexerList.Add(indexer);
                            Console.WriteLine("CAUGHT AT " + indexer);
                        }
                        Console.WriteLine(indexer + "     " + "Add1" + "     " + y + "\n");


                        if (y.Contains(_text))
                        {
                            addToList = true;
                        }
                    }

                    if (!string.IsNullOrEmpty(x.AddPC) && !string.IsNullOrWhiteSpace(x.AddPC))
                    {
                        indexer++;
                        string y = "";
                        try
                        {
                            y = x.AddPC.ToLower();
                        }
                        catch (NullReferenceException ex)
                        {
                            LoginPage.SendError("M17", "Search List Parse Error part 6", ex.Message);
                            indexerList.Add(indexer);
                            Console.WriteLine("CAUGHT AT " + indexer);
                        }
                        Console.WriteLine(indexer + "     " + "Add1" + "     " + y + "\n");


                        if (y.Contains(_text))
                        {
                            addToList = true;
                        }
                    }

                    if (!string.IsNullOrEmpty(x.Description) && !string.IsNullOrWhiteSpace(x.Description))
                    {
                        indexer++;
                        string y = "";
                        try
                        {
                            y = x.Description.ToLower();
                        }
                        catch (NullReferenceException ex)
                        {
                            LoginPage.SendError("M18", "Search List Parse Error part 7", ex.Message);
                            indexerList.Add(indexer);
                            Console.WriteLine("CAUGHT AT " + indexer);
                        }
                        Console.WriteLine(indexer + "     " + "Add1" + "     " + y + "\n");


                        if (y.Contains(_text))
                        {
                            addToList = true;
                        }
                    }

                    if (!string.IsNullOrEmpty(x.JobNumber) && !string.IsNullOrWhiteSpace(x.JobNumber))
                    {
                        indexer++;
                        string y = "";
                        try
                        {
                            y = x.JobNumber.ToLower();
                        }
                        catch (NullReferenceException ex)
                        {
                            LoginPage.SendError("M19", "Search List Parse Error part 8", ex.Message);
                            indexerList.Add(indexer);
                            Console.WriteLine("CAUGHT AT " + indexer);
                        }
                        Console.WriteLine(indexer + "     " + "Add1" + "     " + y + "\n");


                        if (y.Contains(_text))
                        {
                            addToList = true;
                        }
                    }

                    if (Maincnx.MD == "md")
                    {
                        if (!string.IsNullOrEmpty(x.PO) && !string.IsNullOrWhiteSpace(x.PO))
                        {
                            indexer++;
                            string y = "";

                            try
                            {
                                y = x.Builder.ToLower();
                            }
                            catch (NullReferenceException ex)
                            {
                                LoginPage.SendError("M20", "Search List Parse Error part 9", ex.Message);
                                indexerList.Add(indexer);
                                Console.WriteLine("CAUGHT AT " + indexer);
                            }
                            Console.WriteLine(indexer + "     " + "Add1" + "     " + y + "\n");


                            if (y.Contains(_text))
                            {
                                addToList = true;
                            }
                        }

                        if (!string.IsNullOrEmpty(x.Builder) && !string.IsNullOrWhiteSpace(x.Builder))
                        {
                            indexer++;
                            string y = x.PO;


                            if (y.Contains(_text) && _text.Length > 5)
                            {
                                addToList = true;
                            }
                        }
                    }
                   
                    if (addToList == true)
                    {
                        try
                        {
                            List.Add(cell);
                        }
                        catch (NullReferenceException ex)
                        {
                            LoginPage.SendError("M21", "Search List Parse Error part 10 - adding to cell", ex.Message);
                            Console.WriteLine("Found Putting in Cell");
                        }

                    }
                }

                var osf = List;
                return List;

            }
            else
            {
                updateList(Maincnx, null);
                return null;
            }
        }

        private async void Searchbar_TextChanged(object sender, TextChangedEventArgs e)
        {
            if(e.NewTextValue == "" && e.OldTextValue.Length > 0)
            {
                if(searchBarRefreshed == false)
                {
                    searchBarRefreshed = true;
                    searchBarText = "";
                    await updateList(Maincnx, null);
                    searchbar.Focus();
                }
            }

            if (e.NewTextValue.Length > 0)
            {
                searchBarRefreshed = false;
            }
        }

        public void SearchBarButtonPressedChanged (object sender, EventArgs e)
        {
            SearchBar SearchBarSender = sender as SearchBar;
            string Text = SearchBarSender.Text;
            List<JobCell> List = FormListFromText(Text);

            if(List != null && List.Count > 0)
            {
                listView.ItemsSource = List;
            }
            else
            {
                updateList(Maincnx, null);
            }
        }

        private async Task<JobAmounts> GetAmounts(Connection cnx)
        {
            string key = string.Format("/api/api/5?id={0}$~{1}~cmd$~getjobnums", cnx.MD ,cnx.ID);
            var jsonRaw = await Client.GET(cnx.Token, key);
            if (jsonRaw == "errorerrorerror")
            {
                LoginPage.SendError("M06", "Http GET request error on Client.GET() call. - Task<JobAmounts> GetAmounts()");
                await DisplayAlert("Http Request Error", "Please try again.\n\nError Code: M06\n\nIf this keeps happening, please contact us.", "Ok");
                //Make sure that if you return null here it doesnt break the app.
                return null;
            }

            JobAmounts jobAmounts = JobAmounts.FromJson(jsonRaw);
            return jobAmounts;

        }


        public async void ListView_ItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            var cnx = Maincnx;

            var item = e.SelectedItem as JobCell;

            Job _item = new Job();
            _item = item.job;


            string jobKey = String.Format("/api/api/5?id={0}$~{1}~cmd$~getjobdetails~{2}", cnx.MD , cnx.ID, item.JobNumber);

            //List<MegaParser.groupitem> groupitems = MegaParser.ParseJobs(await GetJob(jobKey, cnx.Token));

            string rawJson = await GetJob(jobKey, cnx.Token);
            if (connected == false) return;

            if (rawJson == "no jobs" || string.IsNullOrEmpty(rawJson))
            {
                await DisplayAlert("No Info", "No Further Information", "ok");
                Console.WriteLine("RETURNED: no jobs");
                updateList(Maincnx, null);
            }
            else if (rawJson[0] != '[')
            {
                await DisplayAlert("No Info", "No Further Information", "ok");
                Console.WriteLine("RETURNED: Improper Format");
                updateList(Maincnx, null);
            }
            else
            {
                var x = JobsFromJson.FromJson(rawJson);

                List<SubJob> _subJobs = new List<SubJob>();

                foreach (Dictionary<string, string> i in x)
                {
                    SubJob sj = new SubJob();

                    if (i["subjob"] == "")
                    {
                        sj.Subjob = 0;
                    }
                    else
                    {
                        sj.Subjob = Convert.ToInt32(i["subjob"]);
                    }

                    if (i["esent"] == "")
                    {
                        sj.Sent = 0;
                    }
                    else
                    {
                        sj.Sent = Convert.ToInt32(i["esent"]);
                    }

                    sj.Status = i["status"];
                    sj.Created = i["created"];
                    sj.Description = i["description"];
                    sj.GrossValue = i["grossValue"];
                    sj.NetValue = i["netValue"];
                    sj.VatValue = i["vatValue"];
                    sj.Notes = i["notes"];
                    sj.PO = i["ponumber"];

                    _subJobs.Add(sj);



                }

                List<Job> subjobs = new List<Job>();

                foreach (SubJob _sj in _subJobs)
                {

                    Job job = new Job();
                    if (_sj.Description == "none")
                    {
                        _sj.Description = "";
                    }

                    if (_sj.GrossValue == null || _sj.GrossValue == "")
                    {
                        if (_item.grossValue == null || _item.grossValue == 0)
                        {
                            _sj.GrossValue = "0.00";
                            job.grossValue = Convert.ToDouble(_sj.GrossValue);
                        }
                        else if (_sj.Subjob == 0)
                        {
                            job.grossValue = _item.grossValue;
                        }
                    }
                    else
                    {
                        job.grossValue = Convert.ToDouble(_sj.GrossValue);
                    }

                    if (_sj.NetValue == null || _sj.NetValue == "")
                    {
                        if (_item.netValue == null || _item.netValue == 0)
                        {
                            _sj.NetValue = "0.00";
                            job.netValue = Convert.ToDouble(_sj.NetValue);
                        }
                        else if (_sj.Subjob == 0)
                        {
                            job.netValue = _item.netValue;
                        }
                    }
                    else
                    {
                        job.netValue = Convert.ToDouble(_sj.NetValue);
                    }

                    if (_sj.VatValue == null || _sj.VatValue == "")
                    {
                        if (_item.vatValue == null || _item.vatValue == 0)
                        {
                            _sj.VatValue = "0.00";
                            job.vatValue = Convert.ToDouble(_sj.VatValue);
                        }
                        else if (_sj.Subjob == 0)
                        {
                            job.vatValue = _item.vatValue;
                        }
                    }
                    else
                    {
                        job.vatValue = Convert.ToDouble(_sj.VatValue);
                    }



                    job.job = _item.job;
                    job.subjob = _sj.Subjob;
                    job.status = _sj.Status;
                    job.created = Convert.ToDateTime(_sj.Created, System.Globalization.CultureInfo.CreateSpecificCulture("en-GB"));
                    job.add1 = _item.add1;
                    job.add2 = _item.add2;
                    job.add3 = _item.add3;
                    job.add4 = _item.add4;
                    job.addpc = _item.addpc;
                    job.awarded = _item.awarded;
                    job.buildername = _item.buildername;
                    job.description = _sj.Description;
                    job.sentcount = _sj.Sent;
                    job.Notes = _sj.Notes;
                    job.PO = _sj.PO;

                    subjobs.Add(job);
                }


                if (subjobs.Count == 1)
                {
                    JobCell _job = new JobCell
                    {
                        JobColor = StatusSorter.JobNumberColor(subjobs[0]),
                        Status = StatusSorter.StatusText(subjobs[0]),
                        StatusColor = StatusSorter.StatusColor(subjobs[0]),
                        CellColor = StatusSorter.CellColor(subjobs[0]),
                        Add1 = subjobs[0].add1,
                        Add2 = subjobs[0].add2,
                        Add3 = subjobs[0].add3,
                        Add4 = subjobs[0].add4,
                        AddPC = subjobs[0].addpc,
                        Awarded = subjobs[0].awarded,
                        Builder = subjobs[0].buildername,
                        Created = subjobs[0].created,
                        Description = subjobs[0].description,
                        JobNumber = subjobs[0].job.ToString(),
                        SentCount = subjobs[0].sentcount,
                        GrossValue = subjobs[0].grossValue,
                        NetValue = subjobs[0].netValue,
                        VatValue = subjobs[0].vatValue,
                        Notes = subjobs[0].Notes,
                        noSubs = true,

                        job = _item
                    };


                    await Navigation.PushAsync(new JobSpecific(cnx, _job, false));      
                }
                else
                {
                    await Navigation.PushAsync(new Subjobs_List(cnx, subjobs, _item));
                }
            }
        }


        private async void ContactUsButtonClicked(object sender, EventArgs e)
        {
            string phoneNumber = await LoginPage.GetPhoneNumber();

            try
            {
                //CHANGE TO REQUEST NUMBER
                PhoneDialer.Open(phoneNumber);
            }
            catch (FeatureNotSupportedException ex)
            {
                LoginPage.SendError("M07", "Dialer feature not supported. Application can't access dialer", ex.Message);
                await DisplayAlert("ERROR", "Dialer feature not supported.\n\nError Code: M07", "OK");
                return;

            }
            catch (Exception ex)
            {
                LoginPage.SendError("M22", "Phone Dialer Unkown Exception", ex.Message);
                return;
            }
        }

        

        private async void LogOutButtonClicked(object sender, EventArgs e)
        {
            //Warning Message
            bool logout = await DisplayAlert("Logout", "Do you wish to logout?", "Yes", "No");

            if(logout == true)
            {
                //is it id dependant?
                try
                {
                    SecureStorage.RemoveAll();
                }
                catch (Exception ex)
                {
                    LoginPage.SendError("M23", "Exception thrown during logout - SecureStorage.RemoveAll()", ex.Message);
                    Console.WriteLine(ex.Message);
                }
                string key = $"/api/api/5?id=id$~{Maincnx.ID}~cmd$~logout";

                string response = await Client.GET(Maincnx.Token, key);
                if(response == "errorerrorerror")
                {
                    LoginPage.SendError("M08", "Http GET request error on Client.GET() call. - Logout request.");
                    await Logoff();
                    return;
                }

                
                if (response.Contains("error unautherised user"))
                {
                    LoginPage.SendError("M09", "Logout request has returned unauthorised user error, (See Olly)");
                    await Logoff();
                }
                else
                {
                    await Logoff();
                }
            }
        }

        private async Task Logoff()
        {
            LoginPage.loggedin = false;
            //CHECK THAT THIS ACTUALLY WORKS
            await DisplayAlert("Logged Off", "You have been logged off", "Ok");
            await Navigation.PopAsync();
        }

        private async void RefreshClicked(object sender, EventArgs e)
        {
            searchbar.Text = "";
            await updateList(Maincnx, null);
        }

        protected override void OnAppearing()
        {
            if (firstLoad == false)
            {
                updateList(Maincnx, null);
            }

            base.OnAppearing();
        }

        private async void SettingsClicked(object sender, EventArgs e)
        {
            //Get Settings


            //NOT FOR MERCHANTS!!!
            string key = $"/api/api/5?id={Maincnx.MD}$~{Maincnx.ID}~cmd$~getestimatingsettings";
            var response = await Client.GET(Maincnx.Token, key);
            if (response == "errorerrorerror")
            {
                LoginPage.SendError("M10", "Http GET request error on Client.GET() call. - estimator settings");
                await DisplayAlert("Error", "There has been an error contacting our server, please try again.\n\nError Code: M10\n\nIf this keeps happening please restart the app.", "Ok");
                return;
            }

            if (string.IsNullOrEmpty(response))
            {
                LoginPage.SendError("M27", "Estimating settings response = null or empty", response);
                await DisplayAlert("Error", "There has been an error contacting our server, please try again.\n\nError Code: M27\n\nIf this keeps happening please restart the app.", "Ok");
                return;
            }

            try
            {
                SettingsObject[] receivedEstSettings = SettingsObject.FromJson(response);
                List<SettingsObject> EstSettings = new List<SettingsObject>();

                foreach(SettingsObject setting in receivedEstSettings)
                {
                    EstSettings.Add(setting);
                }


                await Navigation.PushAsync(new Settings(Maincnx, EstSettings, "Estimating Settings"));
            }
            catch (Exception ex)
            {
                LoginPage.SendError("M25", "Error Occured putting setting response into List<SettingObject>", ex.Message);
                await DisplayAlert("Error", "An Error has occured, please try again\n\nError Code: M25", "Ok");
                return;
            }

            //Go To Settings Page as a navigation page with list.

        }

        private async void MaterialsClicked(object sender, EventArgs e)
        {
            //Get Materials
            string key = $"/api/api/5?id={Maincnx.MD}$~{Maincnx.ID}~cmd$~getmaterialsettings";
            var response = await Client.GET(Maincnx.Token, key);
            if (response == "errorerrorerror")
            {
                LoginPage.SendError("M11", "Http GET request error on Client.GET() call. - material settings");
                await DisplayAlert("Error", "There has been an error contacting our server, please try again.\n\nError Code: M11\n\nIf this keeps happening please restart the app.", "Ok");
                return;
            }

            if (string.IsNullOrEmpty(response))
            {
                LoginPage.SendError("M26", "Material settings response = null or empty", response);
                await DisplayAlert("Error", "There has been an error contacting our server, please try again.\n\nError Code: M26\n\nIf this keeps happening please restart the app.", "Ok");
                return;
            }

            try
            {
                SettingsObject[] receivedMatSettings = SettingsObject.FromJson(response);
                List<SettingsObject> MatSettings = new List<SettingsObject>();

                foreach (SettingsObject setting in receivedMatSettings)
                {
                    MatSettings.Add(setting);
                }

                await Navigation.PushAsync(new Settings(Maincnx, MatSettings, "Material Settings"));

            }
            catch (Exception ex)
            {
                LoginPage.SendError("M24", "Error Occured putting setting response into List<SettingObject>", ex.Message);
                await DisplayAlert("Error", "An Error has occured, please try again\n\nError Code: M24", "Ok");
                return;
            }
        }

        protected override bool OnBackButtonPressed()
        {
            if (LoginPage.loggedin)
            {
                return true;
            }
            return base.OnBackButtonPressed();
        }
    }
}