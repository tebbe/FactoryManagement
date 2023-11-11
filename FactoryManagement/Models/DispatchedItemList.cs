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
    
    public partial class DispatchedItemList
    {
        public long Id { get; set; }
        public int DispatchedId { get; set; }
        public int InventoryId { get; set; }
        public int LocationId { get; set; }
        public double Quantity { get; set; }
        public Nullable<double> ReturnQuantity { get; set; }
        public Nullable<int> ReturnReasonId { get; set; }
        public string ReturnReason { get; set; }
        public Nullable<System.DateTime> ReturnDate { get; set; }
        public Nullable<long> ReturnBy { get; set; }
        public Nullable<int> OrderId { get; set; }
        public Nullable<int> MachineId { get; set; }
        public Nullable<int> UnitId { get; set; }
        public Nullable<int> DeptId { get; set; }
        public Nullable<int> LineId { get; set; }
        public Nullable<int> StoreId { get; set; }
        public Nullable<int> WareId { get; set; }
        public bool IsAlreadyUsed { get; set; }
        public Nullable<double> UsedQuan { get; set; }
        public Nullable<long> InsertedBy { get; set; }
        public Nullable<System.DateTime> InsertedDate { get; set; }
    }
}
