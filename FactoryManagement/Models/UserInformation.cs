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
    
    public partial class UserInformation
    {
        public long UserId { get; set; }
        public Nullable<int> Title { get; set; }
        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        public string LastName { get; set; }
        public string EmpId { get; set; }
        public string CardNumber { get; set; }
        public Nullable<int> RoleId { get; set; }
        public int SiteId { get; set; }
        public Nullable<int> UnitId { get; set; }
        public int DeptId { get; set; }
        public Nullable<int> LineId { get; set; }
        public Nullable<int> MachineId { get; set; }
        public int DesignationId { get; set; }
        public string Picture { get; set; }
        public string PictureOriginalName { get; set; }
        public int UserType { get; set; }
        public System.DateTime DateOfBirth { get; set; }
        public int Gender { get; set; }
        public string Nationality { get; set; }
        public string NationalId { get; set; }
        public string NationalIdBackImg { get; set; }
        public string NationalIdFontImg { get; set; }
        public string EmailAddress { get; set; }
        public int Religion { get; set; }
        public string Phone { get; set; }
        public string MobileNo { get; set; }
        public Nullable<decimal> BasicSalary { get; set; }
        public string ParAddress { get; set; }
        public string ParAddressLine1 { get; set; }
        public string ParCountry { get; set; }
        public string ParState { get; set; }
        public Nullable<int> ParDivisionId { get; set; }
        public string ParCity { get; set; }
        public string ParArea { get; set; }
        public string ParPotalCode { get; set; }
        public bool SamePresentAddress { get; set; }
        public string PreAddress { get; set; }
        public string PreAddressLine1 { get; set; }
        public string PreCountry { get; set; }
        public string PreState { get; set; }
        public Nullable<int> PreDivisionId { get; set; }
        public string PreCity { get; set; }
        public string PreArea { get; set; }
        public string PrePostalCode { get; set; }
        public int Status { get; set; }
        public Nullable<System.DateTime> JoinDate { get; set; }
        public System.DateTime CreatedDate { get; set; }
        public long CreatedBy { get; set; }
        public Nullable<System.DateTime> UpdatedDate { get; set; }
        public Nullable<long> UpdatedBy { get; set; }
        public bool AssginSalary { get; set; }
        public Nullable<int> SalaryPackageId { get; set; }
        public string x1 { get; set; }
        public string y1 { get; set; }
        public string x2 { get; set; }
        public string y2 { get; set; }
        public string width { get; set; }
        public string height { get; set; }
        public Nullable<int> HolidayPackId { get; set; }
        public Nullable<bool> IsCustomePack { get; set; }
        public Nullable<int> NoOfPaidLeave { get; set; }
        public Nullable<long> HolidayAssignedBy { get; set; }
        public Nullable<System.DateTime> HolidayAssignedDate { get; set; }
        public Nullable<int> WorkingScheduleId { get; set; }
        public Nullable<long> WorkScheduleAssignedBy { get; set; }
        public Nullable<System.DateTime> WorkScheduleAssignedDate { get; set; }
        public byte[] ImageData { get; set; }
    }
}