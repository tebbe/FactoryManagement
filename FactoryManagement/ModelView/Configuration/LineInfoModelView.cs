using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace FactoryManagement.ModelView.Configuration
{
    public class LineInfoModelView
    {
        public int LineId { get; set; }
        public int DeptId { get; set; }
        public string LineName { get; set; }
        public string LineAcronym { get; set; }
        public int NumberOfMachine { get; set; }
        public int Status { get; set; }
        public System.DateTime CreatedDate { get; set; }
        public long CreatedBy { get; set; }
        public Nullable<System.DateTime> UpdatedDate { get; set; }
        public Nullable<long> UpdatedBy { get; set; }

        [Display(Name = "Dept. Name")]
        public string DeptName { get; set; }
        [Display(Name = "Dept. Acronym")]
        public string DeptAcronym { get; set; }
        [Display(Name="No.Of Line")]
        public Nullable<int> NumberOfLine { get; set; }
        public string AllLineDetails { get; set; }
        public Nullable<int> LineCount { get; set; }
        public Nullable<int> SiteId { get; set; }
        public string SiteName { get; set; }
        public Nullable<int> UnitId { get; set; }
        public string UnitName { get; set; }
    }
}