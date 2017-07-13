using Social.Infrastructure.Facebook;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Social.IntegrationTest.Infrastructure.Facebook
{
    public class FbClientTest : TestBase
    {
        [Fact]
        public async Task ShouldGetTaggedVisitorPost()
        {
            var result = await FbClient.GetTaggedVisitorPosts(TestFacebookAccount.SocialUser.SocialId, TestFacebookAccount.Token);

            Assert.True(result.data.Count > 0);
            Assert.NotNull(result.paging);
        }

        [Fact]
        public async Task ShouldGetNextPageTaggedVisitorPost()
        {
            var result = await FbClient.GetTaggedVisitorPosts(TestFacebookAccount.SocialUser.SocialId, TestFacebookAccount.Token, 1);

            result = await FbClient.GetTaggedVisitorPosts(TestFacebookAccount.SocialUser.SocialId, TestFacebookAccount.Token, 1, result.paging.cursors.after);

            Assert.NotNull(result.data);
            Assert.NotNull(result.paging);
        }

        [Fact]
        public async Task ShouldGetPostComment()
        {
            var comment = await FbClient.GetPostComment("153568051877788_153962908504969", TestFacebookAccount.Token);
            Assert.NotNull(comment);
        }


        [Fact]
        public async Task ShouldGetLatestMessageFromConversation()
        {
            FbMessage message = await FbClient.GetLastMessageFromConversationId
                (TestFacebookAccount.Token, "t_mid.$cAAdZrm4k4UZh9X1vd1bxDgkg7Bo9");

            Assert.NotNull(message);
            Assert.NotEmpty(message.Id);
            Assert.NotEmpty(message.SenderId);
            Assert.NotEmpty(message.ReceiverId);
        }
    }
}
