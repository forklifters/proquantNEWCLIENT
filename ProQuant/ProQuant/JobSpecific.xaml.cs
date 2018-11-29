using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Refit;
using Xamarin.Essentials;
using System.Net;
using Microsoft.AppCenter.Analytics;

namespace ProQuant
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class JobSpecific : ContentPage
    {
        bool connected;
        bool loaded = false;
        bool toRefresh = false;
        bool _comingFromSubjob = false;

        double Price;
        double Total;
        double VatVal;
        string _JobNumber;


        JobCell jx;
        JobCell _jobcell;
        Connection MainCnx;

        public JobSpecific(Connection cnx, JobCell job, bool comingFromSubjob)
        {
            InitializeComponent();
            BackgroundColor = Color.FromHex("#B80000");
            _comingFromSubjob = comingFromSubjob;
            jx = job;
            MainCnx = cnx;
            updateView(job);   
            _jobcell = job;
            loaded = true;
            if (job.Status == "Completed")
            {
                SendPDFButton.IsEnabled = true;
                SendPDFButton.IsVisible = true;
            }
            else
            {
                SendPDFButton.IsEnabled = false;
                SendPDFButton.IsVisible = false;
                AbsoluteLayout.SetLayoutBounds(CallUsButton, new Rectangle(.5,.975,.25,.1));
            }
            
            
        }

        protected override async void OnDisappearing()
        {
            if(toRefresh == true && _comingFromSubjob == true)
            {
                await Navigation.PushAsync(new main(MainCnx)
                {
                    BarBackgroundColor = Color.FromHex("#B80000"),
                    BarTextColor = Color.White

                });
            }     
            base.OnDisappearing();
        }

        private void updateView(JobCell job)
        {
            if (MainCnx.MD == "md")
            {
                ConvertToMerchantView();
                if (string.IsNullOrEmpty(job.PO))
                {
                    PONumber.Text = job.job.PO;
                }
                else
                {
                    PONumber.Text = job.PO;
                }
                
            }

            if (job.Status != "Sent")
            {
                SendPDFButton.IsEnabled = false;
            }

            _JobNumber = job.job.job.ToString();
            IDLabel.Text = string.Format("Job: {0}", job.job.job.ToString());
            if(string.IsNullOrEmpty(job.SubJobNumber))
            {
                SJLabel.Text = "";
            }
            else
            {
                if(job.SubJobNumber == "0")
                {
                    job.SubJobNumber = "1";
                }
                SJLabel.Text = string.Format("Part: {0}", job.SubJobNumber);
            }
            

            RearrangeAndDisplayAddress(job);
            Notes.Text = job.Notes;
            Description.Text = job.Description;
            Total = job.GrossValue;
            Price = job.NetValue;
            VatVal = job.VatValue;
            var awarded = job.job.awarded;

            {
                switch (awarded)
                {
                    case "Won":
                    {
                        AwardedPicker.SelectedIndex = 0;
                        AwardedPicker.TextColor = Color.Green;
                        break;
                    }
                    case "Lost":
                    {
                        AwardedPicker.SelectedIndex = 1;
                        AwardedPicker.TextColor = Color.Red;
                        break;
                    }
                    default:
                    {
                        AwardedPicker.SelectedIndex = -1;
                        break;
                    }
                }
            }




            Status.Text = job.Status;
            Status.TextColor = job.StatusColor;


            PayButton.IsEnabled = false;
            PayButton.Text = "No Payment Due";

            if (Total != 0)
            {
                PayButton.IsEnabled = true;
                PayButton.Text = String.Format("Pay: £{0}", Total);
            }


        }

        private void RearrangeAndDisplayAddress(JobCell job)
        {
            List<string> address = new List<string>();
            if (!string.IsNullOrWhiteSpace(job.job.add1))
            {
                address.Add(job.job.add1);
            }
            if (!string.IsNullOrWhiteSpace(job.job.add2))
            {
                address.Add(job.job.add2);
            }
            if (!string.IsNullOrWhiteSpace(job.job.add3))
            {
                address.Add(job.job.add3);
            }
            if (!string.IsNullOrWhiteSpace(job.job.add4))
            {
                address.Add(job.job.add4);
            }
            if (!string.IsNullOrWhiteSpace(job.job.addpc))
            {
                address.Add(job.job.addpc);
            }

            for (int i = 0; i < 5; i++)
            {
                address.Add("");
            }

            Add1.Text = address[0];
            Add2.Text = address[1];
            Add3.Text = address[2];
            Add4.Text = address[3];
            AddPC.Text = address[4];
        }

        private void ConvertToMerchantView()
        {
            specificBackround.Source = "Specific_Merchant.png";
            PayButton.IsEnabled = false;
            PayButton.IsVisible = false;
            AwardedPicker.IsEnabled = false;
            AwardedPicker.IsVisible = false;
            PONumber.IsVisible = true;
            POText.IsVisible = true;
            PONumber.IsEnabled = true;
            POText.IsEnabled = true;
            AbsoluteLayout.SetLayoutBounds(Add1, new Rectangle(.12, .375, .58, .045));
            AbsoluteLayout.SetLayoutBounds(Add2, new Rectangle(.12, .415, .58, .045));
            AbsoluteLayout.SetLayoutBounds(Add3, new Rectangle(.12, .455, .58, .045));
            AbsoluteLayout.SetLayoutBounds(Add4, new Rectangle(.12, .495, .58, .045));
            AbsoluteLayout.SetLayoutBounds(AddPC, new Rectangle(.12, .535, .58, .045));
            AbsoluteLayout.SetLayoutBounds(Description, new Rectangle(.475, .69, .9, .1));
            AbsoluteLayout.SetLayoutBounds(Notes, new Rectangle(.475, .81, .9, .1));
        }

        private async void PayButton_Clicked(object sender, EventArgs e)
        {
            ConnectionCheck();
            var response = await DisplayAlert(String.Format("Total: £{0}", Total),
                                                String.Format("Our Price: £{0} \n" +
                                                                "Vat: £{1} \n",
                                                                Price,
                                                                VatVal,
                                                                Total),
                                                String.Format("Pay £{0}", Total), "Cancel");

            if (response == true)
            {
                //PUT REQUEST FOR PAY LINK HERE
                Console.WriteLine("GO TO PAY LINK");

                string jobnumber = _jobcell.job.job.ToString();
                string subjobnumber = _jobcell.SubJobNumber;
                if (string.IsNullOrEmpty(subjobnumber))
                {
                    subjobnumber = "0";
                }

                string payrequest = String.Format("/api/api/5?id=id$~{0}~cmd$~getpaylink~{1}~{2}", MainCnx.ID, jobnumber, subjobnumber);


                ConnectionCheck();
                if (connected == true)
                {
                    try
                    {
                        //string paylink = GetJob(payrequest, MainCnx.Token).Result; //return link isnt working.
                        string message = await Client.GET(MainCnx.Token, payrequest);
                        if (message == "errorerrorerror")
                        {
                            LoginPage.SendError("JS01", "Http GET request error on Client.GET() call. - Payment Link");
                            await DisplayAlert("Http Request Error", "Please try again.\n\nError Code: JS01\n\nIf this keeps happening, please contact us.", "Ok");
                            return;
                        }

                        PayLinkJsonParse PayLinkJson = PayLinkJsonParse.FromJson(message);
                        var xx = PayLinkJson;

                        if (string.IsNullOrEmpty(PayLinkJson.Error))
                        {
                            Uri link = new Uri(PayLinkJson.Message, UriKind.Absolute);
                            await Browser.OpenAsync(link, BrowserLaunchMode.SystemPreferred);

                            string amount = "";

                            try
                            {
                                amount = Price.ToString();
                            }catch(Exception ex)
                            {
                                amount = "ExceptionOccured";
                            }

                            Analytics.TrackEvent("Payment Browser Opened", new Dictionary<string, string>
                            {
                                {"Job", jobnumber},
                                {"SubJob", subjobnumber},
                                {"Amount", amount},
                                {"ID", MainCnx.ID},
                                {"Email", MainCnx.User},
                                {"Name", MainCnx.Name}
                            });
                        }
                        else
                        {
                            LoginPage.SendError("JS05", "PaymentLinkJson Error", PayLinkJson.Error);
                            await DisplayAlert("ERROR", "There was an error transferring you to our payment portal, your have not been charged. \n\nError Code: JS05", "Ok");
                            
                        }

                    }
                    catch (UriFormatException Ex)
                    {
                        Console.WriteLine(Ex.Message);
                        LoginPage.SendError("JS06", "Url Format Exception", Ex.Message);
                        await DisplayAlert("Error", Ex.Message + "\n\n Error Code: JS06", "Cancel");
                    }
                }
            }
            else
            {
                return;
            }
        }

        private async void SendPDFButton_Clicked(object sender, EventArgs e)
        {
            if (jx.SentCount == 0)
            {
                Console.WriteLine("SEND PDF");
                //put a pdf request here
                sendpdfs();

            }

            if (jx.SentCount >= 1)
            {
                //var response = await DisplayAlert("Send Completed Job?", string.Format("This job has been sent {0} time(s) previously", jx.SentCount), "Send", "Cancel");
                var response = await DisplayAlert("Send Completed Job?", $"Send email to:\n\n{MainCnx.User}", "Send", "Cancel");
                if (response == true)
                {
                    Console.WriteLine("SEND PDF");
                    sendpdfs();//put a pdf request here
                }
                else
                {
                    return;
                }
            }


        }

        private async void sendpdfs()
        {

            string jobnumber = _jobcell.job.job.ToString();
            string subjobnumber = _jobcell.SubJobNumber;
            string key = string.Format("/api/api/5?id=id$~{0}~cmd$~emailpdfs~{1}~{2}~{3}", MainCnx.ID, jobnumber, subjobnumber, MainCnx.User);

            ConnectionCheck();
            if (connected == true)
            {
                string response = await GetJob(key, MainCnx.Token);
                if (string.IsNullOrEmpty(response))
                {
                    LoginPage.SendError("JS02", " Response of sendpdf request has returned null.");
                    await DisplayAlert("Error", "There has been an issue sending this pdf. \n\nError Code: JS02\n\n Please try again.", "Ok");
                    return;
                }

                await DisplayAlert("PDF Sent!", $"Email containing PDF has been sent to:\n\n{MainCnx.User}", "Ok");

                Analytics.TrackEvent("PDF Sent", new Dictionary<string, string>
                            {
                                {"Job", jobnumber},
                                {"SubJob", subjobnumber},
                                {"ID", MainCnx.ID},
                                {"Email", MainCnx.User},
                                {"Name", MainCnx.Name}
                            }
                );
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

        async Task<string> GetJob(string key, string token)
        {
            ConnectionCheck();
            if (connected == true)
            {
                var response = await Client.GET(token, key);
                if (response == "errorerrorerror")
                {
                    LoginPage.SendError("JS03", "Http GET request error on Client.GET() call - TASK<string> GetJob()");
                    await DisplayAlert("Http Request Error", "Please try again.\n\nError Code: JS03\n\nIf this keeps happening, please contact us.", "Ok");
                    return null;
                }
                var x = response;
                return response;
            }
            return null;
        }

        private async void CallUsButton_Clicked_1(object sender, EventArgs e)
        {
            try
            {
                //CHANGE TO REQUEST NUMBER
                PhoneDialer.Open("+441625420821");
            }
            catch (FeatureNotSupportedException ex)
            {
                LoginPage.SendError("JS07", "Dialer feature supported", ex.Message);
                await DisplayAlert("ERROR", "Dialer feature not supported.", "OK");
                return;

            }
            catch (Exception ex)
            {
                LoginPage.SendError("", "Call us button unknown exception", ex.Message);
                return;
            }
        }

        private async void AwardedPicker_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (loaded == true)
            {
                Picker picker = sender as Picker;
                string effect = "";
                string key = "";
                var selected = picker.SelectedItem;
                toRefresh = true;

            
                switch (selected)
                {
                    case "Won":
                        AwardedPicker.TextColor = Color.Green;
                        effect = "Won";
                        key = $"/api/api/5?id=id$~{MainCnx.ID}~cmd$~setawarded~{effect}~{_JobNumber}~0";
                        break;

                    case "Lost":
                        AwardedPicker.TextColor = Color.Red;
                        effect = "Lost";
                        key = $"/api/api/5?id=id$~{MainCnx.ID}~cmd$~setawarded~{effect}~{_JobNumber}~0";
                        break;

                    case "Neither":
                        AwardedPicker.TextColor = Color.Black;
                        effect = "";
                        key = $"/api/api/5?id=id$~{MainCnx.ID}~cmd$~setawarded~{effect}~{_JobNumber}~0";
                        break;

                    default:
                        AwardedPicker.TextColor = Color.Black;
                        break;
                }

                var response = await Client.GET(MainCnx.Token, key);
                if(response == "errorerrorerror")
                {
                    LoginPage.SendError("JS04", "Http GET request error on Client.GET() call - Set Awarded.");
                    await DisplayAlert("Error", "There was some difficulty contacting the server, please try again \n\nError Code JS04", "Ok.");
                    return;
                }

                Analytics.TrackEvent("Awarded Status Changed", new Dictionary<string, string>
                            {
                                {"Job", _jobcell.JobNumber},
                                {"SubJob", _jobcell.SubJobNumber},
                                {"Awarded?", effect},
                                {"ID", MainCnx.ID},
                                {"Email", MainCnx.User},
                                {"Name", MainCnx.Name}
                            });


            }
        }

    }
}