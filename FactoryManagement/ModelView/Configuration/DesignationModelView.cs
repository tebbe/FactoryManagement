using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace FactoryManagement.ModelView.Configuration
{
    public class DesignationModelView
    {
        public int DesignationId { get; set; }
        [Required(ErrorMessage="Please enter designation name")]
        [Display(Name="Designation")]
        [Remote("DesignationNameExist", "RemoteValidation", AdditionalFields = "DesignationId", HttpMethod = "Post", ErrorMessage = "Designation name already exsits")]
        public string DesignationName { get; set; }
        public int Status { get; set; }
        public System.DateTime CreatedDate { get; set; }
        public long CreatedBy { get; set; }
        public Nullable<System.DateTime> UpdatedDate { get; set; }
        public Nullable<long> UpdatedBy { get; set; }
    }
}