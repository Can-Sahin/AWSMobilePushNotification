using Amazon.SimpleNotificationService.Model;
using AWSMobilePushNotificationService.Operators.DynamoDB.Table;
using AWSMobilePushNotificationService.Model.DynamoDb.Tables;
using AWSMobilePushNotificationService.Model.Exceptions;
using System.Collections.Generic;
using System.Threading.Tasks;
using AWSMobilePushNotificationService.Model;
using System.Linq;
using AWSMobilePushNotificationService.Model.Results;
using System;

namespace AWSMobilePushNotificationService.Operators.Tagging
{
    internal abstract class TagOperatorBase : AWSMobilePushNotificationOperator
    {
        internal DynamoTagsTableOperator tagTableOperator { get; }
        internal DynamoIterativeTagsTableOperator iterativeTagTableOperator { get; }
        internal DynamoSNSTopicTagsTableOperator snsTopicTagTableOperator { get; }
        public TagOperatorBase(IAWSMobilePushNotificationConfigProvider provider) : base(provider)
        {
            this.tagTableOperator = new DynamoTagsTableOperator(provider);
            this.iterativeTagTableOperator = new DynamoIterativeTagsTableOperator(provider);
            this.snsTopicTagTableOperator = new DynamoSNSTopicTagsTableOperator(provider);
        }
    }
    internal class TagOperator : TagOperatorBase
    {
        private readonly DynamoSubscribersTableOperator subscriberTableOperator;
        public TagOperator(IAWSMobilePushNotificationConfigProvider provider) : base(provider)
        {
            this.subscriberTableOperator = new DynamoSubscribersTableOperator(provider);

        }
        public bool IsTaggingAvailable()
        {
            return tagTableOperator.IsTableExists();
        }
        public async Task<TaggingOperationResult> AddUserToTagsAsync(string userId, List<PNAttributedTag> tags)
        {
            
            List<DynamoSubscriber> subscribers = await subscriberTableOperator.GetAllSubcribersOfUserAsync(userId);
            if (subscribers.Count == 0)
            {
                return new TagCRUDFailedResult(new UserNotFoundException(userId));
            }

            foreach (var subscriber in subscribers)
            {
                await SubscribeToTagsAsync(subscriber, tags);
            }
            TagCRUDSuccessfulResult result = new TagCRUDSuccessfulResult();
            result.NumberOfModifications = subscribers.Count;
            return result;
        }
        public async Task<TagCRUDSuccessfulResult> RemoveUserFromTagsAsync(string userId, List<PNTag> tags, bool removeAllTags = false)
        {
            TagCRUDSuccessfulResult result = new TagCRUDSuccessfulResult();
            if (removeAllTags)
            {
                int modification = await RemoveAllTagsOfUserAsync(userId);
                result.NumberOfModifications = modification;
                return result;
            }
            else
            {
                foreach (var tagToRemove in tags)
                {
                    DynamoTag tag = await tagTableOperator.GetTagAsync(tagToRemove.Tag);
                    if (tag == null)
                    {
                        continue;
                    }
                    List<DynamoSubscriber> dynamoSubscribers = await subscriberTableOperator.GetAllSubcribersOfUserAsync(userId);
                    var subscribers = dynamoSubscribers.Select(s => s.Subscriber).ToList();

                    await RemoveSubscribersFromTagAsync(tag.Tag, subscribers, tag.TaggingTypeEnum);
                    result.NumberOfModifications = result.NumberOfModifications + subscribers.Count;

                }
            }
            return result;

        }
        public async Task<List<PNAttributedTag>> GetAllTagsOfUserAsAtrributedTags(string userId)
        {
            var tags = await GetAllTagsOfUser(userId);
            return tags.Select(t => new PNAttributedTag
            {
                Tag = t.Tag,
                TagMethod = t.TagType,
            }).ToList();

        }
        public async Task<List<IDynamoTagEntry>> GetAllTagsForSubscribers(List<Subscriber> subscribers)
        {
            List<IDynamoTagEntry> tags = new List<IDynamoTagEntry>();
            List<DynamoIterativeTag> iterativeTags = await iterativeTagTableOperator.GetAllTagsForSubscribersAsync(subscribers);
            List<DynamoSNSTopicTag> snsTags = await snsTopicTagTableOperator.GetAllTagsForSubscribersAsync(subscribers);

            tags.AddRange(iterativeTags);
            tags.AddRange(snsTags);
            return tags;
        }
        public async Task<List<IDynamoTagEntry>> GetAllTagsOfUser(string userId)
        {
            List<DynamoSubscriber> dynamoSubscribers = await subscriberTableOperator.GetAllSubcribersOfUserAsync(userId);
            var subscribers = dynamoSubscribers.Select(s => s.Subscriber).ToList();
            var tags = await GetAllTagsForSubscribers(subscribers);
            return tags;

        }
        public async Task SubscribeToTagsAsync(DynamoSubscriber subscriber, List<PNAttributedTag> tags)
        {
            if (tags == null) { return; }
            await Task.WhenAll(tags.Select(tag => SubscribeToTagAsync(subscriber, tag)));
            // foreach (var tag in tags)
            // {
            //     await SubscribeToTagAsync(subscriber, tag);
            // }
        }
        public async Task SubscribeToTagAsync(DynamoSubscriber subscriber, PNAttributedTag tag)
        {
            switch (tag.TagMethod)
            {
                case PNTagType.Iterative:
                    await SubscribeToIterativeTagAsync(tag.Tag, subscriber.UserId, subscriber.NotificationToken, subscriber.EndpointARN, subscriber.Platform);
                    break;
                case PNTagType.SNSTopic:
                    await SubscribeToSNSTopicTagAsync(tag.Tag, subscriber.UserId, subscriber.NotificationToken, base.Provider.AppIdentifier, subscriber.EndpointARN);
                    break;
                default:
                    return;
            }
        }

