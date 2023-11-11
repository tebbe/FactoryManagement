using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace FactoryManagement.ModelView.Inventory
{
    public class MachineTypeModelView
    {
        public int Id { get; set; }
        [Display(Name = "Machine Type")]
        [Required(ErrorMessage = "Please select machine type")]
        public int MachineType { get; set; }
       // [Required(ErrorMessage="Please enter machine name")]
        public string Name { get; set; }
        [Required(ErrorMessage = "Please enter machine acronym")]
        [Display(Name = "Acronym")]
        public string Acroynm { get; set; }
        [Display(Name="Model Name")]
       // [Required(ErrorMessage = "Please enter model name")]
        public string ModelName { get; set; }
        [Display(Name = "Brand Name")]
        [Required(ErrorMessage = "Please select brand name")]
        public int BrandId { get; set; }
        public string BrandName { get; set; }

        [Display(Name = "Country Of Origin")]
        [Required(ErrorMessage = "Please select country")]
        public string CountryOfOrigin { get; set; }
        public string Mgf { get; set; }

        public int Quantity { get; set; }
        public System.DateTime CreatedDate { get; set; }
        public long CreatedBy { get; set; }
        public System.DateTime UpdatedDate { get; set; }
        public long UpdatedBy { get; set; }
        public string AllSpareParts { get; set; }

        public string AllPerishableId { get; set; }
        public string AllRepDate { get; set; }
        public int AllPartsSame { get; set; }


        public string MachineAcronym { get; set; }
        public string MachineTypeName { get; set; }
        public string CountryName { get; set; }
    }
}