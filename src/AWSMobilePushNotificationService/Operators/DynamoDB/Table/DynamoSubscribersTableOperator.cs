using AWSMobilePushNotificationService.Model.OperatorModel;
using AWSMobilePushNotificationService.Model.DynamoDb.Tables;
using System.Threading.Tasks;
using System.Collections.Generic;
using Amazon.DynamoDBv2.Model;

namespace AWSMobilePushNotificationService.Operators.DynamoDB.Table
{
    using SelfModel = DynamoSubscriber;

    internal class DynamoSubscribersTableOperator : DynamoTableOperator<SelfModel>
    {
        protected override string _TABLENAME => SelfModel.RAWTABLENAME;

        public DynamoSubscribersTableOperator(IAWSMobilePushNotificationConfigProvider provider) : base(provider)
        {

        }
        public async Task CreateTableAsync(int readCapacity = 1, int writecapacity = 1, bool ttlEnabled = false)
        {
            string tableName = TABLENAME;
            var client = Provider.DynamoDBClient;

            var response = await client.CreateTableAsync(new CreateTableRequest
            {
                TableName = tableName,
                AttributeDefinitions = new List<AttributeDefinition>()
                              {
                                  new AttributeDefinition
                                  {
                                      AttributeName = "UserId", // Hash
                                      AttributeType = "S"
                                  },
                                  new AttributeDefinition
                                  {
                                      AttributeName = "NotificationToken", // Range
                                      AttributeType = "S"
                                  }
                              },
                KeySchema = new List<KeySchemaElement>()
                              {
                                   new KeySchemaElement
                                  {
                                      AttributeName = "UserId",
                                      KeyType = "HASH"
                                  },
                                  new KeySchemaElement
                                  {
                                      AttributeName = "NotificationToken",
                                      KeyType = "RANGE"
                                  }
                              },
                ProvisionedThroughput = new ProvisionedThroughput
                {
                    ReadCapacityUnits = readCapacity,
                    WriteCapacityUnits = writecapacity,
                }
            });

            WaitTillTableCreated(response).Wait();
            if (ttlEnabled)
            {
                client.UpdateTimeToLiveAsync(new UpdateTimeToLiveRequest
                {
                    TableName = tableName,
                    TimeToLiveSpecification = new TimeToLiveSpecification
                    {
                        Enabled = true,
                        AttributeName = "ttl"
                    }
                }).Wait();
            }
        }

        public async Task<SelfModel> GetSubscriberAsync(string userId, string token)
        {
            SelfModel entry = await dynamoService.GetItemAsync<SelfModel>(key: userId, rangeValue: token);
            return entry;
        }

        public async Task<List<SelfModel>> GetAllSubcribersOfUserAsync(string userId)
        {
            List<SelfModel> entries = await dynamoService.QueryGetAll<SelfModel>(hashKeyValue: userId);
            return entries;
        }

        public async Task AddSubscriberAsync(SelfModel subscriber)
        {
            await dynamoService.StoreAsync(subscriber);
        }

        public async Task RemoveSubscriberAsync(SelfModel subscriber)
        {
            await dynamoService.DeleteItemAsync(subscriber);
        }
        
        public async Task RemoveSubscriberAsync(string userId, string token)
        {
            await dynamoService.DeleteItemAsync<SelfModel>(hashKey: userId, rangeKey: token);
        }

    }
}
