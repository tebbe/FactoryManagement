using Quartz;
using Quartz.Impl;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;


namespace FactoryManagement
{
    public class JobScheduler
    {
        public static void Start()
        {
            int offset = Convert.ToInt32(ConfigurationManager.AppSettings["localTime"]);
            DateTime now = DateTime.UtcNow.AddMinutes(offset);

            //IScheduler scheduler = (IScheduler)StdSchedulerFactory.GetDefaultScheduler();
            //scheduler.Start();

            //bool lastDay = now.Month != now.AddDays(1).Month;
            //DateTime temp = now;
            //temp = temp.AddDays(1);

            //****************** Everyday At 11.58pm Runs This Schedule To Create Daily Finished Product Report ***********************
            #region
            //IJobDetail Inventory_finished_Quan = JobBuilder.Create<Inventory_Finished_Pro_Rep>().Build();
            //ITrigger trigger_inven_finished_pro = TriggerBuilder.Create()
            //.WithIdentity("trigger1", "group1")
            //.StartNow()
            //.WithDailyTimeIntervalSchedule
            //      (s =>
            //         s.WithIntervalInHours(24)
            //        .OnEveryDay()
            //        .StartingDailyAt(TimeOfDay.HourAndMinuteOfDay(23,55))
            //      )
            //.Build();
            //scheduler.ScheduleJob(Inventory_finished_Quan, trigger_inven_finished_pro);
            #endregion


            //****************** Last Day of Every Month At 11.58pm Runs This Schedule To Create Employee Salary ***********************
            #region
            //IJobDetail Salary_Create = JobBuilder.Create<SalaryCreate>().Build();
            //ITrigger trigger_salary_create = TriggerBuilder.Create()
            //.WithIdentity("trigger2", "group2")
            //.StartNow()
            //.WithDailyTimeIntervalSchedule
            //      (s =>
            //         s.WithIntervalInHours(24)
            //        .OnEveryDay()
            //        .StartingDailyAt(TimeOfDay.HourAndMinuteOfDay(23, 58))
            //      )
            //.Build();
            //if (temp.Day == 1)
            //{
            //    scheduler.ScheduleJob(Salary_Create, trigger_salary_create);
            //}
            #endregion


            //****************** Job Schedule For Inventory Stock Entry For Month End *************************
            #region
            //IJobDetail Inventory_Stock_Quan = JobBuilder.Create<Inventory_stock>().Build();
            //ITrigger trigger_inven_stock_quan = TriggerBuilder.Create()
            //.WithIdentity("trigger4", "group1")
            //.WithSchedule(CronScheduleBuilder.DailyAtHourAndMinute(23, 50))
            //.ForJob(Inventory_Stock_Quan)
            //.Build();

            //ITrigger trigger5 = TriggerBuilder.Create()
            //    .WithDailyTimeIntervalSchedule
            //      (s =>
            //         s.WithIntervalInHours(24)
            //        .OnEveryDay()
            //        .StartingDailyAt(TimeOfDay.HourAndMinuteOfDay(23, 58))
            //      )
            //    .Build();
            //if (temp.Day == 1)
            //{
            //    scheduler.ScheduleJob(Inventory_Stock_Quan, trigger_inven_stock_quan);
            //}
            #endregion

        }
    }
}