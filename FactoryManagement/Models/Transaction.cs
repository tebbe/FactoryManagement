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
    
    public partial class Transaction
    {
        public long TranId { get; set; }
        public string TranName { get; set; }
        public int TranTypeId { get; set; }
        public long UserId { get; set; }
        public decimal Amount { get; set; }
        public long InsertedBy { get; set; }
        public System.DateTime InsertedDate { get; set; }
        public long RefTableId { get; set; }
    }
}