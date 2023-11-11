//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace FactoryManagement.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class View_Voucher_Transaction
    {
        public long Id { get; set; }
        public string TranName { get; set; }
        public long VoucherId { get; set; }
        public Nullable<int> InternalAccId { get; set; }
        public Nullable<int> BankId { get; set; }
        public Nullable<int> BranchId { get; set; }
        public Nullable<int> BankAccId { get; set; }
        public bool AmountIncrease { get; set; }
        public Nullable<decimal> Amout { get; set; }
        public Nullable<long> CreatedBy { get; set; }
        public Nullable<System.DateTime> CreatedDate { get; set; }
        public Nullable<System.DateTime> UpdatedDate { get; set; }
        public Nullable<long> UpdatedBy { get; set; }
        public int ApproveStatus { get; set; }
        public Nullable<long> ApprovedBy { get; set; }
        public Nullable<System.DateTime> ApprovedDate { get; set; }
        public Nullable<long> ReceivedBy { get; set; }
        public Nullable<System.DateTime> ReceivedDate { get; set; }
        public string VoucherTitle { get; set; }
        public string Number { get; set; }
        public Nullable<int> VoucherTypeId { get; set; }
        public string VoucherTypeName { get; set; }
        public string CreatedDateName { get; set; }
        public string CreatedUserName { get; set; }
        public string CreatedUserPic { get; set; }
        public string UpdatedDateFormate { get; set; }
        public string UpdatedUserName { get; set; }
        public string UpdatedUserPic { get; set; }
        public string ApproveFormate { get; set; }
        public string ApprovedUserName { get; set; }
        public string ApprovedUserPic { get; set; }
        public string ReceivedDateName { get; set; }
        public string ReceivedUserName { get; set; }
        public string ReceivedUserPic { get; set; }
    }
}
