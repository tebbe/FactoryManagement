using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FactoryManagement.ModelView.Voucher
{
    public class Voucher_Other_Costs_ListModelView
    {
        public int OtherCostId { get; set; }
        public int VoucherId { get; set; }
        public int ServiceId { get; set; }
        public decimal Amount { get; set; }
        public System.DateTime CreatedDate { get; set; }
        public long CreatedBy { get; set; }
        public Nullable<System.DateTime> UpdatedDate { get; set; }
        public Nullable<long> UpdatedBy { get; set; }
    }
}