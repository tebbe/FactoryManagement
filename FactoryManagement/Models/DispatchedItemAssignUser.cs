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
    
    public partial class DispatchedItemAssignUser
    {
        public int Id { get; set; }
        public int DispatchId { get; set; }
        public Nullable<int> DispatchItemId { get; set; }
        public Nullable<int> OrderId { get; set; }
        public Nullable<int> MachineId { get; set; }
        public Nullable<int> UnitId { get; set; }
        public Nullable<int> DeptId { get; set; }
        public Nullable<int> LineId { get; set; }
        public Nullable<int> StoreId { get; set; }
        public Nullable<int> WareId { get; set; }
        public Nullable<long> UserId { get; set; }
        public string UserName { get; set; }
    }
}