using FactoryManagement.Filters;
using FactoryManagement.Models;
using FactoryManagement.ModelView.Accounting;
using FactoryManagement.ModelView.Configuration;
using FactoryManagement.ModelView.HR;
using FactoryManagement.ModelView.Inventory.Mahinaries;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace FactoryManagement.Controllers
{
    [Authorize]
    [DisplayName("Configurations & Settings")]
    public class ConfigurationController : Controller
    {
        #region Private Properties
        private FactoryManagementEntities db = new FactoryManagementEntities();
        private FactoryManagementEntities dbAud = new FactoryManagementEntities();
        static private int offset = Convert.ToInt32(ConfigurationManager.AppSettings["localTime"]);
        DateTime now = DateTime.UtcNow.AddMinutes(offset);
        #endregion
        //**********************************************************************************************************************
        //************************************** Designation Create,Update,Delete(Start) ***************************************
        //**********************************************************************************************************************
        #region Designation Create,Update,Delete
        
        public ActionResult DesignationList()
        {
            return View();
        }
        public PartialViewResult _ShowAllDesignationList(int? desigId)
        {
            if (desigId != null)
            {
                var searchList = (List<Designation>)Session["AllDesignationsBySearch"];
                var desigBysearch = (from d in searchList
                                     where d.DesignationId == desigId
                                     select new Designation
                                     {
                                         DesignationId = d.DesignationId,
                                         DesignationName = d.DesignationName,
                                         Status = d.Status,
                                         CreatedBy = d.CreatedBy,
                                         CreatedDate = d.CreatedDate,
                                         UpdatedBy = d.UpdatedBy,
                                         UpdatedDate = d.UpdatedDate
                                     }).ToList();
                return PartialView(desigBysearch);
            }
            else
            {
                var list = db.Designations.ToList();
                Session["AllDesignations"] = list;
                return PartialView(list);
            }
        }
        public JsonResult GetDesignationBySearch(string prefix)
        {
            if (prefix == "")
            {
                return Json("", JsonRequestBehavior.AllowGet);
            }
            else
            {
                var searchList = (List<Designation>)Session["AllDesignations"];
                var title = (from d in searchList
                             where (d.DesignationName.ToLower().Contains(prefix.ToLower()))
                             select new Designation
                             {
                                 DesignationId = d.DesignationId,
                                 DesignationName = d.DesignationName,
                                 Status = d.Status,
                                 CreatedBy = d.CreatedBy,
                                 CreatedDate = d.CreatedDate,
                                 UpdatedBy = d.UpdatedBy,
                                 UpdatedDate = d.UpdatedDate
                             }).ToList();
                Session["AllDesignationsBySearch"] = title;
                return Json(title, JsonRequestBehavior.AllowGet);
            }
        }
        public PartialViewResult DesignationCreatePopUp(int? id)
        {
            ViewBag.DesignationId = id;
            return PartialView();
        }
        public PartialViewResult DesignationCreate(int? id)
        {
            DesignationModelView model = new DesignationModelView();
            if (id != null && id != 0)
            {
                Designation desig = db.Designations.Find(id);
                model.DesignationId = desig.DesignationId;
                model.DesignationName = desig.DesignationName;
                model.CreatedBy = desig.CreatedBy;
                model.CreatedDate = desig.CreatedDate;
                model.UpdatedBy = desig.UpdatedBy;
                model.UpdatedDate = desig.UpdatedDate;
                model.Status = desig.Status;
            }
            return PartialView(model);
        }
        public JsonResult DesignationCreateSave(DesignationModelView model)
        {
            if (ModelState.IsValid)
            {
                int DesignationId;
                string msg = "";
                string OperationName = "";
                int ColumnId = 0;
                int OperationStatus = 1;

                Designation desig = new Designation();
                if (model.DesignationId > 0)
                {
                    desig = db.Designations.Find(model.DesignationId);
                    desig.DesignationName = model.DesignationName;
                    desig.Status = model.Status;
                }
                else
                {
                    desig.DesignationName = model.DesignationName;
                    desig.CreatedDate = now;
                    desig.Status = 1;
                    desig.CreatedBy = model.CreatedBy;
                    desig.CreatedDate = now;
                }
                try
                {
                    if (model.DesignationId > 0)
                    {
                        desig.UpdatedDate = now;
                        desig.UpdatedBy = model.CreatedBy;
                        db.Entry(desig).State = EntityState.Modified;
                        db.SaveChanges();
                        DesignationId = model.DesignationId;
                        OperationName = "Designation Info Update";
                        msg = "Designation '" + desig.DesignationName + "' has been successfully updated on " + now.ToString("MMM dd,yyyy hh:mm tt");
                    }
                    else
                    {
                        db.Designations.Add(desig);
                        db.SaveChanges();
                        DesignationId = db.Designations.Where(m => m.CreatedBy == model.CreatedBy).Max(m => m.DesignationId);
                        OperationName = "Create New Designation";
                        msg = "New designation '" + desig.DesignationName + "' has been successfully created on " + now.ToString("MMM dd,yyyy hh:mm tt");
                    }
                    OperationStatus = 1;
                    ColumnId = DesignationId;
                }
                catch (Exception)
                {
                    OperationStatus = -1;
                }
                SaveAuditLog(OperationName, msg, "Configuration", "DesignationList", "Designation", ColumnId, model.CreatedBy, now, OperationStatus);
                if (OperationStatus == 1)
                {
                    return Json("Success", JsonRequestBehavior.AllowGet);
                }
            }
            return Json("Error", JsonRequestBehavior.AllowGet);
        }
        public JsonResult DeleteDesignation(int? DesignationId, string DesignationName, long UserId)
        {
            if (DesignationId == null)
            {
                return Json("Error", JsonRequestBehavior.AllowGet);
            }
            else
            {
                string msg = "", value = "";
                int ColumnId = 0;
                int OperationStatus = 1;

                Designation desig = new Designation();
                if (db.UserInformations.Where(m => m.DesignationId == DesignationId).Any())
                {
                    msg = "Designation '" + DesignationName + "' cannot be deleted, already been used ";
                    OperationStatus = -1;
                    value = msg;
                }
                else
                {
                    desig = db.Designations.Find(DesignationId);
                    OperationStatus = 1;
                }
                try
                {
                    if (OperationStatus == 1)
                    {
                        db.Designations.Remove(desig);
                        db.SaveChanges();
                        msg = "Designation '" + desig.DesignationName + "' has been successfully deleted on " + now.ToString("MMM dd,yyyy hh:mm tt");
                        value = "Success";
                    }
                }
                catch (Exception)
                {
                    OperationStatus = -1;
                    msg = "Designation '" + desig.DesignationName + "' delete has been unsuccessfull on " + now.ToString("MMM dd,yyyy hh:mm tt");
                    value = "Error";
                }
                ColumnId = Convert.ToInt32(DesignationId);
                SaveAuditLog("Designation Delete", msg, "Configuration", "Designation", "Designation", ColumnId, UserId, now, OperationStatus);
                return Json(value, JsonRequestBehavior.AllowGet);
              
            }
        }
        public JsonResult CanDesigStatusUpdate(DesignationModelView model)
        {
            if (model.DesignationId > 0)
            {
                string msg = "";
                int ColumnId = 0;
                int OperationStatus = 1;
                try
                {
                    if (db.UserInformations.Where(m => m.DesignationId == model.DesignationId).Any())
                    {
                        msg = "Designation '" + model.DesignationName + "' status cannot be changed,already been used.";
                        OperationStatus = -1;
                    }
                    else
                    {
                        msg = "Success";
                        OperationStatus = 1;
                    }
                }
                catch (Exception)
                {
                    OperationStatus = -1;
                    msg = "Designation '" + model.DesignationName + "' information update unsuccessfull on " + now.ToString("MMM dd,yyyy hh:mm tt");
                }
                ColumnId = model.DesignationId;
                if (OperationStatus == -1)
                {
                    SaveAuditLog("Designation Info Update", msg, "Configuration", "Designation", "Designation", ColumnId, model.CreatedBy, now, OperationStatus);
                }
                return Json(msg, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json("Error", JsonRequestBehavior.AllowGet);
            }
        }
        #endregion

        //**********************************************************************************************************************
        //************************************** Designation Create,Update,Delete(End) *****************************************
        //**********************************************************************************************************************
      
        //**********************************************************************************************************************
        //************************************** Site Create,Update,Delete(Start) **********************************************
        //**********************************************************************************************************************
        #region Site Create,Update

        public ActionResult AllSiteInfo()
        {
            return View(db.View_SiteLists.ToList());
        }
        [EncryptedActionParameter]
        public ActionResult SiteCreate(int? id)
        {
            SiteListModelView model = new SiteListModelView();
            if (id == null)
            {
                ViewBag.Title = "Add New Site";
            }
            else
            {
                SiteList site = db.SiteLists.Find(id);
                model.Id = site.Id;
                model.SiteName = site.SiteName;
                model.SiteAcronym = site.SiteAcronym;
                model.Address = site.Address;
                model.AddressLine1 = site.AddressLine1;
                model.Country = site.Country;
                model.Statename = site.SiteName;
                model.DivisionId = site.DivisionId;
                model.City = site.City;
                model.Area = site.Area;
                model.PostalCode = site.PostalCode;
                model.IsComposite = site.IsComposite;
                model.UnitNumber = site.UnitNumber;
                model.Status = site.Status;
                ViewBag.Title = "Site Information Update";
            }
            ViewBag.DivisionList = new SelectList(db.DivisionLists, "DivisionId", "DivisionName", model.DivisionId);
            ViewBag.CountryId = new SelectList(db.CountryLists, "CountryCode", "CountryName", model.Country);
            return View(model);
        }
 
        public JsonResult SiteCreateSave(SiteListModelView model)
        {
            if (ModelState.IsValid)

            {
                int siteId;
                string msg = "";
                string OperationName = "";
                string PageName = "";
                int ColumnId = 0;
                int OperationStatus = 1;
                SiteList site = new SiteList();
                if (model.Id > 0)
                {
                    site = db.SiteLists.Find(model.Id);
                    if (model.IsComposite == false && db.Site_Unit_Lists.Where(m => m.SiteId == model.Id).Any())
                    {
                        var pmodel = db.Site_Unit_Lists.Where(m => m.SiteId == model.Id).ToList();
                        foreach (var r in pmodel)
                        {
                            db.Site_Unit_Lists.Remove(r);
                            db.SaveChanges();
                        }
                    }
                    if (model.IsComposite == true && db.DepartmentLists.Where(m => m.SiteId == model.Id).Any())
                    {
                        var qmodel = db.DepartmentLists.Where(m => m.SiteId == model.Id).ToList();
                        foreach (var r in qmodel)
                        {
                            db.DepartmentLists.Remove(r);
                            db.SaveChanges();
                        }
                    }
                }
                site.SiteName = model.SiteName;
                site.SiteAcronym = model.SiteAcronym;
                site.Address = model.Address;
                site.AddressLine1 = model.AddressLine1;
                site.Country = model.Country;
                if (site.Country == "BD")
                {
                    site.DivisionId = model.DivisionId;
                    site.Statename = null;
                }
                else
                {
                    site.Statename = model.Statename;
                    site.DivisionId = null;
                }
                site.City = model.City;
                site.Area = model.Area;
                site.PostalCode = model.PostalCode;
                site.IsResidential = model.IsResidential;
                site.IsComposite = model.IsComposite;
                if (model.Id > 0)
                {
                    site.Status = model.Status;
                    site.UpdatedDate = now;
                    site.UpdatedBy = model.CreatedBy;
                    db.Entry(site).State = EntityState.Modified;
                    OperationName = "Site Info Update";
                    PageName = "Site Information";
                }
                else
                {
                    site.Status = 1;
                    site.CreatedDate = now;
                    site.CreatedBy = model.CreatedBy;
                    db.SiteLists.Add(site);
                    OperationName = "Site Create";
                    PageName = "New Site Create";
                }
                try
                {
                    db.SaveChanges();
                    if (model.Id > 0)
                    {
                        siteId = model.Id;
                        msg = (OperationStatus == 1) ? "Site '" + site.SiteName + "' information has been successfully updated on " + now.ToString("MMM dd,yyyy hh:mm tt") :
                            "Site '" + site.SiteName + "' information update unsuccessful on " + now.ToString("MMM dd,yyyy hh:mm tt");
                    }
                    else
                    {
                        siteId = db.SiteLists.Where(m => m.CreatedBy == model.CreatedBy).Max(m => m.Id);
                        msg = (OperationStatus == 1) ? "New site '" + site.SiteName + "' has been successfully created on " + now.ToString("MMM dd,yyyy hh:mm tt") :
                            "New site '" + site.SiteName + "' creation was unsuccessful.";
                    }
                    ColumnId = siteId;
                }
                catch (Exception)
                {
                    OperationStatus = -1;
                    ColumnId = 0;
                }
                SaveAuditLog(OperationName, msg, "Configuration", PageName, "SiteLists", ColumnId, model.CreatedBy, now, OperationStatus);
                if (OperationStatus == 1)
                {
                    model.Id = ColumnId;
                    return Json(model, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json("Error", JsonRequestBehavior.AllowGet);
                }
            }
            return Json("Error", JsonRequestBehavior.AllowGet);
        }
       
        public JsonResult SiteDelete(int? id, long UserId)
        {
            if (id != null)
            {
                string msg = "";
                int ColumnId = 0;
                int OperationStatus = 1;
                string value = "";
                SiteList site = db.SiteLists.Find(id);
                if (site != null)
                {
                    if (db.View_UserLists.Where(m => m.SiteId == id).Any())
                    {
                        msg = "Site '" + site.SiteName + "' can not be deleted, it's already in use.";
                        value = msg;
                        OperationStatus = -1;
                    }
                    else
                    {
                        msg = "Site '" + site.SiteName + "' has been successfully deleted on " + now.ToString("MMM dd,yyyy hh:mm tt");
                        value = "Success";
                        OperationStatus = 1;
                    }
                    ColumnId = site.Id;
                    try
                    {
                        if (OperationStatus == 1)
                        {
                            db.SiteLists.Remove(site);
                            db.SaveChanges();
                        }
                    }
                    catch (Exception)
                    {
                        OperationStatus = -1;
                        msg = "Site '" + site.SiteName + "' deletion was unsuccessful on " + now.ToString("MMM dd,yyyy hh:mm tt");
                        value = "Error";
                    }
                    SaveAuditLog("Site Delete", msg, "Configuration", "All Site Lists", "SiteLists", ColumnId, UserId, now, OperationStatus);
                    return Json(value, JsonRequestBehavior.AllowGet);
                }
            }
            return Json("Error", JsonRequestBehavior.AllowGet);
        }
 
        public JsonResult CanSiteStatusUpdate(SiteListModelView model)
        {
            if (model.Id > 0)
            {
                string msg = "";
                int ColumnId = 0;
                int OperationStatus = 1;
                try
                {
                    if (db.View_UserLists.Where(m => m.SiteId == model.Id).Any() ||
                         db.DepartmentLists.Where(m => m.SiteId == model.Id).Any() || db.Site_Unit_Lists.Where(m => m.SiteId == model.Id).Any())
                    {
                        msg = "Site '" + model.SiteName + "' status cannot be changed, it's already been used.";
                        OperationStatus = -1;
                    }
                    else
                    {
                        msg = "Success";
                        OperationStatus = 1;
                    }
                }
                catch (Exception)
                {
                    OperationStatus = -1;
                    msg = "Site '" + model.SiteName + "' information update unsuccessfull on " + now.ToString("MMM,dd,yyyy hh:mm tt");
                }
                ColumnId = model.Id;
                if (OperationStatus == -1)
                {
                    SaveAuditLog("Site Info Update", msg, "Configuration", "Site Information", "SiteLists", ColumnId, model.CreatedBy, now, OperationStatus);
                }
                return Json(msg, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json("Error", JsonRequestBehavior.AllowGet);
            }
        }
        #endregion
     
       
        //***********************************************************************************************************************
        //************************************** Site Create,Update,Delete(End) **************************************************
        //************************************************************************************************************************


        //**********************************************************************************************************************
        //************************************** Site Unit Create,Update,Delete(Start) *****************************************
        //**********************************************************************************************************************
        #region Site_Unit Create,Update,Delete
        public PartialViewResult _UnitAddPopUp(int? siteId, int? UnitId)
        {
            Site_Unit_Lists model = new Site_Unit_Lists();
            if (UnitId != null)
            {
                model = db.Site_Unit_Lists.Find(UnitId);
            }
            model.SiteId = Convert.ToInt16(siteId);
            return PartialView(model);
        }
        public PartialViewResult _UnitAdd(int? siteId, int? UnitId)
        {
            Site_Unit_Lists model = new Site_Unit_Lists();
            if (UnitId != null && UnitId != 0)
            {
                model = db.Site_Unit_Lists.Find(UnitId);
            }
            model.SiteId = Convert.ToInt16(siteId);
            return PartialView(model);
        }
        public JsonResult AddMoreUnit(string[] allUnitName, string[] allUnitAcryonm, int siteId, long CreatedBy)
        {
            if (allUnitName.Length > 0 && allUnitAcryonm.Length > 0)
            {
                string msg = "";
                int ColumnId = 0;
                int Status = 1;
                SiteList site = db.SiteLists.Find(siteId);

                for (int i = 0; i < allUnitName.Length; i++)
                {
                    Site_Unit_Lists siteunit = new Site_Unit_Lists();
                    siteunit.SiteId = site.Id;
                    siteunit.UnitName = allUnitName[i];
                    siteunit.UnitAcryonm = allUnitAcryonm[i];
                    siteunit.UnitStatus = 0;
                    siteunit.CreatedBy = CreatedBy;
                    siteunit.CreatedDate = now;
                    db.Site_Unit_Lists.Add(siteunit);
                    db.SaveChanges();
                    try
                    {
                        db.SaveChanges();
                        Status = 1;
                        ColumnId = db.Site_Unit_Lists.Where(m => m.SiteId == site.Id && m.CreatedBy == CreatedBy).Max(m => m.UnitId);
                        msg = " New unit '" + allUnitName[i] + "' has been successfully added to site '" + site.SiteName + "' on " + now.ToString("MMM dd,yyyy hh:mm tt");
                    }
                    catch (Exception)
                    {
                        Status = -1;
                        msg = " New unit '" + allUnitName[i] + "' addition to site '" + site.SiteName + "' was unsuccessful on " + now.ToString("MMM dd,yyyy hh:mm tt");
                    }
                    SaveAuditLog("Add Unit To Site", msg, "Configuration", "Site Information", "Site_Unit_Lists", ColumnId, CreatedBy, now, Status);
                }
                int totalUnit = db.Site_Unit_Lists.Where(m => m.SiteId == site.Id).Count();
                site.UnitNumber = totalUnit;
                db.Entry(site).State = EntityState.Modified;
                db.SaveChanges();

                return Json("Success", JsonRequestBehavior.AllowGet);
            }
            return Json("Error", JsonRequestBehavior.AllowGet);
        }
        public JsonResult CheckUnitNameExists(string UnitName, int SiteId)
        {
            bool isExits = true;
            if (db.Site_Unit_Lists.Where(m => m.SiteId == SiteId && m.UnitName == UnitName).Any())
            {
                isExits = false;
            }
            else
            {
                isExits = true;
            }
            return Json(isExits, JsonRequestBehavior.AllowGet);
        }
     
        [EncryptedActionParameter]
        public ActionResult DefineUnitOfSite(int? SiteId, int? UnitNo, bool isResi, int? page)
        {
            if (SiteId == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            SiteList site = db.SiteLists.Find(SiteId);
            SiteListModelView model = new SiteListModelView();
            model.SiteId = site.Id;
            model.SiteName = site.SiteName;
            model.SiteAcronym = site.SiteAcronym;
            model.IsResidential = isResi;
            if (UnitNo != null)
            {
                model.UnitNumber = UnitNo;
                model.UnitCount = (site.UnitNumber == null) ? 0 : site.UnitNumber;
            }
            else
            {
                model.UnitNumber = site.UnitNumber;
                model.UnitCount = 0;
            }
            ViewBag.page = page;
            return View(model);
        }
        public JsonResult SaveSiteUnitInfo(SiteListModelView model)
        {
            if (model.AllUnitDetails.Length > 0)
            {
                string msg = "";
                int ColumnId = 0;
                int Status = 1;
                SiteList site = db.SiteLists.Find(model.SiteId);
                site.UnitNumber = (db.Site_Unit_Lists.Where(m => m.SiteId == model.SiteId).Any()) ? (site.UnitNumber + model.AllUnitDetails.Length) :
                    model.AllUnitDetails.Length;
                try
                {
                    site.IsComposite = true;
                    site.Status = 1;
                    site.UpdatedBy = model.CreatedBy;
                    site.UpdatedDate = now;
                    db.Entry(site).State = EntityState.Modified;
                    for (var i = 0; i < model.AllUnitDetails.Length; i++)
                    {
                        Site_Unit_Lists siteunit = new Site_Unit_Lists();
                        siteunit.SiteId = model.SiteId;
                        siteunit.UnitName = model.AllUnitDetails[i];
                        siteunit.UnitAcryonm = model.AllUnitAcryonm[i];
                        siteunit.UnitStatus = 0;
                        siteunit.CreatedBy = model.CreatedBy;
                        siteunit.CreatedDate = now;
                        db.Site_Unit_Lists.Add(siteunit);
                        db.SaveChanges();
                    }
                    ColumnId = site.Id;
                    msg = model.AllUnitDetails.Length + " new unit has been successfully added to site '" + site.SiteName + "' on " + now.ToString("MMM dd,yyyy hh:mm tt");
                }
                catch (Exception)
                {
                    Status = -1;
                    msg = "New unit add to site '" + site.SiteName + "' was unsuccessful on " + now.ToString("MMM dd,yyyy hh:mm tt");
                }
                SaveAuditLog("Add Unit To Site", msg, "Configuration", "Define Unit Information", "SiteLists", ColumnId, model.CreatedBy, now, Status);
                if (Status == 1)
                {
                    return Json(site, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json("Error", JsonRequestBehavior.AllowGet);
                }
            }
            return Json("Error", JsonRequestBehavior.AllowGet);
        }
        public PartialViewResult _AddUnitToSitePopUp(int? siteId, bool isresi)
        {
            SiteListModelView model = new SiteListModelView();
            model.SiteId = Convert.ToInt32(siteId);
            model.IsResidential = isresi;
            return PartialView(model);
        }
        public PartialViewResult _AddUnitToSite(int? siteId, bool isresi)
        {
            SiteListModelView model = new SiteListModelView();
            model.SiteId = Convert.ToInt32(siteId);
            model.IsResidential = isresi;
            return PartialView(model);
        }
        public PartialViewResult UnitEditPopUp(int unitId)
        {
            ViewBag.unitId = unitId;
            return PartialView();
        }
        public PartialViewResult LandingPageForUnitEdit(int unitId)
        {
            ViewBag.unitId = unitId;
            return PartialView();
        }
        public PartialViewResult _SiteUnitDisplayEdit(int? unitId)
        {
            DepartmentListModelView model = new DepartmentListModelView();
            if (unitId != null)
            {
                Site_Unit_Lists unitdetail = db.Site_Unit_Lists.Find(unitId);
                model.UnitId = unitdetail.UnitId;
                model.UnitAcryonm = unitdetail.UnitAcryonm;
                model.SiteId = unitdetail.SiteId;
                model.UnitName = unitdetail.UnitName;
                model.Status = unitdetail.UnitStatus;
                model.CreatedDate = unitdetail.CreatedDate;
                model.SiteName = db.SiteLists.Where(x => x.Id == unitdetail.SiteId).Select(x => x.SiteName).FirstOrDefault();
            }
            return PartialView(model);
        }
        public JsonResult CanSiteUnitStatusUpdate(DepartmentListModelView model)
        {
            if (model.UnitId > 0)
            {
                string msg = "";
                int ColumnId = 0;
                int OperationStatus = 1;
                try
                {
                    //db.View_UserLists
                    if (db.DepartmentLists.Where(m => m.UnitId == model.UnitId).Any())
                    {
                        msg = "Site unit '" + model.UnitName + "' status cannot be changed, already been used.";
                        OperationStatus = -1;
                    }
                    else
                    {
                        msg = "Success";
                        OperationStatus = 1;
                    }
                }
                catch (Exception)
                {
                    OperationStatus = -1;
                    msg = "Site unit '" + model.UnitName + "' information update unsuccessfull on " + now.ToString("MMM dd,yyyy hh:mm tt");
                }
                ColumnId = Convert.ToInt32(model.UnitId);
                if (OperationStatus == -1)
                {
                    SaveAuditLog("Site Unit Info Update", msg, "Configuration", "Site Unit Details", "Site_Unit_Lists", ColumnId, model.CreatedBy, now, OperationStatus);
                }
                return Json(msg, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json("Error", JsonRequestBehavior.AllowGet);
            }
        }
        public JsonResult UpdateSiteUnitInfo(DepartmentListModelView model)
        {
            if (model.UnitId > 0)
            {
                Site_Unit_Lists sUnit = db.Site_Unit_Lists.Find(model.UnitId);
                if (sUnit != null && model.SiteName != null)
                {
                    sUnit.UnitName = model.UnitName;
                    sUnit.UnitStatus = model.Status;
                    sUnit.UpdatedBy = model.CreatedBy;
                    sUnit.UnitAcryonm = model.UnitAcryonm;
                    sUnit.UpdatedDate = now;
                    string msg = "Site Unit '" + sUnit.UnitName + "' of site '" + model.SiteName + "' has been successfully updated on " + now.ToString("MMM dd,yyyy hh:mm tt");
                    int ColumnId = sUnit.UnitId;
                    int OperationStatus = 1;
                    try
                    {
                        db.Entry(sUnit).State = EntityState.Modified;
                        db.SaveChanges();
                    }
                    catch (Exception)
                    {
                        OperationStatus = -1;
                        msg = "Site Unit '" + sUnit.UnitName + "' of site '" + model.SiteName + "' update unsuccessful on " + now.ToString("MMM dd,yyyy hh:mm tt");
                    }
                    SaveAuditLog("Site Unit Info Update", msg, "Configuration", "Site Unit Details", "Site_Unit_Lists", ColumnId, model.CreatedBy, now, OperationStatus);
                    if (OperationStatus == 1)
                    {
                        return Json(sUnit, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        return Json("Error", JsonRequestBehavior.AllowGet);
                    }
                }
            }
            return Json("Error", JsonRequestBehavior.AllowGet);
        }
        //public JsonResult ChkUnitCount(int? siteId, long UserId)
        //{
        //    string msg = "";
        //    int OperationStatus = 0, ColumnId = 0;
        //    var list = db.Site_Unit_Lists.Where(m => m.SiteId == siteId).Count();
        //    if (list == 1)
        //    {
        //        var siteInfo = db.SiteLists.Find(siteId);
        //        siteInfo.IsComposite = false;
        //        siteInfo.Status = 0;
        //        db.Entry(siteInfo).State = EntityState.Modified;
        //        try
        //        {
        //            msg = "Site Unit '" + siteInfo.SiteName + "' status has been successfully inactivated on " + now.ToString("MMM dd,yyyy hh:mm tt");
        //            db.SaveChanges();
        //            OperationStatus = 1;
        //        }
        //        catch (Exception)
        //        {
        //            OperationStatus = -1;
        //            msg = "Site Unit status inactivate unsuccessful on " + now.ToString("MMM dd,yyyy hh:mm tt");
        //        }
        //        ColumnId = Convert.ToInt32(siteId);
        //        SaveAuditLog("Site Unit Info Update", msg, "Configuration", "Site Information", "SiteLists", ColumnId, UserId, now, OperationStatus);
        //    }
        //    return Json(list, JsonRequestBehavior.AllowGet);
        //}
        public JsonResult ChkUnitCount(int? siteId, long UserId)
        {
            string msg = "";
            int OperationStatus = 0, ColumnId = 0;
            var list = db.Site_Unit_Lists.Where(m => m.SiteId == siteId).Count();
            if (list == 1)
            {
                var siteInfo = db.SiteLists.Find(siteId);
               // siteInfo.IsComposite = false;
                siteInfo.Status = 0;
                db.Entry(siteInfo).State = EntityState.Modified;
                try
                {
                    msg = "Site Unit '" + siteInfo.SiteName + "' status has been successfully inactivated on " + now.ToString("MMM dd,yyyy hh:mm tt");
                    db.SaveChanges();
                    OperationStatus = 1;
                }
                catch (Exception)
                {
                    OperationStatus = -1;
                    msg = "Site Unit status inactivate unsuccessful on " + now.ToString("MMM dd,yyyy hh:mm tt");
                }
                ColumnId = Convert.ToInt32(siteId);
                SaveAuditLog("Site Unit Info Update", msg, "Configuration", "Site Information", "SiteLists", ColumnId, UserId, now, OperationStatus);
            }
            return Json(list, JsonRequestBehavior.AllowGet);
        }
        public JsonResult SiteUnitDelete(int? id, string siteName, long UserId)
        {
            if (id != null)
            {
                string msg = "";
                int ColumnId = 0;
                int OperationStatus = 1;
                string value = "";
                bool canDeleteDept = true;
                var deptlist = new List<DepartmentList>();
                Site_Unit_Lists sUnit = db.Site_Unit_Lists.Find(id);
                if (sUnit != null)
                {
                    if (db.DepartmentLists.Where(m => m.UnitId == id).Any())
                    {
                        deptlist = db.DepartmentLists.Where(m => m.UnitId == id).ToList();
                        foreach (var dept in deptlist)
                        {
                            if (dept.Status == 0) { canDeleteDept = true; OperationStatus = 1; }
                            else { canDeleteDept = false; OperationStatus = -1; break; }
                        }
                    }
                    try
                    {
                        if (OperationStatus == 1 && canDeleteDept)
                        {
                            foreach (var udept in deptlist)
                            {
                                DepartmentList unit_dept = db.DepartmentLists.Find(udept.DeptId);
                                db.DepartmentLists.Remove(unit_dept);
                            }
                            db.Site_Unit_Lists.Remove(sUnit);
                            SiteList site = db.SiteLists.Find(sUnit.SiteId);
                            site.UnitNumber = site.UnitNumber - 1;
                            site.UpdatedBy = UserId;
                            site.UpdatedDate = now;
                            db.Entry(site).State = EntityState.Modified;
                            db.SaveChanges();
                            msg = sUnit.UnitName + " from site '" + siteName + "' has been successfully deleted on " + now.ToString("MMM dd,yyyy hh:mm tt");
                            value = "Success";
                        }
                        else
                        {
                            msg = sUnit.UnitName + " from site '" + siteName + "' already used,Unit cannot be deleted.";
                            value = msg;
                        }
                        ColumnId = sUnit.UnitId;
                    }
                    catch (Exception)
                    {
                        OperationStatus = -1;
                        msg = sUnit.UnitName + " from site '" + siteName + "' deletion unsuccessful on " + now.ToString("MMM dd,yyyy hh:mm tt");
                        value = "Error";
                    }
                    SaveAuditLog("Site Unit Delete", msg, "Configuration", "ShowSiteInfoWithDeptList", "Site_Unit_Lists", ColumnId, UserId, now, OperationStatus);
                    return Json(value, JsonRequestBehavior.AllowGet);
                }
            }
            return Json("Error", JsonRequestBehavior.AllowGet);
        }
        
        #endregion
        //**********************************************************************************************************************
        //************************************** Site Unit Create,Update,Delete(End) *******************************************
        //**********************************************************************************************************************
     
        //***********************************************************************************************************************
        //************************************** Department Create,Update,Delete(Start) *****************************************
        //***********************************************************************************************************************
        #region Dept Create,Update,Delete
        public PartialViewResult DepartmentCreatePopUp(int? siteId, string siteName, int? UnitId)
        {
            DepartmentListModelView model = new DepartmentListModelView();
            if (siteId != null)
            {
                model.SiteId = Convert.ToInt16(siteId);
                model.SiteName = siteName;
            }
            if (UnitId != null)
            {
                model.UnitId = Convert.ToInt16(UnitId);
                model.UnitName = siteName;
            }
            return PartialView(model);
        }
        public PartialViewResult DepartmentCreate(int? siteId, string siteName, int? unitId)
        {
            DepartmentListModelView model = new DepartmentListModelView();
            if (siteId != null)
            {
                model.SiteId = Convert.ToInt16(siteId);
                model.SiteName = siteName;
            }
            if (unitId != null)
            {
                model.UnitId = Convert.ToInt16(unitId);
                model.UnitName = siteName;
            }
            return PartialView(model);
        }
        public JsonResult DepartmentCreateSave(DepartmentListModelView model)
        {
            string msg = "";
            string OperationName = "";
            string PageName = "";
            int ColumnId = 0;
            int OperationStatus = -1;

            DepartmentList dept = new DepartmentList();
            if (model.DeptId > 0)
            {
                OperationName = "Department Info Update";
                PageName = "Department Details";
                dept = db.DepartmentLists.Find(model.DeptId);
                dept.DeptName = model.DeptName;
                dept.DeptAcronym = model.DeptAcronym;
                dept.Status = model.Status;
                dept.UpdatedBy = model.CreatedBy;
                dept.UpdatedDate = now;
                db.Entry(dept).State = EntityState.Modified;
                try
                {
                    db.SaveChanges();
                    OperationStatus = 1;
                    ColumnId = model.DeptId;
                    msg = (dept.SiteId != null) ? "Department '" + model.DeptName + "' of site '" + model.SiteName + "' information has been succesfully updated on " + now.ToString("MMM dd,yyyy hh:mm tt") : "Department '" + model.DeptName + "' of unit '" + model.UnitName + "' information has been succesfully created on " + now.ToString("MMM dd,yyyy hh:mm tt");
                }
                catch (Exception)
                {
                    ColumnId = 0;
                    OperationStatus = -1;
                    msg = (dept.SiteId != null) ? "Department '" + model.DeptName + "' of site '" + model.SiteName + "' information update was unsuccessful on " + now.ToString("MMM dd,yyyy hh:mm tt") : "Department '" + model.DeptName + "' of unit '" + model.UnitName + "' information updated was unsuccessful on " + now.ToString("MMM dd,yyyy hh:mm tt");
                }
            }
            else
            {
                OperationName = "New Department Create";
                dept.DeptName = model.DeptName;
                dept.DeptAcronym = model.DeptAcronym;
                if (model.UnitId > 0)
                {
                    dept.UnitId = model.UnitId;
                    if (db.Site_Unit_Lists.Any(x => x.UnitId == model.UnitId && x.UnitStatus == 0))
                    {
                        var updateUnit = db.Site_Unit_Lists.Find(model.UnitId);
                        updateUnit.UnitStatus = 1;
                        db.Entry(updateUnit).State = EntityState.Modified;
                    }
                }
                else
                {
                    dept.SiteId = model.SiteId;
                    if (db.SiteLists.Any(x => x.Id == model.SiteId && x.Status == 0))
                    {
                        var updateSite = db.SiteLists.Find(model.SiteId);
                        updateSite.Status = 1;
                    }
                }
                dept.CreatedBy = model.CreatedBy;
                dept.CreatedDate = now;
                dept.Status = 0;
                db.DepartmentLists.Add(dept);
                try
                {
                    db.SaveChanges();
                    OperationStatus = 1;
                    ColumnId = db.DepartmentLists.Where(m => m.CreatedBy == model.CreatedBy).Max(m => m.DeptId);
                    msg = (dept.SiteId != null) ? "New department '" + model.DeptName + "' of site '" + model.SiteName + "' has been successfully created on " + now.ToString("MMM,dd,yyyy hh:mm tt") : "New department '" + model.DeptName + "' of unit '" + model.UnitName + "' has been successfully created on " + now.ToString("MMM,dd,yyyy hh:mm tt");
                    if (model.UnitId != null)
                    {
                        PageName = "Site Unit Details";
                        dept.UnitId = model.UnitId;
                        Site_Unit_Lists sUnit = db.Site_Unit_Lists.Find(model.UnitId);
                        sUnit.UnitStatus = 1;
                        db.Entry(sUnit).State = EntityState.Modified;
                    }
                    else if (model.SiteId != null)
                    {
                        PageName = "Site Information Details";
                        dept.SiteId = model.SiteId;
                        SiteList up_site = db.SiteLists.Find(model.SiteId);
                        up_site.Status = 1;
                        db.Entry(up_site).State = EntityState.Modified;
                    }
                }
                catch (Exception)
                {
                    OperationStatus = -1;
                    ColumnId = 0;
                    OperationStatus = -1;
                    msg = (dept.SiteId != null) ? "New department '" + model.DeptName + "' of site '" + model.SiteName + "' create was unsuccessfull on " + now.ToString("MMM,dd,yyyy hh:mm tt") :
                            "New department '" + model.DeptName + "' of unit '" + model.UnitName + "' create unsuccessfull on " + now.ToString("MMM,dd,yyyy hh:mm tt");
                }
            }
            SaveAuditLog(OperationName, msg, "Configuration", PageName, "DepartmentLists", ColumnId, model.CreatedBy, now, OperationStatus);
            if (OperationStatus == 1)
            {
                model.DeptId = ColumnId;
                return Json(model, JsonRequestBehavior.AllowGet);
            }
            return Json("Error", JsonRequestBehavior.AllowGet);
        }
        public JsonResult ChkDeptCount(int? siteId, int? unitId, long UserId)
        {
            string msg = "", name = "", OperationName = "", PageName = "", TableName = "";
            int OperationStatus = 0, ColumnId = 0;
            var list = 0;
            if (siteId != null)
            {
                list = db.DepartmentLists.Where(m => m.SiteId == siteId).Count();
                if (list == 1)
                {
                    var siteInfo = db.SiteLists.Find(siteId);
                    siteInfo.Status = 0;
                    siteInfo.UpdatedBy = UserId;
                    siteInfo.UpdatedDate = now;
                    db.Entry(siteInfo).State = EntityState.Modified;
                    name = siteInfo.SiteName;
                    OperationName = "Site Info Update";
                    PageName = "Site Information";
                    TableName = "SiteLists";
                    ColumnId = siteInfo.Id;
                }
            }
            else
            {
                list = db.DepartmentLists.Where(m => m.UnitId == unitId).Count();
                if (list == 1)
                {
                    var siteUnitInfo = db.Site_Unit_Lists.Find(unitId);
                    siteUnitInfo.UnitStatus = 0;
                    siteUnitInfo.UpdatedBy = UserId;
                    siteUnitInfo.UpdatedDate = now;
                    db.Entry(siteUnitInfo).State = EntityState.Modified;
                    name = siteUnitInfo.UnitName;
                    OperationName = "Site Unit Info Update";
                    PageName = "Site Unit Details";
                    TableName = "Site_Unit_Lists";
                    ColumnId = siteUnitInfo.UnitId;
                }
            }
            try
            {
                db.SaveChanges();
                msg = (siteId != null) ? "Site '" + name + "' status has been successfully inactivated on " + now.ToString("MMM dd,yyyy hh:mm tt") : "Site Unit '" + name + "' status has been successfully inactivated on " + now.ToString("MMM,dd,yyyy hh:mm tt");
                OperationStatus = 1;
            }
            catch (Exception)
            {
                msg = (siteId != null) ? "Site '" + name + "' status inactivate unsuccessful on " + now.ToString("MMM dd,yyyy hh:mm tt") : "Site Unit '" + name + "' status inactivate unsuccessfull on " + now.ToString("MMM,dd,yyyy hh:mm tt");
            }
            if (list == 1 && OperationStatus == 1)
            {
                SaveAuditLog(OperationName, msg, "Configuration", PageName, TableName, ColumnId, UserId, now, OperationStatus);
            }
            return Json(list, JsonRequestBehavior.AllowGet);
        }
        public JsonResult DeptDelete(int? id, string siteName, long UserId, string unitName)
        {
            if (id != null)
            {
                string msg = "";
                int ColumnId = 0;
                int OperationStatus = 1;
                string value = "", PageName = (siteName != null) ? "Site Information" : "Site Unit Details";

                DepartmentList dept = db.DepartmentLists.Find(id);
                if (dept != null)
                {
                    if (db.View_UserLists.Where(m => m.DeptId == id).Any() || (db.LineInfoes.Where(m => m.DeptId == id && m.Status == 1).Count()) > 0)
                    {
                        msg = (siteName != null) ? "Department '" + dept.DeptName + "' from site '" + siteName + "' has already been used on " + now.ToString("MMM dd,yyyy hh:mm tt") :
                            "Department '" + dept.DeptName + "' from unit '" + unitName + "' has already been used on " + now.ToString("MMM dd,yyyy hh:mm tt");
                        OperationStatus = -1;
                        value = msg;
                    }
                    else
                    {
                        if (dept.NumberOfLine > 0)
                        {
                            var linelist = db.LineInfoes.Where(m => m.DeptId == dept.DeptId).ToList();
                            string linename = "";
                            foreach (var lines in linelist)
                            {
                                linename = lines.LineAcronym;
                                LineInfo deptLine = db.LineInfoes.Find(lines.LineId);
                                db.LineInfoes.Remove(deptLine);
                                db.SaveChanges();
                            }
                            msg = "Line '" + linename + "' from department '" + dept.DeptName + "' has been successfully deleted on " + now.ToString("MMM dd,yyyy hh:mm tt");
                            SaveAuditLog("Line Delete", msg, "Configuration", PageName, "LineInfo", ColumnId, UserId, now, 1);
                        }
                        if (dept.UnitId > 0)
                        {
                            if (db.DepartmentLists.Where(x => x.UnitId == dept.UnitId).Count() == 1)
                            {
                                var update = db.Site_Unit_Lists.Find(dept.UnitId);
                                update.UnitStatus = 0;
                                db.Entry(update).State = EntityState.Modified;
                            }
                        }
                        db.DepartmentLists.Remove(dept);
                        try
                        {
                            db.SaveChanges();
                            ColumnId = dept.DeptId;
                            OperationStatus = 1;
                            msg = (siteName != null) ? "Department '" + dept.DeptName + "' from site '" + siteName + "' has been successfully deleted on " + now.ToString("MMM dd,yyyy hh:mm tt") :
                          "Department '" + dept.DeptName + "' from unit '" + unitName + "' has been successfully deleted on " + now.ToString("MMM dd,yyyy hh:mm tt");
                            value = "Success";
                        }
                        catch (Exception)
                        {
                            OperationStatus = -1;
                            msg = (siteName != null) ? "Dept " + dept.DeptName + " from site " + siteName + " deletion unsuccessful on " + now.ToString("MMM dd,yyyy hh:mm tt") :
                                "Dept " + dept.DeptName + " from unit " + unitName + " deletion unsuccessful on " + now.ToString("MMM dd,yyyy hh:mm tt");
                            value = "Error";
                        }
                        SaveAuditLog("Department Delete", msg, "Configuration", PageName, "DepartmentList", ColumnId, UserId, now, OperationStatus);
                    }

                    return Json(value, JsonRequestBehavior.AllowGet);
                }
            }
            return Json("Error", JsonRequestBehavior.AllowGet);
        }
        public JsonResult CanDeptStatusUpdate(DepartmentListModelView model)
        {
            if (model.DeptId > 0)
            {
                string msg = "";
                int ColumnId = 0;
                int OperationStatus = 1;
                try
                {
                    if (db.View_UserLists.Where(m => m.DeptId == model.DeptId).Any() || db.DepartmentLists.Find(model.DeptId).NumberOfLine > 0)
                    {
                        msg = "Department '" + model.DeptName + "' status cannot be changed,already been used.";
                        OperationStatus = -1;
                    }
                    else
                    {
                        msg = "Success";
                        OperationStatus = 1;
                    }
                }
                catch (Exception)
                {
                    OperationStatus = -1;
                    msg = "Department " + model.DeptName + " information update unsuccessfull on " + now.ToString("MMM,dd,yyyy hh:mm tt");
                }
                ColumnId = model.DeptId;
                if (OperationStatus == -1)
                {
                    SaveAuditLog("Dept Info Update", msg, "Configuration", "Department Details", "DepartmentLists", ColumnId, model.CreatedBy, now, OperationStatus);
                }
                return Json(msg, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json("Error", JsonRequestBehavior.AllowGet);
            }
        }
        #endregion
        //**********************************************************************************************************************
        //************************************** Department Create,Update,Delete(End) *******************************************
        //**********************************************************************************************************************
       
        //**********************************************************************************************************************
        //******************************************  Line Create,Update,Delete(Start) *****************************************
        //**********************************************************************************************************************
        #region Dept_Line Create,Update,Delete
        public ActionResult DefineLine(int? deptId, int? lineNo)
        {
            if (deptId == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            DepartmentList dept = db.DepartmentLists.Find(deptId);
            LineInfoModelView model = new LineInfoModelView();
            model.DeptId = dept.DeptId;
            model.DeptName = dept.DeptName;
            model.DeptAcronym = dept.DeptAcronym;
            if (dept.SiteId != null)
            {
                model.SiteId = dept.SiteId;
            }
            else if (dept.UnitId != null)
            {
                model.UnitId = dept.UnitId;
            }
            if (lineNo != null)
            {
                model.NumberOfLine = lineNo;
                model.LineCount = (dept.NumberOfLine == null) ? 0 : dept.NumberOfLine;
            }
            else
            {
                model.NumberOfLine = dept.NumberOfLine;
                model.LineCount = 0;
            }
            ViewBag.Title = "Define Line Information";
            return View(model);
        }
      
        public JsonResult SaveLineInfo(LineInfoModelView model, string[] AllLineNumber, string[] AllLineName)
        {
            if (model.AllLineDetails != "")
            {
                string msg = "";
                int ColumnId = 0;
                int count = 0;
                DepartmentList dept = db.DepartmentLists.Where(m => m.DeptId == model.DeptId).FirstOrDefault();
                if (dept.SiteId != null)
                {
                    model.SiteName = db.SiteLists.Find(dept.SiteId).SiteName;
                }
                else
                {
                    model.UnitName = db.Site_Unit_Lists.Find(dept.UnitId).UnitName;
                }
                dept.Status = 1;
                dept.UpdatedBy = model.CreatedBy;
                dept.UpdatedDate = now;
                for (int i = 0; i < AllLineNumber.Length; i++)
                {
                    int status = -1;
                    LineInfo line = new LineInfo();
                    line.DeptId = model.DeptId;
                    line.LineName = AllLineNumber[i];
                    line.LineAcronym = AllLineName[i];
                    line.NumberOfMachine = 0;
                    line.Status = 0;
                    line.CreatedDate = now;
                    line.CreatedBy = model.CreatedBy;
                    db.LineInfoes.Add(line);
                    try
                    {
                        db.SaveChanges();
                        status = 1;
                        count++;
                        if (dept.SiteId != null)
                        {
                            msg = "New Line '" + line.LineAcronym + "' has been successfully added to department '" + dept.DeptName + "' of site '" + model.SiteName + "' on " + now.ToString("MMM dd,yyyy hh:mm tt");
                        }
                        else
                        {
                            msg = "New Line '" + line.LineAcronym + "' has been successfully added to department '" + dept.DeptName + "' of unit '" + model.UnitName + "' on " + now.ToString("MMM dd,yyyy hh:mm tt");
                        }
                    }
                    catch (Exception)
                    {
                        status = -1;
                        if (dept.SiteId != null)
                        {
                            msg = "New Line '" + line.LineAcronym + "' addition to department '" + dept.DeptName + "' of site '" + model.SiteName + "' was unsuccessful on " + now.ToString("MMM dd,yyyy hh:mm tt");
                        }
                        else
                        {
                            msg = "New Line '" + line.LineAcronym + "' addition to department '" + dept.DeptName + "' of unit '" + model.UnitName + "' was unsuccessful on " + now.ToString("MMM dd,yyyy hh:mm tt");
                        }
                    }
                    SaveAuditLog("Add Line To Department", msg, "Configuration", "Define Line Information", "LineInfo", line.LineId, model.CreatedBy, now, status);
                }
                dept.NumberOfLine = Convert.ToInt32(dept.NumberOfLine) + count;
                db.Entry(dept).State = EntityState.Modified;
                db.SaveChanges();
                ColumnId = dept.DeptId;
                if (dept.SiteId != null)
                {
                    msg = count + " new line has been successfully added to department '" + dept.DeptName + "' of site '" + model.SiteName + "' on " + now.ToString("MMM dd,yyyy hh:mm tt");
                }
                else
                {
                    msg = count + " new line has been successfully added to department '" + dept.DeptName + "' of unit '" + model.UnitName + "' on " + now.ToString("MMM dd,yyyy hh:mm tt");
                }
                SaveAuditLog("Add Line To Department", msg, "Configuration", "Define Line Information", "DepartmentList", dept.DeptId, model.CreatedBy, now, 1);
                return Json(dept, JsonRequestBehavior.AllowGet);
            }
            return Json("Error", JsonRequestBehavior.AllowGet);
        }
        public JsonResult UpdateLineInfo(LineInfoModelView linemodel)
        {
            if (linemodel.LineId > 0)
            {
                LineInfo line = db.LineInfoes.Find(linemodel.LineId);
                if (line != null && linemodel.DeptName != null)
                {
                    line.LineName = linemodel.LineName;
                    line.LineAcronym = linemodel.LineAcronym;
                    line.UpdatedBy = linemodel.CreatedBy;
                    line.UpdatedDate = now;
                    string msg = "Line '" + line.LineName + "' from department '" + linemodel.DeptName + "' of site '" + linemodel.SiteName + "' has been successfully updated on " + now.ToString("MMM dd,yyyy hh:mm tt");
                    int ColumnId = line.LineId;
                    int OperationStatus = 1;
                    try
                    {
                        db.Entry(line).State = EntityState.Modified;
                        db.SaveChanges();
                    }
                    catch (Exception)
                    {
                        OperationStatus = -1;
                        msg = "Line '" + line.LineName + "' from department '" + linemodel.DeptName + "' of site '" + linemodel.SiteName + "' update was unsuccessful on " + now.ToString("MMM dd,yyyy hh:mm tt");
                    }
                    SaveAuditLog("Line Info Update", msg, "Configuration", "Deptartment Details", "LineInfoes", ColumnId, linemodel.CreatedBy, now, OperationStatus);
                    if (OperationStatus == 1)
                    {
                        return Json("Success", JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        return Json("Error", JsonRequestBehavior.AllowGet);
                    }
                }
            }
            return Json("Error", JsonRequestBehavior.AllowGet);
        }
        #endregion
        //**********************************************************************************************************************
        //******************************************* Line Create,Update,Delete(End) *******************************************
        //**********************************************************************************************************************
   
        #region Site with Unit,Dept and Dept with line
     
        [EncryptedActionParameter]
        public ActionResult ShowSiteInfoWithDeptList(int? siteId, bool IsDisplay)
        {
            if (siteId == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            SiteListModelView model = new SiteListModelView();
            SiteList site = db.SiteLists.Find(siteId);
            model.Id = Convert.ToInt32(siteId); ;
            model.SiteName = site.SiteName;
            model.IsComposite = site.IsComposite;
            model.IsDisplay = IsDisplay;
            return View(model);
        }
        public PartialViewResult _ShowSiteDetailsPopUp(int? siteId, string view)
        {
            ViewBag.SiteId = siteId;
            ViewBag.View = view;
            return PartialView();
        }
        public PartialViewResult _ShowSiteDetails(int? siteId, string view)
        {
            View_SiteLists sitedetails = db.View_SiteLists.Where(m => m.Id == siteId).FirstOrDefault();
            if (view == "partial")
            {
                return PartialView(sitedetails);
            }
            else
            {
                SiteListModelView model = new SiteListModelView();
                SiteList site = db.SiteLists.Find(siteId);
                model.SiteId = site.Id;
                model.SiteName = site.SiteName;
                model.SiteAcronym = site.SiteAcronym;
                model.Address = site.Address;
                model.AddressLine1 = site.AddressLine1;
                model.Country = site.Country;
                model.DivisionId = site.DivisionId;
                model.Statename = site.Statename;
                model.City = site.City;
                model.Area = site.Area;
                model.PostalCode = site.PostalCode;
                model.Status = site.Status;
                model.IsComposite = site.IsComposite;
                model.UnitNumber = site.UnitNumber;
                model.CreatedDate = site.CreatedDate;
                model.CreatedBy = site.CreatedBy;
                ViewBag.DivisionList = new SelectList(db.DivisionLists, "DivisionId", "DivisionName", model.DivisionId);
                ViewBag.CountryId = new SelectList(db.CountryLists, "CountryCode", "CountryName", model.Country);
                return PartialView("_SiteEditDetails", model);
            }
        }
        public PartialViewResult _SiteEditDetails(int? siteId, string view)
        {
            SiteListModelView model = new SiteListModelView();
            SiteList site = db.SiteLists.Find(siteId);
            model.SiteId = site.Id;
            model.SiteName = site.SiteName;
            model.SiteAcronym = site.SiteAcronym;
            model.Address = site.Address;
            model.AddressLine1 = site.AddressLine1;
            model.Country = site.Country;
            model.DivisionId = site.DivisionId;
            model.Statename = site.Statename;
            model.City = site.City;
            model.Area = site.Area;
            model.PostalCode = site.PostalCode;
            model.Status = site.Status;
            model.IsComposite = site.IsComposite;
            model.UnitNumber = site.UnitNumber;
            model.CreatedDate = site.CreatedDate;
            model.CreatedBy = site.CreatedBy;
            ViewBag.DivisionList = new SelectList(db.DivisionLists, "DivisionId", "DivisionName", model.DivisionId);
            ViewBag.CountryId = new SelectList(db.CountryLists, "CountryCode", "CountryName", model.Country);
            return PartialView("_SiteEditDetails", model);

        }
        [EncryptedActionParameter]
        public ActionResult ShowUnitWithDeptList(int? unitId, bool? IsDisplay, bool? display)
        {
            if (unitId == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            DepartmentListModelView model = new DepartmentListModelView();
            Site_Unit_Lists unitdetail = db.Site_Unit_Lists.Find(unitId);
            model.UnitId = unitdetail.UnitId;
            model.UnitName = unitdetail.UnitName;
            model.Status = unitdetail.UnitStatus;
            model.SiteId = unitdetail.SiteId;
            model.SiteName = db.SiteLists.Find(model.SiteId).SiteName;
            model.CreatedBy = unitdetail.CreatedBy;
            model.CreatedDate = unitdetail.CreatedDate;
            model.Displaytype = Convert.ToBoolean(IsDisplay);
            model.IsInfo = Convert.ToBoolean(display);
            return View(model);
        }
        public PartialViewResult _ShowUnitDetails(int? unitId)
        {
            Site_Unit_Lists model = db.Site_Unit_Lists.Find(unitId);
            return PartialView(model);
        }
        public PartialViewResult _ShowAllDeptListBySite(int? siteId, int? unitId)
        {
            if (siteId != null)
            {
                ViewBag.id = siteId;
                SiteList site = db.SiteLists.Where(m => m.Id == siteId).FirstOrDefault();
                ViewBag.isResi = site.IsResidential;
                if (site.IsComposite == true)
                {
                    var unitList = db.Site_Unit_Lists.Where(m => m.SiteId == siteId).ToList();
                    return PartialView("_ShowAllUnitListBySite", unitList);
                }
                else
                {
                    var deptList = db.DepartmentLists.Where(m => m.SiteId == siteId).ToList();
                    return PartialView(deptList);
                }
            }
            else
            {
                ViewBag.id = unitId;
                return PartialView(db.DepartmentLists.Where(m => m.UnitId == unitId));
            }
        }
        //*************************** Line Details Page(Start) *********************************************************
        [EncryptedActionParameter]
        public ActionResult ShowDeptWithLineInfo(int? deptId, bool? display, bool? IsInfo, string Backtolist)
        {
            if (deptId == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            DepartmentListModelView model = new DepartmentListModelView();
            var dept = db.DepartmentLists.Find(deptId);
            if (dept != null)
            {
                model.DeptId = dept.DeptId;
                model.NumberOfLine = dept.NumberOfLine;
                if (dept.SiteId != null)
                {
                    model.SiteId = dept.SiteId;
                    model.SiteName = db.SiteLists.Find(dept.SiteId).SiteName;
                    model.UnitId = 0;
                }
                else
                {
                    model.UnitId = dept.UnitId;
                    model.UnitName = db.Site_Unit_Lists.Find(dept.UnitId).UnitName;
                    model.SiteId = 0;
                }
                model.Displaytype = Convert.ToBoolean(display);
                model.IsInfo = Convert.ToBoolean(IsInfo);
                if (Backtolist != null)
                {
                    ViewBag.Parent = Backtolist.Split(',')[0];
                    ViewBag.Page = Backtolist.Split(',')[1];
                }
                return View(model);
            }
            else
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
        }
        public PartialViewResult DepartmentEditPopUp(int deptId)
        {
            ViewBag.deptId = deptId;
            return PartialView();
        }
        public PartialViewResult _ShowDeptDetails(int? deptId, bool isDisplay)
        {
            DepartmentListModelView modelpartial = new DepartmentListModelView();
            DepartmentList dept = db.DepartmentLists.Find(deptId);
            modelpartial.DeptId = dept.DeptId;
            modelpartial.SiteId = dept.SiteId;
            modelpartial.DeptName = dept.DeptName;
            modelpartial.DeptAcronym = dept.DeptAcronym;
            modelpartial.NumberOfLine = dept.NumberOfLine;
            modelpartial.Status = dept.Status;
            modelpartial.CreatedDate = dept.CreatedDate;
            modelpartial.CreatedBy = dept.CreatedBy;
            modelpartial.UnitId = dept.UnitId;
            modelpartial.UnitName = db.Site_Unit_Lists.Where(x => x.UnitId == dept.UnitId).Select(x => x.UnitName).FirstOrDefault();
            if (isDisplay)
            {
                return PartialView(modelpartial);
            }
            else
            {
                return PartialView("_DeptEditDetails", modelpartial);
            }
        }
        public PartialViewResult _ShowAllLineInfoByDept(int? deptId, bool? isDisplay)
        {
            ViewBag.IsDisplay = isDisplay;
            var line = db.LineInfoes.Where(m => m.DeptId == deptId).ToList();
            return PartialView(line);
        }
        public JsonResult CheckDeptLineCount(int? DeptId, long UserId)
        {
            string msg = "";
            int OperationStatus = 0, ColumnId = 0;
            var list = db.LineInfoes.Where(m => m.DeptId == DeptId).Count();
            if (list == 1)
            {
                var deptInfo = db.DepartmentLists.Find(DeptId);
                deptInfo.Status = 0;
                deptInfo.UpdatedBy = UserId;
                deptInfo.UpdatedDate = now;
                db.Entry(deptInfo).State = EntityState.Modified;
                try
                {
                    msg = "Dept '" + deptInfo.DeptName + "' status has successfully inactivated on " + now.ToString("MMM,dd,yyyy hh:mm tt");
                    db.SaveChanges();
                    OperationStatus = 1;
                }
                catch (Exception)
                {
                    OperationStatus = -1;
                    msg = "Dept status inactivate unsuccessfull on " + now.ToString("MMM,dd,yyyy hh:mm tt");
                }
                ColumnId = Convert.ToInt32(DeptId);
                SaveAuditLog("Department Info Update", msg, "Configuration", "Department Details", "DepartmentLists", ColumnId, UserId, now, OperationStatus);
            }
            return Json(list, JsonRequestBehavior.AllowGet);
        }
        public JsonResult RemoveLineFromDept(int? LineId, int? DeptId, string SiteName, long UserId)
        {
            if (LineId != null && DeptId != null)
            {
                LineInfo line = db.LineInfoes.Find(LineId);
                if ((!db.View_UserLists.Where(m => m.LineId == LineId).Any()) && (!db.LineMachineLists.Where(m => m.LineId == LineId).Any()))
                {
                    DepartmentList dept = db.DepartmentLists.Find(DeptId);
                    if (line != null && dept != null)
                    {
                        string msg = "Line '" + line.LineName + "' from department '" + dept.DeptName + "' of site '" + SiteName + "' has been successfully deleted on " + now.ToString("MMM dd,yyyy hh:mm tt");
                        int ColumnId = line.LineId;
                        int OperationStatus = 1;
                        db.LineInfoes.Remove(line);
                        try
                        {
                            dept.NumberOfLine = dept.NumberOfLine - 1;
                            dept.UpdatedBy = UserId;
                            dept.UpdatedDate = now;
                            db.Entry(dept).State = EntityState.Modified;
                            db.SaveChanges();
                        }
                        catch (Exception)
                        {
                            OperationStatus = -1;
                            msg = "Line '" + line.LineName + "' from department '" + dept.DeptName + "' of site '" + SiteName + "' deletion unsuccessful on " + now.ToString("MMM dd,yyyy hh:mm tt");
                        }
                        SaveAuditLog("Line Delete", msg, "Configuration", "Deptartment Details", "LineInfoes", ColumnId, UserId, now, OperationStatus);
                        if (OperationStatus == 1)
                        {
                            return Json("Success", JsonRequestBehavior.AllowGet);
                        }
                    }
                }
                else
                {
                    return Json("Line can not be deleted.", JsonRequestBehavior.AllowGet);
                }
            }
            return Json("Error", JsonRequestBehavior.AllowGet);
        }
        public PartialViewResult _EditLineInfoPopUp(int id)
        {
            LineInfoModelView model = (from m in db.LineInfoes
                                       where m.LineId == id
                                       select new LineInfoModelView
                                       {
                                           LineId = m.LineId,
                                           LineAcronym = m.LineAcronym,
                                           LineName = m.LineName,
                                           Status = m.Status,
                                           DeptId = m.DeptId,
                                       }).FirstOrDefault();
            return PartialView(model);
        }
        public PartialViewResult _EditLineInfo(int id)
        {
            LineInfoModelView model = (from m in db.LineInfoes
                                       where m.LineId == id
                                       select new LineInfoModelView
                                       {
                                           LineId = m.LineId,
                                           LineAcronym = m.LineAcronym,
                                           LineName = m.LineName,
                                           Status = m.Status,
                                           DeptId = m.DeptId,
                                       }).FirstOrDefault();
            return PartialView(model);
        }
        [EncryptedActionParameter]
        public ActionResult ShowMachineWithLine(int? LineId)
        {
            LineInfoModelView model = new LineInfoModelView();
            model.LineId = Convert.ToInt32(LineId);
            return View(model);
        }
        public PartialViewResult _ShowLineDetails(int? LineId, bool isDisplay)
        {
            LineInfoModelView modelpartial = new LineInfoModelView();
            LineInfo line = db.LineInfoes.Find(LineId);
            modelpartial.LineId = line.LineId;
            modelpartial.LineAcronym = line.LineAcronym;
            modelpartial.LineName = line.LineName;
            modelpartial.Status = line.Status;
            modelpartial.DeptId = line.DeptId;
            modelpartial.CreatedBy = line.CreatedBy;
            modelpartial.CreatedDate = line.CreatedDate;
            modelpartial.DeptName = db.DepartmentLists.Where(n => n.DeptId == line.DeptId).Select(p => p.DeptName).FirstOrDefault();
            return PartialView(modelpartial);
        }
        public PartialViewResult _ShowAllMachineOfLine(int? LineId)
        {
            var machinId = db.LineMachineLists.Where(m => m.LineId == LineId).Select(m => m.MachineId).ToList();
            var list = (from m in db.View_Machine_Lists
                        where machinId.Contains(m.MachineId)
                        select new View_Machine_ListsModelView
                        {
                            Id = m.Id,
                            MachineId = m.MachineId,
                            LineId = LineId.Value,
                            MachineAcronym = m.MachineAcronym,
                            MachineTypeId = m.MachineTypeId,
                            BrandName = m.BrandName,
                            CountryName = m.CountryName,
                            MachineType = m.MachineType,
                            ModelName = m.ModelName,
                            Acroynm = m.Acroynm,
                            Quantity = m.Quantity,
                            MachineTypeName = m.MachineTypeName,
                            Mgf = m.Mgf
                        }).ToList();
            ViewBag.LineId = LineId;
            return PartialView(list);
        }
        
        //*************************** Line Details Page(Edit) **********************************************************
   
        #endregion
       
        //******************************************  Assign Machine With Line(Start) *****************************************
        #region MachineWithLine
        public ActionResult AssignMachineWithLine(int? deptId)
        {
            ViewBag.deptId = deptId;
            if (deptId == null)
            {
                ViewBag.deptId = "All";
            }
            return View();
        }
        public PartialViewResult MachineListInPartialPage(string searchText, string deptId)
        {
            ViewBag.searchText = searchText;
            ViewBag.deptId = deptId;
            return PartialView();
        }
        public PartialViewResult _MacineWithLine()
        {
            return PartialView();
        }
        public PartialViewResult LineListInPartialPage(string deptId)
        {
            ViewBag.deptId = deptId;
            return PartialView();
        }

        public JsonResult GetAllMachineTypeName()
        {
            var mahineId = db.MachineLists.Where(m => m.AssignStatus == false).Select(m => m.MachineTypeId).ToList();
            var MachineTypeName = (from m in db.Machines
                                   join n in db.MachineTypeNames on m.MachineType equals n.MachineId
                                   where mahineId.Contains(m.Id)
                                   select new
                                   {
                                       id = n.MachineId + "-n",
                                       type = "parent",
                                       text = n.MachineTypeName1,
                                       hasChild = db.MachineLists.Where(p => p.MachineTypeId == m.Id && p.AssignStatus == false).Any()
                                   }).Distinct().ToList();
            return Json(MachineTypeName, JsonRequestBehavior.AllowGet);
        }
        public JsonResult GetAllMachineTypeList(string searchText, string machineTypeId)
        {
            int Id = Convert.ToInt16(machineTypeId.Split('-')[0].ToString());
            var MachineTypeName = (from m in db.Machines
                                   join n in db.MachineLists on m.Id equals n.MachineTypeId
                                   where n.AssignStatus == false && m.MachineType == Id
                                   select new
                                   {
                                       id = m.Id + "-t",
                                       type = "parent",
                                       text = m.Acroynm,
                                       hasChild = db.MachineLists.Where(p => p.MachineTypeId == m.Id && p.AssignStatus == false).Any()
                                   }).Distinct().ToList();
            return Json(MachineTypeName, JsonRequestBehavior.AllowGet);
        }
        public JsonResult GetAllMachine(string machineTypeId)
        {
            int Id = Convert.ToInt16(machineTypeId.Split('-')[0].ToString());
            var machine = (from m in db.MachineLists
                           join t in db.Machines on m.MachineTypeId equals t.Id
                           where m.MachineTypeId == Id && m.AssignStatus == false
                           select new
                           {
                               id = m.MachineId + "-m",
                               machineId = m.MachineId,
                               machineTypeId = m.MachineTypeId,
                               type = "child",
                               text = (t.ModelName.Length > 0)?m.MachineAcronym+"( Model : "+t.ModelName+" )":m.MachineAcronym
                           }).ToList();
            return Json(machine, JsonRequestBehavior.AllowGet);
        }
        public JsonResult GetAllSiteList()
        {
            var siteList = (from m in db.SiteLists
                            where m.Status == 1
                            select new
                            {
                                id = m.Id + "-s",
                                siteId = m.Id,
                                text = m.SiteName,
                                type = "parent",
                                isComposite = m.IsComposite,
                                hasDept = db.DepartmentLists.Where(d => d.SiteId == m.Id).Any(),
                                hasUnit = db.Site_Unit_Lists.Where(k => k.SiteId == m.Id).Any(),
                                hasChild = db.DepartmentLists.Where(d => d.SiteId == m.Id).Any() || db.Site_Unit_Lists.Where(k => k.SiteId == m.Id).Any()
                            }).Distinct().ToList();
            for (int i = 0; i < siteList.Count; i++)
            {
                if (siteList[i].hasDept == false && siteList[i].hasUnit == false) // there will only one item with Number = 1   
                {
                    siteList.Remove(siteList[i]);
                }
            }
            return Json(siteList, JsonRequestBehavior.AllowGet);
        }
        public JsonResult GetAllDept(string site)
        {
            int siteId = Convert.ToInt32(site.Split('-')[0]);
            bool isComposite = Convert.ToBoolean(site.Split('-')[1]);
            if (isComposite)
            {
                var UnitList = (from m in db.Site_Unit_Lists
                                where m.SiteId == siteId
                                select new
                                {
                                    id = m.UnitId + "-u",
                                    text = m.UnitName,
                                    unitId = m.UnitId,
                                    type = "parent",
                                    hasLine = db.DepartmentLists.Where(d => d.UnitId == m.UnitId && d.Status == 1).Any()
                                }).Distinct().ToList();
                return Json(UnitList, JsonRequestBehavior.AllowGet);
            }
            else
            {
                var UnitList = (from m in db.DepartmentLists
                                where m.SiteId == siteId && m.Status == 1
                                select new
                                {

                                    id = m.DeptId + "-d",
                                    text = m.DeptName,
                                    deptId = m.DeptId,
                                    type = "parent",
                                    hasLine = db.LineInfoes.Where(n => n.DeptId == m.DeptId).Any()
                                }).Distinct().ToList();
                return Json(UnitList, JsonRequestBehavior.AllowGet);
            }

        }
        public JsonResult GetAllLine(string deptId)
        {
            string type = deptId.Split('-')[1].ToString();
            int Id = Convert.ToInt16(deptId.Split('-')[0].ToString());
            if (type == "d")
            {
                var LineList = (from m in db.LineInfoes
                                where m.DeptId == Id
                                select new
                                {
                                    id = m.LineId + "-l",
                                    text = m.LineName,
                                    lineId = m.LineId,
                                    type = "parent",
                                    hasLine = db.LineMachineLists.Where(n => n.LineId == m.LineId).Any()
                                }).ToList();
                return Json(LineList, JsonRequestBehavior.AllowGet);
            }
            else if (type == "u")
            {
                var DeptList = (from m in db.DepartmentLists
                                from o in db.LineInfoes
                                where m.UnitId == Id && m.Status == 1 && o.DeptId == m.DeptId
                                select new
                                {
                                    id = m.DeptId + "-d",
                                    text = m.DeptName,
                                    deptId = m.DeptId,
                                    type = "parent",
                                    hasLine = db.LineInfoes.Where(n => n.DeptId == m.DeptId).Any()
                                }).Distinct().ToList();
                return Json(DeptList, JsonRequestBehavior.AllowGet);
            }
            else
            {
                var MachineList = (from m in db.LineMachineLists
                                   from n in db.LineInfoes
                                   from o in db.MachineLists
                                   where m.LineId == Id && m.LineId == n.LineId && m.MachineId == o.MachineId
                                   select new
                                   {
                                       id = m.MachineId + "-m",
                                       text = o.MachineAcronym,
                                       machineId = m.MachineId,
                                       machineTypeId = o.MachineTypeId,
                                       type = "parent",
                                       hasLine = false
                                   }).Distinct().ToList();
                return Json(MachineList, JsonRequestBehavior.AllowGet);
            }
        }
        public JsonResult AssignMachineSave(int[] allMachineId, int LineId, int userId)
        {
            string msg = "";
            int ColumnId = 0, OperationStatus = 1;
            string LineName = db.LineInfoes.Where(m => m.LineId == LineId).Select(m => m.LineName).FirstOrDefault();
            for (int i = 0; i < allMachineId.Length; i++)
            {
                int MachineId = allMachineId[i];
                string machineName = db.MachineLists.Where(m => m.MachineId == MachineId).Select(m => m.MachineAcronym).FirstOrDefault();
                LineMachineList sample = new LineMachineList();
                sample.LineId = LineId;
                sample.MachineId = MachineId;
                sample.AssignedBy = userId;
                sample.AssignedDate = now;
                db.LineMachineLists.Add(sample);
                try
                {
                    db.SaveChanges();
                    int UserId = db.LineMachineLists.Where(m => m.AssignedBy == userId && m.Id == sample.Id).Select(m => m.Id).FirstOrDefault();
                    ColumnId = UserId;
                    msg = "New Machine '" + machineName + "' has been successfully assigned to line '" + LineName + "' on " + now.ToString("MMM dd,yyyy hh:mm tt");
                }
                catch (Exception)
                {
                    msg = "New Machine '" + machineName + "' assign was unsuccessful in Line '" + LineName + "' on " + now.ToString("MMM dd,yyyy hh:mm tt");
                    OperationStatus = -1;
                }
                SaveAuditLog("Create Assigned Machine", msg, "Configuration", "AssignMachineWithLine", "LineMachineList", ColumnId, userId, now, OperationStatus);

                MachineList modelmachine = db.MachineLists.Where(m => m.MachineId == MachineId).FirstOrDefault();
                modelmachine.AssignStatus = true;
                db.Entry(modelmachine).State = EntityState.Modified;
                db.SaveChanges();

                LineInfo modelline = db.LineInfoes.Where(m => m.LineId == LineId).FirstOrDefault();
                modelline.Status = 1;
                db.Entry(modelline).State = EntityState.Modified;
                db.SaveChanges();
            }
            return Json("Success", JsonRequestBehavior.AllowGet);
        }
        public JsonResult DeleteAssignMachineFromLine(int MachineId, int LineId, int userId, string machineName, string LineName)
        {
            string msg = "";
            int ColumnId = 0, OperationStatus = 1;

            if (db.UserInformations.Where(m => m.MachineId == MachineId).Any())
            {
                return Json("You can not delete this machine from line.Because this machine is assigned with user.", JsonRequestBehavior.AllowGet);
            }
            else
            {
                LineMachineList model = db.LineMachineLists.Where(m => m.LineId == LineId && m.MachineId == MachineId).FirstOrDefault();
                ColumnId = MachineId;
                db.LineMachineLists.Remove(model);
                try
                {
                    db.SaveChanges();
                    msg = "Machine '" + machineName + "' has been successfully removed from Line '" + LineName + "' on " + now.ToString("MMM dd,yyyy hh:mm tt");
                }
                catch (Exception)
                {
                    msg = "Machine '" + machineName + "' remove unsuccessfull '" + LineName + "' on " + now.ToString("MMM dd,yyyy hh:mm tt");
                    OperationStatus = -1;
                }
                SaveAuditLog("Delete Assigned Machine", msg, "Configuration", "AssignMachineWithLine", "LineMachineList", ColumnId, userId, now, OperationStatus);

                MachineList modelmachine = db.MachineLists.Where(m => m.MachineId == MachineId).FirstOrDefault();
                modelmachine.AssignStatus = false;
                db.Entry(modelmachine).State = EntityState.Modified;
                db.SaveChanges();

                LineInfo modelline = db.LineInfoes.Where(m => m.LineId == LineId).FirstOrDefault();
                modelline.Status = 0;
                db.Entry(modelline).State = EntityState.Modified;
                db.SaveChanges();
                return Json("Success", JsonRequestBehavior.AllowGet);
            }
        }
      
        #endregion
        #region
      
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