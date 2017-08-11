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
            return new ExceptionWithCode(40001, fbOauthException.Message, fbOauthException);
        }

        public static ExceptionWithCode OriginalPostOrTweetHasBeenDeleted()
        {
            return new ExceptionWithCode(40002, $"The original Post or Tweet is deleted.");
        }

        public static ExceptionWithCode BadRequest(string msg)
        {
            return new ExceptionWithCode(40000, msg);
        }

        public static NotFoundException ConversationIdNotExists(int id)
        {
            return new NotFoundException($"Conversation id '{id}' not exists.");
        }

        public static NotFoundException TwitterAccountNotExists(int id)
        {
            return new NotFoundException($"Twitter Account with id '{id}' not exists.");
        }

        public static NotFoundException FacebookPageNotExists(int id)
        {
            return new NotFoundException($"Facebook page with id '{id}' not exists.");
        }
    }
}
