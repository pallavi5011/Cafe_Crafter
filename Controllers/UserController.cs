using Cafe_Crafter.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.IO;
using System.Net.Mail;
using System.Threading.Tasks;
using System.Web;

namespace Cafe_Crafter.Controllers
{
    [RoutePrefix("api/user")]
    
    public class UserController : ApiController
    {
        CafeEntities db = new CafeEntities();
        Response response = new Response();

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

        [HttpGet, Route("checkToken")]
        [CustomAuthenticationFilter]

        public HttpResponseMessage checkToken()
        {
            return Request.CreateResponse(HttpStatusCode.OK, new { message = "true" });
        }

        [HttpGet, Route("getAllUser")]
        [CustomAuthenticationFilter]

        public HttpResponseMessage GetAllUser()
        {
            try {
                var token = Request.Headers.GetValues("authorization").First();
                TokenClaim tokenClaim = TokenManager.ValidateToken(token);
                if(tokenClaim.Role != "admin")
                {
                    return Request.CreateResponse(HttpStatusCode.Unauthorized);
                }
                var result = db.Users.Select(u => new { u.id, u.name, u.contactNumber, u.email, u.status, u.role, }).Where(x => (x.role == "user")).ToList();
                return Request.CreateResponse(HttpStatusCode.OK, result);
                }

            catch(Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex);
            }
        }


        [HttpPost,Route("updateUserStatus")]
        [CustomAuthenticationFilter]

        public HttpResponseMessage UpdateUserStatus(User user)
        {
            try {
                var token = Request.Headers.GetValues("authorization").First();
                TokenClaim tokenClaim = TokenManager.ValidateToken(token);
                if(tokenClaim.Role != "admin")
                {
                    return Request.CreateResponse(HttpStatusCode.Unauthorized);
                }
                User userObj = db.Users.Find(user.id);
                if(userObj == null)
                {
                    response.message = "User id does not found";
                    return Request.CreateResponse(HttpStatusCode.OK, response);
                }
                userObj.status = user.status;
                db.Entry(userObj).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();
                response.message = "User status Updated Successfully";
                return Request.CreateResponse(HttpStatusCode.OK, response);

            
            }
            catch (Exception ex){
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex);
            
            }
        }

        [HttpPost,Route("changePassword")]
        [CustomAuthenticationFilter]


        public HttpResponseMessage ChangePassword(ChangePassword changePassword)
        {
            try
            {
                var token = Request.Headers.GetValues("authorization").First();
                TokenClaim tokenClaim = TokenManager.ValidateToken(token);

                User userObj = db.Users.Where(x => (x.email == tokenClaim.Email && x.password == changePassword.OldPassword)).FirstOrDefault();
                if(userObj != null)
                {
                    userObj.password = changePassword.NewPassword;
                    db.Entry(userObj).State = System.Data.Entity.EntityState.Modified;
                    db.SaveChanges();
                    response.message = "password updated successfully";
                    return Request.CreateResponse(HttpStatusCode.OK, response);
                }
                else
                {
                    response.message = "Incorrect Old Password";
                    return Request.CreateResponse(HttpStatusCode.BadRequest, response);
                }


            }
            catch(Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex);
            }
        }


        private string createEmailBody(string email,string password)
        {
            try
            {
                string body = string.Empty;
                using (StreamReader reader = new StreamReader(HttpContext.Current.Server.MapPath("/Template/forgot-password.html")))
                {
                    body = reader.ReadToEnd();
                }
                body = body.Replace("{email}", email);
                body = body.Replace("{password}", password);
                body = body.Replace("{frontendUrl}", "http://localhost:4200/");
                return body;
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex);
                return null;
            }

        }

        [HttpPost,Route("forgotPassword")]

        public async Task<HttpResponseMessage> ForgotPassword([FromBody] User user)
        {
            User userObj = db.Users.Where(x => x.email == user.email).FirstOrDefault();
            response.message = "Password sent successfully to your email";
            if(userObj == null)
            {
                return Request.CreateResponse(HttpStatusCode.OK, response);
            }
            var message = new MailMessage();
            message.To.Add(new MailAddress(user.email));
            message.Subject = "Password by Cafe Crafter";
            message.Body = createEmailBody(user.email, userObj.password);
            message.IsBodyHtml = true;
            using (var smtp = new SmtpClient())
            {
                await smtp.SendMailAsync(message);
                await Task.FromResult(0);
            }
            return Request.CreateResponse(HttpStatusCode.OK, response);
        }


    }
}
