using System;

namespace FakeXrmEasy.Core.CommercialLicense.Exceptions
{
    /// <summary>
    /// The current subscription expired
    /// </summary>
    public class SubscriptionExpiredException: Exception
    {
        private const string _url =
            CommercialLicenseTroubleshootingLinks.BaseUrl + "/subscription-expired-exception/";
        
        /// <summary>
        /// Throws an exception where the current subscription expired
        /// </summary>
        /// <param name="expiredOn"></param>
        public SubscriptionExpiredException(DateTime expiredOn) : base($"The current subscription expired on {expiredOn.ToLongDateString()}. More info at {_url}.")
        {
            
        }
    }
}