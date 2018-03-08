using System.Collections.Generic;
using AWSMobilePushNotificationService.Model.Exceptions;
using AWSMobilePushNotificationService.Model.Notification;

namespace AWSMobilePushNotificationService.Model.OperatorModel
{
    internal interface IPublishOperatorModel { }


    internal class PublishOperatorModel : IPublishOperatorModel
    {
        public Platform? TargetPlatform { get; set; }
        public TargetEnvironmentType TargetEnvironment { get; set; }
        public string SnsDefaultMessage { get; set; }
        public int? TimeToLive { get; set; }

        public NotificationPayload NotificationPayload { get; private set; }
        public APNSNotificationPayload APNSNotificationPayload { get; private set; }
        public GCMNotificationPayload GCMNotificationPayload { get; private set; }

        public PublishOperatorModel(PublishRequest model, bool consistentRead = false)
        {
            this.TimeToLive = model.TimeToLive;
            this.SnsDefaultMessage = model.SnsDefaultMessage;
            this.TargetEnvironment = model.TargetEnvironment;
            this.TargetPlatform = model.TargetPlatform;
            this.NotificationPayload = model.NotificationPayload;
            this.APNSNotificationPayload = model.APNSNotificationPayload ?? new APNSNotificationPayload(model.NotificationPayload);
            this.GCMNotificationPayload = model.GCMNotificationPayload ?? new GCMNotificationPayload(model.NotificationPayload);
        }
        public string Validate()
        {
            string error = string.Empty;
            return error;
        }
    }
    internal class PublishToUserOperatorModel : PublishOperatorModel
    {
        public string UserId { get; set; }

        public PublishToUserOperatorModel(PublishToUserRequest model) : base(model)
        {
            this.UserId = model.UserId;
        }
        public new void Validate()
        {
            string error = base.Validate();
            if (string.IsNullOrEmpty(UserId))
            {
                error = error + "UserId is empty \n";
            }
            if (!string.IsNullOrEmpty(error))
            {
                throw new ModelInvalidException(error);
            }
        }

    }
    internal class PublishToTagOperatorModel : PublishOperatorModel
    {
        public string Tag { get; set; }

        public PublishToTagOperatorModel(PublishToTagRequest model) : base(model)
        {
            this.Tag = model.Tag;
        }
        public new void Validate()
        {
            string error = base.Validate();
            if (string.IsNullOrEmpty(Tag))
            {
                error = error + "TagName is empty \n";
            }
            if (!string.IsNullOrEmpty(error))
            {
                throw new ModelInvalidException(error);
            }
        }
    }
}
