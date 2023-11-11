using System;
using System.ComponentModel.DataAnnotations;

namespace FactoryManagement.Infrastructure.Identity
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class ClaimsGroupAttribute : Attribute
    {
        public ClaimResources Resource { get; private set; }

        public ClaimsGroupAttribute(ClaimResources resource)
        {
            Resource = resource;
        }

        public String GetGroupId()
        {
            return ((int)Resource).ToString();
        }
    }
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class ClaimsActionAttribute : Attribute
    {
        public ClaimsActions Claim { get; private set; }
        public ClaimResources function { get; private set; }
        public ClaimsActionAttribute(ClaimsActions claim)
        {
            Claim = claim;
        }
    }
    public enum ClaimsActions
    {
        Index,
        View,
        Create,
        Edit,
        Delete,
        Detials,
        Info,
        List
    }

    public enum ClaimGroup
    {
        Accounting = 1,
        Acquisition = 2,
        Configuration = 3,
        CRMClient= 4,
        CRM = 5,
        Factory = 6,
        HumanResource = 7,
        Inventory=8,
        Manage = 9,
        SalaryConfig = 10
    }  
    public enum ClaimResources
    {
        Accounting,
        Acquisition,
        Configuration,
        CRMClient,
        CRM,
        Factory,
        HumanResource,
        Inventory,
        Manage,
        SalaryConfig
    }
}