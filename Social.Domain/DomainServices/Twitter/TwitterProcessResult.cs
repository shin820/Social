using Social.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Social.Domain.DomainServices.Twitter
{
    public class TwitterProcessResult
    {
        public Conversation ConversationCreated { get; set; }
        public Conversation ConversationUpdated { get; set; }
        public IList<Message> MessagesCreated { get; set; }

        public TwitterProcessResult()
        {
            MessagesCreated = new List<Message>();
        }

        public void CreateConversation(Conversation conversation)
        {
            if (ConversationCreated == null)
            {
                ConversationCreated = conversation;
            }
        }

        public void UpdateConversation(Conversation conversation)
        {
            ConversationUpdated = conversation;
        }

        public void CreateMessage(Message message)
        {
            MessagesCreated.Add(message);
        }
    }
}
