using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Xamarin.Essentials;

using Refit;
using Newtonsoft.Json;

namespace ProQuant
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class main : TabbedPage
    {
        bool connected = false;
        public Connection Maincnx;
        public bool firstLoad = true;
        SearchBar searchbar;
        List<JobCell> _Cells = new List<JobCell>();
        string searchBarText;






        public main(Connection cnx)
        {
            InitializeComponent();
            if (firstLoad == true)
            {
                searchBarText = "";
            }
            updateList(cnx, null);
            Maincnx = cnx;
            firstLoad = false;
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

        public async Task<string> GetJob(string key, string token)
        {
            ConnectionCheck();
            if (connected == true)
            {

                var response = await Client.GET(token, key);
                if (response == "errorerrorerror")
                {
                    await DisplayAlert("Http Request Error", "Please try again.\n\nIf this keeps happening, please contact us.", "Ok");
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

            string jobKey = String.Format("/api/api/5?id=id$~{0}~cmd$~getjob~{1}~{2}", cnx.ID, listStart, listEnd);

            string jsonRaw = "";


            jsonRaw = await Client.GET(cnx.Token, jobKey);
            if (jsonRaw == "errorerrorerror")
            {
                await DisplayAlert("Http Request Error", "Please try again.\n\nIf this keeps happening, please contact us.", "Ok");

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

                Jobs.Add(job);

            }

            return Jobs;
        }

        async void updateList(Connection cnx, List<JobCell> JOBCELLS)
        {
            List<Job> Jobs = new List<Job>(); ;
            List<JobCell> Cells = new List<JobCell>();


            if (JOBCELLS == null)
            {
                JobAmounts jobamounts = await GetAmounts(cnx);
                if (!string.IsNullOrEmpty(jobamounts.jobs))
                {
                    int endNumber = Int32.Parse(jobamounts.jobs);
                    Jobs = await GetContent(0, endNumber, cnx);
                    if(Jobs == null)
                    {
                        await DisplayAlert("Error", "There has been an issue retreiving your jobs. Please try again.\n\nIf this keeps happening please restart the app.", "Ok");
                        return;
                    }
                }
                else
                {
                    await DisplayAlert("Error", "There has been an issue retreiving your job count. Please try again.\n\nIf this keeps happening please restart the app.", "Ok");
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

            searchbar = new SearchBar()
            {
                Placeholder = "Search:",
                Text = searchBarText,
                CancelButtonColor = Color.Red,
                SearchCommand = new Command(() => { Searchbar_SearchButtonPressed(searchbar.Text); })
            };

            

        

        ListView listView = new ListView()
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

                    return new ViewCell
                    {
                        View = new StackLayout
                        {
                            Padding = new Thickness(0, 5),
                            Orientation = StackOrientation.Horizontal,
                            Children =
                            {
                                JobNumber,
                                //stick in an image here for whatever you want.
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


                })
            };

            listView.ItemSelected += ListView_ItemSelected;

            this.Padding = new Thickness(10, 20, 10, 5);


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

        private void Searchbar_SearchButtonPressed(string Text)
        {
            //SearchBar search = sender as SearchBar;
            


            if (!string.IsNullOrEmpty(Text) && !string.IsNullOrWhiteSpace(Text))
            {

                string _text = "";

                try
                {
                    searchBarText = Text;
                    _text = Text.ToLower();
                    
                }
                catch(NullReferenceException)
                {
                    Console.WriteLine("CAUGHT THE EXCEPTION 1");
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
                        catch (NullReferenceException)
                        {
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
                        catch (NullReferenceException)
                        {
                            indexerList.Add(indexer);
                            Console.WriteLine("CAUGHT AT " + indexer);
                        }
                        Console.WriteLine(indexer + "     " + "Add1" + "     " + y + "\n");
                        

                        if (y.Contains(_text))
                        {
                            addToList = true;
                        }
                        //indexer++;
                        //string y = x.Add2.ToLower();
                        //Console.WriteLine(indexer + "     " + "Add2" + "     " + y + "\n");
                        //indexerList.Add(indexer);
                        //if (y.Contains(text))
                        //{
                        //    addToList = true;
                        //}
                    }

                    if (!string.IsNullOrEmpty(x.Add3) && !string.IsNullOrWhiteSpace(x.Add3))
                    {
                        indexer++;
                        string y = "";
                        try
                        {
                            y = x.Add3.ToLower();
                        }
                        catch (NullReferenceException)
                        {
                            indexerList.Add(indexer);
                            Console.WriteLine("CAUGHT AT " + indexer);
                        }
                        Console.WriteLine(indexer + "     " + "Add1" + "     " + y + "\n");
                        

                        if (y.Contains(_text))
                        {
                            addToList = true;
                        }
                        //indexer++;
                        //string y = x.Add3.ToLower();
                        //Console.WriteLine(indexer + "     " + "Add3" + "     " + y + "\n");
                        //indexerList.Add(indexer);
                        //if (y.Contains(text))
                        //{
                        //    addToList = true;
                        //}
                    }

                    if (!string.IsNullOrEmpty(x.Add4) && !string.IsNullOrWhiteSpace(x.Add4))
                    {
                        indexer++;
                        string y = "";
                        try
                        {
                            y = x.Add4.ToLower();
                        }
                        catch (NullReferenceException)
                        {
                            indexerList.Add(indexer);
                            Console.WriteLine("CAUGHT AT " + indexer);
                        }
                        Console.WriteLine(indexer + "     " + "Add1" + "     " + y + "\n");
                        

                        if (y.Contains(_text))
                        {
                            addToList = true;
                        }
                        //indexer++;
                        //string y = x.Add4.ToLower();
                        //Console.WriteLine(indexer + "     " + "Add4" + "     " + y + "\n");
                        //indexerList.Add(indexer);
                        //if (y.Contains(text))
                        //{
                        //    addToList = true;
                        //}
                    }

                    if (!string.IsNullOrEmpty(x.AddPC) && !string.IsNullOrWhiteSpace(x.AddPC))
                    {
                        indexer++;
                        string y = "";
                        try
                        {
                            y = x.AddPC.ToLower();
                        }
                        catch (NullReferenceException)
                        {
                            indexerList.Add(indexer);
                            Console.WriteLine("CAUGHT AT " + indexer);
                        }
                        Console.WriteLine(indexer + "     " + "Add1" + "     " + y + "\n");
                        

                        if (y.Contains(_text))
                        {
                            addToList = true;
                        }
                        //indexer++;
                        //string y = x.AddPC.ToLower();
                        //Console.WriteLine(indexer + "     " + "AddPC" + "     " + y + "\n");
                        //indexerList.Add(indexer);
                        //if (y.Contains(text))
                        //{
                        //    addToList = true;
                        //}
                    }

                    if (!string.IsNullOrEmpty(x.Description) && !string.IsNullOrWhiteSpace(x.Description))
                    {
                        indexer++;
                        string y = "";
                        try
                        {
                            y = x.Description.ToLower();
                        }
                        catch (NullReferenceException)
                        {
                            indexerList.Add(indexer);
                            Console.WriteLine("CAUGHT AT " + indexer);
                        }
                        Console.WriteLine(indexer + "     " + "Add1" + "     " + y + "\n");
                        

                        if (y.Contains(_text))
                        {
                            addToList = true;
                        }
                        //indexer++;
                        //string y = x.Description.ToLower();
                        //Console.WriteLine(indexer + "     " + "Description" + "     " + y + "\n");
                        //indexerList.Add(indexer);
                        //if (y.Contains(text))
                        //{
                        //    addToList = true;
                        //}
                    }

                    if (!string.IsNullOrEmpty(x.JobNumber) && !string.IsNullOrWhiteSpace(x.JobNumber))
                    {
                        indexer++;
                        string y = "";
                        try
                        {
                            y = x.JobNumber.ToLower();
                        }
                        catch (NullReferenceException)
                        {
                            indexerList.Add(indexer);
                            Console.WriteLine("CAUGHT AT " + indexer);
                        }
                        Console.WriteLine(indexer + "     " + "Add1" + "     " + y + "\n");
                        

                        if (y.Contains(_text))
                        {
                            addToList = true;
                        }
                        //indexer++;
                        //string y = x.JobNumber.ToLower();
                        //Console.WriteLine(indexer + "     " + "JobNumber" + "     " + y + "\n");
                        //indexerList.Add(indexer);
                        //if (y.Contains(text))
                        //{
                        //    addToList = true;
                        //}
                    }

                    if (addToList == true)
                    {
                        try
                        {
                            List.Add(cell);
                        }
                        catch (NullReferenceException)
                        {

                            Console.WriteLine("Found Putting in Cell");
                        }
                       
                    }
                }
                var osf = List;
                try
                {
                    updateList(Maincnx, List);
                }
                catch (NullReferenceException)
                {

                    Console.WriteLine("ITS DEFINETLY HAPPENING DURING UPDATING");
                }
                
            }
            else
            {
                updateList(Maincnx, null);
                searchBarText = "";
            }
        }

        //private void ONTEXTCHANGEEVENT(object sender, TextChangedEventArgs e)
        //{
        //    if(!string.IsNullOrEmpty(e.NewTextValue) && !string.IsNullOrWhiteSpace(e.NewTextValue))
        //    {
        //        searchBarText = e.NewTextValue;
        //        string text = e.NewTextValue.ToLower();
        //        string y;
                
                
        //        List<JobCell> List = new List<JobCell>();

        //        foreach (JobCell cell in _Cells)
        //        {
        //            bool addToList = false;
        //            JobCell x = cell;
        //            if (!string.IsNullOrEmpty(x.Add1))
        //            {
        //                y = x.Add1.ToLower();
        //                if (y.Contains(text))
        //                {
        //                    addToList = true;
        //                }
        //            }
                    
        //            if (!string.IsNullOrEmpty(x.Add2))
        //            {
        //                y = x.Add2.ToLower();
        //                if (y.Contains(text))
        //                {
        //                    addToList = true;
        //                }
        //            }
                    
        //            if (!string.IsNullOrEmpty(x.Add3))
        //            {
        //                y = x.Add3.ToLower();
        //                if (y.Contains(text))
        //                {
        //                    addToList = true;
        //                }
        //            }
                    
        //            if (!string.IsNullOrEmpty(x.Add4))
        //            {
        //                y = x.Add4.ToLower();
        //                if (y.Contains(text))
        //                {
        //                    addToList = true;
        //                }
        //            }

        //            if (!string.IsNullOrEmpty(x.AddPC))
        //            {
        //                y = x.AddPC.ToLower();
        //                if (y.Contains(text))
        //                {
        //                    addToList = true;
        //                }
        //            }

        //            if (!string.IsNullOrEmpty(x.Description))
        //            {
        //                y = x.Description.ToLower();
        //                if (y.Contains(text))
        //                {
        //                    addToList = true;
        //                }
        //            }

        //            if (!string.IsNullOrEmpty(x.JobNumber))
        //            {
        //                y = x.JobNumber;
        //                if (y.Contains(text))
        //                {
        //                    addToList = true;
        //                }
        //            }

        //            if (addToList == true)
        //            {
        //                List.Add(cell);
        //            }
        //        }

        //        updateList(Maincnx, List);
        //        searchbar.Focus();
        //    }
        //    else
        //    {
        //        updateList(Maincnx, null);
        //        searchBarText = "";
        //        searchbar.Focus();
        //    }

        //}

        private async Task<JobAmounts> GetAmounts(Connection cnx)
        {
            string key = string.Format("/api/api/5?id=id$~{0}~cmd$~getjobnums", cnx.ID);
            var jsonRaw = await Client.GET(cnx.Token, key);
            if (jsonRaw == "errorerrorerror")
            {
                await DisplayAlert("Http Request Error", "Please try again.\n\nIf this keeps happening, please contact us.", "Ok");
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


            string jobKey = String.Format("/api/api/5?id=id$~{0}~cmd$~getjobdetails~{1}", cnx.ID, item.JobNumber);

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
                        noSubs = true,



                        job = _item
                    };


                    await Navigation.PushAsync(new JobSpecific(cnx, _job));

                }
                else
                {

                    await Navigation.PushAsync(new Subjobs_List(cnx, subjobs, _item));
                }
            }
        }


        protected override void OnAppearing()
        {
            if (firstLoad == false)
            {
                updateList(Maincnx, null);
            }

            base.OnAppearing();
        }



        private async void ChangePasswordButtonClicked(object sender, EventArgs e)
        {
            //go to change password page.
            await Navigation.PushModalAsync(new ChangePassword(Maincnx.TokenInfoJsonProps));
        }

        private async void ContactUsButtonClicked(object sender, EventArgs e)
        {
            //call the office
            try
            {
                //CHANGE TO REQUEST NUMBER
                PhoneDialer.Open("+441625420821");
            }
            catch (FeatureNotSupportedException ex)
            {
                await DisplayAlert("ERROR", "Dialer feature not supported.", "OK");
                return;

            }
            catch (Exception ex)
            {
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
                string key = "/api/api/5?id=id$~9999~cmd$~logout";
                //string key = $"/api/api/5?id=id$~{Maincnx.ID}~cmd$~logout";

                string response = await Client.GET(Maincnx.Token, key);
                if(response == "errorerrorerror")
                {
                    await DisplayAlert("Error", "There has been an error logging you out, please try again.\n\nIf this keeps happening please restart the app.", "Ok");
                    return;
                }


                if (response.Contains("error unautherised user"))
                {
                    await DisplayAlert("Error", "There has been an error logging you out, please try again.\n\nIf this keeps happening please restart the app.", "Ok");
                }
                else
                {
                    //CHECK THAT THIS ACTUALLY WORKS
                    await DisplayAlert("Logged Off", "You have been logged off", "Ok");
                    await Navigation.PopAsync();
                }


            }
        }

        private async void RefreshClicked(object sender, EventArgs e)
        {
            searchbar.Text = "";
            updateList(Maincnx, null);
        }
    }
}