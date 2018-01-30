
using AWSMobilePushNotificationService.Model.Exceptions;

namespace AWSMobilePushNotificationService.Model.Results
{
    /// <summary>
    /// Base class represents a result of an operation
    /// </summary>
    public class OperationResult
    {
        /// <summary>
        /// Flag for determining if the operation has succeded or failed
        /// </summary>
        public bool IsSuccessful { get; set; }

        /// <summary>
        /// Error message produced if the operation has failed
        /// </summary>
        public string ErrorMessage { get; set; }

        /// <summary>
        /// Alias for the reason of operation's failure
        /// </summary>
        public ErrorReason? ErrorAlias { get; set; }

        /// <summary>
        /// Successful by default
        /// </summary>
        public OperationResult()
        {
            IsSuccessful = true;
        }

        /// <summary>
        /// Failed with only a message
        /// </summary>
        public OperationResult(string errorMessage)
        {
            IsSuccessful = false;
            ErrorMessage = errorMessage;
        }

        /// <summary>
        /// Failed with a message and a reason
        /// </summary>
        public OperationResult(string errorMessage, ErrorReason errorAlias)
        {
            IsSuccessful = false;
            ErrorMessage = errorMessage;
            ErrorAlias = errorAlias;
        }
        /// <summary>
        /// Failed with a AWSMobilePushNotificationServiceException
        /// </summary>
        public OperationResult(AWSMobilePushNotificationServiceException ex)
        {
            IsSuccessful = false;
            ErrorMessage = ex.Message;
            ErrorAlias = ex.ErrorAlias;
        }
    }
}
