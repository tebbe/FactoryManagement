using FactoryManagement.Filters;
using FactoryManagement.Models;
using FactoryManagement.ModelView.Acquisition;
using FactoryManagement.ModelView.Configuration;
using FactoryManagement.ModelView.CRM;
using FactoryManagement.ModelView.Inventory;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;
using System.Web.Script.Serialization;

namespace FactoryManagement.Controllers
{
    [Authorize]
    [DisplayName("Business Relationship Management")]
    public class CRMController : Controller
    {

        #region Private Properties
        private FactoryManagementEntities db = new FactoryManagementEntities();
        static private int offset = Convert.ToInt32(ConfigurationManager.AppSettings["localTime"]);
        DateTime now = DateTime.UtcNow.AddMinutes(offset);
        const int ShowUserPerPage = 20;
        #endregion


        //*********************************  BUSSINESS ORDER  **************************************

        #region Business Order Create,Update,Delete

        public PartialViewResult BusinessOrderCreatePopUp(int? BusinessOrderId, int? OrderType)
        {
            ViewBag.BusinessOrderId = BusinessOrderId;
            ViewBag.OrderType = OrderType;
            return PartialView();
        }
        public PartialViewResult BusinessOrderCreate(int? BusinessOrderId, int? OrderType)
        {
            BusinessOrderListModelView model = new BusinessOrderListModelView();
            if (BusinessOrderId != null && BusinessOrderId > 0)
            {
                BusinessOrderList business = db.BusinessOrderLists.SingleOrDefault(m => m.BusinessOrderId == BusinessOrderId);
                model.BusinessOrderId = business.BusinessOrderId;
                model.OrderType = business.OrderType;
                model.OrderStatus = business.OrderStatus;
                model.OrderName = business.OrderName;
                model.CreatedBy = business.CreatedBy;
                model.CreatedDate = business.CreatedDate;
                model.ClientId = business.ClientId;
            }
            else
            {
                model.OrderType = (Int32)OrderType;
            }

            //ViewBag.OrderStatus = OrderStatus;
            ViewBag.ClientId = new SelectList(db.ClientLists.Where(m => m.IsBuyer == true).OrderBy(m => m.Name), "ClientId", "Name", model.ClientId);
            return PartialView(model);
        }
        public JsonResult GetClientList(int OrderType)
        {
            if (OrderType == 1)
            {
                var clientList = (from m in db.ClientLists
                                  where m.IsBuyer == true
                                  select new ClientListModelView
                                  {
                                      ClientId = m.ClientId,
                                      //ClientType = m.ClientType,
                                      Name = m.Name
                                  }).OrderBy(m => m.Name).ToList();

                return Json(clientList, JsonRequestBehavior.AllowGet);
            }
            else
            {
                var clientList = (from m in db.ClientLists
                                  where m.IsSupplier == true
                                  select new ClientListModelView
                                  {
                                      ClientId = m.ClientId,
                                      //ClientType = m.ClientType,
                                      Name = m.Name
                                  }).OrderBy(m => m.Name).ToList();

                return Json(clientList, JsonRequestBehavior.AllowGet);
            }
        }

        //********************* BUSINESS ORDER FROM BUYER ***********************************

