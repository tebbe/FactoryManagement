using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FactoryManagement.ModelView.HR
{
    public class AllUserLoanListModelView
    {
        public int LoanId { get; set; }
        public long UserId { get; set; }
        public int UserType { get; set; }
        public int PayType { get; set; }
        public Nullable<int> InernalAccId { get; set; }
        public Nullable<int> BankId { get; set; }
        public Nullable<int> BankAccId { get; set; }
        public string BankAccSlipNo { get; set; }
        public decimal Amount { get; set; }
        public Nullable<decimal> PaidAmount { get; set; }
        public int PaidStatus { get; set; }
        public long LoanPaidBy { get; set; }
        public System.DateTime LoanPaidDate { get; set; }
        public Nullable<long> LoanReceivedBy { get; set; }
        public Nullable<System.DateTime> LoanReceivedDate { get; set; }
        public bool DeductionFromSalary { get; set; }
        public Nullable<System.DateTime> LoanAmntReturnDeadLine { get; set; }
        public Nullable<decimal> DeductAmountPerMonth { get; set; }
        public string LoanPaidByName { get; set; }
        public string Username { get; set; }
        public string LoanReceivedByName { get; set; }
    }
}