using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FactoryManagement.ModelView.HR
{
    public class AllSalaryPayListModelView
    {
        public long Id { get; set; }
        public int EmpSalaryPayId { get; set; }
        public int UserId { get; set; }
        public int UserType { get; set; }
        public int PayType { get; set; }
        public Nullable<int> InernalAccId { get; set; }
        public Nullable<decimal> Amount { get; set; }
        public Nullable<int> BankId { get; set; }
        public Nullable<int> BankAccId { get; set; }
        public string BankAccSlipNo { get; set; }
        public long InsertBy { get; set; }
        public string InsertByName { get; set; }
        public System.DateTime InsertedDate { get; set; }
        public string Username { get; set; }
        public string Userpic { get; set; }
        public string GivenUsername { get; set; }
        public string GivenUserpic { get; set; }

    }
}