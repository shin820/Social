using Facebook;
using Framework.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Social.Infrastructure
{
    public static class ExceptionHelper
    {
        public static ExceptionWithCode FacebookOauthException(FacebookOAuthException fbOauthException)
        {
            return new ExceptionWithCode(40000, fbOauthException.Message, fbOauthException);
        }

        public static ExceptionWithCode InvalidParameter(string key)
        {
            return new ExceptionWithCode(40001, $"Invalid parameter '{key}'.");
        }
    }
}
