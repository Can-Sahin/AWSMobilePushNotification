using System.Threading.Tasks;
using AWSMobilePushNotificationService.Model.Results;
using AWSMobilePushNotificationService.Model.Exceptions;
using System;
using AWSMobilePushNotificationService.Operators;
using AWSMobilePushNotificationService.Model.Results.Publish;
using Amazon.DynamoDBv2.DocumentModel;
using System.Collections.Generic;

namespace AWSMobilePushNotificationService
{
    /// <summary>
    /// Static PushNotification Logger accessible publicly
    /// 
    /// First call Configure(), then log notifications
    /// </summary>
    public static class PNLogger
    {
        private static Lazy<LoggingRequest> _loggingRequest;

        /// <summary>
        /// Configuration for the library resources
        /// </summary>
        /// <param name="provider"></param>
        public static void Configure(IAWSMobilePushNotificationConfigProvider provider)
        {
            _loggingRequest = new Lazy<LoggingRequest>(() =>
            {
                return new LoggingRequest(provider);
            });
        }

        /// <summary>
        /// Validates and makes the request
        /// </summary>
        /// <param name="results"> PushNotification's results</param>
        /// <param name="document"> DynamoDB Document Model item with custom key value pairs to insert into the table</param>
        public static async Task<OperationResult> LogAsync(IEnumerable<PublishResult> results, Document document)
        {
            if (_loggingRequest == null)
            {
                throw new ModelInvalidException("PushNotificationLogger is not configured");
            }
            return await _loggingRequest.Value.SendAsync(results, document);
        }

        /// <summary>
        /// Validates and makes the request
        /// </summary>
        /// <param name="result"> PushNotification's result</param>
        /// <param name="document"> DynamoDB Document Model item with custom key value pairs to insert into the table</param>
        public static async Task<OperationResult> LogAsync(PublishResult result, Document document)
        {
            if (_loggingRequest == null)
            {
                throw new ModelInvalidException("PushNotificationLogger is not configured");
            }
            return await _loggingRequest.Value.SendAsync(result, document);
        }
    }

    /// <summary>
    /// Log published PushNotification's result into DynamoDB 'Log' table
    /// </summary>
    public class LoggingRequest : AMPSRequestBase
    {
        /// <summary>
        /// Default constructor for logging request
        /// </summary>
        /// <param name="provider">Configuration for the library resources</param>
        public LoggingRequest(IAWSMobilePushNotificationConfigProvider provider) : base(provider) { }

        /// <summary>
        /// Validates and makes the request
        /// </summary>
        /// <param name="results"> PushNotification's results</param>
        /// <param name="document"> DynamoDB Document Model item with custom key value pairs to insert into the table</param>
        /// <returns></returns>
        public async Task<OperationResult> SendAsync(IEnumerable<PublishResult> results, Document document)
        {
            try
            {
                Logger logger = new Logger(base.Provider);
                foreach (var result in results)
                {
                    Validate(result);
                    await logger.LogNotificationAsync(result, document);
                }
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

        /// <summary>
        /// Validates and makes the request
        /// </summary>
        /// <param name="result"> PushNotification's result</param>
        /// <param name="document"> DynamoDB Document Model item with custom key value pairs to insert into the table</param>
        /// <returns></returns>
        public async Task<OperationResult> SendAsync(PublishResult result, Document document)
        {
            try
            {
                Validate(result);
                Logger logger = new Logger(base.Provider);
                await logger.LogNotificationAsync(result, document);
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

        private void Validate(PublishResult result)
        {
            if (!result.IsSuccessful) return;
            string error = string.Empty;

            if (result is PublishToSNSSuccessfulResult)
            {
                if (string.IsNullOrEmpty(result.MessageId))
                {
                    error = error + "MessageId of response is empty \n";
                }
            }
            if (result is PublishToSNSTopicTagResult)
            {
                if (string.IsNullOrEmpty(result.MessageId))
                {
                    error = error + "MessageId of response is empty \n";
                }
            }

            if (!string.IsNullOrEmpty(error))
            {
                throw new ModelInvalidException(error);
            }
        }
    }
}