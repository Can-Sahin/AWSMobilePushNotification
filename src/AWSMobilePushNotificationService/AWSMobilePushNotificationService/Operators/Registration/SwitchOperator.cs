
using AWSMobilePushNotificationService.Operators.DynamoDB.Table;
using AWSMobilePushNotificationService.Operators.Tagging;
using AWSMobilePushNotificationService.Model.OperatorModel;
using System.Threading.Tasks;
using AWSMobilePushNotificationService.Model.Results;
using AWSMobilePushNotificationService.Model.DynamoDb.Tables;
using System.Collections.Generic;
using System.Linq;
using AWSMobilePushNotificationService.Model;

namespace AWSMobilePushNotificationService.Operators.Registration
{
    internal class SwitchOperator : AWSMobilePushNotificationOperator
    {
        protected DynamoSubscribersTableOperator subscribersTableOperator { get; }
        private TagOperator tagOperator { get; }
        private UnRegisterOperator unRegisterOperator { get; }
        private RegisterOperator registerOperator { get; }

        public SwitchOperator(IAWSMobilePushNotificationConfigProvider provider) : base(provider)
        {
            subscribersTableOperator = new DynamoSubscribersTableOperator(provider);
            tagOperator = new TagOperator(provider);
            unRegisterOperator = new UnRegisterOperator(provider);
            registerOperator = new RegisterOperator(provider);
        }

        public async Task<SwitchSubscriberResult> SwitchSubscriberAsync(SwitchSubscriberModel model)
        {
            DynamoSubscriber subscriber = await subscribersTableOperator.GetSubscriberAsync(model.PrevUserId, model.PrevToken);
            List<IDynamoTagEntry> tags = null;
            if (subscriber != null)
            {
                tags = await tagOperator.GetAllTagsForSubscribers(new List<Subscriber> { subscriber.Subscriber });
                var unregisterResult = await unRegisterOperator.UnRegisterSubscriberAsync(subscriber, tags);
            }

            var atributedTags = tags?.Where(t => {
                if(model.TagsToIgnore?.Any(tIgnore => tIgnore.Tag.Equals(t.Tag)) ?? false ){
                    return false;
                }
                else{
                    return true;
                }
            }).Select(t => new PNAttributedTag { Tag = t.Tag, TagMethod = t.TagType }).ToList();
            
            if(subscriber == null){
                subscriber = new DynamoSubscriber();
            }
            subscriber.UserId = model.NewUserId;
            subscriber.NotificationToken = model.NewToken;
            subscriber.Platform = model.Platform;

            await registerOperator.RegisterSubscriberAsync(new RegisterSubscriberModel(subscriber, atributedTags, model.ApplicationPlatformArn));

            return new SwitchSubscriberSuccessfulResult();
        }
    }
}
