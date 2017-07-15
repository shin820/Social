using Framework.Core;
using Framework.Core.UnitOfWork;
using Social.Application;
using Social.Domain;
using Social.Domain.Entities;
using Social.Infrastructure.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;

namespace Social.IntegrationTest
{
    public class TestBase : IDisposable
    {
        protected static DependencyResolver DependencyResolver;
        protected SocialAccount TestFacebookAccount;
        protected IUnitOfWorkManager UnitOfWorkManager { get; set; }
        protected IUnitOfWork CurrentUnitOfWork { get { return UnitOfWorkManager.Current; } }

        static TestBase()
        {
            DependencyResolver = new DependencyResolver();
            DependencyResolver.RegisterAssemblyByConvention(Assembly.GetExecutingAssembly());
            new ApplicationServicesRegistrar(DependencyResolver).RegisterServices();
        }

        private IUnitOfWork _unitOfWork;

        public TestBase()
        {
            UnitOfWorkManager = DependencyResolver.Resolve<IUnitOfWorkManager>();
            _unitOfWork = UnitOfWorkManager.Begin(/*new UnitOfWorkOptions { IsTransactional = false }*/) as IUnitOfWork;
            CreateTestFacebookAccount();
        }

        private void CreateTestFacebookAccount()
        {
            using (var uow = UnitOfWorkManager.Begin(TransactionScopeOption.RequiresNew))
            {
                using (CurrentUnitOfWork.SetSiteId(10000))
                {
                    IRepository<SocialUser> socailUserRepo = DependencyResolver.Resolve<IRepository<SocialUser>>();
                    TestFacebookAccount = socailUserRepo.FindAll().Where(t => t.SocialId == "1974003879498745").Select(t => t.SocialAccount).FirstOrDefault();

                    if (TestFacebookAccount == null)
                    {
                        SocialUser socialUser = new SocialUser
                        {
                            Name = "Shin's Test",
                            SocialId = "1974003879498745",
                            Type = SocialUserType.Facebook,
                            SocialAccount = new SocialAccount
                            {
                                Token = "EAAR8yzs1uVQBAEBWQbsXb8HBP7cEbkTZB7CuqvuQlU1lx0ZCmlZCoy25HsxahMcCGfi8PirSyv5ZA62rvnm21EdZC3PZBK4FXfSti6cc8zIPKMb06fdR15sJqteOW2cIzTV64ZBZBZAnDLBwkNvYszc497CafdqAZCNRaip8w5SjmZCBwZDZD",
                                IfConvertMessageToConversation = true,
                                IfConvertVisitorPostToConversation = true,
                                IfConvertWallPostToConversation = true,
                                IfEnable = true,
                            }
                        };

                        socailUserRepo.Insert(socialUser);
                        TestFacebookAccount = socialUser.SocialAccount;
                    }
                    uow.Complete();
                }
            }
        }

        public void Dispose()
        {
            _unitOfWork.Dispose();
        }
    }
}
