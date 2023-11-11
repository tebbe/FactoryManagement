using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FactoryManagement.ModelView.Inventory
{
    public class MachineListModelView
    {
        public int MachineId { get; set; }
        public int MachineTypeId { get; set; }
        public string MachineAcronym { get; set; }
        public int Status { get; set; }
        public bool AssignStatus { get; set; }
        public System.DateTime CreatedDate { get; set; }
        public long CreatedBy { get; set; }
        public Nullable<System.DateTime> UpdatedDate { get; set; }
        public Nullable<long> UpdatedBy { get; set; }

        public int Quantity { get; set; }
        public Nullable<int> Pre_Quantity { get; set; }
    }
}