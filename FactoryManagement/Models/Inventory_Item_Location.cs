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
    
    public partial class Inventory_Item_Location
    {
        public int LocationId { get; set; }
        public int InventoryId { get; set; }
        public Nullable<long> ImportId { get; set; }
        public Nullable<int> StoreId { get; set; }
        public Nullable<int> WarehouseId { get; set; }
        public Nullable<int> DeptId { get; set; }
        public Nullable<int> LineId { get; set; }
        public double Quantity { get; set; }
        public int UnitId { get; set; }
        public Nullable<int> LastOrderId { get; set; }
        public double MinimumQuantity { get; set; }
        public Nullable<long> InsertedBy { get; set; }
        public Nullable<System.DateTime> InsertedDate { get; set; }
        public Nullable<long> ReceivedBy { get; set; }
        public Nullable<System.DateTime> ReceivedDate { get; set; }
        public Nullable<System.DateTime> UpdatedDate { get; set; }
        public Nullable<long> UpdatedBy { get; set; }
        public string Description { get; set; }
    }
}
