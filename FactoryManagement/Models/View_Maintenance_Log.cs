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
    
    public partial class View_Maintenance_Log
    {
        public long Id { get; set; }
        public int MachineId { get; set; }
        public int MachineTypeId { get; set; }
        public string MachineAcronym { get; set; }
        public int PartsId { get; set; }
        public string PartsName { get; set; }
        public int Type { get; set; }
        public System.DateTime MaintenanceDate { get; set; }
        public long MaintenanceBy { get; set; }
        public string OperationDoneUserName { get; set; }
        public string OperationDoneUserPic { get; set; }
        public string OperationDoneDateName { get; set; }
        public string Msg { get; set; }
    }
}
