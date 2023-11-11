﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FactoryManagement.ModelView.HR
{
    public class View_Attendance_BCRModelView
    {
        public int Id { get; set; }
        public long UserId { get; set; }
        public string CardNumber { get; set; }
        public System.DateTime EntryDateTime { get; set; }
        public string Dates { get; set; }
        public string DayName { get; set; }
        public string EntryDateTimeFormate { get; set; }
        public bool IsIN { get; set; }
        public string EntryType { get; set; }
        public string UserName { get; set; }
        public string EmpId { get; set; }
        public string SiteName { get; set; }
        public string DeptName { get; set; }
        public string DesignationName { get; set; }
        public string Picture { get; set; }
        public string UserTypeName { get; set; }
        public string JoinDateFormate { get; set; }
        public Nullable<System.DateTime> JoinDate { get; set; }
        public int SiteId { get; set; }
        public int DeptId { get; set; }
        public Nullable<int> LineId { get; set; }
        public Nullable<int> UnitId { get; set; }
        public Nullable<int> MachineId { get; set; }
        public int DesignationId { get; set; }
        public Nullable<int> RoleId { get; set; }
        public int Gender { get; set; }
    }
}