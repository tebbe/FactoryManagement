using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using FactoryManagement.Models;
using FactoryManagement.ModelView.Auth;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;

namespace FactoryManagement.Infrastructure.Identity
{
    public class RoleManager : RoleManager<ApplicationRole>
    {
        private readonly FactoryManagementEntities context;


        public RoleManager(FactoryManagementEntities context): base(new RoleStore<ApplicationRole>(context))
        {
            this.context = context;
        }


        public async Task<List<Claim>> GetClaimsAsync(string RoleId,string roleName)
        {
            var roleClaims = await context.AuthRoleClaims
                .Where(rc => rc.RoleId == roleName)
                .ToListAsync();

            var claims = roleClaims
                .Select(rc => new Claim(rc.ClaimType, rc.ClaimValue))
                .ToList();

            return claims;
        }


        public async Task AddClaimAsync(string roleId, Claim claim)
        {
            var roleClaim = new AuthRoleClaim()
            {
                RoleId = roleId,
                ClaimType = claim.Type,
                ClaimValue = claim.Value,
            };

            context.AuthRoleClaims.Add(roleClaim);

            await context.SaveChangesAsync();
        }


        public async Task RemoveClaimAsync(string roleId, Claim claim)
        {
            var toRemoveClaim = await context.AuthRoleClaims
                .FirstOrDefaultAsync(rc => rc.RoleId == roleId && rc.ClaimType == claim.Type && rc.ClaimValue == claim.Value);

            if (toRemoveClaim != null)
            {
                context.AuthRoleClaims.Remove(toRemoveClaim);
                await context.SaveChangesAsync();
            }
        }
    }
}