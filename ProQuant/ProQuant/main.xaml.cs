using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

using Refit;
using Newtonsoft.Json;

namespace ProQuant
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class main : TabbedPage
    {
        bool connected = false;
        public int jobStartNumber = 0;
        public int jobEndNumber = 5;
        public Connection Maincnx;
        public bool firstLoad = true;




        public main(Connection cnx)
        {
            InitializeComponent();
            updateList(cnx);
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

        async Task<string> GetJob(string key, string token)
        {
            ConnectionCheck();
            if (connected == true)
            {

                var response = await Client.GET(token, key);
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

            
            
            
            var x = JobsFromJson.FromJson(jsonRaw);

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

        async void updateList(Connection cnx)
        {

            List<Job> Jobs = await GetContent(jobStartNumber, jobEndNumber, cnx);

            List<JobCell> Cells = new List<JobCell>();



            foreach (Job job in Jobs)
            {
                JobCell cell = new JobCell();

                cell.JobNumber = job.job.ToString();
                cell.Add1 = job.add1;
                cell.JobColor = StatusSorter.JobNumberColor(job);
                cell.Status = StatusSorter.StatusText(job);
                cell.StatusColor = StatusSorter.StatusColor(job);
                cell.CellColor = StatusSorter.CellColor(job);
                cell.job = job;
                Cells.Add(cell);
            }

            Label jobHeader = new Label
            {
                Text = Jobs[0].buildername,
                TextColor = Color.Black,
                FontSize = Device.GetNamedSize(NamedSize.Large, typeof(Label)),
                HorizontalOptions = LayoutOptions.Center,
                HorizontalTextAlignment = TextAlignment.Center
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

            Button next = new Button()
            {
                Text = "Next",
            };

            Button prev = new Button()
            {
                Text = "Prev"
            };

            next.Clicked += Next_Clicked;
            prev.Clicked += Prev_Clicked;



            StackLayout buttonStack = new StackLayout()
            {
                Orientation = StackOrientation.Horizontal,
                HorizontalOptions = LayoutOptions.Center,
                Children =
                {
                    prev,
                    next
                }

            };

            Tab1.Content = new StackLayout
            {
                Children =
                {
                    jobHeader,
                    listView,
                    buttonStack
                }
            };
        }

        private void Prev_Clicked(object sender, EventArgs e)
        {
            if (jobStartNumber < 10)
            {
                jobStartNumber = 0;
                jobEndNumber = 10;
                updateList(Maincnx);


            }
            else
            {
                jobStartNumber -= 10;
                jobEndNumber -= 10;
                updateList(Maincnx);
            }
        }

        private void Next_Clicked(object sender, EventArgs e)
        {

            jobStartNumber += 10;
            jobEndNumber += 10;
            updateList(Maincnx);
            //have to add in a end of available jobs check.
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
                updateList(Maincnx);
            }
            else if (rawJson[0] != '[')
            {
                await DisplayAlert("No Info", "No Further Information", "ok");
                Console.WriteLine("RETURNED: Improper Format");
                updateList(Maincnx);
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

                    if (i["sent"] == "")
                    {
                        sj.Sent = 0;
                    }
                    else
                    {
                        sj.Sent = Convert.ToInt32(i["sent"]);
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
                    JobCell _job = new JobCell();

                    _job.JobColor = StatusSorter.JobNumberColor(subjobs[0]);
                    _job.Status = StatusSorter.StatusText(subjobs[0]);
                    _job.StatusColor = StatusSorter.StatusColor(subjobs[0]);
                    _job.CellColor = StatusSorter.CellColor(subjobs[0]);
                    _job.Add1 = subjobs[0].add1;
                    _job.Add2 = subjobs[0].add2;
                    _job.Add3 = subjobs[0].add3;
                    _job.Add4 = subjobs[0].add4;
                    _job.AddPC = subjobs[0].addpc;
                    _job.Awarded = subjobs[0].awarded;
                    _job.Builder = subjobs[0].buildername;
                    _job.Created = subjobs[0].created;
                    _job.Description = subjobs[0].description;
                    _job.JobNumber = subjobs[0].job.ToString();
                    _job.SentCount = subjobs[0].sentcount;
                    _job.GrossValue = subjobs[0].grossValue;
                    _job.NetValue = subjobs[0].netValue;
                    _job.VatValue = subjobs[0].vatValue;
                    _job.noSubs = true;



                    _job.job = _item;


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
                updateList(Maincnx);
            }

            base.OnAppearing();
        }








    }
}