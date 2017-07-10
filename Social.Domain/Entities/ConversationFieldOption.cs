using Framework.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Social.Domain.Entities
{
    [Table("t_Social_ConversationFieldOption")]
    public class ConversationFieldOption : EntityWithSite, IHaveCreation, IHaveModification
    {
        public int FieldId { get; set; }
        public string Name { get; set; }
        public int Order { get; set; }
        public DateTime CreatedTime { get; set; }
        public int CreatedBy { get; set; }
        public int? ModifiedBy { get; set; }
        public DateTime? ModifiedTime { get; set; }

        public virtual ConversationField Field { get; set; }
    }
}
