using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace FactoryManagement.ModelView.CRM
{
    public class Client_User_ListsModelView
    {
        public int UserId { get; set; }
        public int ClientId { get; set; }
      
        [Required(ErrorMessage = "Please enter user name")]
        [StringLength(40, MinimumLength = 2, ErrorMessage = "Invalid")]
        public string UserName { get; set; }
        [Required(ErrorMessage = "Please enter designation")]
        [StringLength(200, MinimumLength = 2, ErrorMessage = "Invalid")]
        public string Designation { get; set; }
        [Required(ErrorMessage = "Please enter address")]
        [StringLength(200, MinimumLength = 2, ErrorMessage = "Invalid")]
        public string Address { get; set; }
        public string AddressLine1 { get; set; }
        [Required(ErrorMessage = "Please select country")]
        public string Country { get; set; }
        public string State { get; set; }
        public Nullable<int> DivisionId { get; set; }
        [Required(ErrorMessage = "Please enter city")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Invalid")]
        public string City { get; set; }
        [Required(ErrorMessage = "Please enter postal code")]
        [StringLength(40, MinimumLength = 2, ErrorMessage = "Invalid")]
        public string PostalCode { get; set; }
        public string Email { get; set; }
        public string Fax { get; set; }
        public System.DateTime CreatedDate { get; set; }
        public string PictureName { get; set; }
        public string PictureOriginalName { get; set; }
        public long CreatedBy { get; set; }
        public Nullable<System.DateTime> UpdatedDate { get; set; }
        public Nullable<long> UpdatedBy { get; set; }


        public string AllContactList { get; set; }
        public string CountryName { get; set; }
        public string DivisionName { get; set; }
        //from Contact
        public int Id { get; set; }
        public string Contact { get; set; }
        public int ContactTypeId { get; set; }
        public string ContactTypeName { get; set; }
        public string Comments { get; set; }

    }
}