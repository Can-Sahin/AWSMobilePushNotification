using AWSMobilePushNotificationService.Model.Exceptions;

namespace AWSMobilePushNotificationService.Model.Results.Publish
{
    #pragma warning disable 1591

    /// <summary>
    /// Abstract class represents the result of a Publish operation.
    /// Result has to be of type of one of the followings: 'PublishToSNSResult', 'PublishToTagResult'.
    /// </summary>
    public abstract class PublishResult : OperationResult
    {
        /// <summary>
        /// MessageId returned from AWS SNS Publish
        /// </summary>
        public abstract string MessageId { get; set;}
        public PublishResult() : base() { }

        public PublishResult(string errMsg) : base(errMsg) { }

        public PublishResult(string errMsg, ErrorReason errAlias) : base(errMsg, errAlias) { }

        public PublishResult(AWSMobilePushNotificationServiceException ex) : base(ex) { }
    }
    #pragma warning restore 1591
}
