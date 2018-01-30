using System.Collections.Generic;
using System.Threading.Tasks;
using AWSMobilePushNotificationService.Model;
using AWSMobilePushNotificationService.Operators.Tagging;
using AWSMobilePushNotificationService.Model.Results;
using AWSMobilePushNotificationService.Model.Exceptions;
using System;

namespace AWSMobilePushNotificationService
{
    /// <summary>
    /// Abstract Tag Request
    /// </summary>
    public abstract class AMPSTagRequest : AMPSRequestBase
    {
        internal AMPSTagRequest(IAWSMobilePushNotificationConfigProvider provider) : base(provider) { }

    }
    /// <summary>
    /// Assign User to Tags
    /// </summary>
    public class AddUserToTagRequest : AMPSTagRequest
    {
        /// <summary>
        /// Default constructor for tag request
        /// </summary>
        /// <param name="provider">Configuration for the library resources</param>
        public AddUserToTagRequest(IAWSMobilePushNotificationConfigProvider provider) : base(provider) { }

        /// <summary>
        /// UserId to assign to a Tag
        /// </summary>
        public string UserId { get; set; }

        /// <summary>
        /// List of Tags to add to the User
        /// </summary>
        public List<PNAttributedTag> Tags { get; set; }

        /// <summary>
        /// Validate and make the request 
        /// </summary>
        public async Task<TaggingOperationResult> SendAsync()
        {
            try
            {
                Validate();
                TagOperator tagService = new TagOperator(base.Provider);
                TaggingOperationResult result = await tagService.AddUserToTagsAsync(this.UserId, this.Tags);
                return result;
            }
            catch (AWSMobilePushNotificationServiceException ex)
            {
                return new TagCRUDFailedResult(ex);
            }
            catch (Exception ex)
            {
                if (Provider.CatchAllExceptions)
                {
                    return new TagCRUDFailedResult(ex.Message);
                }
                else
                {
                    throw ex;
                }
            }

        }
        private void Validate()
        {
            string error = string.Empty;
            if (string.IsNullOrEmpty(UserId))
            {
                error = error + "UserId is empty \n";
            }
            if (Tags == null || Tags.Count == 0)
            {
                error = error + "Tags is empty \n";
            }

            if (!string.IsNullOrEmpty(error))
            {
                throw new ModelInvalidException(error);
            }
        }
    }
    /// <summary>
    /// Remove the User from a Tag
    /// </summary>
    public class RemoveUserFromTagsRequest : AMPSTagRequest
    {
        /// <summary>
        /// Default constructor for publish request
        /// </summary>
        /// <param name="provider">Configuration for the library resources</param>
        public RemoveUserFromTagsRequest(IAWSMobilePushNotificationConfigProvider provider) : base(provider) { }

        /// <summary>
        /// Id of the User
        /// </summary>
        public string UserId { get; set; }

        /// <summary>
        /// If set to True, User will be removed from all of its Tags
        /// </summary>
        public bool RemoveAllTags { get; set; }

        /// <summary>
        /// List of Tags to remove the User from
        /// </summary>
        public List<PNTag> Tags { get; set; }

        /// <summary>
        /// Validate and make the request 
        /// </summary>
        public async Task<TaggingOperationResult> SendAsync()
        {
            try
            {
                Validate();
                TagOperator tagService = new TagOperator(base.Provider);
                TagCRUDSuccessfulResult result = await tagService.RemoveUserFromTagsAsync(this.UserId, this.Tags);
                return result;
            }
            catch (AWSMobilePushNotificationServiceException ex)
            {
                return new TagCRUDFailedResult(ex);
            }
            catch (Exception ex)
            {
                if (Provider.CatchAllExceptions)
                {
                    return new TagCRUDFailedResult(ex.Message);
                }
                else
                {
                    throw ex;
                }
            }
        }
        private void Validate()
        {
            string error = string.Empty;
            if (string.IsNullOrEmpty(UserId))
            {
                error = error + "UserId is empty \n";
            }
            if (Tags == null || Tags.Count == 0)
            {
                error = error + "Tags is empty \n";
            }

            if (!string.IsNullOrEmpty(error))
            {
                throw new ModelInvalidException(error);
            }
        }

    }
    /// <summary>
    /// Delete the Tag and all of its Users
    /// </summary>
    public class DeleteTagRequest : AMPSTagRequest
    {
        /// <summary>
        /// Default constructor for publish request
        /// </summary>
        /// <param name="provider">Configuration for the library resources</param>
        public DeleteTagRequest(IAWSMobilePushNotificationConfigProvider provider) : base(provider) { }

        /// <summary>
        /// Tag to delete
        /// </summary>
        public PNTag Tag { get; set; }

        /// <summary>
        /// Validate and make the request 
        /// </summary>
        public async Task<TaggingOperationResult> SendAsync()
        {
            try
            {
                Validate();
                TagOperator tagService = new TagOperator(base.Provider);
                TaggingOperationResult result = await tagService.DeleteTagAsync(this.Tag);
                return result;
            }
            catch (AWSMobilePushNotificationServiceException ex)
            {
                return new TagCRUDFailedResult(ex);
            }
            catch (Exception ex)
            {
                if (Provider.CatchAllExceptions)
                {
                    return new TagCRUDFailedResult(ex.Message);
                }
                else
                {
                    throw ex;
                }
            }
        }
        private void Validate()
        {
            string error = string.Empty;
            if (string.IsNullOrEmpty(Tag?.Tag))
            {
                error = error + "Tag is empty \n";
            }

            if (!string.IsNullOrEmpty(error))
            {
                throw new ModelInvalidException(error);
            }
        }
    }
}
