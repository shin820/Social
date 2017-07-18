using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Social.Application.Dto
{
    public class AddFaceboookPageDto
    {
        [Required]
        public string FacebookId { get; set; }
        [Required]
        public string AccessToken { get; set; }
        public string Avatar { get; set; }
        public string Name { get; set; }
        public string Category { get; set; }
        public string SignInAs { get; set; }
        public string Link { get; set; }
    }
}
