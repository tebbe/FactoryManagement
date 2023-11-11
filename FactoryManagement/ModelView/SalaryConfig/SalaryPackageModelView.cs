using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace FactoryManagement.ModelView.SalaryConfig
{
    public class SalaryPackageModelView
    {
        public int PackageId { get; set; }

        [Required(ErrorMessage = "Please enter package name")]
        [Remote("PackageNameExist", "RemoteValidation", AdditionalFields = "PackageId",ErrorMessage = "Package name already exsits")]
        [StringLength(30, ErrorMessage = "Invalid")]
        [Display(Name = "Package Name")]
        public string PackageName { get; set; }
        public Nullable<decimal> Amount { get; set; }
        public System.DateTime CreatedDate { get; set; }
        public long CreatedBy { get; set; }
        public Nullable<System.DateTime> UpdatedDate { get; set; }
        public Nullable<long> UpdatedBy { get; set; }
    }
}