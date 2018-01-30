using Newtonsoft.Json;
using System.Collections.Generic;
using System.Threading.Tasks;
using AWSMobilePushNotificationService.Model;
using AWSMobilePushNotificationService.Operators.Publish;
using AWSMobilePushNotificationService.Model.Exceptions;
using AWSMobilePushNotificationService.Model.Results.Publish;
using AWSMobilePushNotificationService.Model.OperatorModel;
using AWSMobilePushNotificationService.Model.Notification;
using System;

namespace AWSMobilePushNotificationService
{

    /// <summary>
    /// Abstract Request base for Publish
    /// </summary>
    public abstract class PublishRequestBase : AMPSRequestBase
    {
        internal PublishRequestBase(IAWSMobilePushNotificationConfigProvider provider) : base(provider) { }

        /// <summary>
        /// TTL for SNS Mobile Push Notification in seconds
        /// </summary>
        public int? TimeToLive { get; set; }

        /// <summary>
        /// Determine the PushNotification's platform. It is required because 'Sandbox' requires APNS_SANDBOX field in an SNS Publish Message.
        /// </summary>
        public TargetEnvironmentType TargetEnvironment { get; set; }

        /// <summary>
        /// Default message for SNS to use.
        /// </summary>
        public string SnsDefaultMessage { get; set; }
    }

    /// <summary>
    /// Abstract PublishRequest
    /// </summary>
    public abstract class PublishRequest : PublishRequestBase
    {
        internal PublishRequest(IAWSMobilePushNotificationConfigProvider provider) : base(provider) { }

        /// <summary>
        /// Specific platform that the notification will only be valid for.
        /// If the target device's platform doesn't match, publishing will result in failure before publishing to SNS
        /// </summary>
        public Platform? TargetPlatform { get; set; }

        /// <summary>
        /// Generic payload for both APNS and GCM platforms
        /// </summary>
        public NotificationPayload NotificationPayload { get; private set; }

        /// <summary>
        /// Payload for only APNS platform
        /// </summary>
        public APNSNotificationPayload APNSNotificationPayload { get; private set; }

        /// <summary>
        /// Payload for only GCM platform
        /// </summary>
        public GCMNotificationPayload GCMNotificationPayload { get; private set; }

        /// <summary>
        /// Set the payload for this notification publish request
        /// </summary>
        /// <param name="payload"> Payload of the notification for both APNS and GCM </param>
        public void SetPayload(NotificationPayload payload)
        {
            this.NotificationPayload = payload;
        }

        /// <summary>
        /// Set the payload for this notification publish request
        /// </summary>
        /// <param name="apnsPayload"> Payload for only APNS </param>
        /// <param name="gcmPayload"> Payload for only GCM </param>
        public void SetPayload(APNSNotificationPayload apnsPayload, GCMNotificationPayload gcmPayload)
        {
            this.APNSNotificationPayload = apnsPayload;
            this.GCMNotificationPayload = gcmPayload;
        }

        /// <summary>
        /// Validate the request model
        /// </summary>
        protected string Validate()
        {
            string error = string.Empty;
            if (NotificationPayload == null && APNSNotificationPayload == null && GCMNotificationPayload == null)
            {
                error = error + "Payload is empty \n";
            }
            return error;
        }

    }


    /// <summary>
    /// Sends notification to a 'User' specified with a UserId(string). There can be multiple subscriptions of the user
    /// </summary>
    public class PublishToUserRequest : PublishRequest
    {
        /// <summary>
        /// Default constructor for publish request
        /// </summary>
        /// <param name="provider">Configuration for the library resources</param>
        public PublishToUserRequest(IAWSMobilePushNotificationConfigProvider provider) : base(provider) { }

        /// <summary>
        /// User to publish to
        /// </summary>
        public string UserId { get; set; }

