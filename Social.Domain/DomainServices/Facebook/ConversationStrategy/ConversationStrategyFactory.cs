using Framework.Core;
using Social.Infrastructure.Facebook;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Social.Domain.DomainServices.Facebook
{
    public class ConversationStrategyFactory : IConversationStrategyFactory
    {
        private IDependencyResolver _dependencyResolver;

        public ConversationStrategyFactory(IDependencyResolver dependencyResolver)
        {
            _dependencyResolver = dependencyResolver;
        }

        public IConversationSrategy Create(FbHookChange change)
        {
            var strategies = _dependencyResolver.ResolveAll<IConversationSrategy>();
            var strategory = strategies.FirstOrDefault(t => t.IsMatch(change));
            return strategory;
        }
    }
}
