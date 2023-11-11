using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;


namespace FactoryManagement.ModelView.Inventory.Store
{
    public class StoreListsModelView
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "Please enter store name")]
        [Remote("StoreNameExist", "RemoteValidation", AdditionalFields = "Id", HttpMethod = "Post", ErrorMessage = "Store name already exsits")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "Invalid")]
        public string StoreName { get; set; }
        public string StoreAcroynm { get; set; }
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