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
    
    public partial class DuePaymentRecord
    {
        public int Id { get; set; }
        public long PaymentId { get; set; }
        public long SalaryPayId { get; set; }
        public decimal Amount { get; set; }
        public long UserId { get; set; }
        public int UserType { get; set; }
        public System.DateTime PaidDate { get; set; }
        public long PaidBy { get; set; }
    }
}
