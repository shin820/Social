using Framework.Core;
using Social.Domain.Entities;
using Social.Infrastructure.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Social.Domain.DomainServices
{
    public interface IConversationFieldService : IDomainService<ConversationField>
    {
        IList<ConversationField> FinAllAndFillOptions();
    }


    public class ConversationFieldService : DomainService<ConversationField>, IConversationFieldService
    {
        private IDepartmentService _departmentService;
        private IAgentService _agentService;
        private ISocialUserService _socialUserService;

        public ConversationFieldService(
            IDepartmentService departmentService,
            IAgentService agentService,
            ISocialUserService socialUserService
            )
        {
            _departmentService = departmentService;
            _agentService = agentService;
            _socialUserService = socialUserService;
        }

        public IList<ConversationField> FinAllAndFillOptions()
        {
            var fields = this.FindAll().ToList();
            FillAgentOptions(fields);
            FillDepartmentOptions(fields);
            FillSocialAccountOptions(fields);
            FillDateTimeOptions(fields);

            return fields;
        }

        private void FillAgentOptions(IList<ConversationField> fields)
        {
            if (!fields.Any())
            {
                return;
            }

            var agentsFieldNames = new List<string> { "Agent Assignee", "Replied Agents", "Last Replied Agent" };
            var agentFields = fields.Where(t => t.IfSystem == true && t.DataType == FieldDataType.Option && agentsFieldNames.Contains(t.Name));
            if (agentFields.Any())
            {
                var agents = _agentService.FindAll().ToList();
                foreach (var agentField in agentFields)
                {
                    agentField.Options = agents.Select(t => new ConversationFieldOption
                    {
                        Id = t.Id,
                        Name = t.Name,
                        SiteId = agentField.SiteId,
                        FieldId = agentField.Id,
                        Value = t.Id.ToString()
                    }).ToList();
                }
            }
        }

        private void FillDepartmentOptions(IList<ConversationField> fields)
        {
            if (!fields.Any())
            {
                return;
            }

            var departmentFieldNames = new List<string> { "Department Assignee" };
            var departmentFields = fields.Where(t => t.IfSystem == true && t.DataType == FieldDataType.Option && departmentFieldNames.Contains(t.Name));
            if (departmentFields.Any())
            {
                var departments = _departmentService.FindAll().ToList();
                foreach (var departmentField in departmentFields)
                {
                    departmentField.Options = departments.Select(t => new ConversationFieldOption
                    {
                        Id = t.Id,
                        Name = t.Name,
                        SiteId = departmentField.SiteId,
                        FieldId = departmentField.Id,
                        Value = t.Id.ToString()
                    }).ToList();
                }
            }
        }

        private void FillSocialAccountOptions(IList<ConversationField> fields)
        {
            if (!fields.Any())
            {
                return;
            }

            var fieldNames = new List<string> { "Social Accounts" };
            var matchFileds = fields.Where(t => t.IfSystem == true && t.DataType == FieldDataType.Option && fieldNames.Contains(t.Name));
            if (matchFileds.Any())
            {
                var accounts = _socialUserService.FindAll().Where(t => t.Type == SocialUserType.IntegrationAccount).ToList();
                foreach (var matchField in matchFileds)
                {
                    matchField.Options = accounts.Select(t => new ConversationFieldOption
                    {
                        Id = t.Id,
                        Name = t.Name,
                        SiteId = matchField.SiteId,
                        FieldId = matchField.Id,
                        Value = t.Id.ToString()
                    }).ToList();
                }
            }
        }

        private void FillDateTimeOptions(IList<ConversationField> fields)
        {
            if (!fields.Any())
            {
                return;
            }

            var fieldNames = new List<string> { "Last Message Sent", "Created", "Last Modified" };
            var matchFileds = fields.Where(t => t.IfSystem == true && t.DataType == FieldDataType.DateTime && fieldNames.Contains(t.Name));
            if (matchFileds.Any())
            {
                foreach (var matchField in matchFileds)
                {
                    matchField.Options.Clear();
                    matchField.Options.Add(new ConversationFieldOption { Name = "Today", Value = DateTime.UtcNow.ToString(), SiteId = matchField.SiteId, FieldId = matchField.Id });
                    matchField.Options.Add(new ConversationFieldOption { Name = "Yesterday", Value = DateTime.UtcNow.ToString(), SiteId = matchField.SiteId, FieldId = matchField.Id });
                    matchField.Options.Add(new ConversationFieldOption { Name = "7 Days Ago", Value = DateTime.UtcNow.ToString(), SiteId = matchField.SiteId, FieldId = matchField.Id });
                    matchField.Options.Add(new ConversationFieldOption { Name = "30 Days Ago", Value = DateTime.UtcNow.ToString(), SiteId = matchField.SiteId, FieldId = matchField.Id });
                    matchField.Options.Add(new ConversationFieldOption { Name = "Custom", Value = string.Empty, SiteId = matchField.SiteId, FieldId = matchField.Id });
                }
            }
        }
    }
}
