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
    
    public partial class Salary_Package_Account_Lists
    {
        public int Id { get; set; }
        public int PackageId { get; set; }
        public int CatagoryId { get; set; }
        public int AccountType { get; set; }
        public bool IsBaseSalary { get; set; }
        public string Name { get; set; }
        public int AccPayType { get; set; }
        public Nullable<int> HolidayId { get; set; }
        public Nullable<decimal> Amount { get; set; }
        public Nullable<decimal> Percentage { get; set; }
        public bool IsAddition { get; set; }
        public long CreatedBy { get; set; }
        public System.DateTime CreatedDate { get; set; }
        public Nullable<System.DateTime> UpdatedDate { get; set; }
        public Nullable<long> UpdatedBy { get; set; }
    }
}
