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
    
    public partial class User_Uploaded_Document
    {
        public long DocumentId { get; set; }
        public long UserId { get; set; }
        public string DocumentName { get; set; }
        public string DocumentOrginalName { get; set; }
        public string ContentType { get; set; }
        public long ContentSize { get; set; }
        public System.DateTime UploadedDate { get; set; }
        public long UploadedBy { get; set; }
    }
}
