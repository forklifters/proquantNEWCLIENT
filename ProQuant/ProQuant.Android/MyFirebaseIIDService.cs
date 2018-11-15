using System;
using System.Collections.Generic;
using System.Text;
using Android.App;
using Firebase.Iid;
using ProQuant;
using Xamarin.Forms;


namespace ProQuant
{
    //ONLY FOR ANDROID!!
    [Service]
    [IntentFilter(new[] { "com.google.firebase.INSTANCE_ID_EVENT" })]
    public class MyFirebaseIIDService : FirebaseInstanceIdService
    {
        const string TAG = "MyFirebaseIIDService";
        public override void OnTokenRefresh()
        {
            var refreshedToken = FirebaseInstanceId.Instance.Token;
            Console.WriteLine(TAG + "Refreshed token: " + refreshedToken);
            SendRegistrationToServer(refreshedToken);
        }
        async void SendRegistrationToServer(string token)
        {
            // Add custom implementation, as needed.
        }
    }
}
