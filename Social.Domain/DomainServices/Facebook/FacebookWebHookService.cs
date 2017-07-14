using Facebook;
using Framework.Core;
using Social.Domain.DomainServices.Facebook;
using Social.Domain.Entities;
using Social.Infrastructure.Enum;
using Social.Infrastructure.Facebook;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Social.Domain.DomainServices
{
    public interface IFacebookService : ITransient
    {
        Task ProcessWebHookData(FbHookData fbData);
        Task SyncTaggedVisitorPost(int siteId);
    }

    public class FacebookService : ServiceBase, IFacebookService
    {
        private IRepository<SocialAccount> _socialAccountRepo;
        private IRepository<SocialUser> _socialUserRepo;
        private IRepository<Conversation> _conversationRepo;
        private IRepository<Message> _messageRepo;
        private IConversationStrategyFactory _strategyFactory;

        public FacebookService(
            IRepository<SocialAccount> socialAccountRepo,
            IRepository<SocialUser> socialUserRepo,
            IRepository<Conversation> conversationRepo,
            IRepository<Message> messageRepo,
            IConversationStrategyFactory strategyFactory
            )
        {
            _socialAccountRepo = socialAccountRepo;
            _socialUserRepo = socialUserRepo;
            _conversationRepo = conversationRepo;
            _messageRepo = messageRepo;
            _strategyFactory = strategyFactory;
        }

        public async Task ProcessWebHookData(FbHookData fbData)
        {
            if (fbData == null || !fbData.Entry.Any())
            {
                return;
            }

            var changes = fbData.Entry.First().Changes;
            if (changes == null || !changes.Any())
            {
                return;
            }

            if (fbData.Object != "page")
            {
                return;
            }

            string pageId = fbData.Entry.First().Id;
            var socialAccount = _socialAccountRepo.FindAll().FirstOrDefault(t => t.SocialUser.SocialId == pageId);
            foreach (var change in changes)
            {
                if (socialAccount != null)
                {
                    var strategory = _strategyFactory.Create(change);
                    if (strategory != null)
                    {
                        await strategory.Process(socialAccount, change);
                    }
                }
            }
        }

        public async Task SyncTaggedVisitorPost(int siteId)
        {
            var socialAcctouns = _socialAccountRepo.FindAll().Where(t => t.SocialUser.Type == SocialUserType.Facebook && t.IfEnable == true).ToList();
            foreach (var socialAcccount in socialAcctouns)
            {
                await SyncTaggedVisitorPost(socialAcccount);
            }
        }

        //private Func<int, string, FbComment, Task> GetSyncPostCommentFunc()
        //{

        //}


        private Func<int, string, FbPost, Task> GetSyncPostFunc()
        {
            return async (siteId, token, post) =>
            {
                using (var uow = UnitOfWorkManager.Begin(System.Transactions.TransactionScopeOption.RequiresNew))
                {
                    using (CurrentUnitOfWork.SetSiteId(siteId))
                    {
                        var existingConversation = _conversationRepo.FindAll().FirstOrDefault(t => t.SocialId == post.id);
                        if (existingConversation == null)
                        {
                            existingConversation = new Conversation
                            {
                                SocialId = post.id,
                                Source = ConversationSource.FacebookVisitorPost,
                                Priority = ConversationPriority.Normal,
                                Status = ConversationStatus.New,
                                Subject = GetSubject(post.message)
                                //LastMessageSenderId = sender.Id,
                                //LastMessageSentTime = post.created_time
                            };
                            var firstMessage = await GetMessageFromPost(token, post);
                            existingConversation.Messages.Add(firstMessage);

                            var commentMessages = await GetMessagesFromComments(token, post.comments);
                            foreach (var commentMessage in commentMessages)
                            {
                                if (commentMessage.Parent == null)
                                {
                                    commentMessage.Parent = firstMessage;
                                }
                                existingConversation.Messages.Add(commentMessage);
                            }
                            existingConversation.LastMessageSenderId = existingConversation.Messages.Last().SenderId;
                            existingConversation.LastMessageSentTime = existingConversation.Messages.Last().SendTime;
                            await _conversationRepo.InsertAsync(existingConversation);
                        }
                        else
                        {
                            if (existingConversation.LastMessageSentTime == post.updated_time)
                            {
                                return;
                            }

                            foreach (var comment in post.comments.data)
                            {
                                if (_messageRepo.FindAll().Any(t => t.SocialId == comment.id))
                                {
                                    if (comment.comment_count == 0)
                                    {
                                        continue;
                                    }
                                    foreach (var replyComment in comment.comments.data)
                                    {
                                        if (_messageRepo.FindAll().Any(t => t.SocialId == replyComment.id))
                                        {
                                            continue;
                                        }
                                        else
                                        {
                                            Message message = await GetMessageFromComment(token, replyComment);
                                            existingConversation.Messages.Add(message);
                                        }
                                    }
                                }
                                else
                                {
                                    Message message = await GetMessageFromComment(token, comment);
                                    existingConversation.Messages.Add(message);
                                }
                            }
                            existingConversation.LastMessageSenderId = existingConversation.Messages.Last().SenderId;
                            existingConversation.LastMessageSentTime = existingConversation.Messages.Last().SendTime;

                            await _conversationRepo.UpdateAsync(existingConversation);
                        }
                        uow.Complete();
                    }
                }
            };
        }
        private async Task SyncTaggedVisitorPost(SocialAccount socialAccount)
        {
            var posts = await FbClient.GetTaggedVisitorPosts(socialAccount.SocialUser.SocialId, socialAccount.Token);

            Func<int, string, FbPost, Task> action = GetSyncPostFunc();

            await FbClient.Process(socialAccount.SiteId, socialAccount.Token, posts, action);

            foreach (var post in posts.data)
            {
                using (var uow = UnitOfWorkManager.Begin(System.Transactions.TransactionScopeOption.RequiresNew))
                {
                    using (CurrentUnitOfWork.SetSiteId(socialAccount.SiteId))
                    {
                        var existingConversation = _conversationRepo.FindAll().FirstOrDefault(t => t.SocialId == post.id);
                        if (existingConversation == null)
                        {
                            existingConversation = new Conversation
                            {
                                SocialId = post.id,
                                Source = ConversationSource.FacebookVisitorPost,
                                Priority = ConversationPriority.Normal,
                                Status = ConversationStatus.New,
                                Subject = GetSubject(post.message)
                                //LastMessageSenderId = sender.Id,
                                //LastMessageSentTime = post.created_time
                            };
                            var firstMessage = await GetMessageFromPost(socialAccount.Token, post);
                            existingConversation.Messages.Add(firstMessage);
                            var commentMessages = await GetMessagesFromComments(socialAccount.Token, post.comments);
                            foreach (var commentMessage in commentMessages)
                            {
                                if (commentMessage.Parent == null)
                                {
                                    commentMessage.Parent = firstMessage;
                                }
                                existingConversation.Messages.Add(commentMessage);
                            }
                            existingConversation.LastMessageSenderId = existingConversation.Messages.Last().SenderId;
                            existingConversation.LastMessageSentTime = existingConversation.Messages.Last().SendTime;
                            await _conversationRepo.InsertAsync(existingConversation);
                        }
                        else
                        {
                            if (existingConversation.LastMessageSentTime == post.updated_time)
                            {
                                continue;
                            }

                            foreach (var comment in post.comments.data)
                            {
                                if (_messageRepo.FindAll().Any(t => t.SocialId == comment.id))
                                {
                                    if (comment.comment_count == 0)
                                    {
                                        continue;
                                    }
                                    foreach (var replyComment in comment.comments.data)
                                    {
                                        if (_messageRepo.FindAll().Any(t => t.SocialId == replyComment.id))
                                        {
                                            continue;
                                        }
                                        else
                                        {
                                            Message message = await GetMessageFromComment(socialAccount.Token, replyComment);
                                            existingConversation.Messages.Add(message);
                                        }
                                    }
                                }
                                else
                                {
                                    Message message = await GetMessageFromComment(socialAccount.Token, comment);
                                    existingConversation.Messages.Add(message);
                                }
                            }
                            existingConversation.LastMessageSenderId = existingConversation.Messages.Last().SenderId;
                            existingConversation.LastMessageSentTime = existingConversation.Messages.Last().SendTime;

                            await _conversationRepo.UpdateAsync(existingConversation);
                        }
                        uow.Complete();
                    }
                }
            }
        }

        private async Task<List<Message>> GetMessagesFromComments(string token, FbPagingData<FbComment> comments)
        {
            List<Message> messages = new List<Message>();

            foreach (var comment in comments.data)
            {
                Message commentMessage = await GetMessageFromComment(token, comment);
                messages.Add(commentMessage);

                if (comment.comment_count > 0)
                {
                    foreach (var replyComment in comment.comments.data)
                    {
                        SocialUser sender = await GetOrCreateSocialUser(token, comment.from.id);
                        Message replyCommentMessage = await GetMessageFromComment(token, replyComment);
                        replyCommentMessage.Parent = commentMessage;
                        messages.Add(replyCommentMessage);
                    }
                }
            }

            return messages;
        }

        private async Task<Message> GetMessageFromComment(string token, FbComment comment)
        {
            SocialUser sender = await GetOrCreateSocialUser(token, comment.from.id);
            Message message = new Message
            {
                SenderId = sender.Id,
                Source = MessageSource.FacebookPostComment,
                SocialId = comment.id,
                SendTime = comment.created_time,
                Content = comment.message,
                SocialLink = comment.permalink_url,
            };

            if (comment.attachment != null && comment.attachment.media != null && comment.attachment.media.image != null)
            {
                message.Attachments.Add(new MessageAttachment
                {
                    Url = comment.attachment.media.image.src,
                    MimeType = new Uri(comment.attachment.media.image.src).GetMimeType()
                });
            }

            return message;
        }

        private async Task<Message> GetMessageFromPost(string token, FbPost post)
        {
            SocialUser sender = await GetOrCreateSocialUser(token, post.from.id);
            var message = new Message
            {
                SenderId = sender.Id,
                Source = MessageSource.FacebookPost,
                SocialId = post.id,
                SendTime = post.created_time,
                Content = post.message,
                SocialLink = post.permalink_url,
                Story = post.story
            };

            if (!string.IsNullOrEmpty(post.link))
            {
                message.Attachments.Add(new MessageAttachment
                {
                    Url = post.link,
                    MimeType = new Uri(post.link).GetMimeType(),
                });
            }

            return message;
        }

        private async Task<SocialUser> GetOrCreateSocialUser(string token, string fbUserId)
        {
            var user = _socialUserRepo.FindAll().Where(t => t.SocialId == fbUserId).FirstOrDefault();
            if (user == null)
            {
                FbUser fbUser = await FbClient.GetUserInfo(token, fbUserId);
                user = new SocialUser
                {
                    SocialId = fbUser.id,
                    Name = fbUser.name,
                    Email = fbUser.email
                };
                await _socialUserRepo.InsertAsync(user);
            }
            return user;
        }

        protected string GetSubject(string message)
        {
            if (string.IsNullOrWhiteSpace(message))
            {
                return "No Subject";
            }

            return message.Length <= 200 ? message : message.Substring(200);
        }
    }
}
