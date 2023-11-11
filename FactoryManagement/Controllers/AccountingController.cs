
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
using FactoryManagement.ModelView.Accounting;
using FactoryManagement.Infrastructure.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using System.Configuration;
using FactoryManagement.Filters;

namespace FactoryManagement.Controllers
{
    [Authorize]
    [DisplayName("Accounting")]
    public class AccountingController : Controller
    {
        #region Private Properties
        private FactoryManagementEntities db = new FactoryManagementEntities();
        private FactoryManagementEntities dbAud = new FactoryManagementEntities();

        static private int offset = Convert.ToInt32(ConfigurationManager.AppSettings["localTime"]);
        DateTime now = DateTime.UtcNow.AddMinutes(offset);

        int ShowBankPerPage = 20;
        int ShowAccountPerPage = 20;
        int ShowBranchPerPage = 20;
        int ShowSalaryPayBankPerPage = 20;
        int ShowLoanHistoryAccPerPage = 20;
        int ShowLoanHistoryBankPerPage = 20;
        const int InvoiTranshow = 20;
        const int VoucherTranshowBank = 20;
        #endregion

        #region Account Group Create,Update,Delete
        [ClaimsAction(ClaimsActions.List)]
        [Display(Name = "Add New Account Group")]

        public ActionResult AccountGroup()
        {
            return View();
        }
        public ActionResult _ShowAllAcc_Group(int? AccId)
        {
            if (AccId != null)
            {
                var searchList = (List<AccountName>)Session["AllAccountGroupSearch"];
                var desigBysearch = (from d in searchList
                                     where d.AccId == AccId
                                     select new AccountName
                                     {
                                         AccId = d.AccId,
                                         AccountName1 = d.AccountName1,
                                         Acc_CashType = d.Acc_CashType,
                                         Acc_Type = d.Acc_Type,
                                         SiteId = d.SiteId,
                                         UnitId = d.UnitId,
                                         StoreId = d.StoreId,
                                         WareId = d.WareId,
                                         Balance = d.Balance,
                                         TotalCreditLimit = d.TotalCreditLimit,
                                         TransactionHigestAmount = d.TransactionHigestAmount,
                                         NoOfTranPerMonth = d.NoOfTranPerMonth,
                                         Description = d.Description,
                                         CreatedBy = d.CreatedBy,
                                         CreatedDate = d.CreatedDate,
                                         UpdatedBy = d.UpdatedBy,
                                         UpdatedDate = d.UpdatedDate
                                     }).ToList();
                return PartialView(desigBysearch);
            }
            else
            {
                var list = db.AccountNames.ToList();
                Session["AllAccountGroup"] = list;
                return PartialView(list);
            }
        }
        public JsonResult GetAllAccName(string prefix)
        {
            if (prefix == "")
            {
                return Json("", JsonRequestBehavior.AllowGet);
            }
            else
            {
                var searchList = (List<AccountName>)Session["AllAccountGroup"];
                var title = (from d in searchList
                             where (d.AccountName1.ToLower().Contains(prefix.ToLower()))
                             select new AccountName
                             {
                                 AccId = d.AccId,
                                 AccountName1 = d.AccountName1,
                                 Acc_CashType = d.Acc_CashType,
                                 Acc_Type = d.Acc_Type,
                                 SiteId = d.SiteId,
                                 UnitId = d.UnitId,
                                 StoreId = d.StoreId,
                                 WareId = d.WareId,
                                 Balance = d.Balance,
                                 TotalCreditLimit = d.TotalCreditLimit,
                                 TransactionHigestAmount = d.TransactionHigestAmount,
                                 NoOfTranPerMonth = d.NoOfTranPerMonth,
                                 Description = d.Description,
                                 CreatedBy = d.CreatedBy,
                                 CreatedDate = d.CreatedDate,
                                 UpdatedBy = d.UpdatedBy,
                                 UpdatedDate = d.UpdatedDate
                             }).ToList();
                Session["AllAccountGroupSearch"] = title;
                return Json(title, JsonRequestBehavior.AllowGet);
            }
        }
        public JsonResult DeleteAccountName(int? id, string name, long userId)
        {
            if (id > 0)
            {
                //if (db.AllSalaryPayLists.Where(m => m.InernalAccId == id).Any() || db.AllUserLoanLists.Where(m => m.InernalAccId == id).Any())
                //{
                //    return Json("You can not delete this account name because it is already in use.", JsonRequestBehavior.AllowGet);
                //}
                //else
                //{
                int status = -1; int ColumnId = 0;
                string msg = "";
                AccountName account = db.AccountNames.Find(id);
                if (account != null)
                {
                    db.AccountNames.Remove(account);
                    try
                    {
                        db.SaveChanges();
                        status = 1;
                        msg = "Account name '" + name + "' has been successfully deleted.";
                        ColumnId = Convert.ToInt32(id);
                    }
                    catch (Exception)
                    {
                        status = -1;
                        msg = "Account name '" + name + "' has been successfully deleted.";
                    }
                    SaveAuditLog("Account Name Delete ", msg, "Accounting", "All Account Name", "AccountName", ColumnId, userId, now, status);
                    if (status == 1)
                    {
                        return Json("Success", JsonRequestBehavior.AllowGet);
                    }
                    //}
                }
            }
            return Json("Error", JsonRequestBehavior.AllowGet);
        }


