using System.Threading.Tasks;
using Social.Domain.Entities;
using Social.Infrastructure.Facebook;
using Social.Infrastructure.Enum;

namespace Social.Domain.DomainServices.Facebook
{
    public class NewCommentStrategy : WebHookStrategy
    {
        public override bool IsMatch(FbHookChange change)
        {
            return change.Field == "feed"
                && change.Value.PostId != null
                && change.Value.CommentId != null
                && change.Value.Item == "comment"
                && change.Value.Verb == "add";
        }

        public async override Task Process(SocialAccount socialAccount, FbHookChange change)
        {
            string token = socialAccount.Token;
            if (IsDuplicatedMessage(change.Value.CommentId))
            {
                return;
            }

            FbComment comment = FbClient.GetComment(socialAccount.Token, change.Value.CommentId);
            SocialUser sender = await GetOrCreateFacebookUser(socialAccount.Token, comment.from.id);

            var conversation = GetConversation(change.Value.PostId);
            if (conversation == null)
            {
                return;
            }

            Message message = FacebookConverter.ConvertToMessage(token, comment);
            message.SenderId = sender.Id;
            var parent = GetParent(change.Value.PostId, comment);
            if (parent != null)
            {
                message.ParentId = parent.Id;
                message.ReceiverId = parent.SenderId;
            }

            message.ConversationId = conversation.Id;
            conversation.IfRead = false;
            conversation.Messages.Add(message);
            conversation.Status = sender.Id != socialAccount.Id ? ConversationStatus.PendingInternal : ConversationStatus.PendingExternal;
            conversation.LastMessageSenderId = message.SenderId;
            conversation.LastMessageSentTime = message.SendTime;
            conversation.TryToMakeWallPostVisible(socialAccount);

            await UpdateConversation(conversation);
        }

        private Message GetParent(string postId, FbComment comment)
        {
            Message parent;
            if (comment.parent == null)
            {
                parent = GetMessage(MessageSource.FacebookPost, postId);
            }
            else
            {
                parent = GetMessage(MessageSource.FacebookPostComment, comment.parent.id);
            }

            return parent;
        }
    }
}
