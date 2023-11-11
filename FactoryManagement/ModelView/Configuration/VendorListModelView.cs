using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace FactoryManagement.ModelView.Configuration
{
    public class VendorListModelView
    {
        public int Id { get; set; }
        [Display(Name="Vendor Name")]
        [Required(ErrorMessage="Please enter vendor name")]
        public string Name { get; set; }
        [Display(Name = "Vendor Code")]
        [Required(ErrorMessage = "Please enter vendor code")]
        public string VendorCode { get; set; }
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
        public string Mobile { get; set; }
        [Required(ErrorMessage = "Please enter email address")]
        [RegularExpression(@"\w+([-+.']\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*", ErrorMessage = "Invalid email address")]
        public string Email { get; set; }
        public string Comments { get; set; }
        public string Website { get; set; }
        public System.DateTime CreatedDate { get; set; }
        public long CreatedBy { get; set; }
        public Nullable<System.DateTime> UpdatedDate { get; set; }
        public Nullable<long> UpdatedBy { get; set; }

        public string AllContactList { get; set; }
        public string AllContactComments { get; set; }
    }
}