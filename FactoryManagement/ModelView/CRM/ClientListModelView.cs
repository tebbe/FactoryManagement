using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace FactoryManagement.ModelView.CRM
{
    public class ClientListModelView
    {
        public int ClientId { get; set; }
        
        [Display(Name = "Client Name")]
        [Required(ErrorMessage = "Please enter client name")]
        [Remote("ClientNameExist", "RemoteValidation", AdditionalFields = "ClientId", HttpMethod = "Post", ErrorMessage = "Client name already exsits")]
        [StringLength(40, MinimumLength = 2, ErrorMessage = "Invalid")]
        public string Name { get; set; }
        
        [Display(Name = "Client Code")]
        public string ClientCode { get; set; }
        public bool IsBuyer { get; set; }
        public bool IsSupplier { get; set; }
        public bool IsOutSourcer { get; set; }
        public bool IsLogistic { get; set; }
        public string ClientType { get; set; }
        
        [Required(ErrorMessage = "Please enter address")]
        public string Address { get; set; }
        
        [Display(Name = "Address Line1")]
        public string AddressLine1 { get; set; }
        [Required(ErrorMessage = "Please select country")]
        public string Country { get; set; }
        public string State { get; set; }
        public Nullable<int> DivisionId { get; set; }
        public string City { get; set; }
        [Display(Name = "Postal Code")]
        public string PostalCode { get; set; }
        public string Fax { get; set; }
        public string Website { get; set; }
        public string Mobile { get; set; }
        [Required(ErrorMessage = "Please enter email address")]
        [RegularExpression(@"\w+([-+.']\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*", ErrorMessage = "Invalid email address")]
        public string Email { get; set; }

        
        public System.DateTime CreatedDate { get; set; }
        public long CreatedBy { get; set; }
        public Nullable<System.DateTime> UpdatedDate { get; set; }
        public Nullable<long> UpdatedBy { get; set; }

        public string AllContactList { get; set; }
        public string AllContactComments { get; set; }
       
        ////for model only
        public bool ClientTypeId { get; set; }
        public string ClientTypeName { get; set; }
        public string DivisionName { get; set; }
        public string CountryName { get; set; }
        public bool IsDisplay { get; set; }
        public string CreatedByName { get; set; }
        public string UpdatedByName { get; set; }
        public string UpdatedDateName { get; set; }
        public string CreatedByPicture { get; set; }
        public string UpdatedByPicture { get; set; }
        public string CreatedDateName { get; set; }
        public long UserId { get; set; }
        public int UserType { get; set; }

        ////from Client contact list
        public int CientContactId { get; set; }
        public string Contact { get; set; }
        [Required(ErrorMessage = "Please select contact type")]
        public int ContactTypeId { get; set; }
        public string ContactTypeName { get; set; }
        public string Comments { get; set; }
        public bool isDisplay { get; set; }
    }
}