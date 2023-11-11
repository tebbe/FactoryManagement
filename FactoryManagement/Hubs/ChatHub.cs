using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNet.SignalR;
using FactoryManagement.Models;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR.Hubs;
using System.Data.Entity;


namespace FactoryManagement.Hubs
{
    public class UserData
    {
        public string UserName { get; set; }
        public string Picture { get; set; }
        public long UserId { get; set; }
        public string Designation { get; set; }
    }
    public class ChatHub : Hub
    {
        private FactoryManagementEntities db = new FactoryManagementEntities();
        public static List<string> OnlineUsers = new List<string>();
        public void SendChatMessage(ChatHistory history)
        {
            string name = db.UserLogins.Where(x => x.UserId == history.ReceiverId).Select(x => x.UserName).FirstOrDefault();//Context.User.Identity.Name;            
            try
            {
                history.SendDate = DateTime.Now;
                db.ChatHistories.Add(history);
                db.SaveChanges();
                var data = new
                {
                    history = db.View_Chat_History.Where(x => x.ConversationId == history.ConversationId).FirstOrDefault(),
                    senderPic = db.UserInformations.Where(x => x.UserId == history.SenderId).Select(x => x.Picture).FirstOrDefault(),
                };
                Clients.Group(name).addChatMessage(data);
                Clients.Caller.mysentMessage(data);
            }
            catch (Exception)
            {
            }
        }
        public void GetOnlineUsers()
        {
            List<View_UserLists> data = new List<View_UserLists>();
            if (OnlineUsers.Count() > 0)
            {
                var AllOnlineUserId = db.UserLogins.Where(m => OnlineUsers.Contains(m.UserName)).Select(m => m.UserId).ToArray();
                data = db.View_UserLists.Where(m => AllOnlineUserId.Contains(m.UserId)).ToList();
            }
            Clients.All.OnlineUser(data);
        }
        public override Task OnConnected()
        {
            string name = Context.User.Identity.Name;
            Groups.Add(Context.ConnectionId, name);
            var user = db.View_UserLists.Where(c => c.UserId == db.UserLogins.Where(x => x.UserName == name).Select(x => x.UserId).FirstOrDefault()).Select(c => new
            {
                UserName = c.FirstName + " " + c.MiddleName + " " + c.LastName,
                Picture = c.Picture,
                Designation = c.DesignationName,
                UserId = c.UserId
            }).FirstOrDefault();
            Clients.Others.Online(user);
            OnlineUsers.Add(name);
            return base.OnConnected();
        }
        public void ClinetOffline()
        {
            string name = Context.User.Identity.Name;
            Groups.Remove(Context.ConnectionId, name);
            var user = db.View_UserLists.Where(c => c.UserId == db.UserLogins.Where(x => x.UserName == name).Select(x => x.UserId).FirstOrDefault()).Select(c => new
            {
                UserName = c.FirstName + " " + c.MiddleName + " " + c.LastName,
                Picture = c.Picture,
                Designation = c.DesignationName,
                UserId = c.UserId
            }).FirstOrDefault();
            Clients.All.Ofline(user);
            OnlineUsers.Remove(name);

        }
        public void MessageSeen(int recieverId, int senderId)
        {
            try
            {
                var data = db.ChatHistories.Where(x => x.ReceiverId == recieverId && x.SenderId == senderId && (x.Seen == false || x.Seen == null)).ToList();
                foreach (var unseen in data)
                {
                    unseen.Seen = true;
                    unseen.SeenTime = DateTime.Now;
                    db.Entry(unseen).State = EntityState.Modified;
                    db.SaveChanges();
                }
                string name = db.UserLogins.Where(x => x.UserId == senderId).Select(x => x.UserName).FirstOrDefault();

                if (Clients.Group(name) != null)
                {
                    if (data.Count > 0)
                    {
                        var modifiedData = data.Select(x => new
                        {
                            ConversationId = x.ConversationId,
                            SenderId = x.SenderId,
                            RecieverId = x.ReceiverId,
                            Chat = x.Chat,
                            Seen = x.Seen,
                            SeenTime = DateTime.Now.ToString("MMM dd,yyyy hh:mm tt")
                        }).ToList();
                        Clients.Group(name).ChangeToSeen(modifiedData);
                    }

                }
            }
            catch (Exception) { }
        }
        public void GetUnseenCounter()
        {
            var userId = db.UserLogins.Where(x => x.UserName == Context.User.Identity.Name).Select(x => x.UserId).FirstOrDefault();
            Clients.Caller.totalUnseenMessage(db.ChatHistories.Where(x => x.ReceiverId == userId && (x.Seen == false || x.Seen == null)).Count());
        }
        public override Task OnDisconnected(bool stopCalled)
        {
            string name = Context.User.Identity.Name;
            Groups.Remove(Context.ConnectionId, name);
            OnlineUsers.Remove(name);
            return base.OnDisconnected(stopCalled);
        }
    }
}