        public async Task SubscribeToIterativeTagAsync(string iterativeTag, string userId, string token, string endpointArn, Platform platform)
        {
            string tag = iterativeTag;
            if (string.IsNullOrEmpty(tag))
            {
                return;
            }
            DynamoTag pnTag = await tagTableOperator.GetTagAsync(iterativeTag);
            if (pnTag != null && pnTag.TaggingTypeEnum != PNTagType.Iterative)
            {
                throw new OverrideExistingTagTypeException(pnTag.Tag, pnTag.TaggingTypeEnum.ToString());
            }

            if (pnTag == null)
            {
                pnTag = new DynamoTag();
                pnTag.Tag = iterativeTag;
                pnTag.TaggingTypeEnum = PNTagType.Iterative;
                await tagTableOperator.AddTagAsync(pnTag);
            }

            Func<DateTime?> calculateTTL = (() =>
            {
                if (Provider.IterativeTagTimeToLive.HasValue)
                {
                    return DateTime.UtcNow.Add(Provider.IterativeTagTimeToLive.Value);
                }
                return null;
            });

            DynamoIterativeTag entry = new DynamoIterativeTag
            {
                Tag = tag,
                Subscriber = new Subscriber { UserId = userId, Token = token },
                EndpointArn = endpointArn,
                Platform = platform,
                ttl = calculateTTL()
            };

            await iterativeTagTableOperator.AddSubscriberAsync(entry);

            // await tagTableService.IncrementNumberOfSubscribers(pnTag.Tag, 1);

        }

