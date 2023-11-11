using FactoryManagement.Models;
using FactoryManagement.ModelView.Acquisition;
using FactoryManagement.ModelView.Auth;
using FactoryManagement.ModelView.Inventory;
using FactoryManagement.ModelView.Inventory.Mahinaries;
using FactoryManagement.ModelView.Inventory.Product;
using FactoryManagement.ModelView.Inventory.Store;
using FactoryManagement.ModelView.Inventory.Warehouse;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data.Entity;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace FactoryManagement.Controllers
{
    public class SoldierController : Controller
    {
        #region Private Properties
        private FactoryManagementEntities db = new FactoryManagementEntities();
        int ShowItemPerPage = 20;
        static private int offset = Convert.ToInt32(ConfigurationManager.AppSettings["localTime"]);
        DateTime now = DateTime.UtcNow.AddMinutes(offset);
        #endregion

        #region Create,Delete,Update and List of ProductType
        public ActionResult ProductTypeList()
        {
            return View();
        }
        public ActionResult _ShowAllProductTypeList()
        {
            var AllParentProductType = (from p in db.View_Categories
                                        where p.HasParent == false
                                        select new ProductTypeViewModel
                                        {
                                            ProductTypeId = p.CategoryId,
                                            ProductTypeName = p.CategoryName,
                                            CreatedBy = p.CreatedBy,
                                            CreatedDateName = p.CreatedDateName,
                                            HasChild = p.HasChild,
                                            UpdatedDate = p.UpdatedDate,
                                            UpdatedBy = p.UpdatedBy,
                                            UpdatedDateFormate = p.UpdatedDateFormate,
                                            CanBeDeleted = p.CanBeDeleted
                                        }).OrderBy(p => p.ProductTypeName).ToList();
            ViewBag.AllParentProType = AllParentProductType;
            return PartialView();
        }
        public JsonResult GetAllProductTypeForList(int? ParentId)
        {
            int id = Convert.ToInt32(ParentId);
            var list = (from m in db.View_Categories
                        where m.ParentId == id
                        select new
                        {
                            ProductTypeId = m.CategoryId,
                            ProductTypeName = m.CategoryName,
                            HasParent = m.HasParent,
                            ParentId = m.ParentId,
                            ParentName = m.ParentName,
                            CreatedBy = m.CreatedBy,
                            CreatedDateName = m.CreatedDateName,
                            HasChild = m.HasChild,
                            UpdatedDateFormate = m.UpdatedDateFormate,
                        }).OrderBy(p => p.ProductTypeName).ToList();

            return Json(list, JsonRequestBehavior.AllowGet);
        }
        public ActionResult ProductTypeCreate(int? ProductTypeId, int? ParentId, string ParentName)
        {
            ProductTypeViewModel model = new ProductTypeViewModel();
            if (ParentId != null)
            {
                model.HasParent = true;
                model.ParentId = ParentId;
                model.ParentName = ParentName;
            }
            if (ProductTypeId != null)
            {
                View_Categories pro_type = db.View_Categories.Where(m => m.CategoryId == ProductTypeId).FirstOrDefault();
                model.ProductTypeId = pro_type.CategoryId;
                model.ProductTypeName = pro_type.CategoryName;
                model.HasParent = pro_type.HasParent;
                model.ParentId = pro_type.ParentId;
                model.ParentName = pro_type.ParentName;
                model.HasChild = pro_type.HasChild;
            }
            return PartialView(model);
        }
        public JsonResult ProductTypeCreateSave(Category model)
        {
            if (ModelState.IsValid)
            {
                int ColumnId = 0;
                int OperationStatus = 1;

                Category pro_type = new Category();
                if (model.CategoryId > 0)
                {
                    pro_type = db.Categories.Find(model.CategoryId);
                    pro_type.CategoryName = model.CategoryName;
                    pro_type.HasParent = model.HasParent;
                    pro_type.ParentId = model.ParentId;
                    pro_type.UpdatedBy = model.CreatedBy;
                    pro_type.UpdatedDate = now;
                    db.Entry(pro_type).State = EntityState.Modified;
                    ColumnId = pro_type.CategoryId;
                }
                else
                {
                    pro_type.CategoryName = model.CategoryName;
                    pro_type.HasParent = model.HasParent;
                    pro_type.ParentId = model.ParentId;
                    pro_type.CreatedBy = model.CreatedBy;
                    pro_type.CreatedDate = now;
                    pro_type.CanBeDeleted = true;
                    db.Categories.Add(pro_type);                   
                }
                try
                {
                    db.SaveChanges();
                    if (model.CategoryId == 0)
                    {
                        ColumnId = db.Categories.Max(m => m.CategoryId);
                    }
                }
                catch (Exception)
                {
                    OperationStatus = -1;
                }
                if (OperationStatus == 1)
                {
                    return Json("Success", JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json("Error", JsonRequestBehavior.AllowGet);
                }
            }
            else
            {
                return Json("Error", JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult ProductTypeDestroy(int? ProductTypeId, long UserId, string ParentName)
        {

            var allCat = db.Categories.ToList();
            foreach(var v in allCat)
            {
                db.Categories.Remove(v);
                db.SaveChanges();
            }
            var allPro = db.ProductItemLists.ToList();
            foreach(var p in allPro)
            {
                db.ProductItemLists.Remove(p);
                db.SaveChanges();
            }
            if (ProductTypeId != null)
            {
                return Json("Success", JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json("Error", JsonRequestBehavior.AllowGet);
            }
        }
        #endregion
    }
}