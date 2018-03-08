using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using AWSMobilePushNotificationService.Model.DynamoDb.Tables;
using AWSMobilePushNotificationService.Model.Exceptions;
using AWSMobilePushNotificationService.Model.Results.Publish;
using AWSMobilePushNotificationService.Operators.DynamoDB.Table;

using static AWSMobilePushNotificationService.QueryLogsRequest;

namespace AWSMobilePushNotificationService.Operators
{
    internal class Logger : AWSMobilePushNotificationOperator
    {
        private DynamoLogsTableOperator logsTableOperator { get; }

        public Logger(IAWSMobilePushNotificationConfigProvider provider) : base(provider)
        {
            logsTableOperator = new DynamoLogsTableOperator(provider);
        }
        public async Task LogNotificationAsync<T>(PublishResult publishResult, T log) where T: DynamoLog, new()
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
                        LogSuccessfulNotification((PublishToSNSSuccessfulResult)result, log).Wait();
                    }
                }
            }
            else if (publishResult is PublishToSNSTopicTagResult)
            {
                await LogSuccessfulNotification((PublishToSNSTopicTagResult)publishResult, log);
            }
            else
            {
                if (publishResult is PublishToSNSSuccessfulResult)
                {
                    await LogSuccessfulNotification((PublishToSNSSuccessfulResult)publishResult, log);
                }
            }

        }
        private async Task LogSuccessfulNotification<T>(PublishToSNSSuccessfulResult result, T log) where T: DynamoLog, new()
        {
            if (string.IsNullOrEmpty(result.UserId))
            {
                throw new ModelInvalidException("UserId of response is empty");
            }

            log.UserId = result.UserId;
            log.SNSMessageId = result.MessageId;
            log.Date = DateTime.UtcNow;

            await logsTableOperator.AddLogAsync(log);
        }
        private async Task LogSuccessfulNotification<T>(PublishToSNSTopicTagResult result, T log) where T: DynamoLog, new()
        {
            if (string.IsNullOrEmpty(result.MessageId))
            {
                throw new ModelInvalidException("MessageId of SNS Topic response is empty");
            }
            log.UserId = result.Tag;
            log.SNSMessageId = result.MessageId;
            log.Date = DateTime.UtcNow;

            await logsTableOperator.AddLogAsync(log);
        }

        public async Task<List<T>> QueryAsync<T>(QueryConfig query) where T : DynamoLog
        {
            switch (query.type)
            {
                case QueryConfig.QueryType.HashValueOnly:
                    return await logsTableOperator.GetAllLogsWithUserId<T>(query.UserId);
                case QueryConfig.QueryType.LSI:
                    return await logsTableOperator.GetAllLogsWithUserIdAndDate<T>(query.UserId, query.DateMin, query.DateMax);
                case QueryConfig.QueryType.GSI:
                    return await logsTableOperator.GetAllLogsWithGSI<T>(query.GSIHashValue, query.GSI_Name);
                default:
                    return null;
            }
        }
    }
}
