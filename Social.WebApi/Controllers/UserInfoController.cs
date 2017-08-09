using Social.Application.AppServices;
using Social.Application.Dto.UserInfo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;

namespace Social.WebApi.Controllers
{
    [RoutePrefix("api/UserInfos")]
    public class UserInfoController
    {
        private IUserInfoAppService _appService;

        public UserInfoController(IUserInfoAppService appService)
        {
            _appService = appService;
        }

        [Route("{OriginalId}", Name = "GetUserInfo")]
        public UserInfoDto GetUserInfo(string OriginalId)
        {
            return _appService.Find(OriginalId);
        }
    }
}