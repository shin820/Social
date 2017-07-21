using Facebook;
using Framework.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Social.Infrastructure
{
    public static class SocialExceptions
    {
        public static ExceptionWithCode FacebookOauthException(FacebookOAuthException fbOauthException)
        {
            return new ExceptionWithCode(40000, fbOauthException.Message, fbOauthException);
        }

        public static ExceptionWithCode InvalidParameter(string key)
        {
            return new ExceptionWithCode(40001, $"Invalid parameter '{key}'.");
        }

        public static BadRequestException BadReqeust(string msg)
        {
            return new BadRequestException(msg);
        }

        public static NotFoundException ConversationIdNotExists(int id)
        {
            return new NotFoundException($"Conversation id '{id}' not exists.");
        }
    }
}
