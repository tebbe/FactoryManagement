using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FactoryManagement.ModelView.Configuration
{
    public class ActivationCodeModelView
    {
        public int CodeId { get; set; }
        public int AppId { get; set; }
        public string AppNumber { get; set; }
        public string Code { get; set; }
        public bool IsActive { get; set; }
    }
}