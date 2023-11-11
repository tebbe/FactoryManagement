using FactoryManagement.Models;
using Quartz;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Threading.Tasks;

namespace FactoryManagement
{
    public class Inventory_Finished_Pro_Rep : IJob
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
            string time = now.ToString("dd-mm-yy");
            var path = String.Format("{0}XMLfile", AppDomain.CurrentDomain.BaseDirectory);
            var fact_title = db.Factory_Finished_Pro_Title.ToList();
            foreach (var title in fact_title)
            {
                string file_name = time + "_" + title.Id + "_Finished_Report.xml";
                var allPro = db.Finished_Product_List.Where(m => m.TitleId == title.Id).ToList();

                StringWriter stringwriter = new StringWriter();
                XmlTextWriter xmlTextWriter = new XmlTextWriter(stringwriter);
                xmlTextWriter.Formatting = Formatting.Indented;
                xmlTextWriter.WriteStartDocument();
                xmlTextWriter.WriteStartElement("products");
                int count = 1;

                foreach (var i in allPro)
                {
                    xmlTextWriter.WriteStartElement("product");
                    xmlTextWriter.WriteElementString("SL", count.ToString());
                    xmlTextWriter.WriteElementString("Count", i.Count_Name.ToString());
                    xmlTextWriter.WriteElementString("MC", i.M_C_Qty.ToString());
                    xmlTextWriter.WriteElementString("Lot", i.Lot.ToString());
                    xmlTextWriter.WriteElementString("LotDate", i.Lot_StartedDate.ToString("dd-mm-yy"));
                    xmlTextWriter.WriteElementString("Bag", i.Bag.ToString());
                    xmlTextWriter.WriteElementString("KG", i.KG);
                    xmlTextWriter.WriteElementString("Unit", db.UnitLists.Where(m => m.UnitId == i.UnitId).Select(m => m.UnitName).FirstOrDefault());
                    xmlTextWriter.WriteElementString("Remark", i.Remark);
                    xmlTextWriter.WriteEndElement();
                    count++;
                }
                xmlTextWriter.WriteEndElement();
                xmlTextWriter.WriteEndDocument();
                XmlDocument docSave = new XmlDocument();
                docSave.LoadXml(stringwriter.ToString());
                docSave.Save(@"" + path + "\\" + file_name);

                //************** Save File Name Into Database *************************
                Finished_Pro_Report_List report = new Finished_Pro_Report_List();
                report.TitleId = title.Id;
                report.Date = now;
                report.ReportName = file_name;
                report.CreatedDate = now;
                db.Finished_Pro_Report_List.Add(report);
                db.SaveChanges();
            }



            //************** Create Daily Raw Cotton Lists *************************
            var path2 = String.Format("{0}DailyRawCottonXML", AppDomain.CurrentDomain.BaseDirectory);
            string raw_file_name = time + "_RawCottonReport.xml";

            DateTime today = DateTime.Today;
            int loop_count = 0; int sl_count = 0;

            var u_list = db.Site_Unit_Lists.Where(m => m.UnitStatus == 1).ToList();
            var AllList = db.View_Dispacth_Item_Details.Where(m => m.DispatchedDate > today).ToList();
            var list = db.InventoryLists.Where(m => m.ProductTypeId == 6).ToList();
            var country_code = (from bs in list
                                group bs by bs.Country into g
                                select new FactoryManagement.Models.CountryList
                                {
                                    CountryCode = g.FirstOrDefault().Country
                                }).ToList();

            double grnd_ttl = 0, grnd_imp = 0, grnd_stock = 0, grnd_issue = 0;

            StringWriter stringwriter2 = new StringWriter();
            XmlTextWriter xmlTextWriter2 = new XmlTextWriter(stringwriter2);
            xmlTextWriter2.Formatting = Formatting.Indented;
            xmlTextWriter2.WriteStartDocument();

            xmlTextWriter2.WriteStartElement("items");
            int count2 = 1;

