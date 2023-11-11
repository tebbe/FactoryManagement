using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace FactoryManagement.ModelView.HR
{
    public class User_Bank_Account_ModelView
    {
            public int BankId { get; set; }
            [Required(ErrorMessage = "Please enter bank name")]
            [StringLength(50, MinimumLength = 4, ErrorMessage = "Minimum length 4")]
            [Remote("BanknameExistsforUser", "RemoteValidation", AdditionalFields = "userId", HttpMethod = "Post", ErrorMessage = "Bank information already exists")]
            public string BankName { get; set; }
            [Required(ErrorMessage = "Please enter branch name")]
            [StringLength(50, MinimumLength = 4, ErrorMessage = "Minimum length 4")]
            public string BranchName { get; set; }
            [Required(ErrorMessage = "Please enter bank account number")]
            [StringLength(50, MinimumLength = 5, ErrorMessage = "Minimum length 5")]
            public string AccountNumber { get; set; }
            public bool IsPrimary { get; set; }
            public long UserId { get; set; }
            public long CreatedBy { get; set; }
            public System.DateTime CreatedDate { get; set; }
            public Nullable<long> UpdatedBy { get; set; }
            public Nullable<System.DateTime> UpdatedDate { get; set; }   
    }
}