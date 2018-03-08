using Newtonsoft.Json;

namespace AWSMobilePushNotificationService.Model.Notification
{

    internal class GCMPushMessage
    {
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string title { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string body { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int? badge { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string sound { get; set; }
    }

    internal class APNSPushMessage
    {
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public APNSAlert alert { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int? badge { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string sound { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, PropertyName = "content-available")]
        public int? contentAvailable { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, PropertyName = "mutable-content")]
        public int? mutable_content { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string category { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore, PropertyName = "thread-id")]
        public string thread_id { get; set; }

        public class APNSAlert
        {
            public APNSAlert() { }

            [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
            public string title { get; set; }

            [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
            public string body { get; set; }

            [JsonProperty(NullValueHandling = NullValueHandling.Ignore, PropertyName = "title-loc-key")]
            public string title_loc_key { get; set; }

            [JsonProperty(NullValueHandling = NullValueHandling.Ignore, PropertyName = "title-loc-args")]
            public string[] title_loc_args { get; set; }

            [JsonProperty(NullValueHandling = NullValueHandling.Ignore, PropertyName = "action-loc-key")]
            public string action_loc_key { get; set; }

            [JsonProperty(NullValueHandling = NullValueHandling.Ignore, PropertyName = "loc-key")]
            public string loc_key { get; set; }

            [JsonProperty(NullValueHandling = NullValueHandling.Ignore, PropertyName = "loc-args")]
            public string[] loc_args { get; set; }

            [JsonProperty(NullValueHandling = NullValueHandling.Ignore, PropertyName = "launch-image")]
            public string launch_image { get; set; }
        }
    }
}