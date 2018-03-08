using Amazon.SimpleNotificationService.Model;
using AWSMobilePushNotificationService.Model.OperatorModel;
using AWSMobilePushNotificationService.Model.Notification;
using AWSMobilePushNotificationService.Model.Exceptions;
using AWSMobilePushNotificationService.Model.Results.Publish;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Threading.Tasks;
using AWSMobilePushNotificationService.Utility;
using AWSMobilePushNotificationService.Model;


namespace AWSMobilePushNotificationService.Operators.Publish
{
    internal class PublishOperator : AWSMobilePushNotificationOperator
    {
        private PublishOperatorModel model { get; }

        public PublishOperator(PublishOperatorModel model, IAWSMobilePushNotificationConfigProvider provider) : base(provider)
        {
            this.model = model;
        }
        protected bool IsTargetPlatformRestrictsSubscriber(Platform? target, Platform subscriber)
        {
            if (target.HasValue)
            {
                if (target != subscriber)
                {
                    return true;
                }
            }
            return false;
        }

        protected string SerializeMessageFromProperties(Platform? platform)
        {
            if (!platform.HasValue)
            {
                GCMNotificationPayload gcmPayload = model.GCMNotificationPayload ?? new GCMNotificationPayload(model.NotificationPayload);
                APNSNotificationPayload apnsPayload = model.APNSNotificationPayload ?? new APNSNotificationPayload(model.NotificationPayload);

                APNSPushMessage generatedApns = GenerateAPNSMessageFromPayload(apnsPayload);
                GCMPushMessage generatedGcm = GenerateGCMMessageFromPayload(gcmPayload);
                return CreateSNSNotificationMessage(generatedApns, generatedGcm, model.TargetEnvironment, apnsPayload.CustomPayload, gcmPayload.CustomPayload, model.SnsDefaultMessage);
            }

            switch (platform.Value)
            {
                case Platform.APNS:
                    APNSNotificationPayload apnsPayload = model.APNSNotificationPayload ?? new APNSNotificationPayload(model.NotificationPayload);
                    APNSPushMessage generatedApns = GenerateAPNSMessageFromPayload(apnsPayload);
                    return CreateSNSNotificationMessage(generatedApns, null, model.TargetEnvironment, apnsPayload.CustomPayload, null, model.SnsDefaultMessage);
                case Platform.GCM:
                    GCMNotificationPayload gcmPayload = model.GCMNotificationPayload ?? new GCMNotificationPayload(model.NotificationPayload);
                    GCMPushMessage generatedGcm = GenerateGCMMessageFromPayload(gcmPayload);
                    return CreateSNSNotificationMessage(null, generatedGcm, model.TargetEnvironment, null, gcmPayload.CustomPayload, model.SnsDefaultMessage);
                default:
                    return "";
            }
        }

        protected GCMPushMessage GenerateGCMMessageFromPayload(GCMNotificationPayload payload)
        {
            GCMPushMessage gcmMessage = new GCMPushMessage();
            gcmMessage.title = payload.Title;
            gcmMessage.body = payload.Body;
            gcmMessage.badge = payload.Badge;
            gcmMessage.sound = payload.Sound;

            return gcmMessage;
        }

        protected APNSPushMessage GenerateAPNSMessageFromPayload(APNSNotificationPayload payload)
        {
            APNSPushMessage apnsMessage = new APNSPushMessage();

            var alert = new APNSPushMessage.APNSAlert();
            alert.title = payload.Title;
            alert.body = payload.Body;
            alert.title_loc_key = payload.Title_Loc_Key;
            alert.title_loc_args = payload.Title_Loc_Args;
            alert.action_loc_key = payload.Action_Loc_Key;
            alert.loc_key = payload.Loc_Key;
            alert.loc_args = payload.Loc_Args;
            alert.launch_image = payload.Launch_Image;
            apnsMessage.alert = alert;

            apnsMessage.badge = payload.Badge;
            apnsMessage.sound = payload.Sound;
            apnsMessage.contentAvailable = payload.Content_Available;
            apnsMessage.mutable_content = payload.Mutable_Content;
            apnsMessage.category = payload.Category;
            apnsMessage.thread_id = payload.Thread_Id;
            return apnsMessage;
        }

