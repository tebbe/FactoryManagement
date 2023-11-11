using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace FactoryManagement.ModelView.Accounting
{
    public class AccountNameModelView
    {
        public int AccountNameId { get; set; }
        [Display(Name = "Account Group")]
        [Required(ErrorMessage = "Please select one type")]
        public int Acc_Grp_Id { get; set; }
        [Display(Name = "Name")]
        [Required(ErrorMessage = "Please enter account name")]
        [Remote("AccountNameExsits", "RemoteValidation", AdditionalFields = "AccountNameId", HttpMethod = "Post", ErrorMessage = "Account name already exsits")]
        public string AccountName1 { get; set; }
        [Display(Name = "Account Type")]
        [Required(ErrorMessage = "Please select account type")]
        public int Acc_Type { get; set; }
        public long CreatedBy { get; set; }
        public System.DateTime CreatedDate { get; set; }
        public Nullable<System.DateTime> UpdatedDate { get; set; }
        public Nullable<long> UpdatedBy { get; set; }
    }
}