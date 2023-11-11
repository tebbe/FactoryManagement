using FactoryManagement.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace FactoryManagement.Controllers
{
    public class RemoteValidationController : Controller
    {
        private FactoryManagementEntities db = new FactoryManagementEntities();


        //**********************************************************************************************************************
        //********************************************** FOR CONFIGURATION *****************************************************
        //**********************************************************************************************************************

        public JsonResult DepartmentNameExist(int? DeptId, string DeptName, int? SiteId, int? UnitId)
        {
            bool exsits;
            if (SiteId != null)
            {
                exsits = (from m in db.DepartmentLists.Where(m => (DeptId.HasValue) ? (m.DeptId != DeptId && m.DeptName == DeptName && m.SiteId == SiteId) : (m.DeptName == DeptName)) select m).Any();
            }
            else
            {
                exsits = (from m in db.DepartmentLists.Where(m => (DeptId.HasValue) ? (m.DeptId != DeptId && m.DeptName == DeptName && m.UnitId == UnitId) : (m.DeptName == DeptName)) select m).Any();
            }
            if (exsits) { return Json(false, JsonRequestBehavior.AllowGet); }
            else { return Json(true, JsonRequestBehavior.AllowGet); }
        }
        public JsonResult DepartmentAcronymExist(int? DeptId, string DeptAcronym, int? SiteId, int? UnitId)
        {
            bool exsits;
            if (SiteId != null)
            {
                exsits = (from m in db.DepartmentLists.Where(m => (DeptId.HasValue) ? (m.DeptId != DeptId && m.DeptAcronym == DeptAcronym && m.SiteId == SiteId) : (m.DeptAcronym == DeptAcronym)) select m).Any();
            }
            else
            {
                exsits = (from m in db.DepartmentLists.Where(m => (DeptId.HasValue) ? (m.DeptId != DeptId && m.DeptAcronym == DeptAcronym && m.UnitId == UnitId) : (m.DeptAcronym == DeptAcronym)) select m).Any();
            }
            if (exsits) { return Json(false, JsonRequestBehavior.AllowGet); }
            else { return Json(true, JsonRequestBehavior.AllowGet); }
        }
        public JsonResult SiteUnitNameExist(int? UnitId, string UnitName, int? SiteId)
        {
            bool exsits = (from m in db.Site_Unit_Lists.Where(m => (UnitId.HasValue) ? (m.UnitId != UnitId && m.UnitName == UnitName && m.SiteId == SiteId) : (m.UnitName == UnitName)) select m).Any();
            if (exsits) { return Json(false, JsonRequestBehavior.AllowGet); }
            else { return Json(true, JsonRequestBehavior.AllowGet); }
        }
        public JsonResult SiteUnitAcryonmExist(string UnitAcryonm, int? SiteId, int? UnitId)
        {
            bool exists = db.Site_Unit_Lists.Where(x => (SiteId.HasValue) ? (x.SiteId == SiteId && x.UnitAcryonm == UnitAcryonm && x.UnitId != UnitId) : (x.UnitAcryonm == UnitAcryonm)).Any();
            if (exists) { return Json(false, JsonRequestBehavior.AllowGet); }
            else { return Json(true, JsonRequestBehavior.AllowGet); }
        }


        public JsonResult BankNameExist(int? BankId, string BankName)
        {
            bool exsits = (from m in db.BankLists.Where(m => (BankId.HasValue) ? (m.BankId != BankId && m.BankName == BankName) : (m.BankName == BankName)) select m).Any();
            if (exsits) { return Json(false, JsonRequestBehavior.AllowGet); }
            else { return Json(true, JsonRequestBehavior.AllowGet); }
        }
        public JsonResult SiteNameExist(int? Id, string SiteName)
        {
            bool exsits = (from m in db.SiteLists.Where(m => (Id.HasValue) ? (m.Id != Id && m.SiteName == SiteName) : (m.SiteName == SiteName)) select m).Any();
            if (exsits) { return Json(false, JsonRequestBehavior.AllowGet); }
            else { return Json(true, JsonRequestBehavior.AllowGet); }
        }
        public JsonResult DesignationNameExist(int? DesignationId, string DesignationName)
        {
            bool exsits = (from m in db.Designations.Where(m => (DesignationId.HasValue) ? (m.DesignationId != DesignationId && m.DesignationName == DesignationName) : (m.DesignationName == DesignationName)) select m).Any();
            if (exsits) { return Json(false, JsonRequestBehavior.AllowGet); }
            else { return Json(true, JsonRequestBehavior.AllowGet); }
        }
        
        //**********************************************************************************************************************
        //********************************************** FOR HR ****************************************************************
        //**********************************************************************************************************************

        public JsonResult ClientNameExist(int? ClientId, string Name)
        {
            bool exsits = (from m in db.ClientLists.Where(m => (ClientId.HasValue) ? (m.ClientId != ClientId && m.Name == Name) : (m.Name == Name)) select m).Any();
            if (exsits) { return Json(false, JsonRequestBehavior.AllowGet); }
            else { return Json(true, JsonRequestBehavior.AllowGet); }
        }
        public JsonResult EmpIdExist(int? UserId, string EmpId)
        {
            bool exsits = (from m in db.UserInformations.Where(m => (UserId.HasValue) ? (m.UserId != UserId && m.EmpId == EmpId) : (m.EmpId == EmpId)) select m).Any();
            if (exsits) { return Json(false, JsonRequestBehavior.AllowGet); }
            else { return Json(true, JsonRequestBehavior.AllowGet); }
        }
        public JsonResult HolidayPackNameExists(int? HolidayPackId, string HolidayPackName)
        {
            bool exsits = (from m in db.HolidayPackageLists.Where(m => (HolidayPackId.HasValue) ? (m.HolidayPackId != HolidayPackId && m.HolidayPackName == HolidayPackName) : (m.HolidayPackName == HolidayPackName)) select m).Any();
            if (exsits) { return Json(false, JsonRequestBehavior.AllowGet); }
            else { return Json(true, JsonRequestBehavior.AllowGet); }
        }
        public JsonResult WorkingSchedulNameExists(int? WorkingScheduleId, string SchedulName)
        {
            bool exsits = (from m in db.WorkingScheduleLists.Where(m => (WorkingScheduleId.HasValue) ? (m.WorkingScheduleId != WorkingScheduleId && m.SchedulName == SchedulName) : (m.SchedulName == SchedulName)) select m).Any();
            if (exsits) { return Json(false, JsonRequestBehavior.AllowGet); }
            else { return Json(true, JsonRequestBehavior.AllowGet); }
        }

        //**********************************************************************************************************************
        //********************************************** FOR INVENTORY *********************************************************
        //**********************************************************************************************************************

        #region REMOTE VALIDATION FOR INAVENTORY MODULE
        public JsonResult MachineTypenameExsits(int? MachineId, string MachineTypeName1)
        {
            bool exsits = (from m in db.MachineTypeNames.Where(m => (MachineId.HasValue) ? (m.MachineId != MachineId && m.MachineTypeName1 == MachineTypeName1) : (m.MachineTypeName1 == MachineTypeName1)) select m).Any();
            if (exsits) { return Json(false, JsonRequestBehavior.AllowGet); }
            else { return Json(true, JsonRequestBehavior.AllowGet); }
        }
        public JsonResult MachineAcronynmExist(int? MachineTypeId, string MachineAcronym)
        {
            bool exsits = (from m in db.MachineLists.Where(m => (MachineTypeId.HasValue) ? (m.MachineTypeId == MachineTypeId && m.MachineAcronym == MachineAcronym) : (m.MachineAcronym == MachineAcronym)) select m).Any();
            if (exsits) { return Json(false, JsonRequestBehavior.AllowGet); }
            else { return Json(true, JsonRequestBehavior.AllowGet); }
        }
        public JsonResult SparePartsNameExist(int? PartsId, string PartsName)
        {
            bool exsits = (from m in db.SparePartsLists.Where(m => (PartsId.HasValue) ? (m.PartsId != PartsId && m.Name == PartsName) : (m.Name == PartsName)) select m).Any();
            if (exsits) { return Json(false, JsonRequestBehavior.AllowGet); }
            else { return Json(true, JsonRequestBehavior.AllowGet); }
        }
        public JsonResult WarehouseNameExist(int? Id, string WarehouseName)
        {
            bool exsits = (from m in db.WarehouseLists.Where(m => (Id.HasValue) ? (m.Id != Id && m.WarehouseName == WarehouseName) : (m.WarehouseName == WarehouseName)) select m).Any();
            if (exsits) { return Json(false, JsonRequestBehavior.AllowGet); }
            else { return Json(true, JsonRequestBehavior.AllowGet); }
        }
        public JsonResult StoreNameExist(int? Id, string StoreName)
        {
            bool exsits = (from m in db.StoreLists.Where(m => (Id.HasValue) ? (m.Id != Id && m.StoreName == StoreName) : (m.StoreName == StoreName)) select m).Any();
            if (exsits) { return Json(false, JsonRequestBehavior.AllowGet); }
            else { return Json(true, JsonRequestBehavior.AllowGet); }
        }
        public JsonResult ProductTypeNameExist(int? ProductTypeId, string ProductTypeName)
        {
            bool exsits = (from m in db.ProductTypeLists.Where(m => (ProductTypeId.HasValue) ? (m.ProductTypeId != ProductTypeId && m.ProductTypeName == ProductTypeName) : (m.ProductTypeName == ProductTypeName)) select m).Any();
            if (exsits) { return Json(false, JsonRequestBehavior.AllowGet); }
            else { return Json(true, JsonRequestBehavior.AllowGet); }
        }
        public JsonResult ProductNameExist(int? InventoryId, string ProductName)
        {
            bool exsits = (from m in db.InventoryLists.Where(m => (InventoryId.HasValue) ? (m.ProductTypeId != InventoryId && m.ProductName == ProductName) : (m.ProductName == ProductName)) select m).Any();
            if (exsits) { return Json(false, JsonRequestBehavior.AllowGet); }
            else { return Json(true, JsonRequestBehavior.AllowGet); }
        }
        public JsonResult BrandNameExist(int? BrandId, string BrandName)
        {
            bool exsits = (from m in db.BrandLists.Where(m => (BrandId.HasValue) ? (m.BrandId != BrandId && m.BrandName == BrandName) : (m.BrandName == BrandName)) select m).Any();
            if (exsits) { return Json(false, JsonRequestBehavior.AllowGet); }
            else { return Json(true, JsonRequestBehavior.AllowGet); }
        }
        public JsonResult UnitNameExist(int? UnitId, string UnitName)
        {
            bool exsits = (from m in db.UnitLists.Where(m => (UnitId.HasValue) ? (m.UnitId != UnitId && m.UnitName == UnitName) : (m.UnitName == UnitName)) select m).Any();
            if (exsits) { return Json(false, JsonRequestBehavior.AllowGet); }
            else { return Json(true, JsonRequestBehavior.AllowGet); }
        }
        #endregion


        //**********************************************************************************************************************
        //********************************************** FOR SALARY PACKAGE ****************************************************
        //**********************************************************************************************************************
        #region REMOTE VALIDATION FOR INAVENTORY MODULE
        public JsonResult PackageNameExist(int? PackageId, string PackageName)
        {
            bool exsits = (from m in db.Salary_Package_List.Where(m => (PackageId.HasValue) ? (m.PackageId != PackageId && m.PackageName == PackageName) : (m.PackageName == PackageName)) select m).Any();
            if (exsits) { return Json(false, JsonRequestBehavior.AllowGet); }
            else { return Json(true, JsonRequestBehavior.AllowGet); }
        }
        #endregion



        //**********************************************************************************************************************
        //********************************************** FOR SALARY ACCOUNTING *************************************************
        //**********************************************************************************************************************
        #region REMOTE VALIDATION FOR AACCOUNTING MODULE
        public JsonResult AccountGrpNameExsits(int? AccId, string AccountName)
        {
            bool exsits = (from m in db.AccountNames.Where(m => (AccId.HasValue) ? (m.AccId != AccId && m.AccountName1 == AccountName) : (m.AccountName1 == AccountName)) select m).Any();
            if (exsits) { return Json(false, JsonRequestBehavior.AllowGet); }
            else { return Json(true, JsonRequestBehavior.AllowGet); }
        }
        //public JsonResult AccountNameExsits(int? AccountNameId, string AccountName1)
        //{
        //    bool exsits = (from m in db.AccountNames.Where(m => (AccountNameId.HasValue) ? (m.AccountNameId != AccountNameId && m.AccountName1 == AccountName1) : (m.AccountName1 == AccountName1)) select m).Any();
        //    if (exsits) { return Json(false, JsonRequestBehavior.AllowGet); }
        //    else { return Json(true, JsonRequestBehavior.AllowGet); }
        //}
        #endregion
        public JsonResult LineNameExist(int? LineId, string LineName, int? DeptId)
        {
            bool exsits = (from m in db.LineInfoes.Where(m => (LineId.HasValue) ? (m.LineId != LineId && m.LineName == LineName && m.DeptId == DeptId) : (m.LineName == LineName)) select m).Any();
            if (exsits) { return Json(false, JsonRequestBehavior.AllowGet); }
            else { return Json(true, JsonRequestBehavior.AllowGet); }
        }
        public JsonResult LineAcronymExist(int? LineId, string LineAcronym, int? DeptId)
        {
            bool exsits = (from m in db.LineInfoes.Where(m => (LineId.HasValue) ? (m.LineId != LineId && m.LineAcronym == LineAcronym && m.DeptId == DeptId) : (m.LineAcronym == LineAcronym)) select m).Any();
            if (exsits) { return Json(false, JsonRequestBehavior.AllowGet); }
            else { return Json(true, JsonRequestBehavior.AllowGet); }
        }
        public JsonResult BranchNameExist(int? BranchId, int BankId, string BranchName)
        {
            bool exsits = (from m in db.BankBranchLists.Where(m => (BranchId.HasValue) ? (m.BranchId != BranchId && m.BranchName == BranchName && m.BankId == BankId) : (m.BranchName == BranchName)) select m).Any();
            if (exsits) { return Json(false, JsonRequestBehavior.AllowGet); }
            else { return Json(true, JsonRequestBehavior.AllowGet); }
        }

        //***********************E-mail remote validation****************
        public JsonResult CheckUserEmail(string email)
        {
            bool exsits = (from m in db.UserInformations.Where(m => m.EmailAddress == email) select m).Any();
            if (exsits) { return Json(true, JsonRequestBehavior.AllowGet); }
            else { return Json(false, JsonRequestBehavior.AllowGet); }
        }

        public JsonResult CheckVoucherName(string VoucherName)
        {
     
            bool voucherExit = (from m in db.ManualIndentVouchers.Where(m =>m.Name.Trim() == VoucherName.Trim()) select m).Any();
            if (voucherExit) { return Json(true, JsonRequestBehavior.AllowGet); }
            else { return Json(false, JsonRequestBehavior.AllowGet); }
        }
    }
}