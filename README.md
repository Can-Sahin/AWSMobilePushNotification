# AWSMobilePushNotification
Serverless Mobile-Push-Notification server-side client and database abstraction/management for sending mobile push notification to APNS and GCM utilizing [Amazon DynamoDB] and [Amazon SimpleNotificationService]

###  Key Features and Functionality
- Provides a **UserId and Token** level abstraction for your server-side application enabling you to only work with 'server-side defined `User` concept' and lifting the burden of token management/storage/validation. 

- NoSQL DynamoDB provides limitless auto scaling and incredible speed for your application's need. 

- `Tagging` enables to send batch notifications to multiple `Users` assigned to tags.

- Handles unavailable devices after notification publish or optionally sets TimeToLive to clean-up later.

- Optionally writes logs to DynamoDB of sent notifications to query later. 


## Overview
Simplified visual summary
![Library Overview]

**Coding Workflow**

All the functions follows the same pattern like AWSSDK.

`Create Request -> Send Request -> Retrieve Response`

1) Create a request to make. (Requires providing IAmazonDynamoDB and IAmazonSimpleNotificationService clients that the library will work on)
2) Modify the properties of the request
3) Send the request async
4) Evaluate the response

## Glossary and Undestanding the working logic
Library uses DynamoDB for the database and its incredibly fast. It creates and handles tables for certain functions, but the tables are available to you for adding extra functionalities or queries that are up to you.

Library provides an abstraction to your back-end's `Users` and internally stores / matches them as notification-sendable devices, letting you to work with only back-end specific identifiers which is called `User` in this library. A `User` can have multiple `Subscribers`, meaning mutliple devices that is available for sending a notification. For instance, a user logged in to multiple devices with the same account. In the absence of `User`, (Userless back-end logic) 'Device Identifier' of the mobile device can be used as an user identifier. 

`User` is a collection of `Subscribers`. Its only represented by UserID

`Subscriber` is tuple of UserId and NotificationToken. This represents a single notification-sendable endpoint for a User. Its the primary key and has to be unique. Without UserId its the tuple of DeviceId and NotificationToken. Still unique.

![DynamoDBSnapshot]

So to register a notification-sendable enpoint to your system you must specify both UserId and NotificationToken. In other words, a `Subscriber` can only be registered not `User`. However, when publishing a notification you can publish both to a `User` or a `Subscriber`. `User` notifications will be send to all of its registered endpoints(Subscribers), since its a collection of endpoints, which is mostly the desired functionality.

`AppIdentfier` property is used for DynamoDB table prefixes so that each of the back-end application is working with seperate tables with selected provisioned throughputs.

## Getting Started 

Requirements: Amazon SNS and Amazon DynamoDB

1. Obtain `Platform Application ARN` from [AWS SNS] using either your APNS Certificate or GCM API Keys
2. Create DynamoDB Tables at the first run (See example code below)
3. Its ready to operate...

## Example Code
Check [WIKI] page for detailed explanation

Implement the abstract class required for the any request to the library (for accessing AWS resources and configuration)

```csharp
public abstract class DefaultAWSMobilePushNotificationConfigProvider : IAWSMobilePushNotificationConfigProvider
{

    public abstract IAmazonDynamoDB DynamoDBClient { get; } //DynamoDB client

    public abstract IAmazonSimpleNotificationService SNSClient { get; } //SNS client

    public abstract string AppIdentifier { get; } //DynamoDB table and SNS topic prefixes

    public abstract TimeSpan? SubscriberTimeToLive { get; } // TTL for a subscriber

    public abstract TimeSpan? IterativeTagTimeToLive { get; } // TTL for a tag

    // Default context. Make sure you set prefixes if you directly implement the interface but not the abstract class
    public DynamoDBContextConfig DynamoDBCcontextConfig
    {
        get
        {
            return new DynamoDBContextConfig
            {
                TableNamePrefix = AppIdentifier,
                ConsistentRead = false,
                SkipVersionCheck = true
            };
        }
    }
    
    public abstract bool CatchAllExceptions { get; } // Catch every exception and put it in the response

}
```

Creating required DynamoDB tables at the first run (you can adjust provisioned throughput at anytime)

