using System.Collections.Generic;
using System.Threading.Tasks;
using AWSMobilePushNotificationService.Operators.Registration;
using AWSMobilePushNotificationService.Model.OperatorModel;
using AWSMobilePushNotificationService.Model;
using AWSMobilePushNotificationService.Model.Results;
using AWSMobilePushNotificationService.Model.Exceptions;
using System;

namespace AWSMobilePushNotificationService
{
    /// <summary>
    /// Abstract request to a User
    /// </summary>
    public abstract class AMPSUserRequest : AMPSRequestBase
    {
        internal AMPSUserRequest(IAWSMobilePushNotificationConfigProvider provider) : base(provider) { }

        /// <summary>
        /// Id of the User
        /// </summary>
        public string UserId { get; set; }

        internal string Validate()
        {
            string error = string.Empty;
            if (string.IsNullOrEmpty(UserId))
            {
                error = error + "UserId is empty \n";
            }

            return error;
        }
    }
    /// <summary>
    /// Abstract request to a Subscriber
    /// </summary>
    public abstract class AMPSSubscriberRequest : AMPSUserRequest
    {
        internal AMPSSubscriberRequest(IAWSMobilePushNotificationConfigProvider provider) : base(provider) { }

        /// <summary>
        /// Notification Token for the Subscriber obtained from the device
        /// </summary>
        public string NotificationToken { get; set; }
        internal new string Validate()
        {
            string error = base.Validate();
            if (string.IsNullOrEmpty(NotificationToken))
            {
                error = error + "Token is empty \n";
            }
            return error;
        }
    }
    /// <summary>
    /// Register a 'Subscriber' to the system. A Subscriber is represented with 'UserId,Token' tuple together.
    /// </summary>
    public class RegisterSubscriberRequest : AMPSSubscriberRequest
    {
        /// <summary>
        /// Default constructor for register request
        /// </summary>
        /// <param name="provider">Configuration for the library resources</param>
        public RegisterSubscriberRequest(IAWSMobilePushNotificationConfigProvider provider) : base(provider) { }

        /// <summary>
        /// DeviceId of the subscriber. Optionally stored in DynamoDB
        /// </summary>
        public string DeviceId { get; set; }

        /// <summary>
        /// PushNotification Platform of the subscriber
        /// </summary>
        public Platform Platform { get; set; }

        /// <summary>
        /// Tags to assign to the subscriber initially
        /// </summary>
        public List<PNAttributedTag> Tags { get; set; }

        /// <summary>
        /// ApplicationPlatformArn that is obtained from AWS SNS by creating platform application with certificates or api keys
        /// </summary>
        public string ApplicationPlatformArn { get; set; }

        /// <summary>
        /// Validate and make the request 
        /// </summary>
        public async Task<RegisterResult> SendAsync()
        {
            try
            {
                Validate();
                RegisterOperator registerService = new RegisterOperator(base.Provider);
                RegisterResult result = await registerService.RegisterSubscriberAsync(new RegisterSubscriberModel(this));
                return result;
            }
            catch (AWSMobilePushNotificationServiceException ex)
            {
                return new RegisterFailedResult(ex);
            }
            catch (Exception ex)
            {
                if (Provider.CatchAllExceptions)
                {
                    return new RegisterFailedResult(ex.Message);
                }
                else
                {
                    throw ex;
                }
            }

        }
        private new void Validate()
        {
            string error = base.Validate();
            if (string.IsNullOrEmpty(ApplicationPlatformArn))
            {
                error = error + "ApplicationPlatformArn is empty \n";
            }

            if (!string.IsNullOrEmpty(error))
            {
                throw new ModelInvalidException(error);
            }
        }
    }

    /// <summary>
    /// Switch a subscriber with new one.
    /// A Subscriber is represented with 'UserId,Token' tuple together.
    /// Should be used if 'UserId,Token' tuple is mutated but still represents the same subscriber in the application
    /// (When a mobile user logs out from your application and DeviceId is now the subscriber of the same user)
    /// </summary>
    public class SwitchSubscriberRequest : AMPSSubscriberRequest
    {
        /// <summary>
        /// Default constructor for switch request
        /// </summary>
        /// <param name="provider">Configuration for the library resources</param>
        public SwitchSubscriberRequest(IAWSMobilePushNotificationConfigProvider provider) : base(provider) { }

        /// <summary>
        /// The new UserId to be replaced with
        /// </summary>
        public string NewUserId { get; set; }

        /// <summary>
        /// The new NotificationToken to be replaced with
        /// </summary>
        public string NewNotificationToken { get; set; }

        /// <summary>
        /// ApplicationPlatformArn that is obtained from AWS SNS by creating platform application with certificates or api keys
        /// </summary>
        public string ApplicationPlatformArn { get; set; }

        /// <summary>
        /// Tags which will not be transfered from the old subscriber. New subscriber will not be registered to these tags
        /// </summary>
        public List<PNTag> TagsToIgnore { get; set; }

        /// <summary>
        /// PushNotification Platform of the subscriber
        /// </summary>
        public Platform Platform { get; set; }

        /// <summary>
        /// Validate and make the request 
        /// </summary>
        public async Task<SwitchSubscriberResult> SendAsync()
        {
            try
            {
                Validate();
                SwitchOperator switchService = new SwitchOperator(base.Provider);
                SwitchSubscriberResult result = await switchService.SwitchSubscriberAsync(new SwitchSubscriberModel(this));
                return result;
            }
            catch (AWSMobilePushNotificationServiceException ex)
            {
                return new SwitchSubscriberFailedResult(ex.Message);
            }
            catch (Exception ex)
            {
                if (Provider.CatchAllExceptions)
                {
                    return new SwitchSubscriberFailedResult(ex.Message);
                }
                else
                {
                    throw ex;
                }
            }
        }
        private new void Validate()
        {
            string error = base.Validate();
            if (string.IsNullOrEmpty(ApplicationPlatformArn))
            {
                error = error + "ApplicationPlatformArn is empty \n";
            }
            if (string.IsNullOrEmpty(NewUserId))
            {
                error = error + "NewUserId is empty \n";
            }
            if (string.IsNullOrEmpty(NewNotificationToken))
            {
                error = error + "NewNotificationToken is empty \n";
            }

            if (!string.IsNullOrEmpty(error))
            {
                throw new ModelInvalidException(error);
            }
        }
    }
}
