using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Foundation;
using Rollbar;
using UIKit;

namespace Sample.Xamarin.Forms.iOS
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

            LoadApplication(new App());

            return base.FinishedLaunching(app, options);
        }

        #region Overrides of UIApplicationDelegate

        #endregion

        /// <summary>
        /// Informs the app that the activity of the <paramref name="userActivityType" /> type could not be continued, and specifies a <paramref name="error" /> as the reason for the failure.
        /// </summary>
        /// <param name="application">To be added.</param>
        /// <param name="userActivityType">To be added.</param>
        /// <param name="error">To be added.</param>
        /// <remarks>To be added.</remarks>
        public override void DidFailToContinueUserActivitiy(UIApplication application, string userActivityType, NSError error)
        {
            IDictionary<string, object> custom = new Dictionary<string, object>();
            custom["NSError.Description"] = error.Description;
            custom["NSError.DebugDescription"] = error.DebugDescription;
            custom["NSError.Code"] = error.Code;
            custom["NSError.Domain"] = error.Domain;
            custom["NSError.LocalizedDescription"] = error.LocalizedDescription;
            custom["NSError.LocalizedFailureReason"] = error.LocalizedFailureReason;
            custom["NSError.LocalizedRecoveryOptions"] = error.LocalizedRecoveryOptions;
            custom["NSError.LocalizedRecoverySuggestion"] = error.LocalizedRecoverySuggestion;

            string message = "NSError during user activity type: " + userActivityType;

            RollbarLocator.RollbarInstance.AsBlockingLogger(RollbarHelper.RollbarTimeout).Error(message, custom);

            base.DidFailToContinueUserActivitiy(application, userActivityType, error);
        }

    }
}
