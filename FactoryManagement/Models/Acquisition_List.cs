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
    
    public partial class Acquisition_List
    {
        public int AcquisitionOrderId { get; set; }
        public string Name { get; set; }
        public int AcquisitionType { get; set; }
        public Nullable<int> BusinessOrderId { get; set; }
        public Nullable<int> PartsId { get; set; }
        public Nullable<int> MachineId { get; set; }
        public int SiteId { get; set; }
        public int AcquisitionStatus { get; set; }
        public long CreatedBy { get; set; }
        public System.DateTime CreatedDate { get; set; }
        public Nullable<long> UpdatedBy { get; set; }
        public Nullable<System.DateTime> UpdatedDate { get; set; }
    }
}