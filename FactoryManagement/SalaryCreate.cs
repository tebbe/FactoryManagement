using FactoryManagement.Models;
using Quartz;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using System.Web;


namespace FactoryManagement
{
    public class SalaryCreate : IJob
    {
        private FactoryManagementEntities db = new FactoryManagementEntities();
        public async Task Execute(IJobExecutionContext context)
        {
            try
            {
                await ProcessAsync();
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        private Task ProcessAsync()
        {
            int offset = Convert.ToInt32(ConfigurationManager.AppSettings["localTime"]);
            DateTime now = DateTime.UtcNow.AddMinutes(offset);

            var AllEmpList = db.UserInformations.Where(m => m.Status == 1 && m.SalaryPackageId > 0).ToList();
            foreach (var user in AllEmpList)
            {
                decimal baseSalary = 0;
                decimal total = 0;
                baseSalary = Convert.ToDecimal(db.Salary_Package_Account_Lists.Where(m => m.PackageId == user.SalaryPackageId && m.IsBaseSalary).Select(m => m.Amount).FirstOrDefault());
                var list = db.Salary_Package_Account_Lists.Where(m => m.PackageId == user.SalaryPackageId).ToList();

                EmpSalaryPaymentDetail empSalary = new EmpSalaryPaymentDetail();
                empSalary.UserId = user.UserId;
                empSalary.UserType = user.UserType;
                empSalary.Year = now.Year;
                empSalary.MonthId = now.Month;
                empSalary.Month = now.ToString("MMMM");
                empSalary.TotalAmount = 0;
                empSalary.Status = 0;
                empSalary.GeneratedDate = now;
                db.EmpSalaryPaymentDetails.Add(empSalary);
                db.SaveChanges();
                long PaymentId = db.EmpSalaryPaymentDetails.Where(m => m.UserId == user.UserId && m.UserType == user.UserType).Max(m => m.Id);

                foreach (var li in list)
                {
                    decimal amnt = 0;
                    if (li.AccPayType == 3)
                    {
                        if (li.Percentage == null)
                        {
                            amnt = Convert.ToDecimal(li.Amount);
                        }
                        else
                        {
                            decimal percentage = Convert.ToDecimal(li.Percentage);
                            amnt = baseSalary * (percentage / 100);
                        }
                        if (li.IsAddition)
                        {
                            total = total + amnt;
                        }
                        else
                        {
                            total = total - amnt;
                        }
                    }
                    else
                    {
                        var allmonthId = db.Salary_Acc_Pay_MonthList.Where(m => m.Salary_Pck_Acc_Id == li.Id).Select(m => m.MonthId).ToList();
                        foreach (var month in allmonthId)
                        {
                            if (month == now.Month)
                            {
                                if (li.Percentage == null)
                                {
                                    amnt = Convert.ToDecimal(li.Amount);
                                }
                                else
                                {
                                    decimal percentage = Convert.ToDecimal(li.Percentage);
                                    amnt = baseSalary * (percentage / 100);
                                }
                                if (li.IsAddition)
                                {
                                    total = total + amnt;
                                }
                                else
                                {
                                    total = total - amnt;
                                }
                            }
                        }
                    }
                    SalaryPaymentPckDetail salaryDetails = new SalaryPaymentPckDetail();
                    salaryDetails.PaymentId = Convert.ToInt32(PaymentId);
                    salaryDetails.IsDeduct = li.IsAddition ? false : true;
                    salaryDetails.IsBasic = li.IsBaseSalary;
                    salaryDetails.Acc_Name = li.Name;
                    salaryDetails.Amount = amnt;
                    db.SalaryPaymentPckDetails.Add(salaryDetails);
                    db.SaveChanges();
                }

                EmpSalaryPaymentDetail emp = db.EmpSalaryPaymentDetails.Find(PaymentId);
                emp.TotalAmount = total;
                db.Entry(emp).State = EntityState.Modified;
                db.SaveChanges();
            }

            return Task.CompletedTask;
        }
    }
}