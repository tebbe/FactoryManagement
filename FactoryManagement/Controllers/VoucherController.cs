using FactoryManagement.Filters;
using FactoryManagement.Models;
using FactoryManagement.ModelView.Configuration;
using FactoryManagement.ModelView.HR;
using FactoryManagement.ModelView.Voucher;
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
    public class VoucherController : Controller
    {
        #region Private Properties
        private FactoryManagementEntities db = new FactoryManagementEntities();
        private FactoryManagementEntities dbAud = new FactoryManagementEntities();
        static private int offset = Convert.ToInt32(ConfigurationManager.AppSettings["localTime"]);
        DateTime now = DateTime.UtcNow.AddMinutes(offset);
        int ShowVoucherPerPage = 50;
        const int InvoiceTran = 50;
        const int NotRcvInvoiceTran = 50;
        const int InvoiTranshow = 50;
        const int VoucherTranshowInternal = 50;
        const int InvServListCount = 50;
        #endregion

        #region voucher
        [EncryptedActionParameter]
        public ActionResult CreateVoucher(bool IsAccSelect,int? type,int? accId)
        {
            ViewBag.IsAccSelect = IsAccSelect;
            if (IsAccSelect)
            {
                string name = "";
                if (type == 1)
                {
                    name = db.AccountNames.Where(m => m.AccId == accId).Select(m => m.AccountName1).FirstOrDefault();
                    //ViewBag.VoucherType = new SelectList(db.VoucherTypes.Where(m => m.VoucherTypeId < 3), "VoucherTypeId", "VoucherTypeName");
                }
                else
                {
                   name = db.BankAccountLists.Where(m => m.BankAccountId == accId).Select(m => m.AccountName).FirstOrDefault();
                   //ViewBag.VoucherType = new SelectList(db.VoucherTypes.Where(m => m.VoucherTypeId < 3), "VoucherTypeId", "VoucherTypeName");
                }
                ViewBag.name = name;
            }
            ViewBag.VoucherType = new SelectList(db.VoucherTypes, "VoucherTypeId", "VoucherTypeName");
            ViewBag.type = type;
            ViewBag.accId = accId;
            return View();
        }
        public JsonResult GetVoucherOtherCost(int? id)
        {
            if (id > 0)
            {
                var list = (from m in db.Voucher_Other_Cost_Type
                            where m.Id != id
                            select new
                            {
                                Id = m.Id,
                                HasPercentile = m.HasPercentile,
                                Percentage = m.Percentage,
                                ExtraServiceName = m.ExtraServiceName
                            }).ToList();

                return Json(list, JsonRequestBehavior.AllowGet);
            }
            else
            {
                var list = (from m in db.Voucher_Other_Cost_Type
                            select new
                            {
                                Id = m.Id,
                                HasPercentile = m.HasPercentile,
                                Percentage = m.Percentage,
                                ExtraServiceName = m.ExtraServiceName
                            }).ToList();

                return Json(list, JsonRequestBehavior.AllowGet);
            }

        }
        public JsonResult SaveVoucher(VoucherModelView model, string[] AllOtherSevice, string[] AllVoucherProduct)
        {
            string msg = "";
            int ColumnId = 0;
            int serviceColumnId = 0;
            int otherServiceColumnId = 0;
            int OperationStatus = 1;
            Voucher vchr = new Voucher();
            vchr.VoucherTitle = model.VoucherTitle;
            vchr.Number = model.Number;
            vchr.VoucherTypeId = model.VoucherTypeId;
            vchr.CreatedBy = model.CreatedBy;
            vchr.TotalAmount = model.TotalAmount;
            vchr.CreatedDate = now;
            vchr.Status = 0;
            vchr.IsAccountSelected = model.IsAccountSelected;
            vchr.InternalAccId = model.InternalAccId;
            vchr.BankAccId = model.BankAccId;
            vchr.IsArchive = false;
            if (model.LocationId != null)
            {
                string charId = model.LocationId.Split('-')[1];
                int id = Convert.ToInt32(model.LocationId.Split('-')[0]);
                if (charId == "s") { vchr.SiteId = id; }
                if (charId == "u") { vchr.UnitId = id; }
                if (charId == "d") { vchr.DeptId = id; }
                if (charId == "l") { vchr.LineId = id; }
                if (charId == "st") { vchr.StoreId = id; }
                if (charId == "w") { vchr.WareId = id; }
            }
            db.Vouchers.Add(vchr);
            try
            {
                db.SaveChanges();
                ColumnId =Convert.ToInt32(db.Vouchers.Where(m => m.CreatedBy == model.CreatedBy).Max(m => m.VoucherId));

                if (AllVoucherProduct != null)
                {
                    for (int i = 0; i < AllVoucherProduct.Length; i++)
                    {
                        Voucher_Service_List prdct = new Voucher_Service_List();
                        prdct.VoucherId = ColumnId;
                        prdct.Name = AllVoucherProduct[i].Split('|')[0];
                        prdct.Amount = Convert.ToDecimal(AllVoucherProduct[i].Split('|')[1]);
                        prdct.CreatedBy = model.CreatedBy;
                        prdct.CreatedDate = now;
                        db.Voucher_Service_List.Add(prdct);
                        try
                        {
                            db.SaveChanges();
                            OperationStatus = 1;
                            serviceColumnId = db.Voucher_Service_List.Where(m => m.CreatedBy == model.CreatedBy).Max(m => m.VoucherServiceId);
                            msg = "Voucher service '" + prdct.Name + "' has been successfully added to voucher '" + model.VoucherTitle + "' on " + now.ToString("MMM dd , yyy hh:mm tt") + " .";
                        }
                        catch (Exception)
                        {
                            OperationStatus = -1;
                            msg = "Voucher service '" + prdct.Name + "' add with voucher '" + model.VoucherTitle + "' on " + now.ToString("MMM dd , yyy hh:mm tt") + "unsuccessful.";
                            throw;
                        }
                        SaveAuditLog("Create Voucher Service", msg, "Voucher", "CreateVoucher", "Voucher_Service_List", serviceColumnId, model.CreatedBy, now, OperationStatus);
                    }
                }
                if (AllOtherSevice != null)
                {
                    for (int i = 0; i < AllOtherSevice.Length; i++)
                    {
                        Voucher_Other_Costs_List service = new Voucher_Other_Costs_List();
                        service.VoucherId = ColumnId;
                        service.ServiceId = Convert.ToInt32(AllOtherSevice[i].Split('|')[0]);
                        service.Amount = Convert.ToDecimal(AllOtherSevice[i].Split('|')[1]);
                        service.CreatedBy = model.CreatedBy;
                        service.CreatedDate = now;
                        db.Voucher_Other_Costs_List.Add(service);
                        string otherServiceName = db.Voucher_Other_Cost_Type.Where(m => m.Id == service.ServiceId).Select(m => m.ExtraServiceName).FirstOrDefault();
                        try
                        {
                            db.SaveChanges();
                            OperationStatus = 1;
                            otherServiceColumnId = db.Voucher_Other_Costs_List.Where(m => m.CreatedBy == model.CreatedBy).Max(m => m.OtherCostId);
                            msg = "Voucher other service '" + otherServiceName + "' has been successfully added to voucher '" + model.VoucherTitle + "' on " + now.ToString("MMM dd , yyy hh:mm tt") + " .";
                        }
                        catch (Exception ex)
                        {
                            string errorMsg = ex.Message.ToString();
                            OperationStatus = -1;
                            msg = "Voucher other service '" + otherServiceName + "' add with voucher '" + model.VoucherTitle + "' on " + now.ToString("MMM dd , yyy hh:mm tt") + " unsuccessful.";
                        }
                        SaveAuditLog("Create Voucher Other Service ", msg, "Voucher", "CreateVoucher", "Voucher_Other_Costs_List", otherServiceColumnId, model.CreatedBy, now, OperationStatus);
                    }
                }
                msg = "New Voucher '" + model.VoucherTitle + "' has been successfully created on " + now.ToString("MMM dd , yyy hh:mm tt") + " .";
            }
            catch (Exception)
            {
                OperationStatus = -1;
                msg = "New Voucher '" + model.VoucherTitle + "' creation unsuccessful on " + now.ToString("MMM dd , yyy hh:mm tt") + " .";
            }
            SaveAuditLog("Create New Voucher", msg, "Voucher", "CreateVoucher", "Voucher", ColumnId, model.CreatedBy, now, OperationStatus);
            if (OperationStatus == 1)
            {
                return Json("Success", JsonRequestBehavior.AllowGet);
            }
            return Json("Error", JsonRequestBehavior.AllowGet);
        }
        public JsonResult DeleteVoucher(int Id, long CreatedBy)
        {
            string msg = "";
            int ColumnId = 0;
            int ServiceColumnId = 0;
            int OtherServiceColumnId = 0;
            int OperationStatus = 1;
            Voucher voucher = db.Vouchers.Find(Id);
            ColumnId = Convert.ToInt32(voucher.VoucherId);
            var voucherProduct = db.Voucher_Service_List.Where(m => m.VoucherId == Id).ToList();
            if (voucherProduct.Count() > 0)
            {
                foreach (var product in voucherProduct)
                {
                    string service = product.Name;
                    ServiceColumnId = product.VoucherServiceId;
                    db.Voucher_Service_List.Remove(product);
                    try
                    {
                        db.SaveChanges();
                        OperationStatus = 1;
                        msg = "Voucher service '" + service + "' has been successfully deleted from voucher '" + voucher.VoucherTitle + "' on " + now.ToString("MMM dd , yyy hh:mm tt") + " .";
                    }
                    catch (Exception ex)
                    {
                        string errorMsg = ex.Message.ToString();
                        OperationStatus = -1;
                        msg = "Voucher service '" + service + "' delete unsuccessful from voucher '" + voucher.VoucherTitle + "' on " + now.ToString("MMM dd , yyy hh:mm tt") + " .";
                    }
                    SaveAuditLog("Voucher Service Delete ", msg, "Voucher", "VoucherList", "Voucher_Service_List", ServiceColumnId, CreatedBy, now, OperationStatus);
                }
            }
            var voucherOtherService = db.Voucher_Other_Costs_List.Where(m => m.VoucherId == Id).ToList();
            if (voucherOtherService.Count() > 0)
            {
                foreach (var otherService in voucherOtherService)
                {
                    string otherServiceName = db.Voucher_Other_Cost_Type.Where(m => m.Id == otherService.ServiceId).Select(m => m.ExtraServiceName).FirstOrDefault();
                    OtherServiceColumnId = otherService.OtherCostId;
                    db.Voucher_Other_Costs_List.Remove(otherService);
                    try
                    {
                        db.SaveChanges();
                        OperationStatus = 1;
                        msg = "Voucher other service '" + otherServiceName + "' has been successfully deleted from voucher '" + voucher.VoucherTitle + "' on " + now.ToString("MMM dd , yyy hh:mm tt") + " .";
                    }
                    catch (Exception ex)
                    {
                        string errorMsg = ex.Message.ToString();
                        OperationStatus = -1;
                        msg = "Voucher other service '" + otherServiceName + "' delete unsuccessful from voucher '" + voucher.VoucherTitle + "' on " + now.ToString("MMM dd , yyy hh:mm tt") + " .";
                    }
                    SaveAuditLog("Voucher Other Service Delete ", msg, "Voucher", "VoucherList", "Voucher_Other_Costs_List", OtherServiceColumnId, CreatedBy, now, OperationStatus);
                }
            }
            var archievedVoucher = db.Vouchers.Where(m => m.ParentId == Id).ToList();
            if (archievedVoucher.Count > 0)
            {
                foreach (var archive in archievedVoucher) {
                    var acrhiveService = db.Voucher_Service_List.Where(m => m.VoucherId == archive.VoucherId).ToList();
                    if (acrhiveService.Count > 0) {
                        foreach (var arService in acrhiveService)
                        {
                            db.Voucher_Service_List.Remove(arService);
                            db.SaveChanges();
                        }
                    }
                    var acrhiveOtherService = db.Voucher_Other_Costs_List.Where(m => m.VoucherId == archive.VoucherId).ToList();
                    if (acrhiveOtherService.Count > 0)
                    {
                        foreach (var arOtherService in acrhiveOtherService)
                        {
                            db.Voucher_Other_Costs_List.Remove(arOtherService);
                            db.SaveChanges();
                        }
                    }
                    db.Vouchers.Remove(archive);
                    db.SaveChanges();
                }
            }
            db.Vouchers.Remove(voucher);
            try
            {
                db.SaveChanges();
                OperationStatus = 1;
                msg = "Voucher" + " '" + voucher.VoucherTitle + "' " + "has been successfully deleted on " + now.ToString("MMM dd , yyy hh:mm tt") + " .";
            }
            catch (Exception ex)
            {
                string errmsg = ex.ToString();
                OperationStatus = -1;
                ColumnId = 0;
                msg = "Voucher" + " '" + voucher.VoucherTitle + "' " + " deletion unsuccessful on " + now.ToString("MMM dd , yyy hh:mm tt") + " .";
            }
            SaveAuditLog("Delete Voucher", msg, "Voucher", "VoucherList", "Voucher", ColumnId, CreatedBy, now, OperationStatus);
            if (OperationStatus == 1)
            {
                return Json(db.Vouchers.Any(), JsonRequestBehavior.AllowGet);
            }
            return Json("Error", JsonRequestBehavior.AllowGet);
        }
        public PartialViewResult VoucherEditInfo(int VoucherId)
        {
            Voucher voucher = db.Vouchers.Where(m => m.VoucherId == VoucherId).FirstOrDefault();
            VoucherModelView model = new VoucherModelView();
            model.VoucherId = VoucherId;
            model.IsAccountSelected = voucher.IsAccountSelected;
            return PartialView(model);
        }
        public PartialViewResult ShowVoucher(int VoucherId, int Display)
        {
            View_VoucherInfo model = db.View_VoucherInfo.Where(m => m.VoucherId == VoucherId).FirstOrDefault();
            ViewBag.Display = Display;
            return PartialView(model);
        }
        public PartialViewResult _ShowAllArchVoucher(int VoucherId)
        {
            var voucherName = db.View_VoucherInfo.Where(m => m.IsArchive == true && m.ParentId == VoucherId).ToList();
            return PartialView(voucherName);
        }
        public PartialViewResult DisplayArchieveVoucher(int VoucherId)
        {
            VoucherModelView model = new VoucherModelView();
            View_VoucherInfo voucher = db.View_VoucherInfo.Where(m => m.VoucherId == VoucherId).FirstOrDefault();
            model.VoucherTypeId = Convert.ToInt32(voucher.VoucherTypeId);
            model.Number = voucher.Number;
            model.VoucherTypeName = voucher.VoucherTypeName;
            model.VoucherId = VoucherId;
            model.VoucherTitle = voucher.VoucherTitle;
            model.TotalAmount = voucher.TotalAmount;
            return PartialView(model);
        }
        public PartialViewResult DisplayVoucher(int VoucherId)
        {
            VoucherModelView model = new VoucherModelView();
            Voucher voucher = db.Vouchers.Find(VoucherId);
            model.VoucherTypeId = Convert.ToInt32(voucher.VoucherTypeId);
            model.Number = voucher.Number;
            model.VoucherTypeName = db.VoucherTypes.Where(m => m.VoucherTypeId == model.VoucherTypeId).Select(m => m.VoucherTypeName).FirstOrDefault();
            model.VoucherId = VoucherId;
            model.VoucherTitle = voucher.VoucherTitle;
            model.TotalAmount = voucher.TotalAmount;
            model.IsAccountSelected = voucher.IsAccountSelected;
            return PartialView(model);
        }
        public PartialViewResult EditVoucher(int VoucherId)
        {
            VoucherModelView model = new VoucherModelView();
            Voucher voucher = db.Vouchers.Find(VoucherId);
            var ServiceTotal = db.Voucher_Service_List.Where(m => m.VoucherId == VoucherId).ToList();
            model.ServiceTotal = ServiceTotal.Sum(m => m.Amount);
            var OtherServiceTotal = db.Voucher_Other_Costs_List.Where(m => m.VoucherId == VoucherId).ToList();
            model.OtherServiceTotal = OtherServiceTotal.Sum(m => m.Amount);
            model.VoucherId = VoucherId;
            model.VoucherTypeId = Convert.ToInt32(voucher.VoucherTypeId);
            model.VoucherTitle = voucher.VoucherTitle;
            model.TotalAmount = voucher.TotalAmount;
            model.Number = voucher.Number;
            if (voucher.IsAccountSelected == true) {
                model.VoucherServiceId = ServiceTotal.Select(m => m.VoucherServiceId).FirstOrDefault();
            }
            model.InternalAccId = voucher.InternalAccId;
            model.BankAccId = voucher.BankAccId;
            model.DeptId = voucher.DeptId;
            model.LineId = voucher.LineId;
            model.SiteId = voucher.SiteId;
            model.UnitId = voucher.UnitId;
            model.WareId = voucher.WareId;
            model.StoreId = voucher.StoreId;
            model.IsAccountSelected = voucher.IsAccountSelected;
            string name = "";
            if (voucher.InternalAccId > 0)
            {
                name = db.AccountNames.Where(m => m.AccId == voucher.InternalAccId).Select(m => m.AccountName1).FirstOrDefault();
            }
            else
            {
                name = db.BankAccountLists.Where(m => m.BankAccountId == voucher.BankAccId).Select(m => m.AccountName).FirstOrDefault();
            }
            ViewBag.name = name;
            ViewBag.VoucherType = new SelectList(db.VoucherTypes, "VoucherTypeId", "VoucherTypeName", model.VoucherTypeId);
            return PartialView(model);
        }
        public JsonResult CheckIdForVatAmount(int Id)
        {
            if (db.Voucher_Other_Cost_Type.Where(m => m.Id == Id && m.HasPercentile == true).Any())
            {
                return Json(db.Voucher_Other_Cost_Type.Where(m => m.Id == Id && m.HasPercentile == true).FirstOrDefault(), JsonRequestBehavior.AllowGet);
            }
            return Json(false, JsonRequestBehavior.AllowGet);
        }
        public JsonResult UpdateVoucher(VoucherModelView model, int VoucherId, string[] AllOtherSevice, string[] AllVoucherProduct)
        {
            string msg = "";
            string msg1 = "";
            int OperationStatus = 1;
            long VoucherArchieveId = 0;
            int serviceColumnId = 0;
            int otherServiceColumnId = 0;
            if (VoucherId > 0)
            {
                Voucher archive = db.Vouchers.Find(VoucherId);
                Voucher vchr = new Voucher();
                vchr.VoucherTitle = model.VoucherTitle;
                vchr.VoucherTypeId = model.VoucherTypeId;
                vchr.Number = model.Number;
                vchr.TotalAmount = model.TotalAmount;
                vchr.CreatedBy = archive.CreatedBy;
                vchr.CreatedDate = archive.CreatedDate;
                vchr.Status = model.Status;
                vchr.UpdatedBy = model.CreatedBy;
                vchr.InternalAccId = model.InternalAccId;
                vchr.BankAccId = model.BankAccId;
                vchr.IsArchive = false;
                vchr.UpdatedDate = now;
                vchr.IsAccountSelected = model.IsAccountSelected;
                if (model.LocationId != null)
                {
                    string charId = model.LocationId.Split('-')[1];
                    int id = Convert.ToInt32(model.LocationId.Split('-')[0]);
                    if (charId == "s") { vchr.SiteId = id; }
                    if (charId == "u") { vchr.UnitId = id; }
                    if (charId == "d") { vchr.DeptId = id; }
                    if (charId == "l") { vchr.LineId = id; }
                    if (charId == "st") { vchr.StoreId = id; }
                    if (charId == "w") { vchr.WareId = id; }
                }
                db.Vouchers.Add(vchr);
                try
                {
                    db.SaveChanges();
                    OperationStatus = 1;
                    VoucherArchieveId = db.Vouchers.Where(m => m.UpdatedBy == model.CreatedBy).Max(m => m.VoucherId);
                    archive.IsArchive = true;
                    archive.ParentId = VoucherArchieveId;
                    db.Entry(archive).State = EntityState.Modified;
                    try
                    {
                        db.SaveChanges();
                        if (db.Vouchers.Where(m => m.ParentId == archive.VoucherId).Any()) {
                            var oldVou = db.Vouchers.Where(m => m.ParentId == archive.VoucherId).ToList();
                            foreach(var y in oldVou){
                           Voucher t = db.Vouchers.Where(m=>m.VoucherId == y.VoucherId).FirstOrDefault();
                            t.ParentId = VoucherArchieveId;
                            db.Entry(t).State = EntityState.Modified;
                            db.SaveChanges();
                            }
                        }
                        msg = "Voucher '" + archive.VoucherTitle + "' has been successfully archived on " + now.ToString("MMM dd , yyy hh:mm tt") + " .";
                    }
                    catch (Exception)
                    {
                        OperationStatus = -1;
                        msg = "Voucher '" + archive.VoucherTitle + "' archive was unsuccessful on " + now.ToString("MMM dd , yyy hh:mm tt") + " .";
                    }
                    SaveAuditLog("Voucher Archived", msg, "Voucher", "VoucherList", "Voucher", (int)VoucherId, model.CreatedBy, now, OperationStatus);
                    msg1 = "Voucher '" + model.VoucherTitle + "' has been successfully updated on " + now.ToString("MMM dd , yyy hh:mm tt") + " .";
                }
                catch (Exception)
                {
                    OperationStatus = -1;
                    msg1 = "Voucher '" + model.VoucherTitle + "' update unsuccessful on " + now.ToString("MMM dd , yyy hh:mm tt") + " .";
                    throw;
                }
                SaveAuditLog("Voucher Updated", msg1, "Voucher", "VoucherList", "Voucher", (int)VoucherArchieveId, model.CreatedBy, now, OperationStatus);
                if (AllVoucherProduct != null)
                {
                    for (int i = 0; i < AllVoucherProduct.Length; i++)
                    {
                        Voucher_Service_List prdct = new Voucher_Service_List();
                        prdct.VoucherId = Convert.ToInt32(VoucherArchieveId);
                        prdct.Name = AllVoucherProduct[i].Split('|')[0];
                        prdct.Amount = Convert.ToDecimal(AllVoucherProduct[i].Split('|')[1]);
                        prdct.CreatedBy = model.CreatedBy;
                        prdct.CreatedDate = now;
                        db.Voucher_Service_List.Add(prdct);
                        try
                        {
                            db.SaveChanges();
                            OperationStatus = 1;
                            serviceColumnId = db.Voucher_Service_List.Where(m => m.CreatedBy == model.CreatedBy).Max(m => m.VoucherServiceId);
                            msg = "Voucher service '" + prdct.Name + "' has been successfully updated to voucher '" + model.VoucherTitle + "' on " + now.ToString("MMM dd , yyy hh:mm tt") + " .";
                        }
                        catch (Exception)
                        {
                            OperationStatus = -1;
                            msg = "Voucher service '" + prdct.Name + "' update with voucher '" + model.VoucherTitle + "' on " + now.ToString("MMM dd , yyy hh:mm tt") + "unsuccessful.";
                            throw;
                        }
                        SaveAuditLog("Update Voucher Service", msg, "Voucher", "CreateVoucher", "Voucher_Service_List", serviceColumnId, model.CreatedBy, now, OperationStatus);
                    }
                }
                if (AllOtherSevice != null)
                {
                    for (int i = 0; i < AllOtherSevice.Length; i++)
                    {
                        Voucher_Other_Costs_List service = new Voucher_Other_Costs_List();
                        service.VoucherId = Convert.ToInt32(VoucherArchieveId);
                        service.ServiceId = Convert.ToInt32(AllOtherSevice[i].Split('|')[0]);
                        service.Amount = Convert.ToDecimal(AllOtherSevice[i].Split('|')[1]);
                        service.CreatedBy = model.CreatedBy;
                        service.CreatedDate = now;
                        db.Voucher_Other_Costs_List.Add(service);
                        string otherServiceName = db.Voucher_Other_Cost_Type.Where(m => m.Id == service.ServiceId).Select(m => m.ExtraServiceName).FirstOrDefault();
                        try
                        {
                            db.SaveChanges();
                            OperationStatus = 1;
                            otherServiceColumnId = db.Voucher_Other_Costs_List.Where(m => m.CreatedBy == model.CreatedBy).Max(m => m.OtherCostId);
                            msg = "Voucher other service '" + otherServiceName + "' has been successfully update to voucher '" + model.VoucherTitle + "' on " + now.ToString("MMM dd , yyy hh:mm tt") + " .";
                        }
                        catch (Exception ex)
                        {
                            string errorMsg = ex.Message.ToString();
                            OperationStatus = -1;
                            msg = "Voucher other service '" + otherServiceName + "' update with voucher '" + model.VoucherTitle + "' on " + now.ToString("MMM dd , yyy hh:mm tt") + " unsuccessful.";
                        }
                        SaveAuditLog("Update Voucher Other Service ", msg, "Voucher", "CreateVoucher", "Voucher_Other_Costs_List", otherServiceColumnId, model.CreatedBy, now, OperationStatus);
                    }
                }
                msg = "New Voucher '" + model.VoucherTitle + "' has been successfully created on " + now.ToString("MMM dd , yyy hh:mm tt") + " .";
                return Json("Success", JsonRequestBehavior.AllowGet);
            }
            return Json("Error", JsonRequestBehavior.AllowGet);
        }                
        #endregion

        //********************** All Voucher List ******************************
        #region
        public ActionResult VoucherList()
        {
            return View();
        }
        public ActionResult _VoucherList(int? VoucherId)
        {
            var all_Voucher_list = db.View_VoucherInfo.Where(m => m.IsArchive == false).OrderByDescending(m => m.CreatedDate).ToList();
            if (VoucherId != null)
            {
                all_Voucher_list = all_Voucher_list.Where(m => m.VoucherId == VoucherId).ToList();
            }
            ViewBag.VoucherList = all_Voucher_list.Take(ShowVoucherPerPage).ToList();
            ViewBag.TotalVoucher = all_Voucher_list.Count();
            Session["TotalVoucherCount"] = all_Voucher_list.Count();
            Session["AllVoucher"] = all_Voucher_list;
            return PartialView(ViewBag.VoucherList);
        }
        public PartialViewResult _VoucherListDayWise(int days)
        {
            var all_Voucher_list = db.View_VoucherInfo.Where(m => m.IsArchive == false).OrderByDescending(m => m.CreatedDate).ToList();
            DateTime countDate = DateTime.Today;
            int day = Convert.ToInt32(days);
            if (day > 1)
            {
                countDate = countDate.AddDays(-(day));
            }
            if (day > 0)
            {
                all_Voucher_list = all_Voucher_list.Where(m => m.CreatedDate > countDate).OrderByDescending(m => m.CreatedDate).ToList();
            }
            ViewBag.VoucherList = all_Voucher_list.Take(ShowVoucherPerPage).ToList();
            ViewBag.TotalVoucher = all_Voucher_list.Count();
            Session["TotalVoucherCount"] = all_Voucher_list.Count();
            Session["AllVoucher"] = all_Voucher_list;
            return PartialView("_VoucherList", ViewBag.VoucherList);
        }
        public JsonResult GetVoucherList(int? page)
        {
            page = page ?? 0;
            int total = Convert.ToInt16(Session["TotalVoucherCount"]);
            int skip = ShowVoucherPerPage * Convert.ToInt16(page);
            var canPage = skip < total;
            if (canPage)
            {
                var listVoucherFromSession = (List<View_VoucherInfo>)Session["AllVoucher"];
                var voucherList = listVoucherFromSession.Skip(skip).Take(ShowVoucherPerPage).ToList();
                return Json(voucherList, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json("", JsonRequestBehavior.AllowGet);
            }
        }
        public JsonResult SearchVoucherList(string prefix)
        {
            if (prefix == "")
            {
                return Json("", JsonRequestBehavior.AllowGet);
            }
            else
            {
                var allvoucher = (List<View_VoucherInfo>)Session["AllVoucher"];
                var all_Voucher_list = db.View_VoucherInfo.Where(m => m.VoucherTitle.ToLower().Contains(prefix.ToLower()) && m.IsArchive == false).OrderBy(m => m.VoucherTitle).ToList();
                Session["AllVoucherBySearch"] = all_Voucher_list;
                return Json(all_Voucher_list, JsonRequestBehavior.AllowGet);
            }
        }
        #endregion
        #region Transaction list

        public PartialViewResult _TransactionforInternalAccount(int? InternalAccId)
        {
            var list = db.View_Voucher_Transaction.Where(m => m.InternalAccId == InternalAccId).OrderByDescending(m => m.CreatedDate).ToList();
            Session["AllVoucherTranShow"] = list;
            Session["TotalVoucherTranShow"] = list.Count();
            ViewBag.Count = list.Count();
            list = list.Take(VoucherTranshowInternal).ToList();
            return PartialView(list);
        }
        public JsonResult GetVouTranForAccount(int? page, int? page1 , int? page2)
        {
            page = page ?? 0;
            int total = Convert.ToInt16(Session["TotalVoucherTranShow"]);
            int skip = VoucherTranshowInternal * Convert.ToInt16(page);
            var canPage = skip < total;
            if (canPage)
            {
                var listBackFromSession = (List<View_Voucher_Transaction>)Session["AllVoucherTranShow"];
                var partslist = listBackFromSession.Skip(skip).Take(VoucherTranshowInternal).ToList();
                return Json(partslist, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json("", JsonRequestBehavior.AllowGet);
            }
        }

        #endregion

        #region
        public ActionResult ConnectAccWithInvoice(long? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            else
            {
                View_VoucherInfo model = db.View_VoucherInfo.Where(m => m.VoucherId == id).FirstOrDefault();
                ViewBag.InternalAccId = new SelectList(db.AccountNames.Where(m => m.Acc_CashType == 1), "AccId", "AccountName1");
                ViewBag.BankAccId = new SelectList(db.BankAccountLists, "BankAccountId", "AccountNumber");
                return View(model);
            }
        }
        public JsonResult GetInternalAcc(int typeId)
        {
            if (typeId == 2)
            {
                return Json(db.AccountNames.Where(m => m.Acc_CashType== 1|| m.Acc_CashType ==3).ToList(),JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(db.AccountNames.Where(m => m.Acc_CashType == 2).ToList(), JsonRequestBehavior.AllowGet);
            }
        }
        public JsonResult GetBankAcc(int typeId)
        {
            if (typeId == 3)
            {
                var list = (from m in db.BankAccountLists
                            join b in db.BankBranchLists on m.BranchId equals b.BranchId
                            where m.Acc_Type == 1
                            select new
                            {
                                BankAccountId = m.BankAccountId,
                                AccountNumber = m.AccountNumber+"(Bank Name :"+db.BankLists.Where(p=> p.BankId == b.BankId).Select(p => p.BankName).FirstOrDefault()+ "; Branch Name :"+b.BranchName+" )",
                                Balance = m.Balance
                            }).ToList();
                return Json(list, JsonRequestBehavior.AllowGet);
            }
            else
            {
                var list = (from m in db.BankAccountLists
                            join b in db.BankBranchLists on m.BranchId equals b.BranchId
                            where m.Acc_Type == 2
                            select new
                            {
                                BankAccountId = m.BankAccountId,
                                AccountNumber = m.AccountNumber + "(Bank Name :" + db.BankLists.Where(p => p.BankId == b.BankId).Select(p => p.BankName).FirstOrDefault() + "; Branch Name :" + b.BranchName + " )",
                                Balance = m.Balance
                            }).ToList();
                return Json(list, JsonRequestBehavior.AllowGet);
            }
        }
        public JsonResult SaveConnectedAmount(long? voucherId, int accId,decimal amnt,long userId, int? forDebit)
        {
            if (voucherId > 0 && accId > 0 && amnt > 0)
            {
                Voucher vcher = db.Vouchers.Find(voucherId);
                if (vcher != null)
                {
                    long maxtTranId = 1;

                    int status = -1;
                    vcher.Status = 1;
                    vcher.AccId = accId;
                    vcher.DispatchedAmnt = amnt;
                    vcher.AmntDispatchedBy = userId;
                    vcher.AmntDispatchedDate = now;
                    db.Entry(vcher).State = EntityState.Modified;
                    try
                    {
                        db.SaveChanges();
                        status = 1;
                        if (db.Voucher_Transaction_List.Any())
                        {
                            maxtTranId = db.Voucher_Transaction_List.Count();
                            maxtTranId++;
                        }

                        Voucher_Transaction_List tran = new Voucher_Transaction_List();
                        tran.TranName = "Transaction#" + maxtTranId.ToString("000");
                        tran.VoucherId =Convert.ToInt32(voucherId);
                        if (vcher.VoucherTypeId > 2)
                        {
                            var bankAccDetails = db.BankAccountLists.Find(accId);
                            tran.BankAccId = bankAccDetails.BankAccountId;
                            tran.BranchId = bankAccDetails.BranchId;
                            tran.AmountIncrease = (vcher.VoucherId == 4) ? true : false;
                        }
                        else
                        {
                            tran.InternalAccId = accId;
                            tran.AmountIncrease = (vcher.VoucherId == 1) ? true : false;
                        }
                        tran.Amout = amnt;
                        tran.CreatedBy = userId;
                        tran.CreatedDate = now;
                        if (forDebit == 1)
                        {
                            tran.ApproveStatus = 1;
                            tran.ApprovedBy = userId;
                            tran.ApprovedDate = now;
                        }
                        else {
                            tran.ApproveStatus = 0;
                        }
                        db.Voucher_Transaction_List.Add(tran);
                        db.SaveChanges();
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
                
            }
            return Json("Error",JsonRequestBehavior.AllowGet);
        }
        #endregion


        //********************** Voucher Transaction List ******************************************
        #region
        public ActionResult InvoiceTransaction()
        {
            return View();
        }
        public PartialViewResult _InvoiceTransaction()
        {
            var list = db.View_Voucher_Transaction.Where(m => m.ApproveStatus < 1).OrderByDescending(m => m.CreatedDate).ToList();
            Session["AllInvoiceTran"] = list;
            Session["TotalInvoiceTran"] = list.Count();
            ViewBag.Count = list.Count();
            list = list.Take(InvoiceTran).ToList();
            return PartialView(list);
        }
        public PartialViewResult _InvoiceTransactionDayWise(int days)
        {
            DateTime countDate = DateTime.Today;
            var list = db.View_Voucher_Transaction.Where(m => m.ApproveStatus < 1).OrderByDescending(m => m.CreatedDate).ToList();
            int day = Convert.ToInt32(days);
            if (day > 1)
            {
                countDate = countDate.AddDays(-(day));
            }
            if (day > 0)
            {
                list = list.Where(m => m.CreatedDate > countDate).OrderByDescending(m => m.CreatedDate).ToList();
            }

            Session["AllInvoiceTran"] = list;
            Session["TotalInvoiceTran"] = list.Count();
            ViewBag.Count = list.Count();
            list = list.Take(InvoiceTran).ToList();
            return PartialView("_InvoiceTransaction", list);
        }
        public JsonResult GetVouTran(int? page)
        {
            page = page ?? 0;
            int total = Convert.ToInt16(Session["TotalInvoiceTran"]);
            int skip = InvoiceTran * Convert.ToInt16(page);
            var canPage = skip < total;
            if (canPage)
            {
                var listBackFromSession = (List<View_Voucher_Transaction>)Session["AllInvoiceTran"];
                var partslist = listBackFromSession.Skip(skip).Take(InvoiceTran).ToList();
                return Json(partslist, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json("", JsonRequestBehavior.AllowGet);
            }
        }
        public JsonResult InvoiceTranStatusChange(long? id,long userId,int status)
        {
            if (id > 0)
            {
                int opstatus = -1;
                string msg = "";
                string opName = "";
                Voucher_Transaction_List tran = db.Voucher_Transaction_List.Find(id);
                tran.ApproveStatus = status;
                if (status == 1)
                {
                    tran.ApprovedBy = userId;
                    tran.ApprovedDate = now;
                }
                else if (status == 2)
                {
                    tran.ReceivedBy = userId;
                    tran.ReceivedDate = now;
                }
                else
                {
                    tran.UpdatedBy = userId;
                    tran.UpdatedDate = now;
                }
                db.Entry(tran).State = EntityState.Modified;
                try
                {
                    db.SaveChanges();
                    opstatus = 1;
                    if (status == 1)
                    {
                        opName = "Transaction Approve";
                        msg = "Transaction '" + tran.TranName + "' has been successfully approved on " + now.ToString("MMM dd, yyyy hh:mm tt") + " .";
                    }
                    else if (status == 1)
                    {
                        opName = "Transaction Amount Received";
                        msg = "Transaction '" + tran.TranName + "' amount has been successfully received on " + now.ToString("MMM dd, yyyy hh:mm tt") + " .";
                    }
                    else
                    {
                        opName = "Transaction Reject";
                        msg = "Transaction '" + tran.TranName + "' has been successfully rejected on " + now.ToString("MMM dd, yyyy hh:mm tt") + " .";
                    }
                    SaveAuditLog(opName, msg, "Finance", "All Transaction", "Voucher_Transaction_List", Convert.ToInt32(tran.Id), userId, now, 1);

                    if (status == 1)
                    {
                        //Voucher vchr = db.Vouchers.Find(tran.VoucherId);
                        if (tran.InternalAccId > 0)
                        {
                            AccountName internal_Acc = db.AccountNames.Find(tran.InternalAccId);
                            if (tran.AmountIncrease)
                            {
                                internal_Acc.Balance = internal_Acc.Balance + Convert.ToDecimal(tran.Amout);
                            }
                            else
                            {
                                internal_Acc.Balance = internal_Acc.Balance - Convert.ToDecimal(tran.Amout);
                            }
                            internal_Acc.UpdatedBy = userId;
                            internal_Acc.UpdatedDate = now;
                            db.Entry(internal_Acc).State = EntityState.Modified;
                            try
                            {
                                db.SaveChanges();
                                opName = "Internal Account Information Update";
                                msg = "Internal account '" + internal_Acc.AccountName1 + "' amount has been successfully updated on " + now.ToString("MMM dd, yyyy hh:mm tt") + " .";
                                SaveAuditLog(opName, msg, "Finance", "All Transaction", "AccountName", Convert.ToInt32(internal_Acc.AccId), userId, now, 1);
                            }
                            catch (Exception)
                            {
                                opName = "Internal Account Information Update";
                                msg = "Internal account '" + internal_Acc.AccountName1 + "' amountupdate was unsuccessful on " + now.ToString("MMM dd, yyyy hh:mm tt") + " .";
                                SaveAuditLog(opName, msg, "Finance", "All Transaction", "AccountName", Convert.ToInt32(internal_Acc.AccId), userId, now, 1);
 
                            }
                        }
                        //********************** Bank Account Amount Update ***************************
                        if (tran.BankAccId > 0)
                        {
                            opName = "Bank Account Information Update";
                            BankAccountList bank_Acc = db.BankAccountLists.Find(tran.BankAccId);
                            if (tran.AmountIncrease)
                            {
                                bank_Acc.Balance = bank_Acc.Balance + Convert.ToDecimal(tran.Amout);
                            }
                            else
                            {
                                bank_Acc.Balance = bank_Acc.Balance - Convert.ToDecimal(tran.Amout);
                            }
                            bank_Acc.UpdatedBy = userId;
                            bank_Acc.UpdatedDate = now;
                            db.Entry(bank_Acc).State = EntityState.Modified;
                            try
                            {
                                db.SaveChanges();
                                msg = "Bank account '" + bank_Acc.AccountNumber + "' amount has been successfully updated on " + now.ToString("MMM dd, yyyy hh:mm tt") + " .";
                                SaveAuditLog(opName, msg, "Finance", "All Transaction", "BankAccountList", Convert.ToInt32(bank_Acc.BankAccountId), userId, now, 1);
                            }
                            catch (Exception)
                            {
                                msg = "Internal account '" + bank_Acc.AccountNumber + "' amountupdate was unsuccessful on " + now.ToString("MMM dd, yyyy hh:mm tt") + " .";
                                SaveAuditLog(opName, msg, "Finance", "All Transaction", "BankAccountList", Convert.ToInt32(bank_Acc.BankAccountId), userId, now, 1);
                            }
                        }
                    }
                }
                catch (Exception)
                {
                    opstatus = -1;
                    if (status == 1)
                    {
                        opName = "Transaction Approve";
                        msg = "Transaction '" + tran.TranName + "' successfully approve was unsuccessful on " + now.ToString("MMM dd, yyyy hh:mm tt") + " .";
                    }
                    else if (status == 1)
                    {
                        opName = "Transaction Amount Received";
                        msg = "Transaction '" + tran.TranName + "' amount receive was unsuccessful on " + now.ToString("MMM dd, yyyy hh:mm tt") + " .";
                    }
                    else
                    {
                        opName = "Transaction Reject";
                        msg = "Transaction '" + tran.TranName + "' reject was unsuccessful on " + now.ToString("MMM dd, yyyy hh:mm tt") + " .";
                    }
                    SaveAuditLog(opName, msg, "Finance", "All Transaction", "Voucher_Transaction_List", Convert.ToInt32(tran.Id), userId, now, 1);
                }
                if (opstatus == 1)
                {
                    return Json("Success", JsonRequestBehavior.AllowGet);
                }
                
            }
            return Json("Error",JsonRequestBehavior.AllowGet);
        }
        public PartialViewResult InvoiceTranDetails(long id)
        {
            var model = db.View_Voucher_Transaction.Where(m => m.Id == id).FirstOrDefault();
            return PartialView(model);
        }
        #endregion

        //********************** Voucher Transaction List ******************************************
        #region
        public ActionResult InvoiceTransNotReceive()
        {
            return View();
        }
        public PartialViewResult _InvoiceTransNotReceive()
        {
            var list = db.View_Voucher_Transaction.Where(m => m.ApproveStatus == 1).OrderByDescending(m => m.CreatedDate).ToList();
            Session["AllNotRcvInvoiceTran"] = list;
            Session["TotalNotRcvInvoiceTran"] = list.Count();
            ViewBag.Count = list.Count();
            list = list.Take(NotRcvInvoiceTran).ToList();
            return PartialView(list);
        }
        public PartialViewResult _InvoiceTransNotReceiveDayWise(int days)
        {
            DateTime countDate = DateTime.Today;
            var list = db.View_Voucher_Transaction.Where(m => m.ApproveStatus == 1).OrderByDescending(m => m.CreatedDate).ToList();
            int day = Convert.ToInt32(days);
            if (day > 1)
            {
                countDate = countDate.AddDays(-(day));
            }
            if (day > 0)
            {
                list = list.Where(m => m.CreatedDate > countDate).OrderByDescending(m => m.CreatedDate).ToList();
            }

            Session["AllNotRcvInvoiceTran"] = list;
            Session["TotalNotRcvInvoiceTran"] = list.Count();
            ViewBag.Count = list.Count();
            list = list.Take(NotRcvInvoiceTran).ToList();
            return PartialView("_InvoiceTransaction", list);
        }
        public JsonResult GetVouTranNotRecieve(int? page)
        {
            page = page ?? 0;
            int total = Convert.ToInt16(Session["TotalNotRcvInvoiceTran"]);
            int skip = NotRcvInvoiceTran * Convert.ToInt16(page);
            var canPage = skip < total;
            if (canPage)
            {
                var listBackFromSession = (List<View_Voucher_Transaction>)Session["AllNotRcvInvoiceTran"];
                var partslist = listBackFromSession.Skip(skip).Take(NotRcvInvoiceTran).ToList();
                return Json(partslist, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json("", JsonRequestBehavior.AllowGet);
            }
        }
        #endregion


        //********************** Voucher Transaction List ******************************************
        #region
        public ActionResult InvoiceTransactionForShow()
        {
            return View();
        }
        public PartialViewResult _InvoiceTransactionForShow()
        {
            var list = db.View_Voucher_Transaction.OrderByDescending(m => m.CreatedDate).ToList();
            Session["AllInvoiTranShow"] = list;
            Session["TotalInvoiTranShow"] = list.Count();
            ViewBag.Count = list.Count();
            list = list.Take(InvoiTranshow).ToList();
            return PartialView(list);
        }
        public PartialViewResult _InvoiceTranShowDayWise(int days)
        {
            DateTime countDate = DateTime.Today;
            var list = db.View_Voucher_Transaction.OrderByDescending(m => m.CreatedDate).ToList();
            int day = Convert.ToInt32(days);
            if (day > 1)
            {
                countDate = countDate.AddDays(-(day));
            }
            if (day > 0)
            {
                list = list.Where(m => m.CreatedDate > countDate).OrderByDescending(m => m.CreatedDate).ToList();
            }

            Session["AllInvoiTranShow"] = list;
            Session["TotalInvoiTranShow"] = list.Count();
            ViewBag.Count = list.Count();
            list = list.Take(InvoiTranshow).ToList();
            return PartialView("_InvoiceTransactionForShow", list);
        }
        public JsonResult GetVouTranForShow(int? page)
        {
            page = page ?? 0;
            int total = Convert.ToInt16(Session["TotalInvoiTranShow"]);
            int skip = InvoiTranshow * Convert.ToInt16(page);
            var canPage = skip < total;
            if (canPage)
            {
                var listBackFromSession = (List<View_Voucher_Transaction>)Session["AllInvoiTranShow"];
                var partslist = listBackFromSession.Skip(skip).Take(InvoiTranshow).ToList();
                return Json(partslist, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json("", JsonRequestBehavior.AllowGet);
            }
        }
        public PartialViewResult _TransactionListDayWise(int days)
        {
            DateTime countDate = now.Date;
            var list = db.View_Voucher_Transaction.OrderByDescending(m => m.CreatedDate).ToList();
            int day = Convert.ToInt32(days);
            if (day > 1)
            {
                countDate = countDate.AddDays(-(day));
            }
            if (day > 0)
            {
                list = list.Where(m => m.CreatedDate > countDate).OrderByDescending(m => m.CreatedDate).ToList();
            }

            Session["AllVoucherTranShow"] = list;
            Session["TotalVoucherTranShow"] = list.Count();
            ViewBag.Count = list.Count();
            list = list.Take(VoucherTranshowInternal).ToList();
            return PartialView("_TransactionforInternalAccount", list);
        }
        #endregion
        
        //********************** All Voucher List ***************************************************

        #region
        public ActionResult InvoiceServiceList()
        {
            return View();
        }
        public PartialViewResult _InvoiceServiceList()
        {
            var list = db.View_Invoice_Service.OrderByDescending(m => m.CreatedDate).ToList();
            Session["AllVouServ"] = list;
            Session["TotalVouServ"] = list.Count();
            ViewBag.Count = list.Count();
            list = list.Take(InvServListCount).ToList();
            return PartialView(list);
        }
        public PartialViewResult _InvoiceServiceListDayWise(int days)
        {
            DateTime countDate = DateTime.Today;
            var list = db.View_Invoice_Service.OrderByDescending(m => m.CreatedDate).ToList();
            int day = Convert.ToInt32(days);
            if (day > 1)
            {
                countDate = countDate.AddDays(-(day));
            }
            if (day > 0)
            {
                list = list.Where(m => m.CreatedDate > countDate).OrderByDescending(m => m.CreatedDate).ToList();
            }

            Session["AllVouServ"] = list;
            Session["TotalVouServ"] = list.Count();
            ViewBag.Count = list.Count();
            list = list.Take(InvServListCount).ToList();
            return PartialView("_InvoiceServiceList", list);
        }
        public PartialViewResult _InvoiceServiceListLocWise(string locaId)
        {
            var list = db.View_Invoice_Service.OrderByDescending(m => m.CreatedDate).ToList();
            if (locaId != String.Empty)
            {
                string conchar = locaId.Split('-')[1];
                int id =Convert.ToInt32(locaId.Split('-')[0]);
                if (conchar == "s")
                {
                    list = list.Where(m => m.SiteId == id).OrderByDescending(m => m.CreatedDate).ToList();
                }
                else if (conchar == "u")
                {
                    list = list.Where(m => m.UnitId == id).OrderByDescending(m => m.CreatedDate).ToList();
                }
                else if (conchar == "d")
                {
                    list = list.Where(m => m.DeptId == id).OrderByDescending(m => m.CreatedDate).ToList();
                }
                else if (conchar == "l")
                {
                    list = list.Where(m => m.LineId == id).OrderByDescending(m => m.CreatedDate).ToList();
                }
                else if (conchar == "st")
                {
                    list = list.Where(m => m.StoreId == id).OrderByDescending(m => m.CreatedDate).ToList();
                }
                else if (conchar == "w")
                {
                    list = list.Where(m => m.WareId == id).OrderByDescending(m => m.CreatedDate).ToList();
                }
            }
            Session["AllVouServ"] = list;
            Session["TotalVouServ"] = list.Count();
            ViewBag.Count = list.Count();
            list = list.Take(InvServListCount).ToList();
            return PartialView("_InvoiceServiceList", list);
        }
        public JsonResult GetInvoiceServiceList(int? page)
        {
            page = page ?? 0;
            int total = Convert.ToInt16(Session["TotalVouServ"]);
            int skip = InvServListCount * Convert.ToInt16(page);
            var canPage = skip < total;
            if (canPage)
            {
                var listBackFromSession = (List<View_Invoice_Service>)Session["AllVouServ"];
                var partslist = listBackFromSession.Skip(skip).Take(InvServListCount).ToList();
                return Json(partslist, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json("", JsonRequestBehavior.AllowGet);
            }
        }
        public JsonResult GetAllPlaceId()
        {
            var list = (from m in db.View_Invoice_Service
                        select new 
                        {
                            SiteId = (m.SiteId > 0) ? m.SiteId : (m.UnitId > 0) ? m.UnitId : (m.DeptId > 0) ? m.DeptId : (m.LineId > 0) ? m.LineId : (m.StoreId > 0) ? m.StoreId : m.WareId,
                            SiteIdWithChar = (m.SiteId > 0) ? m.SiteId + "-s" : (m.UnitId > 0) ? m.UnitId + "-u" : (m.DeptId > 0) ? m.DeptId + "-d" : (m.LineId > 0) ? m.LineId + "-l" : (m.StoreId > 0) ? m.StoreId + "-st" : m.WareId + "-w",
                            SiteName = (m.SiteId > 0) ? m.SiteName : (m.UnitId > 0) ? m.UnitName : (m.DeptId > 0) ? m.DeptName : (m.LineId > 0) ? m.LineName : (m.StoreId > 0) ? m.StoreName : m.WareName
                        }).ToList();

            list = (from bs in list
                         group bs by bs.SiteId into g
                         select new 
                             {
                                 SiteId = g.FirstOrDefault().SiteId,
                                 SiteIdWithChar = g.FirstOrDefault().SiteIdWithChar,
                                 SiteName = g.FirstOrDefault().SiteName
                             }).ToList();
            return Json(list,JsonRequestBehavior.AllowGet);
        }
       
        #endregion

        #region

        //public JsonResult DebitTransactionApproval(int? id, long CreatedBy) 
        //{
           
        //    Voucher_Transaction_List voucher = db.Voucher_Transaction_List.Where(m => m.Id == id).FirstOrDefault();
        //    voucher.ApproveStatus = 1;
        //    db.Entry(voucher).State = EntityState.Modified;
        //    db.SaveChanges();
        //    return Json("Success", JsonRequestBehavior.AllowGet);
        //}
        #endregion

        //Salary Pay details
        
       public PartialViewResult SalaryPayDetails(int? id)
        {
            var model = db.View_All_Salary_Pay_His.Where(m => m.Id == id).FirstOrDefault();
            return PartialView(model);
           
        }
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