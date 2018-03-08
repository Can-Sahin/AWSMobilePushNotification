using Amazon.DynamoDBv2.DataModel;

namespace AWSMobilePushNotificationService.Model.DynamoDb.Tables
{

    [DynamoDBTable("PNSNSTopicTags")]
    internal class DynamoSNSTopicTag : IDynamoTagEntry, IDynamoTable
    {
        public string PrimaryKey => Tag + ":::" + Subscriber.ToString();

        public const string RAWTABLENAME = "PNSNSTopicTags";

        public const string SCITagName = "Subscriber-index";

        [DynamoDBHashKey]
        public string Tag { get; set; }

        [DynamoDBRangeKey]
        [DynamoDBGlobalSecondaryIndexHashKey(SCITagName)]
        [DynamoDBProperty(typeof(SubscriberConverter))]
        public Subscriber Subscriber { get; set; }

        [DynamoDBIgnore]
        public PNTagType TagType { get { return PNTagType.SNSTopic; } }

        public string SnsSubscriptionArn { get; set; }


    }
}
