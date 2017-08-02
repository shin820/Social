using Social.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tweetinvi;
using Tweetinvi.Models;

namespace Social.Domain.DomainServices.Twitter
{
    public class TweetTreeWalker
    {
        public List<ITweet> BuildTweetTree(ITweet currentTweet)
        {
            List<ITweet> tweets = new List<ITweet>();

            tweets.Add(currentTweet);

            // for performance reason, we just get 10 parent tweet in the tree.
            if (tweets.Count >= 10)
            {
                return tweets;
            }

            if (currentTweet.InReplyToStatusId != null)
            {
                ITweet inReplyToTweet = Tweet.GetTweet(currentTweet.InReplyToStatusId.Value);
                if (inReplyToTweet == null)
                {
                    return tweets;
                }
                tweets.AddRange(BuildTweetTree(inReplyToTweet));
            }

            return tweets;
        }

        public List<string> FindCustomerOriginalIdsInTweetTree(ITweet currentTweet, List<ITweet> tweets, IList<SocialAccount> socialAccounts)
        {
            return FindCustomerOriginalIdsFromAncestorsInTweetTree(currentTweet, tweets, socialAccounts)
                   .Concat(FindCustomerOriginalIdsFromDecendantsInTweetTree(currentTweet, tweets, socialAccounts)).Distinct().ToList();
        }


        private List<string> FindCustomerOriginalIdsFromAncestorsInTweetTree(ITweet currentTweet, List<ITweet> tweets, IList<SocialAccount> socialAccounts)
        {
            List<string> customerOriginalIds = new List<string>();
            if (currentTweet == null)
            {
                return customerOriginalIds;
            }

            if (!IsIntegrationAccount(currentTweet.CreatedBy.IdStr, socialAccounts))
            {
                customerOriginalIds.Add(currentTweet.CreatedBy.IdStr);
            }

            if (currentTweet.UserMentions.Any() && !IsIntegrationAccount(currentTweet.UserMentions.First().IdStr, socialAccounts))
            {
                customerOriginalIds.Add(currentTweet.UserMentions.First().IdStr);
            }

            if (currentTweet.InReplyToStatusId != null)
            {
                ITweet ancestor = tweets.FirstOrDefault(t => t.Id == currentTweet.InReplyToStatusId);
                customerOriginalIds.AddRange(FindCustomerOriginalIdsFromAncestorsInTweetTree(ancestor, tweets, socialAccounts));
            }

            return customerOriginalIds;
        }

        private List<string> FindCustomerOriginalIdsFromDecendantsInTweetTree(ITweet currentTweet, List<ITweet> tweets, IList<SocialAccount> socialAccounts)
        {
            List<string> customerOriginalIds = new List<string>();

            if (!IsIntegrationAccount(currentTweet.CreatedBy.IdStr, socialAccounts))
            {
                customerOriginalIds.Add(currentTweet.CreatedBy.IdStr);
            }

            if (currentTweet.UserMentions.Any() && !IsIntegrationAccount(currentTweet.UserMentions.First().IdStr, socialAccounts))
            {
                customerOriginalIds.Add(currentTweet.UserMentions.First().IdStr);
            }

            var decendants = tweets.Where(t => t.InReplyToStatusId == currentTweet.Id);
            if (!decendants.Any())
            {
                return customerOriginalIds;
            }

            foreach (var decendant in decendants)
            {
                customerOriginalIds.AddRange(FindCustomerOriginalIdsFromDecendantsInTweetTree(decendant, tweets, socialAccounts));
            }

            return customerOriginalIds;
        }

        private bool IsIntegrationAccount(string originalId, IList<SocialAccount> socialAccounts)
        {
            return socialAccounts.Any(t => t.SocialUser.OriginalId == originalId);
        }
    }
}
