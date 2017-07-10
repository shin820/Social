using Framework.Core;
using Social.Infrastructure.Enum;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Social.Domain.Entities
{
    [Table("t_Social_FilterCondition")]
    public class FilterCondition : EntityWithSite, IHaveModification, IHaveCreation
    {
        public int FilterId { get; set; }
        public int FieldId { get; set; }
        public ConditionMatchType MatchType { get; set; }
        public string Value { get; set; }
        public int CreatedBy { get; set; }
        public DateTime CreatedTime { get; set; }
        public int? ModifiedBy { get; set; }
        public DateTime? ModifiedTime { get; set; }

        public virtual Filter Filter { get; set; }
        public virtual ConversationField Field { get; set; }
    }
}
