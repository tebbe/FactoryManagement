
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

namespace FactoryManagement.Controllers
{
    [Authorize]
    public class AttendanceController : Controller
    {
        #region Private Properties
        static private int offset = Convert.ToInt32(ConfigurationManager.AppSettings["localTime"]);
        DateTime now = DateTime.UtcNow.AddMinutes(offset);
        private FactoryManagementEntities db = new FactoryManagementEntities();
        private FactoryManagementEntities dbAud = new FactoryManagementEntities();
        #endregion

        const int ShowUserPerPage = 20;
        const int ShowUserPerPageForHis = 50;
        const int SpecUserAttenPerPage = 500;

        const int PerPageUserAtten = 50;

        //******************************* All User List ********************************************
        #region
        public ActionResult Attendance()
        {
            var list = new SelectList(db.View_User_Att_Input.Where(m => m.Status == 1).Select(x => new { x.SiteId, x.SiteName }).Distinct(), "SiteId", "SiteName");
            ViewBag.AllSiteList = list;
            return View();
        }
        public ActionResult _Attendance(int? siteId, long? UserId)
        {
            if (siteId == null && UserId == null)
            {
                var user = db.View_User_Att_Input.Where(m => m.Status == 1).OrderBy(o => o.UserName).ToList(); 
                if (user != null)
                {
                    Session["AllUsers"] = user;
                    ViewBag.TotalUser = user.Count();
                    Session["TotalUserCount"] = user.Count();
                    ViewBag.UserList = user.Take(ShowUserPerPage).ToList();
                }
                else
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }
            }
            else
            {
                if (siteId != null)
                {
                    var user = db.View_User_Att_Input.Where(m => m.Status == 1).Where(m => m.SiteId == siteId).OrderBy(o => o.UserName).ToList(); 
                    Session["AllUsers"] = user;
                    ViewBag.TotalUser = user.Count();
                    Session["TotalUserCount"] = user.Count();
                    ViewBag.UserList = user.Take(ShowUserPerPage).ToList();
                }
                else
                {
                    var allUser = (List<View_User_Att_Input>)Session["AllUsers"];
                    var allUserBySearch = allUser.Where(m => m.UserId == UserId).OrderBy(o => o.UserName).ToList();
                    Session["AllUsers"] = allUserBySearch;
                    ViewBag.TotalUser = allUserBySearch.Count();
                    Session["TotalUserCount"] = allUserBySearch.Count();
                    ViewBag.UserList = allUserBySearch.Take(ShowUserPerPage).ToList();
                }
            }
            return PartialView(ViewBag.UserList);
        }
        public JsonResult GetAttendance(int? page)
        {
            page = page ?? 0;
            int total = Convert.ToInt16(Session["TotalUserCount"]);
            int skip = ShowUserPerPage * Convert.ToInt16(page);
            var canPage = skip < total;
            var listBackFromSession = (List<View_User_Att_Input>)Session["AllUsers"];
            var userList = listBackFromSession.Skip(skip).Take(ShowUserPerPage).ToList();
            return Json(userList, JsonRequestBehavior.AllowGet);
        }
        public JsonResult SearchUserList(string prefix)
        {
            if (prefix == "")
            {
                return Json("", JsonRequestBehavior.AllowGet);
            }
            else
            {
                var allUser = (List<View_User_Att_Input>)Session["AllUsers"];
                var user = allUser.Where(s => s.UserName.ToLower().Contains(prefix.ToLower()) || s.DesignationName.ToLower().Contains(prefix.ToLower())).ToList();
                
                Session["AllUsersBySearch"] = user;
                return Json(user, JsonRequestBehavior.AllowGet);
            }
        }

        public PartialViewResult _LeaveType(long? id)
        {
            ViewBag.SelectedUserId = id;
            return PartialView();
        }
        public PartialViewResult _LeaveTypePartial()
        {
            ViewBag.LeaveType = new SelectList(db.SickLeaveTypes, "Id", "Name");
            return PartialView();
        }

