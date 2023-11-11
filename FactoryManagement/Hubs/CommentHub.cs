using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNet.SignalR;
using FactoryManagement.Models;
using FactoryManagement.Controllers;
using System.Threading.Tasks;

namespace FactoryManagement.Hubs
{
    public class CommentHub : Hub
    {
        private FactoryManagementEntities db = new FactoryManagementEntities();
        PerformanceController performance = new PerformanceController();
        public void SaveandSendReplyComment(UserPerformanceReview review, string groupName)
        {
           // var groupName = db.UserLogins.Where(x => x.UserId == review.UserId).Select(x => x.UserName).FirstOrDefault();
            try
            {
                var savedData = performance.SaveReplyComment(review);
                Clients.Group(groupName).ShowReplyComment(savedData.Data);
               // Clients.Caller.ShowReplyComment(savedData.Data);
            }
            catch (Exception) { }
        }
        public void SaveandSendComment(long selectedUserId, int userType, string comment, long userId, string groupName)
        {
          //  var groupName = db.UserLogins.Where(x => x.UserId == selectedUserId).Select(x => x.UserName).FirstOrDefault();
            try
            {
                var issaved = performance.UserPerReviewSave(selectedUserId, userType, comment,null, userId);
                Clients.Group(groupName).ShowComment(issaved.Data);
                //Clients.Caller.ShowComment(issaved.Data);
            }
            catch (Exception)
            {

            }
        }
        public void CommentUpdate(UserPerformanceReview review, string groupName)
        {
           // var groupName = db.UserLogins.Where(x => x.UserId == review.UserId).Select(x => x.UserName).FirstOrDefault();
            var updateComment = performance.UpdateComment(review);
            if (updateComment.Data != null) {
                Clients.Group(groupName).UpdateComment(updateComment.Data);
                //Clients.Caller.UpdateComment(updateComment.Data);
            }
    }
        public void MainCommentUpdate(UserPerformanceReview review, string groupName)
        {
            //var groupName = db.UserLogins.Where(x => x.UserId == review.UserId).Select(x => x.UserName).FirstOrDefault();
            var updateComment = performance.UpdateComment(review);
            if (updateComment.Data != null)
            {
                Clients.Group(groupName).MainUpdateComment(updateComment.Data);
                //Clients.Caller.MainUpdateComment(updateComment.Data);
            }
        }
        public void DeleteComment(int id, string groupName)
        {
           // var groupName = db.UserLogins.Where(x => x.UserId == EmpId).Select(x => x.UserName).FirstOrDefault();
            var delete = performance.deleteComment(id);
            Clients.Group(groupName).CommentDelete(delete.Data);
            //Clients.Caller.CommentDelete(delete.Data);
        }
        public void JoinGroup(string groupName)
        {
            Groups.Add(Context.ConnectionId, groupName);
        }
        //public override Task OnConnected()
        //{
        //    string name = Context.User.Identity.Name;
        //    Groups.Add(Context.ConnectionId, name);
        //    return base.OnConnected();
        //}
        public override Task OnDisconnected(bool stopCalled)
        {
            string name = Context.User.Identity.Name;
            Groups.Remove(Context.ConnectionId, name);
            return base.OnDisconnected(stopCalled);
        }      
    }
}