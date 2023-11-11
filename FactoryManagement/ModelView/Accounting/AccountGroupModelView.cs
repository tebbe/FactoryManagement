using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace FactoryManagement.ModelView.Accounting
{
    public class AccountGroupModelView
    {
        public int AccId { get; set; }

        [Display(Name = "Name")]
        [Required(ErrorMessage = "Please enter account name")]
        [Remote("AccountGrpNameExsits", "RemoteValidation", AdditionalFields = "AccId", HttpMethod = "Post", ErrorMessage = "Account group name already exsits")]
        public string AccountName { get; set; }
        public string Description { get; set; }
        public Nullable<int> Acc_Type { get; set; }
        public int Acc_CashType { get; set; }

        public Nullable<int> SiteId { get; set; }
        public Nullable<int> UnitId { get; set; }
        public Nullable<int> StoreId { get; set; }
        public Nullable<int> WareId { get; set; }
        public Nullable<int> DeptId { get; set; }
        public string AllAssignUserId { get; set; }
        public decimal Balance { get; set; }
        public Nullable<decimal> WorkingBalance { get; set; }
        public Nullable<decimal> TotalCreditLimit { get; set; }

        public long CreatedBy { get; set; }
        public System.DateTime CreatedDate { get; set; }
        public Nullable<long> UpdatedBy { get; set; }
        public Nullable<System.DateTime> UpdatedDate { get; set; }


        public Nullable<decimal> TransactionHigestAmount { get; set; }
        [RegularExpression("([1-9][0-9]*)", ErrorMessage = "Transaction no. must be a an integer number")]
        public Nullable<int> NoOfTranPerMonth { get; set; }
        public Nullable<bool> SetBudget { get; set; }

        public bool IsPayable { get; set; }
        public bool IsReceivable { get; set; }

    }
}