        public JsonResult SaveAttendance(Daily_Attendance model)
        {
            if (model != null)
            {
                int ColumnId = 0;
                string msg = "";
                int Status = -1;
                var user_info = db.View_UserLists.Where(m => m.UserId == model.UserId).FirstOrDefault();
                var inserted_User_Info = db.View_UserLists.Where(m => m.UserId == model.InsertedBy).FirstOrDefault();

                model.AttDate = now.Date;
                model.AttTime = now.ToString("HH:mm:ss tt");
                model.Year = now.Year;
                model.MonthId = now.Month;
                model.MonthName = now.ToString("MMMM");
                model.AttDatetime = now;
                model.InsertedDate = now;
                db.Daily_Attendance.Add(model);
                try
                {
                    db.SaveChanges();
                    Status = 1;
                    ColumnId =Convert.ToInt32(db.Daily_Attendance.Where(m => m.UserId == model.UserId && m.InsertedBy == model.InsertedBy).Max(m => m.Id));
                    msg = "' " + user_info.UserName + "'s ' daily attendance has been successfully saved by ' " + ((inserted_User_Info != null) ? inserted_User_Info.UserName : "Super Admin") + " ' on "+now.ToString("MMM dd,yyyy hh:mm tt");
                }
                catch (Exception)
                {
                    Status = -1;
                    msg = "' " + user_info.UserName + "'s ' daily attendance save was unsuccessful by ' " +((inserted_User_Info!=null)?inserted_User_Info.UserName:"Super Admin") + " ' on " + now.ToString("MMM dd,yyyy hh:mm tt");
                }
                SaveAuditLog("Employee Attendance Input", msg, "HR", "Attendance", "Daily_Attendance", ColumnId, model.InsertedBy, now, Status);
                if (Status == 1)
                {
                    return Json("Success",JsonRequestBehavior.AllowGet);
                }
            }
            return Json("Error",JsonRequestBehavior.AllowGet);
        }
        public JsonResult UpdateAttendance(Daily_Attendance model)
        {
            if (model != null)
            {
                int ColumnId =(int)model.Id;
                string msg = "";
                int Status = -1;
                var user_info = db.View_UserLists.Where(m => m.UserId == model.UserId).FirstOrDefault();
                var inserted_User_Info = db.View_UserLists.Where(m => m.UserId == model.InsertedBy).FirstOrDefault();

                Daily_Attendance atten = db.Daily_Attendance.Find(model.Id);
                atten.AttType = model.AttType;
                atten.LeaveType = model.LeaveType;
                atten.InsertedBy = model.InsertedBy;
                atten.InsertedDate = now;

                db.Entry(atten).State = EntityState.Modified;
                try
                {
                    db.SaveChanges();
                    Status = 1;

                    msg = "' " + user_info.UserName + "'s ' daily attendance has been successfully changed by ' " + ((inserted_User_Info != null) ? inserted_User_Info.UserName : "Super Admin") + " ' on " + now.ToString("MMM dd,yyyy");
                }
                catch (Exception)
                {
                    Status = -1;
                    msg = "' " + user_info.UserName + "'s ' daily attendance change was unsuccessful by ' " + ((inserted_User_Info != null) ? inserted_User_Info.UserName : "Super Admin") + " ' on " + now.ToString("MMM dd,yyyy");
                }
                SaveAuditLog("Employee Attendance Input", msg, "HR", "Attendance", "Daily_Attendance", ColumnId, model.InsertedBy, now, Status);
                if (Status == 1)
                {
                    return Json("Success", JsonRequestBehavior.AllowGet);
                }
            }
            return Json("Error", JsonRequestBehavior.AllowGet);
        }
        #endregion


