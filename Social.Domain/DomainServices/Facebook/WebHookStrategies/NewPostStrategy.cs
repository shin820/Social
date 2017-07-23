using System;
using System.Threading.Tasks;
using Social.Domain.Entities;
using Social.Infrastructure.Facebook;
using Framework.Core;
using Social.Infrastructure.Enum;
using System.Linq;

namespace Social.Domain.DomainServices.Facebook
{
    public class NewPostStrategy : WebHookStrategy
    {
        public override bool IsMatch(FbHookChange change)
        {
            bool isTextPost = change.Field == "feed"
                && change.Value.PostId != null
                && change.Value.Item == "post"
                && change.Value.Verb == "add";

            bool isPhotOrVideoPost = change.Field == "feed"
                && change.Value.PostId != null
                //&& change.Value.Item == "photo"
                && change.Value.Verb == "add"
                && !string.IsNullOrWhiteSpace(change.Value.Link)
                && change.Value.IsPublished;

            bool isWallPost = change.Field == "feed"
                && change.Value.PostId != null
                && change.Value.Item == "status"
                && change.Value.Verb == "add"
                && change.Value.IsPublished;

            return isTextPost || isPhotOrVideoPost || isWallPost;
        }

        public override async Task Process(SocialAccount socialAccount, FbHookChange change)
        {
            string token = socialAccount.Token;
            if (IsDuplicatedMessage(change.Value.PostId))
            {
                return;
            }

            FbPost post = await FbClient.GetPost(socialAccount.Token, change.Value.PostId);
            SocialUser sender = await GetOrCreateFacebookUser(socialAccount.Token, post.from.id);

            var existingConversation = GetConversation(change.Value.PostId);
            if (existingConversation != null)
            {
                Message message = FacebookConverter.ConvertToMessage(token, post);
                message.SenderId = sender.Id;
                if (message.SenderId != socialAccount.Id)
                {
                    message.ReceiverId = socialAccount.Id;
                }
                message.ConversationId = existingConversation.Id;
                existingConversation.IfRead = false;
                existingConversation.Messages.Add(message);
                existingConversation.Status = ConversationStatus.PendingInternal;
                existingConversation.LastMessageSenderId = message.SenderId;
                existingConversation.LastMessageSentTime = message.SendTime;
                UpdateConversation(existingConversation);
            }
            else
            {
                Message message = FacebookConverter.ConvertToMessage(token, post);
                message.SenderId = sender.Id;
                if (message.SenderId != socialAccount.Id)
                {
                    message.ReceiverId = socialAccount.Id;
                }
                var conversation = new Conversation
                {
                    OriginalId = change.Value.PostId,
                    Source = ConversationSource.FacebookVisitorPost,
                    Priority = ConversationPriority.Normal,
                    Status = ConversationStatus.New,
                    Subject = GetSubject(message.Content),
                    LastMessageSenderId = message.SenderId,
                    LastMessageSentTime = message.SendTime
                };

                if (change.Value.Item == "status" && message.SenderId == socialAccount.Id)
                {
                    conversation.Source = ConversationSource.FacebookWallPost;
                    conversation.IsHidden = true;
                }

                conversation.Messages.Add(message);
                await AddConversation(socialAccount, conversation);
            }
        }
    }
}
