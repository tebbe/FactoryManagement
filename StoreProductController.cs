using FactoryManagement.Filters;
using FactoryManagement.Models;
using FactoryManagement.ModelView.Acquisition;
using FactoryManagement.ModelView.Inventory;
using FactoryManagement.ModelView.Inventory.Product;
using FactoryManagement.ModelView.Inventory.Store;
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
    [DisplayName("Store Management")]
    public class StoreProductController : Controller
    {
        #region Private Properties
        private FactoryManagementEntities db = new FactoryManagementEntities();
        private FactoryManagementEntities dbAud = new FactoryManagementEntities();
        int ShowItemPerPage = 20;
        const int DisHisPerPage = 50;
        const int DisVouPerPage = 50;
        const int ImportPerPage = 50;
        const int ImportPerPageLC = 50;
        const int RouteVouPerPage = 50;
        const int DisMAchineWiseHisPerPage = 50;
        const int DisDeptWiseHisPerPage = 50;
        const int PerPageProArch = 20;
        static private int offset = Convert.ToInt32(ConfigurationManager.AppSettings["localTime"]);
        DateTime now = DateTime.UtcNow.AddMinutes(offset);
        #endregion

        //*******************************************************************************************************************
        //******************************************** INVENTORY ITEM INSERT ************************************************
        //*******************************************************************************************************************
        #region Inventory Item Insert
        public ActionResult InventoryItemInsert(int? id, int? StoreId, int? wareId)
        {
            int currency = db.FactoryInformations.Select(m => m.CurrencyId).FirstOrDefault();
            var symbol = db.CurrencyLists.Where(m => m.CurrencyId == currency).Select(m => m.CurrencySymbol).FirstOrDefault();
            ViewBag.getCurrency = symbol;

            InventoryListModelView model = new InventoryListModelView();
            if (id != null)
            {
                int inventoryId = Convert.ToInt32(id);
                View_All_InventoryList value = db.View_All_InventoryList.Where(m => m.InventoryId == inventoryId).FirstOrDefault();
                model.InventoryId = value.InventoryId;
                model.ProductName = value.ProductName;
                model.ProductTypeId = value.ProductTypeId;

                model.Brand = value.BrandName;
                model.BrandId = value.BrandId;
                model.ModelName = value.Model;
                model.Quantity =Convert.ToDouble(value.Quantity);
                model.UnitId = value.UnitId;
                model.Country = value.Country;
                model.CanbeBreakable = value.CanbeBreakable;
                if (model.CanbeBreakable)
                {
                    model.SubQuantity = value.SubQuantity;
                    model.SubUnits = value.SubUnitId;
                }
                model.Price = value.Price;
                model.CanbeSold = Convert.ToBoolean(value.CanbeSold);
                model.CanbeOrdered = Convert.ToBoolean(value.CanbeOrdered);
                model.CanbeRepaired = Convert.ToBoolean(value.CanbeRepaired);
                model.CanbePerishable = Convert.ToBoolean(value.CanbePerishable);
                if (model.CanbeRepaired)
                {
                    if (Convert.ToBoolean(value.HasReplacementWarranty))
                    {
                        model.WarrantyFor = "1";
                    }
                    else
                    {
                        model.WarrantyFor = "2";
                    }
                    model.WarrantyDay = value.WarrantyDays;
                    model.IsWarrantyStart = Convert.ToBoolean(value.IsWarrantyStartNow);
                }
                model.ExpiredDate = value.ExpiredDate;
                model.Description = value.Description;
                model.InMultipleLocation = value.InMultipleLocation;
                model.LocationId = value.LocationId;
                if (model.InMultipleLocation == 0)
                {
                    model.StoreId = db.Inventory_Item_Location.Where(m => m.InventoryId == model.InventoryId).Select(m => m.StoreId).FirstOrDefault();
                    model.WareId = db.Inventory_Item_Location.Where(m => m.InventoryId == model.InventoryId).Select(m => m.WarehouseId).FirstOrDefault();
                }
            }
            else
            {
                model.StoreId = StoreId;
                model.WareId = wareId;
            }
            ViewBag.StoreId = new SelectList(db.StoreLists, "Id", "StoreName", model.StoreId);
            ViewBag.WareId = new SelectList(db.WarehouseLists, "Id", "WarehouseName", model.WareId);
            ViewBag.ProductTypeId = new SelectList(db.ProductTypeLists.Where(m => m.HasParent == false).OrderBy(m => m.ProductTypeName), "ProductTypeId", "ProductTypeName", model.ProductTypeId);
            ViewBag.BrandId = new SelectList(db.BrandLists.Where(m => m.HasParent == false).OrderBy(m => m.BrandName), "BrandId", "BrandName", model.BrandId);
            ViewBag.BeakableUnit = new SelectList(db.UnitLists.OrderBy(m => m.UnitName), "UnitId", "UnitName", model.SubUnits);
            ViewBag.UnitId = new SelectList(db.UnitLists.Where(m => m.HasParentId == false).OrderBy(m => m.UnitName), "UnitId", "UnitName", model.UnitId);
            ViewBag.SubUnitId = new SelectList(db.UnitLists.Where(m => m.HasParentId == false).OrderBy(m => m.UnitName), "UnitId", "UnitName", model.SubUnits);
            ViewBag.Country = new SelectList(db.CountryLists.OrderBy(m => m.CountryName), "Id", "CountryName", model.Country);
            if (id > 0)
            {
                return PartialView("_StoreProEdit", model);
            }
            else
            {
                return View(model);
            }
        }
        public JsonResult InventoryItemSave(InventoryListModelView invenModel, IEnumerable<HttpPostedFileBase> files)
        {
            if (ModelState.IsValid)
            {
                string msg = "";
                string opname = "Inventory New Item Add";
                int ColumnId = 0;
                int OperationStatus = 1;

                InventoryList inlist = new InventoryList();
                if ((invenModel.BrandId == 0 || invenModel.BrandId == null) && invenModel.Brand != null)
                {
                    BrandList brand = new BrandList();
                    brand.BrandName = invenModel.Brand;
                    brand.HasParent = false;
                    brand.CreatedDate = now;
                    brand.CreatedBy = invenModel.CreatedBy;
                    db.BrandLists.Add(brand);
                    db.SaveChanges();
                    invenModel.BrandId = db.BrandLists.Where(m => m.CreatedBy == invenModel.CreatedBy).Max(m => m.BrandId);
                }
                inlist.ProductName = invenModel.ProductName;
                inlist.ProductTypeId = invenModel.ProductTypeId;
                inlist.Country = invenModel.Country;
                inlist.CanbeBreakable = invenModel.CanbeBreakable;
                inlist.CanbeSold = invenModel.CanbeSold;
                inlist.CanbeOrdered = invenModel.CanbeOrdered;
                inlist.CanbeRepaired = invenModel.CanbeRepaired;
                inlist.CanbeReplaceable = invenModel.CanbeReplaceable;
                inlist.CanbePerishable = invenModel.CanbePerishable;
                inlist.CanbeTopup = invenModel.CanbeTopUp;
                inlist.Quantity = 0;
                inlist.UnitId = invenModel.UnitId;
                inlist.Description = invenModel.Description;
                inlist.ExpiredDate = invenModel.ExpiredDate;
                inlist.Brand = invenModel.Brand;
                inlist.BrandId = invenModel.BrandId;
                inlist.Model = invenModel.ModelName;
                inlist.Price = invenModel.Price;
                inlist.InternalReferenceNo = now.ToString("yyyyMMddmmss").ToString() + "" + invenModel.CreatedBy;
                if (inlist.CanbeBreakable)
                {
                    inlist.SubQuantity = invenModel.SubQuantity;
                    inlist.SubUnitId = invenModel.SubUnits;
                }
                if (invenModel.ExpiredDate != null)
                {
                    DateDifference dateDifference = new DateDifference(invenModel.ExpiredDate.Value, now);
                    inlist.WarrantyDay = dateDifference.Days;
                    inlist.WarrantyMonth = dateDifference.Months;
                    inlist.WarrantyYear = dateDifference.Years;
                }
                else
                {
                    inlist.WarrantyDay = 0;
                    inlist.WarrantyMonth = 0;
                    inlist.WarrantyYear = 0;
                }

                inlist.CreatedDate = now;
                inlist.CreatedBy = invenModel.CreatedBy;
                db.InventoryLists.Add(inlist);
                try
                {
                    db.SaveChanges();
                    invenModel.InventoryId = db.InventoryLists.Where(m => m.CreatedBy == invenModel.CreatedBy).Max(m => m.InventoryId);
                    msg = "New item '"+inlist.ProductName+"' has been successfully added into inventory on "+now+" .";
                    ColumnId = invenModel.InventoryId;

                    int imgCount = 0;
                    if (files != null)
                    {
                        foreach (var file in files)
                        {
                            if (file != null && file.ContentLength > 0)
                            {
                                string s = file.FileName;
                                int idx = s.LastIndexOf('.');
                                string fileName = s.Substring(0, idx);
                                string extension = s.Substring(idx);

                                string picName = now.ToString("yyyyMMddmmss").ToString()+invenModel.InventoryId + "_" + invenModel.CreatedBy + imgCount + extension;
                                string PathForOriginal = Path.Combine(Server.MapPath("~/Images/Inventory/Store/Original"), picName);
                                file.SaveAs(PathForOriginal);

                                var imgForGallery = Image.FromStream(file.InputStream, true, true);
                                using (var resizeImg = ScaleImage(imgForGallery, 150, 150))
                                {
                                    string Paths = Path.Combine(Server.MapPath("~/Images/Inventory/Store/Resize"), picName);
                                    resizeImg.Save(Paths);
                                }
                                Inventory_Image_Lists pic = new Inventory_Image_Lists();
                                pic.InventoryId = invenModel.InventoryId;
                                pic.ImageName = picName;
                                db.Inventory_Image_Lists.Add(pic);
                                db.SaveChanges();
                            }
                            imgCount++;
                        }
                    }
                }
                catch (Exception)
                {
                    msg = "New item '" + inlist.ProductName + "' addition was unsuccessful.";
                    OperationStatus = -1;
                    ColumnId = 0;
                }
                SaveAuditLog(opname, msg, "Inventory", "Store", "InventoryList", ColumnId, invenModel.CreatedBy, now, OperationStatus);
                if (OperationStatus == 1)
                {
                    return Json(invenModel, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json("Error", JsonRequestBehavior.AllowGet);
                }
            }
            return Json("Error", JsonRequestBehavior.AllowGet);
        }
        public ActionResult InventoryItemLocationSet(long? id)
        {
            View_All_InventoryList model = new View_All_InventoryList();
            model.InventoryId =Convert.ToInt32(id);
            if (id > 0)
            {
                model = db.View_All_InventoryList.Where(m => m.InventoryId == id).FirstOrDefault();
                if (model == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }
            } 
            return View(model);
        }
        public PartialViewResult ProductDetailsofAutocomplete(int? inventoryId)
        {
            View_All_InventoryList data = new View_All_InventoryList();
            if (db.InventoryLists.Where(x => x.InventoryId == inventoryId).Any())
            {
                data = db.View_All_InventoryList.Where(x => x.InventoryId == inventoryId).FirstOrDefault();
                return PartialView(data);
            }
            return PartialView(data);
        }
        public JsonResult GetMinimumQuantity(int InventoryId, int storewareId, bool IsStore)
        {
            if (db.Inventory_Item_Location.Where(x => x.InventoryId == InventoryId && (x.WarehouseId == storewareId || x.StoreId == storewareId)).Any())
            {
                if (IsStore)
                {
                    var data = db.Inventory_Item_Location.Where(x => x.InventoryId == InventoryId && x.StoreId == storewareId).Select(x => new { x.Quantity, x.MinimumQuantity }).FirstOrDefault();
                    return Json(data, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    var data = db.Inventory_Item_Location.Where(x => x.InventoryId == InventoryId && x.WarehouseId == storewareId).Select(x => new { x.Quantity, x.MinimumQuantity }).FirstOrDefault();
                    return Json(data, JsonRequestBehavior.AllowGet);
                }
            }
            else
            {
                return Json("Error", JsonRequestBehavior.AllowGet);
            }
        }
        public JsonResult InventoryLocSave(Import_History model, string AllLocId, decimal[] AllQuan, decimal[] AllMinQuan, int UnitId)
        {
            if (AllLocId != "")
            {
                long impid = 0;
                bool loc_exsists = false;
                string[] allLoc = AllLocId.Split(',');
                string msg = "";
                string opname = "Inventory Item Store";
                var saveStatus = -1;
                var inven_details = db.View_All_InventoryList.Where(x => x.InventoryId == model.InventoryId).FirstOrDefault();
                model.InsertedDate = now;
                int lastCountDis = 0;
                if (db.Import_History.Any())
                {
                    lastCountDis = db.Import_History.Count();
                }
                if (db.Import_History.Any() && db.Import_History.Where(m => m.L_C == model.L_C && m.InventoryId == model.InventoryId).Any())
                {
                    Import_History imp_his = db.Import_History.Where(m => m.L_C == model.L_C && m.InventoryId == model.InventoryId).FirstOrDefault();
                    imp_his.Quantity = imp_his.Quantity + model.Quantity;
                    db.Entry(imp_his).State = EntityState.Modified;
                    try
                    {
                        db.SaveChanges();
                        saveStatus = 1;
                        model.ParentId = imp_his.ImportId;
                        impid = imp_his.ImportId;
                    }
                    catch (Exception)
                    {
                        saveStatus = -1;
                    }
                    if (saveStatus == 1)
                    {
                        lastCountDis += 1;
                        model.TransactionName = "IMT_" + ((lastCountDis < 1000) ? lastCountDis.ToString("000") : lastCountDis.ToString());
                        model.ParentId = imp_his.ImportId;
                        try
                        {
                            db.Import_History.Add(model);
                            db.SaveChanges();
                            saveStatus = 1;
                            msg = "New Inventory item '" + inven_details.ProductName + "' with quantity '" + model.Quantity + "' has been succesfully imported on " + now.ToString("MMM dd, yyyy hh:mm tt") + ".";
                        }
                        catch (Exception)
                        {
                            msg = "New Inventory item '" + inven_details.ProductName + "' with quantity '" + model.Quantity + "'s import was unsuccessful on " + now.ToString("MMM dd, yyyy hh:mm tt") + ".";
                            SaveAuditLog("Import Inventory Item", msg, "Inventory", "Store", "Import_History", model.InventoryId, model.InsertedBy, now, saveStatus);
                            return Json("Error", JsonRequestBehavior.AllowGet);
                        }
                        SaveAuditLog("Import Inventory Item", msg, "Inventory", "Store", "Import_History", Convert.ToInt32(model.ImportId), model.InsertedBy, now, saveStatus);
                    }
                }
                else
                {
                    lastCountDis += 1;
                    model.TransactionName = "IMT_" + ((lastCountDis < 1000) ? lastCountDis.ToString("000") : lastCountDis.ToString());
                    try
                    {
                        db.Import_History.Add(model);
                        db.SaveChanges();
                        impid = model.ImportId;
                        saveStatus = 1;
                        msg = "New Inventory item '" + inven_details.ProductName + "' with quantity '" + model.Quantity + "' has been succesfully imported on " + now.ToString("MMM dd, yyyy hh:mm tt") + ".";
                    }
                    catch (Exception)
                    {
                        msg = "New Inventory item '" + inven_details.ProductName + "' with quantity '" + model.Quantity + "'s import was unsuccessful on " + now.ToString("MMM dd, yyyy hh:mm tt") + ".";
                        SaveAuditLog("Import Inventory Item", msg, "Inventory", "Store", "Import_History", model.InventoryId, model.InsertedBy, now, saveStatus);
                        return Json("Error", JsonRequestBehavior.AllowGet);
                    }
                    SaveAuditLog("Import Inventory Item", msg, "Inventory", "Store", "Import_History", Convert.ToInt32(model.ImportId), model.InsertedBy, now, saveStatus);
                }
                msg = "";
                if (saveStatus == 1)
                {
                    for (int i = 0; i < allLoc.Length; i++)
                    {
                        string locname = "";
                        int status = -1;
                        int id = 0;

                        Import_Item_Location import_location = new Import_Item_Location();
                        import_location.ImportId = model.ImportId;
                        import_location.Quantity = Convert.ToDouble(AllQuan[i]);
                        import_location.ParentId = model.ParentId;

                        Inventory_Item_Location item_location = new Inventory_Item_Location();
                        item_location.InventoryId = model.InventoryId;
                        if (allLoc[i].Split('-')[1] == "s")
                        {  
                            int storeid =  Convert.ToInt32(allLoc[i].Split('-')[0]);
                            if (db.Inventory_Item_Location.Where(m => m.ImportId == impid && m.InventoryId == model.InventoryId && m.StoreId == storeid).Any())
                            {
                                item_location = db.Inventory_Item_Location.Where(m => m.ImportId == impid && m.InventoryId == model.InventoryId && m.StoreId == storeid).FirstOrDefault();
                                loc_exsists = true;
                            }
                            else
                            {
                                item_location.StoreId = storeid;
                            }
                            import_location.StoreId = storeid;
                            locname = db.StoreLists.Where(m => m.Id == item_location.StoreId).Select(m => m.StoreName).FirstOrDefault();
                        }
                        else if (allLoc[i].Split('-')[1] == "w")
                        {
                            int wareid = Convert.ToInt32(allLoc[i].Split('-')[0]);
                            if (db.Inventory_Item_Location.Where(m => m.ImportId == impid && m.InventoryId == model.InventoryId && m.WarehouseId == wareid).Any())
                            {
                                item_location = db.Inventory_Item_Location.Where(m => m.ImportId == impid && m.InventoryId == model.InventoryId && m.WarehouseId == wareid).FirstOrDefault();
                                loc_exsists = true;
                            }
                            else
                            {
                                item_location.WarehouseId = wareid;
                            }
                            import_location.WareId = Convert.ToInt32(allLoc[i].Split('-')[0]);
                            locname = db.WarehouseLists.Where(m => m.Id == item_location.WarehouseId).Select(m => m.WarehouseName).FirstOrDefault();
                        }
                        else if (allLoc[i].Split('-')[1] == "d")
                        {
                            item_location.DeptId = Convert.ToInt32(allLoc[i].Split('-')[0]);
                            locname = db.DepartmentLists.Where(m => m.DeptId == item_location.DeptId).Select(m => m.DeptName).FirstOrDefault();
                        }
                        else
                        {
                            item_location.LineId = Convert.ToInt32(allLoc[i].Split('-')[0]);
                            locname = db.LineInfoes.Where(m => m.LineId == item_location.LineId).Select(m => m.LineAcronym).FirstOrDefault();
                        }
                        if (loc_exsists)
                        {
                            item_location.Quantity = item_location.Quantity+Convert.ToDouble(AllQuan[i]);
                            item_location.InsertedDate = now;
                            item_location.InsertedBy = model.InsertedBy;
                        }
                        else
                        {
                            item_location.ImportId = model.ImportId;
                            item_location.Quantity = Convert.ToDouble(AllQuan[i]);
                            item_location.MinimumQuantity = Convert.ToDouble(AllMinQuan[i]);
                            item_location.UnitId = inven_details.UnitId;
                            item_location.InsertedDate = now;
                            item_location.InsertedBy = model.InsertedBy; 
                        }
                        import_location.InsertedDate = now;
                        import_location.InsertedBy = model.InsertedBy;
                        db.Import_Item_Location.Add(import_location);
                        try
                        {
                            db.SaveChanges();
                            if (loc_exsists)
                            {
                                db.Entry(item_location).State = EntityState.Modified;
                            }
                            else
                            {
                                db.Inventory_Item_Location.Add(item_location);
                            }
                            try
                            {
                                db.SaveChanges();
                                msg = "Inventory item '" + inven_details.ProductName + "'  quantity " + AllQuan[i] + " " + inven_details.UnitName + " has been successfully stored into '" + locname + "'  on " + now.ToString("MMM dd, yyyy hh:mm tt") + ".";
                                status = 1;
                                id = item_location.LocationId;
                            }
                            catch (Exception)
                            {
                                status = -1;
                                msg = "Inventory Item '" + inven_details.ProductName + "'  quantity " + AllQuan[i] + " " + inven_details.UnitName + " store into '" + locname + "'  was unsuccessful on " + now.ToString("MMM dd, yyyy hh:mm tt") + ".";
                            }
                            SaveAuditLog(opname, msg, "Inventory", "Store", "Inventory_Item_Location", id, inven_details.CreatedBy, now, status);
                        }
                        catch (Exception)
                        {
                            status = -1;
                        }
                       // SaveAuditLog("Import Inventory Item ", msg, "Inventory", "Store", "Import_Item_Location", id, model.InsertedBy, now, status);
                    }
                    return Json("Success", JsonRequestBehavior.AllowGet);
                } 
            }
            return Json("Error", JsonRequestBehavior.AllowGet);
        }
        public JsonResult GetAllStoreWare()
        {
            var storeList = (from m in db.StoreLists
                             select new
                             {
                                 Id = m.Id,
                                 IdWithChar = m.Id + "-s",
                                 Name = m.StoreName,
                                 IsStore = true
                             }).ToList();

            var wareList = (from m in db.WarehouseLists
                            select new
                            {
                                Id = m.Id,
                                IdWithChar = m.Id + "-w",
                                Name = m.WarehouseName,
                                IsStore = false,
                            }).ToList();
            foreach (var ware in wareList)
            {
                storeList.Add(new { Id = ware.Id, IdWithChar = ware.IdWithChar, Name = ware.Name, IsStore = ware.IsStore });
            }
            return Json(storeList, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region
        public ActionResult InventoryMinQuanFinished(string v)
        {
            ViewBag.v = v;
            return View();
        }
        public PartialViewResult _InventoryMinQuanFinished(string v)
        {
            var list = db.View_Inventory_Finish_Item_List.OrderBy(m => m.FinishedDate).ToList();
            if (v == "ware")
            {
                list = list.Where(m => m.WarehouseId > 0).ToList();
            }
            if (v == "store")
            {
                list = list.Where(m => m.StoreId > 0).ToList();
            }
            return PartialView(list);
        }
        public PartialViewResult QuantityLocation(int? id)
        {
            ViewBag.id = id;
            return PartialView();
        }
        #endregion

        #region
        public PartialViewResult StoreProductEdit(int id)
        {
            ViewBag.id = id;
            return PartialView();
        }
        public PartialViewResult _StoreProEdit(int id)
        {
            InventoryListModelView model = new InventoryListModelView();
            View_All_InventoryList value = db.View_All_InventoryList.Where(m => m.InventoryId == id).FirstOrDefault();
            model.InventoryId = value.InventoryId;
            model.ProductName = value.ProductName;
            model.ProductTypeId = value.ProductTypeId;

            model.Brand = value.BrandName;
            model.BrandId = value.BrandId;
            model.ModelName = value.Model;
            model.Quantity = value.Quantity;
            model.UnitId = value.UnitId;
            model.Country = value.Country;
            model.CanbeBreakable = value.CanbeBreakable;
            if (model.CanbeBreakable)
            {
                model.SubQuantity = value.SubQuantity;
                model.SubUnits = value.SubUnitId;
            }
            model.Price = value.Price;
            model.CanbeSold = Convert.ToBoolean(value.CanbeSold);
            model.CanbeOrdered = Convert.ToBoolean(value.CanbeOrdered);
            model.CanbeRepaired = Convert.ToBoolean(value.CanbeRepaired);
            model.CanbePerishable = Convert.ToBoolean(value.CanbePerishable);
            if (model.CanbeRepaired)
            {
                if (Convert.ToBoolean(value.HasReplacementWarranty))
                {
                    model.WarrantyFor = "1";
                }
                else
                {
                    model.WarrantyFor = "2";
                }
                model.WarrantyDay = value.WarrantyDays;
                model.IsWarrantyStart = Convert.ToBoolean(value.IsWarrantyStartNow);
            }
            model.ExpiredDate = value.ExpiredDate;
            model.Description = value.Description;
            model.InMultipleLocation = value.InMultipleLocation;
            model.LocationId = value.LocationId;
            if (model.InMultipleLocation == 0)
            {
                model.StoreId = db.Inventory_Item_Location.Where(m => m.InventoryId == model.InventoryId).Select(m => m.StoreId).FirstOrDefault();
                model.WareId = db.Inventory_Item_Location.Where(m => m.InventoryId == model.InventoryId).Select(m => m.WarehouseId).FirstOrDefault();
            }

            ViewBag.StoreId = new SelectList(db.StoreLists, "Id", "StoreName", model.StoreId);
            ViewBag.WareId = new SelectList(db.WarehouseLists, "Id", "WarehouseName", model.WareId);
            ViewBag.ProductTypeId = new SelectList(db.ProductTypeLists.Where(m => m.HasParent == false).OrderBy(m => m.ProductTypeName), "ProductTypeId", "ProductTypeName", model.ProductTypeId);
            ViewBag.BrandId = new SelectList(db.BrandLists.Where(m => m.HasParent == false), "BrandId", "BrandName", model.BrandId);
            ViewBag.UnitId = new SelectList(db.UnitLists.Where(m => m.HasParentId == false), "UnitId", "UnitName", model.UnitId);
            ViewBag.SubUnitId = new SelectList(db.UnitLists.Where(m => m.HasParentId == false), "UnitId", "UnitName", model.SubUnits);
            ViewBag.Country = new SelectList(db.CountryLists, "Id", "CountryName", model.Country);

            return PartialView(model);
        }
        public JsonResult StoreProInfoUpdate(InventoryListModelView model, IEnumerable<HttpPostedFileBase> files, int[] AllPreImgId)
        {
            if (model.InventoryId > 0)
            {
                //string msg = "";
                //string opname = "Inventory Item Information Updated";
                int ColumnId = model.InventoryId;
                int status = 1;

                if ((model.BrandId == 0 || model.BrandId == null) && model.Brand != null)
                {
                    BrandList brand = new BrandList();
                    brand.BrandName = model.Brand;
                    brand.HasParent = false;
                    brand.CreatedDate = now;
                    brand.CreatedBy = model.CreatedBy;
                    db.BrandLists.Add(brand);
                    db.SaveChanges();
                    model.BrandId = db.BrandLists.Where(m => m.CreatedBy == model.CreatedBy).Max(m => m.BrandId);
                }

                InventoryList inven = db.InventoryLists.Find(model.InventoryId);
                inven.ProductName = model.ProductName;
                inven.ProductTypeId = model.ProductTypeId;
                inven.Country = model.Country;
                inven.Description = model.Description;
                inven.Brand = model.Brand;
                inven.BrandId = model.BrandId;
                inven.Model = model.ModelName;


                //inven.CanbeBreakable = model.CanbeBreakable;
                //inven.CanbeSold = model.CanbeSold;
                //inven.CanbeOrdered = model.CanbeOrdered;
                //inven.CanbeRepaired = model.CanbeRepaired;
                //inven.CanbeReplaceable = model.CanbeReplaceable;
                //inven.CanbePerishable = model.CanbePerishable;
                //inven.CanbeTopup = model.CanbeTopUp;
                //inven.UnitId = model.UnitId;
                //inven.ExpiredDate = model.ExpiredDate;

                //inven.Price = model.Price;
                //if (inven.CanbeBreakable)
                //{
                //    inven.SubQuantity = model.SubQuantity;
                //    inven.SubUnitId = model.SubUnits;
                //}
                //if (model.ExpiredDate != null)
                //{
                //    DateDifference dateDifference = new DateDifference(model.ExpiredDate.Value, now);
                //    inven.WarrantyDay = dateDifference.Days;
                //    inven.WarrantyMonth = dateDifference.Months;
                //    inven.WarrantyYear = dateDifference.Years;
                //}
                //else
                //{
                //    inven.WarrantyDay = 0;
                //    inven.WarrantyMonth = 0;
                //    inven.WarrantyYear = 0;
                //}

                inven.UpdatedDate = now;
                inven.UpdatedBy = model.CreatedBy;

                db.Entry(inven).State = EntityState.Modified;
                try
                {
                    db.SaveChanges();
                    status = 1;

                    //************************ Delete Previous Image *******************
                    if (AllPreImgId != null)
                    {
                        if (AllPreImgId.Length > 0)
                        {
                            for (int i = 0; i < AllPreImgId.Length; i++)
                            {
                                int imgId = Convert.ToInt32(AllPreImgId[i]);
                                Inventory_Image_Lists image = db.Inventory_Image_Lists.Find(imgId);
                                if (image != null)
                                {
                                    string OriginalImg = Path.Combine(Server.MapPath("~/Images/Inventory/Store/Original"), image.ImageName);
                                    if (System.IO.File.Exists(OriginalImg))
                                    {
                                        System.IO.File.Delete(OriginalImg);
                                    }
                                    string ResizeImg = Path.Combine(Server.MapPath("~/Images/Inventory/Store/Resize"), image.ImageName);
                                    if (System.IO.File.Exists(ResizeImg))
                                    {
                                        System.IO.File.Delete(ResizeImg);
                                    }
                                    db.Inventory_Image_Lists.Remove(image);
                                    db.SaveChanges();
                                }
                            }
                        }
                    }


                    //**************** New Uploaded Image Save **************************
                    int imgCount = 0;
                    if (files != null)
                    {
                        foreach (var file in files)
                        {
                            if (file != null && file.ContentLength > 0)
                            {
                                string s = file.FileName;
                                int idx = s.LastIndexOf('.');
                                string fileName = s.Substring(0, idx);
                                string extension = s.Substring(idx);

                                string picName = now.ToString("yyyyMMddmmss").ToString() + model.InventoryId + "_" + model.CreatedBy + imgCount + extension;
                                string PathForOriginal = Path.Combine(Server.MapPath("~/Images/Inventory/Store/Original"), picName);
                                file.SaveAs(PathForOriginal);

                                var imgForGallery = Image.FromStream(file.InputStream, true, true);
                                using (var resizeImg = ScaleImage(imgForGallery, 150, 150))
                                {
                                    string Paths = Path.Combine(Server.MapPath("~/Images/Inventory/Store/Resize"), picName);
                                    resizeImg.Save(Paths);
                                }
                                Inventory_Image_Lists pic = new Inventory_Image_Lists();
                                pic.InventoryId = model.InventoryId;
                                pic.ImageName = picName;
                                db.Inventory_Image_Lists.Add(pic);
                                db.SaveChanges();
                            }
                            imgCount++;
                        }
                    }

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
            return Json("Error", JsonRequestBehavior.AllowGet);
        }
        public ActionResult StoreProductCreate(int? id)
        {
            int currency = db.FactoryInformations.Select(m => m.CurrencyId).FirstOrDefault();
            var symbol = db.CurrencyLists.Where(m => m.CurrencyId == currency).Select(m => m.CurrencySymbol).FirstOrDefault();
            ViewBag.getCurrency = symbol;
            InventoryListModelView model = new InventoryListModelView();
            if (id != null)
            {
                int inventoryId = Convert.ToInt32(id);
                View_All_InventoryList value = db.View_All_InventoryList.Where(m => m.InventoryId == inventoryId).FirstOrDefault();
                model.InventoryId = value.InventoryId;
                model.ProductName = value.ProductName;
                model.ProductTypeId = value.ProductTypeId;

                model.Brand = value.BrandName;
                model.BrandId = value.BrandId;
                model.ModelName = value.Model;
                model.Quantity = value.Quantity;
                model.UnitId = value.UnitId;
                model.Country = value.Country;
                model.CanbeBreakable = value.CanbeBreakable;
                if (model.CanbeBreakable)
                {
                    model.SubQuantity = value.SubQuantity;
                    model.SubUnits = value.SubUnitId;
                }
                model.Price = value.Price;
                model.CanbeSold = Convert.ToBoolean(value.CanbeSold);
                model.CanbeOrdered = Convert.ToBoolean(value.CanbeOrdered);
                model.CanbeRepaired = Convert.ToBoolean(value.CanbeRepaired);
                model.CanbePerishable = Convert.ToBoolean(value.CanbePerishable);
                if (model.CanbeRepaired)
                {
                    if (Convert.ToBoolean(value.HasReplacementWarranty))
                    {
                        model.WarrantyFor = "1";
                    }
                    else
                    {
                        model.WarrantyFor = "2";
                    }
                    model.WarrantyDay = value.WarrantyDays;
                    model.IsWarrantyStart = Convert.ToBoolean(value.IsWarrantyStartNow);
                }
                model.ExpiredDate = value.ExpiredDate;
                model.Description = value.Description;
                model.InMultipleLocation = value.InMultipleLocation;
                model.LocationId = value.LocationId;
                if (model.InMultipleLocation == 0)
                {
                    model.StoreId = db.Inventory_Item_Location.Where(m => m.InventoryId == model.InventoryId).Select(m => m.StoreId).FirstOrDefault();
                    model.WareId = db.Inventory_Item_Location.Where(m => m.InventoryId == model.InventoryId).Select(m => m.WarehouseId).FirstOrDefault();
                }
            }
            ViewBag.ProductTypeId = new SelectList(db.ProductTypeLists.Where(m => m.HasParent == false).OrderBy(m => m.ProductTypeName), "ProductTypeId", "ProductTypeName", model.ProductTypeId);
            ViewBag.BrandId = new SelectList(db.BrandLists.Where(m => m.HasParent == false), "BrandId", "BrandName", model.BrandId);
            ViewBag.UnitId = new SelectList(db.UnitLists.Where(m => m.HasParentId == false), "UnitId", "UnitName", model.UnitId);
            ViewBag.SubUnitId = new SelectList(db.UnitLists.Where(m => m.HasParentId == false), "UnitId", "UnitName", model.SubUnits);
            ViewBag.Country = new SelectList(db.CountryLists, "Id", "CountryName", model.Country);
            if (id > 0)
            {
                return PartialView("_StoreProEdit",model);
            }
            else
            {
                return View(model);
            }
        }
        public JsonResult ProductNameSearching(string prefix, bool isStore, bool isware)
        {
            if (prefix == "")
            {
                return Json("", JsonRequestBehavior.AllowGet);
            }
            else
            {
                if (isStore)
                {
                    var allInventoryId = db.Inventory_Item_Location.Where(m => m.StoreId > 0).Select(m => m.InventoryId).ToList();
                    var allproduct = (from s in db.InventoryLists
                                      where (s.ProductName.ToLower().Contains(prefix.ToLower()) && allInventoryId.Contains(s.InventoryId))
                                      select new InventoryListModelView
                                      {
                                          InventoryId = s.InventoryId,
                                          ProductName = s.ProductName
                                      }).ToList();
                    return Json(allproduct, JsonRequestBehavior.AllowGet);
                }
                if (isware)
                {
                    var allInventoryId = db.Inventory_Item_Location.Where(m => m.WarehouseId > 0).Select(m => m.InventoryId).ToList();
                    var allproduct = (from s in db.InventoryLists
                                      where (s.ProductName.ToLower().Contains(prefix.ToLower()) && allInventoryId.Contains(s.InventoryId))
                                      select new InventoryListModelView
                                      {
                                          InventoryId = s.InventoryId,
                                          ProductName = s.ProductName
                                      }).ToList();
                    return Json(allproduct, JsonRequestBehavior.AllowGet);
                }
                var list = (from s in db.InventoryLists
                                  where (s.ProductName.ToLower().Contains(prefix.ToLower()))
                                  select new InventoryListModelView
                                  {
                                      InventoryId = s.InventoryId,
                                      ProductName = s.ProductName
                                  }).ToList();
                return Json(list, JsonRequestBehavior.AllowGet);
            }
        }
        public JsonResult LcNameSearching(string prefix)
        {
            if (prefix == "")
            {
                return Json("", JsonRequestBehavior.AllowGet);
            }
            else
            {
                var allproduct = (from s in db.Import_History
                                  where (s.L_C.ToLower().Contains(prefix.ToLower()))
                                  select new 
                                  {
                                      Import = s.ImportId,
                                      Lc = s.L_C,
                                      ParentId = s.ParentId
                                  }).ToList();
                allproduct.RemoveAll(x => x.ParentId > 1);                
                return Json(allproduct, JsonRequestBehavior.AllowGet);
            }
        }
        public JsonResult BrandNameSearching(string prefix)
        {
            if (prefix == "")
            {
                return Json("", JsonRequestBehavior.AllowGet);
            }
            else
            {
                var allBrand = (from s in db.BrandLists
                                  where (s.BrandName.ToLower().Contains(prefix.ToLower()))
                                  select new BrandListViewModel
                                  {
                                      BrandId = s.BrandId,
                                      BrandName = s.BrandName
                                  }).ToList();
                return Json(allBrand, JsonRequestBehavior.AllowGet);
            }
        }
        public ActionResult _AcquisitionBySearch(int? InventoryId)
        {
            if (InventoryId != null && db.View_All_InventoryList.Where(m => m.InventoryId == InventoryId).Any())
            {
                var acq = db.View_All_InventoryList.Where(m => m.InventoryId == InventoryId).FirstOrDefault();

                if (db.View_All_InventoryList.Where(m => m.InventoryId == acq.InventoryId && m.CanbeBreakable).Any())
                {
                    int reserd_quan = 0, total_child = 0, total_ParentQuantity = 0, total_ChildQuantity = 0;
                    if (db.Inventory_Reserved_Item_Lists
                        .Where(m => m.InventoryId == acq.InventoryId && m.IsAssigned == false && m.IsSubUnit == false).Any())
                    {
                        total_ParentQuantity = db.Inventory_Reserved_Item_Lists
                        .Where(m => m.InventoryId == acq.InventoryId && m.IsAssigned == false && m.IsSubUnit == false)
                        .Select(t => t.Quantity).DefaultIfEmpty().Sum();
                    }
                    if (db.Inventory_Reserved_Item_Lists
                        .Where(m => m.InventoryId == acq.InventoryId && m.IsAssigned == false && m.IsSubUnit == true).Any())
                    {
                        total_ChildQuantity = db.Inventory_Reserved_Item_Lists
                        .Where(m => m.InventoryId == acq.InventoryId && m.IsAssigned == false && m.IsSubUnit == true)
                        .Select(t => t.Quantity).DefaultIfEmpty().Sum();
                    }
                    if (total_ChildQuantity >= acq.SubQuantity)
                    {
                        double quotient = total_ChildQuantity / (int)acq.SubQuantity;
                        double ceiling = Math.Ceiling(quotient);
                        total_child = (int)ceiling;
                    }
                    reserd_quan = total_ParentQuantity + total_child;
                    acq.Available_Quantity = (acq.Quantity - reserd_quan);
                }
                else
                {
                    int total_qntity = Convert.ToInt32(acq.Quantity);
                    int resrvd_qntity = Convert.ToInt32(acq.ReservedQuantity);
                    acq.Available_Quantity = (total_qntity - resrvd_qntity);
                }
                return PartialView(acq);
            }
            return PartialView();
        }
        public JsonResult StoreProductInsert(InventoryListModelView invenModel, IEnumerable<HttpPostedFileBase> files)
        {
            if (ModelState.IsValid)
            {
                string msg = "";
                string opname = "Add New Inventory Item";
                int ColumnId = 0;
                int OperationStatus = 1;
                int id = 0;

                InventoryList inlist = new InventoryList();
                if ((invenModel.BrandId == 0 || invenModel.BrandId == null) && invenModel.Brand != null)
                {
                    BrandList brand = new BrandList();
                    brand.BrandName = invenModel.Brand;
                    brand.HasParent = false;
                    brand.CreatedDate = now;
                    brand.CreatedBy = invenModel.CreatedBy;
                    db.BrandLists.Add(brand);
                    db.SaveChanges();
                    invenModel.BrandId = db.BrandLists.Where(m => m.CreatedBy == invenModel.CreatedBy).Max(m => m.BrandId);
                }
                inlist.ProductName = invenModel.ProductName;
                inlist.ProductTypeId = invenModel.ProductTypeId;
                inlist.Country = invenModel.Country;
                inlist.CanbeBreakable = invenModel.CanbeBreakable;
                inlist.CanbeSold = invenModel.CanbeSold;
                inlist.CanbeOrdered = invenModel.CanbeOrdered;
                inlist.CanbeRepaired = invenModel.CanbeRepaired;
                inlist.CanbeReplaceable = invenModel.CanbeReplaceable;
                inlist.CanbePerishable = invenModel.CanbePerishable;
                inlist.CanbeTopup = invenModel.CanbeTopUp;
                inlist.Quantity = Convert.ToInt32(invenModel.Quantity);
                inlist.UnitId = invenModel.UnitId;
                inlist.Description = invenModel.Description;
                inlist.Brand = invenModel.Brand;
                inlist.BrandId = invenModel.BrandId;
                inlist.Model = invenModel.ModelName;
                inlist.Price = invenModel.Price;
                inlist.InternalReferenceNo = now.ToString("yyyyMMddmmss").ToString() + "" + invenModel.CreatedBy;

                inlist.WarrantyDay = 0;
                inlist.WarrantyMonth = 0;
                inlist.WarrantyYear = 0;

                if (inlist.CanbeBreakable)
                {
                    inlist.SubQuantity = invenModel.SubQuantity;
                    inlist.SubUnitId = invenModel.SubUnits;
                }
                db.InventoryLists.Add(inlist);
                invenModel.InventoryId = inlist.InventoryId;
                try
                {
                    inlist.CreatedDate = now;
                    inlist.CreatedBy = invenModel.CreatedBy;

                    db.SaveChanges();
                    id = db.InventoryLists.Where(m => m.CreatedBy == invenModel.CreatedBy).Max(m => m.InventoryId);
                    msg = "New item '" + inlist.ProductName + "' has been successfully insert into inventory on "+now.ToString("MMM dd, yyyy hh:mm tt")+" .";
                    ColumnId = id;

                    invenModel.InventoryId = id;
                    string locChar = invenModel.SelectedLocId.Split('-')[1];
                    int locid = Convert.ToInt32(invenModel.SelectedLocId.Split('-')[0]);

                    Inventory_Item_Location item_location = new Inventory_Item_Location();
                    item_location.InventoryId = invenModel.InventoryId;
                    if(locChar == "s")
                    {
                        item_location.StoreId = locid;
                        item_location.WarehouseId = 0;
                    }
                    else
                    {
                        item_location.StoreId = 0;
                        item_location.WarehouseId = locid;
                    }
                    item_location.DeptId = invenModel.LineId;
                    item_location.LineId = invenModel.LineId;
                    item_location.Quantity = Convert.ToDouble(invenModel.Quantity);
                    item_location.MinimumQuantity = Convert.ToDouble(invenModel.Min_Quantity);
                    item_location.UnitId = invenModel.UnitId;
                    item_location.InsertedDate = now;
                    item_location.InsertedBy = invenModel.CreatedBy;
                    item_location.Description = invenModel.Rack;
                    db.Inventory_Item_Location.Add(item_location);
                    db.SaveChanges();

                    int count = Convert.ToInt32(invenModel.Quantity);
                    //StoreItemList plist = new StoreItemList();
                    //for (int i = 1; i <= count; i++)
                    //{
                    //    plist.InventoryId = id;
                    //    plist.CreatedDate = now;
                    //    plist.CreatedBy = invenModel.CreatedBy;
                    //    if (invenModel.ExpiredDate != null)
                    //    {
                    //        TimeSpan span = (Convert.ToDateTime(invenModel.ExpiredDate)).Subtract(now);
                    //        int totaldays = (int)span.TotalDays;
                    //        plist.ExpiredDateCount = totaldays;
                    //        plist.ExpiredDate = invenModel.ExpiredDate; 
                    //    }
                    //    plist.ItemStatus = 1;
                    //    db.StoreItemLists.Add(plist);
                    //    db.SaveChanges();
                    //}

                    int imgCount = 0;
                    if (files != null)
                    {
                        foreach (var file in files)
                        {
                            if (file != null && file.ContentLength > 0)
                            {
                                string s = file.FileName;
                                int idx = s.LastIndexOf('.');
                                string fileName = s.Substring(0, idx);
                                string extension = s.Substring(idx);

                                string picName = invenModel.InventoryId + "_" + invenModel.CreatedBy + imgCount + extension;
                                string PathForOriginal = Path.Combine(Server.MapPath("~/Images/Inventory/Store/Original"), picName);
                                file.SaveAs(PathForOriginal);

                                var imgForGallery = Image.FromStream(file.InputStream, true, true);
                                using (var resizeImg = ScaleImage(imgForGallery, 150, 150))
                                {
                                    string Paths = Path.Combine(Server.MapPath("~/Images/Inventory/Store/Resize"), picName);
                                    resizeImg.Save(Paths);
                                }
                                Inventory_Image_Lists pic = new Inventory_Image_Lists();
                                pic.InventoryId = invenModel.InventoryId;
                                pic.ImageName = picName;
                                db.Inventory_Image_Lists.Add(pic);
                                db.SaveChanges();
                            }
                            imgCount++;
                        }
                    }
                }
                catch (Exception)
                {
                    msg = "New product '" + inlist.ProductName + "' insert into store was unsuccessfulon on " + now.ToString("MMM dd, yyyy hh:mm tt") + " .";
                    OperationStatus = -1;
                    ColumnId = 0;
                }
                SaveAuditLog(opname, msg, "Inventory", "Store", "InventoryList", ColumnId, invenModel.CreatedBy, now, OperationStatus);
                if (OperationStatus == 1)
                {
                    return Json(invenModel, JsonRequestBehavior.AllowGet);
                }   
            }
            return Json("Error",JsonRequestBehavior.AllowGet);
        }
        public JsonResult ExistingProQunatityChange(InventoryListModelView model)                                                                                              
        {
            string msg = "";
            string location = "";
            if (model.StoreId > 0){location = "Store";}
            else if (model.WareId > 0){location = "Ware-house";}
            else{location = "Line";}
            string opname = "Store New Product Into " + location;
            int OperationStatus = 1;
            bool alreadyExists = false;
            InventoryList inlist = db.InventoryLists.Find(model.InventoryId);
            Inventory_Item_Location item_location = new Inventory_Item_Location();
            item_location.ReceivedDate = now;
            item_location.ReceivedBy = model.CreatedBy;
            if (model.StoreId > 0 && (db.Inventory_Item_Location.Where(m => m.InventoryId == model.InventoryId && m.StoreId == model.StoreId).Any()))
            {
                item_location = db.Inventory_Item_Location.Where(m => m.InventoryId == model.InventoryId && m.StoreId == model.StoreId).FirstOrDefault();
                alreadyExists = true;
            }
            else if (model.WareId > 0 && (db.Inventory_Item_Location.Where(m => m.InventoryId == model.InventoryId && m.WarehouseId == model.WareId).Any()))
            {
                item_location = db.Inventory_Item_Location.Where(m => m.InventoryId == model.InventoryId && m.StoreId == model.StoreId).FirstOrDefault(); 
                alreadyExists = true;
            }
            else if (model.LineId > 0 && (db.Inventory_Item_Location.Where(m => m.InventoryId == model.InventoryId && m.LineId == model.LineId).Any()))
            {
                item_location = db.Inventory_Item_Location.Where(m => m.InventoryId == model.InventoryId && m.LineId == model.LineId).FirstOrDefault();
                alreadyExists = true;
            }
            if (alreadyExists)
            {
                item_location.Quantity = (Convert.ToDouble(item_location.Quantity) + Convert.ToDouble(model.Quantity));
                db.Entry(item_location).State = EntityState.Modified;
            }
            else
            {
                item_location.InventoryId = model.InventoryId;
                item_location.StoreId = model.StoreId;
                item_location.WarehouseId = model.WareId;
                item_location.DeptId = model.LineId;
                item_location.LineId = model.LineId;
                item_location.Quantity = Convert.ToDouble(model.Quantity);
                item_location.MinimumQuantity = Convert.ToDouble(model.Min_Quantity);
                item_location.UnitId = model.UnitId;
                db.Inventory_Item_Location.Add(item_location);
            }
            try
            {
                db.SaveChanges();
                OperationStatus = 1;
                int count = Convert.ToInt32(model.Quantity);
                StoreItemList plist = new StoreItemList();
                for (int i = 1; i <= count; i++)
                {
                    plist.InventoryId = model.InventoryId;
                    plist.CreatedDate = now;
                    plist.CreatedBy = model.CreatedBy;
                    if (model.NewExpiredDate != null)
                    {
                        TimeSpan span = (Convert.ToDateTime(model.NewExpiredDate)).Subtract(now);
                        int totaldays = (int)span.TotalDays;

                        plist.ExpiredDateCount = totaldays;
                        plist.ExpiredDate = model.NewExpiredDate;
                    }
                    plist.ItemStatus = 1;
                    db.StoreItemLists.Add(plist);
                    db.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                string strErr = ex.ToString();
                OperationStatus = -1;
            }
            SaveAuditLog(opname, msg, "Inventory", "Store", "InventoryList", model.InventoryId, model.CreatedBy, now, OperationStatus);
            if (OperationStatus == 1)
            {
                return Json("Success",JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json("Error", JsonRequestBehavior.AllowGet);
            }
        }

        //**********************************************************************************
        //****************************ALL INVENTORY LIST VIEW :START ***********************
        //**********************************************************************************

        public ActionResult StoreProList()
        {
            return View();
        }
        public ActionResult _StoreProList()
        {
            var List = (from i in db.View_All_InventoryList
                        select new InventoryListModelView
                        {
                            InventoryId = i.InventoryId,
                            ProductName = i.ProductName,
                            ProductTypeId = i.ProductTypeId,
                            Quantity =(int) i.Quantity,
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
                            Available_Quantity =(int) i.Available_Quantity,
                            Description = i.Description,
                            InMultipleLocation = i.InMultipleLocation,
                            ProductImageName = db.Inventory_Image_Lists.Where(m => m.InventoryId == i.InventoryId).Select(m => m.ImageName).FirstOrDefault(),
                            CreatedBy = i.CreatedBy,
                            CreatedDate = i.CreatedDate
                        }).OrderBy(m => m.ProductName).ToList();
            Session["AllStorePro"] = List;
            return PartialView(List);
        }
        public JsonResult GetProList(int? page)
        {
            page = page ?? 0;
            int total = Convert.ToInt16(Session["TotalAcquisitionOrderCount"]);
            int skip = ShowItemPerPage * Convert.ToInt16(page);
            var canPage = skip < total;
            if (canPage)
            {
                var listBackFromSession = (List<AcquisitionViewModel>)Session["AllAcquisitionOrder"];
                var orderlist = listBackFromSession.Skip(skip).Take(ShowItemPerPage).ToList();
                return Json(orderlist, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json("", JsonRequestBehavior.AllowGet);
            }
        }
        public JsonResult DeleteStoreProduct(int? id,long UserId)
        {
            if (id != null)
            {
                InventoryList model = db.InventoryLists.Find(id);
                if (model != null)
                {
                    if (db.StoreItemLists.Where(m => m.InventoryId == model.InventoryId).Any())
                    {
                        var in_pro_list = db.StoreItemLists.Where(m => m.InventoryId == model.InventoryId).ToList();
                        foreach (var p in in_pro_list)
                        {
                            if (p != null)
                            {
                                db.StoreItemLists.Remove(p);
                                db.SaveChanges();
                            }
                        }
                    }
                    db.InventoryLists.Remove(model);
                    db.SaveChanges();
                    return Json("Success", JsonRequestBehavior.AllowGet);
                }
            }
            return Json("Error",JsonRequestBehavior.AllowGet);
        }
        public ActionResult ShowAllInventoryList(int? storeid)
        {
            if (storeid == null)
            {
                ViewBag.storeid = 0;
            }
            else
            {
                ViewBag.storeid = storeid;
                ViewBag.storeName = db.StoreLists.Where(m => m.Id == storeid).Select(m => m.StoreName).FirstOrDefault();
            }
            return View();
        }
        public ActionResult ProductCategory()
        {
            var allInventoryId = db.Inventory_Item_Location.Where(m => m.StoreId > 0).Select(m => m.InventoryId).ToList();
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
        public ActionResult _StoreItemList(int? inventoryId, int? productId,int? storeid, int viewId)
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
                if (viewId == 1)
                {
                    return PartialView(allProduct.Take(ShowItemPerPage).ToList());
                }
                else
                {
                    return PartialView("_ShowAllInventoryProductInGroup", allProduct.Take(ShowItemPerPage).ToList());
                }
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
                    if (viewId == 1)
                    {
                        return PartialView(allProduct.Take(ShowItemPerPage).ToList());
                    }
                    else
                    {
                        return PartialView("_ShowAllInventoryProductInGroup", allProduct.Take(ShowItemPerPage).ToList());
                    }
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
                    if (viewId == 1)
                    {
                        return PartialView(allProduct.Take(ShowItemPerPage).ToList());
                    }
                    else
                    {
                        return PartialView("_ShowAllInventoryProductInGroup", allProduct.Take(ShowItemPerPage).ToList());
                    }
                }
            }
            else if (storeid != null)
            {
                var allInventoryId = db.Inventory_Item_Location.Where(m => m.StoreId == storeid).Select(m => m.InventoryId).ToList();
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
                if (viewId == 1)
                {
                    return PartialView(allProduct.Take(ShowItemPerPage).ToList());
                }
                else
                {
                    return PartialView("_ShowAllInventoryProductInGroup", allProduct.Take(ShowItemPerPage).ToList());
                }
            }
            else
            {
                var allInventoryId = db.Inventory_Item_Location.Where(m => m.StoreId > 0).Select(m => m.InventoryId).ToList();
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
                if (viewId == 1)
                {
                    return PartialView(allProduct.Take(ShowItemPerPage).ToList());
                }
                else
                {
                    return PartialView("_ShowAllInventoryProductInGroup", allProduct.Take(ShowItemPerPage).ToList());
                }
            }
        }
        public JsonResult GetInvenPro(int? page)
        {
            page = page ?? 0;
            int total = Convert.ToInt16(Session["totalProduct"]);
            int skip = ShowItemPerPage * Convert.ToInt16(page);
            var canPage = skip < total;
            if (canPage)
            {
                var listBackFromSession = (List<InventoryListModelView>)Session["Products"];
                var partslist = listBackFromSession.Skip(skip).Take(ShowItemPerPage).ToList();
                return Json(partslist, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json("", JsonRequestBehavior.AllowGet);
            }
        }     
        public JsonResult GetMultiple(int id)
        {
            var multipleList = (from m in db.Inventory_Item_Location
                                join s in db.StoreLists on m.StoreId equals s.Id
                                where m.InventoryId == id
                                select new
                                {
                                    LocationId = m.LocationId,
                                    StoreName = s.StoreName,
                                    Quantity = m.Quantity
                                }).ToList();
            return Json(multipleList,JsonRequestBehavior.AllowGet);
        }      
        public JsonResult TempOrderList(int id, int quantity,long UserId,bool isWare,bool isStore)
        {
            TmpInventoryOrderList model = new TmpInventoryOrderList();
            model.WareItem = isWare;
            model.StoreItem = isStore;
            model.InventoryId = id;
            model.Quantity = quantity;
            model.OrderedBy = UserId;
            model.OrderedDate = now;
            model.Status = 0;
            db.TmpInventoryOrderLists.Add(model);
            //if (db.TmpInventoryOrderLists.Count() != 0)
            //{
            //    bool exist = db.TmpInventoryOrderLists.Where(m => m.InventoryId == id && m.OrderedBy == UserId).Any();
            //    if (exist)
            //    {
            //        TmpInventoryOrderList findId = db.TmpInventoryOrderLists.Where(m => m.InventoryId == id && m.OrderedBy == UserId).FirstOrDefault();
            //        findId.Quantity = quantity;
            //        findId.OrderedDate = now;
            //        db.TmpInventoryOrderLists.Attach(findId);
            //        db.Entry(findId).State = EntityState.Modified;
            //    }
            //    else
            //    {
            //        model.WareItem = isWare;
            //        model.StoreItem = isStore;
            //        model.InventoryId = id;
            //        model.Quantity = quantity;
            //        model.OrderedBy = UserId;
            //        model.OrderedDate = now;
            //        model.Status = 0;
            //        db.TmpInventoryOrderLists.Add(model);
            //    }
            //}
            //else
            //{
            //    model.WareItem = isWare;
            //    model.StoreItem = isStore;
            //    model.InventoryId = id;
            //    model.Quantity = quantity;
            //    model.OrderedBy = UserId;
            //    model.OrderedDate = now;
            //    model.Status = 0;
            //    db.TmpInventoryOrderLists.Add(model);
            //}
            db.SaveChanges();
            return Json("success", JsonRequestBehavior.AllowGet);
        }
        public JsonResult TempOrderListUpdate(int id, int quantity)
        {
            TmpInventoryOrderList modelUpdate = db.TmpInventoryOrderLists.Where(m => m.InventoryId == id).Select(m => m).FirstOrDefault();
            if (quantity != 0)
            {
                modelUpdate.Quantity = quantity;
                modelUpdate.OrderedDate = now;
                db.TmpInventoryOrderLists.Attach(modelUpdate);
                db.Entry(modelUpdate).State = EntityState.Modified;
                db.SaveChanges();
            }
            else
            {
                db.TmpInventoryOrderLists.Remove(modelUpdate);
                db.SaveChanges();
            }
            return Json("success", JsonRequestBehavior.AllowGet);
        }
        public JsonResult SearchProduct(string prefix, int storeId)
        {
            if (prefix == "")
            {
                return Json("", JsonRequestBehavior.AllowGet);
            }
            else
            {
                if (storeId > 0)
                {
                    var allInventoryId = db.Inventory_Item_Location.Where(m => m.StoreId == storeId).Select(m => m.InventoryId).ToList();
                    var allPro = (List<InventoryListModelView>)Session["Products"];
                    var list = (from i in db.View_All_InventoryList
                                where (i.ProductName.ToLower().Contains(prefix.ToLower())) && allInventoryId.Contains(i.InventoryId)
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
                    var allInventoryId = db.Inventory_Item_Location.Where(m => m.StoreId > 0).Select(m => m.InventoryId).ToList();
                    var allPro = (List<InventoryListModelView>)Session["Products"];
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
        #endregion

        //******************************************************************************************************************
        //************************************ ALL INVENTORY LIST VIEW :END ************************************************
        //******************************************************************************************************************


        //**************************************** Store Product Details ***************************************************

        #region Invenoty Product Details
        [EncryptedActionParameter]
        public ActionResult StoreProductWithLCDetails(int? id, string v)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            else
            {
                var model = db.View_ProList_WithLC.Where(m => m.LocationId == id).FirstOrDefault();
                if (model != null)
                {
                    if (v == "p")
                    {
                        return PartialView("_StoreProductWithLCDetails", model);
                    }
                    else
                    {
                        return View(model);
                    }

                }
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
        }
        [EncryptedActionParameter]
        public ActionResult StoreProductDetails(int? id, string v)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            else
            {
                var model = db.View_All_InventoryList.Where(m => m.InventoryId == id).FirstOrDefault();
                if (model != null)
                {
                    if (v == "p")
                    {
                        return PartialView("_StoreProductDetails", model);
                    }
                    else
                    {
                        return View(model);
                    }

                }
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
        }
        public ActionResult _ProductImages(int? id)
        {
            if (id != null && db.Inventory_Image_Lists.Where(m => m.InventoryId == id).Any())
            {
                var allImage = db.Inventory_Image_Lists.Where(m => m.InventoryId == id).ToList();
                return PartialView(allImage);
            }
            return PartialView();
        }
        public PartialViewResult ProductReserveDetails(int inventoryId)
        {
            var list = (from m in db.Inventory_Reserved_Item_Lists
                        join a in db.Acquisition_List on m.AcquisitionOrderId equals a.AcquisitionOrderId
                        join u in db.View_All_InventoryList on m.InventoryId equals u.InventoryId
                        where m.InventoryId == inventoryId && m.IsAssigned == false
                        select new AcquisitionViewModel
                        {
                            AcquisitionOrderId = m.AcquisitionOrderId,
                            Name = a.Name,
                            Quantity = m.Quantity,
                            UnitName = u.UnitName,
                            CreatedBy = m.ReserveredBy,
                            CreatedDate = m.ReserveredDate
                        }).ToList();
            return PartialView(list);
        }
        public PartialViewResult _ProRequistionHis(int id)
        {
            ViewBag.id = id;
            return PartialView();
        }
        public PartialViewResult _AllProPeqHis(int id)
        {
            var list = db.View_All_AcquisitionList.Where(m => m.ProductId == id).ToList();
            return PartialView(list);
        }
        public PartialViewResult _ProBuyingHis(int id)
        {
            ViewBag.id = id;
            return PartialView();
        }
        public PartialViewResult _AllProBuyHis(int id)
        {
            var list = db.BusinessOrderProductLists.Where(m => m.ProductId == id).ToList();
            return PartialView(list);
        }
        public PartialViewResult _ProDispatchedHis(int id,int type,long? locId)
        {
            ViewBag.id = id;
            ViewBag.type = type;
            ViewBag.locId = locId;
            return PartialView();
        }
        public PartialViewResult _AllProDisHis(int id, int type, long? locId)
        {
            var list = (from i in db.DispatchedItemLists
                        join d in db.StoreItemDispatchedHistories on i.DispatchedId equals d.Id
                        join v in db.View_All_InventoryList on i.InventoryId equals v.InventoryId
                        where i.InventoryId == id && d.Type == type
                        select new DispatchedItemListModelView
                        {
                            Id= i.Id,
                            DispatchedId = d.Id,
                            Dispatchedname = d.Name,
                            DispatchedBy = d.DispatchedBy,
                            DispatchedUserPic = (d.DispatchedBy > 1)?db.View_UserLists.Where(m => m.UserId == d.DispatchedBy).Select(m => m.Picture).FirstOrDefault():"admin.jpg",
                            DispatchedUserName = (d.DispatchedBy > 1) ? db.View_UserLists.Where(m => m.UserId == d.DispatchedBy).Select(m => m.UserName).FirstOrDefault() : "Super Admin",
                            InventoryId = i.InventoryId,
                            LocationId= i.LocationId,
                            Productname = v.ProductName,
                            Quantity = i.Quantity,
                            Unitname = v.UnitName,
                            OrderId = i.OrderId,
                            Ordername = (i.OrderId > 0)?(db.BusinessOrderLists.Where(o => o.BusinessOrderId == i.OrderId).Select(o => o.OrderName).FirstOrDefault()):"",
                            MachineId= i.MachineId,
                            Machinename = (i.MachineId > 0) ? (db.MachineLists.Where(o => o.MachineId == i.MachineId).Select(o => o.MachineAcronym).FirstOrDefault()) : "",
                            WareId = i.WareId,
                            Warename = (i.WareId > 0) ? (db.WarehouseLists.Where(o => o.Id == i.WareId).Select(o => o.WarehouseName).FirstOrDefault()) : "",
                            StoreId = i.StoreId,
                            Storename = (i.StoreId > 0) ? (db.StoreLists.Where(o => o.Id == i.StoreId).Select(o => o.StoreName).FirstOrDefault()) : "",
                            DeptId = i.DeptId,
                            DeptName = (i.DeptId > 0) ? (db.DepartmentLists.Where(o => o.DeptId == i.DeptId).Select(o => o.DeptName).FirstOrDefault()) : ""
                        }).ToList();
            if (locId > 0)
            {
                list = list.Where(m => m.LocationId == locId).ToList();
            }
            ViewBag.type = type;
            return PartialView(list);
        }
        public PartialViewResult _ProImportHis(int id, long? impid)
        {
            ViewBag.id = id;
            ViewBag.impid = impid;
            return PartialView();
        }
        public PartialViewResult _ProAllImportHis(int id, long? impid)
        {
            var list = db.View_Import_History_Details.Where(m => m.InventoryId == id).ToList();
            if (impid > 0)
            {
                list = list.Where(m => m.ImportId == impid || m.ParentId == impid).ToList();
            }
            return PartialView(list);
        }
        public PartialViewResult _ProLCHis()
        {
            return PartialView();
        }
        public PartialViewResult _ProLCList(int id)
        {
            var list = db.View_Import_History_Details.Where(m => m.InventoryId == id).GroupBy(m => m.L_C)
                         .Select(m => m.FirstOrDefault()).OrderByDescending(m => m.InsertedDate).ToList();
            return PartialView(list);
        }
        #endregion

        //******************************************* STORE (STORE AND WAREHOUSE TOGETHER) *********************************

        #region
        public ActionResult Store()
        {
            return View();
        }
        public ActionResult _StoreList(List<int?> locationId, int? productId, int? storeid, int? wareid, int viewId)
        {
            
            var list = db.View_ProList_WithLC.ToList();
            var allProduct = (from bs in list
                              group bs by new { bs.InventoryId, bs.LC } into i
                              select new InventoryListModelView
                              {
                                  InventoryId = i.FirstOrDefault().InventoryId,
                                  ImportId = i.FirstOrDefault().ImportId,
                                  LC = i.FirstOrDefault().LC,
                                  ProductName = i.FirstOrDefault().ProductName,
                                  ProductTypeId = i.FirstOrDefault().ProductTypeId,
                                  Quantity = i.Sum(x => x.Quantity),
                                  Brand = i.FirstOrDefault().BrandName,
                                  ModelName = i.FirstOrDefault().Model,
                                  Price = i.FirstOrDefault().Price,
                                  Withunit = i.FirstOrDefault().Quantity + " " + i.FirstOrDefault().UnitName,
                                  SubQuantity = i.FirstOrDefault().SubQuantity,
                                  SubUnits = i.FirstOrDefault().SubUnitId,
                                  UnitName = i.FirstOrDefault().UnitName,
                                  SubUnitName = i.FirstOrDefault().SubUnitName,
                                  CanbeBreakable = i.FirstOrDefault().CanbeBreakable,
                                  LocationId = i.FirstOrDefault().LocationId,
                                  Description = i.FirstOrDefault().Description,
                                  CountryName = i.FirstOrDefault().CountryName
                              }).OrderBy(m => m.ProductName).ToList();
            if (locationId != null)
            {
                allProduct = allProduct.Where(x => locationId.Contains(x.LocationId)).ToList();
               /*allProduct.Where(x=> locationIds.Contains(x.LocationId)).ToList();*/
                ViewBag.Count = allProduct.Count();
            }
            else if (productId != null)
            {
                if (db.ProductTypeLists.Where(m => m.ParentId == productId).Any())
                {
                    var childId = db.ProductTypeLists.Where(m => m.ParentId == productId).Select(m => m.ProductTypeId).Distinct();
                    allProduct = allProduct.Where(m => childId.Contains(m.ProductTypeId)).ToList();
                }
                else
                {
                    allProduct = allProduct.Where(m => m.ProductTypeId == productId).ToList();
                }
            }
            Session["Products"] = allProduct;
            Session["totalProduct"] = allProduct.Count();
            ViewBag.Count = allProduct.Count();
            ViewBag.storeid = storeid;
            ViewBag.wareid = wareid;
            if (viewId == 1)
            {
                return PartialView(allProduct.Take(ShowItemPerPage).ToList());
            }
            else
            {
                return PartialView("_ShowAllInventoryProductInGroup", allProduct.Take(ShowItemPerPage).ToList());
            }
        }
        public PartialViewResult _LoadMultilocationItem(int id, int? impid,int count)
        {
            ViewBag.count = count;
            var model = db.View_ProList_WithLC.Where(m => m.InventoryId == id && m.ImportId == impid).FirstOrDefault();
            return PartialView(model);
        }
        public ActionResult ProductCategoryAll(long userId,int roleId)
        {
            if(userId < 1 || roleId == 2)
            {
                var allproductType = (from i in db.View_All_InventoryList
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
                                         }).ToList()
                            });
                        }

                    }

                }
                return Json(typeList, JsonRequestBehavior.AllowGet);
            }
            else
            {
                var CottonId = db.ProductTypeLists.Where(m => m.ProductTypeName == "Cotton").Select(m => m.ProductTypeId).ToList();
                var allproductType = (from i in db.View_All_InventoryList
                                      where !CottonId.Contains(i.ProductTypeId)
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
                                         }).ToList()
                            });
                        }

                    }

                }
                return Json(typeList, JsonRequestBehavior.AllowGet);
            }
        }
        public JsonResult GetAllProductNames() {
            
            var list = (from i in db.View_ProList_WithLC where i.Quantity>0                      
                        group i by new { i.InventoryId, i.ImportId } into m
                        select new InventoryListModelView
                        {
                            LocationId = m.FirstOrDefault().LocationId,
                            InventoryId = m.FirstOrDefault().InventoryId,
                            ImportId = m.FirstOrDefault().ImportId,
                            LC = m.FirstOrDefault().LC,
                            ProNameWithLc = m.FirstOrDefault().ProductName + " ( L/C :" + m.FirstOrDefault().LC + " )",
                            ProductName = m.FirstOrDefault().ProductName,
                            ProductTypeId = m.FirstOrDefault().ProductTypeId
                        }).OrderBy(o => o.ProductName).ToList();
            return Json(list, JsonRequestBehavior.AllowGet);
        }
        public JsonResult SearchProductAll(string prefix, long userId, int roleId)
        {
            if (prefix == "")
            {
                return Json("", JsonRequestBehavior.AllowGet);
            }
            else
            {
                if(roleId == 2)
                {
                    var CottonId = db.ProductTypeLists.Where(m => m.ProductTypeName == "Cotton").Select(m => m.ProductTypeId).ToList();
                    var list = (from i in db.View_ProList_WithLC
                                where ((i.ProductName.ToLower().Contains(prefix.ToLower()) || i.LC.ToLower().Contains(prefix.ToLower())))
                                group i by new { i.InventoryId, i.ImportId } into m
                                select new InventoryListModelView
                                {
                                    LocationId = m.FirstOrDefault().LocationId,
                                    InventoryId =m.FirstOrDefault().InventoryId,
                                    ImportId = m.FirstOrDefault().ImportId,
                                    LC = m.FirstOrDefault().LC,
                                    ProNameWithLc = m.FirstOrDefault().ProductName + " ( L/C :" + m.FirstOrDefault().LC + " )",
                                    ProductName = m.FirstOrDefault().ProductName,
                                    ProductTypeId =m.FirstOrDefault().ProductTypeId
                                }).OrderBy(o => o.ProductName).ToList();
                    return Json(list, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    var list = db.View_ProList_WithLC.ToList();
                    var allProduct = (from bs in list
                                      group bs by new { bs.InventoryId, bs.ImportId } into i
                                      select new InventoryListModelView
                                      {
                                          LocationId = i.FirstOrDefault().LocationId,
                                          InventoryId = i.FirstOrDefault().InventoryId,
                                          ImportId = i.FirstOrDefault().ImportId,
                                          LC = i.FirstOrDefault().LC,
                                          ProNameWithLc = i.FirstOrDefault().ProductName + " ( L/C :" + i.FirstOrDefault().LC+" )",
                                          ProductName = i.FirstOrDefault().ProductName,
                                          ProductTypeId = i.FirstOrDefault().ProductTypeId
                                      }).OrderBy(m => m.ProductName).ToList();
                    allProduct = allProduct.Where(i => i.ProNameWithLc.ToLower().Contains(prefix.ToLower())).ToList();
                    return Json(allProduct, JsonRequestBehavior.AllowGet);
                }
                
            }
        }
        public PartialViewResult _PlaceCartItem()
        {
            return PartialView();
        }
        public PartialViewResult _CartItemLocation()
        {
            return PartialView();
        }
        public JsonResult LoadAllStoreWare()
        {
            var storeList = (from m in db.StoreLists
                             select new StoreWareList
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
            foreach (var ware in wareList)
            {
                storeList.Add(new StoreWareList { Id = ware.Id, IdWithChar = ware.IdWithChar, Name = ware.Name });
            }
            return Json(storeList, JsonRequestBehavior.AllowGet);
        }
        public ActionResult CartItemAssignedWithPlace(string vname, string[] allPlaceId, string[] allSelectedItem, long userId, long[] allAssignUser, bool isOne, bool isStore)
        {
            int[] AddedLocId = new int[1000]; int addedLocCount = 0;
            string msg = ""; int ColumnId = 0; int opStatus = -1;
            int loopCount = 0;
            int lastCountDis = 0;
            if (db.StoreItemDispatchedHistories.Any())
            {
                lastCountDis = db.StoreItemDispatchedHistories.Count();
            }

            //***************************** Created A Transaction For History *********************************
            StoreItemDispatchedHistory dispatchHis = new StoreItemDispatchedHistory();
            dispatchHis.Name = "RTT_" + ((lastCountDis < 1000) ? lastCountDis.ToString("000") : lastCountDis.ToString());
            dispatchHis.Type = 1;
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
                int deptId = 0, count = 0; string strAllUser = "", countWithChar = "", conchar = "";
                var storedetails = new StoreList(); var waredetails = new WarehouseList();

                if (isOne)
                {
                    deptId = Convert.ToInt32(allPlaceId[j].Split('-')[0]);
                    countWithChar = (allPlaceId[j].Split('-')[1]);
                    conchar = countWithChar.Split('|')[0];
                    count = Convert.ToInt32(countWithChar.Split('|')[1]);
                }
                else
                {
                    deptId = Convert.ToInt32(allPlaceId[j].Split('-')[0]);
                    countWithChar = (allPlaceId[j].Split('-')[1]);
                    conchar = countWithChar.Split('|')[0];
                    count = Convert.ToInt32(countWithChar.Split('|')[1]);
                    strAllUser = countWithChar.Split('|')[2];
                }
                storedetails = db.StoreLists.Find(deptId);
                waredetails = db.WarehouseLists.Find(deptId);


                for (int i = 0; i < count; i++)
                {
                    int locationid = Convert.ToInt32(allSelectedItem[loopCount].Split('-')[0]);
                    double quan = Convert.ToDouble(allSelectedItem[loopCount].Split('-')[1]);
                    var inven_loc = db.Inventory_Item_Location.Find(locationid);
                    var proDetails = db.View_All_InventoryList.Where(m => m.InventoryId == inven_loc.InventoryId).FirstOrDefault();
                    if (quan > 0)
                    {
                        var dep_details = db.DepartmentLists.Find(deptId);
                        DispatchedItemList dis_Item = new DispatchedItemList();
                        dis_Item.DispatchedId = disHisId;
                        dis_Item.InventoryId = inven_loc.InventoryId;
                        dis_Item.LocationId = locationid;
                        if (conchar == "w")
                        {
                            dis_Item.WareId = deptId;
                        }
                        else
                        {
                            dis_Item.StoreId = deptId;
                        }
                        dis_Item.Quantity = quan;
                        dis_Item.IsAlreadyUsed = false;
                        db.DispatchedItemLists.Add(dis_Item);
                        db.SaveChanges();
                        try
                        {
                            db.SaveChanges();
                            if (conchar == "w")
                            {
                                msg = "Inventory item '" + proDetails.ProductName + "' quantity " + quan + " " + proDetails.UnitName + " has been successfully routed to  warehouse '" + waredetails.WarehouseName + "' on " + now.ToString("MMM dd , yyyy hh:mm tt") + ".";
                            }
                            else
                            {
                                msg = "Inventory item '" + proDetails.ProductName + "' quantity " + quan + " " + proDetails.UnitName + " has been successfully routed to  store '" + storedetails.StoreName + "' on " + now.ToString("MMM dd , yyyy hh:mm tt") + ".";
                            }
                            ColumnId = Convert.ToInt32(dis_Item.Id);
                            opStatus = 1;
                        }
                        catch (Exception)
                        {
                            if (conchar == "w")
                            {
                                msg = "Inventory item '" + proDetails.ProductName + "' quantity " + quan + " " + proDetails.UnitName + " dispatch for warehouse '" + waredetails.WarehouseName + "' on " + now.ToString("MMM dd , yyyy hh:mm tt") + " was unsuccessful.";
                            }
                            else
                            {
                                msg = "Inventory item '" + proDetails.ProductName + "' quantity " + quan + " " + proDetails.UnitName + " dispatch for store '" + storedetails.StoreName + "' on " + now.ToString("MMM dd , yyyy hh:mm tt") + " was unsuccessful.";
                            }
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
                        }
                        loopCount++;

                        if (!isOne)
                        {
                            string[] allUserId = strAllUser.Split(',');
                            for (int u = 0; u < allUserId.Length; u++)
                            {
                                long assigUserId = Convert.ToInt64(allUserId[u]);
                                if (!db.DispatchedItemAssignUsers.Where(m => m.DispatchId == disHisId && m.UserId == assigUserId && m.DeptId == deptId).Any())
                                {
                                    DispatchedItemAssignUser dis_user = new DispatchedItemAssignUser();
                                    dis_user.DispatchId = disHisId;
                                    dis_user.DispatchItemId = Convert.ToInt32(dis_Item.Id);
                                    dis_user.UserId = assigUserId;
                                    dis_user.DeptId = deptId;
                                    db.DispatchedItemAssignUsers.Add(dis_user);
                                    db.SaveChanges();
                                }
                            }
                        }
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
        public ActionResult AllInvntProListForLcDetails()
        {
            return View();
        }
        #endregion

        //******************************************* Show All Dispatch History ********************************************
        #region Inventory Item Dispatch History
        public ActionResult InvenDisVoucherList(bool? isStore)
        {
            ViewBag.isStore = isStore;
            return View();
        }
        public PartialViewResult _InvenDisVoucherList(bool? isStore, int? AssignType)
        {
            var list = db.View_Inven_Dis_Voucher_List.Where(m => m.Type == 2).OrderByDescending(m => m.DispatchedDate).ToList();
            if (AssignType > 0)
            {
                list = list.Where(m => m.Type == 2 && m.AssignType == AssignType).ToList();
            }
            if (isStore != null)
            {
                list = list.Where(m => m.IsStore == isStore).ToList();
            }
            Session["AllInvenDisVouList"] = list;
            Session["TotalInvenDisVouCount"] = list.Count();
            ViewBag.Count = list.Count();
            list = list.Take(DisVouPerPage).ToList();
            return PartialView(list);
        }
        public PartialViewResult _InvenDisVoucherListDayWise(bool? isStore, int days, int? AssignType)
        {
            var list = db.View_Inven_Dis_Voucher_List.Where(m => m.Type == 2).OrderByDescending(m => m.DispatchedDate).ToList();
            if (AssignType > 0)
            {
                list = list.Where(m => m.Type == 2 && m.AssignType == AssignType).ToList();
            }
            if (isStore != null)
            {
                list = list.Where(m => m.IsStore == isStore).ToList();
            }
            DateTime countDate = now.Date;
            int day = Convert.ToInt32(days);
            if (day > 1)
            {
                countDate = countDate.AddDays(-(day));
            }
            if (day > 0)
            {
                list = list.Where(m => m.DispatchedDate > countDate).OrderByDescending(m => m.DispatchedDate).ToList();
            }
            Session["AllInvenDisVouList"] = list;
            Session["TotalInvenDisVouCount"] = list.Count();
            ViewBag.Count = list.Count();
            list = list.Take(DisVouPerPage).ToList();
            return PartialView("_InvenDisVoucherList", list);
        }
        public JsonResult GetInvenVoucher(int? page)
        {
            page = page ?? 0;
            int total = Convert.ToInt16(Session["TotalInvenDisVouCount"]);
            int skip = DisVouPerPage * Convert.ToInt16(page);
            var canPage = skip < total;
            if (canPage)
            {
                var listBackFromSession = (List<View_Inven_Dis_Voucher_List>)Session["AllInvenDisVouList"];
                var partslist = listBackFromSession.Skip(skip).Take(DisVouPerPage).ToList();
                return Json(partslist, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json("", JsonRequestBehavior.AllowGet);
            }
        }
        [EncryptedActionParameter]
        public ActionResult DispatchHistoryDetails(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            else
            {
                var dis_his = (from m in db.StoreItemDispatchedHistories
                               where m.Id == id
                               select new StoreItemDispatchedHistoryModelView
                               {
                                   Id = m.Id,
                                   Name = m.Name,
                                   VoucherName = m.VoucherName,
                                   DispatchedBy = m.DispatchedBy,
                                   DispatchedDate = m.DispatchedDate,
                                   DispatchUserName =(m.DispatchedBy > 0)?db.View_UserLists.Where(u => u.UserId == m.DispatchedBy).Select(u => u.UserName).FirstOrDefault():"Super Admin",
                                   DispatchUserPic = (m.DispatchedBy > 0) ? db.View_UserLists.Where(u => u.UserId == m.DispatchedBy).Select(u => u.Picture).FirstOrDefault() : "admin.jpg",
                               }).FirstOrDefault();
                return View(dis_his);
            }
        }
        [EncryptedActionParameter]
        public ActionResult DispatchHistoryDetailsPrint(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            else
            {
                var dis_his = (from m in db.StoreItemDispatchedHistories
                               where m.Id == id
                               select new StoreItemDispatchedHistoryModelView
                               {
                                   Id = m.Id,
                                   Name = m.Name,
                                   VoucherName = m.VoucherName,
                                   DispatchedBy = m.DispatchedBy,
                                   DispatchedDate = m.DispatchedDate,
                                   DispatchUserName = (m.DispatchedBy > 0) ? db.View_UserLists.Where(u => u.UserId == m.DispatchedBy).Select(u => u.UserName).FirstOrDefault() : "Super Admin",
                                   DispatchUserPic = (m.DispatchedBy > 0) ? db.View_UserLists.Where(u => u.UserId == m.DispatchedBy).Select(u => u.Picture).FirstOrDefault() : "admin.jpg",
                               }).FirstOrDefault();
                return View(dis_his);
            }
        }
        public ActionResult InventoryItemDispatchHis()
        {
            return View();
        }
        public PartialViewResult _InventoryItemDispatchHis()
        {
            var list = (from bs in db.View_Dispacth_Item_Details
                        group bs by bs.InventoryId into g
                        orderby g.Sum(x => x.Quantity)
                        select new View_Dispacth_Item_DetailsModelView
                        {
                            Id = g.FirstOrDefault().Id,
                            DispatchedId = g.FirstOrDefault().DispatchedId,
                            Name = g.FirstOrDefault().Name,
                            VoucherName=g.FirstOrDefault().VoucherName,
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
            Session["AllInvenDisHisList"] = list;
            Session["TotalInvenDisHisCount"] = list.Count();
            ViewBag.Count = list.Count();
            ViewBag.list = list.Take(DisHisPerPage).ToList();
            return PartialView();
        }
        public PartialViewResult _InventoryItemDispatchHisDayWise(int days)
        {
            DateTime countDate = DateTime.Today;
            var AllList = db.View_Dispacth_Item_Details.ToList();
            int day = Convert.ToInt32(days);
            if (day > 1)
            {
                countDate = countDate.AddDays(-(day));
            }
            if (day > 0)
            {
                AllList = AllList.Where(m => m.DispatchedDate > countDate).OrderByDescending(m => m.DispatchedDate).ToList();
            }

            var list = (from bs in AllList
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
                        }).OrderByDescending(o => o.DispatchedDate).ToList();

            
            Session["AllInvenDisHisList"] = list;
            Session["TotalInvenDisHisCount"] = list.Count();
            ViewBag.Count = list.Count();
            ViewBag.list = list.Take(DisHisPerPage).ToList();
            return PartialView("_InventoryItemDispatchHis", ViewBag.list);
        }
        public JsonResult GetInvenItemDisHis(int? page)
        {
            page = page ?? 0;
            int total = Convert.ToInt16(Session["TotalInvenDisHisCount"]);
            int skip = DisHisPerPage * Convert.ToInt16(page);
            var canPage = skip < total;
            if (canPage)
            {
                var listBackFromSession = (List<View_Dispacth_Item_DetailsModelView>)Session["AllInvenDisHisList"];
                var partslist = listBackFromSession.Skip(skip).Take(DisHisPerPage).ToList();
                return Json(partslist, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json("", JsonRequestBehavior.AllowGet);
            }
        }
        public PartialViewResult ShowInvenDispacthQuanDetails(int invenid, int? DisId, int? days)
        {
            if (DisId == null)
            {
                ViewBag.hasdisId = false;
                DateTime countDate = DateTime.Today;
                var list = db.View_Dispacth_Item_Details.Where(m => m.InventoryId == invenid).ToList();
                int day = Convert.ToInt32(days);
                if (day > 1)
                {
                    countDate = countDate.AddDays(-(day));
                }
                if (day > 0)
                {
                    list = list.Where(m => m.DispatchedDate > countDate).OrderByDescending(m => m.DispatchedDate).ToList();
                }
                return PartialView(list);
            }
            else
            {
                ViewBag.hasdisId = true;
                var list = db.View_Dispacth_Item_Details.Where(m => m.LocationId == invenid && m.DispatchedId == DisId).ToList();
                return PartialView(list);
            }
        }
        public PartialViewResult ShowInvenDispacthReQuanDetails(int invenid, int? DisId)
        {
            if (DisId == null)
            {
                ViewBag.hasdisId = false;
                var list = db.View_Dispacth_Item_Details.Where(m => m.InventoryId == invenid && m.ReturnQuantity > 0).ToList();
                return PartialView(list);
            }
            else
            {
                ViewBag.hasdisId = true;
                var list = db.View_Dispacth_Item_Details.Where(m => m.LocationId == invenid && m.ReturnQuantity > 0  && m.DispatchedId == DisId).ToList();
                return PartialView(list);
            }
        }
        public PartialViewResult DispatchItemQuanEditMain(long id)
        {
            ViewBag.id = id;
            return PartialView();
        }
        public PartialViewResult DispatchItemQuanEdit(long id)
        {
            var model = db.View_Dispacth_Item_Details.Where(m => m.Id == id).FirstOrDefault();
            ViewBag.ReasonId = new SelectList(db.TransactionEditReasons, "Id", "Reason");
            return PartialView(model);
        }
        public JsonResult DisItemReturnQuan(long Id, int ReturnReasonId, double ReturnQuantity, long ReturnBy,string proname, string voucher)
        {
            if (Id > 0 && ReturnReasonId > 0 && ReturnQuantity > 0)
            {
                DispatchedItemList model = db.DispatchedItemLists.Find(Id);
                if (model != null)
                {
                    int status = -1;
                    string msg = "";
                    model.ReturnQuantity = Convert.ToDouble(model.ReturnQuantity) + ReturnQuantity;
                    model.ReturnReasonId = ReturnReasonId;
                    model.ReturnBy = ReturnBy;
                    model.ReturnDate = now;
                    db.Entry(model).State = EntityState.Modified;
                    try
                    {
                        db.SaveChanges();
                        status = 1;
                        msg = "Dispatch item '" + proname + "' in voucher '" + voucher + "' quantity has been successfully edited on " + now.ToString("MMM dd , yyy hh:mm tt") + " .";
                    }
                    catch (Exception)
                    {
                        status = -1;
                        msg = "Dispatch item '" + proname + "' in voucher '" + voucher + "' quantity edit was unsuccessful on " + now.ToString("MMM dd , yyy hh:mm tt") + " .";
                    }
                    SaveAuditLog("Dispatch Item Quantity Edit", msg, "Inventory", "DispatchHistoryDetails", "DispatchedItemList", Convert.ToInt32(Id), ReturnBy, now, status);
                    if (status == 1)
                    {
                        Inventory_Item_Location inven_loc = db.Inventory_Item_Location.Find(model.LocationId);
                        inven_loc.Quantity = Convert.ToDouble(inven_loc.Quantity) + ReturnQuantity;
                        inven_loc.UpdatedDate = now;
                        inven_loc.UpdatedBy = ReturnBy;
                        db.Entry(inven_loc).State = EntityState.Modified;
                        try
                        {
                            db.SaveChanges();
                            status = 1;
                            msg = "Inventory item '" + proname + "' quantity has been successfully updated on " + now.ToString("MMM dd , yyy hh:mm tt") + " .";
                        }
                        catch (Exception)
                        {
                            status = -1;
                            msg = "Inventory item '" + proname + "' quantity updated was unsuccessful on " + now.ToString("MMM dd , yyy hh:mm tt") + " .";
                        }
                        SaveAuditLog("Inventory Item Quantity Updated", msg, "Inventory", "DispatchHistoryDetails", "Inventory_Item_Location", Convert.ToInt32(inven_loc.LocationId), ReturnBy, now, status);
                        return Json("Success", JsonRequestBehavior.AllowGet);
                    }
                }
            }
            return Json("Error", JsonRequestBehavior.AllowGet);
        }
        public PartialViewResult ShowInvenDispacthUsedQuanDetails(int invenid, int? DisId)
        {
            if (DisId == null)
            {
                ViewBag.hasdisId = false;
                var list = db.View_Dispacth_Item_Details.Where(m => m.InventoryId == invenid && m.UsedQuan > 0).ToList();
                return PartialView(list);
            }
            else
            {
                ViewBag.hasdisId = true;
                var list = db.View_Dispacth_Item_Details.Where(m => m.InventoryId == invenid && m.UsedQuan > 0 && m.DispatchedId == DisId).ToList();
                return PartialView(list);
            }
        }
        #endregion

        //******************************************* Show All Routed History **********************************************
        #region Inventory Item Routed History
        public ActionResult InvenRouteVoucherList(bool? isStore)
        {
            ViewBag.isStore = isStore;
            return View();
        }
        public PartialViewResult _InvenRouteVoucherList(bool? isStore)
        {
            var list = db.View_Inven_Dis_Voucher_List.Where(m=> m.Type == 1).OrderByDescending(m => m.DispatchedDate).ToList();
            if (isStore != null)
            {
                list = list.Where(m => m.IsStore == isStore).ToList();
            }
            Session["AllInvenRouteVouList"] = list;
            Session["TotalInvenRouteVouCount"] = list.Count();
            ViewBag.Count = list.Count();
            list = list.Take(RouteVouPerPage).ToList();
            return PartialView(list);
        }
        public PartialViewResult _InvenRouteVoucherListDayWise(bool? isStore, int days)
        {
            var list = db.View_Inven_Dis_Voucher_List.Where(m => m.Type == 1).OrderByDescending(m => m.DispatchedDate).ToList();
            if (isStore != null)
            {
                list = list.Where(m => m.IsStore == isStore).ToList();
            }
            DateTime countDate = DateTime.Today;
            int day = Convert.ToInt32(days);
            if (day > 1)
            {
                countDate = countDate.AddDays(-(day));
            }
            if (day > 0)
            {
                list = list.Where(m => m.DispatchedDate > countDate).OrderByDescending(m => m.DispatchedDate).ToList();
            }
            Session["AllInvenRouteVouList"] = list;
            Session["TotalInvenRouteVouCount"] = list.Count();
            ViewBag.Count = list.Count();
            list = list.Take(RouteVouPerPage).ToList();
            return PartialView("_InvenRouteVoucherList", list);
        }
        public JsonResult GetInvenRouteVoucher(int? page)
        {
            page = page ?? 0;
            int total = Convert.ToInt16(Session["TotalInvenRouteVouCount"]);
            int skip = DisVouPerPage * Convert.ToInt16(page);
            var canPage = skip < total;
            if (canPage)
            {
                var listBackFromSession = (List<View_Inven_Dis_Voucher_List>)Session["AllInvenRouteVouList"];
                var partslist = listBackFromSession.Skip(skip).Take(RouteVouPerPage).ToList();
                return Json(partslist, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json("", JsonRequestBehavior.AllowGet);
            }
        }
        [EncryptedActionParameter]
        public ActionResult RoutedHistoryDetails(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            else
            {
                var dis_his = (from m in db.StoreItemDispatchedHistories
                               where m.Id == id
                               select new StoreItemDispatchedHistoryModelView
                               {
                                   Id = m.Id,
                                   Name = m.Name,
                                   VoucherName = m.VoucherName,
                                   DispatchedBy = m.DispatchedBy,
                                   DispatchedDate = m.DispatchedDate,
                                   DispatchUserName = (m.DispatchedBy > 0) ? db.View_UserLists.Where(u => u.UserId == m.DispatchedBy).Select(u => u.UserName).FirstOrDefault() : "Super Admin",
                                   DispatchUserPic = (m.DispatchedBy > 0) ? db.View_UserLists.Where(u => u.UserId == m.DispatchedBy).Select(u => u.Picture).FirstOrDefault() : "admin.jpg"
                               }).FirstOrDefault();
                return View(dis_his);
            }
        }
        public PartialViewResult ShowInvenRouteQuanDetails(int invenid, int? DisId, int? days)
        {
            if (DisId == null)
            {
                ViewBag.hasdisId = false;
                DateTime countDate = DateTime.Today;
                var list = db.View_Dispacth_Item_Details.Where(m => m.InventoryId == invenid).ToList();
                int day = Convert.ToInt32(days);
                if (day > 1)
                {
                    countDate = countDate.AddDays(-(day));
                }
                if (day > 0)
                {
                    list = list.Where(m => m.DispatchedDate > countDate).OrderByDescending(m => m.DispatchedDate).ToList();
                }
                return PartialView(list);
            }
            else
            {
                ViewBag.hasdisId = true;
                var list = db.View_Dispacth_Item_Details.Where(m => m.InventoryId == invenid && m.DispatchedId == DisId).ToList();
                return PartialView(list);
            }
        }
        public PartialViewResult ShowInvenRouteReQuanDetails(int invenid, int? DisId)
        {
            if (DisId == null)
            {
                ViewBag.hasdisId = false;
                var list = db.View_Dispacth_Item_Details.Where(m => m.InventoryId == invenid && m.ReturnQuantity > 0).ToList();
                return PartialView(list);
            }
            else
            {
                ViewBag.hasdisId = true;
                var list = db.View_Dispacth_Item_Details.Where(m => m.InventoryId == invenid && m.ReturnQuantity > 0 && m.DispatchedId == DisId).ToList();
                return PartialView(list);
            }
        }
        public PartialViewResult RouteItemInsertMain(long id)
        {
            ViewBag.id = id;
            return PartialView();
        }
        public PartialViewResult RouteItemInsert(long id)
        {
            var model = db.View_Dispacth_Item_Details.Where(m => m.Id == id).FirstOrDefault();
            ViewBag.ReasonId = new SelectList(db.TransactionEditReasons, "Id", "Reason");
            return PartialView(model);
        }
        public JsonResult RouteItemInsertSave(long Id,int inven, double Quantity, string proname, string voucher, long UserId,int? storeid,int? wareid)
        {
            if (Id > 0 && Quantity > 0 && UserId > 0)
            {
                DispatchedItemList model = db.DispatchedItemLists.Find(Id);
                if (model != null)
                {
                    int status = -1;
                    string msg = "";
                    model.IsAlreadyUsed = true;
                    model.UsedQuan = Quantity;
                    model.InsertedBy = UserId;
                    model.InsertedDate = now;
                    db.Entry(model).State = EntityState.Modified;
                    try
                    {
                        db.SaveChanges();
                        status = 1;
                        msg = "Routed item '" + proname + "' in voucher '" + voucher + "' quantity has been successfully inserted on " + now.ToString("MMM dd , yyy hh:mm tt") + " .";
                    }
                    catch (Exception)
                    {
                        status = -1;
                        msg = "Routed item '" + proname + "' in voucher '" + voucher + "' quantity insert was unsuccessful on " + now.ToString("MMM dd , yyy hh:mm tt") + " .";
                    }
                    SaveAuditLog("Routed Item Insert", msg, "Inventory", "DispatchHistoryDetails", "DispatchedItemList", Convert.ToInt32(Id), UserId, now, status);
                    if (status == 1)
                    {
                        int UnitId = db.View_All_InventoryList.Where(m => m.InventoryId == inven).Select(m => m.UnitId).FirstOrDefault();
                        Inventory_Item_Location inven_loc = new Inventory_Item_Location();
                        if(storeid > 0)
                        {
                            if (db.Inventory_Item_Location.Where(m => m.InventoryId == inven && m.StoreId == storeid).Any())
                            {
                                inven_loc = db.Inventory_Item_Location.Where(m => m.InventoryId == inven && m.StoreId == storeid).FirstOrDefault();
                                inven_loc.Quantity = Convert.ToDouble(inven_loc.Quantity) + Quantity;
                                inven_loc.UpdatedDate = now;
                                inven_loc.UpdatedBy = UserId;
                                db.Entry(inven_loc).State = EntityState.Modified;
                            }
                            else
                            {
                                inven_loc.InventoryId = inven;
                                inven_loc.StoreId = storeid;
                                inven_loc.Quantity = Convert.ToDouble(inven_loc.Quantity) + Quantity;
                                inven_loc.UnitId = UnitId;
                                inven_loc.InsertedDate = now;
                                inven_loc.InsertedBy = UserId;
                                db.Inventory_Item_Location.Add(inven_loc);
                            }
                        }
                        if (wareid > 0)
                        {
                            if (db.Inventory_Item_Location.Where(m => m.InventoryId == inven && m.WarehouseId == wareid).Any())
                            {
                                inven_loc = db.Inventory_Item_Location.Where(m => m.InventoryId == inven && m.WarehouseId == wareid).FirstOrDefault();
                                inven_loc.Quantity = Convert.ToDouble(inven_loc.Quantity) + Quantity;
                                inven_loc.UpdatedDate = now;
                                inven_loc.UpdatedBy = UserId;
                                db.Entry(inven_loc).State = EntityState.Modified;
                            }
                            else
                            {
                                inven_loc.InventoryId = inven;
                                inven_loc.WarehouseId = wareid;
                                inven_loc.Quantity = Convert.ToDouble(inven_loc.Quantity) + Quantity;
                                inven_loc.UnitId = UnitId;
                                inven_loc.InsertedDate = now;
                                inven_loc.InsertedBy = UserId;
                                db.Inventory_Item_Location.Add(inven_loc);
                            }
                        }
                        try
                        {
                            db.SaveChanges();
                            status = 1;
                            msg = "Inventory item '" + proname + "' quantity has been successfully updated on " + now.ToString("MMM dd , yyy hh:mm tt") + " .";
                        }
                        catch (Exception)
                        {
                            status = -1;
                            msg = "Inventory item '" + proname + "' quantity updated was unsuccessful on " + now.ToString("MMM dd , yyy hh:mm tt") + " .";
                        }
                        SaveAuditLog("Inventory Item Quantity Updated", msg, "Inventory", "DispatchHistoryDetails", "Inventory_Item_Location", Convert.ToInt32(inven_loc.LocationId), UserId, now, status);
                        return Json("Success", JsonRequestBehavior.AllowGet);
                    }
                }
            }
            return Json("Error", JsonRequestBehavior.AllowGet);
        }
        [EncryptedActionParameter]
        public ActionResult RoutedHistoryDetailsPrint(int id)
        {
            if (id == 0)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var dis_his = (from m in db.StoreItemDispatchedHistories
                           where m.Id == id
                           select new StoreItemDispatchedHistoryModelView
                           {
                               Id = m.Id,
                               Name = m.Name,
                               VoucherName = m.VoucherName,
                               DispatchedBy = m.DispatchedBy,
                               DispatchedDate = m.DispatchedDate,
                               DispatchUserName = (m.DispatchedBy > 0) ? db.View_UserLists.Where(u => u.UserId == m.DispatchedBy).Select(u => u.UserName).FirstOrDefault() : "Super Admin",
                               DispatchUserPic = (m.DispatchedBy > 0) ? db.View_UserLists.Where(u => u.UserId == m.DispatchedBy).Select(u => u.Picture).FirstOrDefault() : "admin.jpg"
                           }).FirstOrDefault();
            return View(dis_his);
        }
        public JsonResult RouteVoucherDelete(long id, long deletedBy)
        {
            int status = -1;
            StoreItemDispatchedHistory route = db.StoreItemDispatchedHistories.Find(id);
            if (route != null)
            {
                var allRouteItem = db.DispatchedItemLists.Where(m => m.DispatchedId == route.Id).ToList();
                foreach(var i in allRouteItem)
                {
                    var location = db.Inventory_Item_Location.Find(i.LocationId);
                    location.Quantity = location.Quantity + i.Quantity;
                    db.Entry(location).State = EntityState.Modified;
                    db.DispatchedItemLists.Remove(i);
                    db.SaveChanges();
                }

                db.StoreItemDispatchedHistories.Remove(route);
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
            return Json("Error", JsonRequestBehavior.AllowGet);
        }

        #endregion

        //******************************************* Show All IMPORT History **********************************************
        #region
        public ActionResult ImportHistoryList()
        {
            return View();
        }
        public PartialViewResult _ImportHistoryList(int? days)
        {
            var list = db.View_Import_History_Details.OrderByDescending(m => m.InsertedDate).ToList();
            if (days > 0)
            {
                DateTime countDate = now.Date;
                int day = Convert.ToInt32(days);
                countDate = countDate.AddDays(-(day));
                list = list.Where(m => m.InsertedDate > countDate).OrderByDescending(m => m.InsertedDate).ToList();
            }
            Session["AllImportHistoryList"] = list;
            Session["TotalImportHistoryCount"] = list.Count();
            ViewBag.Count = list.Count();
            list = list.Take(ImportPerPage).ToList();
            return PartialView(list);
        } 
        public PartialViewResult _ImportHistoryLCList()
        {
            var list = db.View_Import_History_Details.GroupBy(m => m.L_C).Select(m => m.FirstOrDefault()).OrderByDescending(m => m.InsertedDate).ToList();
            Session["AllImportHistoryLCList"] = list;
            Session["TotalImportHistoryLCCount"] = list.Count();
            ViewBag.Count = list.Count();
            list = list.Take(ImportPerPageLC).ToList();
            return PartialView(list);
        }
        public JsonResult GetImportHistory(int? page, int tab)
        {
            page = page ?? 0;
            if (tab == 1)
            {
                int total = Convert.ToInt16(Session["TotalImportHistoryCount"]);
                int skip = ImportPerPage * Convert.ToInt16(page);
                var canPage = skip < total;
                if (canPage)
                {
                    var listBackFromSession = (List<View_Import_History_Details>)Session["AllImportHistoryList"];
                    var list = listBackFromSession.Skip(skip).Take(ImportPerPage).ToList();
                    return Json(list, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json("", JsonRequestBehavior.AllowGet);
                }
            }
            else
            {
                int total = Convert.ToInt16(Session["TotalImportHistoryLCCount"]);
                int skip = ImportPerPageLC * Convert.ToInt16(page);
                var canPage = skip < total;
                if (canPage)
                {
                    var listBackFromSession = (List<View_Import_History_Details>)Session["AllImportHistoryLCList"];
                    var list = listBackFromSession.Skip(skip).Take(ImportPerPageLC).ToList();
                    return Json(list, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json("", JsonRequestBehavior.AllowGet);
                }
            }
        }
        public PartialViewResult _ImportHistoryListLCDayWise(int days)
        {
            var list = db.View_Import_History_Details.GroupBy(m => m.L_C).Select(m => m.FirstOrDefault()).OrderByDescending(m => m.InsertedDate).ToList();
            DateTime countDate = DateTime.Today;
            int day = Convert.ToInt32(days);
            if (day > 1)
            {
                countDate = countDate.AddDays(-(day));
            }
            if (day > 0)
            {
                list = list.Where(m => m.InsertedDate > countDate).OrderByDescending(m => m.InsertedDate).ToList();
            }
            Session["AllImportHistoryLCList"] = list;
            Session["TotalImportHistoryLCCount"] = list.Count();
            ViewBag.Count = list.Count();
            list = list.Take(ImportPerPage).ToList();
            return PartialView("_ImportHistoryLCList", list);
        }
        public PartialViewResult ImportHistoryDetails(int? id)
        {
            if (id == null)
            {
                return PartialView();
            }
            else
            {
                var dis_his = db.View_Import_History_Details.Where(x => x.ImportId == id).FirstOrDefault();

                return PartialView(dis_his);
            }
        }
        public PartialViewResult ImportHistoryDetailsLC(string lc)
        {
            if (lc == String.Empty)
            {
                return PartialView();
            }
            else
            {
                var dis_his = db.View_Import_History_Details.Where(x => x.L_C == lc).FirstOrDefault();
                return PartialView(dis_his);
            }
        }
        public PartialViewResult _ImportHistoryListStoreWareWise(int ImportedTo)
        {
            List<View_Import_History_Details> list = new List<View_Import_History_Details>();
            List<long> importId = new List<long>();
            if (ImportedTo == 1)
            {
                importId = db.Import_Item_Location.Where(x => x.WareId != 0 && x.WareId != null).Select(x => x.ImportId).Distinct().ToList();
            }
            else if (ImportedTo == 2)
            {
                importId = db.Import_Item_Location.Where(x => x.StoreId != 0 && x.StoreId != null).Select(x => x.ImportId).Distinct().ToList();
            }
            foreach (var id in importId)
            {
                list.Add(db.View_Import_History_Details.Where(x => x.ImportId == id).FirstOrDefault());
            }
            Session["AllImportHistoryList"] = list;
            Session["TotalImportHistoryCount"] = list.Count();
            ViewBag.Count = list.Count();
            list = list.Take(DisVouPerPage).ToList();
            return PartialView("_ImportHistoryList", list);
        }
        [EncryptedActionParameter]
        public ActionResult ImportHistoryDetailsPrint(int id)
        {
            var dis_his = db.View_Import_History_Details.Where(x => x.ImportId == id).FirstOrDefault();
            return View(dis_his);
        }
        [EncryptedActionParameter]
        public ActionResult ImportHisLCDetailsPrint(int id)
        {
            var dis_his = db.View_Import_History_Details.Where(x => x.ImportId == id).FirstOrDefault();
            return View(dis_his);
        }
        #endregion

        //*************************************** LC WISE IMPORT AND DISPATCH DETAILS ***************************************
        #region
        public ActionResult LCList()
        {
            return View();
        }
        public PartialViewResult _LCList(long? id)
        {
            var list = db.View_Import_History_Details.GroupBy(m => m.L_C).Select(m => m.FirstOrDefault()).OrderByDescending(m => m.LastImpDate).ToList();
            if (id > 0)
            {
                list = list.Where(m => m.ImportId == id).ToList();
            }
            Session["AllLCList"] = list;
            Session["TotalLCCount"] = list.Count();
            ViewBag.Count = list.Count();
            list = list.Take(20).ToList();
            return PartialView(list);
        }
        public PartialViewResult _LCListDayWise(int days)
        {
            var list = db.View_Import_History_Details.GroupBy(m => m.L_C).Select(m => m.FirstOrDefault()).OrderByDescending(m => m.LastImpDate).ToList();
            DateTime countDate = DateTime.Today;
            int day = Convert.ToInt32(days);
            if (day > 1)
            {
                countDate = countDate.AddDays(-(day));
            }
            if (day > 0)
            {
                list = list.Where(m => m.LastImpDate > countDate).OrderByDescending(m => m.LastImpDate).ToList();
            }
            Session["AllLCList"] = list;
            Session["TotalLCCount"] = list.Count();
            ViewBag.Count = list.Count();
            list = list.Take(20).ToList();
            return PartialView("_LCList", list);
        }
        public JsonResult GetMoreLC(int? page)
        {
            page = page ?? 0;
            int total = Convert.ToInt16(Session["TotalLCCount"]);
            int skip = 20 * Convert.ToInt16(page);
            var canPage = skip < total;
            if (canPage)
            {
                var listFromSession = (List<View_Import_History_Details>)Session["AllLCList"];
                var list = listFromSession.Skip(skip).Take(20).ToList();
                return Json(list, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json("", JsonRequestBehavior.AllowGet);
            }
        }
        public JsonResult SearchLCList(string prefix)
        {
            if (prefix == "")
            {
                return Json("", JsonRequestBehavior.AllowGet);
            }
            else
            {
                var LcList = (List<View_Import_History_Details>)Session["AllLCList"];
                LcList = LcList.Where(m => m.L_C.ToLower().Contains(prefix.ToLower())).ToList();
                return Json(LcList, JsonRequestBehavior.AllowGet);
            }
        }
        public PartialViewResult ImportExportDetailsLC(string lc)
        {
            if (lc == String.Empty)
            {
                return PartialView();
            }
            else
            {
                var dis_his = db.View_Import_History_Details.Where(x => x.L_C == lc).FirstOrDefault();
                return PartialView(dis_his);
            }
        }
        [EncryptedActionParameter]
        public ActionResult ImportExportDetailsLCPrint(int id)
        {
            var dis_his = db.View_Import_History_Details.Where(x => x.ImportId == id).FirstOrDefault();
            return View(dis_his);
        }
        #endregion

        // ************************************ Inventory Item Dispatch History (Machine Wise)*******************************
        #region
        public ActionResult InventoryItemDispatchMachineWise()
        {
            return PartialView();
        }
        public PartialViewResult _InventoryItemDispatchHisMachineWise(int? days)
        {
            var list = (from bs in db.View_Dispacth_Item_Details
                        where bs.MachineId > 0
                        group bs by bs.MachineId into g
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
                            MachineName = g.FirstOrDefault().MachineName,
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
                        }).ToList();

            if (days > 0)
            {
                DateTime countDate = now.Date;
                int day = Convert.ToInt32(days);
                if (day > 1)
                {
                    countDate = countDate.AddDays(-(day));
                }
                list = list.Where(m => m.DispatchedDate > countDate).OrderByDescending(m => m.DispatchedDate).ToList();
            }
            Session["AllMachineDisHisList"] = list;
            Session["TotalMachineDisHisCount"] = list.Count();
            ViewBag.Count = list.Count();
            ViewBag.list = list.Take(DisMAchineWiseHisPerPage).ToList();
            return PartialView();
        }
        public JsonResult GetMachineDisHis(int? page)
        {
            page = page ?? 0;
            int total = Convert.ToInt16(Session["TotalMachineDisHisCount"]);
            int skip = DisMAchineWiseHisPerPage * Convert.ToInt16(page);
            var canPage = skip < total;
            if (canPage)
            {
                var listBackFromSession = (List<View_Dispacth_Item_DetailsModelView>)Session["AllMachineDisHisList"];
                var partslist = listBackFromSession.Skip(skip).Take(DisMAchineWiseHisPerPage).ToList();
                return Json(partslist, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json("", JsonRequestBehavior.AllowGet);
            }
        }
        public PartialViewResult ShowInvenDispacthMachineDetails(int? machineid)
        {
            var list = db.View_Dispacth_Item_Details.Where(m => m.MachineId == machineid).ToList();
            return PartialView(list);
        }
        #endregion

        // ************************************ Inventory item dispatch (dept wise)******************************************
        #region
        public ActionResult InventoryItemDispatchDeptWise()
        {
            var list = (from bs in db.View_Dispacth_Item_Details
                        where bs.DeptId > 0
                        group bs by bs.DeptId into g
                        select new View_Dispacth_Item_DetailsModelView
                        {
                            Quantity = g.FirstOrDefault().Quantity,
                            DeptId = g.FirstOrDefault().DeptId,
                            DeptName = g.FirstOrDefault().DeptName,
                            DeptName_Unit = g.FirstOrDefault().DeptName_Unit,
                        }).ToList();
            return PartialView(list);
        }
        public ActionResult _DeptWiseDispatchList(int? dId, int? days, string v)
        {
            var list = db.View_Dispacth_Item_Details.Where(m => m.DeptId > 0).OrderByDescending(m => m.DispatchedDate).ToList();
            if (dId > 0)
            {
                list = list.Where(m => m.DeptId == dId).ToList();
            }
            if (days > 0)
            {
                DateTime countDate = now.Date;
                int day = Convert.ToInt32(days);
                countDate = countDate.AddDays(-(day));
                list = list.Where(m => m.DispatchedDate > countDate).OrderByDescending(m => m.DispatchedDate).ToList();
            }
            Session["AllDeptDisHisList"] = list;
            Session["TotalDeptDisHisCount"] = list.Count();
            ViewBag.v = v;
            ViewBag.Count = list.Count();
            ViewBag.list = list.Take(DisMAchineWiseHisPerPage).ToList();
            return PartialView();
        }
        public JsonResult GetDeptDisHis(int? page)
        {
            page = page ?? 0;
            int total = Convert.ToInt16(Session["TotalDeptDisHisCount"]);
            int skip = DisDeptWiseHisPerPage * Convert.ToInt16(page);
            var canPage = skip < total;
            if (canPage)
            {
                var listBackFromSession = (List<View_Dispacth_Item_Details>)Session["AllDeptDisHisList"];
                var partslist = listBackFromSession.Skip(skip).Take(DisDeptWiseHisPerPage).ToList();
                return Json(partslist, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json("", JsonRequestBehavior.AllowGet);
            }
        }
        public PartialViewResult ShowInvenDispacthDeptDetails(int? deptid)
        {
            var list = db.View_Dispacth_Item_Details.Where(m => m.DeptId == deptid).ToList();
            return PartialView(list);
        }
        public PartialViewResult _InventoryItemDispatchDeptWise()
        {
            var list = (from bs in db.View_Dispacth_Item_Details
                        where bs.DeptId > 0
                        group bs by bs.DeptId into g
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
                            MachineName = g.FirstOrDefault().MachineName,
                            MachineId = g.FirstOrDefault().MachineId,
                            Site_Unit_Id = g.FirstOrDefault().Site_Unit_Id,
                            DeptId = g.FirstOrDefault().DeptId,
                            DeptName=g.FirstOrDefault().DeptName,
                            DeptName_Unit = g.FirstOrDefault().DeptName_Unit,
                            LineId = g.FirstOrDefault().LineId,
                            DispatchedDate = g.FirstOrDefault().DispatchedDate.Value,
                            DispatchedBy = g.FirstOrDefault().DispatchedBy.Value,
                            DispatchedDateName = g.FirstOrDefault().DispatchedDateName,
                            DispatchedUserName = g.FirstOrDefault().DispatchedUserName,
                            DispatchedUserPic = g.FirstOrDefault().DispatchedUserPic,
                            Total_Quantity = g.Sum(x => x.Quantity),
                            Total_Return = g.Sum(x => x.ReturnQuantity.Value),
                            Total_UsedQuan = g.Sum(x => x.UsedQuan.Value)
                        }).ToList();

            Session["AllDeptDisHisList"] = list;
            Session["TotalDeptDisHisCount"] = list.Count();
            ViewBag.Count = list.Count();
            ViewBag.list = list.Take(DisMAchineWiseHisPerPage).ToList();
            return PartialView();
        }
        public PartialViewResult _InventoryItemDispatchDeptWiseDayWise(int days)
        {
            DateTime countDate = DateTime.Today;
            var AllList = db.View_Dispacth_Item_Details.ToList();
            int day = Convert.ToInt32(days);
            if (day > 1)
            {
                countDate = countDate.AddDays(-(day));
            }
            if (day > 0)
            {
                AllList = AllList.Where(m => m.DispatchedDate > countDate).OrderByDescending(m => m.DispatchedDate).ToList();
            }
            var list = (from bs in AllList
                        where bs.DeptId > 0
                        group bs by bs.DeptId into g
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
                            MachineName = g.FirstOrDefault().MachineName,
                            MachineId = g.FirstOrDefault().MachineId,
                            Site_Unit_Id = g.FirstOrDefault().Site_Unit_Id,
                            DeptId = g.FirstOrDefault().DeptId,
                            DeptName = g.FirstOrDefault().DeptName,
                            DeptName_Unit = g.FirstOrDefault().DeptName_Unit,
                            LineId = g.FirstOrDefault().LineId,
                            DispatchedDate = g.FirstOrDefault().DispatchedDate.Value,
                            DispatchedBy = g.FirstOrDefault().DispatchedBy.Value,
                            DispatchedDateName = g.FirstOrDefault().DispatchedDateName,
                            DispatchedUserName = g.FirstOrDefault().DispatchedUserName,
                            DispatchedUserPic = g.FirstOrDefault().DispatchedUserPic,
                            Total_Quantity = g.Sum(x => x.Quantity),
                            Total_Return = g.Sum(x => x.ReturnQuantity.Value),
                            Total_UsedQuan = g.Sum(x => x.UsedQuan.Value)
                        }).ToList();
            Session["AllDeptDisHisList"] = list;
            Session["TotalDeptDisHisCount"] = list.Count();
            ViewBag.Count = list.Count();
            ViewBag.list = list.Take(DisDeptWiseHisPerPage).ToList();
            return PartialView("_InventoryItemDispatchDeptWise", ViewBag.list);
        }
        #endregion

        #region Image Resize
        public static Image ScaleImage(Image image, int maxWidth, int maxHeight)
        {
            var ratioX = (double)maxWidth / image.Width;
            var ratioY = (double)maxHeight / image.Height;
            var ratio = Math.Min(ratioX, ratioY);

            var newWidth = (int)(image.Width * ratio);
            var newHeight = (int)(image.Height * ratio);

            var newImage = new Bitmap(newWidth, newHeight);

            using (var graphics = Graphics.FromImage(newImage))
                graphics.DrawImage(image, 0, 0, newWidth, newHeight);

            return newImage;
        }
        #endregion

        public class StoreWareList
        {
            public int Id { get; set; }
            public string IdWithChar { get; set; }
            public string Name { get; set; }
        }
        #region
        public ActionResult ProductInsertFor71(int? id)
        {
            ProductItemList model = new ProductItemList();
            if (id != null)
            {
            }
            ViewBag.CategoryId = new SelectList(db.Categories.Where(m => m.HasParent == false).OrderBy(m => m.CategoryName), "CategoryId", "CategoryName", model.CategoryId);
            return View(model);
        }
        public JsonResult ProductTypeHasChildern(int id)
        {
            return Json(db.Categories.Where(m => m.ParentId == id).Any(), JsonRequestBehavior.AllowGet);
        }
        public JsonResult GetAllProductType(int? ParentId)
        {
            if (ParentId != null)
            {
                int id = Convert.ToInt32(ParentId);
                var list = (from m in db.Categories
                            where m.ParentId == id
                            select new
                            {
                                ProductTypeId = m.CategoryId,
                                ProductTypeIdEdit = m.CategoryId,
                                ProductTypeName = m.CategoryName
                            }).ToList();

                return Json(list, JsonRequestBehavior.AllowGet);
            }
            else
            {
                var list = (from m in db.Categories
                            where m.HasParent == false
                            select new
                            {
                                ProductTypeId = m.CategoryId,
                                ProductTypeIdEdit = m.CategoryId,
                                ProductTypeName = m.CategoryName
                            }).ToList();

                return Json(list, JsonRequestBehavior.AllowGet);
            }

        }
        public JsonResult ProductSaveFor71(ProductItemList model, IEnumerable<HttpPostedFileBase> files)
        {
            if (ModelState.IsValid)
            {
                int OperationStatus = -1;
                string picName = "";
                long max_id = 0;
                if (db.ProductItemLists.Any())
                {
                    max_id = db.ProductItemLists.Max(m => m.ItemId);
                }
                ProductItemList pro = new ProductItemList();

                pro.ProductName = model.ProductName;
                pro.CategoryId = model.CategoryId;
                pro.Price = model.Price;
                pro.Quantity = model.Quantity;
                pro.Description = model.Description;
                pro.CreatedBy = model.CreatedBy;
                pro.CreatedDate = now;
                foreach (var file in files)
                {
                    if (file != null && file.ContentLength > 0)
                    {
                        byte[] imageByte = null;
                        BinaryReader rdr = new BinaryReader(file.InputStream);
                        imageByte = rdr.ReadBytes((int)file.ContentLength);
                        pro.ImageData = imageByte;

                        string s = file.FileName;
                        int idx = s.LastIndexOf('.');
                        string fileName = s.Substring(0, idx);
                        string extension = s.Substring(idx);

                        picName = max_id + "_" + pro.CreatedBy + extension;
                        string PathForOriginal = Path.Combine(Server.MapPath("~/Images/Soldier71/Original"), picName);
                        file.SaveAs(PathForOriginal);

                        var imgForGallery = Image.FromStream(file.InputStream, true, true);
                        using (var resizeImg = ScaleImage(imgForGallery, 150, 150))
                        {
                            string Paths = Path.Combine(Server.MapPath("~/Images/Soldier71/Resize"), picName);
                            resizeImg.Save(Paths);
                        }
                    }
                }
                pro.ImageName = picName;
                db.ProductItemLists.Add(pro);
                try
                {
                    db.SaveChanges();
                    OperationStatus = 1;
                }
                catch (Exception)
                {
                    OperationStatus = -1;
                }
                if (OperationStatus == 1)
                {
                    return Json("Success", JsonRequestBehavior.AllowGet);
                }
            }
            return Json("Error", JsonRequestBehavior.AllowGet);
        }
        #endregion

        // ************************************ Inventory Item Monthy Transaction Report *****************
        #region
        public ActionResult Invenlist()
        {
            return View();
        }
        public PartialViewResult InventoryProductListByType(int? productTypeId,int? page,string name)
        {
            ViewBag.page = page;
            ViewBag.productTypeId = productTypeId;
            ViewBag.name = name;
            ViewBag.productTypeId = productTypeId;
            return PartialView();
            //if (productTypeId == null || productTypeId <= 0)
            //{
            //    return PartialView(db.View_All_InventoryList.OrderBy(x => x.ProductName).ToList());
            //}
            //return PartialView(db.View_All_InventoryList.Where(x => x.ProductTypeId == productTypeId).OrderBy(x => x.ProductName).ToList());
        }
        public PartialViewResult InventoryProductListByTypePartial(int? productTypeId, int? page) {
            ViewBag.productTypeId = productTypeId;ViewBag.page = page;
            if (productTypeId == null || productTypeId <= 0)
            {
                var data = db.View_All_InventoryList.OrderBy(x => x.ProductName).ToList();
                Session["AllProList"] = data;                
                Session["ProListCount"] = data.Count();
                ViewBag.totalCount = data.Count();
                return PartialView(data.Take(ShowItemPerPage));
            }
            var data2 = db.View_All_InventoryList.Where(x => x.ProductTypeId == productTypeId).OrderBy(x => x.ProductName).ToList();
            Session["AllProList"] = data2;
            Session["ProListCount"] = data2.Count();
            ViewBag.totalCount = data2.Count();
            return PartialView(data2.Take(ShowItemPerPage));
        }
        public JsonResult getProListonScroll(int? page) {
            page = page ?? 0;
            int total = Convert.ToInt16(Session["ProListCount"]);
            int skip = ShowItemPerPage * Convert.ToInt16(page);
            var canPage = skip < total;
            var listBackFromSession = (List<View_All_InventoryList>)Session["AllProList"];
            var proList = listBackFromSession.Skip(skip).Take(ShowItemPerPage).ToList();
            return Json(proList, JsonRequestBehavior.AllowGet);
        }
        public ActionResult InvenProMonthReport(string q)
        {
            var base64EncodedBytes = System.Convert.FromBase64String(q);
            var base64String = System.Text.Encoding.UTF8.GetString(base64EncodedBytes);
            var splitbyand = base64String.Split('&');
            var AllId = splitbyand[0].Split('=')[1];
            var ids = AllId.Split(',');
            int[] productIds = new int[ids.Length];
            for (int i = 0; i < ids.Length; i++)
            {
                productIds[i] = Convert.ToInt32(ids[i]);
            }
            var list = db.InventoryLists.Where(m => productIds.Contains(m.InventoryId)).ToList();
            int year = Convert.ToInt32(Convert.ToUInt32(splitbyand[2].Split('=')[1]));
            int month = Convert.ToInt32(Convert.ToUInt32(splitbyand[1].Split('=')[1]));
            ViewBag.startDate = new DateTime(year, month, 1);
            return View(list);
        }
        [EncryptedActionParameter]
        public ActionResult InvenProTodayReport(string q)
        {
            var base64EncodedBytes = System.Convert.FromBase64String(q);
            var AllId = System.Text.Encoding.UTF8.GetString(base64EncodedBytes);
            var ids = AllId.Split(',');
            int[] productIds = new int[ids.Length];
            for (int i = 0; i < ids.Length; i++)
            {
                productIds[i] = Convert.ToInt32(ids[i]);
            }
            var list = db.InventoryLists.Where(m => productIds.Contains(m.InventoryId)).ToList();
            return View(list);
        }
        public ActionResult InvenProSpecificReport(string AllId, string startDate, string endDate)
        {
            var ids = AllId.Split(',');
            int[] productIds = new int[ids.Length];
            for (int i = 0; i < ids.Length; i++)
            {
                productIds[i] = Convert.ToInt32(ids[i]);
            }
            var list = db.InventoryLists.Where(m => productIds.Contains(m.InventoryId)).ToList();
            ViewBag.startDate = Convert.ToDateTime(startDate);
            ViewBag.endDate = Convert.ToDateTime(endDate);
            return View(list);
        }
        public PartialViewResult MonthSpecReportPopup(string view)
        {
            ViewBag.view = view;
            List<int> yearList = new List<int>();
            for (var i = 0; i < 10; i++)
            {
                yearList.Add(now.Year - i);
            }
            ViewBag.yearList = new SelectList(yearList);
            return PartialView();
        }
        public JsonResult GetAllCategory()
        {
            var allproductType = (from i in db.View_All_InventoryList
                                  select new ProductTypeViewModel
                                  {
                                      ProductTypeName = i.ProductType,
                                      ProductTypeId = i.ProductTypeId,
                                      Count = db.InventoryLists.Where(coun => coun.ProductTypeId == i.ProductTypeId).Count()
                                  }).GroupBy(g => g.ProductTypeId).Select(s => s.FirstOrDefault()).OrderBy(m => m.ProductTypeName).ToList();
            return Json(allproductType, JsonRequestBehavior.AllowGet);
        }
        #endregion

        // ************************************ Manually Entry Waste **************************************
        #region
        public ActionResult WasteList()
        {
            return View();
        }
        public PartialViewResult _InvenProListForWaste(int? id)
        {
            var list = db.View_All_InventoryList.ToList();
            if (id > 0)
            {
                list = list.Where(m => m.InventoryId == id).ToList();
            }
            return PartialView(list);
        }
        public PartialViewResult _LoadWasteByType(int type, int order)
        {
            ViewBag.type = type;
            ViewBag.order = order;
            return PartialView();
        }
        public PartialViewResult _InvenMcsWasteList(int type, int order, int? InventoryId, DateTime? fromDate, DateTime? toDate)
        {
            ViewBag.type = type;
            ViewBag.order = order;
            var list = (from c in db.Inventory_Pro_Waste_List
                        where c.Type == type
                        group c by new { c.InventoryId, c.InsertedDate } into g
                        select new Inventory_Pro_Waste_ListModelView
                        {
                            Id = g.FirstOrDefault().Id,
                            InventoryId = g.FirstOrDefault().InventoryId,
                            ProductName = db.InventoryLists.Where(u => u.InventoryId == g.FirstOrDefault().InventoryId).Select(un => un.ProductName).FirstOrDefault(),
                            LocationId = g.FirstOrDefault().LocationId,
                            Type = g.FirstOrDefault().Type,
                            Quantity = g.FirstOrDefault().Quantity,
                            UnitId = g.FirstOrDefault().UnitId,
                            Unitname = db.UnitLists.Where(u => u.UnitId == g.FirstOrDefault().UnitId).Select(un => un.UnitName).FirstOrDefault(),
                            InsertedDate = DbFunctions.TruncateTime(g.FirstOrDefault().InsertedDate).Value,
                            InsertedBy = g.FirstOrDefault().InsertedBy
                        }).ToList();
            list = (order == 1) ? list.OrderByDescending(x => x.InsertedDate).ToList() : list.OrderBy(x => x.InsertedDate).ToList();
            list = (InventoryId != null) ? list.Where(x => x.InventoryId == InventoryId).ToList() : list;
            if(fromDate != null && toDate != null)
            {
                DateTime startDate = fromDate.Value;
                DateTime endDate = toDate.Value;
                if (startDate == endDate)
                {
                    list = list.Where(x => x.InsertedDate == startDate).ToList();
                }
                else
                {
                    list = list.Where(x => x.InsertedDate >= startDate && x.InsertedDate <= endDate).ToList();
                }
                
            }
            return PartialView(list);
        }
        public PartialViewResult _middlePageOfEntryInsert(int id, string name, int unitId, int type)
        {
            ViewBag.id = id;
            ViewBag.type = type;
            ViewBag.name = name;
            ViewBag.unitId = unitId;
            return PartialView();
        }
        public PartialViewResult _InsertMCSNumber(int id, string name, int unitId)
        {
            ViewBag.id = id;
            ViewBag.name = name;
            ViewBag.UnitId = new SelectList(db.UnitLists.Where(m => m.HasParentId == false).OrderBy(m => m.UnitName), "UnitId", "UnitName", unitId);
            return PartialView();
        }
        public JsonResult SaveWaste(Inventory_Pro_Waste_List model, string[] allUnitQuan, string Name)
        {
            int status = 1;
            string msg = "";
            model.InsertedDate = now;
            if (model.InventoryId > 0 && allUnitQuan.Length > 0)
            {
                for (int i = 0; i < allUnitQuan.Length; i++)
                {
                    model.LocationId = Convert.ToInt32(allUnitQuan[i].Split('|')[0]);
                    model.Quantity = Convert.ToDouble(allUnitQuan[i].Split('|')[1]);
                    db.Inventory_Pro_Waste_List.Add(model);
                    try
                    {
                        db.SaveChanges();
                        status = 1;
                        msg = "New waste entry for product '" + Name + "' has ben successfully added on " + now.ToString("MMM, dd, yyy hh:mm tt");
                        SaveAuditLog("Manually Entry Waste", msg, "StoreProduct", "WasteList", "Inventory_Pro_Waste_List", Convert.ToInt32(model.Id), model.InsertedBy, now, status);
                    }
                    catch (Exception)
                    {
                        status = -1;
                        msg = "New waste entry for product '" + Name + "' add failed on " + now.ToString("MMM, dd, yyy hh:mm tt");
                        SaveAuditLog("Manually Entry Waste", msg, "StoreProduct", "WasteList", "Inventory_Pro_Waste_List", 0, model.InsertedBy, now, status);
                        return Json("Error", JsonRequestBehavior.AllowGet);
                    }
                }
            }

            return Json("Success", JsonRequestBehavior.AllowGet);
        }
        public PartialViewResult _middlePageOfEntryUpdate(int invenid, DateTime? date, int type, string productName)
        {
            ViewBag.invenid = invenid;
            ViewBag.date = date;
            ViewBag.type = type;
            ViewBag.name = productName;
            return PartialView();
        }
        public PartialViewResult _UpdateMCSNumber(int invenid, int? type, DateTime? date, string name)
        {
            var list = db.Inventory_Pro_Waste_List.Where(x => x.InventoryId == invenid && x.Type == type && x.InsertedDate.Day >= date.Value.Day && x.InsertedDate.Year >= date.Value.Year && x.InsertedDate.Month >= date.Value.Month).ToList();
            int unitId = list.Select(m => m.UnitId).FirstOrDefault();
            ViewBag.UnitId = new SelectList(db.UnitLists.Where(m => m.HasParentId == false).OrderBy(m => m.UnitName), "UnitId", "UnitName", unitId);
            ViewBag.list = list;
            ViewBag.name = name;
            return PartialView();
        }
        public JsonResult UpdateWaste(string name, string[] allUnitQuan, int UnitId)
        {
            int status = 1;
            string msg = "";
            if (allUnitQuan != null && allUnitQuan.Length > 0)
            {
                for (int i = 0; i < allUnitQuan.Length; i++)
                {
                    long id = Convert.ToInt64(allUnitQuan[i].Split('|')[0]);
                    var updateData = db.Inventory_Pro_Waste_List.Find(id);
                    updateData.Quantity = Convert.ToDouble(allUnitQuan[i].Split('|')[1]);
                    updateData.UnitId = UnitId;
                    db.Entry(updateData).State = EntityState.Modified;
                    try
                    {
                        db.SaveChanges();
                        status = 1;
                        msg = "Waste entry update for product '" + name + "' has ben successfully updated on " + now.ToString("MMM, dd, yyy hh:mm tt");
                        SaveAuditLog("Manually Entry Waste Update", msg, "StoreProduct", "WasteList", "Inventory_Pro_Waste_List", Convert.ToInt32(updateData.Id), updateData.InsertedBy, now, status);
                    }
                    catch (Exception)
                    {
                        status = -1;
                        msg = "Waste entry update for product '" + name + "' was unsuccessful on " + now.ToString("MMM, dd, yyy hh:mm tt");
                        SaveAuditLog("Manually Entry Waste Update", msg, "StoreProduct", "WasteList", "Inventory_Pro_Waste_List", 0, updateData.InsertedBy, now, status);
                    }
                }
            }
            return Json("Success", JsonRequestBehavior.AllowGet);
        }
        public JsonResult DeleteWaste(int inventoryId, int type, DateTime insertedDate, long actionby)
        {
            int status; string msg, productName; long id;
            var dataforDelete = db.Inventory_Pro_Waste_List
                                  .Where(x => x.InventoryId == inventoryId && x.Type == type &&
                                  DbFunctions.TruncateTime(x.InsertedDate) == insertedDate.Date).ToList();

            foreach (var data in dataforDelete)
            {
                db.Inventory_Pro_Waste_List.Remove(data);
                id = data.Id;
                productName = db.InventoryLists.Where(x => x.InventoryId == inventoryId).Select(x => x.ProductName).FirstOrDefault();
                try
                {
                    db.SaveChanges();
                    status = 1;
                    msg = "Waste entry for product '" + productName + "' has ben successfully deleted on " + now.ToString("MMM, dd, yyy hh:mm tt");
                    SaveAuditLog("Manually Entry Waste Delete", msg, "StoreProduct", "WasteList", "Inventory_Pro_Waste_List", Convert.ToInt32(id), actionby, now, status);
                }
                catch (Exception)
                {
                    status = -1;
                    msg = "Waste entry for product '" + productName + "' delete failed on " + now.ToString("MMM, dd, yyy hh:mm tt");
                    SaveAuditLog("Manually Entry Waste Delete", msg, "StoreProduct", "WasteList", "Inventory_Pro_Waste_List", Convert.ToInt32(id), actionby, now, status);
                    return Json("Error", JsonRequestBehavior.AllowGet);
                }

            }
            return Json("Success", JsonRequestBehavior.AllowGet);
        }
        public JsonResult ProductNameSearchForWaste(string prefix,int type)
        {
            if (prefix == "")
            {
                return Json("", JsonRequestBehavior.AllowGet);
            }
            var invenid = db.Inventory_Pro_Waste_List.Where(m => m.Type == type).Select(m => m.InventoryId).Distinct();
            var AllId = db.InventoryLists.Where(x =>invenid.Contains(x.InventoryId) && x.ProductName.ToLower().Contains(prefix.ToLower())).Select(x => new
            {
                ProductName = x.ProductName,
                InventoryId = x.InventoryId
            }).ToList();
            return Json(AllId, JsonRequestBehavior.AllowGet);
        }
        #endregion

        // ************************************ Manually Finished Product *********************************
        #region
        public ActionResult FinishedProList()
        {
            return View();
        }
        public ActionResult _FinishedProList()
        {
            var list = db.Factory_Finished_Pro_Title.OrderByDescending(m => m.CreatedDate).ToList();
            ViewBag.TotalPro = list.Count();
            list = list.Take(20).ToList();
            ViewBag.ProTitleList = list;
            return PartialView();
        }
        public ActionResult FinishedProTitleWise(int? id)
        {
            if (id > 0)
            {
                var title = db.Factory_Finished_Pro_Title.Find(id);
                if (title != null)
                {
                    return View(title);
                }
            }
            return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        }
        public ActionResult AddFinishedProduct()
        {
            return View();
        }
        public ActionResult EditFinishedProduct(int? id)
        {
            if (id > 0)
            {
                var title = db.Factory_Finished_Pro_Title.Find(id);
                if (title != null)
                {
                    return View(title);
                }
            }
            return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        }
        public JsonResult GetUnit()
        {
            return Json(db.UnitLists.ToList(), JsonRequestBehavior.AllowGet);
        }
        public JsonResult GetCountName(string content)
        {
            return Json(db.Finished_Product_List.ToList(), JsonRequestBehavior.AllowGet);
        }
        public JsonResult SaveFinishedPro(Factory_Finished_Pro_Title fact, string[] AllName, string[] AllMC, string[] AllLot, string[] AllLotDate, string[] AllBag, string[] AllKg, string[] AllUnit, string[] AllRemark)
        {
            if (AllName.Length > 0)
            {
                fact.CreatedDate = now;
                db.Factory_Finished_Pro_Title.Add(fact);
                db.SaveChanges();
                int status = -1;
                for (int i = 0; i < AllName.Length; i++)
                {
                    Finished_Product_List model = new Finished_Product_List();
                    model.TitleId = fact.Id;
                    model.Count_Name = AllName[i].Split('|')[0];
                    long count_id = Convert.ToInt64(AllName[i].Split('|')[1]);
                    if (count_id > 0)
                    {
                        model.ParentId = count_id;
                    }
                    if(AllMC[i] != "")
                    {
                        model.M_C_Qty =Convert.ToDouble(AllMC[i]);
                    }
                    model.Lot = AllLot[i];
                    model.Lot_StartedDate = Convert.ToDateTime(AllLotDate[i]);
                    model.Bag = AllBag[i];
                    model.KG = AllKg[i];
                    model.UnitId = Convert.ToInt32(AllUnit[i]);
                    model.Remark = AllRemark[i];
                    model.InsertedDate = now;
                    db.Finished_Product_List.Add(model);
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
                return Json("Success", JsonRequestBehavior.AllowGet);
            }
            return Json("Error", JsonRequestBehavior.AllowGet);
        }
        public JsonResult EditFinishedPro(Factory_Finished_Pro_Title fact,long[] deletedid, string[] AllName, string[] AllMC, string[] AllLot, string[] AllLotDate, string[] AllBag, string[] AllKg, string[] AllUnit, string[] AllRemark)
        {
            if (AllName.Length > 0)
            {
                fact.CreatedDate = now;
                Factory_Finished_Pro_Title title = db.Factory_Finished_Pro_Title.Find(fact.Id);
                title.Title = fact.Title;
                title.SubTitle = fact.SubTitle;
                title.UpdatedBy = fact.CreatedBy;
                title.UpdatedDate = now;
                db.Entry(title).State = EntityState.Modified;
                db.SaveChanges();
                if (deletedid != null)
                {
                    if (deletedid.Length > 0)
                    {
                        for (int i = 0; i < deletedid.Length; i++)
                        {
                            Finished_Product_List remove_pro = db.Finished_Product_List.Find(deletedid[i]);
                            if (remove_pro != null)
                            {
                                db.Finished_Product_List.Remove(remove_pro);
                                db.SaveChanges();
                            }
                        }
                    }
                }
                int status = -1;
                for (int i = 0; i < AllName.Length; i++)
                {
                    Finished_Product_List model = new Finished_Product_List();
                    model.TitleId = fact.Id;
                    int exsit_id = Convert.ToInt32(AllName[i].Split('|')[0]);
                    if (exsit_id > 0)
                    {
                        model = db.Finished_Product_List.Find(exsit_id);
                    }
                    model.Count_Name = AllName[i].Split('|')[2];
                    long count_id = Convert.ToInt64(AllName[i].Split('|')[1]);
                    if (count_id > 0)
                    {
                        model.ParentId = count_id;
                    }
                    if (AllMC[i] != "")
                    {
                        model.M_C_Qty = Convert.ToDouble(AllMC[i]);
                    }
                    model.Lot = AllLot[i];
                    model.Lot_StartedDate = Convert.ToDateTime(AllLotDate[i]);
                    model.Bag = AllBag[i];
                    model.KG = AllKg[i];
                    model.UnitId = Convert.ToInt32(AllUnit[i]);
                    model.Remark = AllRemark[i];
                    if (exsit_id > 0)
                    {
                        model.UpdatedDate = now;
                        model.UpdatedBy = fact.CreatedBy;
                        db.Entry(model).State = EntityState.Modified;
                    }
                    else
                    {
                        model.InsertedDate = now;
                        model.InsertedBy = fact.CreatedBy;
                        db.Finished_Product_List.Add(model);

                    }
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
                return Json("Success", JsonRequestBehavior.AllowGet);
            }
            return Json("Error", JsonRequestBehavior.AllowGet);
        }
        #endregion



        // ************************************ Store Item Catagory List *********************************
        public ActionResult AllStoreNameList()
        {
            return View(db.StoreLists.ToList());
        }
        [EncryptedActionParameter]
        public ActionResult AllStoreCatagoryList(int storeid)
        {
            var list = (from i in db.View_Inven_Item_Loc_Details
                        where i.StoreId == storeid
                        select new ProductTypeViewModel
                        {
                            ProductTypeName = i.ProductType,
                            ProductTypeId = i.ProductTypeId.Value
                        }).GroupBy(g => g.ProductTypeId).Select(s => s.FirstOrDefault()).OrderBy(m => m.ProductTypeName).ToList();
            ViewBag.list = list;
            ViewBag.storeid = storeid;
            return View();
        }
        [EncryptedActionParameter]
        public ActionResult AllGoodsList(int id, int storeid,string catname)
        {
            ViewBag.storename = db.StoreLists.Where(m => m.Id == storeid).Select(m => m.StoreName).FirstOrDefault();
            ViewBag.catname = catname;
            ViewBag.id = id;
            ViewBag.storeid = storeid;
            return View(db.View_Inven_Item_Loc_Details.Where(m => m.StoreId == storeid && m.ProductTypeId == id).OrderBy(m=> m.ProductName).ToList());
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