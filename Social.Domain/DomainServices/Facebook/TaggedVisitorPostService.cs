using Facebook;
using Framework.Core;
using Social.Domain.Entities;
using Social.Infrastructure.Enum;
using Social.Infrastructure.Facebook;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;

namespace Social.Domain.DomainServices.Facebook
{
    public interface ITaggedVisitorPostService : ITransient
    {
        Task Process(SocialAccount account);
    }

    public class TaggedVisitorPostService : ServiceBase, ITaggedVisitorPostService
    {
        private List<FbPost> PostsToBeCreated { get; set; } = new List<FbPost>();
        private List<FbComment> CommentsToBeCreated { get; set; } = new List<FbComment>();
        private List<FbComment> ReplyCommentsToBeCretaed { get; set; } = new List<FbComment>();

        private SocialAccount _account;

        private IRepository<Conversation> _conersationRepo;
        private IRepository<Message> _messageRepo;
        private IRepository<SocialUser> _socialUserRepo;
        private int _siteId;

        public TaggedVisitorPostService(
            IRepository<Conversation> conersationRepo,
            IRepository<Message> messageRepo,
            IRepository<SocialUser> socialUserRepo
            )
        {
            _conersationRepo = conersationRepo;
            _messageRepo = messageRepo;
            _socialUserRepo = socialUserRepo;
        }

        public async Task Process(SocialAccount account)
        {
            _account = account;
            var data = await FbClient.GetTaggedVisitorPosts(_account.SocialUser.SocialId, _account.Token);
            await Init(data);
            RemoveDuplicated();
            await AddPosts(PostsToBeCreated);
            await AddComments(CommentsToBeCreated);
            await AddReplyComments(ReplyCommentsToBeCretaed);
        }

        private async Task AddPosts(List<FbPost> posts)
        {
            if (!posts.Any())
            {
                return;
            }

            posts = posts.OrderBy(t => t.created_time).ToList();

            using (var uow = UnitOfWorkManager.Begin(TransactionScopeOption.RequiresNew))
            {
                using (CurrentUnitOfWork.SetSiteId(_account.SiteId))
                {
                    List<SocialUser> senders = await GetOrCreateSocialUsers(_account.Token, posts.Select(t => t.from).ToList());
                    List<Conversation> conversations = new List<Conversation>();
                    foreach (var post in posts)
                    {
                        var sender = senders.FirstOrDefault(t => t.SocialId == post.from.id);
                        if (sender == null)
                        {
                            continue;
                        }

                        var firstMessage = FacebookConverter.ConvertToMessage(_account.Token, post);
                        firstMessage.SenderId = sender.Id;
                        var conversation = new Conversation
                        {
                            SocialId = post.id,
                            Source = ConversationSource.FacebookVisitorPost,
                            Priority = ConversationPriority.Normal,
                            Status = ConversationStatus.New,
                            Subject = GetSubject(post.message),
                            LastMessageSenderId = firstMessage.SenderId,
                            LastMessageSentTime = firstMessage.SendTime
                        };
                        conversation.Messages.Add(firstMessage);

                        if (_account.ConversationAgentId.HasValue && _account.ConversationPriority.HasValue)
                        {
                            conversation.AgentId = _account.ConversationAgentId.Value;
                            conversation.Priority = _account.ConversationPriority.Value;
                        }

                        if (_account.ConversationDepartmentId.HasValue && _account.ConversationPriority.HasValue)
                        {
                            conversation.DepartmentId = _account.ConversationDepartmentId.Value;
                            conversation.Priority = _account.ConversationPriority.Value;
                        }

                        conversations.Add(conversation);
                    }
                    await _conersationRepo.InsertManyAsync(conversations.ToArray());
                    uow.Complete();
                }
            }
        }

