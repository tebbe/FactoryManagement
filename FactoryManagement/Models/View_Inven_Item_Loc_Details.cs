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
    
    public partial class View_Inven_Item_Loc_Details
    {
        public int LocationId { get; set; }
        public int InventoryId { get; set; }
        public string ProductType { get; set; }
        public string ProductName { get; set; }
        public Nullable<int> ProductTypeId { get; set; }
        public Nullable<int> StoreId { get; set; }
        public Nullable<int> WarehouseId { get; set; }
        public string StoreName { get; set; }
        public string WareName { get; set; }
        public double Quantity { get; set; }
        public string UnitName { get; set; }
        public string Description { get; set; }
    }
}