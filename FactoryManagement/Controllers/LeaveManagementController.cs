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
using FactoryManagement.ModelView.HR;
using System.Web.Helpers;
using FactoryManagement.ModelView.SalaryConfig;
using System.ComponentModel;
using System.Configuration;
using System.Data.Entity.Core.Objects;

namespace FactoryManagement.Controllers
{
    [Authorize]
    [DisplayName("Leave Management")]
    public class LeaveManagementController : Controller
    {

        #region Private Properties
        private FactoryManagementEntities db = new FactoryManagementEntities();
        private FactoryManagementEntities dbAdu = new FactoryManagementEntities();
        static private int offset = Convert.ToInt32(ConfigurationManager.AppSettings["localTime"]);
        DateTime now = DateTime.UtcNow.AddMinutes(offset);
        const int ShowLeaveUserPerPage = 15;
        #endregion


        //************************ Manually Give Leave  *****************************************
        #region
        public ActionResult LeaveApplication(int? id, string v)
        {
            Leave_Application_List model = new Leave_Application_List();
            if (id != null)
            {
                model = db.Leave_Application_List.Find(id);
            }
            if (v == "p")
            {
                if (model.ApplicationUserId > 0)
                {
                    ViewBag.Empname = db.View_UserLists.Where(m => m.UserId == model.ApplicationUserId).Select(m => m.UserName).FirstOrDefault();
                }
                return PartialView("_LeaveApplication", model);
            }
            return View(model);
        }
        public JsonResult LeaveAppSave(Leave_Application_List model,long userId)
        {
            string name = "Leave Approve";
            string msg = "";
            int ColumnId = 0;
            int status = -1;

            if (ModelState.IsValid)
            {
                model.ApprovedDate = now;
                db.Leave_Application_List.Add(model);
                try
                {
                    db.SaveChanges();
                    msg = "Leave has been successfully approved on " + now.ToString("MMM dd,yyyy hh:mm tt");
                    status = 1;
                    ColumnId =Convert.ToInt32(db.Leave_Application_List.Where(m => m.ApplicationUserId == model.ApplicationUserId && m.ApprovedBy == model.ApprovedBy).Max(m => m.LeaveId));
                }
                catch (Exception)
                {
                    msg = "Leave approve was unsuccessful on "+now.ToString("MMM dd, yyyy hh: mm tt");;
                    status = -1; 
                }
                SaveAuditLog(name, msg, "HR", "Leave Management", "Leave_Application_List", ColumnId, userId, now, status);
                if (status == 1)
                {
                    return Json("Success", JsonRequestBehavior.AllowGet);
                }
                
            }
            return Json("Error",JsonRequestBehavior.AllowGet);
        }
        public JsonResult EmployeeeNameSearching(string prefix)
        {
            if (prefix == "")
            {
                return Json("", JsonRequestBehavior.AllowGet);
            }
            else
            {
                var allUser = (from s in db.View_UserLists
                                  where (s.UserName.ToLower().Contains(prefix.ToLower()))
                                  && s.Status == 1
                                  select new 
                                  {
                                      UserId = s.UserId,
                                      UserName = s.UserName+" (Sitename: "+s.SiteName+" ; EmpId: "+s.EmpId+" )",
                                      Sitename = s.SiteName,
                                      DesignationName = s.DesignationName
                                  }).ToList();
                return Json(allUser, JsonRequestBehavior.AllowGet);
            }
        }
        #endregion


        //************************ Leave  Application  *******************************************
        #region
        public ActionResult LeaveApplicationForm(int? id,long userId)
        {
            Leave_Application_List model = new Leave_Application_List();
            model.ApplicationUserId = userId;
            if (id != null)
            {
                model = db.Leave_Application_List.Find(id);
            }
            return View(model);
        }
        public JsonResult LeaveAppRequestSave(Leave_Application_List model)
        {
            string name = "Leave Application";
            string msg = "";
            int ColumnId = 0;
            int status = -1;

            if (ModelState.IsValid)
            {
                model.AppliedDate = now;
                db.Leave_Application_List.Add(model);
                try
                {
                    db.SaveChanges();
                    msg = "Leave application has been successfully saved.";
                    status = 1;
                    ColumnId = Convert.ToInt32(db.Leave_Application_List.Where(m => m.ApplicationUserId == model.ApplicationUserId).Max(m => m.LeaveId));
                }
                catch (Exception)
                {
                    msg = "Leave application save was unsuccessful.";
                    status = -1;
                }
                SaveAuditLog(name, msg, "HR", "Leave Management", "Leave_Application_List", ColumnId, model.ApplicationUserId, now, status);
                if (status == 1)
                {
                    return Json("Success", JsonRequestBehavior.AllowGet);
                }

            }
            return Json("Error", JsonRequestBehavior.AllowGet);
        }
        #endregion


        //************************** All Requested Leave Application *****************************
        #region
        public ActionResult ShowAllRequestedLeaveApplication()
        {
            return View();
        }
        #endregion


        #region
        public PartialViewResult _LeaveAppDetails(int id)
        {
            var model = db.Leave_Application_List.Find(id);
            return PartialView(model);
        }
        #endregion

