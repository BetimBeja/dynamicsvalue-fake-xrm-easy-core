using System;

namespace FakeXrmEasy.Core.CommercialLicense.Exceptions
{
    /// <summary>
    /// Exception thrown if the info about the current subscription plan is unknown
    /// </summary>
    public class NoSubscriptionPlanInfoException: Exception 
    {
        private const string _url =
            "https://dynamicsvalue.github.io/fake-xrm-easy-docs/licensing/commercial-license/troubleshooting/no-subscription-plan-info-exception/";
        
        /// <summary>
        /// Default constructor
        /// </summary>
        public NoSubscriptionPlanInfoException() : base($"The current subscription info is unknown. More info at {_url}.")
        {
            
        }
    }
}