```csharp
CreateApplicationTablesRequest request = new CreateApplicationTablesRequest(MY_IMPLEMENTATION_OF_INTERFACE);
request.SubscribersTable = new CreateApplicationTablesRequest.TableProperty { ReadCapacity = 1, WriteCapacity = 1, TTLEnabled = false };

//Rest is optional if you want to enable Tagging
request.TagsTable = new CreateApplicationTablesRequest.TableProperty { ReadCapacity = 1, WriteCapacity = 1, TTLEnabled = false };
request.IterativeTagsTable = new CreateApplicationTablesRequest.TableProperty { ReadCapacity = 1, WriteCapacity = 1, TTLEnabled = false };
request.SNSTopicTagsTable = new CreateApplicationTablesRequest.TableProperty { ReadCapacity = 1, WriteCapacity = 1 };

//Optional Log table to write sending logs
request.LogTable = new CreateApplicationTablesRequest.TableProperty { ReadCapacity = 1, WriteCapacity = 1 };

var result = await request.SendAsync();
            
```

Register the User first after retrieving its token from the device

```csharp
RegisterSubscriberRequest request = new RegisterSubscriberRequest(MY_IMPLEMENTATION_OF_INTERFACE);
request.DeviceId = "DeviceId_from_mobile_device"; //Stored in DynamoDB 
request.UserId = "Identifier_of_this_user_for_my_back-end"; // Use DeviceId if UserID isnt available
request.NotificationToken = "Token_from_mobile_devices";
request.Platform = 1; // 1 for APNS, 2 for GCM

// Put your own ApplicationPlatformARNs
if (request.Platform == Platform.APNS)
{
    request.ApplicationPlatformArn = appSettings.APNSApplicationPlatformARN;
}
else
{
    request.ApplicationPlatformArn = appSettings.GCMApplicationPlatformARN;
}

// For example assign this subscriber to a tag directly at registration
// Check wiki for tagging details and types
var tags = new List<PNAttributedTag>();
tags.Add(new PNAttributedTag{Tag = "AllSubscribers", TagMethod = PNTagType.SNSTopic});
request.Tags = tags;

RegisterResult result = await request.SendAsync();

```

Send a notification to a `User` (meaning to all of its `Subscribers`, devices - endpoints )

```csharp
PublishToUserRequest request = new PublishToUserRequest(MY_IMPLEMENTATION_OF_INTERFACE);

APNSNotificationPayload apnsPayload = new APNSNotificationPayload();
apnsPayload.Body = "MessageBody";
apnsPayload.Title = "Title";
apnsPayload.Content_Available = 1;
apnsPayload.Add("CustomKey", "CustomValue");


GCMNotificationPayload gcmPayload = new GCMNotificationPayload(apnsPayload);
gcmPayload.Body = "MessageBody";
gcmPayload.Title = "Title";
gcmPayload.Add("CustomKey1", "CustomValue1");

request.SetPayload(apnsPayload, gcmPayload);

// Or instead of seperate payloads use mutual payload
/*
NotificationPayload mutualPayload = new NotificationPayload();
mutualPayload.Add("CustomKey2", "CustomValue2");
mutualPayload.Body = "MessageBody";
mutualPayload.Title = "Title";
request.SetPayload(mutualPayload);
    */

request.UserId = "Identifier_of_this_user_for_my_back-end";
request.SnsDefaultMessage = "DefaultSNSMessage";
request.TimeToLive = "TTL_in_Seconds";

// Its published to a User. So, there can be list of results return from publishing multiple subscribers
List<PublishToSNSResult> results = await request.SendAsync();

```

**Check [WIKI] page for API Reference and detailed explanations**

# License
MIT


[Amazon DynamoDB]: <https://aws.amazon.com/dynamodb/>
[Amazon SimpleNotificationService]: <https://aws.amazon.com/sns/>
[AWS SNS]: <https://eu-west-1.console.aws.amazon.com/sns/v2/home?region=eu-west-1#/applications>
[WIKI]: <../../wiki>
[Library Overview]: <ReadMeImages/LibraryDiagram.png>
[DynamoDBSnapshot]: <ReadmeImages/DynamoDBSnapshot.png>