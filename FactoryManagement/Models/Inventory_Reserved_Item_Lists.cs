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
    
    public partial class Inventory_Reserved_Item_Lists
    {
        public int ReservedId { get; set; }
        public int InventoryId { get; set; }
        public Nullable<int> LocationId { get; set; }
        public int Quantity { get; set; }
        public bool IsSubUnit { get; set; }
        public int AcquisitionOrderId { get; set; }
        public bool IsAssigned { get; set; }
        public long ReserveredBy { get; set; }
        public System.DateTime ReserveredDate { get; set; }
    }
}