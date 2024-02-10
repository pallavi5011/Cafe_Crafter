using Cafe_Crafter.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace Cafe_Crafter.Controllers
{
    [RoutePrefix("api/category")]
    public class CategoryController : ApiController
    {
        CafeEntities db = new CafeEntities();
        Response response = new Response();

        [HttpPost,Route("addNewCategory")]
        [CustomAuthenticationFilter]

        public HttpResponseMessage AddNewCategory([FromBody] Category category)
        {
            try
            {
                var token = Request.Headers.GetValues("authorization").First();
                TokenClaim tokenClaim = TokenManager.ValidateToken(token);
                if(tokenClaim.Role != "admin")
                {
                    return Request.CreateResponse(HttpStatusCode.Unauthorized);
                }
                db.Categories.Add(category);
                db.SaveChanges();
                response.message = "Category Added Successfully";
                return Request.CreateResponse(HttpStatusCode.OK, response);

            }
            catch(Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex);
 
            }
        }

        [HttpPost,Route("getAllCategories")]
        [CustomAuthenticationFilter]

        public HttpResponseMessage GetAllCategory()
        {
            try
            {
                return Request.CreateResponse(HttpStatusCode.OK, db.Categories.ToList());

            }
            catch(Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex);
            }
        }


        [HttpPost,Route("updateCategory")]
        [CustomAuthenticationFilter]

        public HttpResponseMessage UpdateCategory(Category category)
        {
            try
            {
                var token = Request.Headers.GetValues("authorization").First();
                TokenClaim tokenClaim = TokenManager.ValidateToken(token);
                if (tokenClaim.Role != "admin")
                {
                    return Request.CreateResponse(HttpStatusCode.Unauthorized);
                }

                Category categoryObj = db.Categories.Find(category.id);
                if(categoryObj == null)
                {
                    response.message = "Category id does not found";
                    return Request.CreateResponse(HttpStatusCode.OK, response);
                }
                categoryObj.name = category.name;
                db.Entry(categoryObj).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();
                response.message = "Category Updated Successfully";
                return Request.CreateResponse(HttpStatusCode.OK, response);

            }
            catch(Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex);
            }
        }


    }
}
