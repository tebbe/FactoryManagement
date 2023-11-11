using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace FactoryManagement.ModelView.Accounting
{
    public class BankListModelView
    {
        public int BankId { get; set; }
        [Required(ErrorMessage = "Please enter bank name")]
        [Remote("BankNameExist", "RemoteValidation", AdditionalFields = "BankId", HttpMethod = "Post")]
        [StringLength(40, MinimumLength = 2, ErrorMessage = "Invalid")]
        public string BankName { get; set; }
        public int Status { get; set; }
        public System.DateTime CreratedDate { get; set; }
        public long CreatedBy { get; set; }
        public Nullable<long> UpdatedBy { get; set; }
        public Nullable<System.DateTime> UpdatedDate { get; set; }

        // Only for Model View
        public string CreatedByName { get; set; }
        public string UpdatedByName { get; set; }
        public string CreatedDateName { get; set; }
        public string CreatedByPicture { get; set; }
        public string UpdatedDateName { get; set; }
        public string UpdatedByPicture { get; set; }
        public int ForDisplay { get; set; }
    }
}