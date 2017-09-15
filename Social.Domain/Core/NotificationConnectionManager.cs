using Framework.Core;
using Framework.Core.SignalR;
using Social.Domain.DomainServices;
using Social.Domain.Entities;
using Social.Domain.Entities.LiveChat;
using Social.Infrastructure;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Social.Infrastructure.Enum;
using LinqKit;
using AutoMapper;
using Social.Infrastructure.Cache;

namespace Social.Domain.Core
{
    public class NotificationConnectionManager : ServiceBase, INotificationConnectionManager
    {
        private readonly static ConnectionMapping<AgentCacheItem> _agentConnectionCache = new ConnectionMapping<AgentCacheItem>();
        private readonly static ConnectionMapping<FilterCacheItem> _filterConnectionCache = new ConnectionMapping<FilterCacheItem>();

        private IFilterService _fitlerService;
        private IDepartmentService _departmentService;
        private IFilterExpressionFactory _filterExpressionFactory;
        private IConversationService _conversationService;

        public NotificationConnectionManager(
            IFilterService filterService,
            IDepartmentService departmentService,
            IFilterExpressionFactory filterExpressionFactory,
            IConversationService conversationService
            )
        {
            _fitlerService = filterService;
            _filterExpressionFactory = filterExpressionFactory;
            _conversationService = conversationService;
            _departmentService = departmentService;
        }

        public void Connect(int siteId, int agentId, string connectionId)
        {
            AgentCacheItem user = GetAgent(siteId, agentId);
            _agentConnectionCache.Add(user, connectionId);
            var filters = GetAgentFilters(siteId, agentId);
            foreach (var filter in filters)
            {
                _filterConnectionCache.Add(filter, connectionId);
            }
        }

        public void Reconnect(int siteId, int agentId, string connectionId)
        {
            AgentCacheItem user = GetAgent(siteId, agentId);
            if (!_agentConnectionCache.GetConnections(user).Contains(connectionId))
            {
                _agentConnectionCache.Add(user, connectionId);
            }
            var filters = GetAgentFilters(siteId, agentId);
            foreach (var filter in filters)
            {
                if (!_filterConnectionCache.GetConnections(filter).Contains(connectionId))
                {
                    _filterConnectionCache.Add(filter, connectionId);
                }
            }
        }

        public void Disconnect(int siteId, int agentId, string connectionId)
        {
            AgentCacheItem user = GetAgent(siteId, agentId);
            _agentConnectionCache.Remove(user, connectionId);
            _filterConnectionCache.Remove(connectionId);
        }

        public void RefreshCacheItem<T>(T key, OperationType oprtType)
        {
            Filter filter = key as Filter;
            if (filter != null)
            {
                RefreshFilterCache(filter, oprtType);
            }
        }

        public IList<string> GetAllConnections()
        {
            return _agentConnectionCache.GetConnections(t => true).ToList();
        }

        public IList<string> GetConnectionsForConversation(int siteId, int conversationId)
        {
            var result = new List<string>();

            var agents = _agentConnectionCache.GetKeys().Where(t => t.SiteId == siteId);
            if (!agents.Any())
            {
                return result;
            }

            var conversation = _conversationService.Find(conversationId);
            if (conversation == null)
            {
                return result;
            }

            foreach (var agent in agents)
            {
                var agentConnections = _agentConnectionCache.GetConnections(agent);
                var filters = _filterConnectionCache.GetKeys(agentConnections).Where(t => t.SiteId == siteId);
                var expressionBuildOptions = new ExpressionBuildOptions
                {
                    UserId = agent.Id,
                    MyDepartmentId = agent.DepartmentId,
                    MyDepartmentMembers = agent.DepartmentMembers
                };

                foreach (var filter in filters)
                {
                    var filterEntity = Mapper.Map<Filter>(filter);
                    var expression = _filterExpressionFactory.Create(filterEntity, expressionBuildOptions);
                    if (expression.Invoke(conversation))
                    {
                        result.AddRange(_filterConnectionCache.GetConnections(filter));
                    }
                }
            }

            return result.Distinct().ToList();
        }

        private void RefreshFilterCache(Filter filter, OperationType oprtType)
        {
            if (oprtType == OperationType.Add)
            {
                AddFilter(filter);
            }
            if (oprtType == OperationType.Update)
            {
                UpdateFilter(filter);
            }
            if (oprtType == OperationType.Delete)
            {
                DeleteFilter(filter);
            }
        }

        private void AddFilter(Filter filter)
        {
            if (filter.IfPublic)
            {
                var connections = _agentConnectionCache.GetConnections(t => t.SiteId == filter.SiteId);
                _filterConnectionCache.Add(Mapper.Map<FilterCacheItem>(filter), connections);
            }
            else
            {
                var connnections = _agentConnectionCache.GetConnections(t => t.SiteId == filter.SiteId && t.Id == filter.CreatedBy);
                _filterConnectionCache.Add(Mapper.Map<FilterCacheItem>(filter), connnections);
            }
        }

        private void UpdateFilter(Filter filter)
        {
            _filterConnectionCache.Remove(Mapper.Map<FilterCacheItem>(filter));
            AddFilter(filter);
        }

        private void DeleteFilter(Filter filter)
        {
            _filterConnectionCache.Remove(Mapper.Map<FilterCacheItem>(filter));
        }

        private IList<FilterCacheItem> GetAgentFilters(int siteId, int agentId)
        {
            IList<Filter> filters = new List<Filter>();
            UnitOfWorkManager.RunWithoutTransaction(siteId, () =>
            {
                filters = _fitlerService.FindFiltersInlucdeConditions(agentId).ToList();
            });
            return filters.Select(t => Mapper.Map<FilterCacheItem>(t)).ToList();
        }

        private AgentCacheItem GetAgent(int siteId, int agentId)
        {
            var user = new AgentCacheItem
            {
                SiteId = siteId,
                Id = agentId,
                IfAdmin = false
            };

            UnitOfWorkManager.RunWithoutTransaction(siteId, () =>
            {
                user.DepartmentId = _departmentService.GetMyDepartmentId(agentId);
                user.DepartmentMembers = _departmentService.GetMyDepartmentMembers(agentId);
            });

            _agentConnectionCache.RefreshKey(user);

            return user;
        }
    }
}