        //******************************** Add New Account Group *******************************************
        [ClaimsAction(ClaimsActions.Create)]
        public PartialViewResult AddAccountGroup(int? Acc_Group_Id)
        {
            AccountGroupModelView model = new AccountGroupModelView();
            if (Acc_Group_Id != null)
            {
                AccountName acc_grp = db.AccountNames.Find(Acc_Group_Id);
                model.AccId = acc_grp.AccId;
                model.AccountName = acc_grp.AccountName1;
                model.Description = acc_grp.Description;
                model.Balance = acc_grp.Balance;
                model.Acc_Type = acc_grp.Acc_Type;
                model.Acc_CashType = acc_grp.Acc_CashType;
            }
            ViewBag.Acc_Type = new SelectList(db.AccountTypes, "Acc_Type_Id", "Name", model.Acc_Type);
            List<SelectListItem> allUserList = new SelectList(db.View_UserLists.Where(m => m.Status == 1).OrderBy(o => o.UserName), "UserId", "UserName").ToList();
            ViewBag.allUserList = allUserList;
            return PartialView(model);
        }
        public ActionResult _AccTypeSalary()
        {
            var list = (from m in db.SiteLists
                        where m.Status == 1
                        select new
                        {
                            SiteId = m.Id,
                            SiteName = m.SiteName,
                            IsComposite = m.IsComposite
                        }).ToList();
            ViewBag.list = list;
            return PartialView();
        }
        public JsonResult AllSiteList()
        {
            var list = (from m in db.SiteLists
                        where m.Status == 1
                        select new
                        {
                            SiteId = m.Id,
                            SiteName = m.SiteName,
                            IsComposite = m.IsComposite
                        }).ToList();
            return Json(list, JsonRequestBehavior.AllowGet);
        }
        public JsonResult AllUnitList(int siteId)
        {
            var list = (from m in db.Site_Unit_Lists
                        where m.SiteId == siteId
                        select new
                        {
                            UnitId = m.UnitId,
                            UnitName = m.UnitName
                        }).OrderBy(o => o.UnitName).ToList();
            return Json(list, JsonRequestBehavior.AllowGet);
        }
        public JsonResult AllUserList(int SiteId)
        {
            if (SiteId == 0)
            {
                var list = (from m in db.View_UserLists
                            where m.Status == 1
                            select new
                            {
                                UserId = m.UserId,
                                Username = m.UserName + "( Employee Id:" + m.EmpId + " )",
                                UserType = m.UserTypeName,
                                UserEmpId = m.EmpId,
                                Picture = m.Picture
                            }).OrderBy(o => o.Username).ToList();
                return Json(list, JsonRequestBehavior.AllowGet);
            }
            else
            {
                var list = (from m in db.View_UserLists
                            where m.SiteId == SiteId && m.Status == 1
                            select new
                            {
                                UserId = m.UserId,
                                Username = m.UserName,
                                UserType = m.UserTypeName,
                                UserEmpId = m.EmpId,
                                Picture = m.Picture
                            }).ToList();
                return Json(list, JsonRequestBehavior.AllowGet);
            }
        }
        public JsonResult AllStoreWareDept()
        {
            var sitelist = (from m in db.DepartmentLists
                            where m.Status == 1
                            select new AllDepList
                            {
                                Id = m.DeptId,
                                IdWithChar = m.DeptId + "-d",
                                Name = "Dept. :" + m.DeptName
                            }).ToList();
            var storeList = (from m in db.StoreLists
                             select new
                             {
                                 Id = m.Id,
                                 IdWithChar = m.Id + "-st",
                                 Name = "Store :" + m.StoreName
                             }).ToList();

            var wareList = (from m in db.WarehouseLists
                            select new
                            {
                                Id = m.Id,
                                IdWithChar = m.Id + "-w",
                                Name = "Warehouse :" + m.WarehouseName
                            }).ToList();
            foreach (var store in storeList)
            {
                sitelist.Add(new AllDepList { Id = store.Id, IdWithChar = store.IdWithChar, Name = store.Name });
            }
            foreach (var ware in wareList)
            {
                sitelist.Add(new AllDepList { Id = ware.Id, IdWithChar = ware.IdWithChar, Name = ware.Name });
            }
            return Json(sitelist, JsonRequestBehavior.AllowGet);
        }
        public JsonResult AllSiteListRevenue()
        {
            var sitelist = (from m in db.SiteLists
                            where m.Status == 1
                            select new AllDepList
                            {
                                Id = m.Id,
                                IdWithChar = m.Id + "-s",
                                Name = m.SiteName
                            }).ToList();
            var unitList = (from m in db.Site_Unit_Lists
                            join s in db.SiteLists on m.SiteId equals s.Id
                            select new
                            {
                                Id = m.UnitId,
                                IdWithChar = m.UnitId + "-u",
                                Name = m.UnitName + " ( " + s.SiteName + " )"
                            }).ToList();
            var deptList = (from m in db.DepartmentLists
                            join d in db.SiteLists on m.SiteId equals d.Id
                            where m.Status == 1 && m.SiteId > 0
                            select new AllDepList
                            {
                                Id = m.DeptId,
                                IdWithChar = m.DeptId + "-d",
                                Name = m.DeptName + " ( Site name : " + d.SiteName + " )"
                            }).ToList();
            var unitDept = (from m in db.DepartmentLists
                            join d in db.Site_Unit_Lists on m.UnitId equals d.UnitId
                            where m.Status == 1 && m.UnitId > 0
                            select new AllDepList
                            {
                                Id = m.DeptId,
                                IdWithChar = m.DeptId + "-d",
                                Name = m.DeptName + " ( Unit name : " + d.UnitName + " )"
                            }).ToList();
            var allLine = (from m in db.LineInfoes
                           join d in db.DepartmentLists
                           on m.DeptId equals d.DeptId
                           select new AllDepList
                           {
                               Id = m.LineId,
                               IdWithChar = m.LineId + "-l",
                               Name = m.LineAcronym + " ( Line name : " + m.LineAcronym + " ; Dept. : " + d.DeptName + ")"
                           }).ToList();
            var allSoteId = (from m in db.StoreLists
                             select new AllDepList
                             {
                                 Id = m.Id,
                                 IdWithChar = m.Id + "-st",
                                 Name = m.StoreName + " ( Store name : " + m.StoreName + " )"
                             }).ToList();
            var allwareId = (from m in db.WarehouseLists
                             select new AllDepList
                             {
                                 Id = m.Id,
                                 IdWithChar = m.Id + "-w",
                                 Name = m.WarehouseName + " ( Warehouse name : " + m.WarehouseName + " )"
                             }).ToList();

            foreach (var ware in unitList)
            {
                sitelist.Add(new AllDepList { Id = ware.Id, IdWithChar = ware.IdWithChar, Name = ware.Name });
            }
            foreach (var ware in unitDept)
            {
                sitelist.Add(new AllDepList { Id = ware.Id, IdWithChar = ware.IdWithChar, Name = ware.Name });
            }
            foreach (var store in deptList)
            {
                sitelist.Add(new AllDepList { Id = store.Id, IdWithChar = store.IdWithChar, Name = store.Name });
            }
            foreach (var line in allLine)
            {
                sitelist.Add(new AllDepList { Id = line.Id, IdWithChar = line.IdWithChar, Name = line.Name });
            }
            foreach (var store in allSoteId)
            {
                sitelist.Add(new AllDepList { Id = store.Id, IdWithChar = store.IdWithChar, Name = store.Name });
            }
            foreach (var ware in allwareId)
            {
                sitelist.Add(new AllDepList { Id = ware.Id, IdWithChar = ware.IdWithChar, Name = ware.Name });
            }
            return Json(sitelist, JsonRequestBehavior.AllowGet);
        }
        public JsonResult SaveAccGroup(AccountGroupModelView model, string AllUserId, string AllLocationId)
        {
            if (ModelState.IsValid)
            {
                string msg = "";
                string opname = "";
                int ColumnId = 0;
                int OperationStatus = 1;

                AccountName acc_grp = new AccountName();
                acc_grp.AccountName1 = model.AccountName;
                acc_grp.Acc_CashType = model.Acc_CashType;
                acc_grp.Balance = model.Balance;
                if (acc_grp.Acc_CashType == 1 || acc_grp.Acc_CashType == 3)
                {
                    acc_grp.TotalCreditLimit = model.TotalCreditLimit;
                    acc_grp.TransactionHigestAmount = model.TransactionHigestAmount;
                    acc_grp.NoOfTranPerMonth = model.NoOfTranPerMonth;

                    acc_grp.Acc_Type = Convert.ToInt32(model.Acc_Type);
                    acc_grp.SiteId = model.SiteId;
                    if (model.Acc_Type == 1)
                    {
                        acc_grp.UnitId = model.UnitId;
                    }
                }
                else
                {
                    acc_grp.SiteId = model.SiteId;
                    acc_grp.UnitId = model.UnitId;
                    acc_grp.DeptId = model.DeptId;
                }
                acc_grp.Description = model.Description;
                acc_grp.CreatedBy = model.CreatedBy;
                acc_grp.CreatedDate = now;
                db.AccountNames.Add(acc_grp);
                msg = "New account '" + model.AccountName + "' has been successfully created on " + now.ToString("MMM dd , yyyy hh:mm tt") + " .";
                opname = "Add Account Group";
                try
                {
                    db.SaveChanges();
                    ColumnId = db.AccountNames.Where(m => m.CreatedBy == model.CreatedBy).Max(m => m.AccId);
                    if (AllUserId != null)
                    {
                        string[] allUser = AllUserId.Split(',');
                        for (int j = 0; j < allUser.Length; j++)
                        {
                            AccNameAssignedWithUser accname = new AccNameAssignedWithUser();
                            accname.UserId = Convert.ToInt32(allUser[j]);
                            accname.AccId = ColumnId;
                            accname.UserType = 1;
                            accname.AssignedDate = now;
                            accname.AssignedBy = model.CreatedBy;
                            db.AccNameAssignedWithUsers.Add(accname);
                            db.SaveChanges();
                        }
                    }
                }
                catch (Exception)
                {
                    msg = "New account " + model.AccountName + " created was unsuccessful on " + now.ToString("MMM dd , yyyy hh:mm tt") + " .";
                    OperationStatus = -1;
                }
                SaveAuditLog(opname, msg, "Finance", "All Account Name", "AccountName", ColumnId, model.CreatedBy, now, OperationStatus);

                //if (model.Acc_CashType == 1 && model.Acc_Type > 2 && AllLocationId != "")
                //{
                //    string[] AllSotreId = AllLocationId.Split(',');
                //    for (int i = 0; i < AllSotreId.Length; i++)
                //    {
                //        string idWithChar = AllSotreId[i].ToString();
                //        if (idWithChar.Split('-')[1] == "s")
                //        {
                //            acc_grp.UnitId = null; acc_grp.DeptId = null; acc_grp.StoreId = null; acc_grp.WareId = null;
                //            acc_grp.SiteId = Convert.ToInt32(idWithChar.Split('-')[0]);
                //        }
                //        else if (idWithChar.Split('-')[1] == "u")
                //        {
                //            acc_grp.SiteId = null; acc_grp.DeptId = null; acc_grp.StoreId = null; acc_grp.WareId = null;
                //            acc_grp.UnitId = Convert.ToInt32(idWithChar.Split('-')[0]);
                //        }
                //        else if (idWithChar.Split('-')[1] == "d")
                //        {
                //            acc_grp.SiteId = null; acc_grp.UnitId = null; acc_grp.StoreId = null; acc_grp.WareId = null;
                //            acc_grp.DeptId = Convert.ToInt32(idWithChar.Split('-')[0]);
                //        }
                //        else if (idWithChar.Split('-')[1] == "st")
                //        {
                //            acc_grp.SiteId = null; acc_grp.UnitId = null; acc_grp.DeptId = null; acc_grp.WareId = null;
                //            acc_grp.StoreId = Convert.ToInt32(idWithChar.Split('-')[0]);
                //        } 
                //        else
                //        {
                //            acc_grp.SiteId = null; acc_grp.UnitId = null; acc_grp.DeptId = null; acc_grp.StoreId = null; 
                //            acc_grp.WareId = Convert.ToInt32(idWithChar.Split('-')[0]);
                //        }
                //        db.AccountNames.Add(acc_grp);
                //        msg = "New account  '" + model.AccountName + "' has been successfully created on " + now.ToString("MMM dd , yyyy hh:mm tt") + " .";
                //        opname = "Add New Account";
                //        try
                //        {
                //            db.SaveChanges();
                //            ColumnId = db.AccountNames.Where(m => m.CreatedBy == model.CreatedBy).Max(m => m.AccId);
                //            SaveAuditLog(opname, msg, "Finance", "All Account Name", "AccountName", ColumnId, model.CreatedBy, now, OperationStatus);

                //            if (AllUserId != null)
                //            {
                //                string[] allUser = AllUserId.Split(',');
                //                for (int j = 0; j < allUser.Length; j++)
                //                {
                //                    AccNameAssignedWithUser accname = new AccNameAssignedWithUser();
                //                    accname.UserId = Convert.ToInt32(allUser[j]);
                //                    accname.AccId = ColumnId;
                //                    accname.UserType = 1;
                //                    accname.AssignedDate = now;
                //                    accname.AssignedBy = model.CreatedBy;
                //                    db.AccNameAssignedWithUsers.Add(accname);
                //                    db.SaveChanges();
                //                }
                //            }

                //        }
                //        catch (Exception)
                //        {
                //            msg = "New account group " + model.AccountName + " create was unsuccessful on " + now.ToString("MMM dd , yyyy hh:mm tt") + " .";
                //            OperationStatus = -1;
                //            SaveAuditLog(opname, msg, "Finance", "All Account Name", "AccountName", 0, model.CreatedBy, now, OperationStatus);
                //        }
                //    }
                //}
                //else
                //{
                //    db.AccountNames.Add(acc_grp);
                //    msg = "New account group '" + model.AccountName + "' has been successfully created on " + now.ToString("MMM dd , yyyy hh:mm tt") + " .";
                //    opname = "Add Account Group";
                //    try
                //    {
                //        db.SaveChanges();
                //        ColumnId = db.AccountNames.Where(m => m.CreatedBy == model.CreatedBy).Max(m => m.AccId);

                //        if (AllUserId != null)
                //        {
                //            string[] allUser = AllUserId.Split(',');
                //            for (int j = 0; j < allUser.Length; j++)
                //            {
                //                AccNameAssignedWithUser accname = new AccNameAssignedWithUser();
                //                accname.UserId = Convert.ToInt32(allUser[j]);
                //                accname.AccId = ColumnId;
                //                accname.UserType = 1;
                //                accname.AssignedDate = now;
                //                accname.AssignedBy = model.CreatedBy;
                //                db.AccNameAssignedWithUsers.Add(accname);
                //                db.SaveChanges();
                //            }
                //        }
                //    }
                //    catch (Exception)
                //    {
                //        msg = "New account group " + model.AccountName + " created was unsuccessful on " + now.ToString("MMM dd , yyyy hh:mm tt") + " .";
                //        OperationStatus = -1;
                //    }
                //    SaveAuditLog(opname, msg, "Finance", "All Account Name", "AccountName", ColumnId, model.CreatedBy, now, OperationStatus);
                //}
                if (OperationStatus == 1)
                {
                    return Json("Success", JsonRequestBehavior.AllowGet);
                }
            }
            return Json("Error", JsonRequestBehavior.AllowGet);
        }

