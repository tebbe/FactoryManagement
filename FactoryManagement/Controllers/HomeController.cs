using FactoryManagement.Helpers;
using FactoryManagement.Models;
using FactoryManagement.ModelView;
using FactoryManagement.ModelView.CRM;
using FactoryManagement.ModelView.HR;
using FactoryManagement.ModelView.Inventory;
using FactoryManagement.ModelView.Inventory.Product;
using FactoryManagement.ModelView.Inventory.Store;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Xml;
using System.Xml.Linq;

namespace FactoryManagement.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        #region Private Properties
        private FactoryManagementEntities db = new FactoryManagementEntities();
        static private int offset = Convert.ToInt32(ConfigurationManager.AppSettings["localTime"]);
        DateTime now = DateTime.UtcNow.AddMinutes(offset);
        const int RecentImpHisCount = 10;
        #endregion

        public ActionResult Dashboard()
        {

            HttpCookie cookie = HttpContext.Request.Cookies.Get("CookieAdminInfo");
            if (cookie == null)
            {
                return RedirectToAction("Login", "Account");
            }
            int roleId = Convert.ToInt32(cookie.Values["roleid"].ToString());
            ViewBag.userid = Convert.ToInt32(cookie.Values["userid"].ToString());
            ViewBag.userfirstname = cookie.Values["userfirstname"].ToString();
            ViewBag.userpic = cookie.Values["userpic"].ToString();
            ViewBag.username = cookie.Values["username"].ToString();
           
            if (roleId == 5 || roleId == 2)
            {
                return RedirectToAction("InvenDashboard", "Home");
            }
            if (roleId == 7)
            {
                return RedirectToAction("EquipDash", "Home");
            }
            if (roleId == 8 || roleId == 9 || roleId == 10)
            {
                return RedirectToAction("Store", "StoreProduct");
            }
            if (roleId == 11 || roleId == 12)
            {
                return RedirectToAction("ShowAllIndentVoucher", "Acquisition");
            }
            return View();
        }
        public PartialViewResult _AllRecentLoanList()
        {
            var user = db.View_All_Loan_History.OrderByDescending(o => o.LoanPaidDate).ToList().Take(20);
            return PartialView(user);
        }

        //************************** All Acquisition ***************************************
        #region
        public PartialViewResult _AllAquiList()
        {
            var list = db.Acquisition_List.ToList();
            return PartialView(list);
        }
        #endregion

        //************************** All Inventory *****************************************
        #region
        public PartialViewResult _AllInvenList()
        {
            var allproductType = (from i in db.View_All_InventoryList
                                  select new ProductTypeViewModel
                                  {
                                      ProductTypeName = i.ProductType,
                                      ProductTypeId = i.ProductTypeId
                                  }).GroupBy(g => g.ProductTypeId).Select(s => s.FirstOrDefault()).OrderBy(m => m.ProductTypeName).ToList();

            List<ProductTypeViewModel> typeList = new List<ProductTypeViewModel>();
            foreach (var m in allproductType)
            {
                if (m.HasParent)
                {
                    ProductTypeList model = db.ProductTypeLists.Find(m.ParentId);
                    if (!typeList.Where(p => p.ProductTypeId == model.ProductTypeId).Any())
                    {
                        typeList.Add(new ProductTypeViewModel
                        {
                            ProductTypeId = model.ProductTypeId,
                            ProductTypeName = (model.ProductTypeName)
                        });
                    }

                }
                else
                {
                    if (!typeList.Where(p => p.ProductTypeId == m.ProductTypeId).Any())
                    {
                        typeList.Add(new ProductTypeViewModel
                        {
                            ProductTypeName = (m.ProductTypeName),
                            ProductTypeId = m.ProductTypeId
                        });
                    }

                }

            }
            ViewBag.ProductTypeId = new SelectList(typeList, "ProductTypeId", "ProductTypeName");
            return PartialView();
        }
        public PartialViewResult _InvenList(int? ProductTypeId)
        {
            if (ProductTypeId == null) {
                var list = db.View_All_InventoryList.OrderBy(m => m.ProductName).ToList();
                return PartialView(list);
            }
            else
            {
                var list = db.View_All_InventoryList.Where(m => m.ProductTypeId == ProductTypeId).OrderBy(m => m.ProductName).ToList();
                return PartialView(list);
            }
        }
        #endregion

        //************************** All Business Order ************************************
        #region
        public PartialViewResult _AllBusList()
        {
            var list = db.View_Busi_Details_ForClient.Where(m => m.OrderType == 1).ToList();
            var allSuplist = db.View_Busi_Details_ForClient.Where(m => m.OrderType == 2).ToList();
            ViewBag.allSuplist = allSuplist;
            return PartialView(list);
        }
        public ActionResult OrderDetails(int? BusinessOrderId, int OrderType)
        {
            BusinessOrderListModelView model = new BusinessOrderListModelView();
            if (BusinessOrderId == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            else
            {
                model = (from m in db.View_Busi_Details_ForClient
                         where m.BusinessOrderId == BusinessOrderId && m.OrderType == 1
                         select new BusinessOrderListModelView
                         {
                             BusinessOrderId = m.BusinessOrderId,
                             OrderName = m.OrderName,
                             OrderStatus = m.OrderStatus,
                             ClientId = m.ClientId,
                             ClientName = m.ClientName,
                             OrderType = m.OrderType,
                             OrderAprvStatus = m.OrderAprvStatus
                         }).FirstOrDefault();
                ViewBag.AllFileList = db.Business_Order_FileLists.Where(m => m.BusinessOrderId == BusinessOrderId).OrderBy(m => m.CreatedDate).ToList();
                if (db.Acquisition_List.Where(m => m.BusinessOrderId == model.BusinessOrderId && m.AcquisitionType == 2).Any())
                {
                    model.AcquisitionOrderId = db.Acquisition_List.Where(m => m.BusinessOrderId == model.BusinessOrderId && m.AcquisitionType == 2)
                                              .Select(m => m.AcquisitionOrderId).FirstOrDefault();
                }
            }
            return PartialView(model);
        }
        #endregion

        //************************** All Daily Raw Cotton **********************************
        #region
        public PartialViewResult _DailyRowCotton()
        {
            var lists = db.View_All_InventoryList.Where(m => m.ProductTypeId == 6).ToList();
            var allInvenId = db.View_All_InventoryList.Where(m => m.ProductTypeId == 6).Select(m => m.InventoryId).ToList();

            var allItemList = db.Inventory_Item_Location.Where(m => allInvenId.Contains(m.InventoryId) && m.ReceivedDate > DateTime.Today).ToList();
            var list = (from m in db.Inventory_Item_Location
                        join v in db.View_All_InventoryList
                        on m.InventoryId equals v.InventoryId
                        where allInvenId.Contains(m.InventoryId) && m.ReceivedDate > DateTime.Today
                        select new InventoryListModelView
                        {
                            InventoryId = m.InventoryId,
                            ProductName = v.ProductName,
                            Description = v.Description,
                            lcno =m.LastOrderId,
                            Recv_Quan =(m.Quantity).ToString(),
                            UnitName = v.UnitName
                        }).ToList();
            DateTime countDate = DateTime.Today;
            countDate = countDate.AddDays(-7);
            ViewBag.seven = (from m in db.Inventory_Item_Location
                        join v in db.View_All_InventoryList
                        on m.InventoryId equals v.InventoryId
                        where allInvenId.Contains(m.InventoryId) && m.ReceivedDate > countDate
                        select new InventoryListModelView
                        {
                            InventoryId = m.InventoryId,
                            ProductName = v.ProductName,
                            Description = v.Description,
                            lcno = m.LastOrderId,
                            Recv_Quan = (m.Quantity).ToString(),
                            UnitName = v.UnitName
                        }).ToList();
            ViewBag.all = (from m in db.Inventory_Item_Location
                         join v in db.View_All_InventoryList
                         on m.InventoryId equals v.InventoryId
                         where allInvenId.Contains(m.InventoryId) && m.ReceivedDate != null
                         select new InventoryListModelView
                         {
                             InventoryId = m.InventoryId,
                             ProductName = v.ProductName,
                             Description = v.Description,
                             lcno = m.LastOrderId,
                             Recv_Quan = (m.Quantity).ToString(),
                             UnitName = v.UnitName
                         }).ToList();
            return PartialView(list);
        }
        #endregion

        //************************** All Recent Transaction ********************************
        #region
        public PartialViewResult _RecentTransaction()
        {
            var list = db.View_All_Salary_Pay_His.Where(m => m.IsRevert == false).OrderByDescending(x => x.InsertedDate).Take(20).ToList();
            return PartialView(list);
        }
        #endregion

        //************************** All To Do List ****************************************
        #region
        public PartialViewResult _ToDoList()
        {
            return PartialView();
        }
        #endregion


        //************************** Inventory Dashboard ****************************************
        #region
        public ActionResult InvenDashboard()
        {
            return View();
        }
        public PartialViewResult _RecentImpList()
        {
            var list = db.View_Import_Item_Details.ToList();
            list = list.Take(RecentImpHisCount).ToList();
            return PartialView(list);
        }
        public PartialViewResult _RecentDisList()
        {
            var list = (from bs in db.View_Dispacth_Item_Details
                        group bs by bs.InventoryId into g
                        orderby g.Sum(x => x.Quantity)
                        select new View_Dispacth_Item_DetailsModelView
                        {
                            Id = g.FirstOrDefault().Id,
                            DispatchedId = g.FirstOrDefault().DispatchedId,
                            Name = g.FirstOrDefault().Name,
                            VoucherName = g.FirstOrDefault().VoucherName,
                            ProductName = g.FirstOrDefault().ProductName,
                            InventoryId = g.FirstOrDefault().InventoryId,
                            LocationId = g.FirstOrDefault().LocationId,
                            Quantity = g.FirstOrDefault().Quantity,
                            UnitId = g.FirstOrDefault().UnitId.Value,
                            UnitName = g.FirstOrDefault().UnitName,
                            ReturnQuantity = g.FirstOrDefault().ReturnQuantity,
                            OrderId = g.FirstOrDefault().OrderId,
                            MachineId = g.FirstOrDefault().MachineId,
                            Site_Unit_Id = g.FirstOrDefault().Site_Unit_Id,
                            DeptId = g.FirstOrDefault().DeptId,
                            LineId = g.FirstOrDefault().LineId,
                            DispatchedDate = g.FirstOrDefault().DispatchedDate.Value,
                            DispatchedBy = g.FirstOrDefault().DispatchedBy.Value,
                            DispatchedDateName = g.FirstOrDefault().DispatchedDateName,
                            DispatchedUserName = g.FirstOrDefault().DispatchedUserName,
                            DispatchedUserPic = g.FirstOrDefault().DispatchedUserPic,
                            Total_Quantity = g.Sum(x => x.Quantity),
                            Total_Return = g.Sum(x => x.ReturnQuantity.Value),
                            Total_UsedQuan = g.Sum(x => x.UsedQuan.Value)
                        }).OrderBy(o => o.DispatchedDate).ToList();
            list = list.Take(RecentImpHisCount).ToList();
            return PartialView(list);
        }
        public PartialViewResult _RecentStockList()
        {
            var list = db.View_All_InventoryList.Where(m=> m.Quantity > 0).OrderBy(m => m.ProductName).ToList();
            //list = list.Take(RecentImpHisCount).ToList();
            return PartialView(list);
        }
        public PartialViewResult _RecentFinishedList()
        {
            var list = db.View_Finished_Product_List.ToList();
            list = list.Take(RecentImpHisCount).ToList();
            return PartialView(list);
        }
        #endregion

        //************************** Equipment Dashboard ****************************************
        #region
        public ActionResult EquipDash()
        {
            return View();
        }
        public PartialViewResult _CriticalMaintenance()
        {
            var list = db.View_Machine_Parts.Where(x=>x.Left_Day<=14).OrderBy(o => o.ReplaceDate).ToList();
            list = list.Take(20).ToList();
            return PartialView(list);
        }
        public PartialViewResult _AvailableMachine()
        {
            var list = db.View_Machine_Parts.OrderBy(o => o.ReplaceDate).ToList();
            list = list.Take(20).ToList();
            return PartialView(list);
        }
        public PartialViewResult _MachinVoucherList()
        {
            var list = db.View_Inven_Dis_Voucher_List.Where(m => m.Type == 2 && m.AssignType == 1).OrderByDescending(m => m.DispatchedDate).ToList();
            list = list.Take(20).ToList();
            return PartialView(list);
        }
        public PartialViewResult _DailyMaintenanceMain(int? machineId)
        {
            ViewBag.machineId = machineId;
            return PartialView();
        }
        public PartialViewResult _DailyMaintenance(int? type, int? machineId)
        {
            if (machineId > 0)
            {
                var list = db.View_Maintenance_Log.Where(m => m.MachineId == machineId).OrderBy(o => o.MaintenanceDate).Take(20).ToList();
                if (type > 0)
                {
                    list = list.Where(m => m.Type == type).ToList();
                }
                return PartialView(list);
            }
            else
            {
                DateTime countDate = now.Date;
                var list = db.View_Maintenance_Log.Where(m => m.MaintenanceDate > countDate).OrderBy(o => o.MaintenanceDate).Take(20).ToList();
                if (type > 0)
                {
                    list = list.Where(m => m.Type == type).ToList();
                }
                return PartialView(list);
            }
            
        }
        public PartialViewResult _UnitWiseMachine(int? uid,string v)
        {
            var list = db.View_Dispacth_Item_Details.Where(m => m.AssignType == 1 && m.MachineId > 0).OrderByDescending(m => m.DispatchedDate).ToList();
            list = (from bs in list
                    group bs by new { bs.MachineId, bs.InventoryId } into g
                    select new FactoryManagement.Models.View_Dispacth_Item_Details
                    {
                        Id = g.FirstOrDefault().Id,
                        DispatchedId = g.FirstOrDefault().DispatchedId,
                        Name = g.FirstOrDefault().Name,
                        MachineId = g.FirstOrDefault().MachineId,
                        MachineName = g.FirstOrDefault().MachineName,
                        ProductName = g.FirstOrDefault().ProductName,
                        InventoryId = g.FirstOrDefault().InventoryId,
                        LocationId = g.FirstOrDefault().LocationId,
                        UnitName = g.FirstOrDefault().UnitName,
                        Quantity = g.Sum(x => x.Quantity),
                        UsedQuan = g.Sum(x => x.UsedQuan),
                        ReturnQuantity = g.Sum(x => x.ReturnQuantity),
                    }).OrderBy(o => o.MachineName).ToList();
            if (uid > 0)
            {
                var alldeptId = db.DepartmentLists.Where(m => m.UnitId == uid).Select(m => m.DeptId).ToList();
                var allLine = db.LineInfoes.Where(m => alldeptId.Contains(m.DeptId)).Select(m => m.LineId).ToList();
                var allmachine = db.LineMachineLists.Where(m => allLine.Contains(m.LineId)).Select(m => m.MachineId).ToList();
                list = list.Where(m => allmachine.Contains(m.MachineId.Value)).ToList();
            }
            ViewBag.v = v;
            list = list.Take(20).ToList();
            return PartialView(list);
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
        public ActionResult AllButtonPage()
        {
            return View();
        }
        public JsonResult GetEncryptedData(string text)
        {
            return Json(MyExtensions.Encrypt(text), JsonRequestBehavior.AllowGet);
        }
    }
}