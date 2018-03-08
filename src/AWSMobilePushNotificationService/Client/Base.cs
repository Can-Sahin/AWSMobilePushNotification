using System;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.SimpleNotificationService;

namespace AWSMobilePushNotificationService
{
    /// <summary>
    /// Base for all requests to make
    /// </summary>
    public abstract class AMPSRequestBase
    {
        /// <summary>
        /// Provider of the AWSMobilePushNotification
        /// </summary>
        protected readonly IAWSMobilePushNotificationConfigProvider Provider;
        internal AMPSRequestBase(IAWSMobilePushNotificationConfigProvider provider)
        {
            this.Provider = provider;
        }
    }
    /// <summary>
    /// Interface defining all the requirements for using AWSMobilePushNotification.
    /// Dont directly implement this unless you have to. Instead use DefaultAWSMobilePushNotificationConfigProvider
    /// </summary>
    public interface IAWSMobilePushNotificationConfigProvider
    {
        /// <summary>
        /// IAmazonDynamoDB to work with. Consider using static instances
        /// </summary>
        IAmazonDynamoDB DynamoDBClient { get; }
        /// <summary>
        /// IAmazonSimpleNotificationService to work with. Consider using static instances
        /// </summary>
        IAmazonSimpleNotificationService SNSClient { get; }

        /// <summary>
        /// Used for DynamoDB table name prefixes and SNS Topics' name prefixes to isolate the tables/SNSsubscriptions of each application.
        /// </summary>
        string AppIdentifier { get; }

        /// <summary>
        /// TimeToLive for a Subscriber in DynamoDB Subscribers table. 
        /// Called everytime when a Subscriber is being added anytime, so always use the getter like
        /// For Ex: SubscriberTimeToLive = { get { return TimeSpan.FromDays(180); // 6 Months}}
        /// 
        /// Provide null to disable expiration.
        /// </summary>
        TimeSpan? SubscriberTimeToLive { get; }

        /// <summary>
        /// TimeToLive for a IterativeTag in DynamoDB IterativeTags table. 
        /// Called everytime when a Tag is being added anytime, so always use the getter like
        /// For Ex: IterativeTagTimeToLive = { get { return TimeSpan.FromDays(180); // 6 Months}}
        /// 
        /// Provide null to disable expiration.
        /// </summary>
        TimeSpan? IterativeTagTimeToLive { get; }

        /// <summary>
        /// WARNING: You must declare table name prefix with AppIdentifier if you are implementing this field. Ex:
        /// 
        /// return new DynamoDBContextConfig { TableNamePrefix = AppIdentifier, ConsistentRead = false, SkipVersionCheck = true};
        /// 
        /// </summary>
        DynamoDBContextConfig DynamoDBCcontextConfig { get; }

        /// <summary>
        /// If set to true no exceptions will be thrown but instead will be written in results
        /// </summary>
        bool CatchAllExceptions { get; }

    }
    /// <summary>
    /// Default properties with DynamoDBContextConfig having TableNameprefix with AppIdentifier
    /// </summary>
    public abstract class DefaultAWSMobilePushNotificationConfigProvider : IAWSMobilePushNotificationConfigProvider
    {
        /// <summary>
        /// IAmazonDynamoDB to work with. Consider using static instances
        /// </summary>
        public abstract IAmazonDynamoDB DynamoDBClient { get; }
        /// <summary>
        /// IAmazonSimpleNotificationService to work with. Consider using static instances
        /// </summary>
        public abstract IAmazonSimpleNotificationService SNSClient { get; }
        /// <summary>
        /// Used for DynamoDB table name prefixes and SNS Topics' name prefixes to isolate the tables/SNSsubscriptions of each application.
        /// </summary>
        public abstract string AppIdentifier { get; }

        /// <summary>
        /// TimeToLive for a Subscriber in DynamoDB Subscribers table. 
        /// Called everytime when a Subscriber is being added anytime, so always use the getter like
        /// For Ex: SubscriberTimeToLive = { get { return TimeSpan.FromDays(180); // 6 Months}}
        /// 
        /// Provide null to disable expiration.
        /// </summary>
        public abstract TimeSpan? SubscriberTimeToLive { get; }
        /// <summary>
        /// TimeToLive for a IterativeTag in DynamoDB IterativeTags table. 
        /// Called everytime when a Tag is being added anytime, so always use the getter like
        /// For Ex: IterativeTagTimeToLive = { get { return TimeSpan.FromDays(180); // 6 Months}}
        /// 
        /// Provide null to disable expiration.
        /// </summary>
        public abstract TimeSpan? IterativeTagTimeToLive { get; }

        /// <summary>
        /// TableNamePrefix is set to AppIdentifier. 
        /// ConsistentRead is false.
        /// SkipVersionCheck is true
        /// </summary>
        public DynamoDBContextConfig DynamoDBCcontextConfig
        {
            get
            {
                return new DynamoDBContextConfig
                {
                    TableNamePrefix = AppIdentifier,
                    ConsistentRead = false,
                    SkipVersionCheck = true
                };
            }
        }
        /// <summary>
        /// If set to true no exceptions will be thrown but instead will be written in results' error message
        /// </summary>
        public abstract bool CatchAllExceptions { get; }

    }

}
