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
    
    public partial class View_Categories
    {
        public int CategoryId { get; set; }
        public string CategoryName { get; set; }
        public bool HasParent { get; set; }
        public Nullable<int> ParentId { get; set; }
        public string ParentName { get; set; }
        public string HasChild { get; set; }
        public long CreatedBy { get; set; }
        public string CreatedDateName { get; set; }
        public Nullable<System.DateTime> UpdatedDate { get; set; }
        public Nullable<long> UpdatedBy { get; set; }
        public bool CanBeDeleted { get; set; }
        public string UpdatedDateFormate { get; set; }
    }
}
