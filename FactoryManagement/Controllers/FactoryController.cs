using FactoryManagement.Models;
using FactoryManagement.ModelView.Factory;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace FactoryManagement.Controllers
{
    [Authorize]
    public class FactoryController : Controller
    {
        #region Private Properties
        private FactoryManagementEntities db = new FactoryManagementEntities();
        static private int offset = Convert.ToInt32(ConfigurationManager.AppSettings["localTime"]);
        DateTime now = DateTime.UtcNow.AddMinutes(offset);
        #endregion

        #region Factory Create , Update
        public ActionResult FactoryCreate(int? id)
        {
            FactoryInfoModelView model = new FactoryInfoModelView();
            if (id == null)
            {
                ViewBag.Title = "New Factory Create";
            }
            else
            {
                FactoryInformation info = db.FactoryInformations.Find(id);
                model.Id = info.Id;
                model.Name = info.Name;
                model.OwnerName = info.OwnerName;
                model.OwnerPicture = info.OwnerPicture;
                model.PhoneNumber = info.PhoneNumber;
                model.MobileNumber = info.MobileNumber;
                model.Fax = info.Fax;
                model.Email = info.Email;
                model.FullAddress = info.Address;
                var addressField = info.Address.Split(',');
                model.Address = addressField[0];
                model.CurrencyId = info.CurrencyId;
                model.CurrencyName = db.CurrencyLists.Find(info.CurrencyId).CurrencyName;
                ViewBag.Title = "Factory Information";
            }
            ViewBag.CurrencyList = new SelectList(db.CurrencyLists, "CurrencyId", "CurrencyName", model.CurrencyId);
            ViewBag.DivisionList = new SelectList(db.DivisionLists, "DivisionId", "DivisionName", model.DivisionId);
            return View(model);
        }

        public JsonResult FactoryCreateSave(FactoryInfoModelView model, HttpPostedFileBase files)
        {
            if (ModelState.IsValid)
            {

            }
            return Json("Error",JsonRequestBehavior.AllowGet);
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
            db.AuditInformations.Add(audit);
            db.SaveChanges();
        }
        #endregion
    }
}