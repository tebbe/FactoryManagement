﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Claims;
using System.Web.Mvc;
using FactoryManagement.Infrastructure.Helpers;

namespace FactoryManagement.Infrastructure.Identity
{
    public class ClaimedActionsProvider
    {
        public List<ClaimsGroup> GetClaimGroups()
        {
            var claimedGroups = Assembly.GetAssembly(typeof(MvcApplication)).GetTypes()
                .Where(t => !t.IsAbstract && t.IsSubclassOf(typeof(Controller)))
                .Select(c => new ClaimsGroup()
                {
                    GroupName = c.Name,
                    Claims = GetActionClaims(c)
                })
                .ToList();
            return claimedGroups;
        }
        private String GetGroupName(Type controllerType)
        {
            var result = controllerType.GetCustomAttribute<ClaimsGroupAttribute>().Resource.GetDisplayName();
            return result;
        }
        private int GetGroupId(Type controllerType)
        {
            var claimsGroupAttribute = controllerType.GetCustomAttribute<ClaimsGroupAttribute>();
            var result = (int)claimsGroupAttribute.Resource;
            return result;
        }
        private List<String> GetActionClaims(Type controllerType)
        {
            var result = controllerType.GetMethods()
                .Where(m => m.IsDefined(typeof(ClaimsActionAttribute)))
                .SelectMany(m => m.GetCustomAttributes<ClaimsActionAttribute>())
                .Select(a => a.Claim)
                .Distinct()
                .Select(a => a.ToString())
                .ToList();

            return result;
        }
    }


    public class ClaimsGroup
    {
        public ClaimsGroup()
        {
            Claims = new List<string>();
        }

        public String GroupName { get; set; }

        public int GroupId { get; set; }

        public Type ControllerType { get; set; }

        public List<String> Claims { get; set; }

        public List<Claim> GetAllClaims()
        {
            return Claims.Select(c => new Claim(GroupId.ToString(), c)).ToList();
        }
    }
}