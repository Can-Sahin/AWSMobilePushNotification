using AWSMobilePushNotificationService.Model.DynamoDb.Tables;
using System.Threading.Tasks;
using System.Collections.Generic;
using Amazon.DynamoDBv2.Model;
using Amazon.DynamoDBv2.DocumentModel;
using System;

namespace AWSMobilePushNotificationService.Operators.DynamoDB.Table
{
    using SelfModel = DynamoLog;

    internal class DynamoLogsTableOperator : DynamoTableOperator<SelfModel>
    {
        protected override string _TABLENAME => SelfModel.RAWTABLENAME;
        public DynamoLogsTableOperator(IAWSMobilePushNotificationConfigProvider provider) : base(provider)
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
            var userDateIndex = new LocalSecondaryIndex()
            {
                IndexName = SelfModel.LSIName,
                KeySchema = {
                new KeySchemaElement {
                    AttributeName = "UserId", KeyType = "HASH" //Partition key
                },
                new KeySchemaElement {
                    AttributeName = "Date", KeyType = "RANGE" //Partition key
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
                                      AttributeName = "UserId", // Hash
                                      AttributeType = "S"
                                  },
                                  new AttributeDefinition
                                  {
                                      AttributeName = "SNSMessageId", // Range
                                      AttributeType = "S"
                                  },
                                  new AttributeDefinition
                                  {
                                      AttributeName = "Date",
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
                                      AttributeName = "SNSMessageId",
                                      KeyType = "RANGE"
                                  },
                              },
                ProvisionedThroughput = ptIndex,
                LocalSecondaryIndexes = { userDateIndex }
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

        public async Task AddLogAsync<T>(T logEntry) where T: SelfModel, new()
        {
            await dynamoService.StoreAsync(logEntry);
        }

        public async Task<List<T>> GetAllLogsWithUserIdAndDate<T>(string userId, DateTime dateMin, DateTime dateMax) where T: SelfModel
        {
            IEnumerable<object> values = new List<object> { dateMin, dateMax };
            List<T> entries = await dynamoService.QuerySecondaryIndexGetAll<T>(userId, QueryOperator.Between, values, SelfModel.LSIName);
            return entries;
        }
        public async Task<List<T>> GetAllLogsWithUserId<T>(string userId) where T: SelfModel
        {
            List<T> entries = await dynamoService.QueryGetAll<T>(userId);
            return entries;
        }
        public async Task<List<T>> GetAllLogsWithGSI<T>(string hashValue, string indexName) where T: SelfModel
        {
            List<T> entries = await dynamoService.QuerySecondaryIndexGetAll<T>(hashValue,indexName);
            return entries;
        }
    }
}
