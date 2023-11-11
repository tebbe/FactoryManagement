using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FactoryManagement.ModelView.Inventory.Mahinaries
{
    public class View_Machine_ListsModelView
    {
        public int MachineId { get; set; }
        public Nullable<int> MachineTypeId { get; set; }
        public string MachineAcronym { get; set; }
        public int Status { get; set; }
        public bool AssignStatus { get; set; }
        public System.DateTime CreatedDate { get; set; }
        public long CreatedBy { get; set; }
        public Nullable<System.DateTime> UpdatedDate { get; set; }
        public Nullable<long> UpdatedBy { get; set; }
        public int Id { get; set; }
        public int MachineType { get; set; }
        public string Name { get; set; }
        public string ModelName { get; set; }
        public string Acroynm { get; set; }
        public string Brand { get; set; }
        public string CountryOfOrigin { get; set; }
        public int Quantity { get; set; }
        public System.DateTime MachinesCreatedDate { get; set; }
        public long MachinesCreatedBy { get; set; }
        public Nullable<System.DateTime> MachinesUpdateDate { get; set; }
        public Nullable<long> MachinesUpdateBy { get; set; }
        public string CountryName { get; set; }
        public string MachineTypeName { get; set; }
        public string UserName { get; set; }
        public string PictureName { get; set; }
        public string CreatedDateName { get; set; }
        public int LineId { get; set; }
        public string BrandName { get; set; }
        public string Mgf { get; set; }
    }
}