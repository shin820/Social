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
            var result = await FbClient.GetTaggedVisitorPosts(TestFacebookAccount.SocialUser.OriginalId, TestFacebookAccount.Token);

            Assert.True(result.data.Count > 0);
            Assert.NotNull(result.paging);
        }

        [Fact]
        public async Task ShouldGetPostComment()
        {
            var comment = await FbClient.GetPostComment("153568051877788_153962908504969", TestFacebookAccount.Token);
            Assert.NotNull(comment);
        }
    }
}
