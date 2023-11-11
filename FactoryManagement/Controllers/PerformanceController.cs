
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data;
using System.Data.Entity;
using System.Net;
using FactoryManagement.Models;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using System.Xml;
using Newtonsoft.Json;
using System.Collections;
using System.Data.SqlClient;
using System.IO;
using System.Web.Script.Serialization;
using FactoryManagement.ModelView;
using System.Web.Helpers;
using FactoryManagement.ModelView.SalaryConfig;
using FactoryManagement.ModelView.HR;
using FactoryManagement.ModelView.Accounting;
using FactoryManagement.Infrastructure.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using System.Configuration;


namespace FactoryManagement.Controllers
{
    public class PerformanceController : Controller
    {
        private FactoryManagementEntities db = new FactoryManagementEntities();
        static private int offset = Convert.ToInt32(ConfigurationManager.AppSettings["localTime"]);
        DateTime now = DateTime.UtcNow.AddMinutes(offset);
        public static int ResourcesPerPage = 5;
        public static long UserId ;
        public static string LoginUserName = "";
        public static string LoginUserPicture = "";
        public static int InstitutionId = 1;
        const int CommentsPerPage = 5;
        #region
        public PartialViewResult _performancePartial()
        {
            return PartialView();
        }
        public void LoginUserNameAndPic()
        {

            HttpCookie cookie = HttpContext.Request.Cookies.Get("CookieAdminInfo");
            string userid = cookie.Values["userid"].ToString();
            string userpic = cookie.Values["userpic"].ToString(); 
            UserId = Convert.ToInt64(userid);
            UserInformationModelView model = new UserInformationModelView();
            model = (from m in db.View_UserLists
                     where m.UserId == UserId
                     select new UserInformationModelView
                     {
                         Picture = m.Picture,
                         UserName = m.UserName
                     }).FirstOrDefault();
            LoginUserName = model.UserName;
            LoginUserPicture = model.Picture;
        }
        public ActionResult Performance()
        {
            return View();
        }
        public PartialViewResult _SelectedUseInfo(int? userId,int userType)
        {
            return PartialView(db.View_UserLists.Where(m => m.UserId == userId && m.UserType == userType).FirstOrDefault());
        }
        public PartialViewResult _SelectedUserReview(int? userId, int loginUserId, int? filtered)
        {
            if (userId == loginUserId) {
                ViewBag.canComment = false;
            }
            else if (userId != loginUserId) {
                ViewBag.canComment = true;
            }
            if (filtered == 0 || filtered == null)
            {
                var list = db.GetUserPerformanceReview(userId, loginUserId).OrderByDescending(x => x.Id).ToList();
                return PartialView(list);
            }
            else
            {
                var list = db.GetUserPerformanceReview(userId, loginUserId).Where(x => x.ReviewBy == loginUserId).OrderByDescending(x => x.Id).ToList();
                return PartialView(list);
            }                 
        }      
        public void LoadAllCommentsToSession(long? selectedUserId, string commentOrder)
        {
            if (commentOrder == null)
            {
                commentOrder = "1"; //Top Comments
            }
            var list = db.GetUserPerformanceReview(selectedUserId, UserId).ToList();
            if (commentOrder == "1")
            {//top reviews
                var libraryComment = list.Where(m => m.HasParent == false).OrderByDescending(m => m.Likes).ThenByDescending(m => m.Reply).ThenByDescending(m => m.ReviewDate).ToList();
                int cmnIndex = 1;
                Session["Comments"] = libraryComment.ToDictionary(m => cmnIndex++, m => m);
                Session["totalComment"] = libraryComment.Count();
            }
            else if (commentOrder == "2")
            {//newest first
                var libraryComment = list.Where(m => m.HasParent == false)
                                            .OrderByDescending(m => m.ReviewDate).ToList();
                int cmnIndex = 1;
                Session["Comments"] = libraryComment.ToDictionary(m => cmnIndex++, m => m);
                Session["totalComment"] = libraryComment.Count();
            }
            else if (commentOrder == "3")
            {//oldest first
                var libraryComment = list.Where(m => m.HasParent == false)
                                            .OrderBy(m => m.ReviewDate).ToList();
                int cmnIndex = 1;
                Session["Comments"] = libraryComment.ToDictionary(m => cmnIndex++, m => m);
                Session["totalComment"] = libraryComment.Count();
            }
            else
            {//my comments
                var libraryComment = list.Where(m => m.UserId == UserId && m.HasParent == false)
                                            .OrderByDescending(m => m.ReviewDate).ToList();
                int cmnIndex = 1;
                Session["Comments"] = libraryComment.ToDictionary(m => cmnIndex++, m => m);
                Session["totalComment"] = libraryComment.Count();
            }
        }
        public ActionResult _PartialUserRatingLoad(long selectedUserId, long userId)
        {
            ViewBag.selectedUserId = selectedUserId;
            ViewBag.Rating = Convert.ToDouble(db.User_Rating_List.Where(m => m.UserId == selectedUserId && m.RatedBy == userId).Select(m => m.Rate).FirstOrDefault());
            return PartialView();
        }
        public JsonResult RateUserSubmit(long selectedUserId, long userId, string rating)
        {
            User_Rating_List rate = new User_Rating_List();
            if (db.User_Rating_List.Where(m => m.RatedBy == userId && m.UserId == selectedUserId).Any())
            {
                rate = db.User_Rating_List.Where(m => m.RatedBy == userId && m.UserId == selectedUserId).Select(m => m).FirstOrDefault();
                rate.Rate = Convert.ToDecimal(rating);
                db.User_Rating_List.Attach(rate);
                db.Entry(rate).State = EntityState.Modified;
                db.SaveChanges();
            }
            else
            {
                rate.UserId = selectedUserId;
                rate.RatedBy = userId;
                rate.Rate = Convert.ToDecimal(rating);
                rate.RatingDate = now;

                db.User_Rating_List.Add(rate);
                db.SaveChanges();
            }
            return Json(true, JsonRequestBehavior.AllowGet);
        }
        public JsonResult UserPerReviewSave(long? selectedUserId,int userType, string comment, int? parentId, long userId)
        {
            long value;
            if (selectedUserId != null && comment != String.Empty)
            {
                UserPerformanceReview model = new UserPerformanceReview();
                model.UserId = Convert.ToInt64(selectedUserId);
                model.UserType = userType;
                model.Review = comment;
                model.ReviewBy = userId;
                model.ReviewDate = now;
                model.IsUpdated = false;
                if (parentId != null)
                {
                    model.ParentId = parentId;
                    model.HasParent = true;
                }
                else
                {
                    model.HasParent = false;
                }
                db.UserPerformanceReviews.Add(model);
                db.SaveChanges();
                long reviewId = db.UserPerformanceReviews.Where(m => m.UserId == selectedUserId && m.ReviewBy == userId).Max(m => m.Id);
                value = reviewId;
            }
            else
            {
                value = 0;
            }
            var data = new { value = value, comment = comment, user = db.View_UserLists.Where(x => x.UserId == userId).Select(x => new {
            userName=x.UserName,
            pic=x.Picture,
            reviewBy=x.UserId
            }).FirstOrDefault() };
            return this.Json(data, JsonRequestBehavior.AllowGet);
        }
        #endregion
        #region Added By Kaikubad 02-02-2017
        public JsonResult SaveReplyComment(UserPerformanceReview performanceReview) {
            performanceReview.HasParent = true;
            performanceReview.ReviewDate = now;
            db.UserPerformanceReviews.Add(performanceReview);
            db.SaveChanges();
            var data = new
            {
                performanceReviewId = performanceReview.Id,
                comment = performanceReview.Review,
                reviewBy=performanceReview.ReviewBy,
                parentId=performanceReview.ParentId,
                user = db.View_UserLists.Where(x => x.UserId == performanceReview.ReviewBy).Select(x => new
                {
                    userName = x.UserName,
                    pic = x.Picture                    
                }).FirstOrDefault()
            };            
            return Json(data, JsonRequestBehavior.AllowGet);
        }
        public JsonResult AddRemoveDislike(EmpPerFormanceReview_LikeUnLike performanceReview) {
            var reviewData = db.EmpPerFormanceReview_LikeUnLike.Where(x => x.UserReviewId == performanceReview.UserReviewId && x.UserId == performanceReview.UserId).FirstOrDefault();
            EmpPerFormanceReview_LikeUnLike performancelike = new EmpPerFormanceReview_LikeUnLike();
            if (performanceReview.LikeStatus)
            {
                if (reviewData != null)
                {
                    if (reviewData.LikeStatus == false)
                    {
                        
                        return Json("UnLiked", JsonRequestBehavior.AllowGet);
                    }
                    reviewData.LikeStatus = false;
                    reviewData.LikeUnlikeDate = now;
                    db.Entry(reviewData).State = EntityState.Modified;
                    db.SaveChanges();
                    
                    return Json("UnLiked", JsonRequestBehavior.AllowGet);
                }
                else
                {
                    performancelike.LikeStatus = false;
                    performancelike.LikeUnlikeDate = now;
                    performancelike.UserId = performanceReview.UserId;
                    performancelike.UserReviewId = performanceReview.UserReviewId;
                    db.EmpPerFormanceReview_LikeUnLike.Add(performancelike);
                    db.SaveChanges();
                   
                    return Json("UnLiked", JsonRequestBehavior.AllowGet);
                }
            }
            else
            {
                if (reviewData != null)
                {
                    var data = db.EmpPerFormanceReview_LikeUnLike.Where(x => x.UserId == performanceReview.UserId && x.UserReviewId == performanceReview.UserReviewId).FirstOrDefault();
                    db.EmpPerFormanceReview_LikeUnLike.Remove(data);
                    db.SaveChanges();
                    
                    return Json("UnLikeRemoved", JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json("Success", JsonRequestBehavior.AllowGet);
                }
            }
        }
        public JsonResult IncreaseLike(EmpPerFormanceReview_LikeUnLike performanceReview)
        {
            var reviewData = db.EmpPerFormanceReview_LikeUnLike.Where(x => x.UserReviewId == performanceReview.UserReviewId && x.UserId == performanceReview.UserId).FirstOrDefault();
            EmpPerFormanceReview_LikeUnLike performancelike = new EmpPerFormanceReview_LikeUnLike();
            if (performanceReview.LikeStatus)
            {
                if (reviewData != null)
                {
                    if (reviewData.LikeStatus == true)
                    {
                        
                        return Json("Liked", JsonRequestBehavior.AllowGet);
                    }
                    reviewData.LikeStatus = true;
                    reviewData.LikeUnlikeDate = now;
                    db.Entry(reviewData).State = EntityState.Modified;
                    db.SaveChanges();
                    
                    return Json("Liked", JsonRequestBehavior.AllowGet);
                }
                else {
                    performancelike.LikeStatus = true;
                    performancelike.LikeUnlikeDate = now;
                    performancelike.UserId = performanceReview.UserId;
                    performancelike.UserReviewId = performanceReview.UserReviewId;
                    db.EmpPerFormanceReview_LikeUnLike.Add(performancelike);
                    db.SaveChanges();
                    
                    return Json("Liked", JsonRequestBehavior.AllowGet);
                }
            }
            else {
                if (reviewData != null)
                {
                    var data = db.EmpPerFormanceReview_LikeUnLike.Where(x => x.UserId == performanceReview.UserId && x.UserReviewId == performanceReview.UserReviewId).FirstOrDefault();
                    db.EmpPerFormanceReview_LikeUnLike.Remove(data);
                    db.SaveChanges();
                   
                    return Json("LikeRemoved", JsonRequestBehavior.AllowGet);
                }
                else {
                    return Json("Success", JsonRequestBehavior.AllowGet);
                }
            }
            
        }
        public JsonResult RemoveLike(EmpPerFormanceReview_LikeUnLike performanceReview) {
            var data = db.EmpPerFormanceReview_LikeUnLike.Where(x => x.UserReviewId == performanceReview.UserReviewId && x.UserId == performanceReview.UserId && x.LikeStatus == true).FirstOrDefault();
            db.EmpPerFormanceReview_LikeUnLike.Remove(data);
            try
            {
                db.SaveChanges();
                return Json("Success", JsonRequestBehavior.AllowGet);
            }
            catch (Exception) {
                return Json("Error", JsonRequestBehavior.DenyGet);
            }
        }
        public JsonResult UpdateComment(UserPerformanceReview performanceReview)
        {
            var previousComment = db.UserPerformanceReviews.Find(performanceReview.Id);
            if (previousComment.ReviewBy == performanceReview.ReviewBy)
            {
                int status;
                previousComment.Review = performanceReview.Review;
                previousComment.IsUpdated = true;
                previousComment.UpdateDate = now;
                db.Entry(previousComment).State = EntityState.Modified;
                try
                {
                    db.SaveChanges();
                    status = 1;
                }
                catch (Exception) {
                    status = -1;
                }
                if (status == 1)
                {
                    var data = new
                    {
                        updatedData = previousComment,
                        Status = "Success"
                    };
                    return Json(data, JsonRequestBehavior.AllowGet); }
                else {
                    return Json(null, JsonRequestBehavior.DenyGet);
                }
            }

            return Json(null, JsonRequestBehavior.DenyGet);
        }
        public JsonResult deleteComment(int id)
        {
            var deleteData = db.UserPerformanceReviews.Find(id);
            try {
                db.UserPerformanceReviews.Remove(deleteData);
                var subcomments = db.UserPerformanceReviews.Where(x => x.HasParent == true && x.ParentId == id).ToList();
                if (subcomments.Count > 0)
                {
                    foreach (var data in subcomments)
                    {
                        db.UserPerformanceReviews.Remove(data);
                    }
                }
                db.SaveChanges();
                var deleted = new { 
                id=id,
                hasParent=deleteData.HasParent,
                parentId=deleteData.ParentId,
                Status="Success"
                };
                return Json(deleted, JsonRequestBehavior.AllowGet);
            }
            catch(Exception){
                var deleted = new
                {
                    id = id,
                    Status = false
                };
                return Json(deleted, JsonRequestBehavior.DenyGet);
            }
        }
        
        #endregion
        #region messaging by Kaikubad
        public JsonResult GetAllUserForChat() {
            var data = db.View_UserLists.Where(m => m.UserType == 1 && m.Status == 1).ToList();
            return Json(data, JsonRequestBehavior.AllowGet);
        }
        public JsonResult GetSelectedUserMessage(int loginUserId, int selecteduserId) {
            try
            {
                var chat = db.View_Chat_History.Where(x => (x.ReceiverId == selecteduserId && x.SenderId == loginUserId) || (x.ReceiverId == loginUserId && x.SenderId == selecteduserId)).ToList();
                var data = new { 
                chatHistory=chat,
                Status="Success",
                SelecteduserPic=db.UserInformations.Where(x=>x.UserId==selecteduserId).Select(x=>x.Picture).FirstOrDefault()
                };
                return Json(data, JsonRequestBehavior.AllowGet);
            }
            catch (Exception) {
                return Json("Error", JsonRequestBehavior.DenyGet);
            }
        }
        public JsonResult GetAllUnseenMessage(int id)
        {
        var data = db.View_Chat_History.Where(x => x.ReceiverId == id && (x.Seen == false || x.Seen == null)).Select(x => new
            {
                ConversationId=x.ConversationId,
                SenderId=x.SenderId,
                SenderName=x.SenderName,
                RecieverId=x.ReceiverId,
                RecieverName=x.ReveiverName,
                Chat=x.Chat,
                SendDate=x.SendDateFormate,
                SendTime=x.SendTime,
                Seen=x.Seen,
                SeenTime = x.SeenDateFormate+" "+x.SeenTime,
                SenderImage=db.View_UserLists.Where(c=>c.UserId==x.SenderId).Select(c=>c.Picture).FirstOrDefault()
            }).ToList();
            return Json(data, JsonRequestBehavior.AllowGet);
        }
        #endregion
        #region added by Kaikubad for Report of a comment
        public PartialViewResult GetAllReportNames(long id)
        {
            ViewBag.commentId = id;
            ViewBag.ReportComment = db.CommentReportCategories.ToList();
            return PartialView();
        }
        public JsonResult ReportComment(ReportedComment report) {
            var comment = db.UserPerformanceReviews.Find(report.CommentId);
            comment.IsReported=true;
            db.Entry(comment).State=EntityState.Modified;
            report.ReportedDate = now;
            db.ReportedComments.Add(report);
            try{
                db.SaveChanges();
                
            }catch(Exception){
                return Json("Error",JsonRequestBehavior.DenyGet);
            }
           
            return Json("Success", JsonRequestBehavior.AllowGet);
        }
        #endregion
    }
}