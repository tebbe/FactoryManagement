using FactoryManagement.Models;
using FactoryManagement.ModelView.Configuration;
using FactoryManagement.ModelView.HR;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace FactoryManagement.Controllers
{
    [RoutePrefix("Api/BCRApi")]
    public class BCRApiController : ApiController
    {
        #region Private Properties
        static private int offset = Convert.ToInt32(ConfigurationManager.AppSettings["localTime"]);
        DateTime now = DateTime.UtcNow.AddMinutes(offset);
        private FactoryManagementEntities db = new FactoryManagementEntities();
        //private DesktopAppEntities db2 = new DesktopAppEntities();
        #endregion

        #region Get Data From WINFORM
        [Route("SaveAttendance")]
        [HttpPost]
        public string Attendance(JArray allListData)
        {
            if (allListData != null)
            {
                //int oparetionStatus = 1;
                List<Attendance> attendanceList = ((JArray)allListData).Select(x => new Attendance
                {
                    Card_UID = (string)x["ser_no"],
                    Status = (string)x["status"],
                    InsertedDate = (DateTime)x["read_time"]
                }).ToList();
                if (attendanceList.Count > 0)
                {
                    for (int i = 0; i < attendanceList.Count; i++)
                    {
                        Attendance attendance = new Attendance();
                        attendance.Card_UID = attendanceList[i].Card_UID;
                        attendance.Status = attendanceList[i].Status;
                        attendance.InsertedDate = attendanceList[i].InsertedDate;
                        db.Attendances.Add(attendance);
                        try
                        {
                            db.SaveChanges();
                            //oparetionStatus = 1;
                        }
                        catch (Exception ex)
                        {
                            string errorMsg = ex.Message.ToString();
                            //oparetionStatus = -1;
                        }
                    }
                    return "Successfully Saved Data Into Server.";
                }
                return "No Data Received!";
            }
            return "Failed Store!";
        }
        #endregion


        #region Get Data From WINFORM
        [Route("PostAttendance")]
        [HttpPost]
        public IHttpActionResult GetAllDataFromWFP(IEnumerable<Attendance_BCR> data)
        {
            if (data.Count() > 0)
            {
                foreach (var m in data)
                {
                    Attendance_BCR atten = new Attendance_BCR();
                    atten.UserId = m.UserId;
                    atten.CardNumber = m.CardNumber;
                    atten.EntryDateTime = m.EntryDateTime;
                    atten.IsIN = m.IsIN;
                    atten.IsSendDate = true;
                    db.Attendance_BCR.Add(atten);
                    try
                    {
                        db.SaveChanges();

                    }
                    catch (Exception)
                    {
                        return BadRequest("Error");
                    }
                }
            }
            return Ok("Success");
        }
        #endregion

        //Check Desktop App Is Active Or Not
        #region
        [Route("ChkAppIsValid")]
        [HttpPost]
        public IHttpActionResult ChkAppIsValid(All_App_List appnumber)
        {
            if (appnumber != null)
            {
                if (db.All_App_List.Where(m => m.AppNumber == appnumber.AppNumber).Any())
                {
                    if (db.All_App_List.Where(m => m.AppNumber == appnumber.AppNumber && m.IsActive == true).Any())
                    {
                        return Ok(1);
                    }
                    else
                    {
                        return Ok(0);
                    }
                }
                else
                {
                    return Ok(-1);
                }
            }
            return BadRequest("Send data was empty...!!!");
        }
        #endregion

        //Check Desktop App Is Active Or Not
        [Route("GetAllActiveUser")]
        [HttpGet]
        public IHttpActionResult SendAllActiveUser()
        {
            if (db.UserInformations.Where(m => m.Status == 1).Any())
            {
                var list = db.View_UserLists.Where(m => m.Status == 1).ToList();
                if (list.Count() > 0)
                {
                    return Ok(list);
                }
            }
            return BadRequest("Error");
        }


        //Pass App Permission User List 
        [Route("GetAllAppUser")]
        [HttpGet]
        public IHttpActionResult SendAllAppUser()
        {
            if (db.App_User_List.Where(m => m.IsActive == true).Any())
            {
                var list = db.App_User_List.Where(m => m.IsActive == true).ToList();
                if (list.Count() > 0)
                {
                    return Ok(list);
                }
            }
            return BadRequest("Error");
        }


        //Check Enter Activation Code Is Validate or Not
        #region
        [Route("GetActivationCode")]
        [HttpPost]
        public IHttpActionResult GetActivationCode(ActivationCodeModelView code)
        {
            if (code != null)
            {
                if (db.All_App_List.Where(m => m.AppNumber == code.AppNumber).Any())
                {
                    All_App_List app_list = db.All_App_List.Where(m => m.AppNumber == code.AppNumber).FirstOrDefault();
                    if (db.ActivationCodes.Where(m => m.Code == code.Code && m.AppId == app_list.Id).Any())
                    {
                        var activation_code = db.ActivationCodes.Where(m => m.Code == code.Code && m.AppId == app_list.Id).FirstOrDefault();
                        if (activation_code.IsActive)
                        {
                            app_list.IsActive = true;
                            app_list.ExpiredDate = DateTime.Now.AddDays(365);
                            db.Entry(app_list).State = EntityState.Modified;
                            db.SaveChanges();

                            return Ok(365);
                        }
                    }
                    else
                    {
                        return Ok(-1);
                    }
                }
                else
                {
                    return BadRequest("Invalid app number.");
                }
                
            }
            return BadRequest("Send data was empty...!!!");
        }
        #endregion

    }
}
