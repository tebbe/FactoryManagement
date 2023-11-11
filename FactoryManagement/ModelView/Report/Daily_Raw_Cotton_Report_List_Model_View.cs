using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FactoryManagement.ModelView.Report
{
    public class Daily_Raw_Cotton_Report_List_Model_View
    {
        public long Id { get; set; }
        public System.DateTime Date { get; set; }
        public string ReportName { get; set; }
        public System.DateTime CreatedDate { get; set; }
        public string CreatedDateFormat { get; set; }
    }
}