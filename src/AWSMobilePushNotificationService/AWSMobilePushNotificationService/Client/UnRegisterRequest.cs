using System.Collections.Generic;
using System.Threading.Tasks;
using AWSMobilePushNotificationService.Operators.Registration;
using AWSMobilePushNotificationService.Model.OperatorModel;
using AWSMobilePushNotificationService.Model;
using AWSMobilePushNotificationService.Model.Results;
using AWSMobilePushNotificationService.Model.Exceptions;
using System;

namespace AWSMobilePushNotificationService
{
    /// <summary>
    /// Unregister and remove the 'Subscriber' from the system with all of its relations
    /// A Subscriber is represented with 'UserId,Token' tuple together.
    /// </summary>
    public class UnRegisterSubscriberRequest : AMPSSubscriberRequest
    {
        /// <summary>
        /// Default constructor for unregister request
        /// </summary>
        /// <param name="provider">Configuration for the library resources</param>
        public UnRegisterSubscriberRequest(IAWSMobilePushNotificationConfigProvider provider) : base(provider) { }

        /// <summary>
        /// Validate and make the request 
        /// </summary>
        public async Task<UnRegisterResult> SendAsync()
        {
            try
            {
                Validate();
                UnRegisterOperator unRegisterService = new UnRegisterOperator(base.Provider);
                UnRegisterResult result = await unRegisterService.UnRegisterSubscriberAsync(this.UserId, this.NotificationToken);
                return result;
            }
            catch (AWSMobilePushNotificationServiceException ex)
            {
                return new UnRegisterFailedResult(ex);
            }
            catch (Exception ex)
            {
                if (Provider.CatchAllExceptions)
                {
                    return new UnRegisterFailedResult(ex.Message);
                }
                else
                {
                    throw ex;
                }
            }
        }

        private new void Validate()
        {
            string error = base.Validate();
            if (!string.IsNullOrEmpty(error))
            {
                throw new ModelInvalidException(error);
            }
        }
    }

    /// <summary>
    /// Unregister and remove the 'User' and its subscriptions from the system with all of its relations.
    /// A User is represented with 'UserId' only and might have multiple subscriptions.
    /// </summary>
    public class UnRegisterUserRequest : AMPSUserRequest
    {
        /// <summary>
        /// Default constructor for publish request
        /// </summary>
        /// <param name="provider">Configuration for the library resources</param>
        public UnRegisterUserRequest(IAWSMobilePushNotificationConfigProvider provider) : base(provider) { }

        /// <summary>
        /// Validate and make the request 
        /// </summary>
        public async Task<UnRegisterResult> SendAsync()
        {
            try
            {
                Validate();
                UnRegisterOperator unRegisterService = new UnRegisterOperator(base.Provider);
                UnRegisterResult result = await unRegisterService.UnRegisterUserAsync(this.UserId);
                return result;
            }
            catch (AWSMobilePushNotificationServiceException ex)
            {
                return new UnRegisterFailedResult(ex);
            }
            catch (Exception ex)
            {
                if (Provider.CatchAllExceptions)
                {
                    return new UnRegisterFailedResult(ex.Message);
                }
                else
                {
                    throw ex;
                }
            }

        }
        private new void Validate()
        {
            string error = base.Validate();
            if (!string.IsNullOrEmpty(error))
            {
                throw new ModelInvalidException(error);
            }
        }
    }

}