
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
using System.Globalization;
using System.Configuration;
using FactoryManagement.Filters;


namespace FactoryManagement.Controllers
{
    public class HolidayController : Controller
    {
        #region Private Properties
        private FactoryManagementEntities db = new FactoryManagementEntities();
        private FactoryManagementEntities dbaud = new FactoryManagementEntities();
        static private int offset = Convert.ToInt32(ConfigurationManager.AppSettings["localTime"]);
        DateTime now = DateTime.UtcNow.AddMinutes(offset);
        #endregion



        //************************* Holiday Package *******************************************
        #region
        public ActionResult HolidayPackage()
        {
            return View();
        }
        public PartialViewResult _HolidayPackage()
        {
            var list = db.View_HolidayPackage.ToList();
            ViewBag.list = list;
            return PartialView();
        }
        public PartialViewResult _HolidayPackageCreate()
        {
            return PartialView();
        }
        public JsonResult HolidayPackSave(HolidayPackageList model)
        {
            if (ModelState.IsValid)
            {
                int status = -1;
                string msg = "";
                string opName = "Add New Holiday Package";
                int ColumnId = 0;
                HolidayPackageList holi_pack = new HolidayPackageList();
                holi_pack.HolidayPackName = model.HolidayPackName;
                holi_pack.NoOfPaidLeave = model.NoOfPaidLeave;
                holi_pack.CreatedBy = model.CreatedBy;
                holi_pack.CreatedDate = now;
                db.HolidayPackageLists.Add(holi_pack);
                try
                {
                    db.SaveChanges();
                    status = 1;
                    msg = "New holiday package '" + model.HolidayPackName + "' has been successfully created on " + now.ToString("MMM dd, yyyy hh:mm tt") + " .";
                    ColumnId = db.HolidayPackageLists.Where(m => m.CreatedBy == model.CreatedBy).Max(m => m.HolidayPackId);
                }
                catch (Exception)
                {
                    status = -1;
                    msg = "New holiday package '" + model.HolidayPackName + "' creation was unsuccessfull on " + now.ToString("MMM dd, yyyy hh:mm tt") + " .";
                }
                SaveAuditLog(opName, msg, "Holiday", "Holiday Package", "HolidayPackageList", ColumnId, model.CreatedBy, now, status);
                if (status == 1)
                {
                    return Json("Success", JsonRequestBehavior.AllowGet);
                }
            }
            return Json("Error", JsonRequestBehavior.AllowGet);
        }
        public PartialViewResult _HolidayPackInfo(int id, string view, long? UserId)
        {
            var model = db.View_HolidayPackage.Where(m => m.HolidayPackId == id).FirstOrDefault();
            ViewBag.view = view;
            ViewBag.UserId = UserId;
            return PartialView(model);
        }
        [EncryptedActionParameter]
        public ActionResult HolidayPackDetails(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            else
            {
                var model = db.View_HolidayPackage.Where(m => m.HolidayPackId == id).FirstOrDefault();
                if (model != null)
                {
                    return View(model);
                }
            }
            return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        }
        #endregion




