using Cafe_Crafter.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace Cafe_Crafter.Controllers
{
    [RoutePrefix("api/user")]
    public class UserController : ApiController
    {
        CafeEntities db = new CafeEntities();

        [HttpPost, Route("signup")]

        public HttpResponseMessage Signup([FromBody] User user)
        {
            try
            {
                User userObj = db.Users.Where(u => u.email == user.email).FirstOrDefault();
                if (userObj == null)
                {
                    user.role = "user";
                    user.status = "false";
                    db.Users.Add(user);
                    db.SaveChanges();
                    return Request.CreateResponse(HttpStatusCode.OK, new { message = "SuccessFully Registered " });

                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, new { message = " Email already exist" });
                }

            }
            catch(Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex);
            }
        }


        [HttpPost,Route("login")]

        public HttpResponseMessage Login([FromBody] User user)
        {
            try
            {
                User userObj = db.Users.Where(u => (u.email == user.email && u.password == user.password)).FirstOrDefault();    
                    if(userObj != null)
                    {
                        if(userObj.status == "true")
                        {
                            return Request.CreateResponse(HttpStatusCode.OK, new { token = TokenManager.GenerateToken(userObj.email, userObj.role) });
                        }
                        else
                        {
                        return Request.CreateResponse(HttpStatusCode.Unauthorized, new { message = " Wait for Admin Approval" });
                         }
                    }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.Unauthorized, new { message = " Invalid username or password" });
                }

            }
            catch(Exception e)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, e);
            }
        }


    }
}