        private async Task AddComments(List<FbComment> comments)
        {
            if (!comments.Any())
            {
                return;
            }

            comments = comments.OrderBy(t => t.created_time).ToList();
            using (var uow = UnitOfWorkManager.Begin(TransactionScopeOption.RequiresNew))
            {
                using (CurrentUnitOfWork.SetSiteId(_account.SiteId))
                {
                    List<SocialUser> senders = await GetOrCreateSocialUsers(_account.Token, comments.Select(t => t.from).ToList());
                    var postIds = comments.Select(t => t.PostId).Distinct().ToList();
                    var conversations = _conersationRepo.FindAll().Where(t => postIds.Contains(t.SocialId)).ToList();
                    var parents = _messageRepo.FindAll().Where(t => postIds.Contains(t.SocialId)).ToList();

                    foreach (var comment in comments)
                    {
                        var sender = senders.FirstOrDefault(t => t.SocialId == comment.from.id);
                        if (sender == null)
                        {
                            continue;
                        }

                        var conversation = conversations.FirstOrDefault(t => t.SocialId == comment.PostId);
                        if (conversation == null)
                        {
                            continue;
                        }

                        var parent = parents.FirstOrDefault(t => t.SocialId == comment.PostId);
                        if (parent == null)
                        {
                            continue;
                        }

                        var message = FacebookConverter.ConvertToMessage(_account.Token, comment);
                        message.SenderId = sender.Id;
                        message.ParentId = parent.Id;
                        conversation.Messages.Add(message);
                        conversation.Status = ConversationStatus.PendingInternal;
                        conversation.LastMessageSenderId = message.SenderId;
                        conversation.LastMessageSentTime = message.SendTime;
                    }

                    uow.Complete();
                }
            }
        }

        private async Task AddReplyComments(List<FbComment> replyComments)
        {
            if (!replyComments.Any())
            {
                return;
            }

            replyComments = replyComments.OrderBy(t => t.created_time).ToList();
            using (var uow = UnitOfWorkManager.Begin(TransactionScopeOption.RequiresNew))
            {
                using (CurrentUnitOfWork.SetSiteId(_account.SiteId))
                {
                    List<SocialUser> senders = await GetOrCreateSocialUsers(_account.Token, replyComments.Select(t => t.from).ToList());

                    var postIds = replyComments.Select(t => t.PostId).Distinct().ToList();
                    var conversations = _conersationRepo.FindAll().Where(t => postIds.Contains(t.SocialId)).ToList();

                    var parentIds = replyComments.Select(t => t.parent.id).Distinct().ToList();
                    var parents = _messageRepo.FindAll().Where(t => parentIds.Contains(t.SocialId)).ToList();

                    foreach (var replyComment in replyComments)
                    {
                        var sender = senders.FirstOrDefault(t => t.SocialId == replyComment.from.id);
                        if (sender == null)
                        {
                            continue;
                        }

                        var conversation = conversations.FirstOrDefault(t => t.SocialId == replyComment.PostId);
                        if (conversation == null)
                        {
                            continue;
                        }

                        var parent = parents.FirstOrDefault(t => t.SocialId == replyComment.parent.id);
                        if (parent == null)
                        {
                            continue;
                        }

                        var message = FacebookConverter.ConvertToMessage(_account.Token, replyComment);
                        message.SenderId = sender.Id;
                        message.ParentId = parent.Id;
                        conversation.Messages.Add(message);
                        conversation.Status = ConversationStatus.PendingInternal;
                        conversation.LastMessageSenderId = message.SenderId;
                        conversation.LastMessageSentTime = message.SendTime;
                    }
                    uow.Complete();
                }
            }
        }

        protected string GetSubject(string message)
        {
            if (string.IsNullOrWhiteSpace(message))
            {
                return "No Subject";
            }

            return message.Length <= 200 ? message : message.Substring(200);
        }

