using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace FactoryManagement.ModelView.Factory
{
    public class FactoryInfoModelView
    {
        public int Id { get; set; }

        [Display(Name = "Factory Name")]
        [Required(ErrorMessage = "Please enter factory name")]
        [StringLength(50, MinimumLength = 5, ErrorMessage = "Invalid")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Please enter owner name")]
        [Display(Name = "Owner Name")]
        [StringLength(20, MinimumLength = 2, ErrorMessage = "Invalid")]
        public string OwnerName { get; set; }
        public string OwnerPicture { get; set; }
        public string PhoneNumber { get; set; }
        public string MobileNumber { get; set; }
        public string Fax { get; set; }
        public string Email { get; set; }
        public string FullAddress { get; set; }

        [Required(ErrorMessage = "Please enter address")]
        [StringLength(100, MinimumLength = 5, ErrorMessage = "Invalid")]
        public string Address { get; set; }
        [Display(Name = "Address Line1")]
        [StringLength(100, MinimumLength = 5, ErrorMessage = "Invalid")]
        public string AddressLine1 { get; set; }
        [Required(ErrorMessage = "Please enter country name")]
        [StringLength(20, MinimumLength = 3, ErrorMessage = "Invalid")]
        public string Country { get; set; }
        [Display(Name = "Division")]
        [Required(ErrorMessage = "Please select division")]
        public int DivisionId { get; set; }
        public string City { get; set; }
        public string Area { get; set; }
        [Display(Name = "Postal Code")]
        [Required(ErrorMessage = "Please enter postal code")]
        [StringLength(20, MinimumLength = 3, ErrorMessage = "Invalid")]
        public string PostalCode { get; set; }
        [Display(Name = "Currency")]
        [Required(ErrorMessage = "Please select Currency")]
        public int CurrencyId { get; set; }
        public string CurrencyName { get; set; }
        public string FactoryIcon { get; set; }
    }
}