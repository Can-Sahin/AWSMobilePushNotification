using Amazon.SimpleNotificationService.Model;
using AWSMobilePushNotificationService.Model.OperatorModel;
using AWSMobilePushNotificationService.Model.DynamoDb.Tables;
using System.Threading.Tasks;
using AWSMobilePushNotificationService.Model.Results;
using AWSMobilePushNotificationService.Operators.Tagging;
using System.Collections.Generic;
using System.Linq;
using AWSMobilePushNotificationService.Model;

namespace AWSMobilePushNotificationService.Operators.Registration
{

    internal class UnRegisterOperator : RegistrationOperator
    {
        private TagOperator tagOperator { get; }

        public UnRegisterOperator(IAWSMobilePushNotificationConfigProvider provider) : base(provider)
        {
            tagOperator = new TagOperator(provider);
        }

        public async Task<UnRegisterUserSuccessfulResult> UnRegisterUserAsync(string userId)
        {
            UnRegisterUserSuccessfulResult result = new UnRegisterUserSuccessfulResult();
            List<DynamoSubscriber> subscribers = await subscribersTableOperator.GetAllSubcribersOfUserAsync(userId);

            if (subscribers.Count == 0)
            {
                result.NotRegistered = true;
                return result;
            }
            if (tagOperator.IsTaggingAvailable())
            {
                var tagSubscibers = subscribers.Select(s => new Subscriber { UserId = s.UserId, Token = s.NotificationToken }).ToList();
                await tagOperator.RemoveSubscribers(tagSubscibers);
            }
            foreach (var subscriber in subscribers)
            {
                DeleteEndpointRequest deReq = new DeleteEndpointRequest();
                deReq.EndpointArn = subscriber.EndpointARN;
                await snsClient.DeleteEndpointAsync(deReq);
                await subscribersTableOperator.RemoveSubscriberAsync(subscriber);
            }

            return result;
        }
        public async Task<UnRegisterSubscriberSuccessfulResult> UnRegisterSubscriberAsync(string userId, string token)
        {
            DynamoSubscriber entry = await subscribersTableOperator.GetSubscriberAsync(userId, token);
            return await UnRegisterSubscriberAsync(entry);
        }
        public async Task<UnRegisterSubscriberSuccessfulResult> UnRegisterSubscriberAsync(DynamoSubscriber subscriber)
        {
            UnRegisterSubscriberSuccessfulResult result = new UnRegisterSubscriberSuccessfulResult();
            List<IDynamoTagEntry> tags = new List<IDynamoTagEntry>();
            if (tagOperator.IsTaggingAvailable())
            {
                var subscribers = new List<Subscriber> { new Subscriber { UserId = subscriber.UserId, Token = subscriber.NotificationToken } };
                tags = await tagOperator.GetAllTagsForSubscribers(subscribers);
            }
            return await UnRegisterSubscriberAsync(subscriber,tags);
        }
        public async Task<UnRegisterSubscriberSuccessfulResult> UnRegisterSubscriberAsync(DynamoSubscriber subscriber, List<IDynamoTagEntry> tags)
        {
            UnRegisterSubscriberSuccessfulResult result = new UnRegisterSubscriberSuccessfulResult();
            if (subscriber != null)
            {
                if (tagOperator.IsTaggingAvailable())
                {
                    await tagOperator.RemoveTagSubscribers(tags);
                }
                DeleteEndpointRequest deReq = new DeleteEndpointRequest();
                deReq.EndpointArn = subscriber.EndpointARN;
                await snsClient.DeleteEndpointAsync(deReq);
                await subscribersTableOperator.RemoveSubscriberAsync(subscriber);
            }
            else
            {
                result.NotRegistered = true;
                return result;
            }

            return result;
        }

    }
}
