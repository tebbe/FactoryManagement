using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace FactoryManagement.ModelView.HR
{
    public class HolidayListModelView
    {
        public int Id { get; set; }
        public int HolidayPackId { get; set; }

        [Required(ErrorMessage="Please type holiday name.")]
        public string HolidayName { get; set; }
        public bool IsMultipleDay { get; set; }
        public string MonthName { get; set; }
        public Nullable<int> MonthCount { get; set; }
        public int TotalDay { get; set; }
        public Nullable<System.DateTime> Date { get; set; }
        public string Year { get; set; }
        public System.DateTime CreatedDate { get; set; }
        public long CreatedBy { get; set; }
        public Nullable<System.DateTime> UpdatedDate { get; set; }
        public Nullable<long> UpdateBy { get; set; }


        public Nullable<System.DateTime> StartDate { get; set; }
        public Nullable<System.DateTime> EndDate { get; set; }

    }
}