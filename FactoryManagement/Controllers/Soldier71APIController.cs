using FactoryManagement.Models;
using FactoryManagement.ModelView.Soldier71;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace FactoryManagement.Controllers
{
    [RoutePrefix("Soldier71/api")]

    public class Soldier71APIController : ApiController
    {
        #region Private Properties
        static private int offset = Convert.ToInt32(ConfigurationManager.AppSettings["localTime"]);
        DateTime now = DateTime.UtcNow.AddMinutes(offset);
        private FactoryManagementEntities db = new FactoryManagementEntities();
        //private Soldier71Entities db = new Soldier71Entities();
        #endregion

        [HttpGet]
        [Route("GetCategories")]
        public IHttpActionResult GetAllCategories()
        {
            List<CategoriesModelView> CategoriesList = new List<CategoriesModelView>();
            var categories = db.Categories.ToList();
            foreach (var data in categories)
            {
                CategoriesModelView model = new CategoriesModelView();
                if (!data.HasParent)
                {
                    model.CategoryId = data.CategoryId;
                    model.CategoryName = data.CategoryName;

                    model.SubCategories = categories.Where(x => x.HasParent == true && x.ParentId == data.CategoryId).Select(x => new SubCategoryModelView
                    {
                        CategoryName = x.CategoryName,
                        ParentId = x.CategoryId,
                        HasParent = x.HasParent,
                        CanBeDeleted = x.CanBeDeleted,
                        CategoryId = x.CategoryId,
                        SubSubCategories = categories.Where(c => c.HasParent == true && c.ParentId == x.CategoryId).ToList()
                    }).ToList();
                    CategoriesList.Add(model);
                }
            }
            return Ok(CategoriesList);
        }
        [HttpGet]
        [Route("GetProducts")]
        public IHttpActionResult GetProgucts(int? id)
        {
            if (id == null || id == 0)
            {
                return Ok(db.ProductItemLists.ToList());
            }
            else
            {
                return Ok(db.ProductItemLists.Where(x => x.CategoryId == id).ToList());
            }
        }
    }
}
