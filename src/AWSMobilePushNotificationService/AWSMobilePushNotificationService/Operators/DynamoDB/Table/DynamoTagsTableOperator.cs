using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using AWSMobilePushNotificationService.Model.DynamoDb.Tables;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AWSMobilePushNotificationService.Operators.DynamoDB.Table
{
    using SelfModel = DynamoTag;
    internal class DynamoTagsTableOperator : DynamoOptionalTableOperator<SelfModel>
    {
        protected override string _TABLENAME => SelfModel.RAWTABLENAME;

        public DynamoTagsTableOperator(IAWSMobilePushNotificationConfigProvider provider) : base(provider)
        {
        }

        public async Task CreateTableAsync(int readCapacity = 1, int writecapacity = 1)
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
                                      AttributeName = "Tag", // Hash
                                      AttributeType = "S"
                                  },

                              },
                KeySchema = new List<KeySchemaElement>()
                              {
                                   new KeySchemaElement
                                  {
                                      AttributeName = "Tag",
                                      KeyType = "HASH"
                                  },
                              },
                ProvisionedThroughput = new ProvisionedThroughput
                {
                    ReadCapacityUnits = readCapacity,
                    WriteCapacityUnits = writecapacity,
                }
            });
            WaitTillTableCreated(response).Wait();
        }

        public async Task<SelfModel> GetTagAsync(string tag)
        {
            SelfModel tagEntry = await dynamoService.GetItemAsync<SelfModel>(tag);
            return tagEntry;
        }
        public async Task DeleteTagAsync(string tag)
        {
            await dynamoService.DeleteItemAsync<SelfModel>(tag);
        }
        public async Task AddTagAsync(SelfModel tag)
        {
            await dynamoService.StoreAsync(tag);
        }

        // Not used for now
        public async Task<int> IncrementNumberOfSubscribers(string tag, int incrBy)
        {
            var request = new UpdateItemRequest
            {
                TableName = TABLENAME,
                Key = new Dictionary<string, AttributeValue>() { { "Tag", new AttributeValue { S = tag } } },
                ExpressionAttributeNames = new Dictionary<string, string>()
                {
                    {"#Q", "NumberOfSubscribers"}
                },
                ExpressionAttributeValues = new Dictionary<string, AttributeValue>()
                {
                    {":incr",new AttributeValue {N = incrBy.ToString()}}
                },
                UpdateExpression = "SET #Q = #Q + :incr",
                ReturnValues = ReturnValue.UPDATED_NEW,
            };

            UpdateItemResponse response = await dynamoclient.UpdateItemAsync(request);
            return int.Parse(response.Attributes["NumberOfSubscribers"].N);
        }

        // Not used for now
        public async Task<int> DecrementNumberOfSubscribers(string tag, int decrBy)
        {
            var request = new UpdateItemRequest
            {
                TableName = TABLENAME,
                Key = new Dictionary<string, AttributeValue>() { { "Tag", new AttributeValue { S = tag } } },
                ExpressionAttributeNames = new Dictionary<string, string>()
                {
                    {"#Q", "NumberOfSubscribers"}
                },
                ExpressionAttributeValues = new Dictionary<string, AttributeValue>()
                {
                    {":decr",new AttributeValue {N = decrBy.ToString()}}
                },
                UpdateExpression = "SET #Q = #Q - :decr",
                ReturnValues = ReturnValue.UPDATED_NEW,
            };

            UpdateItemResponse response = await dynamoclient.UpdateItemAsync(request);
            return int.Parse(response.Attributes["NumberOfSubscribers"].N);
        }

    }
}
