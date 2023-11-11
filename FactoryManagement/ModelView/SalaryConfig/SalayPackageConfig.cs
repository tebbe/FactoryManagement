using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FactoryManagement.ModelView.SalaryConfig
{
    public class SalayPackageConfig
    {
        public int Id { get; set; }
        public int PackageId { get; set; }
        public string Name { get; set; }
        public bool IsAddition { get; set; }
        public Nullable<decimal> Amount { get; set; }
        public Nullable<decimal> Percentage { get; set; }
        public long CreatedBy { get; set; }
        public System.DateTime CreatedDate { get; set; }
        public Nullable<System.DateTime> UpdatedDate { get; set; }
        public Nullable<long> UpdatedBy { get; set; }



        public string PackageName { get; set; }
        public bool HasBaseSalary { get; set; }
    }
}