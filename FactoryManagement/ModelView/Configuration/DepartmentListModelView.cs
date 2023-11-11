using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace FactoryManagement.ModelView.Configuration
{
    public class DepartmentListModelView
    {
        public int DeptId { get; set; }
        [Display(Name = "Dept. Name")]
        [Required(ErrorMessage = "Please enter department name")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "Minimum length is 2")]
        [Remote("DepartmentNameExist", "RemoteValidation", AdditionalFields = "DeptId", HttpMethod = "Post", ErrorMessage = "Department name already exsits")]
        public string DeptName { get; set; }
        [Display(Name = "Dept. Acronym")]

        [Remote("DepartmentAcronymExist", "RemoteValidation", AdditionalFields = "DeptId", HttpMethod = "Post", ErrorMessage = "Department acronym already exsits")]
        [StringLength(10, MinimumLength = 1, ErrorMessage = "Invalid")]
        public string DeptAcronym { get; set; }
        public Nullable<int> SiteId { get; set; }
        public string SiteName { get; set; }
        public Nullable<int> UnitId { get; set; }

        [Remote("SiteUnitNameExist", "RemoteValidation", AdditionalFields = "UnitId,SiteId", HttpMethod = "Post", ErrorMessage = "Unit name already exsits")]
        public string UnitName { get; set; }
        [Remote("SiteUnitAcryonmExist", "RemoteValidation", AdditionalFields = "UnitId,SiteId", HttpMethod = "Post", ErrorMessage = "Unit acronym already exsits")]
        public string UnitAcryonm { get; set; }

        [Display(Name="No.Of Line")]
        public Nullable<int> NumberOfLine { get; set; }
        public int Status { get; set; }
        public System.DateTime CreatedDate { get; set; }
        public long CreatedBy { get; set; }
        public Nullable<System.DateTime> UpdatedDate { get; set; }
        public Nullable<long> UpdatedBy { get; set; }
        public int HasLine { get; set; }
        public bool Displaytype { get; set; }
        public bool IsInfo { get; set; }
    }
}