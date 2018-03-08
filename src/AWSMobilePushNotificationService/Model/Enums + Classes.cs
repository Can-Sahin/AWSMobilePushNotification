using System;
using AWSMobilePushNotificationService.Model.DynamoDb.Tables;

namespace AWSMobilePushNotificationService.Model
{
    /// <summary>
    /// PushNotification Platform of a device
    /// </summary>
    public enum Platform : int
    {
        /// <summary>
        /// APNS Platform
        /// </summary>
        APNS = 1,

        /// <summary>
        /// GCM Platform
        /// </summary>
        GCM = 2
    }

    /// <summary>
    /// Determine the PushNotification's platform. It is required because 'Sandbox' requires APNS_SANDBOX field in an SNS Publish Message.
    /// </summary>
    public enum TargetEnvironmentType : int
    {
        /// <summary>
        /// No specific platform at SNS
        /// </summary>
        None = 0,

        /// <summary>
        /// SANDBOX platform at SNS
        /// </summary>
        Sandbox = 1,

    }

    /// <summary>
    /// A PushNotification Tag can be in 2 types (Iterative , SNSTopic )
    /// 
    /// Check docs for the explanation
    /// </summary>
    public enum PNTagType : int
    {
        /// <summary>
        /// Tag that will be processed sequentially in dynamoDB. Suitable for small and frequently updated Tags.
        /// 
        /// For Example: People that will be notified when Forum Thread is closed.
        /// </summary>
        Iterative = 0,

        /// <summary>
        /// Tag that belongs to a SNS Topic and not processed sequentially in dynamoDB. Suitable for batch notifications to Tags that are NOT updated frequently.
        ///
        /// For Example: All users that are using the previous versions of the my application
        /// </summary>
        SNSTopic = 1
    }

    /// <summary>
    /// Represents a PushNotification Tag 
    /// </summary>
    public class PNTag
    {
        /// <summary>
        /// Tag name of type string
        /// </summary>
        public string Tag { get; set; }
    }

    /// <summary>
    /// Represents a PushNotification Tag and specifies its type (Iterative , SNSTopic )
    /// </summary>
    public class PNAttributedTag : PNTag
    {
        /// <summary>
        /// Type of the Tag
        /// </summary>
        public PNTagType TagMethod { get; set; }
    }

    /// <summary>
    /// Types of a result when published to a Tag
    /// </summary>
    public enum PublishToTagResultType
    {
        /// <summary>
        /// Publish is made to a SNS Topic
        /// </summary>
        PublishedToSNSTopic,

        /// <summary>
        /// Publish is made to SNS Endpoints
        /// </summary>
        PublishedToEndpoints,

    }

    /// <summary>
    /// Collection of reasons for the operation's failure
    /// </summary>
    public enum ErrorReason
    {
        /// <summary>
        /// User not found in DynamoDB
        /// </summary>
        UserNotFound,

        /// <summary>
        /// Tag is already at one type(iterative, snstopic) and being re-assigned to another type
        /// </summary>
        OverrideExistingTagType,

        /// <summary>
        /// Tag is already at one type(iterative, snstopic) and being re-assigned to another type
        /// </summary>
        SubscriberNotFound,

        /// <summary>
        /// Tag lookup in DynamoDB returns null
        /// </summary>
        TagNotFound,

        /// <summary>
        /// Platform is neighter APNS nor GCM
        /// </summary>
        PlatformUnknown,

        /// <summary>
        /// Platform is unmatched with the target platform
        /// </summary>
        PlaformUnmatched,

        /// <summary>
        /// Tag tables are not created in DynamoDB yet wants to be used
        /// </summary>
        TaggingNotAvailable,

        /// <summary>
        /// Both of the APNS and GCM messages are null at the time of publishing
        /// </summary>
        APNSGCMMessageNull,

        /// <summary>
        /// The model that will be used or passed to another function is invalid / incomplete to proceed
        /// </summary>
        ModelInvalid,

        /// <summary>
        /// 'SNS Endpoint Disabled' response from SNS Endpoint
        /// </summary>
        SNSEndpointDisabled,

        /// <summary>
        /// 'SNS Endpoint NotFound' response from SNS Endpoint
        /// </summary>
        SNSEndpointNotFound,

        /// <summary>
        /// SNS notification structured message is null
        /// </summary>
        SNSNotificationMessageNullException,
    }

    /// <summary>
    /// Subscriber representing a User and NotificationToken tuple. 
    /// </summary>
    public struct PushNotificationSubscriber
    {
        /// <summary>
        /// Id of the User
        /// </summary>
        public string UserId { get; set; }

        /// <summary>
        /// Notification Token of the user retrived from mobile devices
        /// </summary>
        public string Token { get; set; }

        // public string PrimaryKeyValue => ToString();

        /// <summary>
        /// UserId and Token concatenated 
        /// </summary>
        public override string ToString()
        {
            return string.Format("{0}:::{1}", UserId, Token);
        }

        internal static PushNotificationSubscriber From(Subscriber subscriber)
        {
            return new PushNotificationSubscriber { UserId = subscriber.UserId, Token = subscriber.Token };
        }
    }

}
