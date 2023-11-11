using FactoryManagement.Models;
using FactoryManagement.ModelView.Auth;
using FactoryManagement.ModelView.HR;
using FactoryManagement.ModelView.Inventory;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using System.Web.Http.Description;


namespace FactoryManagement.Controllers
{
    [RoutePrefix("Api/UserLoginApi")]
    public class UserLoginApiController : ApiController
    {
        //alternative of session  
        private static List<SparePartsList> sparepartsList = new List<SparePartsList>();
        private static List<Machine> machineList = new List<Machine>();
        private static List<View_Machine_Lists> ViewMachineList = new List<View_Machine_Lists>();
        public static List<View_All_InventoryList> inventoryList = new List<View_All_InventoryList>();
        #region Private Properties
        private FactoryManagementEntities db = new FactoryManagementEntities();
        static private int offset = Convert.ToInt32(ConfigurationManager.AppSettings["localTime"]);
        DateTime now = DateTime.UtcNow.AddMinutes(offset);
        #endregion

        #region Login methods
        [HttpPost]
        [Route("user/login")]
        [ResponseType(typeof(UserInformationModelView))]
        public IHttpActionResult UserLogin(UserLoginModelView model)
        {
            //string json = @"" + formData;
            //JObject results = JObject.Parse(json);
            //UserLoginModelView model = JsonConvert.DeserializeObject<UserLoginModelView>(json);
            if (db.UserLogins.Where(m => m.UserName == model.UserName && m.Password == model.Password).Any())
            {
                var userLoginInfo = this.db.UserLogins.Where(m => m.UserName == model.UserName && m.Password == model.Password).FirstOrDefault();
                if (userLoginInfo.UserId == 0)
                {
                    UserInformationModelView adminInfo = new UserInformationModelView();
                    adminInfo.UserId = 0;
                    adminInfo.FirstName = "Super";
                    adminInfo.UserName = "SuperAdmin";
                    adminInfo.Picture = "admin.jpg";
                    return Ok(adminInfo);

                }
                else
                {
                    var userInfo = db.View_UserLists.Where(m => m.UserId == userLoginInfo.UserId).FirstOrDefault();

                    UserInformationModelView information = new UserInformationModelView();
                    information.Picture = userInfo.Picture;
                    information.FirstName = userInfo.FirstName;
                    information.UserName = userInfo.UserName;
                    information.UserId = userInfo.UserId;
                    information.Designation = userInfo.DesignationName;

                    return Ok(information);
                }

            }
            else
            {
                if (!db.UserLogins.Where(m => m.UserName == model.UserName).Any())
                {
                    return BadRequest("Please enter valid username");
                }
                else if (!db.UserLogins.Where(m => m.Password == model.Password).Any())
                {
                    return BadRequest("Please enter valid password");
                }
                else
                {
                    return BadRequest("Unable to login");
                }
            }
        }
        #endregion

     



        #region Get Data From Android Ap
        [Route("SaveAttendance")]
        [HttpPost]
        public string Attendance(JArray allListData)
        {
            if (allListData != null)
            {
                //int oparetionStatus = 1;
                List<Attendance> attendanceList = ((JArray)allListData).Select(x => new Attendance
                {
                    Card_UID = (string)x["ser_no"],
                    //UserId = (string)x["UserId"],
                    Status = (string)x["status"],
                    InsertedDate = (DateTime)x["read_time"]
                }).ToList();
                if (attendanceList.Count > 0)
                {
                    for (int i = 0; i < attendanceList.Count; i++)
                    {
                        Attendance attendance = new Attendance();
                        attendance.Card_UID = attendanceList[i].Card_UID;
                        attendance.Status = attendanceList[i].Status;
                        attendance.InsertedDate = attendanceList[i].InsertedDate;
                        db.Attendances.Add(attendance);
                        try
                        {
                            db.SaveChanges();
                            //oparetionStatus = 1;
                        }
                        catch (Exception ex)
                        {
                            string errorMsg = ex.Message.ToString();
                            //oparetionStatus = -1;
                        }
                    }
                    return "Successfully Saved Data Into Server.";
                }
                return "No Data Received!";
            }
            return "Failed Store!";
        }
        #endregion

