using FactoryManagement.Filters;
using FactoryManagement.Models;
using FactoryManagement.ModelView.Acquisition;
using FactoryManagement.ModelView.Auth;
using FactoryManagement.ModelView.HR;
using FactoryManagement.ModelView.Inventory;
using FactoryManagement.ModelView.Inventory.Mahinaries;
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
    [Authorize]
    [DisplayName("Acquisition")]
    public class AcquisitionController : Controller
    {
        #region Private Properties
        private FactoryManagementEntities db = new FactoryManagementEntities();
        private FactoryManagementEntities dbAud = new FactoryManagementEntities();
        int ShowItemPerPage = 20;
        int ShowIndentTransPerPage = 20;
        int ShowIndentVoucherPerPage = 20;
        static private int offset = Convert.ToInt32(ConfigurationManager.AppSettings["localTime"]);
        DateTime now = DateTime.UtcNow.AddMinutes(offset);
        #endregion
        #region print
           [EncryptedActionParameter]
        public ActionResult Print(int id)
        {
            if (id > 0)
            {
                var voucherItem=db.View_Manual_Indent_Voucher.Where(m => m.VoucherId == id).FirstOrDefault();
                if (voucherItem != null)
                {
                    return PartialView(voucherItem);
                }
                ViewBag.VoucherId = id;
            }
            return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        }
        #endregion
        #region ALL ABOUNT ACQUISITION
        public ActionResult Acquisition(AcquisitionViewModel acq)
        {
            if (acq.AcquisitionOrderId > 0 && db.Acquisition_List.Where(m => m.AcquisitionOrderId == acq.AcquisitionOrderId).Any())
            {
                Acquisition_List acqlist = db.Acquisition_List.Find(acq.AcquisitionOrderId);
                acq.Name = acqlist.Name;
                acq.SiteId = acqlist.SiteId;
                acq.AcquisitionType = acqlist.AcquisitionType;
                acq.BusinessOrderId = acqlist.BusinessOrderId;
            }
            else
            {
                if (acq.AcquisitionType == 2)
                {
                    acq.Name = "Reservation for business order "+db.BusinessOrderLists.Where(m => m.BusinessOrderId == acq.BusinessOrderId).Select(m => m.OrderName).FirstOrDefault();
                }
                if (acq.AcquisitionType == 3)
                {
                    if (acq.PartsId > 0)
                    {
                        acq.Name = "Indent for spare parts " + db.SparePartsLists.Where(m => m.PartsId == acq.PartsId).Select(m => m.Name).FirstOrDefault();
                    }
                    else
                    {
                        acq.Name = "Indent for machine " + db.MachineLists.Where(m => m.MachineId == acq.MachineId).Select(m => m.MachineAcronym).FirstOrDefault();
                    }
                }
            }
            return View(acq);
        }
        public ActionResult _NewProductForAcquisition(int? AcquisitionOrderId, int AcquisitionType, int? BusinessOrderId,int? PartsId, string PartsName )
        {
            AcquisitionViewModel acq = new AcquisitionViewModel();
            if (AcquisitionOrderId != null)
            {
                acq.AcquisitionId = Convert.ToInt32(AcquisitionOrderId);
            }
            acq.AcquisitionType = AcquisitionType;
            acq.BusinessOrderId = BusinessOrderId;
            ViewBag.ProductTypeId = new SelectList(db.ProductTypeLists.Where(m => m.HasParent == false), "ProductTypeId", "ProductTypeName");
            ViewBag.BrandId = new SelectList(db.BrandLists.Where(m => m.HasParent == false), "BrandId", "BrandName");
            ViewBag.UnitId = new SelectList(db.UnitLists.Where(m => m.HasParentId == false), "UnitId", "UnitName");
            ViewBag.Country = new SelectList(db.CountryLists, "Id", "CountryName", acq.Country);
            if (AcquisitionType == 3)
            {
                if (Convert.ToInt32(PartsId) > 0)
                {
                    acq.ProductName = PartsName;
                }
            }
            return PartialView(acq);
        }
        public ActionResult _EditProductForAcquisition(int? AcquisitionId)
        {
            int available_quan = 0;
            AcquisitionEditViewModel model = new AcquisitionEditViewModel();
            if (AcquisitionId != null)
            {
                var acqmodel = db.View_All_AcquisitionList.Where(m => m.AcquisitionId == AcquisitionId).FirstOrDefault();
                model.AcquisitionId = acqmodel.AcquisitionId;
                model.AcquisitionOrderId = acqmodel.AcquisitionOrderId;
                model.ProductNameEdit = acqmodel.ProductName;
                model.ProductId = acqmodel.ProductId;
                model.ProductTypeIdEdit = acqmodel.ProductTypeId;
                model.ProductTypeName = acqmodel.ProductType;
                model.Country = acqmodel.Country;
                model.CountryName = acqmodel.CountryName;
                model.ModelEdit = acqmodel.Model;
                model.BrandIdEdit = acqmodel.Brand;
                model.Brand = acqmodel.BrandName;
                model.QuantityEdit = acqmodel.Quantity;
                model.IsSubQuantity = acqmodel.IsSubQuantity;
                model.UnitIdEdit = acqmodel.UnitId;
                model.UnitName = acqmodel.UnitName;
                model.IsAsapEdit = acqmodel.IsAsap;
                model.ASPTimeEdit = acqmodel.AspTime;
                model.RequiredDateEdit = acqmodel.RequiredDate;
                model.RequiredTimeEdit = acqmodel.RequiredTime;
                model.CommentEdit = acqmodel.Comment;
                if (model.ProductId != null)
                {
                    var inven = db.View_All_InventoryList.Where(m => m.InventoryId == model.ProductId).FirstOrDefault();
                    if (model.IsSubQuantity)
                    {
                        model.UnitName = inven.UnitName;
                    }
                    int total_resrvd = 0;
                    int inven_quan =Convert.ToInt32(inven.Quantity);

                    if (db.View_All_InventoryList.Where(m => m.InventoryId == model.ProductId && m.CanbeBreakable).Any())
                    {
                        int ttl_parent = 0, ttl_sub = 0, total_ParentQuantity = 0, total_ChildQuantity = 0;
                        if (db.Inventory_Reserved_Item_Lists.Where(m => m.InventoryId == model.ProductId && m.IsAssigned == false && m.IsSubUnit == false && m.AcquisitionOrderId != model.AcquisitionOrderId).Any())
                        {
                            total_ParentQuantity = db.Inventory_Reserved_Item_Lists
                            .Where(m => m.InventoryId == model.ProductId && m.IsAssigned == false && m.IsSubUnit == false && m.AcquisitionOrderId != model.AcquisitionOrderId)
                            .Select(t => t.Quantity).DefaultIfEmpty().Sum();
                        }
                        if (db.Inventory_Reserved_Item_Lists.Where(m => m.InventoryId == model.ProductId && m.IsAssigned == false && m.IsSubUnit == true && m.AcquisitionOrderId != model.AcquisitionOrderId).Any())
                        {
                            total_ChildQuantity = db.Inventory_Reserved_Item_Lists
                            .Where(m => m.InventoryId == model.ProductId && m.IsAssigned == false && m.IsSubUnit == true && m.AcquisitionOrderId != model.AcquisitionOrderId)
                            .Select(t => t.Quantity).DefaultIfEmpty().Sum();
                        }
                        if (inven.CanbeBreakable)
                        {
                            total_resrvd = (total_ParentQuantity * (int)inven.SubQuantity) + total_ChildQuantity;
                            inven_quan = Convert.ToInt32(inven.Quantity) * (int)inven.SubQuantity;

                            double quotient = total_resrvd / (int)inven.SubQuantity;
                            double ceiling = Math.Ceiling(quotient);
                            ttl_parent = (int)ceiling;

                            double reminder = total_ChildQuantity % (int)inven.SubQuantity;
                            double ceilings = Math.Ceiling(reminder);
                            ttl_sub = (int)ceilings;

                            model.Available_Quantity = (Convert.ToInt32(inven.Quantity) - ttl_parent);
                        }
                        else
                        {
                            total_resrvd = total_ParentQuantity;
                            inven_quan = Convert.ToInt32(inven.Quantity);
                            model.Available_Quantity = (inven_quan - total_resrvd) + Convert.ToInt32(model.QuantityEdit);
                        }
                        
                    }
                    else
                    {
                        int total_qntity = Convert.ToInt32(inven.Quantity);
                        int resrvd_qntity = Convert.ToInt32(inven.ReservedQuantity);
                        model.Available_Quantity = (total_qntity - resrvd_qntity) + Convert.ToInt32(model.QuantityEdit);
                        available_quan = total_qntity;
                    }
                }
                ViewBag.CountryId = new SelectList(db.CountryLists, "Id", "CountryName", model.Country);
                var acqImg = db.AcquisitionImages.Where(m => m.AcquisitionId == AcquisitionId).ToList();
                ViewBag.AcqImg = acqImg;
                ViewBag.TotalAcqImg = acqImg.Count;
                ViewBag.ProductTypeIdEdit = new SelectList(db.ProductTypeLists.Where(m => m.HasParent == false), "ProductTypeId", "ProductTypeName",model.ProductTypeIdEdit);
                ViewBag.BrandIdEdit = new SelectList(db.BrandLists.Where(m => m.HasParent == false), "BrandId", "BrandName",model.BrandIdEdit);
                ViewBag.UnitIdEdit = new SelectList(db.UnitLists.Where(m => m.HasParentId == false), "UnitId", "UnitName", model.UnitIdEdit);
                ViewBag.Ordername = db.Acquisition_List.Where(m => m.AcquisitionOrderId == model.AcquisitionOrderId).Select(m => m.Name).FirstOrDefault();
            }
            ViewBag.available_quan = available_quan;
            return PartialView(model);

        }
        public JsonResult ProductNameSearching(string prefix, int? orderId)
        {
            if (prefix == "")
            {
                return Json("", JsonRequestBehavior.AllowGet);
            }
            else
            {
                if (orderId == null)
                {
                    var allproduct = (from s in db.InventoryLists
                                      where (s.ProductName.ToLower().Contains(prefix.ToLower()))
                                      select new InventoryListModelView
                                      {
                                          InventoryId = s.InventoryId,
                                          ProductName = s.ProductName
                                      }).ToList();
                    return Json(allproduct, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    int[] ids = new int[100];
                    int count = 0;
                    var list = db.Acquisitions.Where(m => m.AcquisitionOrderId == orderId && m.ProductId != null && m.OrderStatus == 0).Select(m => m.ProductId).ToList();
                    foreach (var l in list)
                    {
                        ids[count] =Convert.ToInt32(l);
                        count++;
                    }

                    var allproduct = (from s in db.InventoryLists
                                      where (s.ProductName.ToLower().Contains(prefix.ToLower()))
                                      && !ids.Contains(s.InventoryId)
                                      select new InventoryListModelView
                                      {
                                          InventoryId = s.InventoryId,
                                          ProductName = s.ProductName
                                      }).ToList();
                    return Json(allproduct, JsonRequestBehavior.AllowGet);
                }
                
            }
        }
        public ActionResult _AcquisitionBySearch(int? InventoryId)
        {
            if (InventoryId != null && db.View_All_InventoryList.Where(m => m.InventoryId == InventoryId).Any())
            {
                var acq = db.View_All_InventoryList.Where(m => m.InventoryId == InventoryId).FirstOrDefault();

                if (db.View_All_InventoryList.Where(m => m.InventoryId == acq.InventoryId && m.CanbeBreakable).Any())
                {
                    int reserd_quan = 0, total_child = 0, total_ParentQuantity = 0, total_ChildQuantity = 0;
                    if (db.Inventory_Reserved_Item_Lists
                        .Where(m => m.InventoryId == acq.InventoryId && m.IsAssigned == false && m.IsSubUnit == false).Any())
                    {
                        total_ParentQuantity = db.Inventory_Reserved_Item_Lists
                        .Where(m => m.InventoryId == acq.InventoryId && m.IsAssigned == false && m.IsSubUnit == false)
                        .Select(t => t.Quantity).DefaultIfEmpty().Sum();
                    }
                    if (db.Inventory_Reserved_Item_Lists
                        .Where(m => m.InventoryId == acq.InventoryId && m.IsAssigned == false && m.IsSubUnit == true).Any())
                    {
                        total_ChildQuantity = db.Inventory_Reserved_Item_Lists
                        .Where(m => m.InventoryId == acq.InventoryId && m.IsAssigned == false && m.IsSubUnit == true)
                        .Select(t => t.Quantity).DefaultIfEmpty().Sum();
                    }

                    if (total_ChildQuantity >= acq.SubQuantity)
                    {
                        double quotient = total_ChildQuantity / (int)acq.SubQuantity;
                        double ceiling = Math.Ceiling(quotient);
                        total_child = (int)ceiling;
                    }
                    reserd_quan = total_ParentQuantity + total_child;
                    acq.Available_Quantity = (acq.Quantity - reserd_quan);
                }
                else
                {
                    int total_qntity =Convert.ToInt32( acq.Quantity);
                    int resrvd_qntity = Convert.ToInt32(acq.ReservedQuantity);
                    acq.Available_Quantity = (total_qntity - resrvd_qntity);
                }
                return PartialView(acq);
            }
            return PartialView();
        }
        public JsonResult GetBreakableProUnit(int id)
        {
            View_All_InventoryList parentUnit = db.View_All_InventoryList.Where(m => m.InventoryId == id).FirstOrDefault();
            List<UnitList> list = new List<UnitList>();
            if (parentUnit != null)
            {
                list.Add(new UnitList { UnitId = parentUnit.UnitId, UnitName = parentUnit.UnitName, HasParentId=false });
                list.Add(new UnitList { UnitId = Convert.ToInt32(parentUnit.SubUnitId), UnitName = parentUnit.SubUnitName,HasParentId=true });
            }
            return Json(list, JsonRequestBehavior.AllowGet);
        }

        public ActionResult _AlreadyAddedAcqProductList(int? AcquisitionOrderId)
        {
            AcquisitionViewModel order = new AcquisitionViewModel();
            if (AcquisitionOrderId != null)
            {
                if (db.Acquisition_List.Where(m => m.AcquisitionOrderId == AcquisitionOrderId).Any())
                {
                    var list = db.Acquisition_List.Where(m => m.AcquisitionOrderId == AcquisitionOrderId).FirstOrDefault();
                    order.AcquisitionOrderId = list.AcquisitionOrderId;
                    order.Name = list.Name;

                    ViewBag.AcqProductName = db.View_All_AcquisitionList.Where(m => m.AcquisitionOrderId == AcquisitionOrderId && m.OrderStatus == 0).ToList();
                }
            }
            return PartialView(order);
        }
        public JsonResult AcquisitionSave(AcquisitionViewModel acqmodel, IEnumerable<HttpPostedFileBase> files)
        {
            string msg = "";
            string opname = "";
            int ColumnId = 0;
            int OperationStatus = 1;

            Acquisition_List acqlist = new Acquisition_List();
            int acqorderId = acqmodel.AcquisitionOrderId;

            if (ModelState.IsValid)
            {
                if (acqmodel.AcquisitionOrderId == 0)
                {
                    acqlist.AcquisitionType = acqmodel.AcquisitionType;
                    acqlist.BusinessOrderId = acqmodel.BusinessOrderId;
                    acqlist.PartsId = acqmodel.PartsId;
                    acqlist.MachineId = acqmodel.MachineId;
                    acqlist.Name = acqmodel.Name;
                    acqlist.CreatedBy = acqmodel.CreatedBy;
                    acqlist.CreatedDate = now;
                    acqlist.AcquisitionStatus = (acqmodel.AcquisitionType == 2) ? 2 : 0;
                    acqlist.SiteId = db.UserInformations.Where(m => m.UserId == acqmodel.CreatedBy).Select(m => m.SiteId).FirstOrDefault();
                    db.Acquisition_List.Add(acqlist);
                    try
                    {
                        db.SaveChanges();
                        acqorderId = db.Acquisition_List.Where(m => m.CreatedBy == acqmodel.CreatedBy).Max(m => m.AcquisitionOrderId);
                        opname = "Add New Requisition Order";
                        msg = "New acquisition order " + acqlist.Name + " successfully created.";
                        ColumnId = acqorderId;
                    }
                    catch (Exception ex)
                    {
                        string errrorMsg = ex.Message.ToString();
                        opname = "Add New Requisition ";
                        msg = "New requition order " + acqlist.Name + " create unsuccessful.";
                        OperationStatus = -1;
                        ColumnId = 0;
                    }
                    SaveAuditLog(opname, msg, "Inventory", "Indent", "Acquisition_List", ColumnId, acqmodel.CreatedBy, now, OperationStatus);
                }
                if (OperationStatus == 1)
                {

                    //************************ ADD PRODUCT **************************
                    ColumnId = 0;
                    Acquisition acq = new Acquisition();
                    acq.AcquisitionOrderId = acqorderId;

                    if (acqmodel.InventoryId > 0)
                    {
                        acq.ProductId = acqmodel.InventoryId;

                        if (acqmodel.AcquisitionType > 1)
                        {
                            var inven = db.View_All_InventoryList.Where(m => m.InventoryId == acqmodel.InventoryId).FirstOrDefault();
                            bool quantity_Exsits = true;
                            int resrvd_quantity = 0;

                            int requed_quan = (int)acqmodel.Quantity;
                            int exists_quan = (int)inven.Quantity;
                            int resvd_id = 0;
                            
                            if (acqmodel.IsSubQuantity)
                            {
                                if (acqmodel.Quantity >= inven.SubQuantity)
                                {
                                    double quotient = (int)acqmodel.Quantity / (int)inven.SubQuantity;
                                    double ceiling = Math.Ceiling(quotient);
                                    quantity_Exsits =((int)ceiling > 0)?true:false;
                                }
                                resrvd_quantity = acqmodel.Quantity;
                            }
                            else
                            {
                                resrvd_quantity = (acqmodel.Quantity > acqmodel.Quantity) ? Convert.ToInt32(acqmodel.Quantity) : Convert.ToInt32(acqmodel.Quantity);
                            }

                            if (inven.CanbeBreakable)
                            {
                                if (!acqmodel.IsSubQuantity)
                                {
                                    requed_quan = (int)inven.SubQuantity * (int)acqmodel.Quantity;
                                }
                                exists_quan = (int)inven.SubQuantity * (int)acqmodel.AvailableQuantity;
                            }

                            //Full Requested Quantity Doesn't Exists In Store

                            if (requed_quan > exists_quan)
                            {
                                int req_quantity = Convert.ToInt32(requed_quan) - Convert.ToInt32(exists_quan);
                                Acquisition_List acq_busi_order = new Acquisition_List();
                                if (acqmodel.AcquisitionType == 2)
                                {
                                    if (db.Acquisition_List.Where(m => m.BusinessOrderId == acqmodel.BusinessOrderId && m.AcquisitionType == 1 && m.AcquisitionStatus == 0).Any())
                                    {
                                        acq_busi_order = db.Acquisition_List.Where(m => m.BusinessOrderId == acqmodel.BusinessOrderId && m.AcquisitionType == 1 && m.AcquisitionStatus == 0).FirstOrDefault();
                                    }
                                    else
                                    {
                                        acq_busi_order.AcquisitionType = 1;
                                        acq_busi_order.BusinessOrderId = acqmodel.BusinessOrderId;
                                        acq_busi_order.Name = acqmodel.Name;
                                        acq_busi_order.CreatedBy = acqmodel.CreatedBy;
                                        acq_busi_order.CreatedDate = now;
                                        acq_busi_order.AcquisitionStatus = 2;
                                        acq_busi_order.SiteId = db.UserInformations.Where(m => m.UserId == acqmodel.CreatedBy).Select(m => m.SiteId).FirstOrDefault();
                                        db.Acquisition_List.Add(acq_busi_order);
                                        db.SaveChanges();
                                        acq_busi_order.AcquisitionOrderId = db.Acquisition_List
                                            .Where(m => m.BusinessOrderId == acqmodel.BusinessOrderId && m.AcquisitionType == 1 && m.AcquisitionStatus == 2 && m.CreatedBy == acqmodel.CreatedBy)
                                                                           .Max(m => m.AcquisitionOrderId);
                                    }
                                }
                                else
                                {
                                    var machine_name = db.MachineLists.Where(m => m.MachineId == acqmodel.MachineId).Select(m => m.MachineAcronym).FirstOrDefault();
                                    if (db.Acquisition_List.Where(m => m.MachineId == acqmodel.MachineId && m.AcquisitionType == 1 && m.AcquisitionStatus == 0).Any())
                                    {
                                        acq_busi_order = db.Acquisition_List.Where(m => m.BusinessOrderId == acqmodel.BusinessOrderId && m.AcquisitionType == 1 && m.AcquisitionStatus == 0).FirstOrDefault();
                                    }
                                    else
                                    {
                                        acq_busi_order.AcquisitionType = 1;
                                        acq_busi_order.MachineId = acqmodel.BusinessOrderId;
                                        acq_busi_order.Name = "Requisiton from machine " + machine_name;
                                        acq_busi_order.CreatedBy = acqmodel.CreatedBy;
                                        acq_busi_order.CreatedDate = now;
                                        acq_busi_order.AcquisitionStatus = 2;
                                        acq_busi_order.SiteId = db.UserInformations.Where(m => m.UserId == acqmodel.CreatedBy).Select(m => m.SiteId).FirstOrDefault();
                                        db.Acquisition_List.Add(acq_busi_order);
                                        db.SaveChanges();
                                        acq_busi_order.AcquisitionOrderId = db.Acquisition_List
                                            .Where(m => m.MachineId == acqmodel.MachineId && m.AcquisitionType == 1 && m.CreatedBy == acqmodel.CreatedBy).Max(m => m.AcquisitionOrderId);
                                    }
                                }
                                Acquisition busi_acq_item = new Acquisition();
                                if (db.Acquisitions.Where(m => m.ProductId == acqmodel.InventoryId && m.AcquisitionOrderId == acq_busi_order.AcquisitionOrderId).Any())
                                {
                                    busi_acq_item = db.Acquisitions.Where(m => m.ProductId == acqmodel.InventoryId && m.AcquisitionOrderId == acq_busi_order.AcquisitionOrderId).FirstOrDefault();
                                    busi_acq_item.Quantity = busi_acq_item.Quantity + req_quantity;
                                    busi_acq_item.OrderStatus = 0;
                                    db.Entry(busi_acq_item).State = EntityState.Modified;
                                }
                                else
                                {
                                    busi_acq_item.AcquisitionOrderId = acq_busi_order.AcquisitionOrderId;
                                    busi_acq_item.ProductId = acqmodel.InventoryId;
                                    busi_acq_item.ProductName = acqmodel.ProductName;
                                    busi_acq_item.Quantity = req_quantity;
                                    busi_acq_item.IsSubQuantity = acqmodel.IsSubQuantity;
                                    busi_acq_item.UnitId = acqmodel.UnitId;
                                    busi_acq_item.IsAsap = acqmodel.IsAsap;
                                    busi_acq_item.RequiredDate = acqmodel.RequiredDate;
                                    busi_acq_item.RequiredTime = acqmodel.RequiredTime;
                                    busi_acq_item.CreatedBy = acqmodel.CreatedBy;
                                    busi_acq_item.CreatedDate = now;
                                    busi_acq_item.OrderStatus = 0;
                                    db.Acquisitions.Add(busi_acq_item);
                                }
                                db.SaveChanges();
                            }
                            //Reserved Inventory Item For Business Order Acquisition
                            if (quantity_Exsits)
                            {
                                Inventory_Reserved_Item_Lists resvd = new Inventory_Reserved_Item_Lists();
                                resvd.InventoryId = acqmodel.InventoryId;
                                resvd.Quantity = resrvd_quantity;
                                resvd.IsSubUnit = acqmodel.IsSubQuantity;
                                resvd.IsAssigned = false;
                                resvd.AcquisitionOrderId = acqorderId;
                                resvd.ReserveredBy = acqmodel.CreatedBy;
                                resvd.ReserveredDate = now;
                                db.Inventory_Reserved_Item_Lists.Add(resvd);
                                try
                                {
                                    db.SaveChanges();
                                    msg = "Inventory item successfully reserved for " + acqlist.Name + " acquisition .";
                                    resvd_id = db.Inventory_Reserved_Item_Lists.Where(m => m.ReserveredBy == acqmodel.CreatedBy && m.AcquisitionOrderId == acqorderId).Max(m => m.ReservedId);
                                }
                                catch (Exception ex)
                                {
                                    string errMsg = ex.ToString();
                                    msg = "Inventory item reserve for " + acqlist.Name + " acquisition  unsuccessful.";
                                }
                                SaveAuditLog("Inventory item reserved", msg, "Inventory", "New Acquisition", "Acquisition", resvd_id, acqmodel.CreatedBy, now, OperationStatus);
                            }
                        }
                    }
                    else
                    {
                        acq.ProductTypeId = acqmodel.ProductTypeId;
                        acq.Country = acqmodel.Country;
                        acq.Brand = acqmodel.BrandId;
                        acq.Model = acqmodel.Model;
                        acq.Description = acqmodel.Comment;
                    }
                    acq.ProductName = acqmodel.ProductName;
                    acq.Quantity = acqmodel.Quantity;
                    acq.IsSubQuantity = acqmodel.IsSubQuantity;
                    acq.UnitId = acqmodel.UnitId;
                    acq.IsAsap = acqmodel.IsAsap;
                    if (acqmodel.IsAsap)
                    {
                        acq.AspTime = acqmodel.ASPTime;
                        acq.AspDate = now.AddHours(Convert.ToInt32(acqmodel.ASPTime));
                    }
                    else
                    {
                        acq.RequiredDate = Convert.ToDateTime(acqmodel.RequiredDate);
                        acq.RequiredTime = acqmodel.RequiredTime;
                    }
                    acq.CreatedBy = acqmodel.CreatedBy;
                    acq.CreatedDate = now;
                    acq.OrderStatus = 0;
                    db.Acquisitions.Add(acq);
                    try
                    {
                        OperationStatus = 1;
                        db.SaveChanges();
                        msg = "New product " + acq.ProductName + " for indent order " + acqlist.Name + " added successfully.";
                        ColumnId = db.Acquisitions.Where(m => m.CreatedBy == acqmodel.CreatedBy && m.AcquisitionOrderId == acq.AcquisitionOrderId).Max(m => m.AcquisitionId);
                    }
                    catch (Exception ex)
                    {
                        string errMsg = ex.ToString();
                        msg = "New product " + acq.ProductName + " for indent order " + acqlist.Name + " addition unsuccessful.";
                        OperationStatus = -1;
                    }
                    SaveAuditLog("Add New Product In indent Order", msg, "Inventory", "New Acquisition", "Acquisition", ColumnId, acqmodel.CreatedBy, now, OperationStatus);
                    if (OperationStatus == 1)
                    {
                        int count = 0;
                        if (files != null)
                        {
                            foreach (var file in files)
                            {
                                if (file != null && file.ContentLength > 0)
                                {
                                    string s = file.FileName;
                                    int idx = s.LastIndexOf('.');
                                    string fileName = s.Substring(0, idx);
                                    string extension = s.Substring(idx);

                                    string picName = ColumnId +"_"+ count + extension;
                                    string PathForOriginal = Path.Combine(Server.MapPath("~/Images/Inventory/Acquisition/Original"), picName);
                                    file.SaveAs(PathForOriginal);

                                    var imgForGallery = Image.FromStream(file.InputStream, true, true);
                                    using (var resizeImg = ScaleImage(imgForGallery, 465, 371))
                                    {
                                        string Paths = Path.Combine(Server.MapPath("~/Images/Inventory/Acquisition/Resize"), picName);
                                        resizeImg.Save(Paths);
                                    }
                                    AcquisitionImage pic = new AcquisitionImage();
                                    pic.AcquisitionId = ColumnId;
                                    pic.ImageName = picName;
                                    pic.ImageOriginalName = file.FileName;
                                    db.AcquisitionImages.Add(pic);
                                    db.SaveChanges();
                                }
                                count++;
                            }
                        }

                    }

                    var acqorder = db.Acquisition_List.Find(acq.AcquisitionOrderId);
                    acqmodel.AcquisitionOrderId = acq.AcquisitionOrderId;
                    acqmodel.Name = acqorder.Name;
                    return Json(acqorder, JsonRequestBehavior.AllowGet);
                }
            }
            return Json("Error",JsonRequestBehavior.AllowGet);       
        }
        public JsonResult AcquisitionSaveEdit(AcquisitionEditViewModel acqmodel, IEnumerable<HttpPostedFileBase> files)
        {
            string msg = "";
            string intro = "";
            int ColumnId = 0;
            int OperationStatus = 1;
            Acquisition acq = new Acquisition();
            InventoryList inven = new InventoryList();
            if (ModelState.IsValid)
            {
                acq = db.Acquisitions.Find(acqmodel.AcquisitionId);
                acq.ProductName = acqmodel.ProductNameEdit;
                acq.ProductId = acqmodel.ProductId;
                acq.ProductTypeId = acqmodel.ProductTypeIdEdit;
                acq.Country = acqmodel.Country;
                acq.Brand = acqmodel.BrandIdEdit;
                acq.Model = acqmodel.ModelEdit;
                acq.IsAsap = acqmodel.IsAsapEdit;
                acq.Quantity = acqmodel.QuantityEdit;
                acq.UnitId = acqmodel.UnitIdEdit;
                acq.IsAsap = acqmodel.IsAsapEdit;
                if (acqmodel.IsAsapEdit)
                {
                    acq.AspTime = acqmodel.ASPTimeEdit;
                    acq.AspDate = now.AddHours(Convert.ToInt32(acqmodel.ASPTimeEdit));
                }
                else
                {
                    acq.RequiredDate = acqmodel.RequiredDateEdit;
                    acq.RequiredTime = acqmodel.RequiredTimeEdit;
                }
                acq.Description = acqmodel.CommentEdit;
                acq.UpdatedBy = acqmodel.CreatedBy;
                acq.UpdatedDate = now;

                try
                {
                    db.Entry(acq).State = EntityState.Modified;
                    db.SaveChanges();

                    if (acqmodel.ProductId > 0)
                    {
                        if (acqmodel.AcquisitionType == 2)
                        {
                            if (acqmodel.QuantityEdit > acqmodel.Available_Quantity)
                            {
                                int req_quantity = Convert.ToInt32(acqmodel.QuantityEdit) - Convert.ToInt32(acqmodel.Available_Quantity);
                                Acquisition_List acq_busi_order = new Acquisition_List();
                                if (db.Acquisition_List.Where(m => m.BusinessOrderId == acqmodel.BusinessOrderId && m.AcquisitionType == 1 && m.AcquisitionStatus == 0).Any())
                                {
                                    acq_busi_order = db.Acquisition_List.Where(m => m.BusinessOrderId == acqmodel.BusinessOrderId && m.AcquisitionType == 1 && m.AcquisitionStatus == 0).FirstOrDefault();                                    
                                }
                                else
                                {
                                    acq_busi_order.AcquisitionType = 1;
                                    acq_busi_order.BusinessOrderId = acqmodel.BusinessOrderId;
                                    acq_busi_order.Name = acqmodel.Name;
                                    acq_busi_order.CreatedBy = acqmodel.CreatedBy;
                                    acq_busi_order.CreatedDate = now;
                                    acq_busi_order.AcquisitionStatus = 0;
                                    acq_busi_order.SiteId = db.UserInformations.Where(m => m.UserId == acqmodel.CreatedBy).Select(m => m.SiteId).FirstOrDefault();
                                    db.Acquisition_List.Add(acq_busi_order);
                                    db.SaveChanges();
                                    acq_busi_order.AcquisitionOrderId = db.Acquisition_List
                                                                       .Where(m => m.BusinessOrderId == acqmodel.BusinessOrderId && m.AcquisitionType == 1 && m.AcquisitionStatus == 0 && m.CreatedBy == acqmodel.CreatedBy)
                                                                       .Max(m => m.AcquisitionOrderId);
                                }
                                if (db.Acquisitions.Where(m => m.AcquisitionOrderId == acq_busi_order.AcquisitionOrderId && m.ProductId == acqmodel.ProductId).Any())
                                {
                                    Acquisition busi_acq_item = db.Acquisitions
                                        .Where(m => m.AcquisitionOrderId == acq_busi_order.AcquisitionOrderId && m.ProductId == acqmodel.ProductId).FirstOrDefault();
                                    busi_acq_item.Quantity = req_quantity;
                                    db.Entry(busi_acq_item).State = EntityState.Modified;
                                    db.SaveChanges();
                                }
                                else
                                {
                                    InventoryList invenItem = db.InventoryLists.Find(acqmodel.ProductId);
                                    Acquisition busi_acq_item = new Acquisition();
                                    busi_acq_item.AcquisitionOrderId = acq_busi_order.AcquisitionOrderId;
                                    busi_acq_item.ProductId = acqmodel.ProductId;
                                    busi_acq_item.ProductName = invenItem.ProductName;
                                    busi_acq_item.Quantity = req_quantity;
                                    busi_acq_item.UnitId = invenItem.UnitId;
                                    busi_acq_item.IsAsap = false;
                                    busi_acq_item.CreatedBy = acqmodel.CreatedBy;
                                    busi_acq_item.CreatedDate = now;
                                    busi_acq_item.OrderStatus = 0;
                                    db.Acquisitions.Add(busi_acq_item);
                                    db.SaveChanges();
                                }
                                
                            }
                            else
                            {
                                if (db.Acquisition_List.Where(m => m.BusinessOrderId == acqmodel.BusinessOrderId && m.AcquisitionType == 1 && m.AcquisitionStatus == 0).Any())
                                {
                                    int acq_busi_ordId = db.Acquisition_List.Where(m => m.BusinessOrderId == acqmodel.BusinessOrderId && m.AcquisitionType == 1 && m.AcquisitionStatus == 0).Select(m => m.AcquisitionOrderId).FirstOrDefault();
                                    if (db.Acquisitions.Where(m => m.AcquisitionOrderId == acq_busi_ordId && m.ProductId == acqmodel.ProductId).Any())
                                    {
                                        Acquisition busi_acq_item = db.Acquisitions.Where(m => m.AcquisitionOrderId == acq_busi_ordId && m.ProductId == acqmodel.ProductId).FirstOrDefault();
                                        db.Acquisitions.Remove(busi_acq_item);
                                        db.SaveChanges();


                                        if (!db.Acquisitions.Where(m => m.AcquisitionOrderId == acq_busi_ordId).Any())
                                        {
                                            Acquisition_List main_order = db.Acquisition_List.Find(acq_busi_ordId);
                                            db.Acquisition_List.Remove(main_order);
                                            db.SaveChanges();
                                        }
                                    }
                                }
                            }
                            
                        }

                        //Reserved Inventory Item Quantity Changed For Business Order Acquisition

                        if (db.Inventory_Reserved_Item_Lists.Where(m => m.AcquisitionOrderId == acqmodel.AcquisitionOrderId && m.InventoryId == acqmodel.ProductId).Any())
                        {
                            int resvd_id = 0;
                            int resrvd_quantity = (acqmodel.QuantityEdit > acqmodel.Available_Quantity) ? Convert.ToInt32(acqmodel.Available_Quantity) : Convert.ToInt32(acqmodel.QuantityEdit);

                            Inventory_Reserved_Item_Lists resvd = db.Inventory_Reserved_Item_Lists.Where(m => m.AcquisitionOrderId == acqmodel.AcquisitionOrderId && m.InventoryId == acqmodel.ProductId).FirstOrDefault();
                            resvd.Quantity = resrvd_quantity;
                            resvd.IsAssigned = false;
                            resvd.ReserveredBy = acqmodel.CreatedBy;
                            resvd.ReserveredDate = now;
                            db.Entry(resvd).State = EntityState.Modified;
                            try
                            {
                                db.SaveChanges();
                                msg = "Reserved Inventory item quantity successfully updated .";
                                resvd_id = resvd.ReservedId;
                            }
                            catch (Exception ex)
                            {
                                string errMsg = ex.ToString();
                                msg = "Reserved Inventory item quantity update unsuccessful.";
                            }
                            SaveAuditLog("Inventory Item Reserved", msg, "Inventory", "New Acquisition", "Acquisition", resvd_id, acqmodel.CreatedBy, now, OperationStatus);
                        }

                    }

                    int count = 0;
                    if (files != null)
                    {
                        foreach (var file in files)
                        {
                            if (file != null && file.ContentLength > 0)
                            {
                                string s = file.FileName;
                                int idx = s.LastIndexOf('.');
                                string fileName = s.Substring(0, idx);
                                string extension = s.Substring(idx);

                                string picName = acq.AcquisitionId + acq.AcquisitionOrderId + count + extension;
                                string PathForOriginal = Path.Combine(Server.MapPath("~/Images/Inventory/Acquisition/Original"), picName);
                                file.SaveAs(PathForOriginal);

                                var imgForGallery = Image.FromStream(file.InputStream, true, true);
                                using (var resizeImg = ScaleImage(imgForGallery, 465, 371))
                                {
                                    string Paths = Path.Combine(Server.MapPath("~/Images/Inventory/Acquisition/Resize"), picName);
                                    resizeImg.Save(Paths);
                                }
                                AcquisitionImage pic = new AcquisitionImage();
                                pic.AcquisitionId = acq.AcquisitionId;
                                pic.ImageName = picName;
                                pic.ImageOriginalName = file.FileName;
                                db.AcquisitionImages.Add(pic);
                                db.SaveChanges();
                            }
                            count++;
                        }
                    }
                    intro = "Update Product Info In An Acquisition Order";
                    msg = "Update product " + acq.ProductName + " in acquisition order " + acqmodel.Name + " updated.";
                    ColumnId = acqmodel.AcquisitionId;
                }
                catch (Exception ex)
                {
                    string errrorMsg = ex.Message.ToString();
                    intro = "Update Product Info In An Acquisition Order";
                    msg = "Update product " + acq.ProductName + "for acquisition order " + acqmodel.Name + " update attempt unsuccessful.";
                    OperationStatus = -1;
                    ColumnId = 0;
                }
                SaveAuditLog(intro, msg, "Inventory", "New Acquisition", "Acquisition", ColumnId, acqmodel.CreatedBy, now, OperationStatus);
                var acqorder = db.Acquisition_List.Find(acq.AcquisitionOrderId);
                acqmodel.AcquisitionOrderId = acq.AcquisitionOrderId;
                acqmodel.SiteId = acqorder.SiteId;
                acqmodel.Name = acqorder.Name;
                if (OperationStatus == 1)
                {
                    return Json(acqmodel, JsonRequestBehavior.AllowGet);
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

        public JsonResult ProductTypeHasChildern(int ProductTypeId)
        {
            return Json(db.ProductTypeLists.Where(m => m.ParentId == ProductTypeId).Any(), JsonRequestBehavior.AllowGet);
        }
        public JsonResult GetAllProductType(int? ParentId)
        {
            if (ParentId != null)
            {
                int id = Convert.ToInt32(ParentId);
                var list = (from m in db.ProductTypeLists
                            where m.ParentId == id
                            select new
                            {
                                ProductTypeId = m.ProductTypeId,
                                ProductTypeIdEdit = m.ProductTypeId,
                                ProductTypeName = m.ProductTypeName
                            }).ToList();

                return Json(list, JsonRequestBehavior.AllowGet);
            }
            else
            {
                var list = (from m in db.ProductTypeLists
                            where m.HasParent == false
                            select new
                            {
                                ProductTypeId = m.ProductTypeId,
                                ProductTypeIdEdit = m.ProductTypeId,
                                ProductTypeName = m.ProductTypeName
                            }).ToList();

                return Json(list, JsonRequestBehavior.AllowGet);
            }

        }
        public JsonResult BrandHasChildern(int BrandId)
        {
            return Json(db.BrandLists.Where(m => m.ParentId == BrandId).Any(), JsonRequestBehavior.AllowGet);
        }
        public JsonResult GetAllBrand(int? ParentId)
        {
            if (ParentId != null)
            {
                int id = Convert.ToInt32(ParentId);
                var list = (from m in db.BrandLists
                            where m.ParentId == id
                            select new
                            {
                                BrandId = m.BrandId,
                                BrandIdEdit = m.BrandId,
                                BrandName = m.BrandName
                            }).ToList();

                return Json(list, JsonRequestBehavior.AllowGet);
            }
            else
            {
                var list = (from m in db.BrandLists
                            where m.HasParent == false
                            select new
                            {
                                BrandId = m.BrandId,
                                BrandIdEdit = m.BrandId,
                                BrandName = m.BrandName
                            }).ToList();

                return Json(list, JsonRequestBehavior.AllowGet);
            }

        }
        public JsonResult UnitHasChildern(int UnitId)
        {
            return Json(db.UnitLists.Where(m => m.ParentId == UnitId).Any(), JsonRequestBehavior.AllowGet);
        }
        public JsonResult GetAllUnit(int? ParentId)
        {
            if (ParentId != null)
            {
                int id = Convert.ToInt32(ParentId);
                var list = (from m in db.UnitLists
                            where m.ParentId == id
                            select new
                            {
                                UnitId = m.UnitId,
                                UnitIdEdit = m.UnitId,
                                UnitName = m.UnitName
                            }).ToList();

                return Json(list, JsonRequestBehavior.AllowGet);
            }
            else
            {
                var list = (from m in db.UnitLists
                            where m.HasParentId == false
                            select new
                            {
                                UnitId = m.UnitId,
                                UnitIdEdit = m.UnitId,
                                UnitName = m.UnitName
                            }).ToList();

                return Json(list, JsonRequestBehavior.AllowGet);
            }

        }

        public JsonResult DeleteAcqByOrder(int? AcquisitionOrderId, long UserId)
        {
            string msg = "";
            string name = "";
            int ColumnId = 0;
            int OperationStatus = 1;
            if (AcquisitionOrderId != null && db.Acquisition_List.Where(m => m.AcquisitionOrderId == AcquisitionOrderId).Any())
            {
                Acquisition_List order = db.Acquisition_List.Find(AcquisitionOrderId);

                if (order.BusinessOrderId != null && db.Acquisition_List.Where(m => m.AcquisitionType == 1 && m.BusinessOrderId == order.BusinessOrderId && m.AcquisitionStatus == 0).Any())
                {
                    Acquisition_List inven_order = db.Acquisition_List.Where(m => m.AcquisitionType == 1 && m.BusinessOrderId == order.BusinessOrderId && m.AcquisitionStatus == 0).FirstOrDefault();
                    name = inven_order.Name;
                    ColumnId = inven_order.AcquisitionOrderId;
                    var inven_order_pro = db.Acquisitions.Where(m => m.AcquisitionOrderId == inven_order.AcquisitionOrderId).ToList();
                    db.Acquisition_List.Remove(inven_order);
                    try
                    {
                        db.SaveChanges();
                        foreach (var p in inven_order_pro)
                        {
                            var acqImg = db.AcquisitionImages.Where(m => m.AcquisitionId == p.AcquisitionId).ToList();
                            db.Acquisitions.Remove(p);
                            db.SaveChanges();
                        }
                        OperationStatus = 1;
                        msg = "Acquisition order " + name + " deleted successfully";
                    }
                    catch (Exception ex)
                    {
                        string errmsg = ex.ToString();
                        msg = "Acquisition order " + name + " deletion unsuccessful";
                        OperationStatus = -1;
                    }
                    SaveAuditLog("Delete Acquisition Order", msg, "Inventory", "AllAcquisitionList", "Acquisition", ColumnId, UserId, now, OperationStatus);
                }

                var productlist = db.Acquisitions.Where(m => m.AcquisitionOrderId == AcquisitionOrderId).ToList();
                var reservedList = db.Inventory_Reserved_Item_Lists.Where(m => m.AcquisitionOrderId == AcquisitionOrderId).ToList();
                name = order.Name;
                ColumnId = order.AcquisitionOrderId;
                try
                {
                    msg ="Acquisition order "+ name + " deleted successfully";
                    db.Acquisition_List.Remove(order);
                    db.SaveChanges();

                    //All Requested Product Delete
                    if (productlist.Count > 0)
                    {
                        foreach (var p in productlist)
                        {
                            var acqImg = db.AcquisitionImages.Where(m => m.AcquisitionId == p.AcquisitionId).ToList();
                            db.Acquisitions.Remove(p);
                            db.SaveChanges();
                            if (acqImg.Count > 0)
                            {
                                foreach (var m in acqImg)
                                {
                                    db.AcquisitionImages.Remove(m);
                                    db.SaveChanges();
                                    string PathForOriginal = Path.Combine(Server.MapPath("~/Images/Inventory/Acquisition/Original"), m.ImageName);
                                    string Paths = Path.Combine(Server.MapPath("~/Images/Inventory/Acquisition/Resize"), m.ImageName);
                                    if (System.IO.File.Exists(PathForOriginal) && System.IO.File.Exists(Paths))
                                    {
                                        System.IO.File.Delete(PathForOriginal);
                                        System.IO.File.Delete(Paths);
                                    }
                                }
                            }
                        }
                    }
                    //All Reserved Product Delete
                    if (reservedList.Count > 0)
                    {
                        foreach (var r in reservedList)
                        {
                            db.Inventory_Reserved_Item_Lists.Remove(r);
                            db.SaveChanges();
                        }
                    }

                    OperationStatus = 1;
                }
                catch (Exception ex)
                {
                    string errorMsg = ex.Message.ToString();
                    msg ="Acquisition order "+ name + " deletion unsuccessful";
                    OperationStatus = -1;
                }
                SaveAuditLog("Delete Acquisition Order", msg, "Inventory", "AllAcquisitionList", "Acquisition", ColumnId, UserId, now, OperationStatus);
                if (OperationStatus == 1)
                {
                    return Json("Success", JsonRequestBehavior.AllowGet);
                }
                
            }
            return Json("Error", JsonRequestBehavior.AllowGet);
        }
        public JsonResult CheckAcqProductCount(int? AcquisitionOrderId)
        {
            var list = db.Acquisitions.Where(m => m.AcquisitionOrderId == AcquisitionOrderId).Count();
            return Json(list, JsonRequestBehavior.AllowGet);
        }
        public JsonResult DeleteAcq(int? Id, long UserId, string OrderName,int? BusiId)
        {
            string msg = "";
            string name = "";
            string proName = "";
            int ColumnId = 0;
            int OperationStatus = 1;
            if (Id == null)
            {
                return Json("Error", JsonRequestBehavior.AllowGet);
            }
            else
            {
                Acquisition acqlist = db.Acquisitions.Find(Id);
                if (acqlist != null)
                {
                    proName = acqlist.ProductName;
                    ColumnId = acqlist.AcquisitionId;
                    int orderid = acqlist.AcquisitionOrderId;
                    var acqImg = db.AcquisitionImages.Where(m => m.AcquisitionId == Id).ToList();
                    if (BusiId != null && acqlist.ProductId != null && db.Acquisition_List.Where(m => m.AcquisitionType == 1 && m.BusinessOrderId == BusiId).Any())
                    {
                        int acqOrdId = db.Acquisition_List.Where(m => m.AcquisitionType == 1 && m.BusinessOrderId == BusiId).Select(m => m.AcquisitionOrderId).FirstOrDefault();
                        if (db.Acquisitions.Where(m => m.AcquisitionOrderId == acqOrdId && m.ProductId == acqlist.ProductId).Any())
                        {
                            var invnAcqOrd = db.Acquisitions.Where(m => m.AcquisitionOrderId == acqOrdId && m.ProductId == acqlist.ProductId).FirstOrDefault();
                            db.Acquisitions.Remove(invnAcqOrd);
                            db.SaveChanges();
                        }
                        if (!db.Acquisitions.Where(m => m.AcquisitionOrderId == acqOrdId).Any())
                        {
                            Acquisition_List acqOrder = db.Acquisition_List.Find(acqOrdId);
                            db.Acquisition_List.Remove(acqOrder);
                            name = acqOrder.Name;
                            try
                            {
                                db.SaveChanges();
                                OperationStatus = 1;
                                msg = name + " order deleted successfully";
                            }
                            catch (Exception ex)
                            {
                                string errorMsg = ex.Message.ToString();
                                OperationStatus = -1;
                                msg = name + " order deletion unsuccessful";
                            }
                            SaveAuditLog("Delete Acquisition Order", msg, "Inventory", "AllAcquisitionList", "Acquisition", ColumnId, UserId, now, OperationStatus);
                        }
                    }
                    if (db.Inventory_Reserved_Item_Lists.Where(m => m.AcquisitionOrderId == acqlist.AcquisitionOrderId && m.InventoryId == acqlist.ProductId).Any())
                    {
                        var reserved = db.Inventory_Reserved_Item_Lists.Where(m => m.AcquisitionOrderId == acqlist.AcquisitionOrderId && m.InventoryId == acqlist.ProductId).FirstOrDefault();
                        db.Inventory_Reserved_Item_Lists.Remove(reserved);
                        db.SaveChanges();
                    }
                    try
                    {
                        msg = proName + " product from acqusition order "+OrderName+" deleted successfully";
                        db.Acquisitions.Remove(acqlist);
                        db.SaveChanges();
                        if (acqImg.Count > 0)
                        {
                            foreach (var m in acqImg)
                            {
                                db.AcquisitionImages.Remove(m);
                                db.SaveChanges();
                                string PathForOriginal = Path.Combine(Server.MapPath("~/Images/Inventory/Acquisition/Original"), m.ImageName);
                                string Paths = Path.Combine(Server.MapPath("~/Images/Inventory/Acquisition/Resize"), m.ImageName);
                                if (System.IO.File.Exists(PathForOriginal) && System.IO.File.Exists(Paths))
                                {
                                    System.IO.File.Delete(PathForOriginal);
                                    System.IO.File.Delete(Paths);
                                }
                            }

                        }
                        if (!db.Acquisitions.Where(m => m.AcquisitionOrderId == orderid).Any())
                        {
                            Acquisition_List acqOrder = db.Acquisition_List.Find(orderid);
                            db.Acquisition_List.Remove(acqOrder);
                            name = acqOrder.Name;
                            try
                            {
                                db.SaveChanges();
                                OperationStatus = 1;
                                msg = name + " order deleted successfully";
                            }
                            catch (Exception ex)
                            {
                                string errorMsg = ex.Message.ToString();
                                OperationStatus = -1;
                                msg = name + " order deletion unsuccessful";
                            }
                        }
                        SaveAuditLog("Delete Acquisition Order", msg, "Inventory", "AllAcquisitionList", "Acquisition", ColumnId, UserId, now, OperationStatus);
                        OperationStatus = 1;
                    }
                    catch (Exception ex)
                    {
                        string errorMsg = ex.Message.ToString();
                        msg = proName + "  product from acqusition order " + OrderName + " deletion unsuccessful";
                        OperationStatus = -1;
                    }
                }
                SaveAuditLog("Delete Product From Acquisition ", msg, "Inventory", "AllAcquisitionList", "Acquisition", ColumnId, UserId, now, OperationStatus);
                return Json("Success", JsonRequestBehavior.AllowGet);
            }
        }
        
        #endregion

        #region All TYPE OF ACQUISITION LIST
        public ActionResult AcquisitionOrderList(int? pageNum)
        {
            pageNum = pageNum ?? 0;
            int page = Convert.ToInt32(pageNum);
            ViewBag.pageNum = pageNum;
            var allOrder = db.Acquisition_List.Where(m => m.AcquisitionStatus > -1).Count();
            ViewBag.TotalAcquisitionOrder = allOrder;
            ViewBag.OrderStatus = new SelectList(db.OrderStatusLists, "OrderStatus", "Status");

            Session["AllAcquisition"] = db.Acquisition_List.ToList();
            return View();
        }
        public ActionResult _ShowAcquisitionOrderList(int? AcqOrderId, int? listName,int acqType)
        {
            if (AcqOrderId != null)
            {
                var allAcqorder = (List<AcquisitionViewModel>)Session["AllAcquisitionOrderBySearch"];
                var allAcqorderBysearch = (from a in allAcqorder
                                           where a.AcquisitionOrderId == AcqOrderId && a.AcquisitionType == acqType
                                           && a.AcquisitionStatus > -1
                                           select new AcquisitionViewModel
                                           {
                                               AcquisitionOrderId = a.AcquisitionOrderId,
                                               AcquisitionType = a.AcquisitionType,
                                               BusinessOrderId = a.BusinessOrderId,
                                               MachineId = a.MachineId,
                                               PartsId = a.PartsId,
                                               Name = a.Name,
                                               SiteId = a.SiteId,
                                               AcquisitionStatus = a.AcquisitionStatus,
                                               Status = a.Status,
                                               CreatedBy = a.CreatedBy,
                                               CreatedDate = a.CreatedDate,
                                               CreatedDateName = a.CreatedDateName,
                                               UserName = (a.CreatedBy == 1) ? "Super Admin" :
                                              db.UserInformations.Where(m => m.UserId == a.CreatedBy).Select(m => m.FirstName + " " + m.MiddleName + " " + m.LastName).FirstOrDefault(),
                                               PictureName = db.UserInformations.Where(m => m.UserId == a.CreatedBy).Select(m => m.Picture).FirstOrDefault()
                                           }).ToList();
                ViewBag.TotalAcquisitionOrder = allAcqorderBysearch.Count();
                ViewBag.AcquisitionOrderList = allAcqorderBysearch.Take(ShowItemPerPage).ToList();
            }
            else
            {
                if (listName != null)
                {
                    var allAcqOrder = (from a in db.Acquisition_List
                                       join s in db.OrderStatusLists
                                       on a.AcquisitionStatus equals s.OrderStatus
                                       where a.AcquisitionStatus == listName && a.AcquisitionType == acqType && a.AcquisitionStatus > -1
                                       select new AcquisitionViewModel
                                       {
                                           AcquisitionOrderId = a.AcquisitionOrderId,
                                           AcquisitionType = a.AcquisitionType,
                                           BusinessOrderId = a.BusinessOrderId,
                                           MachineId = a.MachineId,
                                           PartsId = a.PartsId,
                                           Name = a.Name,
                                           SiteId = a.SiteId,
                                           AcquisitionStatus = a.AcquisitionStatus,
                                           Status = s.Status,
                                           CreatedBy = a.CreatedBy,
                                           CreatedDate = a.CreatedDate,
                                           CreatedDateName = DbFunctions.TruncateTime(a.CreatedDate).ToString(),
                                           UserName = (a.CreatedBy == 1) ? "Super Admin" :
                                           db.UserInformations.Where(m => m.UserId == s.CreatedBy).Select(m => m.FirstName + " " + m.MiddleName + " " + m.LastName).FirstOrDefault(),
                                           PictureName = db.UserInformations.Where(m => m.UserId == a.CreatedBy).Select(m => m.Picture).FirstOrDefault()
                                       }).OrderByDescending(m => m.CreatedDate).ToList();
                    ViewBag.TotalAcquisitionOrder = allAcqOrder.Count();
                    ViewBag.AcquisitionOrderList = allAcqOrder.Take(ShowItemPerPage).ToList();
                    Session["AllAcquisitionOrder"] = allAcqOrder;
                }
                else
                {
                    var allAcqOrder = (from a in db.Acquisition_List
                                       join s in db.OrderStatusLists
                                       on a.AcquisitionStatus equals s.OrderStatus
                                       where a.AcquisitionType == acqType && a.AcquisitionStatus > -1
                                       select new AcquisitionViewModel
                                       {
                                           AcquisitionOrderId = a.AcquisitionOrderId,
                                           AcquisitionType = a.AcquisitionType,
                                           BusinessOrderId = a.BusinessOrderId,
                                           MachineId = a.MachineId,
                                           PartsId = a.PartsId,
                                           Name = a.Name,
                                           SiteId = a.SiteId,
                                           AcquisitionStatus = a.AcquisitionStatus,
                                           Status = s.Status,
                                           CreatedBy = a.CreatedBy,
                                           CreatedDate = a.CreatedDate,
                                           CreatedDateName = DbFunctions.TruncateTime(a.CreatedDate).ToString(),
                                           UserName = (a.CreatedBy == 1) ? "Super Admin" :
                                           db.UserInformations.Where(m => m.UserId == s.CreatedBy).Select(m => m.FirstName + " " + m.MiddleName + " " + m.LastName).FirstOrDefault(),
                                           PictureName = db.UserInformations.Where(m => m.UserId == a.CreatedBy).Select(m => m.Picture).FirstOrDefault()
                                       }).OrderByDescending(m => m.CreatedDate).ToList();
                    ViewBag.TotalAcquisitionOrder = allAcqOrder.Count();
                    ViewBag.AcquisitionOrderList = allAcqOrder.Take(ShowItemPerPage).ToList();
                    Session["AllAcquisitionOrder"] = allAcqOrder;
                    Session["TotalAcquisitionOrderCount"] = allAcqOrder.Count();
                }
            }
            return PartialView(ViewBag.AcquisitionOrderList);
        }
        public PartialViewResult _RequisitonList(int type,int reqType)
        {
            if (type == 1)
            {
                var allAcqOrder = (from a in db.Acquisition_List
                                   join s in db.OrderStatusLists
                                   on a.AcquisitionStatus equals s.OrderStatus
                                   where a.AcquisitionType == reqType && a.AcquisitionStatus > -1
                                   select new AcquisitionViewModel
                                   {
                                       AcquisitionOrderId = a.AcquisitionOrderId,
                                       AcquisitionType = a.AcquisitionType,
                                       BusinessOrderId = a.BusinessOrderId,
                                       MachineId = a.MachineId,
                                       PartsId = a.PartsId,
                                       Name = a.Name,
                                       SiteId = a.SiteId,
                                       AcquisitionStatus = a.AcquisitionStatus,
                                       Status = s.Status,
                                       CreatedBy = a.CreatedBy,
                                       CreatedDate = a.CreatedDate,
                                       CreatedDateName = DbFunctions.TruncateTime(a.CreatedDate).ToString(),
                                       UserName = (a.CreatedBy == 1) ? "Super Admin" :
                                       db.UserInformations.Where(m => m.UserId == s.CreatedBy).Select(m => m.FirstName + " " + m.MiddleName + " " + m.LastName).FirstOrDefault(),
                                       PictureName = db.UserInformations.Where(m => m.UserId == a.CreatedBy).Select(m => m.Picture).FirstOrDefault()
                                   }).OrderByDescending(m => m.CreatedDate).ToList();
                ViewBag.Title = "All Requisiton From Inventory";
                return PartialView(allAcqOrder);
            }
            else if (type == 2)
            {
                var allAcqOrder = (from a in db.Acquisition_List
                                   join s in db.OrderStatusLists
                                   on a.AcquisitionStatus equals s.OrderStatus
                                   where a.AcquisitionType == reqType && a.AcquisitionStatus > -1 && a.BusinessOrderId > 0
                                   select new AcquisitionViewModel
                                   {
                                       AcquisitionOrderId = a.AcquisitionOrderId,
                                       AcquisitionType = a.AcquisitionType,
                                       BusinessOrderId = a.BusinessOrderId,
                                       MachineId = a.MachineId,
                                       PartsId = a.PartsId,
                                       Name = a.Name,
                                       SiteId = a.SiteId,
                                       AcquisitionStatus = a.AcquisitionStatus,
                                       Status = s.Status,
                                       CreatedBy = a.CreatedBy,
                                       CreatedDate = a.CreatedDate,
                                       CreatedDateName = DbFunctions.TruncateTime(a.CreatedDate).ToString(),
                                       UserName = (a.CreatedBy == 1) ? "Super Admin" :
                                       db.UserInformations.Where(m => m.UserId == s.CreatedBy).Select(m => m.FirstName + " " + m.MiddleName + " " + m.LastName).FirstOrDefault(),
                                       PictureName = db.UserInformations.Where(m => m.UserId == a.CreatedBy).Select(m => m.Picture).FirstOrDefault()
                                   }).OrderByDescending(m => m.CreatedDate).ToList();
                if (reqType == 1)
                {
                    ViewBag.Title = "All Requisiton From Business Order";
                }
                else
                {
                    ViewBag.Title = "All Indent From Business Order";
                }
                return PartialView(allAcqOrder);

            }
            else
            {
                var allAcqOrder = (from a in db.Acquisition_List
                                   join s in db.OrderStatusLists
                                   on a.AcquisitionStatus equals s.OrderStatus
                                   where a.AcquisitionType == reqType && a.AcquisitionStatus > -1 && a.MachineId > 0
                                   select new AcquisitionViewModel
                                   {
                                       AcquisitionOrderId = a.AcquisitionOrderId,
                                       AcquisitionType = a.AcquisitionType,
                                       BusinessOrderId = a.BusinessOrderId,
                                       MachineId = a.MachineId,
                                       PartsId = a.PartsId,
                                       Name = a.Name,
                                       SiteId = a.SiteId,
                                       AcquisitionStatus = a.AcquisitionStatus,
                                       Status = s.Status,
                                       CreatedBy = a.CreatedBy,
                                       CreatedDate = a.CreatedDate,
                                       CreatedDateName = DbFunctions.TruncateTime(a.CreatedDate).ToString(),
                                       UserName = (a.CreatedBy == 1) ? "Super Admin" :
                                       db.UserInformations.Where(m => m.UserId == s.CreatedBy).Select(m => m.FirstName + " " + m.MiddleName + " " + m.LastName).FirstOrDefault(),
                                       PictureName = db.UserInformations.Where(m => m.UserId == a.CreatedBy).Select(m => m.Picture).FirstOrDefault()
                                   }).OrderByDescending(m => m.CreatedDate).ToList();
                if (reqType == 1)
                {
                    ViewBag.Title = "All Requisiton From Machine";
                }
                else
                {
                    ViewBag.Title = "All Indent From Machine";
                }
                return PartialView(allAcqOrder);
            }
        }

        public JsonResult GetAcquisitionOrderList(int? page)
        {
            page = page ?? 0;
            int total = Convert.ToInt16(Session["TotalAcquisitionOrderCount"]);
            int skip = ShowItemPerPage * Convert.ToInt16(page);
            var canPage = skip < total;
            if (canPage)
            {
                var listBackFromSession = (List<AcquisitionViewModel>)Session["AllAcquisitionOrder"];
                var orderlist = listBackFromSession.Skip(skip).Take(ShowItemPerPage).ToList();
                return Json(orderlist, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json("", JsonRequestBehavior.AllowGet);
            }
        }
        public JsonResult AcquisitionOrderSearching(string prefix)
        {
            if (prefix == "")
            {
                return Json("", JsonRequestBehavior.AllowGet);
            }
            else
            {
                var allAcqOrder = (List<AcquisitionViewModel>)Session["AllAcquisitionOrder"];
                var title = (from a in allAcqOrder
                             where (a.Name.ToLower().Contains(prefix.ToLower()))
                             select new AcquisitionViewModel
                             {
                                 AcquisitionOrderId = a.AcquisitionOrderId,
                                 Name = a.Name,
                                 SiteId = a.SiteId,
                                 AcquisitionStatus = a.AcquisitionStatus,
                                 Status = a.Status,
                                 CreatedBy = a.CreatedBy,
                                 CreatedDate = a.CreatedDate,
                                 UserName = a.UserName,
                                 CreatedDateName = a.CreatedDateName,
                                 PictureName = a.PictureName
                             }).ToList();
                Session["AllAcquisitionOrderBySearch"] = title;
                return Json(title, JsonRequestBehavior.AllowGet);
            }
        }


        //******************************** ACQUISITION ORDER STATUS CHANGE FUNCTION ********************************************************

        public JsonResult AcquisitionStatus(int? Id, long UserId, int Status)
        {
            string msg = "";
            string name = "";
            string intro = "";
            int ColumnId = 0;
            int OperationStatus = 1;

            if (Id == null)
            {
                return Json("Error", JsonRequestBehavior.AllowGet);
            }
            else
            {
                Acquisition_List order = db.Acquisition_List.Find(Id);
                if (order != null)
                {
                    name = order.Name;
                    ColumnId = order.AcquisitionOrderId;
                    intro = (Status == 1) ? "Approve Acquisition order" : "Reject Acquisition order";
                    try
                    {
                        order.AcquisitionStatus = Status;
                        db.Acquisition_List.Attach(order);
                        db.Entry(order).State = EntityState.Modified;
                        db.SaveChanges();
                        msg = (Status == 1) ? name + " approved successfully" : name + " rejected successfully";
                        OperationStatus = 1;
                    }
                    catch (Exception ex)
                    {
                        string errorMsg = ex.Message.ToString();
                        msg = (Status == 1) ? name + " approvation unsuccessful" : name + " rejection unsuccessful";
                        OperationStatus = -1;
                    }
                }
                SaveAuditLog(intro, msg, "Inventory", "AllAcquisitionList", "Acquisition", ColumnId, UserId, now, OperationStatus);
                return Json("Success", JsonRequestBehavior.AllowGet);
            }
        }

        //******************************** ACQUISITION ORDER STATUS CHANGE FUNCTION ********************************************************

        public JsonResult AssingInvenAllItem(int? Id, long UserId, int Status, int orderId)
        {
            string msg = "";
            string name = "";
            int ColumnId = 0;
            int OperationStatus = 1;

            if (Id != null)
            {
                Acquisition_List order = db.Acquisition_List.Find(Id);
                if (order != null)
                {
                    name = order.Name;
                    ColumnId = order.AcquisitionOrderId;
                    try
                    {
                        order.AcquisitionStatus = Status;
                        db.Acquisition_List.Attach(order);
                        db.Entry(order).State = EntityState.Modified;
                        db.SaveChanges();
                        msg = "Reserved inventory item successfully assigned with business order.";
                        OperationStatus = 1;

                        if (db.Inventory_Reserved_Item_Lists.Where(m => m.AcquisitionOrderId == orderId && m.IsAssigned == false).Any())
                        {
                            var all_Rsvd_Item = db.Inventory_Reserved_Item_Lists.Where(m => m.AcquisitionOrderId == orderId && m.IsAssigned == false).ToList();
                            foreach (var resrv in all_Rsvd_Item)
                            {
                                resrv.IsAssigned = true;
                                db.Entry(resrv).State = EntityState.Modified;
                                db.SaveChanges();
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        string errorMsg = ex.Message.ToString();
                        msg = "Reserved inventory item assigned with business order unsuccessful.";
                        OperationStatus = -1;
                    }
                }
                SaveAuditLog("Assign Reserved Item", msg, "Inventory", "AllAcquisitionList", "Acquisition", ColumnId, UserId, now, OperationStatus);
                if (OperationStatus == 1)
                {
                    return Json("Success", JsonRequestBehavior.AllowGet);
                }
            }
            return Json("Error", JsonRequestBehavior.AllowGet);
        }
        public JsonResult AssingInvenSingleItem(int? Id,long UserId,int quantity)
        {
            string msg = "";
            string name = "";
            int ColumnId = 0;
            int OperationStatus = 1;

            if (Id != null)
            {
                Acquisition acq_pro = db.Acquisitions.Find(Id);

                if (acq_pro != null)
                {
                    name = acq_pro.ProductName;
                    acq_pro.AssignedQuantity = Convert.ToInt32(acq_pro.AssignedQuantity)+ quantity;
                    acq_pro.AssignedDate = now;
                    acq_pro.AssignedBy = UserId;
                    if (quantity == acq_pro.Quantity)
                    {
                        acq_pro.OrderStatus = 1;
                    }
                    db.Entry(acq_pro).State = EntityState.Modified;
                    try
                    {
                        db.SaveChanges();
                        msg = "Reserved inventory item successfully assigned with business order.";
                        OperationStatus = 1;

                        if (db.Inventory_Reserved_Item_Lists
                            .Where(m => m.AcquisitionOrderId == acq_pro.AcquisitionOrderId && m.IsAssigned == false && m.InventoryId == acq_pro.ProductId).Any())
                        {
                            var resrv = db.Inventory_Reserved_Item_Lists.Where(m => m.AcquisitionOrderId == acq_pro.AcquisitionOrderId && m.IsAssigned == false && m.InventoryId == acq_pro.ProductId).FirstOrDefault();

                            resrv.IsAssigned = true;
                            db.Entry(resrv).State = EntityState.Modified;
                            db.SaveChanges();
                        }
                    }
                    catch (Exception ex)
                    {
                        string errorMsg = ex.Message.ToString();
                        msg = "Reserved inventory item assigned with business order unsuccessful.";
                        OperationStatus = -1;
                    }
                }
                SaveAuditLog("Assign Reserved Item", msg, "Inventory", "AllAcquisitionList", "Acquisition", ColumnId, UserId, now, OperationStatus);
                if (OperationStatus == 1)
                {
                    return Json("Success", JsonRequestBehavior.AllowGet);
                }
            }
            return Json("Error", JsonRequestBehavior.AllowGet);
        }

        public ActionResult AcquisitionOrderDetails(int? AcqOrderId)
        {
            if (AcqOrderId == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            else
            {
                AcquisitionViewModel model = (from m in db.Acquisition_List
                                              join s in db.OrderStatusLists on
                                              m.AcquisitionStatus equals s.OrderStatus
                                              where m.AcquisitionOrderId == AcqOrderId
                                              select new AcquisitionViewModel
                                              {
                                                  AcquisitionOrderId = m.AcquisitionOrderId,
                                                  AcquisitionType = m.AcquisitionType,
                                                  BusinessOrderId = m.BusinessOrderId,
                                                  PartsId = m.PartsId,
                                                  MachineId = m.MachineId,
                                                  Name = m.Name,
                                                  AcquisitionStatus = m.AcquisitionStatus,
                                                  Status = s.Status
                                              }).FirstOrDefault();
                return View(model);
            }
        }
        public ActionResult _AllAcquistionProList(int AcquisitionOrderId,string type)
        {
            var list = (from a in db.View_All_AcquisitionList
                        where a.AcquisitionOrderId == AcquisitionOrderId
                        select new AcquisitionViewModel
                        {
                            AcquisitionId = a.AcquisitionId,
                            AcquisitionOrderId = a.AcquisitionOrderId,
                            ProductName = a.ProductName,
                            ProductTypeId = a.ProductTypeId,
                            ProductTypeName = a.ProductType,
                            Country = a.Country,
                            Brand = a.BrandName,
                            BrandId = a.Brand,
                            Model = a.Model,
                            Quantity = a.Quantity,
                            UnitName = a.UnitName,
                            Comment = a.Comment,
                            CreatedBy = a.CreatedBy,
                            UserName = a.UserName,
                            CreatedDateName = a.CreatedDateName,
                            RequiredDateName = a.RequiredDateName,
                            RequiredTime = a.RequiredTime,
                            PictureName = a.PictureName,
                            OrderStatus = a.OrderStatus,
                            AssignedQuantity = a.AssignedQuantity
                        }).OrderByDescending(m => m.OrderStatus).ToList();
            ViewBag.Type = type;
            return PartialView(list);
        }

        //******************************** ACQUISITION ORDER PRODUCT DETAILS ********************************************************

        public ActionResult AcqusiProductDetails(int? AcqId)
        {
            if (AcqId == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            else
            {
                View_All_AcquisitionList detail = db.View_All_AcquisitionList.Where(m => m.AcquisitionId == AcqId).FirstOrDefault();
                return PartialView(detail);
            }
        }
        public ActionResult _AcquisitionBasicInfo(int? AcqId)
        {
            if (AcqId == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            AcquisitionViewModel acq = (from a in db.View_All_AcquisitionList
                                        where a.AcquisitionId == AcqId
                                        select new AcquisitionViewModel
                                        {
                                            AcquisitionId = a.AcquisitionId,
                                            AcquisitionOrderId = a.AcquisitionOrderId,
                                            ProductName = a.ProductName,
                                            ProductTypeId = a.ProductTypeId,
                                            ProductTypeName = a.ProductType,
                                            Country = a.Country,
                                            CountryName = a.CountryName,
                                            Brand = a.BrandName,
                                            Model = a.Model,
                                            Quantity = a.Quantity,
                                            UnitName = a.UnitName,
                                            CreatedBy = a.CreatedBy,
                                            UserName = a.UserName,
                                            CreatedDateName = a.CreatedDateName,
                                            RequiredDateName = a.RequiredDateName,
                                            RequiredTime = a.RequiredTime,
                                            PictureName = a.PictureName
                                        }).FirstOrDefault();
            return PartialView(acq);
        }
        public ActionResult _AcquisitionImages(int? AcqId)
        {
            if (AcqId != null && db.AcquisitionImages.Where(m => m.AcquisitionId == AcqId).Any())
            {
                var acqImg = db.AcquisitionImages.Where(m => m.AcquisitionId == AcqId).ToList();
                ViewBag.AcqImg = acqImg;
                ViewBag.TotalAcqImg = acqImg.Count;
            }
            return PartialView();
        }
        #endregion


        #region All Rejected Acquisition List
        public ActionResult RejectedAcqusitionList(int? pageNum)
        {
            pageNum = pageNum ?? 0;
            int page = Convert.ToInt32(pageNum);
            ViewBag.pageNum = pageNum;
            var allOrder = db.Acquisition_List.Where(m => m.AcquisitionStatus < 0 ).Count();
            ViewBag.TotalAcquisitionOrder = allOrder;
            ViewBag.OrderStatus = new SelectList(db.OrderStatusLists, "OrderStatus", "Status");
            Session["AllAcquisition"] = db.Acquisition_List.ToList();
            return View();
        }
        public ActionResult _RejectedAcqusitionList(int? AcqOrderId, int? listName, int acqType)
        {
            if (AcqOrderId != null)
            {
                var allAcqorder = (List<AcquisitionViewModel>)Session["AllAcquisitionOrderBySearch"];
                var allAcqorderBysearch = (from a in allAcqorder
                                           where a.AcquisitionOrderId == AcqOrderId && a.AcquisitionType == acqType
                                           && a.AcquisitionStatus < 0
                                           select new AcquisitionViewModel
                                           {
                                               AcquisitionOrderId = a.AcquisitionOrderId,
                                               AcquisitionType = a.AcquisitionType,
                                               BusinessOrderId = a.BusinessOrderId,
                                               MachineId = a.MachineId,
                                               PartsId = a.PartsId,
                                               Name = a.Name,
                                               SiteId = a.SiteId,
                                               AcquisitionStatus = a.AcquisitionStatus,
                                               Status = a.Status,
                                               CreatedBy = a.CreatedBy,
                                               CreatedDate = a.CreatedDate,
                                               CreatedDateName = a.CreatedDateName,
                                               UserName = (a.CreatedBy == 1) ? "Super Admin" :
                                              db.UserInformations.Where(m => m.UserId == a.CreatedBy).Select(m => m.FirstName + " " + m.MiddleName + " " + m.LastName).FirstOrDefault(),
                                               PictureName = db.UserInformations.Where(m => m.UserId == a.CreatedBy).Select(m => m.Picture).FirstOrDefault()
                                           }).ToList();
                ViewBag.TotalAcquisitionOrder = allAcqorderBysearch.Count();
                ViewBag.AcquisitionOrderList = allAcqorderBysearch.Take(ShowItemPerPage).ToList();
            }
            else
            {
                if (listName != null)
                {
                    var allAcqOrder = (from a in db.Acquisition_List
                                       join s in db.OrderStatusLists
                                       on a.AcquisitionStatus equals s.OrderStatus
                                       where a.AcquisitionStatus == listName && a.AcquisitionType == acqType
                                       && a.AcquisitionStatus < 0
                                       select new AcquisitionViewModel
                                       {
                                           AcquisitionOrderId = a.AcquisitionOrderId,
                                           AcquisitionType = a.AcquisitionType,
                                           BusinessOrderId = a.BusinessOrderId,
                                           MachineId = a.MachineId,
                                           PartsId = a.PartsId,
                                           Name = a.Name,
                                           SiteId = a.SiteId,
                                           AcquisitionStatus = a.AcquisitionStatus,
                                           Status = s.Status,
                                           CreatedBy = a.CreatedBy,
                                           CreatedDate = a.CreatedDate,
                                           CreatedDateName = DbFunctions.TruncateTime(a.CreatedDate).ToString(),
                                           UserName = (a.CreatedBy == 1) ? "Super Admin" :
                                           db.UserInformations.Where(m => m.UserId == s.CreatedBy).Select(m => m.FirstName + " " + m.MiddleName + " " + m.LastName).FirstOrDefault(),
                                           PictureName = db.UserInformations.Where(m => m.UserId == a.CreatedBy).Select(m => m.Picture).FirstOrDefault()
                                       }).OrderByDescending(m => m.CreatedDate).ToList();
                    ViewBag.TotalAcquisitionOrder = allAcqOrder.Count();
                    ViewBag.AcquisitionOrderList = allAcqOrder.Take(ShowItemPerPage).ToList();
                    Session["AllAcquisitionOrder"] = allAcqOrder;
                }
                else
                {
                    var allAcqOrder = (from a in db.Acquisition_List
                                       join s in db.OrderStatusLists
                                       on a.AcquisitionStatus equals s.OrderStatus
                                       where a.AcquisitionType == acqType
                                       && a.AcquisitionStatus < 0
                                       select new AcquisitionViewModel
                                       {
                                           AcquisitionOrderId = a.AcquisitionOrderId,
                                           AcquisitionType = a.AcquisitionType,
                                           BusinessOrderId = a.BusinessOrderId,
                                           MachineId = a.MachineId,
                                           PartsId = a.PartsId,
                                           Name = a.Name,
                                           SiteId = a.SiteId,
                                           AcquisitionStatus = a.AcquisitionStatus,
                                           Status = s.Status,
                                           CreatedBy = a.CreatedBy,
                                           CreatedDate = a.CreatedDate,
                                           CreatedDateName = DbFunctions.TruncateTime(a.CreatedDate).ToString(),
                                           UserName = (a.CreatedBy == 1) ? "Super Admin" :
                                           db.UserInformations.Where(m => m.UserId == s.CreatedBy).Select(m => m.FirstName + " " + m.MiddleName + " " + m.LastName).FirstOrDefault(),
                                           PictureName = db.UserInformations.Where(m => m.UserId == a.CreatedBy).Select(m => m.Picture).FirstOrDefault()
                                       }).OrderByDescending(m => m.CreatedDate).ToList();
                    ViewBag.TotalAcquisitionOrder = allAcqOrder.Count();
                    ViewBag.AcquisitionOrderList = allAcqOrder.Take(ShowItemPerPage).ToList();
                    Session["AllAcquisitionOrder"] = allAcqOrder;
                    Session["TotalAcquisitionOrderCount"] = allAcqOrder.Count();
                }
            }
            return PartialView(ViewBag.AcquisitionOrderList);
        }
        #endregion

        #region Image Resize
        public static Image ScaleImage(Image image, int maxWidth, int maxHeight)
        {
            var ratioX = (double)maxWidth / image.Width;
            var ratioY = (double)maxHeight / image.Height;
            var ratio = Math.Min(ratioX, ratioY);

            var newWidth = (int)(image.Width * ratio);
            var newHeight = (int)(image.Height * ratio);

            var newImage = new Bitmap(newWidth, newHeight);

            using (var graphics = Graphics.FromImage(newImage))
                graphics.DrawImage(image, 0, 0, newWidth, newHeight);

            return newImage;
        }
        #endregion

        #region Created Manually Indent
        [EncryptedActionParameter]
        public ActionResult ManuallyIndentForInven(int type, int? MachineId, int? StoreId, int? WareId)
        {
            ViewBag.UnitId = new SelectList(db.UnitLists.Where(m => m.HasParentId == false), "UnitId", "UnitName");
            ViewBag.type = type;
            ViewBag.MachineId = MachineId;
            ViewBag.StoreId = StoreId;
            ViewBag.WareId = WareId;
            return View();
        }
        public JsonResult ProductNameSearchingInvenFinish(string prefix)
        {
            if (prefix == "" || prefix == null)
            {
                var allproduct = (from s in db.InventoryLists
                                  select new InventoryListModelView
                                  {
                                      InventoryId = s.InventoryId,
                                      ProductName = s.ProductName
                                  }).OrderBy(m => m.ProductName).ToList();
                return Json(allproduct, JsonRequestBehavior.AllowGet);
            }
            else
            {
                var allproduct = (from s in db.InventoryLists
                                  where (s.ProductName.ToLower().Contains(prefix.ToLower()))
                                  select new InventoryListModelView
                                  {
                                      InventoryId = s.InventoryId,
                                      ProductName = s.ProductName
                                  }).OrderBy(m => m.ProductName).ToList();
                return Json(allproduct, JsonRequestBehavior.AllowGet);
            }
        }
        //changed by hasan
        public JsonResult SaveManualIndent(ManualIndent model, string vouchername, string[] allProName, double[] allQuan, int[] allUnit, string[] allUPrice, string[] allCom, long userId)
        {
            if (allProName.Length > 0)
            {
                string msg = "";
                int ColumnId = 0;
                int OperationStatus = 1;
                ManualIndentVoucher voucher = new ManualIndentVoucher();
                if (vouchername != String.Empty)
                {
                    voucher.Name = vouchername;
                    voucher.CreatedBy = userId;
                    voucher.CreatedDate = now;
                    db.ManualIndentVouchers.Add(voucher);
                    msg = "New indent voucher  '" + vouchername + "' has been successfully created on " + now.ToString("MMM dd, yyyy hh:mm tt") + ".";
                    db.SaveChanges();
                    var columnId = db.ManualIndentVouchers.Where(m => m.CreatedBy == userId).Max(m => m.VoucherId);
                    ColumnId = Convert.ToInt32(columnId);
                }
                SaveAuditLog("Create Indent Voucher", msg, "Inventory", "Add New Indent", "ManualIndentVoucher", ColumnId, userId, now, OperationStatus);
                for (int i = 0; i < allProName.Length; i++)
                {
                    if (allProName[i] != "")
                    {
                        ManualIndent indent = new ManualIndent();
                        indent.VoucherId = voucher.VoucherId;
                        indent.IndentType = model.IndentType;
                        indent.MachineId = model.MachineId;
                        indent.StoreId = model.StoreId;
                        indent.WareId = model.WareId;
                        indent.ProName = allProName[i];
                        indent.Quantity = allQuan[i];
                        indent.ProUnitId = allUnit[i];
                        indent.ProUnitPrice = Convert.ToDecimal(allUPrice[i]);
                        indent.Comments = allCom[i];
                        indent.IndentStatus = 0;
                        indent.CreatedBy = userId;
                        indent.CreatedDate = now;
                        db.ManualIndents.Add(indent);

                        try
                        {
                            db.SaveChanges();
                            msg = "New indent voucher product '" + indent.ProName + "' has been successfully created on " + now.ToString("MMM dd, yyyy hh:mm tt") + ".";
                            var colId = db.ManualIndents.Where(m => m.CreatedBy == userId).Max(m => m.IndentId);
                            ColumnId = Convert.ToInt32(colId);
                            OperationStatus = 1;
                        }
                        catch (Exception)
                        {
                            OperationStatus = -1;
                            msg = "New indent voucher product '" + indent.ProName + "' creation was unsuccessfully on " + now.ToString("MMM dd, yyyy hh:mm tt") + ".";
                        }
                        SaveAuditLog("Create Indent Voucher Product", msg, "Inventory", "Add New Indent", "ManualIndent", ColumnId, userId, now, OperationStatus);

                    }
                }
                    return Json("Success", JsonRequestBehavior.AllowGet);
                }
         
            return Json("Error", JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region Show All Manual Indent
        public ActionResult ShowAllIndentVoucher()
        {
            return View();
        }
        //change by hasan
        public PartialViewResult _ShowAllIndentVoucher(int? status, string VoucherId, int? days, DateTime? StartDate, DateTime? EndDate)
        {
            var list = db.View_Manual_Indent_Voucher.Where(m => m.VoucherStatus != -1).OrderByDescending(m=>m.CreatedDate).ToList();
            List<View_Manual_Indent_Voucher> voucher = new List<View_Manual_Indent_Voucher>();
            if (!string.IsNullOrEmpty(VoucherId))
            {
                string[] sId;
                long searchId = 0;
                sId = VoucherId.Split(',');
                for (int j = 0; j < sId.Length; j++)
                {
                    searchId = Convert.ToInt64(sId[j]);
                    var myList = (from m in db.View_Manual_Indent_Voucher
                                  where m.VoucherId == searchId
                                  select m).ToList();
                    foreach (var item in myList)
                    {
                        voucher.Add(item);
                    }

                }
                return PartialView(voucher);
            }
            if (status != null && status == -1)
            {
                list = db.View_Manual_Indent_Voucher.Where(m => m.VoucherStatus == status).OrderByDescending(m => m.CreatedDate).ToList();
            }
            if (status != null)
            {
                list = list.Where(m => m.VoucherStatus == status).OrderByDescending(m => m.CreatedDate).ToList();
            }
            int day = Convert.ToInt32(days);
            if (day > 0)
            {
                DateTime countDate = now.Date;
                countDate = countDate.AddDays(-(day));
                list = list.Where(m => m.CreatedDate.Date > countDate.Date).OrderByDescending(m => m.CreatedDate).ToList();
            }
            list = (StartDate != null && EndDate != null) ? list.Where(x => x.CreatedDate != null && x.CreatedDate.Date >= StartDate.Value.Date && x.CreatedDate.Date <= EndDate.Value.Date).ToList() : list;
            ViewBag.list = list.Count();
            Session["IndentVoucherBySearch"] = list;
            Session["IndentVoucherCount"] = list.Count();
            ViewBag.status = status;
            list = list.Take(ShowIndentVoucherPerPage).ToList();
            return PartialView(list);
        }
        //added by hasan
        public JsonResult GetIndentVoucher(int? page)
        {

            page = page ?? 0;
            int total = Convert.ToInt16(Session["IndentVoucherCount"]);
            int skip = ShowIndentVoucherPerPage * Convert.ToInt16(page);
            var canPage = skip < total;
            if (canPage)
            {
                var listBackFromSession = (List<View_Manual_Indent_Voucher>)Session["IndentVoucherBySearch"];
                var partslist = listBackFromSession.Skip(skip).Take(ShowIndentVoucherPerPage).ToList();
                return Json(partslist, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json("", JsonRequestBehavior.AllowGet);
            }
        }
        public JsonResult SearchIndentVoucherList(int? Status)
        {
            List<ManualIndentVoucher> voucherList = new List<ManualIndentVoucher>();
            var allVoucher = db.ManualIndentVouchers.Where(m => m.VoucherStatus != -1).OrderByDescending(m=>m.CreatedDate).ToList();

            if (Status != null)
            {
                allVoucher = db.ManualIndentVouchers.Where(m => m.VoucherStatus == Status).OrderByDescending(m => m.CreatedDate).ToList();
            }
            foreach (var names in allVoucher)
            {
                if (!voucherList.Where(m => m.Name == names.Name).Any())
                {
                    ManualIndentVoucher voucherName = new ManualIndentVoucher();
                    voucherName.Name = names.Name;
                    voucherName.VoucherId = names.VoucherId;
                    voucherList.Add(voucherName);
                }
            }
            if (allVoucher.Count() == 0)
            {
                ManualIndentVoucher name = new ManualIndentVoucher();
                name.Name = "No data found";
                name.VoucherId = -111;
                voucherList.Add(name);
            }
            return Json(voucherList, JsonRequestBehavior.AllowGet);
        }
        [EncryptedActionParameter]
        public ActionResult IndentVoucherDetails(int? id, string v)
        {
            if (id > 0)
            {
                var title = db.ManualIndentVouchers.Find(id);
                if (title != null)
                {
                    ViewBag.v = v;

                }
                return View(title);
            }
            return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        }
        public PartialViewResult IndentVoucherListPopUp()
        {
            var list = db.ManualIndentVouchers.Where(m => m.VoucherStatus != -1).ToList();

            return PartialView(list);
        }
        public PartialViewResult _AllManualIndent(string v, int status,long id)
        {
            var list = db.View_Manual_Indent.Where(m => m.IndentStatus > -1 && m.VoucherId==id).ToList();
            if (status == -1)
            {
                list = db.View_Manual_Indent.Where(m => m.IndentStatus == status && m.VoucherId == id).ToList();
            }
            ViewBag.list = list;
            ViewBag.v = v;
            ViewBag.status = status;
            return PartialView();
        }
        [EncryptedActionParameter]
        public ActionResult IndentVoucherDetailsPrint(int id)
        {
            if (id > 0)
            {
                var title = db.View_Manual_Indent_Voucher.Where(m=>m.VoucherId==id).FirstOrDefault();
                if (title != null)
                {
                    return View(title);
                }
            }
            return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        }
        #endregion

        #region Indent Product Info
        public PartialViewResult _IndentInfo(int id)
        {
            View_Manual_Indent indent = db.View_Manual_Indent.Where(m => m.IndentId == id).FirstOrDefault();
            return PartialView(indent);
        }
        #endregion

        #region Edit Indent Action
        public PartialViewResult _EditIndent(int id)
        {
            View_Manual_Indent indent = db.View_Manual_Indent.Where(m => m.IndentId == id).FirstOrDefault();
            ViewBag.UnitId = new SelectList(db.UnitLists.Where(m => m.HasParentId == false), "UnitId", "UnitName", indent.ProUnitId);
            return PartialView(indent);
        }
        //changed by hasan
        public JsonResult EditManualIndent(ManualIndent model)
        {
            string msg = "";
            string opeName = "";
            int ColumnId = 0;
            var VoucherName = "";
            int OperationStatus = 0;
            ManualIndent indent = db.ManualIndents.Find(model.IndentId);
            if (indent.IndentId > 0)
            {
                indent.ProName = model.ProName;
                indent.Quantity = model.Quantity;
                indent.ProUnitId = model.ProUnitId;
                indent.ProUnitPrice = model.ProUnitPrice;
                indent.Comments = model.Comments;
                indent.UpdatedBy = model.UpdatedBy;
                VoucherName = db.ManualIndentVouchers.Where(m => m.VoucherId == indent.VoucherId).Select(m => m.Name).FirstOrDefault();
                indent.UpdatedDate = now;
                db.Entry(indent).State = EntityState.Modified;
                opeName = "Update Indent Voucher Product";
                try
                {
                    db.SaveChanges();
                    ColumnId = Convert.ToInt32(model.IndentId);

                    OperationStatus = 1;
                    if (OperationStatus == 1)
                    {
                        msg = "Indent voucher product '" + model.ProName + "' of Voucher '" + VoucherName + "' has been successfully updated on " + now.ToString("MMM dd, yyy hh:mm tt") + ".";
                    }

                }
                catch (Exception)
                {
                    OperationStatus = -1;
                    msg = "Indent voucher product '" + model.ProName + "' of Voucher '" + VoucherName + "' update was unsuccessfully on " + now.ToString("MMM dd, yyy hh:mm tt") + ".";

                }
                int updatedBy = Convert.ToInt32(model.UpdatedBy);
                SaveAuditLog(opeName, msg, "Inventory", "Indent Voucher Details", "ManualIndent", ColumnId, updatedBy, now, OperationStatus);
                if (OperationStatus == 1)
                {
                    return Json("Success", JsonRequestBehavior.AllowGet);
                }
            }
            return Json("Error", JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region Indent Status Change
        public JsonResult ApprovManualIndent(long voucherId,long ApprovedBy,int indentStatus)
        {
            ManualIndentVoucher indent = db.ManualIndentVouchers.Find(voucherId);
            if (indent != null)
            {
                int status = -1;
                indent.VoucherStatus= indentStatus;
                indent.ApprovedBy = ApprovedBy;
                indent.ApprovedDate = now;
                db.Entry(indent).State = EntityState.Modified;
                try
                {
                    db.SaveChanges();
                    status = 1;
                }
                catch (Exception)
                {
                    status = -1;
                }
                if (status == 1)
                {
                    return Json("Success", JsonRequestBehavior.AllowGet);
                }
            }
            return Json("Error",JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region Cancel Indent Action
        //changed by hasan
        public JsonResult CancelManualIndent(long IndentId, long UserId)
        {
            string msg = "";
            int ColumnId = 0;
            int OperationStatus = 1;
            ManualIndent indent = db.ManualIndents.Find(IndentId);
            if (indent != null)
            {

                db.ManualIndents.Remove(indent);
                try
                {
                    db.SaveChanges();
                    OperationStatus = 1;
                    msg = "Indent Voucher product '" + indent.ProName + "' has been successfully Deleted on " + now.ToString("MMM dd, yyyy hh:mm tt") + ".";
                    ColumnId = Convert.ToInt32(IndentId);
                }
                catch (Exception)
                {
                    OperationStatus = -1;
                    msg = "Indent Voucher product '" + indent.ProName + "' delete was unsuccessfully on " + now.ToString("MMM dd, yyyy hh:mm tt") + ".";
                    ColumnId = Convert.ToInt32(IndentId);
                }
                if (OperationStatus == 1)
                {
                    return Json(IndentId, JsonRequestBehavior.AllowGet);
                }
                SaveAuditLog("Delete Indent Voucher Product", msg, "Inventory", "Indent Voucher Details", "ManualIndent", ColumnId, UserId, now, OperationStatus);
            }

            return Json("Error", JsonRequestBehavior.AllowGet);
        }
        //added by hasan
        public JsonResult IndentVoucherStatusChange(int VoucherId, long CreatedBy, int voucherStatus, int? preState)
        {
            string msg = "";
            string opeName = "";
            int ColumnId = 0;
            int OperationStatus = 1;

            if (db.ManualIndentVouchers.Where(m => m.VoucherId == VoucherId).Any())
            {
                if (voucherStatus == -1)
                {
                    var Indentid = db.ManualIndents.Where(m => m.VoucherId == VoucherId).Select(m => m.IndentId).ToList();
                    foreach (var id in Indentid)
                    {
                        ManualIndent indent = db.ManualIndents.Find(id);
                        indent.IndentStatus = -1;
                        indent.UpdatedBy = CreatedBy;
                        indent.UpdatedDate = now;
                        db.Entry(indent).State = EntityState.Modified;
                        db.SaveChanges();
                    }
                }
              
                ManualIndentVoucher manualIndent = db.ManualIndentVouchers.Find(VoucherId);
                manualIndent.VoucherStatus = voucherStatus;
                if (voucherStatus != -1)
                {
                    manualIndent.ApprovedBy = CreatedBy;
                    manualIndent.ApprovedDate = now;
                }
                else
                {
                    manualIndent.UpdatedBy = CreatedBy;
                    manualIndent.UpdatedDate = now;
                }
               
                db.Entry(manualIndent).State = EntityState.Modified;
                ColumnId = Convert.ToInt32(VoucherId);
                try
                {
                    db.SaveChanges();
                    OperationStatus = 1;
                    if (voucherStatus == -1)
                    {
                        opeName = "Delete Indent Voucher";
                        msg = "Indent voucher '" + manualIndent.Name + "' has been successfully deleted from status '" + preState + "' on " + now.ToString("MMM dd, yyy hh:mm tt") + ".";
                    }
                    if (voucherStatus == 1)
                    {
                        opeName = "Approve Indent Voucher";
                        msg = "Indent voucher '" + manualIndent.Name + "' has been successfully approved on " + now.ToString("MMM dd, yyy hh:mm tt") + ".";
                    }
                }
                catch (Exception ex)
                {
                    string errmsg = ex.ToString();
                    OperationStatus = -1;
                    if (voucherStatus == -1)
                    {
                        opeName = "Delete Indent Voucher";
                        msg = "Indent voucher '" + manualIndent.Name + "' deletion was  unsuccessful from status '" + preState + "' on " + now.ToString("MMM dd, yyy hh:mm tt") + ".";
                    }
                    if (voucherStatus == 1)
                    {
                        opeName = "Approve Inded Voucher";
                        msg = "Indent voucher '" + manualIndent.Name + "' approving  was unsuccessful on " + now.ToString("MMM dd, yyy hh:mm tt") + ".";
                    }

                }
                SaveAuditLog(opeName, msg, "Inventory", "Indent History", "ManualIndentVoucher", ColumnId, CreatedBy, now, OperationStatus);
                if (OperationStatus == 1)
                {
                    var voucherId = manualIndent.VoucherId;
                    return Json(voucherId, JsonRequestBehavior.AllowGet);
                }
            }
            return Json("Error", JsonRequestBehavior.AllowGet);
        }
        #endregion

        //******************* Receive Item Quantity *********************
        #region
        public PartialViewResult _EnterQuantity(long id)
        {
            View_Manual_Indent indent = db.View_Manual_Indent.Where(m => m.IndentId == id).FirstOrDefault();
            return PartialView(indent);
        }
        //changed by Hasan
        public JsonResult RcvManualIndentQty(ManualIndentReceiveTransaction model)
        {
            string msg = "";
            string opeName = "";
            double RcvQuantity = 0.0;
            int ColumnId = 0;
            int OperationStatus = -1;
            model.InsertedDate = now;
            if (model.IndentId > 0 && model.VoucherId > 0 && model.Quantity > 0)
            {
                var voucherName = db.ManualIndentVouchers.Where(m => m.VoucherId == model.VoucherId).Select(m => m.Name).FirstOrDefault();
                var tranRcvQuantity = db.ManualIndentReceiveTransactions.Where(m => m.VoucherId == model.VoucherId && m.IndentId == model.IndentId).Any();
                if (tranRcvQuantity == true)
                {
                     RcvQuantity = db.ManualIndentReceiveTransactions.Where(m => m.VoucherId == model.VoucherId && m.IndentId == model.IndentId).Sum(m=>m.Quantity);
                }
                else
                {
                    RcvQuantity = 0.0;
                }
                ManualIndent indent = db.ManualIndents.Find(model.IndentId);
                if (indent.Quantity > RcvQuantity)
                {
                    long lastCountDis = db.ManualIndentReceiveTransactions.Count();
                    lastCountDis += 1;
                    model.TransactionName = "IndentTransaction_#" + ((lastCountDis < 1000) ? lastCountDis.ToString("000") : lastCountDis.ToString());
                    db.ManualIndentReceiveTransactions.Add(model);
                    opeName = "Create Indent Transaction";
                    try
                    {
                        db.SaveChanges();
                        var columnId = db.ManualIndentReceiveTransactions.Where(m => m.IndentId == model.IndentId).Max(m => m.Id);
                        ColumnId = Convert.ToInt32(columnId);
                        OperationStatus = 1;

                        if (OperationStatus == 1)
                        {
                            msg = "Indent Transaction '" + model.TransactionName + "' has been successfully created on " + now.ToString("MMM dd, yyy hh:mm tt") + ".";
                        }
                    }
                    catch (Exception)
                    {
                        OperationStatus = -1;
                        msg = "Indent Transaction '" + model.TransactionName + "' create was unsuccessfully on " + now.ToString("MMM dd, yyy hh:mm tt") + ".";
                    }
                    SaveAuditLog(opeName, msg, "Inventory", "Indent Voucher Details", "ManualIndentReceiveTransaction", ColumnId, model.InsertedBy, now, OperationStatus);
                }
                double total_rcv_qty = db.ManualIndentReceiveTransactions.Where(m => m.VoucherId == model.VoucherId && m.IndentId == model.IndentId).Sum(m => m.Quantity);
                indent.IndentStatus = (indent.Quantity == total_rcv_qty) ? 2 : 1;
                indent.ApprovedBy = model.InsertedBy;
                indent.ApprovedDate = now;
                db.Entry(indent).State = EntityState.Modified;
                opeName = "Receive Indent Quantity";
                try
                {
                    db.SaveChanges();
                    OperationStatus = 1;
                    if (OperationStatus == 1)
                    {
                        msg = "Quantity of '" + indent.ProName + "' of Voucher '" + voucherName + "' has been successfully received on " + now.ToString("MMM dd, yyy hh:mm tt") + ".";
                    }
                }
                catch (Exception)
                {
                    OperationStatus = -1;
                    msg = "Quantity of '" + indent.ProName + "' of Voucher '" + voucherName + "' receive was unsuccessfully on " + now.ToString("MMM dd, yyy hh:mm tt") + ".";
                }
                SaveAuditLog(opeName, msg, "Inventory", "Indent Voucher Details", "ManualIndent", ColumnId, model.InsertedBy, now, OperationStatus);

            }
            if (OperationStatus == 1)
            {
                return Json(ColumnId, JsonRequestBehavior.AllowGet);
            }
            return Json("Error", JsonRequestBehavior.AllowGet);
        }
        //added by hasan
        public JsonResult CompleteVoucher(int VoucherId, int InsertedBy, int? comState)
        {
            string msg = "";
            string opeName = "";
            int ColumnId = 0;
            int OperationStatus = 1;
            int voustatus = 1;
            if (!db.ManualIndents.Where(m => m.VoucherId == VoucherId && m.IndentStatus < 2).Any() && comState == 2)
            {
                ManualIndentVoucher voucher = db.ManualIndentVouchers.Find(VoucherId);
                voucher.VoucherStatus = Convert.ToInt32(comState);
                voucher.UpdatedBy = InsertedBy;
                voucher.UpdatedDate = now;
                db.Entry(voucher).State = EntityState.Modified;
                opeName = "Complete Indent Voucher";
                try
                {
                    db.SaveChanges();
                    OperationStatus = 1;
                    if (OperationStatus == 1)
                    {
                        msg = "Indent voucher '" + voucher.Name + "' has been successfully completed on " + now.ToString("MMM dd, yyy hh:mm tt") + ".";
                    }
                    voustatus = 2;
                }
                catch (Exception)
                {
                    OperationStatus = -1;
                    msg = "Indent voucher '" + voucher.Name + "' complete was unsuccessfully on " + now.ToString("MMM dd, yyy hh:mm tt") + ".";

                }
                SaveAuditLog(opeName, msg, "Inventory", "Indent Voucher Details", "ManualIndentVoucher", ColumnId, InsertedBy, now, OperationStatus);
            }
            if (OperationStatus == 1)
            {
                return Json(voustatus, JsonRequestBehavior.AllowGet);
            }
            return Json("Error", JsonRequestBehavior.AllowGet);
        }
        //added by hasan
        public JsonResult ReceiveAllProdQuantity(int VoucherId, int InsertedBy)
        {
            string msg = "";
            string opeName = "";
            int ColumnId = 0;
            double receiveQuantity = 0;
            int OperationStatus = -1;

            if (VoucherId > 0)
            {
                var Indentid = db.ManualIndents.Where(m => m.VoucherId == VoucherId).Select(m => m.IndentId).ToList();
                var voucherName = db.ManualIndentVouchers.Where(m => m.VoucherId == VoucherId).Select(m => m.Name).FirstOrDefault();

                foreach (var id in Indentid)
                {
                    if (db.ManualIndentReceiveTransactions.Where(m => m.VoucherId == VoucherId && m.IndentId == id).Any())
                    {
                        receiveQuantity = db.ManualIndentReceiveTransactions.Where(m => m.VoucherId == VoucherId && m.IndentId == id).Sum(m => m.Quantity);
                    }
                    else
                    {
                        receiveQuantity = 0.0;
                    }
                    var totalQuantity = db.ManualIndents.Where(m => m.VoucherId == VoucherId && m.IndentId == id).Select(m => m.Quantity).FirstOrDefault();
                    ManualIndentReceiveTransaction model = new ManualIndentReceiveTransaction();
                    long lastCountDis = db.ManualIndentReceiveTransactions.Count();
                    if (totalQuantity > receiveQuantity)
                    {
                        lastCountDis += 1;
                        model.TransactionName = "IndentTransaction_#" + ((lastCountDis < 1000) ? lastCountDis.ToString("000") : lastCountDis.ToString());
                        model.InsertedBy = InsertedBy;
                        model.VoucherId = VoucherId;
                        model.Quantity = (totalQuantity - receiveQuantity);
                        model.IndentId = id;
                        model.InsertedDate = now;
                        db.ManualIndentReceiveTransactions.Add(model);

                        try
                        {
                            db.SaveChanges();
                            var columnId = db.ManualIndentReceiveTransactions.Where(m => m.VoucherId == VoucherId).Max(m => m.Id);
                            ColumnId = Convert.ToInt32(columnId);
                            OperationStatus = 1;
                            opeName = "Create Indent Transaction";
                            if (OperationStatus == 1)
                            {
                                msg = "Indent Transaction '" + model.TransactionName + "' has been successfully created on " + now.ToString("MMM dd, yyy hh:mm tt") + ".";
                            }
                        }
                        catch (Exception)
                        {
                            OperationStatus = -1;
                            msg = "Indent Transaction '" + model.TransactionName + "' was unsuccessfully on " + now.ToString("MMM dd, yyy hh:mm tt") + ".";
                        }
                        SaveAuditLog(opeName, msg, "Inventory", "Indent Voucher Details", "ManualIndentReceiveTransaction", ColumnId, model.InsertedBy, now, OperationStatus);

                    }
                    ManualIndent indent = db.ManualIndents.Find(id);
                    indent.IndentStatus = 2;
                    indent.ApprovedBy = InsertedBy;
                    indent.ApprovedDate = now;
                    db.Entry(indent).State = EntityState.Modified;
                    try
                    {
                        db.SaveChanges();
                        ColumnId = Convert.ToInt32(id);
                        OperationStatus = 1;
                        opeName = "Receive Indent Quantity";
                        if (OperationStatus == 1)
                        {
                            msg = "Indent quantity of '" + indent.ProName + "' of voucher '" + voucherName + "' has been successfully received on " + now.ToString("MMM dd, yyy hh:mm tt") + ".";
                        }
                    }
                    catch (Exception)
                    {
                        OperationStatus = -1;
                        msg = "Indent quantity '" + indent.ProName + "' of voucher '" + voucherName + "' receive was unsuccessfully on " + now.ToString("MMM dd, yyy hh:mm tt") + ".";

                    }
                    SaveAuditLog(opeName, msg, "Inventory", "Indent Voucher Details", "ManualIndent", ColumnId, model.InsertedBy, now, OperationStatus);
                }
            }
            if (OperationStatus == 1)
            {
                return Json(ColumnId, JsonRequestBehavior.AllowGet);
            }
            return Json("Error", JsonRequestBehavior.AllowGet);
        }
        //added by hasan
        public PartialViewResult DisplayUserDetails(long? UserId)
        {
            View_UserLists model = db.View_UserLists.Where(m => m.UserId == UserId).FirstOrDefault();
            UserInformationModelView deg = new UserInformationModelView();
              if(UserId >0){
                
                    deg.UserId = model.UserId;
                    deg.UserName = model.UserName;
                    deg.Title = model.Title;
                    deg.Picture = model.Picture;
                    deg.DateOfBirth = model.DateOfBirth;
                    deg.Designation = model.DesignationName;
                    deg.UserTypeName = model.UserTypeName;
                    deg.Nationality = model.Nationality;
                    deg.Gender = model.Gender;
                    deg.GenderName = (model.Gender == 1) ? "Male" : "Female";              
                    deg.EmailAddress = model.EmailAddress;
                    deg.Phone = model.Phone;
               
                }
              return PartialView(deg);
        }
        #endregion

        //*************** Show Transacton History ***********************
        #region
        public ActionResult IndentTransactionHis()
        {
            return View();
        }
        //changed by hasan
        public PartialViewResult _IndentTransactionHis(string search, int? days, DateTime? StartDate, DateTime? EndDate)
        {
            List<View_Indent_Transaction> tran = new List<View_Indent_Transaction>();
            var list = db.View_Indent_Transaction.OrderByDescending(m=>m.InsertedDate).ToList();
            if (!string.IsNullOrEmpty(search))
            {
                string[] sId;
                long searchId = 0;
                string column;
                sId = search.Split(',');
                for (int j = 0; j < sId.Length; j++)
                {
                    searchId = Convert.ToInt64(sId[j].Split('-')[0]);
                    column = sId[j].Split('-')[1];

                    if (column == "p")
                    {

                        var myList = (from m in list
                                      where m.IndentId == searchId
                                      select m).ToList();
                        foreach (var item in myList)
                        {
                            tran.Add(item);
                        }

                    }
                    if (column == "t")
                    {

                        var myList = (from m in list
                                      where m.Id == searchId
                                      select m).ToList();
                        foreach (var item in myList)
                        {
                            tran.Add(item);
                        }
                    }
                    if (column == "v")
                    {

                        var myList = (from m in list
                                      where m.VoucherId == searchId
                                      select m).ToList();
                        foreach (var item in myList)
                        {
                            tran.Add(item);
                        }
                    }


                }
                return PartialView(tran);
            }

            int day = Convert.ToInt32(days);
            if (day > 0)
            {
                DateTime countDate = now.Date;
                countDate = countDate.AddDays(-(day));
                list = list.Where(m => m.InsertedDate.Date > countDate.Date).OrderByDescending(m => m.InsertedDate).ToList();
            }
            list = (StartDate != null && EndDate != null) ? list.Where(x => x.InsertedDate != null && x.InsertedDate.Date >= StartDate.Value.Date && x.InsertedDate.Date <= EndDate.Value.Date).ToList() : list;
            Session["AllIndentTransList"] = list;
            Session["IndentTransCount"] = list.Count();
            ViewBag.TotalTran = list.Count();
            list = list.Take(ShowIndentTransPerPage).ToList();

            return PartialView(list);
        }
        //added by hasan
        public JsonResult SearchTransactionList()
        {
            List<SearchModelView> searchList = new List<SearchModelView>();
            var trans = db.View_Indent_Transaction.OrderByDescending(m=>m.InsertedDate).ToList();
            foreach (var names in trans)
            {
                if (!searchList.Where(x => x.SearchTag == names.ProName).Any())
                {
                    SearchModelView view = new SearchModelView();
                    view.SearchTag = names.ProName;
                    view.Id = names.IndentId + "-p";
                    searchList.Add(view);
                }
            }
            foreach (var names in trans)
            {
                if (!searchList.Where(x => x.SearchTag == names.TransactionName).Any())
                {
                    SearchModelView view = new SearchModelView();
                    view.SearchTag = names.TransactionName;
                    view.Id = names.Id + "-t";
                    searchList.Add(view);
                }
            }
            foreach (var names in trans)
            {
                if (!searchList.Where(x => x.SearchTag == names.Name).Any())
                {
                    SearchModelView view = new SearchModelView();
                    view.SearchTag = names.Name;
                    view.Id = names.VoucherId + "-v";
                    searchList.Add(view);
                }
            }

            return Json(searchList, JsonRequestBehavior.AllowGet);
        }
        public JsonResult GetIndentTransaction(int? page)
        {
            page = page ?? 0;
            int total = Convert.ToInt16(Session["IndentTransCount"]);
            int skip = ShowIndentTransPerPage * Convert.ToInt16(page);
            var canPage = skip < total;
            if (canPage)
            {
                var listBackFromSession = (List<View_Indent_Transaction>)Session["AllIndentTransList"];
                var partslist = listBackFromSession.Skip(skip).Take(ShowIndentTransPerPage).ToList();
                return Json(partslist, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json("", JsonRequestBehavior.AllowGet);
            }
        }
        public PartialViewResult _IndentTransDetails(long id)
        {
            var model = db.View_Indent_Transaction.Where(m => m.Id == id).FirstOrDefault();
            return PartialView(model);
        }
        [EncryptedActionParameter]
        public ActionResult IndentTransactionPrint(long id)
        {
            var model = db.View_Indent_Transaction.Where(m => m.Id == id).FirstOrDefault();
            return View(model);
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