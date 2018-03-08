using System;
using Amazon.DynamoDBv2.DataModel;
using AWSMobilePushNotificationService.Model.OperatorModel;

namespace AWSMobilePushNotificationService.Model.DynamoDb.Tables
{

    [DynamoDBTable("PNSubscribers")]
    internal class DynamoSubscriber : IDynamoTable
    {
        public string PrimaryKey => UserId + ":::" + NotificationToken;
        public const string RAWTABLENAME = "PNSubscribers";

        [DynamoDBHashKey]
        public string UserId { get; set; }

        [DynamoDBRangeKey]
        public string NotificationToken { get; set; }
        public string DeviceId { get; set; }
        public int PlatformId { get { return (int)Platform; } set { Platform = (Platform)value; } }
        public DateTime UpdatedDateTime { get; set; }
        public string EndpointARN { get; set; }

        [DynamoDBProperty(StoreAsEpoch = true)]
        public DateTime? ttl { get; set; }
        public DynamoSubscriber() { }
        public DynamoSubscriber(RegisterSubscriberModel model, string endpointArn, TimeSpan? ttl)
        {
            this.UserId = model.UserId;
            this.NotificationToken = model.Token;
            this.DeviceId = model.DeviceId;
            this.Platform = model.Platform;
            this.EndpointARN = endpointArn;
            this.UpdatedDateTime = DateTime.UtcNow;
            if (ttl.HasValue)
            {
                this.ttl = DateTime.UtcNow.Add(ttl.Value);
            }
        }
        [DynamoDBIgnore]
        public Subscriber Subscriber => new Subscriber { UserId = UserId, Token = NotificationToken };

        [DynamoDBIgnore]
        public Platform Platform { get; set; }
    }
}
