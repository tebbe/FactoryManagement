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
    
    public partial class View_Manual_Indent
    {
        public long IndentId { get; set; }
        public int IndentType { get; set; }
        public string ProName { get; set; }
        public double Quantity { get; set; }
        public int ProUnitId { get; set; }
        public string UnitName { get; set; }
        public decimal ProUnitPrice { get; set; }
        public string Comments { get; set; }
        public int IndentStatus { get; set; }
        public long CreatedBy { get; set; }
        public System.DateTime CreatedDate { get; set; }
        public string CreatedDateName { get; set; }
        public string CreatedUserName { get; set; }
        public string CreatedUserPic { get; set; }
        public Nullable<System.DateTime> UpdatedDate { get; set; }
        public string UpdatedDateFormate { get; set; }
        public Nullable<long> UpdatedBy { get; set; }
        public string UpdateUsername { get; set; }
        public string UpdateUserPic { get; set; }
        public Nullable<long> ApprovedBy { get; set; }
        public Nullable<System.DateTime> ApprovedDate { get; set; }
        public string ApprovedUsername { get; set; }
        public string ApprovedUserPic { get; set; }
        public string ApprovedDateFormate { get; set; }
        public Nullable<int> MachineId { get; set; }
        public Nullable<int> StoreId { get; set; }
        public Nullable<int> WareId { get; set; }
        public long VoucherId { get; set; }
        public string Name { get; set; }
        public int VoucherStatus { get; set; }
    }
}