        private async Task<List<SocialUser>> GetOrCreateSocialUsers(string token, List<FbUser> fbSenders)
        {
            List<SocialUser> senders = new List<SocialUser>();
            var fbSenderIds = fbSenders.Select(t => t.id).ToList();
            var existingUsers = _socialUserRepo.FindAll().Where(t => fbSenderIds.Contains(t.SocialId)).ToList();
            senders.AddRange(existingUsers);
            fbSenders.RemoveAll(t => existingUsers.Any(e => e.SocialId == t.id));
            foreach (var fbSender in fbSenders)
            {
                var sender = new SocialUser()
                {
                    SocialId = fbSender.id,
                    Name = fbSender.name,
                    Email = fbSender.email
                };

                await _socialUserRepo.InsertAsync(sender);
                senders.Add(sender);
            }
            return senders;
        }

        private async Task Init(FbPagingData<FbPost> posts)
        {
            if (!posts.data.Any())
            {
                return;
            }

            FillPostIdForComments(posts);

            PostsToBeCreated.AddRange(posts.data);

            await AddCommentIds(posts);

            if (posts.paging.next != null)
            {
                FacebookClient client = new FacebookClient();
                var nextPagePosts = await client.GetTaskAsync<FbPagingData<FbPost>>(posts.paging.next);
                await Init(nextPagePosts);
            }
        }

        private void FillPostIdForComments(FbPagingData<FbPost> posts)
        {
            foreach (var post in posts.data)
            {
                foreach (var comment in post.comments.data)
                {
                    comment.PostId = post.id;

                    if (comment.comments != null)
                    {
                        foreach (var replyComment in comment.comments.data)
                        {
                            replyComment.PostId = post.id;
                        }
                    }
                }
            }
        }

        private async Task AddCommentIds(FbPagingData<FbPost> posts)
        {
            foreach (var post in posts.data)
            {
                var comments = post.comments;

                while (comments != null && comments.data.Any())
                {
                    CommentsToBeCreated.AddRange(comments.data);

                    await AddReplyCommentIds(comments);

                    if (comments.paging.next != null)
                    {
                        FacebookClient client = new FacebookClient();
                        comments = await client.GetTaskAsync<FbPagingData<FbComment>>(comments.paging.next);
                    }
                    else
                    {
                        break;
                    }
                }
            }
        }

        private async Task AddReplyCommentIds(FbPagingData<FbComment> comments)
        {
            foreach (var comment in comments.data)
            {
                var replyComments = comment.comments;
                while (replyComments != null && replyComments.data.Any())
                {
                    ReplyCommentsToBeCretaed.AddRange(replyComments.data);

                    if (replyComments.paging.next != null)
                    {
                        FacebookClient client = new FacebookClient();
                        replyComments = await client.GetTaskAsync<FbPagingData<FbComment>>(replyComments.paging.next);
                    }
                    else
                    {
                        break;
                    }
                }
            }
        }

        private void RemoveDuplicated()
        {
            if (PostsToBeCreated.Any())
            {
                var postIds = PostsToBeCreated.Select(t => t.id).ToList();
                var existingPostIds = _conersationRepo.FindAll().Where(t => postIds.Contains(t.SocialId)).Select(t => t.SocialId).ToList();
                PostsToBeCreated.RemoveAll(t => existingPostIds.Contains(t.id));
            }
            if (CommentsToBeCreated.Any())
            {
                var commentIds = CommentsToBeCreated.Select(t => t.id).ToList();
                var existingCommentIds = _messageRepo.FindAll().Where(t => commentIds.Contains(t.SocialId)).Select(t => t.SocialId).ToList();
                CommentsToBeCreated.RemoveAll(t => existingCommentIds.Contains(t.id));
            }
            if (ReplyCommentsToBeCretaed.Any())
            {
                var replyCommentIds = ReplyCommentsToBeCretaed.Select(t => t.id).ToList();
                var existingReplyCommentIds = _messageRepo.FindAll().Where(t => replyCommentIds.Contains(t.SocialId)).Select(t => t.SocialId).ToList();
                ReplyCommentsToBeCretaed.RemoveAll(t => existingReplyCommentIds.Contains(t.id));
            }
        }
    }
}
