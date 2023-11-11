using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace FactoryManagement.ModelView.Auth
{
    public class UserLoginModelView
    {
        public int Id { get; set; }
        public long UserId { get; set; }
        [Required(ErrorMessage = "Please enter user name")]
        public string UserName { get; set; }
        [Required(ErrorMessage = "Please enter password")]
        public string Password { get; set; }
        public System.DateTime Entrydate { get; set; }
        public long EntryBy { get; set; }
        [Display(Name = "Remember me?")]
        public bool RememberMe { get; set; }
    }
}