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
