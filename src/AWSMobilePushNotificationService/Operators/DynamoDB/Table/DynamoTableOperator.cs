using System;
using System.Threading.Tasks;
using Amazon.DynamoDBv2.Model;
using AWSMobilePushNotificationService.Model.DynamoDb.Tables;

namespace AWSMobilePushNotificationService.Operators.DynamoDB.Table
{
    internal interface IDynamoTableService<T> where T : class, IDynamoTable
    {
    }
    internal abstract class DynamoTableOperator<T> : AWSMobilePushNotificationOperator, IDynamoTableService<T> where T : class, IDynamoTable
    {
        private readonly IAWSMobilePushNotificationConfigProvider provider;
        protected abstract string _TABLENAME { get; }
        public string TABLENAME { get { return provider.AppIdentifier + _TABLENAME; } }
        public DynamoTableOperator(IAWSMobilePushNotificationConfigProvider provider) : base(provider)
        {
            this.provider = provider;
        }
        protected Task WaitTillTableCreated(CreateTableResponse response)
        {
            var tableDescription = response.TableDescription;

            string status = tableDescription.TableStatus;

            Console.WriteLine(TABLENAME + " - " + status);

            // Let us wait until table is created. Call DescribeTable.
            while (status != "ACTIVE")
            {
                System.Threading.Thread.Sleep(5000); // Wait 5 seconds.
                try
                {
                    var res = Provider.DynamoDBClient.DescribeTableAsync(TABLENAME).Result;
                    Console.WriteLine("Table name: {0}, status: {1}", res.Table.TableName,
                              res.Table.TableStatus);
                    status = res.Table.TableStatus;
                }
                // Try-catch to handle potential eventual-consistency issue.
                catch (ResourceNotFoundException)
                { }
            }
            return Task.CompletedTask;
        }
    }

    internal abstract class DynamoOptionalTableOperator<T> : DynamoTableOperator<T> where T : class, IDynamoTable
    {
        private static bool? tableExists = null;
        public bool IsTableExists()
        {
            if (tableExists.HasValue)
            {
                return tableExists.Value;
            }

            try
            {
                var description = dynamoclient.DescribeTableAsync(TABLENAME).Result;
                tableExists = true;
                return true;
            }
            catch (ResourceNotFoundException)
            {
                tableExists = false;
                return false;
            }
        }
        public DynamoOptionalTableOperator(IAWSMobilePushNotificationConfigProvider provider) : base(provider)
        {

        }
    }
}
