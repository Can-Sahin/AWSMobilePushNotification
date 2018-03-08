
using AWSMobilePushNotificationService.Operators.DynamoDB.Table;
using AWSMobilePushNotificationService.Operators.Tagging;
using AWSMobilePushNotificationService.Model.OperatorModel;

namespace AWSMobilePushNotificationService.Operators.Registration
{
    internal class RegistrationOperator : AWSMobilePushNotificationOperator
    {
        protected DynamoSubscribersTableOperator subscribersTableOperator { get; }

        public RegistrationOperator(IAWSMobilePushNotificationConfigProvider provider) : base(provider)
        {
            subscribersTableOperator = new DynamoSubscribersTableOperator(provider);
        }
    }
}
