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
    [Table("t_Social_ConversationField")]
    public class ConversationField : EntityWithSite, IHaveCreation, IHaveModification
    {
        public ConversationField()
        {
            Conditions = new List<FilterCondition>();
        }

        public int ConversationId { get; set; }
        public bool IfSystem { get; set; }
        public FieldDataType DataType { get; set; }
        public string Name { get; set; }
        public DateTime CreatedTime { get; set; }
        public int CreatedBy { get; set; }
        public int? ModifiedBy { get; set; }
        public DateTime? ModifiedTime { get; set; }

        public virtual Conversation Conversation { get; set; }
        public virtual IList<FilterCondition> Conditions { get; set; }

    }
}
