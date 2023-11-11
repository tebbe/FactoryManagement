using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FactoryManagement.ModelView.HR
{
    public class Leave_Application_ListModelView
    {
        public long LeaveId { get; set; }
        public long ApplicationUserId { get; set; }
        public string ApplicationUserName { get; set; }
        public string ApplicationUserPic { get; set; }

        public System.DateTime StartDate { get; set; }
        public System.DateTime EndDate { get; set; }
        public int ApproveStatus { get; set; }
        
        public Nullable<System.DateTime> ApprovedDate { get; set; }
        public Nullable<long> ApprovedBy { get; set; }
        public string ApprovedUserName { get; set; }
        public string ApprovedUserPic { get; set; }

        public string Reason { get; set; }
        public Nullable<System.DateTime> AppliedDate { get; set; }

        public int SiteId { get; set; }
        public string SiteName { get; set; }


        public int DesignationId { get; set; }
        public string DesignationName { get; set; }
    }
}