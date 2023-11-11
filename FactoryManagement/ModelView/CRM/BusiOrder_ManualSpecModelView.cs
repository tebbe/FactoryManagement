using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FactoryManagement.ModelView.CRM
{
    public class BusiOrder_ManualSpecModelView
    {
        public int Id { get; set; }
        public int BusOrdId { get; set; }
        public string Spec_lbl { get; set; }
        public int Quantity { get; set; }
        public int UnitId { get; set; }
        public string Spec_Description { get; set; }
        public long CreatedBy { get; set; }
        public System.DateTime CreatedDate { get; set; }
        public int ProStatus { get; set; }
        public string UnitName { get; set; }
        public Nullable<bool> IsStoreIntoStock { get; set; }
    }
}