        //********************** Holiday Create,Update ,Delete *****************************************
        #region
        public ActionResult _AllHolidayList(int HolidayPackId, string view)
        {
            ViewBag.view = view;
            return PartialView(db.View_Holiday_Lists.Where(m => m.HolidayPackId == HolidayPackId && m.Date > now).OrderBy(o => o.Date).ToList());
        }
        public PartialViewResult _HolidayCreateMain(int? id)
        {
            ViewBag.id = id;
            return PartialView();
        }
        public ActionResult _HolidayCreate(int? id)
        {
            HolidayListModelView model = new HolidayListModelView();
            if (id != null)
            {
                View_Holiday_Lists holiday = db.View_Holiday_Lists.Where(m => m.Id == id).FirstOrDefault();
                model.Id = holiday.Id;
                model.HolidayName = holiday.HolidayName;
                model.IsMultipleDay = holiday.IsMultipleDay;
                model.MonthCount = holiday.MonthCount;
                model.MonthName = holiday.MonthName;
                model.Date = holiday.Date;
                if (model.IsMultipleDay)
                {
                    model.StartDate = holiday.Date;
                    model.EndDate = holiday.EndDate;
                }
            }
            return PartialView(model);
        }
        public JsonResult SaveNewHoliday(HolidayListModelView model)
        {
            if (ModelState.IsValid)
            {
                string msg = "";
                string opName = "";
                int ColumnId = 0;
                int OperationStatus = 1;
                int TotalDay = 1;
                DateTime start = Convert.ToDateTime(model.StartDate);
                DateTime end = Convert.ToDateTime(model.EndDate);
                if (model.IsMultipleDay)
                {
                    TotalDay = new DateDifference(start, end).Days;
                    model.Date = model.StartDate;
                    TotalDay = TotalDay + 1;
                }
                string monthName = Convert.ToDateTime(model.Date).ToString("MMMM");
                int monthCount = Convert.ToInt16(Convert.ToDateTime(model.Date).ToString("MM"));
                string year = Convert.ToDateTime(model.Date).Year.ToString();
                HolidayList holiday = new HolidayList();
                if (model.Id > 0)
                {
                    holiday = db.HolidayLists.Find(model.Id);
                    holiday.HolidayName = model.HolidayName;
                    holiday.IsMultipleDay = model.IsMultipleDay;
                    holiday.MonthName = monthName;
                    holiday.MonthCount = monthCount;
                    holiday.TotalDay = TotalDay;
                    holiday.Date = model.Date;
                    if (model.IsMultipleDay)
                    {
                        holiday.EndDate = model.EndDate;
                    }
                    holiday.Year = year;
                    holiday.UpdatedDate = now;
                    holiday.UpdateBy = model.CreatedBy;
                    db.Entry(holiday).State = EntityState.Modified;
                    ColumnId = holiday.Id;
                    msg = "Holiday information of '" + model.HolidayName + "' updated successfully on "+now.ToString("MMM dd,yyyy hh:mm tt");
                    opName = "Holiday Info Update.";
                }
                else
                {
                    holiday.HolidayPackId = model.HolidayPackId;
                    holiday.HolidayName = model.HolidayName;
                    holiday.IsMultipleDay = model.IsMultipleDay;
                    holiday.MonthName = monthName;
                    holiday.MonthCount = monthCount;
                    holiday.TotalDay = TotalDay;
                    holiday.Date = model.Date;
                    holiday.Year = year;
                    if (model.IsMultipleDay)
                    {
                        holiday.EndDate = model.EndDate;
                    }
                    holiday.CreatedDate = now;
                    holiday.CreatedBy = model.CreatedBy;
                    db.HolidayLists.Add(holiday);
                    msg = "New holiday '" + model.HolidayName + "' has been successfully created on "+now.ToString("MMM dd, yyyy hh: mm tt");
                    opName = "New Holiday Create.";
                }
                try
                {
                    db.SaveChanges();
                    if (model.Id == 0)
                    {
                        ColumnId = db.HolidayLists.Max(m => m.Id);
                    }

                    if (model.IsMultipleDay)
                    {
                        var dates = new List<DateTime>();
                        for (var dt = start; dt <= end; dt = dt.AddDays(1))
                        {
                            dates.Add(dt);
                        }
                        if (db.Multiple_HolidayLists.Where(m => m.ParentId == ColumnId).Any())
                        {
                            var alreadyExsitsDate = db.Multiple_HolidayLists.Where(m => m.ParentId == ColumnId).ToList();
                            foreach (var AddedDate in alreadyExsitsDate)
                            {
                                db.Multiple_HolidayLists.Remove(AddedDate);
                                db.SaveChanges();
                            }
                        }
                        foreach (var d in dates)
                        {
                            Multiple_HolidayLists multiple = new Multiple_HolidayLists();
                            multiple.ParentId = ColumnId;
                            multiple.Date = d.Date;
                            multiple.MonthName = Convert.ToDateTime(d.Date).ToString("MMMM");
                            multiple.MonthCount = Convert.ToInt16(Convert.ToDateTime(model.Date).ToString("MM"));
                            db.Multiple_HolidayLists.Add(multiple);
                            db.SaveChanges();
                        }
                    }
                }
                catch (Exception ex)
                {
                    string errorMsg = ex.Message.ToString();
                    if (model.Id > 0)
                    {
                        msg = model.HolidayName + " holiday information update was  unsuccessful on " + now.ToString("MMM dd, yyyy hh: mm tt");
                    }
                    else
                    {
                        msg = "New holiday '" + model.HolidayName + "' creation was unsuccessful on " + now.ToString("MMM dd, yyyy hh: mm tt");
                    }
                    OperationStatus = -1;
                }
                SaveAuditLog(opName, msg, "HR", "Holiday Create", "HolidayList", ColumnId, model.CreatedBy, now, OperationStatus);
                return Json("Success", JsonRequestBehavior.AllowGet);
            }
            return Json("Error", JsonRequestBehavior.AllowGet);
        }
        public JsonResult DeleteHoliday(int? id, long UserId, string name, int packId)
        {
            if (id != null)
            {
                string msg = "";
                int ColumnId = 0;
                int OperationStatus = 1;
                HolidayList holiday = db.HolidayLists.Find(id);
                if (holiday != null)
                {
                    ColumnId = holiday.Id;
                    db.HolidayLists.Remove(holiday);
                    if (db.Multiple_HolidayLists.Where(m => m.ParentId == holiday.Id).Any())
                    {
                        var alldate = db.Multiple_HolidayLists.Where(m => m.ParentId == holiday.Id).ToList();
                        foreach (var d in alldate)
                        {
                            db.Multiple_HolidayLists.Remove(d);
                            db.SaveChanges();
                        }
                    }
                }
                try
                {
                    db.SaveChanges();
                    msg = "Holiday '" + name + "' has been successfully deleted on " + now.ToString("MMM dd, yyyy hh: mm tt");
                    OperationStatus = 1;
                }
                catch (Exception)
                {
                    msg = "Holiday '" + name + "' delete was unsuccessful.";
                    OperationStatus = -1;
                }
                SaveAuditLog("Holiday Delete", msg, "HR", "Holiday Package Details", "HolidayLists", ColumnId, UserId, now, OperationStatus);
                if (OperationStatus == 1)
                {
                    return Json(db.HolidayLists.Where(m => m.Date > now && m.HolidayPackId == packId).Any(), JsonRequestBehavior.AllowGet);
                }
            }
            return Json("Error", JsonRequestBehavior.AllowGet);
        }
        #endregion


