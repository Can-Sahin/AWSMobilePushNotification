using AWSMobilePushNotificationService.Model.DynamoDb.Tables;
using System.Threading.Tasks;
using System.Collections.Generic;
using Amazon.DynamoDBv2.Model;
using Amazon.DynamoDBv2.DocumentModel;


namespace AWSMobilePushNotificationService.Operators.DynamoDB.Table
{
    using SelfModel = DynamoLog;

    internal class DynamoLogsTableOperator : DynamoTableOperator<SelfModel>
    {
        protected override string _TABLENAME => SelfModel.RAWTABLENAME;

        private static Amazon.DynamoDBv2.DocumentModel.Table Table;

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
                IndexName = SelfModel.LSITagName,
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
                                      AttributeName = "MessageId", // Range
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
                                      AttributeName = "MessageId",
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

        public async Task AddLogAsync(SelfModel logEntry, Document document)
        {
            if (Table == null)
            {
                Table = Amazon.DynamoDBv2.DocumentModel.Table.LoadTable(dynamoclient, TABLENAME);
            }
            document["UserId"] = logEntry.UserId;
            document["MessageId"] = logEntry.SNSMessageId;
            document["Date"] = logEntry.Date;

            await Table.PutItemAsync(document);
        }

    }
}
