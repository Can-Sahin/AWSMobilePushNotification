using AWSMobilePushNotificationService.Model.DynamoDb.Tables;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using Amazon.DynamoDBv2.Model;
using AWSMobilePushNotificationService.Model;

namespace AWSMobilePushNotificationService.Operators.DynamoDB.Table
{
    using SelfModel = DynamoIterativeTag;

    internal class DynamoIterativeTagsTableOperator : DynamoOptionalTableOperator<SelfModel>
    {
        protected override string _TABLENAME => SelfModel.RAWTABLENAME;

        public DynamoIterativeTagsTableOperator(IAWSMobilePushNotificationConfigProvider provider) : base(provider)
        {
        }
        public async Task CreateTableAsync(int readCapacity = 1, int writecapacity = 1, bool ttlEnabled = false)
        {
            string tableName = TABLENAME;
            var client = Provider.DynamoDBClient;
            var ptIndex = new ProvisionedThroughput
            {
                ReadCapacityUnits = readCapacity,
                WriteCapacityUnits = writecapacity
            };
            var subscriberIndex = new GlobalSecondaryIndex()
            {
                IndexName = SelfModel.SCITagName,
                ProvisionedThroughput = ptIndex,
                KeySchema = {
                new KeySchemaElement {
                    AttributeName = "Subscriber", KeyType = "HASH" //Partition key
                }
              
            },
                Projection = new Projection
                {
                    ProjectionType = "ALL"
                }
            };
            var response = await client.CreateTableAsync(new CreateTableRequest
            {
                TableName = tableName,
                AttributeDefinitions = new List<AttributeDefinition>()
                              {
                                  new AttributeDefinition
                                  {
                                      AttributeName = "Tag", // Hash
                                      AttributeType = "S"
                                  },
                                  new AttributeDefinition
                                  {
                                      AttributeName = "Subscriber", // Range
                                      AttributeType = "S"
                                  }
                              },
                KeySchema = new List<KeySchemaElement>()
                              {
                                   new KeySchemaElement
                                  {
                                      AttributeName = "Tag",
                                      KeyType = "HASH"
                                  },
                                  new KeySchemaElement
                                  {
                                      AttributeName = "Subscriber",
                                      KeyType = "RANGE"
                                  }
                              },
                ProvisionedThroughput = ptIndex,
                GlobalSecondaryIndexes = {subscriberIndex}
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
        public async Task AddSubscriberAsync(SelfModel iterativeTag)
        {
            await dynamoService.StoreAsync(iterativeTag);
        }

        public async Task<List<SelfModel>> GetAllSubscribersForTagAsync(string tag)
        {
            List<SelfModel> entries = await dynamoService.QueryGetAll<SelfModel>(hashKeyValue: tag);
            return entries;
        }

        public async Task<List<SelfModel>> GetAllTagsForSubscribersAsync(List<Subscriber> usersSubscribers)
        {
            List<SelfModel> entries = new List<SelfModel>(); ;
            foreach (var subscriber in usersSubscribers)
            {
                List<SelfModel> tags = await dynamoService.QuerySecondaryIndexGetAll<SelfModel>(hashKeyValue: subscriber.PrimaryKeyValue, indexName: SelfModel.SCITagName);
                entries.AddRange(tags);
            }
            return entries;

        }
        public async Task RemoveSubscribersFromTagAsync(string tag, List<Subscriber> usersSubscribers)
        {
            var keys = usersSubscribers.Select(s => new DynamoDBPrimaryKey { HashKey = tag, rangeKey = s.PrimaryKeyValue });
            await dynamoService.BatchDeleteAsync<SelfModel>(keys);
        }

        public async Task RemoveSubsriberFromTagAsync(string tag, Subscriber subscriber)
        {
            await dynamoService.DeleteItemAsync<SelfModel>(hashKey: tag, rangeKey: subscriber.PrimaryKeyValue);
        }
    }
}