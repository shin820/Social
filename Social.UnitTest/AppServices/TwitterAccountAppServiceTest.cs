using Framework.Core;
using Moq;
using Social.Application.AppServices;
using Social.Application.Dto;
using Social.Domain.DomainServices;
using Social.Domain.Entities;
using Social.Infrastructure;
using Social.Infrastructure.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tweetinvi.Models;
using Xunit;

namespace Social.UnitTest.AppServices
{
    public class TwitterAccountAppServiceTest: TestBase
    {
        [Fact]
        public async void ShouldAddAccountAsync()
        {
            //Arrange
            var socialAccountService = new Mock<ISocialAccountService>();
            var socialUserService = new Mock<ISocialUserService>();
            var twitterAuthService = new Mock<ITwitterAuthService>();
            var authenticatedUser = new Mock<IAuthenticatedUser>();
            var twitterCredentials = new Mock<ITwitterCredentials>();

            twitterCredentials.Setup(t => t.AccessToken).Returns("token");
            twitterCredentials.Setup(t => t.AccessTokenSecret).Returns("tokenSecret");
            authenticatedUser.Setup(t => t.Credentials).Returns(twitterCredentials.Object);
            authenticatedUser.Setup(t => t.IdStr).Returns("originalId");
            socialUserService.Setup(t => t.FindByOriginalId("originalId", SocialUserSource.Twitter, SocialUserType.Customer)).Returns(new SocialUser
            {
               Id = 1
            });
            twitterAuthService.Setup(t => t.ValidateAuthAsync("123", "verifier")).ReturnsAsync(authenticatedUser.Object);
            TwitterAccountAppService TwitterAccountAppService = new TwitterAccountAppService(socialAccountService.Object,
                socialUserService.Object,twitterAuthService.Object);
            //Act
            TwitterAccountAppService.AddAccountAsync("123", "verifier");
            //Assert
            socialUserService.Verify(t => t.Update(It.Is<SocialUser>(r => r.Id == 1 && r.Type == SocialUserType.IntegrationAccount && r.SocialAccount.Token == "token")));
            socialAccountService.Verify(t => t.InsertSocialAccountInGeneralDb(It.Is<SocialAccount>(r => r.Token == "token"&& r.TokenSecret == "tokenSecret"
            && r.SocialUser.Id == 1)));

        }

        [Fact]
        public async void ShouldAddAccountAsyncWhenSocialUserIsNotFound()
        {
            //Arrange
            var socialAccountService = new Mock<ISocialAccountService>();
            var socialUserService = new Mock<ISocialUserService>();
            var twitterAuthService = new Mock<ITwitterAuthService>();
            var authenticatedUser = new Mock<IAuthenticatedUser>();
            var twitterCredentials = new Mock<ITwitterCredentials>();

            twitterCredentials.Setup(t => t.AccessToken).Returns("token");
            twitterCredentials.Setup(t => t.AccessTokenSecret).Returns("tokenSecret");
            authenticatedUser.Setup(t => t.Credentials).Returns(twitterCredentials.Object);
            authenticatedUser.Setup(t => t.IdStr).Returns("originalId");
            authenticatedUser.Setup(t => t.Name).Returns("name");
            authenticatedUser.Setup(t => t.ScreenName).Returns("screenName");
            authenticatedUser.Setup(t => t.Email).Returns("email");
            authenticatedUser.Setup(t => t.ProfileImageUrl).Returns("avatar");
            socialUserService.Setup(t => t.FindByOriginalId("", SocialUserSource.Twitter, SocialUserType.Customer)).Returns<SocialUser>(null);
            twitterAuthService.Setup(t => t.ValidateAuthAsync("123", "verifier")).ReturnsAsync(authenticatedUser.Object);
            TwitterAccountAppService TwitterAccountAppService = new TwitterAccountAppService(socialAccountService.Object,
                socialUserService.Object, twitterAuthService.Object);
            //Act
            TwitterAccountAppService.AddAccountAsync("123", "verifier");
            //Assert
            socialAccountService.Verify(t => t.InsertAsync(It.Is<SocialAccount>(r => r.Token == "token" && r.TokenSecret == "tokenSecret"&& r.SocialUser.Name == "name"
            && r.SocialUser.ScreenName == "screenName" && r.SocialUser.Email == "email" && r.SocialUser.Source == SocialUserSource.Twitter && r.SocialUser.Type == SocialUserType.IntegrationAccount
            && r.SocialUser.Avatar == "avatar" && r.SocialUser.OriginalId == "originalId" && r.SocialUser.OriginalLink == TwitterHelper.GetUserUrl("screenName"))));
        }

