using System;
using AWSMobilePushNotificationService.Model.Results;

namespace AWSMobilePushNotificationService.Model.Exceptions
{
    #pragma warning disable 1591

    /// <summary>
    /// Base Exception that AWSMobilePushNotification can produce
    /// </summary>
    public abstract class AWSMobilePushNotificationServiceException : Exception
    {
        internal abstract ErrorReason ErrorAlias { get; }
        public AWSMobilePushNotificationServiceException(string message) : base(message) { }

    }

    /// <summary>
    /// Exception when Tag is already at one type(iterative, snstopic) and being re-assigned to another type
    /// </summary>
    public class OverrideExistingTagTypeException : AWSMobilePushNotificationServiceException
    {
        internal override ErrorReason ErrorAlias => ErrorReason.OverrideExistingTagType;

        public OverrideExistingTagTypeException(string tag, string type) :
                        base(String.Format("Overriding tag type is forbidden. Tag: {0} , Type: {1}", tag, type))
        { }

    }

    /// <summary>
    /// Exception when User,NotificationToken lookup in DynamoDB returns null
    /// </summary>
    public class SubscriberNotFoundException : AWSMobilePushNotificationServiceException
    {
        internal override ErrorReason ErrorAlias => ErrorReason.SubscriberNotFound;

        public SubscriberNotFoundException(string userId, string token) : base(String.Format("Subscriber Not Found. UserId: {0} , Token: {1} ", userId, token)) { }

    }
    /// <summary>
    /// Exception when User lookup in DynamoDB returns null
    /// </summary>
    public class UserNotFoundException : AWSMobilePushNotificationServiceException
    {
        internal override ErrorReason ErrorAlias => ErrorReason.UserNotFound;

        public UserNotFoundException(string userId) : base(String.Format("User Not Found. UserId: {0}", userId)) { }

    }

    /// <summary>
    /// Exception when Tag lookup in DynamoDB returns null
    /// </summary>
    public class TagNotFoundException : AWSMobilePushNotificationServiceException
    {
        internal override ErrorReason ErrorAlias => ErrorReason.TagNotFound;

        public TagNotFoundException(string tag) : base(String.Format("Tag Not Found. Tag: {0}", tag)) { }
    }

    /// <summary>
    /// Exception when Tag tables are not created in DynamoDB yet wants to be used
    /// </summary>
    public class TaggingNotAvailableException : AWSMobilePushNotificationServiceException
    {
        internal override ErrorReason ErrorAlias => ErrorReason.TaggingNotAvailable;

        public TaggingNotAvailableException() : base(String.Format("Tag is not available")) { }
    }

    /// <summary>
    /// Exception when Platform is neighter APNS nor GCM
    /// </summary>
    public class PlatformUnknownException : AWSMobilePushNotificationServiceException
    {
        internal override ErrorReason ErrorAlias => ErrorReason.PlatformUnknown;

        public PlatformUnknownException() : base("Platform is unknown. Neighter APNS nor GCM") { }

    }
    /// <summary>
    /// Exception when Platform is unmatched with the target platform
    /// </summary>
    public class PlatformUnmatchedException : AWSMobilePushNotificationServiceException
    {
        internal override ErrorReason ErrorAlias => ErrorReason.PlaformUnmatched;

        public PlatformUnmatchedException(Platform? target, Platform? subscriber) : base(string.Format("Target platform {0} is not matched with {1}", target.ToString(), subscriber.ToString())) { }

    }

    /// <summary>
    /// Exception when SNS notification structured message is null
    /// </summary>
    public class SNSNotificationMessageNullException : AWSMobilePushNotificationServiceException
    {
        internal override ErrorReason ErrorAlias => ErrorReason.SNSNotificationMessageNullException;

        public SNSNotificationMessageNullException() : base("SNS notification structured message is null") { }

    }

    /// <summary>
    /// Exception when both of the APNS and GCM messages are null at the time of publishing
    /// </summary>
    public class APNSGCMMessageNullException : AWSMobilePushNotificationServiceException
    {
        internal override ErrorReason ErrorAlias => ErrorReason.APNSGCMMessageNull;

        public APNSGCMMessageNullException() : base("APNS and GCM message fields are both NULL") { }

    }

    /// <summary>
    /// Exception when the model that will be used or passed to another function is invalid / incomplete to proceed
    /// </summary>
    public class ModelInvalidException : AWSMobilePushNotificationServiceException
    {
        internal override ErrorReason ErrorAlias => ErrorReason.ModelInvalid;

        public ModelInvalidException(string msg) : base(String.Format("Model is invalid: {0}", msg)) { }
    }
    #pragma warning restore 1591

}
