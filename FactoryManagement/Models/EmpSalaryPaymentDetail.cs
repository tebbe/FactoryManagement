//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace FactoryManagement.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class EmpSalaryPaymentDetail
    {
        public long Id { get; set; }
        public long UserId { get; set; }
        public int UserType { get; set; }
        public int Year { get; set; }
        public int MonthId { get; set; }
        public string Month { get; set; }
        public decimal TotalAmount { get; set; }
        public int Status { get; set; }
        public Nullable<decimal> PaidAmount { get; set; }
        public Nullable<System.DateTime> PaidDate { get; set; }
        public Nullable<long> PaidBy { get; set; }
        public System.DateTime GeneratedDate { get; set; }
        public Nullable<decimal> DeductedLoanAmnt { get; set; }
        public Nullable<long> DeductedLoanId { get; set; }
    }
}
