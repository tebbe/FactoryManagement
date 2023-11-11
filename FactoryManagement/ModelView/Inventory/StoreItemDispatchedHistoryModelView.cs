using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FactoryManagement.ModelView.Inventory
{
    public class StoreItemDispatchedHistoryModelView
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string VoucherName { get; set; }
        public long DispatchedBy { get; set; }
        public System.DateTime DispatchedDate { get; set; }
        public string DispatchUserName { get; set; }
        public string DispatchUserPic { get; set; }
    }
}