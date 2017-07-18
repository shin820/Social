using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Social.Application.Dto
{
    public class FacebookPagesToBeAddDto
    {
        public FacebookSignInAsDto SignInAs { get; set; }
        public List<FacebookPageToBeAddDto> Pages { get; set; }
    }
}
