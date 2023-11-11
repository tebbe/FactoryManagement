using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace FactoryManagement.ModelView.Acquisition
{
    public class AcquisitionEditViewModel
    {
        public string Name { get; set; }
        public int AcquisitionOrderId { get; set; }
        public int AcquisitionId { get; set; }
        public Nullable<int> ProductId { get; set; }

        [StringLength(100, MinimumLength = 3, ErrorMessage = "Invalid")]
        public string ProductNameEdit { get; set; }

        public int AcquisitionType { get; set; }
        public Nullable<int> BusinessOrderId { get; set; }
        public Nullable<int> PartsId { get; set; }
        public Nullable<int> MachineId { get; set; }

        [Required(ErrorMessage = "Please enter Product type")]
        public Nullable<int> ProductTypeIdEdit { get; set; }
        public string ProductTypeName { get; set; }
        [Required(ErrorMessage = "Please select a Country")]
        public Nullable<int> Country { get; set; }
        public string CountryName { get; set; }
        public string Brand { get; set; }
        public Nullable<int> BrandIdEdit { get; set; }

        [StringLength(50, MinimumLength = 3, ErrorMessage = "Invalid")]
        public string ModelEdit { get; set; }

        [Required(ErrorMessage = "Quantity is required")]
        [RegularExpression(@"^[1-9]\d*$", ErrorMessage = "Invalid number")]
        public int QuantityEdit { get; set; }
        public string UnitName { get; set; }
        [Required(ErrorMessage = "This field is required")]
        public int UnitIdEdit { get; set; }
        [Required(ErrorMessage = "Please select one")]
        public bool IsAsapEdit { get; set; }
        public Nullable<System.DateTime> RequiredDateEdit { get; set; }
        public string RequiredTimeEdit { get; set; }
        [StringLength(500, MinimumLength = 3, ErrorMessage = "Invalid")]
        public string CommentEdit { get; set; }
        public long CreatedBy { get; set; }
        public string UserName { get; set; }
        public int SiteId { get; set; }
        public System.DateTime CreatedDate { get; set; }
        public int ApproveStatus { get; set; }
        public Nullable<long> UpdatedBy { get; set; }
        public Nullable<System.DateTime> UpdatedDate { get; set; }
        public string CreatedDateName { get; set; }
        public string RequiredDateName { get; set; }
        public string ProductImageNameEdit { get; set; }
        public string PictureName { get; set; }
        public int InventoryId { get; set; }
        public int Available_Quantity { get; set; }
        public bool IsSubQuantity { get; set; }
        public Nullable<int> ASPTimeEdit { get; set; } 
    }
}