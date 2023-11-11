using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace FactoryManagement.ModelView.Acquisition
{
    public class AcquisitionViewModel
    {
        [Required(ErrorMessage = "Please enter Order name")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "Invalid")]
        public string Name { get; set; }
        public int AcquisitionOrderId { get; set; }
        public int AcquisitionId { get; set; }
        public Nullable<int> ProductId { get; set; }

        public int AcquisitionType { get; set; }
        public Nullable<int> BusinessOrderId { get; set; }
        public Nullable<int> PartsId { get; set; }
        public Nullable<int> MachineId { get; set; }

        [StringLength(100, MinimumLength = 3, ErrorMessage = "Invalid")]
        public string ProductName { get; set; }

        [Required(ErrorMessage = "Please enter Product type")]
        public Nullable<int> ProductTypeId { get; set; }

        public string ProductTypeName { get; set; }
        [Required(ErrorMessage = "Please select a Country")]
        public Nullable<int> Country { get; set; }
        public string CountryName { get; set; }
        public string Brand { get; set; }
        public Nullable<int> BrandId { get; set; }
        [StringLength(50, MinimumLength = 3, ErrorMessage = "Invalid")]
        public string Model { get; set; }
        [Required(ErrorMessage = "Quantity is required")]
        [RegularExpression(@"^[1-9]\d*$", ErrorMessage = "Only Be A Positive Number")]
        public int Quantity { get; set; }
        public string UnitName { get; set; }
        [Required(ErrorMessage = "This field is required")]
        public int UnitId { get; set; }
        [Required(ErrorMessage = "Please select one")]
        public bool IsAsap { get; set; }

        public Nullable<System.DateTime> RequiredDate { get; set; }
        public string RequiredTime { get; set; }
        [StringLength(200, MinimumLength = 3, ErrorMessage = "Invalid")]
        public string Comment { get; set; }
        public long CreatedBy { get; set; }
        public string UserName { get; set; }
        public int SiteId { get; set; }
        public System.DateTime CreatedDate { get; set; }
        public string Status { get; set; }
        public int AcquisitionStatus { get; set; }
        public Nullable<long> UpdatedBy { get; set; }
        public Nullable<System.DateTime> UpdatedDate { get; set; }
        public string CreatedDateName { get; set; }
        public string RequiredDateName { get; set; }
        public string ProductImageName { get; set; }
        public string ProductImageNameEdit { get; set; }
        public string PictureName { get; set; }
        public int InventoryId { get; set; }



        public int TotalQuantity { get; set; }
        public string BusinessOrderName { get; set; }
        public Nullable<int> AvailableQuantity { get; set; }

        public string PartsName { get; set; }

        public int OrderStatus { get; set; }
        public string Description { get; set; }
        public Nullable<int> AssignedQuantity { get; set; }
        public Nullable<System.DateTime> AssignedDate { get; set; }
        public Nullable<long> AssignedBy { get; set; }
        public bool IsSubQuantity { get; set; }
        public Nullable<int> ASPTime { get; set; } 
    }
}