
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using FactoryManagement.Models;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using FactoryManagement.Infrastructure.Identity;
using System.Security.Claims;
using System.Reflection;
using System.ComponentModel;
using System.Configuration;

namespace FactoryManagement.Controllers
{
    [Authorize]
    public class RoleSetController : Controller
    {
        #region Private Properties
        private FactoryManagementEntities db = new FactoryManagementEntities();
        #endregion
        static private int offset = Convert.ToInt32(ConfigurationManager.AppSettings["localTime"]);
        DateTime now = DateTime.UtcNow.AddMinutes(offset);
        const int ShowUserPerPage = 20;

        public ActionResult RolePermissionSet()
        {
            return View();
        }
        public PartialViewResult ShowAllRoleName()
        {
            var list = db.RoleNameLists.ToList();
            return PartialView(list);
        }
        public PartialViewResult AllPermissionList(int? RoleId,string roleName)
        {
            List<ControllerNameList> list = new List<ControllerNameList>();
            var controllers = Assembly.GetExecutingAssembly().GetExportedTypes()
                            .Where(c => typeof(ControllerBase).IsAssignableFrom(c)).Select(c => c).OrderBy(o => o.Name);

            foreach (Type controller in controllers)
            {
                string controllerName;
                var atts = controller.GetCustomAttributes(typeof(DisplayNameAttribute), false);
                if (atts.Length > 0)
                    controllerName = ((DisplayNameAttribute)atts[0]).DisplayName;
                else
                    controllerName = controller.Name;


                if (controller.Name != "AccountController" && controller.Name != "HomeController" && controller.Name != "RoleSetController")
                {
                    if (String.Join(",", controller.GetCustomAttributes().Select(a => a.GetType().Name.Replace("Attribute", ""))) != "")
                    {
                        list.Add(new ControllerNameList
                        {
                            UniqueId = controller.GUID.ToString(),
                            Name =  controller.Name,
                            ControllerName = controllerName,
                            Actions = controller.GetMethods()
                                     .Where(t => t.Name != "Dispose" && !t.IsSpecialName
                                         && t.DeclaringType.IsSubclassOf(typeof(ControllerBase))
                                         && t.IsPublic && !t.IsStatic && t.ReturnType.ToString() != "System.Void")
                                     .Select(t => new ActionName
                                     {
                                         Name = t.Name,
                                         //DisplayName = ((DisplayNameAttribute)(t.GetCustomAttributes(typeof(DisplayNameAttribute), false)[0])).DisplayName,
                                         ReturnType = t.ReturnType.ToString(),
                                         DeclaringType = t.DeclaringType.ToString(),
                                         Module = t.Module.ToString()
                                     }).ToList(),
                            Claims = GetActionClaims(controller),
                            ClaimGroups = new List<FunctionGroup> { 
                            new FunctionGroup() {
                                Name = "Function",
                                ClaimAction = controller.GetMethods()
                                  .Where(t => t.Name != "Dispose" && !t.IsSpecialName && t.DeclaringType.IsSubclassOf(typeof(ControllerBase)) && t.IsPublic && !t.IsStatic)
                                  .Select(t => new ActionName
                                  {
                                      Name = t.Name
                                  }).ToList()
                            }
                        }
                        });
                    }
                }

            }
            if (RoleId != null)
            {
                var allSavePermission = db.RolePermissionLists.Where(m => m.RoleId == RoleId).ToList();
                ViewBag.allSavePermission = allSavePermission;
            }

            ViewBag.list = list;
            ViewBag.RoleId = RoleId;
            ViewBag.roleName = roleName;
            return PartialView();
        }
        private List<String> GetActionClaims(Type controllerType)
        {
            var result = controllerType.GetMethods()
                .Where(m => m.IsDefined(typeof(ClaimsActionAttribute)))
                .SelectMany(m => m.GetCustomAttributes<ClaimsActionAttribute>())
                .Select(a => a.Claim)
                .Distinct()
                .Select(a => a.ToString())
                .ToList();
            return result;
        }
        public JsonResult SavePermission(string[] allData, long userId, int roleId, string rolename, int[] allPreChekId)
        {
            string opname = "Permission Settings With Role";
            string msg = "";
            int OperationStatus = -1;

            if (allData != null)
            {
                for (int i = 0; i < allData.Length; i++)
                {
                    string conname = allData[i].Split('|')[0];
                    string conIdWithname = allData[i].Split('|')[1];
                    string conId = conIdWithname.Split(';')[0];
                    string Pername = conIdWithname.Split(';')[1];
                    var lists = db.RolePermissionLists.Where(m => m.RoleId == roleId).ToList();
                    if (!lists.Where(m => m.RoleId == roleId && m.ControllerName == conname && m.Permissionname == Pername).Any())
                    {
                        msg = "";
                        OperationStatus = -1;

                        RolePermissionList model = new RolePermissionList();
                        model.RoleId = roleId;
                        model.ControllerName = conname;
                        model.ConUniqueId = conId;
                        model.Permissionname = Pername;
                        model.AssignedBy = userId;
                        model.AssignedDate = now;
                        db.RolePermissionLists.Add(model);
                        try
                        {
                            db.SaveChanges();
                            OperationStatus = 1;
                            msg = model.Permissionname + " permision for " + model.ControllerName + " module set with " + rolename + " role.";
                        }
                        catch (Exception ex)
                        {
                            string strErr = ex.ToString();
                            OperationStatus = -1;
                            msg = model.Permissionname + " permision for " + model.ControllerName + " module set with " + rolename + " role save unsuccessful.";
                        }
                        SaveAuditLog(opname, msg, "Access Management", "Permission Settings", "RolePermissionList", roleId, userId, now, OperationStatus);
                    }
                    if (allPreChekId != null)
                    {
                        var list = db.RolePermissionLists.Where(m => allPreChekId.Contains(m.Id)).ToList();
                        foreach (var l in list)
                        {
                            if (l != null)
                            {
                                msg = "";
                                msg = l.Permissionname +" deleted from "+rolename+" role";
                                db.RolePermissionLists.Remove(l);
                                db.SaveChanges();
                                SaveAuditLog("Delete Permission", msg, "Access Management", "Permission Settings", "RolePermissionList", roleId, userId, now, OperationStatus);
                            }
                        }
                    }
                }
            }
            return Json("Success",JsonRequestBehavior.AllowGet);
        }


