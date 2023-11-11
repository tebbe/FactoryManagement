using FactoryManagement.Filters;
using FactoryManagement.Models;
using FactoryManagement.ModelView.Acquisition;
using FactoryManagement.ModelView.Auth;
using FactoryManagement.ModelView.CRM;
using FactoryManagement.ModelView.Inventory;
using FactoryManagement.ModelView.Inventory.Mahinaries;
using FactoryManagement.ModelView.Inventory.Product;
using FactoryManagement.ModelView.Inventory.Store;
using FactoryManagement.ModelView.Inventory.Warehouse;
using FactoryManagement.ModelView.Report;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data.Entity;
using System.Data.Entity.SqlServer;
using System.Drawing;
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
    public class ReportController : Controller
    {
        #region Private Properties
        static private int offset = Convert.ToInt32(ConfigurationManager.AppSettings["localTime"]);
        private FactoryManagementEntities db = new FactoryManagementEntities();
        DateTime now = DateTime.UtcNow.AddMinutes(offset);
        const int PerPageProArch = 20;
        #endregion

        public ActionResult ReportDashboard() {
            return View();
        }
        //******************************************* DAILY RAW COTTON STOCK *********************************
        #region
        public ActionResult DailyRawCottonStock()
        {
            var list = db.InventoryLists.Where(m => m.ProductTypeId == 6).ToList();
            var country_code = (from bs in list
                                group bs by bs.Country into g
                                select new FactoryManagement.Models.CountryList
                                {
                                    CountryCode = g.FirstOrDefault().Country
                                }).ToList();
            return View(country_code);
        }
        #endregion

        // ************************************ Finished Product Report(Archive) *********************************
        #region
        public ActionResult FinishedProArchive()
        {
            return View();
        }
        public PartialViewResult _FinishedProArchive()
        {
            var list = (from p in db.Finished_Pro_Report_List
                        join f in db.Factory_Finished_Pro_Title on p.TitleId equals f.Id into ps
                        from m in ps.DefaultIfEmpty()
                        select new Finished_Pro_Report_ListModelView
                        {
                            Id = p.Id,
                            TitleId = p.TitleId,
                            Title = m.Title,
                            SubTitle = m.SubTitle,
                            TitleCreatedDate = SqlFunctions.DateName("dd", m.CreatedDate) + "-" + SqlFunctions.DateName("mm", m.CreatedDate) + "-" + SqlFunctions.DateName("year", m.CreatedDate),
                            Date = p.Date,
                            ReportName = p.ReportName,
                            CreatedDate = p.CreatedDate,
                            CreatedDateFormate = SqlFunctions.DateName("dd", p.CreatedDate) + "-" + SqlFunctions.DateName("mm", p.CreatedDate) + "-" + SqlFunctions.DateName("year", p.CreatedDate)
                        }).ToList();
            Session["Total_Finished_Archive"] = list;
            Session["Total_Finished_Archive_count"] = list.Count();
            ViewBag.TotalPro = list.Count();
            list = list.Take(20).ToList();
            ViewBag.ProTitleList = list;
            return PartialView();
        }
        public JsonResult GetProArch(int? page)
        {
            page = page ?? 0;
            int total = Convert.ToInt16(Session["TotalUserCount"]);
            int skip = PerPageProArch * Convert.ToInt16(page);
            var canPage = skip < total;
            var listBackFromSession = (List<Finished_Pro_Report_ListModelView>)Session["Total_Finished_Archive"];
            var list = listBackFromSession.Skip(skip).Take(PerPageProArch).ToList();
            return Json(list, JsonRequestBehavior.AllowGet);
        }
        [EncryptedActionParameter]
        public ActionResult ShowproArchiveDetails(long? id)
        {
            if (id > 0)
            {
                var report = db.Finished_Pro_Report_List.Find(id);
                if (report != null)
                {
                    return View(report);
                }
            }
            return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        }
        #endregion

        // ************************************ Daile Raw Cotton Report List *********************************
        #region
        public ActionResult RawCottonRepArchive()
        {
            return View();
        }
        public PartialViewResult _RawCottonRepArchive()
        {
            var list = (from m in db.Daily_Raw_Cotton_Report_List
                        select new Daily_Raw_Cotton_Report_List_Model_View
                        {
                            Id = m.Id,
                            CreatedDateFormat = SqlFunctions.DateName("dd", m.CreatedDate) + "-" + SqlFunctions.DateName("mm", m.CreatedDate) + "-" + SqlFunctions.DateName("year", m.CreatedDate),
                            Date = m.Date,
                            ReportName = m.ReportName,
                            CreatedDate = m.CreatedDate
                        }).ToList();
            Session["Total_RawCotton_Archive"] = list;
            Session["Total_RawCotton_Archive_count"] = list.Count();
            ViewBag.Totalrep = list.Count();
            list = list.Take(20).ToList();
            ViewBag.ReportList = list;
            return PartialView();
        }
        public JsonResult GetRawRepArch(int? page)
        {
            page = page ?? 0;
            int total = Convert.ToInt16(Session["TotalUserCount"]);
            int skip = PerPageProArch * Convert.ToInt16(page);
            var canPage = skip < total;
            var listBackFromSession = (List<Finished_Pro_Report_ListModelView>)Session["Total_Finished_Archive"];
            var list = listBackFromSession.Skip(skip).Take(PerPageProArch).ToList();
            return Json(list, JsonRequestBehavior.AllowGet);
        }
        [EncryptedActionParameter]
        public ActionResult ShowRawCottonArchiveDetails(long? id)
        {
            if (id > 0)
            {
                var report = db.Daily_Raw_Cotton_Report_List.Find(id);
                if (report != null)
                {
                    return View(report);
                }
            }
            return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        }
        #endregion

        #region
        [EncryptedActionParameter]
        public ActionResult AllGoodsListPrint(int id, int storeid, string catname)
        {
            ViewBag.storename = db.StoreLists.Where(m => m.Id == storeid).Select(m => m.StoreName).FirstOrDefault();
            ViewBag.catname = catname;
            ViewBag.id = id;
            ViewBag.storeid = storeid;
            return View(db.View_Inven_Item_Loc_Details.Where(m => m.StoreId == storeid && m.ProductTypeId == id).OrderBy(m => m.ProductName).ToList());
        }
        #endregion
    }
}