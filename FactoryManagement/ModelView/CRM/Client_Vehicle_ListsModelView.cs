using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace FactoryManagement.ModelView.CRM
{
    public class Client_Vehicle_ListsModelView
    {
        public int VehicleId { get; set; }
        public int ClientId { get; set; }
      
        [Required(ErrorMessage = "Please enter vehicle name")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Invalid")]
        public string VehicleName { get; set; }
        [Required(ErrorMessage = "Please enter register no")]
        [StringLength(40, MinimumLength = 2, ErrorMessage = "Invalid")]
        public string RegisterNo { get; set; }
      
        [StringLength(400, MinimumLength = 1, ErrorMessage = "Invalid")]
        public string Description { get; set; }
        [Required(ErrorMessage = "Please enter address")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Invalid")]
        public string Address { get; set; }
        public string AddressLine1 { get; set; }
        [Required(ErrorMessage = "Please enter country")]
       
        public string Country { get; set; }
        public string State { get; set; }
        public Nullable<int> DivisionId { get; set; }
        [Required(ErrorMessage = "Please enter city")]
        [StringLength(80, MinimumLength = 2, ErrorMessage = "Invalid")]
        public string City { get; set; }
        [Required(ErrorMessage = "Please enter postal code")]
        [StringLength(80, MinimumLength = 2, ErrorMessage = "Invalid")]
        public string PostalCode { get; set; }
        public string PictureName { get; set; }
        public System.DateTime CreatedDate { get; set; }
        public long CreatedBy { get; set; }
        public Nullable<System.DateTime> UpdatedDate { get; set; }
        public Nullable<long> UpdatedBy { get; set; }

        public string CountryName { get; set; }
        public string DivisionName { get; set; }
        public string PictureOriginalName { get; set; }
    }
}