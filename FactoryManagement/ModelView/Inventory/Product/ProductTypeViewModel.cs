using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace FactoryManagement.ModelView.Inventory.Product
{
    public class ProductTypeViewModel
    {
        public int ProductTypeId { get; set; }

        [Required(ErrorMessage = "Please enter product type name")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "Type atleast three character")]
        [Remote("ProductTypeNameExist", "RemoteValidation", AdditionalFields = "ProductTypeId", ErrorMessage = "Product type already exists")]
        public string ProductTypeName { get; set; }
        public bool HasParent { get; set; }
        public Nullable<int> ParentId { get; set; }
        public System.DateTime CreatedDate { get; set; }
        public long CreatedBy { get; set; }
        public Nullable<System.DateTime> UpdatedDate { get; set; }
        public Nullable<long> UpdatedBy { get; set; }
        public string UserName { get; set; }
        public string UserPicName { get; set; }
        //public bool HasChild { get; set; }
        public string CreatedDateName { get; set; }
        public string Picture { get; set; }
        public string ParentName { get; set; }
        public string HasChild { get; set; }

        //*************** For TreeView ************
        public string text { get; set; }
        public string id { get; set; }
        public List<ChildClassForSubcategory> items { get; set; }

        public int Count { get; set; }
        public bool CanBeDeleted { get; set; }

        public string UpdatedDateFormate { get; set; }
        public string UpdatedUserName { get; set; }
        public string UpdatedUserPic { get; set; }
    }
}