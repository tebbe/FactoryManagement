using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FactoryManagement.ModelView.Voucher
{
    public class Voucher_Service_ListModelView
    {
        public int VoucherServiceId { get; set; }
        public int VoucherId { get; set; }
        public string Name { get; set; }
        public decimal Amount { get; set; }
        public System.DateTime CreatedDate { get; set; }
        public long CreatedBy { get; set; }
        public Nullable<System.DateTime> UpdatedDate { get; set; }
        public Nullable<long> UpdatedBy { get; set; }
    }
}