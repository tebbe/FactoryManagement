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
    
    public partial class SalaryPaymentPckDetail
    {
        public long Id { get; set; }
        public int PaymentId { get; set; }
        public string Acc_Name { get; set; }
        public decimal Amount { get; set; }
        public bool IsBasic { get; set; }
        public bool IsDeduct { get; set; }
    }
}
