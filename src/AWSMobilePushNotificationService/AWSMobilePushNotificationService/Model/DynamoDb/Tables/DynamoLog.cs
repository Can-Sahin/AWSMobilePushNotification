using System;
using Amazon.DynamoDBv2.DataModel;
using Newtonsoft.Json;

namespace AWSMobilePushNotificationService.Model.DynamoDb.Tables
{

    /// <summary>
    /// POCO for Object Persistence Model representing table attributes for the PNLogs table
    /// </summary>
    [DynamoDBTable("PNLogs")]
    public class DynamoLog : IDynamoTable
    {
        /// <summary>
        /// Concatanated primary key value
        /// </summary>
        [JsonIgnore]
        public string PrimaryKey => UserId + ":::" + SNSMessageId;
        internal const string RAWTABLENAME = "PNLogs";
        internal const string LSIName = "UserId-Date-index";

        /// <summary>
        /// Default constructor
        /// </summary>
        public DynamoLog()
        {
        }

        /// <summary>
        /// UserId represents the identifier of the log. If its published to user it will be the user id. If its published to SNS topic it will be the sns topic name
        /// </summary>
        [DynamoDBHashKey]
        public string UserId { get; internal set; }

        /// <summary>
        /// MessageId returned from SNS
        /// </summary>
        [DynamoDBRangeKey]
        public string SNSMessageId { get; internal set; }

        /// <summary>
        /// Date for the LSI range key
        /// </summary>
        [DynamoDBLocalSecondaryIndexRangeKey(LSIName)]
        public DateTime Date { get; set; }
    }
}