        protected async Task SubscribeToSNSTopicTagAsync(string topicTag, string userId, string token, string appIdentifier, string endpointArn)
        {
            if (string.IsNullOrEmpty(topicTag))
            {
                return;
            }
            DynamoTag pnTag = await tagTableOperator.GetTagAsync(topicTag);

            if (pnTag != null && pnTag.TaggingTypeEnum != PNTagType.SNSTopic)
            {
                throw new OverrideExistingTagTypeException(pnTag.Tag, pnTag.TaggingTypeEnum.ToString());
            }
            string topicArn = pnTag?.SnsTopicArn;
            if (string.IsNullOrEmpty(topicArn))
            {
                CreateTopicRequest ctReq = new CreateTopicRequest();
                ctReq.Name = appIdentifier + "___" + topicTag;
                CreateTopicResponse ctResponse = await snsClient.CreateTopicAsync(ctReq);
                topicArn = ctResponse.TopicArn;
            }


            SubscribeRequest sReq = new SubscribeRequest();
            sReq.TopicArn = topicArn;
            sReq.Protocol = "application";
            sReq.Endpoint = endpointArn;
            SubscribeResponse sResponse = await snsClient.SubscribeAsync(sReq);


            if (pnTag == null)
            {
                pnTag = new DynamoTag();
                pnTag.Tag = topicTag;
                pnTag.TaggingTypeEnum = PNTagType.SNSTopic;
                pnTag.SnsTopicArn = topicArn;
                await tagTableOperator.AddTagAsync(pnTag);
            }

            DynamoSNSTopicTag entry = new DynamoSNSTopicTag
            {
                Tag = topicTag,
                Subscriber = new Subscriber { UserId = userId, Token = token },
                SnsSubscriptionArn = sResponse.SubscriptionArn
            };
            await snsTopicTagTableOperator.AddTagAsync(entry);

            // await tagTableService.IncrementNumberOfSubscribers(pnTag.Tag, 1);

        }

        public async Task<TaggingOperationResult> DeleteTagAsync(PNTag tag)
        {
            DynamoTag pnTag = await tagTableOperator.GetTagAsync(tag.Tag);

            if (pnTag == null)
            {
                return new TagCRUDFailedResult(new TagNotFoundException(tag.Tag));
            }

            int totalDeleted = 0;
            if (pnTag.TaggingTypeEnum == PNTagType.Iterative)
            {
                //Warning: You could delete by rangekey conditional expression. Check later
                var iterativeTags = await iterativeTagTableOperator.GetAllSubscribersForTagAsync(tag.Tag);
                var subscribers = iterativeTags.Select(t => t.Subscriber).ToList();
                await RemoveSubscribersFromIterativeTagAsync(pnTag.Tag, subscribers);
                totalDeleted = totalDeleted + subscribers.Count;
            }

            if (pnTag.TaggingTypeEnum == PNTagType.SNSTopic)
            {
                var snsTags = await snsTopicTagTableOperator.GetAllSubscribersForTagAsync(tag.Tag);
                var subscribers = snsTags.Select(t =>
                {
                    var subscriber = t.Subscriber;
                    subscriber.SNSSubscriptionArn = t.SnsSubscriptionArn;
                    return subscriber;
                }).ToList();
                await RemoveSubscribersFromSNSTopicTagAsync(pnTag.Tag, subscribers);
                totalDeleted = totalDeleted + subscribers.Count;
            }
            await RemoveDynamoTagAsync(pnTag.Tag);

            TagCRUDSuccessfulResult result = new TagCRUDSuccessfulResult();
            result.NumberOfModifications = totalDeleted;
            return result;
        }
        public async Task<int> RemoveAllTagsOfUserAsync(string userId)
        {
            var tags = await GetAllTagsOfUser(userId);
            await RemoveTagSubscribers(tags);

            return tags.Count;
        }

        public async Task RemoveSubscribers(List<Subscriber> subscribers)
        {
            var tags = await GetAllTagsForSubscribers(subscribers);
            var tagCollection = tags.GroupBy(t => t.Tag).Select(g => new DynamoTypedTagCollection { Tag = g.Key, Subscribers = g.Select(s => s.Subscriber).ToList(), TagType = g.First().TagType }).ToList();

            await RemoveSubscribersFromTagsAsync(tagCollection);
        }

        public async Task RemoveTagSubscribers(List<IDynamoTagEntry> tagSubscribers)
        {
            var tagCollection = tagSubscribers.GroupBy(t => t.Tag).Select(g => new DynamoTypedTagCollection { Tag = g.Key, Subscribers = g.Select(s => s.Subscriber).ToList(), TagType = g.First().TagType }).ToList();

            await RemoveSubscribersFromTagsAsync(tagCollection);
        }

