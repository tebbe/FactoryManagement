using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FactoryManagement.ModelView.Inventory
{
    public class MachineModelView
    {
        public int Id { get; set; }
        public int MachineType { get; set; }
        public string Name { get; set; }
        public string Acroynm { get; set; }
        public string ModelName { get; set; }
        public int BrandId { get; set; }
        public string Mgf { get; set; }
        public string CountryOfOrigin { get; set; }
        public int Quantity { get; set; }
        public System.DateTime CreatedDate { get; set; }
        public long CreatedBy { get; set; }
        public Nullable<System.DateTime> UpdatedDate { get; set; }
        public Nullable<long> UpdatedBy { get; set; }
    }
}