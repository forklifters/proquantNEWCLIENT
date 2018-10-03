﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Refit;
using Xamarin.Essentials;
using System.Net;

namespace ProQuant
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class JobSpecific : ContentPage
    {
        bool connected;
        Double Price;
        Double Total;
        Double VatVal;
        JobCell _jobcell;

        Connection MainCnx;

        JobCell jx;



        public JobSpecific(Connection cnx, JobCell job)
        {
            InitializeComponent();
            jx = job;
            updateView(job);
            MainCnx = cnx;
            _jobcell = job;
        }





        private void updateView(JobCell job)
        {
            if (job.Status != "Sent")
            {
                SendPDFButton.IsEnabled = false;
            }

            IDLabel.Text = string.Format("Job: {0}", job.job.job.ToString());
            SJLabel.Text = string.Format("Subjob: {0}", job.SubJobNumber);
            Add1.Text = job.job.add1;
            Add2.Text = job.job.add2;
            Add3.Text = job.job.add3;
            Add4.Text = job.job.add4;
            AddPC.Text = job.job.addpc;
            Description.Text = job.Description;
            Total = job.GrossValue;
            Price = job.NetValue;
            VatVal = job.VatValue;




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
                            await DisplayAlert("Http Request Error", "Please try again.\n\nIf this keeps happening, please contact us.", "Ok");
                            return;
                        }

                        PayLinkJsonParse PayLinkJson = PayLinkJsonParse.FromJson(message);
                        var xx = PayLinkJson;

                        if (string.IsNullOrEmpty(PayLinkJson.Error))
                        {
                            Uri link = new Uri(PayLinkJson.Message, UriKind.Absolute);
                            await Browser.OpenAsync(link, BrowserLaunchMode.SystemPreferred);
                        }
                        else
                        {
                            await DisplayAlert("ERROR", PayLinkJson.Error, "Ok");
                        }
                        
                    }
                    catch (UriFormatException Ex)
                    {
                        Console.WriteLine(Ex.Message);
                        await DisplayAlert("Error", Ex.Message, "Cancel");
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
                var response = await DisplayAlert("Send Completed Job?", string.Format("This has been sent {0} time(s) previously", jx.SentCount), "Send Again", "Cancel");
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
            string key = string.Format("/api/api/5?id=id$~{0}~cmd$~emailpdfs~{1}~{2}~{3}", MainCnx.ID, jobnumber, subjobnumber, "oliver.filmer@proquantestimating.co.uk"); //replace this with builder email.

            ConnectionCheck();
            if (connected == true)
            {
                string response = await GetJob(key, MainCnx.Token);
                if (string.IsNullOrEmpty(response))
                {
                    await DisplayAlert("Error", "There has been an issue sending this pdf. Please try again.", "Ok");
                }
                await DisplayAlert("PDF Sent!", "Email containing PDF has been sent to the email associated with this account", "Ok");
            }
        }


        //async Task<string> GetJob(string key, string token)
        //{
        //    string auth = "Bearer " + token;
        //    var nsAPI = RestService.For<IMakeUpApi>("https://proq.remotewebaccess.com:58330");
        //    var response = await nsAPI.GetKey(key, auth);
        //    return response;
        //}

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
                //string auth = "Bearer " + token;
                //var nsAPI = RestService.For<IMakeUpApi>("https://proq.remotewebaccess.com:58330");
                //var response = await nsAPI.GetKey(key, auth);

                //IMPLEMENT A REFRESH BUTTON ON A TOOLBAR
                var response = await Client.GET(token, key);
                if (response == "errorerrorerror")
                {
                    await DisplayAlert("Http Request Error", "Please try again.\n\nIf this keeps happening, please contact us.", "Ok");
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
                await DisplayAlert("ERROR", "Dialer feature not supported.", "OK");
                return;

            }
            catch (Exception ex)
            {
                return;
            }
        }

        private void AwardedPicker_SelectedIndexChanged(object sender, EventArgs e)
        {
            Picker picker = sender as Picker;
            var selected = picker.SelectedItem;
            switch (selected)
            {
                case "Won":
                    AwardedPicker.TextColor = Color.Green;
                    //update server with won
                    break;

                case "Lost":
                    AwardedPicker.TextColor = Color.Red;
                    //update server with lost
                    break;

                case "Neither":
                    AwardedPicker.TextColor = Color.Black;
                    //update server with Not a Tender
                    break;

                default:
                    AwardedPicker.TextColor = Color.Black;
                    break;
            }
        }
    }
}