using Framework.WebApi;
using Microsoft.AspNet.SignalR;
using Social.Infrastructure.Facebook;
using Social.WebApi.Hubs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using static Social.Infrastructure.Facebook.FacebookWebHookClient;

namespace Social.WebApi.Controllers
{
    [IgnoreSiteId]
    public class FacebookWebHookController : ApiController
    {
        private Lazy<IHubContext> _hub = new Lazy<IHubContext>(() => GlobalHost.ConnectionManager.GetHubContext<FacebookHub>());

        protected IHubContext Hub
        {
            get { return _hub.Value; }
        }

        // GET api/values/5
        public int Get()
        {
            var queryStrings = Request.GetQueryNameValuePairs();
            var challenge = queryStrings.FirstOrDefault(t => t.Key == "hub.challenge").Value;
            return int.Parse(challenge);
        }

        // POST api/values
        public async Task<IHttpActionResult> Post()
        {
            try
            {
                var request = Request;
                string rawData = await request.Content.ReadAsStringAsync();
                FbHookData data = await request.Content.ReadAsAsync<FbHookData>();

                if (data == null || !data.Entry.Any())
                {
                    return Ok();
                }

                //var changes = data.Entry.First().Changes;
                //if (changes != null && changes.Any())
                //{
                //    var change = changes.FirstOrDefault();
                //    if (change != null)
                //    {
                //        if (change.Field == "feed" && change.Value.Item == "post" && change.Value.Verb == "add")
                //        {
                //            Hub.Clients.All.newFeed(change.Value.SenderName, change.Value.Message, change.Value.PostId);
                //        }
                //        if (change.Field == "feed" && change.Value.Item == "post" && change.Value.Verb == "remove")
                //        {
                //            Hub.Clients.All.removeFeed(change.Value.SenderName, change.Value.PostId);
                //        }
                //        if (change.Field == "feed" && change.Value.Item == "comment" && change.Value.Verb == "add")
                //        {
                //            Hub.Clients.All.newComment(change.Value.SenderName, change.Value.Message, change.Value.CommentId);
                //        }
                //        if (change.Field == "feed" && change.Value.Item == "comment" && change.Value.Verb == "remove")
                //        {
                //            Hub.Clients.All.removeComment(change.Value.SenderName, change.Value.CommentId);
                //        }
                //        //if (change.Field == "conversations" && change.Value.ThreadId != null)
                //        //{
                //        //    FacebookClient client = new FacebookClient();
                //        //    var message = await client.GetLastMessageOfConversation(change.Value.ThreadId, "EAAR8yzs1uVQBAKGGunoQjgZAQgm7KuEkXDL1QGLE7BCUwK6VLsjakRqcK8pdhPvS6EzKQdRKU6i9BNK3LBYDFYB4J5C3hx2yFTlElpOZAins0rWHN8rBZBGO6K7kahUdbdwf2drYSpQEA4YHeF4CwcnqsJW3BqZAZAbNV9WbLbQZDZD");
                //        //    Hub.Clients.All.newMessage(message.From.Name, message.Message);
                //        //}
                //    }
                //}

                Hub.Clients.All.newRaw(rawData);
            }
            catch (Exception ex)
            {
                Hub.Clients.All.newRaw(ex.Message);
                return Ok();
            }

            return Ok();
        }
    }
}