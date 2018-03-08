using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.SimpleNotificationService;
using AWSMobilePushNotificationService.Operators.DynamoDB;
using System;

namespace AWSMobilePushNotificationService.Operators
{
    internal abstract class AWSMobilePushNotificationOperator
    {
        private Lazy<DynamoService> _dynamoService { get; set; }

        private Lazy<DynamoDBContext> _dynamoContext { get; set; }

        protected DynamoService dynamoService => _dynamoService.Value;
        protected IAmazonSimpleNotificationService snsClient => Provider.SNSClient;
        protected IAmazonDynamoDB dynamoclient => Provider.DynamoDBClient;
        protected DynamoDBContext dynamoContext => _dynamoContext.Value;

        protected IAWSMobilePushNotificationConfigProvider Provider { get; }

        public AWSMobilePushNotificationOperator(IAWSMobilePushNotificationConfigProvider provider)
        {
            this.Provider = provider;
            _dynamoService = new Lazy<DynamoService>(() => new DynamoService(provider));
            _dynamoContext = new Lazy<DynamoDBContext>(() => dynamoService.DynamoDBContext);
        }
    }
}
