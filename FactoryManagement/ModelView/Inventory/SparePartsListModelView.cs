using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace FactoryManagement.ModelView.Inventory
{
    public class SparePartsListModelView
    {
        public int PartsId { get; set; }
        [Required(ErrorMessage = "Please enter parts name")]
        [Remote("SparePartsNameExist", "RemoteValidation", AdditionalFields = "PartsId", HttpMethod = "Post", ErrorMessage = "Parts name already exsits")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "Invalid")]
        public string PartsName { get; set; }

        [StringLength(200, MinimumLength = 3, ErrorMessage = "Invalid")]
        public string Description { get; set; }
        [Required(ErrorMessage="Please select type")]
        public int Type { get; set; }
        public System.DateTime CreatedDate { get; set; }
        public long CreatedBy { get; set; }
        public System.DateTime UpdatedDate { get; set; }
        public long UpatedBy { get; set; }
        public string ExpireDate { get; set; }

        public string UserName { get; set; }
        public string Picture { get; set; }
        public string TypeName { get; set; }
        public string CreatedDateName { get; set; }

        public System.DateTime ReplaceDate { get; set; }
        public int LeftDate { get; set; }
        public int Id { get; set; }

        public Nullable<decimal> Quantity { get; set; }
        public Nullable<decimal> NotInstalledQuan { get; set; }
        public string Unitname { get; set; }
        public int InvenLocId { get; set; }
        public int MachineId { get; set; }
        public string MachineName { get; set; }
        public string MachineAcronym { get; set; }

        public Nullable<double> NotInsQuan { get; set; }
        public bool CanbeBreakable { get; set; }
        public Nullable<int> SubQuantity { get; set; }
        public Nullable<int> SubUnitId { get; set; }
        public string SubUnitName { get; set; }
    }
}