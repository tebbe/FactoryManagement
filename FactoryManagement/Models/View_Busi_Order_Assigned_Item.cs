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
    
    public partial class View_Busi_Order_Assigned_Item
    {
        public string ProductName { get; set; }
        public Nullable<int> AssignedQuantity { get; set; }
        public Nullable<int> ReceivedQuantity { get; set; }
        public string UnitName { get; set; }
        public bool IsAssigned { get; set; }
        public int AcquisitionOrderId { get; set; }
        public int InventoryId { get; set; }
        public System.DateTime ReserveredDate { get; set; }
        public long ReserveredBy { get; set; }
        public bool IsSubUnit { get; set; }
        public int ReservedQuantity { get; set; }
        public int ReservedId { get; set; }
    }
}