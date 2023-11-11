using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace FactoryManagement.ModelView.SalaryConfig
{
    public class SalaryAccAddModelView
    {
        public int Id { get; set; }
        public int PackageId { get; set; }
        [Required(ErrorMessage = "Please Enter Name")]
        public string Name { get; set; }
        public bool IsAddition { get; set; }
        public Nullable<decimal> Amount { get; set; }
        public Nullable<decimal> Percentage { get; set; }
        public long CreatedBy { get; set; }
        public System.DateTime CreatedDate { get; set; }
        public Nullable<System.DateTime> UpdatedDate { get; set; }
        public Nullable<long> UpdatedBy { get; set; }



        [Required(ErrorMessage = "Please Select One Type")]
        public int Catagory { get; set; }

        [Required(ErrorMessage = "Please Select One Type")]
        public int AccountType { get; set; }


        [Display(Name="Month")]
        public Nullable<int> MonthId { get; set; }
        public string AllMonthId { get; set; }
        public string AllMonthName { get; set; }

        [Display(Name = "Holiday List")]
        public Nullable<int> HolidayId { get; set; }

        [Required(ErrorMessage = "Please Select One Type")]
        public int AccPayType { get; set; }

       // [Required(ErrorMessage = "Please Select One Type")]
        public string AmountType { get; set; }


        public bool chkForMonth { get; set; }
        public bool chkForEvent { get; set; }
        public bool chkForAnual { get; set; }

        public bool IsBaseSalary { get; set; }
        public Nullable<decimal> BaseSalary { get; set; }
        public int BaseSalaryExsist { get; set; }
        public string CatagoryName { get; set; }


        public int Acc_Id { get; set; }
    }
}