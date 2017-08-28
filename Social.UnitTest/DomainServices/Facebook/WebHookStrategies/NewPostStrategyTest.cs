using Framework.Core;
using Moq;
using Social.Domain.DomainServices.Facebook;
using Social.Domain.Entities;
using Social.Infrastructure.Facebook;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Social.UnitTest.DomainServices.Facebook.WebHookStrategies
{
    public class NewPostStrategyTest
    {
        [Fact]
        public void ShouldMatchStrategyIfVisitorAddTextPost()
        {
            // Arrange
            var dependencyResolverMock = new Mock<IDependencyResolver>();
            var strategy = new NewPostStrategy(dependencyResolverMock.Object);
            var fbHookChange = new FbHookChange
            {
                Field = "feed",
                Value = new FbHookChangeValue
                {
                    Item = "post",
                    PostId = "123",
                    Verb = "add"
                }
            };

            // Act
            bool isMatch = strategy.IsMatch(fbHookChange);

            // Assert
            Assert.True(isMatch);
        }

        [Fact]
        public void ShouldMatchStrategyIfVisitorAddPhotoOrVideoPost()
        {
            // Arrange
            var dependencyResolverMock = new Mock<IDependencyResolver>();
            var strategy = new NewPostStrategy(dependencyResolverMock.Object);
            var fbHookChange = new FbHookChange
            {
                Field = "feed",
                Value = new FbHookChangeValue
                {
                    PostId = "123",
                    Verb = "add",
                    IsPublished = true,
                    Link = "http://www.abc.com/test.jpg"
                }
            };

            // Act
            bool isMatch = strategy.IsMatch(fbHookChange);

            // Assert
            Assert.True(isMatch);
        }

        [Fact]
        public void ShouldMatchStrategyIfWallPost()
        {
            // Arrange
            var dependencyResolverMock = new Mock<IDependencyResolver>();
            var strategy = new NewPostStrategy(dependencyResolverMock.Object);
            var fbHookChange = new FbHookChange
            {
                Field = "feed",
                Value = new FbHookChangeValue
                {
                    Item = "status",
                    PostId = "123",
                    Verb = "add",
                    IsPublished = true,
                }
            };

            // Act
            bool isMatch = strategy.IsMatch(fbHookChange);

            // Assert
            Assert.True(isMatch);
        }

        [Fact]
        public void ShouldNotMatch()
        {
            // Arrange
            var dependencyResolverMock = new Mock<IDependencyResolver>();
            var strategy = new NewPostStrategy(dependencyResolverMock.Object);
            var fbHookChange = new FbHookChange
            {
                Field = "conversation",
            };

            // Act
            bool isMatch = strategy.IsMatch(fbHookChange);

            // Assert
            Assert.False(isMatch);
        }

        [Fact]
        public async Task ShouldIgnoreWallPostIfDisabledConvertWallPostToConversation()
        {
            // Arrange
            var dependencyResolverMock = new Mock<IDependencyResolver>();
            var strategy = new NewPostStrategy(dependencyResolverMock.Object);
            var fbHookChange = new FbHookChange
            {
                Field = "feed",
                Value = new FbHookChangeValue
                {
                    Item = "status",
                    PostId = "123",
                    Verb = "add",
                    IsPublished = true,
                }
            };
            var socialAccount = new SocialAccount
            {
                IfConvertWallPostToConversation = false
            };

            // Act
            var processResult = await strategy.Process(socialAccount, fbHookChange);

            // Assert
            Assert.False(processResult.NewMessages.Any());
            Assert.False(processResult.NewConversations.Any());
            Assert.False(processResult.UpdatedConversations.Any());
        }

        [Fact]
        public async Task ShouldIgnoreVisitorPostIfDisabledConvertPostToConversation()
        {
            // Arrange
            var dependencyResolverMock = new Mock<IDependencyResolver>();
            var strategy = new NewPostStrategy(dependencyResolverMock.Object);
            var fbHookChange = new FbHookChange
            {
                Field = "feed",
                Value = new FbHookChangeValue
                {
                    Item = "post",
                    PostId = "123",
                    Verb = "add"
                }
            };
            var socialAccount = new SocialAccount
            {
                IfConvertVisitorPostToConversation = false
            };

            // Act
            var processResult = await strategy.Process(socialAccount, fbHookChange);

            // Assert
            Assert.False(processResult.NewMessages.Any());
            Assert.False(processResult.NewConversations.Any());
            Assert.False(processResult.UpdatedConversations.Any());
        }
    }
}
