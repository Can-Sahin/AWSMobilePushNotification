using System.Threading.Tasks;
using AWSMobilePushNotificationService.Model.Results;
using AWSMobilePushNotificationService.Model.Exceptions;
using System;
using AWSMobilePushNotificationService.Operators;
using AWSMobilePushNotificationService.Model.Results.Publish;
using Amazon.DynamoDBv2.DocumentModel;
using System.Collections.Generic;
using Amazon.DynamoDBv2.DataModel;
using AWSMobilePushNotificationService.Model.DynamoDb.Tables;

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
        /// <param name="log"> Object of type DynamoLog to be inserted as attributes</param>
        public static async Task<OperationResult> LogAsync<T>(IEnumerable<PublishResult> results, T log) where T : DynamoLog, new()
        {
            if (_loggingRequest == null)
            {
                throw new ModelInvalidException("PushNotificationLogger is not configured");
            }
            return await _loggingRequest.Value.SendAsync(results, log);
        }

        /// <summary>
        /// Validates and makes the request
        /// </summary>
        /// <param name="result"> PushNotification's result</param>
        /// <param name="log"> Object of type DynamoLog to be inserted as attributes</param>
        public static async Task<OperationResult> LogAsync<T>(PublishResult result, T log) where T : DynamoLog, new()
        {
            if (_loggingRequest == null)
            {
                throw new ModelInvalidException("PushNotificationLogger is not configured");
            }
            return await _loggingRequest.Value.SendAsync(result, log);
        }
    }

    /// <summary>
    /// Log published PushNotification's result into DynamoDB 'Log' table
    /// </summary>
    public class LoggingRequest : AMPSRequestBase
    {
        private static Logger logger;
        /// <summary>
        /// Default constructor for logging request
        /// </summary>
        /// <param name="provider">Configuration for the library resources</param>
        public LoggingRequest(IAWSMobilePushNotificationConfigProvider provider) : base(provider)
        {
            if (logger == null)
            {
                logger = new Logger(base.Provider);
            }
        }


        /// <summary>
        /// Validates and makes the request
        /// </summary>
        /// <param name="results"> PushNotification's results</param>
        /// <param name="log"> Object of type DynamoLog to be inserted as attributes</param>
        public async Task<OperationResult> SendAsync<T>(IEnumerable<PublishResult> results, T log) where T : DynamoLog, new()
        {
            try
            {
                foreach (var result in results)
                {
                    Validate(result);
                    await logger.LogNotificationAsync(result, log);
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
        /// <param name="log"> Object of type DynamoLog to be inserted as attributes</param>
        public async Task<OperationResult> SendAsync<T>(PublishResult result, T log) where T : DynamoLog, new()
        {
            try
            {
                Validate(result);
                await logger.LogNotificationAsync(result, log);
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
    /// <summary>
    /// Query the PNLogs table 
    /// </summary>
    public class QueryLogsRequest : AMPSRequestBase
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="provider">Configuration for the library resources</param>
        public QueryLogsRequest(IAWSMobilePushNotificationConfigProvider provider) : base(provider) { }

        /// <summary>
        /// Configuration of the query to make.
        /// </summary>
        public QueryConfig queryConfiguration { get; set; }

        /// <summary>
        /// Validates and makes the request
        /// </summary>
        public async Task<List<T>> SendAsync<T>() where T : DynamoLog
        {
            try
            {
                Validate();
                Logger logger = new Logger(base.Provider);
                return await logger.QueryAsync<T>(queryConfiguration);
            }
            catch (AWSMobilePushNotificationServiceException ex)
            {
                throw ex; // Will not encapsulate in a result object
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        private void Validate()
        {
            if (queryConfiguration == null)
            {
                throw new ModelInvalidException("queryConfiguration is null");
            }
        }

        /// <summary>
        /// Configuration of the Logs query. Define how to query based on which attributes
        /// </summary>
        public class QueryConfig
        {
            internal enum QueryType { HashValueOnly, LSI, GSI }
            internal QueryType type { get; private set; }
            internal string UserId { get; private set; }
            internal DateTime DateMin { get; private set; }
            internal DateTime DateMax { get; private set; }
            internal string GSI_Name { get; private set; }
            internal string GSIHashValue { get; private set; }

            /// <summary>
            /// Query all with only UserId hash key
            /// </summary>
            public static QueryConfig WithUserId(string userId)
            {
                return new QueryConfig { type = QueryType.HashValueOnly, UserId = userId };
            }
            /// <summary>
            /// </summary>
            /// <param name="userId">Identifier</param>
            /// <param name="dateMin">Min date for range key </param>
            /// <param name="dateMax">Max date for range key</param>
            public static QueryConfig WithUserIdAndDate(string userId, DateTime dateMin, DateTime dateMax)
            {
                return new QueryConfig { type = QueryType.LSI, UserId = userId, DateMin = dateMin, DateMax = dateMax, };
            }
            /// <summary>
            /// Query all with GSI 
            /// </summary>
            public static QueryConfig WithGlobalSecodaryIndex(string hashValue, string indexName)
            {
                return new QueryConfig { type = QueryType.GSI, GSIHashValue = hashValue, GSI_Name = indexName };
            }
        }

    }
}