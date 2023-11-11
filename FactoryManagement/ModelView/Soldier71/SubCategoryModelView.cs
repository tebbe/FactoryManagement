using FactoryManagement.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FactoryManagement.ModelView.Soldier71
{
    public class SubCategoryModelView
    {
        public int CategoryId { get; set; }
        public string CategoryName { get; set; }
        public bool HasParent { get; set; }
        public Nullable<int> ParentId { get; set; }
        public System.DateTime CreatedDate { get; set; }
        public long CreatedBy { get; set; }
        public Nullable<System.DateTime> UpdatedDate { get; set; }
        public Nullable<long> UpdatedBy { get; set; }
        public bool CanBeDeleted { get; set; }
        public List<Category> SubSubCategories { get; set; }
    }
}