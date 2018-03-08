namespace AWSMobilePushNotificationService.Model.OperatorModel
{

    internal class CreateTableModel
    {
        public TableProperty SubscribersTable { get; set; }
        public TableProperty? TagsTable { get; set; }
        public TableProperty? IterativeTagsTable { get; set; }
        public TableProperty? SNSTopicTagsTable { get; set; }
        public TableProperty? LogsTable { get; set; }


        public struct TableProperty
        {
            public int Read { get; set; }
            public int Write { get; set; }
            public bool TTLEnabled { get; set; }

        }
        public CreateTableModel(CreateApplicationTablesRequest model)
        {
            this.SubscribersTable = new TableProperty { Read = model.SubscribersTable.ReadCapacity, Write = model.SubscribersTable.WriteCapacity, TTLEnabled = model.SubscribersTable.TTLEnabled };
            if (model.TagsTable.HasValue)
            {
                this.TagsTable = new TableProperty { Read = model.TagsTable.Value.ReadCapacity, Write = model.TagsTable.Value.WriteCapacity };
            }
            if (model.IterativeTagsTable.HasValue)
            {
                this.IterativeTagsTable = new TableProperty { Read = model.IterativeTagsTable.Value.ReadCapacity, Write = model.IterativeTagsTable.Value.WriteCapacity, TTLEnabled = model.IterativeTagsTable.Value.TTLEnabled };
            }
            if (model.SNSTopicTagsTable.HasValue)
            {
                this.SNSTopicTagsTable = new TableProperty { Read = model.SNSTopicTagsTable.Value.ReadCapacity, Write = model.SNSTopicTagsTable.Value.WriteCapacity };
            }
            if (model.LogTable.HasValue)
            {
                this.LogsTable = new TableProperty { Read = model.LogTable.Value.ReadCapacity, Write = model.LogTable.Value.WriteCapacity };
            }
        }

    }
}