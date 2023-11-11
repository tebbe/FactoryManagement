using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace FactoryManagement.ModelView.HR
{
    public class WorkingScheduleListModelView
    {
        public int WorkingScheduleId { get; set; }
        public Nullable<int> BreakTimeType { get; set; }
     
        public Nullable<decimal> BreakTime { get; set; }     
        [Required(ErrorMessage = "Please enter schedule name")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "Minimum length 2")]
        [Remote("WorkingSchedulNameExists", "RemoteValidation", AdditionalFields = "WorkingScheduleId", HttpMethod = "Post", ErrorMessage = "Schedule name already exists")]
        public string SchedulName { get; set; }
        public string StartTime { get; set; }
        public string Start_Allowance { get; set; }
        public string EndTime { get; set; }
        public string End_Allowance { get; set; }
        public System.DateTime CreatedDate { get; set; }
        public long CreatedBy { get; set; }
        public Nullable<System.DateTime> UpdatedDate { get; set; }
        public Nullable<long> UpdatedBy { get; set; }
        public bool SameWorkHour { get; set; }
        public string ChkWorkingDays { get; set; }
    }
}