        /// <summary>
        /// Validate and make the request.
        /// </summary>
        /// <returns> List of results corresponding to the each subscriber of the user</returns>
        public async Task<List<PublishToSNSResult>> SendAsync()
        {
            try
            {
                Validate();
                PublishToUserOperator publishService = new PublishToUserOperator(new PublishToUserOperatorModel(this), base.Provider);
                List<PublishToSNSResult> results = await publishService.PublishToUserAsync();
                return results;
            }
            catch (AWSMobilePushNotificationServiceException ex)
            {
                return new List<PublishToSNSResult> { new PublishToSNSFailedResult(ex) };
            }
            catch (Exception ex)
            {
                if (Provider.CatchAllExceptions)
                {
                    return new List<PublishToSNSResult> { new PublishToSNSFailedResult(ex.Message) };
                }
                else
                {
                    throw ex;
                }
            }
        }

        /// <summary>
        /// Validate the request model
        /// </summary>
        protected new void Validate()
        {
            string error = base.Validate();
            if (string.IsNullOrEmpty(UserId))
            {
                error = error + "UserId is empty \n";
            }
            if (!string.IsNullOrEmpty(error))
            {
                throw new ModelInvalidException(error);
            }
        }
    }

    /// <summary>
    /// Sends notification to a Subscriber. Subscriber is UserId,NotificationToken tuple. It represent a specific device to publish to
    /// </summary>
    public class PublishToSubscriberRequest : PublishToUserRequest
    {
        /// <summary>
        /// Default constructor for publish request
        /// </summary>
        /// <param name="provider">Configuration for the library resources</param>
        public PublishToSubscriberRequest(IAWSMobilePushNotificationConfigProvider provider) : base(provider) { }

        /// <summary>
        /// User's NotificationToken for a specific device
        /// </summary>
        public string NotificationToken { get; set; }

        /// <summary>
        /// Validate and make the request.
        /// </summary>
        public new async Task<PublishToSNSResult> SendAsync()
        {
            try
            {
                Validate();
                PublishToUserOperator publishService = new PublishToUserOperator(new PublishToUserOperatorModel(this), base.Provider);
                PublishToSNSResult result = await publishService.PublishToSubscriberAsync(NotificationToken);
                return result;
            }
            catch (AWSMobilePushNotificationServiceException ex)
            {
                return new PublishToSNSFailedResult(ex.Message);
            }
            catch (Exception ex)
            {
                if (Provider.CatchAllExceptions)
                {
                    return new PublishToSNSFailedResult(ex.Message);
                }
                else
                {
                    throw ex;
                }
            }

        }
        private new void Validate()
        {
            base.Validate();
            string error = string.Empty;
            if (string.IsNullOrEmpty(NotificationToken))
            {
                error = error + "NotificationToken is empty \n";
            }
            if (!string.IsNullOrEmpty(error))
            {
                throw new ModelInvalidException(error);
            }
        }
    }

    /// <summary>
    /// Sends notification to a Tag
    /// </summary>
    public class PublishToTagRequest : PublishRequest
    {

        /// <summary>
        /// Default constructor for publish request
        /// </summary>
        /// <param name="provider">Configuration for the library resources</param>
        
        public PublishToTagRequest(IAWSMobilePushNotificationConfigProvider provider) : base(provider) { }

        /// <summary>
        /// Tag to publish to
        /// </summary>
        public string Tag { get; set; }

        /// <summary>
        /// Validate and make the request.
        /// </summary>
        /// <returns> A result that can be casted to either PublishToIterativeTagResult or PublishToSNSTagResult by examining the value of Resultype.
        /// Because 'Tag' will be resolved later and return values are different for different tag types</returns>
        public async Task<PublishToTagResult> SendAsync()
        {
            try
            {
                Validate();
                PublishToTagOperator publishService = new PublishToTagOperator(new PublishToTagOperatorModel(this), base.Provider);
                PublishToTagResult results = await publishService.PublishToTagAsync();
                return results;
            }
            catch (AWSMobilePushNotificationServiceException ex)
            {
                return new PublishToTagFailedResult(ex);
            }
            catch (Exception ex)
            {
                if (Provider.CatchAllExceptions)
                {
                    return new PublishToTagFailedResult(ex.Message);
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
            if (string.IsNullOrEmpty(Tag))
            {
                error = error + "TagName is empty \n";
            }
            if (!string.IsNullOrEmpty(error))
            {
                throw new ModelInvalidException(error);
            }
        }
    }

}
