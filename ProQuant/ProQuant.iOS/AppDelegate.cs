using System;
using System.Collections.Generic;
using System.Linq;

using Foundation;
using UIKit;
using UserNotifications;
using Xamarin;
using Xamarin.Forms;
using Xamarin.Essentials;


namespace ProQuant.iOS
{
    // The UIApplicationDelegate for the application. This class is responsible for launching the 
    // User Interface of the application, as well as listening (and optionally responding) to 
    // application events from iOS.
    [Register("AppDelegate")]
    public partial class AppDelegate : global::Xamarin.Forms.Platform.iOS.FormsApplicationDelegate
    {
        //
        // This method is invoked when the application has loaded and is ready to run. In this 
        // method you should instantiate the window, load the UI into it and then make the window
        // visible.
        //
        // You have 17 seconds to return from this method, or iOS will terminate your application.
        //
        public override bool FinishedLaunching(UIApplication app, NSDictionary options)
        {        
            global::Xamarin.Forms.Forms.Init();

            new main(null); //non used stub to make compiler see that code is being written in this. 

            DefaultTempToken();

            if (UIDevice.CurrentDevice.CheckSystemVersion(8, 0)) //REGISTER FOR REMOTE NOTIFICATIONS WITH APNS!
            {
                var pushSettings = UIUserNotificationSettings.GetSettingsForTypes(
                    UIUserNotificationType.Alert | UIUserNotificationType.Badge | UIUserNotificationType.Sound,
                    new NSSet());

                UIApplication.SharedApplication.RegisterUserNotificationSettings(pushSettings);
                UIApplication.SharedApplication.RegisterForRemoteNotifications();
            }
            else
            {
                UIRemoteNotificationType notificationTypes =
                    UIRemoteNotificationType.Alert | UIRemoteNotificationType.Badge | UIRemoteNotificationType.Sound;
                UIApplication.SharedApplication.RegisterForRemoteNotificationTypes(notificationTypes);

            }

            //Get rid of notifications
            if (UIDevice.CurrentDevice.CheckSystemVersion(10, 0))
            {
                UNUserNotificationCenter.Current.RemoveAllPendingNotificationRequests(); // To remove all pending notifications which are not delivered yet but scheduled.
                UNUserNotificationCenter.Current.RemoveAllDeliveredNotifications(); // To remove all delivered notifications
            }
            else
            {
                UIApplication.SharedApplication.CancelAllLocalNotifications();
            }

            LoadApplication(new App());

            return base.FinishedLaunching(app, options);

        }

        public override void RegisteredForRemoteNotifications(
            UIApplication application, NSData deviceToken)
        {
            // Get current device token
            var DeviceToken = deviceToken.Description;
            if (!string.IsNullOrWhiteSpace(DeviceToken))
            {
                DeviceToken = DeviceToken.Trim('<').Trim('>');
            }

            SetTempToken(DeviceToken);

            // Get previous device token
            var oldDeviceToken = NSUserDefaults.StandardUserDefaults.StringForKey("PushDeviceToken");

            // Has the token changed?
            if (string.IsNullOrEmpty(oldDeviceToken) || !oldDeviceToken.Equals(DeviceToken))
            {

                SetTempToken(DeviceToken);
                new UIAlertView("Registering Completed", DeviceToken, null, "OK", null).Show();

            }

            // Save new device token
            NSUserDefaults.StandardUserDefaults.SetString(DeviceToken, "PushDeviceToken");
        }

        public override void FailedToRegisterForRemoteNotifications(UIApplication application, NSError error)
        {
            new UIAlertView("Error registering push notifications", error.LocalizedDescription, null, "OK", null).Show();
        }

        public async void SetTempToken(string token)
        {
            await SecureStorage.SetAsync("APNStemp", token);
        }

        public async void DefaultTempToken()
        {
            await SecureStorage.SetAsync("APNStemp", "");
        }



    }
}
