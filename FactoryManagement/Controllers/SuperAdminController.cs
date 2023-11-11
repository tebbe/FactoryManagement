using FactoryManagement.Models;
using FactoryManagement.ModelView.Configuration;
using FactoryManagement.ModelView.HR;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Configuration;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace FactoryManagement.Controllers
{
    [Authorize]
    [DisplayName("Audit Logs")]
    public class SuperAdminController : Controller
    {
        #region Private Properties
        private FactoryManagementEntities db = new FactoryManagementEntities();
        int ShowItemPerPage = 50;
        static private int offset = Convert.ToInt32(ConfigurationManager.AppSettings["localTime"]);
        DateTime now = DateTime.UtcNow.AddMinutes(offset);
        #endregion

        public ActionResult AuditLog(bool IsOwn)
        {
            if (IsOwn)
            {
                ViewBag.TableName = new SelectList(db.View_Audit_Information.GroupBy(m => m.TableName)
                                                    .Select(t => new
                                                    {
                                                        Id = t.Select(w => w.TableName).FirstOrDefault(),
                                                        TableName = t.Select(w => w.TableName).FirstOrDefault()
                                                    }).OrderBy(o => o.TableName), "Id", "TableName");

                ViewBag.ModuleName = new SelectList(db.View_Audit_Information.GroupBy(m => m.ModuleName)
                                                    .Select(t => new
                                                    {
                                                        Id = t.Select(w => w.ModuleName).FirstOrDefault(),
                                                        ModuleName = t.Select(w => w.ModuleName).FirstOrDefault()
                                                    }).OrderBy(o => o.ModuleName), "Id", "ModuleName");
            }
            else
            {
                ViewBag.TableName = new SelectList(db.View_Audit_Information.GroupBy(m => m.TableName)
                                                    .Select(t => new
                                                    {
                                                        Id = t.Select(w => w.TableName).FirstOrDefault(),
                                                        TableName = t.Select(w => w.TableName).FirstOrDefault()
                                                    }).OrderBy(o => o.TableName), "Id", "TableName");
                ViewBag.ModuleName = new SelectList(db.View_Audit_Information.GroupBy(m => m.ModuleName)
                                                   .Select(t => new
                                                   {
                                                       Id = t.Select(w => w.ModuleName).FirstOrDefault(),
                                                       ModuleName = t.Select(w => w.ModuleName).FirstOrDefault()
                                                   }).OrderBy(o => o.ModuleName), "Id", "ModuleName");
            }
            ViewBag.IsOwn = IsOwn;
            return View();
        }
        public PartialViewResult _AuditLog(int? days, bool IsOwn, long userId)
        {
            var allAudit = db.View_Audit_Information.OrderByDescending(m => m.OperationStatus).ThenByDescending(m=>m.Date).ToList();
            if (IsOwn)
            {
                allAudit = allAudit.Where(m => m.UserId == userId).ToList();
            }
            if (days > 0)
            {
                DateTime countDate = now.Date;
                int day = Convert.ToInt32(days);
                if (day > 1)
                {
                    countDate = countDate.AddDays(-(day));
                    allAudit = allAudit.Where(m => m.Date > countDate).ToList();
                }
            }
            Session["AllAduitList"] = allAudit;
            Session["TotalAduitCount"] = allAudit.Count();
            ViewBag.Count = allAudit.Count();
            ViewBag.list = allAudit.Take(ShowItemPerPage).ToList();
            return PartialView(ViewBag.list);
        }
        public PartialViewResult _AuditLogForSpecificUser(string moduleName, long eventDoneBy)
        {
            var allAudit = db.View_Audit_Information.Where(m => m.UserId == eventDoneBy).OrderByDescending(m => m.Date).ToList();
            if (moduleName != "")
            {
                allAudit = allAudit.Where(m => m.ModuleName == moduleName).ToList();
            }
            Session["AllAduitList"] = allAudit;
            Session["TotalAduitCount"] = allAudit.Count();
            ViewBag.Count = allAudit.Count();
            ViewBag.list = allAudit.Take(ShowItemPerPage).ToList();
            return PartialView("_AuditLog", ViewBag.list);
        }
        public PartialViewResult _AuditLogStatus(int status, long? eventDoneBy,string moduleName, long userId, bool IsOwn)
        {
            if (status == 1)
            {
                var list = db.View_Audit_Information.OrderByDescending(m => m.OperationStatus).ThenByDescending(m => m.Date).ToList();
                Session["AduitListModule"] = list;
            }
            else
            {
                var list = db.View_Audit_Information.OrderBy(m => m.OperationStatus).ThenByDescending(m => m.Date).ToList();
                Session["AduitListModule"] = list;
            }
            var allAudit = (List<View_Audit_Information>)Session["AduitListModule"];
            if (eventDoneBy != null)
            {
                allAudit = db.View_Audit_Information.Where(m => m.UserId == eventDoneBy).OrderByDescending(m => m.OperationStatus).ThenByDescending(m => m.Date).ToList();
                if (moduleName != "")
                {
                    allAudit = allAudit.Where(m => m.ModuleName == moduleName).ToList();
                }                
            }
            else
            {
                if (IsOwn)
                {
                    allAudit = db.View_Audit_Information.Where(m => m.UserId == userId).OrderByDescending(m => m.OperationStatus).ThenByDescending(m => m.Date).ToList();
                    if (moduleName != "")
                    {
                        allAudit = allAudit.Where(m => m.ModuleName == moduleName).ToList();
                    }                  
                }
                else
                {
                    allAudit = db.View_Audit_Information.OrderByDescending(m => m.OperationStatus).ThenByDescending(m => m.Date).ToList();
                    if (moduleName != "")
                    {
                        allAudit = allAudit.Where(m => m.ModuleName == moduleName).ToList();
                    }
                }
            }
            Session["AllAduitList"] = allAudit;
            Session["TotalAduitCount"] = allAudit.Count();
            ViewBag.Count = allAudit.Count();
            ViewBag.list = allAudit.Take(ShowItemPerPage).ToList();
            return PartialView("_AuditLog", ViewBag.list);
        }
        public PartialViewResult _AuditModuleChange(string moduleName,long? eventDoneBy, long userId, bool IsOwn)
        {
            if (moduleName == "")
            {
                var list = db.View_Audit_Information.OrderByDescending(m => m.Date).ToList();
                Session["AduitListModule"] = list;
            }
            else
            {
                var list = db.View_Audit_Information.Where(m => m.ModuleName == moduleName).OrderByDescending(m => m.Date).ToList();
                Session["AduitListModule"] = list;
            }
            var allAudit = (List<View_Audit_Information>)Session["AduitListModule"];
            if (eventDoneBy > -1)
            {
                allAudit = allAudit.Where(m => m.UserId == eventDoneBy).OrderByDescending(m => m.Date).ToList();
                if (moduleName != "")
                {
                    allAudit = allAudit.Where(m => m.ModuleName == moduleName).ToList();
                }
            }
            else
            {
                if (IsOwn)
                {
                    allAudit = allAudit.Where(m => m.UserId == userId).OrderByDescending(m => m.Date).ToList();
                    if (moduleName != "")
                    {
                        allAudit = allAudit.Where(m => m.ModuleName == moduleName).ToList();
                    }
                }
                else
                {
                    allAudit = allAudit.OrderByDescending(m => m.Date).ToList();
                    if (moduleName != "")
                    {
                        allAudit = allAudit.Where(m => m.ModuleName == moduleName).ToList();
                    }
                }
            }
            Session["AllAduitList"] = allAudit;
            Session["TotalAduitCount"] = allAudit.Count();
            ViewBag.Count = allAudit.Count();
            ViewBag.list = allAudit.Take(ShowItemPerPage).ToList();
            return PartialView("_AuditLog", ViewBag.list);
        }
        public PartialViewResult _AuditDayWise(int days,string moduleName, long? eventDoneBy, long userId, bool IsOwn)
        {
            DateTime countDate = DateTime.Today;
            var allAudit = db.View_Audit_Information.OrderByDescending(m => m.Date).ToList();
            int day = Convert.ToInt32(days);
            if (day > 1)
            {
                countDate = countDate.AddDays(-(day));      
            }
            if (day > 0)
            {
                allAudit = allAudit.Where(m => m.Date > countDate).OrderByDescending(m => m.Date).ToList();
            }
            if (eventDoneBy > -1)
            {
                allAudit = allAudit.Where(m => m.UserId == eventDoneBy).OrderByDescending(m => m.Date).ToList();
                if (moduleName != "")
                {
                    allAudit = allAudit.Where(m => m.ModuleName == moduleName).ToList();
                }
            }
            else
            {
                if (IsOwn)
                {
                    if (moduleName != "")
                    {
                        allAudit = allAudit.Where(m => m.ModuleName == moduleName && m.UserId == userId).OrderByDescending(m => m.Date).ToList();
                    }
                    else
                    {
                        allAudit = allAudit.Where(m => m.UserId == userId).OrderByDescending(m => m.Date).ToList();
                    }
                }
                else
                {                    
                    if (moduleName != "")
                    {
                        allAudit = allAudit.Where(m => m.ModuleName == moduleName).OrderByDescending(m => m.Date).ToList();
                    }
                    else
                    {
                        allAudit = allAudit.OrderByDescending(m => m.Date).ToList();
                    }
                }
            }
            Session["AllAduitList"] = allAudit;
            Session["TotalAduitCount"] = allAudit.Count();
            ViewBag.Count = allAudit.Count();
            ViewBag.list = allAudit.Take(ShowItemPerPage).ToList();
            return PartialView("_AuditLog", ViewBag.list);
        }

        public JsonResult GetAuditLog(int? page)
        {
            page = page ?? 0;
            int total = Convert.ToInt16(Session["TotalAduitCount"]);
            int skip = ShowItemPerPage * Convert.ToInt16(page);
            var canPage = skip < total;
            if (canPage)
            {
                var listBackFromSession = (List<View_Audit_Information>)Session["AllAduitList"];
                var partslist = listBackFromSession.Skip(skip).Take(ShowItemPerPage).ToList();
                return Json(partslist, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json("", JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult AuditLogDetails(int? Id)
        {
            if (Id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            else
            {
                return View(db.AuditInformations.Find(Id));
            }
        }
        public JsonResult SearchUserList(string prefix)
        {
            if (prefix == "")
            {
                return Json("", JsonRequestBehavior.AllowGet);
            }
            else
            {
                var allUser = (List<View_Audit_Information>)Session["AllAduitList"];
                var user = (from s in allUser
                            where (s.UserName.ToLower().Contains(prefix.ToLower()))
                            select new UserInformationModelView
                            {
                                UserId = s.UserId,
                                UserName = s.UserName,
                                Picture = s.PictureName
                            }).GroupBy(g => g.UserId).Select(s => s.FirstOrDefault()).OrderBy(m => m.UserName).ToList();

                var list = allUser.Where(m=> m.UserName.ToLower().Contains(prefix.ToLower())).ToList();          
                return Json(list, JsonRequestBehavior.AllowGet);
            }
        }
    }
}