        protected async Task RemoveSubscribersFromTagsAsync(IEnumerable<DynamoTypedTagCollection> tagsToDelete)
        {
            if (tagsToDelete == null) { return; }

            foreach (var tagCollection in tagsToDelete)
            {
                await RemoveSubscribersFromTagAsync(tagCollection.Tag, tagCollection.Subscribers, tagCollection.TagType);
            }
        }

        protected async Task RemoveSubscribersFromTagAsync(string tag, List<Subscriber> user, PNTagType tagType)
        {
            switch (tagType)
            {
                case PNTagType.Iterative:
                    await RemoveSubscribersFromIterativeTagAsync(tag, user);
                    break;
                case PNTagType.SNSTopic:
                    await RemoveSubscribersFromSNSTopicTagAsync(tag, user);
                    break;
                default:
                    break;
            }
        }
        protected async Task RemoveSubscribersFromIterativeTagAsync(string tag, List<Subscriber> user)
        {
            await iterativeTagTableOperator.RemoveSubscribersFromTagAsync(tag, user);
            // int remainingSubs = await tagTableService.DecrementNumberOfSubscribers(tag, 1);
            // if (remainingSubs == 0)
            // {
            //     await RemoveDynamoTagAsync(tag);
            // }
        }
        protected async Task RemoveSubscriberFromIterativeTagAsync(string tag, Subscriber subscriber)
        {
            await iterativeTagTableOperator.RemoveSubsriberFromTagAsync(tag, subscriber);
            // int remainingSubs = await tagTableService.DecrementNumberOfSubscribers(tag, 1);
            // if (remainingSubs == 0)
            // {
            //     await RemoveDynamoTagAsync(tag);
            // }
        }
        protected async Task RemoveSubscribersFromSNSTopicTagAsync(string tag, List<Subscriber> user)
        {
            foreach (var subscriber in user)
            {
                if (subscriber.Equals(user.Last()))
                {
                    await RemoveSubscriberFromSNSTopicTagAsync(tag, subscriber, checkDeleteTopic: true);
                }
                else
                {
                    await RemoveSubscriberFromSNSTopicTagAsync(tag, subscriber, checkDeleteTopic: false);
                }
            }
        }
        protected async Task RemoveSubscriberFromSNSTopicTagAsync(string tag, Subscriber subscriber, bool checkDeleteTopic)
        {
            string subscribtionArn = subscriber.SNSSubscriptionArn;
            if (string.IsNullOrEmpty(subscribtionArn))
            {
                DynamoSNSTopicTag snsTagEntry = await snsTopicTagTableOperator.GetTagForSubscriberAsync(tag, subscriber);
                if (snsTagEntry == null) { return; }
                subscribtionArn = snsTagEntry.SnsSubscriptionArn;
            }

            var t1 = snsClient.UnsubscribeAsync(subscribtionArn);
            var t2 = snsTopicTagTableOperator.RemoveSubsriberFromTagAsync(tag, subscriber);
            await Task.WhenAll(t1, t2);
            // await tagTableService.DecrementNumberOfSubscribers(tag, 1);

            if (checkDeleteTopic)
            {
                DynamoTag tagEntry = await tagTableOperator.GetTagAsync(tag);
                if (tagEntry != null)
                {
                    // Topic attributes have out-of-date data. Check later
                    GetTopicAttributesResponse taResponse = await snsClient.GetTopicAttributesAsync(tagEntry.SnsTopicArn);
                    int numberOfSubs = 0;
                    if (int.TryParse(taResponse.Attributes["SubscriptionsConfirmed"], out numberOfSubs))
                    {
                        if (numberOfSubs <= 0)
                        {
                            await snsClient.DeleteTopicAsync(tagEntry.SnsTopicArn);
                            await RemoveDynamoTagAsync(tagEntry.Tag);
                        }
                    }
                }
            }

        }

        private async Task RemoveDynamoTagAsync(string tag)
        {
            await tagTableOperator.DeleteTagAsync(tag);
        }
    }
}
