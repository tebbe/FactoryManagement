using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FactoryManagement.ModelView.Inventory.Store
{
    public class DispatchedItemListModelView
    {
        public long Id { get; set; }
        public int DispatchedId { get; set; }
        public string Dispatchedname { get; set; }
        public int InventoryId { get; set; }
        public string Productname { get; set; }
        public int LocationId { get; set; }
        
        public string Unitname { get; set; }
        public string Ordername { get; set; }
        public string Machinename { get; set; }
        public long DispatchedBy { get; set; }
        public string DispatchedUserName { get; set; }
        public string DispatchedUserPic { get; set; }
        public double Quantity { get; set; }
        public Nullable<double> ReturnQuantity { get; set; }
        public Nullable<double> MinimumQuantity { get; set; }
        public Nullable<int> OrderId { get; set; }
        public Nullable<int> MachineId { get; set; }
        public Nullable<int> UnitId { get; set; }
        public Nullable<int> DeptId { get; set; }
        public Nullable<int> LineId { get; set; }
        public Nullable<int> StoreId { get; set; }
        public Nullable<int> WareId { get; set; }
        public string Warename { get; set; }
        public string Storename { get; set; }
        public string DeptName { get; set; }

        //***************** For Import History **************
        public string Lc { get; set; }
        public Nullable<System.DateTime> InsertedDate { get; set; }
        public Nullable<long> ApprovedBy { get; set; }
        public Nullable<System.DateTime> ApprovedDate { get; set; }
    }
}