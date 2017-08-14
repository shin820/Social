using Social.Application.AppServices;
using Social.Application.Dto;
using Social.Application.Dto.UserInfo;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Http.Description;

namespace Social.WebApi.Controllers
{
    [RoutePrefix("api/UserInfos")]
    public class UserInfoController : ApiController
    {
        private IUserInfoAppService _appService;

        public UserInfoController(IUserInfoAppService appService)
        {
            _appService = appService;
        }

        [Route("{OriginalId}", Name = "GetUserInfo")]
        public UserInfoDto GetUserInfo([MaxLength(200)]string OriginalId)
        {
            return _appService.Find(OriginalId);
        }

        [Route("{OriginalId}/user-conversations")]
        public IList<ConversationDto> GetUserConversations([MaxLength(200)]string OriginalId)
        {
            return _appService.FindConversations(OriginalId);
        }
    }
}