        //*********************** Edit Leave Application Form ****************************************
        #region
        public PartialViewResult _LeaveAppEdit(int? id)
        {
            ViewBag.id = id;
            return PartialView();
        }
        public JsonResult LeaveAppEdit(Leave_Application_List model)
        {
            string name = "Leave Application Edit";
            string msg = "";
            int ColumnId = 0;
            int status = -1;
            if (model.LeaveId>0)
            {
                Leave_Application_List leave = db.Leave_Application_List.Find(model.LeaveId);
                leave.PreStartDate = leave.StartDate;
                leave.PreEndDate = leave.EndDate;

                leave.StartDate = model.StartDate;
                leave.EndDate = model.EndDate;
                leave.UpdatedDate = now;
                leave.UpdatedBy = model.UpdatedBy;
                leave.Reason = model.Reason;
                db.Entry(leave).State = EntityState.Modified;
                try
                {
                    db.SaveChanges();
                    msg = "Leave application has been successfully upated on "+now.ToString("MMM dd,yyyy hh:mm tt")+".";
                    status = 1;
                    // ColumnId = Convert.ToInt32(db.Leave_Application_List.Where(m => m.ApplicationUserId == model.ApplicationUserId && m.ApprovedBy == model.ApprovedBy).Max(m => m.LeaveId));
                  //Already have the leave application id. 
                    
                    //LeaveId data type is long and ColumnId data type is int.
                    //It may raise problem in long run;
                    ColumnId = (int)model.LeaveId;
                }
                catch (Exception)
                {
                    msg = "Leave application update was unsuccessfulon " + now.ToString("MMM dd,yyyy hh:mm tt") + ".";
                    status = -1;
                }
                SaveAuditLog(name, msg, "HR", "Leave Management", "Leave_Application_List", ColumnId, Convert.ToInt32(model.UpdatedBy), now, status);
                if (status == 1)
                {
                    return Json("Success", JsonRequestBehavior.AllowGet);
                }

            }
            return Json("Error", JsonRequestBehavior.AllowGet);
        }
        #endregion

        //*********************** Approve Application Form ****************************************
        #region
        public JsonResult LeaveAppApprove(long? id, long userId)
        {
            string name = "Leave Application Approve";
            string msg = "";
            int status = -1;

            if (id != null)
            {
                Leave_Application_List model = db.Leave_Application_List.Find(id);
                model.ApproveStatus = 1;
                model.ApprovedBy = userId;
                model.ApprovedDate = now;
                db.Entry(model).State = EntityState.Modified;
                try
                {
                    db.SaveChanges();
                    msg = "Leave application has been successfully approved on " + now.ToString("MMM dd, yyyy hh:mm tt") + " .";
                    status = 1;
                }
                catch (Exception)
                {
                    msg = "Leave application approve was unsuccessful on " + now.ToString("MMM dd, yyyy hh:mm tt") + " .";
                    status = -1;
                }
                SaveAuditLog(name, msg, "HR", "Leave Management", "Leave_Application_List", Convert.ToInt32(model.LeaveId), userId, now, status);
                if (status == 1)
                {
                    return Json("Success", JsonRequestBehavior.AllowGet);
                }
            }
            return Json("Error", JsonRequestBehavior.AllowGet);
        }
        #endregion

        //*********************** Reject Application Form ****************************************
        #region
        public PartialViewResult _LeaveRejectReason(long? id)
        {
            ViewBag.id = id;
            return PartialView();
        }
        public JsonResult LeaveAppReject(long? id, long userId, string Reason)
        {
            string name = "Leave Application Reject";
            string msg = "";
            int status = -1;

            if (id != null)
            {
                Leave_Application_List model = db.Leave_Application_List.Find(id);
                model.ApproveStatus = -1;
                model.ApprovedBy = userId;
                model.ApprovedDate = now;
                model.Reject_Reason = Reason;
                db.Entry(model).State = EntityState.Modified;
                try
                {
                    db.SaveChanges();
                    msg = "Leave application has been successfully rejected on "+now.ToString("MMM dd, yyyy hh:mm tt")+" .";
                    status = 1;
                }
                catch (Exception)
                {
                    msg = "Leave application rejection was unsuccessful on " + now.ToString("MMM dd, yyyy hh:mm tt") + " .";
                    status = -1;
                }
                SaveAuditLog(name, msg, "HR", "Leave Management", "Leave_Application_List",Convert.ToInt32(model.LeaveId), userId, now, status);
                if (status == 1)
                {
                    return Json("Success", JsonRequestBehavior.AllowGet);
                }
            }
            return Json("Error", JsonRequestBehavior.AllowGet);
        }
        #endregion

        //*********************** Rejected Leave Application Form Delete ****************************************
        #region
        public JsonResult LeaveAppDelete(long? id, long userId)
        {
            string name = "Rejected Leave Application Delete Parmanently";
            string msg = "";
            int status = -1;

            if (id != null)
            {
                Leave_Application_List model = db.Leave_Application_List.Find(id);
                if (model != null)
                {
                    db.Leave_Application_List.Remove(model);
                    try
                    {
                        db.SaveChanges();
                        msg = "Rejected leave application has been successfully deleted on " + now.ToString("MMM dd, yyyy hh:mm tt") + " .";
                        status = 1;
                    }
                    catch (Exception)
                    {
                        msg = "Rejected leave application deletetion was unsuccessful on " + now.ToString("MMM dd, yyyy hh:mm tt") + " .";
                        status = -1;
                    }
                    SaveAuditLog(name, msg, "HR", "Leave Management", "Leave_Application_List", Convert.ToInt32(model.LeaveId), userId, now, status);
                    if (status == 1)
                    {
                        return Json("Success", JsonRequestBehavior.AllowGet);
                    }
                }
                
            }
            return Json("Error", JsonRequestBehavior.AllowGet);
        }
        #endregion


