using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FactoryManagement.ModelView.Inventory.Store
{
    public class Inventory_Pro_Waste_ListModelView
    {
        public long Id { get; set; }
        public long InventoryId { get; set; }
        public string ProductName { get; set; }
        public int LocationId { get; set; }
        public int Type { get; set; }
        public double Quantity { get; set; }
        public int UnitId { get; set; }
        public string Unitname { get; set; }
        public System.DateTime InsertedDate { get; set; }
        public long InsertedBy { get; set; }
    }
}