        protected string CreateSNSNotificationMessage(APNSPushMessage apnsPushMessage, GCMPushMessage gcmPushMessage, TargetEnvironmentType targetEnvironment,
                                                         Dictionary<string, object> APNSCustomPayload, Dictionary<string, object> GCMCustomPayload, string snsDefaultMessage)
        {
            if (apnsPushMessage == null && gcmPushMessage == null)
            {
                return "";
            }

            var eo = new ExpandoObject();

            dynamic eoDynamic = eo;
            string apnsContents = null;
            string gcmContents = null;

            if (apnsPushMessage != null)
            {
                var dict = APNSCustomPayload;

                if (dict != null)
                {
                    var eoColl = (ICollection<KeyValuePair<string, object>>)eo;

                    foreach (var kvp in dict)
                    {
                        eoColl.Add(kvp);
                    }
                }

                eoDynamic.aps = apnsPushMessage;
                apnsContents = JsonConvert.SerializeObject(eoDynamic).Replace("\"", "\"");
            }
            if (gcmPushMessage != null)
            {
                var dict = GCMCustomPayload;

                dynamic dataFieldValue = gcmPushMessage.ToDynamic();
                if (dict != null)
                {
                    var eoColl = (ICollection<KeyValuePair<string, object>>)dataFieldValue;

                    foreach (var kvp in dict)
                    {
                        eoColl.Add(kvp);
                    }
                }

                eoDynamic.data = dataFieldValue;
                gcmContents = JsonConvert.SerializeObject(eoDynamic).Replace("\"", "\"");
            }

            SNSNotificationMessage notification = new SNSNotificationMessage(snsDefaultMessage);

            bool isSandbox = targetEnvironment == TargetEnvironmentType.Sandbox;
            if (isSandbox)
            {
                notification.APNS_SANDBOX = apnsContents;
                notification.GCM = gcmContents;
            }
            else
            {
                notification.APNS = apnsContents;
                notification.GCM = gcmContents;
            }
            return JsonConvert.SerializeObject(notification);
        }

        protected async Task<PublishToSNSResult> PublishToEndpointAsync(string endpointArn, string message)
        {
            Amazon.SimpleNotificationService.Model.PublishRequest pReq = new Amazon.SimpleNotificationService.Model.PublishRequest();

            if ((model.TimeToLive ?? 0) > 0)
            {
                string ttlAttrKeyAPNS = "";
                switch (model.TargetEnvironment)
                {
                    case TargetEnvironmentType.Sandbox:
                        ttlAttrKeyAPNS = "AWS.SNS.MOBILE.APNS_SANDBOX.TTL";
                        break;
                    default:
                        ttlAttrKeyAPNS = "AWS.SNS.MOBILE.APNS.TTL";
                        break;
                }
                string ttlAttrKeyGCM = "AWS.SNS.MOBILE.GCM.TTL";

                Dictionary<String, MessageAttributeValue> messageAttributes = new Dictionary<String, MessageAttributeValue>();
                MessageAttributeValue value = new MessageAttributeValue();
                value.DataType = "String";
                value.StringValue = model.TimeToLive.ToString();

                messageAttributes[ttlAttrKeyAPNS] = value;
                messageAttributes[ttlAttrKeyGCM] = value;
                pReq.MessageAttributes = messageAttributes;

            }

            pReq.TargetArn = endpointArn;
            pReq.MessageStructure = "json";
            pReq.Message = message;

            try
            {
                PublishResponse pRes = await snsClient.PublishAsync(pReq);
                return new PublishToSNSSuccessfulResult(pRes.MessageId);

            }
            catch (EndpointDisabledException)
            {
                return new PublishToSNSEndpointDisabledResult();
            }
            catch (InvalidParameterException exception)
            {
                if (exception.Message.Contains("No endpoint found for the target arn specified"))
                {
                    return new PublishToSNSEndpointNotFoundResult();
                }
                throw exception;
            }
        }
        protected async Task<PublishToSNSResult> PublishToTopicAsync(string topicArn, string message)
        {
            Amazon.SimpleNotificationService.Model.PublishRequest pReq = new Amazon.SimpleNotificationService.Model.PublishRequest();
            pReq.TopicArn = topicArn;
            pReq.MessageStructure = "json";
            pReq.Message = message;

            try
            {
                PublishResponse pRes = await snsClient.PublishAsync(pReq);
                return new PublishToSNSSuccessfulResult(pRes.MessageId);
            }
            catch (EndpointDisabledException)
            {
                return new PublishToSNSEndpointDisabledResult();
            }
            catch (InvalidParameterException exception)
            {
                if (exception.Message.Contains("No endpoint found for the target arn specified"))
                {
                    return new PublishToSNSEndpointNotFoundResult();
                }
                throw exception;
            }
        }
    }
}