            foreach (var country in country_code)
            {

                int id = Convert.ToInt32(country.CountryCode);
                string countryname = db.CountryLists.Where(c => c.Id == id).Select(c => c.CountryName).FirstOrDefault();
                loop_count++;
                var Pro_list = db.View_ProList_WithLC
                               .Where(m => m.ProductTypeId == 6 && m.Country == country.CountryCode)
                               .ToList();
                Pro_list = (from bs in Pro_list
                            group bs by new { bs.InventoryId, bs.ImportId } into g
                            select new FactoryManagement.Models.View_ProList_WithLC
                            {
                                InventoryId = g.FirstOrDefault().InventoryId,
                                ProductName = g.FirstOrDefault().ProductName,
                                ImportId = g.FirstOrDefault().ImportId,
                                LC = g.FirstOrDefault().LC,
                                Quantity = g.Sum(x => x.Quantity),
                                Description = g.FirstOrDefault().Description
                            }).OrderBy(o => o.ProductName).ToList();

                double total_o_blnc = 0;
                double total_imp = 0;
                double total_ise = 0;

                if (Pro_list.Count() > 0)
                {
                    xmlTextWriter2.WriteStartElement("country");
                    foreach (var item in Pro_list)
                    {
                        sl_count++;
                        double opening_blc = 0, stock = 0, total_stock = 0, total_issue = 0, imp_quan = 0;
                        var locId = db.Inventory_Item_Location.Where(m => m.InventoryId == item.InventoryId && m.ImportId == item.ImportId).Select(m => m.LocationId).ToList();
                        var imp_quan_list = (from bs in db.View_Import_Item_Details
                                             where bs.InventoryId == item.InventoryId && bs.ImportId == item.ImportId
                                             && System.Data.Entity.DbFunctions.TruncateTime(bs.InsertedDate) == today.Date
                                             group bs by new { bs.InventoryId, bs.ImportId } into g
                                             select new
                                             {
                                                 Quantity = g.Sum(x => x.Quantity),
                                             }).FirstOrDefault();
                        if (imp_quan_list != null)
                        {
                            imp_quan = imp_quan_list.Quantity;
                        }
                        double today_total_dis = AllList.Where(m => m.InventoryId == item.InventoryId && locId.Contains(m.LocationId)).Sum(m => m.Quantity);
                        opening_blc = item.Quantity - Convert.ToDouble(imp_quan);
                        if (today_total_dis > 0)
                        {
                            opening_blc = opening_blc + Convert.ToDouble(today_total_dis);
                        }
                        stock = opening_blc + Convert.ToDouble(imp_quan);
                        total_o_blnc = total_o_blnc + opening_blc;
                        total_imp = total_imp + Convert.ToDouble(imp_quan);
                        grnd_ttl = grnd_ttl + opening_blc;
                        grnd_imp = grnd_imp + imp_quan;
                        grnd_stock = grnd_stock + stock;

                        xmlTextWriter2.WriteStartElement("item");
                        xmlTextWriter2.WriteElementString("SL", sl_count.ToString());
                        xmlTextWriter2.WriteElementString("Product", item.ProductName.ToString());
                        xmlTextWriter2.WriteElementString("Country", countryname.ToString());
                        xmlTextWriter2.WriteElementString("Staple", (item.Description == null) ? "" : item.Description.ToString());
                        xmlTextWriter2.WriteElementString("LC", item.LC.ToString());
                        xmlTextWriter2.WriteElementString("OBalance", opening_blc.ToString());
                        xmlTextWriter2.WriteElementString("RBalance", imp_quan.ToString());
                        xmlTextWriter2.WriteElementString("Stock", stock.ToString());
                        int u_count = 0;
                        foreach (var u in u_list)
                        {
                            u_count++;
                            var u_dis_list = (from bs in AllList
                                              where bs.InventoryId == item.InventoryId
                                              && bs.Site_Unit_Id == u.UnitId && locId.Contains(bs.LocationId)
                                              group bs by new { bs.Site_Unit_Id } into g
                                              select new
                                              {
                                                  Quantity = g.Sum(x => x.Quantity)
                                              }).FirstOrDefault();
                            string str_quan = "";
                            double u_quan = 0;
                            if (u_dis_list != null)
                            {
                                u_quan = u_dis_list.Quantity;
                                total_issue = total_issue + u_quan;
                                total_ise = total_ise + u_quan;
                                grnd_issue = grnd_issue + u_quan;
                                str_quan = (u_dis_list.Quantity > 0) ? u_dis_list.Quantity.ToString() : "";
                            }
                            xmlTextWriter2.WriteElementString("u_" + u_count, u.UnitAcryonm.ToString() + "|" + str_quan.ToString());
                        }
                        xmlTextWriter2.WriteElementString("TotalIssue", item.Quantity.ToString());
                        xmlTextWriter2.WriteElementString("Closing", item.Quantity.ToString());
                        xmlTextWriter2.WriteElementString("Remarks", item.Quantity.ToString());
                        xmlTextWriter2.WriteEndElement(); //end of item
                        count2++;
                    }

                    xmlTextWriter2.WriteStartElement("Total");
                    xmlTextWriter2.WriteElementString("total_o_blnc", total_o_blnc.ToString());
                    xmlTextWriter2.WriteElementString("total_imp", total_imp.ToString());
                    xmlTextWriter2.WriteElementString("total_stock", total_o_blnc.ToString());
                    int u_count2 = 0;
                    foreach (var u in u_list)
                    {
                        u_count2++;
                        xmlTextWriter2.WriteElementString("TotalU_" + u_count2, total_ise.ToString());
                    }
                    xmlTextWriter2.WriteElementString("total_ise", total_ise.ToString());
                    xmlTextWriter2.WriteElementString("total_closing", (total_o_blnc - total_ise).ToString());
                    xmlTextWriter2.WriteEndElement();//end of total

                    xmlTextWriter2.WriteEndElement(); //end of Country
                }
            }

            //************** FOR GRAND TOTAL **************************
            xmlTextWriter2.WriteStartElement("GrandTotal");
            xmlTextWriter2.WriteElementString("GrandTotal", grnd_ttl.ToString());
            xmlTextWriter2.WriteElementString("GrandImp", grnd_imp.ToString());
            xmlTextWriter2.WriteElementString("GrandStock", grnd_stock.ToString());
            int u_count3 = 0;
            foreach (var u in u_list)
            {
                u_count3++;
                xmlTextWriter2.WriteElementString("GrandU_" + u_count3, "0");
            }
            xmlTextWriter2.WriteElementString("GrandIssue", grnd_issue.ToString());
            xmlTextWriter2.WriteElementString("GrandClosing", (grnd_stock - grnd_issue).ToString());
            xmlTextWriter2.WriteEndElement();//end of GrandTotal

            xmlTextWriter2.WriteEndElement(); //end of items

            XmlDocument docSave2 = new XmlDocument();
            docSave2.LoadXml(stringwriter2.ToString());
            docSave2.Save(@"" + path2 + "\\" + raw_file_name);

            //************** Save File Name Into Database *************************
            Daily_Raw_Cotton_Report_List rawCotton = new Daily_Raw_Cotton_Report_List();
            rawCotton.Date = now;
            rawCotton.ReportName = raw_file_name;
            rawCotton.CreatedDate = now;
            db.Daily_Raw_Cotton_Report_List.Add(rawCotton);
            db.SaveChanges();

            return Task.CompletedTask;
        }
         
    }
}
