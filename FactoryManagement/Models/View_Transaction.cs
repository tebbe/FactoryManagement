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
    
    public partial class View_Transaction
    {
        public long TranId { get; set; }
        public string TranName { get; set; }
        public int TranTypeId { get; set; }
        public string Name { get; set; }
        public long UserId { get; set; }
        public long InsertedBy { get; set; }
        public decimal Amount { get; set; }
        public System.DateTime InsertedDate { get; set; }
        public string InsertedDateFormate { get; set; }
        public long RefTableId { get; set; }
        public string EmpId { get; set; }
        public int SiteId { get; set; }
        public string SiteName { get; set; }
        public int DesignationId { get; set; }
        public string DesignationName { get; set; }
        public string UserName { get; set; }
        public string Picture { get; set; }
        public string InsertUser { get; set; }
        public string InsertUserPic { get; set; }
    }
}
