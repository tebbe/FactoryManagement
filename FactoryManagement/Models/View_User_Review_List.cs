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
    
    public partial class View_User_Review_List
    {
        public long Id { get; set; }
        public long UserId { get; set; }
        public string Review { get; set; }
        public bool HasParent { get; set; }
        public System.DateTime ReviewDate { get; set; }
        public Nullable<long> ParentId { get; set; }
        public long ReviewBy { get; set; }
        public string UserName { get; set; }
        public string Picture { get; set; }
        public string CommentTime { get; set; }
        public bool IsUpdated { get; set; }
        public Nullable<int> Likes { get; set; }
        public Nullable<int> UnLikes { get; set; }
        public Nullable<decimal> Rate { get; set; }
    }
}
