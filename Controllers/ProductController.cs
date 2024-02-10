using Cafe_Crafter.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace Cafe_Crafter.Controllers
{
    [RoutePrefix("api/product")]
    public class ProductController : ApiController
    {
        CafeEntities db = new CafeEntities();
        Response response = new Response();

        [HttpPost, Route("addNewProduct")]
        [CustomAuthenticationFilter]

        public HttpResponseMessage AddNewProduct([FromBody] Product product)
        {
            try
            {
                var token = Request.Headers.GetValues("authorization").First();
                TokenClaim tokenClaim = TokenManager.ValidateToken(token);
                if (tokenClaim.Role != "admin")
                {
                    return Request.CreateResponse(HttpStatusCode.Unauthorized);
                }
                product.status = "true";
                db.Products.Add(product);
                db.SaveChanges();
                response.message = "Product Added SuccessFully";
                return Request.CreateResponse(HttpStatusCode.OK, response);


            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex);

            }
        }


        [HttpPost,Route("getAllProduct")]
        [CustomAuthenticationFilter]

        public HttpResponseMessage GetAllProduct()
        {
            try
            {
                var result = from Products in db.Products
                             join Category in db.Categories on Products.categoryID equals Category.id
                             select new
                             {
                                 Products.id,
                                 Products.name,
                                 Products.description,
                                 Products.price,
                                 Products.status,
                                 CategoryId = Category.id,
                                 categoryName = Category.name
                             };
                return Request.CreateResponse(HttpStatusCode.OK, result);
            }
            catch(Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex);
            }
        }

    }
}
