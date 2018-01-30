using AWSMobilePushNotificationService.Model.Exceptions;

namespace AWSMobilePushNotificationService.Model.Results
{
    #pragma warning disable 1591
    /// <summary>
    /// Abstract class represents the result of a Tag operation
    /// Result has to be of type of one of the followings:
    /// 'TagCRUDSuccessfulResult', 'TagCRUDFailedResult'.
    /// It can be casted to one of them by examining the 'IsSuccessful' field
    /// </summary>
    public abstract class TaggingOperationResult : OperationResult
    {
        public TaggingOperationResult() : base() { }
        public TaggingOperationResult(string errMsg) : base(errMsg) { }
        public TaggingOperationResult(string errMsg, ErrorReason errAlias) : base(errMsg, errAlias) { }
        public TaggingOperationResult(AWSMobilePushNotificationServiceException ex) : base(ex) { }


    }

    /// <summary>
    /// Represents a failed operation on Tag 
    /// </summary>
    public class TagCRUDFailedResult : TaggingOperationResult
    {
        public TagCRUDFailedResult(string errMsg) : base(errMsg) { }
        public TagCRUDFailedResult(string errMsg, ErrorReason errAlias) : base(errMsg, errAlias) { }
        public TagCRUDFailedResult(AWSMobilePushNotificationServiceException ex) : base(ex) { }


    }

    /// <summary>
    /// Represents a successful operation on Tag
    /// </summary>
    public class TagCRUDSuccessfulResult : TaggingOperationResult
    {
        /// <summary>
        /// Number of modifications on Tag has been made by the operation
        /// </summary>
        public int NumberOfModifications { get; set; }
        public TagCRUDSuccessfulResult(int numOfModifitcations)
        {
            this.NumberOfModifications = numOfModifitcations;
        }
        public TagCRUDSuccessfulResult() : base(){}
    }
    #pragma warning restore 1591
}
