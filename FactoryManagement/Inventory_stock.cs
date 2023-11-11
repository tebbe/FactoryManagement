using FactoryManagement.Models;
using Quartz;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web;


namespace FactoryManagement
{
    public class Inventory_stock : IJob
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

            var all_cotton_list = db.InventoryLists.Where(m => m.ProductTypeId == 6).Select(m => m.InventoryId).ToList();
            var all_Inven = db.View_All_InventoryList.Where(m => all_cotton_list.Contains(m.InventoryId)).ToList();

            foreach (var item in all_Inven)
            {
                if (item.Quantity > 0)
                {
                    Inventory_Monthly_Stock stock = new Inventory_Monthly_Stock();
                    stock.InventoryId = item.InventoryId;
                    stock.LocationId = 0;
                    stock.Quantity = Convert.ToDouble(item.Quantity);
                    stock.MonthId = now.Month;
                    stock.MonthName = now.ToString("MMM");
                    stock.Year = now.Year;
                    stock.DataInsertedDate = now;
                    db.Inventory_Monthly_Stock.Add(stock);
                    try
                    {
                        db.SaveChanges();
                    }
                    catch (Exception)
                    {
                        throw;
                    }
                }
            }

            return Task.CompletedTask;
        }

    }
}