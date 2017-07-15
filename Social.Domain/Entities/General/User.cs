using Framework.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Social.Domain.Entities.LiveChat
{
    [Table("t_User")]
    public class User : Entity, IShardingBySiteId
    {
        [Required]
        [MaxLength(128)]
        public string Name { get; set; }

        public short UserType { get; set; }

        public bool IfActive { get; set; }

        public bool IfDeleted { get; set; }
    }
}
