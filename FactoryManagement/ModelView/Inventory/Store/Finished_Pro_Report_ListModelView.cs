using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FactoryManagement.ModelView.Inventory.Store
{
    public class Finished_Pro_Report_ListModelView
    {
        public long Id { get; set; }
        public int TitleId { get; set; }
        public string Title { get; set; }
        public string SubTitle { get; set; }
        public long TitleCreatedBy { get; set; }
        public string TitleCreatedDate { get; set; }
        public Nullable<long> TitleUpdatedBy { get; set; }
        public string TitleUpdatedDate { get; set; }
        public System.DateTime Date { get; set; }
        public string ReportName { get; set; }
        public System.DateTime CreatedDate { get; set; }
        public string CreatedDateFormate { get; set; }
    }
}