        [HttpPost]
        [Route("signup")]
        public IHttpActionResult RegisterUser(UserLoginModelView userData)
        {
            if(userData != null){
              
            }

            return Ok();
        }
        [HttpPost]
        [Route("signup2")]
        public IHttpActionResult RegisterUser(JObject userData)
        {
            if (userData != null)
            {

            }

            return Ok();
        }

        #region All Spare Pirts List by kaikubad
        /*Controllers Added By Kaikubad
         * 
         * 
         * 
         * 
         */

        [HttpGet]
        [Route("getallspareparts")]
        public IHttpActionResult AllSpareParts()
        {           
            sparepartsList = db.SparePartsLists.ToList();
            var allparts = sparepartsList.Select(x => new
            {
                CreatedBy=x.CreatedBy,
                CreatedByName = db.View_UserLists.Where(c=>c.UserId==x.CreatedBy).Select(c=>c.UserName).FirstOrDefault(),
                createdDate =x.CreatedDate.ToString(),
                Name = x.Name,
                Type = x.Type,
                PartsId = x.PartsId,
                Picture=db.View_UserLists.Where(m=>m.UserId==x.CreatedBy).Select(m=>m.Picture).FirstOrDefault(),
                Count=db.SparePartsLists.Count()
            }).ToList();
           
            var datas = allparts.Take(10);
                return Ok(datas);
        }
        [HttpGet]
        [Route("getpartsonscroll")]
        public IHttpActionResult GetPartsOnScroll(int size) {
            if (size == 0)
            {
                sparepartsList = db.SparePartsLists.OrderBy(x=>x.Name).ToList();
            }
            var data2 = sparepartsList.Select(x => new
            {
                CreatedBy = x.CreatedBy,
                CreatedByName = db.View_UserLists.Where(c => c.UserId == x.CreatedBy).Select(c => c.UserName).FirstOrDefault(),
                CreatedDate = x.CreatedDate.ToString("MMM dd,yyyy"),
                Name = x.Name,
                Type = x.Type,
                PartsId = x.PartsId,
                Picture = db.View_UserLists.Where(m => m.UserId == x.CreatedBy).Select(m => m.Picture).FirstOrDefault(),
                Description=x.Description,
                Count = db.SparePartsLists.Count()
            }).ToList();
            if (size == 0)
                return Ok(data2.OrderBy(x=>x.Name).Take(20));
            else
            {
                var data = data2.Skip(size).OrderBy(x => x.Name).Take(20);
                return Ok(data);
            }
        }
        [HttpPost]
        [Route("deletePartsById")]
        public IHttpActionResult DeletePartsById(int id) {
            try
            {
                
                if(id==0)
                {
                    return BadRequest("Please Provide Id for delete");
                }
                using (var dbase = new FactoryManagementEntities()) {
                    if (dbase.MachinePartsLists.Where(x => x.PartsId == id).Any())
                    {
                        return BadRequest("Can not delete This Parts.This Parts is running on a Machine");
                    }
                    var data = dbase.SparePartsLists.Where(x => x.PartsId == id).FirstOrDefault();
                    dbase.SparePartsLists.Remove(data);
                    dbase.SaveChanges();
                }
                return Ok();
            }
            catch(Exception ex) { 
            return BadRequest(ex.Message);
            }
        }
        [HttpPost]
        [Route("addNewParts")]
        public IHttpActionResult AddNewParts(string parts) {
            string json = @"" + parts;
            JObject results = JObject.Parse(json);
            SparePartsList partsList = JsonConvert.DeserializeObject<SparePartsList>(json);
            try
            {
                partsList.CreatedDate = DateTime.UtcNow;
                using (var dbase = new FactoryManagementEntities()) {
                    dbase.SparePartsLists.Add(partsList);
                    dbase.SaveChanges();
                }
                return Ok();
            }
            catch (Exception ex) {
                return BadRequest(ex.Message);
            }
        }
        [HttpPost]
        [Route("editUpdateParts")]
        public IHttpActionResult EditUpdateParts(string parts) {
            string json = @"" + parts;
            JObject results = JObject.Parse(json);
            SparePartsList partsList = JsonConvert.DeserializeObject<SparePartsList>(json);
            try
            {
                using (var dbase = new FactoryManagementEntities())
                {
                    var data = dbase.SparePartsLists.Where(x => x.PartsId == partsList.PartsId).FirstOrDefault();
                    data.Name = partsList.Name;
                    data.Description = partsList.Description;
                    data.UpdatedDate = DateTime.UtcNow;
                    data.UpatedBy = partsList.UpatedBy;
                    data.Type = partsList.Type;
                    dbase.SaveChanges();
                }
                return Ok();
            }
            catch (Exception ex) {
                return BadRequest(ex.Message);
            }
        }      
        #endregion 
        # region All Machine list by Kaikubad
        [HttpGet]
        [Route("GetAllMAchineList")]
        public IHttpActionResult GetAllMAchineList(int size) {
            if (size == 0)
            {
                machineList = db.Machines.OrderBy(x=>x.Name).ToList();
            }
            var machineModelList = machineList.Select(x => new
            {
                MachineName=x.Name,
                MachineAcronym=x.Acroynm,
                CreatedBy=x.CreatedBy,
                CreatedByName=db.View_UserLists.Where(c=>c.UserId==x.CreatedBy).Select(c=>c.UserName).FirstOrDefault(),
                CreatedDate=x.CreatedDate.ToString("MMM dd ,yyyy"),
                UpdatedBy=x.UpdatedBy,
                UpdatedByName=db.View_UserLists.Where(a=>a.UserId==x.UpdatedBy).FirstOrDefault(),
                UpdateDate = x.UpdatedDate.ToString(),
                Count=db.Machines.Count()
            }).ToList();
            if (size == 0)
            {
                var data = machineModelList.Take(20).ToList();
                return Ok(data);
            }
            else {
                var data = machineModelList.Skip(size).Take(20);
                return Ok(data);
            }
        }
        #endregion
        #region All Equipment By kaikubad
        [HttpGet]
        [Route("getequipments")]
        public IHttpActionResult AllEquipmentList(int size,int type) {
            if (size == 0) {
                ViewMachineList = db.View_Machine_Lists.OrderBy(x => x.Acroynm).ToList();
            }
            if (type==0)
            {
                var machineData = ViewMachineList.Select(x => new
                {
                    MachineType=x.MachineType,
                    MachineName = x.Acroynm,
                    MachineAcronym = x.MachineAcronym,
                    MachineTypeId = x.MachineTypeId,
                    MachineTypeName=x.MachineTypeName,
                    CreatedBy = x.CreatedBy,
                    Status=x.Status,
                    Model=x.ModelName,
                    CountryOrigin=x.CountryName,
                    CreatedByName = db.View_UserLists.Where(c => c.UserId == x.CreatedBy).Select(c => c.UserName).FirstOrDefault(),
                    CreatedDate = x.CreatedDate.ToString("MMM dd,yyyy"),
                    Count = db.View_Machine_Lists.Count()
                }).ToList();
                if (size == 0)
                {
                    var data = machineData.Take(20).ToList();
                    return Ok(data);
                }
                else
                {
                    var data = machineData.Skip(size).Take(20).ToList();
                    return Ok(data);
                }
            }
            else if (type!=0)
            {
                var machineData = ViewMachineList.Where(x => x.MachineTypeId == type).Select(x => new
                {
                    MachineName = x.Acroynm,
                    MachineAcronym = x.MachineAcronym,
                    MachineTypeId = x.MachineTypeId,
                    MachineTypeName = x.MachineTypeName,
                    CreatedBy = x.CreatedBy,
                    Status=x.Status,
                    CreatedByName = db.View_UserLists.Where(c => c.UserId == x.CreatedBy).Select(c => c.UserName).FirstOrDefault(),
                    CreatedDate = x.CreatedDate.ToString("MMM dd,yyyy"),
                    Count = db.View_Machine_Lists.Where(z => z.MachineTypeId == type).Count()
                }).ToList();
                if (size == 0)
                {
                    var data = machineData.Take(20).ToList();
                    return Ok(data);
                }
                else
                {
                    var data = machineData.Skip(size).Take(20).ToList();
                    return Ok(data);
                }
            }
            return Ok();
        }
        [HttpGet]
        [Route("getallmachinename")]
        public IHttpActionResult GetMachinesNames() {
            var data = db.Machines.Select(x => new { Type = x.Id, Name = x.Name }).ToList();
            return Ok(data);
        }
      
