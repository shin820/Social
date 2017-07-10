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
    public class RemoveVisitorPostStrategy : IWebHookSrategy
    {
        private IRepository<Conversation> _conversationRepo;
        private IRepository<Message> _messageRepo;
        private ISocialUserInfoService _socialUserInfoService;

        public RemoveVisitorPostStrategy(
            IRepository<Conversation> conversationRepo,
            IRepository<Message> messageRepo,
            ISocialUserInfoService socialUserInfoService
            )
        {
            _conversationRepo = conversationRepo;
            _messageRepo = messageRepo;
            _socialUserInfoService = socialUserInfoService;
        }


        public bool IsMatch(FbHookChange change)
        {
            return change.Field == "feed"
                && change.Value.PostId != null
                && change.Value.Item == "post"
                && change.Value.Verb == "remove";
        }

        public async Task Process(SocialAccount socialAccount, FbHookChange data)
        {
            var conversation = _conversationRepo.FindAll().FirstOrDefault(t => t.SiteId == socialAccount.SiteId && t.SocialId == data.Value.PostId);
            if (conversation != null)
            {
                await _conversationRepo.DeleteAsync(conversation);
            }
        }
    }
}