        public ActionResult BusinessOrder()
        {
            return View();
        }
        public PartialViewResult _ShowAllBusinessOrder(int OrderType, int? OrderStatus)
        {
            var list = db.BusinessOrderLists.Where(m => m.OrderType == OrderType).ToList();
            if (OrderStatus != null)
            {
                list = list.Where(m => m.OrderStatus == OrderStatus).ToList();
            }
            ViewBag.list = list;
            ViewBag.OrderType = OrderType;
            ViewBag.OrderStatus = OrderStatus;
            return PartialView();
        }
        public PartialViewResult _ShowAllBusinessOrderDayWise(int days, int OrderType, int? OrderStatus)
        {
            var list = db.BusinessOrderLists.Where(m => m.OrderType == OrderType).OrderByDescending(m => m.CreatedDate).ToList();
            DateTime countDate = DateTime.Today;
            int day = Convert.ToInt32(days);
            if (day > 1)
            {
                countDate = countDate.AddDays(-(day));
            }
            if (day > 0)
            {
                if (OrderStatus != null)
                {
                    list = list.Where(m => m.OrderStatus == OrderStatus && m.CreatedDate > countDate).ToList();
                }
                else
                {
                    list = list.Where(m => m.CreatedDate > countDate).ToList();
                }

            }
            if (day == 0) //For All days
            {
                if (OrderStatus != null)
                {
                    list = list.Where(m => m.OrderStatus == OrderStatus).OrderByDescending(m => m.CreatedDate).ToList();
                }
            }
            ViewBag.list = list;
            ViewBag.OrderType = OrderType;
            ViewBag.OrderStatus = OrderStatus;
            return PartialView("_ShowAllBusinessOrder");
        }
        public PartialViewResult _ShowAllBusinessOrderDateWise(DateTime? fromDate, DateTime? toDate, int OrderType, int? OrderStatus)
        {
            var list = db.BusinessOrderLists.Where(m => m.OrderType == OrderType).OrderByDescending(m => m.CreatedDate).ToList();
            toDate = (toDate == null) ? toDate : toDate.Value.Date;
            if (OrderStatus == null)
            {
                list = (fromDate != null && toDate != null) ? list.Where(m => m.CreatedDate.Date >= fromDate.Value.Date && m.CreatedDate.Date <= toDate.Value.Date).ToList() : list.ToList();
                list = (fromDate != null && toDate == null) ? list.Where(m => m.CreatedDate.Date == fromDate.Value.Date).ToList() : list.ToList();
                list = (fromDate == null && toDate != null) ? list.Where(m => m.CreatedDate.Date == toDate.Value.Date).ToList() : list.ToList();
                list = (fromDate == null && toDate == null) ? list.ToList() : list.ToList();
            }
            else
            {
                list = list.Where(m => m.OrderStatus == OrderStatus).ToList();
                list = (fromDate != null && toDate != null) ? list.Where(m => m.CreatedDate.Date >= fromDate.Value.Date && m.CreatedDate.Date <= toDate.Value.Date).ToList() : list.ToList();
                list = (fromDate != null && toDate == null) ? list.Where(m => m.CreatedDate.Date == fromDate.Value.Date).ToList() : list.ToList();
                list = (fromDate == null && toDate != null) ? list.Where(m => m.CreatedDate.Date == toDate.Value.Date).ToList() : list.ToList();
                list = (fromDate == null && toDate == null) ? list.ToList() : list.ToList();
            }
            ViewBag.list = list;
            ViewBag.OrderType = OrderType;
            ViewBag.fromDate = (fromDate != null) ? fromDate.Value.ToString("MMMM dd yyyy") : "";
            ViewBag.toDate = (toDate != null) ? toDate.Value.ToString("MMMM dd yyyy") : "";
            ViewBag.OrderStatus = OrderStatus;
            return PartialView("_ShowAllBusinessOrder");
        }
        public JsonResult NewBusinessOrderSave(BusinessOrderListModelView model)
        {
            string msg = "";
            string opname = "New Business Order";
            int ColumnId = model.BusinessOrderId;
            int OperationStatus = 1;

            if (ModelState.IsValid)
            {
                BusinessOrderList business = new BusinessOrderList();
                if (model.BusinessOrderId > 0)
                {
                    business = db.BusinessOrderLists.Find(model.BusinessOrderId);
                    business.OrderName = model.OrderName;
                    business.ClientId = model.ClientId;
                    business.OrderStatus = 0;
                    business.UpdatedDate = now;
                    business.UpdatedBy = model.CreatedBy;
                    db.Entry(business).State = EntityState.Modified;
                    opname = "Update Business Order ";
                    msg = "Business order '" + business.OrderName + "' information has been successfully updated on " + now.ToString("MMM dd,yyyy hh:mm tt.");
                }
                else
                {
                    business.OrderName = model.OrderName;
                    business.OrderType = model.OrderType;
                    business.ClientId = model.ClientId;
                    business.OrderStatus = 0;
                    business.CreatedDate = now;
                    business.CreatedBy = model.CreatedBy;
                    db.BusinessOrderLists.Add(business);
                    msg = "New business order '" + business.OrderName + "' has been successfully created on " + now.ToString("MMM dd,yyyy hh:mm tt.");
                }

                try
                {
                    db.SaveChanges();
                    if (model.BusinessOrderId == 0)
                    {
                        ColumnId = db.BusinessOrderLists.Where(m => m.CreatedBy == model.CreatedBy).Max(m => m.BusinessOrderId);
                    }
                }
                catch (Exception ex)
                {
                    string errmsg = ex.ToString();
                    ColumnId = 0;
                    OperationStatus = -1;
                    if (model.BusinessOrderId > 0)
                    {
                        msg = "Business order '" + business.OrderName + "' information update was unsuccessful on " + now.ToString("MMM dd,yyyy hh:mm tt.");
                    }
                    else
                    {
                        msg = "New business order '" + business.OrderName + "' creation was unsuccessful on " + now.ToString("MMM dd,yyyy hh:mm tt.");
                    }
                }
                SaveAuditLog(opname, msg, "CRM", "Business Order", "BusinessOrderList", ColumnId, model.CreatedBy, now, OperationStatus);
                if (OperationStatus == 1)
                {
                    return Json("Success", JsonRequestBehavior.AllowGet);
                }
            }
            return Json("Error", JsonRequestBehavior.AllowGet);
        }
        [EncryptedActionParameter]
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
                         where m.BusinessOrderId == BusinessOrderId
                         && m.OrderType == OrderType
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
            return View(model);
        }
        public PartialViewResult _OrderNameSatatusDetails(int BusinessOrderId)
        {
            BusinessOrderListModelView model = new BusinessOrderListModelView();
            if (BusinessOrderId > 0)
            {
                View_Busi_Details_ForClient business = db.View_Busi_Details_ForClient.FirstOrDefault(m => m.BusinessOrderId == BusinessOrderId);
                model.BusinessOrderId = business.BusinessOrderId;
                model.OrderName = business.OrderName;
                model.ClientName = business.ClientName;
                model.OrderStatus = business.OrderStatus;
                model.OrderType = business.OrderType;
                model.OrderAprvStatus = business.OrderAprvStatus;
            }
            return PartialView(model);
        }
        public JsonResult OrderMainFileCheck(int BusinessOrderId, int FileType)
        {
            if (db.Business_Order_FileLists.Where(m => m.BusinessOrderId == BusinessOrderId && m.FileType == FileType && m.IsMainFile == true).Any())
            {
                return Json(true, JsonRequestBehavior.AllowGet);
            }
            return Json(false, JsonRequestBehavior.AllowGet);
        }
        public PartialViewResult _OderMainFileList(int BusinessOrderId, bool? IsPrint)
        {
            var mainFileLists = db.View_BusinessOrder_File.Where(m => m.BusinessOrderId == BusinessOrderId && m.IsMainFile == true).ToList();
            ViewBag.IsPrint = IsPrint;
            return PartialView(mainFileLists);
        }
        public PartialViewResult _OrderFileLists(int? BusinessOrderId, int FileType, bool? IsPrint)
        {
            ViewBag.IsPrint = IsPrint;
            return PartialView(db.View_BusinessOrder_File.Where(m => m.BusinessOrderId == BusinessOrderId && m.FileType == FileType).OrderByDescending(m => m.CreatedDate).ToList());
        }
        public ActionResult _OrderTimeLine(int? BusinessOrderId, bool? IsPrint)
        {
            if (BusinessOrderId != null)
            {
                var list = db.View_BusinessOrder_File.Where(m => m.BusinessOrderId == BusinessOrderId)
                            .GroupBy(m => m.CreatedDateName).Select(i => i.FirstOrDefault())
                            .OrderBy(m => m.CreatedDate).ToList();

                ViewBag.IsPrint = IsPrint;
                return PartialView(list);
            }
            return PartialView();
        }
        public JsonResult DeleteBusinessOrder(int BusinessOrderId)
        {
            string msg = "";
            int ColumnId = 0;
            int OperationStatus = 1;
            AuditInformation audit = new AuditInformation();
            if (!db.BusinessOrderLists.Where(m => m.BusinessOrderId == BusinessOrderId).Any())
            {
                return Json("Error", JsonRequestBehavior.AllowGet);
            }
            else
            {
                BusinessOrderList business = db.BusinessOrderLists.Find(BusinessOrderId);

                if (ModelState.IsValid)
                {
                    try
                    {
                        db.BusinessOrderLists.Remove(business);
                        db.SaveChanges();
                        audit.OperationStatus = 1;
                    }
                    catch (Exception ex)
                    {
                        string errmsg = ex.ToString();
                        audit.OperationStatus = -1;
                        msg = "Business order '" + business.OrderName + "' deletion was unsuccessfully on " + now.ToString("MMM dd,yyyy hh:mm tt.");
                    }
                    ColumnId = BusinessOrderId;
                    msg = "Business order '" + business.OrderName + "' has been successfully deleted on " + now.ToString("MMM dd,yyyy hh:mm tt.");
                    SaveAuditLog("Delete Business Order", msg, "CRM", "DeleteBusinessOrder", "BusinessOrderList", ColumnId, business.CreatedBy, now, OperationStatus);
                }
                return Json("Success", JsonRequestBehavior.AllowGet);
            }
        }
        public PartialViewResult SpecificationFileUploadEditPoPUp(int FileId)
        {
            //ViewBag.FileType = FileType;
            ViewBag.FileId = FileId;
            return PartialView();
        }
        public PartialViewResult SpecificationFileUploadPoPUp(int FileType)
        {
            ViewBag.FileType = FileType;
            return PartialView();
        }
        public PartialViewResult SpecificationFileUploadEdit(int FileId)
        {
            Business_Order_FileListsModelView model = new Business_Order_FileListsModelView();
            if (FileId > 0)
            {
                Business_Order_FileLists fileList = db.Business_Order_FileLists.Find(FileId);
                model.FileId = fileList.FileId;
                model.FileType = fileList.FileType;
                model.ClientId = (db.BusinessOrderLists.Where(m => m.BusinessOrderId == fileList.BusinessOrderId).Select(m => m.ClientId).FirstOrDefault()).Value;
                model.BusinessOrderId = fileList.BusinessOrderId;
                model.FileSendBy = fileList.FileSendBy;
                model.IsMainFile = fileList.IsMainFile;
                model.FileOriginalName = fileList.FileOriginalName;
                model.Client_User_Id = fileList.Client_User_Id;

                ViewBag.SelectedName = db.View_BusinessOrder_File.FirstOrDefault(m => m.FileId == fileList.FileId).ClientUserName;
            }
            return PartialView(model);
        }
        public PartialViewResult SpecificationFileUpload(int FileType, int orderId)
        {
            Business_Order_FileListsModelView model = new Business_Order_FileListsModelView();
            if (orderId > 0)
            {
                model.FileType = FileType;
                model.ClientId = (Int32)db.BusinessOrderLists.Where(m => m.BusinessOrderId == orderId).Select(m => m.ClientId).FirstOrDefault();
                model.BusinessOrderId = orderId;
            }
            return PartialView(model);
        }
        [EncryptedActionParameter]
        public ActionResult OrderDetailsPrint(int? BusinessOrderId, int OrderType)
        {
            BusinessOrderListModelView model = new BusinessOrderListModelView();
            if (BusinessOrderId == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            else
            {
                model = (from m in db.View_Busi_Details_ForClient
                         where m.BusinessOrderId == BusinessOrderId
                         && m.OrderType == OrderType
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
            return View(model);
        }
        public JsonResult GetUserName(int ClientId, int BusinessOrderId)
        {
            if (ClientId == 0)
            {
                var allSuplierId = db.BusinessOrderSupplierLists.Where(m => m.BusinessOrderId == BusinessOrderId)
                    .Select(m => m.ClientId).ToList();
                var model = (from m in db.Client_User_Lists
                             where allSuplierId.Contains(m.ClientId)
                             select new
                             {
                                 UserId = m.UserId,
                                 Name = m.UserName,
                                 Picture = m.PictureName
                             }).ToList();
                return Json(model, JsonRequestBehavior.AllowGet);
            }
            else
            {
                var model = (from m in db.Client_User_Lists
                             join u in db.ClientLists on m.ClientId equals u.ClientId
                             where m.ClientId == ClientId
                             select new
                             {
                                 UserId = m.UserId,
                                 Name = m.UserName,
                                 Picture = m.PictureName,
                                 ClientName = u.Name
                             }).ToList();
                return Json(model, JsonRequestBehavior.AllowGet);
            }

        }
        public JsonResult GetInhouseUser(int UserId)
        {
            var model = (from m in db.View_UserLists
                         where m.UserType == 1
                         select new
                         {
                             UserId = m.UserId,
                             Name = m.UserName,
                             Picture = m.Picture,
                             UserType = m.UserTypeName,
                             EmpId = m.EmpId
                         }).ToList();
            return Json(model, JsonRequestBehavior.AllowGet);
        }
        public ActionResult SpecificationFileSave(IEnumerable<HttpPostedFileBase> files)
        {
            return Content("");
        }
        public JsonResult FileEditSaveForSpecification(Business_Order_FileListsModelView model, IEnumerable<HttpPostedFileBase> files)
        {

            string msg = "";
            string fileType = "";
            int ColumnId = 0;
            int OperationStatus = -1;
            if (model.FileType == 1)
            {
                fileType = "Specification file '";
            }
            else if (model.FileType == 2)
            {
                fileType = "Negotiation file '";
            }
            else
            {
                fileType = "Finance file '";
            }
            JavaScriptSerializer serializer = new JavaScriptSerializer();
            List<string> fileNameList = serializer.Deserialize<List<string>>(model.AllFilename);
            if (files != null)
            {
                int i = 0;
                foreach (var file in files)
                {
                    if (file != null && file.ContentLength > 0)
                    {
                        Business_Order_FileLists businessFile = db.Business_Order_FileLists.SingleOrDefault(m => m.FileId == model.FileId);
                        var oldFilepath = Path.Combine(Server.MapPath("~/AllUploadedFile/BusinessOrderFiles/FileForSpecification/"), businessFile.FileName);
                        System.IO.File.Delete(oldFilepath);
                        int maxid = 0;
                        if (db.Business_Order_FileLists.Where(m => m.BusinessOrderId == model.BusinessOrderId).Any())
                        {
                            maxid = Convert.ToInt32(db.Business_Order_FileLists.Where(m => m.BusinessOrderId == model.BusinessOrderId).Max(m => m.FileId));
                        }
                        string fileName = fileNameList[i].ToString();
                        string s = fileName;
                        int idex = s.LastIndexOf('.');
                        fileName = s.Substring(0, idex);
                        string extension = s.Substring(idex);

                        string filefullName = model.BusinessOrderId + "" + model.Client_User_Id + "" + maxid + extension;
                        string Paths = Path.Combine(Server.MapPath("~/AllUploadedFile/BusinessOrderFiles/FileForSpecification/"), filefullName);
                        file.SaveAs(Paths);

                        businessFile.BusinessOrderId = model.BusinessOrderId;
                        businessFile.FileType = model.FileType;
                        businessFile.FileSendBy = model.FileSendBy;
                        businessFile.IsMainFile = model.IsMainFile;
                        businessFile.FileName = filefullName;
                        businessFile.FileOriginalName = fileNameList[i].ToString();
                        businessFile.FileContentType = file.ContentType;
                        if (model.Client_User_Id != null || model.Client_User_Id <= 0)
                        {
                            businessFile.Client_User_Id = model.Client_User_Id;
                        }
                        businessFile.UpatedDate = now;
                        businessFile.UpatedBy = model.UpatedBy;
                        try
                        {
                            db.SaveChanges();
                            OperationStatus = 1;
                            msg = fileType + businessFile.FileName + "' has been successfully updated on " + now.ToString("MMM dd,yyyy hh:mm tt.");
                            ColumnId = Convert.ToInt32(businessFile.FileId);
                            if (model.IsMainFile && db.Business_Order_FileLists.Where(m => m.BusinessOrderId == model.BusinessOrderId && m.FileId != ColumnId && m.IsMainFile == true && m.FileType == model.FileType).Any())
                            {
                                Business_Order_FileLists existsFile = db.Business_Order_FileLists
                                    .Where(m => m.BusinessOrderId == model.BusinessOrderId && m.FileId != ColumnId && m.IsMainFile == true && m.FileType == model.FileType)
                                    .FirstOrDefault();
                                existsFile.IsMainFile = false;
                                db.Entry(existsFile).State = EntityState.Modified;
                                db.SaveChanges();
                            }
                        }
                        catch (Exception ex)
                        {
                            string errmsg = ex.ToString();
                            OperationStatus = -1;
                            ColumnId = 0;
                            msg = fileType + businessFile.FileName + "' update was unsuccessful on " + now.ToString("MMM dd,yyyy hh:mm tt.");
                        }
                        SaveAuditLog("Update Business Order File ", msg, "CRM", "FileEditSaveForSpecification", "Business_Order_FileLists", ColumnId, (long)model.UpatedBy, now, OperationStatus);
                        i++;
                    }
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
                Business_Order_FileLists businessFile = db.Business_Order_FileLists.SingleOrDefault(m => m.FileId == model.FileId);
                string fileName = businessFile.FileOriginalName;
                string s = fileName;
                int idex = s.LastIndexOf('.');
                fileName = s.Substring(0, idex);
                string extension = s.Substring(idex);

                businessFile.BusinessOrderId = model.BusinessOrderId;
                businessFile.FileType = model.FileType;
                businessFile.FileSendBy = model.FileSendBy;
                businessFile.IsMainFile = model.IsMainFile;
                //businessFile.FileOriginalName = model.FileOriginalName + extension;
                if (model.Client_User_Id != null || model.Client_User_Id <= 0)
                {
                    businessFile.Client_User_Id = model.Client_User_Id;
                }
                businessFile.UpatedDate = now;
                businessFile.UpatedBy = model.UpatedBy;
                try
                {
                    db.SaveChanges();
                    OperationStatus = 1;
                    msg = fileType + businessFile.FileName + "' has been successfully updated on " + now.ToString("MMM dd,yyyy hh:mm tt.");
                    ColumnId = Convert.ToInt32(businessFile.FileId);
                    if (model.IsMainFile && db.Business_Order_FileLists.Where(m => m.BusinessOrderId == model.BusinessOrderId && m.FileId != ColumnId && m.IsMainFile == true && m.FileType == model.FileType).Any())
                    {
                        Business_Order_FileLists existsFile = db.Business_Order_FileLists
                            .Where(m => m.BusinessOrderId == model.BusinessOrderId && m.FileId != ColumnId && m.IsMainFile == true && m.FileType == model.FileType)
                            .FirstOrDefault();
                        existsFile.IsMainFile = false;
                        db.Entry(existsFile).State = EntityState.Modified;
                        db.SaveChanges();
                    }
                }
                catch (Exception ex)
                {
                    string errmsg = ex.ToString();
                    OperationStatus = -1;
                    ColumnId = 0;
                    msg = fileType + businessFile.FileName + "' update was unsuccessful on " + now.ToString("MMM dd,yyyy hh:mm tt.");
                }
                SaveAuditLog("Update Business Order File ", msg, "CRM", "FileEditSaveForSpecification", "Business_Order_FileLists", ColumnId, (long)model.UpatedBy, now, OperationStatus);
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
        public JsonResult FileUploadSaveForSpecification(Business_Order_FileListsModelView model, IEnumerable<HttpPostedFileBase> files)
        {
            string msg = "";
            int ColumnId = 0;
            int OperationStatus = -1;
            string fileType = "";
            if (model.FileType == 1)
            {
                fileType = "Specification file '";
            }
            else if (model.FileType == 2)
            {
                fileType = "Negotiation file '";
            }
            else
            {
                fileType = "Finance file '";
            }
            JavaScriptSerializer serializer = new JavaScriptSerializer();
            List<string> fileNameList = serializer.Deserialize<List<string>>(model.AllFilename);
            if (files != null)
            {
                int i = 0;
                foreach (var file in files)
                {
                    if (file != null && file.ContentLength > 0)
                    {
                        int maxid = 0;
                        if (db.Business_Order_FileLists.Where(m => m.BusinessOrderId == model.BusinessOrderId).Any())
                        {
                            maxid = Convert.ToInt32(db.Business_Order_FileLists.Where(m => m.BusinessOrderId == model.BusinessOrderId).Max(m => m.FileId));
                        }
                        string fileName = fileNameList[i].ToString();
                        string s = fileName;
                        int idex = s.LastIndexOf('.');
                        fileName = s.Substring(0, idex);
                        string extension = s.Substring(idex);

                        string filefullName = model.BusinessOrderId + "" + model.Client_User_Id + "" + maxid + extension;
                        string Paths = Path.Combine(Server.MapPath("~/AllUploadedFile/BusinessOrderFiles/FileForSpecification/"), filefullName);
                        file.SaveAs(Paths);

                        Business_Order_FileLists businessFile = new Business_Order_FileLists();
                        businessFile.BusinessOrderId = model.BusinessOrderId;
                        businessFile.FileType = model.FileType;
                        businessFile.FileSendBy = model.FileSendBy;
                        businessFile.IsMainFile = model.IsMainFile;
                        businessFile.FileName = filefullName;
                        businessFile.FileOriginalName = fileNameList[i].ToString();
                        businessFile.FileContentType = file.ContentType;
                        businessFile.Client_User_Id = model.Client_User_Id;
                        businessFile.CreatedDate = now;
                        businessFile.CreatedBy = model.CreatedBy;
                        try
                        {
                            db.Business_Order_FileLists.Add(businessFile);
                            db.SaveChanges();
                            OperationStatus = 1;
                            msg = fileType + businessFile.FileName + "' has been successfully uploaded on " + now.ToString("MMM dd,yyyy hh:mm tt.");
                            ColumnId = Convert.ToInt32(businessFile.FileId);
                            if (model.IsMainFile && db.Business_Order_FileLists.Where(m => m.BusinessOrderId == model.BusinessOrderId && m.FileId != ColumnId && m.IsMainFile == true && m.FileType == model.FileType).Any())
                            {
                                Business_Order_FileLists existsFile = db.Business_Order_FileLists
                                    .Where(m => m.BusinessOrderId == model.BusinessOrderId && m.FileId != ColumnId && m.IsMainFile == true && m.FileType == model.FileType)
                                    .FirstOrDefault();
                                existsFile.IsMainFile = false;
                                db.Entry(existsFile).State = EntityState.Modified;
                                db.SaveChanges();
                            }
                        }
                        catch (Exception ex)
                        {
                            string errmsg = ex.ToString();
                            OperationStatus = -1;
                            ColumnId = 0;
                            msg = fileType + businessFile.FileName + "' upload was unsuccessful on " + now.ToString("MMM dd,yyyy hh:mm tt.");
                        }
                        SaveAuditLog("Upload Business Order File", msg, "CRM", "FileUploadSaveForSpecification", "Business_Order_FileLists", ColumnId, model.CreatedBy, now, OperationStatus);
                        i++;
                    }
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
            return Json("Error", JsonRequestBehavior.AllowGet);
        }
        [EncryptedActionParameter]
        public FileResult DownloadMaterial(string fileName, string contentType, bool? isPrint)
        {
            string fullPath = Path.Combine(Server.MapPath("~/AllUploadedFile/BusinessOrderFiles/FileForSpecification/"), fileName);
            if (isPrint == true)
            {
                string ext = fileName.Split('.')[1].ToLower();
                string[] docType = { "doc", "docx", "xlsx", "xls", "ppt", "pptx", "zip", "rar" };
                foreach (var item in docType)
                {
                    if (ext.Contains(item))
                    {
                        string originalName = db.Business_Order_FileLists.Where(m => m.FileName == fileName).Select(m => m.FileOriginalName).FirstOrDefault();
                        return File(fullPath, contentType, originalName);
                    }
                }
                return File(fullPath, contentType);
            }
            else
            {
                string originalName = db.Business_Order_FileLists.Where(m => m.FileName == fileName).Select(m => m.FileOriginalName).FirstOrDefault();
                return File(fullPath, contentType, originalName);
            }
        }
        public JsonResult OrderFileRename(int? FileId, string FileName, long userId)
        {
            if (FileId == null)
            {
                return Json("Error", JsonRequestBehavior.AllowGet);
            }
            else
            {
                string msg = "";
                string name = "";
                int ColumnId = 0;
                int OperationStatus = -1;

                Business_Order_FileLists model = db.Business_Order_FileLists.Find(FileId);
                model.FileOriginalName = FileName;
                ColumnId = Convert.ToInt32(model.FileId);
                model.UpatedDate = now;
                model.UpatedBy = userId;
                try
                {
                    db.Entry(model).State = EntityState.Modified;
                    db.SaveChanges();
                    OperationStatus = 1;
                    msg = "File " + name + " has been successfully renamed on " + now.ToString("MMM dd,yyyy hh:mm tt.");
                }
                catch (Exception ex)
                {
                    string errmsg = ex.ToString();
                    OperationStatus = -1;
                    msg = "File " + name + " rename was unsuccessful on " + now.ToString("MMM dd,yyyy hh:mm tt.");
                }
                SaveAuditLog("Rename Business Order File", msg, "CRM", "OrderDetailsFileRename", "Business_Order_FileLists", ColumnId, userId, now, OperationStatus);
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
        public JsonResult DeleteUploadedFiles(int? FileId, long userId)
        {
            if (FileId != null)
            {
                string msg = "";
                string name = "";
                int ColumnId = 0;
                int OperationStatus = 1;

                Business_Order_FileLists model = db.Business_Order_FileLists.Find(FileId);
                if (model != null)
                {
                    ColumnId = Convert.ToInt32(model.FileId);
                    name = model.FileOriginalName;
                    try
                    {
                        db.Business_Order_FileLists.Remove(model);
                        db.SaveChanges();
                        OperationStatus = 1;
                        msg = "File '" + name + "' has been successfully deleted on " + now.ToString("MMM dd,yyyy hh:mm tt.");
                        string FilePath = Path.Combine(Server.MapPath("~/AllUploadedFile/BusinessOrderFiles/FileForSpecification/"), model.FileName);
                        if (System.IO.File.Exists(FilePath))
                        {
                            System.IO.File.Delete(FilePath);
                        }

                    }
                    catch (Exception ex)
                    {
                        string errmsg = ex.ToString();
                        OperationStatus = -1;
                        msg = "File '" + name + "' deletion was unsuccessful on " + now.ToString("MMM dd,yyyy hh:mm tt.");
                    }
                    SaveAuditLog("Delete Business Order File", msg, "CRM", "OrderDetails", "Business_Order_FileLists", ColumnId, userId, now, OperationStatus);
                    if (OperationStatus == 1)
                    {
                        return Json("Success", JsonRequestBehavior.AllowGet);
                    }
                }
            }
            return Json("Error", JsonRequestBehavior.AllowGet);
        }
        public PartialViewResult BusinessOrderFileDetails(int FileId)
        {
            View_BusinessOrder_File model = db.View_BusinessOrder_File.Where(m => m.FileId == FileId).FirstOrDefault();
            return PartialView(model);
        }
        public JsonResult CheckBusiOrderHasFile(int BusiId)
        {
            return Json(db.Business_Order_FileLists.Where(m => m.BusinessOrderId == BusiId).Any());
        }
        public JsonResult BusinessOrderStatusChange(BusinessOrderList bus)
        {
            if (bus.BusinessOrderId > 0)
            {
                BusinessOrderList model = db.BusinessOrderLists.Find(bus.BusinessOrderId);
                if (model != null)
                {
                    string msg = "";
                    string opname = "Business Order Confirmed";
                    int ColumnId = Convert.ToInt32(bus.BusinessOrderId);
                    int OperationStatus = -1;

                    model.OrderStatus = bus.OrderStatus;
                    model.ApprovedBy = bus.ApprovedBy;
                    model.ApprovedDate = now;
                    if (bus.OrderStatus == 3)
                    {
                        model.OrderDeadLine = bus.OrderDeadLine;
                        msg = "Business order '" + model.OrderName + "' has been complete successfully on " + now.ToString("MMM dd,yyyy hh:mm tt.");
                    }
                    else
                    {
                        msg = "Business order '" + model.OrderName + "' has been confirmed successfully on " + now.ToString("MMM dd,yyyy hh:mm tt.");
                    }
                    try
                    {
                        db.Entry(model).State = EntityState.Modified;
                        db.SaveChanges();
                        OperationStatus = 1;

                    }
                    catch (Exception ex)
                    {
                        string ermsg = ex.ToString();
                        OperationStatus = -1;
                        msg = "Business order '" + model.OrderName + "' confirmation was unsuccessful on " + now.ToString("MMM dd,yyyy hh:mm tt.");
                    }
                    SaveAuditLog(opname, msg, "CRM", "Business Order", "BusinessOrderList", ColumnId, (long)bus.ApprovedBy, now, OperationStatus);
                    if (OperationStatus == 1)
                    {
                        return Json("Success", JsonRequestBehavior.AllowGet);
                    }
                }
            }
            return Json("Error", JsonRequestBehavior.AllowGet);
        }
        public PartialViewResult _OrderDeadLine()
        {
            return PartialView();
        }
        public JsonResult GetUnitId()
        {
            var list = (from m in db.UnitLists
                        select new
                        {
                            UnitId = m.UnitId,
                            UnitName = m.UnitName
                        }).ToList();
            return Json(list, JsonRequestBehavior.AllowGet);
        }
        #endregion

        //********************* BUSINESS ORDER FROM FACTORY TO SUPPLIER ***********************************
        #region Business Order From Factory to Supplier
        public ActionResult BusiOrderDetailsForSup(int? id)
        {
            BusinessOrderListModelView model = new BusinessOrderListModelView();
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            else
            {
                model = (from m in db.BusinessOrderLists
                         join c in db.ClientLists on m.ClientId equals c.ClientId
                         where m.BusinessOrderId == id && m.OrderType == 2
                         select new BusinessOrderListModelView
                         {
                             BusinessOrderId = m.BusinessOrderId,
                             OrderName = m.OrderName,
                             OrderStatus = m.OrderStatus,
                             OrderType = m.OrderType,
                             ClientId = m.ClientId,
                             SupplierName = c.Name,
                             TotalCost = m.TotalCost,
                             TotalProCost = m.TotalProCost,
                             TotalOthersCost = m.TotalOthersCost
                         }).FirstOrDefault();

                if (db.Acquisition_List.Where(m => m.BusinessOrderId == model.BusinessOrderId && m.AcquisitionType == 2).Any())
                {
                    model.AcquisitionOrderId = db.Acquisition_List.Where(m => m.BusinessOrderId == model.BusinessOrderId && m.AcquisitionType == 2)
                                              .Select(m => m.AcquisitionOrderId).FirstOrDefault();
                }
                //ViewBag.allSupList = (from m in db.BusinessOrderSupplierLists
                //                  join c in db.ClientLists on m.ClientId equals c.ClientId
                //                  where m.BusinessOrderId == id
                //                  select new BusinessOrderListModelView
                //                  {
                //                      ClientId = m.ClientId,
                //                      ClientName = c.Name,
                //                      Client_User_Id = m.Id
                //                  }).ToList();
                return View(model);
            }
        }
        public ActionResult _WaitingToBeDeliverdItemList(int? busId)
        {
            List<View_All_Approved_AcquisitionList> acq_list = new List<View_All_Approved_AcquisitionList>();

            var list = (from v in db.View_All_Approved_AcquisitionList
                        join a in db.BusinessOrderProductLists on v.AcquisitionId equals a.AcquisitionOrderId
                        where a.BusinessOrderId == busId
                        select new
                        {
                            Id = a.Id,
                            AcquisitionId = v.AcquisitionId,
                            AcquisitionOrderId = v.AcquisitionOrderId,
                            ProductId = v.ProductId,
                            ProductName = v.ProductName,
                            Quantity = a.Quantity,
                            UnitName = v.UnitName,
                            Status = a.ReceivedStatus
                        }).ToList();

            foreach (var l in list)
            {
                if (!acq_list.Where(m => m.ProductId == l.ProductId).Any() || (l.ProductId == null))
                {
                    acq_list.Add(new View_All_Approved_AcquisitionList
                    {
                        AcquisitionId = l.AcquisitionId,
                        AcquisitionOrderId = l.Id,
                        ProductId = l.ProductId,
                        ProductName = l.ProductName,
                        Quantity = Convert.ToInt32(l.Quantity),
                        UnitName = l.UnitName,
                        OrderStatus = l.Status
                    });
                }
            }
            ViewBag.busId = busId;
            return PartialView(acq_list);
        }
        public ActionResult _ReadyToUsedItem(int? busId)
        {
            List<View_All_Approved_AcquisitionList> acq_list = new List<View_All_Approved_AcquisitionList>();

            var list = (from v in db.View_All_Approved_AcquisitionList
                        join a in db.BusinessOrderProductLists on v.AcquisitionId equals a.AcquisitionOrderId
                        where a.BusinessOrderId == busId
                        select new
                        {
                            Id = a.Id,
                            AcquisitionId = v.AcquisitionId,
                            AcquisitionOrderId = v.AcquisitionOrderId,
                            ProductId = v.ProductId,
                            ProductName = v.ProductName,
                            Quantity = a.Quantity,
                            UnitName = v.UnitName,
                            Status = a.ReceivedStatus
                        }).ToList();

            foreach (var l in list)
            {
                if (!acq_list.Where(m => m.ProductId == l.ProductId).Any() || (l.ProductId == null))
                {
                    acq_list.Add(new View_All_Approved_AcquisitionList
                    {
                        AcquisitionId = l.AcquisitionId,
                        AcquisitionOrderId = l.Id,
                        ProductId = l.ProductId,
                        ProductName = l.ProductName,
                        Quantity = Convert.ToInt32(l.Quantity),
                        UnitName = l.UnitName,
                        OrderStatus = l.Status
                    });
                }
            }
            ViewBag.busId = busId;
            return PartialView(acq_list);
        }
        public ActionResult _AssignButNotAcceptItem(int? busId)
        {
            List<View_All_Approved_AcquisitionList> acq_list = new List<View_All_Approved_AcquisitionList>();

            var list = (from v in db.View_All_Approved_AcquisitionList
                        join a in db.BusiReceivedItemAssignLists on v.AcquisitionId equals a.AcqId
                        where a.BusinessId == busId
                        select new
                        {
                            Id = a.BusItemRcvdId,
                            AcquisitionId = v.AcquisitionId,
                            AcquisitionOrderId = v.AcquisitionOrderId,
                            ProductId = v.ProductId,
                            ProductName = v.ProductName,
                            Quantity = a.Quantity,
                            UnitName = v.UnitName,
                            AssignedDate = a.AssignedDate,
                            AssignedBy = a.AssignedBy
                        }).ToList();

            foreach (var l in list)
            {
                if (!acq_list.Where(m => m.ProductId == l.ProductId).Any() || (l.ProductId == null))
                {
                    acq_list.Add(new View_All_Approved_AcquisitionList
                    {
                        AcquisitionId = l.AcquisitionId,
                        AcquisitionOrderId = l.Id,
                        ProductId = l.ProductId,
                        ProductName = l.ProductName,
                        Quantity = Convert.ToInt32(l.Quantity),
                        UnitName = l.UnitName,
                        CreatedBy = l.AssignedBy,
                        UpdatedDate = l.AssignedDate
                    });
                }
            }
            ViewBag.busId = busId;
            return PartialView(acq_list);
        }
        public JsonResult ReceiveItemSave(string[] id, string[] qan)
        {
            for (int i = 0; i < id.Length; i++)
            {
                int bus_id = Convert.ToInt32(id[i]);
                BusinessOrderProductList pro = db.BusinessOrderProductLists.Find(bus_id);
                pro.ReceivedStatus = 2;
                pro.ReceivedQuantity = Convert.ToInt32(qan[i]);
                db.Entry(pro).State = EntityState.Modified;
                db.SaveChanges();
            }
            return Json("Success", JsonRequestBehavior.AllowGet);
        }
        public ActionResult ReceivedItemStoreLocation()
        {
            var list = (from s in db.SiteLists
                        select new
                        {
                            Id = s.Id,
                            SiteName = s.SiteName,
                            IsComposite = s.IsComposite
                        }).ToList();
            ViewBag.AllSiteId = list;
            ViewBag.SiteId = new SelectList(db.SiteLists, "Id", "SiteName", "IsComposite");
            ViewBag.StoreId = new SelectList(db.StoreLists, "Id", "StoreName");
            ViewBag.WareHouseId = new SelectList(db.WarehouseLists, "Id", "WarehouseName");
            return PartialView();
        }
        public JsonResult ChkIsSiteComposite(int siteId)
        {
            return Json(db.SiteLists.Where(m => m.Id == siteId).Select(m => m.IsComposite).FirstOrDefault());
        }
        public JsonResult GetAllUnitName(int? ParentId)
        {
            if (ParentId != null)
            {
                int id = Convert.ToInt32(ParentId);
                var list = (from m in db.Site_Unit_Lists
                            where m.SiteId == id
                            select new
                            {
                                UnitId = m.UnitId,
                                UnitName = m.UnitName
                            }).ToList();

                return Json(list, JsonRequestBehavior.AllowGet);
            }
            else
            {
                var list = (from m in db.Site_Unit_Lists
                            select new
                            {
                                UnitId = m.UnitId,
                                UnitName = m.UnitName
                            }).ToList();
                return Json(list, JsonRequestBehavior.AllowGet);
            }
        }
        public JsonResult GetAllDeptName(int? ParentId, bool IsCom)
        {
            if (ParentId != null)
            {
                if (IsCom)
                {
                    int id = Convert.ToInt32(ParentId);
                    var list = (from m in db.DepartmentLists
                                where m.UnitId == id
                                select new
                                {
                                    DeptId = m.DeptId,
                                    DeptName = m.DeptName
                                }).ToList();

                    return Json(list, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    int id = Convert.ToInt32(ParentId);
                    var list = (from m in db.DepartmentLists
                                where m.SiteId == id
                                select new
                                {
                                    DeptId = m.DeptId,
                                    DeptName = m.DeptName
                                }).ToList();

                    return Json(list, JsonRequestBehavior.AllowGet);
                }

            }
            else
            {
                var list = (from m in db.DepartmentLists
                            select new
                            {
                                DeptId = m.DeptId,
                                DeptName = m.DeptName
                            }).ToList();
                return Json(list, JsonRequestBehavior.AllowGet);
            }
        }
        public JsonResult GetAllLineName(int? ParentId)
        {
            if (ParentId != null)
            {
                int id = Convert.ToInt32(ParentId);
                var list = (from m in db.LineInfoes
                            where m.DeptId == id
                            select new
                            {
                                LineId = m.LineId,
                                LineName = m.LineName
                            }).ToList();

                return Json(list, JsonRequestBehavior.AllowGet);
            }
            else
            {
                var list = (from m in db.LineInfoes
                            select new
                            {
                                LineId = m.LineId,
                                LineName = m.LineName
                            }).ToList();
                return Json(list, JsonRequestBehavior.AllowGet);
            }
        }
        public JsonResult AssignReceivedItem(string[] id, string[] qan, int type, int? storeId, int? wareId, int? lineId, long UserId, int bus_id)
        {
            for (int i = 0; i < id.Length; i++)
            {
                int acqid = Convert.ToInt32(id[i]);
                BusinessOrderProductList pro = db.BusinessOrderProductLists.Where(m => m.AcquisitionOrderId == acqid && m.BusinessOrderId == bus_id).FirstOrDefault();
                pro.ReceivedStatus = 3;
                pro.AssignedQuantity = Convert.ToInt32(pro.AssignedQuantity) + Convert.ToInt32(qan[i]);
                db.Entry(pro).State = EntityState.Modified;
                db.SaveChanges();

                bool exsits = false;
                if (type == 1)
                {
                    if (db.BusiReceivedItemAssignLists.Where(m => m.BusinessId == pro.BusinessOrderId && m.AcqId == pro.AcquisitionOrderId && m.StoreId == storeId).Any())
                    {
                        BusiReceivedItemAssignList assign_item = db.BusiReceivedItemAssignLists.Where(m => m.BusinessId == pro.BusinessOrderId && m.AcqId == pro.AcquisitionOrderId && m.StoreId == storeId).FirstOrDefault();
                        assign_item.Quantity = (Convert.ToInt32(assign_item.Quantity) + Convert.ToInt32(qan[i]));
                        db.Entry(assign_item).State = EntityState.Modified;
                        db.SaveChanges();
                        exsits = true;
                    }
                }
                else if (type == 2)
                {
                    if (db.BusiReceivedItemAssignLists.Where(m => m.BusinessId == pro.BusinessOrderId && m.AcqId == pro.AcquisitionOrderId && m.WareId == wareId).Any())
                    {
                        BusiReceivedItemAssignList assign_item = db.BusiReceivedItemAssignLists.Where(m => m.BusinessId == pro.BusinessOrderId && m.AcqId == pro.AcquisitionOrderId && m.WareId == wareId).FirstOrDefault();
                        assign_item.Quantity = (Convert.ToInt32(assign_item.Quantity) + Convert.ToInt32(qan[i]));
                        db.Entry(assign_item).State = EntityState.Modified;
                        db.SaveChanges();
                        exsits = true;
                    }
                }
                else
                {
                    if (db.BusiReceivedItemAssignLists.Where(m => m.BusinessId == pro.BusinessOrderId && m.AcqId == pro.AcquisitionOrderId && m.LineId == lineId).Any())
                    {
                        BusiReceivedItemAssignList assign_item = db.BusiReceivedItemAssignLists.Where(m => m.BusinessId == pro.BusinessOrderId && m.AcqId == pro.AcquisitionOrderId && m.LineId == lineId).FirstOrDefault();
                        assign_item.Quantity = (Convert.ToInt32(assign_item.Quantity) + Convert.ToInt32(qan[i]));
                        db.Entry(assign_item).State = EntityState.Modified;
                        db.SaveChanges();
                        exsits = true;
                    }
                }
                if (!exsits)
                {
                    BusiReceivedItemAssignList assign_item = new BusiReceivedItemAssignList();
                    assign_item.BusinessId = pro.BusinessOrderId;
                    assign_item.InventoryId = pro.ProductId;
                    assign_item.AcqId = Convert.ToInt32(pro.AcquisitionOrderId);
                    assign_item.Quantity = Convert.ToInt32(qan[i]);
                    assign_item.BusProId = pro.Id;
                    assign_item.StoreId = storeId;
                    assign_item.WareId = wareId;
                    assign_item.LineId = lineId;
                    assign_item.AssignedDate = now;
                    assign_item.AssignedBy = UserId;
                    db.BusiReceivedItemAssignLists.Add(assign_item);
                    db.SaveChanges();
                }
            }
            return Json("Success", JsonRequestBehavior.AllowGet);
        }
        public ActionResult AcceptAssignedItem(int id, long UserId)
        {
            int OperationStatus = -1;
            string msg;
            int ColumnId = 0;

            BusiReceivedItemAssignList pro = db.BusiReceivedItemAssignLists.Find(id);
            if (pro.InventoryId == null)
            {
                bool already_exts = false;
                Inventory_Item_Location item_quan = new Inventory_Item_Location();
                if (pro.StoreId > 0)
                {
                    if (db.Inventory_Item_Location.Where(m => m.LastOrderId == pro.BusProId && m.StoreId == pro.StoreId).Any())
                    {
                        item_quan = db.Inventory_Item_Location.Where(m => m.LastOrderId == pro.BusProId && m.StoreId == pro.StoreId).FirstOrDefault();
                        already_exts = true;
                    }
                }
                else if (pro.WareId > 0)
                {
                    if (db.Inventory_Item_Location.Where(m => m.LastOrderId == pro.BusProId && m.WarehouseId == pro.WareId).Any())
                    {
                        item_quan = db.Inventory_Item_Location.Where(m => m.LastOrderId == pro.BusProId && m.WarehouseId == pro.WareId).FirstOrDefault();
                        already_exts = true;
                    }
                }
                else
                {
                    if (db.Inventory_Item_Location.Where(m => m.LastOrderId == pro.BusProId && m.LineId == pro.LineId).Any())
                    {
                        item_quan = db.Inventory_Item_Location.Where(m => m.LastOrderId == pro.BusProId && m.LineId == pro.LineId).FirstOrDefault();
                        already_exts = true;
                    }
                }
                if (already_exts)
                {
                    item_quan.Quantity = (Convert.ToDouble(item_quan.Quantity) + Convert.ToDouble(pro.Quantity));
                    db.Entry(item_quan).State = EntityState.Modified;
                    db.SaveChanges();
                }
                else
                {
                    var Orderdetails = db.Acquisitions.Find(pro.AcqId);
                    InventoryList inlist = new InventoryList();
                    if (db.Inventory_Item_Location.Where(m => m.LastOrderId == pro.BusProId).Any())
                    {
                        int invenId = db.Inventory_Item_Location.Where(m => m.LastOrderId == pro.BusProId).Select(m => m.InventoryId).FirstOrDefault();
                        inlist = db.InventoryLists.Find(invenId);
                    }
                    else
                    {
                        inlist.ProductName = Orderdetails.ProductName;
                        inlist.ProductTypeId = Convert.ToInt32(Orderdetails.ProductTypeId);
                        inlist.Country = Orderdetails.Country.ToString();
                        inlist.CanbeBreakable = false;
                        inlist.CanbeSold = false;
                        inlist.CanbeOrdered = true;
                        inlist.CanbeRepaired = false;
                        inlist.CanbePerishable = false;
                        inlist.Quantity = pro.Quantity;
                        inlist.UnitId = Orderdetails.UnitId;
                        inlist.Description = Orderdetails.Description;
                        inlist.BrandId = Orderdetails.Brand;
                        inlist.Model = Orderdetails.Model;
                        inlist.InternalReferenceNo = now.ToString("yyyyMMddmmss").ToString() + "" + UserId;
                        inlist.CreatedBy = UserId;
                        inlist.CreatedDate = now;
                        db.InventoryLists.Add(inlist);
                    }
                    try
                    {
                        db.SaveChanges();
                        int InventoryId = 0;
                        if (db.Inventory_Item_Location.Where(m => m.LastOrderId == pro.BusProId).Any())
                        {
                            InventoryId = inlist.InventoryId;
                            msg = "Product quantity increase.";
                        }
                        else
                        {
                            InventoryId = db.InventoryLists.Where(m => m.CreatedBy == UserId).Max(m => m.InventoryId);
                            msg = "New product insert into store successful.";
                        }

                        ColumnId = InventoryId;
                        OperationStatus = 1;
                        int count = pro.Quantity;

                        for (int j = 1; j <= count; j++)
                        {
                            StoreItemList plist = new StoreItemList();
                            plist.InventoryId = InventoryId;
                            plist.CreatedDate = now;
                            plist.CreatedBy = UserId;
                            db.StoreItemLists.Add(plist);
                            db.SaveChanges();
                        }
                        int imgCount = 0;
                        if (db.AcquisitionImages.Where(m => m.AcquisitionId == pro.AcqId).Any())
                        {
                            var imgList = db.AcquisitionImages.Where(m => m.AcquisitionId == pro.AcqId).ToList();
                            foreach (var img in imgList)
                            {
                                string s = img.ImageName;
                                int idx = s.LastIndexOf('.');
                                string fileName = s.Substring(0, idx);
                                string extension = s.Substring(idx);

                                Inventory_Image_Lists pic = new Inventory_Image_Lists();
                                pic.InventoryId = InventoryId;
                                pic.ImageName = InventoryId + "" + imgCount + "" + extension;
                                db.Inventory_Image_Lists.Add(pic);
                                db.SaveChanges();

                                string search_file = Path.Combine(Server.MapPath("~/Images/Inventory/Acquisition/Original"), s);
                                string destFile = Path.Combine(Server.MapPath("~/Images/Inventory/Store/Original"), pic.ImageName);
                                System.IO.File.Copy(search_file, destFile, true);

                                string search_file_resize = Path.Combine(Server.MapPath("~/Images/Inventory/Acquisition/Resize/"), s);
                                string destFile_resize = Path.Combine(Server.MapPath("~/Images/Inventory/Store/Resize"), pic.ImageName);
                                System.IO.File.Copy(search_file_resize, destFile_resize, true);

                                imgCount++;
                            }
                        }
                        Inventory_Item_Location item_location = new Inventory_Item_Location();
                        item_location.InventoryId = InventoryId;
                        item_location.StoreId = pro.StoreId;
                        item_location.WarehouseId = pro.WareId;
                        item_location.DeptId = pro.LineId;
                        item_location.LineId = pro.LineId;
                        item_location.Quantity = Convert.ToDouble(pro.Quantity);
                        item_location.UnitId = Orderdetails.UnitId;
                        item_location.LastOrderId = pro.BusProId;
                        item_location.ReceivedDate = now;
                        item_location.ReceivedBy = UserId;
                        db.Inventory_Item_Location.Add(item_location);
                        db.SaveChanges();
                    }
                    catch (Exception ex)
                    {
                        string errrorMsg = ex.Message.ToString();
                        msg = "New product insert into store unsuccessful.";
                        OperationStatus = -1;
                        ColumnId = 0;
                    }
                }

            }
            else
            {
                bool pro_exists = false;
                Inventory_Item_Location item_quan = new Inventory_Item_Location();
                if (pro.StoreId > 0)
                {
                    if (db.Inventory_Item_Location.Where(m => m.InventoryId == pro.InventoryId && m.StoreId == pro.StoreId).Any())
                    {
                        item_quan = db.Inventory_Item_Location.Where(m => m.InventoryId == pro.InventoryId && m.StoreId == pro.StoreId).FirstOrDefault();
                        pro_exists = true;
                    }
                }
                else if (pro.WareId > 0)
                {
                    if (db.Inventory_Item_Location.Where(m => m.InventoryId == pro.InventoryId && m.WarehouseId == pro.WareId).Any())
                    {
                        item_quan = db.Inventory_Item_Location.Where(m => m.InventoryId == pro.InventoryId && m.WarehouseId == pro.WareId).FirstOrDefault();
                        pro_exists = true;
                    }
                }
                else
                {
                    if (db.Inventory_Item_Location.Where(m => m.InventoryId == pro.InventoryId && m.LineId == pro.LineId).Any())
                    {
                        item_quan = db.Inventory_Item_Location.Where(m => m.InventoryId == pro.InventoryId && m.LineId == pro.LineId).FirstOrDefault();
                        pro_exists = true;
                    }
                }
                if (pro_exists)
                {
                    item_quan.Quantity = (Convert.ToDouble(item_quan.Quantity) + Convert.ToDouble(pro.Quantity));
                    db.Entry(item_quan).State = EntityState.Modified;
                    db.SaveChanges();
                    OperationStatus = 1;
                }
                else
                {
                    var invenDetails = db.View_All_InventoryList.Where(m => m.InventoryId == pro.InventoryId).FirstOrDefault();
                    Inventory_Item_Location item_location = new Inventory_Item_Location();
                    item_location.InventoryId = Convert.ToInt32(pro.InventoryId);
                    item_location.StoreId = pro.StoreId;
                    item_location.WarehouseId = pro.WareId;
                    item_location.DeptId = pro.LineId;
                    item_location.LineId = pro.LineId;
                    item_location.Quantity = Convert.ToDouble(pro.Quantity);
                    item_location.UnitId = invenDetails.UnitId;
                    item_location.LastOrderId = pro.BusProId;
                    item_location.ReceivedDate = now;
                    item_location.ReceivedBy = UserId;
                    db.Inventory_Item_Location.Add(item_location);
                    try
                    {
                        db.SaveChanges();
                        OperationStatus = 1;
                    }
                    catch (Exception ex)
                    {
                        string strErr = ex.ToString();
                        OperationStatus = -1;
                    }
                }
            }
            if (OperationStatus == 1)
            {
                if (!db.BusinessOrderProductLists.Where(m => m.BusinessOrderId == pro.BusinessId && m.ReceivedStatus < 3).Any())
                {
                    BusinessOrderList bus_ord = db.BusinessOrderLists.Find(pro.BusinessId);
                    bus_ord.OrderStatus = 3;
                    db.Entry(bus_ord).State = EntityState.Modified;
                }
                db.BusiReceivedItemAssignLists.Remove(pro);
                db.SaveChanges();
                return Json("Success", JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json("Error", JsonRequestBehavior.AllowGet);
            }
        }
        #endregion

        #region
        public ActionResult BusinessOrderEdit(int? BusinessOrderId, int OrderType, bool IsDisplay)
        {
            BusinessOrderListModelView model = new BusinessOrderListModelView();
            if (BusinessOrderId != null)
            {
                if (OrderType == 1)
                {
                    model = (from m in db.BusinessOrderLists
                             join c in db.ClientLists
                             on m.ClientId equals c.ClientId
                             where m.BusinessOrderId == BusinessOrderId && m.OrderType == 1
                             select new BusinessOrderListModelView
                             {
                                 BusinessOrderId = m.BusinessOrderId,
                                 OrderName = m.OrderName,
                                 OrderStatus = m.OrderStatus,
                                 ClientId = m.ClientId,
                                 ClientName = c.Name,
                                 OrderType = m.OrderType
                             }).FirstOrDefault();
                    ViewBag.ClientId = new SelectList(db.ClientLists.Where(m => m.IsBuyer == true), "ClientId", "Name", model.ClientId);
                }
                else
                {
                    model = (from m in db.BusinessOrderLists
                             where m.BusinessOrderId == BusinessOrderId && m.OrderType == 2
                             select new BusinessOrderListModelView
                             {
                                 BusinessOrderId = m.BusinessOrderId,
                                 OrderName = m.OrderName,
                                 OrderStatus = m.OrderStatus,
                                 OrderType = m.OrderType
                             }).FirstOrDefault();
                    List<SelectListItem> allSupList = new List<SelectListItem>();
                    var allSup = db.BusinessOrderSupplierLists.Where(m => m.BusinessOrderId == BusinessOrderId).ToList();
                    for (int i = 0; i < allSup.Count(); i++)
                    {
                        int tid = Convert.ToInt32(allSup[i].ClientId);
                        allSupList.Add(new SelectListItem { Text = db.ClientLists.Find(tid).Name, Value = tid.ToString() });
                    }
                    ViewBag.SelectedSup = allSupList;
                    ViewBag.ClientId = new SelectList(db.ClientLists.Where(m => m.IsSupplier == true), "ClientId", "Name", model.ClientId);
                }
                if (IsDisplay)
                {
                    return PartialView("_BusiOrderInfoDisplay", model);
                }
                else
                {
                    return PartialView(model);
                }

            }
            return PartialView();
        }
        public JsonResult BusinessOrderInfoUpdate(BusinessOrderListModelView model, int BusinessOrderId, string[] AllAcquisitionOrderId)
        {
            string msg = "";
            string opname = "New Business Order";
            int ColumnId = 0;
            int OperationStatus = 1;

            if (ModelState.IsValid)
            {
                BusinessOrderList business = new BusinessOrderList();
                business = db.BusinessOrderLists.Find(model.BusinessOrderId);
                business.OrderName = model.OrderName;
                business.OrderStatus = 0;
                business.UpdatedDate = now;
                business.UpdatedBy = model.CreatedBy;
                db.Entry(business).State = EntityState.Modified;
                opname = "Update Business Order Info";
                try
                {
                    db.SaveChanges();
                    ColumnId = model.BusinessOrderId;

                    //************************ SAVE ALL SUPPLIER NAME **********************************
                    if (model.OrderType == 2)
                    {
                        if (db.BusinessOrderSupplierLists.Where(m => m.BusinessOrderId == model.BusinessOrderId).Any())
                        {
                            var pre_Sup_list = db.BusinessOrderSupplierLists.Where(m => m.BusinessOrderId == model.BusinessOrderId).ToList();
                            foreach (var sup in pre_Sup_list)
                            {
                                db.BusinessOrderSupplierLists.Remove(sup);
                                db.SaveChanges();
                            }
                        }

                        string[] supplier = model.AllSupplierId.Split(',');
                        for (int i = 0; i < supplier.Length; i++)
                        {
                            OperationStatus = 1;
                            int supId = 0;
                            BusinessOrderSupplierList sup = new BusinessOrderSupplierList();
                            sup.BusinessOrderId = ColumnId;
                            sup.ClientId = Convert.ToInt32(supplier[i]);
                            db.BusinessOrderSupplierLists.Add(sup);
                            try
                            {
                                db.SaveChanges();
                                supId = db.BusinessOrderSupplierLists.Where(m => m.BusinessOrderId == ColumnId).Max(m => m.Id);
                                msg = "Supplier add with business order " + model.OrderName + " .";
                            }
                            catch (Exception ex)
                            {
                                string errmsg = ex.ToString();
                                supId = 0;
                                OperationStatus = -1;
                                msg = "Supplier add with business order " + model.OrderName + " unsuccessful.";
                            }
                            //SaveAuditLog("Supplier Add ", msg, "CRM", "Business Order", "BusinessOrderSupplierLists", supId, model.CreatedBy, now, OperationStatus);
                        }
                    }

                    msg = "Business order " + model.OrderName + " info successfully updated.";
                }
                catch (Exception ex)
                {
                    string errmsg = ex.ToString();
                    ColumnId = 0;
                    OperationStatus = -1;
                    msg = "Business order " + model.OrderName + " info updated unsuccessful.";
                }
                SaveAuditLog(opname, msg, "CRM", "Business Order", "BusinessOrderList", ColumnId, model.CreatedBy, now, OperationStatus);
                if (OperationStatus == 1)
                {
                    return Json("Success", JsonRequestBehavior.AllowGet);
                }
            }
            return Json("Error", JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region ALL ABOUT MANUALLY ADDED SPECIFICATION FUNCATIONALITY
        public PartialViewResult ManualSpecification(int BusinessOrderId, bool? IsPrint)
        {
            BusinessOrderListModelView model = new BusinessOrderListModelView();
            model.BusinessOrderId = BusinessOrderId;
            ViewBag.IsPrint = IsPrint;
            return PartialView(model);
        }
        public PartialViewResult AddedManualSpec(int BusinessOrderId, bool? IsPrint)
        {
            var list = (from m in db.BusiOrder_ManualSpec
                        join u in db.UnitLists on m.UnitId equals u.UnitId
                        where m.BusOrdId == BusinessOrderId
                        select new BusiOrder_ManualSpecModelView
                        {
                            Id = m.Id,
                            BusOrdId = m.BusOrdId,
                            Spec_lbl = m.Spec_lbl,
                            Quantity = m.Quantity,
                            UnitId = m.UnitId,
                            UnitName = u.UnitName,
                            Spec_Description = m.Spec_Description,
                            CreatedBy = m.CreatedBy,
                            CreatedDate = m.CreatedDate,
                            ProStatus = m.ProStatus,
                            IsStoreIntoStock = m.IsStoreIntoStock
                        }).ToList();
            ViewBag.IsPrint = IsPrint;
            return PartialView(list);
        }
        public PartialViewResult BusManualSpecAddPopUp(int BusinessOrderId)
        {
            ViewBag.busId = BusinessOrderId;
            return PartialView();
        }
        public PartialViewResult BusManualSpecEditPopUp(int SpecId)
        {
            ViewBag.SpecId = SpecId;
            return PartialView();
        }
        public ActionResult BusManualSpecAdd(int BusinessOrderId)
        {
            BusiOrder_ManualSpecModelView model = new BusiOrder_ManualSpecModelView();
            model.BusOrdId = BusinessOrderId;
            ViewBag.UnitId = new SelectList(db.UnitLists, "UnitId", "UnitName");
            return PartialView(model);
        }
        public ActionResult BusManualSpecEdit(int SpecId)
        {

            BusiOrder_ManualSpecModelView model = new BusiOrder_ManualSpecModelView();
            if (SpecId > 0)
            {
                BusiOrder_ManualSpec manual_Spec = db.BusiOrder_ManualSpec.Find(SpecId);
                model.Id = manual_Spec.Id;
                model.BusOrdId = manual_Spec.BusOrdId;
                model.Spec_lbl = manual_Spec.Spec_lbl;
                model.Quantity = manual_Spec.Quantity;
                model.UnitId = manual_Spec.UnitId;
                model.Spec_Description = manual_Spec.Spec_Description;
                model.CreatedBy = manual_Spec.CreatedBy;
                model.ProStatus = manual_Spec.ProStatus;
            }
            ViewBag.UnitId = new SelectList(db.UnitLists, "UnitId", "UnitName", model.UnitId);
            return PartialView(model);
        }
        public JsonResult SaveManualSpec(int busId, string[] lbl, string[] quan, string[] unitId, string[] des, long UserId)
        {
            int opStatus = -1;
            int ColumnId = 0;
            string msg = "";
            for (int i = 0; i < lbl.Count(); i++)
            {
                BusiOrder_ManualSpec model = new BusiOrder_ManualSpec();
                model.BusOrdId = busId;
                model.Spec_lbl = lbl[i];
                model.Quantity = Convert.ToInt32(quan[i]);
                model.UnitId = Convert.ToInt32(unitId[i]);
                if (des[i] != ",")
                {
                    model.Spec_Description = des[i];
                }

                model.CreatedBy = UserId;
                model.CreatedDate = now;
                model.ProStatus = 0;
                db.BusiOrder_ManualSpec.Add(model);
                msg = "Manual specification '" + model.Spec_lbl + "' has been successfully added on " + now.ToString("MMM dd,yyyy hh:mm tt.");
                try
                {
                    db.SaveChanges();
                    opStatus = 1;
                    ColumnId = model.Id;
                }
                catch (Exception Ex)
                {
                    string errmsg = Ex.ToString();
                    opStatus = -1;
                    msg = "Manual specification '" + model.Spec_lbl + "' add was unsuccessful on " + now.ToString("MMM dd,yyyy hh:mm tt.");
                }
                SaveAuditLog("Add Manual Specification", msg, "CRM", "Order Details", "BusiOrder_ManualSpec", ColumnId, UserId, now, opStatus);
            }
            if (opStatus == 1)
            {
                return Json("Success", JsonRequestBehavior.AllowGet);
            }
            return Json("Error", JsonRequestBehavior.AllowGet);
        }
        public JsonResult EditManualSpec(BusiOrder_ManualSpec model)
        {
            int opStatus = -1;
            int ColumnId = 0;
            string msg = "";
            BusiOrder_ManualSpec manual_Spec = db.BusiOrder_ManualSpec.Find(model.Id);
            manual_Spec.Spec_lbl = model.Spec_lbl;
            manual_Spec.Quantity = model.Quantity;
            manual_Spec.UnitId = model.UnitId;
            manual_Spec.Spec_Description = model.Spec_Description;
            manual_Spec.CreatedBy = model.CreatedBy;
            manual_Spec.CreatedDate = now;
            manual_Spec.ProStatus = model.ProStatus;
            db.Entry(manual_Spec).State = EntityState.Modified;
            msg = "Manual specification '" + model.Spec_lbl + "' has been successfully updated on " + now.ToString("MMM dd,yyyy hh:mm tt.");
            try
            {
                db.SaveChanges();
                opStatus = 1;
                ColumnId = manual_Spec.Id;
            }
            catch (Exception ex)
            {
                string errmsg = ex.ToString();
                opStatus = -1;
                msg = "Manual specification '" + model.Spec_lbl + "' updated was unsuccessful on " + now.ToString("MMM dd,yyyy hh:mm tt.");
            }
            SaveAuditLog("Edit Manual Specification", msg, "CRM", "Order Details", "BusiOrder_ManualSpec", ColumnId, model.CreatedBy, now, opStatus);
            if (opStatus == 1)
            {
                return Json("Success", JsonRequestBehavior.AllowGet);
            }
            return Json("Error", JsonRequestBehavior.AllowGet);
        }
        public JsonResult DeleteManualSpec(int? Id, long UserId)
        {
            int opStatus = -1;
            int ColumnId = 0;
            string msg = "";
            if (Id != null)
            {
                BusiOrder_ManualSpec manual_spec = db.BusiOrder_ManualSpec.Find(Id);
                if (manual_spec != null)
                {
                    db.BusiOrder_ManualSpec.Remove(manual_spec);
                    msg = "Manual added specification '" + manual_spec.Spec_lbl + "' has been deleted successfully on " + now.ToString("MMM dd,yyyy hh:mm tt.");
                    try
                    {
                        db.SaveChanges();
                        opStatus = 1;
                        ColumnId = manual_spec.Id;
                    }
                    catch (Exception ex)
                    {
                        string errmsg = ex.ToString();
                        opStatus = -1;
                        msg = "Manual added specification '" + manual_spec.Spec_lbl + "' deletion was unsuccessful on " + now.ToString("MMM dd,yyyy hh:mm tt.");
                    }
                    SaveAuditLog("Delete Manual Specification", msg, "CRM", "Order Details", "BusiOrder_ManualSpec", ColumnId, UserId, now, opStatus);
                    if (opStatus == 1)
                    {
                        return Json("Success", JsonRequestBehavior.AllowGet);
                    }
                }
            }
            return Json("Error", JsonRequestBehavior.AllowGet);
        }
        public JsonResult StoreIntoStock(int? Bus_Ord_Id, int? Bus_Spec_Id, long UserId)
        {
            int opStatus = -1;
            int id = 0;
            string msg = "";
            BusiOrder_ManualSpec spec = db.BusiOrder_ManualSpec.Find(Bus_Spec_Id);
            StockItemList stock = new StockItemList();
            stock.Bus_Ord_Id = Bus_Ord_Id;
            stock.Busi_Spec_Id = Bus_Spec_Id;
            stock.StoreBy = UserId;
            stock.StoreDate = now;
            stock.Status = 0;
            db.StockItemLists.Add(stock);
            try
            {
                db.SaveChanges();
                opStatus = 1;
                msg = "Business order completed item '" + spec.Spec_lbl + "' has been successfully store into stock on " + now.ToString("MMM dd,yyyy hh:mm tt.");
                id = db.StockItemLists.Where(m => m.StoreBy == UserId).Max(m => m.StockItemId);
                spec.IsStoreIntoStock = true;
                db.Entry(spec).State = EntityState.Modified;
                db.SaveChanges();
            }
            catch (Exception ex)
            {
                string errmsg = ex.ToString();
                opStatus = -1;
                msg = "Business order completed item '" + spec.Spec_lbl + "' store into stock was unsuccessful on " + now.ToString("MMM dd,yyyy hh:mm tt.");
            }
            SaveAuditLog("Stock Business Order Item", msg, "CRM", "Order Details", "StockItemList", id, UserId, now, opStatus);
            if (opStatus == 1)
            {
                return Json("Success", JsonRequestBehavior.AllowGet);
            }
            return Json("Error", JsonRequestBehavior.AllowGet);
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