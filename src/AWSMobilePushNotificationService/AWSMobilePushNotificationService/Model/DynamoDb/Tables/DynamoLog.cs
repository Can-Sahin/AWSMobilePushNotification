using System;
using Amazon.DynamoDBv2.DataModel;

namespace AWSMobilePushNotificationService.Model.DynamoDb.Tables
{

    [DynamoDBTable("PNLogs")]
    internal class DynamoLog : IDynamoTable
    {
        public string PrimaryKey => UserId + ":::" + SNSMessageId;
        public const string RAWTABLENAME = "PNLogs";
        public const string LSITagName = "UserId-Date-index";

        public DynamoLog()
        {
        }

        [DynamoDBHashKey]
        public string UserId { get; set; }

        [DynamoDBRangeKey]
        public string SNSMessageId { get; set; }
        public DateTime Date { get; set; }

        public DynamoLog(string userId, string messageId)
        {
            this.UserId = userId;
            this.SNSMessageId = messageId;
            Date = DateTime.UtcNow;
        }


    }
}
