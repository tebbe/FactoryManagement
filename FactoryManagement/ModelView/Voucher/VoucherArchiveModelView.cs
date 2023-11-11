using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FactoryManagement.ModelView.Voucher
{
    public class VoucherArchiveModelView
    {
        public int VoucherId { get; set; }
        public int Ref_VoucherId { get; set; }
        public string VoucherTitle { get; set; }
        public string Number { get; set; }
        public Nullable<int> VoucherTypeId { get; set; }
        public Nullable<decimal> TotalAmount { get; set; }
        public long CreatedBy { get; set; }
        public System.DateTime CreatedDate { get; set; }
        public Nullable<long> UpdatedBy { get; set; }
        public Nullable<System.DateTime> UpdatedDate { get; set; }

        public string VoucherTypeName { get; set; }
    }
}