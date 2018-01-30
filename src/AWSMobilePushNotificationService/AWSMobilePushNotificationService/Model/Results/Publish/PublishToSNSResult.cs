using AWSMobilePushNotificationService.Model.Exceptions;

namespace AWSMobilePushNotificationService.Model.Results.Publish
{
    #pragma warning disable 1591

    /// <summary>
    /// Abstract class represents the result of a Publish operation to a SNS Endpoint.
    /// Result has to be of type of one of the followings:
    /// 'PublishToSNSFailedResult', 'PublishToSNSSuccessfulResult', 'PublishToSNSEndpointDisabledResult', 'PublishToSNSEndpointNotFoundResult' .
    /// It can be casted to one of them by examining the 'ResultType' field
    /// </summary>
    public abstract class PublishToSNSResult : PublishResult
    {
        /// <summary>
        /// User that the notification was published to
        /// </summary>
        public string UserId { get; set; }
        public PublishToSNSResult() : base() { }
        public PublishToSNSResult(string errMsg) : base(errMsg) { }
        public PublishToSNSResult(string errMsg, ErrorReason errAlias) : base(errMsg, errAlias) { }
        public PublishToSNSResult(AWSMobilePushNotificationServiceException ex) : base(ex) { }
    }

    /// <summary>
    /// Represent a failed publish operation to SNS Endpoint
    /// </summary>
    public class PublishToSNSFailedResult : PublishToSNSResult
    {
        /// <summary>
        /// MessageId returned from AWS SNS Publish
        /// </summary>
        public override string MessageId { get { return null; } set { } }
        public PublishToSNSFailedResult(string errMsg) : base(errMsg) { }
        public PublishToSNSFailedResult(string errMsg, ErrorReason errAlias) : base(errMsg, errAlias) { }
        public PublishToSNSFailedResult(AWSMobilePushNotificationServiceException ex) : base(ex) { }
    }

    /// <summary>
    /// Represent a successful publish to SNS Endpoint
    /// </summary>
    public class PublishToSNSSuccessfulResult : PublishToSNSResult
    {
        /// <summary>
        /// MessageId returned from SNS Endpoint Publish
        /// </summary>
        public override string MessageId { get; set; }

        public PublishToSNSSuccessfulResult(string messageId)
        {
            this.MessageId = messageId;
        }
    }
    /// <summary>
    /// Represent 'SNS Endpoint Disabled' response from SNS Endpoint
    /// </summary>
    public class PublishToSNSEndpointDisabledResult : PublishToSNSFailedResult
    {
        public PublishToSNSEndpointDisabledResult() : base("Endpoint Disabled", ErrorReason.SNSEndpointDisabled) { }


    }
    /// <summary>
    /// Represent 'SNS Endpoint NotFound' response from SNS Endpoint
    /// </summary>
    public class PublishToSNSEndpointNotFoundResult : PublishToSNSFailedResult
    {
        public PublishToSNSEndpointNotFoundResult() : base("Endpoint Not Found", ErrorReason.SNSEndpointNotFound) { }

    }
    #pragma warning restore 1591
}
