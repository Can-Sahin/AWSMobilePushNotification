using AWSMobilePushNotificationService.Model.DynamoDb.Tables;
using System.Collections.Generic;
using AWSMobilePushNotificationService.Model.Exceptions;

namespace AWSMobilePushNotificationService.Model.OperatorModel
{
    internal interface IRegistrationOperatorModel
    {
        string UserId { get; set; }
    }
    internal class RegistrationModel : IRegistrationOperatorModel
    {
        public string UserId { get; set; }
        public string Token { get; set; }
        public string DeviceId { get; set; }

        public RegistrationModel(AMPSSubscriberRequest model, bool consistentRead = false)
        {
            this.UserId = model.UserId;
            this.Token = model.NotificationToken;
        }
        public RegistrationModel(DynamoSubscriber model, bool consistentRead = false)
        {
            this.UserId = model.UserId;
            this.Token = model.NotificationToken;
            this.DeviceId = model.DeviceId;
        }

        public string Validate()
        {
            string error = string.Empty;
            if (string.IsNullOrEmpty(UserId))
            {
                error = error + "UserId is empty \n";
            }
            if (string.IsNullOrEmpty(Token))
            {
                error = error + "Token is empty \n";
            }
            return error;
        }

    }
    internal class RegisterSubscriberModel : RegistrationModel
    {
        public Platform Platform { get; set; }
        public List<PNAttributedTag> Tags { get; set; }
        public string ApplicationPlatformArn { get; set; }

        public RegisterSubscriberModel(RegisterSubscriberRequest model) : base(model)
        {
            this.DeviceId = model.DeviceId;
            this.Platform = model.Platform;
            this.Tags = model.Tags;
            this.ApplicationPlatformArn = model.ApplicationPlatformArn;
        }
        public RegisterSubscriberModel(DynamoSubscriber model, List<PNAttributedTag> tags, string applicationPlatformArn) : base(model)
        {
            this.Platform = model.Platform;
            this.Tags = tags;
            this.ApplicationPlatformArn = applicationPlatformArn;
        }
        public new void Validate()
        {
            string error = base.Validate();
            if (string.IsNullOrEmpty(ApplicationPlatformArn))
            {
                error = error + "ApplicationPlatformArn is empty \n";
            }

            if (!string.IsNullOrEmpty(error))
            {
                throw new ModelInvalidException(error);
            }
        }
    }
    internal class SwitchSubscriberModel
    {
        public string NewUserId { get; set; }
        public string NewToken { get; set; }
        public string PrevUserId { get; set; }
        public string PrevToken { get; set; }
        public string ApplicationPlatformArn { get; set; }
        public Platform Platform { get; set; }
        public List<PNTag> TagsToIgnore { get; set; }

        public SwitchSubscriberModel(SwitchSubscriberRequest model)
        {
            this.NewUserId = model.NewUserId;
            this.NewToken = model.NewNotificationToken;
            this.PrevUserId = model.UserId;
            this.PrevToken = model.NotificationToken;
            this.ApplicationPlatformArn = model.ApplicationPlatformArn;
            this.Platform = model.Platform;
            this.TagsToIgnore = model.TagsToIgnore;
        }

        public void Validate()
        {
            string error = "";
            if (string.IsNullOrEmpty(PrevUserId))
            {
                error = error + "PrevUserId is empty \n";
            }
            if (string.IsNullOrEmpty(PrevToken))
            {
                error = error + "PrevUserToken is empty \n";
            }
            if (string.IsNullOrEmpty(NewUserId))
            {
                error = error + "NewUserId is empty \n";
            }
            if (string.IsNullOrEmpty(NewToken))
            {
                error = error + "NewToken is empty \n";
            }

            if (!string.IsNullOrEmpty(error))
            {
                throw new ModelInvalidException(error);
            }
        }
    }
}