        //************************** Main Page *****************************
        #region
        public ActionResult LeaveManagement()
        {
            var list = db.View_Leave_Application.ToList();
            var siteList = new SelectList(list.Select(x => new { x.SiteId, x.SiteName }).Distinct(), "SiteId", "SiteName");
            ViewBag.AllSiteList = siteList;
            return View();
        }
        //no need
        public PartialViewResult _ShowAllRequestedLeave(int? siteId, long? UserId,int? days,DateTime? FromDate,DateTime? ToDate)
        {
            ViewBag.fromDate = (FromDate == null) ? null : Convert.ToDateTime(FromDate).ToString("MMMM dd yyyy");
            ViewBag.toDate = (ToDate == null) ? null : Convert.ToDateTime(ToDate).ToString("MMMM dd yyyy");
            int day = Convert.ToInt32(days);
            if (siteId == null && UserId == null)
            {
                var list = db.View_Leave_Application.Where(m=> m.ApproveStatus == 0).ToList();
                list = (days != null && day > 0) ? (day == 1) ? list.Where(x => x.StartDate.Date == now.Date || x.EndDate.Date == now.Date).ToList() : list.Where(x => x.StartDate.Date >= now.AddDays(-day).Date).ToList() : list;
                list = (FromDate != null && ToDate == null) ? list.Where(x => x.StartDate.Date <= FromDate.Value.Date && x.EndDate.Date>=FromDate.Value.Date).ToList() : list;
                list = (FromDate == null && ToDate != null) ? list.Where(x => x.StartDate.Date <= ToDate.Value.Date || x.EndDate.Date >= ToDate.Value.Date).ToList() : list;
                list = (FromDate != null && ToDate != null) ? list.Where(x => (x.StartDate.Date <= FromDate.Value.Date && x.EndDate.Date >= FromDate.Value.Date) || (x.StartDate.Date <= ToDate.Value.Date && x.EndDate.Date >= ToDate.Value.Date)).ToList() : list;

                Session["All_Leave_User"] = list;
                Session["Total_Leave_User"] = list.Count();
                ViewBag.totalCount = list.Count();

                var siteList = new SelectList(list.Select(x => new { x.SiteId, x.SiteName }).Distinct(), "SiteId", "SiteName");
                ViewBag.AllSiteList = siteList;
                ViewBag.list = list.Take(ShowLeaveUserPerPage).ToList();
            }
            else
            {
                if (siteId != null && UserId != null)
                {
                    var list = db.View_Leave_Application.Where(m => m.ApproveStatus == 0 && m.SiteId == siteId && m.ApplicationUserId == UserId).ToList();
                    list = (days != null && day > 0) ? (day == 1) ? list.Where(x => x.StartDate.Date == now.Date || x.EndDate.Date == now.Date).ToList() : list.Where(x => x.StartDate.Date >= now.AddDays(-day).Date).ToList() : list;
                    list = (FromDate != null && ToDate == null) ? list.Where(x => x.StartDate.Date <= FromDate.Value.Date && x.EndDate.Date >= FromDate.Value.Date).ToList() : list;
                    list = (FromDate == null && ToDate != null) ? list.Where(x => x.StartDate.Date <= ToDate.Value.Date || x.EndDate.Date >= ToDate.Value.Date).ToList() : list;
                    list = (FromDate != null && ToDate != null) ? list.Where(x => (x.StartDate.Date <= FromDate.Value.Date && x.EndDate.Date >= FromDate.Value.Date) || (x.StartDate.Date <= ToDate.Value.Date && x.EndDate.Date >= ToDate.Value.Date)).ToList() : list;

                    Session["All_Leave_User"] = list;
                    Session["Total_Leave_User"] = list.Count();
                    ViewBag.totalCount = list.Count();

                    var siteList = new SelectList(list.Select(x => new { x.SiteId, x.SiteName }).Distinct(), "SiteId", "SiteName");
                    ViewBag.AllSiteList = siteList;
                    ViewBag.list = list.Take(ShowLeaveUserPerPage).ToList();
                }
                else
                {
                    if (siteId != null)
                    {
                        var list = db.View_Leave_Application.Where(m => m.ApproveStatus == 0 && m.SiteId == siteId).ToList();
                        list = (days != null && day > 0) ? (day == 1) ? list.Where(x => x.StartDate.Date == now.Date || x.EndDate.Date == now.Date).ToList() : list.Where(x => x.StartDate.Date >= now.AddDays(-day).Date).ToList() : list;
                        list = (FromDate != null && ToDate == null) ? list.Where(x => x.StartDate.Date <= FromDate.Value.Date && x.EndDate.Date >= FromDate.Value.Date).ToList() : list;
                        list = (FromDate == null && ToDate != null) ? list.Where(x => x.StartDate.Date <= ToDate.Value.Date || x.EndDate.Date >= ToDate.Value.Date).ToList() : list;
                        list = (FromDate != null && ToDate != null) ? list.Where(x => (x.StartDate.Date <= FromDate.Value.Date && x.EndDate.Date >= FromDate.Value.Date) || (x.StartDate.Date <= ToDate.Value.Date && x.EndDate.Date >= ToDate.Value.Date)).ToList() : list;

                        Session["All_Leave_User"] = list;
                        Session["Total_Leave_User"] = list.Count();
                        ViewBag.totalCount = list.Count();

                        var siteList = new SelectList(list.Select(x => new { x.SiteId, x.SiteName }).Distinct(), "SiteId", "SiteName");
                        ViewBag.AllSiteList = siteList;
                        ViewBag.list = list.Take(ShowLeaveUserPerPage).ToList();
                    }
                    else
                    {
                        var list = db.View_Leave_Application.Where(m => m.ApproveStatus == 0 && m.ApplicationUserId == UserId).ToList();

                        list = (days != null && day > 0) ? (day == 1) ? list.Where(x => x.StartDate.Date == now.Date || x.EndDate.Date == now.Date).ToList() : list.Where(x => x.StartDate.Date >= now.AddDays(-day).Date).ToList() : list;
                        list = (FromDate != null && ToDate == null) ? list.Where(x => x.StartDate.Date <= FromDate.Value.Date && x.EndDate.Date >= FromDate.Value.Date).ToList() : list;
                        list = (FromDate == null && ToDate != null) ? list.Where(x => x.StartDate.Date <= ToDate.Value.Date || x.EndDate.Date >= ToDate.Value.Date).ToList() : list;
                        list = (FromDate != null && ToDate != null) ? list.Where(x => (x.StartDate.Date <= FromDate.Value.Date && x.EndDate.Date >= FromDate.Value.Date) || (x.StartDate.Date <= ToDate.Value.Date && x.EndDate.Date >= ToDate.Value.Date)).ToList() : list;

                        Session["All_Leave_User"] = list;
                        Session["Total_Leave_User"] = list.Count();
                        ViewBag.totalCount = list.Count();

                        var siteList = new SelectList(list.Select(x => new { x.SiteId, x.SiteName }).Distinct(), "SiteId", "SiteName");
                        ViewBag.AllSiteList = siteList;
                        ViewBag.list = list.Take(ShowLeaveUserPerPage).ToList();
                    }
                }

            }
            return PartialView("_ShowAllRequestedLeave", ViewBag.list);
        }
        //no need
        public PartialViewResult _ShowAllTodayLeave(int? siteId, long? UserId,int? days)
        {
            int day = Convert.ToInt32(days);
            if (siteId == null && UserId == null)
            {
                var list = db.View_Leave_Application.Where(m => m.IsInLeave == true).ToList();
                list = (days != null && day > 0) ? (day == 1) ? list.Where(x => x.StartDate.Date == now.Date || x.EndDate.Date == now.Date).ToList() : list.Where(x => x.StartDate.Date >= now.AddDays(-day).Date).ToList() : list;

                Session["All_Leave_User"] = list;
                Session["Total_Leave_User"] = list.Count();
                ViewBag.totalCount = list.Count();

                var siteList = new SelectList(list.Select(x => new { x.SiteId, x.SiteName }).Distinct(), "SiteId", "SiteName");
                ViewBag.AllSiteList = siteList;
                ViewBag.list = list.Take(ShowLeaveUserPerPage).ToList();
            }
            else
            {
                if (siteId != null && UserId != null)
                {
                    var list = db.View_Leave_Application.Where(m => m.IsInLeave == true && m.SiteId == siteId && m.ApplicationUserId == UserId).ToList();
                    list = (days != null && day > 0) ? (day == 1) ? list.Where(x => x.StartDate.Date == now.Date || x.EndDate.Date == now.Date).ToList() : list.Where(x => x.StartDate.Date >= now.AddDays(-day).Date).ToList() : list;

                    Session["All_Leave_User"] = list;
                    Session["Total_Leave_User"] = list.Count();
                    ViewBag.totalCount = list.Count();

                    var siteList = new SelectList(list.Select(x => new { x.SiteId, x.SiteName }).Distinct(), "SiteId", "SiteName");
                    ViewBag.AllSiteList = siteList;
                    ViewBag.list = list.Take(ShowLeaveUserPerPage).ToList();
                }
                else
                {
                    if (siteId != null)
                    {
                        var list = db.View_Leave_Application.Where(m => m.IsInLeave == true && m.SiteId == siteId).ToList();
                        list = (days != null && day > 0) ? (day == 1) ? list.Where(x => x.StartDate.Date == now.Date || x.EndDate.Date == now.Date).ToList() : list.Where(x => x.StartDate.Date >= now.AddDays(-day).Date).ToList() : list;

                        Session["All_Leave_User"] = list;
                        Session["Total_Leave_User"] = list.Count();
                        ViewBag.totalCount = list.Count();

                        var siteList = new SelectList(list.Select(x => new { x.SiteId, x.SiteName }).Distinct(), "SiteId", "SiteName");
                        ViewBag.AllSiteList = siteList;
                        ViewBag.list = list.Take(ShowLeaveUserPerPage).ToList();
                    }
                    else
                    {
                        var list = db.View_Leave_Application.Where(m => m.IsInLeave == true && m.ApplicationUserId == UserId).ToList();
                        list = (days != null && day > 0) ? (day == 1) ? list.Where(x => x.StartDate.Date == now.Date || x.EndDate.Date == now.Date).ToList() : list.Where(x => x.StartDate.Date >= now.AddDays(-day).Date).ToList() : list;

                        Session["All_Leave_User"] = list;
                        Session["Total_Leave_User"] = list.Count();
                        ViewBag.totalCount = list.Count();

                        var siteList = new SelectList(list.Select(x => new { x.SiteId, x.SiteName }).Distinct(), "SiteId", "SiteName");
                        ViewBag.AllSiteList = siteList;
                        ViewBag.list = list.Take(ShowLeaveUserPerPage).ToList();
                    }
                }
            }
            return PartialView("_ShowAllRequestedLeave", ViewBag.list);
        }
        public PartialViewResult _ShowAllRequest(int? siteId,int? status, long? UserId,int? days,DateTime? FromDate,DateTime? ToDate)
        {
            ViewBag.fromDate = (FromDate == null) ? null : Convert.ToDateTime(FromDate).ToString("MMMM dd yyyy");
            ViewBag.ToDate = (ToDate == null) ? null : Convert.ToDateTime(ToDate).ToString("MMMM dd yyyy");
            int day = Convert.ToInt32(days);

            if (status==null) {
                var list = (siteId == null && UserId == null) ? db.View_Leave_Application.ToList() :
                (siteId != null && UserId != null) ? db.View_Leave_Application.Where(m => m.SiteId == siteId && m.ApplicationUserId == UserId).ToList() :
                (siteId != null) ? db.View_Leave_Application.Where(m => m.SiteId == siteId).ToList() : db.View_Leave_Application.Where(m => m.ApplicationUserId == UserId).ToList();

                list = (days != null && day > 0) ? (day == 1) ? list.Where(x => x.StartDate.Date == now.Date || x.EndDate.Date == now.Date).ToList() : list.Where(x => x.StartDate.Date >= now.AddDays(-day).Date || x.EndDate.Date <= now.AddDays(-day).Date).ToList() : list;
                if (FromDate != null || ToDate != null)
                {
                    //list = (FromDate != null) ? list.Where(x => x.StartDate.Date <= FromDate.Value.Date && x.EndDate.Date >= FromDate.Value.Date).ToList() : list;
                    //list = (ToDate != null) ? list.Where(x => x.StartDate.Date <= ToDate.Value.Date && x.EndDate.Date >= ToDate.Value.Date).ToList() : list;

                    list = (FromDate != null && ToDate != null) ? list.Where(x => (x.StartDate.Date <= FromDate.Value.Date && x.EndDate.Date >= FromDate.Value.Date) || (x.StartDate.Date <= ToDate.Value.Date && x.EndDate.Date >= ToDate.Value.Date)).ToList() :
                    (FromDate != null && ToDate == null) ? list.Where(x => x.StartDate.Date <= FromDate.Value.Date && x.EndDate.Date >= FromDate.Value.Date).ToList() : list.Where(x => x.StartDate.Date <= ToDate.Value.Date && x.EndDate.Date >= ToDate.Value.Date).ToList();

                }
                Session["All_Leave_User"] = list;
                Session["Total_Leave_User"] = list.Count();
                ViewBag.totalCount = list.Count();

                var siteList = new SelectList(list.Select(x => new { x.SiteId, x.SiteName }).Distinct(), "SiteId", "SiteName");
                ViewBag.AllSiteList = siteList;
                ViewBag.list = list.Take(ShowLeaveUserPerPage).ToList();
            }
            else {
                var list = (siteId == null && UserId == null) ? db.View_Leave_Application.Where(x=>x.ApproveStatus==status).ToList() :
                (siteId != null && UserId != null) ? db.View_Leave_Application.Where(m => m.SiteId == siteId && m.ApplicationUserId == UserId && m.ApproveStatus == status).ToList() :
                (siteId != null) ? db.View_Leave_Application.Where(m => m.SiteId == siteId && m.ApproveStatus == status).ToList() : db.View_Leave_Application.Where(m => m.ApplicationUserId == UserId && m.ApproveStatus == status).ToList();

                list = (days != null && day > 0) ? (day == 1) ? list.Where(x => x.StartDate.Date == now.Date || x.EndDate.Date == now.Date).ToList() : list.Where(x => x.StartDate.Date >= now.AddDays(-day).Date || x.EndDate.Date <= now.AddDays(-day).Date).ToList() : list;
                if (status == 1) {
                    list = list.Where(x => x.StartDate >= now.Date ).ToList();
                }
                if (FromDate != null || ToDate != null)
                {
                    list = (FromDate != null && ToDate != null) ? list.Where(x => (x.StartDate.Date <= FromDate.Value.Date && x.EndDate.Date >= FromDate.Value.Date) || (x.StartDate.Date <= ToDate.Value.Date && x.EndDate.Date >= ToDate.Value.Date)).ToList() :
                    (FromDate != null && ToDate == null) ? list.Where(x => x.StartDate.Date <= FromDate.Value.Date && x.EndDate.Date >= FromDate.Value.Date).ToList() : list.Where(x => x.StartDate.Date <= ToDate.Value.Date && x.EndDate.Date >= ToDate.Value.Date).ToList();

                }
                  Session["All_Leave_User"] = list;
                Session["Total_Leave_User"] = list.Count();
                ViewBag.totalCount = list.Count();

                var siteList = new SelectList(list.Select(x => new { x.SiteId, x.SiteName }).Distinct(), "SiteId", "SiteName");
                ViewBag.AllSiteList = siteList;
                ViewBag.list = list.Take(ShowLeaveUserPerPage).ToList();
            }
            return PartialView("_ShowAllRequestedLeave", ViewBag.list);
        }
        //no need
        public PartialViewResult _ShowAllRejectedLeave(int? siteId, long? UserId,int? days,DateTime? FromDate,DateTime? ToDate)
        {
            ViewBag.fromDate = (FromDate == null) ? null : Convert.ToDateTime(FromDate).ToString("MMMM dd yyyy");
            ViewBag.toDate = (ToDate == null) ? null : Convert.ToDateTime(ToDate).ToString("MMMM dd yyyy");
            int day = Convert.ToInt32(days);
            if (siteId == null && UserId == null)
            {
                var list = db.View_Leave_Application.Where(m => m.ApproveStatus == -1).ToList();
                list = (days != null && day > 0) ? (day == 1) ? list.Where(x => x.StartDate.Date == now.Date || x.EndDate.Date == now.Date).ToList() : list.Where(x => x.StartDate.Date >= now.AddDays(-day).Date).ToList() : list;
                list = (FromDate != null && ToDate == null) ? list.Where(x => x.StartDate.Date <= FromDate.Value.Date && x.EndDate.Date >= FromDate.Value.Date).ToList() : list;
                list = (FromDate == null && ToDate != null) ? list.Where(x => x.StartDate.Date <= ToDate.Value.Date || x.EndDate.Date >= ToDate.Value.Date).ToList() : list;
                list = (FromDate != null && ToDate != null) ? list.Where(x => (x.StartDate.Date <= FromDate.Value.Date && x.EndDate.Date >= FromDate.Value.Date) || (x.StartDate.Date <= ToDate.Value.Date && x.EndDate.Date >= ToDate.Value.Date)).ToList() : list;


                Session["All_Leave_User"] = list;
                Session["Total_Leave_User"] = list.Count();
                ViewBag.totalCount = list.Count();

                var siteList = new SelectList(list.Select(x => new { x.SiteId, x.SiteName }).Distinct(), "SiteId", "SiteName");
                ViewBag.AllSiteList = siteList;
                ViewBag.list = list.Take(ShowLeaveUserPerPage).ToList();
            }
            else
            {
                if (siteId != null && UserId != null)
                {
                    var list = db.View_Leave_Application.Where(m => m.ApproveStatus == -1 && m.SiteId == siteId && m.ApplicationUserId == UserId).ToList();
                    list = (days != null && day > 0) ? (day == 1) ? list.Where(x => x.StartDate.Date == now.Date || x.EndDate.Date == now.Date).ToList() : list.Where(x => x.StartDate.Date >= now.AddDays(-day).Date).ToList() : list;
                    list = (FromDate != null && ToDate == null) ? list.Where(x => x.StartDate.Date <= FromDate.Value.Date && x.EndDate.Date >= FromDate.Value.Date).ToList() : list;
                    list = (FromDate == null && ToDate != null) ? list.Where(x => x.StartDate.Date <= ToDate.Value.Date || x.EndDate.Date >= ToDate.Value.Date).ToList() : list;
                    list = (FromDate != null && ToDate != null) ? list.Where(x => (x.StartDate.Date <= FromDate.Value.Date && x.EndDate.Date >= FromDate.Value.Date) || (x.StartDate.Date <= ToDate.Value.Date && x.EndDate.Date >= ToDate.Value.Date)).ToList() : list;


                    Session["All_Leave_User"] = list;
                    Session["Total_Leave_User"] = list.Count();
                    ViewBag.totalCount = list.Count();

                    var siteList = new SelectList(list.Select(x => new { x.SiteId, x.SiteName }).Distinct(), "SiteId", "SiteName");
                    ViewBag.AllSiteList = siteList;
                    ViewBag.list = list.Take(ShowLeaveUserPerPage).ToList();
                }
                else
                {
                    if (siteId != null)
                    {
                        var list = db.View_Leave_Application.Where(m => m.ApproveStatus == -1 && m.SiteId == siteId).ToList();
                        list = (days != null && day > 0) ? (day == 1) ? list.Where(x => x.StartDate.Date == now.Date || x.EndDate.Date == now.Date).ToList() : list.Where(x => x.StartDate.Date >= now.AddDays(-day).Date).ToList() : list;
                        list = (FromDate != null && ToDate == null) ? list.Where(x => x.StartDate.Date <= FromDate.Value.Date && x.EndDate.Date >= FromDate.Value.Date).ToList() : list;
                        list = (FromDate == null && ToDate != null) ? list.Where(x => x.StartDate.Date <= ToDate.Value.Date || x.EndDate.Date >= ToDate.Value.Date).ToList() : list;
                        list = (FromDate != null && ToDate != null) ? list.Where(x => (x.StartDate.Date <= FromDate.Value.Date && x.EndDate.Date >= FromDate.Value.Date) || (x.StartDate.Date <= ToDate.Value.Date && x.EndDate.Date >= ToDate.Value.Date)).ToList() : list;


                        Session["All_Leave_User"] = list;
                        Session["Total_Leave_User"] = list.Count();
                        ViewBag.totalCount = list.Count();

                        var siteList = new SelectList(list.Select(x => new { x.SiteId, x.SiteName }).Distinct(), "SiteId", "SiteName");
                        ViewBag.AllSiteList = siteList;
                        ViewBag.list = list.Take(ShowLeaveUserPerPage).ToList();
                    }
                    else
                    {
                        var list = db.View_Leave_Application.Where(m => m.ApproveStatus == -1 && m.ApplicationUserId == UserId).ToList();
                        list = (days != null && day > 0) ? (day == 1) ? list.Where(x => x.StartDate.Date == now.Date || x.EndDate.Date == now.Date).ToList() : list.Where(x => x.StartDate.Date >= now.AddDays(-day).Date).ToList() : list;
                        list = (FromDate != null && ToDate == null) ? list.Where(x => x.StartDate.Date <= FromDate.Value.Date && x.EndDate.Date >= FromDate.Value.Date).ToList() : list;
                        list = (FromDate == null && ToDate != null) ? list.Where(x => x.StartDate.Date <= ToDate.Value.Date || x.EndDate.Date >= ToDate.Value.Date).ToList() : list;
                        list = (FromDate != null && ToDate != null) ? list.Where(x => (x.StartDate.Date <= FromDate.Value.Date && x.EndDate.Date >= FromDate.Value.Date) || (x.StartDate.Date <= ToDate.Value.Date && x.EndDate.Date >= ToDate.Value.Date)).ToList() : list;


                        Session["All_Leave_User"] = list;
                        Session["Total_Leave_User"] = list.Count();
                        ViewBag.totalCount = list.Count();

                        var siteList = new SelectList(list.Select(x => new { x.SiteId, x.SiteName }).Distinct(), "SiteId", "SiteName");
                        ViewBag.AllSiteList = siteList;
                        ViewBag.list = list.Take(ShowLeaveUserPerPage).ToList();
                    }
                }
            }
            return PartialView("_ShowAllRequestedLeave", ViewBag.list);
        }
        //no need
        public PartialViewResult _ShowAllUpcomingLeave(int? siteId, long? UserId,int? days,DateTime? FromDate,DateTime? ToDate)
        {
            ViewBag.fromDate = (FromDate == null) ? null : Convert.ToDateTime(FromDate).ToString("MMMM dd yyyy");
            ViewBag.toDate = (ToDate == null) ? null : Convert.ToDateTime(ToDate).ToString("MMMM dd yyyy");
            int day = Convert.ToInt32(days);
            if (siteId == null && UserId == null)
            {
                var list = db.View_Leave_Application.Where(m => m.ApproveStatus == 1 && DbFunctions.TruncateTime(m.StartDate) >= now.Date).ToList();
                list = (days != null && day > 0) ? (day == 1) ? list.Where(x => x.StartDate.Date == now.Date || x.EndDate.Date == now.Date).ToList() : list.Where(x => x.StartDate.Date >= now.AddDays(-day).Date).ToList() : list;
                list = (FromDate != null && ToDate == null) ? list.Where(x => x.StartDate.Date <= FromDate.Value.Date && x.EndDate.Date >= FromDate.Value.Date).ToList() : list;
                list = (FromDate == null && ToDate != null) ? list.Where(x => x.StartDate.Date <= ToDate.Value.Date || x.EndDate.Date >= ToDate.Value.Date).ToList() : list;
                list = (FromDate != null && ToDate != null) ? list.Where(x => (x.StartDate.Date <= FromDate.Value.Date && x.EndDate.Date >= FromDate.Value.Date) || (x.StartDate.Date <= ToDate.Value.Date && x.EndDate.Date >= ToDate.Value.Date)).ToList() : list;

                Session["All_Leave_User"] = list;
                Session["Total_Leave_User"] = list.Count();
                ViewBag.totalCount = list.Count();

                var siteList = new SelectList(list.Select(x => new { x.SiteId, x.SiteName }).Distinct(), "SiteId", "SiteName");
                ViewBag.AllSiteList = siteList;
                ViewBag.list = list.Take(ShowLeaveUserPerPage).ToList();
            }
            else
            {
                if (siteId != null && UserId != null)
                {
                    var list = db.View_Leave_Application
                        .Where(m => m.ApproveStatus == 1 && DbFunctions.TruncateTime(m.StartDate) >= now.Date && m.SiteId == siteId && m.ApplicationUserId == UserId)
                        .ToList();
                    list = (days != null && day > 0) ? (day == 1) ? list.Where(x => x.StartDate.Date == now.Date || x.EndDate.Date == now.Date).ToList() : list.Where(x => x.StartDate.Date >= now.AddDays(-day).Date).ToList() : list;
                    list = (FromDate != null && ToDate == null) ? list.Where(x => x.StartDate.Date <= FromDate.Value.Date && x.EndDate.Date >= FromDate.Value.Date).ToList() : list;
                    list = (FromDate == null && ToDate != null) ? list.Where(x => x.StartDate.Date <= ToDate.Value.Date || x.EndDate.Date >= ToDate.Value.Date).ToList() : list;
                    list = (FromDate != null && ToDate != null) ? list.Where(x => (x.StartDate.Date <= FromDate.Value.Date && x.EndDate.Date >= FromDate.Value.Date) || (x.StartDate.Date <= ToDate.Value.Date && x.EndDate.Date >= ToDate.Value.Date)).ToList() : list;

                    Session["All_Leave_User"] = list;
                    Session["Total_Leave_User"] = list.Count();
                    ViewBag.totalCount = list.Count();

                    var siteList = new SelectList(list.Select(x => new { x.SiteId, x.SiteName }).Distinct(), "SiteId", "SiteName");
                    ViewBag.AllSiteList = siteList;
                    ViewBag.list = list.Take(ShowLeaveUserPerPage).ToList();
                }
                else
                {
                    if (siteId != null)
                    {
                        var list = db.View_Leave_Application
                       .Where(m => m.ApproveStatus == 1 && (DbFunctions.TruncateTime(m.StartDate) >= now.Date) && m.SiteId == siteId)
                       .ToList();
                        Session["All_Leave_User"] = list;
                        Session["Total_Leave_User"] = list.Count();
                        ViewBag.totalCount = list.Count();
                        list = (days != null && day > 0) ? (day == 1) ? list.Where(x => x.StartDate.Date == now.Date || x.EndDate.Date == now.Date).ToList() : list.Where(x => x.StartDate.Date >= now.AddDays(-day).Date).ToList() : list;
                        list = (FromDate != null && ToDate == null) ? list.Where(x => x.StartDate.Date <= FromDate.Value.Date && x.EndDate.Date >= FromDate.Value.Date).ToList() : list;
                        list = (FromDate == null && ToDate != null) ? list.Where(x => x.StartDate.Date <= ToDate.Value.Date || x.EndDate.Date >= ToDate.Value.Date).ToList() : list;
                        list = (FromDate != null && ToDate != null) ? list.Where(x => (x.StartDate.Date <= FromDate.Value.Date && x.EndDate.Date >= FromDate.Value.Date) || (x.StartDate.Date <= ToDate.Value.Date && x.EndDate.Date >= ToDate.Value.Date)).ToList() : list;

                        var siteList = new SelectList(list.Select(x => new { x.SiteId, x.SiteName }).Distinct(), "SiteId", "SiteName");
                        ViewBag.AllSiteList = siteList;
                        ViewBag.list = list.Take(ShowLeaveUserPerPage).ToList();
                    }
                    else
                    {
                        var list = db.View_Leave_Application
                       .Where(m => m.ApproveStatus == 1 && DbFunctions.TruncateTime(m.StartDate) >= now.Date &&  m.ApplicationUserId == UserId)
                       .ToList();

                        list = (days != null && day > 0) ? (day == 1) ? list.Where(x => x.StartDate.Date == now.Date || x.EndDate.Date == now.Date).ToList() : list.Where(x => x.StartDate.Date >= now.AddDays(-day).Date).ToList() : list;
                        list = (FromDate != null && ToDate == null) ? list.Where(x => x.StartDate.Date <= FromDate.Value.Date && x.EndDate.Date >= FromDate.Value.Date).ToList() : list;
                        list = (FromDate == null && ToDate != null) ? list.Where(x => x.StartDate.Date <= ToDate.Value.Date || x.EndDate.Date >= ToDate.Value.Date).ToList() : list;
                        list = (FromDate != null && ToDate != null) ? list.Where(x => (x.StartDate.Date <= FromDate.Value.Date && x.EndDate.Date >= FromDate.Value.Date) || (x.StartDate.Date <= ToDate.Value.Date && x.EndDate.Date >= ToDate.Value.Date)).ToList() : list;



                        Session["All_Leave_User"] = list;
                        Session["Total_Leave_User"] = list.Count();
                        ViewBag.totalCount = list.Count();

                        var siteList = new SelectList(list.Select(x => new { x.SiteId, x.SiteName }).Distinct(), "SiteId", "SiteName");
                        ViewBag.AllSiteList = siteList;
                        ViewBag.list = list.Take(ShowLeaveUserPerPage).ToList();
                    }
                }
            }
            return PartialView("_ShowAllRequestedLeave", ViewBag.list);
        }
        public PartialViewResult _ShowAllUpcomingLeaveFilter(int? siteId, long? UserId,Leave_Application_List model)
        {
            var allDay = new List<DateTime?>();
            for (var date = model.StartDate; date <= model.EndDate; date = date.AddDays(1))
            {
                allDay.Add(date);
            }
            var list = db.View_Leave_Application.Where(m => m.ApproveStatus == 1).ToList();
            list = list.Where(m => allDay.Contains(m.StartDate) || allDay.Contains(m.EndDate)).ToList();
            if (siteId == null && UserId == null)
            {
                list = list.Where(m => allDay.Contains(m.StartDate) || allDay.Contains(m.EndDate)).ToList();
            }
            else
            {
                if (siteId != null && UserId != null)
                {
                    list = list.Where(m => m.SiteId == siteId && m.ApplicationUserId == UserId).ToList();
                }
                else
                {
                    if (siteId != null)
                    {
                        list = list.Where(m => m.SiteId == siteId).ToList();

                    }
                    else
                    {
                        list = list.Where(m => m.ApplicationUserId == UserId).ToList();
                    }
                }
            }
            Session["All_Leave_User"] = list;
            Session["Total_Leave_User"] = list.Count();
            ViewBag.totalCount = list.Count();

            var siteList = new SelectList(list.Select(x => new { x.SiteId, x.SiteName }).Distinct(), "SiteId", "SiteName");
            ViewBag.AllSiteList = siteList;
            ViewBag.list = list.Take(ShowLeaveUserPerPage).ToList();
            return PartialView("_ShowAllRequestedLeave", ViewBag.list);
        }
        public JsonResult GetLeaveUserList(int? page)
        {
            page = page ?? 0;
            int total = Convert.ToInt16(Session["Total_Leave_User"]);
            int skip = ShowLeaveUserPerPage * Convert.ToInt16(page);
            var canPage = skip < total;
            var listBackFromSession = (List<View_Leave_Application>)Session["All_Leave_User"];
            var userList = listBackFromSession.Skip(skip).Take(ShowLeaveUserPerPage).ToList();
            return Json(userList, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region Audit Log
        public void SaveAuditLog(string OperationName, string Message, string ModuleName, string PageName, string TableName, int ColumnId, long UserId, DateTime Date, int OperationStatus)
        {
            AuditInformation audit = new AuditInformation();
            audit.OperationName = OperationName;
            audit.Message = Message;
            audit.ModuleName = ModuleName;
            audit.PageName = PageName;
            audit.TableName = TableName;
            audit.ColumnId = ColumnId;
            audit.UserId = UserId;
            audit.Date = Date;
            audit.OperationStatus = OperationStatus;
            dbAdu.AuditInformations.Add(audit);
            dbAdu.SaveChanges();
        }
        #endregion

    }
}