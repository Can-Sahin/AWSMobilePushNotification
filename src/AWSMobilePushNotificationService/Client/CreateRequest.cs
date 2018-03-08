using System.Threading.Tasks;
using AWSMobilePushNotificationService.Model.OperatorModel;
using AWSMobilePushNotificationService.Model.Results;
using AWSMobilePushNotificationService.Model.Exceptions;
using AWSMobilePushNotificationService.Operators.DynamoDB;
using System;

namespace AWSMobilePushNotificationService
{
    /// <summary>
    /// Creates required DynamoDB tables for the application's push notification service
    /// </summary>
    public class CreateApplicationTablesRequest : AMPSRequestBase
    {
        /// <summary>
        /// Default constructor for creating application tables in DynamoDB
        /// </summary>
        /// <param name="provider"> Configuration for the library resources</param>
        public CreateApplicationTablesRequest(IAWSMobilePushNotificationConfigProvider provider) : base(provider) { }

        /// <summary>
        /// Properties of the table 'Subscribers'
        /// </summary>
        public TableProperty SubscribersTable { get; set; }
        /// <summary>
        /// Properties of the optional table 'TagsTable'
        /// </summary>
        public TableProperty? TagsTable { get; set; }
        /// <summary>
        /// Properties of the optional table 'IterativeTagsTable'
        /// </summary>
        public TableProperty? IterativeTagsTable { get; set; }
        /// <summary>
        /// Properties of the optional table 'SNSTopicTagsTable'
        /// </summary>
        public TableProperty? SNSTopicTagsTable { get; set; }

        /// <summary>
        /// Properties of the optional table 'Logs'
        /// </summary>
        public TableProperty? LogTable { get; set; }


        /// <summary>
        /// Defines properties for a DynamoDB table to create
        /// </summary>
        public struct TableProperty
        {
            /// <summary>
            /// Provisioned Read Capacity
            /// </summary>
            public int ReadCapacity { get; set; }
            
            /// <summary>
            /// Provisioned Write Capacity
            /// </summary>
            public int WriteCapacity { get; set; }

            /// <summary>
            /// Flag to enable TimeToLive
            /// </summary>
            public bool TTLEnabled { get; set; }
        }

        /// <summary>
        /// Validates and makes the request
        /// </summary>
        public async Task<OperationResult> SendAsync()
        {
            try
            {
                Validate();
                CreateTablesOperator tablesOperator = new CreateTablesOperator(base.Provider);
                await tablesOperator.CreateTablesAsync(new CreateTableModel(this));
                return new OperationResult();
            }
            catch (AWSMobilePushNotificationServiceException ex)
            {
                return new OperationResult(ex);
            }
            catch (Exception ex)
            {
                if (Provider.CatchAllExceptions)
                {
                    return new OperationResult(ex.Message);
                }
                else
                {
                    throw ex;
                }
            }
        }

        private void Validate()
        {
            string error = string.Empty;
            if (SubscribersTable.Equals(default(TableProperty)))
            {
                error = error + "SubscribersTable properties are invalid \n";
            }
            if (!string.IsNullOrEmpty(error))
            {
                throw new ModelInvalidException(error);
            }
        }
    }
}