        #region 
        public PartialViewResult _RoleCreate(int? id)
        {
            RoleNameList role = new RoleNameList();
            if (id != null)
            {
                role = db.RoleNameLists.Find(id);
            }
            return PartialView(role);
        }
        public JsonResult SaveRoleName(RoleNameList model)
        {
            int OperationStatus = -1;
            string opName = "Add New Role";
            string msg = "";
            bool isEdit = false;
            if (ModelState.IsValid)
            {
                Guid UniqueID = Guid.NewGuid();
                RoleNameList role = new RoleNameList();
                if (model.Id > 0)
                {
                    role = db.RoleNameLists.Find(model.Id);
                    opName = "Edit Role Information";
                    isEdit = true;
                }
                role.RoleId = UniqueID;
                role.RoleName = model.RoleName;
                role.CreatedBy = model.CreatedBy;
                role.CreatedDate = now;
                db.RoleNameLists.Add(role);
                try
                {
                    db.SaveChanges();
                    OperationStatus = 1;
                    if (isEdit)
                    {
                        msg = model.RoleName + " role information upated successfully.";
                    }
                    else
                    {
                        msg = "New role " + model.RoleName + " added successfully.";
                    }
                }
                catch(Exception ex){
                    string errmsg = ex.ToString();
                    msg = "";
                    OperationStatus = 1;
                    if (isEdit)
                    {
                        msg = model.RoleName + " information update unsuccessfully.";
                    }
                    else
                    {
                        msg = "New role " + model.RoleName + " addition unsuccessfully.";
                    }
                    
                }
                SaveAuditLog(opName, msg, "Access Management", "Permission Settings", "RoleNameLists", model.Id, model.CreatedBy, now, OperationStatus);
            }
            return Json("Error",JsonRequestBehavior.AllowGet);
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
       

        //public ActionResult Create()
        //{
        //    return View();
        //}
        //[HttpPost]
        //public ActionResult Create(CreateRoleViewModel viewModel)
        //{
        //    Guid UniqueID = Guid.NewGuid();

        //    var newRole = new AuthRole()
        //    {
        //        UniqueId = UniqueID.ToString(),
        //        Name = viewModel.Name,
        //        Discriminator = "ApplicationRole"
        //    };
        //    db.AuthRoles.Add(newRole);
        //    db.SaveChanges();
        //    return RedirectToAction("RoleList");
        //}
        //public ActionResult EditClaims(int id)
        //{
        //    Assembly asm = Assembly.GetAssembly(typeof(MvcApplication));
        //    var controlleractionlist = asm.GetTypes()
        //            .Where(type => typeof(System.Web.Mvc.Controller).IsAssignableFrom(type))
        //            .SelectMany(type => type.GetMethods(BindingFlags.Instance | BindingFlags.DeclaredOnly | BindingFlags.Public))
        //            .Where(m => !m.GetCustomAttributes(typeof(System.Runtime.CompilerServices.CompilerGeneratedAttribute), true).Any())
        //            .Select(x => new
        //            {
        //                Controller = x.DeclaringType.Name,
        //                Action = x.Name,
        //                ReturnType = x.ReturnType.Name,
        //                Attributes = String.Join(",", x.GetCustomAttributes().Select(a => a.GetType().Name.Replace("Attribute", "")))
        //            })
        //            .OrderBy(x => x.Controller).ThenBy(x => x.Action).ToList();

        //    var test = new ClaimedActionsProvider();
        //    var role = db.AuthRoles.Find(id);
        //    var claimGroups = test.GetClaimGroups();
        //    claimGroups.RemoveAll(m => m.Claims.Count == 0);

        //    //var assignedClaims = await roleManager.GetClaimsAsync(role.Name);
        //    var viewModel = new RoleClaimsViewModel()
        //    {
        //        RoleId = role.UniqueId,
        //        RoleName = role.Name,
        //    };

        //    foreach (var claimGroup in claimGroups)
        //    {
        //        var claimGroupModel = new RoleClaimsViewModel.ClaimGroup()
        //        {
        //            GroupId = claimGroup.GroupId,
        //            GroupName = claimGroup.GroupName,
        //            GroupClaimsCheckboxes = claimGroup.Claims
        //                .Select(c => new SelectListItem()
        //                {
        //                    Value = String.Format("{0}#{1}", claimGroup.GroupId, c),
        //                    Text = c,
        //                    Selected = false
        //                    //Selected = assignedClaims.Any(ac => ac.Type == claimGroup.GroupId.ToString() && ac.Value == c)
        //                }).ToList()
        //        };
        //        viewModel.ClaimGroups.Add(claimGroupModel);
        //    }
        //    return View(viewModel);
        //}

    }


