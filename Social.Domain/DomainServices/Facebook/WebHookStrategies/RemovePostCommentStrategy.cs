using System.Threading.Tasks;
using Social.Domain.Entities;
using Social.Infrastructure.Facebook;

namespace Social.Domain.DomainServices.Facebook
{
    public class RemovePostCommentStrategy : WebHookStrategy
    {
        public override bool IsMatch(FbHookChange change)
        {
            return change.Field == "feed"
                && change.Value.PostId != null
                && change.Value.CommentId != null
                && change.Value.Item == "comment"
                && change.Value.Verb == "remove";
        }

        public async override Task Process(SocialAccount socialAccount, FbHookChange change)
        {
            var message = GetMessage(change.Value.CommentId);
            if (message != null)
            {
                await DeleteMessage(message);
            }
        }
    }
}
