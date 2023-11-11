using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace FactoryManagement.ModelView.Voucher
{
    public class VoucherModelView
    {
        public long VoucherId { get; set; }
        [Required(ErrorMessage = "Please enter title")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Invalid")]
        public string VoucherTitle { get; set; }
        [Required(ErrorMessage = "Please enter number")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Invalid")]
        public string Number { get; set; }
        [Required(ErrorMessage = "Please select Voucher Type")]
        public Nullable<int> VoucherTypeId { get; set; }
        public Nullable<decimal> TotalAmount { get; set; }
        public long CreatedBy { get; set; }
       
        public System.DateTime CreatedDate { get; set; }
        public Nullable<long> UpdatedBy { get; set; }
       
        public Nullable<System.DateTime> UpdatedDate { get; set; }

     // Only for model view
        public decimal ServiceTotal { get; set; }
        public decimal OtherServiceTotal { get; set; }
        public int Display { get; set; }
        public string VoucherTypeName { get; set; }
        public string CreatedDateName { get; set; }
        public string UpdatedDateName { get; set; }
        public string UpdatedByName { get; set; }
        public string CreatedByPicture { get; set; }
        public string CreatedByName { get; set; }
        public string UpdatedByPicture { get; set; }

        // only for voucher product
        public int VoucherServiceId { get; set; }
        public string VoucherProduct { get; set; }
        public Nullable<decimal> Amount { get; set; }
        public string LocationId { get; set; }

        public bool IsAccountSelected { get; set; }
        public int Status { get; set; }
        public Nullable<int> AccId { get; set; }
        public Nullable<long> AmntDispatchedBy { get; set; }
        public Nullable<System.DateTime> AmntDispatchedDate { get; set; }
        public Nullable<decimal> DispatchedAmnt { get; set; }
        public Nullable<System.DateTime> ReceivedDate { get; set; }
        public Nullable<long> ReceivedBy { get; set; }
        public Nullable<int> InternalAccId { get; set; }
        public Nullable<int> BankAccId { get; set; }
        public Nullable<int> SiteId { get; set; }
        public Nullable<int> UnitId { get; set; }
        public Nullable<int> DeptId { get; set; }
        public Nullable<int> LineId { get; set; }
        public Nullable<int> StoreId { get; set; }
        public Nullable<int> WareId { get; set; }
    }
}