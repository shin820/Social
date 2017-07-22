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
                // todo : add log
                return;
            }

            Message message = FacebookConverter.ConvertToMessage(token, comment);
            message.SenderId = sender.Id;
            FillParentId(change.Value.PostId, message, comment, socialAccount);

            message.ConversationId = conversation.Id;
            conversation.IfRead = false;
            conversation.Messages.Add(message);
            conversation.Status = sender.Id != socialAccount.SocialUser.Id ? ConversationStatus.PendingInternal : ConversationStatus.PendingExternal;
            conversation.LastMessageSenderId = message.SenderId;
            conversation.LastMessageSentTime = message.SendTime;
            conversation.TryToMakeWallPostVisible(socialAccount);

            await UpdateConversation(conversation);
        }

        private void FillParentId(string postId, Message message, FbComment comment, SocialAccount socialAccount)
        {
            Message parent;
            if (comment.parent == null)
            {
                parent = GetMessage(postId);
                if (parent != null)
                {
                    message.ParentId = parent.Id;
                }
            }
            else
            {
                parent = GetMessage(comment.parent.id);
                if (parent != null)
                {
                    message.ParentId = parent.Id;
                }
            }
        }
    }
}
