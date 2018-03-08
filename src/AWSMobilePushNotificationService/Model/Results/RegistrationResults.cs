using AWSMobilePushNotificationService.Model.Exceptions;

namespace AWSMobilePushNotificationService.Model.Results
{
    #pragma warning disable 1591

    /// <summary>
    /// Abstract class represents the result of a Register operation
    /// Result has to be of type of one of the followings: 'RegisterSuccessfulResult', 'RegisterFailedResult'.
    /// It can be casted to one of them by examining the 'IsSuccessful' field
    /// </summary>
    public abstract class RegisterResult : OperationResult
    {
        public RegisterResult() : base() { }

        public RegisterResult(string errMsg) : base(errMsg) { }
        public RegisterResult(string errMsg, ErrorReason errAlias) : base(errMsg, errAlias) { }
        public RegisterResult(AWSMobilePushNotificationServiceException ex) : base(ex) { }


    }

    /// <summary>
    /// Represents a result of a succesfull register operation
    /// </summary>
    public class RegisterSuccessfulResult : RegisterResult
    {
        /// <summary>
        /// SNS EndpointARN
        /// </summary>
        public string EndpointArn { get; set; }
    }

    /// <summary>
    /// Represents a result of a failed register operation
    /// </summary>
    public class RegisterFailedResult : RegisterResult
    {
        public RegisterFailedResult(string errMsg) : base(errMsg) { }
        public RegisterFailedResult(string errMsg, ErrorReason errAlias) : base(errMsg, errAlias) { }
        public RegisterFailedResult(AWSMobilePushNotificationServiceException ex) : base(ex) { }


    }

    /// <summary>
    /// Abstract class represents the result of a UnRegister operation.
    /// Result has to be of type of one of the followings:
    /// 'UnRegisterUserSuccessfulResult', 'UnRegisterSubscriberSuccessfulResult', 'UnRegisterFailedResult'
    /// It can be casted to one of them by examining the 'IsSuccessful' field
    /// </summary>
    public abstract class UnRegisterResult : OperationResult
    {
        public UnRegisterResult() : base() { }

        public UnRegisterResult(string errMsg) : base(errMsg) { }
        public UnRegisterResult(string errMsg, ErrorReason errAlias) : base(errMsg, errAlias) { }
        public UnRegisterResult(AWSMobilePushNotificationServiceException ex) : base(ex) { }

    }

    /// <summary>
    /// Represents a result of a succesfull unregister operation of a User
    /// </summary>
    public class UnRegisterUserSuccessfulResult : UnRegisterResult
    {
        /// <summary>
        /// If evaluates to true, then the user is not found
        /// </summary>
        public bool NotRegistered { get; set; }
    }

    /// <summary>
    /// Represents a result of a succesfull unregister operation of a Subscriber
    /// </summary>
    public class UnRegisterSubscriberSuccessfulResult : UnRegisterResult
    {
        /// <summary>
        /// If evaluates to true, then the subscriber is not found
        /// </summary>
        public bool NotRegistered { get; set; }
    }

    /// <summary>
    /// Represents a result of a failed unregister operation
    /// </summary>
    public class UnRegisterFailedResult : UnRegisterResult
    {
        public UnRegisterFailedResult(string errMsg) : base(errMsg) { }
        public UnRegisterFailedResult(string errMsg, ErrorReason errAlias) : base(errMsg, errAlias) { }
        public UnRegisterFailedResult(AWSMobilePushNotificationServiceException ex) : base(ex) { }


    }

    /// <summary>
    /// Abstract class represents the result of a SwitchSubscriber operation
    /// Result has to be of type of one of the followings:
    /// 'SwitchSubscriberSuccessfulResult', 'SwitchSubscriberFailedResult'.
    /// It can be casted to one of them by examining the 'IsSuccessful' field
    /// </summary>
    public abstract class SwitchSubscriberResult : OperationResult
    {
        public SwitchSubscriberResult() : base() { }
        public SwitchSubscriberResult(string errMsg) : base(errMsg) { }
        public SwitchSubscriberResult(string errMsg, ErrorReason errAlias) : base(errMsg, errAlias) { }
        public SwitchSubscriberResult(AWSMobilePushNotificationServiceException ex) : base(ex) { }

    }

    /// <summary>
    /// Represents a result of a successful SwitchSubscriber operation
    /// </summary>
    public class SwitchSubscriberSuccessfulResult : SwitchSubscriberResult
    {
    }

    /// <summary>
    /// Represents a result of a failed SwitchSubscriber operation
    /// </summary>
    public class SwitchSubscriberFailedResult : SwitchSubscriberResult
    {
        public SwitchSubscriberFailedResult(string errMsg) : base(errMsg) { }

        public SwitchSubscriberFailedResult(string errMsg, ErrorReason errAlias) : base(errMsg, errAlias) { }

        public SwitchSubscriberFailedResult(AWSMobilePushNotificationServiceException ex) : base(ex) { }

    }
    #pragma warning restore 1591
}