        //****************************** All Attendance History *************************************
        #region
        public ActionResult AllAttendance()
        {
            var list = new SelectList(db.View_UserLists.Where(m => m.Status == 1).Select(x => new { x.SiteId, x.SiteName }).Distinct(), "SiteId", "SiteName");
            ViewBag.AllSiteList = list;
            return View();
        }
        public PartialViewResult _AllAttendance(int? siteId, long? UserId)
        {
            if (siteId == null && UserId == null)
            {
                var user = db.View_UserLists.OrderBy(o => o.UserName).ToList();
                if (user != null)
                {
                    Session["AttenUsers"] = user;
                    ViewBag.TotalUser = user.Count();
                    Session["TotalAttenUsers"] = user.Count();
                    ViewBag.UserList = user.Take(ShowUserPerPageForHis).ToList();
                }
            }
            else
            {
                if (siteId != null)
                {
                    var user = db.View_UserLists.Where(m => m.SiteId == siteId).OrderBy(o => o.UserName).ToList();
                    Session["AttenUsers"] = user;
                    ViewBag.TotalUser = user.Count();
                    Session["TotalAttenUsers"] = user.Count();
                    ViewBag.UserList = user.Take(ShowUserPerPageForHis).ToList();
                }
                else
                {
                    var allUser = (List<View_UserLists>)Session["AttenUsers"];
                    var allUserBySearch = allUser.Where(m => m.UserId == UserId).OrderBy(o => o.UserName).ToList();
                    Session["AttenUsers"] = allUserBySearch;
                    ViewBag.TotalUser = allUserBySearch.Count();
                    Session["TotalAttenUsers"] = allUserBySearch.Count();
                    ViewBag.UserList = allUserBySearch.Take(ShowUserPerPageForHis).ToList();
                }
            }
            return PartialView(ViewBag.UserList);
        }
        public JsonResult GetAttendanceList(int? page)
        {
            page = page ?? 0;
            int total = Convert.ToInt16(Session["TotalAttenUsers"]);
            int skip = ShowUserPerPageForHis * Convert.ToInt16(page);
            var canPage = skip < total;
            var listBackFromSession = (List<View_UserLists>)Session["AttenUsers"];
            var userList = listBackFromSession.Skip(skip).Take(ShowUserPerPageForHis).ToList();
            return Json(userList, JsonRequestBehavior.AllowGet);
        }
        public JsonResult SearchUserAttendanceList(string prefix)
        {
            if (prefix == "")
            {
                return Json("", JsonRequestBehavior.AllowGet);
            }
            else
            {
                var allUser = (List<View_UserLists>)Session["AttenUsers"];
                var user = allUser.Where(s => s.UserName.ToLower().Contains(prefix.ToLower()) || s.DesignationName.ToLower().Contains(prefix.ToLower())).ToList();
                return Json(user, JsonRequestBehavior.AllowGet);
            }
        }

        public PartialViewResult _SpecificUserHistory(long UserId)
        {
            ViewBag.UserName = db.View_UserLists.Where(x => x.UserId == UserId).Select(x => x.UserName).FirstOrDefault();
            ViewBag.UserId = UserId;
            return PartialView();
        }
        public PartialViewResult _SpecificUserHistoryPartail(long UserId, string option,DateTime? FromDate,DateTime? ToDate,int? days)
        {
            ViewBag.fromDate = (FromDate==null)?null:Convert.ToDateTime(FromDate).ToString("MMMM dd yyyy");
            ViewBag.toDate = (ToDate==null)?null:Convert.ToDateTime(ToDate).ToString("MMMM dd yyyy");
            ViewBag.option = option;
            ViewBag.UserId = UserId;
            int day = Convert.ToInt32(days);
            var list = db.View_Daily_Attendance.Where(m => m.UserId == UserId).ToList();
            list = (FromDate != null && ToDate == null) ? list.Where(x => x.AttDate.Date == FromDate.Value.Date).ToList() : list;
            list = (FromDate == null && ToDate != null) ? list.Where(x => x.AttDate.Date == ToDate.Value.Date).ToList() : list;
            list = (FromDate != null && ToDate != null) ? list.Where(x => x.AttDate.Date >= FromDate.Value.Date && x.AttDate.Date<=ToDate.Value.Date).ToList() : list;
            list = (days != null) ?(day==1)?list.Where(x => x.AttDate.Date == now.Date).ToList():list.Where(x=>x.AttDate.Date>now.AddDays(-day).Date).ToList() : list;

            Session["SpecUserAtten"] = list;
            ViewBag.TotalUser = list.Count();
            Session["TotalSpecUserAtten"] = list.Count();
            var list2 = list.Take(SpecUserAttenPerPage).ToList();
            return PartialView(list2);
        }
        public JsonResult GetSpecificUserAttenList(int? page)
        {
            page = page ?? 0;
            int total = Convert.ToInt16(Session["TotalSpecUserAtten"]);
            int skip = ShowUserPerPageForHis * Convert.ToInt16(page);
            var canPage = skip < total;
            var listBackFromSession = (List<View_Daily_Attendance>)Session["SpecUserAtten"];
            var userList = listBackFromSession.Skip(skip).Take(SpecUserAttenPerPage).ToList();
            return Json(userList, JsonRequestBehavior.AllowGet);
        }
        #endregion