        //************************** Assign Holiday Package *********************************************
        #region
        public PartialViewResult AssignHolidayPack(long UserId)
        {
            var model = db.UserInformations.Find(UserId);
            ViewBag.HolidayPackId = new SelectList(db.HolidayPackageLists, "HolidayPackId", "HolidayPackName");
            return PartialView(model);
        }
        public PartialViewResult ChangeHolidayPack(int packId)
        {
            ViewBag.packId = packId;
            return PartialView();
        }
        public PartialViewResult _ChangeHolidayPack(int packId)
        {
            ViewBag.packId = packId;
            ViewBag.HolidayPackId = new SelectList(db.HolidayPackageLists, "HolidayPackId", "HolidayPackName", packId);
            return PartialView();
        }
        public PartialViewResult selectHolyPack(int id, string v)
        {
            var model = db.View_HolidayPackage.Where(m => m.HolidayPackId == id).FirstOrDefault();
            ViewBag.v = v;
            return PartialView(model);
        }
        public JsonResult AssignHolidayPackWithUser(int HolidayPackId, long UserId, long AssignedBy)
        {
            if (HolidayPackId > 0)
            {
                int status = -1;
                string msg = "";
                int columnId = 0;
                string opName = "Assign Holiday Package";
                string Username = db.View_UserLists.Where(m => m.UserId == UserId).Select(m => m.UserName).FirstOrDefault();
                string Packname = db.HolidayPackageLists.Where(m => m.HolidayPackId == HolidayPackId).Select(m => m.HolidayPackName).FirstOrDefault();
                UserInformation user = db.UserInformations.Find(UserId);
                user.HolidayPackId = HolidayPackId;
                user.IsCustomePack = false;
                user.HolidayAssignedBy = AssignedBy;
                user.HolidayAssignedDate = now;
                db.Entry(user).State = EntityState.Modified;
                try
                {
                    db.SaveChanges();
                    columnId = HolidayPackId;
                    status = 1;
                    msg = "Holiday package '" + Packname + "' has been successfully assigned with employee '" + Username + "' on " + now.ToString("MMM dd, yyyy hh:mm tt") + " .";
                }
                catch (Exception)
                {
                    columnId = HolidayPackId;
                    status = -1;
                    msg = "Holiday package '" + Packname + "' assign with employee '" + Username + "' was unsuccessful on " + now.ToString("MMM dd, yyyy hh:mm tt") + " .";
                }
                SaveAuditLog(opName, msg, "HR", "User Information", "UserInformation", columnId, AssignedBy, now, status);
                if (status == 1)
                {
                    return Json("Success", JsonRequestBehavior.AllowGet);
                }
            }
            return Json("Error", JsonRequestBehavior.AllowGet);
        }
        #endregion

       

