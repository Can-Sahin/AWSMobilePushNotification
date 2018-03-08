using Amazon.DynamoDBv2.DataModel;
using System;

namespace AWSMobilePushNotificationService.Model.DynamoDb.Tables
{
    [DynamoDBTable("PNIterativeTags")]
    internal class DynamoIterativeTag : IDynamoTagEntry, IDynamoTable
    {
        public string PrimaryKey => Tag + ":::" + Subscriber.PrimaryKeyValue;

        public const string RAWTABLENAME = "PNIterativeTags";

        public const string SCITagName = "Subscriber-index";

        [DynamoDBHashKey]
        public string Tag { get; set; }

        [DynamoDBRangeKey]
        [DynamoDBGlobalSecondaryIndexHashKey(SCITagName)]
        [DynamoDBProperty(typeof(SubscriberConverter))]
        public Subscriber Subscriber { get; set; }

        public string EndpointArn { get; set; }

        public int PlatformId { get { return (int)Platform; } set { Platform = (Platform)value; } }
        [DynamoDBProperty(StoreAsEpoch = true)]
        public DateTime? ttl { get; set; }

        [DynamoDBIgnore]
        public PNTagType TagType { get { return PNTagType.Iterative; } }

        // Passthrough for Platform. Reason: Int to Enum parse fails in POCO object creation at AWS SDK. Check later.
        [DynamoDBIgnore]
        public Platform Platform { get; set; }


    }

}
