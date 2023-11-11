using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FactoryManagement.ModelView.Inventory.Store
{
    public class View_Dispacth_Item_DetailsModelView
    {
        public long Id { get; set; }
        public int DispatchedId { get; set; }
        public string Name { get; set; }
        public string ProductName { get; set; }
        public int InventoryId { get; set; }
        public int LocationId { get; set; }
        public double Quantity { get; set; }
        public double Total_Quantity { get; set; }
        public int UnitId { get; set; }
        public string UnitName { get; set; }
        public Nullable<double> ReturnQuantity { get; set; }
        public double Total_Return { get; set; }
        public Nullable<double> Total_UsedQuan { get; set; }
        public Nullable<int> OrderId { get; set; }
        public Nullable<int> MachineId { get; set; }
        public Nullable<int> Site_Unit_Id { get; set; }
        public Nullable<int> DeptId { get; set; }
        public Nullable<int> LineId { get; set; }
        public Nullable<int> WareId { get; set; }
        public Nullable<int> StoreId { get; set; }
        public System.DateTime DispatchedDate { get; set; }
        public long DispatchedBy { get; set; }
        public string DispatchedDateName { get; set; }
        public string DispatchedUserName { get; set; }
        public string DispatchedUserPic { get; set; }
        public string VoucherName { get; set; }
        public string ReturnTransactionName { get; set; }

        public Nullable<long> AcceptBy { get; set; }
        public Nullable<System.DateTime> AcceptDate { get; set; }
        public string AcceptDateName { get; set; }
        public string AcceptBUserName { get; set; }
        public string AcceptUserPic { get; set; }
        public string ReturnFrom { get; set; }


        public Nullable<System.DateTime> ReturnDate { get; set; }
        public Nullable<long> ReturnBy { get; set; }
        public string ReturnDateName { get; set; }
        public string ReturnUserName { get; set; }
        public string ReturnUserPic { get; set; }
        public string MachineName { get; set; }
        public string DeptName { get; set; }
        public string DeptName_Unit { get; set; }
    }
}