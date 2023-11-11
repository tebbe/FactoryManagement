using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace FactoryManagement.ModelView.Accounting
{
    public class BankAccountListModelView
    {
        public int BankAccountId { get; set; }

        //[Required(ErrorMessage = "Please enter account type")]
        public int Acc_Type { get; set; }
        public int BranchId { get; set; }

        [Required(ErrorMessage = "Please enter account number")]
        [StringLength(40, MinimumLength = 2, ErrorMessage = "Invalid")]
        public string AccountNumber { get; set; }

        [Required(ErrorMessage = "Please enter account name")]
        [StringLength(40, MinimumLength = 2, ErrorMessage = "Invalid")]
        public string AccountName { get; set; }
        public decimal Balance { get; set; }
        public Nullable<decimal> TotalCreditLimit { get; set; }
        public int NoOfTransaction { get; set; }
        public decimal HighestAmntPerTansaction { get; set; }
        public string Description { get; set; }
        public long CreatedBy { get; set; }
        public System.DateTime CreatedDate { get; set; }
        public Nullable<long> UpdatedBy { get; set; }
        public Nullable<System.DateTime> UpdatedDate { get; set; }
        public string BankName { get; set; }
        public int ForAccountDisplay { get; set; }
        public string AccountTypeName { get; set; }
        //Only for model view ******************
        public int BankId { get; set; }
    }
}