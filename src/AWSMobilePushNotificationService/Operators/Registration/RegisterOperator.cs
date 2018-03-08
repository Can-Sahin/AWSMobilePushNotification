using Amazon.SimpleNotificationService.Model;
using AWSMobilePushNotificationService.Model.OperatorModel;
using AWSMobilePushNotificationService.Model.DynamoDb.Tables;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AWSMobilePushNotificationService.Model.Results;
using AWSMobilePushNotificationService.Operators.Tagging;


namespace AWSMobilePushNotificationService.Operators.Registration
{

    internal class RegisterOperator : RegistrationOperator
    {
        private const string existingEndpointRegexString = ".*Endpoint (arn:aws:sns[^ ]+) already exists with the same Token.*";
        private Regex existingEndpointRegex = new Regex(existingEndpointRegexString);

        private RegisterSubscriberModel model { get; set; }
        private TagOperator tagOperator { get; }

        public RegisterOperator(IAWSMobilePushNotificationConfigProvider provider) : base(provider)
        {
            tagOperator = new TagOperator(provider);
        }

        public async Task<RegisterSuccessfulResult> RegisterSubscriberAsync(RegisterSubscriberModel model)
        {
            this.model = model;

            DynamoSubscriber subscriber = await subscribersTableOperator.GetSubscriberAsync(model.UserId, model.Token);

            // AWS SNS Mobile Push Algorithm
            // Check: http://docs.aws.amazon.com/sns/latest/dg/mobile-platform-endpoint.html
            bool updateNeeded = false;
            bool createNeeded = (subscriber == null);

            if (createNeeded)
            {
                subscriber = await CreateSubscriberAsync(null);
                createNeeded = false;
            }

            // Look up the endpoint and make sure the data in it is current, even if it was just created
            try
            {
                GetEndpointAttributesRequest geaRequest = new GetEndpointAttributesRequest();
                geaRequest.EndpointArn = subscriber.EndpointARN;
                GetEndpointAttributesResponse geaResponse = await snsClient.GetEndpointAttributesAsync(geaRequest);

                updateNeeded = !geaResponse.Attributes["Token"].Equals(subscriber.NotificationToken) || !geaResponse.Attributes["Enabled"].Equals("true", StringComparison.OrdinalIgnoreCase);

            }
            catch (NotFoundException)
            {
                // We had a stored ARN, but the endpoint associated with it disappeared. Recreate it.
                createNeeded = true;
            }
            if (createNeeded)
            {
                subscriber = await CreateSubscriberAsync(subscriber);
            }

            if (updateNeeded)
            {
                // Endpoint is out of sync with the current data. Update the token and enable it.
                Dictionary<string, string> attrs = new Dictionary<string, string>();
                attrs["Token"] = subscriber.NotificationToken;
                attrs["Enabled"] = "true";
                SetEndpointAttributesRequest seaRequest = new SetEndpointAttributesRequest();
                seaRequest.Attributes = attrs;
                seaRequest.EndpointArn = subscriber.EndpointARN;
                await snsClient.SetEndpointAttributesAsync(seaRequest);
            }

            if (tagOperator.IsTaggingAvailable())
            {
                await tagOperator.SubscribeToTagsAsync(subscriber, model.Tags);
            }

            RegisterSuccessfulResult result = new RegisterSuccessfulResult();
            result.EndpointArn = subscriber.EndpointARN;
            return result;
        }

        private async Task<DynamoSubscriber> CreateSubscriberAsync(DynamoSubscriber prevSubscriber)
        {
            string endpointArn = null;
            try
            {
                CreatePlatformEndpointRequest request = new CreatePlatformEndpointRequest();
                request.PlatformApplicationArn = model.ApplicationPlatformArn;
                request.Token = model.Token;
                CreatePlatformEndpointResponse response = await snsClient.CreatePlatformEndpointAsync(request);
                endpointArn = response.EndpointArn;
            }
            catch (InvalidParameterException ex)
            {
                Console.WriteLine("InvalidParameterException Message = " + ex.Message);

                var match = existingEndpointRegex.Match(ex.Message);
                if (match.Success)
                {
                    // The endpoint already exists for this token, but with additional custom data that
                    // CreateEndpoint doesn't want to overwrite. Just use the existing endpoint.
                    endpointArn = match.Groups[1].Value;
                }
                else
                {
                    throw ex;
                }
            }

            DynamoSubscriber newSubscriber = await CreateDynamoSubscriber(endpointArn);

            if (prevSubscriber != null)
            {
                if (prevSubscriber.EndpointARN != newSubscriber.EndpointARN)
                {
                    await DeleteEndpointAsync(prevSubscriber.EndpointARN);
                }
            }

            if (tagOperator.IsTaggingAvailable())
            {
                await ReAssignTagsForUserWithNewSubscriber(newSubscriber);
            }

            return newSubscriber;
        }
        private async Task DeleteEndpointAsync(string endpointArnToDelete)
        {
            DeleteEndpointRequest deReq = new DeleteEndpointRequest();
            deReq.EndpointArn = endpointArnToDelete;
            await snsClient.DeleteEndpointAsync(deReq);
        }
        private async Task ReAssignTagsForUserWithNewSubscriber(DynamoSubscriber subscriber)
        {
            var currentTags = await tagOperator.GetAllTagsOfUserAsAtrributedTags(model.UserId);
            await tagOperator.SubscribeToTagsAsync(subscriber, currentTags);
        }
        private async Task<DynamoSubscriber> CreateDynamoSubscriber(string endpointArn)
        {
            var subscriber = new DynamoSubscriber(model, endpointArn, Provider.SubscriberTimeToLive);
            await subscribersTableOperator.AddSubscriberAsync(subscriber);
            return subscriber;
        }

    }
}

