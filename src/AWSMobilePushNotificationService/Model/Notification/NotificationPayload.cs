using System.Collections.Generic;
using System.Reflection;
using Newtonsoft.Json;

namespace AWSMobilePushNotificationService.Model.Notification
{
    internal class SNSNotificationMessage
    {
        public SNSNotificationMessage(string defaultMessage)
        {
            this.Default = defaultMessage;
        }
        [JsonProperty(PropertyName = "default")]
        public string Default { get; set; } = "";

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string APNS { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string APNS_SANDBOX { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string GCM { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string GCM_SANDBOX { get; set; }

    }

    /// <summary>
    /// Collection of KeyValue pairs for both APNS and GCM PushNotificationPayload
    /// </summary>
    public class NotificationPayload
    {
        /// <summary>
        /// Custom Key Value pairs added to the payload
        /// </summary>
        public Dictionary<string, object> CustomPayload { get; protected set; }

        /// <summary>
        /// Add a custom key value pair to the payload
        /// </summary>
        /// <param name="key">Key</param>
        /// <param name="jsonObject"> Object of type that can be serialized to json string</param>
        public void Add(string key, object jsonObject)
        {
            if (CustomPayload == null)
            {
                CustomPayload = new Dictionary<string, object>();
            }
            CustomPayload.Add(key, jsonObject);
        }

        /// <summary>
        /// 'title' field of the payload
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// 'body' field of the payload
        /// </summary>
        public string Body { get; set; }

        /// <summary>
        /// 'badge' field of the payload
        /// </summary>
        public int? Badge { get; set; }

        /// <summary>
        /// 'sound' field of the payload
        /// </summary>
        public string Sound { get; set; }

    }
    /// <summary>
    /// Collection of GCM Keys for Push Notification's payload
    /// </summary>
    public class GCMNotificationPayload : NotificationPayload
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public GCMNotificationPayload() { }

        /// <summary>
        /// Create with the fields from already a created payload
        /// </summary>
        public GCMNotificationPayload(NotificationPayload from)
        {
            this.CustomPayload = from.CustomPayload;
            this.Title = from.Title;
            this.Body = from.Body;
            this.Badge = from.Badge;
            this.Sound = from.Sound;
        }
        /// <summary>
        /// Add APNS keys and values to this object as CustomPayload keys and value
        /// </summary>
        public void SetCustomPayloadWithAPNSPayload(APNSNotificationPayload apnsPayload)
        {
            foreach (var propertyInfo in typeof(APNSNotificationPayload).GetProperties())
            {
                var currentValue = propertyInfo.GetValue(apnsPayload);
                CustomPayload.Add(propertyInfo.Name, currentValue);
            }
        }
    }

    /// <summary>
    /// Collection of APNS Keys for Push Notification's payload
    /// </summary>
    public class APNSNotificationPayload : NotificationPayload
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public APNSNotificationPayload() { }

        /// <summary>
        /// Create with the fields from already a created payload
        /// </summary>
        public APNSNotificationPayload(NotificationPayload from)
        {
            this.CustomPayload = from.CustomPayload;
            this.Title = from.Title;
            this.Body = from.Body;
            this.Badge = from.Badge;
            this.Sound = from.Sound;
        }

        /// <summary>
        /// 'content-available' field of the payload
        /// </summary>
        public int? Content_Available { get; set; }

        /// <summary>
        /// 'mutable-content' field of the payload
        /// </summary>
        public int? Mutable_Content { get; set; }

        /// <summary>
        /// 'category' field of the payload
        /// </summary>
        public string Category { get; set; }

        /// <summary>
        /// 'thread-id' field of the payload
        /// </summary>
        public string Thread_Id { get; set; }

        /// <summary>
        /// 'title-loc-key' field of the payload
        /// </summary>
        public string Title_Loc_Key { get; set; }

        /// <summary>
        /// 'title-loc-args' field of the payload
        /// </summary>
        public string[] Title_Loc_Args { get; set; }

        /// <summary>
        /// 'action-loc-key' field of the payload
        /// </summary>
        public string Action_Loc_Key { get; set; }

        /// <summary>
        /// 'loc-key' field of the payload
        /// </summary>
        public string Loc_Key { get; set; }

        /// <summary>
        /// 'loc-args' field of the payload
        /// </summary>
        public string[] Loc_Args { get; set; }

        /// <summary>
        /// 'launch-image' field of the payload
        /// </summary>
        public string Launch_Image { get; set; }
    }
}
