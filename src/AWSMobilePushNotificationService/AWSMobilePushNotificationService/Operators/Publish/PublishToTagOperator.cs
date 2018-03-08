using AWSMobilePushNotificationService.Operators.DynamoDB.Table;
using AWSMobilePushNotificationService.Operators.Tagging;
using AWSMobilePushNotificationService.Model.OperatorModel;
using AWSMobilePushNotificationService.Model.DynamoDb.Tables;
using AWSMobilePushNotificationService.Model.Results.Publish;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AWSMobilePushNotificationService.Model.Exceptions;
using AWSMobilePushNotificationService.Operators.Registration;

using AWSMobilePushNotificationService.Model;

namespace AWSMobilePushNotificationService.Operators.Publish
{
    using IterativeTagResponseTuple = List<Tuple<PublishToSNSResult, DynamoIterativeTag>>;
    internal class PublishToTagOperator : PublishOperator
    {
        private PublishToTagOperatorModel model { get; }
        private TagOperator tagOperator { get; }
        private DynamoSubscribersTableOperator subscriberTableService { get; }
        private UnRegisterOperator unRegisterService { get; }

        public PublishToTagOperator(PublishToTagOperatorModel model, IAWSMobilePushNotificationConfigProvider provider) : base(model, provider)
        {
            this.model = model;
            this.tagOperator = new TagOperator(provider);
            this.subscriberTableService = new DynamoSubscribersTableOperator(provider);
            this.unRegisterService = new UnRegisterOperator(provider);
        }

        public async Task<PublishToTagResult> PublishToTagAsync()
        {
            if (!tagOperator.IsTaggingAvailable())
            {
                return new PublishToTagFailedResult(new TaggingNotAvailableException());
            }

            string tag = model.Tag;
            DynamoTag tagEntry = await tagOperator.tagTableOperator.GetTagAsync(tag);
            if (tagEntry == null)
            {
                return new PublishToTagFailedResult(new TagNotFoundException(tag));
            }
            switch (tagEntry.TaggingTypeEnum)
            {
                case PNTagType.Iterative:
                    List<DynamoIterativeTag> tags = await tagOperator.iterativeTagTableOperator.GetAllSubscribersForTagAsync(tagEntry.Tag);
                    IterativeTagResponseTuple resultTuples = await PublishToIterativeTagAsync(tags);
                    await RemoveUnsuccessfulEndpoints(resultTuples);
                    var returnTuples = resultTuples.Select(t => new Tuple<PublishToSNSResult, PushNotificationSubscriber>(t.Item1, PushNotificationSubscriber.From(t.Item2.Subscriber)));
                    return new PublishToIterativeTagResult(tag, returnTuples);
                case PNTagType.SNSTopic:
                    PublishToSNSResult result = await PublishToSnsTopicTag(tagEntry.SnsTopicArn);
                    return new PublishToSNSTopicTagResult(tag, result);
                default:
                    throw new TagNotFoundException(tag);
            }
        }

        private async Task<IterativeTagResponseTuple> PublishToIterativeTagAsync(List<DynamoIterativeTag> entries)
        {
            // Parallel tasks are not implemented because of AWS-SDK SNS bug in parellel execution cause high memory usage
            // List<Task> tasks = new List<Task>();
            IterativeTagResponseTuple resultTuples = new IterativeTagResponseTuple();
            foreach (var tagEntry in entries)
            {
                if (IsTargetPlatformRestrictsSubscriber(model.TargetPlatform, tagEntry.Platform))
                {
                    var failed = new PublishToSNSFailedResult(new PlatformUnmatchedException(model.TargetPlatform, tagEntry.Platform));
                    resultTuples.Add(new Tuple<PublishToSNSResult, DynamoIterativeTag>(failed, tagEntry));
                    continue;
                }

                string message = SerializeMessageFromProperties(tagEntry.Platform);
                if (string.IsNullOrEmpty(message))
                {
                    var failed = new PublishToSNSFailedResult(new SNSNotificationMessageNullException());
                    resultTuples.Add(new Tuple<PublishToSNSResult, DynamoIterativeTag>(failed, tagEntry));
                    continue;
                }

                var result = await PublishToEndpointAsync(tagEntry.EndpointArn, message);
                result.UserId = tagEntry.Subscriber.UserId;
                resultTuples.Add(new Tuple<PublishToSNSResult, DynamoIterativeTag>(result, tagEntry));
            }
            return resultTuples;
        }
        private async Task<PublishToSNSResult> PublishToSnsTopicTag(string snsTopicArn)
        {
            string message = SerializeMessageFromProperties(null);
            if (string.IsNullOrEmpty(message))
            {
                return new PublishToSNSFailedResult(new SNSNotificationMessageNullException());
            }

            return await PublishToTopicAsync(snsTopicArn, message);
        }
        private async Task RemoveUnsuccessfulEndpoints(IterativeTagResponseTuple resultTuples)
        {
            // Parallel tasks are not implemented because of AWS-SDK SNS bug in parellel execution cause high memory usage
            // List<Task> tasks = new List<Task>();

            foreach (var responseTagTuple in resultTuples)
            {
                PublishToSNSResult result = responseTagTuple.Item1;
                DynamoIterativeTag tag = responseTagTuple.Item2;

                if (result.ErrorAlias == ErrorReason.SNSEndpointDisabled
                    || result.ErrorAlias == ErrorReason.SNSEndpointNotFound)
                {
                    await unRegisterService.UnRegisterSubscriberAsync(tag.Subscriber.UserId, tag.Subscriber.Token);
                }
            }

        }
    }
}
