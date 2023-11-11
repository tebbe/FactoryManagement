using FactoryManagement.Filters;
using FactoryManagement.Helpers;
using FactoryManagement.Models;
using FactoryManagement.ModelView.Acquisition;
using FactoryManagement.ModelView.Auth;
using FactoryManagement.ModelView.Inventory;
using FactoryManagement.ModelView.Inventory.Mahinaries;
using FactoryManagement.ModelView.Inventory.Product;
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
    [DisplayName("Inventory")]
    public class InventoryController : Controller
    {
        #region Private Properties
        private FactoryManagementEntities db = new FactoryManagementEntities();
        private FactoryManagementEntities dbAud = new FactoryManagementEntities();


        int PartsPage = 20;
        const int EqiptPerPage = 50;
        const int MachineMainPerPage = 50;
        const int PerPageWareItem = 50;
        const int PerPageMaintaenceMachine = 50;
        const int ReturnDisPerPage = 50;
        const int WareProPerPage = 20;
        const int LogPerPage = 20;
        const int UDisPerPage = 20;

        static private int offset = Convert.ToInt32(ConfigurationManager.AppSettings["localTime"]);
        DateTime now = DateTime.UtcNow.AddMinutes(offset);
        #endregion

        #region Machine Type Create,Update,Delete
        public ActionResult MachineTypeName()
        {
            return View();
        }
        public PartialViewResult _AllMachineTypeName(string MachineId)
        {
            List<MachineTypeName> machineTypeList = new List<Models.MachineTypeName>();
              var machineType = db.MachineTypeNames.ToList();
            if (!string.IsNullOrEmpty(MachineId))
            {
                string[] mId;
                long searchId = 0;
                mId = MachineId.Split(',');
                foreach (var id in mId)
                {
                    searchId = Convert.ToInt64(id.Split(',')[0]);
                    var mTypeList = (from m in machineType
                                     where m.MachineId == searchId
                                     select m).ToList();
                    foreach (var item in mTypeList)
                    {
                        machineTypeList.Add(item);
                    }
                }
                return PartialView(machineTypeList);
            }
            return PartialView(machineType);
        }
        public JsonResult SearchingMachineTypeName()
        {
            List<MachineTypeName> machineTypeList = new List<MachineTypeName>();
            var machinType = db.MachineTypeNames.ToList();

            foreach (var names in machinType)
            {
                if (!machineTypeList.Where(x => x.MachineTypeName1 == names.MachineTypeName1).Any())
                {
                    MachineTypeName viewModel = new MachineTypeName();
                    viewModel.MachineTypeName1 = names.MachineTypeName1;
                    viewModel.MachineId = names.MachineId;
                    machineTypeList.Add(viewModel);
                }
            }
            return Json(machineTypeList, JsonRequestBehavior.AllowGet);
        }
        public ActionResult MachineTypeNameCreate(int? id)
        {
            MachineTypeNameModelView model = new MachineTypeNameModelView();
            if (id != null)
            {
                MachineTypeName machine =db.MachineTypeNames.Find(id);
                model.MachineId = machine.MachineId;
                model.MachineTypeName1 = machine.MachineTypeName1;
                model.CreatedBy = machine.CreatedBy;
                model.CreatedDate = machine.CreatedDate;
                model.UpdatedBy = machine.UpdatedBy;
                model.UpdatedDate = machine.UpdatedDate;
            }
            return PartialView(model);
        }
        public JsonResult MachineTypeNameSave(MachineTypeNameModelView model)
        {
            if (ModelState.IsValid)
            {
                int machineId =0;
                string msg = "";
                string OperationName = "";
                int OperationStatus = 1;

                MachineTypeName machine = new MachineTypeName();
                if (model.MachineId > 0)
                {
                    machine = db.MachineTypeNames.Find(model.MachineId);
                    machine.MachineTypeName1 = model.MachineTypeName1;
                }
                else
                {
                    machine.MachineTypeName1 = model.MachineTypeName1;
                    machine.CreatedDate = now;
                    machine.CreatedBy = model.CreatedBy;
                }
                try
                {
                    if (model.MachineId > 0)
                    {
                        machine.UpdatedDate = now;
                        machine.UpdatedBy = model.CreatedBy;
                        db.Entry(machine).State = EntityState.Modified;
                        db.SaveChanges();
                        machineId = model.MachineId;
                        OperationName = "Machine Type Name Update";
                        msg = "Machine type name " + machine.MachineTypeName1 + " has been updated successfully.";
                    }
                    else
                    {
                        db.MachineTypeNames.Add(machine);
                        db.SaveChanges();
                        machineId = db.MachineTypeNames.Where(m => m.CreatedBy == model.CreatedBy).Max(m => m.MachineId);
                        OperationName = "Add New Machine Type Name";
                        msg = "New machine type name " + machine.MachineTypeName1 + " has been added successfully.";
                    }
                    OperationStatus = 1;
                }
                catch (Exception ex)
                {
                    string ermsg = ex.ToString();
                    OperationStatus = -1;
                }
                SaveAuditLog(OperationName, msg, "Inventory", "MachineTypeName", "MachineTypeName", machineId, model.CreatedBy, now, OperationStatus);
                if (OperationStatus == 1)
                {
                    return Json("Success", JsonRequestBehavior.AllowGet);
                }
            }
            return Json("Error", JsonRequestBehavior.AllowGet);
        }
        public JsonResult CheckIsMachineTypeInUse(int id)
        {
            var machinId = 0;
            if (db.Machines.Where(m => m.MachineType == id).Any())
            {
                machinId = db.Machines.Where(m => m.MachineType == id).Select(m=>m.MachineType).FirstOrDefault();
            }
            return Json(machinId, JsonRequestBehavior.AllowGet);
          
        }
        public JsonResult DeleteMachineTypeName(int? id, string MachineName, long UserId)
        {
            if (id == null)
            {
                return Json("Error", JsonRequestBehavior.AllowGet);
            }
            else
            {
                string msg = "", value = "";
                int ColumnId = 0;
                int OperationStatus = 1;
                if (db.Machines.Where(m => m.MachineType == id).Any())
                {
                    OperationStatus = -1;
                    value = "This machine type can not be deleted.Its already been used.";
                }
                else
                {
                    MachineTypeName machine = db.MachineTypeNames.Find(id);
                    db.MachineTypeNames.Remove(machine);
                    try
                    {
                        msg = "Machine type name " + machine.MachineTypeName1 + " successfully deleted.";
                        db.SaveChanges();
                        OperationStatus = 1;
                        value = "Success";
                    }
                    catch (Exception ex)
                    {
                        string errorMsg = ex.ToString();
                        OperationStatus = -1;
                        msg = "Machine type name " + machine.MachineTypeName1 + " deletetion unsuccessful.";
                        value = "Error";
                    }
                }
                SaveAuditLog("Machine Type Name Delete", msg, "Inventory", "MachineTypeName", "MachineTypeName", ColumnId, UserId, now, OperationStatus);
                return Json(value, JsonRequestBehavior.AllowGet);
            }
        }  
        #endregion

        #region Created,Update and Delete Spare Parts
        public ActionResult AllPartsList()
        {
            return View();
        }
        [EncryptedActionParameter]
        public ActionResult _ShowAllPartsList(int? PartsId)
        {
            var list = db.View_All_InventoryList.Where(m => m.ProductTypeId == 4071).OrderBy(m => m.ProductName).ToList();
            if (PartsId != null)
            {
                list = db.View_All_InventoryList.Where(m => m.InventoryId == PartsId).OrderBy(m => m.ProductName).ToList();

                ViewBag.TotalCount = list.Count();
                ViewBag.SparePartsList = list;
            }
            Session["AllSpareParts"] = list;
            Session["TotalSparePartsCount"] = list.Count();
            ViewBag.TotalCount = list.Count();
            ViewBag.SparePartsList = list.Take(PartsPage).ToList();
            return PartialView(ViewBag.SparePartsList);
        }
        public JsonResult GetSparePartsList(int? page)
        {
            page = page ?? 0;
            int total = Convert.ToInt16(Session["TotalSparePartsCount"]);
            int skip = PartsPage * Convert.ToInt16(page);
            var canPage = skip < total;
            if (canPage)
            {
                var listBackFromSession = (List<View_All_InventoryList>)Session["AllSpareParts"];
                var partslist = listBackFromSession.Skip(skip).Take(PartsPage).ToList();
                return Json(partslist, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json("", JsonRequestBehavior.AllowGet);
            }

        }
        public JsonResult PartsNameSearching(string prefix)
        {
            if (prefix == "")
            {
                return Json("", JsonRequestBehavior.AllowGet);
            }
            else
            {
                var list = db.View_All_InventoryList.Where(m => m.ProductTypeId == 4071).OrderBy(m => m.ProductName).ToList();
                list = list.Where(m => m.ProductName.ToLower().Contains(prefix.ToLower())).ToList();
                return Json(list, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult SparePartsCreated(int? id)
        {
            SparePartsListModelView model = new SparePartsListModelView();
            if (id != null)
            {
                SparePartsList parts = db.SparePartsLists.Find(id);
                model.PartsId = parts.PartsId;
                model.PartsName = parts.Name;
                model.Type = parts.Type;
                model.Description = parts.Description;
                model.CreatedBy = parts.CreatedBy;
                model.CreatedDate = parts.CreatedDate;
                model.ExpireDate = parts.ExpireDate;
            }
            return View(model);
        }
        [HttpPost]
        public ActionResult SparePartsCreated(SparePartsListModelView model)
        {
            if (ModelState.IsValid)
            {
                SparePartsList parts = new SparePartsList();
                AuditInformation audit = new AuditInformation();
                if (model.PartsId > 0)
                {
                    parts = db.SparePartsLists.Find(model.PartsId);
                    parts.UpatedBy = model.CreatedBy;
                    parts.UpdatedDate = now;

                    audit.OperationName = "Edit Spare Parts Info";
                    audit.Message = model.PartsName + " parts information updated.";
                    audit.PageName = "Edit Spare Parts";
                    audit.ColumnId = model.PartsId;
                }
                else
                {
                    parts.CreatedBy = model.CreatedBy;
                    parts.CreatedDate = now;

                    audit.OperationName = "Add Spare Parts";
                    audit.Message = "New spare part " + model.PartsName + " added.";
                    audit.PageName = "Add Spare Parts";
                }
                parts.Name = model.PartsName;
                parts.Type = model.Type;
                parts.Description = model.Description;
                if (model.Type == 1)
                {
                    parts.ExpireDate = model.ExpireDate;
                }
                audit.UserId = model.CreatedBy;
                audit.Date = now;
                audit.ModuleName = "Inventory";
                audit.TableName = "SparePartsLists";
                try
                {
                    if (model.PartsId > 0)
                    {
                        db.Entry(parts).State = EntityState.Modified;
                        db.SaveChanges();
                    }
                    else
                    {
                        db.SparePartsLists.Add(parts);
                        db.SaveChanges();
                        audit.ColumnId = db.SparePartsLists.Where(m => m.CreatedBy == model.CreatedBy).Max(m => m.PartsId);
                    }
                    audit.OperationStatus = 1;
                    
                }
                catch(Exception ex){
                    string errorMsg = ex.Message.ToString();
                    if (model.PartsId > 0)
                    {
                        audit.Message = model.PartsName + " information update unsuccessful";
                    }
                    else
                    {
                        audit.Message = "New spare part " + model.PartsName + " addition unsuccessful";
                    }
                    audit.OperationStatus = -1;
                    audit.ColumnId = 0;
                }
                db.AuditInformations.Add(audit);
                db.SaveChanges();
            }
            return RedirectToAction("AllPartsList");
        }
        public JsonResult DeleteParts(int? partsId, long UserId)
        {
            string msg = "";
            string name = "";
            int ColumnId = 0;
            int OperationStatus=1;
            
            if (partsId == null)
            {
                return Json("Error",JsonRequestBehavior.AllowGet);
            }
            else
            {
                SparePartsList parts = db.SparePartsLists.Find(partsId);
                name = parts.Name;
                ColumnId = parts.PartsId;
                try
                {
                    msg = name + " deleted successfully";
                    db.SparePartsLists.Remove(parts);
                    db.SaveChanges();
                    OperationStatus = 1;
                    
                }catch(Exception ex){
                    string errorMsg = ex.Message.ToString();
                    msg = name + " deletion unsuccessful";
                    OperationStatus = -1;
                }
                SaveAuditLog("Delete", msg, "Inventory", "Delete Parts", "SparePartsLists", ColumnId, UserId, now, OperationStatus);
                return Json("Success", JsonRequestBehavior.AllowGet);
            }
        }
        #endregion

        #region Created,Update and Delete Spare Machinaries
        public ActionResult AllMachineList()
        {
            return View();
        }
        public JsonResult SearchingMachineByName()
        {
            List<MachineModelView>machineVList=new List<MachineModelView>();
            var machinMList = db.Machines.ToList();

            foreach (var names in machinMList)
            {
                if (!machineVList.Where(x => x.Acroynm == names.Acroynm).Any())
                {
                    MachineModelView viewModel = new MachineModelView();
                    viewModel.Acroynm = names.Acroynm;
                    viewModel.Id= names.Id;
                    machineVList.Add(viewModel);
                }
            }
            return Json(machineVList, JsonRequestBehavior.AllowGet);
        }
        public PartialViewResult _AllMachineList(int? Id,string search)
        {
            string[] sId;
            long searchId = 0;
            List<Machine> machinList = new List<Machine>();
            var AllMachines = db.Machines.OrderBy(p => p.MachineType).ToList();
            if (Id != null && Id > 0)
            {
                AllMachines = (from p in AllMachines
                               where p.Id == Id
                               select p).OrderBy(p => p.MachineType).ToList();
            }
            
          
            if (!string.IsNullOrEmpty(search))
            {
                sId = search.Split(',');
                for (int j = 0; j < sId.Length; j++)
                {
                    searchId = Convert.ToInt64(sId[j]);
                    var myList = (from m in AllMachines
                                      where m.Id == searchId
                                      select new Machine{
                                          Id = m.Id,
                                          MachineType = m.MachineType,
                                          Acroynm = m.Acroynm,
                                          ModelName = m.ModelName,
                                          CreatedBy = m.CreatedBy,
                                          CreatedDate = m.CreatedDate,
                                          UpdatedDate = m.UpdatedDate,
                                          UpdatedBy =m.UpdatedBy                                  
                                      }).ToList();
                        foreach (var item in myList)
                        {
                            machinList.Add(item);
                        }                 
                }
                return PartialView(machinList);
            }
            return PartialView(AllMachines);
        }
        [EncryptedActionParameter]
        public ActionResult AllEquipmentList(string acronynm)
        {
            if (acronynm != "")
            {
                ViewBag.MachineTypeId = new SelectList(db.View_Machine_Lists.Select(x => new { x.Acroynm }).Distinct(), "Acroynm", "Acroynm", acronynm);
            }
            else
            {
                ViewBag.MachineTypeId = new SelectList(db.View_Machine_Lists.Select(x => new { x.Acroynm }).Distinct(), "Acroynm", "Acroynm"); 
            }
            ViewBag.acronynm = acronynm;
            return View();
        }
        public ActionResult _ShowAllMachineList(int? Status, int? MachineId,string acronymIds)
        {
            var machinList = db.View_Machine_Lists.ToList();
            if (MachineId != null)
            {
                if (Status != null)
                {
                    machinList = machinList.Where(m => m.MachineId == MachineId && m.Status == Status).OrderBy(m => m.MachineAcronym).ToList();
                }
                else
                {
                    machinList = machinList.Where(m => m.MachineId == MachineId).OrderBy(m => m.MachineAcronym).ToList();
                }
            }
            else
            {
                if (Status != null)
                {
                    machinList = machinList.Where(m => m.Status == Status).OrderBy(m => m.MachineAcronym).ToList();
                }
                else
                {
                    machinList = machinList.OrderBy(m => m.MachineAcronym).ToList();
                }
            }
            if (!string.IsNullOrEmpty(acronymIds))
            {
                List<int> acronymListInt = new List<int>();
                foreach (var acronym in acronymIds.Split(',').ToList()) {
                    int x = Convert.ToInt32(acronym);
                    acronymListInt.Add(x);
                }
                machinList = machinList.Where(m=> acronymListInt.Contains(m.MachineTypeId)).OrderBy(m => m.MachineAcronym).ToList();
            }
            ViewBag.TotalMachineCount = machinList.Count();
            ViewBag.machineList = machinList.Take(EqiptPerPage).ToList();
            Session["AllMachinList"] = machinList;
            Session["TotalMachinListCount"] = machinList.Count();
            return PartialView(ViewBag.machineList);
        }
        public JsonResult ChangeEquipmentStatus(int? Status, int? MachineId, long UserId)
        {
            int OperationStatus = -1;
            string msg = "";
            string opeName = "";
            var machinTypeName = "";
            int ColumnId =Convert.ToInt32( MachineId);
            MachineList allMachine = new MachineList();
            if (Status != null && MachineId > 0)
            {
                 allMachine = db.MachineLists.Where(m => m.MachineId == MachineId).FirstOrDefault();
                if (allMachine.MachineId >0)
                {
                    if (allMachine.Status == 1)
                    {
                        allMachine.Status = 0;
                    }
                    else
                    {
                        allMachine.Status = 1;
                    }
                }
                db.Entry(allMachine).State = EntityState.Modified;
                opeName="Change Equipment Status";
                
            }
            try
            {
                db.SaveChanges();
                OperationStatus = 1;
                machinTypeName = db.View_Machine_Lists.Where(m => m.MachineId == allMachine.MachineId).Select(m => m.MachineTypeName).FirstOrDefault();
                msg = " Equipment status of '" + machinTypeName + "' has been successfully change  on " + now.ToString("MMM dd , yyy hh:mm tt") + ".";
            }
            catch (Exception)
            {
                OperationStatus = -1;
                msg = " Equipment status of '" + machinTypeName + "' change was unsuccessfully on " + now.ToString("MMM dd , yyy hh:mm tt") + ".";
            }
            SaveAuditLog(opeName, msg, "Inventory", " All Equipment List", "MachineList", ColumnId,UserId, now, OperationStatus);
            if (OperationStatus == 1)
            {
                return Json("Success", JsonRequestBehavior.AllowGet);
            }
            return Json("Error",JsonRequestBehavior.AllowGet);
        }
        public JsonResult GetMachineList(int? page)
        {
            page = page ?? 0;
            int total = Convert.ToInt16(Session["TotalMachinListCount"]);
            int skip = EqiptPerPage * Convert.ToInt16(page);
            var canPage = skip < total;
            if (canPage)
            {
                var listBackFromSession = (List<View_Machine_Lists>)Session["AllMachinList"];
                var partslist = listBackFromSession.Skip(skip).Take(EqiptPerPage).ToList();
                return Json(partslist, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json("", JsonRequestBehavior.AllowGet);
            }

        }
        public JsonResult MachineNameSearching(string prefix,int? type)
        {
            if (prefix == "")
            {
                return Json("", JsonRequestBehavior.AllowGet);
            }
            else
            {
                if (type > 0)
                {
                    var allMachine = (List<View_Machine_Lists>)Session["AllMachinList"];
                    var list = (from m in allMachine
                                where (m.MachineAcronym.ToLower().Contains(prefix.ToLower()) && m.MachineType == type)
                                select new View_Machine_Lists
                                {
                                    MachineId = m.MachineId,
                                    MachineTypeId = m.MachineTypeId,
                                    MachineAcronym = m.MachineAcronym,
                                    Acroynm = m.Acroynm,
                                    MachineTypeName = m.MachineTypeName,
                                    Status = m.Status,
                                    AssignStatus = m.AssignStatus,
                                    CreatedBy = m.CreatedBy,
                                    UpdatedDate = m.UpdatedDate,
                                    UpdatedBy = m.UpdatedBy,
                                    PictureName = m.PictureName,
                                    UserName = m.UserName
                                }).OrderBy(o => o.MachineAcronym).ToList();
                    return Json(list, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    var allMachine = (List<View_Machine_Lists>)Session["AllMachinList"];
                    var list = (from m in allMachine
                                where (m.MachineAcronym.ToLower().Contains(prefix.ToLower()))
                                select new View_Machine_Lists
                                {
                                    MachineId = m.MachineId,
                                    MachineTypeId = m.MachineTypeId,
                                    MachineAcronym = m.MachineAcronym,
                                    Acroynm = m.Acroynm,
                                    MachineTypeName = m.MachineTypeName,
                                    Status = m.Status,
                                    AssignStatus = m.AssignStatus,
                                    CreatedBy = m.CreatedBy,
                                    UpdatedDate = m.UpdatedDate,
                                    UpdatedBy = m.UpdatedBy,
                                    PictureName = m.PictureName,
                                    UserName = m.UserName
                                }).OrderBy(o => o.MachineAcronym).ToList();
                    return Json(list, JsonRequestBehavior.AllowGet);
                }
                
            }
        }
        public JsonResult checkMachineTypeUsed(int id)
        {
            if (db.MachineLists.Where(m => m.MachineTypeId == id).Any())
            {
                return Json(true,JsonRequestBehavior.AllowGet);
            }
            return Json(false, JsonRequestBehavior.AllowGet);
        }
        public JsonResult DeleteMachineType(int id, long userId, string name)
        {
            if (id > 0)
            {
                int status = -1;
                string msg = "";
                Machine machintype = db.Machines.Find(id);
                if (machintype != null)
                {
                    db.Machines.Remove(machintype);
                    try
                    {
                        db.SaveChanges();
                        status = 1;
                        msg = "Machine '" + name + "' has been successfully deleted.";
                    }
                    catch (Exception)
                    {
                        msg = "Machine '" + name + "' deletion was unsuccessful.";
                        status = -1;
                    }
                    SaveAuditLog("Delete Machine", msg, "Inventory", "AllMachineList", "Machine", id, userId, now, status);
                    if (status == 1)
                    {
                        return Json("Success", JsonRequestBehavior.AllowGet);
                    }
                }
            }
            return Json("Error", JsonRequestBehavior.AllowGet);
        }
        public PartialViewResult _AddMoreEquip(int Id)
        {
            MachineTypeModelView model = (from m in db.Machines
                                          join t in db.MachineTypeNames
                                          on m.MachineType equals t.MachineId
                                          join b in db.BrandLists
                                          on m.BrandId equals b.BrandId
                                          join c in db.CountryLists on m.CountryOfOrigin equals c.CountryCode
                                          select new MachineTypeModelView
                                          {
                                              Id = m.Id,
                                              Name = m.Name,
                                              Acroynm = m.Acroynm,
                                              MachineType = m.MachineType,
                                              MachineTypeName = t.MachineTypeName1,
                                              BrandId = m.BrandId,
                                              BrandName = b.BrandName,
                                              ModelName = m.ModelName,
                                              CountryOfOrigin = m.CountryOfOrigin,
                                              CountryName = c.CountryName,
                                              Quantity = m.Quantity
                                          }).FirstOrDefault();
            return PartialView(model);
        }
        public JsonResult MachineAcronymList()
        {
            var data = db.MachineLists.Select(x => new {
                x.MachineAcronym,x.MachineTypeId
            }).Distinct().ToList();
            return Json(data, JsonRequestBehavior.AllowGet);
        }
        public PartialViewResult _EditMachineAcrynm(int Id)
        {
            var model = db.MachineLists.Find(Id);
            return PartialView(model);
        }
        public JsonResult UpdateMachineAcrynm(MachineList model)
        {
            if (model != null)
            {
                int status = -1;
                string msg = "";
                MachineList machine = db.MachineLists.Find(model.MachineId);
                string prename = machine.MachineAcronym;
                machine.MachineAcronym = model.MachineAcronym;
                machine.UpdatedDate = now;
                machine.UpdatedBy = model.CreatedBy;
                db.Entry(machine).State = EntityState.Modified;
                try
                {
                    db.SaveChanges();
                    status = 1;
                    msg = "Machine '" + prename + "'s ' acronynm has been successfully changed as '"+model.MachineAcronym+"' on "+now.ToString("dd MMM , YYYY hh:mm tt")+" . ";
                }
                catch (Exception)
                {
                    status = -1;
                    msg = "Machine '" + prename + "'s ' acronynm change was unsuccessful. ";
                }
                SaveAuditLog("Edit Machine Acronynm", msg, "Inventory", "All Equipment Lists", "MachineList", model.MachineId, model.CreatedBy, now, status);
                if (status == 1)
                {
                    return Json("Success", JsonRequestBehavior.AllowGet);
                }
            }
            return Json("Error", JsonRequestBehavior.AllowGet);
        }

        public ActionResult _PartialViewForSpareParts()
        {
            int id = db.ProductTypeLists.Where(m => m.ProductTypeName == "Spare Parts").Select(m => m.ProductTypeId).FirstOrDefault();
            var allPartsList = (from m in db.View_All_InventoryList
                        where m.ProductTypeId == id
                        select new
                        {
                            PartsId = m.InventoryId,
                            Name = m.ProductName,
                            Canbebreakable = m.CanbeBreakable,
                            CanbeTopup = m.CanbeTopup,
                            UnitId = m.UnitId,
                            UnitName = m.UnitName,
                        }).OrderBy(o => o.Name).ToList();

            ViewBag.allPartsList = allPartsList;
            return PartialView();
        }
        public JsonResult GetAllSpareParts()
        {
            int id = db.ProductTypeLists.Where(m => m.ProductTypeName == "Spare Parts").Select(m => m.ProductTypeId).FirstOrDefault();
            var List = (from m in db.View_All_InventoryList
                        where m.ProductTypeId == id
                        select new
                        {
                            PartsId = m.InventoryId,
                            Name = m.ProductName,
                            Canbebreakable = m.CanbeBreakable,
                            CanbeTopup = m.CanbeTopup,
                            UnitId = m.UnitId,
                            UnitName = m.UnitName,
                        }).OrderBy(o => o.Name).ToList();

            return Json(List,JsonRequestBehavior.AllowGet);
        }
        public JsonResult SpareParts()
        {
            return Json(db.SparePartsLists.ToList(), JsonRequestBehavior.AllowGet);
        }
        [EncryptedActionParameter]
        public ActionResult MachineTypeCreate(string v)
        {
            MachineTypeModelView model = new MachineTypeModelView();
            ViewBag.BrandId = new SelectList(db.BrandLists, "BrandId", "BrandName", model.BrandId);
            ViewBag.CountryId = new SelectList(db.CountryLists.OrderBy(m => m.CountryName), "CountryCode ", "CountryName", model.CountryOfOrigin);
            ViewBag.MachineType = new SelectList(db.MachineTypeNames.OrderBy(m => m.MachineTypeName1), "MachineId", "MachineTypeName1");
            List<SelectListItem> allPartsList = new SelectList(db.SparePartsLists.OrderBy(m => m.Name), "PartsId", "Name").ToList();
            ViewBag.allPartsList = allPartsList;
            ViewBag.v = v;
            return View();
        }
        public PartialViewResult _ShowBrandList()
        {
            var AllParentBrand = (from p in db.View_BrandList
                                  where p.HasParent == false
                                  select new BrandListViewModel
                                  {
                                      BrandId = p.BrandId,
                                      BrandName = p.BrandName,
                                      CreatedBy = p.CreatedBy,
                                      CreatedDateName = p.CreatedDateName,
                                      HasChild = p.HasChild,
                                      UserName = p.UserName,
                                      Picture = p.Picture
                                  }).OrderBy(p => p.BrandName).ToList();
            ViewBag.AllParentBrandList = AllParentBrand;
            return PartialView();
        }
        public JsonResult MahineNameSearching(string prefix)
        {
            if (prefix == "")
            {
                return Json("", JsonRequestBehavior.AllowGet);
            }
            else
            {
                var allMachine = (from s in db.Machines
                                  where s.Acroynm.ToLower().Contains(prefix.ToLower())  
                                  select new MachineTypeModelView
                                  { 
                                      Id = s.Id,
                                      Name = s.Acroynm
                                  }).ToList();
                return Json(allMachine, JsonRequestBehavior.AllowGet);
            }
        }
        public PartialViewResult _SelectedMachineInfo(int Id)
        {
            MachineTypeModelView model = (from m in db.Machines
                             join b in db.BrandLists
                             on m.BrandId equals b.BrandId
                             join c in db.CountryLists on m.CountryOfOrigin equals c.CountryCode
                             select new MachineTypeModelView
                             {
                                 Id = m.Id,
                                 Name= m.Name,
                                 Acroynm= m.Acroynm,
                                 MachineType = m.MachineType,
                                 MachineTypeName = db.MachineTypeNames.Where(t=> t.MachineId == m.MachineType ).Select(t => t.MachineTypeName1).FirstOrDefault(),
                                 BrandId = m.BrandId,
                                 BrandName = b.BrandName,
                                 ModelName = m.ModelName,
                                 CountryOfOrigin= m.CountryOfOrigin,
                                 CountryName=c.CountryName,
                                 Mgf = m.Mgf
                             }).FirstOrDefault();
            return PartialView(model);
        }


        public JsonResult MachineTypeSave(MachineTypeModelView model, int[] AllUnit, string[] AllId, int[] AllSpareId, string[] allPartsQuan, string[] allPartsRepDate)
        {
            if (ModelState.IsValid)
            {
                string msg = "";
                int ColumnId = 0;
                int OperationStatus = 1;

                Machine machine = new Machine();
                if ((model.BrandId == 0 || model.BrandId < 1) && model.BrandName != null)
                {
                    BrandList brand = new BrandList();
                    brand.BrandName = model.BrandName;
                    brand.HasParent = false;
                    brand.CreatedDate = now;
                    brand.CreatedBy = model.CreatedBy;
                    db.BrandLists.Add(brand);
                    db.SaveChanges();
                    machine.BrandId = db.BrandLists.Where(m => m.CreatedBy == model.CreatedBy).Max(m => m.BrandId);
                }
                else
                {
                    machine.BrandId = model.BrandId;
                }
                machine.MachineType = model.MachineType;
                machine.Acroynm = model.Acroynm;
                machine.ModelName = model.ModelName;                
                machine.CountryOfOrigin = model.CountryOfOrigin;
                machine.Mgf = model.Mgf;
                machine.Quantity = model.Quantity;
                machine.CreatedDate = now;
                machine.CreatedBy = model.CreatedBy;
                try
                {
                    db.Machines.Add(machine);
                    db.SaveChanges();

                    int partsId = db.Machines.Max(m => m.Id);
                    ColumnId = db.Machines.Where(m => m.CreatedBy == model.CreatedBy).Max(m => m.Id);
                    OperationStatus = 1;
                    msg = "New machine '" + model.Name + "' has been successfully added on "+now.ToString("MMM dd, yyy hh:mm tt")+" .";
                }
                catch(Exception ){
                    OperationStatus = -1;
                    msg = "New machine '" + model.Name + "' addition was unsuccessful on " + now.ToString("MMM dd, yyy hh:mm tt") + " .";
                }
                SaveAuditLog("Add New Machine", msg, "Inventory", "Add New Machine", "Machine", ColumnId, model.CreatedBy, now, OperationStatus);
                if (OperationStatus == 1)
                {
                    int machineId;

                    for (int i = 0; i < model.Quantity; i++)
                    {
                        MachineList machines = new MachineList();
                        machines.MachineTypeId = ColumnId;
                        machines.Status = (model.AllPartsSame == 1)?1:0;
                        machines.AssignStatus = false;
                        machines.CreatedDate = now;
                        machines.CreatedBy = model.CreatedBy;
                        try
                        {
                            db.MachineLists.Add(machines);
                            db.SaveChanges();
                            msg = "Machine "+i+ " successfully created.";
                            machineId = db.MachineLists.Where(o => o.CreatedBy == model.CreatedBy).Max(o => o.MachineId);

                            OperationStatus = 1;
                            if (model.AllPartsSame == 1)
                            {
                                if (AllSpareId.Length > 0)
                                {
                                    for (int j = 0; j < AllSpareId.Length; j++)
                                    {
                                        int inven_id = AllSpareId[j];
                                        if (Convert.ToBoolean(AllId[j]))
                                        {
                                            MachinePartsList machineParts = new MachinePartsList();
                                            machineParts.MachineId = machineId;
                                            machineParts.PartsId = inven_id;
                                            machineParts.UnitId = AllUnit[j];
                                            machineParts.ExpiredDate = allPartsRepDate[j];
                                            machineParts.ReplaceDate = now.AddDays(Convert.ToInt32(allPartsRepDate[j]));
                                            machineParts.Quantity =Convert.ToDecimal(allPartsQuan[j]);
                                            machineParts.CreatedBy = model.CreatedBy;
                                            machineParts.CreatedDate = now;
                                            machineParts.IsInstalled = true;
                                            db.MachinePartsLists.Add(machineParts);
                                            db.SaveChanges();
                                        }
                                        else
                                        {
                                            int quan = Convert.ToInt32(allPartsQuan[j]);
                                            for (int k = 1; k <= quan; k++)
                                            {
                                                MachinePartsList machineParts = new MachinePartsList();
                                                machineParts.MachineId = machineId;
                                                machineParts.PartsId = inven_id;
                                                machineParts.UnitId = AllUnit[j];
                                                machineParts.ExpiredDate = allPartsRepDate[j];
                                                machineParts.ReplaceDate = now.AddDays(Convert.ToInt32(allPartsRepDate[j]));
                                                machineParts.Quantity = quan;
                                                machineParts.CreatedBy = model.CreatedBy;
                                                machineParts.CreatedDate = now;
                                                machineParts.IsInstalled = true;
                                                db.MachinePartsLists.Add(machineParts);
                                                db.SaveChanges();
                                            }
                                        }
                                        
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            string errmsg = ex.Message.ToString();
                            msg = "Machine " + i + "  creation unsuccessful.";
                            OperationStatus = -1;
                        }
                       SaveAuditLog("Define Machine Name", msg, "Inventory", "Define Machine Name", "MachineLists", ColumnId, model.CreatedBy, now, OperationStatus);
                    }
                    return Json(ColumnId, JsonRequestBehavior.AllowGet);
                }
            }
            return Json("Error",JsonRequestBehavior.AllowGet);
        }
        public JsonResult AddMachine(int id,int quan,long userId,string[] allMachineName)
        {
            if (id > 0 && quan > 0)
            {
                int Status = -1;
                string mesage = "";
                Machine machi = db.Machines.Find(id);
                machi.Quantity = Convert.ToInt32(machi.Quantity) + quan;
                db.Entry(machi).State = EntityState.Modified;
                try
                {
                    db.SaveChanges();
                    Status = 1;
                    mesage = quan + " new " + machi.Name + " machines has been successfully added.";
                }
                catch (Exception)
                {
                    Status = -1;
                    mesage = quan + " new " + machi.Name + " machines addition was unsuccessful.";
                }
                SaveAuditLog("Add New Machine", mesage, "Inventory", "Add New Machine", "MachineLists", id, userId, now, Status);
                if (Status == 1)
                {
                    for (int i = 0; i < quan; i++)
                    {
                        string msg = "";
                        int machineId = 0;
                        int OperationStatus = -1;
                        MachineList machines = new MachineList();
                        machines.MachineTypeId = id;
                        machines.MachineAcronym = allMachineName[i];
                        machines.Status = 0;
                        machines.AssignStatus = false;
                        machines.CreatedDate = now;
                        machines.CreatedBy = userId;

                        try
                        {
                            db.MachineLists.Add(machines);
                            db.SaveChanges();
                            msg = "Machine " + i + " successfully created.";
                            machineId = db.MachineLists.Where(o => o.CreatedBy == userId).Max(o => o.MachineId);

                            OperationStatus = 1;
                        }
                        catch (Exception)
                        {
                            msg = "Machine " + i + "  creation unsuccessful.";
                            OperationStatus = -1;
                        }
                        SaveAuditLog("Add New Machine", msg, "Inventory", "Add New Machine", "MachineLists", id, userId, now, OperationStatus);
                    }
                }
                return Json(id, JsonRequestBehavior.AllowGet);
            }
            return Json("Error",JsonRequestBehavior.AllowGet);
        }
        public PartialViewResult _EditMachine(int Id)
        {
            MachineTypeModelView model = (from m in db.Machines
                                          where m.Id == Id
                                          join b in db.BrandLists
                                          on m.BrandId equals b.BrandId
                                          join c in db.CountryLists on m.CountryOfOrigin equals c.CountryCode
                                          select new MachineTypeModelView
                                          {
                                              Id = m.Id,
                                              Acroynm = m.Acroynm,
                                              MachineType = m.MachineType,
                                              BrandId = m.BrandId,
                                              BrandName = b.BrandName,
                                              ModelName = m.ModelName,
                                              Mgf = m.Mgf,
                                              CountryOfOrigin = m.CountryOfOrigin,
                                              CountryName = c.CountryName
                                          }).FirstOrDefault();
            ViewBag.BrandId = new SelectList(db.BrandLists, "BrandId", "BrandName", model.BrandId);
            ViewBag.CountryId = new SelectList(db.CountryLists, "CountryCode", "CountryName", model.CountryOfOrigin);
            ViewBag.MachineType = new SelectList(db.MachineTypeNames.OrderBy(m => m.MachineTypeName1), "MachineId", "MachineTypeName1", model.MachineType);
            return PartialView(model);
        }
        public JsonResult UpdateMachineType(MachineTypeModelView model)
        {
            if (model != null)
            {
                int status = -1;
                string msg = "";
                Machine machine = db.Machines.Find(model.Id);
                machine.MachineType = model.MachineType;
                machine.Name = model.Name;
                machine.Acroynm = model.Acroynm;
                machine.ModelName = model.ModelName;
                machine.BrandId = model.BrandId;
                machine.CountryOfOrigin = model.CountryOfOrigin;
                machine.UpdatedDate = now;
                machine.UpdatedBy = model.CreatedBy;
                db.Entry(machine).State = EntityState.Modified;
                try
                {
                    db.SaveChanges();
                    status = 1;
                    msg = "Machine '"+model.Name+"'s ' basic information has been successfully updated. ";
                }
                catch (Exception )
                {
                    status = -1;
                    msg = "Machine '" + model.Name + "'s ' basic information update was unsuccessful. ";
                }
                SaveAuditLog("Edit Machine", msg, "Inventory", "All Machine Lists", "Machine", model.Id, model.CreatedBy, now, status);
                if (status == 1)
                {
                    return Json("Success",JsonRequestBehavior.AllowGet);
                }
            }
            return Json("Error",JsonRequestBehavior.AllowGet);
        }
        public JsonResult DeleteMachine(int id,long userId,string name)
        {
            if (id > 0)
            {
                int status = -1;
                string msg = "";
                MachineList machine = db.MachineLists.Find(id);
                if (machine != null)
                {
                    if (db.MachinePartsLists.Where(m => m.MachineId == machine.MachineId).Any())
                    {
                        var machin_parts = db.MachinePartsLists.Where(m => m.MachineId == machine.MachineId).ToList();
                        foreach (var list in machin_parts)
                        {
                            db.MachinePartsLists.Remove(list);
                            db.SaveChanges();
                        }
                    }
                    Machine machintype = db.Machines.Find(machine.MachineTypeId);
                    machintype.Quantity = machintype.Quantity - 1;
                    db.Entry(machintype).State = EntityState.Modified;
                    db.MachineLists.Remove(machine);
                    try
                    {
                        db.SaveChanges();
                        status = 1;
                        msg = "Equipment '" + name + "' has been successfully deleted.";
                    }
                    catch (Exception)
                    {
                        msg = "Equipment '" + name + "' deletion was unsuccessful.";
                        status = -1;  
                    }
                    SaveAuditLog("Delete Equipment", msg, "Inventory", "AllEquipmentList", "MachineLists", id, userId, now, status);
                    if (status == 1)
                    {
                        return Json("Success", JsonRequestBehavior.AllowGet);
                    }
                }
            }
            return Json("Error",JsonRequestBehavior.AllowGet);
        }
        public ActionResult MachineTypeDestroy(int? MachineId, long UserId)
        {
            if (MachineId != null)
            {
                string msg = "";
                string name = "";
                int ColumnId = 0;
                int OperationStatus = 1;
                string value = "Success";
                MachineTypeName pro_type = db.MachineTypeNames.Find(MachineId);
                name = pro_type.MachineTypeName1;
                ColumnId = pro_type.MachineId;
                try
                {
                    var acq_bnd_id = db.View_Machine_Lists.Where(m => m.MachineType == pro_type.MachineId).Select(m => m.MachineType).FirstOrDefault();

                    if (acq_bnd_id == null || db.Machines.Where(m => m.MachineType == MachineId).Any())
                    {
                        db.MachineTypeNames.Remove(pro_type);
                        db.SaveChanges();
                        OperationStatus = 1;
                        msg = "Machine type '" + name + "' has been successfully deleted on " + now.ToString("MMM dd , yyy hh:mm tt") + " .";
                    }
                    else
                    {
                        msg = "Machine type " + name + " delete attempt unsuccessful on " + now.ToString("MMM dd , yyy hh:mm tt") + " .";
                        value = "Machine '" + name + "' has been used.";
                        OperationStatus = -1;
                    }

                }
                catch (Exception ex)
                {
                    string errorMsg = ex.Message.ToString();
                    msg = "Machine type " + name + " delete attempt unsuccessful on " + now.ToString("MMM dd , yyy hh:mm tt") + " .";
                    OperationStatus = -1;
                    value = "Error";
                }
                SaveAuditLog("Delete Machine Type ", msg, "Inventory", "Machine Type Lists", "MachineTypeName", ColumnId, UserId, now, OperationStatus);
                return Json(value, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json("Error", JsonRequestBehavior.AllowGet);
            }
        }
        public PartialViewResult _AddNewSpareParts(SparePartsListModelView model)
        {
            return PartialView();
        }
        public JsonResult AddNewSpareParts(SparePartsListModelView model)
        {
            SparePartsList parts = new SparePartsList();
            parts.Name = model.PartsName;
            parts.Type = model.Type;
            if (model.Type == 1)
            {
                parts.ExpireDate = model.ExpireDate;
            }
            parts.Description = model.Description;
            parts.CreatedBy = model.CreatedBy;
            parts.CreatedDate = now;

            string msg = "";
            int ColumnId = 0;
            int OperationStatus = 1;
            msg = "New spare part " + model.PartsName + " added.";
            try
            {
                db.SparePartsLists.Add(parts);
                db.SaveChanges();
                int partsId = db.SparePartsLists.Max(m => m.PartsId);
                ColumnId = db.SparePartsLists.Max(m => m.PartsId);
            }
            catch (Exception ex)
            {
                string errorMsg = ex.Message.ToString();
                msg = "New spare part " + model.PartsName + " addition unsuccessful";
                OperationStatus = -1;
                ColumnId = 0;
            }
            SaveAuditLog("Add Spare Parts", msg, "Inventory", "Add Spare Parts","SparePartsLists", ColumnId, model.CreatedBy, now, OperationStatus);
            if (OperationStatus == 1)
            {
                return Json(ColumnId, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json("Error", JsonRequestBehavior.AllowGet);
            }
            
        }
        public PartialViewResult DefineMachineDetails(int? id,int quan,bool isAdd)
        {
            MachineListModelView model = new MachineListModelView();
            if (id != null)
            {
                Machine mdl = db.Machines.Where(m => m.Id == id).FirstOrDefault();
                model.MachineAcronym = mdl.Acroynm;
                model.MachineTypeId = Convert.ToInt32(id);
                model.Quantity = quan;
                model.Pre_Quantity = mdl.Quantity;
                ViewBag.Acronym = mdl.Acroynm;  
            }
            ViewBag.isAdd = isAdd;
            return PartialView(model);
        }
        public PartialViewResult _ConfirmPartsQuantity() {
           
            return PartialView();
        }
        public PartialViewResult _ReplacableDate(int Quantity) {
            ViewBag.quantity = Quantity;
            return PartialView();
        }
        public PartialViewResult _ShowPreMachineName(string Acronym,int? Id)
        {
            if (Id != null)
            {
                var partList = db.View_Machine_Parts.Where(m => m.MachineTypeId == Id).ToList();
                return PartialView(partList);
            }
            var All_Id = db.Machines.Where(m => m.Acroynm == Acronym).Select(m => m.Id).ToList();
            return PartialView(db.MachineLists.Where(m => All_Id.Contains(m.MachineTypeId)).OrderBy(m => m.MachineAcronym).ToList());
        }
        public JsonResult SaveMachineName(MachineListModelView model,int AllPartsSame, string[] AllMachineDetails, int[] AllSpareId, string[] allPartsQuan, string[] allPartsRepDate)
        {
            if (AllMachineDetails != null)
            {
                string msg = "";
                int ColumnId = 0;
                int OperationStatus = 1;

                var allMachineList = db.MachineLists.Where(m => m.MachineTypeId == model.MachineTypeId).ToList();
                int count = 0;
                foreach (var machine in allMachineList)
                {
                    machine.MachineTypeId = model.MachineTypeId;
                    machine.MachineAcronym = AllMachineDetails[count];
                    machine.AssignStatus = false;
                    machine.UpdatedDate = now;
                    machine.UpdatedBy = model.CreatedBy;
                    try
                    {
                        db.Entry(machine).State = EntityState.Modified;
                        db.SaveChanges();
                        msg = AllMachineDetails[count] + " machine name save successfully.";
                        int partsId = db.MachineLists.Max(m => m.MachineId);
                        ColumnId = db.Machines.Max(m => m.Id);
                        OperationStatus = 1;
                    }
                    catch (Exception ex)
                    {
                        string errmsg = ex.Message.ToString();
                        msg = AllMachineDetails[count] + " machine name save unsuccessful.";
                        OperationStatus = -1;
                    }
                    //if (model.AllPartsSame == 1)
                    //{
                    //    if (AllSpareId.Length > 0)
                    //    {
                    //        for (int j = 0; j < AllSpareId.Length; j++)
                    //        {
                    //            int inven_id = AllSpareId[j];
                    //            if (Convert.ToBoolean(AllId[j]))
                    //            {
                    //                MachinePartsList machineParts = new MachinePartsList();
                    //                machineParts.MachineId = machineId;
                    //                machineParts.PartsId = inven_id;
                    //                machineParts.UnitId = AllUnit[j];
                    //                machineParts.ExpiredDate = allPartsRepDate[j];
                    //                machineParts.ReplaceDate = now.AddDays(Convert.ToInt32(allPartsRepDate[j]));
                    //                machineParts.Quantity = Convert.ToDecimal(allPartsQuan[j]);
                    //                machineParts.CreatedBy = model.CreatedBy;
                    //                machineParts.CreatedDate = now;
                    //                machineParts.IsInstalled = true;
                    //                db.MachinePartsLists.Add(machineParts);
                    //                db.SaveChanges();
                    //            }
                    //            else
                    //            {
                    //                int quan = Convert.ToInt32(allPartsQuan[j]);
                    //                for (int k = 1; k <= quan; k++)
                    //                {
                    //                    MachinePartsList machineParts = new MachinePartsList();
                    //                    machineParts.MachineId = machineId;
                    //                    machineParts.PartsId = inven_id;
                    //                    machineParts.UnitId = AllUnit[j];
                    //                    machineParts.ExpiredDate = allPartsRepDate[j];
                    //                    machineParts.ReplaceDate = now.AddDays(Convert.ToInt32(allPartsRepDate[j]));
                    //                    machineParts.Quantity = quan;
                    //                    machineParts.CreatedBy = model.CreatedBy;
                    //                    machineParts.CreatedDate = now;
                    //                    machineParts.IsInstalled = true;
                    //                    db.MachinePartsLists.Add(machineParts);
                    //                    db.SaveChanges();
                    //                }
                    //            }

                    //        }
                    //    }
                    //}
                    SaveAuditLog("Define Machine Name", msg, "Inventory", "Define Machine Name", "MachineLists", ColumnId, model.CreatedBy, now, OperationStatus);
                    count++;
                }
            }
            return Json("Success",JsonRequestBehavior.AllowGet);
        }
        public ActionResult MaintainenceShedule()
        {
            return View();
        }
        public JsonResult ReplaceableDate(int partId)
        {
            if (db.SparePartsLists.Where(m => m.Type == 1 && m.PartsId == partId).Any())
            {
                int expiredDate = Convert.ToInt32(db.SparePartsLists.Where(m => m.Type == 1 && m.PartsId == partId).Select(m => m.ExpireDate).FirstOrDefault());
                return Json(expiredDate,JsonRequestBehavior.AllowGet);
            }
            return Json("No replaceable",JsonRequestBehavior.AllowGet);
        }
        [EncryptedActionParameter]
        public ActionResult MachineDetails(int? machineId)
        {
            if (machineId == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            View_Machine_Lists model = db.View_Machine_Lists.Where(m => m.MachineId == machineId).FirstOrDefault();
            return View(model);
        }
        public ActionResult MachineBasicInfo(int? machineId)
        {
            if (machineId == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            MachineTypeModelView model = (from m in db.View_Machine_Lists
                                           where m.MachineId == machineId
                                           select new MachineTypeModelView
                                           {
                                               Name = m.Acroynm,
                                               MachineAcronym = m.MachineAcronym,
                                               MachineTypeName = m.MachineTypeName,
                                               ModelName = m.ModelName,
                                               CountryOfOrigin = m.CountryName
                                           }).FirstOrDefault();

            return PartialView(model);
        }
        public ActionResult MachinePartsList(int? machineId)
        {
            if (machineId == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var allPartsList = (from m in db.MachinePartsLists
                                join p in db.InventoryLists on m.PartsId equals p.InventoryId
                                where m.MachineId == machineId && m.IsInstalled == true
                                select new SparePartsListModelView
                                {
                                     Id = m.Id,
                                     PartsName = p.ProductName,
                                     PartsId = p.InventoryId,
                                     TypeName = (p.CanbeReplaceable) ? "Replaceable" : (p.CanbeReplaceable)? "Perishable":(p.CanbeTopup)? "Top Up":"",
                                     Quantity = m.Quantity,
                                     Unitname = (p.UnitId > 0)?(db.UnitLists.Where(u => u.UnitId == m.UnitId).Select(u => u.UnitName).FirstOrDefault()):""
                                }).GroupBy(g => g.PartsId).Select(grp => grp.FirstOrDefault()).OrderBy(o => o.PartsName).ToList();

            ViewBag.allPartsList = allPartsList;
            return PartialView();
        }
        public ActionResult NotInstalledParts(int? machineId)
        {
            if (machineId == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var allPartsList = (from m in db.MachinePartsNotInstalledLists
                                join p in db.InventoryLists on m.PartsId equals p.InventoryId
                                where m.MachineId == machineId
                                select new SparePartsListModelView
                                {
                                    Id = m.Id,
                                    PartsName = p.ProductName,
                                    PartsId = p.InventoryId,
                                    TypeName = (p.CanbeReplaceable) ? "Replaceable" : (p.CanbeReplaceable) ? "Perishable" : (p.CanbeTopup) ? "Top Up" : "",
                                    NotInsQuan = m.NotInstalledQuan,
                                    Unitname = (p.UnitId > 0) ? (db.UnitLists.Where(u => u.UnitId == m.UnitId).Select(u => u.UnitName).FirstOrDefault()) : ""
                                }).OrderBy(o => o.PartsName).ToList();
            allPartsList = (from g in allPartsList
                    group g by g.PartsId into m
                    select new SparePartsListModelView
                    {
                            Id = m.FirstOrDefault().Id,
                            PartsName = m.FirstOrDefault().PartsName,
                            PartsId = m.FirstOrDefault().PartsId,
                            TypeName = m.FirstOrDefault().TypeName,
                            NotInsQuan = m.Sum(x => x.NotInsQuan),
                            Unitname = m.FirstOrDefault().Unitname
                    }).OrderBy(o => o.PartsName).ToList();
            return PartialView(allPartsList);
        }
        public PartialViewResult _SetUpMahcineParts(int id, int? machineId)
        {
            var allPartsList = (from m in db.MachinePartsNotInstalledLists
                                join p in db.View_All_InventoryList
                                on m.PartsId equals p.InventoryId
                                join u in db.UnitLists on m.UnitId equals u.UnitId
                                where m.Id == id && m.MachineId == machineId
                                select new SparePartsListModelView
                                {
                                    Id = m.Id,
                                    PartsName = p.ProductName,
                                    PartsId = p.InventoryId,
                                    NotInsQuan = m.NotInstalledQuan,
                                    TypeName = (p.CanbeReplaceable) ? "Replaceable" : (p.CanbeReplaceable) ? "Perishable" : (p.CanbeTopup) ? "Top Up" : "",
                                    Unitname = u.UnitName,
                                   
                                }).FirstOrDefault();
            return PartialView(allPartsList);
        }
        public JsonResult SparePatsInstalled(int id,int date,long userId,string name)
        {
            if (id > 0)
            {
                string msg = "";
                int ColumnId = 0;
                int status = 1;

                MachinePartsList machineParts = db.MachinePartsLists.Find(id);
                machineParts.ExpiredDate = date.ToString();
                machineParts.ReplaceDate = now.AddDays(Convert.ToInt32(date));
                machineParts.UpdatedBy = userId;
                machineParts.UpdatedDate = now;
                machineParts.IsInstalled = true;
                db.Entry(machineParts).State = EntityState.Modified;
                try
                {
                    db.SaveChanges();
                    msg = "Machine parts " + name + " has been successfully installed.";
                }
                catch (Exception)
                {
                    status = -1;
                    msg = "Machine parts " + name + " installation unsuccessful.";
                    throw;
                }
                SaveAuditLog("Mahcine Parts Setup ", msg, "Inventory", "Macine Details", "MachineLists", ColumnId, userId, now, status);
                if (status == 1)
                {
                    return Json("Success", JsonRequestBehavior.AllowGet);
                } 
            }
            return Json("Error",JsonRequestBehavior.AllowGet);
        }
        public PartialViewResult _MachinePartsBackToStore(int id, string unitname,string proname,int machineId)
        {
            var model = db.MachinePartsNotInstalledLists.Find(id);
            double total_quan = db.MachinePartsNotInstalledLists.Where(m => m.PartsId == model.PartsId && m.MachineId == model.MachineId).Sum(m => m.NotInstalledQuan);
            ViewBag.total_quan = total_quan;
            ViewBag.proname = proname;
            ViewBag.unitname = unitname;
            ViewBag.machineId = machineId;
            ViewBag.ReasonId = new SelectList(db.TransactionEditReasons, "Id", "Reason");
            return PartialView(model);
        }
        public ActionResult GetVouchersForReturn(int machineId,int partsId) {
            var vouchers = db.MachinePartsNotInstalledLists.Where(x => x.MachineId == machineId && x.VoucherId != null && x.PartsId == partsId)
                .Select(x => new {
                    VoucherName = db.StoreItemDispatchedHistories.Where(c => c.Id == x.VoucherId).Select(c => c.VoucherName).FirstOrDefault(),
                    AvailableQuantity=x.NotInstalledQuan+"-"+x.VoucherId
                }).ToList();
            
            return Json(vouchers,JsonRequestBehavior.AllowGet);
        }
        public JsonResult SparePartsRestore(int id, double quan, int invenId, int assignedId, string proName, string unitName, long userId , int reasonId,int voucherId)
        {
            double total_quan = quan;
            if (quan > 0 && invenId > 0 && assignedId > 0)
            {
                string msg = "";
                int status = 1;
                MachinePartsNotInstalledList machine = db.MachinePartsNotInstalledLists.Find(id);
                var All_sam_parts = db.MachinePartsNotInstalledLists.Where(m => m.MachineId == machine.MachineId && m.PartsId == machine.PartsId).ToList();
                foreach (var item in All_sam_parts)
                {
                    if (item!= null && quan > 0)
                    {
                        double deleted_quan = 0;
                        if (item.NotInstalledQuan == quan || quan < item.NotInstalledQuan)
                        {
                            deleted_quan = quan;
                        }
                        else
                        {
                            deleted_quan = item.NotInstalledQuan;
                        }

                        DispatchedItemList dis_item = db.DispatchedItemLists.Find(item.RefColId);
                        if (db.DispatchedPartsReturnLists.Where(m => m.MachineId == assignedId && m.InventoryId == invenId && m.DispatchedId == dis_item.DispatchedId).Any())
                        {
                            DispatchedPartsReturnList item_return = db.DispatchedPartsReturnLists.Where(m => m.MachineId == assignedId && m.InventoryId == invenId).FirstOrDefault();
                            item_return.Quantity = (Convert.ToDouble(item_return.Quantity) + Convert.ToDouble(deleted_quan));
                            db.Entry(item_return).State = EntityState.Modified;
                        }
                        else
                        {
                            DispatchedPartsReturnList item_return = new DispatchedPartsReturnList();
                            var lastRtt =(db.DispatchedPartsReturnLists.Any())?db.DispatchedPartsReturnLists.ToList().LastOrDefault():null;
                            var last = (db.DispatchedPartsReturnLists.Any())? Convert.ToInt32(lastRtt.ReturnTransactionName==null?"1": lastRtt.ReturnTransactionName.Split('#')[1]) :1;
                            item_return.ReturnTransactionName = "RTT#" + (last + 1).ToString("000");
                            item_return.DispatchedId = dis_item.DispatchedId;
                            item_return.InventoryId = dis_item.InventoryId;
                            item_return.LocationId = dis_item.LocationId;
                            item_return.Quantity = deleted_quan;
                            item_return.ReturnReasonId = reasonId;
                            item_return.ReturnDate = now;
                            item_return.ReturnBy = userId;
                            item_return.MachineId = assignedId;
                            item_return.Dis_Item_Id = machine.RefColId;
                            db.DispatchedPartsReturnLists.Add(item_return);
                        }
                        try
                        {
                            db.SaveChanges();
                        }
                        catch (Exception)
                        {
                            status = -1;

                        }
                        if (status == 1)
                        {
                            if (item.NotInstalledQuan == quan)
                            {
                                db.MachinePartsNotInstalledLists.Remove(item);
                            }
                            else if (quan > item.NotInstalledQuan)
                            {
                                db.MachinePartsNotInstalledLists.Remove(item);
                            }
                            else
                            {
                                item.NotInstalledQuan = Convert.ToDouble(item.NotInstalledQuan) - quan;
                                db.Entry(item).State = EntityState.Modified;
                            }
                            db.SaveChanges();
                            quan = quan - deleted_quan;
                        }
                    }
                }
                if (quan == 0)
                {
                    msg = "Machine parts '" + proName + "' quantity " + total_quan + " " + unitName + " has been successfully returned on " + now.ToString("MMM dd , yyyy hh:mm tt") + " .";
                    SaveAuditLog("Machine Parts Restore ", msg, "Inventory", "Macine Details", "DispatchedPartsReturnList", id, userId, now, status);
                    return Json("Success", JsonRequestBehavior.AllowGet);
                }
            }
            return Json("Error", JsonRequestBehavior.AllowGet);
        }
        public ActionResult AddPartsWithMachine(int machineId)
        {
            int id = db.ProductTypeLists.Where(m => m.ProductTypeName == "Spare Parts").Select(m => m.ProductTypeId).FirstOrDefault();
            var allPartsList = (from m in db.View_All_InventoryList
                               // where m.ProductTypeId == id
                                select new
                                {
                                    PartsId = m.InventoryId,
                                    Name = m.ProductName,
                                    Canbebreakable = m.CanbeBreakable,
                                    CanbeTopup = m.CanbeTopup,
                                    UnitId = m.UnitId,
                                    UnitName = m.UnitName,
                                }).OrderBy(o => o.Name).ToList();

            var addedPartsId = db.MachinePartsLists.Where(m => m.MachineId == machineId).Select(m => m.PartsId).ToList();
            allPartsList = allPartsList.Where(m => !addedPartsId.Contains(m.PartsId)).ToList();
            ViewBag.allPartsList = allPartsList;
            return PartialView();
        }
        public ActionResult AddPartsWithExsistingMachine(int? MachineId, int[] AllSpareId, string[] allPartsQuan, string[] allPartsRepDate, int[] AllUnit, string[] AllId, long CreatedBy,string machineName)
        {
            if (MachineId == null || AllSpareId.Length == 0)
            {
                return Json("Error", JsonRequestBehavior.AllowGet);
            }
            else
            {
                string msg = "";
                int ColumnId = 0;

                MachineList machine = db.MachineLists.Find(MachineId);
                machine.Status = 1;
                db.Entry(machine).State = EntityState.Modified;
                db.SaveChanges();

                for (int j = 0; j < AllSpareId.Length; j++)
                {
                    int status = -1;
                    int inven_id = AllSpareId[j];
                    string partsname = db.InventoryLists.Where(m => m.InventoryId == inven_id).Select(m => m.ProductName).FirstOrDefault();
                    if (Convert.ToBoolean(AllId[j]))
                    {
                        MachinePartsList machineParts = new MachinePartsList();
                        machineParts.MachineId = Convert.ToInt32(MachineId);
                        machineParts.PartsId = inven_id;
                        machineParts.UnitId = AllUnit[j];
                        machineParts.ExpiredDate = allPartsRepDate[j];
                        machineParts.ReplaceDate = now.AddDays(Convert.ToInt32(allPartsRepDate[j]));
                        machineParts.Quantity = Convert.ToDecimal(allPartsQuan[j]);
                        machineParts.CreatedBy = CreatedBy;
                        machineParts.CreatedDate = now;
                        machineParts.IsInstalled = true;
                        db.MachinePartsLists.Add(machineParts);
                        db.SaveChanges();
                        status = 1;
                    }
                    else
                    {
                        int quan = Convert.ToInt32(allPartsQuan[j]);
                        for (int k = 1; k <= quan; k++)
                        {
                            MachinePartsList machineParts = new MachinePartsList();
                            machineParts.MachineId = Convert.ToInt32(MachineId);
                            machineParts.PartsId = inven_id;
                            machineParts.UnitId = AllUnit[j];
                            machineParts.ExpiredDate = allPartsRepDate[j];
                            machineParts.ReplaceDate = now.AddDays(Convert.ToInt32(allPartsRepDate[j]));
                            machineParts.Quantity = quan;
                            machineParts.CreatedBy = CreatedBy;
                            machineParts.CreatedDate = now;
                            machineParts.IsInstalled = true;
                            db.MachinePartsLists.Add(machineParts);
                            db.SaveChanges();
                        }
                        status = 1;
                    }
                    if(status == 1)
                    {
                        msg = "New parts '" + partsname + "' has been successfully added with machine '" + machineName + "' on " + now.ToString("MMM dd, yyyy hh:mm tt") + " .";
                        ColumnId = db.MachinePartsLists.Max(m => m.Id);
                    }
                    else
                    {
                        msg = "New parts '" + partsname + "' addition with machine '" + machineName + "' was unsuccessful on " + now.ToString("MMM dd, yyyy hh:mm tt") + " .";
                    }
                    SaveAuditLog("New Parts Added ", msg, "Inventory", "Define Machine Name", "MachineLists", ColumnId, CreatedBy, now, status);
                }
                return Json("Success", JsonRequestBehavior.AllowGet);
            }
        }
        public PartialViewResult _MachinePartsDisHis(int id)
        {
            var list = db.View_Dispacth_Item_Details.Where(m => m.MachineId == id).ToList();
            return PartialView(list);
        }
        public PartialViewResult _MachineReturnPartsList(int id)
        {
            var list = db.View_DisItemReturnDetails.Where(m => m.MachineId == id).ToList();
            return PartialView(list);
        }

        //****************************  Machine Maintanence ************************************************************
        #region
        [EncryptedActionParameter]
        public ActionResult MachineMaintanence(int? machineId)
        {
            var MachineTypeId = db.View_Machine_Parts.Select(m => m.MachineTypeId).ToList();
            if (machineId != null)
            {
                MachineTypeId = db.View_Machine_Parts.Where(m => m.MachineId == machineId).Select(m => m.MachineTypeId).ToList();
            }
            ViewBag.MachineTypeId = new SelectList(db.Machines.Where(m => MachineTypeId.Contains(m.Id)).Select(x => new { x.Id, x.Name }), "Id", "Name");
            ViewBag.MachineId = machineId;
            return PartialView();
        }
        public PartialViewResult _MachineMaintanence(int? machineId, int? machineTypeId)
        {
            var allPartsList = db.View_Machine_Parts.OrderBy(o=> o.ReplaceDate).ToList();
            if (machineId != null || machineTypeId != null)
            {
                if (machineId != null && machineTypeId != null)
                {

                }
                else
                {
                    if (machineId != null)
                    {
                        allPartsList = allPartsList.Where(m => m.MachineId == machineId).ToList();
                    }
                    else
                    {
                        var AllMachineId = db.MachineLists.Where(m => m.MachineTypeId == machineTypeId).Select(m => m.MachineId).ToList();
                        allPartsList = allPartsList.Where(m => AllMachineId.Contains(m.MachineId)).ToList();
                    }
                }
            }

            ViewBag.allPartsList = allPartsList.Take(MachineMainPerPage).ToList();
            ViewBag.TotalCount = allPartsList.Count();
            Session["AllMaintanenceMachine"] = allPartsList;
            Session["TotalMaintanenceMachine"] = allPartsList.Count();
            return PartialView();
        }
        public JsonResult GetMoreMachineMain(int? page)
        {
            page = page ?? 0;
            int total = Convert.ToInt16(Session["TotalMaintanenceMachine"]);
            int skip = MachineMainPerPage * Convert.ToInt16(page);
            var canPage = skip < total;
            if (canPage)
            {
                var listBackFromSession = (List<View_Machine_Parts>)Session["AllMaintanenceMachine"];
                var partslist = listBackFromSession.Skip(skip).Take(MachineMainPerPage).ToList();
                return Json(partslist, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json("", JsonRequestBehavior.AllowGet);
            }

        }
        public PartialViewResult _PartsRepDateChange(int id)
        {
            var model = db.View_Machine_Parts.Where(m => m.Id == id).FirstOrDefault();
            return PartialView(model);
        }
        public JsonResult MachineReplaceDateUpdate(MachineTypeModelView model, int PartsId, int repDate, string partName)
        {
            string msg = "";
            int OperationStatus = 1;
            MachinePartsList machinePart = db.MachinePartsLists.Find(PartsId);
            machinePart.ReplaceDate = now.AddDays(repDate);
            machinePart.UpdatedDate = now;
            machinePart.UpdatedBy = model.CreatedBy;
            db.Entry(machinePart).State = EntityState.Modified;
            string name = db.View_Machine_Lists.Where(m => m.MachineId == machinePart.MachineId).Select(m => m.Acroynm).FirstOrDefault();
            try
            {
                db.SaveChanges();
                msg = name + " (" + model.MachineAcronym + ") machine parts " + partName + " replace date changed.";
            }
            catch(Exception)
            {
                OperationStatus = -1;
                msg = name + " (" + model.MachineAcronym + ") machine parts " + partName + " replace date changed unsuccessful.";
            }
            SaveAuditLog("Machine Parts Replace Date Update", msg, "Inventory", "Machine Maintanence", "Machine", PartsId, model.CreatedBy, now, OperationStatus);
            if (OperationStatus == 1)
            {
                MachineMaintenanceLog log = new MachineMaintenanceLog();
                log.MachineId = machinePart.MachineId;
                log.PartsId = PartsId;
                log.Type = 1;
                log.MaintenanceDate = now;
                log.MaintenanceBy = model.CreatedBy;
                log.Msg = "Machine parts maintenance date changed.";
                db.MachineMaintenanceLogs.Add(log);
                db.SaveChanges();
                return Json("Success", JsonRequestBehavior.AllowGet);
            }
            return Json("Error", JsonRequestBehavior.AllowGet);
        }
        public JsonResult SearchMachineName(string prefix)
        {
            if (prefix == "")
            {
                return Json("", JsonRequestBehavior.AllowGet);
            }
            else
            {
                var allUser = (List<View_Machine_Parts>)Session["AllMaintanenceMachine"];
                var list = allUser.Where(m => m.MachineAcronym.ToLower().Contains(prefix.ToLower())).GroupBy(m => m.MachineId)
                           .Select(t => new
                            {
                                MachinId = t.Select(w => w.MachineId).FirstOrDefault(),
                                MachineAcronym = t.Select(w => w.MachineAcronym).FirstOrDefault()
                            }).OrderBy(o => o.MachineAcronym).ToList();
                return Json(list, JsonRequestBehavior.AllowGet);
            }
        }
        public JsonResult FindIsPartsIsAvailable(int machinId, int invenId)
        {
            var exsits = db.MachinePartsNotInstalledLists.Where(m => m.MachineId == machinId && m.PartsId == invenId).Any();
            return Json(exsits,JsonRequestBehavior.AllowGet);
        }
        public PartialViewResult _SetReplaceSpareparts(int machinId, int invenId, string machinname,int id)
        {
            var allPartsList = (from m in db.MachinePartsNotInstalledLists
                                join p in db.View_All_InventoryList
                                on m.PartsId equals p.InventoryId
                                join u in db.UnitLists on m.UnitId equals u.UnitId
                                where m.PartsId == invenId && m.MachineId == machinId
                                select new SparePartsListModelView
                                {
                                    Id = m.Id,
                                    PartsName = p.ProductName,
                                    PartsId = p.InventoryId,
                                    NotInsQuan = db.MachinePartsNotInstalledLists.Where(k => k.MachineId == machinId && k.PartsId == invenId).Select(k => k.NotInstalledQuan).Sum(),
                                    TypeName = (p.CanbeReplaceable) ? "Replaceable" : (p.CanbeReplaceable) ? "Perishable" : (p.CanbeTopup) ? "Top Up" : "",
                                    Type = (p.CanbeReplaceable) ? 1 : (p.CanbeReplaceable) ? 2 : (p.CanbeTopup) ? 3 : 0,
                                    Unitname = u.UnitName,
                                    CanbeBreakable = p.CanbeBreakable,
                                    SubQuantity = p.SubQuantity,
                                    SubUnitId = p.SubUnitId,
                                    SubUnitName = p.SubUnitName
                                }).FirstOrDefault();
            ViewBag.id = id;
            ViewBag.machinId = machinId;
            ViewBag.machinname = machinname;
            return PartialView(allPartsList);
        }
        public JsonResult _MachinePartReplace(int machinId, int invenId, int repDate, long userId, string name, string MachineAcronym,double quan)
        {
            MachinePartsNotInstalledList machine = db.MachinePartsNotInstalledLists.Where(m => m.MachineId == machinId && m.PartsId == invenId).FirstOrDefault();
            if (machine != null)
            {
                string msg = "";
                int status = -1;

                MachinePartsList machinePart = db.MachinePartsLists.Where(m => m.MachineId == machinId && m.PartsId == invenId).FirstOrDefault();
                machinePart.ReplaceDate = now.AddDays(repDate);
                machinePart.UpdatedDate = now;
                machinePart.UpdatedBy = userId;
                db.Entry(machinePart).State = EntityState.Modified;
                try
                {
                    db.SaveChanges();
                    status = 1;
                    msg = "Spare parts '" + name + "' of Machine '"  + MachineAcronym + "' has been successfully replaced on "+now.ToString("MMM dd , yyyy hh:mm tt")+" .";
                }
                catch (Exception)
                {
                    status = -1;
                    msg = "Spare parts '" + name + "' of Machine '" + MachineAcronym + "' replace was unsuccessful on " + now.ToString("MMM dd , yyyy hh:mm tt") + " .";
                }
                SaveAuditLog("Machine Spare Parts Replace", msg, "Inventory", "Machine Maintanence", "MachinePartsList", Convert.ToInt32(machinePart.Id), userId, now, status);
                if (status == 1)
                {
                    machine.NotInstalledQuan = Convert.ToDouble(machine.NotInstalledQuan) - quan;
                    DispatchedItemList dis_item = db.DispatchedItemLists.Find(machine.RefColId);
                    if (dis_item != null)
                    {
                        dis_item.IsAlreadyUsed = true;
                        dis_item.UsedQuan = Convert.ToDouble(dis_item.UsedQuan) + quan;
                        db.Entry(dis_item).State = EntityState.Modified;
                    }

                    //*************************** Edit MachinePartsNotInstalledList After Successfully Replaced Spare Parts ****************************
                    if (machine.NotInstalledQuan == 0)
                    {
                        db.MachinePartsNotInstalledLists.Remove(machine);
                    }
                    else
                    {
                        machine.UpdatedBy = userId;
                        machine.UpdatedDate = now;
                        db.Entry(machine).State = EntityState.Modified;
                    }
                    db.SaveChanges();

                    MachineMaintenanceLog log = new MachineMaintenanceLog();
                    log.MachineId = machinePart.MachineId;
                    log.PartsId = machinePart.PartsId;
                    log.Type = 2;
                    log.MaintenanceDate = now;
                    log.MaintenanceBy = userId;
                    log.Msg = "Machine parts replaced.";
                    db.MachineMaintenanceLogs.Add(log);
                    db.SaveChanges();
                    return Json("Success", JsonRequestBehavior.AllowGet);
                }
            }
            return Json("Error",JsonRequestBehavior.AllowGet);
        }
        #endregion

        #endregion

        #region Machine Shedule Create,Update and Delete
        public ActionResult MachineSchedule()
        {
            return View();
        }
        #endregion

        #region Created,Update and Delete Warehouse
        public ActionResult AllWareHouseLists()
        {
            return View();
        }
        public ActionResult _ShowAllWareHouseList()
        {
            var list = db.View_All_WarehouseLists.GroupBy(m => m.Id).Select(n => n.FirstOrDefault()).ToList();
            return PartialView(list);
        }
        [EncryptedActionParameter]
        public ActionResult AddNewWarehouse(int? Id)
        {
            WarehouseListsModelView model = new WarehouseListsModelView();
            if (Id != null)
            {
                var house = db.View_All_WarehouseLists.Where(m => m.Id == Id).FirstOrDefault();
                model.Id = house.Id;
                model.WarehouseName = house.WarehouseName;
                model.WarehouseAcroynm = house.WarehouseAcroynm;

                model.AllAssignUserId = house.AllAssignUserId;
                if (model.AllAssignUserId != null && model.AllAssignUserId != String.Empty)
                {
                    List<SelectListItem> allpreUserList = new List<SelectListItem>();
                    string[] alluser = model.AllAssignUserId.Split(',');
                    for (int i = 0; i < alluser.Length; i++)
                    {
                        int tid = Convert.ToInt32(alluser[i]);
                        allpreUserList.Add(new SelectListItem { Text = db.View_UserLists.Where(m => m.UserId == tid).Select(m => m.UserName).FirstOrDefault(), Value = tid.ToString() });
                    }
                    ViewBag.displayUser = allpreUserList;
                }
                else
                {
                    ViewBag.displayUser = null;
                }
            }
            else
            {
                ViewBag.displayUser = null;
            }
            List<SelectListItem> allUserList = new SelectList(db.View_UserLists.Where(m => m.Status == 1), "UserId", "UserName").ToList();
            ViewBag.allUserList = allUserList;
            return View(model);
        }
        public JsonResult NewWarehouseSave(WarehouseListsModelView model, long[] AllUserId, string[] AllUnitId)
        {
            if (ModelState.IsValid)
            {
                string oprName = "Add New Store";
                string msg = "";
                int ColumnId = 0;
                int OperationStatus = 1;


                WarehouseList house = new WarehouseList();
                house.WarehouseName = model.WarehouseName;
                house.WarehouseAcroynm = model.WarehouseAcroynm;
                house.CreatedBy = model.CreatedBy;
                house.CreatedDate = now;

                try
                {
                    db.WarehouseLists.Add(house);
                    db.SaveChanges();
                    msg = "New Warehouse " + house.WarehouseName + " added.";
                    int wareId = db.WarehouseLists.Where(m => m.CreatedBy == model.CreatedBy).Max(m => m.Id);

                    for (int i = 0; i < AllUserId.Length; i++)
                    {
                        int status = -1;
                        long uId = AllUserId[i];
                        var userInfo = db.View_UserLists.Where(m => m.UserId == uId).FirstOrDefault();

                        AssignStoreWithUser user = new AssignStoreWithUser();
                        user.UserId = Convert.ToInt32(AllUserId[i]);
                        user.StoreId = 0;
                        user.WareId = wareId;
                        user.AssignedDate = now;
                        user.AssignedBy = model.CreatedBy;
                        db.AssignStoreWithUsers.Add(user);
                        db.SaveChanges();


                        string[] UnitId = AllUnitId[i].Split(',');
                        for (int j = 0; j < UnitId.Length; j++)
                        {
                            UserItemDispatchLoc loc = new UserItemDispatchLoc();
                            loc.UserId = uId;
                            loc.StoreId = 0;
                            loc.WareId = wareId;
                            loc.UnitId = Convert.ToInt32(UnitId[j]);
                            loc.AssignedDate = now;
                            loc.AssignedBy = model.CreatedBy;
                            db.UserItemDispatchLocs.Add(loc);
                            try
                            {
                                db.SaveChanges();
                                status = 1;
                            }
                            catch (Exception)
                            {
                                status = -1;
                            }
                        }
                        msg = "Dispatch location has been successfully set for user '" + userInfo.UserName + "' for warehouse '" + house.WarehouseName + "' on " + now.ToString("MMM dd, yyyy hh:mm tt") + ".";
                        SaveAuditLog("Define Dispatch Location For Warehouse User", msg, "Inventory", "Add New Warehouse", "WarehouseList", wareId, model.CreatedBy, now, status);
                    }

                    ColumnId = wareId;
                }
                catch (Exception)
                {
                    msg = "New warehouse '" + house.WarehouseName + "' addition was unsuccessful";
                    OperationStatus = -1;
                    ColumnId = 0;
                }
                SaveAuditLog(oprName, msg, "Inventory", "Add New Warehouse", "WarehouseList", ColumnId, model.CreatedBy, now, OperationStatus);
                if (OperationStatus == 1)
                {
                    return Json("Success", JsonRequestBehavior.AllowGet);
                }
            }
            return Json("Error", JsonRequestBehavior.AllowGet);
        }
        public JsonResult EditWareData(WarehouseListsModelView model, long[] AllUserId, string[] AllUnitId)
        {
            if (ModelState.IsValid && AllUserId.Length > 0 && AllUnitId.Length > 0)
            {
                string oprName = "Add New Store";
                string msg = "";
                int ColumnId = 0;
                int OperationStatus = 1;

                WarehouseList house = db.WarehouseLists.Find(model.Id);
                house.WarehouseName = model.WarehouseName;
                house.WarehouseAcroynm = model.WarehouseAcroynm;
                house.UpdatedBy = model.CreatedBy;
                house.UpdatedDate = now;
                oprName = "Update Warehouse Info";
                try
                {
                    db.Entry(house).State = EntityState.Modified;
                    db.SaveChanges();

                    var preUserlist = db.AssignStoreWithUsers.Where(m => m.WareId == model.Id).ToList();
                    foreach (var m in preUserlist)
                    {
                        var preUList = db.UserItemDispatchLocs.Where(u => u.WareId == house.Id && u.UserId == m.UserId).ToList();
                        foreach (var unit in preUList)
                        {
                            db.UserItemDispatchLocs.Remove(unit);
                            db.SaveChanges();
                        }
                        db.AssignStoreWithUsers.Remove(m);
                        db.SaveChanges();
                    }
                    for (int i = 0; i < AllUserId.Length; i++)
                    {
                        AssignStoreWithUser user = new AssignStoreWithUser();
                        user.UserId = Convert.ToInt32(AllUserId[i]);
                        user.StoreId = 0;
                        user.WareId = house.Id;
                        user.AssignedDate = now;
                        user.AssignedBy = model.CreatedBy;
                        db.AssignStoreWithUsers.Add(user);
                        db.SaveChanges();


                        int status = -1;
                        long uId = AllUserId[i];
                        var userInfo = db.View_UserLists.Where(m => m.UserId == uId).FirstOrDefault();
                        string[] UnitId = AllUnitId[i].Split(',');
                        for (int j = 0; j < UnitId.Length; j++)
                        {
                            UserItemDispatchLoc loc = new UserItemDispatchLoc();
                            loc.UserId = uId;
                            loc.StoreId = 0;
                            loc.WareId = house.Id;
                            loc.UnitId = Convert.ToInt32(UnitId[j]);
                            loc.AssignedDate = now;
                            loc.AssignedBy = model.CreatedBy;
                            db.UserItemDispatchLocs.Add(loc);
                            try
                            {
                                db.SaveChanges();
                                status = 1;
                            }
                            catch (Exception)
                            {
                                status = -1;
                            }
                        }
                        msg = "Dispatch location has been successfully set for user '" + userInfo.UserName + "' for warehouse '" + house.WarehouseName + "' on " + now.ToString("MMM dd, yyyy hh:mm tt") + ".";
                        SaveAuditLog("Define Dispatch Location For Warehouse User", msg, "Inventory", "Add New Warehouse", "WarehouseList", house.Id, model.CreatedBy, now, status);
                    }
                    ColumnId = model.Id;
                    msg = "Warehouse  '" + house.WarehouseName + "'  information has been successfully updated.";
                }
                catch (Exception)
                {
                    msg = "Warehouse  '" + house.WarehouseName + "'  information update was unsuccessful.";
                    OperationStatus = -1;
                    ColumnId = 0;
                }
                SaveAuditLog(oprName, msg, "Inventory", "Add New Warehouse", "WarehouseList", ColumnId, model.CreatedBy, now, OperationStatus);
                if (OperationStatus == 1)
                {
                    return Json("Success", JsonRequestBehavior.AllowGet);
                }
            }
            return Json("Error", JsonRequestBehavior.AllowGet);
        }

        public JsonResult DeleteWarehouse(int? houseId, long UserId)
        {
            string msg = "";
            string name = "";
            string value = "";
            int ColumnId = 0;
            int OperationStatus = 1;

            if (houseId == null)
            {
                return Json("Error", JsonRequestBehavior.AllowGet);
            }
            else
            {
                WarehouseList house = db.WarehouseLists.Find(houseId);
                if (house != null)
                {
                    if (db.Inventory_Item_Location.Where(m => m.WarehouseId == houseId).Any())
                    {
                        return Json("Warehouse can not be deleted because it is in use.", JsonRequestBehavior.AllowGet);
                    }

                    var sitelist = db.SiteWarehouseLists.Where(m => m.WarehouseId == houseId).ToList();
                    var AllUser = db.AssignStoreWithUsers.Where(m => m.WareId == houseId).ToList();

                    name = house.WarehouseName;
                    ColumnId = house.Id;
                    try
                    {
                        msg = name + " deleted successfully";
                        db.WarehouseLists.Remove(house);
                        db.SaveChanges();
                        foreach (var m in sitelist)
                        {
                            db.SiteWarehouseLists.Remove(m);
                            db.SaveChanges();
                        }
                        foreach (var user in AllUser)
                        {
                            var allLoc = db.UserItemDispatchLocs.Where(m => m.UserId == user.UserId && m.WareId == m.WareId).ToList();
                            foreach (var loc in allLoc)
                            {
                                db.UserItemDispatchLocs.Remove(loc);
                                db.SaveChanges();
                            }
                            db.AssignStoreWithUsers.Remove(user);
                            db.SaveChanges();
                        }


                        OperationStatus = 1;
                        value = "Success";
                    }
                    catch (Exception)
                    {
                        msg = name + " deletion unsuccessful";
                        OperationStatus = -1;
                        value = "Error";
                    }
                }
                SaveAuditLog("Delete", msg, "Inventory", "Delete Warehouse", "WarehouseLists", ColumnId, UserId, now, OperationStatus);
                return Json(value, JsonRequestBehavior.AllowGet);
            }
        }
        public JsonResult GetTitlesofWarehouse(string prefix)
        {
            if (prefix == "")
            {
                return Json("", JsonRequestBehavior.AllowGet);
            }
            else
            {
                var house = (List<WarehouseListsModelView>)Session["AllWarehouses"];
                var title = (from w in house
                             where (w.WarehouseName.ToLower().Contains(prefix.ToLower())
                             || w.WarehouseAcroynm.ToLower().Contains(prefix.ToLower()))
                             select new WarehouseListsModelView
                             {
                                 Id = w.Id,
                                 WarehouseName = w.WarehouseName,
                                 WarehouseAcroynm = w.WarehouseAcroynm,
                                 AllSiteLists = w.AllSiteLists,
                                 CreatedBy = w.CreatedBy,
                                 Username = w.Username,
                                 Picture = w.Picture,
                                 CreatedDate = w.CreatedDate,
                                 CreatedDateName = w.CreatedDateName
                             }).ToList();
                Session["AllWarehousesBySearch"] = title;
                return Json(title, JsonRequestBehavior.AllowGet);
            }
        }
        #endregion

        //*********************************************************************************************
        //*******************************************  WARE-HOUSE **************************************
        //*********************************************************************************************

        #region
        [EncryptedActionParameter]
        public ActionResult WareHouse(int? wareid)
        {
            if (wareid == null)
            {
                ViewBag.wareid = 0;
            }
            else
            {
                ViewBag.wareid = wareid;
                ViewBag.warename = db.WarehouseLists.Where(m => m.Id == wareid).Select(m => m.WarehouseAcroynm).FirstOrDefault();
            }
            return View();
        }
        public PartialViewResult _WareProList(int? inventoryId, int? productId, int? wareid)
        {
            if (inventoryId != null)
            {
                var allProduct = (from i in db.View_All_InventoryList
                                  where i.InventoryId == inventoryId
                                  select new InventoryListModelView
                                  {
                                      InventoryId = i.InventoryId,
                                      ProductName = i.ProductName,
                                      ProductTypeId = i.ProductTypeId,
                                      Quantity = i.Quantity,
                                      Brand = i.BrandName,
                                      ModelName = i.Model,
                                      Price = i.Price,
                                      Withunit = i.Quantity + " " + i.UnitName,
                                      SubQuantity = i.SubQuantity,
                                      SubUnits = i.SubUnitId,
                                      UnitName = i.UnitName,
                                      SubUnitName = i.SubUnitName,
                                      CanbeBreakable = i.CanbeBreakable,
                                      ReservedQuantity = i.ReservedQuantity,
                                      Available_Quantity = i.Available_Quantity,
                                      InMultipleLocation = i.InMultipleLocation,
                                      LocationId = i.LocationId,
                                      Description = i.Description,
                                      CountryName = i.CountryName,
                                      ProductImageName = db.Inventory_Image_Lists.Where(m => m.InventoryId == i.InventoryId).Select(m => m.ImageName).FirstOrDefault()
                                  }).OrderBy(m => m.ProductName).ToList();
                ViewBag.Count = 1;
                return PartialView(allProduct.Take(WareProPerPage).ToList());
            }
            else if (productId != null)
            {
                if (db.ProductTypeLists.Where(m => m.ParentId == productId).Any())
                {
                    var childId = db.ProductTypeLists.Where(m => m.ParentId == productId).Select(m => m.ProductTypeId).Distinct();
                    var allProduct = (from i in db.View_All_InventoryList
                                      where childId.Contains(i.ProductTypeId)
                                      select new InventoryListModelView
                                      {
                                          InventoryId = i.InventoryId,
                                          ProductName = i.ProductName,
                                          ProductTypeId = i.ProductTypeId,
                                          Quantity = i.Quantity,
                                          Brand = i.BrandName,
                                          ModelName = i.Model,
                                          Price = i.Price,
                                          Withunit = i.Quantity + " " + i.UnitName,
                                          SubQuantity = i.SubQuantity,
                                          SubUnits = i.SubUnitId,
                                          UnitName = i.UnitName,
                                          SubUnitName = i.SubUnitName,
                                          CanbeBreakable = i.CanbeBreakable,
                                          ReservedQuantity = i.ReservedQuantity,
                                          Available_Quantity = i.Available_Quantity,
                                          InMultipleLocation = i.InMultipleLocation,
                                          LocationId = i.LocationId,
                                          Description = i.Description,
                                          CountryName = i.CountryName,
                                          ProductImageName = db.Inventory_Image_Lists.Where(m => m.InventoryId == i.InventoryId).Select(m => m.ImageName).FirstOrDefault()
                                      }).OrderBy(m => m.ProductName).ToList();

                    Session["totalProduct"] = allProduct.Count();
                    Session["Products"] = allProduct;
                    ViewBag.Count = allProduct.Count();
                    return PartialView(allProduct.Take(WareProPerPage).ToList());
                }
                else
                {
                    var allProduct = (from i in db.View_All_InventoryList
                                      where i.ProductTypeId == productId
                                      select new InventoryListModelView
                                      {
                                          InventoryId = i.InventoryId,
                                          ProductName = i.ProductName,
                                          ProductTypeId = i.ProductTypeId,
                                          Quantity = i.Quantity,
                                          Brand = i.BrandName,
                                          ModelName = i.Model,
                                          Price = i.Price,
                                          Withunit = i.Quantity + " " + i.UnitName,
                                          SubQuantity = i.SubQuantity,
                                          SubUnits = i.SubUnitId,
                                          UnitName = i.UnitName,
                                          SubUnitName = i.SubUnitName,
                                          CanbeBreakable = i.CanbeBreakable,
                                          ReservedQuantity = i.ReservedQuantity,
                                          Available_Quantity = i.Available_Quantity,
                                          InMultipleLocation = i.InMultipleLocation,
                                          LocationId = i.LocationId,
                                          Description = i.Description,
                                          CountryName = i.CountryName,
                                          ProductImageName = db.Inventory_Image_Lists.Where(m => m.InventoryId == i.InventoryId).Select(m => m.ImageName).FirstOrDefault()
                                      }).OrderBy(m => m.ProductName).ToList();

                    Session["totalProduct"] = allProduct.Count();
                    Session["Products"] = allProduct;
                    ViewBag.Count = allProduct.Count();
                    return PartialView(allProduct.Take(WareProPerPage).ToList());
                }
            }
            else if (wareid != null)
            {
                var allInventoryId = db.Inventory_Item_Location.Where(m => m.WarehouseId == wareid).Select(m => m.InventoryId).ToList();
                var allProduct = (from i in db.View_All_InventoryList
                                  where allInventoryId.Contains(i.InventoryId)
                                  select new InventoryListModelView
                                  {
                                      InventoryId = i.InventoryId,
                                      ProductName = i.ProductName,
                                      ProductTypeId = i.ProductTypeId,
                                      Quantity = i.Quantity,
                                      Brand = i.BrandName,
                                      ModelName = i.Model,
                                      Price = i.Price,
                                      Withunit = i.Quantity + " " + i.UnitName,
                                      SubQuantity = i.SubQuantity,
                                      SubUnits = i.SubUnitId,
                                      UnitName = i.UnitName,
                                      SubUnitName = i.SubUnitName,
                                      CanbeBreakable = i.CanbeBreakable,
                                      ReservedQuantity = i.ReservedQuantity,
                                      Available_Quantity = i.Available_Quantity,
                                      InMultipleLocation = i.InMultipleLocation,
                                      LocationId = i.LocationId,
                                      Description = i.Description,
                                      CountryName = i.CountryName,
                                      ProductImageName = db.Inventory_Image_Lists.Where(m => m.InventoryId == i.InventoryId).Select(m => m.ImageName).FirstOrDefault()
                                  }).OrderBy(m => m.ProductName).ToList();

                Session["Products"] = allProduct;
                Session["totalProduct"] = allProduct.Count();
                ViewBag.Count = allProduct.Count();
                return PartialView(allProduct.Take(WareProPerPage).ToList());
            }
            else
            {
                var allInventoryId = db.Inventory_Item_Location.Where(m => m.WarehouseId > 0).Select(m => m.InventoryId).ToList();
                var allProduct = (from i in db.View_All_InventoryList
                                  where allInventoryId.Contains(i.InventoryId)
                                  select new InventoryListModelView
                                  {
                                      InventoryId = i.InventoryId,
                                      ProductName = i.ProductName,
                                      ProductTypeId = i.ProductTypeId,
                                      Quantity = i.Quantity,
                                      Brand = i.BrandName,
                                      ModelName = i.Model,
                                      Price = i.Price,
                                      Withunit = i.Quantity + " " + i.UnitName,
                                      SubQuantity = i.SubQuantity,
                                      SubUnits = i.SubUnitId,
                                      UnitName = i.UnitName,
                                      SubUnitName = i.SubUnitName,
                                      CanbeBreakable = i.CanbeBreakable,
                                      ReservedQuantity = i.ReservedQuantity,
                                      Available_Quantity = i.Available_Quantity,
                                      InMultipleLocation = i.InMultipleLocation,
                                      LocationId = i.LocationId,
                                      Description = i.Description,
                                      CountryName = i.CountryName,
                                      ProductImageName = db.Inventory_Image_Lists.Where(m => m.InventoryId == i.InventoryId).Select(m => m.ImageName).FirstOrDefault()
                                  }).OrderBy(m => m.ProductName).ToList();
                Session["Products"] = allProduct;
                Session["totalProduct"] = allProduct.Count();
                ViewBag.Count = allProduct.Count();
                return PartialView(allProduct.Take(WareProPerPage).ToList());
            }
        }
        public JsonResult GetWarePro(int? page)
        {
            page = page ?? 0;
            int total = Convert.ToInt16(Session["totalProduct"]);
            int skip = WareProPerPage * Convert.ToInt16(page);
            var canPage = skip < total;
            if (canPage)
            {
                var listBackFromSession = (List<InventoryListModelView>)Session["Products"];
                var partslist = listBackFromSession.Skip(skip).Take(WareProPerPage).ToList();
                return Json(partslist, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json("", JsonRequestBehavior.AllowGet);
            }
        }  
        public ActionResult AllWareProList()
        {
            var allInventoryId = db.Inventory_Item_Location.Where(m => m.WarehouseId > 0).Select(m => m.InventoryId).ToList();
            var allproductType = (from i in db.View_All_InventoryList
                              where allInventoryId.Contains(i.InventoryId)
                              select new ProductTypeViewModel
                              {
                                  ProductTypeName = i.ProductType,
                                  ProductTypeId = i.ProductTypeId,
                                  Count = db.InventoryLists.Where(coun => coun.ProductTypeId == i.ProductTypeId).Count()
                              }).GroupBy(g => g.ProductTypeId).Select(s => s.FirstOrDefault()).OrderBy(m => m.ProductTypeName).ToList();

            List<ProductTypeViewModel> typeList = new List<ProductTypeViewModel>();
            typeList.Add(new ProductTypeViewModel { text = "All Products", id = "0" });
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
                            text = (model.ProductTypeName),
                            id = model.ProductTypeId + "-p",
                            items = (from c in db.ProductTypeLists
                                     join i in db.InventoryLists on c.ProductTypeId equals i.ProductTypeId
                                     where c.HasParent == true && c.ParentId == model.ProductTypeId
                                     select new ChildClassForSubcategory
                                     {
                                         id = c.ProductTypeId + "-c",
                                         text = c.ProductTypeName,
                                         Count = db.InventoryLists.Where(coun => coun.ProductTypeId == c.ProductTypeId).Count()
                                     }).GroupBy(g => g.text).Select(s => s.FirstOrDefault()).ToList()
                        });
                    }

                }
                else
                {
                    if (!typeList.Where(p => p.ProductTypeId == m.ProductTypeId).Any())
                    {
                        typeList.Add(new ProductTypeViewModel
                        {
                            text = (m.ProductTypeName),
                            id = m.ProductTypeId + "-p",
                            items = (from c in db.ProductTypeLists
                                     join i in db.InventoryLists on c.ProductTypeId equals i.ProductTypeId
                                     where c.HasParent == true && c.ParentId == m.ProductTypeId
                                     select new ChildClassForSubcategory
                                     {
                                         id = c.ProductTypeId + "-c",
                                         text = c.ProductTypeName
                                         //Count = db.InventoryLists.Where(count => count.ProductTypeId == c.ProductTypeId).Count()
                                     }).ToList()
                        });
                    }

                }

            }
            return Json(typeList, JsonRequestBehavior.AllowGet);
        }
        public JsonResult SearchProduct(string prefix, int wareid)
        {
            if (prefix == "")
            {
                return Json("", JsonRequestBehavior.AllowGet);
            }
            else
            {
                if (wareid > 0)
                {
                    var allInventoryId = db.Inventory_Item_Location.Where(m => m.WarehouseId == wareid).Select(m => m.InventoryId).ToList();

                    var allPro = (List<InventoryListModelView>)Session["WareProList"];
                    var list = (from i in db.View_All_InventoryList
                                where (i.ProductName.ToLower().Contains(prefix.ToLower())) 
                                && allInventoryId.Contains(i.InventoryId)
                                select new InventoryListModelView
                                {
                                    InventoryId = i.InventoryId,
                                    ProductName = i.ProductName,
                                    ProductTypeId = i.ProductTypeId,
                                    Quantity = i.Quantity,
                                    Brand = i.BrandName,
                                    ModelName = i.Model,
                                    Price = i.Price,
                                    Withunit = i.Quantity + " " + i.UnitName,
                                    SubQuantity = i.SubQuantity,
                                    SubUnits = i.SubUnitId,
                                    UnitName = i.UnitName,
                                    SubUnitName = i.SubUnitName,
                                    CanbeBreakable = i.CanbeBreakable,
                                    ReservedQuantity = i.ReservedQuantity,
                                    Available_Quantity = i.Available_Quantity,
                                    Description = i.Description,
                                    ProductImageName = db.Inventory_Image_Lists.Where(m => m.InventoryId == i.InventoryId).Select(m => m.ImageName).FirstOrDefault()
                                }).OrderBy(m => m.ProductName).ToList();
                    return Json(list, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    var allInventoryId = db.Inventory_Item_Location.Where(m => m.WarehouseId > 0).Select(m => m.InventoryId).ToList();
                    var allPro = (List<InventoryListModelView>)Session["WareProList"];
                    var list = (from i in db.View_All_InventoryList
                                where (i.ProductName.ToLower().Contains(prefix.ToLower()))
                                && allInventoryId.Contains(i.InventoryId)
                                select new InventoryListModelView
                                {
                                    InventoryId = i.InventoryId,
                                    ProductName = i.ProductName,
                                    ProductTypeId = i.ProductTypeId,
                                    Quantity = i.Quantity,
                                    Brand = i.BrandName,
                                    ModelName = i.Model,
                                    Price = i.Price,
                                    Withunit = i.Quantity + " " + i.UnitName,
                                    SubQuantity = i.SubQuantity,
                                    SubUnits = i.SubUnitId,
                                    UnitName = i.UnitName,
                                    SubUnitName = i.SubUnitName,
                                    CanbeBreakable = i.CanbeBreakable,
                                    ReservedQuantity = i.ReservedQuantity,
                                    Available_Quantity = i.Available_Quantity,
                                    Description = i.Description,
                                    ProductImageName = db.Inventory_Image_Lists.Where(m => m.InventoryId == i.InventoryId).Select(m => m.ImageName).FirstOrDefault()
                                }).OrderBy(m => m.ProductName).ToList();
                    return Json(list, JsonRequestBehavior.AllowGet);
                }    
            }
        }
        public JsonResult GetUserListToAssigncart(int orderId)
        {
            var list = (from m in db.View_UserLists
                        where m.Status == 1
                        select new
                        {
                            UserId = m.UserId,
                            Username = m.UserName,
                            UserType = m.UserTypeName,
                            UserEmpId = m.EmpId,
                            Picture = m.Picture,
                            orderId = orderId
                        }).ToList();
            return Json(list, JsonRequestBehavior.AllowGet);
        }
        public PartialViewResult _ShowVouName()
        {
            return PartialView(db.StoreItemDispatchedHistories.OrderByDescending(m => m.DispatchedDate).ToList());
        }
        public PartialViewResult _PlaceCartItem()
        {
            return PartialView();
        }
        public PartialViewResult _CartItemLocation()
        {
            return PartialView();
        }
        public PartialViewResult _LoadMachineIntoCart(int machineId, string alllocId, string name)
        {
            int[] locId = new int[1000];
            string[] stringLoc = alllocId.Split(',');
            for (int i = 0; i < stringLoc.Length; i++)
            {
                locId[i] = Convert.ToInt32(stringLoc[i]);
            }
            var AllInventory = db.Inventory_Item_Location.Where(m => locId.Contains(m.LocationId)).ToList();
            var AllPartsId = db.MachinePartsLists.Where(m => m.MachineId == machineId).Select(m => m.PartsId).ToList();
            var ExsitLocId = AllInventory.Where(m => AllPartsId.Contains(m.InventoryId)).ToList();
            ViewBag.machineId = machineId;
            ViewBag.ExsitLocId = ExsitLocId;
            ViewBag.alllocId = alllocId;
            ViewBag.name = name;
            return PartialView();
        }
        public JsonResult AllApprovedOrder()
        {
            var list = db.View_Busi_Details_ForClient.Where(m => m.OrderStatus > 0 && m.OrderStatus < 3).ToList();
            return Json(list,JsonRequestBehavior.AllowGet);
        }
        public JsonResult AllMachine()
        {
            return Json(db.Machines.ToList(), JsonRequestBehavior.AllowGet);
        }
        public JsonResult MachineEupi(int? type)
        {
            if (type == null)
            {
                return Json(db.MachineLists.Where(m => m.AssignStatus == true).ToList(), JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(db.MachineLists.Where(m => m.MachineTypeId == type && m.AssignStatus == true).ToList(), JsonRequestBehavior.AllowGet);
            }
            
        }
        public JsonResult AllSiteName()
        {
            long loginUserId = Convert.ToInt32(Request.Cookies["CookieAdminInfo"].Values["userid"].ToString());

            if (loginUserId > 0)
            {
                if (db.UserItemDispatchLocs.Where(m => m.UserId == loginUserId).Any())
                {
                    var AllUnitId = db.UserItemDispatchLocs.Where(m => m.UserId == loginUserId).Select(m => m.UnitId).Distinct().ToList();
                    var AllSiteId = db.Site_Unit_Lists.Where(m => AllUnitId.Contains(m.UnitId)).Select(m => m.SiteId).Distinct().ToList();
                    var list = db.View_SiteLists.Where(m => AllSiteId.Contains(m.Id)).ToList();
                    return Json(list, JsonRequestBehavior.AllowGet);
                }
                return Json("", JsonRequestBehavior.AllowGet);
            }
            else
            {
                var list = db.View_SiteLists.ToList();
                return Json(list, JsonRequestBehavior.AllowGet);
            }
        }
        public JsonResult AllSite_UnitName(int siteId, long UserId, int roleId)
        {
            if (UserId > 0)
            {
                var unitId = db.UserItemDispatchLocs.Where(m => m.StoreId > 0 && m.UserId == UserId).Select(m => m.UnitId).ToList();
                var list = db.Site_Unit_Lists.Where(m => m.SiteId == siteId && unitId.Contains(m.UnitId)).ToList();
                return Json(list, JsonRequestBehavior.AllowGet);
            }
            else
            {
                var list = db.Site_Unit_Lists.Where(m => m.SiteId == siteId).ToList();
                return Json(list, JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult AllSite_DeptName(int[] id ,bool isCom)
        {
            var list = db.View_Dep_Details.Where(m => id.Contains(m.ParentId.Value) && m.UnitId > 0).ToList();
            ViewBag.DeptList = new SelectList(list.OrderBy(f => f.DeptName), "DeptId", "ParentName");
            return PartialView(); 
        }
        public JsonResult AllSite_LineName(int[] Id)
        {
            var list = db.LineInfoes.Where(m => Id.Contains(m.DeptId)).ToList();
            return Json(list, JsonRequestBehavior.AllowGet);
        }
        public ActionResult CartItemAssignedWithOrder(string vname, string[] allOrderId, string[] allSelectedItem, long userId, long[] allAssignUser, bool isOne, bool isStore)
        {
            string msg = ""; int ColumnId = 0; int opStatus = -1;
            int loopCount = 0;
            int lastCountDis = 0;
            if (db.StoreItemDispatchedHistories.Any())
            {
                lastCountDis = db.StoreItemDispatchedHistories.Count();
            }

            //***************************** Created A Transaction For History **********************************
            StoreItemDispatchedHistory dispatchHis = new StoreItemDispatchedHistory();
            dispatchHis.Name = "Dis_#" + ((lastCountDis < 1000) ? lastCountDis.ToString("000") : lastCountDis.ToString());
            dispatchHis.Type = 2;
            dispatchHis.VoucherName = vname;
            dispatchHis.DispatchedBy = userId;
            dispatchHis.DispatchedDate = now;
            dispatchHis.IsStore = isStore;
            db.StoreItemDispatchedHistories.Add(dispatchHis);
            db.SaveChanges();
            int disHisId = db.StoreItemDispatchedHistories.Where(m => m.DispatchedBy == userId).Max(m => m.Id);

            for (int j = 0; j < allOrderId.Length; j++)
            {
                int orderId = 0, count = 0; string strAllUser = "";
                if (isOne)
                {
                    orderId = Convert.ToInt32(allOrderId[j].Split('-')[0]);
                    count = Convert.ToInt32(allOrderId[j].Split('-')[1]);
                }
                else
                {
                    orderId = Convert.ToInt32(allOrderId[j].Split('-')[0]);
                    string countwithid = allOrderId[j].Split('-')[1];
                    count = Convert.ToInt32(countwithid.Split('|')[0]);
                    strAllUser = countwithid.Split('|')[1];
                }
                int acqId = 0;
                if (db.Acquisition_List.Where(m => m.AcquisitionType == 2 && m.BusinessOrderId == orderId).Any())
                {
                    acqId = db.Acquisition_List.Where(m => m.AcquisitionType == 2 && m.BusinessOrderId == orderId).Select(m => m.AcquisitionOrderId).FirstOrDefault();
                }
                else
                {
                    var Bus_detail = db.BusinessOrderLists.Where(m => m.BusinessOrderId == orderId).FirstOrDefault();
                    Acquisition_List acqlist = new Acquisition_List();
                    acqlist.AcquisitionType = 2;
                    acqlist.BusinessOrderId = orderId;
                    acqlist.Name = "Reservation for " + Bus_detail.OrderName;
                    acqlist.CreatedBy = userId;
                    acqlist.CreatedDate = now;
                    acqlist.AcquisitionStatus = 2;
                    acqlist.SiteId = db.UserInformations.Where(m => m.UserId == userId).Select(m => m.SiteId).FirstOrDefault();
                    db.Acquisition_List.Add(acqlist);
                    db.SaveChanges();
                    acqId = db.Acquisition_List.Where(m => m.AcquisitionType == 2 && m.BusinessOrderId == orderId).Select(m => m.AcquisitionOrderId).FirstOrDefault();
                }
                for (int i = 0; i < count; i++)
                {
                    int proid = Convert.ToInt32(allSelectedItem[loopCount].Split('-')[0]);
                    string locwithQuan = allSelectedItem[loopCount].Split('-')[1];
                    int locationid = Convert.ToInt32(locwithQuan.Split(';')[0]);
                    int quan = Convert.ToInt32(locwithQuan.Split(';')[1]);
                    if (quan > 0)
                    {
                        if (db.Acquisitions.Where(m => m.AcquisitionOrderId == acqId && m.ProductId == proid).Any())
                        {
                            Acquisition acq_pro = db.Acquisitions.Where(m => m.AcquisitionOrderId == acqId && m.ProductId == proid).FirstOrDefault();
                            if (Convert.ToInt32(acq_pro.AssignedQuantity) == 0)
                            {
                                acq_pro.Quantity = quan;
                                acq_pro.AssignedQuantity = quan;
                                acq_pro.ReceivedQuantity = quan;
                            }
                            else
                            {
                                acq_pro.Quantity = Convert.ToInt32(acq_pro.Quantity) + quan;
                                acq_pro.AssignedQuantity = Convert.ToInt32(acq_pro.AssignedQuantity) + quan;
                                acq_pro.ReceivedQuantity = Convert.ToInt32(acq_pro.ReceivedQuantity) + quan;
                            }
                            acq_pro.AssignedBy = userId;
                            acq_pro.AssignedDate = now;
                            acq_pro.ReceivedDate = now;
                            if (acq_pro.Quantity == acq_pro.ReceivedQuantity)
                            {
                                acq_pro.OrderStatus = 1;
                            }
                            else
                            {
                                acq_pro.OrderStatus = 0;
                            }
                            //acq_pro.ReceivedBy = assigUserId;
                            db.Entry(acq_pro).State = EntityState.Modified;

                            if (db.Inventory_Reserved_Item_Lists.Where(m => m.AcquisitionOrderId == acqId && m.InventoryId == proid && m.IsAssigned == false && m.LocationId == locationid).Any())
                            {
                                var resvr = db.Inventory_Reserved_Item_Lists
                                    .Where(m => m.AcquisitionOrderId == acqId && m.InventoryId == proid && m.LocationId == locationid &&  m.IsAssigned == false).FirstOrDefault();

                                if (Convert.ToInt32(resvr.Quantity) == Convert.ToInt32(acq_pro.AssignedQuantity) || Convert.ToInt32(resvr.Quantity) < Convert.ToInt32(acq_pro.AssignedQuantity))
                                {
                                    resvr.IsAssigned = true;
                                    db.Entry(resvr).State = EntityState.Modified;
                                }
                            }
                            db.SaveChanges();
                        }
                        else
                        {
                            var pro_details = db.View_All_InventoryList.Where(m => m.InventoryId == proid).FirstOrDefault();
                            Acquisition acq = new Acquisition();
                            acq.AcquisitionOrderId = acqId;
                            acq.ProductId = pro_details.InventoryId;
                            acq.ProductName = pro_details.ProductName;
                            acq.Quantity = quan;
                            acq.AssignedQuantity = quan;
                            acq.ReceivedQuantity = quan;
                            acq.IsSubQuantity = false;
                            acq.UnitId = pro_details.UnitId;
                            acq.IsAsap = true;
                            acq.RequiredDate = now;
                            acq.CreatedBy = userId;
                            acq.AssignedBy = userId;
                            acq.CreatedDate = now;
                            acq.AssignedDate = now;
                            acq.ReceivedDate = now;
                            //acq.ReceivedBy = assigUserId;
                            acq.OrderStatus = 1;
                            db.Acquisitions.Add(acq);
                            db.SaveChanges();
                        }

                        //***************** Decrease Quantity From Inventory ********************
                        var list = db.Inventory_Item_Location.Find(locationid);
                        if (list != null && quan > 0)
                        {
                            int in_quan = Convert.ToInt32(list.Quantity);
                            if (quan == in_quan)
                            {
                                //db.Inventory_Item_Location.Remove(list);
                                list.Quantity = Convert.ToDouble(in_quan - quan);
                                db.Entry(list).State = EntityState.Modified;
                            }
                            else
                            {
                                list.Quantity = Convert.ToDouble(in_quan - quan);
                                db.Entry(list).State = EntityState.Modified;
                            }
                            db.SaveChanges();
                        }
                       

                        //*************************** Check Any Order Reserver This Item ****************************
                        var resrvList = db.Inventory_Reserved_Item_Lists.Where(m => m.IsAssigned == false && m.InventoryId == proid && m.LocationId == locationid).ToList();
                        int left_quan = Convert.ToInt32(db.Inventory_Reserved_Item_Lists.Where(m => m.LocationId == locationid).Select(m => m.Quantity).FirstOrDefault());
                        foreach (var resrv in resrvList)
                        {
                            int new_quan = 0;
                            if (left_quan > Convert.ToInt32(resrv.Quantity) || left_quan == Convert.ToInt32(resrv.Quantity))
                            {
                                left_quan = left_quan - Convert.ToInt32(resrv.Quantity);
                            }
                            else
                            {
                                if (left_quan == 0)
                                {
                                    new_quan = resrv.Quantity;
                                    db.Inventory_Reserved_Item_Lists.Remove(resrv);
                                    db.SaveChanges();
                                }
                                else
                                {
                                    new_quan = Convert.ToInt32(resrv.Quantity) - left_quan;
                                    resrv.Quantity = left_quan;
                                    db.Entry(resrv).State = EntityState.Modified;
                                    db.SaveChanges();
                                    left_quan = 0;
                                }
                                int bus_Id = Convert.ToInt32(db.Acquisition_List.Where(m => m.AcquisitionOrderId == resrv.AcquisitionOrderId).Select(m => m.BusinessOrderId).FirstOrDefault());
                                int bus_acqid = 0;
                                if (db.Acquisition_List.Where(m => m.AcquisitionType == 1 && m.BusinessOrderId == bus_Id).Any())
                                {
                                    bus_acqid = db.Acquisition_List.Where(m => m.AcquisitionType == 1 && m.BusinessOrderId == bus_Id).Select(m => m.AcquisitionOrderId).FirstOrDefault();
                                }
                                else
                                {
                                    var Bus_detail = db.BusinessOrderLists.Where(m => m.BusinessOrderId == bus_Id).FirstOrDefault();
                                    Acquisition_List acqlist = new Acquisition_List();
                                    acqlist.AcquisitionType = 1;
                                    acqlist.BusinessOrderId = bus_Id;
                                    acqlist.Name = "Requisition for " + Bus_detail.OrderName;
                                    acqlist.CreatedBy = userId;
                                    acqlist.CreatedDate = now;
                                    acqlist.AcquisitionStatus = 2;
                                    acqlist.SiteId = db.UserInformations.Where(m => m.UserId == userId).Select(m => m.SiteId).FirstOrDefault();
                                    db.Acquisition_List.Add(acqlist);
                                    db.SaveChanges();
                                    bus_acqid = db.Acquisition_List.Where(m => m.AcquisitionType == 1 && m.BusinessOrderId == bus_Id).Select(m => m.AcquisitionOrderId).FirstOrDefault();
                                }
                                if (db.Acquisitions.Where(m => m.AcquisitionOrderId == bus_acqid && m.ProductId == resrv.InventoryId).Any())
                                {
                                    Acquisition add_acq = db.Acquisitions.Where(m => m.AcquisitionOrderId == bus_acqid && m.ProductId == resrv.InventoryId).FirstOrDefault();
                                    add_acq.Quantity = Convert.ToInt32(add_acq.Quantity) + new_quan;
                                    add_acq.OrderStatus = 0;
                                    db.Entry(add_acq).State = EntityState.Modified;
                                }
                                else
                                {
                                    var pro_details = db.View_All_InventoryList.Where(m => m.InventoryId == resrv.InventoryId).FirstOrDefault();
                                    Acquisition acq = new Acquisition();
                                    acq.AcquisitionOrderId = bus_acqid;
                                    acq.ProductId = resrv.InventoryId;
                                    acq.ProductName = pro_details.ProductName;
                                    acq.Quantity = new_quan;
                                    acq.IsSubQuantity = false;
                                    acq.UnitId = pro_details.UnitId;
                                    acq.IsAsap = true;
                                    acq.RequiredDate = now;
                                    acq.CreatedBy = userId;
                                    acq.AssignedBy = userId;
                                    acq.CreatedDate = now;
                                    acq.AssignedDate = now;
                                    acq.OrderStatus = 0;
                                    db.Acquisitions.Add(acq);
                                }
                                db.SaveChanges();
                            }
                        }
                    }
                    DispatchedItemList dis_Item = new DispatchedItemList();
                    dis_Item.DispatchedId = disHisId;
                    dis_Item.InventoryId = proid;
                    dis_Item.LocationId = locationid;
                    dis_Item.OrderId = orderId;
                    dis_Item.Quantity = quan;
                    dis_Item.IsAlreadyUsed = false;
                    db.DispatchedItemLists.Add(dis_Item);
                    db.SaveChanges();
                    loopCount++;
                }
                if (isOne)
                {
                    for (int u = 0; u < allAssignUser.Length; u++)
                    {
                        long assigUserId = Convert.ToInt64(allAssignUser[u]);
                        if (!db.DispatchedItemAssignUsers.Where(m => m.DispatchId == disHisId && m.UserId == assigUserId).Any())
                        {
                            DispatchedItemAssignUser dis_user = new DispatchedItemAssignUser();
                            dis_user.DispatchId = disHisId;
                            dis_user.UserId = assigUserId;
                            db.DispatchedItemAssignUsers.Add(dis_user);
                            db.SaveChanges();
                        }
                    }
                }
                else
                {
                    string[] allUserId = strAllUser.Split(',');
                    for (int u = 0; u < allUserId.Length; u++)
                    {
                        long assigUserId = Convert.ToInt64(allUserId[u]);
                        if (!db.DispatchedItemAssignUsers.Where(m => m.DispatchId == disHisId && m.UserId == assigUserId && m.OrderId == orderId).Any())
                        {
                            DispatchedItemAssignUser dis_user = new DispatchedItemAssignUser();
                            dis_user.DispatchId = disHisId;
                            dis_user.UserId = assigUserId;
                            dis_user.OrderId = orderId;
                            db.DispatchedItemAssignUsers.Add(dis_user);
                            db.SaveChanges();
                        }
                    }
                }
            }
            return Json("Success", JsonRequestBehavior.AllowGet);
        }
        public ActionResult CartItemAssignedWithMachine(string vname, string[] allMachineId, string[] allSelectedItem, long userId, List<string> allAssignUser, bool isOne, bool isStore)
        {
            int[] AddedLocId = new int[1000]; int addedLocCount = 0;
            string msg = ""; int ColumnId = 0; int opStatus = -1;
            int loopCount = 0; List<string> strAllUser = allAssignUser.ToList();
            int lastCountDis = 0;
            if (db.StoreItemDispatchedHistories.Any())
            {
                lastCountDis = db.StoreItemDispatchedHistories.Count();
            }

            //***************************** Created A Transaction For History *********************************
            StoreItemDispatchedHistory dispatchHis = new StoreItemDispatchedHistory();
            dispatchHis.Name = "Dis_#" + ((lastCountDis < 1000)?lastCountDis.ToString("000"):lastCountDis.ToString());
            dispatchHis.Type = 2;
            dispatchHis.AssignType = 1;
            dispatchHis.VoucherName = vname;
            dispatchHis.DispatchedBy = userId;
            dispatchHis.DispatchedDate = now;
            dispatchHis.IsStore = isStore;
            db.StoreItemDispatchedHistories.Add(dispatchHis);
            db.SaveChanges();
            int disHisId = db.StoreItemDispatchedHistories.Where(m => m.DispatchedBy == userId).Max(m => m.Id);

            for (int j = 0; j < allMachineId.Length; j++)
            {
                int machineId = 0, count = 0; //string strAllUser ="";
                if (isOne)
                {
                    machineId = Convert.ToInt32(allMachineId[j].Split('-')[0]);
                    count = Convert.ToInt32(allMachineId[j].Split('-')[1]);
                }
                else
                {
                    machineId = Convert.ToInt32(allMachineId[j].Split('-')[0]);
                    string countwithid = allMachineId[j].Split('-')[1];
                    count = Convert.ToInt32(countwithid.Split('|')[0]);
                    //  strAllUser = countwithid.Split('|')[1];
                }
                MachineList machinedetails = db.MachineLists.Find(machineId);

                for (int i = 0; i < count; i++)
                {
                    int locationid = Convert.ToInt32(allSelectedItem[loopCount].Split('-')[0]);
                    double quan = Convert.ToDouble(allSelectedItem[loopCount].Split('-')[1]);
                    var inven_loc = db.Inventory_Item_Location.Find(locationid);
                    var proDetails = db.View_All_InventoryList.Where(m => m.InventoryId == inven_loc.InventoryId).FirstOrDefault();
                    List<Inventory_Item_Location> all_inven_loc = new List<Inventory_Item_Location>();

                    if (inven_loc.StoreId > 0)
                    {
                        all_inven_loc = db.Inventory_Item_Location
                        .Where(m => m.InventoryId == inven_loc.InventoryId && m.ImportId == inven_loc.ImportId && m.StoreId > 0).ToList();
                    }
                    else
                    {
                        all_inven_loc = db.Inventory_Item_Location
                        .Where(m => m.InventoryId == inven_loc.InventoryId && m.ImportId == inven_loc.ImportId && m.WarehouseId > 0).ToList();
                    }
                    if (quan > 0)
                    {
                        var line_machine = db.LineMachineLists.Where(m => m.MachineId == machineId).FirstOrDefault();
                        var line_dept = db.LineInfoes.Where(m => m.LineId == line_machine.LineId).FirstOrDefault();
                        var dept_detials = db.DepartmentLists.Find(line_dept.DeptId);

                        DispatchedItemList dis_Item = new DispatchedItemList();
                        dis_Item.DispatchedId = disHisId;
                        dis_Item.InventoryId = inven_loc.InventoryId;
                        dis_Item.LocationId = locationid;
                        dis_Item.MachineId = machineId;
                        dis_Item.UnitId = dept_detials.UnitId;
                        dis_Item.Quantity = quan;
                        dis_Item.IsAlreadyUsed = false;
                        db.DispatchedItemLists.Add(dis_Item);
                        db.SaveChanges();
                        long refCol = db.DispatchedItemLists.Where(m => m.DispatchedId == disHisId && m.InventoryId == inven_loc.InventoryId && m.MachineId == machineId).Max(m => m.Id);


                        //*************************** Insert Into MachinePartsTable ********************************
                        MachinePartsNotInstalledList machineParts = new MachinePartsNotInstalledList();
                        machineParts.MachineId = machineId;
                        machineParts.PartsId = inven_loc.InventoryId;
                        machineParts.InvenLocId = locationid;
                        machineParts.NotInstalledQuan = quan;
                        machineParts.UnitId = proDetails.UnitId;
                        machineParts.RefColId = refCol;
                        machineParts.CreatedBy = userId;
                        machineParts.CreatedDate = now;
                        machineParts.VoucherId = dis_Item.DispatchedId;
                        db.MachinePartsNotInstalledLists.Add(machineParts);
                        try
                        {
                            db.SaveChanges();
                            msg = "Machine spare parts '" + proDetails.ProductName + "' quantity " + quan + " " + proDetails.UnitName + " has been successfully dispatched for machine '" + machinedetails.MachineAcronym + "' on " + now.ToString("MMM dd , yyyy hh:mm tt") + " .";
                            ColumnId = db.MachinePartsNotInstalledLists.Where(m => m.MachineId == machineId && m.CreatedBy == userId).Max(m => m.Id);
                            opStatus = 1;
                        }
                        catch (Exception)
                        {
                            msg = "Machine spare parts '" + proDetails.ProductName + "' quantity " + quan + " " + proDetails.UnitName + " dispatch for machine '" + machinedetails.MachineAcronym + "' on " + now.ToString("MMM dd , yyyy hh:mm tt") + " was unsuccessful.";
                            opStatus = -1;
                        }
                        SaveAuditLog("Inventory Item Dispatch", msg, "Inventory", "All Store List", "DispatchedItemList", ColumnId, userId, now, opStatus);

                        if (opStatus == 1)
                        {
                            //***************** Decrease Quantity From Inventory ********************

                            foreach (var loc in all_inven_loc)
                            {

                                if (loc != null && loc.Quantity > 0 && quan > 0)
                                {
                                    if (quan == loc.Quantity || quan < loc.Quantity)
                                    {
                                        loc.Quantity = loc.Quantity - quan;
                                        quan = 0;
                                    }
                                    else
                                    {
                                        quan = quan - loc.Quantity;
                                        loc.Quantity = 0;
                                    }
                                    db.Entry(loc).State = EntityState.Modified;
                                    db.SaveChanges();

                                    if ((loc.MinimumQuantity == loc.Quantity) || (loc.Quantity < loc.MinimumQuantity))
                                    {
                                        AddedLocId[addedLocCount] = loc.LocationId;
                                        addedLocCount++;
                                    }
                                }
                            }
                        }
                        loopCount++;

                        if (!isOne)
                        {
                            //string[] allUserId = strAllUser.Split(',');
                            //for (int u = 0; u < allUserId.Length; u++)
                            //{
                            //    long assigUserId = Convert.ToInt64(allUserId[u]);
                            //    if (!db.DispatchedItemAssignUsers.Where(m => m.DispatchId == disHisId && m.UserId == assigUserId && m.MachineId == machineId).Any())
                            //    {
                            //        DispatchedItemAssignUser dis_user = new DispatchedItemAssignUser();
                            //        dis_user.DispatchId = disHisId;
                            //        dis_user.DispatchItemId = Convert.ToInt32(dis_Item.Id);
                            //        dis_user.UserId = assigUserId;
                            //        dis_user.MachineId = machineId;
                            //        db.DispatchedItemAssignUsers.Add(dis_user);
                            //        db.SaveChanges();
                            //    }
                            //}
                            foreach (var user in strAllUser)
                            {
                                if (!db.DispatchedItemAssignUsers.Where(m => m.DispatchId == disHisId && m.UserName == user && m.MachineId == machineId).Any())
                                {
                                    DispatchedItemAssignUser dis_user = new DispatchedItemAssignUser();
                                    dis_user.DispatchId = disHisId;
                                    dis_user.DispatchItemId = Convert.ToInt32(dis_Item.Id);
                                    //dis_user.UserId = assigUserId;
                                    dis_user.UserName = user;
                                    dis_user.MachineId = machineId;
                                    db.DispatchedItemAssignUsers.Add(dis_user);
                                    db.SaveChanges();
                                }
                            }
                        }
                        if (isOne)
                        {
                            //for (int u = 0; u < allAssignUser.Length; u++)
                            //{
                            //    long assigUserId = Convert.ToInt64(allAssignUser[u]);
                            //    if (!db.DispatchedItemAssignUsers.Where(m => m.DispatchId == disHisId && m.UserId == assigUserId).Any())
                            //    {
                            //        DispatchedItemAssignUser dis_user = new DispatchedItemAssignUser();
                            //        dis_user.DispatchId = disHisId;
                            //        dis_user.UserId = assigUserId;
                            //        dis_user.MachineId = machineId;
                            //        db.DispatchedItemAssignUsers.Add(dis_user);
                            //        db.SaveChanges();
                            //    }
                            //}
                            foreach (var user in strAllUser) {
                                if (!db.DispatchedItemAssignUsers.Where(m => m.DispatchId == disHisId && m.UserName == user && m.MachineId == machineId).Any())
                                {
                                    DispatchedItemAssignUser dis_user = new DispatchedItemAssignUser();
                                    dis_user.DispatchId = disHisId;
                                    dis_user.DispatchItemId = Convert.ToInt32(dis_Item.Id);
                                    //dis_user.UserId = assigUserId;
                                    dis_user.UserName = user;
                                    dis_user.MachineId = machineId;
                                    db.DispatchedItemAssignUsers.Add(dis_user);
                                    db.SaveChanges();
                                }
                            }
                        }

                    }
                }
                if (addedLocCount > 0)
                {
                    var allLocList = db.Inventory_Item_Location.Where(m => AddedLocId.Contains(m.LocationId)).ToList();
                    foreach (var list in allLocList)
                    {
                        Inventory_Minimum_Finish_List min_finish = new Inventory_Minimum_Finish_List();
                        min_finish.InventoryId = list.InventoryId;
                        min_finish.LocationId = list.LocationId;
                        min_finish.AvailableQuan = list.Quantity;
                        min_finish.MinimunQuan = list.MinimumQuantity;
                        min_finish.FinishedDate = now;
                        db.Inventory_Minimum_Finish_List.Add(min_finish);
                        db.SaveChanges();
                    }
                }
            }
            return Json("Success", JsonRequestBehavior.AllowGet);
        }
        public ActionResult CartItemAssignedWithPlace(string vname, string[] allPlaceId, string[] allSelectedItem, long userId, List<string> allAssignUser, bool isOne, bool isStore)
        {
            int[] AddedLocId = new int[1000];int addedLocCount=0;
            string msg = ""; int ColumnId = 0; int opStatus = -1;
            int loopCount = 0;
            int lastCountDis = 0;
            if (db.StoreItemDispatchedHistories.Any())
            {
                lastCountDis = db.StoreItemDispatchedHistories.Count();
            }

            //***************************** Created A Transaction For History *********************************
            StoreItemDispatchedHistory dispatchHis = new StoreItemDispatchedHistory();
            dispatchHis.Name = "Dis_#" + ((lastCountDis < 1000) ? lastCountDis.ToString("000") : lastCountDis.ToString());
            dispatchHis.Type = 2;
            dispatchHis.AssignType = 2;
            dispatchHis.VoucherName = vname;
            dispatchHis.DispatchedBy = userId;
            dispatchHis.DispatchedDate = now;
            dispatchHis.IsStore = isStore;
            db.StoreItemDispatchedHistories.Add(dispatchHis);
            db.SaveChanges();
            int disHisId = db.StoreItemDispatchedHistories.Where(m => m.DispatchedBy == userId).Max(m => m.Id);

            for (int j = 0; j < allPlaceId.Length; j++)
            {
                int deptId = 0, count = 0; List<string> strAllUser = allAssignUser;
                if (isOne)
                {
                    deptId = Convert.ToInt32(allPlaceId[j].Split('-')[0]);
                    count = Convert.ToInt32(allPlaceId[j].Split('-')[1]);
                }
                else
                {
                    deptId = Convert.ToInt32(allPlaceId[j].Split('-')[0]);
                    string countwithid = allPlaceId[j].Split('-')[1];
                    count = Convert.ToInt32(countwithid.Split('|')[0]);
                   // strAllUser = countwithid.Split('|')[1];
                }
                var deptDetails = db.DepartmentLists.Find(deptId);

                for (int i = 0; i < count; i++)
                {
                    int locationid = Convert.ToInt32(allSelectedItem[loopCount].Split('-')[0]);
                    double quan = Convert.ToDouble(allSelectedItem[loopCount].Split('-')[1]);
                    var inven_loc = db.Inventory_Item_Location.Find(locationid);
                    var proDetails = db.View_All_InventoryList.Where(m => m.InventoryId == inven_loc.InventoryId).FirstOrDefault();
                    List<Inventory_Item_Location> all_inven_loc = new List<Inventory_Item_Location>();

                    if (quan > 0)
                    {
                        var dep_details = db.DepartmentLists.Find(deptId);
                        DispatchedItemList dis_Item = new DispatchedItemList();
                        dis_Item.DispatchedId = disHisId;
                        dis_Item.InventoryId = inven_loc.InventoryId;
                        dis_Item.LocationId = locationid;
                        dis_Item.DeptId = deptId;
                        dis_Item.UnitId = dep_details.UnitId;
                        dis_Item.Quantity = quan;
                        dis_Item.IsAlreadyUsed = false;
                        db.DispatchedItemLists.Add(dis_Item);
                        db.SaveChanges();
                        try
                        {
                            db.SaveChanges();
                            msg = "Inventory item '" + proDetails.ProductName + "' quantity " + quan + " " + proDetails.UnitName + " has been successfully dispatched for dept '" + deptDetails.DeptName + "' on " + now.ToString("MMM dd , yyyy hh:mm tt") + ".";
                            ColumnId = Convert.ToInt32(dis_Item.Id);
                            opStatus = 1;
                        }
                        catch (Exception)
                        {
                            msg = "Inventory item '" + proDetails.ProductName + "' quantity " + quan + " " + proDetails.UnitName + " dispatch for dept '" + deptDetails.DeptName + "' on " + now.ToString("MMM dd , yyyy hh:mm tt") + " was unsuccessful.";
                            opStatus = -1;
                        }
                        SaveAuditLog("Inventory Item Dispatch", msg, "Inventory", "All Store List", "DispatchedItemList", ColumnId, userId, now, opStatus);
                        if (opStatus == 1)
                        {
                            //***************** Decrease Quantity From Inventory ********************

                            if (inven_loc != null && inven_loc.Quantity > 0 && quan > 0)
                            {
                                if (quan == inven_loc.Quantity || quan < inven_loc.Quantity)
                                {
                                    inven_loc.Quantity = inven_loc.Quantity - quan;
                                    quan = 0;
                                }
                                else
                                {
                                    quan = quan - inven_loc.Quantity;
                                    inven_loc.Quantity = 0;
                                }
                                db.Entry(inven_loc).State = EntityState.Modified;
                                db.SaveChanges();

                                if ((inven_loc.MinimumQuantity == inven_loc.Quantity) || (inven_loc.Quantity < inven_loc.MinimumQuantity))
                                {
                                    AddedLocId[addedLocCount] = inven_loc.LocationId;
                                    addedLocCount++;
                                }
                            }

                            //foreach (var loc in all_inven_loc)
                            //{

                            //    if (loc != null && loc.Quantity > 0 && quan > 0)
                            //    {
                            //        if (quan == loc.Quantity || quan < loc.Quantity)
                            //        {
                            //            loc.Quantity = loc.Quantity - quan;
                            //            quan = 0;
                            //        }
                            //        else
                            //        {
                            //            quan = quan-loc.Quantity;
                            //            loc.Quantity = 0;
                            //        }
                            //        db.Entry(loc).State = EntityState.Modified;
                            //        db.SaveChanges();

                            //        if ((loc.MinimumQuantity == loc.Quantity) || (loc.Quantity < loc.MinimumQuantity))
                            //        {
                            //            AddedLocId[addedLocCount] = loc.LocationId;
                            //            addedLocCount++;
                            //        }
                            //    }
                            //} 
                        }
                        loopCount++;

                        if (!isOne)
                        {
                            //string[] allUserId = strAllUser.Split(',');
                            //for (int u = 0; u < allUserId.Length; u++)
                            //{
                            //    long assigUserId = Convert.ToInt64(allUserId[u]);
                            //    if (!db.DispatchedItemAssignUsers.Where(m => m.DispatchId == disHisId && m.UserId == assigUserId && m.DeptId == deptId).Any())
                            //    {
                            //        DispatchedItemAssignUser dis_user = new DispatchedItemAssignUser();
                            //        dis_user.DispatchId = disHisId;
                            //        dis_user.DispatchItemId = Convert.ToInt32(dis_Item.Id);
                            //        dis_user.UserId = assigUserId;
                            //        dis_user.DeptId = deptId;
                            //        db.DispatchedItemAssignUsers.Add(dis_user);
                            //        db.SaveChanges();
                            //    }
                            //}
                            foreach (var user in strAllUser) { 
                            if (!db.DispatchedItemAssignUsers.Where(m => m.DispatchId == disHisId && m.UserName == user && m.DeptId == deptId).Any())
                            {
                                DispatchedItemAssignUser dis_user = new DispatchedItemAssignUser();
                                dis_user.DispatchId = disHisId;
                                dis_user.DispatchItemId = Convert.ToInt32(dis_Item.Id);
                                //dis_user.UserId = assigUserId;
                                dis_user.UserName = user;
                                dis_user.DeptId = deptId;
                                db.DispatchedItemAssignUsers.Add(dis_user);
                                db.SaveChanges();
                            }
                            }
                        }
                    }
                    if (isOne)
                    {
                        //for (int u = 0; u < allAssignUser.Length; u++)
                        //{
                        //    long assigUserId = Convert.ToInt64(allAssignUser[u]);
                        //    if (!db.DispatchedItemAssignUsers.Where(m => m.DispatchId == disHisId && m.UserId == assigUserId).Any())
                        //    {
                        //        DispatchedItemAssignUser dis_user = new DispatchedItemAssignUser();
                        //        dis_user.DispatchId = disHisId;
                        //        dis_user.UserId = assigUserId;
                        //        dis_user.DeptId = deptId;
                        //        db.DispatchedItemAssignUsers.Add(dis_user);
                        //        db.SaveChanges();
                        //    }
                        //}
                        foreach (var user in strAllUser)
                        {
                            if (!db.DispatchedItemAssignUsers.Where(m => m.DispatchId == disHisId && m.UserName == user && m.DeptId == deptId).Any())
                            {
                                DispatchedItemAssignUser dis_user = new DispatchedItemAssignUser();
                                dis_user.DispatchId = disHisId;
                                //dis_user.DispatchItemId = Convert.ToInt32(dis_Item.Id);
                                //dis_user.UserId = assigUserId;
                                dis_user.UserName = user;
                                dis_user.DeptId = deptId;
                                db.DispatchedItemAssignUsers.Add(dis_user);
                                db.SaveChanges();
                            }
                        }

                    }
                   
                }
            }
            if (addedLocCount > 0)
            {
                var allLocList = db.Inventory_Item_Location.Where(m => AddedLocId.Contains(m.LocationId)).ToList();
                foreach (var list in allLocList)
                {
                    Inventory_Minimum_Finish_List min_finish = new Inventory_Minimum_Finish_List();
                    min_finish.InventoryId = list.InventoryId;
                    min_finish.LocationId = list.LocationId;
                    min_finish.AvailableQuan = list.Quantity;
                    min_finish.MinimunQuan = list.MinimumQuantity;
                    min_finish.FinishedDate = now;
                    db.Inventory_Minimum_Finish_List.Add(min_finish);
                    db.SaveChanges();
                }
            }
            return Json("Success", JsonRequestBehavior.AllowGet);
        }

        #endregion

        #region Created,Update and Delete Store
        public ActionResult AllStoreLists()
        {
            var stores = (from w in db.View_All_StoreLists
                          select new StoreListsModelView
                          {
                              Id = w.Id,
                              StoreName = w.StoreName,
                              StoreAcroynm = w.StoreAcroynm,
                              CreatedBy = w.CreatedBy,
                              Username = w.UserName,
                              Picture = w.PictureName,
                              CreatedDate = w.CreatedDate,
                              CreatedDateName = w.CreatedDateName,
                              AssignUserName = w.AssignUserName,
                              AllAssignUserId = w.AllAssignUserId
                          }).GroupBy(m => m.Id).Select(n => n.FirstOrDefault()).ToList();
            return View(stores);
        }
        [EncryptedActionParameter]
        public ActionResult AddNewStore(int? Id)
        {
            StoreListsModelView model = new StoreListsModelView();
            if (Id != null)
            {
                var store = db.View_All_StoreLists.Where(m => m.Id == Id).FirstOrDefault();
                if (store == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }
                model.Id = store.Id;
                model.StoreName = store.StoreName;
                model.StoreAcroynm = store.StoreAcroynm;
                model.AllAssignUserId = store.AllAssignUserId;
                if (model.AllAssignUserId != null)
                {
                    List<SelectListItem> allpreUserList = new List<SelectListItem>();
                    string[] alluser = model.AllAssignUserId.Split(',');
                    for (int i = 0; i < alluser.Length; i++)
                    {
                        int tid = Convert.ToInt32(alluser[i]);
                        allpreUserList.Add(new SelectListItem { Text = db.View_UserLists.Where(m => m.UserId == tid).Select(m => m.UserName).FirstOrDefault(), Value = tid.ToString() });
                    }
                    ViewBag.displayUser = allpreUserList;
                }
                else
                {
                    ViewBag.displayUser = null;
                }
            }
            else
            {
                ViewBag.displayUser = null;
            }
            List<SelectListItem> allUserList = new SelectList(db.View_UserLists.Where(m => m.Status == 1), "UserId", "UserName").ToList();
            ViewBag.allUserList = allUserList;
            return View(model);
        }
        public PartialViewResult _ShowStoreDetailsById(int Id) {
            ViewBag.Id = Id;
            return PartialView();
        }
        public PartialViewResult ShowStoreDetailsById(int Id) {
            StoreListsModelView model = new StoreListsModelView();
            var store = db.View_All_StoreLists.Where(m => m.Id == Id).FirstOrDefault();
           
            model.Id = store.Id;
            model.StoreName = store.StoreName;
            model.StoreAcroynm = store.StoreAcroynm;
            model.AllAssignUserId = store.AllAssignUserId;
            if (model.AllAssignUserId != null)
            {
                List<SelectListItem> allpreUserList = new List<SelectListItem>();
                string[] alluser = model.AllAssignUserId.Split(',');
                for (int i = 0; i < alluser.Length; i++)
                {
                    int tid = Convert.ToInt32(alluser[i]);
                    allpreUserList.Add(new SelectListItem { Text = db.View_UserLists.Where(m => m.UserId == tid).Select(m => m.UserName).FirstOrDefault(), Value = tid.ToString() });
                }
                ViewBag.displayUser = allpreUserList;
            }
            else
            {
                ViewBag.displayUser = null;
            }
            return PartialView(model);
        }
        public PartialViewResult _loadUserAssiegnedUnit(long uId,int id ,bool isStore)
        {
            if (isStore)
            {
                var AllUId = db.UserItemDispatchLocs.Where(m => m.UserId == uId && m.StoreId == id).Select(m => m.UnitId).ToList();
                var allUnitlist = db.Site_Unit_Lists.Where(m => AllUId.Contains(m.UnitId)).ToList();
                List<SelectListItem> allAssignUId = new List<SelectListItem>();
                foreach (var unit in allUnitlist)
                {
                    allAssignUId.Add(new SelectListItem { Text = unit.UnitName, Value = unit.UnitId.ToString() });
                }
                ViewBag.selectedUnit = allAssignUId;
            }
            else
            {
                var AllUId = db.UserItemDispatchLocs.Where(m => m.UserId == uId && m.WareId == id).Select(m => m.UnitId).ToList();
                var allUnitlist = db.Site_Unit_Lists.Where(m => AllUId.Contains(m.UnitId)).ToList();
                List<SelectListItem> allAssignUId = new List<SelectListItem>();
                foreach (var unit in allUnitlist)
                {
                    allAssignUId.Add(new SelectListItem { Text = unit.UnitName, Value = unit.UnitId.ToString() });
                }
                ViewBag.selectedUnit = allAssignUId;
            }
            List<SelectListItem> allUnitList = new SelectList(db.Site_Unit_Lists, "UnitId", "UnitName").ToList();
            ViewBag.allUnitList = allUnitList;
            ViewBag.uId = uId;
            return PartialView();
        }
        public JsonResult SaveNewStoreData(StoreListsModelView model, long[] AllUserId, string[] AllUnitId)
        {
            if (ModelState.IsValid)
            {
                string oprName = "Add New Store";
                string msg = "";
                int ColumnId = 0;
                int OperationStatus = 1;
                StoreList store = new StoreList();
                store.StoreName = model.StoreName;
                store.StoreAcroynm = model.StoreAcroynm;
                store.CreatedBy = model.CreatedBy;
                store.CreatedDate = now;
                
                try
                {
                    db.StoreLists.Add(store);
                    db.SaveChanges();
                    msg = "New Store " + model.StoreName + " added.";
                    int storeId = db.StoreLists.Where(m => m.CreatedBy == model.CreatedBy).Max(m => m.Id);

                    for (int i = 0; i < AllUserId.Length; i++)
                    {
                        int status = -1;
                        long uId = AllUserId[i];
                        var userInfo = db.View_UserLists.Where(m => m.UserId == uId).FirstOrDefault();

                        AssignStoreWithUser user = new AssignStoreWithUser();
                        user.UserId = Convert.ToInt32(AllUserId[i]);
                        user.StoreId = storeId;
                        user.WareId = 0;
                        user.AssignedDate = now;
                        user.AssignedBy = model.CreatedBy;
                        db.AssignStoreWithUsers.Add(user);
                        db.SaveChanges();


                        string[] UnitId = AllUnitId[i].Split(',');
                        for (int j = 0; j < UnitId.Length; j++)
                        {
                            UserItemDispatchLoc loc = new UserItemDispatchLoc();
                            loc.UserId = uId;
                            loc.StoreId = storeId;
                            loc.WareId = 0;
                            loc.UnitId = Convert.ToInt32(UnitId[j]);
                            loc.AssignedDate = now;
                            loc.AssignedBy = model.CreatedBy;
                            db.UserItemDispatchLocs.Add(loc);
                            try
                            {
                                db.SaveChanges();
                                status = 1;
                            }
                            catch (Exception)
                            {
                                status = -1;
                            }
                        }
                        msg = "Dispatch location has been successfully set for user '" + userInfo.UserName + "' for store '" + store.StoreName + "' on " + now.ToString("MMM dd, yyyy hh:mm tt") + ".";
                        SaveAuditLog("Define Dispatch Location For Store User", msg, "Inventory", "Add New Store", "StoreLists", store.Id, model.CreatedBy, now, status);
                    }

                    ColumnId = storeId;
                }
                catch (Exception)
                {
                    msg = "New Store '" + model.StoreName + "' addition was unsuccessful";
                    OperationStatus = -1;
                    ColumnId = 0;
                }
                SaveAuditLog(oprName, msg, "Inventory", "Add New Store", "StoreLists", ColumnId, model.CreatedBy, now, OperationStatus);
                if (OperationStatus == 1)
                {
                    return Json("Success", JsonRequestBehavior.AllowGet);
                }
            }
            return Json("Error",JsonRequestBehavior.AllowGet);
        }
        public JsonResult EditStoreData(StoreListsModelView model, long[] AllUserId, string[] AllUnitId)
        {
            if (ModelState.IsValid && AllUserId.Length > 0 && AllUnitId.Length > 0)
            {
                string oprName = "Add New Store";
                string msg = "";
                int ColumnId = 0;
                int OperationStatus = 1;
                StoreList store = db.StoreLists.Find(model.Id);
                store.StoreName = model.StoreName;
                store.StoreAcroynm = model.StoreAcroynm;
                store.UpdatedBy = model.CreatedBy;
                store.UpdatedDate = now;
                oprName = "Update Store Info";
                try
                {
                    db.Entry(store).State = EntityState.Modified;
                    db.SaveChanges();

                    var preUserlist = db.AssignStoreWithUsers.Where(m => m.StoreId == model.Id).ToList();
                    foreach (var m in preUserlist)
                    {
                        var preUList = db.UserItemDispatchLocs.Where(u => u.StoreId == store.Id && u.UserId == m.UserId).ToList();
                        foreach (var unit in preUList)
                        {
                            db.UserItemDispatchLocs.Remove(unit);
                            db.SaveChanges();
                        }
                        db.AssignStoreWithUsers.Remove(m);
                        db.SaveChanges();
                    }
                    for (int i = 0; i < AllUserId.Length; i++)
                    {
                        AssignStoreWithUser user = new AssignStoreWithUser();
                        user.UserId = Convert.ToInt32(AllUserId[i]);
                        user.StoreId = model.Id;
                        user.WareId = 0;
                        user.AssignedDate = now;
                        user.AssignedBy = model.CreatedBy;
                        db.AssignStoreWithUsers.Add(user);
                        db.SaveChanges();


                        int status = -1;
                        long uId = AllUserId[i];
                        var userInfo = db.View_UserLists.Where(m => m.UserId == uId).FirstOrDefault();
                        string[] UnitId = AllUnitId[i].Split(',');
                        for (int j = 0; j < UnitId.Length; j++)
                        {
                            UserItemDispatchLoc loc = new UserItemDispatchLoc();
                            loc.UserId = uId;
                            loc.StoreId = store.Id;
                            loc.WareId = 0;
                            loc.UnitId = Convert.ToInt32(UnitId[j]);
                            loc.AssignedDate = now;
                            loc.AssignedBy = model.CreatedBy;
                            db.UserItemDispatchLocs.Add(loc);
                            try
                            {
                                db.SaveChanges();
                                status = 1;
                            }
                            catch (Exception)
                            {
                                status = -1;
                            }
                        }
                        msg = "Dispatch location has been successfully set for user '" + userInfo.UserName + "' for store '" + store.StoreName + "' on " + now.ToString("MMM dd, yyyy hh:mm tt") + ".";
                        SaveAuditLog("Define Dispatch Location For Store User", msg, "Inventory", "Add New Store", "StoreLists", store.Id, model.CreatedBy, now, status);
                    }
                    ColumnId = model.Id;
                    msg = "Store  '" + model.StoreName + "'  information has been successfully updated.";
                }
                catch (Exception)
                {
                    msg = "Store  '" + model.StoreName + "'  information update was unsuccessful.";
                    OperationStatus = -1;
                    ColumnId = 0;
                }
                SaveAuditLog(oprName, msg, "Inventory", "Add New Store", "StoreLists", ColumnId, model.CreatedBy, now, OperationStatus);
                if (OperationStatus == 1)
                {
                    return Json("Success", JsonRequestBehavior.AllowGet);
                }
            }
            return Json("Error", JsonRequestBehavior.AllowGet);
        }
        public JsonResult GetAllUnitName()
        {
            var list = db.Site_Unit_Lists.ToList();
            return Json(list, JsonRequestBehavior.AllowGet);
        }
        public JsonResult DeleteStore(int? storeId, long UserId)
        {
            string msg = "";
            string name = "";
            string value = "";
            int ColumnId = 0;
            int OperationStatus = 1;

            if (storeId == null)
            {
                return Json("Error", JsonRequestBehavior.AllowGet);
            }
            else
            {
                StoreList store = db.StoreLists.Find(storeId);
                if (store != null)
                {
                    if (db.Inventory_Item_Location.Where(m => m.StoreId == store.Id).Any())
                    {
                        return Json("Store can not be deleted because it is in use.", JsonRequestBehavior.AllowGet);
                    }

                    var sitelist = db.AssignStoreWithSites.Where(m => m.StoreId == storeId).ToList();
                    var AllUser = db.AssignStoreWithUsers.Where(m => m.StoreId == store.Id).ToList();
                    name = store.StoreName;
                    ColumnId = store.Id;
                    try
                    {
                        msg = "Store '" + name + "' has been successfully deleted on "+now.ToString("MMM dd , yyyy hh:mm tt")+" .";
                        db.StoreLists.Remove(store);
                        db.SaveChanges();
                        foreach (var m in sitelist)
                        {
                            db.AssignStoreWithSites.Remove(m);
                            db.SaveChanges();
                        }
                        foreach (var user in AllUser)
                        {
                            var allLoc = db.UserItemDispatchLocs.Where(m => m.UserId == user.UserId && m.StoreId == m.StoreId).ToList();
                            foreach (var loc in allLoc)
                            {
                                db.UserItemDispatchLocs.Remove(loc);
                                db.SaveChanges();
                            }
                            db.AssignStoreWithUsers.Remove(user);
                            db.SaveChanges();
                        }
                        OperationStatus = 1;
                        value = "Success";
                    }
                    catch (Exception ex)
                    {
                        string strErr = ex.ToString();
                        msg = name + " deletion unsuccessful";
                        OperationStatus = -1;
                        value = "Error";
                    }
                }
                SaveAuditLog("Delete", msg, "Inventory", "Delete Store", "StoreLists", ColumnId, UserId, now, OperationStatus);
                return Json(value, JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult StoreDetail(int? StoreId)
        {
            if (StoreId == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            StoreListsModelView model = (from w in db.View_All_StoreLists
                                         select new StoreListsModelView
                                         {
                                             Id = w.Id,
                                             StoreName = w.StoreName,
                                             StoreAcroynm = w.StoreAcroynm,
                                             CreatedBy = w.CreatedBy,
                                             Username = w.UserName,
                                             Picture = w.PictureName,
                                             CreatedDate = w.CreatedDate,
                                             CreatedDateName = w.CreatedDateName
                                         }).GroupBy(m => m.Id).Select(n => n.FirstOrDefault()).FirstOrDefault();

            return View(model);
        }
        #endregion
        #region Create,Delete,Update and List of ProductType
        public ActionResult ProductTypeList()
        {
            return View();
        }
        public ActionResult _ShowAllProductTypeList()
        {
            var AllParentProductType = (from p in db.View_Product_Type_Lists
                                        where p.HasParent == false
                                        select new ProductTypeViewModel
                                        {
                                            ProductTypeId = p.ProductTypeId,
                                            ProductTypeName = p.ProductTypeName,
                                            CreatedBy = p.CreatedBy,
                                            CreatedDateName = p.CreatedDateName,
                                            HasChild = p.HasChild,
                                            UserName = p.CreatedUserName,
                                            Picture = p.CreatedUserPic,
                                            UpdatedDate = p.UpdatedDate,
                                            UpdatedBy = p.UpdatedBy,
                                            UpdatedDateFormate = p.UpdatedDateFormate,
                                            UpdatedUserName = p.UpdatedUserName,
                                            UpdatedUserPic = p.UpdatedUserPic,
                                            CanBeDeleted = p.CanBeDeleted
                                        }).OrderBy(p => p.ProductTypeName).ToList();
            ViewBag.AllParentProType = AllParentProductType;
            return PartialView();
        }
        public JsonResult GetAllProductTypeForList(int? ParentId)
        {
            int id = Convert.ToInt32(ParentId);
            var list = (from m in db.View_Product_Type_Lists
                        where m.ParentId == id
                        select new
                        {
                            ProductTypeId = m.ProductTypeId,
                            ProductTypeName = m.ProductTypeName,
                            HasParent = m.HasParent,
                            ParentId = m.ParentId,
                            ParentName = m.ParentName,
                            CreatedBy = m.CreatedBy,
                            CreatedDateName = m.CreatedDateName,
                            HasChild = m.HasChild,
                            UserName = m.CreatedUserName,
                            Picture = m.CreatedUserPic,
                            UpdatedDateFormate = m.UpdatedDateFormate,
                            UpdatedUserName = m.UpdatedUserName,
                            UpdatedUserPic = m.UpdatedUserPic
                        }).OrderBy(p => p.ProductTypeName).ToList();

            return Json(list, JsonRequestBehavior.AllowGet);
        }
        public ActionResult ProductTypeCreate(int? ProductTypeId, int? ParentId, string ParentName)
        {
            ProductTypeViewModel model = new ProductTypeViewModel();
            if (ParentId != null)
            {
                model.HasParent = true;
                model.ParentId = ParentId;
                model.ParentName = ParentName;
            }
            if (ProductTypeId != null)
            {
                View_Product_Type_Lists pro_type = db.View_Product_Type_Lists.Where(m => m.ProductTypeId == ProductTypeId).FirstOrDefault();
                model.ProductTypeId = pro_type.ProductTypeId;
                model.ProductTypeName = pro_type.ProductTypeName;
                model.HasParent = pro_type.HasParent;
                model.ParentId = pro_type.ParentId;
                model.ParentName = pro_type.ParentName;
                model.HasChild = pro_type.HasChild;
            }
            return PartialView(model);
        }
        public JsonResult ProductTypeCreateSave(ProductTypeViewModel model)
        {
            if (ModelState.IsValid)
            {
                string msg = "";
                string opname = "";
                int ColumnId = 0;
                int OperationStatus = 1;

                ProductTypeList pro_type = new ProductTypeList();
                if (model.ProductTypeId > 0)
                {
                    pro_type = db.ProductTypeLists.Find(model.ProductTypeId);
                    pro_type.ProductTypeName = model.ProductTypeName;
                    pro_type.HasParent = model.HasParent;
                    pro_type.ParentId = model.ParentId;
                    pro_type.UpdatedBy = model.CreatedBy;
                    pro_type.UpdatedDate = now;
                    db.Entry(pro_type).State = EntityState.Modified;
                    ColumnId = pro_type.ProductTypeId;
                    msg = (model.HasParent == true && model.ParentName != null) ?
                        "Sub product type " + model.ProductTypeName + " of " + model.ParentName + " information updated successfully." :
                        "Product type " + model.ProductTypeName + " information updated successfully.";
                    opname = "Update Product Type Information";
                }
                else
                {
                    pro_type.ProductTypeName = model.ProductTypeName;
                    pro_type.HasParent = model.HasParent;
                    pro_type.ParentId = model.ParentId;
                    pro_type.CreatedBy = model.CreatedBy;
                    pro_type.CreatedDate = now;
                    pro_type.CanBeDeleted = true;
                    db.ProductTypeLists.Add(pro_type);
                    if (model.HasParent == true && model.ParentName != null)
                    {
                        opname = "Add Product Type";
                        msg = "New sub product type " + model.ProductTypeName + " of " + model.ParentName + " added successfully.";
                    }
                    else
                    {
                        opname = "Create Product Type";
                        msg = "New product type " + model.ProductTypeName + " created successfully.";
                    }
                }
                try
                {
                    db.SaveChanges();
                    if (model.ProductTypeId == 0)
                    {
                        ColumnId = db.ProductTypeLists.Max(m => m.ProductTypeId);
                    }
                }
                catch (Exception ex)
                {
                    string errorMsg = ex.Message.ToString();
                    if (model.ProductTypeId > 0)
                    {
                        msg = (model.HasParent == true && model.ParentName != null) ?
                            "Sub product type " + model.ProductTypeName + " of " + model.ParentName + " information update unsuccessful." :
                            "Product type " + model.ProductTypeName + " information update unsuccessful.";
                    }
                    else
                    {
                        msg = (model.HasParent == true && model.ParentName != null) ?
                            "Add new product type " + model.ProductTypeName + " of " + model.ParentName + " unsuccessful." :
                            "New product type " + model.ProductTypeName + " create unsuccessful.";
                    }
                    OperationStatus = -1;
                }
                SaveAuditLog(opname, msg, "Inventory", "Product Type List", "ProductTypeLists", ColumnId, model.CreatedBy, now, OperationStatus);
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
        public ActionResult ProductTypeDestroy(int? ProductTypeId, long UserId, string ParentName)
        {
            if (ProductTypeId != null)
            {
                string msg = "";
                string name = "";
                int ColumnId = 0;
                int OperationStatus = 1;
                string value = "Success";
                ProductTypeList pro_type = db.ProductTypeLists.Find(ProductTypeId);
                name = pro_type.ProductTypeName;
                ColumnId = pro_type.ProductTypeId;
                try
                {
                    var acq_pro_id = db.View_All_AcquisitionList.Where(m => m.ProductTypeId == pro_type.ProductTypeId).Select(m => m.ProductTypeId).FirstOrDefault();
                    var inv_pro_id = db.InventoryLists.Where(m => m.ProductTypeId == pro_type.ProductTypeId).Select(m => m.ProductTypeId).FirstOrDefault();
                    if (acq_pro_id == null && inv_pro_id == 0)
                    {
                        db.ProductTypeLists.Remove(pro_type);
                        db.SaveChanges();
                        OperationStatus = 1;
                        msg = (ParentName != null) ? "Sub product type " + name + " of " + ParentName + " deleted successfully." :
                              "Product type " + name + " deleted successfully.";
                    }
                    else
                    {
                        msg = (ParentName != null) ? "Sub product type of " + ParentName + " delete attempt unsuccessful. " + name + " has been used." :
                            "Product type delete attempt unsuccessful. " + name + " has been used.";
                        value = "Product type " + name + " has been used.";
                        OperationStatus = -1;
                    }
                }
                catch (Exception ex)
                {
                    string errorMsg = ex.Message.ToString();
                    msg = (ParentName != null) ? "Sub product type " + name + " of " + ParentName + " delete attempt unsuccessful." :
                        "Product type " + name + " delete attempt unsuccessful.";
                    OperationStatus = -1;
                    value = "Error";
                }
                SaveAuditLog("Delete Product Type ", msg, "Inventory", "Product Type Lists", "ProductTypeLists", ColumnId, UserId, now, OperationStatus);
                return Json(value, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json("Error", JsonRequestBehavior.AllowGet);
            }
        }
        #endregion
        #region Create,Delete,Update and List of Brand
        public ActionResult AllBrandList()
        {
            return View();
        }
        public ActionResult _ShowAllBrandList()
        {
            var AllParentBrand = (from p in db.View_BrandList
                                  where p.HasParent == false
                                  select new BrandListViewModel
                                  {
                                      BrandId = p.BrandId,
                                      BrandName = p.BrandName,
                                      CreatedBy = p.CreatedBy,
                                      CreatedDateName = p.CreatedDateName,
                                      HasChild = p.HasChild,
                                      UserName = p.UserName,
                                      Picture = p.Picture
                                  }).OrderBy(p => p.BrandName).ToList();
            ViewBag.AllParentBrandList = AllParentBrand;
            return PartialView();
        }
        public JsonResult GetAllBrandForList(int? ParentId)
        {
            int id = Convert.ToInt32(ParentId);
            var list = (from m in db.View_BrandList
                        where m.ParentId == id
                        select new
                        {
                            BrandId = m.BrandId,
                            BrandName = m.BrandName,
                            HasParent = m.HasParent,
                            ParentId = m.ParentId,
                            ParentName = m.ParentName,
                            CreatedBy = m.CreatedBy,
                            CreatedDateName = m.CreatedDateName,
                            HasChild = m.HasChild,
                            UserName = m.UserName,
                            Picture = m.Picture
                        }).OrderBy(p => p.BrandName).ToList();

            return Json(list, JsonRequestBehavior.AllowGet);
        }
        public ActionResult BrandCreate(int? BrandId, int? ParentId, string ParentName)
        {
            BrandListViewModel model = new BrandListViewModel();
            if (ParentId != null)
            {
                model.HasParent = true;
                model.ParentId = ParentId;
                model.ParentName = ParentName;
            }
            if (BrandId != null)
            {
                View_BrandList brand = db.View_BrandList.Where(m => m.BrandId == BrandId).FirstOrDefault();
                model.BrandId = brand.BrandId;
                model.BrandName = brand.BrandName;
                model.HasParent = brand.HasParent;
                model.ParentId = brand.ParentId;
                model.ParentName = brand.ParentName;
                model.HasChild = brand.HasChild;
            }
            return PartialView(model);
        }
        public ActionResult BrandCreateSave(BrandListViewModel model)
        {
            if (ModelState.IsValid)
            {
                string msg = "";
                string opname = "";
                int ColumnId = 0;
                int OperationStatus = 1;

                BrandList brand = new BrandList();
                if (model.BrandId > 0)
                {
                    brand = db.BrandLists.Find(model.BrandId);
                    brand.BrandName = model.BrandName;
                    brand.HasParent = model.HasParent;
                    brand.ParentId = model.ParentId;
                    brand.UpdatedBy = model.CreatedBy;
                    brand.UpdatedDate = now;
                    db.Entry(brand).State = EntityState.Modified;
                    ColumnId = brand.BrandId;
                    msg = (model.HasParent == true && model.ParentName != null) ?
                        "Sub brand " + model.BrandName + " of " + model.ParentName + " information updated successfully." :
                        "Brand " + model.BrandName + " information updated successfully.";
                    opname = "Update Brand Information";
                }
                else
                {
                    brand.BrandName = model.BrandName;
                    brand.HasParent = model.HasParent;
                    brand.ParentId = model.ParentId;
                    brand.CreatedBy = model.CreatedBy;
                    brand.CreatedDate = now;
                    db.BrandLists.Add(brand);
                    if (model.HasParent == true && model.ParentName != null)
                    {
                        opname = "Add Brand";
                        msg = "New sub brand " + model.BrandName + " of " + model.ParentName + " added successfully.";
                    }
                    else
                    {
                        opname = "Create Brand";
                        msg = "New brand " + model.BrandName + " created successfully.";
                    }
                }
                try
                {
                    db.SaveChanges();
                    if (model.BrandId == 0)
                    {
                        ColumnId = db.BrandLists.Max(m => m.BrandId);
                    }
                }
                catch (Exception ex)
                {
                    string errorMsg = ex.Message.ToString();
                    if (model.BrandId > 0)
                    {
                        msg = (model.HasParent == true && model.ParentName != null) ?
                            "Sub brand " + model.BrandName + " of " + model.ParentName + " information update unsuccessful." :
                            "Brand " + model.BrandName + " information update unsuccessful.";
                    }
                    else
                    {
                        msg = (model.HasParent == true && model.ParentName != null) ?
                            "Add new brand " + model.BrandName + " of " + model.ParentName + " unsuccessful." :
                            "New brand " + model.BrandName + " create unsuccessful.";
                    }
                    OperationStatus = -1;
                }
                SaveAuditLog(opname, msg, "Inventory", "All Brand List", "BrandList", ColumnId, model.CreatedBy, now, OperationStatus);
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
        public ActionResult BrandDestroy(int? BrandId, long UserId, string ParentName)
        {
            if (BrandId != null)
            {
                string msg = "";
                string name = "";
                int ColumnId = 0;
                int OperationStatus = 1;
                string value = "Success";
                BrandList brand = db.BrandLists.Find(BrandId);
                name = brand.BrandName;
                ColumnId = brand.BrandId;
                try
                {
                    var acq_bnd_id = db.View_All_AcquisitionList.Where(m => m.Brand == brand.BrandId).Select(m => m.Brand).FirstOrDefault();
                    var inv_bnd_id = db.InventoryLists.Where(m => m.BrandId == brand.BrandId).Select(m => m.BrandId).FirstOrDefault();
                    if (acq_bnd_id == null && inv_bnd_id == null)
                    {
                        db.BrandLists.Remove(brand);
                        db.SaveChanges();
                        OperationStatus = 1;
                        msg = (ParentName != null) ? "Sub brand " + name + " of " + ParentName + " deleted successfully." :
                              "Brand " + name + " deleted successfully.";
                    }
                    else
                    {
                        msg = (ParentName != null) ? "Sub brand of " + ParentName + " delete attempt unsuccessful. " + name + " has been used." :
                            "Brand delete attempt unsuccessful. " + name + " has been used.";
                        value = "Brand " + name + " has been used.";
                        OperationStatus = -1;
                    }
                }
                catch (Exception ex)
                {
                    string errorMsg = ex.Message.ToString();
                    msg = (ParentName != null) ? "Sub brand " + name + " of " + ParentName + " delete attempt unsuccessful." :
                        "Brand " + name + " delete attempt unsuccessful.";
                    OperationStatus = -1;
                    value = "Error";
                }
                SaveAuditLog("Delete Brand", msg, "Inventory", "All Brand List", "BrandList", ColumnId, UserId, now, OperationStatus);
                return Json(value, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json("Error", JsonRequestBehavior.AllowGet);
            }
        }
        #endregion
        #region Create,Delete,Update and List of Unit
        public ActionResult AllUnitList()
        {
            return View();
        }
        public ActionResult _ShowAllUnitList()
        {
            var AllParentUnit = (from p in db.View_UnitList
                                 where p.HasParentId == false
                                 select new UnitListViewModel
                                 {
                                     UnitId = p.UnitId,
                                     UnitName = p.UnitName,
                                     CreatedBy = p.CreatedBy,
                                     CreatedDateName = p.CreatedDateName,
                                     HasChild = p.HasChild,
                                     UserName = p.UserName,
                                     Picture = p.Picture
                                 }).OrderBy(p => p.UnitName).ToList();
            ViewBag.AllParentUnitList = AllParentUnit;
            return PartialView();
        }
        public JsonResult GetAllUnitForList(int? ParentId)
        {
            int id = Convert.ToInt32(ParentId);
            var list = (from m in db.View_UnitList
                        where m.ParentId == id
                        select new
                        {
                            UnitId = m.UnitId,
                            UnitName = m.UnitName,
                            HasParent = m.HasParentId,
                            ParentId = m.ParentId,
                            ParentName = m.ParentName,
                            CreatedBy = m.CreatedBy,
                            CreatedDateName = m.CreatedDateName,
                            HasChild = m.HasChild,
                            UserName = m.UserName,
                            Picture = m.Picture
                        }).OrderBy(p => p.UnitName).ToList();
            return Json(list, JsonRequestBehavior.AllowGet);
        }
        public ActionResult UnitCreate(int? UnitId, int? ParentId, string ParentName)
        {
            UnitListViewModel model = new UnitListViewModel();
            if (ParentId != null)
            {
                model.HasParent = true;
                model.ParentId = ParentId;
                model.ParentName = ParentName;
            }
            if (UnitId != null)
            {
                View_UnitList unit = db.View_UnitList.Where(m => m.UnitId == UnitId).FirstOrDefault();
                model.UnitId = unit.UnitId;
                model.UnitName = unit.UnitName;
                model.HasParent = unit.HasParentId;
                model.ParentId = unit.ParentId;
                model.ParentName = unit.ParentName;
                model.HasChild = unit.HasChild;
            }
            return PartialView(model);
        }
        public ActionResult UnitCreateSave(UnitListViewModel model)
        {
            if (ModelState.IsValid)
            {
                string msg = "";
                string opname = "";
                int ColumnId = 0;
                int OperationStatus = 1;

                UnitList unit = new UnitList();
                if (model.UnitId > 0)
                {
                    unit = db.UnitLists.Find(model.UnitId);
                    unit.UnitName = model.UnitName;
                    unit.HasParentId = model.HasParent;
                    unit.ParentId = model.ParentId;
                    unit.UpdatedBy = model.CreatedBy;
                    unit.UpdatedDate = now;
                    db.Entry(unit).State = EntityState.Modified;
                    ColumnId = unit.UnitId;
                    msg = (model.HasParent == true && model.ParentName != null) ?
                        "Sub unit " + model.UnitName + " of " + model.ParentName + " information updated successfully." :
                        "Unit " + model.UnitName + " information updated successfully.";
                    opname = "Update Unit Information";
                }
                else
                {
                    unit.UnitName = model.UnitName;
                    unit.HasParentId = model.HasParent;
                    unit.ParentId = model.ParentId;
                    unit.CreatedBy = model.CreatedBy;
                    unit.CreatedDate = now;
                    db.UnitLists.Add(unit);
                    if (model.HasParent == true && model.ParentName != null)
                    {
                        opname = "Add Unit";
                        msg = "New sub unit " + model.UnitName + " of " + model.ParentName + " added successfully.";
                    }
                    else
                    {
                        opname = "Create Unit";
                        msg = "New unit " + model.UnitName + " created successfully.";
                    }
                }
                try
                {
                    db.SaveChanges();
                    if (model.UnitId == 0)
                    {
                        ColumnId = db.UnitLists.Max(m => m.UnitId);
                    }
                }
                catch (Exception ex)
                {
                    string errorMsg = ex.Message.ToString();
                    if (model.UnitId > 0)
                    {
                        msg = (model.HasParent == true && model.ParentName != null) ?
                            "Sub unit " + model.UnitName + " of " + model.ParentName + " information update unsuccessful." :
                            "Unit " + model.UnitName + " information update unsuccessful.";
                    }
                    else
                    {
                        msg = (model.HasParent == true && model.ParentName != null) ?
                            "Add new unit " + model.UnitName + " of " + model.ParentName + " unsuccessful." :
                            "New unit " + model.UnitName + " create unsuccessful.";
                    }
                    OperationStatus = -1;
                }
                SaveAuditLog(opname, msg, "Inventory", "All Unit List", "UnitLists", ColumnId, model.CreatedBy, now, OperationStatus);
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
        public ActionResult UnitDestroy(int? UnitId, long UserId, string ParentName)
        {
            if (UnitId != null)
            {
                string msg = "";
                string name = "";
                int ColumnId = 0;
                int OperationStatus = 1;
                string value = "Success";
                UnitList unit = db.UnitLists.Find(UnitId);
                name = unit.UnitName;
                ColumnId = unit.UnitId;
                try
                {
                    var acq_unit_id = db.View_All_AcquisitionList.Where(m => m.UnitId == unit.UnitId).Select(m => m.UnitId).FirstOrDefault();
                    var inv_unit_id = db.InventoryLists.Where(m => m.UnitId == unit.UnitId).Select(m => m.UnitId).FirstOrDefault();
                    if (acq_unit_id == 0 && inv_unit_id == 0)
                    {
                        db.UnitLists.Remove(unit);
                        db.SaveChanges();
                        OperationStatus = 1;
                        msg = (ParentName != null) ? "Sub unit " + name + " of " + ParentName + " deleted successfully." :
                            "Unit " + name + " deleted successfully.";
                    }
                    else
                    {
                        msg = (ParentName != null) ? "Sub unit of " + ParentName + " delete attempt unsuccessful. " + name + " has been used." :
                            "Unit delete attempt unsuccessful. " + name + " has been used.";
                        value = "Unit " + name + " has been used.";
                        OperationStatus = -1;
                    }
                }
                catch (Exception ex)
                {
                    string errorMsg = ex.Message.ToString();
                    msg = (ParentName != null) ? "Sub unit " + name + " of " + ParentName + " delete attempt unsuccessful." :
                        "Unit " + name + " delete attempt unsuccessful.";
                    OperationStatus = -1;
                    value = "Error";
                }
                SaveAuditLog("Delete Unit", msg, "Inventory", "All Unit List", "UnitLists", ColumnId, UserId, now, OperationStatus);
                return Json(value, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json("Error", JsonRequestBehavior.AllowGet);
            }
        }
        #endregion
        #region STOCK
        public ActionResult StockItemList()
        {
            return View();
        }
        public PartialViewResult _StockItemList()
        {
            var stock = db.StockItemLists.ToList();
            return PartialView(stock);
        }
        #endregion


        #region
        public ActionResult Config()
        {
            return View();
        }
        #endregion
        //****************************** Dispatch Inventory Item Return List *********************************
        #region
        public ActionResult AllAcceptLogWithclick() {
            return View();
        }
        public PartialViewResult _AllAcceptLogWithclick() {
            var list = db.View_DisItemReturnDetails.Where(x => x.IsAccepted == true).ToList();
            return PartialView(list);
        }
        public ActionResult AllAcceptLog() {
            return View();
        }
        public PartialViewResult _AllAcceptLog(DateTime? StartDate,DateTime? EndDate,int? day) {
            if (day != null && day > 0) {
                StartDate = now.AddDays(-(day.Value));
                EndDate = now;
            }
            
            var list = db.View_DisItemReturnDetails.Where(x => x.IsAccepted == true).ToList();
            list = (StartDate != null && EndDate != null) ?list.Where(x=>x.ReturnDate.Value.Date!=null && x.ReturnDate.Value.Date>=StartDate.Value.Date && x.ReturnDate.Value.Date<=EndDate.Value.Date).ToList(): list;
            return PartialView(list);
        }
        public PartialViewResult ShowReturnReason(int Id) {
            var list = db.View_DisItemReturnDetails.Where(x => x.Id ==Id).FirstOrDefault();
            return PartialView(list);
        }
        public ActionResult AcceptLogPrint(int? id,DateTime? StartDate,DateTime? EndDate) {
            if (id != null && id > 0) {
                var list = db.View_DisItemReturnDetails.Where(x => x.Id == id).ToList();
                return View(list);
            }
            var list2 = db.View_DisItemReturnDetails.Where(x=>x.IsAccepted==true).ToList();
            list2 = (StartDate != null && EndDate != null) ? list2.Where(x => x.ReturnDate.Value.Date >= StartDate.Value.Date && x.ReturnDate.Value.Date <= EndDate.Value.Date).ToList():list2;
            return View(list2);
        }
        public PartialViewResult _ShowMachineDetailsById(int machineId) {
            var machineDetails = db.View_Machine_Line_Dept_Unit_List.Where(x=>x.MachineId==machineId).ToList();
            return PartialView(machineDetails);
        }
        public ActionResult DisItemReturnFromMachine()
        {          
            return View();
        }
        public PartialViewResult _DisItemReturnFromMachine(int? days,List<string> dispatch)
        {
            List<int> dispatchedId = new List<int>();
            if (dispatch!=null && dispatch.Count()>0) {
               
                foreach (var item in dispatch) {
                    dispatchedId.Add(Convert.ToInt32(item));
                }
            }

            var list = db.View_DisItemReturnDetails.Where(a => a.IsAccepted != true).GroupBy(c => new { c.DispatchedId, c.MachineId }).Select(x => new View_Dispacth_Item_DetailsModelView
            {
                Id = x.FirstOrDefault().Id,
                MachineId = x.FirstOrDefault().MachineId,
                ReturnFrom=x.FirstOrDefault().ReturnFrom,
                ReturnDateName=x.FirstOrDefault().ReturnDateName,
                ReturnUserName=x.FirstOrDefault().ReturnUserName,
                DispatchedId=x.FirstOrDefault().DispatchedId,
                Total_Quantity=x.Sum(c=>c.Quantity),
                InventoryId=x.FirstOrDefault().InventoryId,
                UnitName=db.View_DisItemReturnDetails.Where(c=>c.InventoryId==x.FirstOrDefault().InventoryId).Select(c=>c.UnitName).FirstOrDefault(),
                VoucherName =db.StoreItemDispatchedHistories.Where(z=>z.Id==x.FirstOrDefault().DispatchedId).Select(z=>z.VoucherName).FirstOrDefault(),
              ReturnTransactionName=x.FirstOrDefault().ReturnTransactionName
            }).ToList();
            list = (dispatchedId.Count() > 0) ? list.Where(x => dispatchedId.Contains(x.DispatchedId)).ToList():list;
           
            Session["AllReturnDisList"] = list;
            Session["TotalReturnDisList"] = list.Count();
            ViewBag.Count = list.Count();
            ViewBag.list = list.Take(ReturnDisPerPage).ToList();
            return PartialView();
        }
        public PartialViewResult _DisItemReturnFromMachineProductwise(List<string> dispatch) {
            List<int> inventoryId = new List<int>();
            if (dispatch != null && dispatch.Count() > 0) {
                foreach (var item in dispatch) {
                    inventoryId.Add(Convert.ToInt32(item));
                }
            }
            var data = db.View_DisItemReturnDetails.ToList();
            var list = data.Where(x => x.IsAccepted != true).GroupBy(c => c.InventoryId).Select(x => new View_DisItemReturnDetails
            {
                Id=x.FirstOrDefault().Id,
                ProductName=x.FirstOrDefault().ProductName,
                DispatchedId=x.FirstOrDefault().DispatchedId,
                InventoryId=x.FirstOrDefault().InventoryId,
                VoucherName=x.FirstOrDefault().VoucherName,
                Quantity=x.Sum(c=>c.Quantity),
                ReturnDateName=x.FirstOrDefault().ReturnDateName,
                
            }).ToList();
            list = (inventoryId.Count() > 0) ? list.Where(x => inventoryId.Contains(x.InventoryId)).ToList() : list;
            return PartialView(list);
        }
        public PartialViewResult _AllReturnVoucherofProduct(int inventoryId) {
            var data = db.View_DisItemReturnDetails.Where(x=>x.InventoryId==inventoryId &&x.IsAccepted!=true).ToList();
            var list = data.GroupBy(x => x.DispatchedId).Select(c => new View_DisItemReturnDetails {
              Id=c.FirstOrDefault().Id,
                ProductName=c.FirstOrDefault().ProductName,
                DispatchedId=c.FirstOrDefault().DispatchedId,
                Quantity=c.Sum(x=>x.Quantity),
                InventoryId=c.FirstOrDefault().InventoryId,
                VoucherName=c.FirstOrDefault().VoucherName,
                ReturnTransactionName=c.FirstOrDefault().ReturnTransactionName,
                ReturnUserName=c.FirstOrDefault().ReturnUserName,
                ReturnDateName=c.FirstOrDefault().ReturnDateName,ReturnFrom=c.FirstOrDefault().ReturnFrom
            }).ToList();
            return PartialView(list);
        }
        public JsonResult VoucherListForMultiSearch(int IsAccept)
        {
            var data = db.View_DisItemReturnDetails.ToList();
            var list = (IsAccept == 0) ? data.Where(m => m.IsAccepted !=true).GroupBy(c => c.DispatchedId).Select(x => new
            {
                VoucherName = x.FirstOrDefault().VoucherName,
                DispatchedId = x.FirstOrDefault().DispatchedId
            }).ToList() : data.Where(m => m.IsAccepted != true).GroupBy(c => c.InventoryId).Select(x => new
            {
                VoucherName = x.FirstOrDefault().ProductName,
                DispatchedId = x.FirstOrDefault().InventoryId
            }).ToList();
            return Json(list, JsonRequestBehavior.AllowGet);
        }
        public JsonResult GetReturnInvenItemDisHis(int? page)
        {
            page = page ?? 0;
            int total = Convert.ToInt16(Session["TotalReturnDisList"]);
            int skip = ReturnDisPerPage * Convert.ToInt16(page);
            var canPage = skip < total;
            if (canPage)
            {
                var listBackFromSession = (List<View_Dispacth_Item_DetailsModelView>)Session["AllReturnDisList"];
                var partslist = listBackFromSession.Skip(skip).Take(ReturnDisPerPage).ToList();
                return Json(partslist, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json("", JsonRequestBehavior.AllowGet);
            }
        }
        public PartialViewResult ShowDispatchHistoryDetailsById(int DispatchedId) {
            var data = db.View_DisItemReturnDetails.Where(x => x.DispatchedId == DispatchedId).ToList();
            return PartialView(data);
        }
        public PartialViewResult ShowInvenDispacthQuanDetails(int DispatchedId)
        {
            var dispatchedDetails = db.View_DisItemReturnDetails.Where(x=>x.Id== DispatchedId).ToList();
             dispatchedDetails = dispatchedDetails.GroupBy(
                x=>x.InventoryId
                ).Select(c=>new View_DisItemReturnDetails {
                    Id=c.FirstOrDefault().Id,
                    InventoryId=c.FirstOrDefault().InventoryId,
                    LocationId=c.FirstOrDefault().LocationId,
                    ProductName=c.FirstOrDefault().ProductName,
                    Name=c.FirstOrDefault().Name,
                    UnitName=c.FirstOrDefault().UnitName,
                    ReturnDateName=c.FirstOrDefault().ReturnDateName,
                    ReturnUserName=c.FirstOrDefault().ReturnUserName,
                    ReturnUserPic=c.FirstOrDefault().ReturnUserPic,
                    Quantity=c.Sum(x=>x.Quantity),
                    ReturnFrom=c.FirstOrDefault().ReturnFrom,
                    DispatchedId=c.FirstOrDefault().DispatchedId
                }).ToList();
            //if (DisId == null)
            //{
            //    ViewBag.hasdisId = false;
            //    DateTime countDate = DateTime.Today;
            //    var list = db.View_DisItemReturnDetails.Where(m => m.InventoryId == invenid).ToList();
            //    int day = Convert.ToInt32(days);
            //    if (day > 1)
            //    {
            //        countDate = countDate.AddDays(-(day));
            //    }
            //    if (day > 0)
            //    {
            //        list = list.Where(m => m.ReturnDate > countDate).OrderByDescending(m => m.ReturnDate).ToList();
            //    }
            //    return PartialView(list);
            //}
            //else
            //{
            //    ViewBag.hasdisId = true;
            //    var list = db.View_DisItemReturnDetails.Where(m => m.InventoryId == invenid && m.DispatchedId == DisId).ToList();
            //    return PartialView(list);
            //}
            return PartialView(dispatchedDetails);
        }
        
        public JsonResult AcceptReturnParts(long id,long userId)
        {
            if (id > 0)
            {
                var machine = db.View_DisItemReturnDetails.Where(m => m.Id == id).FirstOrDefault();
                //var Allparts = db.View_DisItemReturnDetails.Where(m => m.MachineId == machine.MachineId && m.InventoryId == machine.InventoryId).ToList();
                var Allparts = db.View_DisItemReturnDetails.Where(m => m.Id == id).ToList();
                foreach (var parts in Allparts)
                {
                    DispatchedItemList model = db.DispatchedItemLists.Find(parts.Dis_Item_Id);
                    if (model != null)
                    {
                        int status = -1;
                        string msg = "";
                        model.ReturnQuantity = machine.Quantity;
                        model.ReturnReasonId = parts.ReturnReasonId;
                        model.ReturnBy = userId;
                        model.ReturnDate = now;
                        db.Entry(model).State = EntityState.Modified;
                        try
                        {
                            db.SaveChanges();
                            status = 1;
                            msg = "Dispatch item '" + parts.ProductName + "' in voucher '" + parts.Name + "' quantity "+parts.Quantity+ " " +parts.UnitName+ " has been successfully accepted and returned on " + now.ToString("MMM dd , yyy hh:mm tt") + " .";
                        }
                        catch (Exception)
                        {
                            status = -1;
                            msg = "Dispatch item '" + parts.ProductName + "' in voucher '" + parts.Name + "' quantity " + parts.Quantity + " " + parts.UnitName + " accept was unsuccessful on " + now.ToString("MMM dd , yyy hh:mm tt") + " .";
                        }
                        SaveAuditLog("Dispatch Item Quantity Edit", msg, "Inventory", "DispatchHistoryDetails", "DispatchedItemList", Convert.ToInt32(model.Id), userId, now, status);
                        if (status == 1)
                        {
                            Inventory_Item_Location inven_loc = db.Inventory_Item_Location.Find(model.LocationId);
                            inven_loc.Quantity = machine.Quantity;
                            inven_loc.UpdatedDate = now;
                            inven_loc.UpdatedBy = userId;
                            db.Entry(inven_loc).State = EntityState.Modified;
                            try
                            {
                                db.SaveChanges();
                                status = 1;
                                msg = "Inventory item '" + parts.ProductName + "' quantity " + parts.Quantity + " " + parts.UnitName + " has been successfully updated on " + now.ToString("MMM dd , yyy hh:mm tt") + " .";
                            }
                            catch (Exception)
                            {
                                status = -1;
                                msg = "Inventory item '" + parts.ProductName + "' quantity  " + parts.Quantity + " " + parts.UnitName + " updated was unsuccessful on " + now.ToString("MMM dd , yyy hh:mm tt") + " .";
                            }
                            SaveAuditLog("Inventory Item Quantity Updated", msg, "Inventory", "DispatchHistoryDetails", "Inventory_Item_Location", Convert.ToInt32(inven_loc.LocationId), userId, now, status);

                           
                            DispatchedPartsReturnList dis_return = db.DispatchedPartsReturnLists.Find(parts.Id);
                            dis_return.AcceptBy = userId;
                            dis_return.AcceptQuan = model.ReturnQuantity;
                            dis_return.AcceptDate = now;
                            dis_return.IsAccepted = true;
                            db.Entry(dis_return).State = EntityState.Modified;
                           // db.DispatchedPartsReturnLists.Remove(dis_return);
                            db.SaveChanges();
                        }
                    }
                }
                return Json("Success",JsonRequestBehavior.AllowGet);
            }
            return Json("Error", JsonRequestBehavior.AllowGet);
        }
        #endregion



        //******************** Show All Maintenance Log *******************************************************
        #region
        public ActionResult AllMaintenanceLog()
        {
            return View();
        }
        public ActionResult _AllMaintenanceLog()
        {
            var list = db.View_Maintenance_Log.OrderByDescending(m => m.MaintenanceDate).ToList();
            Session["AllMaintenLog"] = list;
            Session["TotalMaintenLog"] = list.Count();
            ViewBag.Count = list.Count();
            list = list.Take(LogPerPage).ToList();
            return PartialView(list);
        }
        public PartialViewResult _AllMaintenanceLogDayWise(int days)
        {
            var list = db.View_Maintenance_Log.OrderByDescending(m => m.MaintenanceDate).ToList();
            DateTime countDate = now.Date;
            int day = Convert.ToInt32(days);
            if (day > 1)
            {
                countDate = countDate.AddDays(-(day));
            }
            if (day > 0)
            {
                list = list.Where(m => m.MaintenanceDate > countDate).OrderByDescending(m => m.MaintenanceDate).ToList();
            }
            Session["AllMaintenLog"] = list;
            Session["TotalMaintenLog"] = list.Count();
            ViewBag.Count = list.Count();
            list = list.Take(LogPerPage).ToList();
            return PartialView("_AllMaintenanceLog", list);
        }
        public JsonResult GetAllMaintenanceLog(int? page)
        {
            page = page ?? 0;
            int total = Convert.ToInt16(Session["TotalMaintenLog"]);
            int skip = LogPerPage * Convert.ToInt16(page);
            var canPage = skip < total;
            if (canPage)
            {
                var listBackFromSession = (List<View_Maintenance_Log>)Session["AllMaintenLog"];
                var partslist = listBackFromSession.Skip(skip).Take(LogPerPage).ToList();
                return Json(partslist, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json("", JsonRequestBehavior.AllowGet);
            }
        }
        #endregion


        //******************************* Unit Wise Cotton Dispatch List ***************************************
        #region
        [EncryptedActionParameter]
        public ActionResult UnitWiseCottonDispatchList(int? uId)
        {
            ViewBag.uId = uId;
            return View();
        }
        public ActionResult _UnitWiseCottonDispatchList(int? uId)
        {
            var list = db.View_Dispacth_Item_Details.Where(m => m.Site_Unit_Id > 0).OrderByDescending(m => m.DispatchedDate).ToList();
            if (uId > 0)
            {
                list = list.Where(m => m.Site_Unit_Id == uId).ToList();
            }
            Session["AllUDisList"] = list;
            Session["TotalUDisList"] = list.Count();
            ViewBag.Count = list.Count();
            ViewBag.tab = uId;
            list = list.Take(UDisPerPage).ToList();
            return PartialView(list);
        }
        public PartialViewResult _UnitWiseCottonDispatchListDayWise(int? days, int? uId, DateTime? from, DateTime? to)
        {
            var list = db.View_Dispacth_Item_Details.Where(m => m.Site_Unit_Id > 0).OrderByDescending(m => m.DispatchedDate).ToList();
            if (uId > 0)
            {
                list = list.Where(m => m.Site_Unit_Id == uId).ToList();
            }
            if (days > 0 && days != null)
            {
                DateTime countDate = now.Date;
                int day = Convert.ToInt32(days);
                countDate = countDate.AddDays(-(day));
                list = list.Where(m => m.DispatchedDate.Value.Date > countDate).OrderByDescending(m => m.DispatchedDate).ToList();
            }
            if (from != null && to != null)
            {
                list = list.Where(m => m.DispatchedDate.Value.Date >= from.Value.Date && m.DispatchedDate.Value.Date <= to.Value.Date).OrderByDescending(m => m.DispatchedDate).ToList();
            }
            Session["AllUDisList"] = list;
            Session["TotalUDisList"] = list.Count();
            ViewBag.tab = uId;
            ViewBag.Count = list.Count();
            list = list.Take(UDisPerPage).ToList();
            return PartialView("_UnitWiseCottonDispatchList", list);
        }
        public JsonResult GetAllUnitWiseCottonDispatchList(int? page)
        {
            page = page ?? 0;
            int total = Convert.ToInt16(Session["TotalUDisList"]);
            int skip = UDisPerPage * Convert.ToInt16(page);
            var canPage = skip < total;
            if (canPage)
            {
                var listBackFromSession = (List<View_Dispacth_Item_Details>)Session["AllUDisList"];
                var partslist = listBackFromSession.Skip(skip).Take(UDisPerPage).ToList();
                return Json(partslist, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json("", JsonRequestBehavior.AllowGet);
            }
        }
        #endregion


        //****************************** Unit Wise Spare Parts Dispatch List ***********************************
        #region
        public ActionResult SparePartsDisList()
        {
            return View();
        }
        public PartialViewResult machineAssignedwithParts(int partsId) {
            var data = db.View_Dispacth_Item_Details.Where(x => x.InventoryId == partsId).ToList();
            var list = data.GroupBy(x => x.MachineId).Select(c => new View_Dispacth_Item_Details
            {
                Id=c.FirstOrDefault().Id,
                DispatchedId=c.FirstOrDefault().DispatchedId,
                MachineId=c.FirstOrDefault().MachineId,
                MachineName=c.FirstOrDefault().MachineName,
                Quantity=c.Sum(x=>x.Quantity),
                ProductName=c.FirstOrDefault().ProductName,
              UnitName=c.FirstOrDefault().UnitName,
              Site_Unit_Name=c.FirstOrDefault().Site_Unit_Name
                
            }).ToList();
            return PartialView(list);
        }
        public PartialViewResult _SparePartsDisList(string v, int? uId, string machinename, string partsname,List<string> inventory)
        {
            List<int> inventoryIds = new List<int>();
            if (inventory != null && inventory.Count() > 0) {
                foreach (var item in inventory) {
                    inventoryIds.Add(Convert.ToInt32(item));
                }
            }
            var list = db.View_Dispacth_Item_Details.Where(m => m.AssignType == 1 && m.MachineId > 0).OrderByDescending(m => m.DispatchedDate).ToList();
            list = (from bs in list
                    group bs by new {bs.InventoryId } into g
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
                        Quantity = g.Sum(m => m.Quantity),
                        UsedQuan = g.Sum(x => x.UsedQuan),
                        ReturnQuantity = g.Sum(x => x.ReturnQuantity),
                    }).OrderBy(o => o.MachineName).ToList();
            if (uId > 0)
            {
                var alldeptId = db.DepartmentLists.Where(m => m.UnitId == uId).Select(m => m.DeptId).ToList();
                var allLine = db.LineInfoes.Where(m => alldeptId.Contains(m.DeptId)).Select(m => m.LineId).ToList();
                var allmachine = db.LineMachineLists.Where(m => allLine.Contains(m.LineId)).Select(m => m.MachineId).ToList();
                list = list.Where(m => allmachine.Contains(m.MachineId.Value)).ToList();
            }
            if (machinename != null)
            {
                list = (from m in list where (m.MachineName.ToLower().Contains(machinename.ToLower())) select m).ToList();
            }
            if (partsname != null)
            {
                list = (from m in list where (m.ProductName.ToLower().Contains(partsname.ToLower())) select m).ToList();
            }
            list = (inventoryIds.Count() > 0) ? list.Where(x => inventoryIds.Contains(x.InventoryId)).ToList() : list;
            ViewBag.v = v;
            ViewBag.uId = Convert.ToInt32(uId);
            Session["AllAvailSpareParts"] = list;
            return PartialView(list);
        }
        public JsonResult AvailableSparePartsForSearch() {
            var data = db.View_Dispacth_Item_Details.ToList();
            var list = data.GroupBy(c => c.InventoryId).Select(x => new {
                ProductName=x.FirstOrDefault().ProductName,
                InventoryId = x.FirstOrDefault().InventoryId
            }).ToList();
            return Json(list, JsonRequestBehavior.AllowGet);
        }
        public JsonResult AvailMachinePartsSearching(string prefix)
        {
            if (prefix == "")
            {
                return Json("", JsonRequestBehavior.AllowGet);
            }
            else
            {
                var allMachine = (List<View_Dispacth_Item_Details>)Session["AllAvailSpareParts"];
                var list = (from m in allMachine
                            where (m.MachineName.ToLower().Contains(prefix.ToLower()))
                            select new View_Dispacth_Item_Details
                            {
                                MachineId = m.MachineId,
                                MachineName = m.MachineName
                            }).OrderBy(o => o.MachineName).ToList();
                return Json(list, JsonRequestBehavior.AllowGet);
            }
        }
        #endregion

        //******************************** Add New Machine Type **************************************************
        #region
        public ActionResult MachineTypeAdd()
        {
            return View();
        }
        public PartialViewResult _ShowAllMachineType(int? MachineId)
        {

            if (MachineId != null && MachineId > 0)
            {
                var AllMachineType = (from p in db.View_Machine_Type_Name
                                      where p.MachineId == MachineId
                                      select new MachineTypeNameModelView
                                      {
                                          MachineId = p.MachineId,
                                          MachineTypeName1 = p.MachineTypeName,
                                          CreatedBy = p.CreatedBy,
                                          CreatedDateName = p.CreatedDateName,
                                          CreatedUserName = p.CreatedUserName,
                                          CreatedUserPic = p.CreatedUserPic,
                                          UpdatedDate = p.UpdatedDate,
                                          UpdatedBy = p.UpdatedBy,
                                          UpdatedDateFormate = p.UpdatedDateFormate,
                                          UpdatedUserName = p.UpdatedUserName,
                                          UpdatedUserPic = p.UpdatedUserPic,
                                      }).OrderBy(p => p.MachineTypeName1).ToList();
                Session["AllMachinTypeList"] = AllMachineType;
                ViewBag.AllMachineType = AllMachineType;

            }
            else
            {
                var AllMachineType = (from p in db.View_Machine_Type_Name
                                      select new MachineTypeNameModelView
                                      {
                                          MachineId = p.MachineId,
                                          MachineTypeName1 = p.MachineTypeName,
                                          CreatedBy = p.CreatedBy,
                                          CreatedDateName = p.CreatedDateName,
                                          CreatedUserName = p.CreatedUserName,
                                          CreatedUserPic = p.CreatedUserPic,
                                          UpdatedDate = p.UpdatedDate,
                                          UpdatedBy = p.UpdatedBy,
                                          UpdatedDateFormate = p.UpdatedDateFormate,
                                          UpdatedUserName = p.UpdatedUserName,
                                          UpdatedUserPic = p.UpdatedUserPic,
                                      }).OrderBy(p => p.MachineTypeName1).ToList();
                Session["AllMachinTypeList"] = AllMachineType;
                ViewBag.AllMachineType = AllMachineType;
            }

            return PartialView();
        }
        //public JsonResult SearchingMachineTypeName(string prefix)
        //{
        //    if (prefix == "")
        //    {
        //        return Json("", JsonRequestBehavior.AllowGet);
        //    }
        //    else
        //    {
        //        var allMachine = (List<MachineTypeNameModelView>)Session["AllMachinTypeList"];
        //        var list = (from m in allMachine
        //                    where (m.MachineTypeName1.ToLower().Contains(prefix.ToLower()))
        //                    select new MachineTypeNameModelView
        //                    {
        //                        MachineId = m.MachineId,
        //                        MachineTypeName1 = m.MachineTypeName1,
        //                        CreatedBy = m.CreatedBy,
        //                        CreatedDateName = m.CreatedDateName,
        //                        CreatedUserName = m.CreatedUserName,
        //                        CreatedUserPic = m.CreatedUserPic,
        //                        UpdatedDate = m.UpdatedDate,
        //                        UpdatedBy = m.UpdatedBy,
        //                        UpdatedDateFormate = m.UpdatedDateFormate,
        //                        UpdatedUserName = m.UpdatedUserName,
        //                        UpdatedUserPic = m.UpdatedUserPic,
        //                    }).OrderBy(p => p.MachineTypeName1).ToList();
        //        return Json(list, JsonRequestBehavior.AllowGet);
        //    }
        //}
        public PartialViewResult CreateNewMachineType(int? MachineId)
        {
            MachineTypeNameModelView model = new MachineTypeNameModelView();

            if (MachineId != null)
            {
                View_Machine_Type_Name pro_type = db.View_Machine_Type_Name.Where(m => m.MachineId == MachineId).FirstOrDefault();
                model.MachineId = pro_type.MachineId;
                model.MachineTypeName1 = pro_type.MachineTypeName;
            }
            return PartialView(model);
        }
        public JsonResult MachineTypeCreateSave(MachineTypeNameModelView model)
        {
            if (ModelState.IsValid)
            {
                string msg = "";
                string opname = "";
                int ColumnId = 0;
                int OperationStatus = 1;

                MachineTypeName pro_type = new MachineTypeName();
                if (model.MachineId > 0)
                {
                    pro_type = db.MachineTypeNames.Find(model.MachineId);
                    pro_type.MachineTypeName1 = model.MachineTypeName1;
                    pro_type.UpdatedBy = model.CreatedBy;
                    pro_type.UpdatedDate = now;
                    db.Entry(pro_type).State = EntityState.Modified;
                    ColumnId = pro_type.MachineId;
                    msg = "Machine type '" + model.MachineTypeName1 + "' information has been successfully updated on " + now.ToString("MMM dd , yyy hh:mm tt") + " .";
                    opname = "Update Machine Type Information";
                }
                else
                {
                    pro_type.MachineTypeName1 = model.MachineTypeName1;
                    pro_type.CreatedBy = model.CreatedBy;
                    pro_type.CreatedDate = now;

                    db.MachineTypeNames.Add(pro_type);

                    opname = "Create Machine Type";
                    msg = "New machine type '" + model.MachineTypeName1 + "' has been successfully created on " + now.ToString("MMM dd , yyy hh:mm tt") + " .";

                }
                try
                {
                    db.SaveChanges();
                    if (model.MachineId == 0)
                    {
                        ColumnId = db.MachineTypeNames.Max(m => m.MachineId);
                    }
                }
                catch (Exception ex)
                {
                    string errorMsg = ex.Message.ToString();
                    if (model.MachineId > 0)
                    {
                        msg = "Machine type " + model.MachineTypeName1 + " information update unsuccessful on " + now.ToString("MMM dd , yyy hh:mm tt") + " .";
                    }
                    else
                    {
                        msg = "New machine type " + model.MachineTypeName1 + " create unsuccessful on " + now.ToString("MMM dd , yyy hh:mm tt") + " .";
                    }
                    OperationStatus = -1;
                }
                SaveAuditLog(opname, msg, "Inventory", "Machine Type List", "MachineTypeName", ColumnId, model.CreatedBy, now, OperationStatus);
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