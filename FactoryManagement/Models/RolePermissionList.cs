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
    
    public partial class RolePermissionList
    {
        public int Id { get; set; }
        public int RoleId { get; set; }
        public string ConUniqueId { get; set; }
        public string ControllerName { get; set; }
        public string Permissionname { get; set; }
        public long AssignedBy { get; set; }
        public System.DateTime AssignedDate { get; set; }
    }
}