        //****************************** All Attendance History Using BarCode and Smart Card *************************************
        #region
        public ActionResult AllAttendanceBCR()
        {
            var list = new SelectList(db.View_Attendance_BCR.Select(x => new { x.SiteId, x.SiteName }).Distinct(), "SiteId", "SiteName");
            ViewBag.AllSiteList = list;
            return View();
        }
        public PartialViewResult _AllAttendanceBCR(int? siteId, long? UserId)
        {
            var user = db.View_Attendance_BCR.OrderBy(o => o.UserName).ToList();
            user = (siteId != null) ? user.Where(x => x.SiteId == siteId).ToList() : user;
            user = (UserId != null) ? user.Where(x => x.UserId == UserId).ToList() : user;
            //if (siteId == null && UserId == null)
            //{


            //}
            //else
            //{
            //    if (siteId != null)
            //    {
            //        user = user.Where(m => m.SiteId == siteId).OrderBy(o => o.UserName).ToList();
            //    }
            //    else
            //    {
            //        user = user.Where(m => m.UserId == UserId).OrderBy(o => o.UserName).ToList();
            //    }
            //}
            if (user != null)
            {
                Session["AttenUsersBCR"] = user;
                ViewBag.TotalUser = user.Count();
                Session["TotalAttenUsersBCR"] = user.Count();
                ViewBag.UserList = user.Take(ShowUserPerPageForHis).ToList();
            }
            return PartialView(ViewBag.UserList);
        }
        public JsonResult GetAttendanceListBCR(int? page)
        {
            page = page ?? 0;
            int total = Convert.ToInt16(Session["TotalAttenUsersBCR"]);
            int skip = PerPageUserAtten * Convert.ToInt16(page);
            var canPage = skip < total;
            var listBackFromSession = (List<View_Attendance_BCR>)Session["AttenUsersBCR"];
            var userList = listBackFromSession.Skip(skip).Take(PerPageUserAtten).ToList();
            return Json(userList, JsonRequestBehavior.AllowGet);
        }
        public JsonResult SearchUserAttendanceListBCR(string prefix)
        {
            if (prefix == "")
            {
                return Json("", JsonRequestBehavior.AllowGet);
            }
            else
            {
                var allUser = (List<View_Attendance_BCR>)Session["AttenUsersBCR"];
                var user = allUser.Where(s => s.UserName.ToLower().Contains(prefix.ToLower()) || s.DesignationName.ToLower().Contains(prefix.ToLower())).ToList();
                return Json(user, JsonRequestBehavior.AllowGet);
            }
        }

        public PartialViewResult _SpecificUserHistoryBCR(long UserId)
        {
            ViewBag.UserId = UserId;
            return PartialView();
        }
        public PartialViewResult _SpecificUserHistoryPartailBCR(long UserId, string option)
        {
            ViewBag.option = option;
            var list = db.View_Daily_Attendance.Where(m => m.UserId == UserId).ToList();
            Session["SpecUserAtten"] = list;
            ViewBag.TotalUser = list.Count();
            Session["TotalSpecUserAtten"] = list.Count();
            var list2 = list.Take(SpecUserAttenPerPage).ToList();
            return PartialView(list2);
        }
        public JsonResult GetSpecificUserAttenListBCR(int? page)
        {
            page = page ?? 0;
            int total = Convert.ToInt16(Session["TotalSpecUserAtten"]);
            int skip = ShowUserPerPageForHis * Convert.ToInt16(page);
            var canPage = skip < total;
            var listBackFromSession = (List<View_Daily_Attendance>)Session["SpecUserAtten"];
            var userList = listBackFromSession.Skip(skip).Take(SpecUserAttenPerPage).ToList();
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
            dbAud.AuditInformations.Add(audit);
            dbAud.SaveChanges();
        }
        #endregion
    }
}