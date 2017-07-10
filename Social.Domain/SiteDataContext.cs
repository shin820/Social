namespace Social.Domain
{
    using Framework.Core;
    using Framework.EntityFramework;
    using log4net;
    using Social.Domain.Entities;
    using Social.Infrastructure.Enum;
    using System.Data.Entity;

    public class SiteDataContext : DataContext
    {
        private static readonly ILog logger = LogManager.GetLogger(typeof(SiteDataContext));

        public SiteDataContext(string nameOrConnectionString, IUserContext userContext) : base(nameOrConnectionString, userContext)
        {
            //Database.SetInitializer<SiteDataContext>(null);
            Database.Log = t => logger.Debug(t);
        }

        public virtual DbSet<Conversation> Conversations { get; set; }
        public virtual DbSet<Message> Messages { get; set; }
        public virtual DbSet<SocialAccount> SocialAccounts { get; set; }
        public virtual DbSet<SocialUser> SocialUsers { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Conversation>()
                .HasMany(t => t.Messages)
                .WithRequired(t => t.Conversation)
                .HasForeignKey(t => t.ConversationId)
                .WillCascadeOnDelete(false);
            modelBuilder.Entity<Conversation>()
                .HasMany(t => t.Logs)
                .WithRequired(t => t.Conversation)
                .HasForeignKey(t => t.ConversationId)
                .WillCascadeOnDelete(false);
            modelBuilder.Entity<Conversation>()
                .HasMany(t => t.Filters)
                .WithRequired(t => t.Conversation)
                .HasForeignKey(t => t.ConversationId)
                .WillCascadeOnDelete(false);
            modelBuilder.Entity<Conversation>()
                .HasRequired(t => t.LastMessageSender)
                .WithMany(t => t.LastSendConversations)
                .HasForeignKey(t => t.LastMessageSenderId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Message>()
                .HasMany(t => t.Attachments)
                .WithRequired(t => t.Message)
                .HasForeignKey(t => t.MessageId).WillCascadeOnDelete(false);
            modelBuilder.Entity<Message>()
                .HasRequired(t => t.Sender)
                .WithMany(t => t.SendMessages)
                .HasForeignKey(t => t.SenderId).WillCascadeOnDelete(false);
            modelBuilder.Entity<Message>()
                .HasRequired(t => t.Receiver)
                .WithMany(t => t.ReceiveMessages)
                .HasForeignKey(t => t.ReceiverId).WillCascadeOnDelete(false);
            modelBuilder.Entity<Message>()
                .HasOptional(t => t.Parent)
                .WithMany(t => t.Children)
                .HasForeignKey(t => t.ParentId).WillCascadeOnDelete(false);

            modelBuilder.Entity<Filter>()
                .HasMany(t => t.Conditions)
                .WithRequired(t => t.Filter)
                .HasForeignKey(t => t.FilterId).WillCascadeOnDelete(false);
            modelBuilder.Entity<FilterCondition>()
                .HasRequired(t => t.Field)
                .WithMany(t => t.Conditions)
                .HasForeignKey(t => t.FilterId).WillCascadeOnDelete(false);

            modelBuilder.Entity<SocialUser>()
                .HasOptional(t => t.SocialAccount)
                .WithRequired(t => t.SocialUser);
        }
    }
}
