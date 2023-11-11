using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace FactoryManagement.ModelView.HR
{
    public class HolidayPackageListModelView
    {
        public int HolidayPackId { get; set; }
        [Required(ErrorMessage="Please enter name")]
        [Display(Name="Name")]

        [StringLength(50, MinimumLength = 3, ErrorMessage = "Minimum lenth 3")]
        [Remote("HolidayPackNameExists", "RemoteValidation", AdditionalFields = "HolidayPackId", HttpMethod = "Post", ErrorMessage = "Package name already exists")]
        public string HolidayPackName { get; set; }

        [Required(ErrorMessage = "Please enter number of paid leave")]
        [RegularExpression("([1-9][0-9]*)", ErrorMessage = "Paid leave must be a an integer number")]
        [Range(0, int.MaxValue, ErrorMessage = "Paid leave must be a an integer number")]
        [Display(Name = "No.of paid Leave")]
        public int NoOfPaidLeave { get; set; }
        public System.DateTime CreatedDate { get; set; }
        public long CreatedBy { get; set; }
        public Nullable<System.DateTime> UpdatedDate { get; set; }
        public Nullable<long> UpdatedBy { get; set; }
        public string StartTime { get; set; }
        public string EndTime { get; set; }
    }
}