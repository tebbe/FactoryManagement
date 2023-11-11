using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace FactoryManagement.ModelView.CRM
{
    public class BusinessOrderListModelView
    {
        public int BusinessOrderId { get; set; }
        [Required(ErrorMessage = "Please enter order name")]
        [StringLength(40, MinimumLength = 2, ErrorMessage = "Invalid")]
        public string OrderName { get; set; }
        //[Required(ErrorMessage = "Please select client")]
        public Nullable<int> ClientId { get; set; }
        public int OrderStatus { get; set; }
        public long CreatedBy { get; set; }
        public System.DateTime CreatedDate { get; set; }
        public Nullable<System.DateTime> UpdatedDate { get; set; }
        public Nullable<long> UpdatedBy { get; set; }


        public string AllFilename { get; set; }
        public string UserName { get; set; }
        public string CreatedByName { get; set; }

        public long FileId { get; set; }
        public int FileType { get; set; }
        public int FileSendBy { get; set; }
        public string FileName { get; set; }
        public string FileOriginalName { get; set; }
        public string FileContentType { get; set; }
        public int Client_User_Id { get; set; }

        public string AllSupplierId { get; set; }
        public string AllAcquisitionOrderId { get; set; }
        public int OrderType { get; set; }

        public string ClientName { get; set; }
        public int AcquisitionOrderId { get; set; }
        public int SupId { get; set; }


        //***************** For Costing ***********************

        public Nullable<decimal> TotalCost { get; set; }
        public Nullable<decimal> TotalProCost { get; set; }
        public Nullable<decimal> TotalOthersCost { get; set; }
        public string SupplierName { get; set; }
        public string HasChild { get; set; }

        public string OrderAprvStatus { get; set; }
    }
}