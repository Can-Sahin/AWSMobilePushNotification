using AWSMobilePushNotificationService.Model.OperatorModel;
using AWSMobilePushNotificationService.Model.DynamoDb.Tables;
using System.Threading.Tasks;
using AWSMobilePushNotificationService.Operators.DynamoDB.Table;
using AWSMobilePushNotificationService.Model.Results.Publish;
using AWSMobilePushNotificationService.Model.Exceptions;
using AWSMobilePushNotificationService.Operators.Registration;
using System.Collections.Generic;
using System;
using AWSMobilePushNotificationService.Model;
using System.Linq;

namespace AWSMobilePushNotificationService.Operators.Publish
{
    internal class PublishToUserOperator : PublishOperator
    {
        private PublishToUserOperatorModel model { get; }
        private DynamoSubscribersTableOperator subscriberTableOperator { get; }
        private UnRegisterOperator unRegisterOperator { get; }

        public PublishToUserOperator(PublishToUserOperatorModel model, IAWSMobilePushNotificationConfigProvider provider) : base(model, provider)
        {
            this.model = model;
            this.subscriberTableOperator = new DynamoSubscribersTableOperator(provider);
            this.unRegisterOperator = new UnRegisterOperator(provider);
        }

        public async Task<List<PublishToSNSResult>> PublishToUserAsync()
        {
            List<DynamoSubscriber> subscribers = await subscriberTableOperator.GetAllSubcribersOfUserAsync(model.UserId);

            if (subscribers == null || subscribers.Count == 0)
            {
                throw new UserNotFoundException(model.UserId);
            }
            List<PublishToSNSResult> results = new List<PublishToSNSResult>();
            foreach (var subscriber in subscribers)
            {
                var result = await PublishToSubscriberAsync(subscriber);
                results.Add(result);
            }
            // await Task.WhenAll(subscribers.Select(subscriber => {
            //     return PublishToSubscriberAsync(subscriber).ContinueWith((tresult => {
            //         var result = tresult.Result;
            //         results.Add(result);
            //     }));
            // }));
            return results;
        }
        public async Task<PublishToSNSResult> PublishToSubscriberAsync(string token)
        {
            DynamoSubscriber subscriber = await subscriberTableOperator.GetSubscriberAsync(model.UserId, token);

            return await PublishToSubscriberAsync(subscriber);
        }
        private async Task<PublishToSNSResult> PublishToSubscriberAsync(DynamoSubscriber subscriber)
        {
            PublishToSNSResult result;
            if (subscriber == null)
            {
                result = new PublishToSNSFailedResult(new SubscriberNotFoundException(subscriber.Subscriber.UserId, subscriber.Subscriber.Token));
                return result;
                // throw new SubscriberNotFoundException(subscriber.Subscriber.UserId, subscriber.Subscriber.Token);
            }
            if (IsTargetPlatformRestrictsSubscriber(model.TargetPlatform, subscriber.Platform))
            {
                result = new PublishToSNSFailedResult(new PlatformUnmatchedException(model.TargetPlatform, subscriber.Platform));
                return result;
                // return new PublishToSNSFailedResult("TargetPlatform doesn't match with the subscribers platform");
            }

            string message = base.SerializeMessageFromProperties(subscriber.Platform);
            if (string.IsNullOrEmpty(message))
            {
                result = new PublishToSNSFailedResult(new SNSNotificationMessageNullException());
                return result;
            }

            // Console.WriteLine("SerializedMessage: " + message);

            result = await PublishToEndpointAsync(subscriber.EndpointARN, message);
            result.UserId = subscriber.UserId;
            if (result.ErrorAlias == ErrorReason.SNSEndpointDisabled
                || result.ErrorAlias == ErrorReason.SNSEndpointNotFound)
            {
                await unRegisterOperator.UnRegisterSubscriberAsync(subscriber.UserId, subscriber.NotificationToken);
            }

            return result;
        }
    }
}
