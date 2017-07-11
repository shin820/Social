using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Social.Domain.Entities;
using Social.Infrastructure.Facebook;
using Framework.Core;

namespace Social.Domain.DomainServices.Facebook.Strategies
{
    public class RemovePostCommentStrategy : WebHookStrategy
    {
        private IRepository<Conversation> _conversationRepo;
        private IRepository<Message> _messageRepo;
        private ISocialUserInfoService _socialUserInfoService;

        public RemovePostCommentStrategy(
            IRepository<Conversation> conversationRepo,
            IRepository<Message> messageRepo,
            ISocialUserInfoService socialUserInfoService
            )
        {
            _conversationRepo = conversationRepo;
            _messageRepo = messageRepo;
            _socialUserInfoService = socialUserInfoService;
        }

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
            var message = _messageRepo.FindAll().FirstOrDefault(t => t.SiteId == socialAccount.SiteId && t.SocialId == change.Value.CommentId);
            if (message != null)
            {
                await _messageRepo.DeleteAsync(message);
            }
        }
    }
}
