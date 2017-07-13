using Framework.Core;
using Framework.Core.UnitOfWork;
using Social.Domain.Entities;
using Social.Infrastructure.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;

namespace Social.IntegrationTest
{
    public class TestBase : IDisposable
    {
        protected static DependencyResolver DependencyResolver;
        protected SocialAccount TestFacebookAccount;

        static TestBase()
        {
            DependencyResolver = new DependencyResolver();
            DependencyResolver.Install(new IntegrationTestInstaller());
        }

        private IUnitOfWork _unitOfWork;

        public TestBase()
        {
            _unitOfWork = DependencyResolver.Resolve<IUnitOfWorkManager>().Begin() as IUnitOfWork;
            CreateTestFacebookAccount();
        }

        private void CreateTestFacebookAccount()
        {
            IRepository<SocialUser> socailUserRepo = DependencyResolver.Resolve<IRepository<SocialUser>>();
            TestFacebookAccount = socailUserRepo.FindAll().Where(t => t.SiteId == 10000 && t.SocialId == "1974003879498745").Select(t => t.SocialAccount).FirstOrDefault();

            if (TestFacebookAccount == null)
            {
                SocialUser socialUser = new SocialUser
                {
                    Name = "Shin's Test",
                    SocialId = "1974003879498745",
                    SiteId = 10000,
                    Type = SocialUserType.Facebook,
                    SocialAccount = new SocialAccount
                    {
                        Token = "EAAR8yzs1uVQBAEBWQbsXb8HBP7cEbkTZB7CuqvuQlU1lx0ZCmlZCoy25HsxahMcCGfi8PirSyv5ZA62rvnm21EdZC3PZBK4FXfSti6cc8zIPKMb06fdR15sJqteOW2cIzTV64ZBZBZAnDLBwkNvYszc497CafdqAZCNRaip8w5SjmZCBwZDZD",
                        SiteId = 10000,
                        IfConvertMessageToConversation = true,
                        IfConvertVisitorPostToConversation = true,
                        IfConvertWallPostToConversation = true,
                        IfEnable = true,
                    }
                };

                socailUserRepo.Insert(socialUser);
                TestFacebookAccount = socialUser.SocialAccount;
            }
        }

        public void Dispose()
        {
            _unitOfWork.Dispose();
        }
    }
}