        //****************************** Show Account Name Details Information *****************************
        [ClaimsAction(ClaimsActions.Info)]
        public PartialViewResult AccountInfo(int AccId)
        {
            AccountName model = db.AccountNames.Where(m => m.AccId == AccId).FirstOrDefault();
            return PartialView(model);
        }
        [EncryptedActionParameter]
        public ActionResult AccountDetails(int? AccId)
        {
            if (AccId == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            AccountName model = db.AccountNames.Where(m => m.AccId == AccId).FirstOrDefault();
            return View(model);
        }

        public PartialViewResult DisplayAccountDetails(int? AccId)
        {
            AccountName model = db.AccountNames.Where(m => m.AccId == AccId).FirstOrDefault();
            return PartialView(model);
        }

        //****************************** Edit Account Name Details Information ********************************
        public PartialViewResult _EditAccountName(int AccId)
        {
            ViewBag.AccId = AccId;
            return PartialView();
        }
        public PartialViewResult _EditAccountNameWin(int AccId)
        {
            View_Account_Name model = db.View_Account_Name.Where(m => m.AccId == AccId).FirstOrDefault();
            return PartialView(model);
        }
        public JsonResult UpdateAccName(int AccId, string AccountName, long UpdatedBy)
        {
            if (AccountName != "" && AccId > 0)
            {
                string msg = "";
                string opname = "Account Name Edit";

                int OperationStatus = -1;
                AccountName acc_grp = db.AccountNames.Find(AccId);
                if (acc_grp != null)
                {
                    string prename = acc_grp.AccountName1;
                    acc_grp.AccountName1 = AccountName;
                    acc_grp.UpdatedBy = UpdatedBy;
                    acc_grp.UpdatedDate = now;
                    db.Entry(acc_grp).State = EntityState.Modified;
                    try
                    {
                        db.SaveChanges();
                        OperationStatus = 1;
                        msg = "Account name '" + prename + "' has been successfully change as '" + AccountName + "'  on " + now.ToString("MMM dd , yyy hh:mm tt") + ".";
                    }
                    catch (Exception)
                    {
                        OperationStatus = -1;
                        msg = "Account name '" + prename + "' change as '" + AccountName + "'  was unsuccessful on " + now.ToString("MMM dd , yyy hh:mm tt") + ".";
                    }
                    SaveAuditLog(opname, msg, "Accounting", "Account Details", "AccountName", AccId, UpdatedBy, now, OperationStatus);
                    if (OperationStatus == 1)
                    {
                        return Json("Success", JsonRequestBehavior.AllowGet);
                    }
                }
            }
            return Json("Error", JsonRequestBehavior.AllowGet);
        }

        public ActionResult AccountEdit(int AccId)
        {

            View_Account_Name data = db.View_Account_Name.Where(m => m.AccId == AccId).FirstOrDefault();
            AccountGroupModelView model = new AccountGroupModelView();
            model.AccountName = data.AccountName;
            model.AccId = data.AccId;
            model.Acc_CashType = data.Acc_CashType;
            if (model.Acc_CashType == 1 || model.Acc_CashType == 3)
            {
                model.Acc_Type = data.Acc_Type;
                model.Balance = data.Balance;
                model.TotalCreditLimit = data.TotalCreditLimit;
                model.TransactionHigestAmount = data.TransactionHigestAmount;
                model.NoOfTranPerMonth = data.NoOfTranPerMonth;
                model.WorkingBalance = data.Balance;
            }
            else
            {
                model.Acc_Type = data.Acc_Type;
                model.Balance = data.Balance;
            }
            model.SiteId = data.SiteId;
            model.UnitId = data.UnitId;
            model.DeptId = data.DeptId;
            model.StoreId = data.StoreId;
            model.WareId = data.WareId;
            model.AllAssignUserId = data.AllAssignUserId;

            ViewBag.Acc_Type = new SelectList(db.AccountTypes, "Acc_Type_Id", "Name", model.Acc_Type);
            return PartialView(model);
        }
        public JsonResult UpdateAccGroup(AccountGroupModelView model, string AllUserId, string AllLocationId)
        {
            string msg = "";
            string opname = "";
            int ColumnId = 0;
            int OperationStatus = 1;
            AccountName acc_grp = db.AccountNames.Find(model.AccId);
            acc_grp.AccountName1 = model.AccountName;
            acc_grp.Acc_CashType = model.Acc_CashType;
            acc_grp.Balance = model.Balance;
            if (acc_grp.Acc_CashType == 1 || acc_grp.Acc_CashType == 3)
            {
                acc_grp.TotalCreditLimit = model.TotalCreditLimit;
                acc_grp.TransactionHigestAmount = model.TransactionHigestAmount;
                acc_grp.NoOfTranPerMonth = model.NoOfTranPerMonth;

                acc_grp.Acc_Type = Convert.ToInt32(model.Acc_Type);
                acc_grp.SiteId = model.SiteId;
                if (model.Acc_Type == 1)
                {
                    acc_grp.UnitId = model.UnitId;
                }
            }
            else
            {
                acc_grp.SiteId = model.SiteId;
                acc_grp.UnitId = model.UnitId;
                acc_grp.DeptId = model.DeptId;
            }
            acc_grp.Description = model.Description;


            if ((model.Acc_CashType == 1 || acc_grp.Acc_CashType == 3) && model.Acc_Type == 3 && AllLocationId != "")
            {
                string[] AllSotreId = AllLocationId.Split(',');
                for (int i = 0; i < AllSotreId.Length; i++)
                {
                    string idWithChar = AllSotreId[i].ToString();
                    if (idWithChar.Split('-')[1] == "s")
                    {
                        acc_grp.DeptId = null; acc_grp.UnitId = null;
                        acc_grp.SiteId = Convert.ToInt32(idWithChar.Split('-')[0]);
                        db.Entry(acc_grp).State = EntityState.Modified;
                        try
                        {
                            db.SaveChanges();
                            msg = "Account Group '" + model.AccountName + "'" + "  has been successfully Updateded on " + now;
                            opname = "Update Account Group";
                            ColumnId = db.AccountNames.Where(m => m.UpdatedBy == model.UpdatedBy && m.AccId == model.AccId).Select(m => m.AccId).FirstOrDefault();
                        }
                        catch (Exception)
                        {
                            msg = "Account group " + model.AccountName + " updated unsuccessful.";
                            OperationStatus = -1;
                            throw;
                        }
                        SaveAuditLog(opname, msg, "Accounting", "Update Account Group", "AccountGroups", ColumnId, model.UpdatedBy.Value, now, OperationStatus);
                    }
                    else if (idWithChar.Split('-')[1] == "u")
                    {
                        acc_grp.DeptId = null; acc_grp.SiteId = model.SiteId;
                        acc_grp.UnitId = Convert.ToInt32(idWithChar.Split('-')[0]);
                        db.Entry(acc_grp).State = EntityState.Modified;
                        try
                        {
                            db.SaveChanges();
                            msg = "Account Group '" + model.AccountName + "'" + "  has been successfully Updateded on " + now;
                            opname = "Update Account Group";
                            ColumnId = db.AccountNames.Where(m => m.UpdatedBy == model.UpdatedBy && m.AccId == model.AccId).Select(m => m.AccId).FirstOrDefault();
                        }
                        catch (Exception)
                        {
                            msg = "Account group " + model.AccountName + " updated unsuccessful.";
                            OperationStatus = -1;
                            throw;
                        }
                        SaveAuditLog(opname, msg, "Accounting", "Update Account Group", "AccountGroups", ColumnId, model.UpdatedBy.Value, now, OperationStatus);
                    }
                    else
                    {
                        acc_grp.SiteId = null; acc_grp.UnitId = null;
                        acc_grp.DeptId = Convert.ToInt32(idWithChar.Split('-')[0]);
                        db.Entry(acc_grp).State = EntityState.Modified;
                        try
                        {
                            db.SaveChanges();
                            msg = "Account Group '" + model.AccountName + "'" + "  has been successfully Updateded on " + now;
                            opname = "Update Account Group";
                            ColumnId = db.AccountNames.Where(m => m.UpdatedBy == model.UpdatedBy && m.AccId == model.AccId).Select(m => m.AccId).FirstOrDefault();
                        }
                        catch (Exception)
                        {
                            msg = "Account group " + model.AccountName + " updated unsuccessful.";
                            OperationStatus = -1;
                            throw;
                        }
                        SaveAuditLog(opname, msg, "Accounting", "Update Account Group", "AccountGroups", ColumnId, model.UpdatedBy.Value, now, OperationStatus);
                    }
                }
            }
            else
            {
                acc_grp.UpdatedBy = model.UpdatedBy;
                acc_grp.UpdatedDate = now;
                db.Entry(acc_grp).State = EntityState.Modified;
                try
                {
                    db.SaveChanges();
                    msg = "Account Group '" + model.AccountName + "'" + "  has been successfully Updateded on " + now;
                    opname = "Update Account Group";
                    ColumnId = db.AccountNames.Where(m => m.UpdatedBy == model.UpdatedBy && m.AccId == model.AccId).Select(m => m.AccId).FirstOrDefault();

                    if (AllUserId != null)
                    {
                        string[] allUser = AllUserId.Split(',');
                        ArrayList allusers = new ArrayList();
                        foreach (var m in allUser)
                        {
                            if (m != string.Empty)
                            {
                                allusers.Add(m);
                            }
                        }
                        if (model.AllAssignUserId != null)
                        {
                            string[] usedUserId = model.AllAssignUserId.Split(',');
                            foreach (var m in usedUserId)
                            {
                                if (allusers.Contains(m))
                                {
                                    allusers.Remove(m);
                                }
                                else
                                {
                                    long uid = Convert.ToInt64(m);
                                    AccNameAssignedWithUser acc = db.AccNameAssignedWithUsers.Where(n => n.AccId == model.AccId && n.UserId == uid).FirstOrDefault();
                                    db.AccNameAssignedWithUsers.Remove(acc);
                                    db.SaveChanges();
                                }
                            }
                        }
                        for (int j = 0; j < allusers.Count; j++)
                        {
                            AccNameAssignedWithUser accname = new AccNameAssignedWithUser();
                            accname.UserId = Convert.ToInt32(allusers[j]);
                            accname.UserType = 1;
                            accname.AccId = model.AccId;
                            accname.AssignedDate = now;
                            accname.AssignedBy = model.UpdatedBy.Value;
                            db.AccNameAssignedWithUsers.Add(accname);
                            db.SaveChanges();
                        }
                    }
                }
                catch (Exception)
                {
                    msg = "Account group " + model.AccountName + " updated unsuccessful.";
                    OperationStatus = -1;
                    throw;
                }
                SaveAuditLog(opname, msg, "Accounting", "Update Account Group", "AccountGroups", ColumnId, model.UpdatedBy.Value, now, OperationStatus);
            }
            return Json("Success", JsonRequestBehavior.AllowGet);
        }
        #endregion


        #region BankList Create, Update, Delete
        public PartialViewResult BankCreatePopUp(int? BankId)
        {
            ViewBag.BankId = BankId;
            return PartialView();
        }
        public PartialViewResult BankCreate(int? BankId)
        {
            BankListModelView model = new BankListModelView();
            if (BankId > 0)
            {
                BankList bank = db.BankLists.Find(BankId);
                model.BankId = bank.BankId;
                model.BankName = bank.BankName;
            }
            return PartialView(model);
        }
        public JsonResult BankAdd(BankListModelView model, int? BankId)
        {
            string msg = "";
            int ColumnId = 0;
            int OperationStatus = 1;
            BankList bank = new BankList();
            if (BankId > 0)
            {
                bank = db.BankLists.Find(BankId);
            }
            bank.BankName = model.BankName;
            if (BankId > 0)
            {
                bank.Status = model.Status;
                bank.UpdatedBy = model.CreatedBy;
                bank.UpdatedDate = now;
                db.Entry(bank).State = EntityState.Modified;
                try
                {
                    db.SaveChanges();
                    msg = "Information of '" + model.BankName + "' has been successfully updated on " + now.ToString("MMM dd , yyy hh:mm tt") + " .";
                    ColumnId = Convert.ToInt32(BankId);
                }
                catch (Exception ex)
                {
                    string errorMsg = ex.Message.ToString();
                    OperationStatus = -1;
                    msg = "Information of '" + model.BankName + "' update unsuccessful on " + now.ToString("MMM dd , yyy hh:mm tt") + " .";
                }
                SaveAuditLog("Update Bank List", msg, "Finance", "BankList", "BankList", ColumnId, Convert.ToInt32(bank.UpdatedBy), now, OperationStatus);
            }
            else
            {
                bank.CreatedBy = model.CreatedBy;
                bank.Status = 1;
                bank.CreratedDate = now;
                db.BankLists.Add(bank);
                try
                {
                    db.SaveChanges();
                    msg = "New bank '" + model.BankName + "' has been successfully added on " + now.ToString("MMM dd , yyy hh:mm tt") + " .";
                    ColumnId = db.BankLists.Where(m => m.CreatedBy == model.CreatedBy).Max(m => m.BankId);
                }
                catch (Exception ex)
                {
                    string errorMsg = ex.Message.ToString();
                    OperationStatus = -1;
                    msg = "New bank '" + model.BankName + "' add was unsuccessful on " + now.ToString("MMM dd , yyy hh:mm tt") + " .";
                }
                SaveAuditLog("Create Bank List", msg, "Finance", "BankList", "BankList", ColumnId, model.CreatedBy, now, OperationStatus);
            }
            if (OperationStatus == 1)
            {
                return Json("Success", JsonRequestBehavior.AllowGet);
            }
            return Json("Error", JsonRequestBehavior.AllowGet);
        }

        // ************** Bank ***************************
        public PartialViewResult CreateBankBranchPopUp(int? BankId)
        {
            ViewBag.BankId = BankId;
            return PartialView();
        }
        public PartialViewResult CreateBankBranch(int? BankId)
        {
            Bank_BranchListModelView model = new Bank_BranchListModelView();
            model.BankId = Convert.ToInt32(BankId);
            ViewBag.DivisionList = new SelectList(db.DivisionLists, "DivisionId", "DivisionName", model.DivisonId);
            ViewBag.CountryId = new SelectList(db.CountryLists, "CountryCode", "CountryName", model.Country);
            ViewBag.BankList = new SelectList(db.BankLists, "BankId", "BankName");
            return PartialView(model);
        }
        public JsonResult BankBranchSave(Bank_BranchListModelView model, int? BranchId)
        {
            string msg = "";
            int ColumnId = 0;
            int OperationStatus = 1;
            string bankName = "";
            BankBranchList bank = new BankBranchList();
            if (BranchId > 0)
            {
                bank = db.BankBranchLists.Find(BranchId);
            }
            bank.BankId = model.BankId;
            bankName = db.BankLists.Where(m => m.BankId == model.BankId).Select(m => m.BankName).FirstOrDefault();
            bank.BranchName = model.BranchName;
            bank.Address = model.Address;
            bank.AddressLine1 = model.AddressLine1;
            bank.City = model.City;
            bank.Country = model.Country;
            bank.DivisonId = model.DivisonId;
            bank.State = model.State;
            bank.PostalCode = model.PostalCode;
            bank.PhoneNo = model.PhoneNo;
            bank.MobileNo = model.MobileNo;
            if (BranchId > 0)
            {
                bank.UpdatedBy = model.CreatedBy;
                bank.UpdatedDate = now;
                bank.Status = model.Status;
                db.Entry(bank).State = EntityState.Modified;
                try
                {
                    db.SaveChanges();
                    msg = "Information of branch '" + model.BranchName + "' of bank '" + bankName + "' has been successfully updated on " + now.ToString("MMM dd , yyy hh:mm tt") + " .";
                    ColumnId = Convert.ToInt32(BranchId);
                }
                catch (Exception ex)
                {
                    string errorMsg = ex.Message.ToString();
                    OperationStatus = -1;
                    msg = "Information of branch '" + model.BranchName + "' of bank '" + bankName + "' update unsuccessful on " + now.ToString("MMM dd , yyy hh:mm tt") + " .";
                }
                SaveAuditLog("Update Bank Branch", msg, "Finance", "BankAccounts", "BankBranchList", ColumnId, Convert.ToInt32(model.UpdatedBy), now, OperationStatus);
            }
            else
            {
                bank.CreatedBy = model.CreatedBy;
                bank.CreatedDate = now;
                bank.Status = 0;
                db.BankBranchLists.Add(bank);
                try
                {
                    BankList bankList = db.BankLists.Where(m => m.BankId == model.BankId).SingleOrDefault();
                    bankList.Status = 1;
                    db.Entry(bankList).State = EntityState.Modified;
                    db.SaveChanges();
                    msg = "New branch '" + model.BranchName + "' of bank '" + bankName + "' has been successfully added on " + now.ToString("MMM dd , yyy hh:mm tt") + " .";
                    ColumnId = db.BankBranchLists.Where(m => m.CreatedBy == model.CreatedBy).Max(m => m.BranchId);
                }
                catch (Exception ex)
                {
                    string errorMsg = ex.Message.ToString();
                    OperationStatus = -1;
                    msg = "New branch '" + model.BranchName + "' of bank '" + bankName + "' added unsuccessful on " + now.ToString("MMM dd , yyy hh:mm tt") + " .";
                }
                SaveAuditLog("Create Bank Branch", msg, "Finance", "BankBranches", "BankBranchList", ColumnId, model.CreatedBy, now, OperationStatus);
            }
            if (OperationStatus == 1)
            {
                return Json("Success", JsonRequestBehavior.AllowGet);
            }
            return Json("Error", JsonRequestBehavior.AllowGet);
        }

        //***************************** Bank List ***********************
        [EncryptedActionParameter]
        public ActionResult AllBankList()
        {
            return View();
        }
        [EncryptedActionParameter]
        public PartialViewResult _AllBankList(int? BankId)
        {
            if (BankId != null)
            {
                var bank = (from m in db.View_Bank_info
                            where m.BankId == BankId
                            select new BankListModelView
                            {
                                BankId = m.BankId,
                                BankName = m.BankName,
                                CreatedBy = m.CreatedBy,
                                CreatedByName = m.UserName,
                                CreratedDate = m.CreratedDate,
                                CreatedDateName = DbFunctions.TruncateTime(m.CreratedDate).ToString(),
                                CreatedByPicture = m.PictureName,
                                UpdatedBy = m.UpdatedBy,
                                UpdatedByName = m.Updated_UserName,
                                UpdatedDate = m.UpdatedDate,
                                UpdatedDateName = DbFunctions.TruncateTime(m.UpdatedDate).ToString(),
                                UpdatedByPicture = m.Updated_UserPic,
                            }).OrderBy(o => o.BankName).ToList();

                ViewBag.BankList = bank.Take(ShowBankPerPage).ToList();
                ViewBag.TotalBank = bank.Count();
                Session["TotalBankCount"] = bank.Count();
                Session["AllBank"] = bank;
            }
            else
            {
                var bank = (from m in db.View_Bank_info
                            select new BankListModelView
                            {
                                BankId = m.BankId,
                                BankName = m.BankName,
                                CreatedBy = m.CreatedBy,
                                CreatedByName = m.UserName,
                                CreratedDate = m.CreratedDate,
                                CreatedDateName = DbFunctions.TruncateTime(m.CreratedDate).ToString(),
                                CreatedByPicture = m.PictureName,
                                UpdatedBy = m.UpdatedBy,
                                UpdatedByName = m.Updated_UserName,
                                UpdatedDate = m.UpdatedDate,
                                UpdatedDateName = DbFunctions.TruncateTime(m.UpdatedDate).ToString(),
                                UpdatedByPicture = m.Updated_UserPic,
                            }).OrderBy(o => o.BankName).ToList();
                ViewBag.BankList = bank.Take(ShowBankPerPage).ToList();
                ViewBag.TotalBank = bank.Count();
                Session["TotalBankCount"] = bank.Count();
                Session["AllBank"] = bank;
            }
            return PartialView(ViewBag.BankList);
        }

        public JsonResult GetBankList(int? page)
        {
            page = page ?? 0;
            int total = Convert.ToInt32(Session["TotalBankCount"]);
            int skip = ShowBankPerPage * Convert.ToInt32(page);
            var canPage = skip < total;
            if (canPage)
            {
                var listBackFromSession = (List<BankListModelView>)Session["AllBank"];
                var bankList = listBackFromSession.Skip(skip).Take(ShowBankPerPage).ToList();
                return Json(bankList, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json("", JsonRequestBehavior.AllowGet);
            }
        }
        public JsonResult SearchBankList(string prefix)
        {
            if (prefix == "")
            {
                return Json("", JsonRequestBehavior.AllowGet);
            }
            else
            {
                var allbank = (List<BankListModelView>)Session["AllBank"];
                var bank = (from m in allbank
                            where (m.BankName.ToLower().Contains(prefix.ToLower()))
                            select new BankListModelView
                            {
                                BankId = m.BankId,
                                BankName = m.BankName,
                                CreatedBy = m.CreatedBy,
                                CreatedByName = m.CreatedByName,
                                CreratedDate = m.CreratedDate,
                                CreatedByPicture = m.CreatedByPicture,
                                UpdatedBy = m.UpdatedBy,
                                UpdatedByName = m.UpdatedByName,
                                UpdatedDate = m.UpdatedDate,
                                UpdatedByPicture = m.UpdatedByPicture,
                            }).ToList();
                Session["AllBankBySearch"] = bank;
                return Json(bank, JsonRequestBehavior.AllowGet);
            }
        }
        //************************************************
        [EncryptedActionParameter]
        public ActionResult BankBranchDetails(int? BankId, int? forDisplay)
        {
            BankListModelView model = new BankListModelView();
            BankList bank = db.BankLists.Find(BankId);
            model.BankId = bank.BankId;
            model.BankName = bank.BankName;
            model.ForDisplay = Convert.ToInt32(forDisplay);
            return View(model);
        }
        public PartialViewResult _ShowBankName(int? BankId, int? forDisplay)
        {
            BankListModelView model = new BankListModelView();
            model.ForDisplay = Convert.ToInt32(forDisplay);
            if (BankId > 0)
            {
                BankList bank = db.BankLists.Find(BankId);
                model.BankId = bank.BankId;
                model.BankName = bank.BankName;
            }
            return PartialView(model);
        }
        public PartialViewResult AllBranchList(int? BankId, int? forDisplay)
        {
            int ForDisplay = Convert.ToInt32(forDisplay);
            ViewBag.Display = ForDisplay;
            ViewBag.BankId = BankId;
            return PartialView();
        }
        public PartialViewResult _AllBranchList(int? BankId, int? BranchId, int? forDisplay)
        {
            int Display = Convert.ToInt32(forDisplay);
            if (BranchId == null)
            {
                var branch = (from m in db.View_Branch_info
                              where m.BankId == BankId
                              select new Bank_BranchListModelView
                              {
                                  BranchId = m.BranchId,
                                  BranchName = m.BranchName,
                                  CreatedBy = m.CreatedBy,
                                  CreatedByName = m.UserName,
                                  CreatedDate = m.CreatedDate,
                                  CreatedDateName = DbFunctions.TruncateTime(m.CreatedDate).ToString(),
                                  CreatedByPicture = m.PictureName,
                                  UpdatedBy = m.UpdatedBy,
                                  UpdatedByName = m.Updated_UserName,
                                  UpdatedDate = m.UpdatedDate,
                                  UpdatedDateName = DbFunctions.TruncateTime(m.UpdatedDate).ToString(),
                                  UpdatedByPicture = m.Updated_UserPic,
                                  ForDisplay = Display
                              }).OrderBy(o => o.BranchName).ToList();
                ViewBag.BranchList = branch.Take(ShowBranchPerPage).ToList();
                ViewBag.TotalBranch = branch.Count();
                Session["TotalBranchCount"] = branch.Count();
                Session["AllBranch"] = branch;
            }
            else
            {
                var branch = (from m in db.View_Branch_info
                              where m.BranchId == BranchId && m.BankId == BankId
                              select new Bank_BranchListModelView
                              {
                                  BranchId = m.BranchId,
                                  BranchName = m.BranchName,
                                  CreatedBy = m.CreatedBy,
                                  CreatedByName = m.UserName,
                                  CreatedDate = m.CreatedDate,
                                  CreatedDateName = DbFunctions.TruncateTime(m.CreatedDate).ToString(),
                                  CreatedByPicture = m.PictureName,
                                  UpdatedBy = m.UpdatedBy,
                                  UpdatedByName = m.Updated_UserName,
                                  UpdatedDate = m.UpdatedDate,
                                  UpdatedDateName = DbFunctions.TruncateTime(m.UpdatedDate).ToString(),
                                  UpdatedByPicture = m.Updated_UserPic,
                              }).OrderBy(o => o.BranchName).ToList();
                ViewBag.BranchList = branch.Take(ShowBranchPerPage).ToList();
                ViewBag.TotalBranch = branch.Count();
                Session["TotalBranchCount"] = branch.Count();
                Session["AllBranch"] = branch;

            }
            return PartialView(ViewBag.BranchList);
        }
        public JsonResult GetBranchList(int? page)
        {
            page = page ?? 0;
            int total = Convert.ToInt32(Session["TotalBranchCount"]);
            int skip = ShowBranchPerPage * Convert.ToInt32(page);
            var canPage = skip < total;
            if (canPage)
            {
                var listBackFromSession = (List<Bank_BranchListModelView>)Session["AllBranch"];
                var branchList = listBackFromSession.Skip(skip).Take(ShowBranchPerPage).ToList();
                return Json(branchList, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json("", JsonRequestBehavior.AllowGet);
            }
        }
        public JsonResult SearchBranchList(string prefix)
        {
            if (prefix == "")
            {
                return Json("", JsonRequestBehavior.AllowGet);
            }
            else
            {
                var allbank = (List<Bank_BranchListModelView>)Session["AllBranch"];
                var bank = (from m in allbank
                            where (m.BranchName.ToLower().Contains(prefix.ToLower()))
                            select new Bank_BranchListModelView
                            {
                                BranchId = m.BranchId,
                                BranchName = m.BranchName,
                                CreatedBy = m.CreatedBy,
                                CreatedDate = m.CreatedDate,
                                UpdatedBy = m.UpdatedBy,
                                UpdatedDate = m.UpdatedDate,
                            }).ToList();
                Session["AllBranchBySearch"] = bank;
                return Json(bank, JsonRequestBehavior.AllowGet);
            }
        }
        [EncryptedActionParameter]
        public ActionResult BankAccountDetails(int? Id, int? forDisplay)
        {
            if (Id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            else
            {
                BankBranchList bank = db.BankBranchLists.Where(m => m.BranchId == Id).FirstOrDefault();
                Bank_BranchListModelView model = new Bank_BranchListModelView();
                model.BranchId = Convert.ToInt32(Id);
                model.BankId = bank.BankId;
                model.BranchName = bank.BranchName;
                model.ForDisplay = Convert.ToInt32(forDisplay);
                return View(model);
            }
        }
        public PartialViewResult _ShowBankInfo(int? Id, int? forDisplay)
        {
            View_Branch_info bank = db.View_Branch_info.Where(m => m.BranchId == Id).FirstOrDefault();
            Bank_BranchListModelView model = new Bank_BranchListModelView();
            model.BankId = Convert.ToInt32(Id);
            model.BankName = bank.BankName;
            model.BranchName = bank.BranchName;
            model.Address = bank.Address;
            model.AddressLine1 = bank.AddressLine1;
            model.Country = bank.Country;
            model.CountryName = bank.CountryName;
            model.City = bank.City;
            model.DivisonId = bank.DivisonId;
            model.DivisionName = bank.DivisionName;
            model.MobileNo = bank.MobileNo;
            model.PhoneNo = bank.PhoneNo;
            model.PostalCode = bank.PostalCode;
            model.State = bank.State;
            model.ForDisplay = Convert.ToInt32(forDisplay);
            return PartialView(model);
        }
        public PartialViewResult _EditBranchInformation(int? BranchId)
        {
            ViewBag.BranchId = BranchId;
            return PartialView();
        }
        public PartialViewResult _EditBankList(int? BranchId)
        {
            Bank_BranchListModelView model = new Bank_BranchListModelView();
            BankBranchList bank = db.BankBranchLists.Find(BranchId);
            model.BranchId = bank.BranchId;
            model.BranchName = bank.BranchName;
            model.BankId = bank.BankId;
            model.Address = bank.Address;
            model.AddressLine1 = bank.AddressLine1;
            model.Country = bank.Country;
            model.City = bank.City;
            model.DivisonId = bank.DivisonId;
            model.MobileNo = bank.MobileNo;
            model.PhoneNo = bank.PhoneNo;
            model.PostalCode = bank.PostalCode;
            model.State = bank.State;
            ViewBag.DivisionList = new SelectList(db.DivisionLists, "DivisionId", "DivisionName", model.DivisonId);
            ViewBag.CountryId = new SelectList(db.CountryLists, "CountryCode", "CountryName", model.Country);
            ViewBag.BankList = new SelectList(db.BankLists, "BankId", "BankName");
            return PartialView(model);
        }
        public JsonResult EditExistingBankInfo(Bank_BranchListModelView model, int? BranchId)
        {
            string msg = "";
            int ColumnId = 0;
            int OperationStatus = 1;
            int branchId;
            string bankName = "";
            BankBranchList bank = db.BankBranchLists.Find(BranchId);
            bank.BankId = model.BankId;
            bankName = db.BankLists.Where(m => m.BankId == model.BankId).Select(m => m.BankName).FirstOrDefault();
            bank.BranchName = model.BranchName;
            bank.Address = model.Address;
            bank.AddressLine1 = model.AddressLine1;
            bank.City = model.City;
            bank.Country = model.Country;
            bank.DivisonId = model.DivisonId;
            bank.State = model.State;
            bank.PostalCode = model.PostalCode;
            bank.PhoneNo = model.PhoneNo;
            bank.MobileNo = model.MobileNo;
            bank.BranchId = model.BranchId;
            bank.Status = model.Status;
            bank.UpdatedBy = model.CreatedBy;
            bank.UpdatedDate = now;
            db.Entry(bank).State = EntityState.Modified;
            try
            {
                db.SaveChanges();
                ColumnId = Convert.ToInt32(BranchId);
                msg = "Information of branch '" + model.BranchName + "' of bank '" + bankName + "' has been successfully updated on " + now.ToString("MMM dd , yyy hh:mm tt") + " .";

            }
            catch (Exception ex)
            {
                string errorMsg = ex.Message.ToString();
                msg = "Information of branch '" + model.BranchName + "' of bank '" + bankName + "' update unsuccessful on " + now.ToString("MMM dd , yyy hh:mm tt") + " .";
                OperationStatus = -1;
            }
            SaveAuditLog("Update Bank Info", msg, "Finance", "EditExistingBankInfo", "BankBranchList", ColumnId, Convert.ToInt32(bank.UpdatedBy), now, OperationStatus);

            if (OperationStatus == 1)
            {

                branchId = Convert.ToInt32(BranchId);
                return Json(branchId, JsonRequestBehavior.AllowGet);
            }

            return Json("Error", JsonRequestBehavior.AllowGet);
        }
        public JsonResult CheckBankExist(BankListModelView model, int BankId)
        {
            BankList data = db.BankLists.Find(BankId);
            if (db.AllSalaryPayLists.Where(m => m.BankId == data.BankId).Any() || db.AllUserLoanLists.Where(m => m.BankId == data.BankId).Any())
            {
                return Json(true, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(false, JsonRequestBehavior.AllowGet);
            }
        }
        public JsonResult DeleteBank(int Id, long CreatedBy)
        {
            string msg = "";
            int ColumnId = 0;
            int OperationStatus = 1;
            if (db.BankLists.Where(m => m.BankId == Id).Any())
            {
                if (db.AllSalaryPayLists.Where(m => m.BankId == Id).Any() || db.AllUserLoanLists.Where(m => m.BankId == Id).Any())
                {
                    return Json("Error", JsonRequestBehavior.AllowGet);
                }
                else
                {
                    BankList bank = db.BankLists.Find(Id);
                    int BankId = bank.BankId;
                    var bankBranch = db.BankBranchLists.Where(m => m.BankId == Id).ToList();
                    if (bankBranch.Count() > 0)
                    {
                        foreach (var branch in bankBranch)
                        {
                            var bankAccount = db.BankAccountLists.Where(m => m.BranchId == branch.BranchId).ToList();
                            if (bankAccount.Count() > 0)
                            {
                                foreach (var account in bankAccount)
                                {
                                    db.BankAccountLists.Remove(account);
                                    ColumnId = account.BankAccountId;
                                    try
                                    {
                                        db.SaveChanges();
                                        OperationStatus = 1;
                                        msg = "Information of account '" + account.AccountName + "'of '" + branch.BranchName + "' of '" + bank.BankName + "' has been successfully deleted on " + now.ToString("MMM dd , yyy hh:mm tt") + " .";
                                    }
                                    catch (Exception ex)
                                    {
                                        string errmsg = ex.ToString();
                                        OperationStatus = -1;
                                        msg = "Information of account '" + account.AccountName + "'of '" + branch.BranchName + "' of '" + bank.BankName + "' delete unsuccessful on " + now.ToString("MMM dd , yyy hh:mm tt") + " .";
                                    }
                                    SaveAuditLog("Delete Account Info", msg, "Finance", "BankList", "BankList", ColumnId, CreatedBy, now, OperationStatus);
                                }
                            }
                            db.BankBranchLists.Remove(branch);
                            ColumnId = branch.BranchId;
                            try
                            {
                                db.SaveChanges();
                                OperationStatus = 1;
                                msg = "Information of branch '" + branch.BranchName + "' of '" + bank.BankName + "' has been successfully deleted on " + now.ToString("MMM dd , yyy hh:mm tt") + " .";
                            }
                            catch (Exception ex)
                            {
                                string errmsg = ex.ToString();
                                OperationStatus = -1;
                                msg = "Information of branch '" + branch.BranchName + "' of '" + bank.BankName + "' delete unsuccessful on " + now.ToString("MMM dd , yyy hh:mm tt") + " .";
                            }
                            SaveAuditLog("Delete Branch Info", msg, "Finance", "BankList", "BankList", ColumnId, CreatedBy, now, OperationStatus);
                        }
                    }
                    db.BankLists.Remove(bank);
                    ColumnId = bank.BankId;
                    try
                    {
                        db.SaveChanges();
                        OperationStatus = 1;
                        msg = "Information of bank '" + bank.BankName + "' has been successfully deleted on " + now.ToString("MMM dd , yyy hh:mm tt") + " .";
                    }
                    catch (Exception ex)
                    {
                        string errmsg = ex.ToString();
                        OperationStatus = -1;
                        msg = "Information of bank '" + bank.BankName + "' delete unsuccessful on " + now.ToString("MMM dd , yyy hh:mm tt") + " .";
                    }
                    SaveAuditLog("Delete Bank Info", msg, "Finance", "BankList", "BankList", ColumnId, CreatedBy, now, OperationStatus);
                    if (OperationStatus == 1)
                    {
                        //return Json("Success", JsonRequestBehavior.AllowGet);
                        return Json(BankId, JsonRequestBehavior.AllowGet);
                    }
                }
            }
            return Json("Error", JsonRequestBehavior.AllowGet);
        }
        public JsonResult CheckBranchExist(int BranchId)
        {
            int[] bankAccounId = db.BankAccountLists.Where(p => p.BranchId == BranchId).Select(n => n.BankAccountId).Distinct().ToArray();
            var allpaylist = (from m in db.AllSalaryPayLists
                              where m.BankAccId > 0
                              select new
                              {
                                  Id = m.Id,
                                  AccountId = m.BankAccId
                              }).ToList();
            var allLoanlist = (from m in db.AllUserLoanLists
                               where m.BankAccId > 0
                               select new
                               {
                                   Id = m.LoanId,
                                   AccountId = m.BankAccId
                               }).ToList();
            if (allpaylist.Where(m => bankAccounId.Contains((int)(m.AccountId))).Any() || allpaylist.Where(m => bankAccounId.Contains((int)(m.AccountId))).Any())
            {
                return Json(true, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(false, JsonRequestBehavior.AllowGet);
            }
        }
        public JsonResult DeleteBranch(int Id, long CreatedBy)
        {
            string msg = "";
            int ColumnId = 0;
            int OperationStatus = 1;
            string bankName = "";
            BankBranchList bank = db.BankBranchLists.Find(Id);
            int BankId = bank.BankId;
            bankName = db.BankLists.Where(m => m.BankId == BankId).Select(m => m.BankName).FirstOrDefault();
            db.BankBranchLists.Remove(bank);
            ColumnId = bank.BankId;
            var bankAccount = db.BankAccountLists.Where(m => m.BranchId == bank.BranchId).ToList();
            if (bankAccount.Count() > 0)
            {
                foreach (var account in bankAccount)
                {
                    db.BankAccountLists.Remove(account);
                    try
                    {
                        db.SaveChanges();
                        OperationStatus = 1;
                        msg = "Information of account '" + account.AccountName + "'of '" + bank.BranchName + "' of '" + bankName + "' has been successfully deleted on " + now.ToString("MMM dd , yyy hh:mm tt") + " .";
                    }
                    catch (Exception ex)
                    {
                        string errmsg = ex.ToString();
                        OperationStatus = -1;
                        msg = "Information of account '" + account.AccountName + "'of '" + bank.BranchName + "' of '" + bankName + "' delete unsuccessful on " + now.ToString("MMM dd , yyy hh:mm tt") + " .";
                    }
                    SaveAuditLog("Delete Account Info", msg, "Finance", "BankList", "BankList", ColumnId, CreatedBy, now, OperationStatus);
                }
            }
            try
            {
                db.SaveChanges();
                if (!db.BankBranchLists.Where(m => m.BankId == BankId).Any())
                {
                    BankList bankList = db.BankLists.Where(m => m.BankId == BankId).SingleOrDefault();
                    bankList.Status = 0;
                    db.Entry(bankList).State = EntityState.Modified;
                    db.SaveChanges();
                }
                OperationStatus = 1;
                msg = "Information of branch '" + bank.BranchName + "' of bank '" + bankName + "' has been successfully deleted on " + now.ToString("MMM dd , yyy hh:mm tt") + " .";
            }
            catch (Exception ex)
            {
                string errmsg = ex.ToString();
                OperationStatus = -1;
                msg = "Information of branch '" + bank.BranchName + "' of bank '" + bankName + "' delete unsuccessful on " + now.ToString("MMM dd , yyy hh:mm tt") + " .";
            }
            SaveAuditLog("Delete Branch Info", msg, "Finance", "BankBranches", "BankBranchList", ColumnId, CreatedBy, now, OperationStatus);
            if (OperationStatus == 1)
            {
                return Json(db.BankBranchLists.Where(m => m.BankId == BankId).Any(), JsonRequestBehavior.AllowGet);
            }
            return Json("Error", JsonRequestBehavior.AllowGet);
        }
        //********************** Bank Acount*********************************
        public PartialViewResult _AddNewAccountPopUp(int? BranchId, int? BankAccountId)
        {
            ViewBag.BranchId = BranchId;
            ViewBag.BankAccountId = BankAccountId;
            return PartialView();
        }
        public PartialViewResult _AddNewAccount(int? BranchId, int? BankAccountId)
        {
            BankAccountListModelView model = new BankAccountListModelView();
            model.BranchId = Convert.ToInt32(BranchId);
            if (BankAccountId > 0)
            {
                BankAccountList account = db.BankAccountLists.Find(BankAccountId);
                model.BankAccountId = account.BankAccountId;
                model.Acc_Type = account.Acc_Type;
                model.AccountName = account.AccountName;
                model.AccountNumber = account.AccountNumber;
                model.Balance = account.Balance;
                model.HighestAmntPerTansaction = account.HighestAmntPerTansaction;
                model.NoOfTransaction = account.NoOfTransaction;
                model.TotalCreditLimit = Convert.ToInt32(account.TotalCreditLimit);
                model.Description = account.Description;
            }
            return PartialView(model);
        }
        public JsonResult AddNewAccount(BankAccountListModelView model, int? BankAccountId)
        {
            string msg = "";
            int ColumnId = 0;
            int OperationStatus = 1;
            BankAccountList bank = new BankAccountList();
            if (model.BankAccountId > 0)
            {
                bank = db.BankAccountLists.Find(BankAccountId);
            }
            bank.AccountName = model.AccountName;
            bank.AccountNumber = model.AccountNumber;
            bank.BranchId = model.BranchId;
            bank.Acc_Type = model.Acc_Type;
            bank.Description = model.Description;
            bank.Balance = model.Balance;
            bank.HighestAmntPerTansaction = model.HighestAmntPerTansaction;
            bank.NoOfTransaction = model.NoOfTransaction;
            bank.TotalCreditLimit = model.TotalCreditLimit;
            if (model.BankAccountId > 0)
            {
                bank.BankAccountId = model.BankAccountId;
                bank.UpdatedBy = model.CreatedBy;
                bank.UpdatedDate = now;
                db.Entry(bank).State = EntityState.Modified;
                ColumnId = Convert.ToInt32(BankAccountId);
                try
                {
                    db.SaveChanges();
                    msg = "Bank account" + " '" + model.AccountName + "' " + "has been successfully updated on " + now.ToString("MMM dd , yyy hh:mm tt") + " .";
                }
                catch (Exception ex)
                {
                    string errorMsg = ex.Message.ToString();
                    msg = "Bank account" + " '" + model.AccountName + "' " + "  update unsuccessful on " + now.ToString("MMM dd , yyy hh:mm tt") + " .";
                    OperationStatus = -1;
                }
                SaveAuditLog("Update New Account", msg, "Finance", "BankAccounts", "BankAccountList", ColumnId, bank.CreatedBy, now, OperationStatus);
            }
            else
            {
                bank.CreatedBy = model.CreatedBy;
                bank.CreatedDate = now;
                db.BankAccountLists.Add(bank);
                try
                {
                    BankBranchList bankList = db.BankBranchLists.Where(m => m.BranchId == model.BranchId).SingleOrDefault();
                    bankList.Status = 1;
                    db.Entry(bankList).State = EntityState.Modified;
                    db.SaveChanges();
                    int UserId = db.BankAccountLists.Where(m => m.CreatedBy == model.CreatedBy).Max(m => m.BankAccountId);
                    ColumnId = UserId;
                    msg = "New account" + " '" + model.AccountName + "' " + "has been successfully added on " + now.ToString("MMM dd , yyy hh:mm tt") + " .";
                }
                catch (Exception ex)
                {
                    string errorMsg = ex.Message.ToString();
                    msg = "New account" + " " + model.AccountName + " " + "  add unsuccessful on " + now.ToString("MMM dd , yyy hh:mm tt") + " .";
                    OperationStatus = -1;
                }
                SaveAuditLog("Add New Account", msg, "Finance", "AddNewAccount", "BankAccountList", ColumnId, bank.CreatedBy, now, OperationStatus);
            }
            if (OperationStatus == 1)
            {
                return Json("Success", JsonRequestBehavior.AllowGet);
            }
            return Json("Error", JsonRequestBehavior.AllowGet);
        }
        public PartialViewResult AccountList(int? pageNum, int Id, int? forDisplay)
        {
            pageNum = pageNum ?? 0;
            int page = Convert.ToInt32(pageNum);
            ViewBag.pageNum = pageNum;
            int ForDisplay = Convert.ToInt32(forDisplay);
            ViewBag.Display = ForDisplay;
            var account = (from m in db.BankAccountLists
                           where m.BranchId == Id
                           select new BankAccountListModelView
                           {
                               BankAccountId = m.BankAccountId,
                               AccountName = m.AccountName,
                               AccountNumber = m.AccountNumber,
                               ForAccountDisplay = ForDisplay
                           }).ToList();
            ViewBag.TotalAccount = account.Count();
            Session["AllAccount"] = account;
            Session["TotalAccountCount"] = account.Count();
            return PartialView();
        }
        public ActionResult _AccountList(int? BankAccountId, int? forDisplay)
        {
            int Display = Convert.ToInt32(forDisplay);
            if (BankAccountId != null)
            {
                var allAccount = (List<BankAccountListModelView>)Session["AllAccountBySearch"];
                var allAccountBySearch = (from m in allAccount
                                          where m.BankAccountId == BankAccountId
                                          select new BankAccountListModelView
                                          {
                                              BankAccountId = m.BankAccountId,
                                              AccountName = m.AccountName,
                                              AccountNumber = m.AccountNumber,
                                              ForAccountDisplay = Display
                                          }).ToList();
                ViewBag.TotalAccount = allAccountBySearch.Count();
                ViewBag.AccountList = allAccountBySearch.Take(ShowAccountPerPage).ToList();
            }
            else
            {
                var allAccount = (List<BankAccountListModelView>)Session["AllAccount"];


                if (allAccount != null)
                {
                    ViewBag.AccountList = allAccount.Take(ShowAccountPerPage).ToList();
                }
            }
            return PartialView(ViewBag.AccountList);
        }
        public JsonResult GetAccountList(int? page)
        {
            page = page ?? 0;
            int total = Convert.ToInt16(Session["TotalAccountCount"]);
            int skip = ShowAccountPerPage * Convert.ToInt16(page);
            var canPage = skip < total;
            if (canPage)
            {
                var listBackFromSession = (List<BankAccountListModelView>)Session["AllAccount"];
                var AccountList = listBackFromSession.Skip(skip).Take(ShowAccountPerPage).ToList();
                return Json(AccountList, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json("", JsonRequestBehavior.AllowGet);
            }
        }
        public JsonResult SearchAccountList(string prefix)
        {
            if (prefix == "")
            {
                return Json("", JsonRequestBehavior.AllowGet);
            }
            else
            {
                var allAccount = (List<BankAccountListModelView>)Session["AllAccount"];
                var account = (from s in allAccount
                               where (s.AccountName.ToLower().Contains(prefix.ToLower()) || s.AccountNumber.ToLower().Contains(prefix.ToLower()))
                               select new BankAccountListModelView
                               {
                                   BankAccountId = s.BankAccountId,
                                   AccountName = s.AccountName,
                                   AccountNumber = s.AccountNumber
                               }).ToList();
                Session["AllAccountBySearch"] = account;
                return Json(account, JsonRequestBehavior.AllowGet);
            }
        }
        [EncryptedActionParameter]
        public ActionResult BankAccountHistory(int BankAccountId)
        {
            BankAccountListModelView model = new BankAccountListModelView();
            BankAccountList account = db.BankAccountLists.Find(BankAccountId);
            model.BankAccountId = Convert.ToInt32(BankAccountId);
            model.AccountName = account.AccountName;
            model.AccountNumber = account.AccountNumber;
            model.BranchId = account.BranchId;

            model.HighestAmntPerTansaction = account.HighestAmntPerTansaction;
            model.Balance = account.Balance;
            model.NoOfTransaction = account.NoOfTransaction;
            model.TotalCreditLimit = Convert.ToInt32(account.TotalCreditLimit);
            model.Description = account.Description;
            return View(model);
        }
        public PartialViewResult DisplayAccountInfo(int BankAccountId)
        {
            BankAccountListModelView model = new BankAccountListModelView();
            BankAccountList account = db.BankAccountLists.Find(BankAccountId);
            model.BankAccountId = account.BankAccountId;
            model.Acc_Type = account.Acc_Type;
            model.AccountTypeName = (account.Acc_Type == 1) ? "Debit" : "Credit";
            model.AccountName = account.AccountName;
            model.AccountNumber = account.AccountNumber;
            model.BranchId = account.BranchId;
            model.HighestAmntPerTansaction = account.HighestAmntPerTansaction;
            model.Balance = account.Balance;
            model.NoOfTransaction = account.NoOfTransaction;
            model.TotalCreditLimit = Convert.ToInt32(account.TotalCreditLimit);
            model.Description = account.Description;
            return PartialView(model);
        }
        public JsonResult CheckAccountExist(int BankAccountId)
        {
            if (db.AllSalaryPayLists.Where(m => m.BankAccId == BankAccountId).Any() || db.AllUserLoanLists.Where(m => m.BankAccId == BankAccountId).Any())
            {
                return Json(true, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(false, JsonRequestBehavior.AllowGet);
            }
        }
        public JsonResult DeleteAccount(int BankAccountId, long CreatedBy)
        {
            string msg = "";
            int ColumnId = 0;
            int OperationStatus = 1;
            if (!db.BankAccountLists.Where(m => m.BankAccountId == BankAccountId).Any())
            {
                return Json("Error", JsonRequestBehavior.AllowGet);
            }
            else
            {
                BankAccountList account = db.BankAccountLists.Find(BankAccountId);
                int Id = account.BranchId;
                var branch = db.BankBranchLists.Where(m => m.BranchId == account.BranchId).FirstOrDefault();
                string bank = db.BankLists.Where(m => m.BankId == branch.BankId).Select(m => m.BankName).FirstOrDefault();
                db.BankAccountLists.Remove(account);
                try
                {
                    db.SaveChanges();
                    if (!db.BankAccountLists.Where(m => m.BranchId == branch.BranchId).Any())
                    {
                        BankBranchList bankList = db.BankBranchLists.Where(m => m.BranchId == branch.BranchId).SingleOrDefault();
                        bankList.Status = 0;
                        db.Entry(bankList).State = EntityState.Modified;
                        db.SaveChanges();
                    }
                    OperationStatus = 1;
                    msg = "Information of account '" + account.AccountName + "' of branch '" + branch.BranchName + "' of bank '" + bank + "' has been successfully deleted on " + now.ToString("MMM dd , yyy hh:mm tt") + " .";
                }
                catch (Exception ex)
                {
                    string errmsg = ex.ToString();
                    OperationStatus = -1;
                    msg = "Information of account '" + account.AccountName + "' of branch '" + branch.BranchName + "' of bank '" + bank + "' delete unsuccessful on " + now.ToString("MMM dd , yyy hh:mm tt") + " .";
                }
                ColumnId = Convert.ToInt32(BankAccountId);
                SaveAuditLog("Delete Bank Account", msg, "Finance", "BankAccount", "BankAccountList", ColumnId, CreatedBy, now, OperationStatus);
                if (OperationStatus == 1)
                {
                    return Json(db.BankAccountLists.Where(m => m.BranchId == Id).Any(), JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json("Error", JsonRequestBehavior.AllowGet);
                }
            }
        }

        //****************** Account Info Display **************

        //public PartialViewResult DisplayAccountHistory(int? BankAccountId)
        //{
        //    BankAccountListModelView model = new BankAccountListModelView();
        //    BankAccountList account = db.BankAccountLists.Find(BankAccountId);
        //    model.BankAccountId = Convert.ToInt32(BankAccountId);
        //    model.AccountName = account.AccountName;
        //    model.AccountNumber = account.AccountNumber;
        //    model.BranchId = account.BranchId;
        //    model.HighestAmntPerTansaction = account.HighestAmntPerTansaction;
        //    model.Balance = account.Balance;
        //    model.NoOfTransaction = account.NoOfTransaction;
        //    model.TotalCreditLimit = Convert.ToInt32(account.TotalCreditLimit);
        //    model.Description = account.Description;
        //    return PartialView(model);
        //}

        //*********************Bank Only Display**************
        [EncryptedActionParameter]
        public PartialViewResult _TransactionforBankAccount(int? BankAccountId)
        {
            var list = db.View_Voucher_Transaction.Where(m => m.BankAccId == BankAccountId).OrderByDescending(m => m.CreatedDate).ToList();
            Session["AllVoucherTranShowBank"] = list;
            Session["TotalVoucherTranShowBank"] = list.Count();
            ViewBag.Count = list.Count();
            list = list.Take(VoucherTranshowBank).ToList();
            return PartialView(list);
        }
        public JsonResult GetVouTranForBank(int? page)
        {
            page = page ?? 0;
            int total = Convert.ToInt16(Session["TotalVoucherTranShowBank"]);
            int skip = VoucherTranshowBank * Convert.ToInt16(page);
            var canPage = skip < total;
            if (canPage)
            {
                var listBackFromSession = (List<View_Voucher_Transaction>)Session["AllVoucherTranShowBank"];
                var partslist = listBackFromSession.Skip(skip).Take(VoucherTranshowBank).ToList();
                return Json(partslist, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json("", JsonRequestBehavior.AllowGet);
            }
        }
        [EncryptedActionParameter]
        public PartialViewResult DisplaySalaryPay(int? BankAccountId)
        {
            var model = db.View_All_Salary_Pay_His.Where(m => m.BankAccId == BankAccountId).OrderByDescending(m => m.InsertedDate).ToList();
            Session["AllSalaryPayShowBank"] = model;
            Session["TotalSalaryPayShowBank"] = model.Count();
            ViewBag.Count = model.Count();
            model = model.Take(ShowSalaryPayBankPerPage).ToList();
            return PartialView(model);
        }
        public JsonResult GetSalaryPayForBank(int? page1)
        {
            page1 = page1 ?? 0;
            int total = Convert.ToInt16(Session["TotalSalaryPayShowBank"]);
            int skip = ShowSalaryPayBankPerPage * Convert.ToInt16(page1);
            var canPage = skip < total;
            if (canPage)
            {
                var listBackFromSession = (List<View_All_Salary_Pay_His>)Session["AllSalaryPayShowBank"];
                var partslist = listBackFromSession.Skip(skip).Take(VoucherTranshowBank).ToList();
                return Json(partslist, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json("", JsonRequestBehavior.AllowGet);
            }
        }

        public PartialViewResult DisplayLoanList(int? BankAccountId)
        {
            var model = db.View_All_Loan_History.Where(m => m.BankAccId == BankAccountId).OrderByDescending(m => m.LoanPaidDate).ToList();
            Session["AllLoanHistoryShowBank"] = model;
            Session["TotalLoanHistoryShowBank"] = model.Count();
            ViewBag.Count = model.Count();
            model = model.Take(ShowLoanHistoryBankPerPage).ToList();
            return PartialView(model);
        }
        public JsonResult GetLoanHistoryForBank(int? page2)
        {
            page2 = page2 ?? 0;
            int total = Convert.ToInt16(Session["TotalLoanHistoryShowBank"]);
            int skip = ShowLoanHistoryBankPerPage * Convert.ToInt16(page2);
            var canPage = skip < total;
            if (canPage)
            {
                var listBackFromSession = (List<View_All_Loan_History>)Session["AllLoanHistoryShowBank"];
                var partslist = listBackFromSession.Skip(skip).Take(ShowLoanHistoryBankPerPage).ToList();
                return Json(partslist, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json("", JsonRequestBehavior.AllowGet);
            }
        }
        #endregion

        #region
        public JsonResult TransactionReceiveCalculation(int? id, int? bankId, int? accId, int? voucherType, long CreatedBy, double amount)
        {
            if (bankId != null)
            {
                BankAccountList bank = db.BankAccountLists.Where(m => m.BankAccountId == bankId).FirstOrDefault();
                var newAmount = Convert.ToInt32(amount);
                if (voucherType == 1 || voucherType == 4)
                {
                    bank.Balance = bank.Balance + newAmount;
                }
                else
                {
                    bank.Balance = bank.Balance - newAmount;
                }
                db.Entry(bank).State = EntityState.Modified;
            }
            else
            {
                AccountName account = db.AccountNames.Where(m => m.AccId == accId).FirstOrDefault();
                var newAmount = Convert.ToInt32(amount);
                if (voucherType == 1 || voucherType == 4)
                {
                    account.Balance = account.Balance + newAmount;
                }
                else
                {
                    account.Balance = account.Balance - newAmount;
                }
                db.Entry(account).State = EntityState.Modified;
            }
            Voucher_Transaction_List voucher = db.Voucher_Transaction_List.Where(m => m.Id == id).FirstOrDefault();
            voucher.ApproveStatus = 2;
            voucher.ReceivedBy = CreatedBy;
            voucher.ReceivedDate = now;
            db.Entry(voucher).State = EntityState.Modified;
            db.SaveChanges();
            return Json("Success", JsonRequestBehavior.AllowGet);
        }
        #endregion

        //******************************* Internal Account Salary Pay*****************************************
        [EncryptedActionParameter]
        public PartialViewResult AccountSalaryPay(int? AccId)
        {
            var model = db.View_All_Salary_Pay_His.Where(m => m.InernalAccId == AccId).OrderByDescending(m => m.InsertedDate).ToList();
            Session["AllSalaryPayShowAcc"] = model;
            Session["TotalSalaryPayShowAcc"] = model.Count();
            ViewBag.Count = model.Count();
            model = model.Take(ShowSalaryPayBankPerPage).ToList();
            return PartialView(model);
        }
        public JsonResult GetSalaryPayForAccount(int? page1)
        {
            page1 = page1 ?? 0;
            int total = Convert.ToInt16(Session["TotalSalaryPayShowAcc"]);
            int skip = ShowSalaryPayBankPerPage * Convert.ToInt16(page1);
            var canPage = skip < total;
            if (canPage)
            {
                var listBackFromSession = (List<View_All_Salary_Pay_His>)Session["AllSalaryPayShowAcc"];
                var partslist = listBackFromSession.Skip(skip).Take(ShowSalaryPayBankPerPage).ToList();
                return Json(partslist, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json("", JsonRequestBehavior.AllowGet);
            }
        }
        public PartialViewResult _AccountSalaryPayListDayWise(int days)
        {
            DateTime countDate = DateTime.Today;
            var model = db.View_All_Salary_Pay_His.OrderByDescending(m => m.InsertedDate).ToList();
            int day = Convert.ToInt32(days);
            if (day > 1)
            {
                countDate = countDate.AddDays(-(day));
            }
            if (day > 0)
            {
                model = model.Where(m => m.InsertedDate > countDate).OrderByDescending(m => m.InsertedDate).ToList(); ;
            }

            Session["AllSalaryPayShowAcc"] = model;
            Session["TotalSalaryPayShowAcc"] = model.Count();
            ViewBag.Count = model.Count();
            model = model.Take(ShowSalaryPayBankPerPage).ToList();
            return PartialView("AccountSalaryPay", model);
        }
        public PartialViewResult AccountLoanHistory(int? AccId)
        {
            var model = db.View_All_Loan_History.Where(m => m.InernalAccId == AccId).OrderByDescending(m => m.LoanPaidDate).ToList();
            Session["AllLoanHistoryShowAcc"] = model;
            Session["TotalLoanHistoryShowAcc"] = model.Count();
            ViewBag.Count = model.Count();
            model = model.Take(ShowLoanHistoryAccPerPage).ToList();
            return PartialView(model);
        }
        public JsonResult GetLoanHistoryForAccount(int? page2)
        {
            page2 = page2 ?? 0;
            int total = Convert.ToInt16(Session["TotalLoanHistoryShowAcc"]);
            int skip = ShowLoanHistoryAccPerPage * Convert.ToInt16(page2);
            var canPage = skip < total;
            if (canPage)
            {
                var listBackFromSession = (List<View_All_Loan_History>)Session["AllLoanHistoryShowAcc"];
                var partslist = listBackFromSession.Skip(skip).Take(ShowLoanHistoryAccPerPage).ToList();
                return Json(partslist, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json("", JsonRequestBehavior.AllowGet);
            }
        }
        public PartialViewResult AccLoanHistory(int id)
        {
            var model = db.View_All_Loan_History.Where(m => m.LoanId == id).FirstOrDefault();
            return PartialView(model);
        }
        public PartialViewResult _AccountLoanHistoryListDayWise(int days)
        {
            DateTime countDate = DateTime.Today;
            var model = db.View_All_Loan_History.OrderByDescending(m => m.LoanPaidDate).ToList();
            int day = Convert.ToInt32(days);
            if (day > 1)
            {
                countDate = countDate.AddDays(-(day));
            }
            if (day > 0)
            {
                model = model.Where(m => m.LoanPaidDate > countDate).OrderByDescending(m => m.LoanPaidDate).ToList(); ;
            }

            Session["AllLoanHistoryShowAcc"] = model;
            Session["TotalLoanHistoryShowAcc"] = model.Count();
            ViewBag.Count = model.Count();
            model = model.Take(ShowLoanHistoryAccPerPage).ToList();
            return PartialView("AccountLoanHistory", model);
        }
        //*******************************************************************************************

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

        public class AllDepList
        {
            public int Id { get; set; }
            public string IdWithChar { get; set; }
            public string Name { get; set; }
        }
    }
}