using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FactoryManagement.ModelView.CRM
{
    public class BusinessOrderProductListModelView
    {
        public int Id { get; set; }
        public int BusinessOrderId { get; set; }
        public Nullable<int> ProductId { get; set; }
        public Nullable<int> AcquisitionOrderId { get; set; }
        public int Quantity { get; set; }
        public Nullable<decimal> PerPiece { get; set; }
        public Nullable<decimal> TotalPriece { get; set; }
        public int ReceivedStatus { get; set; }
        public Nullable<int> ReceivedQuantity { get; set; }
        public Nullable<int> AssignedQuantity { get; set; }

        // for model view
        public string ProductName { get; set; }
        public string UnitName { get; set; }
    }
}