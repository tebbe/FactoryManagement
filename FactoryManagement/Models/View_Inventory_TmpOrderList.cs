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
    
    public partial class View_Inventory_TmpOrderList
    {
        public int InventoryId { get; set; }
        public string ProductName { get; set; }
        public int StockQuantity { get; set; }
        public string UnitName { get; set; }
        public int OrderQuantity { get; set; }
        public Nullable<decimal> Price { get; set; }
        public Nullable<decimal> totalprice { get; set; }
        public long OrderedBy { get; set; }
        public string ItemNameWithPrice { get; set; }
        public bool StoreItem { get; set; }
        public bool WareItem { get; set; }
    }
}