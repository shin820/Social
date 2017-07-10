using Framework.Core;
using Social.Domain.Entities;
using Social.Infrastructure.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Social.IntegrationTest
{
    public class TestBase
    {
        protected static DependencyResolver DependencyResolver;
        protected SocialAccount TestFacebookAccount;

        static TestBase()
        {
            DependencyResolver = new DependencyResolver();
            DependencyResolver.Install(new IntegrationTestInstaller());
        }

        public TestBase()
        {
            IRepository<SocialAccount> socailAccountRepo = DependencyResolver.Resolve<IRepository<SocialAccount>>();
            TestFacebookAccount = socailAccountRepo.FindAll().FirstOrDefault(t => t.SiteId == 10000 && t.SocialUser.SocialId == "1974003879498745");

            if (TestFacebookAccount == null)
            {
                TestFacebookAccount = new SocialAccount
                {
                    SocialUser = new SocialUser
                    {
                        Name = "Shin's Test",
                        SocialId = "1974003879498745",
                        SiteId = 10000,
                        Type = SocialUserType.Facebook
                    },
                    Token = "EAAR8yzs1uVQBAEBWQbsXb8HBP7cEbkTZB7CuqvuQlU1lx0ZCmlZCoy25HsxahMcCGfi8PirSyv5ZA62rvnm21EdZC3PZBK4FXfSti6cc8zIPKMb06fdR15sJqteOW2cIzTV64ZBZBZAnDLBwkNvYszc497CafdqAZCNRaip8w5SjmZCBwZDZD",
                    SiteId = 10000,
                    IfConvertMessageToConversation = true,
                    IfConvertVisitorPostToConversation = true,
                    IfConvertWallPostToConversation = true,
                    IfEnable = true,
                };

                socailAccountRepo.Insert(TestFacebookAccount);
            }
        }
    }
}
