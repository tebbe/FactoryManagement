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
    
    public partial class Daily_Attendance
    {
        public long Id { get; set; }
        public long UserId { get; set; }
        public string MonthName { get; set; }
        public int MonthId { get; set; }
        public int Year { get; set; }
        public System.DateTime AttDate { get; set; }
        public string AttTime { get; set; }
        public System.DateTime AttDatetime { get; set; }
        public long InsertedBy { get; set; }
        public System.DateTime InsertedDate { get; set; }
        public int AttType { get; set; }
        public Nullable<int> LeaveType { get; set; }
    }
}
