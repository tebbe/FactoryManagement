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
    
    public partial class View_Import_Item_Details
    {
        public long ImportId { get; set; }
        public string VoucherName { get; set; }
        public string TransactionName { get; set; }
        public Nullable<int> InventoryId { get; set; }
        public string L_C { get; set; }
        public double Quantity { get; set; }
        public long InsertedBy { get; set; }
        public string InsertedByUserName { get; set; }
        public string InsertedUserPic { get; set; }
        public string ProductName { get; set; }
        public string InsertedDateFormat { get; set; }
        public System.DateTime InsertedDate { get; set; }
        public Nullable<long> ParentId { get; set; }
        public string AssignLocation { get; set; }
    }
}
