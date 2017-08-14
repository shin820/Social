using Facebook;
using Framework.Core;
using Social.Domain.Entities;
using Social.Infrastructure;
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
    public interface IPullJobService : ITransient
    {
        Task PullTaggedVisitorPosts(SocialAccount account);
        Task PullVisitorPostsFromFeed(SocialAccount account);
        Task PullMessagesJob(SocialAccount account);
    }

    public class PullJobService : ServiceBase, IPullJobService
    {
        private List<FbPost> PostsToBeCreated { get; set; } = new List<FbPost>();
        private List<FbConversation> ConversationsToBeCreated { get; set; } = new List<FbConversation>();
        private List<FbMessage> MessagesToBeCreated { get; set; } = new List<FbMessage>();
        private List<FbComment> CommentsToBeCreated { get; set; } = new List<FbComment>();
        private List<FbComment> ReplyCommentsToBeCretaed { get; set; } = new List<FbComment>();
        private SocialAccount _account;

        private IConversationService _conversationService;
        private IMessageService _messageService;
        private ISocialUserService _socialUserService;
        private INotificationManager _notificationManager;

        public PullJobService(
            IConversationService conversationService,
            IMessageService messageService,
            ISocialUserService socialUserService,
            INotificationManager notificationManager
            )
        {
            _conversationService = conversationService;
            _messageService = messageService;
            _socialUserService = socialUserService;
            _notificationManager = notificationManager;
        }

        public async Task PullVisitorPostsFromFeed(SocialAccount account)
        {
            _account = account;
            var data = await FbClient.GetVisitorPosts(_account.SocialUser.OriginalId, _account.Token);
            await Init(data);
            RemoveDuplicated();
            var result = new FacebookProcessResult(_notificationManager);
            await AddPosts(result, PostsToBeCreated);
            await AddComments(result, CommentsToBeCreated);
            await AddReplyComments(result, ReplyCommentsToBeCretaed);
            Clear();
        }

        public async Task PullTaggedVisitorPosts(SocialAccount account)
        {
            if (!account.IfConvertVisitorPostToConversation)
            {
                return;
            }

            _account = account;
            var data = await FbClient.GetTaggedVisitorPosts(_account.SocialUser.OriginalId, _account.Token);
            await Init(data);
            RemoveDuplicated();
            var result = new FacebookProcessResult(_notificationManager);
            await AddPosts(result, PostsToBeCreated);
            await AddComments(result, CommentsToBeCreated);
            await AddReplyComments(result, ReplyCommentsToBeCretaed);
            Clear();
            await result.Notify(account.SiteId);
        }

        public async Task PullMessagesJob(SocialAccount account)
        {
            _account = account;
            var data = await FbClient.GetConversationsMessages(_account.SocialUser.OriginalId, _account.Token);
            await InitForConversation(data);
            RemoveDuplicated();
            await AddConversations(ConversationsToBeCreated);

            Clear();
        }

        private async Task AddPosts(FacebookProcessResult result, List<FbPost> posts)
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
                    List<SocialUser> senders = await _socialUserService.GetOrCreateSocialUsers(_account.Token, posts.Select(t => t.from).ToList());
                    foreach (var post in posts)
                    {
                        var sender = senders.FirstOrDefault(t => t.OriginalId == post.from.id);
                        if (sender == null)
                        {
                            continue;
                        }

                        var firstMessage = FacebookConverter.ConvertToMessage(_account.Token, post);
                        firstMessage.SenderId = sender.Id;
                        if (firstMessage.SenderId != _account.Id)
                        {
                            firstMessage.ReceiverId = _account.Id;
                        }

                        var conversation = new Conversation
                        {
                            OriginalId = post.id,
                            Source = ConversationSource.FacebookVisitorPost,
                            Priority = ConversationPriority.Normal,
                            Status = ConversationStatus.New,
                            Subject = GetSubject(post.message),
                            LastMessageSenderId = firstMessage.SenderId,
                            LastMessageSentTime = firstMessage.SendTime
                        };
                        conversation.Messages.Add(firstMessage);

                        if (sender.OriginalId == _account.SocialUser.OriginalId)
                        {
                            conversation.Source = ConversationSource.FacebookWallPost;
                            conversation.IsHidden = true;
                        }

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

                        if (conversation.Source == ConversationSource.FacebookWallPost && !_account.IfConvertWallPostToConversation)
                        {
                            continue;
                        }

                        if (conversation.Source == ConversationSource.FacebookVisitorPost && !_account.IfConvertVisitorPostToConversation)
                        {
                            continue;
                        }

                        await _conversationService.InsertAsync(conversation);
                        result.WithNewConversation(conversation);
                    }
                    uow.Complete();
                }
            }
        }

        private async Task AddConversations(List<FbConversation> fbConversations)
        {
            if (!fbConversations.Any())
            {
                return;
            }

            using (var uow = UnitOfWorkManager.Begin(TransactionScopeOption.RequiresNew))
            {
                using (CurrentUnitOfWork.SetSiteId(_account.SiteId))
                {
                    List<SocialUser> senders = new List<SocialUser>();
                    List<SocialUser> receivers = new List<SocialUser>();

                    foreach (var fbconversation in fbConversations)
                    {
                        var existingConversation = _conversationService.GetUnClosedConversation(fbconversation.Id);
                        if (existingConversation != null)
                        {
                            fbconversation.Messages.data = fbconversation.Messages.data.OrderBy(t => t.SendTime).ToList();
                            foreach (var fbMessage in fbconversation.Messages.data)
                            {
                                if (fbMessage.SendTime < existingConversation.LastMessageSentTime)
                                {
                                    continue;
                                }

                                var sender = await _socialUserService.GetOrCreateFacebookUser(_account.Token, fbMessage.SenderId);
                                var receiver = await _socialUserService.GetOrCreateFacebookUser(_account.Token, fbMessage.ReceiverId);
                                Message message = FacebookConverter.ConvertMessage(fbMessage, sender, receiver, _account);
                                message.ConversationId = existingConversation.Id;
                                existingConversation.IfRead = false;
                                existingConversation.Status = sender.Id != _account.SocialUser.Id ? ConversationStatus.PendingInternal : ConversationStatus.PendingExternal;
                                existingConversation.Subject = GetSubject(message.Content);
                                existingConversation.LastMessageSenderId = message.SenderId;
                                existingConversation.LastMessageSentTime = message.SendTime;
                                existingConversation.Messages.Add(message);
                                _conversationService.Update(existingConversation);
                                await CurrentUnitOfWork.SaveChangesAsync();
                            }
                        }
                        else
                        {
                            Conversation conversation = new Conversation();
                            conversation.OriginalId = fbconversation.Id;
                            conversation.Source = ConversationSource.FacebookMessage;
                            conversation.Priority = ConversationPriority.Normal;
                            conversation.Status = ConversationStatus.New;
                            fbconversation.Messages.data = fbconversation.Messages.data.OrderBy(t => t.SendTime).ToList();

                            if (fbconversation.Messages.data.All(t => t.SenderId == _account.SocialUser.OriginalId))
                            {
                                continue;
                            }

                            foreach (var fbMessage in fbconversation.Messages.data)
                            {
                                var sender = await _socialUserService.GetOrCreateFacebookUser(_account.Token, fbMessage.SenderId);
                                var receiver = await _socialUserService.GetOrCreateFacebookUser(_account.Token, fbMessage.ReceiverId);
                                Message message = FacebookConverter.ConvertMessage(fbMessage, sender, receiver, _account);

                                conversation.Subject = GetSubject(message.Content);
                                conversation.LastMessageSenderId = message.SenderId;
                                conversation.LastMessageSentTime = message.SendTime;

                                conversation.Messages.Add(message);

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
                            }
                            _conversationService.Insert(conversation);
                            await CurrentUnitOfWork.SaveChangesAsync();
                        }
                    }
                    uow.Complete();
                }
            }
        }



        private async Task AddComments(FacebookProcessResult result, List<FbComment> comments)
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
                    List<SocialUser> senders = await _socialUserService.GetOrCreateSocialUsers(_account.Token, comments.Select(t => t.from).ToList());
                    var postIds = comments.Select(t => t.PostId).Distinct().ToList();
                    var conversations = _conversationService.FindAll().Where(t => postIds.Contains(t.OriginalId)).ToList();
                    var parents = _messageService.FindAll().Where(t => postIds.Contains(t.OriginalId)).ToList();

                    foreach (var comment in comments)
                    {
                        var sender = senders.FirstOrDefault(t => t.OriginalId == comment.from.id);
                        if (sender == null)
                        {
                            continue;
                        }

                        var conversation = conversations.FirstOrDefault(t => t.OriginalId == comment.PostId);
                        if (conversation == null)
                        {
                            continue;
                        }

                        var parent = parents.FirstOrDefault(t => t.OriginalId == comment.PostId);
                        if (parent == null)
                        {
                            continue;
                        }

                        var message = FacebookConverter.ConvertToMessage(_account.Token, comment);
                        message.SenderId = sender.Id;
                        message.ParentId = parent.Id;
                        message.ReceiverId = parent.SenderId;
                        message.ConversationId = conversation.Id;
                        conversation.Messages.Add(message);
                        conversation.Status = ConversationStatus.PendingInternal;
                        conversation.LastMessageSenderId = message.SenderId;
                        conversation.LastMessageSentTime = message.SendTime;
                        conversation.TryToMakeWallPostVisible(_account);
                        result.WithUpdatedConversation(conversation);
                        result.WithNewMessage(message);
                    }

                    uow.Complete();
                }
            }
        }

        private async Task AddReplyComments(FacebookProcessResult result, List<FbComment> replyComments)
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
                    List<SocialUser> senders = await _socialUserService.GetOrCreateSocialUsers(_account.Token, replyComments.Select(t => t.from).ToList());

                    var postIds = replyComments.Select(t => t.PostId).Distinct().ToList();
                    var conversations = _conversationService.FindAll().Where(t => postIds.Contains(t.OriginalId)).ToList();

                    var parentIds = replyComments.Select(t => t.parent.id).Distinct().ToList();
                    var parents = _messageService.FindAll().Where(t => parentIds.Contains(t.OriginalId)).ToList();

                    foreach (var replyComment in replyComments)
                    {
                        var sender = senders.FirstOrDefault(t => t.OriginalId == replyComment.from.id);
                        if (sender == null)
                        {
                            continue;
                        }

                        var conversation = conversations.FirstOrDefault(t => t.OriginalId == replyComment.PostId);
                        if (conversation == null)
                        {
                            continue;
                        }

                        var parent = parents.FirstOrDefault(t => t.OriginalId == replyComment.parent.id);
                        if (parent == null)
                        {
                            continue;
                        }

                        var message = FacebookConverter.ConvertToMessage(_account.Token, replyComment);
                        message.SenderId = sender.Id;
                        message.ReceiverId = parent.SenderId;
                        message.ParentId = parent.Id;
                        message.ConversationId = conversation.Id;
                        conversation.Messages.Add(message);
                        conversation.Status = ConversationStatus.PendingInternal;
                        conversation.LastMessageSenderId = message.SenderId;
                        conversation.LastMessageSentTime = message.SendTime;
                        conversation.TryToMakeWallPostVisible(_account);
                        result.WithUpdatedConversation(conversation);
                        result.WithNewMessage(message);
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

        private async Task Init(FbPagingData<FbPost> posts)
        {
            if (!posts.data.Any())
            {
                return;
            }

            PostsToBeCreated.AddRange(posts.data);

            await AddCommentIds(posts);

            if (posts.paging.next != null)
            {
                FacebookClient client = new FacebookClient();
                var nextPagePosts = await client.GetTaskAsync<FbPagingData<FbPost>>(posts.paging.next);
                await Init(nextPagePosts);
            }
        }

        private async Task InitForConversation(FbPagingData<FbConversation> Conversations)
        {
            if (!Conversations.data.Any())
            {
                return;
            }

            List<FbMessage> FbMessages = new List<FbMessage>();
            foreach (var conversation in Conversations.data)
            {
                await InitForMessages(conversation.Messages);
                conversation.Messages.data = MessagesToBeCreated;
            }
            ConversationsToBeCreated.AddRange(Conversations.data);
            if (Conversations.paging.next != null)
            {
                FacebookClient client = new FacebookClient();
                var nextPagePosts = await client.GetTaskAsync<FbPagingData<FbConversation>>(Conversations.paging.next);
                await InitForConversation(nextPagePosts);
            }
        }

        private async Task InitForMessages(FbPagingData<FbMessage> messages)
        {
            MessagesToBeCreated = new List<FbMessage>();
            if (!messages.data.Any())
            {
                return;
            }
            MessagesToBeCreated.AddRange(messages.data);
            if (messages.paging.next != null)
            {
                FacebookClient client = new FacebookClient();
                var nextPagePosts = await client.GetTaskAsync<FbPagingData<FbMessage>>(messages.paging.next);
                await InitForMessages(nextPagePosts);
            }
        }

        private void Clear()
        {
            PostsToBeCreated.Clear();
            CommentsToBeCreated.Clear();
            ReplyCommentsToBeCretaed.Clear();
        }

        private void FillPostIdForComments(FbPagingData<FbComment> comments, string postId)
        {
            if (comments == null || comments.data == null)
            {
                return;
            }

            foreach (var comment in comments.data)
            {
                comment.PostId = postId;
            }
        }

        private async Task AddCommentIds(FbPagingData<FbPost> posts)
        {
            foreach (var post in posts.data)
            {
                var comments = post.comments;

                while (comments != null && comments.data.Any())
                {
                    FillPostIdForComments(comments, post.id);
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
                    FillPostIdForComments(replyComments, comment.PostId);
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
                var existingPostIds = _conversationService.FindAll()
                    .Where(t => (t.Source == ConversationSource.FacebookVisitorPost || t.Source == ConversationSource.FacebookWallPost) && postIds.Contains(t.OriginalId)).Select(t => t.OriginalId).ToList();
                PostsToBeCreated.RemoveAll(t => existingPostIds.Contains(t.id));
            }
            if (CommentsToBeCreated.Any())
            {
                var commentIds = CommentsToBeCreated.Select(t => t.id).ToList();
                var existingCommentIds = _messageService.FindAll().Where(t => t.Source == MessageSource.FacebookPostComment && commentIds.Contains(t.OriginalId)).Select(t => t.OriginalId).ToList();
                CommentsToBeCreated.RemoveAll(t => existingCommentIds.Contains(t.id));
            }
            if (ReplyCommentsToBeCretaed.Any())
            {
                var replyCommentIds = ReplyCommentsToBeCretaed.Select(t => t.id).ToList();
                var existingReplyCommentIds = _messageService.FindAll().Where(t => t.Source == MessageSource.FacebookPostComment && replyCommentIds.Contains(t.OriginalId)).Select(t => t.OriginalId).ToList();
                ReplyCommentsToBeCretaed.RemoveAll(t => existingReplyCommentIds.Contains(t.id));
            }
            if (ConversationsToBeCreated.Any())
            {
                var ConversationIds = ConversationsToBeCreated.Select(t => t.Id).ToList();
                List<string> MessageIds = new List<string>();
                foreach (var conversation in ConversationsToBeCreated)
                {
                    MessageIds.AddRange(conversation.Messages.data.Select(m => m.Id).ToList());
                }
                var existingMessageIds = _messageService.FindAll().Where(t => t.Source == MessageSource.FacebookMessage && MessageIds.Contains(t.OriginalId)).Select(t => t.OriginalId).ToList();

                foreach (var conversation in ConversationsToBeCreated)
                {
                    conversation.Messages.data.RemoveAll(t => existingMessageIds.Contains(t.Id));
                }
            }
        }
    }
}
