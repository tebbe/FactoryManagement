
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
using System.ComponentModel;
using System.Configuration;
using FactoryManagement.Filters;

namespace FactoryManagement.Controllers
{
    [Authorize]
    [DisplayName("Salary Package Management")]
    public class SalaryConfigController : Controller
    {
        #region Private Properties
        private FactoryManagementEntities db = new FactoryManagementEntities();
        static private int offset = Convert.ToInt32(ConfigurationManager.AppSettings["localTime"]);
        DateTime now = DateTime.UtcNow.AddMinutes(offset);
        #endregion


        #region Salary Create, Update & Delete
        public PartialViewResult _SalaryPackageCreate(int? id)
        {
            ViewBag.id = id;
            return PartialView();
        }
        public PartialViewResult SalaryPackageCreate(int? id)
        {
            SalaryPackageModelView model = new SalaryPackageModelView();
            if (id > 0)
            {
                Salary_Package_List salaryPck = db.Salary_Package_List.Find(id);
                model.PackageId = salaryPck.PackageId;
                model.PackageName = salaryPck.PackageName;
                model.Amount = salaryPck.Amount;
            }
            return PartialView(model);
        }
        public JsonResult SalaryPackageSave(SalaryPackageModelView model)
        {
            string msg = "";
            int ColumnId = 0;
            int OperationStatus = 1;
            string operation = "";

            Salary_Package_List salaryPck = new Salary_Package_List();
            if (model.PackageId > 0)
            {
                operation = "Update Salary Package";
                salaryPck = db.Salary_Package_List.Find(model.PackageId);
                salaryPck.PackageName = model.PackageName;
                salaryPck.Amount = model.Amount;
                salaryPck.UpdatedBy = model.CreatedBy;
                salaryPck.UpdatedDate = now;
                db.Entry(salaryPck).State = EntityState.Modified;
                ColumnId = salaryPck.PackageId;
                msg = "Information of package '" + model.PackageName + "' has been successfully updated on " + now.ToString("MMM dd,yyyy hh:mm tt");
            }
            else
            {
                operation = "Add Salary Package";
                salaryPck.PackageName = model.PackageName;
                salaryPck.Amount = model.Amount;
                salaryPck.CreatedBy = model.CreatedBy;
                salaryPck.CreatedDate = now;
                salaryPck.PckStatus = 1;
                db.Salary_Package_List.Add(salaryPck);
                msg = "New packege '" + model.PackageName + "' has been successfully created on " + now.ToString("MMM dd,yyyy hh:mm tt");
            }
            try
            {
                db.SaveChanges();
                if (model.PackageId == 0)
                {
                    ColumnId = db.Salary_Package_List.Max(m => m.PackageId);
                }
            }
            catch (Exception ex)
            {
                string errorMsg = ex.Message.ToString();
                if (model.PackageId > 0)
                {
                    msg = model.PackageName + " information update unsuccessful on " + now.ToString("MMM dd,yyyy hh:mm tt");
                }
                else
                {
                    msg = "New packege " + model.PackageName + " creation unsuccessful on " + now.ToString("MMM dd,yyyy hh:mm tt");
                }
                OperationStatus = -1;
            }
            SaveAuditLog(operation, msg, "HR", "Salary Package", "Salary_Package_List", ColumnId, model.CreatedBy, now, OperationStatus);
            if (OperationStatus == 1)
            {
                return Json("Success", JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json("Error", JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult SalaryPackageList()
        {
            return View();
        }
        public ActionResult _SalaryPackageList(int? PackageId)
        {
            if (PackageId != null)
            {
                var salary = (from m in db.Salary_Package_List
                              where m.PackageId == PackageId
                              select new SalaryPackageModelView
                              {
                                  PackageId = m.PackageId,
                                  PackageName = m.PackageName,
                                  CreatedBy = m.CreatedBy,
                                  CreatedDate = m.CreatedDate,
                                  UpdatedBy = m.UpdatedBy,
                                  UpdatedDate = m.UpdatedDate,
                              }).ToList();
                ViewBag.SalaryPackage = salary.ToList();
                Session["AllSalaryPackage"] = salary;
            }
            else
            {
                var salary = (from m in db.Salary_Package_List
                              select new SalaryPackageModelView
                              {
                                  PackageId = m.PackageId,
                                  PackageName = m.PackageName,
                                  CreatedBy = m.CreatedBy,
                                  CreatedDate = m.CreatedDate,
                                  UpdatedBy = m.UpdatedBy,
                                  UpdatedDate = m.UpdatedDate,
                              }).ToList();
                ViewBag.SalaryPackage = salary.ToList();
                Session["AllSalaryPackage"] = salary;
            }
            return PartialView(ViewBag.SalaryPackage);
        }
        public JsonResult SearchSalaryPackage(string prefix)
        {
            if (prefix == "")
            {
                return Json("", JsonRequestBehavior.AllowGet);
            }
            else
            {
                var allsalarypackage = (List<SalaryPackageModelView>)Session["AllSalaryPackage"];
                var salary = (from m in allsalarypackage
                              where (m.PackageName.ToLower().Contains(prefix.ToLower()))
                              select new SalaryPackageModelView
                              {
                                  PackageId = m.PackageId,
                                  PackageName = m.PackageName,
                                  CreatedBy = m.CreatedBy,
                                  CreatedDate = m.CreatedDate,
                                  UpdatedBy = m.UpdatedBy,
                                  UpdatedDate = m.UpdatedDate,
                              }).ToList();
                Session["AllBankBySearch"] = salary;
                return Json(salary, JsonRequestBehavior.AllowGet);
            }
        }
        [EncryptedActionParameter]
        public ActionResult SalaryPackageConfig(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            else
            {
                SalayPackageConfig model = new SalayPackageConfig();
                Salary_Package_List salaryPck = db.Salary_Package_List.Where(m => m.PackageId == id).FirstOrDefault();
                model.PackageId = Convert.ToInt32(id);
                model.PackageName = salaryPck.PackageName;
                model.HasBaseSalary = salaryPck.HasBaseSalary;
                ViewBag.HasAddedAccount = db.Salary_Package_Account_Lists.Where(m => m.PackageId == id).Any();
                return View(model);
            }
        }
        public ActionResult AddedAccountWithPackList(int PackageId)
        {
            ViewBag.baseAmount = 0.0;
            if (db.Salary_Package_Account_Lists.Where(m => m.PackageId == PackageId && m.IsBaseSalary == true).Any())
            {
                ViewBag.baseAmount = Convert.ToDouble(db.Salary_Package_Account_Lists.Where(m => m.PackageId == PackageId && m.IsBaseSalary == true).Select(m => m.Amount).FirstOrDefault());
            }
            ViewBag.allAccName = (from m in db.Salary_Package_Account_Lists
                                  where m.PackageId == PackageId
                                  select new SalaryAccAddModelView
                                  {
                                      Id = m.Id,
                                      PackageId = m.PackageId,
                                      Name = m.Name,
                                      IsBaseSalary = m.IsBaseSalary,
                                      AccountType = m.AccountType,
                                      AccPayType = m.AccPayType,
                                      Amount = m.Amount,
                                      Percentage = m.Percentage,
                                      IsAddition = m.IsAddition
                                  }).ToList();
            return PartialView();
        }


        public ActionResult SalaryPackageAccAdd(int PackageId)
        {
            SalaryAccAddModelView model = new SalaryAccAddModelView();
            ViewBag.HolidayId = new SelectList(db.HolidayLists, "Id", "HolidayName", model.HolidayId);
            model.IsBaseSalary = (db.Salary_Package_Account_Lists.Where(m => m.PackageId == PackageId && m.IsBaseSalary == true).Any()) ? true : false;
            if (db.Salary_Package_Account_Lists.Where(m => m.PackageId == PackageId && m.IsBaseSalary == true).Any())
            {
                model.BaseSalary = db.Salary_Package_Account_Lists.Where(m => m.PackageId == PackageId && m.IsBaseSalary == true).Select(m => m.Amount).FirstOrDefault();
            }
            model.PackageId = PackageId;
            return PartialView(model);
        }
        public JsonResult GetSalaryCatagory(bool HasBaseSalary, bool isBas)
        {
            if (HasBaseSalary)
            {
                if (isBas)
                {
                    var list = db.Salary_Package_Catagory.ToList();
                    return Json(list, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    var list = db.Salary_Package_Catagory.Where(m => m.SalaryCatagoryId != 1).ToList();
                    return Json(list, JsonRequestBehavior.AllowGet);
                }
            }
            else
            {
                var list = db.Salary_Package_Catagory.ToList();
                return Json(list, JsonRequestBehavior.AllowGet);
            }
        }
        public JsonResult GetSalaryAccountType(bool IsBaseSalary)
        {
            if (IsBaseSalary)
            {
                var list = db.Salary_Account_Type.ToList();
                return Json(list, JsonRequestBehavior.AllowGet);
            }
            else
            {
                var list = db.Salary_Account_Type.Where(m => m.Id != 2).ToList();
                return Json(list, JsonRequestBehavior.AllowGet);
            }

        }
        public ActionResult AddAccountWithSalaryPck(SalaryAccAddModelView model)
        {
            if (ModelState.IsValid)
            {
                string msg = "";
                int ColumnId = 0;
                int OperationStatus = 1;
                string pckName = "";

                Salary_Package_Account_Lists salary_Acc = new Salary_Package_Account_Lists();
                salary_Acc.PackageId = model.PackageId;
                salary_Acc.CatagoryId = model.Catagory;
                salary_Acc.AccountType = model.AccountType;
                salary_Acc.IsBaseSalary = model.IsBaseSalary;
                salary_Acc.Name = model.Name + " " + model.CatagoryName;
                salary_Acc.AccPayType = Convert.ToInt32(model.AccPayType);
                salary_Acc.Amount = model.Amount;
                salary_Acc.Percentage = model.Percentage;
                salary_Acc.IsAddition = model.IsAddition;
                salary_Acc.HolidayId = model.HolidayId;
                salary_Acc.CreatedBy = model.CreatedBy;
                salary_Acc.CreatedDate = now;
                db.Salary_Package_Account_Lists.Add(salary_Acc);
                try
                {
                    db.SaveChanges();

                    if (model.IsBaseSalary)
                    {
                        Salary_Package_List package = db.Salary_Package_List.Find(model.PackageId);
                        pckName = package.PackageName;
                        package.HasBaseSalary = true;
                        db.Entry(package).State = EntityState.Modified;
                        db.SaveChanges();
                        msg = model.Name + " '" + model.CatagoryName + "' account type successfully added with '" + pckName + "' on " + now.ToString("MMM dd,yyyy hh:mm tt");
                    }

                    ColumnId = db.Salary_Package_Account_Lists.Max(m => m.Id);
                    if (Convert.ToInt32(model.AccPayType) == 1 && model.AllMonthId != "")
                    {
                        string[] allMonId = model.AllMonthId.Split(',');

                        for (int i = 0; i < allMonId.Length; i++)
                        {
                            Salary_Acc_Pay_MonthList month = new Salary_Acc_Pay_MonthList();
                            month.Salary_Pck_Acc_Id = ColumnId;
                            month.MonthId = Convert.ToInt32(allMonId[i].Split('-')[0]);
                            month.MonthName = allMonId[i].Split('-')[1].ToString();
                            db.Salary_Acc_Pay_MonthList.Add(month);
                            db.SaveChanges();
                        }
                    }
                    if (Convert.ToInt32(model.AccPayType) == 2 && model.HolidayId > 0)
                    {
                        int monthId = 0;
                        var holiday = db.HolidayLists.Find(model.HolidayId);
                        if (holiday.IsMultipleDay == false)
                        {
                            monthId = Convert.ToInt32(holiday.MonthCount);
                        }
                        else
                        {
                            monthId = db.Multiple_HolidayLists.Where(m => m.ParentId == holiday.Id).Select(m => m.MonthCount).FirstOrDefault();
                        }
                        Salary_Acc_Pay_MonthList month = new Salary_Acc_Pay_MonthList();
                        month.Salary_Pck_Acc_Id = ColumnId;
                        month.MonthId = monthId;
                        month.MonthName = "";
                        db.Salary_Acc_Pay_MonthList.Add(month);
                        db.SaveChanges();
                    }

                }
                catch (Exception ex)
                {
                    string errorMsg = ex.Message.ToString();
                    OperationStatus = -1;
                    msg = model.Name + " '" + model.CatagoryName + "' account type addition with salary package '" + pckName + "' was unsuccessful on " + now.ToString("MMM dd,yyyy hh:mm tt");
                }
                SaveAuditLog("Account Type Add ", msg, "HR", "Add Account Type", "Salary_Package_Account_Lists", ColumnId, model.CreatedBy, now, OperationStatus);
            }
            return Json("Success", JsonRequestBehavior.AllowGet);
        }


        //********************************* SALARY PACKAGE ACCOUNT UPDATE *******************************************

        public ActionResult SalaryPackageAccEdit(int HasBaseSalary, int PackageId, int AccountId)
        {
            SalaryAccAddModelView model = new SalaryAccAddModelView();
            Salary_Package_Account_Lists salary_Acc = db.Salary_Package_Account_Lists.Find(AccountId);
            model.Acc_Id = AccountId;
            model.PackageId = salary_Acc.PackageId;
            model.Catagory = salary_Acc.CatagoryId;
            model.AccountType = salary_Acc.AccountType;
            model.IsBaseSalary = salary_Acc.IsBaseSalary;
            model.Name = salary_Acc.Name;
            model.AccPayType = salary_Acc.AccPayType;
            model.Amount = salary_Acc.Amount;
            model.Percentage = salary_Acc.Percentage;
            model.IsAddition = salary_Acc.IsAddition;
            model.HolidayId = salary_Acc.HolidayId;

            ViewBag.Catagory = new SelectList(db.Salary_Package_Catagory, "SalaryCatagoryId", "Name", model.Catagory);

            if (HasBaseSalary == 1)
            {
                model.BaseSalary = db.Salary_Package_Account_Lists.Where(m => m.PackageId == PackageId && m.IsBaseSalary == true).Select(m => m.Amount).FirstOrDefault();
                if (model.IsBaseSalary)
                {
                    ViewBag.Catagory = new SelectList(db.Salary_Package_Catagory, "SalaryCatagoryId", "Name", model.Catagory);
                }
                else
                {
                    ViewBag.Catagory = new SelectList(db.Salary_Package_Catagory.Where(m => m.SalaryCatagoryId != 1), "SalaryCatagoryId", "Name", model.Catagory);
                }

                ViewBag.AccountType = new SelectList(db.Salary_Account_Type, "Id", "AccountName", model.AccountType);
            }
            else
            {
                ViewBag.AccountType = new SelectList(db.Salary_Account_Type.Where(m => m.Id != 2), "Id", "AccountName", model.AccountType);
                ViewBag.Catagory = new SelectList(db.Salary_Package_Catagory, "SalaryCatagoryId", "Name", model.Catagory);
            }
            if (model.AccPayType == 1)
            {
                List<SelectListItem> allMonthList = new List<SelectListItem>();
                var monthlist = db.Salary_Acc_Pay_MonthList.Where(m => m.Salary_Pck_Acc_Id == model.Acc_Id).ToList();
                for (int i = 0; i < monthlist.Count; i++)
                {
                    allMonthList.Add(new SelectListItem { Text = monthlist[i].MonthName, Value = monthlist[i].MonthId.ToString() + "-" + monthlist[i].MonthName });
                }
                ViewBag.monthId = allMonthList;
            }
            ViewBag.HolidayId = new SelectList(db.HolidayLists, "Id", "HolidayName", model.HolidayId);
            ViewBag.HasBaseSalary = HasBaseSalary;
            return PartialView(model);
        }
        public ActionResult EditSalaryPckAccountInfo(SalaryAccAddModelView model)
        {
            if (ModelState.IsValid)
            {
                string msg = "";
                int ColumnId = 0;
                int OperationStatus = 1;
                string pckName = "";

                Salary_Package_Account_Lists salary_Acc = db.Salary_Package_Account_Lists.Find(model.Acc_Id);
                salary_Acc.CatagoryId = model.Catagory;
                salary_Acc.AccountType = model.AccountType;
                salary_Acc.IsBaseSalary = model.IsBaseSalary;
                salary_Acc.Name = model.Name + " " + model.CatagoryName;
                salary_Acc.AccPayType = Convert.ToInt32(model.AccPayType);
                salary_Acc.Amount = model.Amount;
                salary_Acc.Percentage = model.Percentage;
                salary_Acc.IsAddition = model.IsAddition;
                salary_Acc.HolidayId = model.HolidayId;
                salary_Acc.UpdatedBy = model.CreatedBy;
                salary_Acc.UpdatedDate = now;
                db.Entry(salary_Acc).State = EntityState.Modified;
                try
                {
                    db.SaveChanges();

                    if (model.IsBaseSalary)
                    {
                        Salary_Package_List package = db.Salary_Package_List.Find(model.PackageId);
                        pckName = package.PackageName;
                        package.HasBaseSalary = true;
                        db.Entry(package).State = EntityState.Modified;
                        db.SaveChanges();
                        msg = model.Name + " " + model.CatagoryName + " account type successfully updated for salary package " + pckName;
                    }

                    ColumnId = model.Acc_Id;
                    if (Convert.ToInt32(model.AccPayType) == 1 && model.AllMonthId != "")
                    {
                        string[] allMonId = model.AllMonthId.Split(',');
                        if (db.Salary_Acc_Pay_MonthList.Where(m => m.Salary_Pck_Acc_Id == model.Acc_Id).Any())
                        {
                            var allMon = db.Salary_Acc_Pay_MonthList.Where(m => m.Salary_Pck_Acc_Id == model.Acc_Id).ToList();
                            foreach (var month in allMon)
                            {
                                db.Salary_Acc_Pay_MonthList.Remove(month);
                                db.SaveChanges();
                            }
                        }
                        for (int i = 0; i < allMonId.Length; i++)
                        {
                            Salary_Acc_Pay_MonthList month = new Salary_Acc_Pay_MonthList();
                            month.Salary_Pck_Acc_Id = model.Acc_Id;
                            month.MonthId = Convert.ToInt32(allMonId[i].Split('-')[0]);
                            month.MonthName = allMonId[i].Split('-')[1].ToString();
                            db.Salary_Acc_Pay_MonthList.Add(month);
                            db.SaveChanges();
                        }
                    }
                    //if (Convert.ToInt32(model.AccPayType) == 2 && model.HolidayId > 0)
                    //{
                    //    int monthId = 0;
                    //    var holiday = db.HolidayLists.Find(model.HolidayId);
                    //    if (holiday.IsMultipleDay == false)
                    //    {
                    //        monthId = Convert.ToInt32(holiday.MonthCount);
                    //    }
                    //    else
                    //    {
                    //        monthId = db.Multiple_HolidayLists.Where(m => m.ParentId == holiday.Id).Select(m => m.MonthCount).FirstOrDefault();
                    //    }
                    //    Salary_Acc_Pay_MonthList month = new Salary_Acc_Pay_MonthList();
                    //    month.Salary_Pck_Acc_Id = ColumnId;
                    //    month.MonthId = monthId;
                    //    month.MonthName = "";
                    //    db.Salary_Acc_Pay_MonthList.Add(month);
                    //    db.SaveChanges();
                    //}

                }
                catch (Exception ex)
                {
                    string errorMsg = ex.Message.ToString();
                    OperationStatus = -1;
                    msg = model.Name + " " + model.CatagoryName + " account type update with salary package " + pckName + " was unsuccessful.";
                }
                SaveAuditLog("Account Type Edit ", msg, "HR", "Salary Packge Config", "Salary_Package_Account_Lists", ColumnId, model.CreatedBy, now, OperationStatus);
            }
            return Json("Success", JsonRequestBehavior.AllowGet);
        }


        //********************************* SALARY PACKAGE DELETE *********************************************
        public JsonResult SalaryPackDelete(int? PackageId, long UserId)
        {
            if (PackageId == null)
            {
                return Json("Error", JsonRequestBehavior.AllowGet);
            }
            else
            {
                string msg = "";
                string name = "";
                //string pckname = "";
                int ColumnId = 0;
                int OperationStatus = 1;

                Salary_Package_List pckList = db.Salary_Package_List.Find(PackageId);
                if (db.Salary_Package_Account_Lists.Where(m => m.PackageId == PackageId).Any())
                {
                    var allAccountList = db.Salary_Package_Account_Lists.Where(m => m.PackageId == PackageId).ToList();
                    foreach (var account in allAccountList)
                    {
                        if (db.Salary_Acc_Pay_MonthList.Where(m => m.Salary_Pck_Acc_Id == account.Id).Any())
                        {
                            var allMonthList = db.Salary_Acc_Pay_MonthList.Where(m => m.Salary_Pck_Acc_Id == account.Id).ToList();
                            foreach (var month in allMonthList)
                            {
                                try
                                {
                                    db.Salary_Acc_Pay_MonthList.Remove(month);
                                    db.SaveChanges();
                                    OperationStatus = 1;

                                }
                                catch (Exception ex)
                                {
                                    string errorMsg = ex.Message.ToString();
                                    OperationStatus = -1;
                                }
                            }
                        }
                        name = account.Name;
                        ColumnId = account.Id;
                        try
                        {
                            msg = "'" + name + "' account type deleted successfully from '" + pckList.PackageName + "' salary package on " + now.ToString("MMM dd,yyyy hh:mm tt");
                            db.Salary_Package_Account_Lists.Remove(account);
                            db.SaveChanges();
                            OperationStatus = 1;

                        }
                        catch (Exception ex)
                        {
                            string errorMsg = ex.Message.ToString();
                            msg = "'" + name + "' account type deletion unsuccessful from '" + pckList.PackageName + "' salary package on " + now.ToString("MMM dd,yyyy hh:mm tt");
                            OperationStatus = -1;
                        }
                        SaveAuditLog("Delete Salary Package", msg, "HR", "Salary Package Lists", "Salary_Package_Account_List", ColumnId, UserId, now, OperationStatus);
                    }
                }

                name = pckList.PackageName;
                ColumnId = pckList.PackageId;
                try
                {
                    msg = "'" + name + "' salary package deleted successfully on " + now.ToString("MMM dd,yyyy hh:mm tt");
                    db.Salary_Package_List.Remove(pckList);
                    db.SaveChanges();
                    OperationStatus = 1;

                }
                catch (Exception ex)
                {
                    string errorMsg = ex.Message.ToString();
                    msg = "'" + name + "' salary package delete was unsuccessful on " + now.ToString("MMM dd,yyyy hh:mm tt");
                    OperationStatus = -1;
                }
                SaveAuditLog("Delete Salary Package", msg, "HR", "Salary Package Lists", "Salary_Package_List", ColumnId, UserId, now, OperationStatus);
                return Json("Success", JsonRequestBehavior.AllowGet);
            }
        }

        //********************************* SALARY PACKAGE ACCOUNT DELETE *********************************************
        public JsonResult SalaryPackAccDelete(int Id, long UserId)
        {
            string msg = "";
            string name = "";
            string pckname = "";
            int ColumnId = 0;
            int OperationStatus = 1;
            int packageId;

            Salary_Package_Account_Lists account = db.Salary_Package_Account_Lists.Find(Id);
            Salary_Package_List list = db.Salary_Package_List.Find(account.PackageId);
            if (account != null)
            {
                name = account.Name;
                pckname = list.PackageName;
                packageId = account.PackageId;
                if (account.IsBaseSalary)
                {
                    list.HasBaseSalary = false;
                    db.Entry(list).State = EntityState.Modified;
                    db.SaveChanges();
                }
                if (account.AccPayType == 1)
                {
                    var monthList = db.Salary_Acc_Pay_MonthList.Where(m => m.Salary_Pck_Acc_Id == account.Id).ToList();
                    foreach (var month in monthList)
                    {
                        db.Salary_Acc_Pay_MonthList.Remove(month);
                        db.SaveChanges();
                    }
                }
                ColumnId = account.Id;
                try
                {
                    msg = name + " account type deleted successfully from " + pckname + " salary package on " + now.ToString("MMM dd,yyyy hh:mm tt");
                    db.Salary_Package_Account_Lists.Remove(account);
                    db.SaveChanges();
                    OperationStatus = 1;

                }
                catch (Exception ex)
                {
                    string errorMsg = ex.Message.ToString();
                    msg = name + " account type deleftion unsuccessful from " + pckname + " salary package on " + now.ToString("MMM dd,yyyy hh:mm tt");
                    OperationStatus = -1;
                }
                SaveAuditLog("Delete Account Type", msg, "HR", "Salary Package Details", "Salary_Package_Account_List", ColumnId, UserId, now, OperationStatus);
                return Json(db.Salary_Package_Account_Lists.Where(m => m.PackageId == packageId).Any(), JsonRequestBehavior.AllowGet);
            }
            return Json("Error", JsonRequestBehavior.AllowGet);
        }

        [EncryptedActionParameter]
        public ActionResult AssignSalaryPackage(long? userId, int? PackageId)
        {
            bool Singleuser = false;
            bool changePck = false;
            if (userId != null)
            {
                Singleuser = true;
                if (PackageId != null)
                {
                    changePck = true;
                }
            }
            ViewBag.userId = userId;

            ViewBag.Singleuser = Singleuser;
            ViewBag.changePck = changePck;
            ViewBag.PackageId = PackageId;
            return View();
        }
        public ActionResult SeletedUserInfoForSalary(long? userId)
        {
            if (userId != null)
            {
                View_UserLists model = db.View_UserLists.Where(m => m.UserId == userId).FirstOrDefault();
                return PartialView(model);
            }
            return PartialView();
        }
        public JsonResult GetUser(string prefix)
        {
            if (prefix == "")
            {
                return Json("", JsonRequestBehavior.AllowGet);
            }
            else
            {
                var userList = (from u in db.View_UserLists
                                where (u.UserName.ToLower().Contains(prefix.ToLower()))
                                select new UserInformationModelView
                                {
                                    UserName = u.UserName,
                                    Designation = u.DesignationName,
                                    UserId = u.UserId,
                                    UserType = u.UserType,
                                    Picture = u.Picture
                                }).ToList();

                return Json(userList, JsonRequestBehavior.AllowGet);
            }
        }
        public JsonResult GetUserList()
        {
            var userList = (from u in db.View_UserLists
                            where u.AssginSalary == false
                            select new UserInformationModelView
                            {
                                UserName = u.UserName,
                                EmpId = u.EmpId,
                                Designation = u.DesignationName,
                                UserId = u.UserId,
                                UserType = u.UserType,
                                Picture = u.Picture,
                                UserId_Type = u.UserId + "-" + u.UserType
                            }).OrderBy(m => m.Designation).ToList();

            return Json(userList, JsonRequestBehavior.AllowGet);
        }

        public ActionResult _AllPackageList()
        {
            return PartialView();
        }
        public JsonResult AllPackageList()
        {
            var allPackage = (from p in db.Salary_Package_List
                              select new
                              {
                                  id = p.PackageId + "-p",
                                  text = p.PackageName,
                                  PackageId = p.PackageId,
                                  hasChildren = db.Salary_Package_Account_Lists.Where(m => m.PackageId == p.PackageId).Any()
                              }).ToList();
            return Json(allPackage, JsonRequestBehavior.AllowGet);
        }
        public JsonResult GetAllAccType(int packageId)
        {
            var allAccLists = (from a in db.Salary_Package_Account_Lists
                               where a.PackageId == packageId
                               select new
                               {
                                   id = a.Id + "-a",
                                   text = a.Name,
                                   AccountType = a.AccountType,
                                   AccPayType = a.AccPayType,
                                   Amount = a.Amount,
                                   Percentage = a.Percentage,
                                   IsBaseSalary = a.IsBaseSalary,
                                   IsAddition = a.IsAddition
                               }).ToList();
            return Json(allAccLists, JsonRequestBehavior.AllowGet);
        }
        public JsonResult GetAllSalaryPackage()
        {
            var allPackage = (from p in db.Salary_Package_List
                              select new
                              {
                                  id = p.PackageId + "-p",
                                  text = p.PackageName,
                                  items = (from a in db.Salary_Package_Account_Lists
                                           where a.PackageId == p.PackageId
                                           select new
                                           {
                                               id = a.Id + "-a",
                                               text = a.Name,
                                               amount = a.Amount,
                                               percentage = a.Percentage,
                                               isBaseSalary = a.IsBaseSalary,
                                               isAddition = a.IsAddition
                                           }).ToList()
                              }).ToList();
            return Json(allPackage, JsonRequestBehavior.AllowGet);
        }
        public ActionResult _SelectedPckDetailsToAssign(int? PackageId)
        {
            if (PackageId == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            else
            {
                SalayPackageConfig model = new SalayPackageConfig();
                Salary_Package_List salaryPck = db.Salary_Package_List.Where(m => m.PackageId == PackageId).FirstOrDefault();
                model.PackageId = Convert.ToInt32(PackageId);
                model.PackageName = salaryPck.PackageName;
                model.HasBaseSalary = salaryPck.HasBaseSalary;
                ViewBag.HasAddedAccount = db.Salary_Package_Account_Lists.Where(m => m.PackageId == PackageId).Any();

                ViewBag.baseAmount = 0.0;
                if (model.HasBaseSalary)
                {
                    ViewBag.baseAmount = Convert.ToDouble(db.Salary_Package_Account_Lists.Where(m => m.PackageId == PackageId && m.IsBaseSalary == true).Select(m => m.Amount).FirstOrDefault());
                }
                ViewBag.allAccName = (from m in db.Salary_Package_Account_Lists
                                      where m.PackageId == PackageId
                                      select new SalaryAccAddModelView
                                      {
                                          Id = m.Id,
                                          PackageId = m.PackageId,
                                          Name = m.Name,
                                          IsBaseSalary = m.IsBaseSalary,
                                          AccountType = m.AccountType,
                                          AccPayType = m.AccPayType,
                                          Amount = m.Amount,
                                          Percentage = m.Percentage,
                                          IsAddition = m.IsAddition
                                      }).ToList();

                return PartialView(model);
            }
        }
        public JsonResult SavePackageIdWithUser(string AllUserId, int PackageId, string pckName, long CreatedBy)
        {
            if (AllUserId != "")
            {
                string msg = "";
                int ColumnId = 0;
                int OperationStatus = 1;
                string tblName = "";

                string[] userId = AllUserId.Split(',');
                for (int i = 0; i < userId.Length; i++)
                {
                    int UserId = Convert.ToInt32(userId[i]);
                    UserInformation user = db.UserInformations.Find(UserId);
                    user.AssginSalary = true;
                    user.SalaryPackageId = PackageId;
                    db.Entry(user).State = EntityState.Modified;
                    tblName = "UserInformation";
                    msg = pckName + " salary package successfully assigned with '" + user.FirstName + " " + user.MiddleName + " " + user.LastName + "' on " + now.ToString("MMM dd,yyyy hh:mm tt");
                    try
                    {
                        db.SaveChanges();
                        ColumnId = UserId;
                        OperationStatus = 1;
                    }
                    catch (Exception ex)
                    {
                        string strErr = ex.ToString();
                        ColumnId = 0;
                        OperationStatus = -1;
                        tblName = "";
                        msg = pckName + " salary package assign with user was unsuccessful on " + now.ToString("MMM dd,yyyy hh:mm tt");
                    }
                    SaveAuditLog("Salary Package Add ", msg, "HR", "Assign Salary Package", tblName, ColumnId, CreatedBy, now, OperationStatus);
                }
                return Json("Success", JsonRequestBehavior.AllowGet);
            }
            return Json("Error", JsonRequestBehavior.AllowGet);
        }

        //********************************* CHECKED SALARY PACKAGE ASSIGN WITH USER OR NOT ******************************

        public JsonResult ChkSalaryPackageInUsed(int? PackageId)
        {
            if (PackageId == null)
            {
                return Json("Error", JsonRequestBehavior.AllowGet);
            }
            else
            {
                if (db.View_UserLists.Where(m => m.AssginSalary == true && m.SalaryPackageId == PackageId).Any())
                {
                    return Json(db.View_UserLists.Where(m => m.AssginSalary == true && m.SalaryPackageId == PackageId).Count(), JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(false, JsonRequestBehavior.AllowGet);
                }
            }
        }
        public ActionResult _AllUserListForSpeficSalaryPack(int PackageId)
        {
            return PartialView(db.View_UserLists.Where(m => m.SalaryPackageId == PackageId).ToList());
        }

        //*********************************  SALARY PACKAGE DUPLICATE  *****************************************************
        public ActionResult _SalaryPckDupName(int PackageId)
        {
            SalaryPackageModelView model = new SalaryPackageModelView();
            model.PackageId = PackageId;
            return PartialView(model);
        }
        public JsonResult SaveSalaryPckDup(SalaryPackageModelView model)
        {
            if (ModelState.IsValid)
            {
                string msg = "";
                int ColumnId = 0;
                int OperationStatus = 1;

                Salary_Package_List dupPck = db.Salary_Package_List.Find(model.PackageId);
                Salary_Package_List salaryPck = new Salary_Package_List();

                salaryPck.PackageName = model.PackageName;
                salaryPck.Amount = model.Amount;
                salaryPck.HasBaseSalary = dupPck.HasBaseSalary;
                salaryPck.CreatedBy = model.CreatedBy;
                salaryPck.CreatedDate = now;
                salaryPck.PckStatus = 1;
                db.Salary_Package_List.Add(salaryPck);
                try
                {
                    db.SaveChanges();
                    msg = dupPck.PackageName + " salary package duplicated on " + now.ToString("MMM dd,yyyy hh:mm tt");
                    ColumnId = db.Salary_Package_List.Max(m => m.PackageId);

                    if (db.Salary_Package_Account_Lists.Where(m => m.PackageId == model.PackageId).Any())
                    {
                        var allAccountList = db.Salary_Package_Account_Lists.Where(m => m.PackageId == model.PackageId).ToList();
                        foreach (var acc in allAccountList)
                        {
                            Salary_Package_Account_Lists salary_Acc = new Salary_Package_Account_Lists();
                            salary_Acc.PackageId = ColumnId;
                            salary_Acc.CatagoryId = acc.CatagoryId;
                            salary_Acc.AccountType = acc.AccountType;
                            salary_Acc.IsBaseSalary = acc.IsBaseSalary;
                            salary_Acc.Name = acc.Name;
                            salary_Acc.AccPayType = acc.AccPayType;
                            salary_Acc.Amount = acc.Amount;
                            salary_Acc.Percentage = acc.Percentage;
                            salary_Acc.IsAddition = acc.IsAddition;
                            salary_Acc.HolidayId = acc.HolidayId;
                            salary_Acc.CreatedBy = acc.CreatedBy;
                            salary_Acc.CreatedDate = now;
                            db.Salary_Package_Account_Lists.Add(salary_Acc);
                            db.SaveChanges();
                            int maxAcc_Id = db.Salary_Package_Account_Lists.Max(m => m.Id);
                            if (db.Salary_Acc_Pay_MonthList.Where(m => m.Salary_Pck_Acc_Id == acc.Id).Any())
                            {
                                var allMonth = db.Salary_Acc_Pay_MonthList.Where(m => m.Salary_Pck_Acc_Id == acc.Id).ToList();
                                foreach (var mon in allMonth)
                                {
                                    Salary_Acc_Pay_MonthList month = new Salary_Acc_Pay_MonthList();
                                    month.Salary_Pck_Acc_Id = maxAcc_Id;
                                    month.MonthId = mon.MonthId;
                                    month.MonthName = mon.MonthName;
                                    db.Salary_Acc_Pay_MonthList.Add(month);
                                    db.SaveChanges();
                                }
                            }
                        }
                    }

                }
                catch (Exception ex)
                {
                    string errorMsg = ex.Message.ToString();
                    msg = dupPck.PackageName + " salary package duplicate unsuccessful on " + now.ToString("MMM dd,yyyy hh:mm tt");
                    OperationStatus = -1;
                }
                SaveAuditLog("Duplicate Salary Packge ", msg, "HR", "Salary Package Config", "Salary_Package_List", ColumnId, model.CreatedBy, now, OperationStatus);
                return Json("Success", JsonRequestBehavior.AllowGet);
            }
            return Json("Error", JsonRequestBehavior.AllowGet);
        }
        #endregion




        public JsonResult CreateUserSalary(long UserId, int UserType, int pckId)
        {
            decimal baseSalary = 0;
            decimal total = 0;
            baseSalary = Convert.ToDecimal(db.Salary_Package_Account_Lists.Where(m => m.PackageId == pckId && m.IsBaseSalary).Select(m => m.Amount).FirstOrDefault());
            var list = db.Salary_Package_Account_Lists.Where(m => m.PackageId == pckId).ToList();
            EmpSalaryPaymentDetail empSalary = new EmpSalaryPaymentDetail();
            empSalary.UserId = UserId;
            empSalary.UserType = UserType;
            empSalary.Year = now.Year;
            empSalary.MonthId = now.Month;
            empSalary.Month = now.ToString("MMMM");
            empSalary.TotalAmount = 0;
            empSalary.Status = 0;
            empSalary.GeneratedDate = now;
            db.EmpSalaryPaymentDetails.Add(empSalary);
            db.SaveChanges();
            long PaymentId = db.EmpSalaryPaymentDetails.Where(m => m.UserId == UserId && m.UserType == UserType).Max(m => m.Id);

            foreach (var li in list)
            {
                decimal amnt = 0;
                if (li.AccPayType == 3)
                {
                    if (li.Percentage == null)
                    {
                        amnt = Convert.ToDecimal(li.Amount);
                    }
                    else
                    {
                        decimal percentage = Convert.ToDecimal(li.Percentage);
                        amnt = baseSalary * (percentage / 100);
                    }
                    if (li.IsAddition)
                    {
                        total = total + amnt;
                    }
                    else
                    {
                        total = total - amnt;
                    }
                }
                else
                {
                    var allmonthId = db.Salary_Acc_Pay_MonthList.Where(m => m.Salary_Pck_Acc_Id == li.Id).Select(m => m.MonthId).ToList();
                    foreach (var month in allmonthId)
                    {
                        if (month == now.Month)
                        {
                            if (li.Percentage == null)
                            {
                                amnt = Convert.ToDecimal(li.Amount);
                            }
                            else
                            {
                                decimal percentage = Convert.ToDecimal(li.Percentage);
                                amnt = baseSalary * (percentage / 100);
                            }
                            if (li.IsAddition)
                            {
                                total = total + amnt;
                            }
                            else
                            {
                                total = total - amnt;
                            }
                        }
                    }
                }
                SalaryPaymentPckDetail salaryDetails = new SalaryPaymentPckDetail();
                salaryDetails.PaymentId = Convert.ToInt32(PaymentId);
                salaryDetails.IsDeduct = li.IsAddition ? false : true;
                salaryDetails.IsBasic = li.IsBaseSalary;
                salaryDetails.Acc_Name = li.Name;
                salaryDetails.Amount = amnt;
                db.SalaryPaymentPckDetails.Add(salaryDetails);
                db.SaveChanges();
            }

            EmpSalaryPaymentDetail emp = db.EmpSalaryPaymentDetails.Find(PaymentId);
            emp.TotalAmount = total;
            db.Entry(emp).State = EntityState.Modified;
            db.SaveChanges();
            return Json("Success", JsonRequestBehavior.AllowGet);
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
            db.AuditInformations.Add(audit);
            db.SaveChanges();
        }
        #endregion

    }
}