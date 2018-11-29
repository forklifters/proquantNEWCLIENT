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
    public partial class Subjobs_List : ContentPage
    {
        Connection MainCnx;
        List<Job> _SubJ;
        Job _It;

        bool firstLoad = true;

        public Subjobs_List(Connection cnx, List<Job> subjobs, Job item)
        {
            InitializeComponent();
            MainCnx = cnx;
            _SubJ = subjobs;
            _It = item;

            if (subjobs.Count >= 1)
            {
                updateList(cnx, subjobs, item);
                firstLoad = false;
            }
            if (subjobs.Count < 1)
            {
                Console.WriteLine("NO SUBJOBS DETECTED");
            }


            //maybe add subjob count to the main list custom cell.


            //subjobs contain all jobs NEED TO RUN THROUGH THAT FOR EACH TO DEBUG.

        }

        protected override void OnAppearing()
        {
            if (firstLoad == false)
            {
                updateList(MainCnx, _SubJ, _It);
            }
            base.OnAppearing();
        }

        void updateList(Connection cnx, List<Job> subjobs, Job item)
        {
            Job JR = item;


            List<JobCell> Cells = new List<JobCell>();



            foreach (Job job in subjobs)
            {
                JobCell cell = new JobCell();

                cell.Add1 = JR.add1;
                cell.Add2 = JR.add2;
                cell.Add3 = JR.add3;
                cell.Add4 = JR.add4;
                cell.AddPC = JR.addpc;
                cell.Builder = JR.buildername;
                cell.JobNumber = JR.job.ToString();
                cell.Awarded = JR.awarded;
                
                

                cell.subjob = job;
                cell.PO = job.PO;
                cell.Notes = job.Notes;
                cell.SubJobNumber = job.subjob.ToString();
                cell.Created = job.created;
                cell.Description = job.description;
                cell.SentCount = job.sentcount;
                cell.GrossValue = job.grossValue;
                cell.NetValue = job.netValue;
                cell.VatValue = job.vatValue;
                cell.JobColor = StatusSorter.JobNumberColor(subjobs[0]);
                cell.Status = StatusSorter.StatusText(subjobs[0]);
                cell.StatusColor = StatusSorter.StatusColor(subjobs[0]);
                cell.CellColor = StatusSorter.CellColor(subjobs[0]);

                //switch (job.status)
                //{
                //    case "S20":
                //        cell.JobColor = Color.Green;
                //        cell.StatusColor = Color.Green;
                //        cell.Status = "Sent";
                //        break;
                //    case "S00":
                //        cell.Status = "Awaiting Appraisal";
                //        break;
                //    case "S02":
                //        cell.CellColor = Color.LightPink;
                //        cell.JobColor = Color.Red;
                //        cell.StatusColor = Color.Red;
                //        cell.Status = "Awaiting Documentation";
                //        break;
                //    case "S03":
                //        cell.Status = "Adjudication Issue";
                //        cell.StatusColor = Color.Purple;
                //        break;
                //    case "S99":
                //        cell.Status = "Cancelled";
                //        cell.CellColor = Color.LightGray;
                //        break;
                //    default:
                //        cell.Status = job.status;
                //        break;
                //}

                cell.job = JR;
                if (job.subjob == 0)
                {
                    cell.NumberAndPart = $"{cell.JobNumber}";
                }
                else
                {
                    cell.NumberAndPart = $"{cell.JobNumber} part: {cell.SubJobNumber}";
                }
                Cells.Add(cell);
            }

            Label jobHeader = new Label
            {
                Text = Cells[0].Add1,
                FontSize = Device.GetNamedSize(NamedSize.Large, typeof(Label)),
                TextColor = Color.Black,
                HorizontalOptions = LayoutOptions.Center
            };

            Label builder = new Label
            {
                Text = Cells[0].Builder,
                FontSize = Device.GetNamedSize(NamedSize.Large, typeof(Label)),
                TextColor = Color.Black,
                HorizontalOptions = LayoutOptions.Center
            };

            ListView listView = new ListView()
            {
                BackgroundColor = Color.White,
                HasUnevenRows = true,
                ItemsSource = Cells,
                ItemTemplate = new DataTemplate(() =>
                {
                    Label JobNumber = new Label()
                    {
                        FontSize = Device.GetNamedSize(NamedSize.Small, typeof(Label))
                    };
                    JobNumber.SetBinding(Label.TextProperty, "NumberAndPart");
                    JobNumber.SetBinding(Label.TextColorProperty, "JobColor");

                    //Label SubJob = new Label()
                    //{
                    //    FontSize = Device.GetNamedSize(NamedSize.Small, typeof(Label)),
                    //};
                    //SubJob.SetBinding(Label.TextProperty, "SubJobNumber");

                    Label JobDiscription = new Label()
                    {
                        FontSize = Device.GetNamedSize(NamedSize.Small, typeof(Label)),
                        FontAttributes = FontAttributes.Bold
                    };
                    JobDiscription.SetBinding(Label.TextProperty, "Description");

                    Label JobAddress = new Label()
                    {
                        FontSize = Device.GetNamedSize(NamedSize.Small, typeof(Label)),
                        FontAttributes = FontAttributes.Italic
                    };
                    JobAddress.SetBinding(Label.TextProperty, "Add1");

                    Label Status = new Label();
                    Status.SetBinding(Label.TextProperty, "Status");
                    Status.SetBinding(Label.TextColorProperty, "StatusColor");

                    return new ViewCell
                    {
                        View = new StackLayout
                        {
                            Padding = new Thickness(0, 5),
                            Children =
                            {
                                new StackLayout
                                {
                                    Orientation = StackOrientation.Horizontal,
                                    Children =
                                    {
                                        new StackLayout
                                        {
                                            Orientation = StackOrientation.Vertical,
                                            HorizontalOptions = LayoutOptions.StartAndExpand,
                                            Children =
                                            {
                                                JobNumber
                                            }
                                        },

                                        new StackLayout
                                        {
                                            Orientation = StackOrientation.Vertical,
                                            Children =
                                            {
                                                Status
                                            }
                                        }

                                    }
                                },
                                new StackLayout
                                {
                                    Orientation = StackOrientation.Horizontal,
                                    Children =
                                    {
                                        new StackLayout
                                        {
                                            Orientation = StackOrientation.Vertical,
                                            HorizontalOptions = LayoutOptions.StartAndExpand,
                                            Children =
                                            {

                                                JobDiscription
                                            }
                                        },

                                        new StackLayout
                                        {
                                            Orientation = StackOrientation.Vertical,
                                            Children =
                                            {
                                                //SubJob
                                            }
                                        }

                                    }
                                }
                            }
                        }
                    };
                })
            };

            listView.ItemSelected += ListView_ItemSelected;


            this.Padding = new Thickness(10, 20, 10, 5);

            if (MainCnx.MD == "md")
            {
                this.Content = new StackLayout
                {
                    Children =
                    {
                        builder,
                        jobHeader,
                        listView
                    }
                };
            }
            else
            {
                this.Content = new StackLayout
                {
                    Children =
                    {
                        jobHeader,
                        listView
                    }
                };
            }
            


        }



        private void ListView_ItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            JobCell _job = new JobCell();
            _job = e.SelectedItem as JobCell;

            go(_job);
        }

        private async void go(JobCell _job)
        {
            await Navigation.PushAsync(new JobSpecific(MainCnx, _job, true));
        }


    }
}