﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace FactoryManagement.ModelView.Inventory.Product
{
    public class UnitListViewModel
    {
        public int UnitId { get; set; }
        [Required(ErrorMessage = "Please enter unit name")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Type atleast two character")]
        [Remote("UnitNameExist", "RemoteValidation", AdditionalFields = "UnitId", ErrorMessage = "Unit name already exists")]
        public string UnitName { get; set; }
        public bool HasParent { get; set; }
        public Nullable<int> ParentId { get; set; }
        public System.DateTime CreatedDate { get; set; }
        public long CreatedBy { get; set; }
        public Nullable<System.DateTime> UpdatedDate { get; set; }
        public Nullable<long> UpdatedBy { get; set; }
        public string UserName { get; set; }
        public string HasChild { get; set; }
        public string CreatedDateName { get; set; }
        public string Picture { get; set; }
        public string ParentName { get; set; }
    }
}
