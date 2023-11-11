using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace FactoryManagement.ModelView.Configuration
{
    public class SiteListModelView
    {
        public int Id { get; set; }

        [Display(Name = "Site Name")]
        [Required(ErrorMessage = "Please enter site name")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "Invalid")]
        [Remote("SiteNameExist", "RemoteValidation", AdditionalFields = "Id", HttpMethod = "Post", ErrorMessage = "Site name already exists..!!!")]
        public string SiteName { get; set; }

        [Display(Name = "Site Acronym")]
        [StringLength(10, MinimumLength = 2, ErrorMessage = "Invalid")]
        public string SiteAcronym { get; set; }

        [Required(ErrorMessage = "Please enter address")]
        [StringLength(100, MinimumLength = 5, ErrorMessage = "Invalid")]
        public string Address { get; set; }
        [Display(Name = "Address Line1")]
        [StringLength(100, MinimumLength = 5, ErrorMessage = "Invalid")]
        public string AddressLine1 { get; set; }


        [Required(ErrorMessage = "Please enter country name")]
        public string Country { get; set; }

        [Display(Name = "State Name")]
        public string Statename { get; set; }
        [Display(Name = "Division")]
        public Nullable<int> DivisionId { get; set; }
        public string City { get; set; }
        public string Area { get; set; }
        [Display(Name = "Postal Code")]
        [Required(ErrorMessage = "Please enter postal code")]
        [StringLength(20, MinimumLength = 3, ErrorMessage = "Invalid")]
        public string PostalCode { get; set; }
        public long CreatedBy { get; set; }
        public System.DateTime CreatedDate { get; set; }
        public Nullable<System.DateTime> UpdatedDate { get; set; }
        public Nullable<long> UpdatedBy { get; set; }
        public int Status { get; set; }
        public bool IsComposite { get; set; }
        public Nullable<int> UnitNumber { get; set; }
        public bool IsDisplay { get; set; }

        //// For Define Unit /////
        public int SiteId { get; set; }
        public int UnitId { get; set; }
        [Remote("SiteUnitNameExist", "RemoteValidation", AdditionalFields = "UnitId", HttpMethod = "Post", ErrorMessage = "Unit name already exists..!!!")]
        public string UnitName { get; set; }
        [Remote("SiteUnitAcryonmExist", "RemoteValidation", AdditionalFields = "UnitId", HttpMethod = "Post", ErrorMessage = "Unit acryonm already exists..!!!")]
        public string UnitAcryonm { get; set; }
        public Nullable<int> UnitStatus { get; set; }
        public Nullable<int> UnitCount { get; set; }
        public string[] AllUnitDetails { get; set; }
        public string[] AllUnitAcryonm { get; set; }
        public bool IsResidential { get; set; }
    }
}