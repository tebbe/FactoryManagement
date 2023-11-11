using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace FactoryManagement.ModelView.Inventory.Mahinaries
{
    public class MachineTypeNameModelView
    {
        public int MachineId { get; set; }
        [Required(ErrorMessage="Please enter machine type name")]
        [Display(Name="Designation")]
        [Remote("MachineTypenameExsits", "RemoteValidation", AdditionalFields = "MachineId", HttpMethod = "Post", ErrorMessage = "Machine type name already exists.")]
        public string MachineTypeName1 { get; set; }
        public long CreatedBy { get; set; }
        public System.DateTime CreatedDate { get; set; }
        public Nullable<long> UpdatedBy { get; set; }
        public Nullable<System.DateTime> UpdatedDate { get; set; }

        public string CreatedUserName { get; set; }
         public string CreatedDateName { get; set; }
         public string CreatedUserPic { get; set; }
         public string UpdatedDateFormate { get; set; }
         public string UpdatedUserName { get; set; }
         public string UpdatedUserPic { get; set; }
        
    }
}