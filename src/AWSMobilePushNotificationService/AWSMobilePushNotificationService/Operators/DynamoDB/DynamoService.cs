using System.Collections.Generic;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using System.Threading.Tasks;
using AWSMobilePushNotificationService.Model.DynamoDb.Tables;

namespace AWSMobilePushNotificationService.Operators.DynamoDB
{
    internal class DynamoService
    {
        private readonly DynamoDBContext DbContext;
        public IAmazonDynamoDB DynamoClient;
        public DynamoDBContext DynamoDBContext { get { return DbContext; } }
        public DynamoService(IAWSMobilePushNotificationConfigProvider config)
        {
            DynamoClient = config.DynamoDBClient;

            DbContext = new DynamoDBContext(DynamoClient, config.DynamoDBCcontextConfig);
        }

        public async Task StoreAsync<T>(T item) where T : IDynamoTable, new()
        {
            await DbContext.SaveAsync(item);
        }

        public async Task BatchStoreAsync<T>(IEnumerable<T> items) where T : class
        {
            var itemBatch = DbContext.CreateBatchWrite<T>();

            itemBatch.AddPutItems(items);

            await itemBatch.ExecuteAsync();
        }

        public async Task BatchDeleteAsync<T>(IEnumerable<T> items) where T : class
        {
            var itemBatch = DbContext.CreateBatchWrite<T>();

            itemBatch.AddDeleteItems(items);

            await itemBatch.ExecuteAsync();
        }

        public async Task BatchDeleteAsync<T>(IEnumerable<DynamoDBPrimaryKey> items) where T : class
        {
            var itemBatch = DbContext.CreateBatchWrite<T>();

            foreach (var item in items)
            {
                itemBatch.AddDeleteKey(item.HashKey, item.rangeKey);
            }
            await itemBatch.ExecuteAsync();
        }
        public async Task<List<T>> BatchGetAsync<T>(IEnumerable<T> items) where T : class
        {
            var itemBatch = DbContext.CreateBatchGet<T>();

            foreach (var item in items)
            {
                itemBatch.AddKey(item);
            }

            await itemBatch.ExecuteAsync();

            return itemBatch.Results;
        }
        public async Task<List<T>> BatchGetAsync<T>(IEnumerable<DynamoDBPrimaryKey> keys) where T : class
        {
            var itemBatch = DbContext.CreateBatchGet<T>();

            foreach (var key in keys)
            {
                if (!string.IsNullOrEmpty(key.rangeKey))
                {
                    itemBatch.AddKey(key.HashKey, key.rangeKey);
                }
                else
                {
                    itemBatch.AddKey(key.HashKey);
                }
            }

            await itemBatch.ExecuteAsync();

            return itemBatch.Results;
        }
        public async Task<T> GetItemAsync<T>(T item) where T : class
        {
            return await DbContext.LoadAsync<T>(item);
        }
        public async Task<T> GetItemAsync<T>(string key, object rangeValue = null) where T : class
        {
            return await DbContext.LoadAsync<T>(key, rangeValue);
        }

        public async Task UpdateItemAsync<T>(T item, bool ifExists = false) where T : class, IDynamoTable
        {
            if (ifExists)
            {
                T savedItem = await DbContext.LoadAsync(item);

                if (savedItem == null)
                {
                    return;
                }
            }

            await DbContext.SaveAsync(item);
        }

        public async Task DeleteItemAsync<T>(T item, bool ifExists = false) where T : IDynamoTable
        {
            if (ifExists)
            {
                var savedItem = await DbContext.LoadAsync(item);

                if (savedItem == null)
                {
                    return;
                    // throw new DynamoDBItemNotFoundException(item.PrimaryKey);
                }
            }
            
            await DbContext.DeleteAsync(item);
        }

        public async Task DeleteItemAsync<T>(string hashKey, string rangeKey = null, bool ifExists = false)
        {
            if (ifExists)
            {
                var savedItem = await DbContext.LoadAsync<T>(hashKey, rangeKey: rangeKey);

                if (savedItem == null)
                {
                    return;
                    // throw new DynamoDBItemNotFoundException(hashKey + " + " + rangeKey);
                }
            }

            await DbContext.DeleteAsync<T>(hashKey, rangeKey: rangeKey);
        }

        public async Task<List<T>> QueryGetAll<T>(object hashKeyValue, DynamoDBOperationConfig operationConfig = null)
        {
            return await DbContext.QueryAsync<T>(hashKeyValue, operationConfig).GetRemainingAsync();
        }

        public async Task<List<T>> QuerySecondaryIndexGetAll<T>(object hashKeyValue, string indexName, DynamoDBOperationConfig operationConfig = null)
        {
            if (operationConfig != null)
            {
                return await DbContext.QueryAsync<T>(hashKeyValue, operationConfig).GetRemainingAsync();
            }

            return await DbContext.QueryAsync<T>(hashKeyValue, new DynamoDBOperationConfig { IndexName = indexName }).GetRemainingAsync();
        }
        
        public async Task<List<T>> QuerySecondaryIndexGetAllWithFilter<T>(object hashKeyValue, string indexName, List<ScanCondition> queryFilter, DynamoDBOperationConfig operationConfig = null)
        {
            if (operationConfig != null)
            {
                return await DbContext.QueryAsync<T>(hashKeyValue, operationConfig).GetRemainingAsync();
            }

            return await DbContext.QueryAsync<T>(hashKeyValue, new DynamoDBOperationConfig { IndexName = indexName, QueryFilter = queryFilter }).GetRemainingAsync();
        }
    }
    internal struct DynamoDBPrimaryKey
    {
        public string HashKey { get; set; }
        public string rangeKey { get; set; }
    }
}