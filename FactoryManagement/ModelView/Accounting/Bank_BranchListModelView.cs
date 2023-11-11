using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace FactoryManagement.ModelView.Accounting
{
    public class Bank_BranchListModelView
    {
        public int BranchId { get; set; }
        public int BankId { get; set; }
        [Required(ErrorMessage = "Please enter branch name")]
        [Remote("BranchNameExist", "RemoteValidation", AdditionalFields = "BranchId", HttpMethod = "Post", ErrorMessage = "Branch name already exsits...!!!")]
        [StringLength(40, MinimumLength = 2, ErrorMessage = "Invalid")]
        public string BranchName { get; set; }
        [Required(ErrorMessage = "Please enter address")]
        public string Address { get; set; }
        public string AddressLine1 { get; set; }
        public Nullable<int> DivisonId { get; set; }
        public string State { get; set; }
         [Required(ErrorMessage = "Please enter city")]
        public string City { get; set; }
         [Required(ErrorMessage = "Please enter postal code")]
        public string PostalCode { get; set; }
         [Required(ErrorMessage = "Please enter country")]
        public string Country { get; set; }
        public string MobileNo { get; set; }
        public string PhoneNo { get; set; }
        public int Status { get; set; }
        public long CreatedBy { get; set; }
        public System.DateTime CreatedDate { get; set; }
        public Nullable<long> UpdatedBy { get; set; }
        public Nullable<System.DateTime> UpdatedDate { get; set; }

        // Only for Model View
        public string BankName { get; set; }
        public int BankAccountId { get; set; }
        public string CountryName { get; set; }
        public string DivisionName { get; set; }
        public string CreatedByName { get; set; }
        public string UpdatedByName { get; set; }
        public string CreatedDateName { get; set; }
        public string CreatedByPicture { get; set; }
        public string UpdatedDateName { get; set; }
        public string UpdatedByPicture { get; set; }
        public int ForDisplay { get; set; }
    }
}