using FactoryManagement.Filters;
using FactoryManagement.Models;
using FactoryManagement.ModelView.CRM;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;

namespace FactoryManagement.Controllers
{
    [Authorize]
    public class CRMClientController : Controller
    {
        #region Private Properties
        private FactoryManagementEntities db = new FactoryManagementEntities();
        static private int offset = Convert.ToInt32(ConfigurationManager.AppSettings["localTime"]);
        DateTime now = DateTime.UtcNow.AddMinutes(offset);
        const int ShowUserPerPage = 20;
        const int ShowVehiclePerPage = 20;
        #endregion

        

        #region Client list and new add
        public ActionResult AllClientList()
        {
            var list = db.View_Client_Info.ToList();
            ViewBag.TotalClient = list.Count();
            return View(ViewBag.TotalClient);
        }
        public PartialViewResult _AllClientList(int? ClientId, int? ClientType)
        {
            var list = db.View_Client_Info.OrderBy(m => m.Name).ToList();
            if (ClientId != null)
            {
                list = list.Where(m => m.ClientId == ClientId).OrderBy(m => m.Name).ToList();
            }
            else if (ClientType == 1)
            {
                list = list.Where(m => m.IsBuyer == true).OrderBy(o => o.Name).ToList();
            }
            else if (ClientType == 2)
            {
                list = list.Where(m => m.IsSupplier == true).OrderBy(o => o.Name).ToList();
            }
            else if (ClientType == 3)
            {
                list = list.Where(m => m.IsOutSourcer == true).OrderBy(o => o.Name).ToList();
            }
            else if (ClientType == 4)
            {
                list = list.Where(m => m.IsLogistic == true).OrderBy(o => o.Name).ToList();
            }
            ViewBag.TotalClient = list.Count();
            Session["TotalVendorCount"] = list.Count();
            ViewBag.ClientList = list.Take(ShowUserPerPage).OrderBy(m => m.Name).ToList();
            Session["AllVendor"] = list;
            return PartialView(ViewBag.ClientList);
        }
        public JsonResult GetVendorList(int? page)
        {
            page = page ?? 0;
            int total = Convert.ToInt16(Session["TotalVendorCount"]);
            int skip = ShowUserPerPage * Convert.ToInt16(page);
            var canPage = skip < total;
            if (canPage)
            {
                var listBackFromSession = (List<View_Client_Info>)Session["AllVendor"];
                var userList = listBackFromSession.Skip(skip).Take(ShowUserPerPage).OrderBy(m => m.Name).ToList();
                return Json(userList, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json("", JsonRequestBehavior.AllowGet);
            }
        }
        public JsonResult SearchVendorList(string prefix)
        {
            if (prefix == "")
            {
                return Json("", JsonRequestBehavior.AllowGet);
            }
            else
            {
                var allVendor = (List<View_Client_Info>)Session["AllVendor"];
                var vendor = (from m in allVendor
                              where (m.Name.ToLower().Contains(prefix.ToLower()))
                              select m).ToList();
                Session["AllVendorBySearch"] = vendor;
                return Json(vendor, JsonRequestBehavior.AllowGet);
            }
        }
        [EncryptedActionParameter]
        public ActionResult ClientDetails(int? Id, bool isDisplay)
        {

            if (Id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            else
            {
                ClientListModelView model = new ClientListModelView();
                model.ClientId = Convert.ToInt32(Id);
                model.Name = db.ClientLists.Where(m => m.ClientId == Id).Select(m => m.Name).FirstOrDefault();
                model.ClientType = db.View_Client_Info.Where(m => m.ClientId == Id).Select(m => m.ClientType).FirstOrDefault();
                model.IsLogistic = db.ClientLists.Where(m => m.ClientId == Id).Select(m => m.IsLogistic).FirstOrDefault();
                model.IsDisplay = isDisplay;
                return View(model);
            }
        }
        public ActionResult ClientCreate()
        {
            ClientListModelView model = new ClientListModelView();
            ViewBag.DivisionList = new SelectList(db.DivisionLists, "DivisionId", "DivisionName", model.DivisionId);
            ViewBag.CountryId = new SelectList(db.CountryLists, "CountryCode", "CountryName", model.Country);
            return View(model);
        }
        public JsonResult AddNewClient(ClientList model, string[] AllContactList)
        {
            string msg = "";
            string msgName = "";
            int CListColumnId = 0;
            int OperationStatus = 1;
            db.ClientLists.Add(model);
            try
            {
                db.SaveChanges();
                CListColumnId = db.ClientLists.Where(m => m.CreatedBy == model.CreatedBy).Max(m => m.ClientId);
                msgName = "Client '" + model.Name + "' has been added successfully on " + now.ToString("MMM,dd,yyyy hh:mm tt");
                if (AllContactList != null && AllContactList.Length > 0)
                {
                    for (int i = 0; i < AllContactList.Length; i++)
                    {
                        ClientContactList contact = new ClientContactList();
                        contact.ClientId = model.ClientId;
                        contact.Contact = AllContactList[i].Split('|')[0];
                        contact.ContactTypeId = Convert.ToInt32(AllContactList[i].Split('|')[1]);
                        if (contact.ContactTypeId == 1)
                        {
                            contact.ContactTypeName = "Land";
                        }
                        else
                        {
                            contact.ContactTypeName = "Mobile";
                        }
                        contact.Comments = AllContactList[i].Split('|')[2];
                        db.ClientContactLists.Add(contact);
                        db.SaveChanges();
                    }
                }
            }
            catch (Exception)
            {
                OperationStatus = -1;
                msg = "Client '" + model.Name + "' adding  failed on " + now.ToString("MMM,dd,yyyy hh:mm tt");
            }
            SaveAuditLog("Add New Client", msgName, "CRMClient", "Client Information", "ClientList", CListColumnId, model.CreatedBy, now, OperationStatus);
            if (OperationStatus == 1)
            {
                return Json("Success", JsonRequestBehavior.AllowGet);
            }

            return Json("Error", JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region Client Edit and Delete
        public PartialViewResult LaunchEditView(int? Id, bool isDisplay)
        {
            ViewBag.id = Id;
            ViewBag.isDisplay = isDisplay;
            return PartialView();
        }
        public PartialViewResult _EditClientBasicInfo(int? Id, bool isDisplay)
        {
            if ((Id > 0) && (isDisplay == false))
            {
                View_Client_Info client = db.View_Client_Info.Where(m => m.ClientId == Id).FirstOrDefault();
                ClientListModelView model = new ClientListModelView();
                model.ClientId = client.ClientId;
                model.Name = client.Name;
                model.ClientCode = client.ClientCode;
                model.ClientType = client.ClientType;
                model.IsBuyer = client.IsBuyer;
                model.IsSupplier = client.IsSupplier;
                model.IsOutSourcer = client.IsOutSourcer;
                model.IsLogistic = client.IsLogistic;
                model.Address = client.Address;
                model.AddressLine1 = client.AddressLine1;
                model.Country = client.Country;
                model.CountryName = client.CountryName;
                model.DivisionId = client.DivisionId;
                model.DivisionName = client.DivisionName;
                model.City = client.City;
                model.PostalCode = client.PostalCode;
                model.State = client.State;
                model.Email = client.Email;
                model.Website = client.Website;
                model.Fax = client.Fax;
                ViewBag.DivisionList = new SelectList(db.DivisionLists, "DivisionId", "DivisionName", client.DivisionId);
                ViewBag.CountryId = new SelectList(db.CountryLists, "CountryCode", "CountryName", client.Country);
                return PartialView(model);
            }
            return PartialView("Id dosen't match");
        }
        public PartialViewResult _ClientBasicInfoDisplay(int Id, bool isDisplay)
        {
            View_Client_Info client = db.View_Client_Info.Where(m => m.ClientId == Id).FirstOrDefault();
            ViewBag.DivisionList = new SelectList(db.DivisionLists, "DivisionId", "DivisionName", client.DivisionId);
            ViewBag.CountryId = new SelectList(db.CountryLists, "CountryCode", "CountryName", client.Country);
            ViewBag.isDisplay = isDisplay;
            return PartialView(client);
        }
        public ActionResult DisplayClientInformation(int? Id)
        {
            ClientListModelView model = new ClientListModelView();
            View_Client_Info client = db.View_Client_Info.Where(m => m.ClientId == Id).FirstOrDefault();
            ClientContactList contact = db.ClientContactLists.Where(m => m.ClientId == Id).FirstOrDefault();
            model.ClientId = client.ClientId;
            model.Name = client.Name;
            model.ClientCode = client.ClientCode;
            model.ClientType = client.ClientType;
            model.Address = client.Address;
            model.AddressLine1 = client.AddressLine1;
            model.Country = client.Country;
            model.CountryName = client.CountryName;
            model.DivisionId = client.DivisionId;
            model.DivisionName = client.DivisionName;
            model.City = client.City;
            model.ClientType = client.ClientType;
            model.IsLogistic = client.IsLogistic;
            model.PostalCode = client.PostalCode;
            model.State = client.State;
            model.Email = client.Email;
            model.Website = client.Website;
            model.Fax = client.Fax;
            model.CientContactId = contact.CientContactId;
            model.Contact = contact.Contact;
            model.ContactTypeId = contact.ContactTypeId;
            model.ContactTypeName = (contact.ContactTypeId == 1) ? "Land" : "Mobile";
            model.Comments = contact.Comments;
            return View(model);
        }
        //************************** Client Info Edit ***********************
        public JsonResult CheckAnyVehicleExsits(int ClientId)
        {
            ClientList data = db.ClientLists.Find(ClientId);
            if ((data.IsLogistic) && db.Client_Vehicle_Lists.Where(m => m.ClientId == data.ClientId).Any())
            {
                return Json(data, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json("", JsonRequestBehavior.AllowGet);
            }
        }
        public JsonResult EditBasicInfoUpdate(ClientListModelView model)
        {
            string msg = "";
            int ColumnId = 0;
            int OperationStatus = 1;
            ClientList client = db.ClientLists.Find(model.ClientId);
            if ((model.IsLogistic == false) && db.Client_Vehicle_Lists.Where(m => m.ClientId == client.ClientId).Any())
            {
                var all_vhicle_list = db.Client_Vehicle_Lists.Where(m => m.ClientId == client.ClientId).ToList();
                foreach (var v in all_vhicle_list)
                {
                    db.Client_Vehicle_Lists.Remove(v);
                    db.SaveChanges();
                }
            }
            client.ClientId = model.ClientId;
            client.Name = model.Name;
            client.ClientCode = model.ClientCode;
            client.IsBuyer = model.IsBuyer;
            client.IsSupplier = model.IsSupplier;
            client.IsLogistic = model.IsLogistic;
            client.IsOutSourcer = model.IsOutSourcer;
            client.Address = model.Address;
            client.AddressLine1 = model.AddressLine1;
            client.Country = model.Country;
            client.City = model.City;
            client.DivisionId = model.DivisionId;
            client.State = model.State;
            client.Fax = model.Fax;
            client.Email = model.Email;
            client.Website = model.Website;
            client.PostalCode = model.PostalCode;
            client.UpdatedBy = model.CreatedBy;
            client.UpdatedDate = now;
            db.Entry(client).State = EntityState.Modified;
            try
            {
                db.SaveChanges();
                OperationStatus = 1;
                msg = "Basic information of client '" + model.Name + "' has been successfully updated on " + now.ToString("MMM dd, yyyy hh:mm tt");

            }
            catch (Exception ex)
            {
                string errmsg = ex.ToString();
                OperationStatus = -1;
                msg = "Basic information of client '" + model.Name + "' update was unssuccessful on " + now.ToString("MMM dd, yyyy hh:mm tt");
            }
            ColumnId = Convert.ToInt32(model.ClientId);
            SaveAuditLog("Client Info Update", msg, "CRMClient", "Client Info Details", "ClientList", ColumnId, model.CreatedBy, now, OperationStatus);
            if (OperationStatus == 1)
            {
                var nId = model.ClientId;
                return Json(nId, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json("", JsonRequestBehavior.AllowGet);
            }
        }
        public JsonResult DeleteClient(int Id)
        {
            string msg = "";
            int ColumnId = 0;
            int OperationStatus = 1;
            if (db.ClientLists.Where(m => m.ClientId == Id).Any())
            {
                if (db.BusinessOrderLists.Where(m => m.ClientId == Id).Any() || db.BusinessOrderSupplierLists.Where(m => m.ClientId == Id).Any())
                {
                    return Json("Error", JsonRequestBehavior.AllowGet);
                }
                else
                {
                    ClientList client = db.ClientLists.Find(Id);
                    ColumnId = client.ClientId;
                    db.ClientLists.Remove(client);
                    var clientContact = db.ClientContactLists.Where(m => m.ClientId == Id).ToList();
                    var clientUser = db.Client_User_Lists.Where(m => m.ClientId == Id).ToList();
                    var clientVehicle = db.Client_Vehicle_Lists.Where(m => m.ClientId == Id).ToList();
                    if (clientContact.Count() > 0)
                    {
                        foreach (var contact in clientContact)
                        {
                            db.ClientContactLists.Remove(contact);
                            db.SaveChanges();
                        }
                    }
                    if (clientUser.Count() > 0)
                    {
                        foreach (var user in clientUser)
                        {
                            string FilePath = Path.Combine(Server.MapPath("~/Images/Client/ClientUserImage/Original/"), user.PictureName);
                            if (System.IO.File.Exists(FilePath))
                            {
                                System.IO.File.Delete(FilePath);
                            }
                            string FilePathThumb = Path.Combine(Server.MapPath("~/Images/Client/ClientUserImage/thumb/"), user.PictureName);
                            if (System.IO.File.Exists(FilePathThumb))
                            {
                                System.IO.File.Delete(FilePathThumb);
                            }
                            var user_ContactList = db.Client_User_ContactList.Where(m => m.UserId == user.UserId).ToList();
                            if (user_ContactList.Count() > 0)
                            {
                                foreach (var user_contact in user_ContactList)
                                {
                                    db.Client_User_ContactList.Remove(user_contact);
                                    db.SaveChanges();
                                }
                            }
                            db.Client_User_Lists.Remove(user);
                            db.SaveChanges();
                        }
                    }
                    if (client.IsLogistic && clientVehicle.Count() > 0)
                    {
                        foreach (var vehicle in clientVehicle)
                        {
                            string FilePath = Path.Combine(Server.MapPath("~/Images/Client/ClientVehicleImage/Original/"), vehicle.PictureName);
                            if (System.IO.File.Exists(FilePath))
                            {
                                System.IO.File.Delete(FilePath);
                            }
                            string FilePathThumb = Path.Combine(Server.MapPath("~/Images/Client/ClientVehicleImage/thumb/"), vehicle.PictureName);
                            if (System.IO.File.Exists(FilePathThumb))
                            {
                                System.IO.File.Delete(FilePathThumb);
                            }
                            db.Client_Vehicle_Lists.Remove(vehicle);
                            db.SaveChanges();
                        }
                    }
                    try
                    {
                        db.SaveChanges();
                        OperationStatus = 1;
                        msg = "Basic information of client '" + client.Name + "' has been successfully deleted on" + now.ToString("MMM,dd,yyyy hh:mm tt");

                    }
                    catch (Exception ex)
                    {
                        string errmsg = ex.ToString();
                        OperationStatus = -1;
                        ColumnId = 0;
                        msg = "Basic information of client '" + client.Name + "' deletion was unsuccessful on " + now.ToString("MMM,dd,yyyy hh:mm tt");

                    }
                    SaveAuditLog("Delete Client", msg, "CRMClient", "Client Information", "ClientList", ColumnId, client.CreatedBy, now, OperationStatus);
                    if (OperationStatus == 1)
                    {
                        return Json("Success", JsonRequestBehavior.AllowGet);
                    }
                }
            }
            return Json("Error", JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region Client Contact Add and View
        public PartialViewResult _ClientContactInfoDisplay(int? Id, bool isDisplay)
        {
            if (Id == null)
            {
                throw new HttpException(400, "Client contact Id is Null");
            }
            else
            {
                var clientContactList = db.ClientContactLists.Where(m => m.ClientId == Id).ToList();
                ViewBag.isDisplay = isDisplay;
                return PartialView(clientContactList);
            }
        }
        public PartialViewResult ClientNewContact(int? ClientId)
        {
            ClientListModelView model = new ClientListModelView();
            var clientId = db.ClientLists.Find(ClientId);
            model.ClientId = clientId.ClientId;
            return PartialView(model);
        }
        public JsonResult AddClientContact(ClientListModelView model, int? CientContactId)
        {
            string msg = "";
            string name = "";
            int ColumnId = 0;

            int OperationStatus = 1;
            ClientContactList contact = new ClientContactList();
            if (CientContactId > 0)
            {
                contact = db.ClientContactLists.Where(m => m.CientContactId == CientContactId).SingleOrDefault();
            }
            contact.ClientId = model.ClientId;
            contact.Contact = model.Contact;
            contact.ContactTypeId = model.ContactTypeId;
            contact.ContactTypeName = (model.ContactTypeId == 1) ? "Land" : "Mobile";
            contact.Comments = model.Comments;
            if (CientContactId > 0)
            {
                db.Entry(contact).State = EntityState.Modified;
                name = db.ClientLists.Where(m => m.ClientId == model.ClientId).Select(m => m.Name).SingleOrDefault();
                try
                {
                    db.SaveChanges();
                    OperationStatus = 1;
                    msg = "Contact information of client '" + name + "' has been updated  successfully on " + now.ToString("MMM dd, yyyy hh:mm tt");

                }
                catch (Exception ex)
                {
                    string errmsg = ex.ToString();
                    OperationStatus = -1;
                    msg = "Contact information of client '" + name + "' update was unssuccessful on " + now.ToString("MMM dd, yyyy hh:mm tt");

                }
                ColumnId = Convert.ToInt32(model.CientContactId);
                SaveAuditLog("Update Client Contact", msg, "CRMClient", "Client Information", "ClientContactList", ColumnId, model.CreatedBy, now, OperationStatus);
            }
            else
            {
                db.ClientContactLists.Add(contact);
                name = db.ClientLists.Where(m => m.ClientId == model.ClientId).Select(m => m.Name).SingleOrDefault();
                try
                {
                    db.SaveChanges();
                    ColumnId = db.ClientContactLists.Max(m => m.CientContactId);
                    OperationStatus = 1;
                    msg = "Contact information of client '" + name + "' has been added successfully on " + now.ToString("MMM dd, yyyy hh:mm tt");

                }
                catch (Exception ex)
                {
                    string errmsg = ex.ToString();
                    OperationStatus = -1;
                    msg = "Contact information of client '" + name + "' adding  failed on  on " + now.ToString("MMM dd, yyyy hh:mm tt");

                }

                SaveAuditLog("Add Client New Contact", msg, "CRMClient", "Client Information", "ClientContactList", ColumnId, model.CreatedBy, now, OperationStatus);
            }

            if (OperationStatus == 1)
            {
                int nId = Convert.ToInt32(model.ClientId);
                return Json(nId, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json("", JsonRequestBehavior.AllowGet);
            }
        }
        public PartialViewResult ClientNewContactEditLoad(int? ClientContactId)
        {
            ClientListModelView model = new ClientListModelView();
            if (ClientContactId > 0)
            {
                model.CientContactId = Convert.ToInt32(ClientContactId);
            }

            return PartialView(model);
        }
        public PartialViewResult ClientContactEditView(int? ClientContactId)
        {
            ClientListModelView model = new ClientListModelView();
            if (ClientContactId > 0)
            {
                ClientContactList contact = db.ClientContactLists.Find(ClientContactId);
                model.CientContactId = contact.CientContactId;
                model.Contact = contact.Contact;
                model.ContactTypeId = contact.ContactTypeId;
                model.ClientId = Convert.ToInt32(contact.ClientId);
                model.Comments = contact.Comments;
            }
            return PartialView(model);
        }
        #endregion

        #region Client Contact Delete
        public JsonResult DeleteClientContact(int ClientContactId)
        {
            string msg = "";

            int ColumnId = 0;
            int OperationStatus = 1;
            if (!db.ClientContactLists.Where(m => m.CientContactId == ClientContactId).Any())
            {
                return Json("Error", JsonRequestBehavior.AllowGet);
            }
            else
            {
                ClientContactList contact = db.ClientContactLists.Find(ClientContactId);
                ClientList client = db.ClientLists.Where(m => m.ClientId == contact.ClientId).SingleOrDefault();
                if (contact.CientContactId > 0)
                {

                    db.ClientContactLists.Remove(contact);
                    try
                    {
                        db.SaveChanges();
                        OperationStatus = 1;
                        msg = "Contact  of client'" + client.Name + "'has been deleted successfully  on " + now.ToString("MMM dd, yyyy hh:mm tt");

                    }
                    catch (Exception ex)
                    {
                        string errmsg = ex.ToString();
                        OperationStatus = -1;
                        msg = "Contact  of client'" + client.Name + "'deletion was unssuccessful on " + now.ToString("MMM dd, yyyy hh:mm tt");
                    }

                    ColumnId = Convert.ToInt32(ClientContactId);
                    SaveAuditLog("Delete Client Contact", msg, "CRMClient", "Client Information", "ClientContactList", ColumnId, client.CreatedBy, now, OperationStatus);
                }
                return Json("Success", JsonRequestBehavior.AllowGet);
            }
        }
        #endregion

        #region User List
        //*************** UserList************************
        public ActionResult UserList(int? pageNum, int Id, bool isDisplay)
        {
            pageNum = pageNum ?? 0;
            int page = Convert.ToInt32(pageNum);
            ViewBag.pageNum = pageNum;
            var user = (from m in db.Client_User_Lists
                        where m.ClientId == Id
                        select new Client_User_ListsModelView
                        {
                            UserId = m.UserId,
                            ClientId = m.ClientId,
                            UserName = m.UserName,
                            Designation = m.Designation,
                            PictureName = m.PictureName
                        }).ToList();
            ViewBag.isDisplay = isDisplay;
            ViewBag.TotalUser = user.Count();
            Session["AllUser"] = user;
            Session["TotalUserCount"] = user.Count();
            return PartialView(user);
        }
        public PartialViewResult _UserList(int? UserId, bool isDisplay)
        {
            if (UserId != null)
            {
                var allUser = (List<Client_User_ListsModelView>)Session["AllUserBySearch"];
                var allUserBySearch = (from m in allUser
                                       where m.UserId == UserId
                                       select new Client_User_ListsModelView
                                       {
                                           UserId = m.UserId,
                                           UserName = m.UserName,
                                           Designation = m.Designation,
                                           PictureName = m.PictureName
                                       }).ToList();
                ViewBag.isDisplay = isDisplay;
                ViewBag.TotalUser = allUserBySearch.Count();
                ViewBag.UserList = allUserBySearch.Take(ShowUserPerPage).ToList();
            }
            else
            {
                var allUser = (List<Client_User_ListsModelView>)Session["AllUser"];
                if (allUser != null)
                {
                    ViewBag.UserList = allUser.Take(ShowUserPerPage).ToList();
                    ViewBag.isDisplay = isDisplay;
                }
                else
                {
                    throw new HttpException(400, "allUser in _UserList is Null");
                }
            }
            return PartialView(ViewBag.VendorList);
        }
        public JsonResult GetUserList(int? page)
        {
            page = page ?? 0;
            int total = Convert.ToInt16(Session["TotalUserCount"]);
            int skip = ShowUserPerPage * Convert.ToInt16(page);
            var canPage = skip < total;
            if (canPage)
            {
                var listBackFromSession = (List<Client_User_ListsModelView>)Session["AllUser"];
                var userList = listBackFromSession.Skip(skip).Take(ShowUserPerPage).ToList();
                return Json(userList, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json("", JsonRequestBehavior.AllowGet);
            }
        }
        public JsonResult SearchUserList(string prefix)
        {
            if (prefix == "")
            {
                return Json("", JsonRequestBehavior.AllowGet);
            }
            else
            {
                var allUser = (List<Client_User_ListsModelView>)Session["AllUser"];
                var user = (from s in allUser
                            where (s.UserName.ToLower().Contains(prefix.ToLower()) || s.Designation.ToLower().Contains(prefix.ToLower()))
                            select new Client_User_ListsModelView
                            {
                                UserId = s.UserId,
                                UserName = s.UserName + " " + "(" + s.Designation + ")",
                                Designation = s.Designation,
                                PictureName = s.PictureName
                            }).ToList();
                Session["AllUserBySearch"] = user;
                return Json(user, JsonRequestBehavior.AllowGet);
            }
        }
        //**************************** UserList For Display *******************
        public ActionResult DisplayUserList(int? pageNum, int Id)
        {
            pageNum = pageNum ?? 0;
            int page = Convert.ToInt32(pageNum);
            ViewBag.pageNum = pageNum;
            var user = (from m in db.Client_User_Lists
                        where m.ClientId == Id
                        select new Client_User_ListsModelView
                        {
                            UserId = m.UserId,
                            UserName = m.UserName,
                            Designation = m.Designation,
                            PictureName = m.PictureName,
                        }).ToList();
            ViewBag.TotalUser = user.Count();
            Session["AllUser"] = user;
            Session["TotalUserCount"] = user.Count();
            return PartialView();
        }
        public PartialViewResult _DisplayUserList(int? UserId)
        {
            if (UserId != null)
            {
                var allUser = (List<Client_User_ListsModelView>)Session["AllUserBySearch"];
                var allUserBySearch = (from m in allUser
                                       where m.UserId == UserId
                                       select new Client_User_ListsModelView
                                       {
                                           UserId = m.UserId,
                                           UserName = m.UserName,
                                           Designation = m.Designation,
                                           PictureName = m.PictureName,
                                       }).ToList();
                ViewBag.TotalUser = allUserBySearch.Count();
                ViewBag.UserList = allUserBySearch.Take(ShowUserPerPage).ToList();
            }
            else
            {
                var allUser = (List<Client_User_ListsModelView>)Session["AllUser"];

                if (allUser != null)
                {
                    ViewBag.UserList = allUser.Take(ShowUserPerPage).ToList();
                }
                else
                {
                    throw new HttpException(400, "allUser in display user list is Null");
                }
            }
            return PartialView(ViewBag.VendorList);
        }
        public JsonResult GetDisplayUserList(int? page)
        {
            page = page ?? 0;
            int total = Convert.ToInt16(Session["TotalUserCount"]);
            int skip = ShowUserPerPage * Convert.ToInt16(page);
            var canPage = skip < total;
            if (canPage)
            {
                var listBackFromSession = (List<Client_User_ListsModelView>)Session["AllUser"];
                var userList = listBackFromSession.Skip(skip).Take(ShowUserPerPage).ToList();
                return Json(userList, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json("", JsonRequestBehavior.AllowGet);
            }
        }
        public JsonResult SearchDisplayUserList(string prefix)
        {
            if (prefix == "")
            {
                return Json("", JsonRequestBehavior.AllowGet);
            }
            else
            {
                var allUser = (List<Client_User_ListsModelView>)Session["AllUser"];
                var user = (from s in allUser
                            where (s.UserName.ToLower().Contains(prefix.ToLower()) || s.Designation.ToLower().Contains(prefix.ToLower()))
                            select new Client_User_ListsModelView
                            {
                                UserId = s.UserId,
                                UserName = s.UserName + " " + "(" + s.Designation + ")",
                                Designation = s.Designation,
                                PictureName = s.PictureName
                            }).ToList();
                Session["AllUserBySearch"] = user;
                return Json(user, JsonRequestBehavior.AllowGet);
            }
        }
        #endregion

        #region Add new user
        public PartialViewResult AddNewUserPopUp(int ClientId)
        {
            ViewBag.clientId = ClientId;
            return PartialView();
        }
        public PartialViewResult _AddNewUser(int ClientId)
        {
            Client_User_ListsModelView model = new Client_User_ListsModelView();
            model.ClientId = ClientId;
            ViewBag.DivisionList = new SelectList(db.DivisionLists, "DivisionId", "DivisionName", model.DivisionId);
            ViewBag.CountryId = new SelectList(db.CountryLists, "CountryCode", "CountryName", model.Country);
            return PartialView(model);
        }
        public JsonResult AddNewUser(Client_User_ListsModelView model, HttpPostedFileBase files)
        {
            string msg = "";

            int ColumnId = 0;
            int OperationStatus = 1;
            string fullName = "";

            Client_User_Lists client = new Client_User_Lists();
            client.ClientId = model.ClientId;
            client.UserName = model.UserName;
            client.Designation = model.Designation;
            client.Address = model.Address;
            client.AddressLine1 = model.AddressLine1;
            client.Country = model.Country;
            client.State = model.State;
            client.DivisionId = model.DivisionId;
            client.City = model.City;
            client.PostalCode = model.PostalCode;
            client.Fax = model.Fax;
            client.CreatedBy = model.CreatedBy;
            client.CreatedDate = now;
            client.Email = model.Email;
            if (files != null)
            {
                Random generator = new Random();
                fullName = generator.Next(1000, 9999) + "_" + model.ClientId + files.FileName;
            }
            client.PictureName = fullName;
            db.Client_User_Lists.Add(client);

            try
            {
                db.SaveChanges();
                ColumnId = db.Client_User_Lists.Where(m => m.CreatedBy == model.CreatedBy).Max(m => m.UserId);
                string Paths = Server.MapPath("~/Images/Client");
                if (!Directory.Exists(Paths))
                {
                    Directory.CreateDirectory(Paths);
                }
                if (files != null)
                {
                    Paths = Path.Combine(Server.MapPath("~/Images/Client/ClientUserImage/Original"), fullName);
                    files.SaveAs(Paths);
                    WebImage imgForGallery = new WebImage(files.InputStream);
                    if (imgForGallery.Width > 70)
                    {
                        imgForGallery.Resize(45, 45, false, true);
                    }
                    string PathsResize = Path.Combine(Server.MapPath("~/Images/Client/ClientUserImage/thumb"), fullName);
                    imgForGallery.Save(PathsResize);
                }
                if (model.AllContactList != "")
                {
                    string[] contactlist = model.AllContactList.Split(',');
                    for (int i = 0; i < contactlist.Length - 1; i++)
                    {
                        Client_User_ContactList contact = new Client_User_ContactList();
                        contact.UserId = ColumnId;
                        contact.Contact = contactlist[i].Split('|')[0];
                        contact.ContactTypeId = Convert.ToInt32(contactlist[i].Split('|')[1]);
                        contact.ContactTypeName = (contact.ContactTypeId == 1) ? "Land" : "Mobile";
                        contact.Comments = contactlist[i].Split('|')[2];
                        db.Client_User_ContactList.Add(contact);
                        db.SaveChanges();
                    }
                }
                msg = "New user '" + model.UserName + "' has been added successfully on " + now.ToString("MMM dd, yyyy hh:mm tt");

            }
            catch (Exception ex)
            {
                string errorMsg = ex.Message.ToString();
                msg = "New user '" + model.UserName + "' adding  failed on " + now.ToString("MMM dd, yyyy hh:mm tt");
                OperationStatus = -1;
            }
            SaveAuditLog("Add New User", msg, "CRMClient", "Client Information", "Client_User_Lists", ColumnId, client.CreatedBy, now, OperationStatus);

            if (OperationStatus == 1)
            {
                return Json("Success", JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json("Error", JsonRequestBehavior.AllowGet);
            }
        }
        #endregion

        #region User Info Display, edit and delete
        //*********************** User info Edit ********************
        public PartialViewResult _DisplayUserInformation(int ClientId, int UserId)
        {
            Client_User_ListsModelView model = new Client_User_ListsModelView();
            Client_User_Lists client = db.Client_User_Lists.Find(UserId);
            Client_User_ContactList contact = db.Client_User_ContactList.Where(m => m.UserId == UserId).FirstOrDefault();
            model.UserId = client.UserId;
            model.UserName = client.UserName;
            model.ClientId = db.ClientLists.Where(m => m.ClientId == ClientId).Select(m => m.ClientId).SingleOrDefault();
            model.Address = client.Address;
            model.AddressLine1 = client.AddressLine1;
            model.City = client.City;
            model.Country = client.Country;
            model.CountryName = db.CountryLists.Where(m => m.CountryCode == model.Country).Select(m => m.CountryName).SingleOrDefault();
            model.DivisionId = client.DivisionId;
            model.DivisionName = db.DivisionLists.Where(m => m.DivisionId == model.DivisionId).Select(m => m.DivisionName).SingleOrDefault();
            model.Designation = client.Designation;
            model.Email = client.Email;
            model.Fax = client.Fax;
            model.PictureName = client.PictureName;
            model.PostalCode = client.PostalCode;
            model.State = client.State;
            model.UpdatedBy = client.CreatedBy;
            model.UpdatedDate = now;
            model.Id = contact.Id;
            model.Contact = contact.Contact;
            model.ContactTypeId = contact.ContactTypeId;
            model.ContactTypeName = (model.ContactTypeId == 1) ? "Land" : "Mobile";
            model.Comments = contact.Comments;
            return PartialView(model);
        }
        public PartialViewResult _EditUserDetails(int UserId)
        {
            Client_User_ListsModelView model = new Client_User_ListsModelView();
            Client_User_Lists client = db.Client_User_Lists.Find(UserId);
            model.UserId = client.UserId;
            model.ClientId = client.ClientId;
            model.PictureName = client.PictureName;
            return PartialView(model);
        }
        public PartialViewResult _EditUserBasicInfo(int? UserId)
        {
            if (UserId != null)
            {

                Client_User_ListsModelView model = new Client_User_ListsModelView();
                Client_User_Lists client = db.Client_User_Lists.Find(UserId);
                model.UserId = client.UserId;
                model.UserName = client.UserName;
                model.ClientId = client.ClientId;
                model.Address = client.Address;
                model.AddressLine1 = client.AddressLine1;
                model.City = client.City;
                model.Country = client.Country;
                model.CountryName = db.CountryLists.Where(m => m.CountryCode == model.Country).Select(m => m.CountryName).SingleOrDefault();
                model.DivisionId = client.DivisionId;
                model.DivisionName = db.DivisionLists.Where(m => m.DivisionId == model.DivisionId).Select(m => m.DivisionName).SingleOrDefault();
                model.Designation = client.Designation;
                model.Email = client.Email;
                model.Fax = client.Fax;
                model.PostalCode = client.PostalCode;
                model.State = client.State;
                model.UpdatedBy = client.CreatedBy;
                model.UpdatedDate = now;
                ViewBag.DivisionList = new SelectList(db.DivisionLists, "DivisionId", "DivisionName");
                ViewBag.CountryId = new SelectList(db.CountryLists, "CountryCode", "CountryName");
                return PartialView(model);
            }
            return PartialView();
        }
        public JsonResult UpdateUserBasicInfo(Client_User_ListsModelView model)
        {
            string msg = "";
            int ColumnId = model.UserId;
            int OperationStatus = 1;
            Client_User_Lists client = db.Client_User_Lists.Find(model.UserId);
            client.UserName = model.UserName;
            client.Designation = model.Designation;
            client.ClientId = model.ClientId;
            client.UserId = model.UserId;
            client.Address = model.Address;
            client.AddressLine1 = model.AddressLine1;
            client.Country = model.Country;
            client.City = model.City;
            client.DivisionId = model.DivisionId;
            client.Fax = model.Fax;
            client.Email = model.Email;
            client.State = model.State;
            client.UpdatedBy = model.CreatedBy;
            client.UpdatedDate = now;
            client.PostalCode = model.PostalCode;
            db.Entry(client).State = EntityState.Modified;
            try
            {
                db.SaveChanges();
                OperationStatus = 1;
                msg = "Basic information of user '" + model.UserName + "' has been updated successfully on " + now.ToString("MMM dd, yyyy hh:mm tt");
            }
            catch (Exception ex)
            {
                string errmsg = ex.ToString();
                OperationStatus = -1;
                msg = "Basic information of user '" + model.UserName + "' update was unsuccessfull on " + now.ToString("MMM dd, yyyy hh:mm tt");
            }

            SaveAuditLog("Update User Basic Info", msg, "CRMClient", "Client Information", "Client_User_Lists", ColumnId, model.CreatedBy, now, OperationStatus);

            if (OperationStatus == 1)
            {
                int nId = model.ClientId;
                return Json(nId, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json("", JsonRequestBehavior.AllowGet);
            }
        }
        public PartialViewResult _EditUserContactList(int? UserId)
        {
            if (UserId != null)
            {

                Client_User_ContactList model = db.Client_User_ContactList.Where(m => m.UserId == UserId).FirstOrDefault();
                Client_User_ListsModelView client = new Client_User_ListsModelView();
                client.UserId = Convert.ToInt32(model.UserId);
                client.Id = model.Id;
                client.Contact = model.Contact;
                client.ContactTypeId = model.ContactTypeId;
                client.ContactTypeName = (model.ContactTypeId == 1) ? "Land" : "Mobile";
                client.Comments = model.Comments;
                return PartialView(client);
            }
            return PartialView();
        }
        public JsonResult AddUserNewContact(Client_User_ListsModelView model)
        {
            string msg = "";
            string name = "";
            int ColumnId = 0;
            int OperationStatus = 1;

            Client_User_ContactList contact = new Client_User_ContactList();
            contact.UserId = model.UserId;
            contact.Contact = model.Contact;
            contact.ContactTypeId = model.ContactTypeId;
            contact.ContactTypeName = (model.ContactTypeId == 1) ? "Land" : "Mobile";
            contact.Comments = model.Comments;
            db.Client_User_ContactList.Add(contact);
            name = db.Client_User_Lists.Where(m => m.UserId == model.UserId).Select(m => m.UserName).SingleOrDefault();
            try
            {
                db.SaveChanges();
                ColumnId = db.Client_User_ContactList.Where(m => m.UserId == model.UserId).Max(m => m.Id);
                OperationStatus = 1;
                msg = "New contact of user '" + name + "' has been added successfully on " + now.ToString("MMM dd, yyyy hh:mm tt");
            }
            catch (Exception ex)
            {
                string errmsg = ex.ToString();
                OperationStatus = -1;
                msg = "New contact of user '" + name + "' adding  failed on " + now.ToString("MMM dd, yyyy hh:mm tt");
            }
            SaveAuditLog("Add User New Contact", msg, "CRMClient", "Client Information", "Client_User_ContactList", ColumnId, model.CreatedBy, now, OperationStatus);
            if (OperationStatus == 1)
            {
                int nId = Convert.ToInt32(model.UserId);
                return Json(nId, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json("Error", JsonRequestBehavior.AllowGet);
            }
        }
        public JsonResult UpdateUserContactInfo(Client_User_ListsModelView model)
        {
            string msg = "";
            string name = "";
            int ColumnId = Convert.ToInt32(model.Id);
            int OperationStatus = 1;

            Client_User_ContactList contact = db.Client_User_ContactList.Where(m => m.Id == model.Id).SingleOrDefault();
            contact.Contact = model.Contact;
            contact.ContactTypeId = model.ContactTypeId;
            contact.ContactTypeName = (model.ContactTypeId == 1) ? "Land" : "Mobile";
            contact.Comments = model.Comments;
            db.Entry(contact).State = EntityState.Modified;
            name = db.Client_User_Lists.Where(m => m.UserId == model.UserId).Select(m => m.UserName).SingleOrDefault();
            try
            {
                db.SaveChanges();
                OperationStatus = 1;
                msg = "Contact of user '" + name + "' has beed updated successfully on " + now.ToString("MMM dd, yyyy hh:mm tt");

            }
            catch (Exception ex)
            {
                string errmsg = ex.ToString();
                OperationStatus = -1;
                msg = "Contact of user '" + name + "' update was unsuccessfully on " + now.ToString("MMM dd, yyyy hh:mm tt");
            }

            SaveAuditLog("Update User Contact ", msg, "CRMClient", "Client Information", "Client_User_ContactList", ColumnId, model.CreatedBy, now, OperationStatus);
            if (OperationStatus == 1)
            {
                return Json("Success", JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json("Error", JsonRequestBehavior.AllowGet);
            }
        }
        public JsonResult DeleteUserContact(int Id)
        {
            string msg = "";

            int ColumnId = Convert.ToInt32(Id);
            int OperationStatus = 1;
            if (!db.Client_User_ContactList.Where(m => m.Id == Id).Any())
            {
                return Json("Error", JsonRequestBehavior.AllowGet);
            }
            else
            {
                Client_User_ContactList contact = db.Client_User_ContactList.Find(Id);
                Client_User_Lists client = db.Client_User_Lists.Where(m => m.UserId == contact.UserId).SingleOrDefault();
                if (ModelState.IsValid)
                {

                    db.Client_User_ContactList.Remove(contact);
                    try
                    {
                        db.SaveChanges();
                        OperationStatus = 1;
                        msg = "Contact of user '" + client.UserName + "' has been deleted successfully on " + now.ToString("MMM dd, yyyy hh:mm tt");
                    }
                    catch (Exception ex)
                    {
                        string errmsg = ex.ToString();
                        OperationStatus = -1;
                        msg = "Contact of user '" + client.UserName + "' delete was unsuccesfull on " + now.ToString("MMM dd, yyyy hh:mm tt");
                    }
                    SaveAuditLog("Delete User Contact", msg, "CRMClient", "Client Information", "Client_User_ContactList", ColumnId, client.CreatedBy, now, OperationStatus);
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
        }
        public JsonResult DeleteUser(int UserId)
        {
            string msg = "";
            int ColumnId = 0;
            int OperationStatus = 1;
            if (!db.Client_User_Lists.Where(m => m.UserId == UserId).Any())
            {
                return Json("Error", JsonRequestBehavior.AllowGet);
            }
            else if (db.Business_Order_FileLists.Where(m => m.Client_User_Id == UserId).Any())
            {
                return Json("Error", JsonRequestBehavior.AllowGet);
            }
            else
            {
                Client_User_Lists client = db.Client_User_Lists.Find(UserId);
                int Id = client.ClientId;
                string FilePath = Path.Combine(Server.MapPath("~/Images/Client/ClientUserImage/Original"), client.PictureName);
                if (System.IO.File.Exists(FilePath))
                {
                    System.IO.File.Delete(FilePath);
                }
                string FilePathThumb = Path.Combine(Server.MapPath("~/Images/Client/ClientUserImage/thumb"), client.PictureName);
                if (System.IO.File.Exists(FilePathThumb))
                {
                    System.IO.File.Delete(FilePathThumb);
                }
                if (ModelState.IsValid)
                {
                    db.Client_User_ContactList.RemoveRange(db.Client_User_ContactList.Where(c => c.UserId == UserId));
                    db.Client_User_Lists.Remove(client);
                    try
                    {
                        db.SaveChanges();
                        OperationStatus = 1;
                        msg = "Information of user'" + client.UserName + "'has been deleted successfully on" + now.ToString("MMM dd, yyyy hh:mm tt");
                    }
                    catch (Exception ex)
                    {
                        string errmsg = ex.ToString();
                        OperationStatus = -1;
                        msg = "Information of user'" + client.UserName + "'delete was unsuccessful on" + now.ToString("MMM dd, yyyy hh:mm tt");
                    }

                    ColumnId = Convert.ToInt32(UserId);
                    SaveAuditLog("Delete User Information", msg, "CRMClient", "Client Information", "Client_User_Lists", ColumnId, client.CreatedBy, now, OperationStatus);
                    if (OperationStatus == 1)
                    {
                        return Json(Id, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        return Json("Error", JsonRequestBehavior.AllowGet);
                    }
                }
                else
                {
                    return Json("Success", JsonRequestBehavior.AllowGet);
                }
            }
        }
        #endregion

        #region Update user pic
        public JsonResult UpdateUserPicture(int UserId, long CreatedBy, HttpPostedFileBase files)
        {
            string msg = "";
            int ColumnId = UserId;
            int OperationStatus = 1;
            string fullName = "";
            string userName = "";

            if (!db.Client_User_Lists.Where(m => m.UserId == UserId).Any())
            {
                return Json("Error", JsonRequestBehavior.AllowGet);
            }
            else
            {
                Client_User_Lists user = db.Client_User_Lists.Find(UserId);
                userName = user.UserName;

                string FilePath = Path.Combine(Server.MapPath("~/Images/Client/ClientUserImage/Original/"), user.PictureName);
                if (System.IO.File.Exists(FilePath))
                {
                    System.IO.File.Delete(FilePath);
                }
                string FilePathThumb = Path.Combine(Server.MapPath("~/Images/Client/ClientUserImage/thumb/"), user.PictureName);
                if (System.IO.File.Exists(FilePathThumb))
                {
                    System.IO.File.Delete(FilePathThumb);
                }

                if (files != null)
                {
                    Random generator = new Random();
                    fullName = user.UserId + files.FileName;
                }
                user.PictureName = fullName;
                db.Entry(user).State = EntityState.Modified;
                try
                {
                    db.SaveChanges();
                    OperationStatus = 1;
                    string Paths = "";
                    if (files != null)
                    {
                        Paths = Path.Combine(Server.MapPath("~/Images/Client/ClientUserImage/Original/"), fullName);
                        files.SaveAs(Paths);
                        WebImage imgForGallery = new WebImage(files.InputStream);
                        if (imgForGallery.Width > 70)
                        {
                            imgForGallery.Resize(45, 45, false, true);
                        }
                        string PathsResize = Path.Combine(Server.MapPath("~/Images/Client/ClientUserImage/thumb/"), fullName);
                        imgForGallery.Save(PathsResize);
                    }

                    msg = "Picture of user '" + userName + "' has been changed successfully on " + now.ToString("MMM dd, yyyy hh:mm tt");
                }
                catch (Exception ex)
                {
                    string errmsg = ex.ToString();
                    OperationStatus = -1;
                    msg = "Picture of user '" + userName + "' change was unsuccessful on " + now.ToString("MMM dd, yyyy hh:mm tt");
                }
                SaveAuditLog("Update User Picture", msg, "CRMClient", "Client Information", "Client_User_Lists", ColumnId, CreatedBy, now, OperationStatus);
                if (OperationStatus == 1)
                {
                    return Json(fullName, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json("Error", JsonRequestBehavior.AllowGet);
                }
            }
        }
        #endregion

        #region Vehicle List
        //***************************VehicleList****************************
        public ActionResult VehicleList(int? pageNum, int Id, bool isDisplay)
        {
            pageNum = pageNum ?? 0;
            int page = Convert.ToInt32(pageNum);
            ViewBag.pageNum = pageNum;
            var client = db.Client_Vehicle_Lists.Where(m => m.ClientId == Id).Select(m => m.ClientId).FirstOrDefault();
            ViewBag.ClientId = client;
            var vehicle = (from m in db.Client_Vehicle_Lists
                           where m.ClientId == Id
                           select new Client_Vehicle_ListsModelView
                           {
                               VehicleId = m.VehicleId,
                               ClientId = m.ClientId,
                               VehicleName = m.VehicleName,
                               RegisterNo = m.RegisterNo,
                               PictureName = m.PictureName
                           }).ToList();
            ViewBag.isDisplay = isDisplay;
            ViewBag.TotalVehicle = vehicle.Count();
            Session["AllVehicle"] = vehicle;
            Session["TotalVehicleCount"] = vehicle.Count();
            return PartialView(vehicle);
        }
        public ActionResult _VehicleList(int? VehicleId, bool isDisplay)
        {
            if (VehicleId != null)
            {
                var allVehicle = (List<Client_Vehicle_ListsModelView>)Session["AllVehicleBySearch"];
                var allVehicleBySearch = (from m in allVehicle
                                          where m.VehicleId == VehicleId
                                          select new Client_Vehicle_ListsModelView
                                          {
                                              VehicleId = m.VehicleId,
                                              VehicleName = m.VehicleName,
                                              RegisterNo = m.RegisterNo,
                                              PictureName = m.PictureName
                                          }).ToList();
                ViewBag.isDisplay = isDisplay;
                ViewBag.TotalVehicle = allVehicleBySearch.Count();
                ViewBag.VehicleList = allVehicleBySearch.Take(ShowVehiclePerPage).ToList();
            }
            else
            {
                var allVehicle = (List<Client_Vehicle_ListsModelView>)Session["AllVehicle"];
                if (allVehicle != null)
                {
                    ViewBag.VehicleList = allVehicle.Take(ShowVehiclePerPage).ToList();
                    ViewBag.isDisplay = isDisplay;
                }
                else
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }
            }
            return PartialView(ViewBag.VendorList);
        }
        public JsonResult GetVehicleList(int? page)
        {
            page = page ?? 0;
            int total = Convert.ToInt16(Session["TotalVehicleCount"]);
            int skip = ShowVehiclePerPage * Convert.ToInt16(page);
            var canPage = skip < total;
            if (canPage)
            {
                var listBackFromSession = (List<Client_Vehicle_ListsModelView>)Session["AllVehicle"];
                var VehicleList = listBackFromSession.Skip(skip).Take(ShowVehiclePerPage).ToList();
                return Json(VehicleList, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json("", JsonRequestBehavior.AllowGet);
            }
        }
        #endregion

        #region Add new Vehicle
        public PartialViewResult AddVehicleView(int ClientId)
        {
            Client_Vehicle_ListsModelView model = new Client_Vehicle_ListsModelView();
            if (ClientId > 0)
            {
                model.ClientId = ClientId;

            }
            return PartialView(model);
        }
        public PartialViewResult _AddNewVehicle(int ClientId)
        {
            Client_Vehicle_ListsModelView model = new Client_Vehicle_ListsModelView();
            model.ClientId = ClientId;
            ViewBag.CountryId = new SelectList(db.CountryLists, "CountryCode", "CountryName", model.Country);
            ViewBag.DivisionList = new SelectList(db.DivisionLists, "DivisionId", "DivisionName", model.DivisionId);
            return PartialView(model);
        }
        public JsonResult AddNewVehicle(Client_Vehicle_ListsModelView model, HttpPostedFileBase files, int ClientId)
        {
            string msg = "";
            int ColumnId = 0;
            int OperationStatus = 1;
            string fullName = "";
            Client_Vehicle_Lists vehicle = new Client_Vehicle_Lists();
            vehicle.ClientId = ClientId;
            vehicle.VehicleName = model.VehicleName;
            vehicle.RegisterNo = model.RegisterNo;
            vehicle.Description = model.Description;
            vehicle.Address = model.Address;
            vehicle.AddressLine1 = model.AddressLine1;
            vehicle.Country = model.Country;
            vehicle.State = model.State;
            vehicle.CreatedBy = model.CreatedBy;
            vehicle.DivisionId = model.DivisionId;
            vehicle.City = model.City;
            vehicle.PostalCode = model.PostalCode;
            vehicle.CreatedBy = model.CreatedBy;
            vehicle.CreatedDate = now;
            if (files != null)
            {
                Random generator = new Random();
                fullName = generator.Next(1000, 9999) + "_" + model.ClientId + files.FileName;
            }

            vehicle.PictureName = fullName;
            db.Client_Vehicle_Lists.Add(vehicle);

            try
            {
                db.SaveChanges();
                ColumnId = db.Client_Vehicle_Lists.Where(m => m.CreatedBy == model.CreatedBy).Max(m => m.VehicleId);
                string Paths = Server.MapPath("~/Images/Client");
                if (!Directory.Exists(Paths))
                {
                    Directory.CreateDirectory(Paths);
                }
                if (files != null)
                {
                    Paths = Path.Combine(Server.MapPath("~/Images/Client/ClientVehicleImage/Original"), fullName);
                    files.SaveAs(Paths);
                    WebImage imgForGallery = new WebImage(files.InputStream);
                    if (imgForGallery.Width > 70)
                    {
                        imgForGallery.Resize(45, 45, false, true);
                    }
                    string PathsResize = Path.Combine(Server.MapPath("~/Images/Client/ClientVehicleImage/thumb"), fullName);
                    imgForGallery.Save(PathsResize);
                }
                msg = "New vehicle '" + model.VehicleName + "' has been added successfully on " + now.ToString("MMM dd, yyyy hh:mm tt");
            }
            catch (Exception ex)
            {
                string errorMsg = ex.Message.ToString();
                msg = "New vehicle '" + model.VehicleName + "' adding  failed on " + now.ToString("MMM dd, yyyy hh:mm tt");
                OperationStatus = -1;
            }

            SaveAuditLog("Add New Vehicle", msg, "CRMClient", "Client Information", "Client_Vehicle_Lists", ColumnId, vehicle.CreatedBy, now, OperationStatus);
            if (OperationStatus == 1)
            {
                return Json("Success", JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json("Error", JsonRequestBehavior.AllowGet);
            }

        }
        #endregion

        #region Edit and delete  Vehicle
        public PartialViewResult LoadVehicleEditPopUp(int? VehicleId)
        {
            Client_Vehicle_ListsModelView model = new Client_Vehicle_ListsModelView();
            if (VehicleId > 0)
            {
                Client_Vehicle_Lists client = db.Client_Vehicle_Lists.Find(VehicleId);
                model.ClientId = client.ClientId;
                model.VehicleId = client.VehicleId;

            }
            return PartialView(model);
        }
        public PartialViewResult _EditVehicleDetails(int? VehicleId)
        {
            if (VehicleId == null)
            {
                throw new HttpException(400, "VehicleId in _EditVehicleDetails is Null");
            }
            else
            {
                Client_Vehicle_ListsModelView model = new Client_Vehicle_ListsModelView();
                Client_Vehicle_Lists client = db.Client_Vehicle_Lists.Find(VehicleId);
                model.VehicleId = client.VehicleId;
                model.ClientId = client.ClientId;
                model.PictureName = client.PictureName;
                model.VehicleName = client.VehicleName;
                model.Description = client.Description;
                model.Address = client.Address;
                model.AddressLine1 = client.AddressLine1;
                model.City = client.City;
                model.Country = client.Country;
                model.CountryName = db.CountryLists.Where(m => m.CountryCode == model.Country).Select(m => m.CountryName).SingleOrDefault();
                model.DivisionId = client.DivisionId;
                model.DivisionName = db.DivisionLists.Where(m => m.DivisionId == model.DivisionId).Select(m => m.DivisionName).SingleOrDefault();
                model.RegisterNo = client.RegisterNo;
                model.PostalCode = client.PostalCode;
                model.State = client.State;
                model.UpdatedBy = client.CreatedBy;
                model.UpdatedDate = now;
                ViewBag.DivisionList = new SelectList(db.DivisionLists, "DivisionId", "DivisionName");
                ViewBag.CountryId = new SelectList(db.CountryLists, "CountryCode", "CountryName");
                return PartialView(model);
            }
        }
        public JsonResult UpdateVehicleInfo(Client_Vehicle_ListsModelView model)
        {
            string msg = "";
            int ColumnId = Convert.ToInt32(model.VehicleId);
            int OperationStatus = 1;
            Client_Vehicle_Lists vehicle = db.Client_Vehicle_Lists.Find(model.VehicleId);
            vehicle.VehicleId = model.VehicleId;
            vehicle.ClientId = model.ClientId;
            vehicle.PictureName = model.PictureName;
            vehicle.VehicleName = model.VehicleName;
            vehicle.Description = model.Description;
            vehicle.Address = model.Address;
            vehicle.AddressLine1 = model.AddressLine1;
            vehicle.City = model.City;
            vehicle.Country = model.Country;
            vehicle.DivisionId = model.DivisionId;
            vehicle.RegisterNo = model.RegisterNo;
            vehicle.PostalCode = model.PostalCode;
            vehicle.State = model.State;
            vehicle.UpdatedBy = model.CreatedBy;
            vehicle.UpdatedDate = now;
            db.Entry(vehicle).State = EntityState.Modified;
            try
            {
                db.SaveChanges();
                OperationStatus = 1;
                msg = "Information of vehicle '" + vehicle.VehicleName + "' has been updated successfully on " + now.ToString("MMM dd, yyyy hh:mm tt");
            }
            catch (Exception ex)
            {
                string errmsg = ex.ToString();
                OperationStatus = -1;
                msg = "Information of vehicle '" + vehicle.VehicleName + "' update was unsuccessfull on " + now.ToString("MMM dd, yyyy hh:mm tt");
            }
            SaveAuditLog("Vehicle Information Update", msg, "CRMClient", "Client Information", "Client_Vehicle_Lists", ColumnId, model.CreatedBy, now, OperationStatus);
            return Json("success", JsonRequestBehavior.AllowGet);
        }
        public JsonResult DeleteVehicle(int VehicleId)
        {
            string msg = "";
            int ColumnId = Convert.ToInt32(VehicleId);
            int OperationStatus = 1;
            if (!db.Client_Vehicle_Lists.Where(m => m.VehicleId == VehicleId).Any())
            {
                return Json("Error", JsonRequestBehavior.AllowGet);
            }
            else
            {
                Client_Vehicle_Lists client = db.Client_Vehicle_Lists.Find(VehicleId);
                int Id = client.ClientId;
                if (client.PictureName != null)
                {
                    string FilePath = Path.Combine(Server.MapPath("~/Images/Client/ClientVehicleImage/Original"), client.PictureName);
                    if (System.IO.File.Exists(FilePath))
                    {
                        System.IO.File.Delete(FilePath);
                    }
                    string FilePathThumb = Path.Combine(Server.MapPath("~/Images/Client/ClientVehicleImage/thumb"), client.PictureName);
                    if (System.IO.File.Exists(FilePathThumb))
                    {
                        System.IO.File.Delete(FilePathThumb);
                    }
                }
                if (ModelState.IsValid)
                {
                    db.Client_Vehicle_Lists.Remove(client);
                    try
                    {
                        db.SaveChanges();
                        OperationStatus = 1;
                        msg = "Information of vehicle '" + client.VehicleName + "' has been deleted successfully on " + now.ToString("MMM dd, yyyy hh:mm tt");
                    }
                    catch (Exception ex)
                    {
                        string errmsg = ex.ToString();
                        OperationStatus = -1;
                        msg = "Information of vehicle '" + client.VehicleName + "' delete was unsuccessful on " + now.ToString("MMM dd, yyyy hh:mm tt");
                    }

                    SaveAuditLog("Delete vehicle Information", msg, "CRMClient", "Client Information", "Client_Vehicle_Lists", ColumnId, client.CreatedBy, now, OperationStatus);
                    if (OperationStatus == 1)
                    {
                        return Json(Id, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        return Json("Error", JsonRequestBehavior.AllowGet);
                    }
                }
                else
                {
                    return Json("Success", JsonRequestBehavior.AllowGet);
                }
            }
        }
        public JsonResult UpdateVehiclePicture(int VehicleId, long CreatedBy, HttpPostedFileBase files)
        {
            string msg = "";
            int ColumnId = VehicleId;
            int OperationStatus = 1;
            string fullName = "";
            string vPicName = "";

            if (!db.Client_Vehicle_Lists.Where(m => m.VehicleId == VehicleId).Any())
            {
                return Json("Error", JsonRequestBehavior.AllowGet);
            }
            else
            {
                Client_Vehicle_Lists user = db.Client_Vehicle_Lists.Find(VehicleId);
                vPicName = user.VehicleName;

                string FilePath = Path.Combine(Server.MapPath("~/Images/Client/ClientVehicleImage/Original"), user.PictureName);
                if (System.IO.File.Exists(FilePath))
                {
                    System.IO.File.Delete(FilePath);
                }
                string FilePathThumb = Path.Combine(Server.MapPath("~/Images/Client/ClientVehicleImage/thumb"), user.PictureName);
                if (System.IO.File.Exists(FilePathThumb))
                {
                    System.IO.File.Delete(FilePathThumb);
                }

                if (files != null)
                {
                    Random generator = new Random();
                    fullName = user.VehicleId + files.FileName;
                }
                user.PictureName = fullName;
                db.Entry(user).State = EntityState.Modified;
                try
                {
                    db.SaveChanges();
                    OperationStatus = 1;
                    string Paths = "";
                    if (files != null)
                    {
                        Paths = Path.Combine(Server.MapPath("~/Images/Client/ClientVehicleImage/Original"), fullName);
                        files.SaveAs(Paths);
                        WebImage imgForGallery = new WebImage(files.InputStream);
                        if (imgForGallery.Width > 70)
                        {
                            imgForGallery.Resize(45, 45, false, true);
                        }
                        string PathsResize = Path.Combine(Server.MapPath("~/Images/Client/ClientVehicleImage/thumb"), fullName);
                        imgForGallery.Save(PathsResize);
                    }

                    msg = "Picture of vehicle '" + vPicName + "' has been updated successfully on " + now.ToString("MMM dd, yyyy hh:mm tt");
                }
                catch (Exception ex)
                {
                    string errmsg = ex.ToString();
                    OperationStatus = -1;
                    msg = "Picture of vehicle '" + vPicName + "' update was unsuccessfull on " + now.ToString("MMM dd, yyyy hh:mm tt");
                }

                SaveAuditLog("Update Vehicle Picture", msg, "CRMClient", "Client Information", "Client_Vehicle_Lists", ColumnId, CreatedBy, now, OperationStatus);
                if (OperationStatus == 1)
                {
                    return Json(fullName, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json("Error", JsonRequestBehavior.AllowGet);
                }
            }
        }
        #endregion

        #region Display vehicle List
        //****************************** Vehicle List For Display *********************
        public PartialViewResult DisplayVehicleList(int? pageNum, int Id)
        {
            pageNum = pageNum ?? 0;
            int page = Convert.ToInt32(pageNum);
            ViewBag.pageNum = pageNum;
            var vehicle = (from m in db.Client_Vehicle_Lists
                           where m.ClientId == Id
                           select new Client_Vehicle_ListsModelView
                           {
                               VehicleId = m.VehicleId,
                               VehicleName = m.VehicleName,
                               RegisterNo = m.RegisterNo,
                               PictureName = m.PictureName,
                           }).ToList();
            ViewBag.TotalVehicle = vehicle.Count();
            Session["AllVehicle"] = vehicle;
            Session["TotalVehicleCount"] = vehicle.Count();
            return PartialView();
        }
        public PartialViewResult _DisplayVehicleList(int? VehicleId)
        {
            if (VehicleId != null)
            {
                var allVehicle = (List<Client_Vehicle_ListsModelView>)Session["AllVehicleBySearch"];
                var allVehicleBySearch = (from m in allVehicle
                                          where m.VehicleId == VehicleId
                                          select new Client_Vehicle_ListsModelView
                                          {
                                              VehicleId = m.VehicleId,
                                              VehicleName = m.VehicleName,
                                              RegisterNo = m.RegisterNo,
                                              PictureName = m.PictureName,
                                          }).ToList();
                ViewBag.TotalVehicle = allVehicleBySearch.Count();
                ViewBag.VehicleList = allVehicleBySearch.Take(ShowVehiclePerPage).ToList();
            }
            else
            {
                var allVehicle = (List<Client_Vehicle_ListsModelView>)Session["AllVehicle"];

                if (allVehicle != null)
                {
                    ViewBag.VehicleList = allVehicle.Take(ShowVehiclePerPage).ToList();
                }
                else
                {
                    throw new HttpException(400, "allVehicle in _DisplayVehicleList is Null");
                }
            }
            return PartialView(ViewBag.VendorList);
        }
        public JsonResult GetDisplayVehicleList(int? page)
        {
            page = page ?? 0;
            int total = Convert.ToInt16(Session["TotalVehicleCount"]);
            int skip = ShowVehiclePerPage * Convert.ToInt16(page);
            var canPage = skip < total;
            if (canPage)
            {
                var listBackFromSession = (List<Client_Vehicle_ListsModelView>)Session["AllVehicle"];
                var VehicleList = listBackFromSession.Skip(skip).Take(ShowVehiclePerPage).ToList();
                return Json(VehicleList, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json("", JsonRequestBehavior.AllowGet);
            }
        }
        public JsonResult SearchDisplayVehicleList(string prefix)
        {
            if (prefix == "")
            {
                return Json("", JsonRequestBehavior.AllowGet);
            }
            else
            {
                var allVehicle = (List<Client_Vehicle_ListsModelView>)Session["AllVehicle"];
                var Vehicle = (from s in allVehicle
                               where (s.VehicleName.ToLower().Contains(prefix.ToLower()) || s.RegisterNo.ToLower().Contains(prefix.ToLower()))
                               select new Client_Vehicle_ListsModelView
                               {
                                   VehicleId = s.VehicleId,
                                   VehicleName = s.VehicleName + " " + "(" + s.RegisterNo + ")",
                                   RegisterNo = s.RegisterNo,
                                   PictureName = s.PictureName
                               }).ToList();
                Session["AllVehicleBySearch"] = Vehicle;
                return Json(Vehicle, JsonRequestBehavior.AllowGet);
            }
        }
        public PartialViewResult _DisplayVehicleInformation(int ClientId, int VehicleId)
        {
            Client_Vehicle_ListsModelView model = new Client_Vehicle_ListsModelView();
            Client_Vehicle_Lists vehicle = db.Client_Vehicle_Lists.Find(VehicleId);
            model.VehicleId = vehicle.VehicleId;
            model.VehicleName = vehicle.VehicleName;
            model.RegisterNo = vehicle.RegisterNo;
            model.ClientId = vehicle.ClientId;
            model.Address = vehicle.Address;
            model.AddressLine1 = vehicle.AddressLine1;
            model.City = vehicle.City;
            model.Country = vehicle.Country;
            model.CountryName = db.CountryLists.Where(m => m.CountryCode == model.Country).Select(m => m.CountryName).SingleOrDefault();
            model.DivisionId = vehicle.DivisionId;
            model.DivisionName = db.DivisionLists.Where(m => m.DivisionId == model.DivisionId).Select(m => m.DivisionName).SingleOrDefault();
            model.PictureName = vehicle.PictureName;
            model.PostalCode = vehicle.PostalCode;
            model.State = vehicle.State;
            model.UpdatedBy = vehicle.CreatedBy;
            model.UpdatedDate = now;
            return PartialView(model);
        }
        #endregion

        #region Business History
        //************************************ SHOW HISTORY ***********************************************
        public PartialViewResult _DisplayBusinessOrderHistory(int ClientId)
        {
            BusinessOrderListModelView model = new BusinessOrderListModelView();
            BusinessOrderList business = db.BusinessOrderLists.Where(m => m.ClientId == ClientId).FirstOrDefault();
            model.ClientId = ClientId;
            return PartialView(model);
        }
        public PartialViewResult _DisplayBusinessOrder(int ClientId)
        {
            var model = (from m in db.View_Busi_Details_ForClient
                         where m.ClientId == ClientId && m.OrderType == 1
                         select new BusinessOrderListModelView
                         {
                             BusinessOrderId = m.BusinessOrderId,
                             OrderName = m.OrderName,
                             TotalCost = m.TotalCost,
                             TotalProCost = m.TotalProCost,
                             TotalOthersCost = m.TotalOthersCost,
                             CreatedBy = m.CreatedBy,
                             CreatedByName = m.UserName,
                             CreatedDate = m.CreatedDate,
                             HasChild = m.HasSpec,
                             OrderAprvStatus = m.OrderAprvStatus
                         }).ToList();
            return PartialView(model);
        }
        #endregion

        public JsonResult GetAllProductTypeForList(int? ParentId)
        {
            int busId = Convert.ToInt32(ParentId);
            var list = (from m in db.BusiOrder_ManualSpec
                        join u in db.UnitLists on m.UnitId equals u.UnitId
                        where m.BusOrdId == ParentId
                        select new FactoryManagement.ModelView.CRM.BusiOrder_ManualSpecModelView
                        {
                            Id = m.Id,
                            BusOrdId = busId,
                            Spec_lbl = m.Spec_lbl,
                            Quantity = m.Quantity,
                            UnitId = m.UnitId,
                            UnitName = u.UnitName,
                            Spec_Description = m.Spec_Description,
                            CreatedBy = m.CreatedBy,
                            CreatedDate = m.CreatedDate,
                            ProStatus = m.ProStatus
                        }).ToList();

            return Json(list, JsonRequestBehavior.AllowGet);
        }

        #region Supplier History
        public PartialViewResult _DisplaySupplyHistory(int ClientId)
        {
            ViewBag.ClientId = ClientId;
            return PartialView();
        }
        public PartialViewResult _DisplayBusinessSupplierList(int ClientId)
        {
            var model = (from m in db.View_Busi_Details_ForClient
                         where m.ClientId == ClientId && m.OrderType == 2
                         select new BusinessOrderListModelView
                         {
                             BusinessOrderId = m.BusinessOrderId,
                             OrderName = m.OrderName,
                             TotalCost = m.TotalCost,
                             TotalProCost = m.TotalProCost,
                             TotalOthersCost = m.TotalOthersCost,
                             CreatedBy = m.CreatedBy,
                             CreatedByName = m.UserName,
                             CreatedDate = m.CreatedDate,
                             OrderAprvStatus = m.OrderAprvStatus
                         }).ToList();
            return PartialView(model);
        }
        public JsonResult GetAllSupplierList(int? ParentId)
        {
            int busId = Convert.ToInt32(ParentId);
            var list = (from m in db.View_Sup_Pro_Lists
                        where m.BusinessOrderId == ParentId
                        select new BusinessOrderProductListModelView
                        {
                            Id = m.Id,
                            BusinessOrderId = busId,
                            ProductName = m.Proname,
                            Quantity = m.Quantity,
                            UnitName = m.UnitName,
                            PerPiece = m.PerPiece,
                            TotalPriece = m.TotalPriece,
                            ReceivedStatus = m.ReceivedStatus,
                            ReceivedQuantity = m.ReceivedQuantity,
                            AssignedQuantity = m.AssignedQuantity
                        }).ToList();
            return Json(list, JsonRequestBehavior.AllowGet);
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