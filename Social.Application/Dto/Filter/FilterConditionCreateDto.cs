using Social.Infrastructure.Enum;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Social.Application.Dto
{
    public class FilterConditionCreateDto
    {
        [Required]
        public int? FieldId { get; set; }
        [Required]
        public ConditionMatchType? MatchType { get; set; }
        [Required]
        public string Value { get; set; }
        public int Index { get; set; }
    }
}
