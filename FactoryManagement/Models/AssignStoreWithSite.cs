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
    
    public partial class AssignStoreWithSite
    {
        public int Id { get; set; }
        public int SiteId { get; set; }
        public int StoreId { get; set; }
        public System.DateTime AssignedDate { get; set; }
        public long AssignedBy { get; set; }
    }
}
