using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace FactoryManagement.ModelView.HR
{
    public class UserInformationModelView
    {
        public long UserId { get; set; }
        [Required(ErrorMessage = "Please enter employeeId")]
        [Remote("EmpIdExist", "RemoteValidation", AdditionalFields = "UserId", HttpMethod = "Post", ErrorMessage = "Employee Id Already Exist!")]
        public string EmpId { get; set; }
        public string CardNumber { get; set; }
        public string SmartCardId { get; set; }
        public Nullable<int> Title { get; set; }

        [Required(ErrorMessage = "Please select site")]
        public int SiteId { get; set; }
        public Nullable<int> UnitId { get; set; }

        [Required(ErrorMessage = "Please select Department")]
        public int DeptId { get; set; }
        public Nullable<int> LineId { get; set; }
        public Nullable<int> MachineId { get; set; }

        [Required(ErrorMessage = "Please select Designation")]
        public int DesignationId { get; set; }

        [Required(ErrorMessage = "Please enter first name")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "Invalid")]
        [RegularExpression(@"^[a-zA-Z]+$", ErrorMessage = "Please use letters only")]
        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        [Required(ErrorMessage = "Please enter last name")]
        [RegularExpression(@"^[a-zA-Z]+$", ErrorMessage = "Please use letters only")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Invalid")]
        public string LastName { get; set; }
        public string Picture { get; set; }
        public string PictureOriginalName { get; set; }
        [Required(ErrorMessage = "Please select user type")]
        public int UserType { get; set; }
        [Required(ErrorMessage = "Please enter Date Of Birth")]
        public System.DateTime DateOfBirth { get; set; }
        [Required(ErrorMessage = "Please select Gender")]
        public int Gender { get; set; }
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Invalid")]
        [Required(ErrorMessage = "Please enter Nationality")]
        public string Nationality { get; set; }
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Invalid")]
        public string NationalId { get; set; }
        [RegularExpression(@"^([a-zA-Z0-9_\-\.]+)@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([a-zA-Z0-9\-]+\.)+))([a-zA-Z]{2,4}|[0-9]{1,3})(\]?)$", ErrorMessage = "Please enter a valid e-mail adress")]
        public string EmailAddress { get; set; }
        [Required(ErrorMessage = "Please select religion")]
        public int Religion { get; set; }

        [Required(ErrorMessage = "Please enter Phone Number")]
        public string Phone { get; set; }
        public string MobileNo { get; set; }
        public Nullable<decimal> BasicSalary { get; set; }
        [StringLength(100, MinimumLength = 3, ErrorMessage = "Invalid")]
        [Required(ErrorMessage = "Please enter Permanent Address")]
        public string ParAddress { get; set; }
        [StringLength(100, MinimumLength = 3, ErrorMessage = "Invalid")]
        public string ParAddressLine1 { get; set; }
        [Required(ErrorMessage = "Please select Permanent Country")]
        public string ParCountry { get; set; }
        public string ParCountryName { get; set; }


        public Nullable<int> ParDivisionId { get; set; }
        [Required(ErrorMessage = "Please enter parmanent city")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "Invalid")]
        public string ParCity { get; set; }
        [StringLength(100, MinimumLength = 3, ErrorMessage = "Invalid")]
        public string ParArea { get; set; }
        public string ParPotalCode { get; set; }
        public bool SamePresentAddress { get; set; }
        [StringLength(100, MinimumLength = 3, ErrorMessage = "Invalid")]
        public string PreAddress { get; set; }
        [StringLength(100, MinimumLength = 3, ErrorMessage = "Invalid")]
        public string PreAddressLine1 { get; set; }
        public string PreCountry { get; set; }
        public string PreCountryName { get; set; }

        public Nullable<int> PreDivisionId { get; set; }
        public string ParState { get; set; }
        public string PreState { get; set; }
        [StringLength(100, MinimumLength = 3, ErrorMessage = "Invalid")]
        public string PreCity { get; set; }
        [StringLength(100, MinimumLength = 3, ErrorMessage = "Invalid")]
        public string PreArea { get; set; }
        public string PrePostalCode { get; set; }
        public int Status { get; set; }
        [Required(ErrorMessage = "Please enter joining date")]
        public Nullable<System.DateTime> JoinDate { get; set; }
        public System.DateTime CreatedDate { get; set; }
        public long CreatedBy { get; set; }
        public Nullable<System.DateTime> UpdatedDate { get; set; }
        public Nullable<long> UpdatedBy { get; set; }


        //only for model view
        public string UserName { get; set; }
        public string TitleName { get; set; }
        public string Designation { get; set; }
        public string UserTypeName { get; set; }
        public string PermanentDivisionName { get; set; }
        public string PresentDivisionName { get; set; }
        public string GenderName { get; set; }
        public string ReligionName { get; set; }
        public string JoinDateName { get; set; }
        public string SiteIdName { get; set; }
        public string DeptIdName { get; set; }
        public string LineIdName { get; set; }
        public string MachineIdName { get; set; }
        public string UserTypeChange { get; set; }
        public string UserId_Type { get; set; }
        public string UnitIdName { get; set; }
        public string NationalityName { get; set; }


        //For Image Area Selection 

        public string x1 { get; set; }
        public string y1 { get; set; }
        public string x2 { get; set; }
        public string y2 { get; set; }
        public string width { get; set; }
        public string height { get; set; }


        //For Salary 
        public bool AssginSalary { get; set; }
        public Nullable<int> SalaryPackageId { get; set; }


        //For Loan 

        public bool HasLoanRecord { get; set; }
        public bool HasDueLoan { get; set; }


        public string NationalIdBackImg { get; set; }
        public string NationalIdFontImg { get; set; }
        public bool isDisplay { get; set; }


        //************** For Holiday Package ****************************
        public Nullable<System.DateTime> HolidayAssignedDate { get; set; }
        public Nullable<long> HolidayAssignedBy { get; set; }
        public Nullable<int> NoOfPaidLeave { get; set; }
        public Nullable<bool> IsCustomePack { get; set; }
        public Nullable<int> HolidayPackId { get; set; }


        //********************* FOR WORKING SCHEDULE *********************

        public Nullable<System.DateTime> WorkScheduleAssignedDate { get; set; }
        public Nullable<long> WorkScheduleAssignedBy { get; set; }
        public int WorkingScheduleId { get; set; }
        public string AllFilename { get; set; }
    }
}