        //************************** Added by Kaikubad on 23 - 01 - 2017 *****************************
        # region Add by Kaikubad
        public PartialViewResult EditaHoliday(int HolidayPackId, string view, string from)
        {
            ViewBag.HolidayPackId = HolidayPackId;
            ViewBag.view = view;
            ViewBag.from = from;
            return PartialView();
        }
        public PartialViewResult _EditHolyPackWin(int HolidayPackId, string view, string from)
        {
            var data = db.View_HolidayPackage.Where(x => x.HolidayPackId == HolidayPackId).FirstOrDefault();
            HolidayPackageListModelView package = new HolidayPackageListModelView();
            package.HolidayPackId = data.HolidayPackId;
            package.HolidayPackName = data.HolidayPackName;
            package.NoOfPaidLeave = data.NoOfPaidLeave;
            ViewBag.view = view;
            ViewBag.from = from;
            return PartialView(package);
        }
        public JsonResult IsEligibleforDelete(int id)
        {
            var ret = false;
            if (db.UserInformations.Where(x => x.HolidayPackId == id).Any())
            {
                ret = true;
            }
            return Json(ret, JsonRequestBehavior.AllowGet);
        }
        public JsonResult CopyHoliday(HolidayPackageList holidaypackage, string pageName)
        {
            int status = -1;
            string msg = "";
            string opName = "Duplicate Holiday Package";
            int ColumnId = 0;
            pageName = (pageName == "HolidayPackDetails") ? "Holiday Package Details" : "All Holiday Package List";


            HolidayPackageList copyFrom = db.HolidayPackageLists.Find(holidaypackage.HolidayPackId);
            string copyFromName = copyFrom.HolidayPackName;
            holidaypackage.NoOfPaidLeave = copyFrom.NoOfPaidLeave;
            holidaypackage.HolidayPackId = 0;
            holidaypackage.CreatedDate = now;
            db.HolidayPackageLists.Add(holidaypackage);
            try
            {
                db.SaveChanges();
                ColumnId = db.HolidayPackageLists.Where(x => x.CreatedBy == holidaypackage.CreatedBy).Max(x => x.HolidayPackId);
                msg = "Holiday Package '" + holidaypackage.HolidayPackName + "' has been successfully duplicated from '" + copyFromName + "' on " + now.ToString("MMM dd, yyyy hh:mm tt") + " .";
                status = 1;
                SaveAuditLog(opName, msg, "Holiday", pageName, "HolidayPackageList", ColumnId, holidaypackage.CreatedBy, now, 1);
                foreach (var day in db.HolidayLists.Where(x => x.HolidayPackId == copyFrom.HolidayPackId).ToList())
                {
                    int preid = day.Id;
                    int newColumnId = 0;
                    opName = "Duplicate Holiday List";

                    day.HolidayPackId = ColumnId;
                    day.CreatedBy = holidaypackage.CreatedBy;
                    day.CreatedDate = now;
                    db.HolidayLists.Add(day);
                    try
                    {
                        db.SaveChanges();
                        newColumnId = db.HolidayLists.Where(x => x.CreatedBy == holidaypackage.CreatedBy).Max(x => x.Id);
                        msg = "Holiday '" + day.HolidayName + "' has been successfully duplicated with Holiday Package '" + holidaypackage.HolidayPackName + "' on " + now.ToString("MMM dd, yyyy hh:mm tt") + " .";

                        if (day.IsMultipleDay)
                        {
                            var AllMultiDay = db.Multiple_HolidayLists.Where(m => m.ParentId == preid).ToList();
                            foreach (var mul_day in AllMultiDay)
                            {
                                mul_day.ParentId = newColumnId;
                                db.Multiple_HolidayLists.Add(mul_day);
                                db.SaveChanges();
                            }
                        }
                    }
                    catch (Exception)
                    {
                        status = -1;
                        msg = "Holiday duplication  with Holiday Package '" + holidaypackage.HolidayPackName + "' was unsuccessful on " + now.ToString("MMM dd, yyyy hh:mm tt") + " .";
                    }
                    SaveAuditLog(opName, msg, "Holiday", pageName, "HolidayList", newColumnId, holidaypackage.CreatedBy, now, status);
                }
            }
            catch (Exception)
            {
                status = -1;
                msg = "Holiday Package '" + holidaypackage.HolidayPackName + "'  duplication from '" + copyFromName + "' was unsuccessful on " + now.ToString("MMM dd, yyyy hh:mm tt") + " .";
            }
            SaveAuditLog(opName, msg, "Holiday", pageName, "HolidayPackageList", ColumnId, holidaypackage.CreatedBy, now, status);
            if (status == 1)
            {
                return Json(ColumnId, JsonRequestBehavior.AllowGet);
            }
            return Json("Error", JsonRequestBehavior.DenyGet);
        }
        public JsonResult EditaHolidayPack(HolidayPackageList holidaypackage)
        {
            int status = -1;
            string msg = "";
            string opName = "Edit Holiday Package";
            int ColumnId = 0;
            HolidayPackageList info = db.HolidayPackageLists.Where(x => x.HolidayPackId == holidaypackage.HolidayPackId).FirstOrDefault();
            info.HolidayPackName = holidaypackage.HolidayPackName;
            info.NoOfPaidLeave = holidaypackage.NoOfPaidLeave;
            info.UpdatedBy = holidaypackage.UpdatedBy;
            info.UpdatedDate = now;
            db.Entry(info).State = EntityState.Modified;
            try
            {
                db.SaveChanges();
                status = 1;
                msg = "Holiday Package '" + holidaypackage.HolidayPackName + "' has been successfully updated on " + now.ToString("MMM dd, yyyy hh:mm tt") + " .";
                ColumnId = holidaypackage.HolidayPackId;
            }
            catch (Exception)
            {
                status = -1;
                msg = "Edit Holiday Package '" + holidaypackage.HolidayPackName + "' was unsuccessfull on " + now.ToString("MMM dd, yyyy hh:mm tt") + " .";
            }
            SaveAuditLog(opName, msg, "Holiday", "EditHoliday", "HolidayPackageList", ColumnId, (long)holidaypackage.UpdatedBy, now, status);
            if (status == 1)
            {
                return Json("Success", JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json("Error", JsonRequestBehavior.DenyGet);
            }
        }
        public JsonResult ChangeHolidayPackWithUser(int HolidayPackId, long UserId, long AssignedBy)
        {
            if (HolidayPackId > 0 && UserId > 0)
            {
                int status = -1;
                string msg = "";

                string opName = "Change Holiday Package";
                string Username = db.View_UserLists.Where(m => m.UserId == UserId).Select(m => m.UserName).FirstOrDefault();
                string Packname = db.HolidayPackageLists.Where(m => m.HolidayPackId == HolidayPackId).Select(m => m.HolidayPackName).FirstOrDefault();
                UserInformation user = db.UserInformations.Find(UserId);
                user.HolidayPackId = HolidayPackId;
                user.IsCustomePack = false;
                user.HolidayAssignedBy = AssignedBy;
                user.HolidayAssignedDate = now;
                db.Entry(user).State = EntityState.Modified;
                try
                {
                    db.SaveChanges();
                    status = 1;
                    msg = "Holiday package '" + Packname + "' has been successfully changed with employee '" + Username + "' on " + now.ToString("MMM dd, yyyy hh:mm tt") + " .";
                }
                catch (Exception)
                {
                    status = -1;
                    msg = "Holiday package '" + Packname + "' change with employee '" + Username + "' was unsuccessful on " + now.ToString("MMM dd, yyyy hh:mm tt") + " .";
                }
                SaveAuditLog(opName, msg, "HR", "User Information", "UserInformation", HolidayPackId, AssignedBy, now, status);
                if (status == 1)
                {
                    return Json("Success", JsonRequestBehavior.AllowGet);
                }
            }
            return Json("Error", JsonRequestBehavior.DenyGet);
        }
        public JsonResult DeleteHolidayPack(int id, long UserId, string name)
        {
            int status = -1;
            string msg = "";
            string opName = "Remove Holiday Package";
            int ColumnId = 0;
            var holiday = db.HolidayPackageLists.Where(x => x.HolidayPackId == id).FirstOrDefault();
            if (holiday != null)
            {
                if (!db.UserInformations.Where(x => x.HolidayPackId == id).Any())
                {
                    db.HolidayPackageLists.Remove(holiday);
                    try
                    {
                        db.SaveChanges();
                        status = 1;
                        ColumnId = id;
                        msg = "Hliday package '" + name + "' has been succesfully deleted on " + now.ToString("MMM dd, yyyy hh:mm tt") + " .";
                        SaveAuditLog(opName, msg, "Holiday", "HolidayPackage", "HolidayPackageList", ColumnId, UserId, now, status);
                        var dayList = db.HolidayLists.Where(x => x.HolidayPackId == id).ToList();
                        foreach (var day in dayList)
                        {
                            opName = "Delete Holiday From Package";
                            if (day.IsMultipleDay)
                            {
                                var AllMultiDay = db.Multiple_HolidayLists.Where(m => m.ParentId == day.Id).ToList();
                                foreach (var mul_day in AllMultiDay)
                                {
                                    db.Multiple_HolidayLists.Remove(mul_day);
                                    db.SaveChanges();
                                }
                            }
                            db.HolidayLists.Remove(day);
                            try
                            {
                                db.SaveChanges();
                                ColumnId = day.Id;
                                msg = "Holiday '" + day.HolidayName + "' has been successfully deleted on " + now.ToString("MMM dd, yyyy hh:mm tt") + " .";
                            }
                            catch (Exception)
                            {
                                msg = "Holiday '" + day.HolidayName + "' delete was unuccessful on " + now.ToString("MMM dd, yyyy hh:mm tt") + " .";
                            }
                            SaveAuditLog(opName, msg, "Holiday", "All Holiday Package List", "HolidayList", ColumnId, UserId, now, status);
                        }
                    }
                    catch (Exception)
                    {
                        status = -1;
                        ColumnId = 0;
                        msg = "Delete holiday package '" + name + "' was unsuccesful on " + now.ToString("MMM dd, yyyy hh:mm tt") + " .";
                        SaveAuditLog(opName, msg, "Holiday", "All Holiday Package List", "HolidayPackageList", ColumnId, UserId, now, status);
                    }
                    return Json("Success", JsonRequestBehavior.AllowGet);
                }
            }
            return Json("Error", JsonRequestBehavior.DenyGet);
        }
        #endregion





        //************************ Aduit Log ************************************************************
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
            dbaud.AuditInformations.Add(audit);
            dbaud.SaveChanges();
        }
        #endregion


    }
}