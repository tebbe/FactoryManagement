//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace FactoryManagement.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class View_BusinessOrder_File
    {
        public long FileId { get; set; }
        public int BusinessOrderId { get; set; }
        public int FileType { get; set; }
        public int FileSendBy { get; set; }
        public string FileSend { get; set; }
        public string FileName { get; set; }
        public string FileOriginalName { get; set; }
        public string FileContentType { get; set; }
        public bool IsMainFile { get; set; }
        public System.DateTime CreatedDate { get; set; }
        public Nullable<int> Client_User_Id { get; set; }
        public string ClientUserName { get; set; }
        public string ClientUserImage { get; set; }
        public string ClientName { get; set; }
        public string CreatedDateName { get; set; }
        public long CreatedBy { get; set; }
        public Nullable<System.DateTime> UpatedDate { get; set; }
        public Nullable<long> UpatedBy { get; set; }
        public string UserName { get; set; }
        public string PictureName { get; set; }
    }
}
