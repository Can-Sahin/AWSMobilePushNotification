using Amazon.DynamoDBv2.DataModel;

namespace AWSMobilePushNotificationService.Model.DynamoDb.Tables
{

    [DynamoDBTable("PNTags")]
    internal class DynamoTag : IDynamoTable
    {
        public string PrimaryKey => Tag;
        public const string RAWTABLENAME = "PNTags";

        public DynamoTag()
        {
            // NumberOfSubscribers = 0;
        }

        [DynamoDBHashKey]
        public string Tag { get; set; }

        // Passthrough for TaggingTypeEnum. Reason: Int to Enum parse fails in POCO object creation at AWS SDK. Check later.
        public int TaggingType
        {
            get { return (int)TaggingTypeEnum; }
            set { TaggingTypeEnum = (PNTagType)value; }
        }

        [DynamoDBIgnore]
        public PNTagType TaggingTypeEnum { get; set; }

        public string SnsTopicArn { get; set; }

        // CAREFULL! Atomic operation
        // public int NumberOfSubscribers { get; set; }

    }
}