        #endregion
        #region All Warehouse List by Kaikubad
        [HttpGet]
        [Route("warehouse")]
        public IHttpActionResult WareHouseList(int size) {
            var allInventoryId = db.Inventory_Item_Location.Where(m => m.WarehouseId > 0).Select(m => m.InventoryId).ToList();
            if (size == 0) {
                
                inventoryList = db.View_All_InventoryList.ToList();
            }
            var allProduct = (from i in inventoryList
                              where allInventoryId.Contains(i.InventoryId)
                              select new
                              {
                                  Count = inventoryList.Where(x => allInventoryId.Contains(x.InventoryId)).Count(),
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
                                  ProductImageName = db.Inventory_Image_Lists.Where(m => m.InventoryId == i.InventoryId).Select(m => m.ImageName).FirstOrDefault(),
                                  WareHouse = db.Inventory_Item_Location.Where(v => v.InventoryId == i.InventoryId).Select(v => new { 
                                  WareHouseId=v.WarehouseId,
                                  Quantity=v.Quantity,
                                  WareHouseAcronym=db.WarehouseLists.Where(aj=>aj.Id==v.WarehouseId).Select(aj=>aj.WarehouseAcroynm).FirstOrDefault()
                                  }).ToList()
                              }).OrderBy(m => m.ProductName).ToList();
            if (size == 0)
            {
                var data = allProduct.Take(10).ToList();
                return Ok(data);
            }
            else
            {
                var data = allProduct.Skip(size).Take(10).ToList();
                return Ok(data);
            }
        }
        [HttpGet]
        [Route("SearchProductById")]
        public IHttpActionResult SearchProductById(int id) {
            var allProduct = inventoryList.Where(i => i.InventoryId == id).
                              Select(i => new
                              {
                                  InventoryId = i.InventoryId,
                                  ProductName = i.ProductName,
                                  ProductTypeId = i.ProductTypeId,
                                  ProductType = i.ProductType,
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
                                  ProductImageName = db.Inventory_Image_Lists.Where(m => m.InventoryId == i.InventoryId).Select(m => m.ImageName).FirstOrDefault(),
                                  WareHouse = db.Inventory_Item_Location.Where(v => v.InventoryId == i.InventoryId).Select(v => new
                                  {
                                      WareHouseId = v.WarehouseId,
                                      Quantity = v.Quantity,
                                      WareHouseAcronym = db.WarehouseLists.Where(aj => aj.Id == v.WarehouseId).Select(aj => aj.WarehouseAcroynm).FirstOrDefault()
                                  }).ToList(),
                                  Imagelist = db.Inventory_Image_Lists.Where(ka => ka.InventoryId == i.InventoryId).Select(ka => new { ka.ImageId,ka.ImageName}).ToList()
                              }).OrderBy(m => m.ProductName).FirstOrDefault();
            return Ok(allProduct);
        }
        #endregion
        #region All Store List by Kaikubad
        [HttpGet]
        [Route("allstorelist")]
        public IHttpActionResult AllStoreList(int size) {
            var allInventoryId = db.Inventory_Item_Location.Where(m => m.StoreId > 0).Select(m => m.InventoryId).ToList();
            if (size == 0) {
                inventoryList.Clear();
                inventoryList = db.View_All_InventoryList.ToList();
            }
            var allProduct = (from i in inventoryList
                              where allInventoryId.Contains(i.InventoryId)
                              select new
                              {
                                  Count = inventoryList.Where(x => allInventoryId.Contains(x.InventoryId)).Count(),
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
                                  ProductImageName = db.Inventory_Image_Lists.Where(m => m.InventoryId == i.InventoryId).Select(m => m.ImageName).FirstOrDefault(),
                                  Store = db.Inventory_Item_Location.Where(aj => aj.InventoryId == i.InventoryId).Select(aj => new { 
                                  StoreId=aj.StoreId,
                                  Storename=db.StoreLists.Where(jk=>jk.Id==aj.StoreId).Select(jk=>jk.StoreName).FirstOrDefault(),
                                  StoreAcronym = db.StoreLists.Where(ak => ak.Id == aj.StoreId).Select(ak => ak.StoreName).FirstOrDefault(),
                                  Quantity=aj.Quantity
                                  }).ToList()
                              }).OrderBy(m => m.ProductName).ToList();
            if (size == 0) {
                var data = allProduct.Take(10).ToList();
                return Ok(data);
            }
            else { var data = allProduct.Skip(size).Take(10).ToList();
            return Ok(data);
            }
        }
        #endregion
        [HttpGet]
        [Route("getAllStoreName")]
        public IHttpActionResult getAllStoreName() { 
            var data=db.View_All_StoreLists.Select(x=>new {
                           StoreName= x.StoreName,
                           StoreAcroynm=x.StoreAcroynm,
                           Id= x.Id,
                           CreatedBy=x.CreatedBy,
                           CreatedByName=db.View_UserLists.Where(j=>j.UserId==x.CreatedBy).Select(j=>j.UserName).FirstOrDefault(),
                           CreatedDate=x.CreatedDate
        }).ToList();
            return Ok(data);
        }
        [HttpGet]
        [Route("getStoreUnit")]
        public IHttpActionResult getStoreUnit() {
            var data = db.SiteLists.Select(x => new { x.SiteName, x.Id }).ToList();
            return Ok(data);
        }
        [HttpPost]
        [Route("AddNewStore")]
        public IHttpActionResult AddNewStore(string newStore) {
            string json = @"" + newStore;
            JObject results = JObject.Parse(json);
            StoreList model = JsonConvert.DeserializeObject<StoreList>(json);

            return Ok();
        }
        [HttpGet]
        [Route("storeproductsbyid")]
        public IHttpActionResult getStorebyId(int id,int size) {
            var allInventoryId = db.Inventory_Item_Location.Where(m => m.StoreId ==id).Select(m => m.InventoryId).ToList();
            if (size == 0)
            {
                inventoryList.Clear();
                inventoryList = db.View_All_InventoryList.ToList();
            }
            var allProduct = (from i in inventoryList
                              where allInventoryId.Contains(i.InventoryId)
                              select new
                              {
                                  Count = inventoryList.Where(x => allInventoryId.Contains(x.InventoryId)).Count(),
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
                                  ProductImageName = db.Inventory_Image_Lists.Where(m => m.InventoryId == i.InventoryId).Select(m => m.ImageName).FirstOrDefault(),
                                  Store = db.Inventory_Item_Location.Where(aj => aj.InventoryId == i.InventoryId).Select(aj => new
                                  {
                                      StoreId = aj.StoreId,
                                      Storename = db.StoreLists.Where(jk => jk.Id == aj.StoreId).Select(jk => jk.StoreName).FirstOrDefault(),
                                      StoreAcronym = db.StoreLists.Where(ak => ak.Id == aj.StoreId).Select(ak => ak.StoreName).FirstOrDefault(),
                                      Quantity = aj.Quantity
                                  }).ToList()
                              }).OrderBy(m => m.ProductName).ToList();
            if (size == 0)
            {
                var data = allProduct.Take(10).ToList();
                return Ok(data);
            }
            else
            {
                var data = allProduct.Skip(size).Take(10).ToList();
                return Ok(data);
            }
        }
    }
}
