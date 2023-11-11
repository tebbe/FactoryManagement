using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FactoryManagement.ModelView.HR
{
    public class EmpSalaryPaymentDetailModelView
    {
        public long Id { get; set; }
        public long UserId { get; set; }
        public int UserType { get; set; }
        public int MonthId { get; set; }
        public string Month { get; set; }
        public decimal TotalAmount { get; set; }
        public int Status { get; set; }
        public Nullable<decimal> PaidAmount { get; set; }
        public Nullable<System.DateTime> PaidDate { get; set; }
        public Nullable<long> PaidBy { get; set; }
        public System.DateTime GeneratedDate { get; set; }

        public string UserName { get; set; }
        public string UserPic { get; set; }


        //******************* For Salary Pay **********************

        public int EmpSalaryPayId { get; set; }
        public int PayType { get; set; }
        public Nullable<int> InernalAccId { get; set; }
        public Nullable<decimal> Amount { get; set; }
        public Nullable<int> BankId { get; set; }
        public Nullable<int> BankAccId { get; set; }
        public string BankAccSlipNo { get; set; }
        public long InsertBy { get; set; }
        public System.DateTime InsertedDate { get; set; }

        //************** for User Inforamation *********************

        public int SiteId { get; set; }
        public string SiteName { get; set; }
        public int DeptId { get; set; }
        public string DeptName { get; set; }
        public Nullable<int> LineId { get; set; }

        public int WorkingScheduleId { get; set; }
        public int HolidayPckId { get; set; }
        public int Year { get; set; }


        //**************FOR MONTHLY SALARY *********************
        public string EmpId { get; set; }
        public string DesignationName { get; set; }
        public string JoinDateFormate { get; set; }
    }
}