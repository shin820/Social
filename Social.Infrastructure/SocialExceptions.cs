using Facebook;
using Framework.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Social.Infrastructure
{
    public static class SocialExceptions
    {
        public static ExceptionWithCode FacebookOauthException(FacebookOAuthException fbOauthException)
        {
            return new ExceptionWithCode(40001, fbOauthException.Message, fbOauthException);
        }

        public static ExceptionWithCode OriginalPostOrTweetHasBeenDeleted()
        {
            return new ExceptionWithCode(40002, $"The original Post or Tweet is deleted.");
        }

        public static ExceptionWithCode BadRequest(string msg)
        {
            return new ExceptionWithCode(HttpStatusCode.BadRequest, 40000, msg);
        }

        public static ExceptionWithCode ConversationIdNotExists(int id)
        {
            return NotFound($"Conversation id '{id}' not exists.");
        }

        public static ExceptionWithCode TwitterAccountNotExists(int id)
        {
            return NotFound($"Twitter Account with id '{id}' not exists.");
        }

        public static ExceptionWithCode FacebookPageNotExists(int id)
        {
            return NotFound($"Facebook page with id '{id}' not exists.");
        }

        public static ExceptionWithCode FilterNotExists(int id)
        {
            return NotFound($"Filter with id '{id}' not exists.");
        }

        private static ExceptionWithCode NotFound(string msg)
        {
            return new ExceptionWithCode(HttpStatusCode.NotFound, 40004, msg);
        }

    }
}
