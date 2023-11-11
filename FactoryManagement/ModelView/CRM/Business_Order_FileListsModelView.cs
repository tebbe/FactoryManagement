using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FactoryManagement.ModelView.CRM
{
    public class Business_Order_FileListsModelView
    {
        public long FileId { get; set; }
        public int BusinessOrderId { get; set; }
        public int FileType { get; set; }
        public int FileSendBy { get; set; }
        public string FileName { get; set; }
        public string FileOriginalName { get; set; }
        public string FileContentType { get; set; }
        public Nullable<int> Client_User_Id { get; set; }
        public string Client_User_Name { get; set; }
        public bool IsMainFile { get; set; }
        public System.DateTime CreatedDate { get; set; }
        public long CreatedBy { get; set; }
        public Nullable<System.DateTime> UpatedDate { get; set; }
        public Nullable<long> UpatedBy { get; set; }

        public int ClientId { get; set; }
        public string AllFilename { get; set; }
    }
}