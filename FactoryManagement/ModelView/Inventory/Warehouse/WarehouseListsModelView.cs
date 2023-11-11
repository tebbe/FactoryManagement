using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace FactoryManagement.ModelView.Inventory.Warehouse
{
    public class WarehouseListsModelView
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "Please enter warehouse name")]
        [Remote("WarehouseNameExist", "RemoteValidation", AdditionalFields = "Id", HttpMethod = "Post", ErrorMessage = "Warehouse name already exsits")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "Invalid")]
        public string WarehouseName { get; set; }
        public string WarehouseAcroynm { get; set; }
        public long CreatedBy { get; set; }
        public System.DateTime CreatedDate { get; set; }
        public Nullable<long> UpdatedBy { get; set; }
        public Nullable<System.DateTime> UpdatedDate { get; set; }

        public string AllSiteLists { get; set; }
        public string CreatedDateName { get; set; }
        public string Username { get; set; }
        public string Picture { get; set; }

        public string AssignUserName { get; set; }
        public string AllAssignUserId { get; set; }
    }
}