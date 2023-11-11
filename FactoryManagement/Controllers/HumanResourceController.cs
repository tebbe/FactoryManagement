
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
using FactoryManagement.ModelView.HR;
using System.Web.Helpers;
using FactoryManagement.ModelView.SalaryConfig;
using System.ComponentModel;
using System.Configuration;
using FactoryManagement.Filters;

namespace FactoryManagement.Controllers
{
    [Authorize]
    [DisplayName("Human Resource")]
    public class HumanResourceController : Controller
    {
        #region Private Properties
        private FactoryManagementEntities db = new FactoryManagementEntities();
        static private int offset = Convert.ToInt32(ConfigurationManager.AppSettings["localTime"]);
        DateTime now = DateTime.UtcNow.AddMinutes(offset);
        const int ShowUserPerPage = 20;
        const int ShowPaidHisPage = 20;
        const int ShowTranPage = 20;
        const int ShowRevertedPerPage = 20;
        const int ShowUserLoanPerPage = 20;
        const int MonthSalUser = 20;
        #endregion

        #region User Create,Edit,Delete
        public ActionResult AddEmployee()
        {
            UserInformationModelView model = new UserInformationModelView();
            ViewBag.DesignationId = new SelectList(db.Designations, "DesignationId", "DesignationName");
            ViewBag.Religion = new SelectList(db.ReligionLists, "ReligionId", "Religion");
            ViewBag.DivisionList = new SelectList(db.DivisionLists, "DivisionId", "DivisionName");
            var SiteList = (from m in db.SiteLists
                            where m.IsResidential == false && m.Status == 1
                            select new
                            {
                                Id = m.Id,
                                SiteName = m.SiteName,
                                IsComposite = m.IsComposite
                            }).ToList();
            ViewBag.SiteId = SiteList;
            ViewBag.CountryId = new SelectList(db.CountryLists, "CountryCode", "CountryName");
            ViewBag.TitleId = new SelectList(db.NameTitles, "Id", "Name");
            return View(model);
        }
        public JsonResult AddEmployeeInfo(UserInformationModelView model, IEnumerable<HttpPostedFileBase> files)
        {
            string msg = "";
            string name = "";
            int ColumnId = 0;
            int OperationStatus = 1;
            string fullName = "";
            string NtlId_FrntImg = "";
            string NtlId_BckImg = "";
            string Paths = Server.MapPath("~/Images/UserPicture/");

            if (!Directory.Exists(Paths))
            {
                Directory.CreateDirectory(Paths);
            }
            UserInformation user = new UserInformation();
            user.Title = model.Title;
            user.FirstName = model.FirstName;
            user.MiddleName = model.MiddleName;
            user.LastName = model.LastName;
            user.UserType = model.UserType;
            user.DateOfBirth = Convert.ToDateTime(model.DateOfBirth);
            user.DesignationId = model.DesignationId;
            user.Gender = model.Gender;
            user.Nationality = model.Nationality;
            user.NationalId = model.NationalId;
            user.Phone = model.Phone;
            user.Gender = model.Gender;
            user.Religion = model.Religion;
            user.BasicSalary = model.BasicSalary;
            user.EmailAddress = model.EmailAddress;
            user.MobileNo = model.MobileNo;
            user.ParAddress = model.ParAddress;
            user.ParAddressLine1 = model.ParAddressLine1;
            user.ParCountry = model.ParCountry;
            user.ParDivisionId = model.ParDivisionId;
            user.ParCity = model.ParCity;
            user.ParArea = model.ParArea;
            user.ParState = model.ParState;
            user.ParPotalCode = model.ParPotalCode;
            user.PictureOriginalName = model.PictureOriginalName;
            user.SiteId = model.SiteId;
            user.CreatedBy = model.CreatedBy;
            user.CreatedDate = now;
            user.JoinDate = model.JoinDate;
            user.Status = 0;
            user.EmpId = model.EmpId;
            user.DeptId = model.DeptId;
            user.SiteId = model.SiteId;
            user.LineId = model.LineId;
            user.UnitId = model.UnitId;
            user.MachineId = model.MachineId;
            user.AssginSalary = false;
            if (model.SamePresentAddress == true)
            {
                user.PreAddress = model.ParAddress;
                user.PreAddressLine1 = model.ParAddressLine1;
                user.PreCountry = model.ParCountry;
                user.PreDivisionId = model.ParDivisionId;
                user.PreCity = model.ParCity;
                user.PrePostalCode = model.ParPotalCode;
                user.PreArea = model.ParArea;
                user.PreState = model.ParState;
                user.SamePresentAddress = true;
            }
            else
            {
                user.PreAddress = model.PreAddress;
                user.PreAddressLine1 = model.PreAddressLine1;
                user.PreCountry = model.PreCountry;
                user.PreState = model.PreState;
                user.PreDivisionId = model.PreDivisionId;
                user.PreCity = model.PreCity;
                user.PrePostalCode = model.PrePostalCode;
                user.PreArea = model.PreArea;
                user.SamePresentAddress = false;
            }

            try
            {
                db.UserInformations.Add(user);
                db.SaveChanges();
                OperationStatus = 1;

                name = user.FirstName + " " + model.MiddleName + " " + model.LastName;
                ColumnId = Convert.ToInt32(db.UserInformations.Where(m => m.CreatedBy == model.CreatedBy).Max(m => m.UserId));
                msg = "User" + " " + "'" + name + "'" + " " + "Created on " + now.ToString("MMM dd , yyy hh:mm tt");
            }
            catch (Exception)
            {
                msg = "User" + " " + "'" + name + "'" + " " + "Creation Failed on " + now.ToString("MMM dd , yyy hh:mm tt");
                OperationStatus = -1;
                // throw;
            }
            SaveAuditLog("Create New User", msg, "HR", "AddEmployee", "UserInformation", ColumnId, Convert.ToInt32(model.CreatedBy), now, OperationStatus);
            if (files != null)
            {
                int i = 0;
                UserInformation anuser = db.UserInformations.Find(user.UserId);
                foreach (var file in files)
                {
                    if (file != null)
                    {
                        Random generator = new Random();
                        string random = generator.Next(0, 1000000).ToString("D6");
                        string s = file.FileName;
                        int idx = s.LastIndexOf('.');
                        string fileName = s.Substring(0, idx);
                        string extension = s.Substring(idx);
                        byte[] imageByte = null;
                        BinaryReader rdr = new BinaryReader(file.InputStream);
                        imageByte = rdr.ReadBytes((int)file.ContentLength);
                        anuser.ImageData = imageByte;
                        if (i == 0)
                        {
                            fullName = "Emp" + ColumnId + random + extension;
                            anuser.Picture = fullName;
                            Paths = Path.Combine(Server.MapPath("~/Images/UserPicture/Original"), fullName);
                            file.SaveAs(Paths);
                            WebImage imgForGallery = new WebImage(file.InputStream);
                            if (imgForGallery.Width > 70)
                            {
                                imgForGallery.Resize(29, 29, false, true);
                            }
                            string PathsResize = Path.Combine(Server.MapPath("~/Images/UserPicture/thumb"), fullName);
                            imgForGallery.Save(PathsResize);
                        }
                        else if (i == 1)
                        {
                            NtlId_FrntImg = "NtlId_FrntImg" + ColumnId + random + extension;
                            anuser.NationalIdFontImg = NtlId_FrntImg;

                            Paths = Path.Combine(Server.MapPath("~/Images/UserPicture/NationalId/Original"), NtlId_FrntImg);
                            file.SaveAs(Paths);
                            WebImage imgForGallery = new WebImage(file.InputStream);
                            if (imgForGallery.Width > 70)
                            {
                                imgForGallery.Resize(29, 29, false, true);
                            }
                            string PathsResize = Path.Combine(Server.MapPath("~/Images/UserPicture/NationalId/thumb"), NtlId_FrntImg);
                            imgForGallery.Save(PathsResize);
                        }
                        else
                        {
                            NtlId_BckImg = "NtlId_BckImg" + ColumnId + random + extension;
                            anuser.NationalIdBackImg = NtlId_BckImg;
                            Paths = Path.Combine(Server.MapPath("~/Images/UserPicture/NationalId/Original"), NtlId_BckImg);
                            file.SaveAs(Paths);
                            WebImage imgForGallery = new WebImage(file.InputStream);
                            if (imgForGallery.Width > 70)
                            {
                                imgForGallery.Resize(29, 29, false, true);
                            }
                            string PathsResize = Path.Combine(Server.MapPath("~/Images/UserPicture/NationalId/thumb"), NtlId_BckImg);
                            imgForGallery.Save(PathsResize);
                        }
                    }
                    else
                    {
                        if (i == 0)
                        {
                            fullName = "blank-person" + ColumnId;
                            System.IO.File.Copy(Server.MapPath("~/Images/Icon/blank-person.jpg"), Server.MapPath("~/Images/UserPicture/Original/" + fullName + ".jpg"));
                            // System.IO.File.Copy(Server.MapPath("~/Images/Icon/blnk_prsn_thmb.jpg"), Server.MapPath("~/Images/UserPicture/thumb/" + fullName + ".jpg"));
                            anuser.Picture = fullName + ".jpg";
                            var blankImage = System.IO.File.ReadAllBytes(Server.MapPath("~/Images/Icon/blank-person.jpg"));
                            WebImage imgForGallery = new WebImage(blankImage);
                            if (imgForGallery.Width > 70)
                            {
                                imgForGallery.Resize(29, 29, false, true);
                            }
                            string PathsResize = Path.Combine(Server.MapPath("~/Images/UserPicture/thumb"), fullName + ".jpg");
                            imgForGallery.Save(PathsResize);
                        }
                    }
                    i++;
                }
                db.Entry(user).State = EntityState.Modified;
                db.SaveChanges();
            }
            //if (files != null)
            //{

            //    int i = 0;
            //    foreach (var file in files)
            //    {
            //        if (i == 0)
            //        {
            //            Paths = Path.Combine(Server.MapPath("~/Images/UserPicture/Original"), fullName);
            //            file.SaveAs(Paths);
            //            WebImage imgForGallery = new WebImage(file.InputStream);
            //            if (imgForGallery.Width > 70)
            //            {
            //                imgForGallery.Resize(29, 29, false, true);
            //            }
            //            string PathsResize = Path.Combine(Server.MapPath("~/Images/UserPicture/thumb"), fullName);
            //            imgForGallery.Save(PathsResize);
            //        }
            //        else if (i == 1)
            //        {
            //            Paths = Path.Combine(Server.MapPath("~/Images/UserPicture/NationalId/Original"), NtlId_FrntImg);
            //            file.SaveAs(Paths);
            //            WebImage imgForGallery = new WebImage(file.InputStream);
            //            if (imgForGallery.Width > 70)
            //            {
            //                imgForGallery.Resize(29, 29, false, true);
            //            }
            //            string PathsResize = Path.Combine(Server.MapPath("~/Images/UserPicture/NationalId/thumb"), NtlId_FrntImg);
            //            imgForGallery.Save(PathsResize);

            //        }
            //        else
            //        {
            //            Paths = Path.Combine(Server.MapPath("~/Images/UserPicture/NationalId/Original"), NtlId_BckImg);
            //            file.SaveAs(Paths);
            //            WebImage imgForGallery = new WebImage(file.InputStream);
            //            if (imgForGallery.Width > 70)
            //            {
            //                imgForGallery.Resize(29, 29, false, true);
            //            }
            //            string PathsResize = Path.Combine(Server.MapPath("~/Images/UserPicture/NationalId/thumb"), NtlId_BckImg);
            //            imgForGallery.Save(PathsResize);
            //        }
            //        i++;
            //    }

            //}
            if (model.UserType == 1)
            {
                Random generator = new Random();
                UserLogin ulogin = new UserLogin();

                ulogin.UserName = model.LastName.Substring(0, 1) + model.FirstName.Substring(0, 1);
                LettersCount md = (from m in db.LettersCounts
                                   where m.Letters.Equals(ulogin.UserName)
                                   select m).FirstOrDefault();
                int count = md.Count + 1;
                ulogin.UserName = ulogin.UserName + count.ToString("000");
                ulogin.Password = generator.Next(0, 1000000).ToString("D6");
                ulogin.UserId = ColumnId;
                ulogin.Entrydate = now;
                ulogin.EntryBy = model.CreatedBy;
                ulogin.HasResetPassword = true;
                ulogin.IsFirstLoginAfterReset = false;
                try
                {
                    db.UserLogins.Add(ulogin);
                    db.SaveChanges();
                    OperationStatus = 1;
                }
                catch (Exception ex)
                {
                    string errmsg = ex.ToString();
                    OperationStatus = -1;
                }
                md.Count = count;
                try
                {
                    db.Entry(md).State = EntityState.Modified;
                    db.SaveChanges();
                    OperationStatus = 1;
                }
                catch (Exception ex)
                {
                    string errmsg = ex.ToString();
                    OperationStatus = -1;
                }
            }
            if (OperationStatus == 1)
            {
                return Json(ColumnId, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json("Error", JsonRequestBehavior.AllowGet);
            }
        }
        public PartialViewResult _GetUnitList(int SiteId, int? UnitId, int UserType, string pageType)
        {
            if (UserType == 2)
            {
                var allDept = db.LineInfoes.Select(n => n.DeptId).Distinct().ToArray();
                var allUnit = db.DepartmentLists.Where(n => n.UnitId > 0 && allDept.Contains(n.DeptId)).Select(s => s.UnitId).Distinct().ToArray();
                var allUnitList = (from m in db.Site_Unit_Lists
                                   where allUnit.Contains(m.UnitId) && m.SiteId == SiteId
                                   select new
                                   {
                                       UnitId = m.UnitId,
                                       UnitName = m.UnitName
                                   }).ToList();
                ViewBag.UnitId = allUnitList;
            }
            else
            {
                var allUnit = db.DepartmentLists.Where(n => n.UnitId > 0).Select(s => s.UnitId).Distinct().ToArray();
                var allUnitList = (from m in db.Site_Unit_Lists
                                   where allUnit.Contains(m.UnitId) && m.SiteId == SiteId
                                   select new
                                   {
                                       UnitId = m.UnitId,
                                       UnitName = m.UnitName
                                   }).ToList();
                ViewBag.UnitId = allUnitList;
            }
            ViewBag.pageType = pageType;
            ViewBag.seletedUnitId = UnitId;
            return PartialView();
        }
        public PartialViewResult _GetDept(int? SiteId, int? UnitId, int? deptId, string pageType, int UserType)
        {
            if (UserType == 2)
            {
                var allDept = db.LineInfoes.Select(n => n.DeptId).Distinct().ToArray();
                if (UnitId > 0)
                {
                    var deptList = (from m in db.DepartmentLists
                                    where allDept.Contains(m.DeptId) && m.UnitId == UnitId
                                    select new
                                    {
                                        DeptId = m.DeptId,
                                        DeptName = m.DeptName
                                    }).ToList();
                    ViewBag.DeptId = deptList;
                }
                else
                {
                    var deptList = (from m in db.DepartmentLists
                                    where allDept.Contains(m.DeptId) && m.SiteId == SiteId
                                    select new
                                    {
                                        DeptId = m.DeptId,
                                        DeptName = m.DeptName
                                    }).ToList();
                    ViewBag.DeptId = deptList;
                }
            }
            else
            {
                if (UnitId > 0)
                {
                    var DeptList = (from m in db.DepartmentLists
                                    where m.UnitId == UnitId
                                    select new
                                    {
                                        DeptId = m.DeptId,
                                        DeptName = m.DeptName
                                    }).ToList();
                    ViewBag.DeptId = DeptList;
                }
                else
                {

                    var DeptList = (from m in db.DepartmentLists
                                    where m.SiteId == SiteId
                                    select new
                                    {
                                        DeptId = m.DeptId,
                                        DeptName = m.DeptName
                                    }).ToList();
                    ViewBag.DeptId = DeptList;

                }
            }
            ViewBag.pageType = pageType;
            ViewBag.seletedDeptId = deptId;
            return PartialView();
        }
        public ActionResult _GetLineList(int DeptId, string pageType, int? LineId)
        {
            ViewBag.Line = new SelectList(db.LineInfoes.Where(m => m.DeptId == DeptId), "LineId", "LineAcronym");
            ViewBag.pageType = pageType;
            ViewBag.seletedLineId = LineId;
            return PartialView();
        }
        public PartialViewResult _GetMachineList(int LineId, string pageType, int? MachineId)
        {
            var machine = (from m in db.LineMachineLists
                           join p in db.MachineLists on m.MachineId equals p.MachineId
                           join s in db.Machines on p.MachineTypeId equals s.Id
                           where m.LineId == LineId
                           select new
                           {
                               MachineId = m.MachineId,
                               MachineName = p.MachineAcronym + " (" + s.Name + ") "
                           }).ToList();
            ViewBag.machine = machine;
            ViewBag.pageType = pageType;
            ViewBag.seletedMachineId = MachineId;
            return PartialView();
        }

        public ActionResult SetUserSalary(long? UserId)
        {
            if (UserId == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            else
            {
                var model = db.View_UserLists.Where(m => m.UserId == UserId).FirstOrDefault();
                ViewBag.SalaryPackages = new SelectList(db.Salary_Package_List, "PackageId", "PackageName");
                ViewBag.WorkingSchedules = new SelectList(db.WorkingScheduleLists, "WorkingScheduleId", "SchedulName");
                ViewBag.HolidayPackId = new SelectList(db.HolidayPackageLists, "HolidayPackId", "HolidayPackName");
                return View(model);
            }
        }
        public JsonResult SetNewUserSalary(long? id, int? Salary, int? Holiday, int? working_Scdl, long? userId)
        {
            if (id > 0 && Salary > 0 && Holiday > 0 && working_Scdl > 0 && userId > 0)
            {
                UserInformation model = db.UserInformations.Find(id);
                if (model != null)
                {
                    int status = -1;
                    model.SalaryPackageId = Salary;
                    model.HolidayPackId = Holiday;
                    model.WorkingScheduleId = working_Scdl;
                    model.Status = 1;
                    model.HolidayAssignedBy = userId;
                    model.HolidayAssignedDate = now;
                    model.WorkScheduleAssignedBy = userId;
                    model.WorkScheduleAssignedDate = now;
                    db.Entry(model).State = EntityState.Modified;
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
            }
            return Json("Error", JsonRequestBehavior.AllowGet);
        }



        //******************************* All User List ********************************************
        public ActionResult AllUserLists()
        {
            var list = new SelectList(db.View_UserLists.Where(m => m.Status > -1).Select(x => new { x.SiteId, x.SiteName }).Distinct(), "SiteId", "SiteName");
            ViewBag.AllSiteList = list;
            return View();
        }
        public PartialViewResult _AllUserLists(int? siteId, string UserId, int? typesort)
        {
            List<long> UserIds = new List<long>();
            if (UserId!=null) {
                var x=UserId.Split(',');
                for (int i = 0; i < x.Length; i++)
                {
                    UserIds.Add(Convert.ToInt64(x[i]));
                }
            }
            ViewBag.typesort = typesort;
            if (siteId == null && UserId == null)
            {
                var user = (from m in db.View_UserLists
                            where m.Status != -1
                            select new UserInformationModelView
                            {
                                UserId = m.UserId,
                                SiteId = m.SiteId,
                                UserName = m.UserName,
                                UserTypeName = m.UserTypeName,
                                UserType = m.UserType,
                                DesignationId = m.DesignationId,
                                Designation = m.DesignationName,
                                JoinDate = m.JoinDate,
                                JoinDateName = DbFunctions.TruncateTime(m.JoinDate).ToString(),
                                Picture = m.Picture,
                                AssginSalary = m.AssginSalary,
                                SalaryPackageId = m.SalaryPackageId,
                                Status=m.Status
                            }).OrderBy(o => o.UserName).ToList();
                user = (typesort == null) ? user.ToList() : (typesort == 1) ? user.OrderBy(x => x.UserTypeName).ToList() : user.OrderByDescending(x => x.UserTypeName).ToList();

                if (user != null)
                {
                    Session["AllUsers"] = user;
                    ViewBag.TotalUser = user.Count();
                    Session["TotalUserCount"] = user.Count();
                    ViewBag.UserList = user.Take(ShowUserPerPage).ToList();
                }
            }
            else
            {
                if (siteId != null)
                {
                    var user = (from m in db.View_UserLists
                                where m.SiteId == siteId && m.Status != -1
                                select new UserInformationModelView
                                {
                                    UserId = m.UserId,
                                    SiteId = m.SiteId,
                                    UserName = m.UserName,
                                    UserTypeName = m.UserTypeName,
                                    UserType = m.UserType,
                                    SiteIdName = m.SiteName,
                                    EmpId = m.EmpId,
                                    DesignationId = m.DesignationId,
                                    Designation = m.DesignationName,
                                    JoinDate = m.JoinDate,
                                    JoinDateName = DbFunctions.TruncateTime(m.JoinDate).ToString(),
                                    Picture = m.Picture,
                                    AssginSalary = m.AssginSalary,
                                    SalaryPackageId = m.SalaryPackageId
                                }).OrderBy(o => o.UserName).ToList();
                    user = (typesort == null) ? user.ToList() : (typesort == 1) ? user.OrderBy(x => x.UserTypeName).ToList() : user.OrderByDescending(x => x.UserTypeName).ToList();

                    Session["AllUsers"] = user;
                    ViewBag.TotalUser = user.Count();
                    Session["TotalUserCount"] = user.Count();
                    ViewBag.UserList = user.Take(ShowUserPerPage).ToList();
                }
                else
                {
                   // var allUser = (List<UserInformationModelView>)Session["AllUsers"];
                    var allUserBySearch = (from m in db.View_UserLists
                                           where UserIds.Contains(m.UserId) && m.Status != -1
                                           select new UserInformationModelView
                                           {
                                               UserId = m.UserId,
                                               SiteId = m.SiteId,
                                               UserName = m.UserName,
                                               UserType = m.UserType,
                                               UserTypeName = m.UserTypeName,
                                               DesignationId = m.DesignationId,
                                               Designation = m.Designation,
                                               JoinDate = m.JoinDate,
                                               Picture = m.Picture,
                                               AssginSalary = m.AssginSalary,
                                               SalaryPackageId = m.SalaryPackageId
                                           }).OrderBy(o => o.UserName).ToList();
                    allUserBySearch = (typesort == null) ? allUserBySearch.ToList() : (typesort == 1) ? allUserBySearch.OrderBy(x => x.UserTypeName).ToList() : allUserBySearch.OrderByDescending(x => x.UserTypeName).ToList();

                    Session["AllUsers"] = allUserBySearch;
                    ViewBag.TotalUser = allUserBySearch.Count();
                    Session["TotalUserCount"] = allUserBySearch.Count();
                    ViewBag.UserList = allUserBySearch.Take(ShowUserPerPage).ToList();
                }
            }
            return PartialView(ViewBag.UserList);
        }
        public PartialViewResult _AllUserListsStatusWise(int status, int? siteId, int? typesort)
        {
            ViewBag.typesort = typesort;
            var user = (from m in db.View_UserLists
                        where m.Status == status
                        select new UserInformationModelView
                        {
                            UserId = m.UserId,
                            SiteId = m.SiteId,
                            UserName = m.UserName,
                            UserTypeName = m.UserTypeName,
                            UserType = m.UserType,
                            DesignationId = m.DesignationId,
                            Designation = m.DesignationName,
                            JoinDate = m.JoinDate,
                            JoinDateName = DbFunctions.TruncateTime(m.JoinDate).ToString(),
                            Picture = m.Picture,
                            AssginSalary = m.AssginSalary,
                            SalaryPackageId = m.SalaryPackageId
                        }).OrderBy(o => o.UserName).ToList();
            user = (siteId == null) ? user.ToList() : user.Where(x => x.SiteId == siteId).ToList();
            user = (typesort == null) ? user.ToList() : (typesort == 1) ? user.OrderBy(x => x.UserTypeName).ToList() : user.OrderByDescending(x => x.UserTypeName).ToList();

            if (user != null)
            {
                Session["AllUsers"] = user;
                ViewBag.TotalUser = user.Count();
                Session["TotalUserCount"] = user.Count();
                ViewBag.UserList = user.Take(ShowUserPerPage).ToList();
            }

            //if (status > 0)
            //{
            //    var user = (from m in db.View_UserLists
            //                where m.Status > 0
            //                select new UserInformationModelView
            //                {
            //                    UserId = m.UserId,
            //                    SiteId = m.SiteId,
            //                    UserName = m.UserName,
            //                    UserTypeName = m.UserTypeName,
            //                    UserType = m.UserType,
            //                    DesignationId = m.DesignationId,
            //                    Designation = m.DesignationName,
            //                    JoinDate = m.JoinDate,
            //                    JoinDateName = DbFunctions.TruncateTime(m.JoinDate).ToString(),
            //                    Picture = m.Picture,
            //                    AssginSalary = m.AssginSalary,
            //                    SalaryPackageId = m.SalaryPackageId
            //                }).OrderBy(o => o.UserName).ToList();
            //    user = (siteId == null) ? user.ToList() : user.Where(x => x.SiteId == siteId).ToList();
            //    user = (typesort == null) ? user.ToList() : (typesort == 1) ? user.OrderBy(x => x.UserTypeName).ToList() : user.OrderByDescending(x => x.UserTypeName).ToList();
            //    if (user != null)
            //    {
            //        Session["AllUsers"] = user;
            //        ViewBag.TotalUser = user.Count();
            //        Session["TotalUserCount"] = user.Count();
            //        ViewBag.UserList = user.Take(ShowUserPerPage).ToList();
            //    }
            //}
            //else if (status == -1) {
            //    var user = (from m in db.View_UserLists
            //                where m.Status == -1 && (m.SiteId > 0 || m.SiteId == siteId)
            //                select new UserInformationModelView
            //                {
            //                    UserId = m.UserId,
            //                    SiteId = m.SiteId,
            //                    UserName = m.UserName,
            //                    UserTypeName = m.UserTypeName,
            //                    UserType = m.UserType,
            //                    DesignationId = m.DesignationId,
            //                    Designation = m.DesignationName,
            //                    JoinDate = m.JoinDate,
            //                    JoinDateName = DbFunctions.TruncateTime(m.JoinDate).ToString(),
            //                    Picture = m.Picture,
            //                    AssginSalary = m.AssginSalary,
            //                    SalaryPackageId = m.SalaryPackageId
            //                }).OrderBy(o => o.UserName).ToList();
            //    user = (siteId == null) ? user.ToList() : user.Where(x => x.SiteId == siteId).ToList();
            //    user = (typesort == null) ? user.ToList() : (typesort == 1) ? user.OrderBy(x => x.UserTypeName).ToList() : user.OrderByDescending(x => x.UserTypeName).ToList();

            //    if (user != null)
            //    {
            //        Session["AllUsers"] = user;
            //        ViewBag.TotalUser = user.Count();
            //        Session["TotalUserCount"] = user.Count();
            //        ViewBag.UserList = user.Take(ShowUserPerPage).ToList();
            //    }
            //}
            //else
            //{
            //    var user = (from m in db.View_UserLists
            //                where m.Status == 0 && (m.SiteId > 0 || m.SiteId == siteId)
            //                select new UserInformationModelView
            //                {
            //                    UserId = m.UserId,
            //                    SiteId = m.SiteId,
            //                    UserName = m.UserName,
            //                    UserTypeName = m.UserTypeName,
            //                    UserType = m.UserType,
            //                    DesignationId = m.DesignationId,
            //                    Designation = m.DesignationName,
            //                    JoinDate = m.JoinDate,
            //                    JoinDateName = DbFunctions.TruncateTime(m.JoinDate).ToString(),
            //                    Picture = m.Picture,
            //                    AssginSalary = m.AssginSalary,
            //                    SalaryPackageId = m.SalaryPackageId
            //                }).OrderBy(o => o.UserName).ToList();
            //    user = (siteId == null) ? user.ToList() : user.Where(x => x.SiteId == siteId).ToList();
            //    user = (typesort == null) ? user.ToList() : (typesort == 1) ? user.OrderBy(x => x.UserTypeName).ToList() : user.OrderByDescending(x => x.UserTypeName).ToList();

            //    if (user != null)
            //    {
            //        Session["AllUsers"] = user;
            //        ViewBag.TotalUser = user.Count();
            //        Session["TotalUserCount"] = user.Count();
            //        ViewBag.UserList = user.Take(ShowUserPerPage).ToList();
            //    }
            //}
            return PartialView("_AllUserLists", ViewBag.list);
        }
        public JsonResult ChangeUserStatus(long UserId,int status) {
            try
            {
                var userInfo = db.UserInformations.Where(x => x.UserId == UserId).FirstOrDefault();
                userInfo.Status = status;
                db.Entry(userInfo).State = EntityState.Modified;
                db.SaveChanges();
                return Json("Success", JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex) {
                return Json("Error", JsonRequestBehavior.DenyGet);
            }
        }
        public JsonResult GetAllUsersList() {
            var users = (from x in db.View_UserLists
                         select new
                         {
                             x.UserName,
                             x.UserId
                         }
                        ).ToList();
            return Json(users, JsonRequestBehavior.AllowGet);
        }
        public JsonResult GeUserList(int? page)
        {
            page = page ?? 0;
            int total = Convert.ToInt16(Session["TotalUserCount"]);
            int skip = ShowUserPerPage * Convert.ToInt16(page);
            var canPage = skip < total;
            var listBackFromSession = (List<UserInformationModelView>)Session["AllUsers"];
            var userList = listBackFromSession.Skip(skip).Take(ShowUserPerPage).ToList();
            return Json(userList, JsonRequestBehavior.AllowGet);
        }
        public JsonResult SearchUserList(string prefix)
        {
            if (prefix == "")
            {
                return Json("", JsonRequestBehavior.AllowGet);
            }
            else
            {
                var allUser = (List<UserInformationModelView>)Session["AllUsers"];
                var user = (from s in allUser
                            where (s.UserName.ToLower().Contains(prefix.ToLower()) || s.Designation.ToLower().Contains(prefix.ToLower()))
                            select new UserInformationModelView
                            {
                                UserId = s.UserId,
                                UserName = s.UserName + " " + "(" + s.Designation + ")",
                                UserType = s.UserType,
                                UserTypeName = s.UserTypeName,
                                DesignationId = s.DesignationId,
                                Designation = s.Designation,
                                JoinDate = s.JoinDate,
                                Picture = s.Picture,
                            }).ToList();
                Session["AllUsersBySearch"] = user;
                return Json(user, JsonRequestBehavior.AllowGet);
            }
        }
        public JsonResult DeleteUser(long Id, long CreatedBy)
        {
            string msg = "";
            int ColumnId = 0;
            int OperationStatus = 1;
            string name = "";
            if (db.UserInformations.Where(m => m.UserId == Id).Any())
            {
                UserInformation user = db.UserInformations.Find(Id);
                user.Status = -1;
                db.Entry(user).State = EntityState.Modified;
                try
                {
                    db.SaveChanges();
                    if (user.UserType == 1)
                    {
                        UserLogin login = db.UserLogins.Where(m => m.UserId == user.UserId).FirstOrDefault();
                        ColumnId = Convert.ToInt32(login.Id);
                        db.UserLogins.Remove(login);
                        try
                        {
                            db.SaveChanges();
                            OperationStatus = 1;
                            name = user.FirstName + " " + user.MiddleName + " " + user.LastName;
                            msg = "Login information of user" + " " + name + " " + "has been successfully deleted on " + now.ToString("MMM dd , yyy hh:mm tt");
                        }
                        catch (Exception ex)
                        {
                            string errmsg = ex.ToString();
                            OperationStatus = -1;
                            ColumnId = 0;
                            msg = "Login information of user" + " " + name + " " + "deletion unsuccessful on " + now.ToString("MMM dd , yyy hh:mm tt");
                        }
                        SaveAuditLog("Delete User Login Info", msg, "HR", "DeleteUser", "UserLogin", ColumnId, CreatedBy, now, OperationStatus);
                        ColumnId = Convert.ToInt32(user.UserId);
                    }
                    OperationStatus = 1;
                    name = user.FirstName + " " + user.MiddleName + " " + user.LastName;
                    msg = "Information of user" + " " + name + " " + "has been successfully deleted on " + now.ToString("MMM dd , yyy hh:mm tt");
                }
                catch (Exception)
                {
                    OperationStatus = -1;
                    ColumnId = 0;
                    msg = "Information of user" + " " + name + " " + "deletion unsuccessful on " + now.ToString("MMM dd , yyy hh:mm tt");
                }
                SaveAuditLog("Delete User Info", msg, "HR", "DeleteUser", "UserInformation", ColumnId, CreatedBy, now, OperationStatus);
                if (OperationStatus == 1)
                {
                    return Json("Success", JsonRequestBehavior.AllowGet);
                }
            }
            return Json("Error", JsonRequestBehavior.AllowGet);
        }


        //**************************************** Show User Info ************************************
        public PartialViewResult EmployeeBasicDetails_left(long UserId) {
            ViewBag.UserId = UserId;
            return PartialView();
        }
        public PartialViewResult _EmployeeBasicDetails(long UserId) {
            ViewBag.UserId = UserId;
            return PartialView();
        }
        public PartialViewResult EmployeeBasicDetails(long UserId) {
            var user = db.View_UserLists.Where(x => x.UserId == UserId).FirstOrDefault();
            return PartialView(user);
        }
        [EncryptedActionParameter]
        public ActionResult DisplayEmployeeDetails(long? UserId, bool isDisplay)
        {
            if (UserId == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            else
            {
                View_UserLists model = db.View_UserLists.Where(m => m.UserId == UserId).FirstOrDefault();
                UserInformationModelView deg = new UserInformationModelView();
                deg.UserId = model.UserId;
                deg.UserName = model.UserName;
                deg.FirstName = model.FirstName;
                deg.MiddleName = model.MiddleName;
                deg.LastName = model.LastName;
                deg.DateOfBirth = model.DateOfBirth;
                deg.Designation = model.DesignationName;
                deg.UserType = model.UserType;
                deg.UserTypeName = model.UserTypeName;
                deg.NationalId = model.NationalId;
                deg.Nationality = model.Nationality;
                deg.Religion = model.Religion;
                deg.ReligionName = model.ReligionName;
                deg.Gender = model.Gender;
                deg.GenderName = (model.Gender == 1) ? "Male" : "Female";
                deg.Phone = model.Phone;
                deg.EmailAddress = model.EmailAddress;
                deg.ParAddress = model.ParAddress;
                deg.ParAddressLine1 = model.ParAddressLine1;
                deg.ParArea = model.ParArea;
                deg.ParCity = model.ParCity;
                deg.ParDivisionId = model.ParDivisionId;
                deg.PermanentDivisionName = model.ParDivisionName;
                deg.ParPotalCode = model.ParPotalCode;
                deg.ParCountry = model.ParCountry;
                deg.Picture = model.Picture;
                deg.PreAddress = model.PreAddress;
                deg.PreAddressLine1 = model.PreAddressLine1;
                deg.PreArea = model.PreArea;
                deg.PreCity = model.PreCity;
                deg.PreCountry = model.PreCountry;
                deg.PreDivisionId = model.PreDivisionId;
                deg.PresentDivisionName = model.PreDivisionName;
                deg.PrePostalCode = model.PrePostalCode;
                deg.MobileNo = model.MobileNo;
                deg.EmpId = model.EmpId;
                deg.DeptId = model.DeptId;
                deg.DeptIdName = model.DeptName;
                deg.SiteId = model.SiteId; ;
                deg.SiteIdName = model.SiteName;
                deg.LineId = model.LineId;
                deg.LineIdName = db.LineInfoes.Where(m => m.LineId == model.LineId).Select(m => m.LineName).SingleOrDefault();
                deg.MachineId = model.MachineId;
                deg.MachineIdName = db.MachineLists.Where(m => m.MachineId == model.MachineId).Select(m => m.MachineAcronym).SingleOrDefault();
                deg.SamePresentAddress = model.SamePresentAddress;
                deg.AssginSalary = model.AssginSalary;
                deg.SalaryPackageId = model.SalaryPackageId;
                deg.isDisplay = isDisplay;
                deg.HolidayPackId = model.HolidayPackId;
                deg.WorkingScheduleId = (model.WorkingScheduleId > 0) ? model.WorkingScheduleId.Value : 0;
                deg.Status = model.Status;
                return View(deg);
            }
        }
        public PartialViewResult _ShowEmpInfo(long? UserId, bool isDisplay)
        {
            View_UserLists model = db.View_UserLists.Where(m => m.UserId == UserId).FirstOrDefault();
            UserInformationModelView deg = new UserInformationModelView();
            deg.UserId = model.UserId;
            deg.TitleName = model.TitleName;
            deg.UserName = model.UserName;
            deg.FirstName = model.FirstName;
            deg.MiddleName = model.MiddleName;
            deg.LastName = model.LastName;
            deg.DateOfBirth = model.DateOfBirth;
            deg.Designation = model.DesignationName;
            deg.UserType = model.UserType;
            deg.UserTypeName = model.UserTypeName;
            deg.NationalId = model.NationalId;
            deg.Nationality = model.Nationality;
            deg.NationalityName = db.CountryLists.Where(m => m.CountryCode == model.Nationality).Select(m => m.CountryName).SingleOrDefault();
            deg.Religion = model.Religion;
            deg.ReligionName = model.ReligionName;
            deg.Gender = model.Gender;
            deg.GenderName = (model.Gender == 1) ? "Male" : "Female";
            deg.Phone = model.Phone;
            deg.CardNumber = model.CardNumber;
            // deg.SmartCardId = model.
            deg.EmailAddress = model.EmailAddress;
            deg.ParAddress = model.ParAddress;
            deg.ParAddressLine1 = model.ParAddressLine1;
            deg.ParArea = model.ParArea;
            deg.ParCity = model.ParCity;
            deg.ParDivisionId = model.ParDivisionId;
            deg.PermanentDivisionName = model.ParDivisionName;
            deg.ParPotalCode = model.ParPotalCode;
            deg.ParCountry = model.ParCountry;
            deg.ParCountryName = model.ParCountryName;
            deg.ParState = model.ParState;
            deg.Picture = model.Picture;
            deg.PreAddress = model.PreAddress;
            deg.PreAddressLine1 = model.PreAddressLine1;
            deg.PreArea = model.PreArea;
            deg.PreCity = model.PreCity;
            deg.PreCountry = model.PreCountry;
            deg.PreCountryName = model.PreCountryName;
            deg.PreState = model.PreState;
            deg.PreDivisionId = model.PreDivisionId;
            deg.PresentDivisionName = model.PreDivisionName;
            deg.PrePostalCode = model.PrePostalCode;
            deg.MobileNo = model.MobileNo;
            deg.EmpId = model.EmpId;
            deg.DeptId = model.DeptId;
            deg.DeptIdName = model.DeptName;
            deg.SiteId = model.SiteId; ;
            deg.SiteIdName = model.SiteName;
            deg.LineId = model.LineId;
            deg.JoinDate = model.JoinDate;
            deg.UnitId = model.UnitId;
            deg.UnitIdName = db.Site_Unit_Lists.Where(m => m.UnitId == model.UnitId).Select(m => m.UnitName).SingleOrDefault();
            deg.LineIdName = db.LineInfoes.Where(m => m.LineId == model.LineId).Select(m => m.LineName).SingleOrDefault();
            deg.MachineId = model.MachineId;
            deg.MachineIdName = db.MachineLists.Where(m => m.MachineId == model.MachineId).Select(m => m.MachineAcronym).SingleOrDefault();
            deg.NationalIdFontImg = model.NationalIdFontImg;
            deg.NationalIdBackImg = model.NationalIdBackImg;
            deg.SamePresentAddress = model.SamePresentAddress;
            deg.AssginSalary = model.AssginSalary;
            deg.SalaryPackageId = model.SalaryPackageId;
            deg.isDisplay = isDisplay;
            return PartialView(deg);
        }
        public PartialViewResult EditBasicInformationPopUp(long? UserId)
        {
            ViewBag.UserId = UserId;
            return PartialView();
        }
        public PartialViewResult EditBasicInformation(long? UserId)
        {
            View_UserLists model = db.View_UserLists.Where(m => m.UserId == UserId).FirstOrDefault();
            UserInformationModelView deg = new UserInformationModelView();
            deg.UserId = model.UserId;
            deg.Title = model.Title;
            deg.UserName = model.UserName;
            deg.FirstName = model.FirstName;
            deg.MiddleName = model.MiddleName;
            deg.Picture = model.Picture;
            deg.NationalIdFontImg = model.NationalIdFontImg;
            deg.NationalIdBackImg = model.NationalIdBackImg;
            deg.LastName = model.LastName;
            deg.DateOfBirth = model.DateOfBirth;
            deg.DesignationId = model.DesignationId;
            deg.Designation = model.DesignationName;
            deg.UserType = model.UserType;
            deg.NationalId = model.NationalId;
            deg.Nationality = model.Nationality;
            deg.Religion = model.Religion;
            deg.ReligionName = model.ReligionName;
            deg.Gender = model.Gender;
            deg.Phone = model.Phone;
            deg.CreatedBy = model.CreatedBy;
            deg.EmpId = model.EmpId;
            deg.SamePresentAddress = model.SamePresentAddress;
            deg.JoinDate = model.JoinDate;
            ViewBag.Religion = new SelectList(db.ReligionLists, "ReligionId", "Religion", deg.Religion);
            ViewBag.CountryId = new SelectList(db.CountryLists, "CountryCode", "CountryName");
            ViewBag.TitleId = new SelectList(db.NameTitles, "Id", "Name");
            return PartialView(deg);
        }
        public JsonResult PersonalInformationUpdate(UserInformationModelView model, string DateOfBirth, long UserId, string UserTypeChange, IEnumerable<HttpPostedFileBase> files)
        {
            string msg = "";
            int ColumnId = 0;
            int OperationStatus = 1;
            string NtlId_FrntImg = "";
            string NtlId_BckImg = "";
            string Paths = Server.MapPath("~/Images/UserPicture/");

            if (!Directory.Exists(Paths))
            {
                Directory.CreateDirectory(Paths);
            }
            UserInformation user = db.UserInformations.Find(UserId);
            user.FirstName = model.FirstName;
            user.MiddleName = model.MiddleName;
            user.LastName = model.LastName;
            user.DateOfBirth = Convert.ToDateTime(DateOfBirth);
            user.Gender = model.Gender;
            user.Nationality = model.Nationality;
            user.NationalId = model.NationalId;
            user.Gender = model.Gender;
            user.Religion = model.Religion;
            user.BasicSalary = model.BasicSalary;
            user.UpdatedBy = model.CreatedBy;
            user.UpdatedDate = now;
            user.Status = 1;
            db.Entry(user).State = EntityState.Modified;
            try
            {
                db.SaveChanges();
                OperationStatus = 1;
                msg = "Basic information of user '" + model.UserName + "' has been successfully updated on " + now.ToString("MMM dd , yyy hh:mm tt") + " .";
            }
            catch (Exception ex)
            {
                string errmsg = ex.ToString();
                OperationStatus = -1;
                msg = "Basic information of user '" + model.UserName + "' update unsuccessful on " + now.ToString("MMM dd , yyy hh:mm tt") + " .";
            }
            ColumnId = Convert.ToInt32(model.UserId);
            SaveAuditLog("Update User Basic Information", msg, "HR", "UserInformation", "UserInformation", ColumnId, model.CreatedBy, now, OperationStatus);

            if (files != null)
            {
                int i = 0;
                UserInformation anuser = db.UserInformations.Find(user.UserId);
                foreach (var file in files)
                {
                    string s = file.FileName;
                    int idx = s.LastIndexOf('.');
                    string fileName = s.Substring(0, idx);
                    string extension = s.Substring(idx);
                    if (i == 0)
                    {
                        NtlId_FrntImg = "NtlId_FrntImg" + ColumnId + extension;
                        anuser.NationalIdFontImg = NtlId_FrntImg;
                    }
                    else
                    {
                        NtlId_BckImg = "NtlId_BckImg" + ColumnId + extension;
                        anuser.NationalIdBackImg = NtlId_BckImg;
                    }
                    i++;
                }
                db.Entry(user).State = EntityState.Modified;
                db.SaveChanges();
            }
            if (files != null)
            {

                int i = 0;
                foreach (var file in files)
                {
                    if (i == 0)
                    {
                        Paths = Path.Combine(Server.MapPath("~/Images/UserPicture/NationalId/Original"), NtlId_FrntImg);
                        file.SaveAs(Paths);
                        WebImage imgForGallery = new WebImage(file.InputStream);
                        if (imgForGallery.Width > 70)
                        {
                            imgForGallery.Resize(29, 29, false, true);
                        }
                        string PathsResize = Path.Combine(Server.MapPath("~/Images/UserPicture/NationalId/thumb"), NtlId_FrntImg);
                        imgForGallery.Save(PathsResize);
                    }
                    else
                    {
                        Paths = Path.Combine(Server.MapPath("~/Images/UserPicture/NationalId/Original"), NtlId_BckImg);
                        file.SaveAs(Paths);
                        WebImage imgForGallery = new WebImage(file.InputStream);
                        if (imgForGallery.Width > 70)
                        {
                            imgForGallery.Resize(29, 29, false, true);
                        }
                        string PathsResize = Path.Combine(Server.MapPath("~/Images/UserPicture/NationalId/thumb"), NtlId_BckImg);
                        imgForGallery.Save(PathsResize);
                    }
                    i++;
                }

            }

            if (OperationStatus == 1)
            {

                return Json(model.UserName, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json("Error", JsonRequestBehavior.AllowGet);
            }
        }
        public PartialViewResult EditPermanentAddressPopUp(long? UserId)
        {
            ViewBag.UserId = UserId;
            return PartialView();
        }
        public PartialViewResult EditPermanentAddress(long? UserId)
        {
            View_UserLists model = db.View_UserLists.Where(m => m.UserId == UserId).FirstOrDefault();
            UserInformationModelView deg = new UserInformationModelView();
            deg.UserName = model.UserName;
            deg.UserId = model.UserId;
            deg.ParAddress = model.ParAddress;
            deg.ParAddressLine1 = model.ParAddressLine1;
            deg.ParArea = model.ParArea;
            deg.ParState = model.ParState;
            deg.ParCity = model.ParCity;
            deg.ParDivisionId = model.ParDivisionId;
            deg.PermanentDivisionName = db.DivisionLists.Where(m => m.DivisionId == model.ParDivisionId).Select(m => m.DivisionName).SingleOrDefault();
            deg.ParPotalCode = model.ParPotalCode;
            deg.ParCountry = model.ParCountry;
            deg.SamePresentAddress = model.SamePresentAddress;
            ViewBag.CountryId = new SelectList(db.CountryLists, "CountryCode", "CountryName");
            ViewBag.DivisionList = new SelectList(db.DivisionLists, "DivisionId", "DivisionName");
            ViewBag.DesignationId = new SelectList(db.Designations, "DesignationId", "DesignationName");
            return PartialView(deg);
        }
        public JsonResult PermanentAddressUpdate(UserInformationModelView model, long UserId)
        {
            string msg = "";
            int ColumnId = 0;
            int OperationStatus = 1;
            string name = "";
            UserInformation user = db.UserInformations.Find(model.UserId);
            user.ParAddress = model.ParAddress;
            user.ParAddressLine1 = model.ParAddressLine1;
            user.ParCountry = model.ParCountry;
            user.ParDivisionId = model.ParDivisionId;
            user.ParState = model.ParState;
            user.ParCity = model.ParCity;
            user.ParArea = model.ParArea;
            user.ParPotalCode = model.ParPotalCode;
            user.UpdatedBy = model.CreatedBy;
            user.UpdatedDate = now;
            user.SamePresentAddress = model.SamePresentAddress;
            if (model.SamePresentAddress == true)
            {
                user.PreAddress = model.ParAddress;
                user.PreAddressLine1 = model.ParAddressLine1;
                user.PreCountry = model.ParCountry;
                user.PreDivisionId = model.ParDivisionId;
                user.PreCity = model.ParCity;
                user.PrePostalCode = model.ParPotalCode;
                user.PreArea = model.ParArea;
                user.PreState = model.ParState;
                user.SamePresentAddress = true;
            }
            db.UserInformations.Attach(user);
            db.Entry(user).State = EntityState.Modified;
            if (user.MiddleName == "")
            {
                name = user.FirstName + " " + user.LastName;
            }
            else
            {
                name = user.FirstName + " " + user.MiddleName + " " + user.LastName;
            }
            try
            {
                db.SaveChanges();
                OperationStatus = 1;
                ColumnId = Convert.ToInt32(UserId);
                msg = "Permanent Address of user '" + name + "' has been successfully updated on " + now.ToString("MMM dd , yyy hh:mm tt") + " .";
            }
            catch (Exception)
            {
                OperationStatus = -1;
                msg = "Permanent Address of user '" + name + "' update unsuccessful on " + now.ToString("MMM dd , yyy hh:mm tt") + " .";
                throw;
            }
            SaveAuditLog("Update User Permanent Address", msg, "HR", "UserInformation", "UserInformation", ColumnId, model.CreatedBy, now, OperationStatus);
            if (OperationStatus == 1)
            {
                return Json(name, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json("Error", JsonRequestBehavior.AllowGet);
            }
        }
        public PartialViewResult EditPresentAddressPopUp(long? UserId)
        {
            ViewBag.UserId = UserId;
            return PartialView();
        }
        public PartialViewResult EditPresentAddress(long? UserId)
        {
            View_UserLists model = db.View_UserLists.Where(m => m.UserId == UserId).FirstOrDefault();
            UserInformationModelView deg = new UserInformationModelView();
            deg.UserId = model.UserId;
            deg.UserName = model.UserName;
            deg.PreAddress = model.PreAddress;
            deg.PreAddressLine1 = model.PreAddressLine1;
            deg.PreState = model.PreState;
            deg.PreArea = model.PreArea;
            deg.PreCity = model.PreCity;
            deg.PreCountry = model.PreCountry;
            deg.PreDivisionId = model.PreDivisionId;
            deg.PresentDivisionName = db.DivisionLists.Where(m => m.DivisionId == model.PreDivisionId).Select(m => m.DivisionName).SingleOrDefault();
            deg.PrePostalCode = model.PrePostalCode;
            deg.SamePresentAddress = model.SamePresentAddress;
            ViewBag.CountryId = new SelectList(db.CountryLists, "CountryCode", "CountryName");
            ViewBag.DivisionList = new SelectList(db.DivisionLists, "DivisionId", "DivisionName");
            ViewBag.DesignationId = new SelectList(db.Designations, "DesignationId", "DesignationName");
            return PartialView(deg);
        }
        public JsonResult PresentAddressUpdate(UserInformationModelView model, long UserId)
        {
            string msg = "";
            int ColumnId = 0;
            int OperationStatus = 1;
            string name = "";
            UserInformation user = db.UserInformations.Find(model.UserId);
            user.PreAddress = model.PreAddress;
            user.PreAddressLine1 = model.PreAddressLine1;
            user.PreCountry = model.PreCountry;
            user.PreDivisionId = model.PreDivisionId;
            user.PreCity = model.PreCity;
            user.PreArea = model.PreArea;
            user.PrePostalCode = model.PrePostalCode;
            user.UpdatedBy = model.CreatedBy;
            user.UpdatedDate = now;
            db.UserInformations.Attach(user);
            db.Entry(user).State = EntityState.Modified;
            db.SaveChanges();
            if (user.MiddleName == "")
            {
                name = user.FirstName + " " + user.LastName;
            }
            else
            {
                name = user.FirstName + " " + user.MiddleName + " " + user.LastName;
            }
            try
            {
                db.SaveChanges();
                OperationStatus = 1;
                ColumnId = Convert.ToInt32(UserId);
                msg = "Present Address of user '" + name + "' has been successfully updated on " + now.ToString("MMM dd , yyy hh:mm tt") + " .";
            }
            catch (Exception)
            {
                OperationStatus = -1;
                msg = "Present Address of user '" + name + "' update unsuccessful on " + now.ToString("MMM dd , yyy hh:mm tt") + " .";
            }
            SaveAuditLog("Update User Present Address", msg, "HR", "UserInformation", "UserInformation", ColumnId, model.CreatedBy, now, OperationStatus);

            if (OperationStatus == 1)
            {
                return Json(name, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json("Error", JsonRequestBehavior.AllowGet);
            }
        }
        public PartialViewResult EditCompanyInformationPopUp(long? UserId)
        {
            ViewBag.UserId = UserId;
            return PartialView();
        }
        public PartialViewResult EditCompanyInfo(long? UserId)
        {
            View_UserLists model = db.View_UserLists.Where(m => m.UserId == UserId).FirstOrDefault();
            UserInformationModelView deg = new UserInformationModelView();
            deg.UserId = model.UserId;
            deg.UserName = model.UserName;
            deg.UserType = model.UserType;
            deg.EmpId = model.EmpId;
            deg.DesignationId = model.DesignationId;
            deg.JoinDate = model.JoinDate;
            deg.SiteId = model.SiteId;
            deg.SiteIdName = model.SiteName;
            deg.DeptId = model.DeptId;
            deg.DeptIdName = model.DeptName;
            deg.LineId = model.LineId;
            deg.UnitId = model.UnitId;
            deg.MachineId = model.MachineId;
            var SiteList = (from m in db.SiteLists
                            where m.IsResidential == false && m.Status == 1
                            select new
                            {
                                Id = m.Id,
                                SiteName = m.SiteName,
                                IsComposite = m.IsComposite
                            }).ToList();
            ViewBag.SiteId = SiteList;
            ViewBag.CountryId = new SelectList(db.CountryLists, "CountryCode", "CountryName");
            ViewBag.DivisionList = new SelectList(db.DivisionLists, "DivisionId", "DivisionName");
            ViewBag.DesignationId = new SelectList(db.Designations, "DesignationId", "DesignationName");
            return PartialView(deg);
        }
        public JsonResult CompanyInformationUpdate(UserInformationModelView model, long UserId, string UserTypeChange)
        {
            string msg = "";
            int ColumnId = 0;
            int OperationStatus = 1;
            UserInformation user = db.UserInformations.Find(UserId);
            user.UserType = model.UserType;
            user.DesignationId = model.DesignationId;
            user.JoinDate = model.JoinDate;
            user.EmpId = model.EmpId;
            user.SiteId = model.SiteId;
            user.DeptId = model.DeptId;
            user.UnitId = model.UnitId;
            if (model.UserType == 2)
            {
                user.LineId = model.LineId;
                user.MachineId = model.MachineId;
            }
            else
            {
                user.LineId = null;
                user.MachineId = null;
            }
            db.Entry(user).State = EntityState.Modified;
            try
            {
                db.SaveChanges();
                OperationStatus = 1;
                msg = "Company information of user '" + model.UserName + "' has been successfully updated on " + now.ToString("MMM dd , yyy hh:mm tt") + " .";
            }
            catch (Exception ex)
            {
                string errmsg = ex.ToString();
                OperationStatus = -1;
                msg = "Company information of user '" + model.UserName + "' update unsuccessful on " + now.ToString("MMM dd , yyy hh:mm tt") + " .";
            }
            ColumnId = Convert.ToInt32(model.UserId);
            SaveAuditLog("Update Company Information", msg, "HR", "UserInformation", "UserInformation", ColumnId, model.CreatedBy, now, OperationStatus);
            if (UserTypeChange == "1" && user.UserType == 1)
            {
                Random generator = new Random();
                UserLogin ulogin = new UserLogin();
                ulogin.UserName = model.LastName.Substring(0, 1) + model.FirstName.Substring(0, 1);
                LettersCount md = (from m in db.LettersCounts
                                   where m.Letters.Equals(ulogin.UserName)
                                   select m).FirstOrDefault();
                int count = md.Count + 1;
                ulogin.UserName = ulogin.UserName + count.ToString("000");
                ulogin.Password = generator.Next(0, 1000000).ToString("D6");
                ulogin.UserId = db.UserInformations.Max(m => m.UserId);
                ulogin.Entrydate = now;
                ulogin.EntryBy = model.CreatedBy;
                try
                {
                    db.UserLogins.Add(ulogin);
                    db.SaveChanges();
                    OperationStatus = 1;
                    msg = "Login Information of user '" + model.UserName + "' has been successfully added on" + now.ToString("MMM dd , yyy hh:mm tt") + " .";
                }
                catch (Exception ex)
                {
                    string errmsg = ex.ToString();
                    OperationStatus = -1;
                    msg = "Login Information of user '" + model.UserName + "' add unsuccessful on" + now.ToString("MMM dd , yyy hh:mm tt") + " .";
                }
                ColumnId = Convert.ToInt32(db.UserLogins.Where(m => m.EntryBy == model.CreatedBy).Max(m => m.Id));
                SaveAuditLog("Create User Login Info", msg, "HR", "UserInformation", "UserLogin", ColumnId, model.CreatedBy, now, OperationStatus);
                md.Count = count;
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
        public PartialViewResult EditCardInfoPopUp(long? UserId)
        {
            ViewBag.UserId = UserId;
            return PartialView();
        }
        public PartialViewResult EditCardInformation(long? UserId)
        {
            View_UserLists model = db.View_UserLists.Where(m => m.UserId == UserId).FirstOrDefault();
            UserInformationModelView deg = new UserInformationModelView();
            deg.UserId = model.UserId;
            deg.UserName = model.UserName;
            deg.CardNumber = model.CardNumber;
            return PartialView(deg);
        }
        public JsonResult CardInformationUpdate(UserInformationModelView model, long UserId)
        {
            string msg = "";
            int ColumnId = 0;
            int OperationStatus = 1;
            string name = "";
            UserInformation user = db.UserInformations.Find(model.UserId);
            user.CardNumber = model.CardNumber;
            db.UserInformations.Attach(user);
            db.Entry(user).State = EntityState.Modified;
            if (user.MiddleName == "")
            {
                name = user.FirstName + " " + user.LastName;
            }
            else
            {
                name = user.FirstName + " " + user.MiddleName + " " + user.LastName;
            }
            try
            {
                db.SaveChanges();
                OperationStatus = 1;
                ColumnId = Convert.ToInt32(UserId);
                msg = "Card information of user '" + name + "' has been successfully updated on " + now.ToString("MMM dd , yyy hh:mm tt") + " .";
            }
            catch (Exception)
            {
                OperationStatus = -1;
                msg = "Card informnation of user '" + name + "' update unsuccessful on " + now.ToString("MMM dd , yyy hh:mm tt") + " .";
                throw;
            }
            SaveAuditLog("Update User Card Information", msg, "HR", "UserInformation", "UserInformation", ColumnId, model.CreatedBy, now, OperationStatus);
            if (OperationStatus == 1)
            {
                return Json(name, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json("Error", JsonRequestBehavior.AllowGet);
            }
        }
        public PartialViewResult EditContactInfoPopup(long? UserId)
        {
            ViewBag.UserId = UserId;
            return PartialView();
        }
        public PartialViewResult EditContactInfo(long? UserId)
        {
            View_UserLists model = db.View_UserLists.Where(m => m.UserId == UserId).FirstOrDefault();
            UserInformationModelView deg = new UserInformationModelView();
            deg.UserId = model.UserId;
            deg.UserName = model.UserName;
            deg.Phone = model.Phone;
            deg.MobileNo = model.MobileNo;
            deg.EmailAddress = model.EmailAddress;
            return PartialView(deg);
        }
        public JsonResult ContactInformationUpdate(UserInformationModelView model, long UserId)
        {
            string msg = "";
            int ColumnId = 0;
            int OperationStatus = 1;
            string name = "";
            UserInformation user = db.UserInformations.Find(model.UserId);
            user.EmailAddress = model.EmailAddress;
            user.Phone = model.Phone;
            user.MobileNo = model.MobileNo;
            db.UserInformations.Attach(user);
            db.Entry(user).State = EntityState.Modified;
            if (user.MiddleName == "")
            {
                name = user.FirstName + " " + user.LastName;
            }
            else
            {
                name = user.FirstName + " " + user.MiddleName + " " + user.LastName;
            }
            try
            {
                db.SaveChanges();
                OperationStatus = 1;
                ColumnId = Convert.ToInt32(UserId);
                msg = "Contact information of user '" + name + "' has been successfully updated on " + now.ToString("MMM dd , yyy hh:mm tt") + " .";
            }
            catch (Exception)
            {
                OperationStatus = -1;
                msg = "Contact informnation of user '" + name + "' update unsuccessful on " + now.ToString("MMM dd , yyy hh:mm tt") + " .";
                throw;
            }
            SaveAuditLog("Update User Contact Information", msg, "HR", "UserInformation", "UserInformation", ColumnId, model.CreatedBy, now, OperationStatus);
            if (OperationStatus == 1)
            {
                return Json(name, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json("Error", JsonRequestBehavior.AllowGet);
            }
        }
        public PartialViewResult _EditUserImage(long UserId)
        {
            ViewBag.UserId = UserId;
            return PartialView();
        }
        public ActionResult EditUserImage(long UserId)
        {
            View_UserLists model = db.View_UserLists.Where(m => m.UserId == UserId).FirstOrDefault();
            UserInformationModelView deg = new UserInformationModelView();
            deg.UserId = model.UserId;
            deg.UserName = model.UserName;
            deg.Picture = model.Picture;
            return PartialView(deg);
        }

        //***************************************************************** Picture ************************************************
        public JsonResult UpdateUserPicture(int UserId, long CreatedBy, HttpPostedFileBase files)
        {
            string msg = "";
            int ColumnId = UserId;
            int OperationStatus = 1;
            string fullName = "";
            string userName = "";

            if (!db.UserInformations.Where(m => m.UserId == UserId).Any())
            {
                return Json("Error", JsonRequestBehavior.AllowGet);
            }
            else
            {
                UserInformation user = db.UserInformations.Find(UserId);
                if (user.MiddleName == "")
                {
                    userName = user.FirstName + " " + user.LastName;
                }
                else
                {
                    userName = user.FirstName + " " + user.MiddleName + " " + user.LastName;
                }
                if (user.Picture != null && user.Picture != "")
                {
                    string FilePath = Path.Combine(Server.MapPath("~/Images/UserPicture/Original"), user.Picture);
                    if (System.IO.File.Exists(FilePath))
                    {
                        System.IO.File.Delete(FilePath);
                    }
                    string FilePathThumb = Path.Combine(Server.MapPath("~/Images/UserPicture/thumb"), user.Picture);
                    if (System.IO.File.Exists(FilePathThumb))
                    {
                        System.IO.File.Delete(FilePathThumb);
                    }
                }
                if (files != null)
                {
                    Random generator = new Random();
                    string random = generator.Next(0, 1000000).ToString("D6");
                    string s = files.FileName;
                    int idx = s.LastIndexOf('.');
                    string fileName = s.Substring(0, idx);
                    string extension = s.Substring(idx);
                    fullName = "Emp" + user.UserId + random + extension;

                    byte[] imageByte = null;
                    BinaryReader rdr = new BinaryReader(files.InputStream);
                    imageByte = rdr.ReadBytes((int)files.ContentLength);
                    user.Picture = fullName;
                    user.ImageData = imageByte;
                    db.Entry(user).State = EntityState.Modified;
                    try
                    {
                        db.SaveChanges();
                        OperationStatus = 1;
                        string Paths = "";
                        Paths = Path.Combine(Server.MapPath("~/Images/UserPicture/Original"), fullName);
                        files.SaveAs(Paths);
                        WebImage imgForGallery = new WebImage(files.InputStream);
                        if (imgForGallery.Width > 70)
                        {
                            imgForGallery.Resize(29, 29, false, true);
                        }
                        string PathsResize = Path.Combine(Server.MapPath("~/Images/UserPicture/thumb"), fullName);
                        imgForGallery.Save(PathsResize);
                        msg = "Picture of user" + " " + "'" + userName + "'" + " " + "has been successfully changed on " + now.ToString("MMM dd , yyy hh:mm tt");
                    }
                    catch (Exception ex)
                    {
                        string errmsg = ex.ToString();
                        OperationStatus = -1;
                        msg = "Picture of user" + " " + "'" + userName + "'" + " " + "change unsuccessful on " + now.ToString("MMM dd , yyy hh:mm tt");
                    }
                }
                else
                {
                    Random generator = new Random();
                    string random = generator.Next(0, 1000000).ToString("D6");
                    string s = "blank-person.jpg";
                    int idx = s.LastIndexOf('.');
                    string fileName = s.Substring(0, idx);
                    string extension = s.Substring(idx);
                    fullName = "Emp" + user.UserId + random + extension;

                    try
                    {
                        System.IO.File.Copy(Server.MapPath("~/Images/Icon/blank-person.jpg"), Server.MapPath("~/Images/UserPicture/Original/" + fullName));
                        var blankImage = System.IO.File.ReadAllBytes(Server.MapPath("~/Images/Icon/blank-person.jpg"));
                        WebImage imgForGallery = new WebImage(blankImage);
                        if (imgForGallery.Width > 70)
                        {
                            imgForGallery.Resize(29, 29, false, true);
                        }
                        string PathsResize = Path.Combine(Server.MapPath("~/Images/UserPicture/thumb"), fullName);
                        imgForGallery.Save(PathsResize);

                        user.Picture = fullName;
                        user.ImageData = blankImage;
                        db.Entry(user).State = EntityState.Modified;
                        db.SaveChanges();
                        OperationStatus = 1;
                        msg = "Picture of user" + " " + "'" + userName + "'" + " " + "has been successfully changed to blank image on " + now.ToString("MMM dd , yyy hh:mm tt");
                    }
                    catch (Exception ex)
                    {
                        string errmsg = ex.ToString();
                        OperationStatus = -1;
                        msg = "Picture of user" + " " + "'" + userName + "'" + " " + "change unsuccessful on " + now.ToString("MMM dd , yyy hh:mm tt");
                    }
                }

                SaveAuditLog("User Picture Change", msg, "HR", "ShowEmployeeDetails", "UserInformation", UserId, CreatedBy, now, OperationStatus);
                if (OperationStatus == 1)
                {
                    return Json(fullName, JsonRequestBehavior.AllowGet);
                }

                return Json("Error", JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult DisplayNationalIdImages(long UserId)
        {
            View_UserLists model = db.View_UserLists.Where(m => m.UserId == UserId).FirstOrDefault();
            UserInformationModelView deg = new UserInformationModelView();
            deg.UserId = model.UserId;
            deg.UserName = model.UserName;
            deg.NationalIdFontImg = model.NationalIdFontImg;
            deg.NationalIdBackImg = model.NationalIdBackImg;
            return PartialView(deg);

        }
        public FileResult DownloadNIDFront(long UserId)
        {
            UserInformation model = db.UserInformations.Find(UserId);
            string fullPath = Path.Combine(Server.MapPath("~/Images/UserPicture/NationalId/Original"), model.NationalIdFontImg);
            byte[] fileBytes = System.IO.File.ReadAllBytes(fullPath);
            string fileName = model.NationalIdFontImg;
            string orginalFilename = fileName.Split('_')[1];
            return File(fileBytes, System.Net.Mime.MediaTypeNames.Application.Octet, orginalFilename);
        }
        public FileResult DownloadNIDBack(long UserId)
        {
            UserInformation model = db.UserInformations.Find(UserId);
            string fullPath = Path.Combine(Server.MapPath("~/Images/UserPicture/NationalId/Original"), model.NationalIdBackImg);

            byte[] fileBytes = System.IO.File.ReadAllBytes(fullPath);
            string fileName = model.NationalIdBackImg;
            string orginalFilename = fileName.Split('_')[1];
            return File(fileBytes, System.Net.Mime.MediaTypeNames.Application.Octet, orginalFilename);
        }

        public PartialViewResult _UserSalaryTab(long? UserId, int UserType, int? PackageId)
        {
            ViewBag.UserId = UserId;
            ViewBag.UserType = UserType;
            ViewBag.PackageId = PackageId;
            return PartialView();
        }
        public PartialViewResult _UserSalary(long? UserId, int? PackageId, int Status)
        {
            ViewBag.Status = Status;
            if (Status == 1)
            {
                var list = db.EmpSalaryPaymentDetails.Where(m => m.UserId == UserId && m.PaidAmount > 0).ToList();
                ViewBag.total_paid_amnt = db.EmpSalaryPaymentDetails.Where(m => m.UserId == UserId).Select(m => m.PaidAmount).Sum();
                return PartialView(list);
            }
            else
            {
                var list = db.EmpSalaryPaymentDetails.Where(m => m.UserId == UserId && m.Status == 0).ToList();
                ViewBag.total_paid_amnt = db.EmpSalaryPaymentDetails.Where(m => m.UserId == UserId).Select(m => m.PaidAmount).Sum();
                return PartialView(list);
            }

        }
        public PartialViewResult _PaidSalaryPackdetails(int id)
        {
            var list = db.SalaryPaymentPckDetails.Where(m => m.PaymentId == id).ToList();
            return PartialView(list);
        }

        public PartialViewResult _UserAttendanceTab(long? UserId)
        {
            ViewBag.UserId = UserId;
            return PartialView();
        }
        public PartialViewResult UserAllAttendList(long UserId, string status, int? wrk_scdl_id)
        {
            ViewBag.wrk_scdl_id = wrk_scdl_id;
            var list = (from m in db.View_Attendance_BCR
                        where m.UserId == UserId
                        select new View_Attendance_BCRModelView
                        {
                            Id = m.Id,
                            UserId = m.UserId,
                            EntryDateTime = m.EntryDateTime,
                            Dates = m.Dates,
                            DayName = m.DayName,
                            EntryDateTimeFormate = m.EntryDateTimeFormate,
                            IsIN = m.IsIN,
                            EntryType = m.EntryType,
                            UserName = m.UserName
                        }).GroupBy(g => g.Dates).Select(s => s.FirstOrDefault()).OrderByDescending(m => m.EntryDateTime).ToList();

            if (status == "out")
            {
                list = list.OrderByDescending(m => m.EntryDateTime).Distinct().ToList();
            }
            else if (status == "in")
            {
                list = list.OrderByDescending(m => m.EntryDateTime).ToList();
            }
            return PartialView(list);

            //if (status == "out")
            //{
            //    var list = db.View_AllAttendance.Where(m => m.UserId == UserId && m.CardInsertStatus == "OUT").OrderByDescending(m => m.EntryDateTime).ToList();
            //    return PartialView(list);
            //}
            //else
            //{
            //    var list = db.View_AllAttendance.Where(m => m.UserId == UserId && m.CardInsertStatus == "IN").OrderByDescending(m => m.EntryDateTime).ToList();
            //    return PartialView(list);
            //}
        }
        public PartialViewResult _InputSingleUserAtten(long UserId)
        {
            Attendance model = new Attendance();
            model.UserId = UserId;
            return PartialView(model);
        }
        public JsonResult SingleUserAttendanceSave(Attendance model)
        {
            if (ModelState.IsValid)
            {
                string msg = "";
                int ColumnId = 0;
                int OperationStatus = -1;
                string username = db.View_UserLists.Where(x => x.UserId == model.UserId).Select(x => x.UserName).FirstOrDefault();
                db.Attendances.Add(model);
                try
                {
                    model.InsertedDate = now;
                    db.SaveChanges();
                    OperationStatus = 1;
                    ColumnId = db.Attendances.Where(m => m.UserId == model.UserId && m.InsertedBy == model.InsertedBy).Max(m => m.Id);
                    msg = "Emplyee(" + username + ") attendance has been successfully saved on " + now.ToString("MMM dd , yyy hh:mm tt");
                }
                catch (Exception)
                {
                    OperationStatus = -1;
                    msg = "Emplyee(" + username + " ) attendance save was unsuccessful on " + now.ToString("MMM dd , yyy hh:mm tt");
                    throw;
                }
                SaveAuditLog("Manually Insert User Attendance", msg, "HR", "DisplayEmployeeDetails", "Attendance", ColumnId, model.InsertedBy, now, OperationStatus);
                if (OperationStatus == 1)
                {
                    return Json("Success", JsonRequestBehavior.AllowGet);
                }
            }
            return Json("Error", JsonRequestBehavior.AllowGet);
        }


        //******************* User Holiday Package ****************************************
        #region
        public PartialViewResult _UserHolidayTab(long? UserId, int UserType, int? holidayPack)
        {
            ViewBag.UserId = UserId;
            ViewBag.UserType = UserType;
            ViewBag.holidayPack = holidayPack;
            return PartialView();
        }
        public PartialViewResult _EmpLeaveRecordList(long UserId)
        {
            var list = db.EmployeeLeaveRecords.Where(m => m.UserId == UserId).ToList();
            return PartialView(list);
        }
        #endregion

        //*************************** SHOW SALARY PACKAGE DETAILS *******************************************************
        public ActionResult ShowSalaryPackageDetails(int? PackageId, string option)
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
                ViewBag.option = option;
                return PartialView(model);
            }
        }
        //*************************** SHOW SALARY CURRENT MONTH DETAILS *************************************************
        public ActionResult ShowCurrentMonthSalaryDetails(long? UserId, int UserType, int? PackageId)
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
        //*************************** SHOW SALARY USER DUE SALARY *******************************************************
        public ActionResult ShowUserDueSalary(long? UserId, int UserType, int? PackageId)
        {
            if (PackageId == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            else
            {
                var list = db.EmpSalaryPaymentDetails.Where(m => m.UserId == UserId && m.UserType == UserType && m.Status == 0).ToList();
                return PartialView(list);
            }
        }

        //*************************** SHOW USER ALL PAID SALARY HISTORY *******************************************************
        public ActionResult ShowPaidSalaryHistory(long? UserId, int UserType, int? PackageId)
        {
            if (PackageId == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            else
            {
                var list = db.AllSalaryPayLists.Where(m => m.UserId == UserId && m.UserType == UserType).ToList();
                ViewBag.total_paid_amnt = db.AllSalaryPayLists.Where(m => m.UserId == UserId && m.UserType == UserType).Select(m => m.Amount).Sum();
                return PartialView(list);
            }
        }

        //*************************** SHOW ATTENDACE *******************************************************
        public ActionResult ShowAllAttendance()
        {
            return View();
        }
        public ActionResult _ShowAllAttendance(string status)
        {
            if (status == "out")
            {
                var allAttendance = db.View_AllAttendance.Where(m => m.CardInsertStatus == "OUT").OrderByDescending(m => m.EntryDateTime).ToList();
                ViewBag.TotalAttendance = allAttendance.Count();
                Session["AllAttendance"] = allAttendance;
                Session["TotalAttendanceCount"] = allAttendance.Count();
                var list = allAttendance.Take(ShowUserPerPage).ToList();
                return PartialView(list);
            }
            else
            {
                var allAttendance = db.View_AllAttendance.Where(m => m.CardInsertStatus == "IN").OrderByDescending(m => m.EntryDateTime).ToList();
                ViewBag.TotalAttendance = allAttendance.Count();
                Session["AllAttendance"] = allAttendance;
                Session["TotalAttendanceCount"] = allAttendance.Count();
                var list = allAttendance.Take(ShowUserPerPage).ToList();
                return PartialView(list);
            }
        }
        public JsonResult GetAllAttendance(int? page)
        {
            page = page ?? 0;
            int total = Convert.ToInt16(Session["TotalAttendanceCount"]);
            int skip = ShowUserPerPage * Convert.ToInt16(page);
            var canPage = skip < total;
            if (canPage)
            {
                var listBackFromSession = (List<View_AllAttendance>)Session["AllAttendance"];
                var allAttendanceList = listBackFromSession.Skip(skip).Take(ShowUserPerPage).ToList();
                return Json(allAttendanceList, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json("", JsonRequestBehavior.AllowGet);
            }

        }
        public PartialViewResult _UserImageChange(long? userId)
        {
            View_UserLists model = db.View_UserLists.Where(m => m.UserId == userId).FirstOrDefault();
            return PartialView(model);
        }
        #endregion


        //************************************************************************************************************
        //*****************************************SALARY PAYMEMT:START **********************************************
        //************************************************************************************************************
        #region Salary Pay
        public ActionResult PaySalary()
        {
            var list = new SelectList(db.View_UserLists.Where(m => m.Status == 1 && m.AssginSalary == true).Select(x => new { x.SiteId, x.SiteName }).Distinct(), "SiteId", "SiteName");
            ViewBag.AllSiteList = list;
            return View();
        }
        public ActionResult _UserListForSalary(int? siteId, int? userId)
        {
            if (siteId == null && userId == null)
            {
                var user = (from m in db.View_UserLists
                            where m.AssginSalary == true && m.Status == 1
                            select new UserInformationModelView
                            {
                                UserId = m.UserId,
                                UserName = m.UserName,
                                UserTypeName = m.UserTypeName,
                                UserType = m.UserType,
                                SiteIdName = m.SiteName,
                                EmpId = m.EmpId,
                                DesignationId = m.DesignationId,
                                Designation = m.DesignationName,
                                JoinDate = m.JoinDate,
                                JoinDateName = DbFunctions.TruncateTime(m.JoinDate).ToString(),
                                Picture = m.Picture,
                                AssginSalary = m.AssginSalary,
                                SalaryPackageId = m.SalaryPackageId
                            }).OrderBy(o => o.UserName).ToList();
                Session["AllUserList"] = user;
                Session["AllUserCount"] = user.Count();
                var usrList = user.Take(ShowUserPerPage).ToList();
                ViewBag.count = user.Count();
                //return PartialView(usrList);
                return PartialView(user);
            }
            else
            {
                if (siteId != null)
                {
                    var user = (from m in db.View_UserLists
                                where m.AssginSalary == true && m.Status == 1 && m.SiteId == siteId
                                select new UserInformationModelView
                                {
                                    UserId = m.UserId,
                                    UserName = m.UserName,
                                    UserTypeName = m.UserTypeName,
                                    UserType = m.UserType,
                                    SiteIdName = m.SiteName,
                                    EmpId = m.EmpId,
                                    DesignationId = m.DesignationId,
                                    Designation = m.DesignationName,
                                    JoinDate = m.JoinDate,
                                    JoinDateName = DbFunctions.TruncateTime(m.JoinDate).ToString(),
                                    Picture = m.Picture,
                                    AssginSalary = m.AssginSalary,
                                    SalaryPackageId = m.SalaryPackageId
                                }).OrderBy(o => o.UserName).ToList();
                    Session["AllUserList"] = user;
                    Session["AllUserCount"] = user.Count();
                    var usrList = user.Take(ShowUserPerPage).ToList();
                    ViewBag.count = user.Count();
                    //return PartialView(usrList);
                    return PartialView(user);
                }
                else
                {
                    var user = (from m in db.View_UserLists
                                where m.AssginSalary == true && m.Status == 1 && m.UserId == userId
                                select new UserInformationModelView
                                {
                                    UserId = m.UserId,
                                    UserName = m.UserName,
                                    UserTypeName = m.UserTypeName,
                                    UserType = m.UserType,
                                    SiteIdName = m.SiteName,
                                    EmpId = m.EmpId,
                                    DesignationId = m.DesignationId,
                                    Designation = m.DesignationName,
                                    JoinDate = m.JoinDate,
                                    JoinDateName = DbFunctions.TruncateTime(m.JoinDate).ToString(),
                                    Picture = m.Picture,
                                    AssginSalary = m.AssginSalary,
                                    SalaryPackageId = m.SalaryPackageId
                                }).OrderBy(o => o.UserName).ToList();
                    Session["AllUserList"] = user;
                    Session["AllUserCount"] = user.Count();
                    var usrList = user.Take(ShowUserPerPage).ToList();
                    ViewBag.count = user.Count();
                    //return PartialView(usrList);
                    return PartialView(user);
                }
            }
        }
        public JsonResult GetUsrForSlary(int? page)
        {
            page = page ?? 0;
            int total = Convert.ToInt16(Session["AllUserCount"]);
            int skip = ShowUserPerPage * Convert.ToInt16(page);
            var canPage = skip < total;
            if (canPage)
            {
                var listBackFromSession = (List<UserInformationModelView>)Session["AllUserList"];
                var allUsrList = listBackFromSession.Skip(skip).Take(ShowUserPerPage).ToList();
                return Json(allUsrList, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json("", JsonRequestBehavior.AllowGet);
            }

        }
        public JsonResult SearchUserForSalary(string prefix)
        {
            if (prefix == "")
            {
                return Json("", JsonRequestBehavior.AllowGet);
            }
            else
            {
                var allUser = (List<UserInformationModelView>)Session["AllUserList"];
                var user = (from m in db.View_UserLists
                            where m.AssginSalary == true && m.Status == 1 &&
                            (m.UserName.ToLower().StartsWith(prefix.ToLower()) || m.SiteName.ToLower().Contains(prefix.ToLower()) || m.EmpId.StartsWith(prefix.ToLower()))
                            select new UserInformationModelView
                            {
                                UserId = m.UserId,
                                UserName = m.UserName,
                                UserTypeName = m.UserTypeName,
                                UserType = m.UserType,
                                SiteIdName = m.SiteName,
                                EmpId = m.EmpId,
                                DesignationId = m.DesignationId,
                                Designation = m.DesignationName,
                                JoinDate = m.JoinDate,
                                JoinDateName = DbFunctions.TruncateTime(m.JoinDate).ToString(),
                                Picture = m.Picture,
                                AssginSalary = m.AssginSalary,
                                SalaryPackageId = m.SalaryPackageId
                            }).OrderBy(o => o.UserName).ToList();

                Session["AllUserList"] = user;
                Session["AllUserCount"] = user.Count();
                return Json(user, JsonRequestBehavior.AllowGet);
            }
        }


        public PartialViewResult _SalaryPayOptionMain(int? userId, int? userType, int? packId, string payType)
        {
            ViewBag.userId = userId;
            ViewBag.userType = userType;
            ViewBag.packId = packId;
            ViewBag.PayType = payType;
            return PartialView();
        }
        //****************************  Current Salary Pay Option ***********************************************
        public PartialViewResult SalaryPayOption(int? userId)
        {
            EmpSalaryPaymentDetailModelView model = (from m in db.EmpSalaryPaymentDetails
                                                     join v in db.View_UserLists on m.UserId equals v.UserId
                                                     where m.UserId == userId
                                                     && (m.MonthId == ((now.Month) - 1) && m.Year == now.Year) &&
                                                     m.Status == 0
                                                     select new EmpSalaryPaymentDetailModelView
                                                     {
                                                         Id = m.Id,
                                                         TotalAmount = m.TotalAmount,
                                                         UserId = m.UserId,
                                                         UserType = m.UserType,
                                                         UserName = v.UserName,
                                                         MonthId= m.MonthId,
                                                         Month = m.Month,
                                                         Year = m.Year,
                                                         UserPic = v.PictureName,
                                                         PaidAmount = m.PaidAmount,
                                                         HolidayPckId = v.HolidayPackId.Value,
                                                         WorkingScheduleId = v.WorkingScheduleId.Value
                                                     }).FirstOrDefault();
            int siteId = db.View_UserLists.Where(m => m.UserId == userId).Select(m => m.SiteId).FirstOrDefault();
            ViewBag.siteId = siteId;
            return PartialView(model);
        }
        public PartialViewResult ShowUserCurrentSalary(int? userId, int? userType)
        {
            var model = (from m in db.EmpSalaryPaymentDetails
                         where m.UserId == userId && m.UserType == userType &&
                         (m.MonthId == now.Month || m.MonthId == ((now.Month) - 1)) &&
                         m.Status == 0
                         select new EmpSalaryPaymentDetailModelView
                         {
                             Month = m.Month,
                             TotalAmount = m.TotalAmount,
                             PaidAmount = m.PaidAmount,
                             UserId = m.UserId,
                             UserType = m.UserType
                         }).FirstOrDefault();
            return PartialView(model);
        }
        //************************* Save Single User Salary Pay ********************
        public JsonResult SaveSingleUserSalary(AllSalaryPayList model, string username, string currency, bool loanDeduct, bool isAdjust, decimal loan_amnt, int adjusted_leave, decimal salary_amnt)
        {
            if (ModelState.IsValid)
            {
                string opname = "Monthly Salary Pay";
                string msg = "";
                int ColumnId = 0;
                int OperationStatus = -1;

                bool SaveSuccussfully = false;
                bool fullPayment = false;
                AllSalaryPayList salary = new AllSalaryPayList();
                salary.EmpSalaryPayId = model.EmpSalaryPayId;
                salary.UserId = model.UserId;
                salary.UserType = model.UserType;
                salary.PayType = model.PayType;
                salary.Amount = model.Amount;
                salary.InernalAccId = model.InernalAccId;
                salary.BankId = model.BankId;
                salary.BankAccId = model.BankAccId;
                salary.BankAccSlipNo = model.BankAccSlipNo;
                salary.InsertBy = model.InsertBy;
                salary.InsertedDate = now;
                salary.IsRevert = false;
                salary.PresentDay = model.PresentDay;
                salary.AbsentDay = model.AbsentDay;
                salary.TtlAmntPresentDays = model.TtlAmntPresentDays;
                db.AllSalaryPayLists.Add(salary);
                try
                {
                    db.SaveChanges();
                    SaveSuccussfully = true;
                    OperationStatus = 1;
                    ColumnId = (int)db.AllSalaryPayLists.Where(m => m.InsertBy == model.InsertBy).Max(m => m.Id);

                    //********************************* Amount Decrease From Acccount ******************************
                    decimal reduce_amnt = 0;
                    if (salary_amnt > loan_amnt)
                    {
                        reduce_amnt = salary_amnt;
                    }
                    else
                    {
                        reduce_amnt = loan_amnt;
                    }
                    if (model.PayType == 1)
                    {
                        AccountName internal_account = db.AccountNames.Find(model.InernalAccId);
                        internal_account.Balance = Convert.ToDecimal(internal_account.Balance) - reduce_amnt;
                        internal_account.UpdatedBy = model.InsertBy;
                        internal_account.UpdatedDate = now;
                        db.Entry(internal_account).State = EntityState.Modified;
                        db.SaveChanges();
                        msg = "Amount " + currency + "" + reduce_amnt + " has been deducted from account on " + now.ToString("MMM dd , yyy hh:mm tt");
                        SaveAuditLog("Account Balance Decrease", msg, "HR", "PaySalary", "AccountName", Convert.ToInt16(model.InernalAccId), model.InsertBy, now, 1);
                    }
                    else
                    {
                        BankAccountList bank_acc = db.BankAccountLists.Find(model.BankAccId);
                        bank_acc.Balance = Convert.ToDecimal(bank_acc.Balance) - reduce_amnt;
                        bank_acc.UpdatedBy = model.InsertBy;
                        bank_acc.UpdatedDate = now;
                        db.Entry(bank_acc).State = EntityState.Modified;
                        db.SaveChanges();
                        msg = "Amount " + currency + "" + reduce_amnt + " has been deducted from account.";
                        SaveAuditLog("Account Balance Decrease", msg, "HR", "PaySalary", "BankAccountList", Convert.ToInt16(model.BankAccId), model.InsertBy, now, 1);
                    }


                    //***************** If Adjusted Paid Leave Keep Paid Leave Record ******************************
                    if (isAdjust && adjusted_leave > 0)
                    {
                        var CurrentMonthPresentList = db.Daily_Attendance.Where(m => m.UserId == model.UserId && m.Year == now.Year && m.MonthId == now.Month).ToList();
                        var count = db.Daily_Attendance.Where(m => m.UserId == model.UserId && m.Year == now.Year && m.MonthId == now.Month).Count();
                        var startDate = new DateTime(now.Year, now.Month, 1);
                        var endDate = now;
                        List<EmployeeLeaveRecord> total_Absent_days = new List<EmployeeLeaveRecord>();


                        List<string> dayslist_for_week = new List<string>();
                        if (db.UserInformations.Where(m => m.HolidayPackId > 0 && m.UserId == model.UserId).Any())
                        {
                            int HolypackId = Convert.ToInt32(db.UserInformations.Where(m => m.HolidayPackId > 0 && m.UserId == model.UserId).Select(m => m.HolidayPackId).FirstOrDefault());
                            //int week_days = db.WorkingDayLists.Where(m => m.HolidayPckId == HolypackId).Count();
                            //dayslist_for_week = db.WorkingDayLists.Where(m => m.HolidayPckId == HolypackId).Select(m => m.Day).ToList();

                            for (DateTime date = startDate; date <= endDate; date = date.AddDays(1))
                            {
                                if (dayslist_for_week.Contains(date.DayOfWeek.ToString()))
                                {
                                    if (!CurrentMonthPresentList.Where(m => m.AttDate.ToShortDateString() == date.ToShortDateString()).Any())
                                    {
                                        total_Absent_days.Add(new EmployeeLeaveRecord { Date = date, Year = date.Year, MonthName = date.ToString("MMMM"), MonthId = date.Month });
                                    }
                                }
                            }
                        }

                        for (int i = 0; i < adjusted_leave; i++)
                        {
                            EmployeeLeaveRecord leave = new EmployeeLeaveRecord();
                            leave.UserId = model.UserId;
                            leave.Date = total_Absent_days[i].Date;
                            leave.Year = total_Absent_days[i].Year;
                            leave.MonthName = total_Absent_days[i].MonthName;
                            leave.MonthId = total_Absent_days[i].MonthId;
                            db.EmployeeLeaveRecords.Add(leave);
                            db.SaveChanges();
                        }

                    }
                    //************************** Loan Amount Deduct From Salary *****************************

                    if (loan_amnt > 0)
                    {
                        AllUserLoanList loan = db.AllUserLoanLists.Where(m => m.UserId == model.UserId && m.PaidStatus == 0 && m.DeductionFromSalary == true).FirstOrDefault();
                        loan.PaidAmount = Convert.ToDecimal(loan.PaidAmount) + Convert.ToDecimal(loan_amnt);
                        loan.LoanReceivedBy = model.InsertBy;
                        loan.LoanReceivedDate = now;
                        db.Entry(loan).State = EntityState.Modified;
                        db.SaveChanges();
                        string acc_name = "";
                        if (loan.PayType == 1)
                        {
                            AccountName internal_account = db.AccountNames.Find(loan.InernalAccId);
                            internal_account.Balance = Convert.ToDecimal(internal_account.Balance) + loan_amnt;
                            internal_account.UpdatedBy = model.InsertBy;
                            internal_account.UpdatedDate = now;
                            db.Entry(internal_account).State = EntityState.Modified;
                            db.SaveChanges();
                            msg = "Amount " + currency + "" + loan_amnt + " has been added into account '" + internal_account.AccountName1 + "' on " + now + " .";
                            SaveAuditLog("Account Balance Increase", msg, "HR", "PaySalary", "AccountName", Convert.ToInt16(model.InernalAccId), model.InsertBy, now, 1);
                            acc_name = internal_account.AccountName1;
                        }
                        else
                        {
                            BankAccountList bank_acc = db.BankAccountLists.Find(loan.BankAccId);
                            bank_acc.Balance = Convert.ToDecimal(bank_acc.Balance) + loan_amnt;
                            bank_acc.UpdatedBy = model.InsertBy;
                            bank_acc.UpdatedDate = now;
                            db.Entry(bank_acc).State = EntityState.Modified;
                            db.SaveChanges();
                            msg = "Amount " + currency + "" + loan_amnt + " has been added into account '" + bank_acc.AccountName + "' on " + now + " .";
                            SaveAuditLog("Account Balance Increase", msg, "HR", "PaySalary", "BankAccountList", Convert.ToInt16(model.BankAccId), model.InsertBy, now, 1);
                            acc_name = bank_acc.AccountName;
                        }

                        msg = "Loan amount " + currency + "" + loan_amnt + " has been returned by '" + username + "' and amount has been added into  account '" + acc_name + "' on " + now + " .";
                        SaveAuditLog("Loan Amount Return", msg, "HR", "PaySalary", "AllUserLoanList", Convert.ToInt16(model.BankAccId), model.InsertBy, now, 1);
                    }

                    long maxtranId = 1;
                    if (db.Transactions.Any())
                    {
                        maxtranId = db.Transactions.Max(m => m.TranId) + 1;
                    }
                    Transaction tran = new Transaction();
                    tran.TranName = "Transaction_" + maxtranId;
                    tran.TranTypeId = 1;
                    tran.Amount = Convert.ToDecimal(model.Amount);
                    tran.UserId = model.UserId;
                    tran.InsertedBy = model.InsertBy;
                    tran.InsertedDate = now;
                    tran.RefTableId = ColumnId;
                    db.Transactions.Add(tran);
                    db.SaveChanges();

                    msg = username + "'s monthly salary " + model.Amount + currency + " paid successfully.";

                    EmpSalaryPaymentDetail empSalary = db.EmpSalaryPaymentDetails.Find(model.EmpSalaryPayId);
                    empSalary.PaidAmount = Convert.ToDecimal(empSalary.PaidAmount) + Convert.ToDecimal(model.Amount);
                    if (salary_amnt > loan_amnt)
                    {
                        if (empSalary.PaidAmount == salary_amnt)
                        {
                            empSalary.Status = 1;
                            fullPayment = true;
                        }
                    }
                    else
                    {
                        empSalary.Status = 1;
                        fullPayment = true;
                    }
                    empSalary.PaidDate = now;
                    empSalary.PaidBy = model.InsertBy;
                    if (loan_amnt > 0)
                    {
                        empSalary.DeductedLoanId = db.AllUserLoanLists.Where(m => m.UserId == model.UserId && m.PaidStatus == 0 && m.DeductionFromSalary == true).Select(m => m.LoanId).FirstOrDefault();
                        empSalary.DeductedLoanAmnt = loan_amnt;
                    }
                    db.Entry(empSalary).State = EntityState.Modified;
                    db.SaveChanges();
                }
                catch (Exception)
                {
                    SaveSuccussfully = false;
                    OperationStatus = -1;
                    msg = username + "'s monthly salary " + model.Amount + currency + " paid unsuccessful.";
                }
                SaveAuditLog(opname, msg, "HR", "PaySalary", "AllSalaryPayList", ColumnId, model.InsertBy, now, OperationStatus);
                if (SaveSuccussfully)
                {
                    return Json(fullPayment, JsonRequestBehavior.AllowGet);
                }
            }
            return Json("Error", JsonRequestBehavior.AllowGet);
        }

        //******************************** Due Salary Pay Option ****************************************************
        public ActionResult SalaryDuePayOption(int? userId, int? userType)
        {
            IEnumerable<EmpSalaryPaymentDetailModelView> model = (from m in db.EmpSalaryPaymentDetails
                                                                  where m.UserId == userId && m.UserType == m.UserType
                                                                  && (m.MonthId < now.Month) &&
                                                                  m.Status == 0
                                                                  select new EmpSalaryPaymentDetailModelView
                                                                  {
                                                                      Id = m.Id,
                                                                      Month = m.Month,
                                                                      MonthId = m.MonthId,
                                                                      Year = m.Year,
                                                                      TotalAmount = m.TotalAmount,
                                                                      UserId = m.UserId,
                                                                      UserType = m.UserType,
                                                                      UserName = db.View_UserLists.Where(v => v.UserId == userId && v.UserType == userType).Select(v => v.UserName).FirstOrDefault(),
                                                                      UserPic = db.View_UserLists.Where(v => v.UserId == userId && v.UserType == userType).Select(v => v.Picture).FirstOrDefault(),
                                                                      PaidAmount = m.PaidAmount
                                                                  }).ToList();
            int siteId = db.View_UserLists.Where(m => m.UserId == userId && m.UserType == userType).Select(m => m.SiteId).FirstOrDefault();
            ViewBag.siteId = siteId;
            return PartialView(model);
        }
        public ActionResult ShowUserDueDetails(int? userId, int? userType)
        {
            var model = db.View_UserLists.Where(m => m.UserId == userId && m.UserType == userType).FirstOrDefault();
            return PartialView(model);
        }
        //************************* Save Single User Salary Due Pay **************************************************
        public ActionResult PaySingleUserDue(AllSalaryPayList model, string[] AllId, string username, string currency)
        {
            if (ModelState.IsValid)
            {
                bool SaveSuccussfully = false;
                string opname = "Due Payment";
                string msg = "";
                int OperationStatus = -1;

                decimal amount = Convert.ToDecimal(model.Amount);
                AllSalaryPayList salary = new AllSalaryPayList();
                salary.EmpSalaryPayId = 0;
                salary.UserId = model.UserId;
                salary.UserType = model.UserType;
                salary.PayType = model.PayType;
                salary.Amount = model.Amount;
                salary.InernalAccId = model.InernalAccId;
                salary.BankId = model.BankId;
                salary.BankAccId = model.BankAccId;
                salary.BankAccSlipNo = model.BankAccSlipNo;
                salary.InsertBy = model.InsertBy;
                salary.InsertedDate = now;
                salary.IsRevert = false;
                db.AllSalaryPayLists.Add(salary);
                try
                {
                    db.SaveChanges();
                    SaveSuccussfully = true;
                    OperationStatus = 1;
                    msg = username + "'s  due " + model.Amount + currency + " paid.";
                    int ColumnId = (int)db.AllSalaryPayLists.Where(m => m.InsertBy == model.InsertBy).Max(m => m.Id);


                    long maxtranId = 1;
                    if (db.Transactions.Any())
                    {
                        maxtranId = db.Transactions.Max(m => m.TranId) + 1;
                    }
                    Transaction tran = new Transaction();
                    tran.TranName = "Transaction_" + maxtranId;
                    tran.TranTypeId = 2;
                    tran.Amount = Convert.ToDecimal(model.Amount);
                    tran.UserId = model.UserId;
                    tran.InsertedBy = model.InsertBy;
                    tran.InsertedDate = now;
                    tran.RefTableId = ColumnId;
                    db.Transactions.Add(tran);
                    db.SaveChanges();
                }
                catch (Exception)
                {
                    SaveSuccussfully = false;
                    msg = username + "'s  due " + model.Amount + currency + " paid unsuccessful.";
                    throw;
                }
                long max_SalaryPayId = db.AllSalaryPayLists.Where(m => m.InsertBy == model.InsertBy).Max(m => m.Id);
                decimal inserted_amnt = 0;
                for (int i = 0; i < AllId.Length; i++)
                {
                    if (amount > 0)
                    {
                        long id = Convert.ToInt64(AllId[i]);
                        decimal remainamnt = 0;
                        inserted_amnt = 0;
                        EmpSalaryPaymentDetail payDetail = db.EmpSalaryPaymentDetails.Find(id);
                        remainamnt = Convert.ToDecimal(payDetail.TotalAmount) - Convert.ToDecimal(payDetail.PaidAmount);
                        if (amount > remainamnt)
                        {
                            inserted_amnt = remainamnt;
                            amount = amount - remainamnt;
                            payDetail.PaidAmount = Convert.ToDecimal(payDetail.PaidAmount) + remainamnt;
                        }
                        else
                        {
                            inserted_amnt = amount;
                            payDetail.PaidAmount = Convert.ToDecimal(payDetail.PaidAmount) + amount;
                            amount = 0;
                        }
                        if (payDetail.TotalAmount == payDetail.PaidAmount)
                        {
                            payDetail.Status = 1;
                        }
                        payDetail.PaidDate = now;
                        payDetail.PaidBy = model.InsertBy;
                        db.Entry(payDetail).State = EntityState.Modified;
                        db.SaveChanges();

                        DuePaymentRecord DueRecord = new DuePaymentRecord();
                        DueRecord.PaymentId = payDetail.Id;
                        DueRecord.SalaryPayId = max_SalaryPayId;
                        DueRecord.Amount = inserted_amnt;
                        DueRecord.UserId = model.UserId;
                        DueRecord.UserType = model.UserType;
                        DueRecord.PaidDate = now;
                        DueRecord.PaidBy = model.InsertBy;

                        db.DuePaymentRecords.Add(DueRecord);
                        db.SaveChanges();
                    }
                }

                SaveAuditLog(opname, msg, "HR", "PaySalary", "AllSalaryPayList", (int)max_SalaryPayId, model.InsertBy, now, OperationStatus);
                if (SaveSuccussfully)
                {
                    return Json("Success", JsonRequestBehavior.AllowGet);
                }
            }
            return Json("Error", JsonRequestBehavior.AllowGet);
        }


        public JsonResult AllAccountList(long userId)
        {
            var accId = db.AccNameAssignedWithUsers.Where(m => m.UserId == userId && m.UserType == 1).Select(m => m.AccId).ToList();
            var list = (from m in db.AccountNames
                        where accId.Contains(m.AccId)
                        select new
                        {
                            AccId = m.AccId,
                            AccountName = m.AccountName1,
                            HightestAmnt = m.TransactionHigestAmount
                        }).ToList();
            return Json(list, JsonRequestBehavior.AllowGet);
        }
        public JsonResult AllBankList()
        {
            var list = (from m in db.BankLists
                        where m.Status == 1
                        select new
                        {
                            BankId = m.BankId,
                            BankName = m.BankName
                        }).ToList();
            return Json(list, JsonRequestBehavior.AllowGet);
        }
        public JsonResult AllBankBranch(int id)
        {
            var list = (from m in db.BankBranchLists
                        where m.BankId == id
                        select new
                        {
                            BranchId = m.BranchId,
                            BankId = m.BankId,
                            BranchName = m.BranchName
                        }).ToList();
            return Json(list, JsonRequestBehavior.AllowGet);
        }
        public JsonResult AllBankAcc(int id)
        {
            var list = (from m in db.BankAccountLists
                        where m.BranchId == id
                        select new
                        {
                            BankAccountId = m.BankAccountId,
                            AccountName = m.AccountName,
                            AccountNumber = m.AccountNumber,
                            HightestAmnt = m.HighestAmntPerTansaction
                        }).ToList();
            return Json(list, JsonRequestBehavior.AllowGet);
        }



        //**************************** Pay All User Salary **********************************
        public PartialViewResult _PaySalaryAllUser(string PayType)
        {
            ViewBag.PayType = PayType;
            return PartialView();
        }
        public PartialViewResult _ShowAllUserForSalaryPay(long[] userIds, int[] userType)
        {
            var user = (from m in db.View_UserLists
                        where m.AssginSalary == true && m.Status == 1 && userIds.Contains(m.UserId)
                        select new UserInformationModelView
                        {
                            UserId = m.UserId,
                            UserName = m.UserName,
                            UserTypeName = m.UserTypeName,
                            UserType = m.UserType,
                            Designation = m.DesignationName,
                            SalaryPackageId = m.SalaryPackageId,
                            Picture = m.Picture,
                        }).OrderBy(o => o.UserName).ToList();
            return PartialView(user);
        }
        public PartialViewResult _ShowAllUserForDuePay(long[] userIds, int[] userType)
        {
            var user = (from m in db.View_UserLists
                        where m.AssginSalary == true && m.Status == 1 && userIds.Contains(m.UserId)
                        select new UserInformationModelView
                        {
                            UserId = m.UserId,
                            UserName = m.UserName,
                            UserTypeName = m.UserTypeName,
                            UserType = m.UserType,
                            Designation = m.DesignationName,
                            SalaryPackageId = m.SalaryPackageId,
                            Picture = m.Picture,
                        }).OrderBy(o => o.UserName).ToList();
            return PartialView(user);
        }
        public JsonResult PayAllUserDue(AllSalaryPayList model, string username, string currency)
        {
            if (ModelState.IsValid)
            {
                bool SaveSuccussfully = false;
                string opname = "Due Payment";
                string msg = "";
                int OperationStatus = -1;

                decimal amount = Convert.ToDecimal(model.Amount);
                AllSalaryPayList salary = new AllSalaryPayList();
                salary.EmpSalaryPayId = 0;
                salary.UserId = model.UserId;
                salary.UserType = model.UserType;
                salary.PayType = model.PayType;
                salary.Amount = model.Amount;
                salary.InernalAccId = model.InernalAccId;
                salary.BankId = model.BankId;
                salary.BankAccId = model.BankAccId;
                salary.BankAccSlipNo = model.BankAccSlipNo;
                salary.InsertBy = model.InsertBy;
                salary.InsertedDate = now;
                salary.IsRevert = false;
                db.AllSalaryPayLists.Add(salary);
                try
                {
                    db.SaveChanges();
                    SaveSuccussfully = true;
                    OperationStatus = 1;
                    msg = username + "'s  due " + model.Amount + currency + " paid.";
                    int ColumnId = (int)db.AllSalaryPayLists.Where(m => m.InsertBy == model.InsertBy).Max(m => m.Id);

                    long maxtranId = 1;
                    if (db.Transactions.Any())
                    {
                        maxtranId = db.Transactions.Max(m => m.TranId) + 1;
                    }
                    Transaction tran = new Transaction();
                    tran.TranName = "Transaction_" + maxtranId;
                    tran.TranTypeId = 2;
                    tran.Amount = Convert.ToDecimal(model.Amount);
                    tran.UserId = model.UserId;
                    tran.InsertedBy = model.InsertBy;
                    tran.InsertedDate = now;
                    tran.RefTableId = ColumnId;
                    db.Transactions.Add(tran);
                    db.SaveChanges();
                }
                catch (Exception ex)
                {
                    string errmsg = ex.ToString();
                    SaveSuccussfully = false;
                    msg = username + "'s  due " + model.Amount + currency + " paid unsuccessful.";
                }
                long max_SalaryPayId = db.AllSalaryPayLists.Where(m => m.InsertBy == model.InsertBy).Max(m => m.Id);
                decimal inserted_amnt = 0;
                var allDueList = db.EmpSalaryPaymentDetails.Where(m => m.UserId == model.UserId && m.UserType == model.UserType && m.Status == 0).ToList();
                foreach (var due in allDueList)
                {
                    if (amount > 0 && due != null)
                    {
                        decimal remainamnt = 0;
                        inserted_amnt = 0;
                        remainamnt = Convert.ToDecimal(due.TotalAmount) - Convert.ToDecimal(due.PaidAmount);
                        if (amount > remainamnt)
                        {
                            inserted_amnt = remainamnt;
                            amount = amount - remainamnt;
                            due.PaidAmount = Convert.ToDecimal(due.PaidAmount) + remainamnt;
                        }
                        else
                        {
                            inserted_amnt = amount;
                            due.PaidAmount = Convert.ToDecimal(due.PaidAmount) + amount;
                            amount = 0;
                        }
                        if (due.TotalAmount == due.PaidAmount)
                        {
                            due.Status = 1;
                        }
                        due.PaidDate = now;
                        due.PaidBy = model.InsertBy;
                        db.Entry(due).State = EntityState.Modified;
                        db.SaveChanges();

                        DuePaymentRecord DueRecord = new DuePaymentRecord();
                        DueRecord.PaymentId = due.Id;
                        DueRecord.SalaryPayId = max_SalaryPayId;
                        DueRecord.Amount = inserted_amnt;
                        DueRecord.UserId = model.UserId;
                        DueRecord.UserType = model.UserType;
                        DueRecord.PaidDate = now;
                        DueRecord.PaidBy = model.InsertBy;

                        db.DuePaymentRecords.Add(DueRecord);
                        db.SaveChanges();
                    }
                }
                SaveAuditLog(opname, msg, "HR", "PaySalary", "AllSalaryPayList", (int)max_SalaryPayId, model.InsertBy, now, OperationStatus);
                if (SaveSuccussfully)
                {
                    return Json("Success", JsonRequestBehavior.AllowGet);
                }
            }
            return Json("Error", JsonRequestBehavior.AllowGet);
        }
        #endregion


        //************************************************************************************************************
        //*****************************************SALARY PAYMEMT:END ************************************************
        //************************************************************************************************************


        //************************************************************************************************************
        //*****************************************LOAN / ADVANCE PAYMEMT:START **************************************
        //************************************************************************************************************
        #region Loan/ Advance
        public ActionResult LoanManagement()
        {
            var list = new SelectList(db.View_UserLists.Where(m => m.Status == 1 && m.AssginSalary == true).Select(x => new { x.SiteId, x.SiteName }).Distinct(), "SiteId", "SiteName");
            ViewBag.AllSiteList = list;
            return View();
        }
        public ActionResult _UserListForLoan(int? siteId, int? userId)
        {
            if (siteId == null && userId == null)
            {
                var user = (from m in db.View_UserLists
                            where m.Status == 1
                            select new UserInformationModelView
                            {
                                UserId = m.UserId,
                                UserName = m.UserName,
                                UserTypeName = m.UserTypeName,
                                UserType = m.UserType,
                                SiteIdName = m.SiteName,
                                EmpId = m.EmpId,
                                DesignationId = m.DesignationId,
                                Designation = m.DesignationName,
                                JoinDate = m.JoinDate,
                                JoinDateName = DbFunctions.TruncateTime(m.JoinDate).ToString(),
                                Picture = m.Picture,
                                AssginSalary = m.AssginSalary,
                                SalaryPackageId = m.SalaryPackageId,
                                HasLoanRecord = db.AllUserLoanLists.Where(s => s.UserId == m.UserId).Any(),
                                HasDueLoan = db.AllUserLoanLists.Where(s => s.UserId == m.UserId && s.PaidStatus == 0).Any()
                            }).OrderBy(o => o.UserName).ToList();
                Session["AllUserList"] = user;
                Session["AllUserCount"] = user.Count();
                var usrList = user.Take(ShowUserPerPage).ToList();
                ViewBag.count = user.Count();
                return PartialView(usrList);
            }
            else
            {
                if (siteId != null)
                {
                    var user = (from m in db.View_UserLists
                                where m.Status == 1 && m.SiteId == siteId
                                select new UserInformationModelView
                                {
                                    UserId = m.UserId,
                                    UserName = m.UserName,
                                    UserTypeName = m.UserTypeName,
                                    UserType = m.UserType,
                                    SiteIdName = m.SiteName,
                                    EmpId = m.EmpId,
                                    DesignationId = m.DesignationId,
                                    Designation = m.DesignationName,
                                    JoinDate = m.JoinDate,
                                    JoinDateName = DbFunctions.TruncateTime(m.JoinDate).ToString(),
                                    Picture = m.Picture,
                                    AssginSalary = m.AssginSalary,
                                    SalaryPackageId = m.SalaryPackageId,
                                    HasLoanRecord = db.AllUserLoanLists.Where(s => s.UserId == m.UserId).Any(),
                                    HasDueLoan = db.AllUserLoanLists.Where(s => s.UserId == m.UserId && s.PaidStatus == 0).Any()
                                }).OrderBy(o => o.UserName).ToList();
                    Session["AllUserList"] = user;
                    Session["AllUserCount"] = user.Count();
                    var usrList = user.Take(ShowUserPerPage).ToList();
                    ViewBag.count = user.Count();
                    return PartialView(usrList);
                }
                else
                {
                    var user = (from m in db.View_UserLists
                                where m.Status == 1 && m.UserId == userId
                                select new UserInformationModelView
                                {
                                    UserId = m.UserId,
                                    UserName = m.UserName,
                                    UserTypeName = m.UserTypeName,
                                    UserType = m.UserType,
                                    SiteIdName = m.SiteName,
                                    EmpId = m.EmpId,
                                    DesignationId = m.DesignationId,
                                    Designation = m.DesignationName,
                                    JoinDate = m.JoinDate,
                                    JoinDateName = DbFunctions.TruncateTime(m.JoinDate).ToString(),
                                    Picture = m.Picture,
                                    AssginSalary = m.AssginSalary,
                                    SalaryPackageId = m.SalaryPackageId,
                                    HasLoanRecord = db.AllUserLoanLists.Where(s => s.UserId == m.UserId).Any(),
                                    HasDueLoan = db.AllUserLoanLists.Where(s => s.UserId == m.UserId && s.PaidStatus == 0).Any()
                                }).OrderBy(o => o.UserName).ToList();
                    Session["AllUserList"] = user;
                    Session["AllUserCount"] = user.Count();
                    var usrList = user.Take(ShowUserPerPage).ToList();
                    ViewBag.count = user.Count();
                    return PartialView(usrList);
                }
            }
        }
        public JsonResult GetUsrForLoan(int? page)
        {
            page = page ?? 0;
            int total = Convert.ToInt16(Session["AllUserCount"]);
            int skip = ShowUserPerPage * Convert.ToInt16(page);
            var canPage = skip < total;
            if (canPage)
            {
                var listBackFromSession = (List<UserInformationModelView>)Session["AllUserList"];
                var allUsrList = listBackFromSession.Skip(skip).Take(ShowUserPerPage).ToList();
                return Json(allUsrList, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json("", JsonRequestBehavior.AllowGet);
            }

        }
        public JsonResult SearchUserForLoan(string prefix)
        {
            if (prefix == "")
            {
                return Json("", JsonRequestBehavior.AllowGet);
            }
            else
            {
                var allUser = (List<UserInformationModelView>)Session["AllUserList"];
                var user = (from m in db.View_UserLists
                            where m.AssginSalary == true && m.Status == 1 &&
                            (m.UserName.ToLower().StartsWith(prefix.ToLower()) || m.SiteName.ToLower().Contains(prefix.ToLower()) || m.EmpId.StartsWith(prefix.ToLower()))
                            select new UserInformationModelView
                            {
                                UserId = m.UserId,
                                UserName = m.UserName,
                                UserTypeName = m.UserTypeName,
                                UserType = m.UserType,
                                SiteIdName = m.SiteName,
                                EmpId = m.EmpId,
                                DesignationId = m.DesignationId,
                                Designation = m.DesignationName,
                                JoinDate = m.JoinDate,
                                JoinDateName = DbFunctions.TruncateTime(m.JoinDate).ToString(),
                                Picture = m.Picture,
                                AssginSalary = m.AssginSalary,
                                SalaryPackageId = m.SalaryPackageId,
                                HasLoanRecord = db.AllUserLoanLists.Where(s => s.UserId == m.UserId).Any(),
                                HasDueLoan = db.AllUserLoanLists.Where(s => s.UserId == m.UserId && s.PaidStatus == 0).Any()
                            }).OrderBy(o => o.UserName).ToList();

                Session["AllUserList"] = user;
                Session["AllUserCount"] = user.Count();
                return Json(user, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult _GiveUserLoan(int? userId, int? userType, int? packId, string payType)
        {
            ViewBag.userId = userId;
            ViewBag.userType = userType;
            ViewBag.packId = packId;
            ViewBag.PayType = payType;
            return PartialView();
        }
        public ActionResult LoanGiveOption(int? userId, int? userType)
        {
            return PartialView();
        }
        public ActionResult ShowSlctdUsrDetails(int? userId, int? userType, string payType)
        {
            var user_details = db.View_UserLists.Where(m => m.UserId == userId && m.UserType == userType).FirstOrDefault();
            ViewBag.payType = payType;
            return PartialView(user_details);
        }
        public JsonResult GiveSingleUserLoan(AllUserLoanList model, string username, string currency)
        {
            if (ModelState.IsValid)
            {
                string opname = "Advance Payment";
                string msg = "";
                int ColumnId = 0;
                int OperationStatus = -1;

                AllUserLoanList loan = new AllUserLoanList();
                loan.UserId = model.UserId;
                loan.UserType = model.UserType;
                loan.PayType = model.PayType;
                loan.Amount = model.Amount;
                loan.InernalAccId = model.InernalAccId;
                loan.BankId = model.BankId;
                loan.BankAccId = model.BankAccId;
                loan.BankAccSlipNo = model.BankAccSlipNo;
                loan.LoanPaidBy = model.LoanPaidBy;
                loan.LoanPaidDate = now;
                loan.DeductionFromSalary = model.DeductionFromSalary;
                loan.LoanAmntReturnDeadLine = model.LoanAmntReturnDeadLine;
                loan.DeductAmountPerMonth = (model.DeductionFromSalary == true) ? model.DeductAmountPerMonth : null;
                db.AllUserLoanLists.Add(loan);
                try
                {
                    db.SaveChanges();
                    OperationStatus = 1;
                    ColumnId = (int)db.AllUserLoanLists.Where(m => m.LoanPaidBy == model.LoanPaidBy).Max(m => m.LoanId);
                    msg = model.Amount + " " + currency + " given to " + username + " as loan.";

                    long maxtranId = 1;
                    if (db.Transactions.Any())
                    {
                        maxtranId = db.Transactions.Max(m => m.TranId) + 1;
                    }
                    Transaction tran = new Transaction();
                    tran.TranName = "Transaction_" + maxtranId;
                    tran.TranTypeId = 3;
                    tran.Amount = Convert.ToDecimal(model.Amount);
                    tran.UserId = model.UserId;
                    tran.InsertedBy = model.LoanPaidBy;
                    tran.InsertedDate = now;
                    tran.RefTableId = db.AllUserLoanLists.Where(m => m.LoanPaidBy == model.LoanPaidBy).Max(m => m.LoanId);
                    db.Transactions.Add(tran);
                    db.SaveChanges();

                }
                catch (Exception)
                {
                    OperationStatus = -1;
                    msg = "Loan payment " + model.Amount + " " + currency + " to " + username + " was unsuccessful";
                    throw;
                }
                SaveAuditLog(opname, msg, "HR", "Give Loan", "AllUserLoanList", ColumnId, model.LoanPaidBy, now, OperationStatus);
                if (OperationStatus == 1)
                {
                    return Json("Success", JsonRequestBehavior.AllowGet);
                }
            }
            return Json("Error", JsonRequestBehavior.AllowGet);
        }


        public PartialViewResult _UserLoanDetails(int userId, string view)
        {
            ViewBag.view = view;
            var list = db.AllUserLoanLists.Where(m => m.UserId == userId).ToList();
            return PartialView(list);
        }
        public PartialViewResult _ReceiveLoan(int userId)
        {
            ViewBag.userId = userId;
            var amount = db.AllUserLoanLists.Where(m => m.UserId == userId && m.PaidStatus == 0).Sum(m => m.Amount);
            var allPaidAmnt = db.AllUserLoanLists.Where(m => m.UserId == userId && m.PaidStatus == 0).Sum(m => m.PaidAmount);
            var remain = Convert.ToDecimal(amount) - Convert.ToDecimal(allPaidAmnt);
            ViewBag.remain = remain;
            return PartialView();
        }
        public JsonResult SaveLoanRetrunAmount(AllUserLoanList model, string username, string currency)
        {
            if (model.PaidAmount > 0)
            {
                bool SaveSuccussfully = false;
                string opname = "Loan Money Return";
                string msg = "";
                int OperationStatus = -1;
                long doneBy = Convert.ToInt64(model.LoanReceivedBy);
                decimal amount = Convert.ToDecimal(model.PaidAmount);
                decimal inserted_amnt = 0;
                var AllLoanList = db.AllUserLoanLists.Where(m => m.UserId == model.UserId && m.UserType == model.UserType && m.PaidStatus == 0).ToList();
                foreach (var loan in AllLoanList)
                {
                    if (amount > 0)
                    {
                        decimal remainamnt = 0;
                        inserted_amnt = 0;
                        remainamnt = Convert.ToDecimal(loan.Amount) - Convert.ToDecimal(loan.PaidAmount);
                        if (amount > remainamnt)
                        {
                            inserted_amnt = remainamnt;
                            amount = amount - remainamnt;
                            loan.PaidAmount = Convert.ToDecimal(loan.PaidAmount) + remainamnt;
                        }
                        else
                        {
                            inserted_amnt = amount;
                            loan.PaidAmount = Convert.ToDecimal(loan.PaidAmount) + amount;
                            amount = 0;
                        }
                        if (loan.Amount == loan.PaidAmount)
                        {
                            loan.PaidStatus = 1;
                        }
                        loan.LoanReceivedBy = model.LoanReceivedBy;
                        loan.LoanReceivedDate = now;
                        db.Entry(loan).State = EntityState.Modified;
                        try
                        {
                            db.SaveChanges();
                            SaveSuccussfully = true;
                            OperationStatus = 1;
                            msg = username + " return advance " + model.Amount + " " + currency;

                            long maxtranId = 1;
                            if (db.Transactions.Any())
                            {
                                maxtranId = db.Transactions.Max(m => m.TranId) + 1;
                            }
                            Transaction tran = new Transaction();
                            tran.TranName = "Transaction_" + maxtranId;
                            tran.TranTypeId = 4;
                            tran.Amount = Convert.ToDecimal(model.PaidAmount);
                            tran.UserId = model.UserId;
                            tran.InsertedBy = Convert.ToInt64(model.LoanReceivedBy);
                            tran.InsertedDate = now;
                            tran.RefTableId = loan.LoanId;
                            db.Transactions.Add(tran);
                            db.SaveChanges();
                        }
                        catch (Exception)
                        {
                            OperationStatus = -1;
                            msg = username + " advance amount " + model.Amount + " " + currency + " return was unsuccessful";
                            throw;
                        }
                        if (OperationStatus == 1)
                        {
                            msg = username + " return " + model.PaidAmount + " " + currency;
                            SaveAuditLog(opname, msg, "HR", "Return Loan", "AllUserLoanList", loan.LoanId, doneBy, now, OperationStatus);
                        }
                        else
                        {
                            msg = username + " return " + model.PaidAmount + " " + currency + " save unsuccessful.";
                            SaveAuditLog(opname, msg, "HR", "Return Loan", "AllUserLoanList", loan.LoanId, doneBy, now, OperationStatus);
                        }
                    }
                }
                if (SaveSuccussfully)
                {
                    if (model.PayType == 1)
                    {
                        AccountName account = db.AccountNames.Find(model.InernalAccId);
                        account.Balance = Convert.ToDecimal(account.Balance) + Convert.ToDecimal(model.PaidAmount);
                        account.UpdatedBy = model.LoanReceivedBy;
                        account.UpdatedDate = now;
                        db.Entry(account).State = EntityState.Modified;
                        db.SaveChanges();
                    }
                    else
                    {
                        BankAccountList bank = db.BankAccountLists.Find(model.BankAccId);
                        bank.Balance = Convert.ToDecimal(amount) + Convert.ToDecimal(bank.Balance);
                        bank.UpdatedBy = model.LoanReceivedBy;
                        bank.UpdatedDate = now;
                        db.Entry(bank).State = EntityState.Modified;
                        db.SaveChanges();
                    }

                    return Json("Success", JsonRequestBehavior.AllowGet);
                }
            }
            return Json("Error", JsonRequestBehavior.AllowGet);
        }
        #endregion
        //*************************************************************************************************************
        //*****************************************LOAN / ADVANCE PAYMEMT:END *****************************************
        //*************************************************************************************************************


        //********************************************* All Salary Paid History *****************************************

        #region
        public ActionResult AllPaidHis()
        {
            var list = new SelectList(db.View_UserLists.Where(m => m.Status == 1 && m.AssginSalary == true).Select(x => new { x.SiteId, x.SiteName }).Distinct(), "SiteId", "SiteName");
            ViewBag.AllSiteList = list;
            return View();
        }
        public ActionResult _AllPaidHis(int? siteId, long? UserId, int? days)
        {
            if (days == null)
            {
                if (siteId == null && UserId == null)
                {
                    var user = db.View_All_Salary_Pay_His.Where(m => m.IsRevert == false).OrderByDescending(o => o.InsertedDate).ToList();
                    if (user != null)
                    {
                        Session["SalaryPaidHis"] = user;
                        ViewBag.TotalUser = user.Count();
                        Session["TotalSalaryPaidHis"] = user.Count();
                        ViewBag.UserList = user.Take(ShowUserPerPage).ToList();
                    }
                    else
                    {
                        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                    }
                }
                else
                {
                    if (siteId != null)
                    {
                        var user = db.View_All_Salary_Pay_His.Where(m => m.SiteId == siteId && m.IsRevert == false).OrderByDescending(o => o.InsertedDate).ToList();
                        Session["SalaryPaidHis"] = user;
                        ViewBag.TotalUser = user.Count();
                        Session["TotalSalaryPaidHis"] = user.Count();
                        ViewBag.UserList = user.Take(ShowUserPerPage).ToList();
                    }
                    else
                    {
                        var user = db.View_All_Salary_Pay_His.Where(m => m.UserId == UserId && m.IsRevert == false).OrderByDescending(o => o.InsertedDate).ToList();
                        Session["SalaryPaidHis"] = user;
                        ViewBag.TotalUser = user.Count();
                        Session["TotalSalaryPaidHis"] = user.Count();
                        ViewBag.UserList = user.Take(ShowUserPerPage).ToList();
                    }
                }
            }
            else
            {
                DateTime countDate = DateTime.Today;
                int day = Convert.ToInt32(days);
                if (day > 1)
                {
                    countDate = countDate.AddDays(-(day));
                }
                if (siteId == null && UserId == null)
                {
                    var user = db.View_All_Salary_Pay_His.Where(m => m.InsertedDate > countDate && m.IsRevert == false).OrderByDescending(o => o.InsertedDate).ToList();
                    if (user != null)
                    {
                        Session["SalaryPaidHis"] = user;
                        ViewBag.TotalUser = user.Count();
                        Session["TotalSalaryPaidHis"] = user.Count();
                        ViewBag.UserList = user.Take(ShowUserPerPage).ToList();
                    }
                    else
                    {
                        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                    }
                }
                else
                {
                    if (siteId != null)
                    {
                        var user = db.View_All_Salary_Pay_His.Where(m => m.SiteId == siteId && m.InsertedDate > countDate && m.IsRevert == false).OrderByDescending(o => o.InsertedDate).ToList();
                        Session["SalaryPaidHis"] = user;
                        ViewBag.TotalUser = user.Count();
                        Session["TotalSalaryPaidHis"] = user.Count();
                        ViewBag.UserList = user.Take(ShowUserPerPage).ToList();
                    }
                    else
                    {
                        var user = db.View_All_Salary_Pay_His.Where(m => m.UserId == UserId && m.InsertedDate > countDate && m.IsRevert == false).OrderByDescending(o => o.InsertedDate).ToList();
                        Session["SalaryPaidHis"] = user;
                        ViewBag.TotalUser = user.Count();
                        Session["TotalSalaryPaidHis"] = user.Count();
                        ViewBag.UserList = user.Take(ShowUserPerPage).ToList();
                    }
                }
            }
            return PartialView(ViewBag.UserList);
        }
        public JsonResult GetSalaryPaidHis(int? page)
        {
            page = page ?? 0;
            int total = Convert.ToInt16(Session["TotalSalaryPaidHis"]);
            int skip = ShowPaidHisPage * Convert.ToInt16(page);
            var canPage = skip < total;
            var listBackFromSession = (List<View_All_Salary_Pay_His>)Session["SalaryPaidHis"];
            var userList = listBackFromSession.Skip(skip).Take(ShowPaidHisPage).ToList();
            return Json(userList, JsonRequestBehavior.AllowGet);
        }
        public JsonResult SearchUserPaidHis(string prefix)
        {
            if (prefix == "")
            {
                return Json("", JsonRequestBehavior.AllowGet);
            }
            else
            {
                var allUser = (List<View_All_Salary_Pay_His>)Session["SalaryPaidHis"];
                var list = allUser.Where(m => m.UserName.ToLower().Contains(prefix.ToLower()) || m.DesignationName.ToLower().Contains(prefix.ToLower())).Distinct().ToList();
                Session["AllUsersBySearch"] = list;
                return Json(list, JsonRequestBehavior.AllowGet);
            }
        }
        public PartialViewResult _SalaryTransDetailsMain(int id)
        {
            ViewBag.id = id;
            return PartialView();
        }
        public PartialViewResult _SalaryTranDetails(int id)
        {
            var model = db.View_All_Salary_Pay_His.Where(m => m.Id == id).FirstOrDefault();
            return PartialView(model);
        }
        public PartialViewResult _SalaryRevertTran(int id)
        {
            ViewBag.id = id;
            return PartialView();
        }
        public PartialViewResult _SalaryRevertTranPartail(int id)
        {
            TransactionsEditCount model = new TransactionsEditCount();
            model.RefTableColumnId = id;
            ViewBag.ReasonId = new SelectList(db.TransactionEditReasons, "Id", "Reason", model.ReasonId);
            return PartialView(model);
        }
        public JsonResult RevertSalaryTransaction(TransactionsEditCount model)
        {
            if (model != null)
            {
                int opStatus = -1;
                model.EditedDate = now;
                db.TransactionsEditCounts.Add(model);
                try
                {
                    db.SaveChanges();
                    opStatus = 1;

                    AllSalaryPayList salaryPay = db.AllSalaryPayLists.Find(model.RefTableColumnId);
                    salaryPay.IsRevert = true;
                    db.Entry(salaryPay).State = EntityState.Modified;

                    if (salaryPay.EmpSalaryPayId > 0)
                    {
                        EmpSalaryPaymentDetail empSalary = db.EmpSalaryPaymentDetails.Find(salaryPay.EmpSalaryPayId);
                        empSalary.PaidAmount = empSalary.PaidAmount - salaryPay.Amount;
                        empSalary.Status = 0;
                        db.Entry(empSalary).State = EntityState.Modified;
                    }
                    if (salaryPay.PayType == 1)
                    {
                        AccountName internal_Acc = db.AccountNames.Find(salaryPay.InernalAccId);
                        internal_Acc.Balance = Convert.ToDecimal(internal_Acc.Balance) + Convert.ToDecimal(salaryPay.Amount);
                        db.Entry(internal_Acc).State = EntityState.Modified;
                    }

                    long maxtranId = 1;
                    if (db.Transactions.Any())
                    {
                        maxtranId = db.Transactions.Max(m => m.TranId) + 1;
                    }
                    Transaction tran = new Transaction();
                    tran.TranName = "Transaction_" + maxtranId;
                    tran.TranTypeId = 5;
                    tran.Amount = Convert.ToDecimal(salaryPay.Amount);
                    tran.UserId = salaryPay.UserId;
                    tran.InsertedBy = model.EditedBy;
                    tran.InsertedDate = now;
                    tran.RefTableId = salaryPay.Id;
                    db.Transactions.Add(tran);
                    db.SaveChanges();

                }
                catch (Exception)
                {
                    opStatus = -1;
                    throw;
                }
                if (opStatus == 1)
                {
                    return Json("Success", JsonRequestBehavior.AllowGet);
                }
            }
            return Json("Error", JsonRequestBehavior.AllowGet);
        }

        public PartialViewResult _SalaryTranEdit(int id)
        {
            ViewBag.id = id;
            return PartialView();
        }
        public PartialViewResult _SalaryTranEditPartail(int id)
        {
            AllSalaryPayList salary = db.AllSalaryPayLists.Find(id);
            if (salary.EmpSalaryPayId > 0)
            {
                ViewBag.maxAmount = db.EmpSalaryPaymentDetails.Where(m => m.Id == salary.EmpSalaryPayId).Select(m => m.TotalAmount).FirstOrDefault();
                ViewBag.hasMaxAmount = true;
            }
            else
            {
                ViewBag.hasMaxAmount = false;
            }
            TransactionsEditCount model = new TransactionsEditCount();
            model.RefTableColumnId = id;
            model.PreAmount = salary.Amount;
            model.NewAmount = salary.Amount;
            ViewBag.ReasonId = new SelectList(db.TransactionEditReasons, "Id", "Reason", model.ReasonId);
            return PartialView(model);
        }
        public JsonResult SalaryTransactionEdit(TransactionsEditCount model)
        {
            if (model != null)
            {
                int opStatus = -1;
                model.EditedDate = now;
                db.TransactionsEditCounts.Add(model);
                try
                {
                    db.SaveChanges();
                    opStatus = 1;

                    AllSalaryPayList salaryPay = db.AllSalaryPayLists.Find(model.RefTableColumnId);
                    salaryPay.Amount = model.NewAmount;
                    if (salaryPay.EmpSalaryPayId > 0)
                    {
                        EmpSalaryPaymentDetail empSalary = db.EmpSalaryPaymentDetails.Find(salaryPay.EmpSalaryPayId);
                        empSalary.PaidAmount = (empSalary.PaidAmount - Convert.ToDecimal(model.PreAmount)) + Convert.ToDecimal(model.NewAmount);
                        empSalary.Status = (empSalary.PaidAmount == empSalary.TotalAmount) ? 1 : 0;
                        db.Entry(empSalary).State = EntityState.Modified;
                    }
                    else
                    {
                        //************ Find Due Amount Paid And change that amount
                    }
                    if (salaryPay.PayType == 1)
                    {
                        AccountName internal_Acc = db.AccountNames.Find(salaryPay.InernalAccId);
                        internal_Acc.Balance = (Convert.ToDecimal(internal_Acc.Balance) - Convert.ToDecimal(model.PreAmount)) + Convert.ToDecimal(model.NewAmount);
                        db.Entry(internal_Acc).State = EntityState.Modified;
                    }
                    else
                    {
                        BankAccountList bank_Acc = db.BankAccountLists.Find(salaryPay.BankAccId);
                        bank_Acc.Balance = (Convert.ToDecimal(bank_Acc.Balance) - Convert.ToDecimal(model.PreAmount)) + Convert.ToDecimal(model.NewAmount);
                        db.Entry(bank_Acc).State = EntityState.Modified;
                    }
                    db.Entry(salaryPay).State = EntityState.Modified;
                    db.SaveChanges();
                    long maxtranId = 1;
                    if (db.Transactions.Any())
                    {
                        maxtranId = db.Transactions.Max(m => m.TranId) + 1;
                    }
                    Transaction tran = new Transaction();
                    tran.TranName = "Transaction_" + maxtranId;
                    tran.TranTypeId = 6;
                    tran.Amount = Convert.ToDecimal(model.NewAmount);
                    tran.UserId = salaryPay.UserId;
                    tran.InsertedBy = model.EditedBy;
                    tran.InsertedDate = now;
                    tran.RefTableId = salaryPay.Id;
                    db.Transactions.Add(tran);
                    db.SaveChanges();
                }
                catch (Exception)
                {
                    opStatus = -1;
                }
                if (opStatus == 1)
                {
                    return Json("Success", JsonRequestBehavior.AllowGet);
                }
            }
            return Json("Error", JsonRequestBehavior.AllowGet);
        }
        #endregion


        //********************************************* Employee Card Details  ******************************************
        #region
        public PartialViewResult ShowCardDetails(long UserId)
        {
            var userInfo = db.UserInformations.Find(UserId);
            return PartialView(userInfo);
        }
        public JsonResult ChangeEmpCard(long UserId, string cardNumber)
        {
            if (UserId > 0 && cardNumber != String.Empty)
            {
                UserInformation userInfo = db.UserInformations.Find(UserId);
                if (userInfo != null)
                {
                    userInfo.CardNumber = cardNumber;
                    db.Entry(userInfo).State = EntityState.Modified;
                    db.SaveChanges();
                    return Json("Success", JsonRequestBehavior.AllowGet);
                }
            }
            return Json("Error",JsonRequestBehavior.AllowGet);
        }
        #endregion


        //********************************************* All Transaction Lists ******************************************
        #region
        public ActionResult AllTransactions()
        {
            var list = new SelectList(db.View_UserLists.Where(m => m.Status == 1 && m.AssginSalary == true).Select(x => new { x.SiteId, x.SiteName }).Distinct(), "SiteId", "SiteName");
            ViewBag.AllSiteList = list;
            return View();
        }
        public ActionResult _AllTransactions(int? siteId, long? UserId, int? days)
        {

            if (days == null)
            {
                if (siteId == null && UserId == null)
                {
                    var user = db.View_Transaction.OrderByDescending(o => o.InsertedDate).ToList();
                    if (user != null)
                    {
                        Session["TrasactionsPaidHis"] = user;
                        ViewBag.TotalUser = user.Count();
                        Session["TotalTrasactions"] = user.Count();
                        ViewBag.UserList = user.Take(ShowUserPerPage).ToList();
                    }
                    else
                    {
                        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                    }
                }
                else
                {
                    if (siteId != null)
                    {
                        var user = db.View_Transaction.Where(m => m.SiteId == siteId).OrderByDescending(o => o.InsertedDate).ToList();
                        Session["TrasactionsPaidHis"] = user;
                        ViewBag.TotalUser = user.Count();
                        Session["TotalTrasactions"] = user.Count();
                        ViewBag.UserList = user.Take(ShowUserPerPage).ToList();
                    }
                    else
                    {
                        var user = db.View_Transaction.Where(m => m.UserId == UserId).OrderByDescending(o => o.InsertedDate).ToList();
                        Session["TrasactionsPaidHis"] = user;
                        ViewBag.TotalUser = user.Count();
                        Session["TotalTrasactions"] = user.Count();
                        ViewBag.UserList = user.Take(ShowUserPerPage).ToList();
                    }
                }
            }
            else
            {
                DateTime countDate = DateTime.Today;
                int day = Convert.ToInt32(days);
                if (day > 1)
                {
                    countDate = countDate.AddDays(-(day));
                }
                if (siteId == null && UserId == null)
                {
                    var user = db.View_Transaction.Where(m => m.InsertedDate > countDate).OrderByDescending(o => o.InsertedDate).ToList();
                    if (user != null)
                    {
                        Session["TrasactionsPaidHis"] = user;
                        ViewBag.TotalUser = user.Count();
                        Session["TotalTrasactions"] = user.Count();
                        ViewBag.UserList = user.Take(ShowUserPerPage).ToList();
                    }
                    else
                    {
                        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                    }
                }
                else
                {
                    if (siteId != null)
                    {
                        var user = db.View_Transaction.Where(m => m.SiteId == siteId && m.InsertedDate > countDate).OrderByDescending(o => o.InsertedDate).ToList();
                        Session["TrasactionsPaidHis"] = user;
                        ViewBag.TotalUser = user.Count();
                        Session["TotalTrasactions"] = user.Count();
                        ViewBag.UserList = user.Take(ShowUserPerPage).ToList();
                    }
                    else
                    {
                        var user = db.View_Transaction.Where(m => m.UserId == UserId && m.InsertedDate > countDate).OrderByDescending(o => o.InsertedDate).ToList();
                        Session["TrasactionsPaidHis"] = user;
                        ViewBag.TotalUser = user.Count();
                        Session["TotalTrasactions"] = user.Count();
                        ViewBag.UserList = user.Take(ShowUserPerPage).ToList();
                    }
                }
            }
            return PartialView(ViewBag.UserList);
        }
        public JsonResult GetTransactions(int? page)
        {
            page = page ?? 0;
            int total = Convert.ToInt16(Session["TotalTrasactions"]);
            int skip = ShowTranPage * Convert.ToInt16(page);
            var canPage = skip < total;
            var listBackFromSession = (List<View_Transaction>)Session["TrasactionsPaidHis"];
            var userList = listBackFromSession.Skip(skip).Take(ShowTranPage).ToList();
            return Json(userList, JsonRequestBehavior.AllowGet);
        }
        public JsonResult SearchTransaction(string prefix)
        {
            if (prefix == "")
            {
                return Json("", JsonRequestBehavior.AllowGet);
            }
            else
            {
                var allUser = (List<View_Transaction>)Session["TrasactionsPaidHis"];
                var list = allUser.Where(m => m.UserName.ToLower().Contains(prefix.ToLower()) || m.DesignationName.ToLower().Contains(prefix.ToLower())).Distinct().ToList();
                Session["AllUsersBySearch"] = list;
                return Json(list, JsonRequestBehavior.AllowGet);
            }
        }
        public PartialViewResult _TransDetailsMain(int id)
        {
            ViewBag.id = id;
            return PartialView();
        }
        public PartialViewResult _TranDetails(int id)
        {
            var model = db.View_Transaction.Where(m => m.TranId == id).FirstOrDefault();
            return PartialView(model);
        }
        #endregion


        //********************************************* All Reverted Transaction Lists *********************************
        #region
        public ActionResult AllRevertTran()
        {
            var list = new SelectList(db.View_UserLists.Where(m => m.Status == 1 && m.AssginSalary == true).Select(x => new { x.SiteId, x.SiteName }).Distinct(), "SiteId", "SiteName");
            ViewBag.AllSiteList = list;
            return View();
        }
        public ActionResult _AllRevertTran(int? siteId, long? UserId, int? days)
        {

            if (days == null)
            {
                if (siteId == null && UserId == null)
                {
                    var reverted = db.View_All_Revert_Trans.Where(m => m.IsRevert == true).OrderByDescending(o => o.EditedDate).ToList();
                    if (reverted != null)
                    {
                        Session["RevertedTrans"] = reverted;
                        ViewBag.TotalUser = reverted.Count();
                        Session["TotalRevertedTrans"] = reverted.Count();
                        ViewBag.UserList = reverted.Take(ShowRevertedPerPage).ToList();
                    }
                    else
                    {
                        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                    }
                }
                else
                {
                    if (siteId != null)
                    {
                        var reverted = db.View_All_Revert_Trans.Where(m => m.SiteId == siteId && m.IsRevert == true).OrderByDescending(o => o.EditedDate).ToList();
                        Session["RevertedTrans"] = reverted;
                        ViewBag.TotalUser = reverted.Count();
                        Session["TotalRevertedTrans"] = reverted.Count();
                        ViewBag.UserList = reverted.Take(ShowRevertedPerPage).ToList();
                    }
                    else
                    {
                        var reverted = db.View_All_Revert_Trans.Where(m => m.UserId == UserId && m.IsRevert == true).OrderByDescending(o => o.EditedDate).ToList();
                        Session["RevertedTrans"] = reverted;
                        ViewBag.TotalUser = reverted.Count();
                        Session["TotalRevertedTrans"] = reverted.Count();
                        ViewBag.UserList = reverted.Take(ShowRevertedPerPage).ToList();
                    }
                }
            }
            else
            {
                DateTime countDate = DateTime.Today;
                int day = Convert.ToInt32(days);
                if (day > 1)
                {
                    countDate = countDate.AddDays(-(day));
                }
                if (siteId == null && UserId == null)
                {
                    var reverted = db.View_All_Revert_Trans.Where(m => m.EditedDate > countDate && m.IsRevert == true).OrderByDescending(o => o.EditedDate).ToList();
                    if (reverted != null)
                    {
                        Session["RevertedTrans"] = reverted;
                        ViewBag.TotalUser = reverted.Count();
                        Session["TotalRevertedTrans"] = reverted.Count();
                        ViewBag.UserList = reverted.Take(ShowRevertedPerPage).ToList();
                    }
                    else
                    {
                        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                    }
                }
                else
                {
                    if (siteId != null)
                    {
                        var reverted = db.View_All_Revert_Trans.Where(m => m.SiteId == siteId && m.EditedDate > countDate && m.IsRevert == true).OrderByDescending(o => o.EditedDate).ToList();
                        Session["RevertedTrans"] = reverted;
                        ViewBag.TotalUser = reverted.Count();
                        Session["TotalRevertedTrans"] = reverted.Count();
                        ViewBag.UserList = reverted.Take(ShowRevertedPerPage).ToList();
                    }
                    else
                    {
                        var reverted = db.View_All_Revert_Trans.Where(m => m.UserId == UserId && m.EditedDate > countDate && m.IsRevert == true).OrderByDescending(o => o.EditedDate).ToList();
                        Session["RevertedTrans"] = reverted;
                        ViewBag.TotalUser = reverted.Count();
                        Session["TotalRevertedTrans"] = reverted.Count();
                        ViewBag.UserList = reverted.Take(ShowRevertedPerPage).ToList();
                    }
                }
            }
            return PartialView(ViewBag.UserList);
        }
        public JsonResult GetRevertTran(int? page)
        {
            page = page ?? 0;
            int total = Convert.ToInt16(Session["TotalRevertedTrans"]);
            int skip = ShowRevertedPerPage * Convert.ToInt16(page);
            var canPage = skip < total;
            var listBackFromSession = (List<View_All_Revert_Trans>)Session["RevertedTrans"];
            var userList = listBackFromSession.Skip(skip).Take(ShowRevertedPerPage).ToList();
            return Json(userList, JsonRequestBehavior.AllowGet);
        }
        public JsonResult SearchRevertTran(string prefix)
        {
            if (prefix == "")
            {
                return Json("", JsonRequestBehavior.AllowGet);
            }
            else
            {
                var allUser = (List<View_All_Revert_Trans>)Session["RevertedTrans"];
                var list = allUser.Where(m => m.UserName.ToLower().Contains(prefix.ToLower()) || m.DesignationName.ToLower().Contains(prefix.ToLower())).Distinct().ToList();
                return Json(list, JsonRequestBehavior.AllowGet);
            }
        }
        public PartialViewResult _RevertTransactionPopUp(int? Id)
        {
            var model = db.View_All_Revert_Trans.Where(m => m.Id == Id).FirstOrDefault();
            return PartialView(model);
        }
        #endregion


        //********************************************* All Salary Paid History ***************************************

        #region
        public ActionResult AllLoanHis()
        {
            var list = new SelectList(db.View_UserLists.Where(m => m.Status == 1 && m.AssginSalary == true).Select(x => new { x.SiteId, x.SiteName }).Distinct(), "SiteId", "SiteName");
            ViewBag.AllSiteList = list;
            return View();
        }
        public ActionResult _AllLoanHis(int? siteId, long? UserId, int? days)
        {

            if (days == null)
            {
                if (siteId == null && UserId == null)
                {
                    var user = db.View_All_Loan_History.OrderByDescending(o => o.LoanPaidDate).ToList();
                    if (user != null)
                    {
                        Session["LoanHis"] = user;
                        ViewBag.TotalUser = user.Count();
                        Session["TotalLoanHis"] = user.Count();
                        ViewBag.UserList = user.Take(ShowUserLoanPerPage).ToList();
                    }
                    else
                    {
                        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                    }
                }
                else
                {
                    if (siteId != null)
                    {
                        var user = db.View_All_Loan_History.Where(m => m.SiteId == siteId).OrderByDescending(o => o.LoanPaidDate).ToList();
                        Session["LoanHis"] = user;
                        ViewBag.TotalUser = user.Count();
                        Session["TotalLoanHis"] = user.Count();
                        ViewBag.UserList = user.Take(ShowUserLoanPerPage).ToList();
                    }
                    else
                    {
                        var user = db.View_All_Loan_History.Where(m => m.UserId == UserId).OrderByDescending(o => o.LoanPaidDate).ToList();
                        Session["LoanHis"] = user;
                        ViewBag.TotalUser = user.Count();
                        Session["TotalLoanHis"] = user.Count();
                        ViewBag.UserList = user.Take(ShowUserLoanPerPage).ToList();
                    }
                }
            }
            else
            {
                DateTime countDate = DateTime.Today;
                int day = Convert.ToInt32(days);
                if (day > 1)
                {
                    countDate = countDate.AddDays(-(day));
                }
                if (siteId == null && UserId == null)
                {
                    var user = db.View_All_Loan_History.Where(m => m.LoanPaidDate > countDate).OrderByDescending(o => o.LoanPaidDate).ToList();
                    if (user != null)
                    {
                        Session["LoanHis"] = user;
                        ViewBag.TotalUser = user.Count();
                        Session["TotalLoanHis"] = user.Count();
                        ViewBag.UserList = user.Take(ShowUserLoanPerPage).ToList();
                    }
                    else
                    {
                        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                    }
                }
                else
                {
                    if (siteId != null)
                    {
                        var user = db.View_All_Loan_History.Where(m => m.SiteId == siteId && m.LoanPaidDate > countDate).OrderByDescending(o => o.LoanPaidDate).ToList();
                        Session["LoanHis"] = user;
                        ViewBag.TotalUser = user.Count();
                        Session["TotalLoanHis"] = user.Count();
                        ViewBag.UserList = user.Take(ShowUserLoanPerPage).ToList();
                    }
                    else
                    {
                        var user = db.View_All_Loan_History.Where(m => m.UserId == UserId && m.LoanPaidDate > countDate).OrderByDescending(o => o.LoanPaidDate).ToList();
                        Session["LoanHis"] = user;
                        ViewBag.TotalUser = user.Count();
                        Session["TotalLoanHis"] = user.Count();
                        ViewBag.UserList = user.Take(ShowUserLoanPerPage).ToList();
                    }
                }
            }
            return PartialView(ViewBag.UserList);
        }
        public JsonResult GetLoanHis(int? page)
        {
            page = page ?? 0;
            int total = Convert.ToInt16(Session["TotalLoanHis"]);
            int skip = ShowPaidHisPage * Convert.ToInt16(page);
            var canPage = skip < total;
            var listBackFromSession = (List<View_All_Loan_History>)Session["LoanHis"];
            var userList = listBackFromSession.Skip(skip).Take(ShowUserLoanPerPage).ToList();
            return Json(userList, JsonRequestBehavior.AllowGet);
        }
        public JsonResult SearchLoanHis(string prefix)
        {
            if (prefix == "")
            {
                return Json("", JsonRequestBehavior.AllowGet);
            }
            else
            {
                var allUser = (List<View_All_Salary_Pay_His>)Session["LoanHis"];
                var list = allUser.Where(m => m.UserName.ToLower().Contains(prefix.ToLower()) || m.DesignationName.ToLower().Contains(prefix.ToLower())).Distinct().ToList();
                return Json(list, JsonRequestBehavior.AllowGet);
            }
        }
        public PartialViewResult _LoanTransDetailsMain(int id)
        {
            ViewBag.id = id;
            return PartialView();
        }
        public PartialViewResult _LoanTranDetails(int id)
        {
            var model = db.View_All_Loan_History.Where(m => m.LoanId == id).FirstOrDefault();
            return PartialView(model);
        }
        #endregion

        //************************************************************************************************************
        //**************************************** WORKING SCHEDULE:START ********************************************
        //************************************************************************************************************

        #region
        public ActionResult Working_Schedule()
        {
            return View();
        }
        public PartialViewResult _WorkingSchedule()
        {
            var list = db.WorkingScheduleLists.ToList();
            ViewBag.list = list;
            return PartialView();
        }
        public PartialViewResult _WorkingScheduleCreateUpdate(int? id)
        {
            ViewBag.SelectedSchedule = id;
            return PartialView();
        }
        public PartialViewResult _WorkingScheduleCreate(int? id)
        {
            WorkingScheduleListModelView dataforEdit = new WorkingScheduleListModelView();
            if (id > 0)
            {
                var data = db.WorkingScheduleLists.Find(id);
                dataforEdit.SameWorkHour = data.SameWorkHour;
                dataforEdit.WorkingScheduleId = data.WorkingScheduleId;
                dataforEdit.SchedulName = data.SchedulName;
                dataforEdit.BreakTime = Convert.ToDecimal(data.BreakTime);
                dataforEdit.BreakTimeType = data.BreakTimeType ?? 0;
                dataforEdit.Start_Allowance = data.Start_Allowance;
                dataforEdit.End_Allowance = data.End_Allowance;
            }
            return PartialView(dataforEdit);
        }
        public JsonResult WorkingScheduleSave(WorkingScheduleList workingSchedule, List<WorkingDayList> workingDayList)
        {
            int status = -1;
            string msg = "";
            string opName = "Add New Working Schedule";
            int ColumnId = 0;
            if (ModelState.IsValid)
            {
                workingSchedule.CreatedDate = now;
                if (!workingSchedule.SameWorkHour)
                {
                    workingSchedule.StartTime = null;
                    workingSchedule.EndTime = null;
                    workingSchedule.Start_Allowance = null;
                    workingSchedule.End_Allowance = null;
                }
                db.WorkingScheduleLists.Add(workingSchedule);
                try
                {
                    db.SaveChanges();
                    status = 1;
                    msg = "New working schedule '" + workingSchedule.SchedulName + "' has been succesfully created on " + now.ToString("MMM dd, yyyy hh:mm tt") + " .";
                    ColumnId = db.WorkingScheduleLists.Where(x => x.CreatedBy == workingSchedule.CreatedBy).Max(x => x.WorkingScheduleId);
                    if (workingDayList.Count() > 0)
                    {
                        foreach (var day in workingDayList)
                        {
                            day.CreatedBy = workingSchedule.CreatedBy;
                            day.CreatedDate = now;
                            day.WorkingScheduleId = workingSchedule.WorkingScheduleId;
                            db.WorkingDayLists.Add(day);
                            db.SaveChanges();
                        }
                    }
                }
                catch (Exception)
                {
                    status = -1;
                    msg = "New Working Schedule '" + workingSchedule.SchedulName + "' creation was unsuccesful on " + now.ToString("MMM dd, yyyy hh:mm tt") + " .";
                    ColumnId = 0;
                }
                SaveAuditLog(opName, msg, "HR", "Working_Schedule", "WorkingScheduleList", ColumnId, workingSchedule.CreatedBy, now, status);
                if (status == 1)
                {
                    return Json("Success", JsonRequestBehavior.AllowGet);
                }
            }
            return Json("Error", JsonRequestBehavior.DenyGet);
        }
        public JsonResult WorkingScheduleUpdate(WorkingScheduleListModelView updateSchedule, List<WorkingDayList> workingDayList)
        {
            List<string> workingDays = new List<string>();

            foreach (var data in workingDayList)
            {
                workingDays.Add(data.Day);
            }
            int status = -1;
            string msg = "";
            string opName = "Working Schedule Edit";
            int ColumnId = updateSchedule.WorkingScheduleId;
            WorkingScheduleList dataforUpdate = db.WorkingScheduleLists.Find(updateSchedule.WorkingScheduleId);
            //dataforUpdate.NoOfPaidLeave = updateSchedule.NoOfPaidLeave;
            dataforUpdate.SchedulName = updateSchedule.SchedulName;
            dataforUpdate.SameWorkHour = updateSchedule.SameWorkHour;

            if (updateSchedule.SameWorkHour)
            {
                dataforUpdate.StartTime = updateSchedule.StartTime;
                dataforUpdate.EndTime = updateSchedule.EndTime;
                dataforUpdate.Start_Allowance = updateSchedule.Start_Allowance;
                dataforUpdate.End_Allowance = updateSchedule.End_Allowance;
            }
            else
            {
                dataforUpdate.StartTime = "";
                dataforUpdate.EndTime = "";
                dataforUpdate.Start_Allowance = "";
                dataforUpdate.End_Allowance = "";
            }
            //dataforUpdate.StartTime = updateSchedule.StartTime;
            //dataforUpdate.EndTime = updateSchedule.EndTime;
            dataforUpdate.UpdatedBy = updateSchedule.CreatedBy;
            dataforUpdate.UpdatedDate = now;
            db.Entry(dataforUpdate).State = EntityState.Modified;
            try
            {
                db.SaveChanges();
                status = 1;
                msg = "Edit working schedule '" + updateSchedule.SchedulName + "' information has been successfully updated on " + now.ToString("MMM dd, yyyy hh:mm tt") + " .";
                var AllExsistingDays = db.WorkingDayLists.Where(m => m.WorkingScheduleId == updateSchedule.WorkingScheduleId).ToList();
                var preDelete = AllExsistingDays.Where(m => !workingDays.Contains(m.Day)).ToList();
                foreach (var day in preDelete)
                {
                    db.WorkingDayLists.Remove(day);
                    db.SaveChanges();
                    msg = "Remove day from  working schedule '" + updateSchedule.SchedulName + "' information has been successfully deleted on " + now.ToString("MMM dd, yyyy hh:mm tt") + " .";
                    SaveAuditLog("Remove working Day", msg, "HR", "Working_Schedule", "WorkingDayList", ColumnId, (long)updateSchedule.UpdatedBy, now, status);
                    AllExsistingDays.Remove(day);
                }
                foreach (var day in AllExsistingDays)
                {
                    // var editDay = db.WorkingDayLists.Where(x=>x.WorkingScheduleId==day.WorkingScheduleId && x.Id==day.Id).FirstOrDefault();
                    var updateData = workingDayList.Where(x => x.Day == day.Day).FirstOrDefault();
                    day.StartTime = updateData.StartTime;
                    day.EndTime = updateData.EndTime;
                    day.UpdatedDate = now;
                    day.BreakTime = updateData.BreakTime;
                    day.Break_Type = updateData.Break_Type;
                    day.Start_Allowance = updateData.Start_Allowance;
                    day.End_Allowance = updateData.End_Allowance;
                    day.UpdatedBy = updateSchedule.CreatedBy;
                    db.Entry(day).State = EntityState.Modified;
                    try
                    {
                        db.SaveChanges();
                    }
                    catch (Exception)
                    {
                    }
                }
                foreach (var day in AllExsistingDays)
                {
                    workingDays.Remove(day.Day);
                }
                foreach (var loopDay in workingDays)
                {
                    WorkingDayList day = new WorkingDayList();
                    var updateData = workingDayList.Where(x => x.Day == loopDay).FirstOrDefault();
                    day.Day = loopDay;
                    day.WorkingScheduleId = updateSchedule.WorkingScheduleId;
                    day.UpdatedBy = (long)updateSchedule.UpdatedBy;
                    day.StartTime = updateData.StartTime;
                    day.EndTime = updateData.EndTime;
                    day.Start_Allowance = updateData.Start_Allowance;
                    day.End_Allowance = updateData.End_Allowance;
                    day.BreakTime = updateData.BreakTime;
                    day.Break_Type = updateData.Break_Type;
                    day.CreatedDate = now;

                    db.WorkingDayLists.Add(day);
                    db.SaveChanges();
                    status = 1;
                    msg = "Add day in working schedule '" + updateSchedule.SchedulName + "' information has been successfully added on " + now.ToString("MMM dd, yyyy hh:mm tt") + " .";
                    SaveAuditLog("Add working Day", msg, "HR", "Working_Schedule", "WorkingDayList", ColumnId, (long)updateSchedule.UpdatedBy, now, status);
                }
            }
            catch (Exception ex)
            {
                status = -1;
                msg = "Working schedule '" + updateSchedule.SchedulName + "' update was unsuccessful on " + now.ToString("MMM dd, yyyy hh:mm tt") + " .";
            }
            SaveAuditLog(opName, msg, "HR", "Working_Shedule", "WorkingScheduleList", ColumnId, (long)updateSchedule.UpdatedBy, now, status);
            if (status == 1)
            {
                return Json("Success", JsonRequestBehavior.AllowGet);
            }
            return Json("Error", JsonRequestBehavior.DenyGet);
        }
        public ActionResult _WorkingScheduleTab(string isDisplay, int id)
        {
            ViewBag.WorkingSchedule = id;
            ViewBag.isDisplay = isDisplay;
            return PartialView();
        }
        public ActionResult AssignWorkingSchedule()
        {
            ViewBag.WorkingScheduleId = new SelectList(db.WorkingScheduleLists, "WorkingScheduleId", "SchedulName");
            return PartialView();
        }
        public PartialViewResult SelectedSchedule(int id, string reason, string view)
        {
            var data = db.WorkingScheduleLists.Where(x => x.WorkingScheduleId == id).FirstOrDefault();
            ViewBag.view = view;
            ViewBag.Reason = reason;
            ViewBag.ScheduleId = id;
            return PartialView(data);
        }
        public JsonResult getWorkingDayListData(int id)
        {
            var dayData = db.WorkingDayLists.Where(x => x.WorkingScheduleId == id).ToList();
            return Json(dayData, JsonRequestBehavior.AllowGet);
        }
        public JsonResult IsEligibleForDelete(int? id)
        {
            if (id == null)
            {
                return Json("Error", JsonRequestBehavior.AllowGet);
            }
            else
            {
                if (db.UserInformations.Where(m => m.WorkingScheduleId == id).Any())
                {
                    return Json(db.View_UserLists.Where(m => m.WorkingScheduleId == id).Count(), JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(false, JsonRequestBehavior.AllowGet);
                }
            }
        }
        public JsonResult DeleteWorkingSchedule(WorkingScheduleList deleteSchedule)
        {
            int status = -1;
            string msg = "";
            string opName = "Working Schedule Delete";
            int ColumnId = 0;
            if (!db.UserInformations.Where(x => x.WorkingScheduleId == deleteSchedule.WorkingScheduleId).Any())
            {
                var data = db.WorkingScheduleLists.Where(x => x.WorkingScheduleId == deleteSchedule.WorkingScheduleId).FirstOrDefault();
                db.WorkingScheduleLists.Remove(data);
                try
                {
                    db.SaveChanges();
                    status = 1;
                    msg = "Working Schedule " + data.SchedulName + " has been successfully deleted on " + now.ToString("MMM dd, yyyy hh:mm tt") + " .";
                }
                catch (Exception)
                {
                    status = -1;
                    msg = "Delete Working Schedule was unsuccessful on " + now.ToString("MMM dd, yyyy hh:mm tt") + " .";
                }
                ColumnId = deleteSchedule.WorkingScheduleId;
                SaveAuditLog(opName, msg, "HR", "Working_Schedule", "WorkingScheduleList", ColumnId, deleteSchedule.CreatedBy, now, status);
                var data2 = db.WorkingDayLists.Where(c => c.WorkingScheduleId == deleteSchedule.WorkingScheduleId).ToList();
                try
                {
                    opName = "Working Day Delete";
                    foreach (var info in data2)
                    {
                        db.WorkingDayLists.Remove(info);
                        db.SaveChanges();
                        ColumnId = (int)info.WorkingScheduleId;
                        status = 1;
                        msg = "Working Day " + info.Day + " has been successfully deleted on " + now.ToString("MMM dd, yyyy hh:mm tt") + " .";
                        SaveAuditLog(opName, msg, "HR", "Working_Schedule", "WorkingDayList", ColumnId, deleteSchedule.CreatedBy, now, status);
                    }
                }
                catch (Exception)
                {
                    status = -1;
                    msg = "Delete Working Day was unsuccessful on " + now.ToString("MMM dd, yyyy hh:mm tt") + " .";
                }
                SaveAuditLog(opName, msg, "HR", "Working_Schedule", "WorkingDayList", ColumnId, deleteSchedule.CreatedBy, now, status);
            }

            else
            {
                status = -1;
                msg = "Delete Working Schedule unsuccessful on " + now.ToString("MMM dd, yyyy hh:mm tt") + " .";
                ColumnId = deleteSchedule.WorkingScheduleId;
                SaveAuditLog(opName, msg, "HR", "Working_Schedule", "WorkingScheduleList", ColumnId, deleteSchedule.CreatedBy, now, status);
            }
            if (status == 1)
            {
                return Json("Success", JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json("Error", JsonRequestBehavior.DenyGet);
            }
        }
        public PartialViewResult _AllUserListForSpeficWorkingPack(int PackageId)
        {
            return PartialView(db.View_UserLists.Where(m => m.WorkingScheduleId == PackageId).ToList());
        }
        #endregion

        //************************************************************************************************************
        //**************************************** WORKING SCHEDULE:END **********************************************
        //************************************************************************************************************


        //************************************************************************************************************
        //**************************************** USER BANK INFO :START *********************************************
        //************************************************************************************************************

        #region
        //*********************************************Added By Kaikubad 01 -02-2017 ****************************************
        public ActionResult _UserBankAcDetails(int userId)
        {
            var model = db.User_Bank_Account.Where(x => x.UserId == userId).ToList();
            ViewBag.BankAcList = model;
            return PartialView();
        }
        public ActionResult AddNewBankAc()
        {
            return PartialView();
        }
        public ActionResult _AddNewBankAc(int UserId, int BankId)
        {
            User_Bank_Account_ModelView bankAccout = new User_Bank_Account_ModelView();
            if (BankId != 0)
            {
                var bankInfo = db.User_Bank_Account.Find(BankId);
                bankAccout.AccountNumber = bankInfo.AccountNumber;
                bankAccout.BankId = bankInfo.BankId;
                bankAccout.BankName = bankInfo.BankName;
                bankAccout.CreatedBy = bankInfo.CreatedBy;
                bankAccout.BranchName = bankInfo.BranchName;
                bankAccout.IsPrimary = bankInfo.IsPrimary;
            }
            return PartialView(bankAccout);
        }
        public JsonResult UpdateBankAcofUser(User_Bank_Account bankInfo, int primary)
        {
            int ColumnId = 0;
            string opName = "Update Bank Account";
            string msg = "";
            int status = -1;
            var previousInfo = db.User_Bank_Account.Find(bankInfo.BankId);
            string username = db.View_UserLists.Where(x => x.UserId == bankInfo.UserId).Select(x => x.UserName).FirstOrDefault();
            previousInfo.BankName = bankInfo.BankName;
            previousInfo.BranchName = bankInfo.BranchName;
            previousInfo.AccountNumber = bankInfo.AccountNumber;
            previousInfo.UpdatedBy = bankInfo.CreatedBy;
            previousInfo.UpdatedDate = now;
            previousInfo.IsPrimary = bankInfo.IsPrimary;
            db.Entry(previousInfo).State = EntityState.Modified;
            try
            {
                db.SaveChanges();
                status = 1;
                ColumnId = bankInfo.BankId;
                msg = "Bank '" + bankInfo.BankName + "' of employee '" + username + "' has been succesfully updated on " + now.ToString("MMM dd, yyyy hh:mm tt") + " .";
            }
            catch (Exception)
            {
                ColumnId = bankInfo.BankId;
                status = -1;
                msg = "Bank '" + bankInfo.BankName + "' of employee '" + username + "' update was unsuccesful  on " + now.ToString("MMM dd, yyyy hh:mm tt") + " .";
            }
            SaveAuditLog(opName, msg, "HR", "User Information", "User_Bank_Account", ColumnId, bankInfo.CreatedBy, now, status);
            if (primary > 0)
            {
                var updatePrimarybankAc = db.User_Bank_Account.Find(primary);
                updatePrimarybankAc.IsPrimary = false;
                db.Entry(updatePrimarybankAc).State = EntityState.Modified;
                try
                {
                    db.SaveChanges();
                    status = 1;
                    ColumnId = primary;
                    opName = "Primary Banking Information Change";
                    msg = "Primary bank account of employee '" + username + "' has been successfully changed to '" + bankInfo.BankName + "' AccountNo '" + bankInfo.AccountNumber + " on " + now.ToString("MMM dd, yyyy hh:mm tt") + " .";
                }
                catch (Exception)
                {
                    status = -1;
                    ColumnId = primary;
                    opName = "Primary Banking Information Change";
                    msg = "Primary bank account change of eployee '" + username + "' was unsuccessful " + " on " + now.ToString("MMM dd, yyyy hh:mm tt") + " .";

                }
                SaveAuditLog(opName, msg, "HR", "User Information", "User_Bank_Account", ColumnId, bankInfo.CreatedBy, now, status);
            }
            if (status == 1)
            {
                return Json("Success", JsonRequestBehavior.AllowGet);
            }
            else { return Json("Error", JsonRequestBehavior.DenyGet); }
        }
        public JsonResult DeleteBankingInformation(User_Bank_Account bankAccount)
        {
            if (bankAccount.BankId > 0 && bankAccount.UserId >= 0)
            {
                int ColumnId = 0;
                string opName = "Delete Bank Information";
                string msg = "";
                int status = -1;
                var removeData = db.User_Bank_Account.Find(bankAccount.BankId);
                string username = db.View_UserLists.Where(x => x.UserId == bankAccount.UserId).Select(x => x.UserName).FirstOrDefault();
                db.User_Bank_Account.Remove(removeData);
                try
                {
                    db.SaveChanges();
                    status = 1;
                    ColumnId = bankAccount.BankId;
                    msg = "Bank '" + removeData.BankName + "' from employee '" + username + "' has been successfully deleted on " + now.ToString("MMM dd, yyyy hh:mm tt") + " .";
                }
                catch (Exception)
                {
                    status = -1;
                    ColumnId = bankAccount.BankId;
                    msg = "Bank '" + removeData.BankName + "' from employee '" + username + "' deletion was unsuccessful  on " + now.ToString("MMM dd, yyyy hh:mm tt") + " .";
                }
                SaveAuditLog(opName, msg, "HR", "User Information", "User_Bank_Account", ColumnId, bankAccount.CreatedBy, now, status);
                if (status == 1) { return Json("Success", JsonRequestBehavior.AllowGet); }
            }
            return Json("Error", JsonRequestBehavior.AllowGet);
        }
        public JsonResult AddBankActoUser(User_Bank_Account bankInfo, int primary)
        {
            bankInfo.CreatedDate = now;
            int ColumnId = 0;
            string opName = "";
            string msg = "";
            int status = -1;
            string username = db.View_UserLists.Where(x => x.UserId == bankInfo.UserId).Select(x => x.UserName).FirstOrDefault();
            db.User_Bank_Account.Add(bankInfo);
            try
            {
                db.SaveChanges();
                ColumnId = db.User_Bank_Account.Where(x => x.CreatedBy == bankInfo.CreatedBy).Max(x => x.BankId);
                status = 1;
                opName = "Add Bank Account";
                msg = "Bank '" + bankInfo.BankName + "' of employee '" + username + "' has been added successfully on " + now.ToString("MMM dd, yyyy hh:mm tt") + " .";
            }
            catch (Exception)
            {
                status = -1;
                ColumnId = 0;
                msg = "Bank '" + bankInfo.BankName + "' of employee '" + username + "' add was unsuccessful  on " + now.ToString("MMM dd, yyyy hh:mm tt") + " .";
            }
            SaveAuditLog(opName, msg, "HR", "User Information", "User_Bank_Account", ColumnId, bankInfo.CreatedBy, now, status);
            if (primary > 0)
            {
                var updatePrimarybankAc = db.User_Bank_Account.Find(primary);
                updatePrimarybankAc.IsPrimary = false;
                db.Entry(updatePrimarybankAc).State = EntityState.Modified;
                try
                {
                    db.SaveChanges();
                    status = 1;
                    ColumnId = primary;
                    opName = "Primary Banking Information Change";
                    msg = "Primary bank account '" + bankInfo.BankName + "' of employee '" + username + "' has been changed successfully  on " + now.ToString("MMM dd, yyyy hh:mm tt") + " .";
                }
                catch (Exception)
                {
                    status = -1;
                    ColumnId = primary;
                    opName = "Primary Banking Information Change";
                    msg = "Primary bank '" + bankInfo.BankName + "' change of employee '" + username + "' was unsuccessful " + " on " + now.ToString("MMM dd, yyyy hh:mm tt") + " .";

                }
                SaveAuditLog(opName, msg, "HR", "User Information", "User_Bank_Account", ColumnId, bankInfo.CreatedBy, now, status);
            }
            if (status == 1)
            {
                return Json("Success", JsonRequestBehavior.AllowGet);
            }
            else { return Json("Error", JsonRequestBehavior.DenyGet); }
        }
        #endregion

        //************************************************************************************************************
        //**************************************** USER BANK INFO : END **********************************************
        //************************************************************************************************************


        //********************************************* Document Upload Function  ***************************************

        #region
        public PartialViewResult _UserDocumentTab(bool isDisplay)
        {
            ViewBag.isDisplay = isDisplay;
            return PartialView();
        }
        public PartialViewResult _UserDocuments(long? UserId)
        {
            UserInformationModelView model = new UserInformationModelView();
            model.UserId = Convert.ToInt32(UserId);
            ViewBag.UserId = UserId;
            return PartialView(model);
        }
        public PartialViewResult _UploadUserDocuments()
        {
            return PartialView();
        }
        public PartialViewResult UploadDoucuments()
        {
            return PartialView();
        }
        public ActionResult UserDocumentFileSave(IEnumerable<HttpPostedFileBase> files)
        {
            return Content("");
        }
        public JsonResult UserDocumentSave(UserInformationModelView user, IEnumerable<HttpPostedFileBase> files)
        {
            string msg = "";
            string name = "";
            int ColumnId = 0;
            int OperationStatus = 1;
            long preId = 1;
            if (db.User_Uploaded_Document.Any())
            {
                preId = db.User_Uploaded_Document.Max(m => m.DocumentId);
            }

            JavaScriptSerializer serializer = new JavaScriptSerializer();
            List<string> fileNameList = serializer.Deserialize<List<string>>(user.AllFilename);
            if (files != null)
            {
                int i = 0;
                foreach (var file in files)
                {
                    string fileName = fileNameList[i].ToString();
                    string filefullName = preId+user.UserId + "" + i + "_" + fileName;
                    string Paths = Path.Combine(Server.MapPath("~/AllUploadedFile/UserDocuments/"), filefullName);
                    file.SaveAs(Paths);
                    int idx = fileNameList[i].ToString().LastIndexOf('.');
                    User_Uploaded_Document document = new User_Uploaded_Document();
                    document.UserId = user.UserId;
                    document.DocumentName = filefullName;
                    document.DocumentOrginalName = fileNameList[i].ToString();
                    document.ContentType = file.ContentType;
                    document.ContentSize = file.ContentLength;
                    document.UploadedDate = now;
                    document.UploadedBy = user.CreatedBy;
                    db.User_Uploaded_Document.Add(document);
                    try
                    {
                        db.SaveChanges();
                        OperationStatus = 1;
                        var userName = db.UserInformations.Where(m => m.UserId == user.UserId).FirstOrDefault();
                        if (userName.MiddleName != null)
                        {
                            name = userName.FirstName + " " + userName.MiddleName + " " + userName.LastName;
                        }
                        else
                        {
                            name = userName.FirstName + " " + userName.LastName;
                        }
                        ColumnId = Convert.ToInt32(db.User_Uploaded_Document.Where(m => m.UploadedBy == user.CreatedBy).Max(m => m.DocumentId));
                        msg = "New document '" + document.DocumentOrginalName + "' has been successfully uploaded to user '" + name + "' on " + now.ToString("MMM dd , yyy hh:mm tt") + " .";
                    }
                    catch (Exception)
                    {
                        OperationStatus = -1;
                        msg = "New document '" + document.DocumentOrginalName + "' upload unsuccessful to user '" + name + "' on " + now.ToString("MMM dd , yyy hh:mm tt") + " .";
                    }
                    SaveAuditLog("New Document Upload", msg, "HR", "UserInformation", "User_Uploaded_Document", ColumnId, Convert.ToInt32(user.CreatedBy), now, OperationStatus);
                    i++;
                }
            }
            if (OperationStatus == 1)
            {
                return Json("Success", JsonRequestBehavior.AllowGet);
            }
            return Json("Error", JsonRequestBehavior.AllowGet);

        }
        public JsonResult DocumentFileRename(int? FileId, string FileName, long CreatedBy)
        {
            string msg = "";
            string name = "";
            int ColumnId = 0;
            int OperationStatus = 1;
            int fileId = Convert.ToInt32(FileId);
            var preFile = db.User_Uploaded_Document.Where(m => m.DocumentId == fileId).FirstOrDefault();
            string preOriginalFileName = preFile.DocumentOrginalName;
            string preFileName = preFile.DocumentName;
            string unique = preFileName.Split('_')[0];
            string filenameWithOutId = preFileName.Split('_')[1];

            User_Uploaded_Document model = db.User_Uploaded_Document.Find(fileId);
            model.DocumentOrginalName = FileName;
            model.DocumentName = unique + "_" + FileName;
            db.Entry(model).State = EntityState.Modified;
            try
            {
                db.SaveChanges();
                OperationStatus = 1;
                var userName = db.UserInformations.Where(m => m.UserId == model.UserId).FirstOrDefault();
                if (userName.MiddleName != null)
                {
                    name = userName.FirstName + " " + userName.MiddleName + " " + userName.LastName;
                }
                else
                {
                    name = userName.FirstName + " " + userName.LastName;
                }
                ColumnId = Convert.ToInt32(FileId);
                msg = "Document of user '" + name + "' has been successfully renamed from '" + preOriginalFileName + "' to '" + FileName + "' on " + now.ToString("MMM dd , yyy hh:mm tt") + " .";
            }
            catch (Exception)
            {
                OperationStatus = -1;
                msg = "Document of user '" + name + "' rename unsuccessful from '" + preOriginalFileName + "' to '" + FileName + "' on " + now.ToString("MMM dd , yyy hh:mm tt") + " .";
            }
            SaveAuditLog("Rename Document", msg, "HR", "UserInformation", "User_Uploaded_Document", ColumnId, CreatedBy, now, OperationStatus);
            db.SaveChanges();

            string NewFileName = unique + "_" + FileName;
            string OldPath = Path.Combine(Server.MapPath("~/AllUploadedFile/UserDocuments/"), preFileName);
            String path = Path.Combine(Server.MapPath("~/AllUploadedFile/UserDocuments/"), NewFileName);
            if (!System.IO.File.Exists(path))
            {
                if (System.IO.File.Exists(OldPath))
                {
                    System.IO.File.Move(OldPath, path);
                }
            }
            if (OperationStatus == 1)
            {
                return Json(FileName, JsonRequestBehavior.AllowGet);
            }
            return Json("Error", JsonRequestBehavior.AllowGet);
        }
        public JsonResult DeleteUploadedFiles(int FileId, long CreatedBy)
        {
            string msg = "";
            string name = "";
            int ColumnId = 0;
            int OperationStatus = 1;
            User_Uploaded_Document model = db.User_Uploaded_Document.Find(FileId);
            string FilePath = Path.Combine(Server.MapPath("~/AllUploadedFile/UserDocuments/"), model.DocumentName);
            if (System.IO.File.Exists(FilePath))
            {
                System.IO.File.Delete(FilePath);
            }
            db.User_Uploaded_Document.Remove(model);
            try
            {
                db.SaveChanges();
                OperationStatus = 1;
                var userName = db.UserInformations.Where(m => m.UserId == model.UserId).FirstOrDefault();
                if (userName.MiddleName != null)
                {
                    name = userName.FirstName + " " + userName.MiddleName + " " + userName.LastName;
                }
                else
                {
                    name = userName.FirstName + " " + userName.LastName;
                }
                ColumnId = Convert.ToInt32(FileId);
                msg = "Document '" + model.DocumentOrginalName + "' of user '" + name + "' has been successfully deleted on " + now.ToString("MMM dd , yyy hh:mm tt") + " .";
            }
            catch (Exception)
            {
                OperationStatus = -1;
                msg = "Document '" + model.DocumentOrginalName + "' of user '" + name + "' delete unsuccessful on " + now.ToString("MMM dd , yyy hh:mm tt") + " .";
            }
            SaveAuditLog("Delete Document", msg, "HR", "UserInformation", "User_Uploaded_Document", ColumnId, CreatedBy, now, OperationStatus);
            db.SaveChanges();
            if (OperationStatus == 1)
            {
                return Json("Success", JsonRequestBehavior.AllowGet);
            }
            return Json("Error", JsonRequestBehavior.AllowGet);
        }
        public FileResult DownloadUserDoc(long fileId)
        {
            User_Uploaded_Document doc = db.User_Uploaded_Document.Find(fileId);
            string fullPath = Path.Combine(Server.MapPath("~/AllUploadedFile/UserDocuments/"), doc.DocumentName);

            byte[] fileBytes = System.IO.File.ReadAllBytes(fullPath);
            string fileName = doc.DocumentName;
            string orginalFilename = fileName.Split('_')[1];
            return File(fileBytes, System.Net.Mime.MediaTypeNames.Application.Octet, orginalFilename);
        }
        #endregion



        //Following Methods are newly added
        #region
        public PartialViewResult _assignSalaryPackage(string view)
        {
            ViewBag.SalaryPackages = new SelectList(db.Salary_Package_List, "PackageId", "PackageName");
            ViewBag.View = view;
            return PartialView();
        }
        public PartialViewResult SalaryPackRefresh(string reason)
        {
            ViewBag.reason = reason;
            return PartialView();
        }
        public ActionResult SelectedSalaryPackDetails(int PackageId)
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
        public JsonResult UserSalaryPackUpdate(int empId, long userId, int packageId, string packageName, string previousPack)
        {
            var data = db.UserInformations.Find(empId);
            data.SalaryPackageId = packageId;
            db.Entry(data).State = EntityState.Modified;
            string msg = "";
            long ColumnId = 0;
            int status = -1;
            string opName = "Change Salary Package";
            try
            {
                db.SaveChanges();
                status = 1;
                ColumnId = data.UserId;
                msg = "Salary Package has been successfully changed from '" + previousPack + "' to '" + packageName + "' for '" + data.FirstName + " " + data.MiddleName + " " + data.LastName + "' on " + now.ToString("MMM dd, yyyy hh:mm tt") + " .";
            }
            catch (Exception)
            {
                status = -1;
                ColumnId = data.UserId;
                msg = "Change Salary Package '" + packageName + "' was  unsuccessful  for '" + data.FirstName + " " + data.MiddleName + " " + data.LastName + "' on " + now.ToString("MMM dd, yyyy hh:mm tt") + " .";
            }
            SaveAuditLog(opName, msg, "HR", "User Information", "UserInformation", (int)ColumnId, userId, now, status);
            if (status == 1)
            {
                return Json("Success", JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json("Error", JsonRequestBehavior.DenyGet);
            }
        }
        #endregion

        #region
        public ActionResult ShowAllEmpSalary()
        {
            return View();
        }
        public ActionResult _ShowAllEmpSalary()
        {
            var list = db.View_User_Salary_Details.Where(m => (m.MonthId == ((now.Month) - 1) && m.Year == now.Year) && m.Status == 0).OrderBy(o => o.EmpId).ToList();
            Session["TotalMonSalaryUser"] = list.Count();
            ViewBag.Count = list.Count();
            list = list.Take(MonthSalUser).ToList();
            return PartialView(list);
        }
        public PartialViewResult _ShowAllEmpSalaryUnitWise(int days)
        {
            var list = db.View_User_Salary_Details.Where(m => (m.MonthId == ((now.Month) - 1) && m.Year == now.Year) && m.Status == 0).OrderBy(o => o.EmpId).ToList();
            Session["AllMonSalaryUser"] = list;
            Session["TotalMonSalaryUser"] = list.Count();
            ViewBag.Count = list.Count();
            list = list.Take(MonthSalUser).ToList();
            return PartialView("_ShowAllEmpSalary", list);
        }
        public JsonResult GetMonthSalaryUser(int? page)
        {
            page = page ?? 0;
            int total = Convert.ToInt16(Session["TotalMonSalaryUser"]);
            int skip = MonthSalUser * Convert.ToInt16(page);
            var canPage = skip < total;
            if (canPage)
            {
                var listBackFromSession = (List<View_Voucher_Transaction>)Session["AllMonSalaryUser"];
                var partslist = listBackFromSession.Skip(skip).Take(MonthSalUser).ToList();
                return Json(partslist, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json("", JsonRequestBehavior.AllowGet);
            }
        }
        #endregion

        #region
        public JsonResult UserPassReset(long? id,long? userId)
        {
            if (id > 0 && userId > 0)
            {
                Random generator = new Random();
                UserLogin ulogin = db.UserLogins.Where(m => m.UserId == id).FirstOrDefault();
                ulogin.Password = generator.Next(0, 1000000).ToString("D6");
                db.Entry(ulogin).State = EntityState.Modified;
                db.SaveChanges();
                return Json(ulogin.Password, JsonRequestBehavior.AllowGet);
            }
            return Json("Error",JsonRequestBehavior.AllowGet);
        }
        #endregion



        public PartialViewResult ChangeWorkingSchedule(int id)
        {
            ViewBag.WorkingSchedules = new SelectList(db.WorkingScheduleLists, "WorkingScheduleId", "SchedulName", id);
            ViewBag.id = id;
            return PartialView();
        }
        public PartialViewResult _ChangeWorkingSchedule(int id)
        {
            ViewBag.WorkingSchedules = new SelectList(db.WorkingScheduleLists, "WorkingScheduleId", "SchedulName", id);
            ViewBag.id = id;
            return PartialView();
        }
        public JsonResult AssignWorkingScheduleWithUser(int WorkingScheduleId, long UserId, long AssignedBy)
        {
            if (WorkingScheduleId > 0)
            {
                int status = -1;
                string msg = "";
                string opName = "Assign Working Schedule";
                int ColumnId = WorkingScheduleId;
                string Username = db.View_UserLists.Where(m => m.UserId == UserId).Select(m => m.UserName).FirstOrDefault();
                string Packname = db.WorkingScheduleLists.Where(m => m.WorkingScheduleId == WorkingScheduleId).Select(m => m.SchedulName).FirstOrDefault();
                UserInformation user = db.UserInformations.Find(UserId);
                user.WorkingScheduleId = WorkingScheduleId;
                user.WorkScheduleAssignedBy = AssignedBy;
                user.WorkScheduleAssignedDate = now;
                db.Entry(user).State = EntityState.Modified;
                try
                {
                    db.SaveChanges();
                    status = 1;
                    msg = "Working schedule '" + Packname + "' has been successfully assigned with employee '" + Username + "' on " + now.ToString("MMM dd, yyyy hh:mm tt") + " .";
                }
                catch (Exception)
                {
                    status = -1;
                    msg = "Working schedule '" + Packname + "' assign with employee '" + Username + "' was unsuccessful on " + now.ToString("MMM dd, yyyy hh:mm tt") + " .";
                }
                SaveAuditLog(opName, msg, "HR", "User Information", "UserInformation", ColumnId, AssignedBy, now, status);
                if (status == 1)
                {
                    return Json("Success", JsonRequestBehavior.AllowGet);
                }
            }
            return Json("Error", JsonRequestBehavior.AllowGet);
        }
        public JsonResult ChangeWorkingScheduleWithUser(int WorkingScheduleId, long UserId, long AssignedBy)
        {
            if (WorkingScheduleId > 0 && UserId > 0)
            {
                int status = -1;
                string msg = "";
                string opName = "Change Assigned Working Schedule";
                int columnId = WorkingScheduleId;
                string Username = db.View_UserLists.Where(m => m.UserId == UserId).Select(m => m.UserName).FirstOrDefault();
                string Packname = db.WorkingScheduleLists.Where(m => m.WorkingScheduleId == WorkingScheduleId).Select(m => m.SchedulName).FirstOrDefault();
                UserInformation user = db.UserInformations.Find(UserId);
                user.WorkingScheduleId = WorkingScheduleId;
                user.WorkScheduleAssignedBy = AssignedBy;
                user.WorkScheduleAssignedDate = now;
                db.Entry(user).State = EntityState.Modified;
                try
                {
                    db.SaveChanges();
                    status = 1;
                    msg = "Working schedule '" + Packname + "' has been successfully changed with employee '" + Username + "' on " + now.ToString("MMM dd, yyyy hh:mm tt") + " .";
                }
                catch (Exception)
                {
                    status = -1;
                    msg = "Working schedule '" + Packname + "' change with employee '" + Username + "' was unsuccessful on " + now.ToString("MMM dd, yyyy hh:mm tt") + " .";
                }
                SaveAuditLog(opName, msg, "HR", "User Information", "UserInformation", columnId, AssignedBy, now, status);
                if (status == 1)
                {
                    return Json("Success", JsonRequestBehavior.AllowGet);
                }
            }
            return Json("Error", JsonRequestBehavior.AllowGet);
        }

        #region Password Reset

        public PartialViewResult PasswordResetPartial(long userId) {
            ViewBag.UserId = userId;
            return PartialView();
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