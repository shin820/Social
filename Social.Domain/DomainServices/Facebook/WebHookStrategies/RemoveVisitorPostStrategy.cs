using System.Threading.Tasks;
using Social.Domain.Entities;
using Social.Infrastructure.Facebook;

namespace Social.Domain.DomainServices.Facebook
{
    public class RemoveVisitorPostStrategy : WebHookStrategy
    {
        public override bool IsMatch(FbHookChange change)
        {
            return change.Field == "feed"
                && change.Value.PostId != null
                && change.Value.Item == "post"
                && change.Value.Verb == "remove";
        }

        public async override Task Process(SocialAccount socialAccount, FbHookChange data)
        {
            var conversation = GetConversation(socialAccount.SiteId, data.Value.PostId);
            if (conversation != null)
            {
                await DeleteConversation(conversation);
            }
        }
    }
}
