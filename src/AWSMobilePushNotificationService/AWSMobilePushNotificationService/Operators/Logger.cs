using System.Threading.Tasks;
using Amazon.DynamoDBv2.DocumentModel;
using AWSMobilePushNotificationService.Model.DynamoDb.Tables;
using AWSMobilePushNotificationService.Model.Exceptions;
using AWSMobilePushNotificationService.Model.Results.Publish;
using AWSMobilePushNotificationService.Operators.DynamoDB.Table;

namespace AWSMobilePushNotificationService.Operators
{
    internal class Logger : AWSMobilePushNotificationOperator
    {
        private DynamoLogsTableOperator logsTableOperator { get; }

        public Logger(IAWSMobilePushNotificationConfigProvider provider) : base(provider)
        {
            logsTableOperator = new DynamoLogsTableOperator(provider);
        }
        public async Task LogNotificationAsync(PublishResult publishResult, Document document)
        {
            if (!publishResult.IsSuccessful)
            {
                return;
            }
            if (publishResult is PublishToIterativeTagResult)
            {
                PublishToIterativeTagResult iterativeResult = (PublishToIterativeTagResult)publishResult;
                foreach (var resultTuple in iterativeResult.EndpointResults)
                {
                    var result = resultTuple.Item1;
                    if (result is PublishToSNSSuccessfulResult)
                    {
                        LogSuccessfulNotification((PublishToSNSSuccessfulResult)result, document).Wait();
                    }
                }
            }
            else if (publishResult is PublishToSNSTopicTagResult)
            {
                await LogSuccessfulNotification((PublishToSNSTopicTagResult)publishResult, document);
            }
            else
            {
                if (publishResult is PublishToSNSSuccessfulResult)
                {
                    await LogSuccessfulNotification((PublishToSNSSuccessfulResult)publishResult, document);
                }
            }

        }
        private async Task LogSuccessfulNotification(PublishToSNSSuccessfulResult result, Document document)
        {
            if (string.IsNullOrEmpty(result.UserId))
            {
                throw new ModelInvalidException("UserId of response is empty");
            }
            DynamoLog logEntry = new DynamoLog(result.UserId, result.MessageId);

            await logsTableOperator.AddLogAsync(logEntry, document);
        }
        private async Task LogSuccessfulNotification(PublishToSNSTopicTagResult result, Document document)
        {
            if (string.IsNullOrEmpty(result.MessageId))
            {
                throw new ModelInvalidException("MessageId of SNS Topic response is empty");
            }
            DynamoLog logEntry = new DynamoLog(result.Tag, result.MessageId);

            await logsTableOperator.AddLogAsync(logEntry, document);
        }
    }
}
