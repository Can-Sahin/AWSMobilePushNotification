using System;
using System.Collections.Generic;
using System.Text;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;

namespace AWSMobilePushNotificationService.Model.DynamoDb.Tables
{

    internal interface IDynamoTagEntry
    {
        Subscriber Subscriber { get; set; }
        string Tag { get; set; }
        PNTagType TagType { get; }
    }


    internal struct DynamoTypedTagCollection
    {
        public List<Subscriber> Subscribers { get; set; }
        public string Tag { get; set; }
        public PNTagType TagType { get; set; }
    }
    internal struct Subscriber
    {
        public string UserId { get; set; }
        public string Token { get; set; }

        public string SNSSubscriptionArn {get; set;} // when its sns topic tag subscribers

        public string PrimaryKeyValue => ToString();

        public override string ToString()
        {
            return string.Format("{0}:::{1}", UserId, Token);
        }
        internal static Subscriber FromString(string str)
        {
            string[] data = str.Split(new string[] { ":::" }, StringSplitOptions.None);
            if (data.Length != 2) throw new ArgumentOutOfRangeException();

            Subscriber complexData = new Subscriber
            {
                UserId = data[0],
                Token = data[1]
            };
            return complexData;
        }
    }


    internal class SubscriberConverter : IPropertyConverter
    {
        public DynamoDBEntry ToEntry(object value)
        {
            string data = string.Empty;
            if(value is string){
                data = value as string;
            }
            if (value is Subscriber){
                data = ((Subscriber)value).ToString();
            }
            // Subscriber subscriber = (Subscriber)value;
            // // if (subscriber == null) throw new ArgumentOutOfRangeException();

            // string data = subscriber.ToString();
            if(string.IsNullOrEmpty(data)){
                throw new ArgumentOutOfRangeException();
            }
            DynamoDBEntry entry = new Primitive
            {
                Value = data
            };
            return entry;
        }

        public object FromEntry(DynamoDBEntry entry)
        {
            Primitive primitive = entry as Primitive;
            if (primitive == null || !(primitive.Value is String) || string.IsNullOrEmpty((string)primitive.Value))
                throw new ArgumentOutOfRangeException();

            return Subscriber.FromString((string)(primitive.Value));
        }
    }
}
