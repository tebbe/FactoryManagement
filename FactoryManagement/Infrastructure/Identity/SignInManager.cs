using System;
using System.Security.Claims;
using System.Threading.Tasks;
using FactoryManagement.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.Owin.Security;


namespace FactoryManagement.Infrastructure.Identity
{
    public class SignInManager : Microsoft.AspNet.Identity.Owin.SignInManager<ApplicationUser, String>
    {
        public SignInManager(UserManager userManager, IAuthenticationManager authenticationManager)
            : base(userManager, authenticationManager)
        {
        }
    }
}