        [Fact]
        public void ShouldGetAccounts()
        {
            //Arrange
            var socialAccountService = new Mock<ISocialAccountService>();

            socialAccountService.Setup(t => t.FindAll()).Returns(new List<SocialAccount>
            {
                new SocialAccount{ Id =1, SocialUser = new SocialUser{ Source = SocialUserSource.Twitter} }
            }.AsQueryable());
            TwitterAccountAppService TwitterAccountAppService = new TwitterAccountAppService(socialAccountService.Object,
                null,null);
            //Act
            IList<TwitterAccountListDto> twitterAccountListDtos = TwitterAccountAppService.GetAccounts();
            //Assert
            Assert.True(twitterAccountListDtos.Any());
            Assert.Equal(1, twitterAccountListDtos.FirstOrDefault().Id);
        }

        [Fact]
        public void ShouldGetAccount()
        {
            //Arrange
            var socialAccountService = new Mock<ISocialAccountService>();

            socialAccountService.Setup(t => t.FindAccount(1,SocialUserSource.Twitter)).Returns(
                new SocialAccount{ Id =1});
            TwitterAccountAppService TwitterAccountAppService = new TwitterAccountAppService(socialAccountService.Object,
                null, null);
            //Act
            TwitterAccountDto twitterAccountDto =TwitterAccountAppService.GetAccount(1);
            //Assert
            Assert.NotNull(twitterAccountDto);
            Assert.Equal(1, twitterAccountDto.Id);
        }

        [Fact]
        public void ShouldGetAccountWhenSocialAccountIsNotFound()
        {
            //Arrange
            var socialAccountService = new Mock<ISocialAccountService>();

            socialAccountService.Setup(t => t.FindAccount(1, SocialUserSource.Twitter)).Returns<SocialAccount>(null);
            TwitterAccountAppService TwitterAccountAppService = new TwitterAccountAppService(socialAccountService.Object,
                null, null);
            //Act
            Action action =() => TwitterAccountAppService.GetAccount(1);
            //Assert
            Assert.Throws<ExceptionWithCode>(action);
        }

        [Fact]
        public void ShouldDeleteAccountAsync()
        {
            //Arrange
            var socialAccountService = new Mock<ISocialAccountService>();

            socialAccountService.Setup(t => t.FindAccount(1, SocialUserSource.Twitter)).Returns(
                new SocialAccount { Id = 1 });
            TwitterAccountAppService TwitterAccountAppService = new TwitterAccountAppService(socialAccountService.Object,
                null, null);
            //Act
            TwitterAccountAppService.DeleteAccountAsync(1);
            //Assert
            socialAccountService.Verify(t => t.DeleteAsync(It.Is<SocialAccount>(r => r.Id == 1)));
        }

        [Fact]
        public void ShouldDeleteAccountAsyncWhenSocialAccountIsNotFound()
        {
            //Arrange
            var socialAccountService = new Mock<ISocialAccountService>();

            socialAccountService.Setup(t => t.FindAccount(1, SocialUserSource.Twitter)).Returns<SocialAccount>(null);
            TwitterAccountAppService TwitterAccountAppService = new TwitterAccountAppService(socialAccountService.Object,
                null, null);
            //Act
            Func<Task> action = () => TwitterAccountAppService.DeleteAccountAsync(1);
            //Assert
            Assert.ThrowsAsync<ExceptionWithCode>(action);
        }

        [Fact]
        public void ShouldUpdateAccount()
        {
            //Arrange
            var socialAccountService = new Mock<ISocialAccountService>();

            socialAccountService.Setup(t => t.FindAccount(1, SocialUserSource.Twitter)).Returns(
                new SocialAccount { Id = 1 });
            TwitterAccountAppService TwitterAccountAppService = new TwitterAccountAppService(socialAccountService.Object,
                null, null);
            //Act
            TwitterAccountDto twitterAccountDto =TwitterAccountAppService.UpdateAccount(1,new UpdateTwitterAccountDto {IfEnable = true });
            //Assert
            Assert.NotNull(twitterAccountDto);
            socialAccountService.Verify(t => t.Update(It.Is<SocialAccount>(r => r.Id == 1&& r.IfEnable == true)));
            Assert.Equal(1, twitterAccountDto.Id);
            Assert.Equal(true, twitterAccountDto.IfEnable);
        }

        [Fact]
        public void ShouldUpdateAccountWhenSocialAccountIsNotFound()
        {
            //Arrange
            var socialAccountService = new Mock<ISocialAccountService>();

            socialAccountService.Setup(t => t.FindAccount(1, SocialUserSource.Twitter)).Returns<SocialAccount>(null);
            TwitterAccountAppService TwitterAccountAppService = new TwitterAccountAppService(socialAccountService.Object,
                null, null);
            //Act
            Action action = () => TwitterAccountAppService.UpdateAccount(1, new UpdateTwitterAccountDto { IfEnable = true });
            //Assert
            Assert.Throws<ExceptionWithCode>(action);
        }
    }
}
