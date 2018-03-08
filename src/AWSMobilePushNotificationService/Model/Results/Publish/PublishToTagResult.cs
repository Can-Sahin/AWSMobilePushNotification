using System;
using System.Collections.Generic;
using AWSMobilePushNotificationService.Model.DynamoDb.Tables;
using AWSMobilePushNotificationService.Model.Exceptions;

namespace AWSMobilePushNotificationService.Model.Results.Publish
{
    #pragma warning disable 1591

    /// <summary>
    /// Abstract class represents the result of a Publish operation to a Tag.
    /// Result has to be of type of one of the followings: 'PublishToTagFailedResult', 'PublishToSNSTagResult', 'PublishToIterativeTagResult'.
    /// It can be casted to one of them by examining the 'ResultType' field
    /// </summary>
    public abstract class PublishToTagResult : PublishResult
    {
        /// <summary>
        /// Result Type returned from publish to tag operation. Null if failed
        /// </summary>
        public abstract PublishToTagResultType? ResultType { get; }

        /// <summary>
        /// Tag that was published to. Its only set after a succesful sns topic publish 
        /// </summary>
        public string Tag { get; set; }
        public PublishToTagResult() : base() { }
        public PublishToTagResult(string errMsg) : base(errMsg) { }
        public PublishToTagResult(string errorMessage, ErrorReason errAlias) : base(errorMessage, errAlias) { }
        public PublishToTagResult(AWSMobilePushNotificationServiceException ex) : base(ex) { }

    }

    /// <summary>
    /// Represent a failed operation's result
    /// </summary>
    public class PublishToTagFailedResult : PublishToTagResult
    {
        /// <summary>
        /// Result Type returned from publish to tag operation
        /// </summary>
        public override PublishToTagResultType? ResultType => null;

        public override string MessageId { get { return null; } set { } }

        public PublishToTagFailedResult(string errorMessage) : base(errorMessage) { }
        public PublishToTagFailedResult(string errorMessage, ErrorReason errAlias) : base(errorMessage, errAlias) { }
        public PublishToTagFailedResult(AWSMobilePushNotificationServiceException ex) : base(ex) { }

    }

    /// <summary>
    /// Represent a successful publish to SNSTopicTag
    /// </summary>
    public class PublishToSNSTopicTagResult : PublishToTagResult
    {
        /// <summary>
        /// Result Type returned from publish to tag operation
        /// </summary>
        public override PublishToTagResultType? ResultType => PublishToTagResultType.PublishedToSNSTopic;

        public override string MessageId { get; set; }
        public PublishToSNSTopicTagResult(string tag, PublishToSNSResult snsResult)
        {
            if (snsResult.IsSuccessful && snsResult is PublishToSNSSuccessfulResult)
            {
                this.MessageId = ((PublishToSNSSuccessfulResult)snsResult).MessageId;
                this.Tag = tag;
            }
            else
            {
                base.IsSuccessful = false;
                base.ErrorMessage = snsResult.ErrorMessage;
                base.ErrorAlias = snsResult.ErrorAlias;
            }
        }
    }

    /// <summary>
    /// Represent a successful publish to IterativeTag
    /// </summary>
    public class PublishToIterativeTagResult : PublishToTagResult
    {
        /// <summary>
        /// Result Type returned from publish to tag operation
        /// </summary>
        public override PublishToTagResultType? ResultType => PublishToTagResultType.PublishedToEndpoints;

        /// <summary>
        /// MessageId returned from AWS SNS Publish
        /// </summary>
        public override string MessageId { get { return null; } set { } }

        /// <summary>
        /// Result retrieved from publishing to each endpoint
        /// </summary>
        /// <returns></returns>
        public IEnumerable<Tuple<PublishToSNSResult, PushNotificationSubscriber>> EndpointResults { get; set; }

        public PublishToIterativeTagResult(string tag, IEnumerable<Tuple<PublishToSNSResult, PushNotificationSubscriber>> EndpointResults)
        {
            this.EndpointResults = EndpointResults;
            this.Tag = Tag;
        }

    }
    #pragma warning restore 1591
}