    public class CreateRoleViewModel
    {
        [Required]
        public String Name { get; set; }
    }
    public class RoleClaimsViewModel
    {
        public RoleClaimsViewModel()
        {
            ClaimGroups = new List<ClaimGroup>();
            SelectedClaims = new List<String>();
        }
        public String RoleId { get; set; }
        public String RoleName { get; set; }
        public List<ClaimGroup> ClaimGroups { get; set; }
        public IEnumerable<String> SelectedClaims { get; set; }
        public class ClaimGroup
        {
            public ClaimGroup()
            {
                GroupClaimsCheckboxes = new List<SelectListItem>();
            }
            public String GroupName { get; set; }
            public int GroupId { get; set; }
            public List<SelectListItem> GroupClaimsCheckboxes { get; set; }
        }
    }
    public class ControllerNameList
    {
        public int Id { get; set; }
        public string UniqueId { get; set; }
        public string Name { get; set; }
        public string ControllerName { get; set; }
        public List<ActionName> Actions { get; set; }
        public List<String> Claims { get; set; }
        public List<FunctionGroup> ClaimGroups { get; set; }
    }
    public class FunctionGroup
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public List<ActionName> ClaimAction { get; set; }
    }
    public class ActionName
    {
        public int Id { get; set; }
        public string DisplayName { get; set; }
        public string Name { get; set; }
        public string ReturnType { get; set; }
        public string Module { get; set; }
        public string DeclaringType { get; set; }
        public bool IsSpecialName { get; set; }
    }
}