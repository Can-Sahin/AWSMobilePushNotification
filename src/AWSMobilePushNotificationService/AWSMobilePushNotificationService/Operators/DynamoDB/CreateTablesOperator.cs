using System.Collections.Generic;
using System.Threading.Tasks;
using AWSMobilePushNotificationService.Operators.DynamoDB.Table;
using AWSMobilePushNotificationService.Model.OperatorModel;

namespace AWSMobilePushNotificationService.Operators.DynamoDB
{
    internal class CreateTablesOperator : AWSMobilePushNotificationOperator
    {

        protected DynamoSubscribersTableOperator subscribersTableOperator { get; }
        protected DynamoTagsTableOperator tagTableOperator { get; }
        protected DynamoIterativeTagsTableOperator iterativeTagTableOperator { get; }
        protected DynamoSNSTopicTagsTableOperator snsTopicTagTableOperator { get; }
        protected DynamoLogsTableOperator logsTableOperator { get; }

        public CreateTablesOperator(IAWSMobilePushNotificationConfigProvider provider) : base(provider)
        {
            this.subscribersTableOperator = new DynamoSubscribersTableOperator(provider);
            this.tagTableOperator = new DynamoTagsTableOperator(provider);
            this.iterativeTagTableOperator = new DynamoIterativeTagsTableOperator(provider);
            this.snsTopicTagTableOperator = new DynamoSNSTopicTagsTableOperator(provider);
            this.logsTableOperator = new DynamoLogsTableOperator(provider);
        }

        public async Task CreateTablesAsync(CreateTableModel model)
        {
            var t1 = subscribersTableOperator.CreateTableAsync(model.SubscribersTable.Read, model.SubscribersTable.Write, model.SubscribersTable.TTLEnabled);
            List<Task> tasks = new List<Task> { t1 };
            if (model.TagsTable.HasValue)
            {
                tasks.Add(tagTableOperator.CreateTableAsync(model.TagsTable.Value.Read, model.TagsTable.Value.Write));
                if (model.IterativeTagsTable.HasValue)
                {
                    tasks.Add(iterativeTagTableOperator.CreateTableAsync(model.IterativeTagsTable.Value.Read, model.IterativeTagsTable.Value.Write, model.IterativeTagsTable.Value.TTLEnabled));
                }
                if (model.SNSTopicTagsTable.HasValue)
                {
                    tasks.Add(snsTopicTagTableOperator.CreateTableAsync(model.SNSTopicTagsTable.Value.Read, model.SNSTopicTagsTable.Value.Write));
                }
            }
            if (model.LogsTable.HasValue)
            {
                tasks.Add(logsTableOperator.CreateTableAsync(model.LogsTable.Value.Read, model.LogsTable.Value.Write));
            }
            await Task.WhenAll(tasks.ToArray());
        }
    }
}